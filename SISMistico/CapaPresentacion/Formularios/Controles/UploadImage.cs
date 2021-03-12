using CapaPresentacion.Properties;
using CapaPresentacion.Servicios.FormsCorreoTicket;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace CapaPresentacion.Formularios.Controles
{
    public partial class UploadImage : UserControl
    {
        ConfiguracionImagen configImagen;
        Observacion observacion;
        PoperContainer container;

        public UploadImage()
        {
            InitializeComponent();

            this.observacion = new Observacion();

            this.btnSeleccionar.Click += BtnSeleccionar_Click;
            this.Load += UploadImage_Load;
            this.btnLimpiar.Click += BtnLimpiar_Click;
            this.btnAgregarComentario.Click += BtnAgregarComentario_Click;
            this.observacion.btnCerrar.Click += BtnCerrar_Click;
            this.btnQuitar.Click += BtnQuitar_Click;
            this.btnScreenShot.Click += BtnScreenShot_Click;
            this.btnConfig.Click += BtnConfig_Click;
        }

        private void BtnQuitar_Click(object sender, EventArgs e)
        {
            if (this.onBtnQuitarClick != null)
                this.onBtnQuitarClick(this, e);
        }

        private void BtnConfig_Click(object sender, EventArgs e)
        {
            this.configImagen = new ConfiguracionImagen();
            this.configImagen.numericTiempoCaptura.Value = 1;
            this.container = new PoperContainer(this.configImagen);
            this.container.Closing += ConObservaciones_Closing;
            this.container.Show(this.btnConfig);
        }

        public event EventHandler onBtnQuitarClick;
        public event EventHandler onBtnCaptureClick;

        public int Tiempo_capture;
        private void BtnScreenShot_Click(object sender, EventArgs e)
        {
            if (Tiempo_capture == 0)
                Tiempo_capture = 2;

            Tiempo_capture = Convert.ToInt32(this.configImagen.numericTiempoCaptura.Value);
            if (this.onBtnCaptureClick != null)
                this.onBtnCaptureClick(this, e);
        }

        /// <summary>
        ///  Crea una captura de pantalla completa usando ScreenCapture();
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="filename"></param>
        /// <param name="format"></param>
        public void FullScreenshotWithClass(String filepath, String filename, ImageFormat format)
        {
            ScreenCapture sc = new ScreenCapture();
            Image img = sc.CaptureScreen();

            string fullpath = filepath + "\\" + filename;

            img.Save(fullpath, format);
        }

        private void ConObservaciones_Closing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            this.Observaciones = this.observacion.txtObservacion.Text;
        }

        private void BtnCerrar_Click(object sender, EventArgs e)
        {
            this.container.Close();
        }

        private void BtnAgregarComentario_Click(object sender, EventArgs e)
        {
            this.container = new PoperContainer(this.observacion);
            this.container.Closing += ConObservaciones_Closing;
            this.container.Show(this.btnAgregarComentario);
        }

        public void AsignarImagen(Image image, string nombre_imagen)
        {
            this.pxImagen.Image = image;
            this.txtImagen.Text = nombre_imagen;
        }

        public void AsignarImagen(string nombre_imagen, string appKey)
        {
            string rutaOr;
            Image Imagen = Imagenes.ObtenerImagen(appKey, nombre_imagen, out rutaOr);
            this.pxImagen.Image = Imagen;
            this.pxImagen.SizeMode = PictureBoxSizeMode.StretchImage;
            this.txtImagen.Text = nombre_imagen;
            this.txtImagen.Tag = rutaOr;
            this.Ruta_origen = rutaOr;
        }

        private void BtnLimpiar_Click(object sender, EventArgs e)
        {
            this.txtImagen.Text = string.Empty;
            this.txtImagen.Tag = null;
            this.pxImagen.Image = Resources.SIN_IMAGENES;
        }

        private void UploadImage_Load(object sender, EventArgs e)
        {
            this.label1.Text = "Imagen " + this.Numero_imagen;
            if (this.Observaciones != null)
            {
                this.observacion.txtObservacion.Text = this.Observaciones;
            }
        }

        private void BtnSeleccionar_Click(object sender, EventArgs e)
        {
            try
            {
                //Creo un objeto de tipo OpenFileDialog y lo instancio
                OpenFileDialog archivo = new OpenFileDialog();
                //Esta linea es para que solo se puedan visualizar los archivos con esta extension
                archivo.Filter = "Documentos válidos|*.jpg;*.png;*.jfif";
                //Lo abro como un Dialog
                DialogResult result = archivo.ShowDialog();
                if (result == DialogResult.OK)
                {
                    Image Image = Image.FromFile(archivo.FileName);
                    this.Imagen = Image;
                    this.pxImagen.Image = Image;
                    this.txtImagen.Text = archivo.SafeFileName;
                    this.txtImagen.Tag = archivo.FileName;
                    this.Ruta_origen = archivo.FileName;
                    this.Nombre_imagen = archivo.SafeFileName;
                }
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "BtnImagen_Click",
                    "Hubo un error al adjuntar la imagen", ex.Message);
            }
        }

        private string _nombre_imagen;
        private string _ruta_origen;

        private string _ruta_destino;
        private int _numero_imagen;
        private string _tipo_imagen;
        private string _observaciones;
        private Image _imagen;

        public string Nombre_imagen { get => _nombre_imagen; set => _nombre_imagen = value; }
        public string Ruta_origen { get => _ruta_origen; set => _ruta_origen = value; }
        public string Ruta_destino { get => _ruta_destino; set => _ruta_destino = value; }
        public int Numero_imagen { get => _numero_imagen; set => _numero_imagen = value; }
        public string Tipo_imagen { get => _tipo_imagen; set => _tipo_imagen = value; }
        public string Observaciones { get => _observaciones; set => _observaciones = value; }
        public Image Imagen { get => _imagen; set => _imagen = value; }
    }
}
