using System;
using System.Data;
using System.Windows.Forms;

namespace CapaPresentacion.Formularios.FormsPedido
{
    public partial class DescuentosOpcionesPedido : UserControl
    {
        public DescuentosOpcionesPedido()
        {
            InitializeComponent();
            this.Load += DescuentosOpcionesPedido_Load;
            this.txtPropina.LostFocus += Txt_LostFocus;
            this.txtPropina.GotFocus += Txt_GotFocus;
            this.txtPropina.KeyPress += Txt_KeyPress;
            this.ListaDescuentos.SelectedValueChanged += ListaDescuentos_SelectedValueChanged;
            this.lblPropinaSugerida.Click += LblPropinaSugerida_Click;
            this.chkEfectivo.CheckedChanged += ChkEfectivo_CheckedChanged;
            this.rdCredito.CheckedChanged += Rd_CheckedChanged;
            this.rdDebito.CheckedChanged += Rd_CheckedChanged;
            this.txtDebito.KeyPress += Txt_KeyPress;
            this.txtDebito.GotFocus += Txt_GotFocus;
            this.txtDebito.LostFocus += Txt_LostFocus;

            this.txtPrecioDesechables.KeyPress += Txt_KeyPress;
            this.txtPrecioDesechables.GotFocus += Txt_GotFocus;
            this.txtPrecioDesechables.LostFocus += Txt_LostFocus;

            this.txtDomicilio.KeyPress += Txt_KeyPress;
            this.txtDomicilio.GotFocus += Txt_GotFocus;
            this.txtDomicilio.LostFocus += Txt_LostFocus;

            this.txtEfectivo.KeyPress += Txt_KeyPress;
            this.txtEfectivo.GotFocus += Txt_GotFocus;
            this.txtEfectivo.LostFocus += Txt_LostFocus;
            this.txtDebito.TextChanged += TxtDebito_TextChanged;
            this.txtEfectivo.TextChanged += TxtEfectivo_TextChanged;
            this.btnDefault.Click += BtnDefault_Click;

            this.chkDesechables.CheckedChanged += ChkDesechables_CheckedChanged;
            this.chkDomicilio.CheckedChanged += ChkDomicilio_CheckedChanged;
        }

        private void ChkDomicilio_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox chk = (CheckBox)sender;
            if (chk.Checked)
                this.txtDomicilio.Visible = true;
            else
                this.txtDomicilio.Visible = false;
        }

        private void ChkDesechables_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox chk = (CheckBox)sender;
            if (chk.Checked)
                this.txtPrecioDesechables.Visible = true;
            else
                this.txtPrecioDesechables.Visible = false;
        }

        private void BtnDefault_Click(object sender, EventArgs e)
        {
            this.chkEfectivo.Checked = true;
            this.rdCredito.Checked = false;
            this.rdDebito.Checked = false;
            this.txtEfectivo.Tag = this.Total;
            this.txtEfectivo.Text = this.Total.ToString("C");
            int debito = 0;
            this.txtDebito.Text = debito.ToString("C");
            this.txtDebito.Tag = debito;
        }

        public DataTable TablaPago()
        {
            DataTable table = new DataTable();
            table.Columns.Add("Pago", typeof(string));
            table.Columns.Add("Valor_pago", typeof(string));
            table.Columns.Add("Vaucher", typeof(string));
            table.Columns.Add("Observaciones", typeof(string));

            if (this.chkEfectivo.Checked)
            {
                DataRow row = table.NewRow();
                row["Pago"] = "EFECTIVO";
                row["Valor_pago"] = this.txtEfectivo.Tag;
                row["Vaucher"] = "";
                row["Observaciones"] = "Ninguna";
                table.Rows.Add(row);
            }

            string otro_pago = this.ObtenerValorPanel(this.panel1);
            if (!otro_pago.Equals(""))
            {
                DataRow row = table.NewRow();
                row["Pago"] = otro_pago;
                row["Valor_pago"] = this.txtDebito.Tag;
                row["Vaucher"] = "";
                row["Observaciones"] = "Ninguna";
                table.Rows.Add(row);
            }
            return table;
        }

        private void TxtEfectivo_TextChanged(object sender, EventArgs e)
        {
            TextBox txt = (TextBox)sender;
            if (!this.ObtenerValorPanel(this.panel1).Equals(""))
            {
                int precio_digitado;
                bool result = int.TryParse(txt.Text, out precio_digitado);
                if (result)
                {
                    int pago_debito = Convert.ToInt32(this.Total) - precio_digitado;
                    this.txtDebito.Text = pago_debito.ToString("C");
                    this.txtDebito.Tag = pago_debito;
                }
            }
        }

        private void TxtDebito_TextChanged(object sender, EventArgs e)
        {
            TextBox txt = (TextBox)sender;
            if (this.chkEfectivo.Checked)
            {
                int precio_digitado;
                bool result = int.TryParse(txt.Text, out precio_digitado);
                if (result)
                {
                    int pago_efectivo = Convert.ToInt32(this.Total) - precio_digitado;
                    this.txtEfectivo.Text = pago_efectivo.ToString("C");
                    this.txtEfectivo.Tag = pago_efectivo;
                }
            }
        }

        private void Rd_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rd = (RadioButton)sender;
            if (rd.Checked)
            {
                if (this.chkEfectivo.Checked)
                {
                    double pago = this.Total / 2;

                    this.txtEfectivo.Enabled = true;
                    this.txtEfectivo.Text = pago.ToString("C");
                    this.txtEfectivo.Tag = pago;

                    this.txtDebito.Text = string.Format("{0:C}", pago);
                    this.txtDebito.Tag = pago;
                    this.txtDebito.Enabled = true;
                }
                else
                {
                    this.txtDebito.Text = this.Total.ToString("C");
                    this.txtDebito.Tag = this.Total;
                    this.txtDebito.Enabled = true;
                }
            }
        }

        private string ObtenerValorPanel(Panel panel)
        {
            string rpta = "";
            foreach (Control control in panel.Controls)
            {
                if (control is RadioButton rd)
                {
                    if (rd.Checked)
                    {
                        rpta = rd.Tag.ToString().ToUpper();
                        break;
                    }
                }
            }
            return rpta;
        }

        private void ChkEfectivo_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox chk = (CheckBox)sender;
            string tipo_pago = this.ObtenerValorPanel(this.panel1);
            if (chk.Checked)
            {
                if (tipo_pago.Equals(""))
                {
                    this.txtEfectivo.Enabled = true;
                    this.txtEfectivo.Text = this.Total.ToString("C");
                    this.txtEfectivo.Tag = this.Total;

                    this.txtDebito.Text = "$0";
                    this.txtDebito.Tag = "0";
                    this.txtDebito.Enabled = false;
                }
                else
                {
                    double pago = this.Total / 2;

                    this.txtEfectivo.Enabled = true;
                    this.txtEfectivo.Text = pago.ToString("C");
                    this.txtEfectivo.Tag = pago;

                    this.txtDebito.Text = string.Format("{0:C}", pago);
                    this.txtDebito.Tag = pago;
                    this.txtDebito.Enabled = true;
                }
            }
            else
            {
                if (tipo_pago.Equals(""))
                {
                    this.chkEfectivo.Checked = true;

                    this.txtEfectivo.Enabled = true;
                    this.txtEfectivo.Text = this.Total.ToString("C");
                    this.txtEfectivo.Tag = this.Total;

                    this.txtDebito.Text = "$0";
                    this.txtDebito.Tag = "0";
                    this.txtDebito.Enabled = false;

                    Mensajes.MensajeErrorForm("Debe de seleccionar como mínimo un método de pago");
                }
                else
                {
                    this.txtEfectivo.Enabled = false;
                    this.txtEfectivo.Text = "$0";
                    this.txtEfectivo.Tag = "0";

                    this.txtDebito.Text = this.Total.ToString("C");
                    this.txtDebito.Tag = this.Total;
                    this.txtDebito.Enabled = true;

                    this.txtEfectivo.Enabled = false;
                }
            }
        }

        private void LblPropinaSugerida_Click(object sender, EventArgs e)
        {
            if (this.txtPropina.Enabled)
            {
                this.txtPropina.Enabled = false;
                this.txtPropina.Tag = this.lblPropinaSugerida.Tag;
                this.txtPropina.Text = string.Format("{0:C}", this.lblPropinaSugerida.Tag);
            }
            else
            {
                this.txtPropina.Enabled = true;
                this.txtPropina.Text = Convert.ToString(this.lblPropinaSugerida.Tag);
            }
            this.OperacionSumarPrecios();
        }

        private void ListaDescuentos_SelectedValueChanged(object sender, EventArgs e)
        {
            ComboBox lista = (ComboBox)sender;
            int valor_descuento;
            bool result = int.TryParse(Convert.ToString(lista.SelectedValue), out valor_descuento);
            if (result)
            {
                if (Convert.ToInt32(lista.SelectedValue) == 0)
                {
                    this.gbCupon.Enabled = false;
                }
                else
                {
                    this.gbCupon.Enabled = true;
                }
                this.OperacionSumarPrecios();
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

            if (!Convert.ToString(txt.Name).Equals("txtDebito"))
            {
                this.OperacionSumarPrecios();
            }
        }

        private void OperacionSumarPrecios()
        {

            if (!decimal.TryParse(this.txtPrecioDesechables.Tag.ToString(), out decimal desechables))
                desechables = 0;

            if (!decimal.TryParse(this.txtDomicilio.Tag.ToString(), out decimal domicilio))
                domicilio = 0;

            decimal desc = Convert.ToDecimal(this.ListaDescuentos.SelectedValue);
            decimal descuento = desc / 100;
            int propina = Convert.ToInt32(this.txtPropina.Tag);
            int subtotal = this.Total_parcial + propina + Convert.ToInt32(desechables) + Convert.ToInt32(domicilio);

            decimal total_con_descuento = subtotal - (subtotal * descuento);

            this.lblSubtotal.Text = subtotal.ToString("C");
            this.lblSubtotal.Tag = subtotal;

            if (descuento == 0)
            {
                this.lblTotal.Text = subtotal.ToString("C");
                this.lblTotal.Tag = subtotal;
                this.Total = Convert.ToDouble(subtotal);
            }
            else
            {
                this.lblTotal.Text = total_con_descuento.ToString("C");
                this.lblTotal.Tag = total_con_descuento;
                this.Total = Convert.ToDouble(total_con_descuento);
            }

            this.frmFacturarPedido.ObtenerTotales(this.Total_parcial, 
                Convert.ToInt32(this.lblSubtotal.Tag), Convert.ToInt32(this.Total));
        }

        private void CrearTablaCuenta()
        {
            DataTable TablaDescuentos = new DataTable();
            TablaDescuentos.Columns.Add("num", typeof(string));
            TablaDescuentos.Columns.Add("descuento", typeof(string));
            int contador = 0;
            while (contador <= 100)
            {
                DataRow row = TablaDescuentos.NewRow();
                row["num"] = contador;
                row["descuento"] = contador + "%";
                TablaDescuentos.Rows.Add(row);
                contador += 5;
            }
            this.ListaDescuentos.DataSource = TablaDescuentos;
            this.ListaDescuentos.ValueMember = "num";
            this.ListaDescuentos.DisplayMember = "descuento";
        }

        private void DescuentosOpcionesPedido_Load(object sender, EventArgs e)
        {
            this.lblTotal.Tag = this.Total_parcial;
            this.lblTotalParcial.Tag = this.Total_parcial;

            this.lblTotalParcial.Text = this.Total_parcial.ToString("C");
            this.lblTotal.Text = this.Total_parcial.ToString("C");

            this.txtPropina.Enabled = false;
            double propinaSugerida = this.Total_parcial * 0.10;

            this.lblPropinaSugerida.Text = propinaSugerida.ToString("C");
            this.lblPropinaSugerida.Tag = propinaSugerida;

            this.txtPropina.Text = 0.ToString("C");
            this.txtPropina.Tag = 0;

            this.CrearTablaCuenta();
            this.chkEfectivo.Checked = true;
            this.txtDebito.Enabled = false;
        }

        private DataTable dtCuenta;
        private double total;
        private int total_parcial;
        private bool _isDomicilio;
        public bool IsDomicilio
        {
            get => _isDomicilio;
            set
            {
                _isDomicilio = value;
                if (value)
                {
                    this.chkDomicilio.Checked = true;
                }
                else
                {
                    this.chkDomicilio.Checked = false;
                }
            }
        }

        public FrmFacturarPedido frmFacturarPedido;
        public DataTable DtCuenta { get => dtCuenta; set => dtCuenta = value; }
        public double Total { get => total; set => total = value; }
        public int Total_parcial { get => total_parcial; set => total_parcial = value; }

    }
}
