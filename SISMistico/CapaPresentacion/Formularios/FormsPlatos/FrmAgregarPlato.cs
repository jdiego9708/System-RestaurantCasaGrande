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

namespace CapaPresentacion.Formularios.FormsPlatos
{
    public partial class FrmAgregarPlato : Form
    {
        public FrmAgregarPlato()
        {
            InitializeComponent();
            this.Load += FrmAgregarPlato_Load;
            this.txtPrecio.KeyPress += TxtPrecio_KeyPress;
            this.btnCancelar.Click += BtnCancelar_Click;
            this.btnGuardar.Click += BtnGuardar_Click;
            this.txtPrecio.GotFocus += TxtPrecio_GotFocus;
            this.txtPrecio.LostFocus += TxtPrecio_LostFocus;
        }

        private void TxtPrecio_LostFocus(object sender, EventArgs e)
        {
            if (this.txtPrecio.Text.Equals(""))
            {
                this.txtPrecio.Text = "0";
            }
        }

        private void TxtPrecio_GotFocus(object sender, EventArgs e)
        {
            this.txtPrecio.SelectAll();
        }

        public void ObtenerDatos(List<string> datos)
        {
            this.IsEditar = true;
            this.adjuntarImagen.IsEditar = true;
            this.ListaPlatos =
                LlenarListas.LlenarListaTipoPlatos(this.ListaPlatos);
            this.Text = "Editar datos de un plato";
            this.Tag = datos[0];
            this.txtNombre.Text = datos[1];
            this.ListaPlatos.Text = datos[8];
            this.adjuntarImagen.AsignarImagen(datos[4]);
            this.txtDescripcion.Text = datos[5];
            this.txtPrecio.Text = datos[3];
        }

        private string[] Variables()
        {
            string[] variables = null;

            if (this.IsEditar)
            {
                variables = new string[]
                {
                    Convert.ToString(this.Tag),
                    this.txtNombre.Text,
                    Convert.ToString(this.ListaPlatos.SelectedValue),
                    this.txtPrecio.Text,
                    this.adjuntarImagen.Nombre_imagen,
                    this.txtDescripcion.Text
                };
            }
            else
            {
                variables = new string[]
                {
                    this.txtNombre.Text,
                    Convert.ToString(this.ListaPlatos.SelectedValue),
                    this.txtPrecio.Text,
                    this.adjuntarImagen.Nombre_imagen,
                    this.txtDescripcion.Text
                };
            }
            return variables;
        }

        private bool Comprobaciones()
        {
            bool result = true;
            List<Control> listaControles = new List<Control>();
            listaControles.Add(this.txtNombre);
            listaControles.Add(this.ListaPlatos);
            listaControles.Add(this.txtPrecio);
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
                    int id_plato = 0;
                    if (this.IsEditar)
                    {
                        rpta = NPlatos.EditarPlatos(this.Variables());
                        mensaje = "actualizó";
                    }
                    else
                    {
                        rpta = NPlatos.InsertarPlatos(this.Variables(), out id_plato);
                        mensaje = "agregó";
                    }

                    if (rpta.Equals("OK"))
                    {
                        if (!this.adjuntarImagen.Nombre_imagen.Equals("SIN IMAGEN"))
                        {
                            rpta = ArchivosAdjuntos.GuardarArchivo(id_plato, "rutaImages",
                                this.adjuntarImagen.Nombre_imagen,
                                this.adjuntarImagen.RutaOrigen);
                        }
                        if (rpta.Equals("OK"))
                        {
                            Mensajes.MensajeOkForm("Se " + mensaje + " el plato correctamente");
                            this.Close();
                        }
                        else
                        {
                            Mensajes.MensajeErrorCompleto(this.Name, "BtnGuardar_Click",
                            "Se " + mensaje + " el plato, pero hubo un error al guardar la imagen", rpta);
                        }
                    }
                }
                else
                {
                    throw new Exception(rpta);
                }

            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "BtnGuardar_Click",
                    "Hubo un error al " + mensaje + " el plato", ex.Message);
            }
        }

        private void BtnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void TxtPrecio_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBox txt = (TextBox)sender;
            if (Char.IsDigit(e.KeyChar))
            {
                e.Handled = false;
            }
            else if (Char.IsControl(e.KeyChar))
            {
                e.Handled = false;
            }
            else if (Char.IsLetter(e.KeyChar))
            {
                e.Handled = true;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void FrmAgregarPlato_Load(object sender, EventArgs e)
        {
            if (this.IsEditar == false)
            {
                this.ListaPlatos =
                    LlenarListas.LlenarListaTipoPlatos(this.ListaPlatos);
                this.txtPrecio.Text = "0";
            }
        }

        private bool IsEditar = false;
    }
}
