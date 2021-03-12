using CapaEntidades.Models;
using CapaNegocio;
using Microsoft.Reporting.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CapaPresentacion.Formularios.FormsEstadisticas
{
    public partial class FrmReporteDiario : Form
    {
        public FrmReporteDiario()
        {
            InitializeComponent();
            this.Load += FrmReporteDiario_Load;
            this.date1.ValueChanged += Date1_ValueChanged;
            this.date2.ValueChanged += Date2_ValueChanged;
        }

        private async void Date2_ValueChanged(object sender, EventArgs e)
        {
            await this.LoadEstadistica(this.date1.Value, this.date2.Value);
        }

        private void Date1_ValueChanged(object sender, EventArgs e)
        {
            DateTimePicker date = (DateTimePicker)sender;
            this.date2.MinDate = date.Value;
        }

        private void FrmReporteDiario_Load(object sender, EventArgs e)
        {
            this.reportViewer1 = new ReportViewer();
            this.Controls.Add(this.reportViewer1);
            this.reportViewer1.LocalReport.ReportEmbeddedResource =
                "CapaPresentacion.ReportesFacturas.NominaEmpleado.rptNominaEmpleado.rdlc";
        }

        public string InformacionEmpleado { get; set; }
        public string CantidadPedidos { get; set; }
        public string ResumenResultados { get; set; }
        public string InformacionEgresos { get; set; }
        public string IdentificacionTurno { get; set; }
        public string FechaHora { get; set; }

        private ReportViewer reportViewer1;
        ControladorImpresion objImpresion = new ControladorImpresion();

        public async Task LoadEstadistica(DateTime date1, DateTime date2)
        {
            MensajeEspera.ShowWait("Cargando...");

            DatosInicioSesion datos = DatosInicioSesion.GetInstancia();

            string informacionEmpleado = "Información de empleado que genera el reporte " +
                datos.Nombre_empleado + " - Teléfono: " + datos.Empleado.Telefono_empleado;
            string cantidadPedidos = "";
            var (rpta, dtPedidos) =
               await NPedido.BuscarPedidos("RANGO FECHAS", date1.ToString("yyyy-MM-dd"), date2.ToString("yyyy-MM-dd"));
            if (dtPedidos == null)
                cantidadPedidos = "No hay ventas el día de hoy";
            else
                cantidadPedidos = "Cantidad de ventas: " + dtPedidos.Rows.Count + "";

            string id_turno = "0";
            StringBuilder resumenResultados = new StringBuilder();

            DataTable dtEstadistica;
            DataTable dtDetalle;
            bool isRango = false;

            if (date1.ToString("yyyy-MM-dd") == date2.ToString("yyyy-MM-dd"))
            {
                isRango = false;
                var result =
                await NNomina.EstadisticasDiarias(datos.Turno.Id_turno, date1.ToString("yyyy-MM-dd"));
                dtEstadistica = result.dtEstadistica;
                dtDetalle = result.dtDetalle;
            }
            else
            {
                isRango = true;
                var result =
                await NNomina.EstadisticasDiarias(date1.ToString("yyyy-MM-dd"), date2.ToString("yyyy-MM-dd"));
                dtEstadistica = result.dtEstadistica;
                dtDetalle = result.dtDetalle;
            }


            if (dtEstadistica != null)
            {
                Turno turno = new Turno(dtEstadistica.Rows[0]);
                id_turno = "Identificación del turno: " + turno.Id_turno.ToString();


                if (!isRango)
                    resumenResultados.Append("Valor inicial: ").Append(turno.Valor_inicial.ToString("C")).Append(Environment.NewLine);
                else
                    resumenResultados.Append("Entre ").Append(date1.ToLongDateString() + " y ").Append(date2.ToLongDateString()).Append(" se registra la siguiente información:").Append(Environment.NewLine);

                resumenResultados.Append("Total ingresos: ").Append(turno.Total_ingresos.ToString("C")).Append(Environment.NewLine);
                resumenResultados.Append("Total egresos: ").Append(turno.Total_egresos.ToString("C")).Append(Environment.NewLine);
                resumenResultados.Append("Total ventas: ").Append(turno.Total_ventas.ToString("C")).Append(Environment.NewLine);
                resumenResultados.Append("Total nomina: ").Append(turno.Total_nomina.ToString("C")).Append(Environment.NewLine);
                resumenResultados.Append("Total: ").Append(turno.Total_turno.ToString("C")).Append(Environment.NewLine);

                List<TipoResumen> resumen = new List<TipoResumen>();
                //PLATOS Y BEBIDAS
                foreach (DataRow row in dtDetalle.Rows)
                {
                    int id_tipo = Convert.ToInt32(row["Id_tipo"]);
                    int cantidad = Convert.ToInt32(row["Cantidad"]);
                    string nombre = Convert.ToString(row["Nombre"]);
                    decimal precio = Convert.ToDecimal(row["Precio"]) * cantidad;

                    List<TipoResumen> results = resumen.Where(x => x.Id_tipo == id_tipo).ToList();
                    if (results.Count > 0)
                    {
                        results[0].Cantidad += cantidad;
                        results[0].Valor_total += precio;
                    }
                    else
                    {
                        TipoResumen tipo = new TipoResumen
                        {
                            Id_tipo = id_tipo,
                            Nombre = nombre,
                            Cantidad = cantidad,
                            Valor_total = precio,
                        };
                        resumen.Add(tipo);
                    }
                }

                if (resumen.Count > 0)
                {
                    IOrderedEnumerable<TipoResumen> detallesOrdenados = from s in resumen
                                            orderby s.Cantidad descending
                                                  select s;

                    resumenResultados.Append("Detalles del día: ").Append(Environment.NewLine);

                    foreach (TipoResumen re in detallesOrdenados)
                    {
                        resumenResultados.Append("* " + re.Nombre + " - Cantidad ").Append(re.Cantidad);
                        resumenResultados.Append(" - Valor total: ").Append(re.Valor_total.ToString("C"));
                        resumenResultados.Append(Environment.NewLine);
                    }
                }
                else
                {
                    resumenResultados.Append("No se encontraron detalles");
                }

            }
            else
            {
                resumenResultados.Append("Hubo un error al encontrar los datos de estadísticas diarias");
            }

            StringBuilder infoEgresos = new StringBuilder();

            DataTable dtEgresos = null;

            if (isRango)
            {
                var result = await NEgresos.BuscarEgresos("RANGO FECHAS", date1.ToString("yyyy-MM-dd"), date2.ToString("yyyy-MM-dd"));
                dtEgresos = result.dtEgresos;
            }
            else
            {
                var result = await NEgresos.BuscarEgresos("FECHA", date1.ToString("yyyy-MM-dd"));
                dtEgresos = result.dtEgresos;
            }

            if (dtEgresos != null)
            {
                infoEgresos.Append("Descripción de los egresos: ").Append(Environment.NewLine);
                int contador = 0;
                foreach (DataRow row in dtEgresos.Rows)
                {
                    contador += 1;
                    Egresos egreso = new Egresos(row);
                    infoEgresos.Append(contador + ") Fecha: ").Append(egreso.Fecha_egreso.ToLongDateString()).Append(" - ");
                    infoEgresos.Append("Valor: ").Append(egreso.Valor_egreso.ToString("C")).Append(" - ");
                    infoEgresos.Append("Observaciones: ").Append(egreso.Descripcion_egreso).Append(Environment.NewLine);
                }
            }
            else
                infoEgresos.Append("No hay descrición adicional");

            this.InformacionEmpleado = informacionEmpleado;
            this.CantidadPedidos = cantidadPedidos;
            this.ResumenResultados = resumenResultados.ToString();
            this.InformacionEgresos = infoEgresos.ToString();
            this.IdentificacionTurno = id_turno;
            this.FechaHora = DateTime.Now.ToLongDateString();
            this.ObtenerReporte();
            MensajeEspera.CloseForm();
        }

        public class TipoResumen
        {
            public int Id_tipo { get; set; }
            public string Nombre { get; set; }
            public int Cantidad { get; set; }
            public decimal Valor_total { get; set; }
        }

        public void ObtenerReporte()
        {
            if (this.gbReporte.Controls.Count > 0)
                this.gbReporte.Controls.Clear();

            this.reportViewer1 = new ReportViewer
            {
                Dock = DockStyle.Fill,
                Location = new Point(0, 0),
            };
            this.gbReporte.Controls.Add(this.reportViewer1);
            this.reportViewer1.LocalReport.ReportEmbeddedResource =
            "CapaPresentacion.Formularios.FormsEstadisticas.rptReporteDiario.rdlc";

            ReportParameter[] reportParameters = new ReportParameter[6];
            reportParameters[0] = new ReportParameter("InformacionEmpleado", InformacionEmpleado);
            reportParameters[1] = new ReportParameter("CantidadPedidos", CantidadPedidos);
            reportParameters[2] = new ReportParameter("ResumenResultados", ResumenResultados);
            reportParameters[3] = new ReportParameter("InformacionEgresos", InformacionEgresos);
            reportParameters[4] = new ReportParameter("IdentificacionTurno", IdentificacionTurno);
            reportParameters[5] = new ReportParameter("FechaHora", FechaHora);
            this.reportViewer1.LocalReport.SetParameters(reportParameters);
            this.reportViewer1.RefreshReport();
        }

        public void ImprimirFactura(int Repetir)
        {
            try
            {
                ReportParameter[] reportParameters = new ReportParameter[6];
                reportParameters[0] = new ReportParameter("InformacionEmpleado", InformacionEmpleado);
                reportParameters[1] = new ReportParameter("CantidadPedidos", CantidadPedidos);
                reportParameters[2] = new ReportParameter("ResumenResultados", ResumenResultados);
                reportParameters[3] = new ReportParameter("InformacionEgresos", InformacionEgresos);
                reportParameters[4] = new ReportParameter("IdentificacionTurno", IdentificacionTurno);
                reportParameters[5] = new ReportParameter("FechaHora", FechaHora);
                this.reportViewer1.LocalReport.SetParameters(reportParameters);
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

    }
}
