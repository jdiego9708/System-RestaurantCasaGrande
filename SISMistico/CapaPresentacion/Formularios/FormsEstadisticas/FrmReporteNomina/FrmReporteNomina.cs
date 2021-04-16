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

namespace CapaPresentacion.Formularios.FormsEstadisticas.FrmReporteNomina
{
    public partial class FrmReporteNomina : Form
    {
        public FrmReporteNomina()
        {
            InitializeComponent();
        }

        private void Date2_ValueChanged(object sender, EventArgs e)
        {
            //await this.LoadEgresos(this.date1.Value, this.date2.Value);
        }

        private void Date1_ValueChanged(object sender, EventArgs e)
        {
            DateTimePicker dateTime = (DateTimePicker)sender;
            this.date2.MinDate = dateTime.Value;
        }

        public string IdentificacionTurno { get; set; }
        public string FechaHora { get; set; }
        public string InformacionEmpleado { get; set; }
        public string CantidadNomina { get; set; }
        public string ResumenResultados { get; set; }


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
            reportParameters[1] = new ReportParameter("CantidadEgresos", CantidadNomina);
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
                reportParameters[1] = new ReportParameter("CantidadEgresos", CantidadNomina);
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
