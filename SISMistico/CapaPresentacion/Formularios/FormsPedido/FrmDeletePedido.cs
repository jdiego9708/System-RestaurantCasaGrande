using CapaNegocio;
using System;
using System.Data;
using System.Windows.Forms;

namespace CapaPresentacion.Formularios.FormsPedido
{
    public partial class FrmDeletePedido : Form
    {
        public FrmDeletePedido()
        {
            InitializeComponent();
            this.date1.ValueChanged += Date1_ValueChanged;
            this.Load += FrmDeletePedido_Load;
        }

        private void FrmDeletePedido_Load(object sender, EventArgs e)
        {
            this.Buscar("FECHA", this.date1.Value.ToShortDateString());
        }

        private void Date1_ValueChanged(object sender, EventArgs e)
        {
            this.Buscar("FECHA", this.date1.Value.ToShortDateString());
        }

        private void Buscar(string tipo_busqueda, string texto_busqueda)
        {
            try
            {
                DataTable dtPedidosEliminados =
                    NPedido.BuscarPedidosEliminados(tipo_busqueda, texto_busqueda);
                if (dtPedidosEliminados != null)
                {
                    this.dgvPedidosEliminados.Enabled = true;
                    this.dgvPedidosEliminados.PageSize = 15;
                    this.dgvPedidosEliminados.SetPagedDataSource(dtPedidosEliminados, this.bindingNavigator1);

                    lblRespuesta.Text =
                        "Se encontraron " + dtPedidosEliminados.Rows.Count + " pedidos eliminados";

                    string[] columns_header_text =
                    {
                        "Id pedido", "Id usuario clave maestra", "Ingresó clave maestra", "Id tipo", "Tipo", "Fecha", "Hora", "Motivo", "Nombre producto",
                        "Precio", "Id usuario sesion", "Tenía la sesión abierta", "Cargo", "Teléfono"
                    };
                    bool[] columns_visible =
                    {
                        false, false, true, false, true, true, true, true, true, true, false, true, false, false
                    };
                    this.dgvPedidosEliminados =
                        DatagridString.ChangeHeaderTextAndVisibleCustomDataGrid(this.dgvPedidosEliminados,
                        columns_header_text, columns_visible);
                    this.dgvPedidosEliminados.Columns["Precio"].DefaultCellStyle.Format = "C";
                    this.dgvPedidosEliminados.Columns["Hora"].DefaultCellStyle.Format = "hh:mm tt";
                }
                else
                {
                    this.dgvPedidosEliminados.clearDataSource();
                    this.dgvPedidosEliminados.Enabled = false;
                    lblRespuesta.Text = "No se encontraron pedidos eliminados";
                }
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "Buscar",
                    "Hubo un error al Buscar los pedidos eliminados",
                    ex.Message);
            }
        }
    }
}
