using CapaNegocio;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace CapaPresentacion.Formularios.FormsPedido.Bebidas
{
    public partial class FrmPedidoBebidas : Form
    {
        public FrmPedidoBebidas()
        {
            InitializeComponent();
            this.Load += FrmPedidoBebidas_Load;
            this.txtBusqueda.ChangedText += TxtBusqueda_ChangedText;
            this.dgvBebidas.DoubleClick += DgvBebidas_DoubleClick;
        }

        private int id_bebida;
        private int precio_cobrar;
        public int Id_bebida { get => id_bebida; set => id_bebida = value; }
        public int Precio_cobrar { get => precio_cobrar; set => precio_cobrar = value; }

        public event EventHandler ondgvBebidasDoubleClick;

        private void DgvBebidas_DoubleClick(object sender, EventArgs e)
        {
            if (this.dgvBebidas.CurrentRow != null)
            {
                this.Id_bebida = Convert.ToInt32(dgvBebidas.CurrentRow.Cells["Id_bebida"].Value);

                int precio_bebida = Convert.ToInt32(dgvBebidas.CurrentRow.Cells["Precio_bebida"].Value);
                int precio_trago = Convert.ToInt32(dgvBebidas.CurrentRow.Cells["Precio_trago"].Value);
                int precio_trago_doble = Convert.ToInt32(dgvBebidas.CurrentRow.Cells["Precio_trago_doble"].Value);

                if (precio_trago == 0 & precio_trago_doble == 0)
                {
                    this.Precio_cobrar = precio_bebida;
                    if (this.ondgvBebidasDoubleClick != null)
                        this.ondgvBebidasDoubleClick(this, e);
                }
                else
                {
                    FrmPreciosBebidas frmPreciosBebidas = new FrmPreciosBebidas
                    {
                        StartPosition = FormStartPosition.CenterScreen
                    };
                    frmPreciosBebidas.ObtenerBebida(this.dgvBebidas.CurrentRow);
                    frmPreciosBebidas.FormClosed += FrmPreciosBebidas_FormClosed;
                    frmPreciosBebidas.ShowDialog();
                    if (frmPreciosBebidas.DialogResult == DialogResult.OK)
                    {
                        if (this.ondgvBebidasDoubleClick != null)
                            this.ondgvBebidasDoubleClick(this, e);
                    }
                }
            }
        }

        private void FrmPreciosBebidas_FormClosed(object sender, FormClosedEventArgs e)
        {
            FrmPreciosBebidas frmPreciosBebidas = (FrmPreciosBebidas)sender;
            if (frmPreciosBebidas.DialogResult == DialogResult.OK)
            {
                this.Precio_cobrar = frmPreciosBebidas.Precio_cobrar;
            }
        }

        private void TxtBusqueda_ChangedText(object sender, EventArgs e)
        {
            CustomTextBox txt = (CustomTextBox)sender;
            this.BuscarBebidas("NOMBRE", txt.Texto);
        }

        private Button Custombtn(string texto)
        {
            this.panelTipoBebidas.BackColor = Color.BlanchedAlmond;
            Button btn = new Button();
            btn.Cursor = Cursors.Hand;
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.BorderColor = Color.Silver;
            btn.FlatAppearance.MouseDownBackColor = Color.Silver;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            btn.FlatStyle = FlatStyle.Flat;
            btn.Location = new Point(0, 0);
            btn.Name = "btn" + texto;
            btn.Size = new Size(this.panelTipoBebidas.Width, 39);
            btn.TabIndex = 1;
            btn.Text = texto;
            btn.UseVisualStyleBackColor = true;
            btn.Click += Btn_Click;
            return btn;
        }

        private void Btn_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            this.BuscarBebidas("TIPO BEBIDA", btn.Text);
        }

        private void TipoBebidas()
        {
            try
            {
                DataTable Tabla =
                    NBebidas.BuscarTipoBebidas("COMPLETO", "");
                if (Tabla != null)
                {
                    int positionY = 0;
                    for (int i = 0; i <= Tabla.Rows.Count - 1; i++)
                    {
                        Button btn =
                            this.Custombtn(Convert.ToString(Tabla.Rows[i]["Tipo_bebida"]));
                        btn.Location = new Point(0, positionY);
                        positionY += btn.Height + 2;
                        this.panelTipoBebidas.Controls.Add(btn);
                    }

                }
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "TipoBebidas",
                "Hubo un error con el metodo tipo bebidas", ex.Message);
            }
        }

        private void BuscarBebidas(string tipo_busqueda, string texto_busqueda)
        {
            try
            {
                DataTable Tabla =
                    NBebidas.BuscarBebida(tipo_busqueda, texto_busqueda, "ACTIVO", out string rpta);
                this.dgvBebidas.DataSource = Tabla;
                if (Tabla != null)
                {
                    this.dgvBebidas.Enabled = true;
                    this.lblResultados.Text =
                        "Se encontraron " + Tabla.Rows.Count + " bebidas";
                    string[] columns_header_text =
                    {
                          "Id bebida", "Nombre", "Precio",
                          "Precio trago", "Precio trago doble", "Precio proveedor",
                          "Id proveedor", "Imagen", "Id Tipo", "Cantidad (Unidades)", "Cantidad por unidad", "Cantidad total", "Estado",
                          "Id tipo", "Tipo"
                    };

                    bool[] columns_visible =
{
                          false, true, true, false, false, false, false, false, false, true, true, true, false, false, true
                    };
                    this.dgvBebidas =
                        DatagridString.ChangeColumnsVisible(this.dgvBebidas, columns_visible);
                }
                else
                {
                    this.dgvBebidas.Enabled = false;
                    this.lblResultados.Text =
                        "No se encontraron bebidas";

                    if (!rpta.Equals("OK"))
                        throw new Exception(rpta);
                }
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "BuscarBebidas",
                    "Hubo un error al buscar una bebida", ex.Message);
            }
        }

        private void FrmPedidoBebidas_Load(object sender, EventArgs e)
        {
            this.TipoBebidas();
            this.dgvBebidas =
                ConfiguracionDatagridview.ConfigurationGrid(this.dgvBebidas);
            this.BuscarBebidas("COMPLETO", "");
        }
    }
}
