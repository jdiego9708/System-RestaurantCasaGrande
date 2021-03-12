using CapaNegocio;
using CapaPresentacion.Formularios.FormsDetalles;
using System;
using System.Data;
using System.Windows.Forms;

namespace CapaPresentacion.Formularios.FormsInsumos
{
    public partial class FrmObservarInsumos : Form
    {
        public FrmObservarInsumos()
        {
            InitializeComponent();
            this.Load += FrmObservarInsumos_Load;
            this.txtBusqueda.onKeyPress += TxtBusqueda_KeyPress;
            this.txtBusqueda.onPxClick += TxtBusqueda_onPxClick;
            this.dgvInsumos.DoubleClick += DgvInsumos_DoubleClick;
            this.btnCargar.Click += BtnCargar_Click;
        }

        private void BtnCargar_Click(object sender, EventArgs e)
        {
            this.BuscarInsumos("COMPLETO", "");
        }

        private void DgvInsumos_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                if (this.dgvInsumos.Enabled)
                {
                    string rpta = "";
                    int fila = this.dgvInsumos.CurrentRow.Cells[0].RowIndex;
                    if (this.agregarInsumos != null)
                    {
                        this.agregarInsumos.ObtenerDatos(DatagridString.ReturnValuesOfCells(sender, fila, out rpta));
                        this.Close();
                    }
                    else if (this.detallePlato != null)
                    {
                        this.detallePlato.DatosInsumos(DatagridString.ReturnValuesOfCells(sender, fila, out rpta));
                    }
                }

            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "DgvInsumos_DoubleClick",
                    "Hubo un error con la tabla de datos", ex.Message);
            }
        }

        public FrmAgregarInsumos agregarInsumos;
        public FrmAgregarDetallePlato detallePlato;

        private void TxtBusqueda_onPxClick(object sender, EventArgs e)
        {
            CustomTextBox txt = (CustomTextBox)sender;
            this.BuscarInsumos("NOMBRE", txt.Texto);
        }

        private void TxtBusqueda_KeyPress(object sender, KeyPressEventArgs e)
        {
            CustomTextBox txt = (CustomTextBox)sender;
            if (e.KeyChar == (char)Keys.Enter)
            {
                this.BuscarInsumos("NOMBRE", txt.Texto);
            }
        }

        private void BuscarInsumos(string tipo_busqueda, string texto_busqueda)
        {
            try
            {
                DataTable table =
                    NInsumos.BuscarInsumos(tipo_busqueda, texto_busqueda, out string rpta);
                if (table != null)
                {
                    this.dgvInsumos.Enabled = true;
                    this.lblResultados.Text = "Se encontraron " + table.Rows.Count + " insumos";

                    this.dgvInsumos.PageSize = 25;
                    this.dgvInsumos.SetPagedDataSource(table, this.bindingNavigator1);

                    string[] columns_text_header =
                    {
                        "Id insumo", "Nombre", "Id tipo insumo", "Cantidad", "Medida", "Observaciones", "Id tipo insumo", "Tipo"
                    };
                    bool[] columns_visible =
                    {
                        false, true, false, true, true, true, false, true
                    };
                    this.dgvInsumos =
                        DatagridString.ChangeHeaderTextAndVisibleCustomDataGrid(this.dgvInsumos, columns_text_header, columns_visible);
                }
                else
                {
                    this.dgvInsumos.clearDataSource();
                    this.dgvInsumos.Enabled = false;
                    this.lblResultados.Text = "No se encontraron insumos";

                    if (!rpta.Equals("OK"))
                        throw new Exception(rpta);
                }
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "BuscarInsumos",
                    "Hubo un error al buscar insumos", ex.Message);
            }
        }

        private void FrmObservarInsumos_Load(object sender, EventArgs e)
        {
            this.txtBusqueda.Texto_inicial = "Escriba para buscar insumos";
            this.txtBusqueda.EstablecerTextoInicial();
            this.BuscarInsumos("COMPLETO", "");
        }
    }
}
