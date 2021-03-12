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

namespace CapaPresentacion.Formularios.FormsEmpleados
{
    public partial class FrmAgregarEmpleado : Form
    {
        public FrmAgregarEmpleado()
        {
            InitializeComponent();
            this.Load += FrmAgregarEmpleado_Load;
            this.btnGuardar.Click += BtnGuardar_Click;
            this.btnCancelar.Click += BtnCancelar_Click;
        }

        private void BtnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        public void ObtenerDatos(List<string> datos)
        {
            this.IsEditar = true;
            this.ListaCargo =
                    LlenarListas.LlenarListaCargo(this.ListaCargo);
            this.Text = "Editar datos de un empleado";
            this.Tag = datos[0];
            this.txtNombre.Text = datos[1];
            this.txtTelefono.Text = datos[2];
            this.txtCedula.Text = datos[3];
            this.txtCorreo.Text = datos[4];
            this.ListaCargo.Text = datos[5];
            this.txtPass1.Text = datos[6];
            this.txtPass2.Text = datos[7];

            this.txtPass1.Visible = false;
            this.txtPass2.Visible = false;
            this.labelPASS1.Visible = false;
            this.labelPASS2.Visible = false;
        }

        private string[] Variables()
        {
            string[] variables = null;

            if (this.IsEditar)
            {
                variables = new string[]
                {
                    Convert.ToString(this.Tag), this.txtNombre.Text,
                    this.txtTelefono.Text, this.txtCedula.Text,
                    this.txtCorreo.Text,
                    this.ListaCargo.Text, this.txtPass2.Text
                };
            }
            else
            {
                variables = new string[]
                {
                    this.txtNombre.Text,
                    this.txtTelefono.Text, this.txtCedula.Text,
                    this.txtCorreo.Text,
                    this.ListaCargo.Text, this.txtPass2.Text
                };
            }
            return variables;
        }

        private bool Comprobaciones()
        {
            bool result = true;
            List<Control> listaControles = new List<Control>();
            listaControles.Add(this.txtNombre);
            listaControles.Add(this.txtTelefono);
            listaControles.Add(this.txtCedula);
            listaControles.Add(this.ListaCargo);
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

        private const string Nombre_boton = "BtnGuardar_Click";
        private const string Informacion = "Hubo un error al Guardar un empleado";

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.Comprobaciones())
                {
                    string rpta = "";
                    string mensaje = "";
                    if (this.IsEditar)
                    {
                        rpta = NEmpleados.EditarEmpleado(this.Variables());
                        mensaje = "actualizó";
                    }
                    else
                    {
                        rpta = NEmpleados.InsertarEmpleado(this.Variables());
                        mensaje = "agregó";
                    }

                    if (rpta.Equals("OK"))
                    {
                        Mensajes.MensajeOkForm("Se " + mensaje + " el empleado correctamente");
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
                Mensajes.MensajeErrorCompleto(ex.Message, Nombre_boton,
                    this.Name, Informacion);
            }
        }

        private bool IsEditar = false;

        private void FrmAgregarEmpleado_Load(object sender, EventArgs e)
        {
            if (!this.IsEditar)
            {
                this.ListaCargo =
                    LlenarListas.LlenarListaCargo(this.ListaCargo);
            }
        }
    }
}
