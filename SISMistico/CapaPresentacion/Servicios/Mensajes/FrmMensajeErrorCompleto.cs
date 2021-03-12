using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CapaPresentacion
{
    public partial class FrmMensajeErrorCompleto : Form
    {
        public FrmMensajeErrorCompleto()
        {
            InitializeComponent();
            this.Load += FrmErrorCompleto_Load;
            this.btnCancelar.Click += BtnCancelar_Click;
            this.timer1.Tick += Timer1_Tick;
            this.btnEnviar.Click += BtnEnviar_Click;
        }

        private void BtnEnviar_Click(object sender, EventArgs e)
        {
            string rpta =
                EnviarEmailErrores.SendEmailError(this.Detalle_informacion, this.MetodoError, 
                this.FormularioError, this.Informacion_corta);
            if (rpta.Equals("OK"))
            {
                Mensajes.MensajeOkForm("Se envió el correo electrónico correctamente");
            }
            else
            {
                Mensajes.MensajeErrorForm("No se pudo enviar el correo electrónico");
            }
        }

        private void BtnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void FrmErrorCompleto_Load(object sender, EventArgs e)
        {
            this.lblMensaje.Text = Informacion_corta;
            this.txtMensaje.Text = Detalle_informacion;
            this.timer1.Start();
        }
        public string MetodoError { get; set; }
        public string FormularioError { get; set; }

        public string Informacion_corta { get; set; }
        public string Detalle_informacion { get; set; }

        private int contador = 20;
        private bool first_color = true;
        private void Timer1_Tick(object sender, EventArgs e)
        {
            this.toolTip1.SetToolTip(this.txtMensaje, contador.ToString() + 
                " segundos para que se cierre el mensaje");
            contador -= 1;
            if (this.first_color)
            {
                this.panel1.BackColor = Color.Red;
                first_color = false;
            }
            else
            {
                first_color = true;
                this.panel1.BackColor = Color.Silver;
            }

            if (contador == -2)
            {
                this.Close();
            }
        }
    }
}
