using System;
using System.Windows.Forms;

namespace CapaPresentacion.Formularios.FormsPedido
{
    public partial class FrmCambiarMesa : Form
    {
        public FrmCambiarMesa()
        {
            InitializeComponent();
            this.Load += FrmCambiarMesa_Load;
            this.txtNumeroMesa.KeyPress += TxtNumeroMesa_KeyPress;
            this.btnTerminar.Click += BtnTerminar_Click;
        }

        private void BtnTerminar_Click(object sender, EventArgs e)
        {
            try
            {
                int numero;
                if (int.TryParse(this.txtNumeroMesa.Text, out numero))
                {
                    this.Numero_mesa_destino = numero;
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    throw new Exception("Escriba un numero de mesa válido");
                }
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorForm(ex.Message);
            }
        }

        private int numero_mesa_destino;

        private int numero_mesa_origen;

        private int id_mesa_origen;
        private int id_pedido_origen;
        private string _estado_mesa_origen;

        public int Numero_mesa_destino { get => numero_mesa_destino; set => numero_mesa_destino = value; }
        public int Numero_mesa_origen { get => numero_mesa_origen; set => numero_mesa_origen = value; }
        public int Id_mesa_origen { get => id_mesa_origen; set => id_mesa_origen = value; }
        public int Id_pedido_origen { get => id_pedido_origen; set => id_pedido_origen = value; }
        public string Estado_mesa_origen { get => _estado_mesa_origen; set => _estado_mesa_origen = value; }

        private void TxtNumeroMesa_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBox txt = (TextBox)sender;
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

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Enter || keyData == Keys.Insert)
            {
                try
                {
                    int numero;
                    if (int.TryParse(this.txtNumeroMesa.Text, out numero))
                    {
                        this.Numero_mesa_destino = numero;
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    else
                    {
                        throw new Exception("Escriba un numero de mesa válido");
                    }
                }
                catch (Exception ex)
                {
                    Mensajes.MensajeErrorForm(ex.Message);
                }
            }
            else if (keyData == Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
            else
            {
                return base.ProcessCmdKey(ref msg, keyData);
            }
            return true;
        }

        private void FrmCambiarMesa_Load(object sender, EventArgs e)
        {

        }
    }
}
