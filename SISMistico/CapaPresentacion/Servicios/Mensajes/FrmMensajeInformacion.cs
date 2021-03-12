using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace CapaPresentacion.Servicios.Mensajes
{
    public partial class FrmMensajeInformacion : Form
    {
        public FrmMensajeInformacion()
        {
            InitializeComponent();
            this.Load += FrmMensajeInformacion_Load;
            this.timer1.Tick += Timer1_Tick;
            this.btnEntendido.Click += BtnEntendido_Click;
        }

        private string texto_boton;

        private void FrmMensajeInformacion_Load(object sender, EventArgs e)
        {
            this.txtMensaje.Text = Mensaje;
            this.btnEntendido.Text = this.Texto_boton.Equals("") ? "Entendido" : this.Texto_boton;
            this.timer1.Start();
        }

        private void BtnEntendido_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        public string Mensaje { get; set; }
        public string Texto_boton { get => texto_boton; set => texto_boton = value; }

        private int contador = 20;
        //private bool first_color = true;
        private void Timer1_Tick(object sender, EventArgs e)
        {
            contador += 1;

            if (contador == 20)
            {
                this.Close();
            }
        }
    }
}
