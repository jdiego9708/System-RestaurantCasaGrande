using CapaNegocio;
using Microsoft.Reporting.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CapaPresentacion
{
    public partial class FrmComandas : Form
    {
        public FrmComandas()
        {
            InitializeComponent();
        }

        public void ObtenerReporte()
        {
            this.Controls.Add(this.reportViewer1);
            this.reportViewer1.LocalReport.ReportEmbeddedResource =
                "CapaPresentacion.ReportesFacturas.Comandas.ComandasPedido.rdlc";
        }

        public void ImprimirFactura(int Repetir)
        {
            try
            {
               
                //Asignar parámetros de observaciones y horas

                string observaciones = "";
                foreach (DataRow row in this.TablaDetallePedido.Rows)
                {
                    observaciones += " - " + Convert.ToString(row["Observaciones"]);
                }

                ReportParameter[] reportParameters = new ReportParameter[2];
                reportParameters[0] = new ReportParameter("parameterObservaciones", observaciones);
                reportParameters[1] = new ReportParameter("parameterHora", DateTime.Now.ToShortTimeString());
                this.reportViewer1.LocalReport.SetParameters(reportParameters);

                ReportDataSource dsDatosPedido = new ReportDataSource("DatosPedido", this.TablaDatosPedido);
                reportViewer1.LocalReport.DataSources.Add(dsDatosPedido);

                ReportDataSource dsDetallePedido = new ReportDataSource("DetallePedido", this.TablaDetallePedido);
                reportViewer1.LocalReport.DataSources.Add(dsDetallePedido);

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

        public void AsignarTablas()
        {
            string rpta = "";
            this.TablaDatosPedido =
                NPedido.BuscarPedidosYDetalle("ID PEDIDO Y DETALLE", Convert.ToString(this.Id_pedido),
                out this.TablaDetallePedido, out rpta);
        }

        public void AsignarTablas(DataTable detallepedido)
        {
            this.TablaDetallePedido = detallepedido;

            this.TablaDatosPedido = 
                NPedido.BuscarPedidos("ID PEDIDO", Convert.ToString(this.Id_pedido));
        }

        #region VARIABLES
        ControladorImpresion objImpresion = new ControladorImpresion();
        DataTable TablaDetallePedido;
        DataTable TablaDatosPedido;
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
        #endregion
    }
}
