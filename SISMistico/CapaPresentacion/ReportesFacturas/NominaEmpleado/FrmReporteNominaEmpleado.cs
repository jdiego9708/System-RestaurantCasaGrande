using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Microsoft.Reporting.WinForms;

namespace CapaPresentacion.ReportesFacturas.NominaEmpleado
{
    public partial class FrmReporteNominaEmpleado : Form
    {
        public FrmReporteNominaEmpleado()
        {
            InitializeComponent();
            this.Load += FrmReporteNominaEmpleado_Load;
        }

        public string FechaHora { get; set; }
        public string InformacionEmpleado { get; set; }
        public string Turno { get; set; }
        public string Platos { get; set; }
        public string Servicios { get; set; }
        public string OtrosIngresos { get; set; }
        public string Egresos { get; set; }
        public string TotalPagar { get; set; }
        public string Observaciones { get; set; }

        private ReportViewer reportViewer1;
        ControladorImpresion objImpresion = new ControladorImpresion();

        public void ObtenerReporte()
        {
            this.reportViewer1 = new ReportViewer();
            this.Controls.Add(this.reportViewer1);
            this.reportViewer1.LocalReport.ReportEmbeddedResource =
            "CapaPresentacion.ReportesFacturas.NominaEmpleado.rptNominaEmpleado.rdlc";
        }

        public void ImprimirFactura(int Repetir)
        {
            try
            {
                ReportParameter[] reportParameters = new ReportParameter[9];
                reportParameters[0] = new ReportParameter("FechaHora", FechaHora);
                reportParameters[1] = new ReportParameter("InformacionEmpleado", InformacionEmpleado);
                reportParameters[2] = new ReportParameter("Turno", Turno);
                reportParameters[3] = new ReportParameter("Platos", Platos);
                reportParameters[4] = new ReportParameter("OtrosIngresos", OtrosIngresos);
                reportParameters[5] = new ReportParameter("Egresos", Egresos);
                reportParameters[6] = new ReportParameter("TotalPagar", TotalPagar);
                reportParameters[7] = new ReportParameter("Observaciones", Observaciones);
                reportParameters[8] = new ReportParameter("Servicios", Servicios);
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

        private void FrmReporteNominaEmpleado_Load(object sender, EventArgs e)
        {
            this.Controls.Add(this.reportViewer1);
            this.reportViewer1.LocalReport.ReportEmbeddedResource =
                "CapaPresentacion.ReportesFacturas.NominaEmpleado.rptNominaEmpleado.rdlc";
        }
    }
}
