using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

using CapaNegocio;

namespace CapaPresentacion.Api
{
    /// <summary>Catálogo de solo lectura: tipos, platos, bebidas, ingredientes y clientes.</summary>
    internal static class ApiCatalogoHandlers
    {
        private const int MaxTexto = 50;

        public static ApiResponse TiposPlatos(ApiRequest req)
        {
            List<object> lista = new List<object>();
            foreach (DataRow r in NApiMovil.TiposPlatos().Rows)
                lista.Add(new Dictionary<string, object>
                {
                    { "id_tipo_plato", Convert.ToInt32(r["Id_tipo_plato"]) },
                    { "tipo_plato", ApiJson.Celda(r, "Tipo_plato") }
                });
            return ApiResponse.Ok(lista);
        }

        public static ApiResponse TiposBebidas(ApiRequest req)
        {
            List<object> lista = new List<object>();
            foreach (DataRow r in NApiMovil.TiposBebidas().Rows)
                lista.Add(new Dictionary<string, object>
                {
                    { "id_tipo_bebida", Convert.ToInt32(r["Id_tipo_bebida"]) },
                    { "tipo_bebida", ApiJson.Celda(r, "Tipo_bebida") }
                });
            return ApiResponse.Ok(lista);
        }

        public static ApiResponse Platos(ApiRequest req)
        {
            ApiResponse error;
            int? tipo = LeerEntero(req, "tipo_plato", out error);
            if (error != null) return error;
            string q = LeerTexto(req, "q", out error);
            if (error != null) return error;

            List<object> lista = new List<object>();
            foreach (DataRow r in NApiMovil.Platos(tipo, null, q).Rows)
                lista.Add(Plato(r, req));
            return ApiResponse.Ok(lista);
        }

        public static ApiResponse PlatoPorId(ApiRequest req)
        {
            ApiResponse error;
            DataRow r = BuscarPlato(req, out error);
            if (error != null) return error;

            Dictionary<string, object> plato = Plato(r, req);
            if ((bool)plato["detallado"])
                plato["ingredientes"] = Ingredientes(NApiMovil.Ingredientes(null));
            return ApiResponse.Ok(plato);
        }

        /// <summary>
        /// Los ingredientes no son por plato: el escritorio ofrece la misma lista (SOPA, ARROZ, ACOMPANANTE, BEBIDA)
        /// en cualquier plato detallado. Un plato no detallado devuelve lista vacía.
        /// </summary>
        public static ApiResponse IngredientesDePlato(ApiRequest req)
        {
            ApiResponse error;
            DataRow r = BuscarPlato(req, out error);
            if (error != null) return error;

            bool detallado = string.Equals(ApiJson.Celda(r, "Plato_detallado"), "ACTIVO", StringComparison.OrdinalIgnoreCase);
            return ApiResponse.Ok(detallado ? Ingredientes(NApiMovil.Ingredientes(null)) : new List<object>());
        }

        public static ApiResponse IngredientesLista(ApiRequest req)
        {
            ApiResponse error;
            string tipo = LeerTexto(req, "tipo", out error);
            if (error != null) return error;
            return ApiResponse.Ok(Ingredientes(NApiMovil.Ingredientes(tipo)));
        }

        public static ApiResponse Bebidas(ApiRequest req)
        {
            ApiResponse error;
            int? tipo = LeerEntero(req, "tipo_bebida", out error);
            if (error != null) return error;
            string q = LeerTexto(req, "q", out error);
            if (error != null) return error;

            List<object> lista = new List<object>();
            foreach (DataRow r in NApiMovil.Bebidas(tipo, q).Rows)
                lista.Add(new Dictionary<string, object>
                {
                    { "id_bebida", Convert.ToInt32(r["Id_bebida"]) },
                    { "nombre", ApiJson.Celda(r, "Nombre_bebida") },
                    { "precio", ApiJson.Dinero(r["Precio_bebida"]) },
                    { "id_tipo_bebida", Convert.ToInt32(r["Id_tipo_bebida"]) },
                    { "tipo_bebida", ApiJson.Celda(r, "Tipo_bebida") },
                    { "descripcion", ApiJson.Celda(r, "Descripcion_bebida") },
                    { "imagen_url", ApiImagenesHandlers.UrlImagen(req, ApiJson.Celda(r, "Imagen")) }
                });
            return ApiResponse.Ok(lista);
        }

        public static ApiResponse Clientes(ApiRequest req)
        {
            ApiResponse error;
            string q = LeerTexto(req, "q", out error);
            if (error != null) return error;

            List<object> lista = new List<object>();
            foreach (DataRow r in NApiMovil.Clientes(q).Rows)
                lista.Add(new Dictionary<string, object>
                {
                    { "id_cliente", Convert.ToInt32(r["Id_cliente"]) },
                    { "nombre", ApiJson.Celda(r, "Nombre_cliente") },
                    { "telefono", ApiJson.Celda(r, "Telefono_cliente") }
                });
            return ApiResponse.Ok(lista);
        }

        // ---------------------------------------------------------------------------------------

        private static DataRow BuscarPlato(ApiRequest req, out ApiResponse error)
        {
            error = null;
            int id;
            if (!int.TryParse(req.Parametros["id"], out id))
            {
                error = ApiResponse.Error(400, "VALIDACION", "El id del plato debe ser un entero.");
                return null;
            }
            DataTable t = NApiMovil.Platos(null, id, null);
            if (t.Rows.Count == 0)
            {
                error = ApiResponse.Error(404, "NO_ENCONTRADO", "No existe un plato activo con ese id.");
                return null;
            }
            return t.Rows[0];
        }

        private static Dictionary<string, object> Plato(DataRow r, ApiRequest req)
        {
            return new Dictionary<string, object>
            {
                { "id_plato", Convert.ToInt32(r["Id_plato"]) },
                { "nombre", ApiJson.Celda(r, "Nombre_plato") },
                { "precio", ApiJson.Dinero(r["Precio_plato"]) },
                { "id_tipo_plato", Convert.ToInt32(r["Id_tipo_plato"]) },
                { "tipo_plato", ApiJson.Celda(r, "Tipo_plato") },
                { "descripcion", ApiJson.Celda(r, "Descripcion_plato") },
                { "detallado", string.Equals(ApiJson.Celda(r, "Plato_detallado"), "ACTIVO", StringComparison.OrdinalIgnoreCase) },
                { "carta", string.Equals(ApiJson.Celda(r, "Plato_carta"), "ACTIVO", StringComparison.OrdinalIgnoreCase) },
                { "imagen_url", ApiImagenesHandlers.UrlImagen(req, ApiJson.Celda(r, "Imagen_plato")) }
            };
        }

        private static List<object> Ingredientes(DataTable t)
        {
            List<object> lista = new List<object>();
            foreach (DataRow r in t.Rows)
                lista.Add(new Dictionary<string, object>
                {
                    { "id_ingrediente", Convert.ToInt32(r["Id_ingrediente"]) },
                    { "tipo", ApiJson.Celda(r, "Tipo_ingrediente") },
                    { "nombre", ApiJson.Celda(r, "Nombre_ingrediente") },
                    { "descripcion", ApiJson.Celda(r, "Descripcion_ingrediente") },
                    { "estado", ApiJson.Celda(r, "Estado_ingrediente") }
                });
            return lista;
        }

        /// <summary>Parámetro de query entero opcional; error 400 si viene y no lo es.</summary>
        private static int? LeerEntero(ApiRequest req, string nombre, out ApiResponse error)
        {
            error = null;
            string v = req.Query[nombre];
            if (string.IsNullOrWhiteSpace(v))
                return null;
            int n;
            if (!int.TryParse(v.Trim(), out n))
            {
                error = ApiResponse.Error(400, "VALIDACION", nombre + " debe ser un entero.");
                return null;
            }
            return n;
        }

        /// <summary>Parámetro de query de texto opcional, recortado y con tope de longitud.</summary>
        private static string LeerTexto(ApiRequest req, string nombre, out ApiResponse error)
        {
            error = null;
            string v = req.Query[nombre];
            if (string.IsNullOrWhiteSpace(v))
                return null;
            v = v.Trim();
            if (v.Length > MaxTexto)
            {
                error = ApiResponse.Error(400, "VALIDACION", nombre + " admite como máximo " + MaxTexto + " caracteres.");
                return null;
            }
            return v;
        }
    }
}
