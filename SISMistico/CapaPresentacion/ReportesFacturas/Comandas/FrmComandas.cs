using CapaEntidades.Models;
using CapaNegocio;
using CapaPresentacion.Formularios;
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
                DatosInicioSesion datos = DatosInicioSesion.GetInstancia();
                int num = datos.NumeroComandas + 1;

                string observaciones = "";
                foreach (DataRow row in this.TablaDetallePedido.Rows)
                {
                    string obs = Convert.ToString(row["Observaciones"]);
                    if (!string.IsNullOrEmpty(obs))
                        observaciones += " - " + obs;
                }

                ReportParameter[] reportParameters = new ReportParameter[3];
                reportParameters[0] = new ReportParameter("parameterObservaciones", observaciones);
                reportParameters[1] = new ReportParameter("parameterHora", DateTime.Now.ToShortTimeString());
                reportParameters[2] = new ReportParameter("NumeroComanda", num.ToString());
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
            this.TablaDatosPedido =
                NPedido.BuscarPedidosYDetalle("ID PEDIDO Y DETALLE", Convert.ToString(this.Id_pedido),
                out this.TablaDetallePedido, out DataTable dtDetallePlatosPedido, out string rpta);

            foreach (DataRow row in this.TablaDetallePedido.Rows)
            {
                int id_tipo = Convert.ToInt32(row["Id_tipo"]);
                string tipo = Convert.ToString(row["Tipo"]);
                string nombre = Convert.ToString(row["Nombre"]);

                if (tipo.Equals("PLATO"))
                {
                    StringBuilder info = new StringBuilder();
                    info.Append("-" + nombre);

                    DataRow[] find = dtDetallePlatosPedido.Select(string.Format("Id_tipo = {0}", id_tipo));
                    if (find.Length > 0)
                    {
                        info.Append(": ").Append(Environment.NewLine);
                        foreach (DataRow re in find)
                        {
                            Ingredientes ing = new Ingredientes(re);
                            info.Append(ing.Nombre_ingrediente).Append(Environment.NewLine);
                        }
                    }

                    info.Append(Environment.NewLine);
                    row["Nombre"] = info.ToString();
                }
            }
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
