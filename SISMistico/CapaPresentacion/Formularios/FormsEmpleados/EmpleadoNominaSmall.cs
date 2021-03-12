using CapaEntidades.Models;
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
            this.txtSalario.KeyPress += OnlyNumbers_KeyPress;
            this.txtEgresos.KeyPress += OnlyNumbers_KeyPress;
            this.txtOtrosIngresos.KeyPress += OnlyNumbers_KeyPress;
            this.txtTotalPropinas.KeyPress += OnlyNumbers_KeyPress;

            this.txtSalario.LostFocus += Txt_LostFocus;
            this.txtEgresos.LostFocus += Txt_LostFocus;
            this.txtOtrosIngresos.LostFocus += Txt_LostFocus;
            this.txtTotalPropinas.LostFocus += Txt_LostFocus;

            this.txtSalario.Enter += Txt_Enter;
            this.txtEgresos.Enter += Txt_Enter;
            this.txtOtrosIngresos.Enter += Txt_Enter;
            this.txtTotalPropinas.LostFocus += Txt_Enter;

            this.btnCalcular.Click += BtnCalcular_Click;
            this.btnImprimir.Click += BtnImprimir_Click;
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

            if (decimal.TryParse(this.txtTotalPropinas.Text, out decimal total_propinas))
            {
                total_pagar += total_propinas;
                this.EmpleadoNominaBinding.Propinas = total_propinas; 
            }
            else
                Mensajes.MensajeInformacion("Verifique el valor de las propinas deben ser solo números", "Entendido");

            if (decimal.TryParse(this.txtSalario.Text, out decimal salario))
            {
                total_pagar += salario;
                this.EmpleadoNominaBinding.Salario = salario;
            }
            else
                Mensajes.MensajeInformacion("Verifique el salario deben ser solo números", "Entendido");

            if (decimal.TryParse(this.txtOtrosIngresos.Text, out decimal otros_ingresos))
            {
                total_pagar += otros_ingresos;
                this.EmpleadoNominaBinding.Otros_ingresos = otros_ingresos;
            }
            else
                Mensajes.MensajeInformacion("Verifique el valor de los otros ingresos deben ser solo números", "Entendido");

            if (decimal.TryParse(this.txtEgresos.Text, out decimal egresos))
            {
                total_pagar -= egresos;
                this.EmpleadoNominaBinding.Egresos = egresos;
            }
            else
                Mensajes.MensajeInformacion("Verifique el valor de los egresos deben ser solo números", "Entendido");

            this.EmpleadoNominaBinding.Total_nomina = total_pagar;
            this.lblTotal.Text = "Total a pagar " + total_pagar.ToString("C");

            if (this.observacion != null)
                this.EmpleadoNominaBinding.Observaciones = this.observacion.txtObservacion.Text;

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
            this.txtTotalPropinas.Text = empleadoNomina.Propinas.ToString("N");
            this.txtSalario.Text = empleadoNomina.Salario.ToString("N");
            this.txtOtrosIngresos.Text = empleadoNomina.Otros_ingresos.ToString("N");
            this.txtEgresos.Text = empleadoNomina.Egresos.ToString("N");
            this.lblTotal.Text = empleadoNomina.Total_nomina.ToString("C");
            this.lblEstado.Text = empleadoNomina.Estado_nomina;

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
