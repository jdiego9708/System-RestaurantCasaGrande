using System;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace CapaPresentacion
{
    public partial class FrmMensajeOk : Form
    {
        public FrmMensajeOk()
        {
            InitializeComponent();
            this.timer1.Tick += Timer1_Tick;
            this.Load += FrmMensajeOk_Load;
        }

        public string Mensaje { get; set; }

        private void FrmMensajeOk_Load(object sender, EventArgs e)
        {
            this.txtMensaje.Text = Mensaje;
            this.timer1.Interval = 250;
            this.timer1.Start();
        }

        private int contador = 0;
        private void Timer1_Tick(object sender, EventArgs e)
        {
            this.Opacity *= 0.9;
            contador += 1;
            if (contador == 11)
            {
                this.Close();
            }
        }
    }
}
