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

namespace CapaPresentacion.ReportesFacturas.IngresosTurno
{
    public partial class FrmReporteIngresos : Form
    {
        public FrmReporteIngresos()
        {
            InitializeComponent();
            this.Load += FrmReporteGastos_Load;
        }

        public string FechaHora { get; set; }
        public string InformacionEmpleado { get; set; }
        public string InformacionIngreso { get; set; }
        public string Valor_ingreso { get; set; }
        public string Observaciones { get; set; }

        private ReportViewer reportViewer1;
        ControladorImpresion objImpresion = new ControladorImpresion();

        public void ObtenerReporte()
        {
            this.reportViewer1 = new ReportViewer();
            this.Controls.Add(this.reportViewer1);
            this.reportViewer1.LocalReport.ReportEmbeddedResource =
            "CapaPresentacion.ReportesFacturas.IngresosTurno.rptComprobanteIngreso.rdlc";
        }

        private void FrmReporteGastos_Load(object sender, EventArgs e)
        {
            if (this.reportViewer1 == null)
                this.reportViewer1 = new ReportViewer();

            this.Controls.Add(this.reportViewer1);
            this.reportViewer1.LocalReport.ReportEmbeddedResource =
            "CapaPresentacion.ReportesFacturas.IngresosTurno.rptComprobanteIngreso.rdlc";
        }

        public void ImprimirFactura(int Repetir)
        {
            try
            {
                ReportParameter[] reportParameters = new ReportParameter[5];
                reportParameters[0] = new ReportParameter("FechaHora", FechaHora);
                reportParameters[1] = new ReportParameter("InformacionEmpleado", InformacionEmpleado);
                reportParameters[2] = new ReportParameter("InformacionIngreso", InformacionIngreso);
                reportParameters[3] = new ReportParameter("Valor_ingreso", Valor_ingreso);
                reportParameters[4] = new ReportParameter("Observaciones", Observaciones);
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
