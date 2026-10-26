using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;

namespace CapaPresentacion.Api
{
    /// <summary>Tabla de rutas, autenticación previa al handler, manejo global de errores y escritura de la respuesta.</summary>
    internal static class ApiRouter
    {
        private sealed class Ruta
        {
            public string Metodo;
            public string[] Segmentos;
            public bool Publica;        // sin X-Api-Key ni token
            public bool RequiereToken;  // además de la clave, exige sesión
            public Func<ApiRequest, ApiResponse> Handler;
        }

        private static readonly List<Ruta> Rutas = new List<Ruta>
        {
            Nueva("GET",  "/ping",        true,  false, ApiHandlers.Ping),
            Nueva("POST", "/auth/login",  false, false, ApiHandlers.Login),
            Nueva("GET",  "/auth/me",     false, true,  ApiHandlers.Me),
            Nueva("POST", "/auth/logout", false, true,  ApiHandlers.Logout),

            // Fase 2: catálogo, mesas e imágenes (solo lectura; todas exigen clave y token)
            Nueva("GET",  "/mesas",                       false, true, ApiMesasHandlers.Listar),
            Nueva("GET",  "/tipos-platos",                false, true, ApiCatalogoHandlers.TiposPlatos),
            Nueva("GET",  "/tipos-bebidas",               false, true, ApiCatalogoHandlers.TiposBebidas),
            Nueva("GET",  "/platos",                      false, true, ApiCatalogoHandlers.Platos),
            Nueva("GET",  "/platos/{id}",                 false, true, ApiCatalogoHandlers.PlatoPorId),
            Nueva("GET",  "/platos/{id}/ingredientes",    false, true, ApiCatalogoHandlers.IngredientesDePlato),
            Nueva("GET",  "/bebidas",                     false, true, ApiCatalogoHandlers.Bebidas),
            Nueva("GET",  "/ingredientes",                false, true, ApiCatalogoHandlers.IngredientesLista),
            Nueva("GET",  "/clientes",                    false, true, ApiCatalogoHandlers.Clientes),
            Nueva("GET",  "/imagenes/{archivo}",          false, true, ApiImagenesHandlers.Obtener),

            // Fase 3: alta de pedidos
            Nueva("POST", "/pedidos",                     false, true, ApiPedidosHandlers.Crear),
            Nueva("GET",  "/pedidos/{id}",                false, true, ApiPedidosHandlers.Obtener),

            // Fase 4: listado y operaciones sobre un pedido existente
            Nueva("GET",    "/pedidos",                                  false, true, ApiPedidosAcciones.Listar),
            Nueva("POST",   "/pedidos/{id}/items",                       false, true, ApiPedidosAcciones.AgregarItems),
            Nueva("DELETE", "/pedidos/{id}/items/{id_detalle_pedido}",   false, true, ApiPedidosAcciones.QuitarItem),
            Nueva("PATCH",  "/pedidos/{id}/estado",                      false, true, ApiPedidosAcciones.CambiarEstado),
            Nueva("POST",   "/pedidos/{id}/cancelar",                    false, true, ApiPedidosAcciones.Cancelar),
            Nueva("POST",   "/pedidos/{id}/imprimir",                    false, true, ApiPedidosAcciones.Imprimir)
        };

        private static Ruta Nueva(string metodo, string plantilla, bool publica, bool token,
                                  Func<ApiRequest, ApiResponse> handler)
        {
            return new Ruta
            {
                Metodo = metodo,
                Segmentos = plantilla.Trim('/').Split('/'),
                Publica = publica,
                RequiereToken = token,
                Handler = handler
            };
        }

        public static void Despachar(HttpListenerContext ctx)
        {
            Stopwatch sw = Stopwatch.StartNew();
            ApiRequest req = null;
            ApiResponse resp;
            try
            {
                req = ApiRequest.Crear(ctx);
                resp = Resolver(req);
            }
            catch (SqlException ex)
            {
                ApiLog.Error("Error SQL", ex);
                resp = EsDeConexion(ex)
                    ? ApiResponse.Error(503, "BD_NO_DISPONIBLE", "No hay conexión con la base de datos.")
                    : ApiResponse.Error(500, "ERROR_BD", "Error de base de datos.");
            }
            catch (Exception ex)
            {
                ApiLog.Error("Error no controlado", ex);
                resp = ApiResponse.Error(500, "ERROR_INTERNO", "Error interno del servidor.");
            }

            Escribir(ctx, resp);
            sw.Stop();
            string quien = req != null && req.Sesion != null ? "emp=" + req.Sesion.Empleado.IdEmpleado : null;
            string cod = string.Join(" ", new[] { resp.Codigo, quien, resp.LogExtra }.Where(x => !string.IsNullOrEmpty(x)));
            ApiLog.Peticion(req != null ? req.Metodo : ctx.Request.HttpMethod,
                            req != null ? req.Ruta : ctx.Request.RawUrl,
                            resp.Status, sw.ElapsedMilliseconds,
                            req != null ? req.Ip : "?", cod);
        }

        private static ApiResponse Resolver(ApiRequest req)
        {
            if (!ApiAuth.IpPermitida(req.DireccionIp))
                return ApiResponse.Error(403, "IP_NO_PERMITIDA", "Solo se aceptan peticiones desde la red local.");

            string[] segs = req.Ruta.Trim('/').Split('/');
            Ruta encontrada = null;
            bool rutaExiste = false;
            foreach (Ruta r in Rutas)
            {
                if (!Coincide(r, segs, req))
                    continue;
                rutaExiste = true;
                if (r.Metodo == req.Metodo)
                {
                    encontrada = r;
                    break;
                }
            }
            if (encontrada == null)
                return rutaExiste
                    ? ApiResponse.Error(405, "METODO_NO_PERMITIDO", "Método no permitido para esta ruta.")
                    : ApiResponse.Error(404, "NO_ENCONTRADO", "Ruta inexistente.");

            if (!encontrada.Publica)
            {
                if (!ApiAuth.ClaveValida(req.Cabecera("X-Api-Key")))
                    return ApiResponse.Error(401, "NO_AUTORIZADO", "X-Api-Key inválida o ausente.");
                if (encontrada.RequiereToken)
                {
                    req.Sesion = ApiAuth.Buscar(req.Token);
                    if (req.Sesion == null)
                        return ApiResponse.Error(401, "NO_AUTORIZADO", "Token ausente, inválido o vencido.");
                    ApiAuth.RegistrarActividad(req.DireccionIp,
                        req.Sesion.Empleado != null ? req.Sesion.Empleado.Nombre : null);
                }
            }
            return encontrada.Handler(req);
        }

        private static bool Coincide(Ruta r, string[] segs, ApiRequest req)
        {
            if (r.Segmentos.Length != segs.Length)
                return false;
            Dictionary<string, string> parametros = new Dictionary<string, string>();
            for (int i = 0; i < segs.Length; i++)
            {
                string plantilla = r.Segmentos[i];
                if (plantilla.StartsWith("{") && plantilla.EndsWith("}"))
                    parametros[plantilla.Substring(1, plantilla.Length - 2)] = Uri.UnescapeDataString(segs[i]);
                else if (!string.Equals(plantilla, segs[i], StringComparison.OrdinalIgnoreCase))
                    return false;
            }
            foreach (KeyValuePair<string, string> kv in parametros)
                req.Parametros[kv.Key] = kv.Value;
            return true;
        }

        private static void Escribir(HttpListenerContext ctx, ApiResponse resp)
        {
            try
            {
                ctx.Response.StatusCode = resp.Status;
                foreach (KeyValuePair<string, string> h in resp.Cabeceras)
                    ctx.Response.Headers[h.Key] = h.Value;

                if (resp.Bytes != null)
                {
                    ctx.Response.ContentType = resp.TipoContenido;
                    ctx.Response.ContentLength64 = resp.Bytes.Length;
                    ctx.Response.OutputStream.Write(resp.Bytes, 0, resp.Bytes.Length);
                }
                else if (resp.Cuerpo == null)
                {
                    ctx.Response.ContentLength64 = 0;
                }
                else
                {
                    byte[] bytes = new UTF8Encoding(false).GetBytes(ApiJson.Serializar(resp.Cuerpo));
                    ctx.Response.ContentType = "application/json; charset=utf-8";
                    ctx.Response.ContentLength64 = bytes.Length;
                    ctx.Response.OutputStream.Write(bytes, 0, bytes.Length);
                }
            }
            catch (Exception ex)
            {
                ApiLog.Error("No se pudo escribir la respuesta (¿el cliente se desconectó?)", ex);
            }
            finally
            {
                try { ctx.Response.Close(); } catch { }
            }
        }

        /// <summary>Errores de conexión/servidor no disponible (503) frente a errores de la consulta (500).</summary>
        private static bool EsDeConexion(SqlException ex)
        {
            switch (ex.Number)
            {
                case -2: case -1: case 2: case 53: case 233: case 4060: case 10053: case 10054: case 10060: case 18456:
                    return true;
                default:
                    return ex.Class >= 20;
            }
        }
    }
}
