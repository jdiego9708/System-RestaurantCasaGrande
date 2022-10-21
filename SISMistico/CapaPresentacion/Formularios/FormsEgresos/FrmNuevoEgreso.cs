using CapaEntidades.Models;
using CapaNegocio;
using CapaPresentacion.ReportesFacturas.GastosTurno;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CapaPresentacion.Formularios.FormsEgresos
{
    public partial class FrmNuevoEgreso : Form
    {
        public FrmNuevoEgreso()
        {
            InitializeComponent();
            this.btnGuardar.Click += BtnGuardar_Click;
            this.txtValor.KeyPress += TxtValor_KeyPress;
            this.txtValor.Enter += TxtValor_Enter;
            this.txtValor.LostFocus += TxtValor_LostFocus;
            this.Load += FrmNuevoEgreso_Load;
        }

        private void FrmNuevoEgreso_Load(object sender, EventArgs e)
        {
            this.txtFecha.Text = DateTime.Now.ToLongDateString();
        }

        private void TxtValor_LostFocus(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.txtValor.Text))
            {
                this.txtValor.Text = "0";
            }
        }

        private void TxtValor_Enter(object sender, EventArgs e)
        {
            this.txtValor.SelectAll();
        }

        private void TxtValor_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(char.IsNumber(e.KeyChar)) && (e.KeyChar != (char)Keys.Back))
            {
                e.Handled = true;
                return;
            }
        }

        public event EventHandler OnEgresoSuccess;

        private bool Comprobaciones(out Egresos egreso)
        {
            DatosInicioSesion datosInicioSesion = DatosInicioSesion.GetInstancia();

            egreso = new Egresos();

            if (string.IsNullOrWhiteSpace(this.txtDescripcion.Text))
            {
                Mensajes.MensajeInformacion("Verifique el campo descripción", "Entendido");
                return false;
            }

            if (!decimal.TryParse(this.txtValor.Text, out decimal valor))
            {
                Mensajes.MensajeInformacion("Verifique el campo valor, no tiene un formato correcto", "Entendido");
                return false;
            }
            else
            {
                if (valor == 0)
                {
                    Mensajes.MensajeInformacion("Verifique el campo valor, debe ser mayor que 0", "Entendido");
                    return false;
                }
            }

            egreso.Id_empleado = datosInicioSesion.EmpleadoLogin.Id_empleado;
            egreso.Empleado = datosInicioSesion.EmpleadoLogin;
            egreso.Fecha_egreso = DateTime.Now;
            egreso.Valor_egreso = valor;
            egreso.Descripcion_egreso = this.txtDescripcion.Text;
            egreso.Estado_egreso = "TERMINADO";

            return true;
        }

        private async void BtnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.Comprobaciones(out Egresos egreso))
                {
                    var (rpta, id_egreso) = await NEgresos.InsertarEgresos(egreso);
                    if (rpta.Equals("OK"))
                    {
                        Mensajes.MensajeInformacion("Se guardó correctamente el egreso", "Entendido");
                        egreso.Id_egreso = id_egreso;

                        StringBuilder infoEmpleado = new StringBuilder();
                        infoEmpleado.Append("Nombre de quien genera el egreso " + egreso.Empleado.Nombre_empleado).Append(Environment.NewLine);
                        infoEmpleado.Append("Identificación " + egreso.Empleado.Identificacion_empleado).Append(Environment.NewLine);

                        StringBuilder infoGasto = new StringBuilder();
                        infoGasto.Append("Concepto/Observaciones del egreso:").Append(Environment.NewLine);
                        infoGasto.Append(egreso.Descripcion_egreso).Append(Environment.NewLine);

                        FrmReporteGastos frmReporteGastos = new FrmReporteGastos
                        {
                            FechaHora = egreso.Fecha_egreso.ToLongDateString() + " - " + DateTime.Now.ToLongTimeString(),
                            InformacionEmpleado = infoEmpleado.ToString(),
                            InformacionGasto = infoGasto.ToString(),
                            Valor_gasto = egreso.Valor_egreso.ToString("C").Replace(",00", ""),
                            Observaciones = string.Empty,
                        };
                        frmReporteGastos.ObtenerReporte();
                        frmReporteGastos.ImprimirFactura(2);

                        this.OnEgresoSuccess?.Invoke(egreso, e);
                        this.Close();
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
