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

namespace CapaPresentacion.Formularios.FormsBebidas
{
    public partial class FrmAgregarBebida : Form
    {
        public FrmAgregarBebida()
        {
            InitializeComponent();
            this.btnGuardar.Click += BtnGuardar_Click;
            this.Load += FrmAgregarBebida_Load;
            this.txtPrecio.KeyPress += TxtPrecio_KeyPress;
            this.txtPrecio.GotFocus += TxtPrecio_GotFocus;
            this.txtPrecio.LostFocus += TxtPrecio_LostFocus;
        }

        private void FrmAgregarBebida_Load(object sender, EventArgs e)
        {
            if (!this.IsEditar)
            {
                this.ListaTipoBebidas =
                    LlenarListas.LlenarListaTipoBebidas(this.ListaTipoBebidas);
            }
        }
        public void AsignarDatos(Bebidas bebida)
        {
            this.Bebida = bebida;
            this.IsEditar = true;

            this.ListaTipoBebidas =
              LlenarListas.LlenarListaTipoBebidas(this.ListaTipoBebidas);
            this.ListaTipoBebidas.SelectedValue = bebida.Id_tipo_bebida;

            this.txtNombre.Text = bebida.Nombre_bebida;
            this.txtPrecio.Text = bebida.Precio_bebida.ToString();
            this.txtDescripcion.Text = bebida.Descripcion_bebida.ToString();

            this.uploadImage1.AsignarImagen(bebida.Imagen, "rutaImages");
        }
        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            string rpta = "";
            string mensaje = "";
            try
            {
                if (this.Comprobaciones(out string[] variables))
                {
                    int id_bebida = 0;
                    if (this.IsEditar)
                    {
                        rpta = NBebidas.EditarBebida(variables.ToList());
                        mensaje = "actualizó";
                    }
                    else
                    {
                        rpta = NBebidas.InsertarBebida(variables.ToList(), out id_bebida);
                        mensaje = "agregó";
                    }

                    if (rpta.Equals("OK"))
                    {
                        if (this.uploadImage1.Nombre_imagen == null)
                            this.uploadImage1.Nombre_imagen = "SIN IMAGEN";

                        if (!this.uploadImage1.Nombre_imagen.Equals("SIN IMAGEN"))
                        {
                            rpta = ArchivosAdjuntos.GuardarArchivo(id_bebida, "rutaImages",
                                this.uploadImage1.Nombre_imagen,
                                this.uploadImage1.Ruta_origen);
                        }
                        if (rpta.Equals("OK"))
                        {
                            Mensajes.MensajeOkForm("Se " + mensaje + " la bebida correctamente");
                            this.Close();
                        }
                        else
                        {
                            Mensajes.MensajeErrorCompleto(this.Name, "BtnGuardar_Click",
                            "Se " + mensaje + " la bebida, pero hubo un error al guardar la imagen", rpta);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "BtnGuardar_Click",
                    "Hubo un error al " + mensaje + " la bebida", ex.Message);
            }
        }
        private bool Comprobaciones(out string[] variables)
        {
            variables = null;
            try
            {
                if (!int.TryParse(Convert.ToString(this.ListaTipoBebidas.SelectedValue), out int id_tipo_bebida))
                {
                    Mensajes.MensajeInformacion("Verifique el tipo de bebida seleccionado");
                    return false;
                }

                if (!decimal.TryParse(this.txtPrecio.Text, out decimal precio_bebida))
                {
                    Mensajes.MensajeInformacion("Verifique el precio de la bebida, el campo es obligatorio y debe ser solo números");
                    return false;
                }

                if (this.IsEditar)
                {
                    variables = new string[]
                    {
                        this.Bebida.Id_bebida.ToString(),
                        this.txtNombre.Text,
                        this.txtDescripcion.Text,
                        precio_bebida.ToString("N2"),
                        "0", "0", "0", "0",
                        this.uploadImage1.Nombre_imagen == null ? "SIN IMAGEN" : this.uploadImage1.Nombre_imagen,
                        id_tipo_bebida.ToString(),
                        "0", "0"
                    };
                }
                else
                {
                    variables = new string[]
                    {
                        "0",
                        this.txtNombre.Text,
                        this.txtDescripcion.Text,
                        precio_bebida.ToString("N2"),
                        "0", "0", "0", "0",
                        this.uploadImage1.Nombre_imagen == null ? "SIN IMAGEN" : this.uploadImage1.Nombre_imagen,
                        id_tipo_bebida.ToString(),
                        "0", "0"
                    };
                }
            }
            catch (Exception ex)
            {
                Mensajes.MensajeInformacion($"Error verificando los datos {ex.Message}");
                return false;
            }
            return true;
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
        private void TxtPrecio_KeyPress(object sender, KeyPressEventArgs e)
        {
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

        private Bebidas _bebida;
        private bool _isEditar;
        public Bebidas Bebida { get => _bebida; set => _bebida = value; }
        public bool IsEditar { get => _isEditar; set => _isEditar = value; }
    }
}
