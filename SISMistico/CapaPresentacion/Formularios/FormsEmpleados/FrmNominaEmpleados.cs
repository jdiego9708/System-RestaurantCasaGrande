using CapaEntidades.Models;
using CapaNegocio;
using CapaPresentacion.ReportesFacturas.NominaEmpleado;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CapaPresentacion.Formularios.FormsEmpleados
{
    public partial class FrmNominaEmpleados : Form
    {
        public FrmNominaEmpleados()
        {
            InitializeComponent();
            this.Load += FrmNominaEmpleados_Load;
            this.btnRefresh.Click += BtnRefresh_Click;
        }

        private async void BtnRefresh_Click(object sender, EventArgs e)
        {
            if (rdPendiente.Checked)
                await BuscarNominaEmpleados("ESTADO NOMINA", "PENDIENTE");
            else
                await BuscarNominaEmpleados("ESTADO NOMINA", "TERMINADO");
        }

        private async void FrmNominaEmpleados_Load(object sender, EventArgs e)
        {
            await BuscarNominaEmpleados("ESTADO NOMINA", "PENDIENTE");
        }

        private async Task BuscarNominaEmpleados(string tipo_busqueda, string texto_busqueda)
        {
            try
            {
                var (rpta, dtNominaEmpleados) = 
                    await NNomina.BuscarNomina(tipo_busqueda, texto_busqueda);

                this.panelNominaEmpleados.clearDataSource();

                if (dtNominaEmpleados != null)
                {
                    List<UserControl> controls = new List<UserControl>();
                    foreach (DataRow row in dtNominaEmpleados.Rows)
                    {
                        EmpleadoNominaBinding nomina = new EmpleadoNominaBinding(row);
                        EmpleadoNominaSmall empleado = new EmpleadoNominaSmall
                        {
                            EmpleadoNominaBinding = nomina,
                        };
                        empleado.OnBtnPagarClick += Empleado_OnBtnPagarClick;
                        empleado.OnBtnImprimirClick += Empleado_OnBtnImprimirClick;
                        controls.Add(empleado);
                    }
                    this.panelNominaEmpleados.AddArrayControl(controls);
                }
                else
                    if (!rpta.Equals("OK"))
                    throw new Exception(rpta);
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "BuscarNominaEmpleados",
                    "HUbo un error al buscar la nómina de empleados", ex.Message);
            }
        }

        private void Empleado_OnBtnImprimirClick(object sender, EventArgs e)
        {
            EmpleadoNominaBinding empleadoNomina = (EmpleadoNominaBinding)sender;

            string infoEmpleado = "Nombre: " + empleadoNomina.Empleado.Nombre_empleado + " - Cédula: " + empleadoNomina.Empleado.Identificacion_empleado +
                " - Celular: " + empleadoNomina.Empleado.Telefono_empleado;

            FrmReporteNominaEmpleado frmReporte = new FrmReporteNominaEmpleado
            {
                FechaHora = DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString(),
                InformacionEmpleado = "Información de empleado: " + Environment.NewLine + infoEmpleado,
                TotalPropinas = "Total de propinas: " + empleadoNomina.Propinas.ToString("C"),
                Salario = "Salario: " + empleadoNomina.Salario.ToString("C"),
                OtrosIngresos = "Otros ingresos: " + empleadoNomina.Otros_ingresos.ToString("C"),
                Egresos = "Egresos: " + empleadoNomina.Egresos.ToString("C"),
                TotalPagar = "Total a pagar: " + empleadoNomina.Total_nomina.ToString("C"),
                Observaciones = empleadoNomina.Observaciones,
            };
            frmReporte.ObtenerReporte();
            frmReporte.ImprimirFactura(1);
        }

        private async void Empleado_OnBtnPagarClick(object sender, EventArgs e)
        {
            EmpleadoNominaBinding empleadoNomina = (EmpleadoNominaBinding)sender;

            if (empleadoNomina.Id_nomina_empleado == 0)
            {
                empleadoNomina.Estado_nomina = "TERMINADO";
                var (rpta, id_nomina) = await NNomina.InsertarEmpleado(empleadoNomina);
                if (rpta.Equals("OK"))
                {
                    Mensajes.MensajeInformacion("Se pagó la nómina correctamente", "Entendido");
                    this.btnRefresh.PerformClick();
                    return;
                }
                else
                {
                    Mensajes.MensajeInformacion("No se pudo pagar la nómina, detalles: " + rpta, "Entendido");
                }
            }
            else
            {
                empleadoNomina.Estado_nomina = "TERMINADO";
                string rpta = await NNomina.EditarNomina(empleadoNomina.Id_nomina_empleado, empleadoNomina);
                if(rpta.Equals("OK"))
                {
                    Mensajes.MensajeInformacion("Se pagó la nómina correctamente", "Entendido");
                    this.btnRefresh.PerformClick();
                    return;
                }
                else
                {
                    Mensajes.MensajeInformacion("No se pudo pagar la nómina, detalles: " + rpta, "Entendido");
                }
            }
        }
    }
}
