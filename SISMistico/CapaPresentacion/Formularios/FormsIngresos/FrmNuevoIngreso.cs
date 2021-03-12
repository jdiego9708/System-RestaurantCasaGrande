using CapaEntidades.Models;
using CapaNegocio;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CapaPresentacion.Formularios.FormsIngresos
{
    public partial class FrmNuevoIngreso : Form
    {
        public FrmNuevoIngreso()
        {
            InitializeComponent();
            this.btnGuardar.Click += BtnGuardar_Click;
            this.txtValor.KeyPress += TxtValor_KeyPress;
            this.txtValor.Enter += TxtValor_Enter;
            this.txtValor.LostFocus += TxtValor_LostFocus;
            this.Load += FrmNuevoIngreso_Load;
        }

        private void FrmNuevoIngreso_Load(object sender, EventArgs e)
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

        public event EventHandler OnIngresoSuccess;

        private bool Comprobaciones(out Ingresos ingreso)
        {
            DatosInicioSesion datosInicioSesion = DatosInicioSesion.GetInstancia();

            ingreso = new Ingresos();

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

            ingreso.Id_empleado = datosInicioSesion.Empleado.Id_empleado;
            ingreso.Empleado = datosInicioSesion.Empleado;
            ingreso.Fecha_ingreso = DateTime.Now;
            ingreso.Valor_ingreso = valor;
            ingreso.Descripcion_ingreso = this.txtDescripcion.Text;
            ingreso.Estado_ingreso = "TERMINADO";

            return true;
        }

        private async void BtnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.Comprobaciones(out Ingresos ingreso))
                {
                    var (rpta, id_ingreso) = await NIngresos.InsertarIngresos(ingreso);
                    if (rpta.Equals("OK"))
                    {
                        Mensajes.MensajeInformacion("Se guardó correctamente el ingreso", "Entendido");
                        ingreso.Id_ingreso = id_ingreso;
                        this.OnIngresoSuccess?.Invoke(ingreso, e);
                        this.Close();
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void TxtValor_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(char.IsNumber(e.KeyChar)) && (e.KeyChar != (char)Keys.Back))
            {
                e.Handled = true;
                return;
            }
        }
    }
}
