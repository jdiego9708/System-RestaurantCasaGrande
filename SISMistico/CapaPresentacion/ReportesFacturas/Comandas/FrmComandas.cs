using CapaEntidades.Models;
using CapaNegocio;
using CapaPresentacion.Formularios;
using CapaPresentacion.ReportesFacturas.Comandas;
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
        public string Titulo { get; set; }
        public void ObtenerReporte(string titulo)
        {
            this.Titulo = titulo;

            if (this.Controls.Count > 0)
            {
                this.Controls.Remove(this.reportViewer1);
                this.reportViewer1.LocalReport.ReportEmbeddedResource = null;
                this.reportViewer1.DataBindings.Clear();
                this.reportViewer1.LocalReport.DataSources.Clear();
            }

            this.Controls.Add(this.reportViewer1);
            this.reportViewer1.LocalReport.ReportEmbeddedResource =
                "CapaPresentacion.ReportesFacturas.Comandas.ComandasPedido.rdlc";
        }

        private int ComprobacionNumComandas()
        {
            var fecha = ConfigComandas.Default.Fecha;
            int numcomanda = ConfigComandas.Default.NumComanda;

            if (fecha == null)
            {
                ConfigComandas.Default.Fecha = DateTime.Now.ToString("yyyy-MM-dd");
                numcomanda = 1;
                ConfigComandas.Default.NumComanda = numcomanda;
            }
            else if (string.IsNullOrEmpty(fecha))
            {
                ConfigComandas.Default.Fecha = DateTime.Now.ToString("yyyy-MM-dd");
                numcomanda += 1;
                ConfigComandas.Default.NumComanda = numcomanda;
            }
            else if (DateTime.Now.ToString("yyyy-MM-dd") == fecha.ToString())
            {
                numcomanda += 1;
                ConfigComandas.Default.NumComanda = numcomanda;
            }
            else
            {
                ConfigComandas.Default.Fecha = DateTime.Now.ToString("yyyy-MM-dd");
                numcomanda = 1;
                ConfigComandas.Default.NumComanda = numcomanda;
            }

            ConfigComandas.Default.Save();

            return numcomanda;
        }

        public void ImprimirFactura(int Repetir)
        {
            try
            {

                //Asignar parámetros de observaciones y horas
                int num = this.ComprobacionNumComandas();

                ReportParameter[] reportParameters = new ReportParameter[4];
                reportParameters[0] = new ReportParameter("parameterObservaciones", this.ObservacionesGeneral);
                reportParameters[1] = new ReportParameter("parameterHora", DateTime.Now.ToShortTimeString());
                reportParameters[2] = new ReportParameter("NumeroComanda", num.ToString());
                reportParameters[3] = new ReportParameter("Titulo", Titulo == string.Empty ? "Casa Grande" : Titulo);
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

            int cantidad_productos = 0;
            foreach (DataRow row in this.TablaDetallePedido.Rows)
            {
                int id_tipo = Convert.ToInt32(row["Id_tipo"]);
                string tipo = Convert.ToString(row["Tipo"]);
                string nombre = Convert.ToString(row["Nombre"]);
                string observaciones = Convert.ToString(row["Observaciones"]);
                int cantidad = Convert.ToInt32(row["Cantidad"]);
                cantidad_productos += cantidad;
            
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

                        if (!string.IsNullOrEmpty(observaciones))
                            info.Append("**" + observaciones);
                    }
                    else
                    {
                        info.Append(Environment.NewLine);

                        if (!string.IsNullOrEmpty(observaciones))
                            info.Append("**" + observaciones);
                    }

                    row["Nombre"] = info.ToString();
                }
                else
                {
                    if (!string.IsNullOrEmpty(observaciones))
                        nombre += Environment.NewLine + "**" + observaciones;

                    row["Nombre"] = nombre;
                }
            }

            this.ObservacionesGeneral = "Cantidad de platos/bebidas " + cantidad_productos;
        }

        public void AsignarTablas(DataTable detallepedido)
        {
            this.TablaDetallePedido = null;

            int cantidad_productos = 0;
            foreach (DataRow row in detallepedido.Rows)
            {
                int cantidad = Convert.ToInt32(row["Cantidad"]);
                string nombre = Convert.ToString(row["Nombre"]);
                string observaciones = Convert.ToString(row["Observaciones"]);
                cantidad_productos += cantidad;

                if (!string.IsNullOrEmpty(observaciones))
                    nombre += Environment.NewLine + "**" + observaciones;

                row["Nombre"] = nombre;
            }

            this.ObservacionesGeneral = "Cantidad de platos/bebidas " + cantidad_productos;

            this.TablaDetallePedido = detallepedido;

            this.TablaDatosPedido =
                NPedido.BuscarPedidos("ID PEDIDO", Convert.ToString(this.Id_pedido));
        }

        #region VARIABLES
        ControladorImpresion objImpresion = new ControladorImpresion();
        DataTable TablaDetallePedido;
        DataTable TablaDatosPedido;
        private int _id_pedido;
        public string ObservacionesGeneral { get; set; }
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
