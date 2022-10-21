using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CapaPresentacion.Formularios.FormsPedido
{
    public partial class MetodoPagoSmall : UserControl
    {
        public MetodoPagoSmall()
        {
            InitializeComponent();
            this.txtValor.KeyPress += Txt_KeyPress;
            this.txtValor.GotFocus += Txt_GotFocus;
            this.txtValor.LostFocus += Txt_LostFocus;
            this.chkMetodo.CheckedChanged += ChkMetodo_CheckedChanged;
        }

        private void ChkMetodo_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox chk = (CheckBox)sender;
            if (chk.Checked)
            {
                if (chk.Text.Equals("EFECTIVO"))
                {
                    this.txtValor.Tag = this.Total;
                    this.txtValor.Text = this.Total.ToString("C").Replace(",00", "");
                }
            }
        }

        private void Txt_KeyPress(object sender, KeyPressEventArgs e)
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

        private void Txt_GotFocus(object sender, EventArgs e)
        {
            TextBox txt = (TextBox)sender;
            txt.Text = Convert.ToString(txt.Tag);
            txt.SelectAll();
        }

        private void Txt_LostFocus(object sender, EventArgs e)
        {
            TextBox txt = (TextBox)sender;
            if (txt.Text.Equals(""))
            {
                string precio = "0";
                txt.Text = string.Format("{0:C}", precio);
            }
            else
            {
                bool result = int.TryParse(txt.Text, out int num);
                if (result)
                {
                    txt.Tag = num;
                    txt.Text = string.Format("{0:C}", txt.Tag);
                }
            }
        }

        private void AsignarDatos(MetodoPagoModel metodo)
        {
            this.chkMetodo.Text = metodo.MetodoPago;
            this.txtValor.Text = 0.ToString("C").Replace(",00", "");
            this.txtValor.Tag = 0;
        }

        private MetodoPagoModel _metodoPago;

        public MetodoPagoModel MetodoPago
        {
            get => _metodoPago;
            set
            {
                _metodoPago = value;
                this.AsignarDatos(value);
            }
        }

        public decimal Total { get; set; }
    }
}
