using System;
using System.Drawing;
using System.Windows.Forms;

namespace CapaPresentacion.Formularios.FormsMesas
{
    public partial class MesaSmall : UserControl
    {
        public MesaSmall()
        {
            InitializeComponent();
            this.MouseEnter += MesaSmall_MouseEnter;
            this.groupBox1.MouseEnter += MesaSmall_MouseEnter;
            this.txtMesa.MouseEnter += MesaSmall_MouseEnter;
            this.panel1.MouseEnter += MesaSmall_MouseEnter;

            this.MouseLeave += MesaSmall_MouseHover;
            this.groupBox1.MouseLeave += MesaSmall_MouseHover;
            this.txtMesa.MouseLeave += MesaSmall_MouseHover;
            this.panel1.MouseLeave += MesaSmall_MouseHover;

            this.txtMesa.Click += TxtMesa_Click;
            this.txtMesa.Enter += TxtMesa_Enter;
        }

        private void TxtMesa_Enter(object sender, EventArgs e)
        {
            ActiveControl = this.panel1;
        }

        public event EventHandler OnMesaClick;

        private void TxtMesa_Click(object sender, EventArgs e)
        {
            this.OnMesaClick?.Invoke(this.Mesa, e);
        }

        private void MesaSmall_MouseHover(object sender, EventArgs e)
        {
            this.panel1.BackColor = Color.Teal;
        }

        private void MesaSmall_MouseEnter(object sender, EventArgs e)
        {
            this.panel1.BackColor = Color.FromArgb(0, 192, 192);
        }

        public EMesa Mesa;

        public void AsignarDatos()
        {
            if (this.Mesa != null)
            {
                this.txtMesa.Text = Mesa.Num_mesa.ToString();
            }
        }
    }
}
