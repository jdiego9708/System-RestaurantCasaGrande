namespace CapaPresentacion.Formularios.FormsEstadisticas.FrmReporteEgresos
{
    using CapaEntidades.Models;
    using CapaNegocio;
    using Microsoft.Reporting.WinForms;
    using System;
    using System.Data;
    using System.Drawing;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    public partial class FrmReporteEgresos : Form
    {
        public FrmReporteEgresos()
        {
            InitializeComponent();
            this.date1.ValueChanged += Date1_ValueChanged;
            this.date2.ValueChanged += Date2_ValueChanged;
        }

        private async void Date2_ValueChanged(object sender, EventArgs e)
        {
            await this.LoadEgresos(this.date1.Value, this.date2.Value);
        }

        private void Date1_ValueChanged(object sender, EventArgs e)
        {
            DateTimePicker dateTime = (DateTimePicker)sender;
            this.date2.MinDate = dateTime.Value;
        }

        public string IdentificacionTurno { get; set; }
        public string FechaHora { get; set; }
        public string InformacionEmpleado { get; set; }
        public string CantidadEgresos { get; set; }
        public string ResumenResultados { get; set; }

        public async Task LoadEgresos(DateTime date1, DateTime date2)
        {
            MensajeEspera.ShowWait("Cargando...");

            DatosInicioSesion datos = DatosInicioSesion.GetInstancia();

            string informacionEmpleado = "Información de empleado que genera el reporte " +
                datos.Nombre_empleado + " - Teléfono: " + datos.EmpleadoLogin.Telefono_empleado;

            string cantidadEgresos = "";

            var (rpta, dtEgresos) =
               await NEgresos.BuscarEgresos("FECHA", date1.ToString("yyyy-MM-dd"), date2.ToString("yyyy-MM-dd"));

            if (dtEgresos == null)
                cantidadEgresos = "No hay egresos en la fecha seleccionada";
            else
                cantidadEgresos = "Cantidad de egresos: " + dtEgresos.Rows.Count + "";

            string id_turno = "0";

            StringBuilder resumenResultados = new StringBuilder();
            decimal total_egresos = 0;

            foreach(DataRow row in dtEgresos.Rows)
            {
                Egresos egreso = new Egresos(row);
                total_egresos += egreso.Valor_egreso;

                resumenResultados.Append("* Fecha ").Append(egreso.Fecha_egreso.ToLongDateString());
                resumenResultados.Append(Environment.NewLine);
                resumenResultados.Append("Concepto/Observaciones: ").Append(egreso.Descripcion_egreso);
                resumenResultados.Append(Environment.NewLine);
                resumenResultados.Append("Valor: ").Append(egreso.Valor_egreso.ToString("C").Replace(",00", ""));
                resumenResultados.Append(Environment.NewLine);
                resumenResultados.Append("Empleado que registra el gasto: ").Append(egreso.Empleado.Nombre_empleado);
                resumenResultados.Append(Environment.NewLine);
            }

            //Obtener el turno
            var (rpta1, dtTurnos) = 
                await NTurnos.BuscarTurnos("ID TURNO", date1.ToString("yyyy-MM-dd"), string.Empty);
            if (dtTurnos != null)
            {
                Turno turno = new Turno(dtTurnos.Rows[0]);
                id_turno = "Identificación del turno " + turno.Id_turno.ToString();
            }

            this.InformacionEmpleado = informacionEmpleado;
            this.CantidadEgresos = cantidadEgresos;
            this.ResumenResultados = resumenResultados.ToString();
            this.IdentificacionTurno = id_turno;
            this.FechaHora = DateTime.Now.ToLongDateString();
            this.ObtenerReporte();
            MensajeEspera.CloseForm();
        }

        private ReportViewer reportViewer1;
        ControladorImpresion objImpresion = new ControladorImpresion();

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
            "CapaPresentacion.Formularios.FormsEstadisticas.FrmReporteEgresos.rptReporteEgresos.rdlc";

            ReportParameter[] reportParameters = new ReportParameter[5];
            reportParameters[0] = new ReportParameter("InformacionEmpleado", InformacionEmpleado);
            reportParameters[1] = new ReportParameter("CantidadEgresos", CantidadEgresos);
            reportParameters[2] = new ReportParameter("ResumenResultados", ResumenResultados);
            reportParameters[3] = new ReportParameter("IdentificacionTurno", IdentificacionTurno);
            reportParameters[4] = new ReportParameter("FechaHora", FechaHora);
            this.reportViewer1.LocalReport.SetParameters(reportParameters);
            this.reportViewer1.RefreshReport();
        }

        public void ImprimirFactura(int Repetir)
        {
            try
            {
                ReportParameter[] reportParameters = new ReportParameter[5];
                reportParameters[0] = new ReportParameter("InformacionEmpleado", InformacionEmpleado);
                reportParameters[1] = new ReportParameter("CantidadEgresos", CantidadEgresos);
                reportParameters[2] = new ReportParameter("ResumenResultados", ResumenResultados);
                reportParameters[3] = new ReportParameter("IdentificacionTurno", IdentificacionTurno);
                reportParameters[4] = new ReportParameter("FechaHora", FechaHora);
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
