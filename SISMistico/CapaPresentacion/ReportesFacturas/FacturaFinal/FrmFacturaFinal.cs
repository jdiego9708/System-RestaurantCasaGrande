using CapaNegocio;
using CapaPresentacion.ReportesFacturas.Comandas;
using CapaPresentacion.ReportesFacturas.FacturaFinal;
using Microsoft.Reporting.WinForms;
using Microsoft.ReportingServices.ReportProcessing.OnDemandReportObjectModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace CapaPresentacion
{
    public partial class FrmFacturaFinal : Form
    {
        public FrmFacturaFinal()
        {
            InitializeComponent();
            this.Load += FrmFacturaFinal_Load;
        }

        public void AsignarReporte()
        {
            this.Controls.Add(this.reportViewer1);

            if (this.Is_precuenta)
            {
                this.reportViewer1.LocalReport.ReportEmbeddedResource =
                    "CapaPresentacion.ReportesFacturas.FacturaFinal.Precuenta.rdlc";
            }
            else
            {
                this.reportViewer1.LocalReport.ReportEmbeddedResource =
                    "CapaPresentacion.ReportesFacturas.FacturaFinal.FacturaFinal.rdlc";
            }
        }

        ReportParameter[] reportParameters;

        private void FrmFacturaFinal_Load(object sender, EventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorForm(ex.Message);
            }
        }

        /// <summary>
        /// Hora de la precuenta en 12 horas con AM/PM (por ejemplo "6:53 PM"), siempre igual y sin depender del
        /// formato de hora de Windows (que en español de Colombia daría "p. m." o 24 horas según el equipo).
        /// </summary>
        public static string HoraPrecuenta(DateTime hora)
        {
            return hora.ToString("h:mm tt", CultureInfo.InvariantCulture);
        }

        public void AsignarTablasPrecuenta(List<string> variables)
        {
            try
            {
                this.reportParameters = new ReportParameter[variables.Count];
                reportParameters[0] = new ReportParameter("Hora_pedido", HoraPrecuenta(DateTime.Now));
                reportParameters[1] = new ReportParameter("Total_parcial", variables[1]);
                reportParameters[2] = new ReportParameter("Propina_pedido", variables[2]);
                reportParameters[3] = new ReportParameter("Subtotal", variables[3]);
                reportParameters[4] = new ReportParameter("Descuento", variables[4]);
                reportParameters[5] = new ReportParameter("Total", variables[8]);
                reportParameters[6] = new ReportParameter("Cupon", variables[5]);
                reportParameters[7] = new ReportParameter("Observaciones", variables[9]);
                reportParameters[8] = new ReportParameter("Precio_desechable", variables[6]);
                reportParameters[9] = new ReportParameter("PrecioDomicilio", variables[7]);

                string rpta;
                this.TablaDatosPrincipales = NPedido.BuscarPedidosYDetalle("ID PEDIDO Y DETALLE",
                    Convert.ToString(this.Id_pedido),
                    out this.TablaDetallePedido,
                    out DataTable dtDetallePlatosPedido, out rpta);

                // El domicilio usa su propia precuenta: con datos de entrega y sin mensaje de propina.
                if (this.TablaDatosPrincipales != null && this.TablaDatosPrincipales.Rows.Count > 0
                    && this.TablaDatosPrincipales.Columns.Contains("Mesa")
                    && "DOMICILIO".Equals(Convert.ToString(this.TablaDatosPrincipales.Rows[0]["Mesa"]),
                        StringComparison.OrdinalIgnoreCase))
                {
                    this.reportViewer1.LocalReport.ReportEmbeddedResource =
                        "CapaPresentacion.ReportesFacturas.FacturaFinal.DeliveryPreBill.rdlc";
                    this.AddProductDetails();
                    ReportParameter[] extended = new ReportParameter[this.reportParameters.Length + 6];
                    this.reportParameters.CopyTo(extended, 0);
                    this.DeliveryParameters().CopyTo(extended, this.reportParameters.Length);
                    this.reportParameters = extended;
                }
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "AsignarTablasPedido", "Hubo un error al asignar los parámetros y tablas",
                    ex.Message);
            }
            //int rows = this.TablaDetalleVenta.Rows.Count;
        }

        public void AsignarTablasCuentaFinal(int? saleId = null)
        {
            this.Is_precuenta = false;
            this.TablaDatosPrincipales = NVentas.BuscarVentaFinal(Convert.ToString(this.Id_pedido),
                out this.TablaDetallePedido, out this.TablaDetalleVenta);
            if (saleId.HasValue)
            {
                // En la reimpresion del historial se usa solo la venta elegida, aunque el pedido tenga mas de una.
                this.TablaDatosPrincipales = SelectSale(this.TablaDatosPrincipales, saleId.Value);
                this.TablaDetalleVenta = SelectSale(this.TablaDetalleVenta, saleId.Value);
                if (this.TablaDatosPrincipales.Rows.Count == 0)
                    throw new InvalidOperationException("No se encontró la venta seleccionada para reimprimir.");
            }
            int rows = this.TablaDetalleVenta.Rows.Count;

            this.Controls.Add(this.reportViewer1);

            // En el reporte de la venta, Num_mesa trae "DOMICILIO" cuando el pedido es de domicilio.
            bool isDelivery = this.TablaDatosPrincipales != null && this.TablaDatosPrincipales.Rows.Count > 0
                && this.TablaDatosPrincipales.Columns.Contains("Num_mesa")
                && "DOMICILIO".Equals(Convert.ToString(this.TablaDatosPrincipales.Rows[0]["Num_mesa"]),
                    StringComparison.OrdinalIgnoreCase);
            int? receiptNumber = !isDelivery && this.TablaDatosPrincipales.Rows.Count > 0
                ? NVentas.GetReceiptNumber(Convert.ToInt32(this.TablaDatosPrincipales.Rows[0]["Id_venta"])) : null;
            List<ReportParameter> parameters = new List<ReportParameter>
            {
                new ReportParameter("Titulo", isDelivery ? "DOMICILIO - FACTURA" : "Restaurante Casa Grande"),
                new ReportParameter("NumFactura", receiptNumber.HasValue
                    ? "N - " + receiptNumber.Value.ToString(CultureInfo.InvariantCulture) : string.Empty),
            };
            if (isDelivery)
            {
                // Factura propia del domicilio: con datos de entrega y sin mensaje de propina.
                this.reportViewer1.LocalReport.ReportEmbeddedResource =
                    "CapaPresentacion.ReportesFacturas.FacturaFinal.DeliveryInvoice.rdlc";
                parameters.AddRange(this.DeliveryParameters());
                this.AddProductDetails();
            }
            this.reportParameters = parameters.ToArray();

            this.reportViewer1.LocalReport.SetParameters(this.reportParameters);

            ReportDataSource dsDatosPrincipales = new ReportDataSource("DatosPrincipales", this.TablaDatosPrincipales);
            reportViewer1.LocalReport.DataSources.Add(dsDatosPrincipales);

            ReportDataSource dsDetallePedido = new ReportDataSource("DetallePedido", this.TablaDetallePedido);
            reportViewer1.LocalReport.DataSources.Add(dsDetallePedido);

            ReportDataSource dsDetallePago = new ReportDataSource("DetallePago", this.TablaDetalleVenta);
            reportViewer1.LocalReport.DataSources.Add(dsDetallePago);
        }

        // Solo en las facturas de domicilio: cada producto lleva su detalle como en las comandas de mesa. Debajo de un plato
        // con ingredientes salen sus ingredientes (sopa, crema, arroz, acompanantes...) y, justo despues, la observacion
        // que se escribio para ESE producto al pedirlo ("**observacion"); las bebidas llevan solo su observacion. Las
        // facturas de mesa no cambian.
        private void AddProductDetails()
        {
            if (this.TablaDetallePedido == null || !this.TablaDetallePedido.Columns.Contains("Id_detalle_pedido")
                || !this.TablaDetallePedido.Columns.Contains("Nombre"))
                return;
            DataTable ingredients = NPedido.GetIngredientLines(this.Id_pedido);
            bool hasNotes = this.TablaDetallePedido.Columns.Contains("Observaciones");
            foreach (DataRow row in this.TablaDetallePedido.Rows)
            {
                bool isDish = !this.TablaDetallePedido.Columns.Contains("Tipo")
                    || string.Equals(Convert.ToString(row["Tipo"]), "PLATO", StringComparison.OrdinalIgnoreCase);
                DataRow[] lines = isDish && ingredients != null
                    ? ingredients.Select("Id_detalle_pedido = " + Convert.ToInt32(row["Id_detalle_pedido"]))
                    : new DataRow[0];
                string note = hasNotes ? Convert.ToString(row["Observaciones"]).Trim() : string.Empty;
                if (lines.Length == 0 && note.Length == 0)
                    continue;

                System.Text.StringBuilder name = new System.Text.StringBuilder(Convert.ToString(row["Nombre"]));
                if (lines.Length > 0)
                {
                    name.Append(':');
                    foreach (DataRow line in lines)
                        name.Append(Environment.NewLine).Append("- ").Append(Convert.ToString(line["Nombre_ingrediente"]));
                }
                if (note.Length > 0)
                    name.Append(Environment.NewLine).Append("**").Append(note);
                row["Nombre"] = name.ToString();
            }
        }

        // Telefono, direccion, barrio y referencia de entrega del pedido para el reporte de domicilio.
        private ReportParameter[] DeliveryParameters()
        {
            string phone = string.Empty, address = string.Empty, neighborhood = string.Empty, reference = string.Empty;
            DataTable contact = NAddresses.GetDeliveryContact(this.Id_pedido);
            if (contact != null && contact.Rows.Count > 0)
            {
                DataRow row = contact.Rows[0];
                phone = Convert.ToString(row["Telefono"]).Trim();
                address = Convert.ToString(row["Direccion"]).Trim();
                neighborhood = Convert.ToString(row["Barrio"]).Trim();
                reference = Convert.ToString(row["Referencia"]).Trim();
            }
            string paymentMethod = NPedido.GetPaymentMethod(this.Id_pedido);
            // Numero de factura del dia de este domicilio (1..N): se asigna al crearlo y las reimpresiones lo conservan.
            int? number = NPedido.GetOrAssignDeliveryNumber(this.Id_pedido);
            return new[]
            {
                new ReportParameter("DeliveryPhone", phone),
                new ReportParameter("DeliveryAddress", address),
                new ReportParameter("DeliveryNeighborhood", neighborhood),
                new ReportParameter("DeliveryReference", reference),
                new ReportParameter("DeliveryPaymentMethod", paymentMethod),
                new ReportParameter("DeliveryNumber", number.HasValue ? number.Value.ToString(CultureInfo.InvariantCulture) : string.Empty),
            };
        }

        private static DataTable SelectSale(DataTable table, int saleId)
        {
            if (table == null || !table.Columns.Contains("Id_venta"))
                throw new InvalidOperationException("No se pudieron consultar los datos de la venta seleccionada.");
            DataTable filtered = table.Clone();
            foreach (DataRow row in table.Select("Id_venta = " + saleId)) filtered.ImportRow(row);
            return filtered;
        }

        public void ImprimirFactura(int Repetir)
        {
            try
            {
                this.Controls.Add(this.reportViewer1);

                if (this.Is_precuenta)
                {
                    this.reportViewer1.LocalReport.SetParameters(this.reportParameters);
                    ReportDataSource dsDatosPrincipales = new ReportDataSource("DatosPrincipales", this.TablaDatosPrincipales);

                    ReportDataSource dsDetallePedido = new ReportDataSource("DetallePedido", this.TablaDetallePedido);
                    reportViewer1.LocalReport.DataSources.Add(dsDetallePedido);
                    reportViewer1.LocalReport.DataSources.Add(dsDatosPrincipales);
                }
                else
                {
                    ReportDataSource dsDatosPrincipales = new ReportDataSource("DatosPrincipales", this.TablaDatosPrincipales);
                    reportViewer1.LocalReport.DataSources.Add(dsDatosPrincipales);

                    ReportDataSource dsDetallePedido = new ReportDataSource("DetallePedido", this.TablaDetallePedido);
                    reportViewer1.LocalReport.DataSources.Add(dsDetallePedido);

                    ReportDataSource dsDetallePago = new ReportDataSource("DetallePago", this.TablaDetalleVenta);
                    reportViewer1.LocalReport.DataSources.Add(dsDetallePago);
                }

                this.reportViewer1.RefreshReport();

                int contador = 0;
                while (contador != Repetir)
                {
                    objImpresion.Imprimir(reportViewer1.LocalReport);
                    objImpresion.Dispose();

                    contador += 1;
                }
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorForm(ex.Message);
            }
        }

        /// <summary>
        /// Renderiza la PRECUENTA a PDF y la guarda en <paramref name="carpeta"/> (la crea si no existe). Es lo mismo
        /// que ImprimirFactura hace con la precuenta (mismo reporte, mismos parámetros y datos) pero sin mandarla a la
        /// impresora: la API móvil lo usa cuando la impresora predeterminada es de PDF, para que no aparezca el diálogo
        /// de "Guardar como". Hay que llamar antes a AsignarReporte y AsignarTablasPrecuenta. Devuelve la ruta.
        /// </summary>
        public string GuardarPdf(string carpeta, string nombreArchivo)
        {
            if (!this.Is_precuenta)
                throw new InvalidOperationException("GuardarPdf solo se usa con la precuenta.");

            this.Controls.Add(this.reportViewer1);
            this.reportViewer1.LocalReport.SetParameters(this.reportParameters);
            this.reportViewer1.LocalReport.DataSources.Clear();
            this.reportViewer1.LocalReport.DataSources.Add(new ReportDataSource("DatosPrincipales", this.TablaDatosPrincipales));
            this.reportViewer1.LocalReport.DataSources.Add(new ReportDataSource("DetallePedido", this.TablaDetallePedido));

            string mimeType, encoding, extension;
            string[] streams;
            Warning[] warnings;
            byte[] bytes = this.reportViewer1.LocalReport.Render("PDF", null,
                out mimeType, out encoding, out extension, out streams, out warnings);

            Directory.CreateDirectory(carpeta);
            string ruta = Path.Combine(carpeta, nombreArchivo + ".pdf");
            File.WriteAllBytes(ruta, bytes);
            return ruta;
        }

        #region VARIABLES
        ControladorImpresion objImpresion = new ControladorImpresion();
        DataTable TablaDatosPrincipales;
        DataTable TablaDetallePedido;
        DataTable TablaDetalleVenta;
        private int _id_pedido;

        public int Id_pedido
        {
            get
            {
                return _id_pedido;
            }

            set
            {
                _id_pedido = value;
            }
        }

        public bool Is_precuenta { get => _is_precuenta; set => _is_precuenta = value; }

        private bool _is_precuenta;
        #endregion
    }
}
