using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CapaEntidades.Models;
using Microsoft.Reporting.WinForms;

namespace CapaPresentacion.Formularios.FormsDomicilios
{
    /// <summary>
    /// Imprime una orden de Rappi como ticket termico (rollo de 58 u 80 mm) con lo guardado de su factura PDF. El PDF
    /// original es de tamano carta y reducido a un rollo quedaria ilegible; el ticket lleva los mismos datos con el
    /// diseno de los demas recibos del programa (logo, productos con sus opciones, valores, forma de pago y codigo de
    /// entrega).
    /// </summary>
    public static class RappiTicketPrinter
    {
        private const string Resource = "CapaPresentacion.ReportesFacturas.FacturaFinal.RappiTicket.rdlc";
        private static readonly CultureInfo Colombia = CultureInfo.GetCultureInfo("es-CO");

        /// <summary>Envia el ticket a la impresora predeterminada, igual que las comandas y facturas.</summary>
        public static void Print(RappiOrder order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            // El visor se arma dentro de un formulario que nunca se muestra, como en FrmFacturaFinal.
            using (Form host = new Form())
            using (ReportViewer viewer = new ReportViewer())
            {
                host.Controls.Add(viewer);
                Configure(viewer.LocalReport, order);
                viewer.RefreshReport();
                using (ControladorImpresion printer = new ControladorImpresion())
                    printer.Imprimir(viewer.LocalReport);
            }
        }

        /// <summary>Carga el reporte con los datos de la orden (tambien lo usan las pruebas de renderizado).</summary>
        public static void Configure(LocalReport report, RappiOrder order)
        {
            report.ReportEmbeddedResource = Resource;

            // Numeracion propia de Rappi, separada de la de los domicilios del programa (1..N por dia).
            if (!order.DayNumber.HasValue)
                order.DayNumber = CapaNegocio.NRappiOrders.EnsureDayNumber(order.ExternalId);

            DateTime? moment = order.OrderDateTime;
            report.SetParameters(new List<ReportParameter>
            {
                new ReportParameter("RappiNumber", order.DayNumber.HasValue ? order.DayNumber.Value.ToString(CultureInfo.InvariantCulture) : string.Empty),
                new ReportParameter("RappiOrderId", order.ExternalId ?? string.Empty),
                new ReportParameter("RappiDate", moment.HasValue ? moment.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : string.Empty),
                new ReportParameter("RappiTime", moment.HasValue ? moment.Value.ToString("h:mm tt", CultureInfo.InvariantCulture) : "sin hora"),
                new ReportParameter("RappiPlatform", order.Platform ?? string.Empty),
                new ReportParameter("RappiCustomer", order.CustomerName ?? string.Empty),
                new ReportParameter("RappiSubtotal", Money(order.Subtotal)),
                new ReportParameter("RappiDiscount", Money(order.Discount)),
                new ReportParameter("RappiShipping", Money(order.ShippingCost)),
                new ReportParameter("RappiTip", Money(order.Tip)),
                new ReportParameter("RappiServiceFee", Money(order.ServiceFee)),
                new ReportParameter("RappiPaidByRappi", Money(order.PaidThroughRappi)),
                new ReportParameter("RappiTotal", Money(order.Total)),
                new ReportParameter("RappiPaymentMethod", order.PaymentMethod ?? string.Empty),
                new ReportParameter("RappiCashLine", CashLine(order)),
                new ReportParameter("RappiDeliveryCode", order.DeliveryCode ?? string.Empty),
                new ReportParameter("DeliveryPhone", order.Phone ?? string.Empty),
                new ReportParameter("DeliveryAddress", order.Address ?? string.Empty),
                new ReportParameter("DeliveryNeighborhood", string.Empty),
                new ReportParameter("DeliveryReference", string.Empty),
            });

            report.DataSources.Clear();
            report.DataSources.Add(new ReportDataSource("DatosPrincipales", EmptyHeader()));
            report.DataSources.Add(new ReportDataSource("DetallePedido", ItemsTable(order)));
        }

        // "$45.250"; los descuentos de Rappi ya traen su signo ("-$21.950").
        private static string Money(int? value)
        {
            int amount = value ?? 0;
            return (amount < 0 ? "-" : string.Empty) + "$" + Math.Abs(amount).ToString("N0", Colombia);
        }

        private static string CashLine(RappiOrder order)
        {
            if (!order.CashPayWith.HasValue || order.CashPayWith.Value <= 0)
                return string.Empty;
            return "Paga con " + Money(order.CashPayWith) + (order.ChangeToBring.HasValue ? " · cambio " + Money(order.ChangeToBring) : string.Empty);
        }

        // Las columnas son las que declara el reporte (heredado del de domicilios); todas como texto.
        private static DataTable EmptyHeader()
        {
            DataTable table = new DataTable("DatosPrincipales");
            foreach (string name in new[] { "Id_pedido", "Fecha_pedido", "Hora_pedido", "Id_mesa", "Mesa", "Id_empleado",
                "Nombre_empleado", "Id_cliente", "Cliente", "Num_mesa" })
                table.Columns.Add(name, typeof(string));
            return table;
        }

        private static DataTable ItemsTable(RappiOrder order)
        {
            DataTable table = new DataTable("DetallePedido");
            foreach (string name in new[] { "Id_pedido", "Id_tipo", "Tipo", "Nombre", "Precio", "Cantidad", "Total", "Observaciones" })
                table.Columns.Add(name, typeof(string));
            foreach (RappiOrderItem item in order.Items)
            {
                // Las opciones del menu van debajo del nombre, una por linea.
                string name = item.Name + string.Concat(item.Notes.Select(note => Environment.NewLine + "· " + note));
                int quantity = Math.Max(1, item.Quantity);
                DataRow row = table.NewRow();
                row["Id_pedido"] = order.ExternalId;
                row["Nombre"] = name;
                row["Precio"] = ((int)Math.Round(item.Price / (double)quantity)).ToString(CultureInfo.InvariantCulture);
                row["Cantidad"] = quantity.ToString(CultureInfo.InvariantCulture);
                row["Total"] = item.Price.ToString(CultureInfo.InvariantCulture);
                row["Observaciones"] = string.Empty;
                table.Rows.Add(row);
            }
            return table;
        }
    }
}
