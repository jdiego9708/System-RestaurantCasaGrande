using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using CapaPresentacion.Properties;
using System.Configuration;

namespace CapaPresentacion.Formularios.Controles
{
    public partial class AdjuntarImagen : UserControl
    {
        public AdjuntarImagen()
        {
            InitializeComponent();
            this.Load += AdjuntarImagen_Load;
            this.btnImagen.Click += BtnImagen_Click;
            this.txtImagen.TextChanged += TxtImagen_TextChanged;
            this.btnLimpiar.Click += BtnLimpiar_Click;
        }

        private void BtnLimpiar_Click(object sender, EventArgs e)
        {
            this.pxImagen.Image = Resources.SIN_IMAGENES;
        }

        private void TxtImagen_TextChanged(object sender, EventArgs e)
        {
            TextBox txt = (TextBox)sender;
            this.RutaOrigen = Convert.ToString(txt.Tag);
            this.Nombre_imagen = txt.Text;
        }

        private string nombre_imagen;
        private string rutaOrigen;

        public string Nombre_imagen { get => nombre_imagen; set => nombre_imagen = value; }
        public string RutaOrigen { get => rutaOrigen; set => rutaOrigen = value; }

        public void AsignarImagen(string nombre_imagen)
        {
            string rutaOrigen = "";
            Image Imagen = Resources.SIN_IMAGENES;
            if (!nombre_imagen.Equals("SIN IMAGEN") & !nombre_imagen.Equals(""))
            {
                Imagen = Imagenes.ObtenerImagen("RUTAIMAGES", nombre_imagen, out rutaOrigen);
            }

            this.pxImagen.Image = Imagen;
            this.pxImagen.SizeMode = PictureBoxSizeMode.StretchImage;
            this.txtImagen.Tag = rutaOrigen;
            this.txtImagen.Text = nombre_imagen;
            this.RutaOrigen = rutaOrigen;
        }

        private void BtnImagen_Click(object sender, EventArgs e)
        {
            try
            {
                //Creo un objeto de tipo OpenFileDialog y lo instancio
                OpenFileDialog archivo = new OpenFileDialog();
                //Esta linea es para que solo se puedan visualizar los archivos con esta extension
                archivo.Filter = "Documentos válidos|*.jpg;*.png;*.jpeg;*.jfif";
                //Lo abro como un Dialog
                DialogResult result = archivo.ShowDialog();
                if (result == DialogResult.OK)
                {
                    pxImagen.Image = Image.FromFile(archivo.FileName);
                    this.txtImagen.Tag = archivo.FileName;
                    this.txtImagen.Text = archivo.SafeFileName;
                    this.RutaOrigen = archivo.FileName;
                }
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto("AdjuntarImagen.cs", "BtnImagen_Click",
                    "Hubo un error al adjuntar una imagen", ex.Message);
            }
        }

        private void AdjuntarImagen_Load(object sender, EventArgs e)
        {
            if (!this.IsEditar)
            {
                this.Nombre_imagen = "SIN IMAGEN";
                this.txtImagen.Text = "SIN IMAGEN";
                this.pxImagen.SizeMode = PictureBoxSizeMode.StretchImage;
                this.pxImagen.Image = Resources.SIN_IMAGENES;
            }
        }

        public bool IsEditar = false;
    }
}
