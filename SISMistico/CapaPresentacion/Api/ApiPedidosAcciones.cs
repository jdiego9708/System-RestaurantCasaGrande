using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;

using CapaNegocio;

namespace CapaPresentacion.Api
{
    /// <summary>
    /// Fase 4: listado y operaciones sobre un pedido existente (agregar, quitar, SALIENDO, cancelar, imprimir).
    /// Las lecturas van por consultas parametrizadas; las escrituras, por los SP transaccionales
    /// db/03_sp_Movil_Agregar_items.sql y db/04_sp_Movil_Quitar_item.sql o por los SP del escritorio
    /// (sp_Cambiar_estado_pedido, sp_Cancelar_pedido).
    /// </summary>
    internal static class ApiPedidosAcciones
    {
        private static readonly string[] EstadosValidos = { "PENDIENTE", "SALIENDO", "CANCELADO", "DISPONIBLE" };

        // =========================================================================================
        // GET /pedidos?fecha=2026-10-21&estado=PENDIENTE&mesa=1
        // =========================================================================================

        public static ApiResponse Listar(ApiRequest req)
        {
            DateTime fecha = DateTime.Today;
            string textoFecha = req.Query["fecha"];
            if (!string.IsNullOrEmpty(textoFecha) &&
                !DateTime.TryParseExact(textoFecha, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out fecha))
                return ApiResponse.Error(400, "VALIDACION", "fecha debe tener el formato yyyy-MM-dd.");

            string estado = null;
            string textoEstado = req.Query["estado"];
            if (!string.IsNullOrEmpty(textoEstado))
            {
                estado = textoEstado.Trim().ToUpperInvariant();
                if (!EstadosValidos.Contains(estado))
                    return ApiResponse.Error(400, "VALIDACION", "estado debe ser PENDIENTE, SALIENDO, CANCELADO o DISPONIBLE.");
            }

            int numMesa = 0;
            string textoMesa = req.Query["mesa"];
            if (!string.IsNullOrEmpty(textoMesa) && (!int.TryParse(textoMesa, out numMesa) || numMesa <= 0))
                return ApiResponse.Error(400, "VALIDACION", "mesa debe ser un número de mesa positivo.");

            DataTable t = NApiMovil.Pedidos(fecha, estado, numMesa > 0 ? (int?)numMesa : null);
            List<object> pedidos = new List<object>();
            foreach (DataRow r in t.Rows)
            {
                object hora = r["Hora_pedido"];
                pedidos.Add(new Dictionary<string, object>
                {
                    { "id_pedido", Convert.ToInt32(r["Id_pedido"]) },
                    { "id_mesa", Convert.ToInt32(r["Id_mesa"]) },
                    { "num_mesa", Convert.ToInt32(r["Num_mesa"]) },
                    { "id_cliente", Convert.ToInt32(r["Id_cliente"]) },
                    { "cliente", ApiJson.Celda(r, "Cliente") },
                    { "id_empleado", Convert.ToInt32(r["Id_empleado"]) },
                    { "empleado", ApiJson.Celda(r, "Empleado") },
                    { "estado", ApiJson.Celda(r, "Estado_pedido") },
                    { "hora", hora is TimeSpan ? ((TimeSpan)hora).ToString(@"hh\:mm") : null },
                    { "tipo_pedido", ApiJson.Celda(r, "Tipo_pedido") },
                    { "cantidad_clientes", Convert.ToInt32(r["CantidadClientes"]) },
                    { "lineas", Convert.ToInt32(r["Lineas"]) },
                    { "total", ApiJson.Dinero(Convert.ToDecimal(r["Total"])) }
                });
            }

            ApiResponse ok = ApiResponse.Ok(new Dictionary<string, object>
            {
                { "fecha", fecha.ToString("yyyy-MM-dd") },
                { "estado", estado },
                { "mesa", numMesa > 0 ? (object)numMesa : null },
                { "pedidos", pedidos }
            });
            ok.LogExtra = "pedidos=" + pedidos.Count;
            return ok;
        }

        // =========================================================================================
        // POST /pedidos/{id}/items
        // =========================================================================================

        public static ApiResponse AgregarItems(ApiRequest req)
        {
            int id;
            ApiResponse error;
            if (!IdDeRuta(req, out id, out error)) return error;

            string cuerpo, motivo;
            if (!req.LeerCuerpo(out cuerpo, out motivo))
                return ApiResponse.Error(400, "VALIDACION", motivo);
            Dictionary<string, object> datos = ApiJson.LeerObjeto(cuerpo, out motivo);
            if (datos == null)
                return ApiResponse.Error(400, "VALIDACION", motivo);

            IList<object> items = Lista(datos.ContainsKey("items") ? datos["items"] : null);
            if (items == null || items.Count == 0)
                return ApiResponse.Error(400, "VALIDACION", "items es obligatorio y no puede estar vacío.");
            if (items.Count > ApiConfig.MaxItems)
                return ApiResponse.Error(400, "VALIDACION", "items admite como máximo " + ApiConfig.MaxItems + " elementos.");

            List<ApiPedidosHandlers.Item> lista = ApiPedidosHandlers.LeerItems(items, out motivo);
            if (lista == null)
                return ApiResponse.Error(400, "VALIDACION", motivo);

            DataRow pedido = Pedido(id);
            if (pedido == null)
                return ApiResponse.Error(404, "NO_ENCONTRADO", "No existe el pedido " + id + ".");
            ApiResponse noEditable = ComprobarEditable(pedido);
            if (noEditable != null) return noEditable;

            ApiResponse invalido = ApiPedidosHandlers.ValidarProductos(lista);
            if (invalido != null) return invalido;

            int filas;
            try
            {
                filas = NApiMovil.AgregarItemsMovil(id, ApiPedidosHandlers.ArmarItemsSp(lista));
            }
            catch (SqlException ex) when (ex.Number >= 50004 && ex.Number <= 50012)
            {
                return DeSpAError(ex, id);
            }

            ApiImpresion.RefrescarMapa();

            Dictionary<string, object> actualizado = ApiPedidosHandlers.ObtenerPedido(id);
            ApiResponse ok = ApiResponse.Ok(actualizado);
            ok.LogExtra = "id_pedido=" + id + " filas=" + filas;
            return ok;
        }

        // =========================================================================================
        // DELETE /pedidos/{id}/items/{id_detalle_pedido}?clave_maestra=9708&cantidad=1
        // =========================================================================================

        public static ApiResponse QuitarItem(ApiRequest req)
        {
            int id;
            ApiResponse error;
            if (!IdDeRuta(req, out id, out error)) return error;

            int idDetalle;
            if (!int.TryParse(req.Parametros["id_detalle_pedido"], out idDetalle) || idDetalle <= 0)
                return ApiResponse.Error(400, "VALIDACION", "id_detalle_pedido debe ser un entero positivo.");

            // Autorización: un PIN de cualquier empleado (igual que FrmComprobacion en el escritorio).
            string textoPin = req.Query["clave_maestra"];
            int pin;
            if (string.IsNullOrEmpty(textoPin) || !int.TryParse(textoPin, out pin) || pin <= 0)
                return ApiResponse.Error(400, "VALIDACION", "clave_maestra es obligatoria: el PIN del empleado que autoriza.");
            if (!NApiMovil.ExistePin(pin))
            {
                ApiAuth.RegistrarFallo(req.Ip);
                return ApiResponse.Error(403, "CLAVE_INVALIDA", "La clave maestra no corresponde a ningún empleado.");
            }

            int cantidad = 1;
            string textoCantidad = req.Query["cantidad"];
            if (!string.IsNullOrEmpty(textoCantidad) && (!int.TryParse(textoCantidad, out cantidad) || cantidad <= 0))
                return ApiResponse.Error(400, "VALIDACION", "cantidad debe ser un entero positivo.");

            DataRow pedido = Pedido(id);
            if (pedido == null)
                return ApiResponse.Error(404, "NO_ENCONTRADO", "No existe el pedido " + id + ".");
            ApiResponse noEditable = ComprobarEditable(pedido);
            if (noEditable != null) return noEditable;

            DataTable detalle = NApiMovil.DetalleDePedido(id, idDetalle);
            if (detalle.Rows.Count == 0)
                return ApiResponse.Error(404, "NO_ENCONTRADO", "La línea " + idDetalle + " no existe en el pedido " + id + ".");
            int enLinea = Convert.ToInt32(detalle.Rows[0]["Cantidad"]);
            if (cantidad > enLinea)
                return ApiResponse.Error(422, "REGLA_NEGOCIO",
                    "La línea " + idDetalle + " tiene " + enLinea + " unidades; no se pueden quitar " + cantidad + ".");

            int restante;
            try
            {
                restante = NApiMovil.QuitarItemMovil(id, idDetalle, cantidad);
            }
            catch (SqlException ex) when (ex.Number >= 50020 && ex.Number <= 50023)
            {
                return DeSpAError(ex, id);
            }

            ApiImpresion.RefrescarMapa();

            Dictionary<string, object> actualizado = ApiPedidosHandlers.ObtenerPedido(id);
            ApiResponse ok = ApiResponse.Ok(actualizado);
            ok.LogExtra = "id_pedido=" + id + " linea=" + idDetalle + " quedan=" + restante;
            return ok;
        }

        // =========================================================================================
        // PATCH /pedidos/{id}/estado   (solo PENDIENTE -> SALIENDO)
        // =========================================================================================

        public static ApiResponse CambiarEstado(ApiRequest req)
        {
            int id;
            ApiResponse error;
            if (!IdDeRuta(req, out id, out error)) return error;

            string cuerpo, motivo;
            if (!req.LeerCuerpo(out cuerpo, out motivo))
                return ApiResponse.Error(400, "VALIDACION", motivo);
            Dictionary<string, object> datos = ApiJson.LeerObjeto(cuerpo, out motivo);
            if (datos == null)
                return ApiResponse.Error(400, "VALIDACION", motivo);

            string estado = ApiJson.Texto(datos, "estado");
            if (!string.Equals(estado, "SALIENDO", StringComparison.OrdinalIgnoreCase))
                return ApiResponse.Error(422, "REGLA_NEGOCIO", "Desde el móvil solo se puede pasar a SALIENDO.");

            DataRow pedido = Pedido(id);
            if (pedido == null)
                return ApiResponse.Error(404, "NO_ENCONTRADO", "No existe el pedido " + id + ".");
            string actual = Convert.ToString(pedido["Estado_pedido"]).ToUpperInvariant();
            if (actual != "PENDIENTE")
                return ApiResponse.Error(422, "REGLA_NEGOCIO",
                    "Solo se pasa a SALIENDO desde PENDIENTE (el pedido está " + actual + ").");

            // sp_Cambiar_estado_pedido reescribe Id_mesa: se le pasa la mesa actual para no moverlo.
            NApiMovil.CambiarEstadoPedido(id, "SALIENDO", Convert.ToInt32(pedido["Id_mesa"]));
            ApiImpresion.RefrescarMapa();

            Dictionary<string, object> actualizado = ApiPedidosHandlers.ObtenerPedido(id);
            ApiResponse ok = ApiResponse.Ok(actualizado);
            ok.LogExtra = "id_pedido=" + id + " estado=SALIENDO";
            return ok;
        }

        // =========================================================================================
        // POST /pedidos/{id}/cancelar
        // =========================================================================================

        public static ApiResponse Cancelar(ApiRequest req)
        {
            int id;
            ApiResponse error;
            if (!IdDeRuta(req, out id, out error)) return error;

            string cuerpo, motivo;
            if (!req.LeerCuerpo(out cuerpo, out motivo))
                return ApiResponse.Error(400, "VALIDACION", motivo);
            Dictionary<string, object> datos = ApiJson.LeerObjeto(cuerpo, out motivo);
            if (datos == null)
                return ApiResponse.Error(400, "VALIDACION", motivo);

            string observaciones = ApiJson.Texto(datos, "observaciones");
            if (string.IsNullOrWhiteSpace(observaciones) || observaciones.Length > 200)
                return ApiResponse.Error(400, "VALIDACION", "observaciones es obligatorio (1 a 200 caracteres).");

            DataRow pedido = Pedido(id);
            if (pedido == null)
                return ApiResponse.Error(404, "NO_ENCONTRADO", "No existe el pedido " + id + ".");
            string actual = Convert.ToString(pedido["Estado_pedido"]).ToUpperInvariant();
            if (actual != "PENDIENTE" && actual != "SALIENDO")
                return ApiResponse.Error(422, "REGLA_NEGOCIO",
                    "Solo se cancela un pedido PENDIENTE o SALIENDO (está " + actual + ").");

            // sp_Cancelar_pedido reemplaza Observaciones_pedido por el motivo de la cancelación.
            NApiMovil.CancelarPedido(id, observaciones.Trim());
            ApiImpresion.RefrescarMapa();

            Dictionary<string, object> actualizado = ApiPedidosHandlers.ObtenerPedido(id);
            ApiResponse ok = ApiResponse.Ok(actualizado);
            ok.LogExtra = "id_pedido=" + id + " estado=CANCELADO";
            return ok;
        }

        // =========================================================================================
        // POST /pedidos/{id}/imprimir
        // =========================================================================================

        public static ApiResponse Imprimir(ApiRequest req)
        {
            int id;
            ApiResponse error;
            if (!IdDeRuta(req, out id, out error)) return error;

            string tipo = "TODO";
            int copias = 1;

            {
                string cuerpo, motivo;
                if (!req.LeerCuerpo(out cuerpo, out motivo))
                    return ApiResponse.Error(400, "VALIDACION", motivo);
                if (!string.IsNullOrWhiteSpace(cuerpo))
                {
                    Dictionary<string, object> datos = ApiJson.LeerObjeto(cuerpo, out motivo);
                    if (datos == null)
                        return ApiResponse.Error(400, "VALIDACION", motivo);

                    if (datos.ContainsKey("tipo") && datos["tipo"] != null)
                    {
                        tipo = (ApiJson.Texto(datos, "tipo") ?? string.Empty).Trim().ToUpperInvariant();
                        if (tipo != "PLATOS" && tipo != "BEBIDAS" && tipo != "TODO")
                            return ApiResponse.Error(400, "VALIDACION", "tipo debe ser PLATOS, BEBIDAS o TODO.");
                    }
                    if (datos.ContainsKey("copias") && datos["copias"] != null &&
                        (!ApiJson.TryEntero(datos, "copias", out copias) || copias < 1 || copias > 5))
                        return ApiResponse.Error(400, "VALIDACION", "copias debe ser un entero entre 1 y 5.");
                }
            }

            if (Pedido(id) == null)
                return ApiResponse.Error(404, "NO_ENCONTRADO", "No existe el pedido " + id + ".");

            bool ocupado;
            bool encolada = ApiImpresion.ImprimirComandas(id, tipo, copias, out ocupado);
            if (ocupado)
                return ApiResponse.Error(409, "IMPRESION_EN_CURSO", "Ya hay una impresión en curso; espera a que termine.");
            if (!encolada)
                return ApiResponse.Error(503, "SIN_UI", "El escritorio no está disponible para imprimir.");

            ApiResponse ok = ApiResponse.Aceptado(new Dictionary<string, object>
            {
                { "id_pedido", id },
                { "tipo", tipo },
                { "copias", copias },
                { "impreso", true }
            });
            ok.LogExtra = "id_pedido=" + id + " tipo=" + tipo + " copias=" + copias;
            return ok;
        }

        // ---- utilidades ----

        private static bool IdDeRuta(ApiRequest req, out int id, out ApiResponse error)
        {
            error = null;
            if (!int.TryParse(req.Parametros["id"], out id) || id <= 0)
            {
                error = ApiResponse.Error(400, "VALIDACION", "El id del pedido debe ser un entero positivo.");
                return false;
            }
            return true;
        }

        private static DataRow Pedido(int id)
        {
            DataTable t = NApiMovil.EstadoPedido(id);
            return t.Rows.Count == 0 ? null : t.Rows[0];
        }

        private static ApiResponse ComprobarEditable(DataRow pedido)
        {
            string estado = Convert.ToString(pedido["Estado_pedido"]).ToUpperInvariant();
            if (estado == "PENDIENTE" || estado == "SALIENDO")
                return null;
            return ApiResponse.Error(422, "REGLA_NEGOCIO",
                "El pedido " + pedido["Id_pedido"] + " está " + estado + " y no admite cambios.");
        }

        /// <summary>Traduce los THROW de los SP a la respuesta del contrato (404 si no existe, 422 si la regla).</summary>
        private static ApiResponse DeSpAError(SqlException ex, int idPedido)
        {
            switch (ex.Number)
            {
                case 50010:
                case 50020:
                    return ApiResponse.Error(404, "NO_ENCONTRADO", "No existe el pedido " + idPedido + ".");
                case 50022:
                    return ApiResponse.Error(404, "NO_ENCONTRADO", "La línea no existe o no pertenece al pedido " + idPedido + ".");
                default:
                    ApiLog.Error("El SP rechazó la operación sobre el pedido " + idPedido, ex);
                    return ApiResponse.Error(422, "REGLA_NEGOCIO", ex.Message);
            }
        }

        /// <summary>JavaScriptSerializer entrega los arrays como object[] o ArrayList.</summary>
        private static IList<object> Lista(object o)
        {
            object[] arr = o as object[];
            if (arr != null) return arr;
            System.Collections.ArrayList al = o as System.Collections.ArrayList;
            return al == null ? null : al.Cast<object>().ToList();
        }
    }
}
