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

namespace CapaPresentacion.Formularios.FormsBebidas
{
    public partial class FrmAgregarBebidas : Form
    {
        public FrmAgregarBebidas()
        {
            InitializeComponent();

            this.txtPrecio.LostFocus += TxtPrecio_LostFocus;
            this.txtPrecioTrago.LostFocus += TxtPrecio_LostFocus;
            this.txtPrecioTragoDoble.LostFocus += TxtPrecio_LostFocus;
            this.txtPrecioProveedor.LostFocus += TxtPrecio_LostFocus;
            this.txtCantidadUnidad.LostFocus += TxtPrecio_LostFocus;
            this.txtCantidadxUnidad.LostFocus += TxtPrecio_LostFocus;

            this.txtPrecio.GotFocus += TxtPrecio_GotFocus;
            this.txtPrecioTrago.GotFocus += TxtPrecio_GotFocus;
            this.txtPrecioTragoDoble.GotFocus += TxtPrecio_GotFocus;
            this.txtPrecioProveedor.GotFocus += TxtPrecio_GotFocus;
            this.txtCantidadUnidad.GotFocus += TxtPrecio_GotFocus;
            this.txtCantidadxUnidad.GotFocus += TxtPrecio_GotFocus;

            this.txtPrecio.KeyPress += Precio_KeyPress;
            this.txtPrecioTrago.KeyPress += Precio_KeyPress;
            this.txtPrecioTragoDoble.KeyPress += Precio_KeyPress;
            this.txtPrecioProveedor.KeyPress += Precio_KeyPress;
            this.txtCantidadUnidad.KeyPress += Precio_KeyPress;
            this.txtCantidadxUnidad.KeyPress += Precio_KeyPress;

            this.txtProveedor.Click += TxtProveedor_Click;
            this.Load += FrmAgregarBebidas_Load;
            this.btnGuardar.Click += BtnGuardar_Click;
            this.btnCancelar.Click += BtnCancelar_Click;
        }

        private void TxtPrecio_LostFocus(object sender, EventArgs e)
        {
            TextBox txt = (TextBox)sender;
            if (txt.Text.Equals(""))
            {
                txt.Text = "0";
                if (txt.Name.Equals("txtCantidadUnidad"))
                {
                    int num1;
                    int num2;
                    bool result1 = int.TryParse(txt.Text, out num1);
                    bool result2 = int.TryParse(txt.Text, out num2);
                    if (result1 & result2)
                    {
                        int resultado = num1 * num2;
                        this.lblCantidadTotal.Text = resultado.ToString();
                    }
                }
            }
        }

        private void TxtPrecio_GotFocus(object sender, EventArgs e)
        {
            TextBox txt = (TextBox)sender;
            txt.SelectAll();
        }

        private void BtnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        public void AsignarDatos(List<string> datos)
        {
            try
            {
                this.ListaTipoBebidas =
                    LlenarListas.LlenarListaTipoBebidas(this.ListaTipoBebidas);
                this.Text = "Editar datos de una bebida";
                this.IsEditar = true;
                this.Tag = datos[0];
                this.txtNombre.Text = datos[1];
                this.txtPrecio.Text = datos[3];
                this.txtPrecioTrago.Text = datos[4];
                this.txtPrecioTragoDoble.Text = datos[5];
                this.txtPrecioProveedor.Text = datos[6];
                this.txtProveedor.Tag = datos[7];
                this.adjuntarImagen.AsignarImagen(datos[8]);
                this.ListaTipoBebidas.SelectedValue = datos[9];
                this.txtCantidadUnidad.Text = datos[10];
                this.txtCantidadxUnidad.Text = datos[11];
                this.lblCantidadTotal.Text = datos[12];
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "AsignarDatos",
                    "Hubo un error al asignar los datos", ex.Message);
            }
        }

        private List<string> Parametros()
        {
            List<string> parametros = new List<string>();
            if (this.IsEditar)
            {
                parametros.Clear();
                parametros.Add(Convert.ToString(this.Tag));
                parametros.Add(this.txtNombre.Text);
                parametros.Add(this.txtPrecio.Text);
                parametros.Add(this.txtPrecioTrago.Text);
                parametros.Add(this.txtPrecioTragoDoble.Text);
                parametros.Add(this.txtPrecioProveedor.Text);
                parametros.Add(this.txtProveedor.Tag.ToString());
                parametros.Add(this.adjuntarImagen.Nombre_imagen);
                parametros.Add(Convert.ToString(this.ListaTipoBebidas.SelectedValue));
                parametros.Add(this.txtCantidadUnidad.Text);
                parametros.Add(this.txtCantidadxUnidad.Text);
            }
            else
            {
                parametros.Clear();
                parametros.Add(this.txtNombre.Text);
                parametros.Add(this.txtPrecio.Text);
                parametros.Add(this.txtPrecioTrago.Text);
                parametros.Add(this.txtPrecioTragoDoble.Text);
                parametros.Add(this.txtPrecioProveedor.Text);
                parametros.Add(this.txtProveedor.Tag.ToString());
                parametros.Add(this.adjuntarImagen.Nombre_imagen);
                parametros.Add(Convert.ToString(this.ListaTipoBebidas.SelectedValue));
                parametros.Add(this.txtCantidadUnidad.Text);
                parametros.Add(this.txtCantidadxUnidad.Text);
            }
            return parametros;
        }

        private bool Comprobaciones()
        {
            bool result = true;
            try
            {
                List<Control> listaControles = new List<Control>();
                listaControles.Add(this.txtNombre);
                listaControles.Add(this.txtPrecio);
                listaControles.Add(this.ListaTipoBebidas);
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
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "Comprobaciones",
                    "Hubo un error al comprobar controles", ex.Message);
            }
            return result;
        }

        private const string Metodo_error = "BtnGuardar_Click";
        private const string Informacion_error = "Hubo un error al guardar una bebida";

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                int id_bebida = 0;
                string rpta = "";
                if (this.Comprobaciones())
                {
                    if (this.IsEditar)
                    {
                        rpta = NBebidas.EditarBebida(this.Parametros());
                    }
                    else
                    {
                        rpta = NBebidas.InsertarBebida(this.Parametros(), out id_bebida);
                    }

                    if (rpta.Equals("OK"))
                    {
                        if (!this.adjuntarImagen.Nombre_imagen.Equals("SIN IMAGEN"))
                        {
                            rpta = ArchivosAdjuntos.GuardarArchivo(id_bebida, "rutaImages",
                            this.adjuntarImagen.Nombre_imagen,
                            this.adjuntarImagen.RutaOrigen);
                        }
                    }

                    if (rpta.Equals("OK"))
                    {
                        Mensajes.MensajeOkForm("Se agregó correctamente la bebida");
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
                Mensajes.MensajeErrorCompleto(this.Name, Metodo_error,
                    Informacion_error, ex.Message);
            }
        }

        private void FrmAgregarBebidas_Load(object sender, EventArgs e)
        {
            if (!this.IsEditar)
            {
                this.ListaTipoBebidas =
                    LlenarListas.LlenarListaTipoBebidas(this.ListaTipoBebidas);
            }
        }

        private void TxtProveedor_Click(object sender, EventArgs e)
        {
            Mensajes.MensajeErrorForm("Opción en mantenimiento");
        }

        private void Precio_KeyPress(object sender, KeyPressEventArgs e)
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

        private bool IsEditar = false;
    }
}
