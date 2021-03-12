using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using CapaNegocio;

namespace CapaPresentacion.Formularios.FormsClientes
{
    public partial class FrmAgregarCliente : Form
    {
        public FrmAgregarCliente()
        {
            InitializeComponent();
            this.btnCancelar.Click += BtnCancelar_Click;
            this.btnGuardar.Click += BtnGuardar_Click;
            this.Load += FrmAgregarCliente_Load;
            this.FormClosing += FrmAgregarCliente_FormClosing;
        }

        public event EventHandler onChangedEmail;
        public event EventHandler OnClienteSuccess;
        private bool _isPedido;
        public bool IsPedido { get => _isPedido; set => _isPedido = value; }

        private void FrmAgregarCliente_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.IsEditar && this.IsPedido)
            {
                e.Cancel = true;
            }
        }

        private void FrmAgregarCliente_Load(object sender, EventArgs e)
        {
            if (this.FrmObservarClientes != null)
            {
                this.btnCancelar.Visible = false;
            }
        }

        public FrmObservarClientes FrmObservarClientes;

        public void ObtenerDatos(List<string> datos)
        {
            this.Text = "Editar los datos de un cliente";
            this.IsEditar = true;
            this.Tag = datos[0];
            this.txtNombre.Text = datos[1];
            this.txtTelefono.Text = datos[2];
            this.txtCorreo.Text = datos[3];
            this.txtDireccion.Text = datos[4];
            this.txtReferencia.Text = datos[5];
            this.txtObservaciones.Text = datos[6];
            this.estado = datos[7];
        }

        private string[] Variables()
        {
            string[] variables = null;

            if (this.IsEditar)
            {
                variables = new string[]
                {
                    Convert.ToString(this.Tag), this.txtNombre.Text,
                    this.txtTelefono.Text, this.txtCorreo.Text, this.txtDireccion.Text,
                    this.txtReferencia.Text, this.txtObservaciones.Text, this.estado
                };
            }
            else
            {
                variables = new string[]
                {
                    this.txtNombre.Text,
                    this.txtTelefono.Text, this.txtCorreo.Text, this.txtDireccion.Text,
                    this.txtReferencia.Text, this.txtObservaciones.Text, "ACTIVO"
                };
            }
            return variables;
        }

        public bool DireccionEmailValida(string direccion, out string errorMessage)
        {
            if (direccion.Length == 0)
            {
                errorMessage = "La dirección de correo es necesaria";
                return false;
            }

            if (direccion.IndexOf("@") > -1)
            {
                if (direccion.IndexOf(".", direccion.IndexOf("@")) > direccion.IndexOf("@"))
                {
                    errorMessage = "OK";
                    return true;
                }
            }

            errorMessage = "La dirección de correo no tiene un formato válido \n" +
               "Por ejemplo 'nombre@gmail.com' ";
            return false;
        }

        private bool Comprobaciones()
        {
            string error;
            bool result = true;

            if (!string.IsNullOrEmpty(this.txtCorreo.Text))
            {
                if (!this.DireccionEmailValida(this.txtCorreo.Text, out error))
                {
                    this.errorProvider1.SetError(txtCorreo, error);
                }
            }

            List<Control> listaControles = new List<Control>();
            listaControles.Add(this.txtNombre);
            listaControles =
                ComprobacionesControles.ComprobacionesInsertar(listaControles);
            if (listaControles.Count > 0)
            {
                result = false;
                int contador = 0;
                foreach (Control control in listaControles)
                {
                    foreach (Control con in this.Controls)
                    {
                        if (con.Name == control.Name)
                        {
                            this.errorProvider1.SetError(control, "Campo obligatorio");
                            contador += 1;
                            break;
                        }
                    }

                    if (contador > listaControles.Count)
                    {
                        break;
                    }
                }
            }

            return result;
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            string rpta = "";
            string mensaje = "";
            try
            {
                if (this.Comprobaciones())
                {
                    if (this.IsEditar)
                    {
                        rpta = NClientes.EditarCliente(this.Variables());
                        mensaje = "actualizó";
                        if (this.IsPedido)
                        {
                            if (this.onChangedEmail != null)
                                this.onChangedEmail(this.txtCorreo, e);
                        }
                    }
                    else
                    {
                        rpta = NClientes.InsertarCliente(this.Variables());
                        mensaje = "agregó";
                    }

                    if (rpta.Equals("OK"))
                    {
                        if (this.FrmObservarClientes != null)
                        {
                            this.OnClienteSuccess?.Invoke(sender, e);
                            this.FrmObservarClientes.Actualizar();
                        }
                        Mensajes.MensajeOkForm("Se " + mensaje + " el cliente correctamente");
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
                    "Hubo un error al " + mensaje + " un cliente", ex.Message);
            }
        }

        private bool IsEditar = false;
        private string estado;

        private void BtnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
