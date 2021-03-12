using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CapaPresentacion.Formularios.FormsDetalles
{
    public partial class ConvertidorPorciones : UserControl
    {
        public ConvertidorPorciones()
        {
            InitializeComponent();
            this.txtCantidad.KeyPress += Txt_KeyPress;
            this.btnConvertir.Click += BtnConvertir_Click;
            this.Load += ConvertidorPorciones_Load;
            this.txtCantidad.LostFocus += TxtCantidad_LostFocus;
        }

        private void TxtCantidad_LostFocus(object sender, EventArgs e)
        {
            TextBox txt = (TextBox)sender;
            if (txt.Text.Equals(""))
            {
                txt.Text = "0";
            }
        }

        private void ConvertidorPorciones_Load(object sender, EventArgs e)
        {
            this.txtPorciones.ReadOnly = true;
            this.txtCantidad.Text = "0";
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
                    this.btnConvertir.PerformClick();
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

        private string ObtenerValorPanel(Panel panel, out string texto)
        {
            texto = "";
            string rpta = "";
            foreach (Control control in panel.Controls)
            {
                if (control is RadioButton rd)
                {
                    if (rd.Checked)
                    {
                        rpta = rd.Tag.ToString().ToUpper();
                        texto = rd.Text.ToUpper();
                        break;
                    }
                }
            }
            return rpta;
        }

        private void BtnConvertir_Click(object sender, EventArgs e)
        {
            try
            {
                double resultado;
                bool result = int.TryParse(this.txtCantidad.Text, out int num_ingresado);
                if (result)
                {
                    int num_convertido;
                    string texto_rd;
                    string tipo = this.ObtenerValorPanel(this.panel1, out texto_rd);
                    if (tipo.Equals("KG"))
                    {
                        //Convertir Kilos a gramos
                        double n;
                        if (this.PorcionesToGramos)
                            n = num_ingresado / 1000;
                        else
                            n = num_ingresado * 1000;

                        n = Math.Round(n, 1, MidpointRounding.AwayFromZero);
                        num_convertido = Convert.ToInt32(n);
                    }
                    else if (tipo.Equals("OZ"))
                    {

                        //Convertir onzas a gramos
                        double n;
                        if (this.PorcionesToGramos)
                            n = num_ingresado / 28.35;
                        else
                            n = num_ingresado * 28.35;

                        n = Math.Round(n, 1, MidpointRounding.AwayFromZero);
                        num_convertido = Convert.ToInt32(n);

                    }
                    else
                    {
                        int num_temp = num_ingresado;
                        if (num_ingresado < 5 && num_ingresado > 0)
                            num_temp = 5;

                        num_convertido = num_temp;
                    }

                    if (this.PorcionesToGramos)
                    {
                        resultado = num_convertido * 5;
                        resultado = Math.Round(resultado, 1, MidpointRounding.AwayFromZero);
                        this.txtPorciones.Text =
                        num_ingresado + " porciones equivalen a " + resultado.ToString() + texto_rd + ".";
                    }
                    else
                    {
                        resultado = num_convertido / 5;
                        resultado = Math.Round(resultado, 1, MidpointRounding.AwayFromZero);
                        this.txtPorciones.Text =
                        num_ingresado + " " + texto_rd + " equivalen a " + resultado.ToString() + " porciones.";
                    }

                    this.txtPorciones.Tag = resultado;
                }
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorForm(ex.Message);
            }
        }

        private bool PorcionesToGramos;
    }
}
