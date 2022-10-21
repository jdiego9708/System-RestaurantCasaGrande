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

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            BuscarNominaEmpleados("GENERAL", "");
        }

        private void FrmNominaEmpleados_Load(object sender, EventArgs e)
        {
            BuscarNominaEmpleados("GENERAL", "");
        }

        private void BuscarNominaEmpleados(string tipo_busqueda, string texto_busqueda)
        {
            try
            {
                DataTable dtNominaEmpleados = NNomina.BuscarNomina(tipo_busqueda, texto_busqueda, out string rpta);

                this.panelNominaEmpleados.clearDataSource();

                if (dtNominaEmpleados != null)
                {
                    List<UserControl> controls = new List<UserControl>();
                    foreach (DataRow row in dtNominaEmpleados.Rows)
                    {
                        Empleados empleado = new Empleados(row);
                        EmpleadoNominaBinding nomina = new EmpleadoNominaBinding
                        {
                            Id_empleado = empleado.Id_empleado,
                            Empleado = empleado,
                            Id_nomina_empleado = 0,
                            Fecha_nomina = DateTime.Now,
                            Turno = 0,
                            Servicios = 0,
                            Platos = 0,
                            Egresos = 0,
                            Total_nomina = 0,
                            Estado_nomina = "PENDIENTE",
                            Observaciones = string.Empty,
                        };
                        EmpleadoNominaSmall empleadoSmall = new EmpleadoNominaSmall
                        {
                            EmpleadoNominaBinding = nomina,
                        };
                        empleadoSmall.OnBtnPagarClick += Empleado_OnBtnPagarClick;
                        empleadoSmall.OnBtnImprimirClick += Empleado_OnBtnImprimirClick;
                        controls.Add(empleadoSmall);
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
                Turno = "Turno: " + empleadoNomina.Turno.ToString("C").Replace(",00", ""),
                Platos = "Platos: " + empleadoNomina.Platos.ToString("C").Replace(",00", ""),
                Servicios = "Servicio: " + empleadoNomina.Servicios.ToString("C").Replace(",00", ""),
                OtrosIngresos = "Otros ingresos: " + empleadoNomina.Otros_ingresos.ToString("C").Replace(",00", ""),
                Egresos = "Egresos: " + empleadoNomina.Egresos.ToString("C").Replace(",00", ""),
                TotalPagar = "Total a pagar: " + empleadoNomina.Total_nomina.ToString("C").Replace(",00", ""),
                Observaciones = empleadoNomina.Observaciones,
            };
            frmReporte.ObtenerReporte();
            frmReporte.ImprimirFactura(2);
        }

        private async void Empleado_OnBtnPagarClick(object sender, EventArgs e)
        {
            EmpleadoNominaBinding empleadoNomina = (EmpleadoNominaBinding)sender;

            if (empleadoNomina.Id_nomina_empleado == 0)
            {
                empleadoNomina.Estado_nomina = "TERMINADO";
                var (rpta, id_nomina) = await NNomina.InsertarNomina(empleadoNomina);
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
        }
    }
}
