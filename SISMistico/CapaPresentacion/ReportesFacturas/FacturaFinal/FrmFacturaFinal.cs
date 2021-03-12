using CapaNegocio;
using Microsoft.Reporting.WinForms;
using System;
using System.Collections.Generic;
using System.Data;
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
                this.Controls.Add(this.reportViewer1);
                this.reportViewer1.LocalReport.ReportEmbeddedResource =
                    "CapaPresentacion.ReportesFacturas.FacturaFinal.FacturaFinal.rdlc";

                ReportDataSource dsDatosPrincipales = new ReportDataSource("DatosPrincipales", this.TablaDatosPrincipales);
                reportViewer1.LocalReport.DataSources.Add(dsDatosPrincipales);

                ReportDataSource dsDetallePedido = new ReportDataSource("DetallePedido", this.TablaDetallePedido);
                reportViewer1.LocalReport.DataSources.Add(dsDetallePedido);

                ReportDataSource dsDetallePago = new ReportDataSource("DetallePago", this.TablaDetalleVenta);
                reportViewer1.LocalReport.DataSources.Add(dsDetallePago);
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorForm(ex.Message);
            }
        }

        public void AsignarTablasPedido(List<string> variables)
        {
            try
            {
                this.Is_precuenta = true;
                this.reportParameters = new ReportParameter[variables.Count];
                reportParameters[0] = new ReportParameter("Hora_pedido", DateTime.Now.ToShortTimeString());
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
                this.TablaDatosPrincipales = NPedido.BuscarPedidosYDetalle("ID PEDIDO",
                    Convert.ToString(this.Id_pedido), out this.TablaDetallePedido, out rpta);
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "AsignarTablasPedido", "Hubo un error al asignar los parámetros y tablas",
                    ex.Message);
            }
            //int rows = this.TablaDetalleVenta.Rows.Count;
        }

        public void AsignarTablas()
        {
            this.Is_precuenta = false;
            this.TablaDatosPrincipales = NVentas.BuscarVentaFinal(Convert.ToString(this.Id_pedido),
                out this.TablaDetallePedido, out this.TablaDetalleVenta);
            int rows = this.TablaDetalleVenta.Rows.Count;
        }

        public void ImprimirFactura(int Repetir)
        {
            try
            {
                this.Controls.Add(this.reportViewer1);

                if (this.Is_precuenta)
                {
                    this.reportViewer1.LocalReport.SetParameters(this.reportParameters);
                    ReportDataSource dsDatosPrincipales = new ReportDataSource("DatosPedido", this.TablaDatosPrincipales);
                    reportViewer1.LocalReport.DataSources.Add(dsDatosPrincipales);

                    ReportDataSource dsDetallePedido = new ReportDataSource("DetallePedido", this.TablaDetallePedido);
                    reportViewer1.LocalReport.DataSources.Add(dsDetallePedido);

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
