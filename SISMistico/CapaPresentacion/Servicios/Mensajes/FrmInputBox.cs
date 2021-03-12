using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CapaPresentacion.Servicios.Mensajes
{
    public partial class FrmInputBox : Form
    {
        public FrmInputBox()
        {
            InitializeComponent();
            this.Load += FrmInputBox_Load;
            this.btn1.Click += Btn1_Click;
            this.btn2.Click += Btn2_Click;
        }

        private void Btn2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.No;
            this.Mensaje = this.txtMensaje.Text;
            this.Close();
        }

        private void Btn1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Yes;
            this.Mensaje = this.txtMensaje.Text;
            this.Close();
        }

        private void FrmInputBox_Load(object sender, EventArgs e)
        {
            this.txtDescripcion.Text = Descripcion;
            this.btn1.Text = this.Texto_boton1;
            this.btn2.Text = this.Texto_boton2;
        }

        private string _mensaje;
        private string _descripcion;
        private string _texto_boton1;
        private string _texto_boton2;

        public string Descripcion { get => _descripcion; set => _descripcion = value; }
        public string Texto_boton1 { get => _texto_boton1; set => _texto_boton1 = value; }
        public string Texto_boton2 { get => _texto_boton2; set => _texto_boton2 = value; }
        public string Mensaje { get => _mensaje; set => _mensaje = value; }
    }
}
