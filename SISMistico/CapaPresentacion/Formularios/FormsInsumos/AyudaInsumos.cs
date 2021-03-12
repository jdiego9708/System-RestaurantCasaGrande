using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CapaPresentacion.Formularios.FormsInsumos
{
    public partial class AyudaInsumos : UserControl
    {
        public AyudaInsumos()
        {
            InitializeComponent();
            this.txtGramos.KeyPress += Txt_KeyPress;
            this.txtPorciones.KeyPress += Txt_KeyPress;
            this.btnConvertir.Click += BtnConvertir_Click;
            this.Load += AyudaInsumos_Load;
        }

        private void AyudaInsumos_Load(object sender, EventArgs e)
        {
            this.txtPorciones.ReadOnly = true;
        }

        private void BtnConvertir_Click(object sender, EventArgs e)
        {
            try
            {
                int num_ingresado;
                double resultado;
                bool result = int.TryParse(this.txtGramos.Text, out num_ingresado);
                if (result)
                {
                    resultado = num_ingresado / 5;
                    resultado = Math.Round(resultado, 1, MidpointRounding.AwayFromZero);
                    this.txtPorciones.Text = resultado.ToString();
                }
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorForm(ex.Message);
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
                if (e.KeyChar == (char)Keys.Enter)
                {
                    try
                    {
                        int num_ingresado;
                        double resultado;
                        bool result = int.TryParse(this.txtGramos.Text, out num_ingresado);
                        if (result)
                        {
                            resultado = num_ingresado / 5;
                            resultado = Math.Round(resultado, 1, MidpointRounding.AwayFromZero);
                            this.txtPorciones.Text = resultado.ToString();
                        }
                    }
                    catch (Exception ex)
                    {
                        Mensajes.MensajeErrorForm(ex.Message);
                    }
                }
                else
                {
                    e.Handled = false;
                }
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
    }
}
