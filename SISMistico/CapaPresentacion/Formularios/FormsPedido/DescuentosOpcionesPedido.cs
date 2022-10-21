using System;
using System.Collections.Generic;
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

            this.txtPrecioDesechables.KeyPress += Txt_KeyPress;
            this.txtPrecioDesechables.GotFocus += Txt_GotFocus;
            this.txtPrecioDesechables.LostFocus += Txt_LostFocus;

            this.txtDomicilio.KeyPress += Txt_KeyPress;
            this.txtDomicilio.GotFocus += Txt_GotFocus;
            this.txtDomicilio.LostFocus += Txt_LostFocus;

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

        public DataTable TablaPago(bool isPrecuenta)
        {
            DataTable table = new DataTable();
            table.Columns.Add("Pago", typeof(string));
            table.Columns.Add("Valor_pago", typeof(string));
            table.Columns.Add("Vaucher", typeof(string));
            table.Columns.Add("Observaciones", typeof(string));

            if (isPrecuenta)
                return table;

            bool result = this.ObtenerMetodosPago(out List<MetodoPagoModel> MetodosPago);

            if (result)
            {
                foreach (MetodoPagoModel metodo in MetodosPago)
                {
                    DataRow row = table.NewRow();
                    row["Pago"] = metodo.MetodoPago;
                    row["Valor_pago"] = metodo.ValorPago;
                    row["Vaucher"] = metodo.Vaucher;
                    row["Observaciones"] = metodo.Observaciones;
                    table.Rows.Add(row);
                }
            }
            else
                return null;

            return table;
        }

        private void LoadMetodosPago()
        {
            List<MetodoPagoModel> metodos = new List<MetodoPagoModel>
            {
                new MetodoPagoModel
                {
                    MetodoPago = "EFECTIVO",
                    Vaucher = string.Empty,
                    ValorPago = 0,
                    Observaciones = string.Empty,
                },
                new MetodoPagoModel
                {
                    MetodoPago = "TARJETA",
                    Vaucher = string.Empty,
                    ValorPago = 0,
                    Observaciones = string.Empty,
                },
                new MetodoPagoModel
                {
                    MetodoPago = "TRANSFERENCIA",
                    Vaucher = string.Empty,
                    ValorPago = 0,
                    Observaciones = string.Empty,
                },
                new MetodoPagoModel
                {
                    MetodoPago = "VALE",
                    Vaucher = string.Empty,
                    ValorPago = 0,
                    Observaciones = string.Empty,
                },
            };

            this.panelMetodosPago.clearDataSource();

            List<UserControl> controls = new List<UserControl>();
            foreach(MetodoPagoModel metodo in metodos)
            {
                MetodoPagoSmall metodoPagoSmall = new MetodoPagoSmall
                {
                    MetodoPago = metodo,
                    Total = this.Total,
                };

                if (metodo.Equals("EFECTIVO"))
                    metodoPagoSmall.chkMetodo.Checked = true;

                controls.Add(metodoPagoSmall);
            }

            this.panelMetodosPago.AddArrayControl(controls);
        }

        private bool ObtenerMetodosPago(out List<MetodoPagoModel> metodos_pago)
        {
            metodos_pago = new List<MetodoPagoModel>();
            int cantidad_metodos_pago = 0;
            decimal total_valores = 0;

            foreach (UserControl control in panelMetodosPago.Controls)
            {
                if (control is MetodoPagoSmall metodo)
                {
                    if (metodo.chkMetodo.Checked)
                    {
                        if (decimal.TryParse(Convert.ToString(metodo.txtValor.Tag), out decimal valor))
                        {
                            metodos_pago.Add(new MetodoPagoModel
                            {
                                MetodoPago = metodo.chkMetodo.Text.ToUpper(),
                                Vaucher = string.Empty,
                                ValorPago = valor,
                            });
                            total_valores += valor;
                            cantidad_metodos_pago += 1;
                        }
                        else
                        {
                            //Mensajes.MensajeInformacion("Verifique los valores en métodos de pago");
                            return false;
                        }                      
                    }
                }
            }

            if (total_valores != this.Total)
            {
                Mensajes.MensajeInformacion("Verifique los valores del método de pago, la suma debe ser igual al valor total");
                return false;
            }

            if (cantidad_metodos_pago > 2)
            {
                Mensajes.MensajeInformacion("Solo se pueden seleccionar 2 métodos de pago");
                return false;
            }

            return true;
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

            this.lblSubtotal.Text = subtotal.ToString("C").Replace(",00", "");
            this.lblSubtotal.Tag = subtotal;

            if (descuento == 0)
            {
                this.lblTotal.Text = subtotal.ToString("C").Replace(",00", "");
                this.lblTotal.Tag = subtotal;
                this.Total = Convert.ToDecimal(subtotal);
            }
            else
            {
                this.lblTotal.Text = total_con_descuento.ToString("C").Replace(",00", "");
                this.lblTotal.Tag = total_con_descuento;
                this.Total = Convert.ToDecimal(total_con_descuento);
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

            this.lblTotalParcial.Text = this.Total_parcial.ToString("C").Replace(",00", "");
            this.lblTotal.Text = this.Total_parcial.ToString("C").Replace(",00", "");

            this.txtPropina.Enabled = false;
            double propinaSugerida = this.Total_parcial * 0.10;

            this.lblPropinaSugerida.Text = propinaSugerida.ToString("C").Replace(",00", "");
            this.lblPropinaSugerida.Tag = propinaSugerida;

            this.txtPropina.Text = propinaSugerida.ToString("C").Replace(",00", "");
            this.txtPropina.Tag = propinaSugerida;

            this.CrearTablaCuenta();
            this.LoadMetodosPago();
        }

        private DataTable dtCuenta;
        private decimal total;
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

        private void CambioTotal()
        {
            if (this.panelMetodosPago.Controls.Count > 0)
            {
                foreach(UserControl control in this.panelMetodosPago.controlsUser)
                {
                    if (control is MetodoPagoSmall metodo)
                    {
                        if (metodo.MetodoPago.MetodoPago.Equals("EFECTIVO"))
                        {
                            metodo.txtValor.Tag = this.Total;
                            metodo.txtValor.Text = this.Total.ToString("C").Replace(",00", "");
                        }
                    }
                }
            }
        }

        public FrmFacturarPedido frmFacturarPedido;
        public DataTable DtCuenta { get => dtCuenta; set => dtCuenta = value; }
        public decimal Total
        {
            get => total;
            set
            {
                total = value;
                this.CambioTotal();
            }
        }
        public int Total_parcial { get => total_parcial; set => total_parcial = value; }
    }
}
