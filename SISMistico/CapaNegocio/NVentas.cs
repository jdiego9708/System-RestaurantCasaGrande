using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using CapaDatos;

namespace CapaNegocio
{
    public class NVentas
    {
        #region INSERTAR VENTA
        public static string InsertarVenta(List<string> variables,
            out int id_venta, DataTable detalle_pago)
        {
            DVenta DVenta = new DVenta();
            return DVenta.InsertarVenta(variables, detalle_pago, out id_venta);
        }

        #endregion

        #region INACTIVAR VENTA
        public static string InactivarVenta(int id_venta)
        {
            DVenta DVenta = new DVenta();
            return DVenta.InactivarVenta(id_venta);
        }

        #endregion

        #region BUSCAR VENTA FINAL

        public static DataTable BuscarVentaFinal(string texto_busqueda,
            out DataTable dtDetallePedido, out DataTable dtDetalleVenta)
        {
            return DVenta.BuscarVentaFinal(texto_busqueda, out dtDetallePedido, out dtDetalleVenta);
        }

        #endregion

        #region BUSCAR VENTA

        public static DataTable BuscarVenta(string tipo_busqueda, string texto_busqueda,
            string fecha1, string fecha2, string hora1, string hora2)
        {
            return DVenta.BuscarVenta(tipo_busqueda,texto_busqueda, fecha1, fecha2, hora1, hora2);
        }

        #endregion
    }
}
