using System;
using System.Collections.Generic;
using CapaDatos;
using CapaEntidades.Models;

namespace CapaNegocio
{
    /// <summary>Consulta de las ordenes de Rappi leidas de las facturas PDF.</summary>
    public static class NRappiOrders
    {
        /// <summary>Ordenes del dia, las mas recientes primero, con busqueda y paginacion (page empieza en 0).</summary>
        public static List<RappiOrder> SearchDay(DateTime day, string text, int page, int pageSize, out int total) =>
            DRappiOrders.SearchDay(day, text, page, pageSize, out total);

        public static int CountDay(DateTime day) => DRappiOrders.CountDay(day);

        public static RappiOrder GetByExternalId(string externalId) => DRappiOrders.GetByExternalId(externalId);

        /// <summary>Numero del dia de la orden (lo asigna si falta); null si la base aun no tiene la numeracion.</summary>
        public static int? EnsureDayNumber(string externalId) => DRappiOrders.EnsureDayNumber(externalId);

        /// <summary>Ordenes de un rango de dias con sus datos basicos (el historial de ventas las lista).</summary>
        public static System.Data.DataTable ListRange(DateTime from, DateTime to) => DRappiOrders.ListRange(from, to);
    }
}
