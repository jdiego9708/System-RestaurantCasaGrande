using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;

using CapaNegocio;

namespace CapaPresentacion.Api
{
    internal static class ApiMesasHandlers
    {
        /// <summary>
        /// GET /mesas?fecha=yyyy-MM-dd. Estado derivado como en el escritorio: PENDIENTE/SALIENDO si la mesa
        /// tiene un pedido abierto de esa fecha; DISPONIBLE en otro caso (incluye los DISPONIBLE y CANCELADO).
        /// </summary>
        public static ApiResponse Listar(ApiRequest req)
        {
            DateTime fecha = DateTime.Today;
            string f = req.Query["fecha"];
            if (!string.IsNullOrWhiteSpace(f) &&
                !DateTime.TryParseExact(f.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out fecha))
                return ApiResponse.Error(400, "VALIDACION", "fecha debe tener el formato yyyy-MM-dd.");

            List<object> lista = new List<object>();
            foreach (DataRow r in NApiMovil.Mesas(fecha).Rows)
            {
                object hora = r["Hora_pedido"];
                lista.Add(new Dictionary<string, object>
                {
                    { "id_mesa", Convert.ToInt32(r["Id_mesa"]) },
                    { "num_mesa", Convert.ToInt32(r["Num_mesa"]) },
                    { "descripcion", ApiJson.Celda(r, "Descripcion_mesa") },
                    { "estado", ApiJson.Celda(r, "Estado") },
                    { "id_pedido", Convert.ToInt32(r["Id_pedido"]) },
                    { "hora", hora is TimeSpan ? ((TimeSpan)hora).ToString(@"hh\:mm") : null },
                    { "cantidad_clientes", Convert.ToInt32(r["CantidadClientes"]) },
                    { "total", ApiJson.Dinero(r["Total"]) }
                });
            }
            return ApiResponse.Ok(lista);
        }
    }
}
