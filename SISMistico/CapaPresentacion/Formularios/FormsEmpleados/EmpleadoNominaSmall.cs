using CapaEntidades.Models;
using CapaNegocio;
using CapaPresentacion.Formularios.Controles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CapaPresentacion.Formularios.FormsEmpleados
{
    public partial class EmpleadoNominaSmall : UserControl
    {
        PoperContainer container;
        Observacion observacion;

        public EmpleadoNominaSmall()
        {
            InitializeComponent();
            this.btnPagar.Click += BtnPagar_Click;
            this.btnObservacion.Click += BtnObservacion_Click;
            this.txtPlatos.KeyPress += OnlyNumbers_KeyPress;
            this.txtEgresos.KeyPress += OnlyNumbers_KeyPress;
            this.txtServicios.KeyPress += OnlyNumbers_KeyPress;
            this.txtTurno.KeyPress += OnlyNumbers_KeyPress;

            this.txtPlatos.LostFocus += Txt_LostFocus;
            this.txtEgresos.LostFocus += Txt_LostFocus;
            this.txtServicios.LostFocus += Txt_LostFocus;
            this.txtTurno.LostFocus += Txt_LostFocus;

            this.txtPlatos.Enter += Txt_Enter;
            this.txtEgresos.Enter += Txt_Enter;
            this.txtServicios.Enter += Txt_Enter;
            this.txtTurno.Enter += TxtTurno_Enter;

            this.btnCalcular.Click += BtnCalcular_Click;
            this.btnImprimir.Click += BtnImprimir_Click;
        }

        private void TxtTurno_Enter(object sender, EventArgs e)
        {
            this.Calcular();
        }

        private void Txt_Enter(object sender, EventArgs e)
        {
            TextBox txt = (TextBox)sender;
            txt.SelectAll();
        }

        private void BtnImprimir_Click(object sender, EventArgs e)
        {
            this.Calcular();
            OnBtnImprimirClick?.Invoke(this.EmpleadoNominaBinding, e);
        }

        private void BtnCalcular_Click(object sender, EventArgs e)
        {
            this.Calcular();
        }

        private decimal Calcular()
        {
            decimal total_pagar = 0;

            StringBuilder info = new StringBuilder();

            if (decimal.TryParse(this.txtValorHora.Text, out decimal valor_hora))
            {
                info.Append($"Valor hora: {valor_hora:C}").Append(Environment.NewLine);
            }
            else
            {
                Mensajes.MensajeInformacion("Verifique el valor de la hora debe ser solo números", "Entendido");
                return 0;
            }

            if (this.numericHoras.Value <= 0)
            {
                Mensajes.MensajeInformacion("Verifique el valor de la hora debe ser solo números", "Entendido");
                return 0;
            }

            info.Append($"Horas {this.numericHoras.Value} | Minutos {this.numericMinutos.Value}").Append(Environment.NewLine);

            decimal total_turno = this.numericHoras.Value * valor_hora + this.numericMinutos.Value * valor_hora / 60;

            total_pagar += total_turno;

            this.EmpleadoNominaBinding.Turno = total_turno;

            this.txtTurno.Text = total_turno.ToString("C").Replace(",00", "");

            info.Append($"Total turno {total_turno:C}").Append(Environment.NewLine);

            if (string.IsNullOrEmpty(txtPlatos.Text))
                this.txtPlatos.Text = "0";

            if (decimal.TryParse(this.txtPlatos.Text, out decimal platos))
            {
                total_pagar += platos;
                this.EmpleadoNominaBinding.Platos = platos;
            }
            else
            {
                Mensajes.MensajeInformacion("Verifique el valor de los platos debe ser solo números", "Entendido");
                return 0;
            }

            if (string.IsNullOrEmpty(txtServicios.Text))
                this.txtServicios.Text = "0";

            if (decimal.TryParse(this.txtServicios.Text, out decimal servicios))
            {
                total_pagar += servicios;
                this.EmpleadoNominaBinding.Servicios = servicios;
            }
            else
            {
                Mensajes.MensajeInformacion("Verifique el valor de los servicios deben ser solo números", "Entendido");
                return 0;
            }

            if (string.IsNullOrEmpty(txtOtrosIngresos.Text))
                this.txtOtrosIngresos.Text = "0";

            if (decimal.TryParse(this.txtOtrosIngresos.Text, out decimal otros_ingresos))
            {
                total_pagar += otros_ingresos;
                this.EmpleadoNominaBinding.Otros_ingresos = otros_ingresos;
            }
            else
            {
                Mensajes.MensajeInformacion("Verifique el valor de los servicios deben ser solo números", "Entendido");
                return 0;
            }

            if (decimal.TryParse(this.txtEgresos.Text, out decimal egresos))
            {
                total_pagar -= egresos;
                this.EmpleadoNominaBinding.Egresos = egresos;
            }
            else
            {
                Mensajes.MensajeInformacion("Verifique el valor de los egresos deben ser solo números", "Entendido");
                return 0;
            }
            this.EmpleadoNominaBinding.Total_nomina = total_pagar;
            this.lblTotal.Text = "Total a pagar " + total_pagar.ToString("C").Replace(",00", "");

            if (this.observacion != null)
                info.Append(this.observacion.txtObservacion.Text);

            this.EmpleadoNominaBinding.Observaciones = info.ToString();

            return total_pagar;
        }

        private void Txt_LostFocus(object sender, EventArgs e)
        {
            this.Calcular();
        }

        private void OnlyNumbers_KeyPress(object sender, KeyPressEventArgs e)
        {
            CultureInfo cc = Thread.CurrentThread.CurrentCulture;
            if (char.IsDigit(e.KeyChar) ||
                (int)e.KeyChar == (int)Keys.Back)
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void BtnObservacion_Click(object sender, EventArgs e)
        {
            if (this.observacion == null)
                this.observacion = new Observacion();

            container = new PoperContainer(observacion);
            container.Show(this.btnObservacion);
        }

        public event EventHandler OnBtnPagarClick;
        public event EventHandler OnBtnImprimirClick;

        private void BtnPagar_Click(object sender, EventArgs e)
        {
            this.Calcular();
            OnBtnPagarClick?.Invoke(this.EmpleadoNominaBinding, e);
        }

        private void AsignarDatos(EmpleadoNominaBinding empleadoNomina)
        {
            this.txtNombre.Text = empleadoNomina.Empleado.Nombre_empleado + " " + empleadoNomina.Empleado.Telefono_empleado;
            this.txtTurno.Text = empleadoNomina.Servicios.ToString("N");
            this.txtPlatos.Text = empleadoNomina.Turno.ToString("N");
            this.txtServicios.Text = empleadoNomina.Platos.ToString("N");
            this.txtEgresos.Text = empleadoNomina.Egresos.ToString("N");
            this.lblTotal.Text = empleadoNomina.Total_nomina.ToString("C").Replace(",00", "");
            this.lblEstado.Text = empleadoNomina.Estado_nomina;

            //Buscar ultima nómina
            DataTable dt = NNomina.BuscarNomina("ID EMPLEADO ULTIMA NOMINA", empleadoNomina.Id_empleado.ToString(), out string rpta);
            if (dt != null && dt.Rows.Count > 0)
            {
                EmpleadoNominaBinding empleado = new EmpleadoNominaBinding(dt.Rows[0]);
                lblEstado.Text = $"Última nómina paga {empleado.Fecha_nomina.ToLongDateString()}";
            }
            else
            {
                lblEstado.Text = "No se han pagado nóminas a este empleado.";
            }

            if (empleadoNomina.Estado_nomina.Equals("PENDIENTE"))
            {
                this.panel1.BackColor = Color.FromArgb(255, 128, 128);
                this.lblEstado.ForeColor = Color.FromArgb(255, 128, 128);
                this.btnPagar.Visible = true;
                this.btnCalcular.Visible = true;
            }
            else
            {
                this.panel1.BackColor = Color.FromArgb(0, 192, 192);
                this.lblEstado.ForeColor = Color.FromArgb(0, 192, 192);
                this.btnPagar.Visible = false;
                this.btnCalcular.Visible = false;

            }
        }

        EmpleadoNominaBinding _eEmpleadoNomina;

        public EmpleadoNominaBinding EmpleadoNominaBinding
        {
            get => _eEmpleadoNomina;
            set
            {
                _eEmpleadoNomina = value;
                this.AsignarDatos(value);
            }
        }
    }
}
