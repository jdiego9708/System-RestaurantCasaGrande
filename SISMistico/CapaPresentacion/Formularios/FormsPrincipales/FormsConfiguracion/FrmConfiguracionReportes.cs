using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CapaPresentacion.Formularios.FormsPrincipales.FormsConfiguracion
{
    public partial class FrmConfiguracionReportes : Form
    {
        public FrmConfiguracionReportes()
        {
            InitializeComponent();
            this.Load += FrmConfiguracionReportes_Load;

            this.txtAncho.KeyPress += OnlyNumbers_KeyPress;
            this.txtAlto.KeyPress += OnlyNumbers_KeyPress;
            this.txtMargenArriba.KeyPress += OnlyNumbers_KeyPress;
            this.txtMargenAbajo.KeyPress += OnlyNumbers_KeyPress;
            this.txtMargenDerecho.KeyPress += OnlyNumbers_KeyPress;
            this.txtMargenIzquierdo.KeyPress += OnlyNumbers_KeyPress;

            this.txtAncho.GotFocus += Txt_GotFocus;
            this.txtAlto.GotFocus += Txt_GotFocus;
            this.txtMargenArriba.GotFocus += Txt_GotFocus;
            this.txtMargenAbajo.GotFocus += Txt_GotFocus;
            this.txtMargenDerecho.GotFocus += Txt_GotFocus;
            this.txtMargenIzquierdo.GotFocus += Txt_GotFocus;

            this.txtAncho.LostFocus += Txt_LostFocus;
            this.txtAlto.LostFocus += Txt_LostFocus;
            this.txtMargenArriba.LostFocus += Txt_LostFocus;
            this.txtMargenAbajo.LostFocus += Txt_LostFocus;
            this.txtMargenDerecho.LostFocus += Txt_LostFocus;
            this.txtMargenIzquierdo.LostFocus += Txt_LostFocus;

            this.btnGuardar.Click += BtnGuardar_Click;
        }

        private bool Comprobaciones()
        {
            bool result = true;

            if (!decimal.TryParse(this.txtAncho.Text, out decimal ancho))
            {
                result = false;
                Mensajes.MensajeInformacion("Verifique el valor de ancho de página", "Entendido");
                return result;
            }

            if (!decimal.TryParse(this.txtAlto.Text, out decimal alto))
            {
                result = false;
                Mensajes.MensajeInformacion("Verifique el valor de alto de página", "Entendido");
                return result;
            }

            if (!decimal.TryParse(this.txtMargenArriba.Text, out decimal arriba))
            {
                result = false;
                Mensajes.MensajeInformacion("Verifique el valor del margen superior", "Entendido");
                return result;
            }

            if (!decimal.TryParse(this.txtMargenAbajo.Text, out decimal abajo))
            {
                result = false;
                Mensajes.MensajeInformacion("Verifique el valor del margen inferior", "Entendido");
                return result;
            }

            if (!decimal.TryParse(this.txtMargenDerecho.Text, out decimal derecho))
            {
                result = false;
                Mensajes.MensajeInformacion("Verifique el valor del margen derecho", "Entendido");
                return result;
            }

            if (!decimal.TryParse(this.txtMargenIzquierdo.Text, out decimal izquierdo))
            {
                result = false;
                Mensajes.MensajeInformacion("Verifique el valor del margen izquierdo", "Entendido");
                return result;
            }

            if (listaMedida.Text.Equals(""))
            {
                result = false;
                Mensajes.MensajeInformacion("Verifique la medida seleccionada", "Entendido");
                return result;
            }

            return result;

        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.Comprobaciones())
                {
                    Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    config.AppSettings.Settings["AnchoHoja"].Value = this.txtAncho.Text;
                    config.AppSettings.Settings["AltoHoja"].Value = this.txtAlto.Text;
                    config.AppSettings.Settings["MargenArriba"].Value = this.txtMargenArriba.Text;
                    config.AppSettings.Settings["MargenAbajo"].Value = this.txtMargenAbajo.Text;
                    config.AppSettings.Settings["MargenIzquierda"].Value = this.txtMargenIzquierdo.Text;
                    config.AppSettings.Settings["MargenDerecha"].Value = this.txtMargenDerecho.Text;
                    config.AppSettings.Settings["MedidaPredeterminada"].Value = this.listaMedida.Text;
                    config.Save(ConfigurationSaveMode.Modified, true);
                    ConfigurationManager.RefreshSection("appSettings");
                    Mensajes.MensajeOkForm("Se guardó correctamente la configuración de página de impresión");
                }
            }
            catch (Exception ex) 
            {
                Mensajes.MensajeErrorCompleto(this.Name, "BtnGuardar_Click",
                    "Hubo un error al guardar la configuración de reportes", ex.Message);
            }
        }

        private void Txt_GotFocus(object sender, EventArgs e)
        {
            TextBox txt = (TextBox)sender;
            txt.SelectAll();
        }

        private void Txt_LostFocus(object sender, EventArgs e)
        {
            TextBox txt = (TextBox)sender;
            if (txt.Text.Equals(""))
                txt.Text = "0";
        }

        private void OnlyNumbers_KeyPress(object sender, KeyPressEventArgs e)
        {
            CultureInfo cc = Thread.CurrentThread.CurrentCulture;
            if (e.KeyChar.ToString() == cc.NumberFormat.NumberDecimalSeparator ||
                char.IsDigit(e.KeyChar) ||
                (int)e.KeyChar == (int)Keys.Back)
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void ObtenerDatos()
        {
            this.txtTitulo.Text = Convert.ToString(ConfigurationManager.AppSettings["Titulo"]);
            this.txtAncho.Text = Convert.ToString(ConfigurationManager.AppSettings["AnchoHoja"]);
            this.txtAlto.Text = Convert.ToString(ConfigurationManager.AppSettings["AltoHoja"]);
            this.txtMargenArriba.Text = Convert.ToString(ConfigurationManager.AppSettings["MargenArriba"]);
            this.txtMargenAbajo.Text = Convert.ToString(ConfigurationManager.AppSettings["MargenAbajo"]);
            this.txtMargenDerecho.Text = Convert.ToString(ConfigurationManager.AppSettings["MargenDerecha"]);
            this.txtMargenIzquierdo.Text = Convert.ToString(ConfigurationManager.AppSettings["MargenIzquierda"]);
            this.listaMedida.Text = Convert.ToString(ConfigurationManager.AppSettings["MedidaPredeterminada"]);
        }

        private void FrmConfiguracionReportes_Load(object sender, EventArgs e)
        {
            this.ObtenerDatos();
        }
    }
}
