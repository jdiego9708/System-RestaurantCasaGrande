using CapaNegocio;
using CapaPresentacion.Formularios.FormsDetalles;
using System;
using System.Data;
using System.Windows.Forms;

namespace CapaPresentacion.Formularios.FormsPlatos
{
    public partial class FrmObservarPlatos : Form
    {
        public FrmObservarPlatos()
        {
            InitializeComponent();
            this.Load += FrmObservarPlatos_Load;
            this.dgvPlatos.DoubleClick += DgvPlatos_DoubleClick;
            this.btnBuscar.Click += BtnBuscar_Click;
            this.txtBusqueda.onKeyPress += TxtBusqueda_KeyPress;
        }

        public event EventHandler OnDgvDoubleClick;

        private void TxtBusqueda_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((int)e.KeyChar == (int)Keys.Enter)
            {
                this.btnBuscar.PerformClick();
            }
        }

        private void BtnBuscar_Click(object sender, EventArgs e)
        {
            if (this.txtBusqueda.Texto.Equals("") |
                this.txtBusqueda.Texto.Equals(this.txtBusqueda.Texto_inicial))
            {
                this.BuscarPlatos("COMPLETO", "");
            }
            else
            {
                this.BuscarPlatos("NOMBRE", this.txtBusqueda.Texto);
            }
        }

        public bool IsEditar { get; set; }
        public FrmAgregarDetallePlato agregarDetallePlato;

        public bool InactivarPlatos = false;

        private void DgvPlatos_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                DataGridViewRow rowGrid = this.dgvPlatos.CurrentRow;
                DataRow row = ((DataRowView)rowGrid.DataBoundItem).Row;
                if (this.dgvPlatos.Enabled && this.dgvPlatos.Focused && row != null)
                {
                    string rpta = "OK";
                    int fila = this.dgvPlatos.CurrentRow.Cells[0].RowIndex;
                    if (this.IsEditar)
                    {
                        CapaEntidades.Models.Platos plato = new CapaEntidades.Models.Platos(row);
                        this.OnDgvDoubleClick?.Invoke(plato, e);                     
                    }
                    else if (this.agregarDetallePlato != null)
                    {
                        this.agregarDetallePlato.ObtenerDatosPlato
                            (DatagridString.ReturnValuesOfCells(sender, fila, out rpta));
                    }
                    else if (this.InactivarPlatos)
                    {
                        Mensajes.MensajePregunta("¿Seguro desea inactivar el plato?",
                            "Continuar", "Cancelar", out DialogResult dialog);
                        if (dialog == DialogResult.Yes)
                        {
                            int id_plato = Convert.ToInt32(row["Id_plato"]);
                            rpta = NPlatos.InactivarPlato(id_plato);
                            if (rpta.Equals("OK"))
                            {
                                Mensajes.MensajeOkForm("Se inactivó el plato correctamente");
                                this.BuscarPlatos("COMPLETO", "");
                                return;
                            }
                            else
                                throw new Exception(rpta);
                        }
                    }

                    if (rpta.Equals("OK"))
                    {
                        this.Close();
                    }
                    else
                    {
                        throw new Exception(rpta);
                    }
                }
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "DgvPlatos_DoubleClick",
                    "Hubo un error con la tabla de datos", ex.Message);
            }
        }

        private void BuscarPlatos(string tipo_busqueda, string texto_busqueda)
        {
            try
            {
                string estado = this.rdActivo.Checked ? "ACTIVO" : "INACTIVO";
                DataTable Tabla =
                    NPlatos.BuscarPlatos(tipo_busqueda, texto_busqueda, estado, out string rpta);
                if (Tabla != null)
                {
                    this.dgvPlatos.Enabled = true;
                    this.dgvPlatos.PageSize = 25;
                    this.dgvPlatos.SetPagedDataSource(Tabla, this.bindingNavigator1);

                    this.lblResultados.Text =
                        "Se encontraron " + Tabla.Rows.Count + " platos";

                    string[] columns_header_text =
                    {
                        "Id plato", "Nombre", "Id tipo", "Precio", "Imagen",
                        "Descripción", "Estado","Plato detallado", "Plato carta", "Id tipo", "Tipo"
                    };

                    bool[] columns_visible =
{
                        false, true, false, true, false, true, false, false, false, false, true
                    };

                    this.dgvPlatos =
                        DatagridString.ChangeHeaderTextAndVisibleCustomDataGrid(this.dgvPlatos,
                        columns_header_text, columns_visible);
                }
                else
                {
                    this.dgvPlatos.clearDataSource();
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

        private void FrmObservarPlatos_Load(object sender, EventArgs e)
        {
            this.BuscarPlatos("COMPLETO", "");
            this.txtBusqueda.Texto_inicial = "Búsqueda de platos";
        }
    }
}
