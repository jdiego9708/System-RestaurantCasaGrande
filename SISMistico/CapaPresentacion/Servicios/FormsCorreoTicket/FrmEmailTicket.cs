using CapaPresentacion.Controles;
using CapaPresentacion.Formularios.Controles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using CapaPresentacion;
using CapaPresentacion.Servicios.FormsCorreoTicket;
using System.IO;

namespace CapaPresentacion.Servicios
{
    public partial class FrmEmailTicket : Form
    {
        public FrmEmailTicket()
        {
            InitializeComponent();
            this.btnAddPaso.Click += BtnAddPaso_Click;
            this.timer1.Tick += Timer1_Tick;
            this.chkPasos.CheckedChanged += ChkPasos_CheckedChanged;
            this.Load += FrmEmailTicket_Load;
            this.btnEnviar.Click += BtnEnviar_Click;
        }


        private void BtnEnviar_Click(object sender, EventArgs e)
        {
            bool result = true;

            if (this.txtAsunto.Texto.Equals(""))
            {
                this.errorProvider1.SetError(this.txtAsunto, "El asunto es obligatorio");
                result = false;
            }

            if (this.txtDescripcion.Text.Equals(""))
            {
                this.errorProvider1.SetError(this.txtDescripcion, "Debe poner una descripción");
                result = false;
            }

            if (result)
            {
                DataTable dtImagenes = new DataTable();
                dtImagenes.Columns.Add("Ruta_origen", typeof(string));
                dtImagenes.Columns.Add("Observaciones", typeof(string));

                foreach(UploadImage image in this.panelPasos.controlsUser)
                {
                    if (image.Imagen != null)
                    {
                        DataRow row = dtImagenes.NewRow();
                        row["Ruta_origen"] = image.Ruta_destino;
                        row["Observaciones"] = image.Observaciones;
                        dtImagenes.Rows.Add(row);
                    }
                }

                string rpta =
                    EnviarEmailErrores2.SendEmailError("", this.txtAsunto.Texto, "",
                    this.txtDescripcion.Text, dtImagenes);
            }

        }

        private void FrmEmailTicket_Load(object sender, EventArgs e)
        {
            this.txtAsunto.Texto_inicial = "Asunto del reporte";
            this.txtAsunto.EstablecerTextoInicial();
            this.lblFechaHora.Text = DateTime.Now.ToLongDateString() + " - " + DateTime.Now.ToLongTimeString();
        }

        private void ChkPasos_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox chk = (CheckBox)sender;
            if (chk.Checked)
            {
                this.panelPasos.Visible = true;
                this.btnAddPaso.Visible = true;
            }
            else
            {
                this.panelPasos.Visible = false;
                this.btnAddPaso.Visible = false;
            }
        }

        int contador = 0;
        private void Timer1_Tick(object sender, EventArgs e)
        {
            int numero_segundos = tiempo_capture;
            if (contador > numero_segundos)
            {
                FullScreenshotWithClass(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Screen_Mistico.jpg", ImageFormat.Jpeg);
                this.timer1.Stop();
            }
            contador += 1;
        }

        int tiempo_capture;
        int num_images;

        private void BtnAddPaso_Click(object sender, EventArgs e)
        {
            if (this.num_images == 0)
                this.num_images = 1;

            if (this.num_images > 4)
            {
                this.panelPasos.AutoSize = false;
                this.panelPasos.AutoScroll = true;
            }

            UploadImage upload = new UploadImage()
            {
                Numero_imagen = this.num_images
            };
            upload.onBtnQuitarClick += Upload_onBtnQuitarClick;
            upload.onBtnCaptureClick += Upload_onBtnCaptureClick;
            this.panelPasos.AddControl(upload);
        }

        private void Upload_onBtnQuitarClick(object sender, EventArgs e)
        {
            UploadImage image = (UploadImage)sender;
            this.panelPasos.RemoveControl(image);
        }

        private void Upload_onBtnCaptureClick(object sender, EventArgs e)
        {
            UploadImage upload = (UploadImage)sender;
            tiempo_capture = upload.Tiempo_capture;
            uploadTemp = upload;
            this.timer1.Start();
        }

        public void FullScreenshotWithClass(String filepath, String filename, ImageFormat format)
        {
            filename = uploadTemp.Numero_imagen+ filename ;
            ScreenCapture sc = new ScreenCapture();
            Image img = sc.CaptureScreen();
            string fullpath = filepath + "\\" + filename;
            uploadTemp.Imagen = img;
            uploadTemp.Nombre_imagen = filename;
            uploadTemp.Ruta_destino = fullpath;
            uploadTemp.AsignarImagen(img, filename);
            img.Save(fullpath, format);
        }

        UploadImage uploadTemp;
    }
}
