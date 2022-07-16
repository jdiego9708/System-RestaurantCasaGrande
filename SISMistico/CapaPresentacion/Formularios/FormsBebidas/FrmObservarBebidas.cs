using CapaNegocio;
using System;
using System.Data;
using System.Windows.Forms;

namespace CapaPresentacion.Formularios.FormsBebidas
{
    public partial class FrmObservarBebidas : Form
    {
        public FrmObservarBebidas()
        {
            InitializeComponent();
            this.Load += FrmObservarBebidas_Load;
            this.dgvBebidas.DoubleClick += DgvBebidas_DoubleClick;
            this.btnBuscar.Click += BtnBuscar_Click;
            this.txtBusqueda.onKeyPress += TxtBusqueda_KeyPress;
        }

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
                this.BuscarBebidas("COMPLETO", "");
            }
            else
            {
                this.BuscarBebidas("NOMBRE", this.txtBusqueda.Texto);
            }
        }

        private void DgvBebidas_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                DataGridViewRow row = this.dgvBebidas.CurrentRow;
                if (this.dgvBebidas.Enabled & row != null)
                {
                    string rpta = "";
                    if (this.FrmAgregarBebidas != null)
                    {
                        DataRow datarow = ((DataRowView)row.DataBoundItem).Row;
                        this.FrmAgregarBebidas.AsignarDatos(new CapaEntidades.Models.Bebidas(datarow));
                        this.FrmAgregarBebidas.Tag = datarow;
                        this.Close();
                    }
                    else if (this.InactivarBebidas)
                    {
                        Mensajes.MensajePregunta("¿Seguro desea inactivar la bebida?",
                            "Continuar", "Cancelar", out DialogResult dialog);
                        if (dialog == DialogResult.Yes)
                        {
                            int id_bebida = Convert.ToInt32(row.Cells["Id_bebida"].Value);
                            rpta = NBebidas.InactivarBebida(id_bebida);
                            if (rpta.Equals("OK"))
                            {
                                Mensajes.MensajeOkForm("Se inactivó la bebida correctamente");
                                this.BuscarBebidas("COMPLETO", "");
                                return;
                            }
                            else
                                throw new Exception(rpta);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "DgvBebidas_DoubleClick",
                    "Hubo un error con la tabla de datos", ex.Message);
            }
        }

        public FrmAgregarBebida FrmAgregarBebidas;
        public bool InactivarBebidas = false;

        private void BuscarBebidas(string tipo_busqueda, string texto_busqueda)
        {
            try
            {
                string estado = this.rdActivo.Checked ? "ACTIVO" : "INACTIVO";
                DataTable Tabla =
                    NBebidas.BuscarBebida(tipo_busqueda, texto_busqueda, estado, out string rpta);
                if (Tabla != null)
                {
                    this.dgvBebidas.Enabled = true;
                    this.dgvBebidas.Enabled = true;
                    this.dgvBebidas.PageSize = 25;
                    this.dgvBebidas.SetPagedDataSource(Tabla, this.bindingNavigator1);

                    this.lblResultados.Text =
                        "Se encontraron " + Tabla.Rows.Count + " bebidas";
                    string[] columns_header_text =
                    {
                          "Id bebida", "Nombre", "Descripcion", "Precio",
                          "Precio trago", "Precio trago doble", "Precio proveedor",
                          "Id proveedor", "Imagen", "Id Tipo", "Cantidad (Unidades)", "Cantidad por unidad", "Cantidad total", "Estado",
                          "Id tipo", "Tipo"
                    };

                    bool[] columns_visible =
{
                          false, true, true, true, false, false, false, false, false, false, false, false, false, false, false, true
                    };

                    this.dgvBebidas =
                        DatagridString.ChangeHeaderTextAndVisibleCustomDataGrid(this.dgvBebidas, columns_header_text, columns_visible);
                }
                else
                {
                    this.dgvBebidas.clearDataSource();
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
     
        private void FrmObservarBebidas_Load(object sender, EventArgs e)
        {
            this.BuscarBebidas("COMPLETO", "");
            this.txtBusqueda.Texto_inicial = "Búsqueda de bebidas";
        }
    }
}
