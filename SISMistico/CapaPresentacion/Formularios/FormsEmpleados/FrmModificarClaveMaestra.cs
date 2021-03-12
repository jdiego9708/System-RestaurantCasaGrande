using CapaNegocio;
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
    public partial class FrmModificarClaveMaestra : Form
    {
        public FrmModificarClaveMaestra()
        {
            InitializeComponent();
            this.Load += FrmModificarClaveMaestra_Load;
            this.btnGuardar.Click += BtnGuardar_Click;
            this.txtClaveMaestra.KeyPress += TxtClaveMaestra_KeyPress;
        }

        private void TxtClaveMaestra_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (this.IsClaveMaestra)
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
        }

        private bool Comprobaciones(out string clave)
        {
            clave = "";
            if (this.IsClaveMaestra)
            {
                if (int.TryParse(this.txtClaveMaestra.Text, out int clave1))
                {
                    clave = clave1.ToString();
                    return true;
                }
                else
                {
                    Mensajes.MensajeInformacion("Verifique la clave maestra, deben ser solo números", "Entendido");
                    return false;
                }
            }
            else
            {
                if (this.txtClaveMaestra.Text.Equals(""))
                {
                    Mensajes.MensajeInformacion("Verifique la contraseña de usuario, no puede estar vacío", "Entendido");
                    return false;
                }
                else
                {
                    clave = this.txtClaveMaestra.Text;
                    return true;
                }
            }
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.Comprobaciones(out string clave))
                {
                    string rpta = "";
                    string mensaje = "";
                    if (this.IsClaveMaestra)
                    {
                        rpta = NEmpleados.ActualizarClaveMaestra(this.EEmpleado.Id_empleado, Convert.ToInt32(clave));
                        mensaje = "Se guardó la clave maestra correctamente";
                    }
                    else
                    {
                        rpta = NEmpleados.ActualizarClaveUsuario(this.EEmpleado.Id_empleado, clave);
                        mensaje = "Se guardó la clave de usuario correctamente";
                    }

                    if (rpta.Equals("OK"))
                    {
                        Mensajes.MensajeOkForm(mensaje);
                        this.Close();
                    }
                    else
                    {
                        throw new Exception(rpta);
                    }
                }

            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "BtnGuardar_Click", 
                    "Hubo un error al guardar la clave maestra o de usuario. IsClaveMaestra: " + this.IsClaveMaestra.ToString() , 
                    ex.Message);
            }
        }

        EEmpleado EEmpleado;

        public void AsignarDatos(EEmpleado eEmpleado, bool isClaveMaestra)
        {
            this.IsClaveMaestra = isClaveMaestra;
            this.EEmpleado = eEmpleado;
            this.txtEmpleado.Text = eEmpleado.Nombre_empleado;

            if (isClaveMaestra)
            {
                this.txtTitulo.Text = "Clave maestra";
                this.Text = "Asignar o modificar clave maestra";
                this.txtClaveMaestra.MaxLength = 4;
            }
            else
            {
                this.txtTitulo.Text = "Contraseña";
                this.txtClaveMaestra.MaxLength = 15;
                this.Text = "Asignar o modificar contraseña de usuario";
            }
        }

        private void FrmModificarClaveMaestra_Load(object sender, EventArgs e)
        {
            this.txtClaveMaestra.Focus();
            this.txtClaveMaestra.SelectAll();
        }

        private bool _isClaveMaestra;

        public bool IsClaveMaestra { get => _isClaveMaestra; set => _isClaveMaestra = value; }
    }
}
