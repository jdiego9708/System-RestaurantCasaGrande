using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using CapaNegocio;

namespace CapaPresentacion.Formularios.FormsVentas
{
    public partial class ResumenVenta : UserControl
    {
        public ResumenVenta()
        {
            InitializeComponent();
        }

        public void ObtenerVenta(DateTime fecha1, DateTime fecha2, string hora1, string hora2)
        {
            try
            {
                DataTable table = NVentas.BuscarVenta("RESUMEN VENTA", "", 
                    fecha1.ToShortDateString(), fecha2.ToShortDateString(), 
                    hora1, hora2);
                if (table != null)
                {
                    int cantidadVentas = Convert.ToInt32(table.Rows[0]["Cantidad_ventas"]);
                    int totalVentas = Convert.ToInt32(table.Rows[0]["Total_ventas"]);
                    int totalPropinas = Convert.ToInt32(table.Rows[0]["Total_propinas"]);
                    int totalVentasSinPropina = Convert.ToInt32(table.Rows[0]["Total_ventas_sin_propina"]);
                    int pagosEfectivo = Convert.ToInt32(table.Rows[0]["Pagos_efectivo"]);
                    int pagosTarjeta = Convert.ToInt32(table.Rows[0]["Pagos_tarjeta"]);
                                     
                    StringBuilder builder = new StringBuilder();
                    builder.Append("Entre " + fecha1.ToLongDateString() + " y " + fecha2.ToLongDateString() +
                        " se realizaron " + cantidadVentas + " ventas");
                    builder.Append(Environment.NewLine);
                    builder.Append("El total de ventas fue de " + totalVentas.ToString("C").Replace(",00", ""));
                    builder.Append(Environment.NewLine);
                    builder.Append("El total de propinas fue de " + totalPropinas.ToString("C").Replace(",00", ""));
                    builder.Append(Environment.NewLine);
                    builder.Append("El total de ventas sin propinas fue de " + totalVentasSinPropina.ToString("C").Replace(",00", ""));
                    builder.Append(Environment.NewLine);

                    if (pagosEfectivo > 0)
                    {
                        int totalPagosEfectivo = Convert.ToInt32(table.Rows[0]["Total_ventas_efectivo"]);
                        builder.Append("Pagos en efectivo: " + pagosEfectivo + " por un valor de " + totalPagosEfectivo.ToString("C2"));
                    }
                    else
                    {
                        builder.Append("No hubo pagos en efectivo");
                    }
                        
                    builder.Append(Environment.NewLine);

                    if (pagosTarjeta > 0)
                    {
                        int totalPagosTarjeta = Convert.ToInt32(table.Rows[0]["Total_ventas_tarjeta"]);
                        builder.Append("Pagos con tarjeta: " + pagosTarjeta + " por un valor de " + totalPagosTarjeta.ToString("C2"));
                    }
                    else
                    {
                        builder.Append("No se encontraron pagos con tarjeta");
                    }

                    this.txtResumen.Text = Convert.ToString(builder);
                }
                else
                {
                    StringBuilder builder = new StringBuilder();
                    builder.Append("Entre " + fecha1.ToLongDateString() + " y " + fecha2.ToLongDateString() +
                        " no se realizaron ventas");
                }
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorForm("Hubo un error al obtener la venta Detalle:" + ex.Message);
            }
        }
    }
}
