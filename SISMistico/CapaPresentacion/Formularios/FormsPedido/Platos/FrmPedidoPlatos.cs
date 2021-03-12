using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CapaNegocio;
using CapaPresentacion.Formularios.FormsPedido;


namespace CapaPresentacion.Formularios.FormsPedido.Platos
{
    public partial class FrmPedidoPlatos : Form
    {
        public FrmPedidoPlatos()
        {
            InitializeComponent();
            this.Load += FrmPedidoPlatos_Load;
            this.txtBusqueda.ChangedText += TxtBusqueda_ChangedText;
            this.dgvPlatos.DoubleClick += DgvPlatos_DoubleClick;
        }

        public event EventHandler ondgvPlatosDoubleClick;

        private void DgvPlatos_DoubleClick(object sender, EventArgs e)
        {
            if (this.dgvPlatos.CurrentRow != null)
            {
                this.Id_plato = Convert.ToInt32(dgvPlatos.CurrentRow.Cells["Id_plato"].Value);
                this.Precio_cobrar = Convert.ToInt32(dgvPlatos.CurrentRow.Cells["Precio_plato"].Value);

                if (this.ondgvPlatosDoubleClick != null)
                    this.ondgvPlatosDoubleClick(this, e);
            }
        }

        private int id_plato;
        private int precio_cobrar;

        public int Id_plato { get => id_plato; set => id_plato = value; }
        public int Precio_cobrar { get => precio_cobrar; set => precio_cobrar = value; }

        private void TxtBusqueda_ChangedText(object sender, EventArgs e)
        {
            CustomTextBox txt = (CustomTextBox)sender;
            this.BuscarPlatos("NOMBRE", txt.Texto);
        }

        private Button Custombtn(string texto)
        {
            this.panelTipoPlatos.BackColor = Color.BlanchedAlmond;
            Button btn = new Button();
            btn.Cursor = Cursors.Hand;
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.BorderColor = Color.Silver;
            btn.FlatAppearance.MouseDownBackColor = Color.Silver;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            btn.FlatStyle = FlatStyle.Flat;
            btn.Location = new Point(0, 0);
            btn.Name = "btn" + texto;
            btn.Size = new Size(this.panelTipoPlatos.Width, 39);
            btn.TabIndex = 1;
            btn.Text = texto;
            btn.UseVisualStyleBackColor = true;
            btn.Click += Btn_Click;
            return btn;
        }

        private void Btn_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            this.BuscarPlatos("TIPO DE PLATO", btn.Text);
        }

        private void TipoPlatos()
        {
            try
            {
                DataTable Tabla =
                    NPlatos.BuscarTipoPlatos("COMPLETO", "");
                if (Tabla != null)
                {
                    int positionY = 0;
                    for (int i = 0; i <= Tabla.Rows.Count - 1; i++)
                    {
                        Button btn =
                            this.Custombtn(Convert.ToString(Tabla.Rows[i]["Tipo_plato"]));
                        btn.Location = new Point(0, positionY);
                        positionY += btn.Height + 2;
                        this.panelTipoPlatos.Controls.Add(btn);
                    }

                }
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "TipoPlatos",
                "Hubo un error con el metodo tipo platos", ex.Message);
            }
        }

        private void BuscarPlatos(string tipo_busqueda, string texto_busqueda)
        {
            try
            {
                DataTable Tabla =
                    NPlatos.BuscarPlatos(tipo_busqueda, texto_busqueda, "ACTIVO", out string rpta);
                this.dgvPlatos.DataSource = Tabla;
                if (Tabla != null)
                {
                    this.dgvPlatos.Enabled = true;
                    this.lblResultados.Text =
                        "Se encontraron " + Tabla.Rows.Count + " platos";
                    string[] columns_header_text =
                    {
                        "Id plato", "Nombre", "Id tipo", "Precio", "Imagen",
                        "Descripción", "Estado", "Id tipo", "Tipo"
                    };

                    bool[] columns_visible =
{
                        false, true, false, true, false, true, false, false, true
                    };

                    this.dgvPlatos =
                        DatagridString.ChangeColumnsVisible(this.dgvPlatos, columns_visible);
                }
                else
                {
                    this.dgvPlatos.Enabled = false;
                    this.lblResultados.Text =
                        "No se encontraron platos";

                    if (!rpta.Equals("OK"))
                        throw new Exception(rpta);
                }
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "BuscarPlatos",
                    "Hubo un error al buscar los platos", ex.Message);
            }
        }

        private void FrmPedidoPlatos_Load(object sender, EventArgs e)
        {
            this.TipoPlatos();
            this.dgvPlatos =
                ConfiguracionDatagridview.ConfigurationGrid(this.dgvPlatos);
        }
    }
}
