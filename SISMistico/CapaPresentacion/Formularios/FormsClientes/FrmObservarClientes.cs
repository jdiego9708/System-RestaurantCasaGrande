using CapaNegocio;
using CapaPresentacion.Formularios.FormsPedido;
using System;
using System.Data;
using System.Windows.Forms;

namespace CapaPresentacion.Formularios.FormsClientes
{
    public partial class FrmObservarClientes : Form
    {
        PoperContainer PoperContainer;

        public FrmObservarClientes()
        {
            InitializeComponent();

            this.Load += FrmObservarClientes_Load;
            this.dgvClientes.DoubleClick += DgvClientes_DoubleClick;
            this.btnAgregarCliente.MouseDown += BtnAgregarCliente_MouseDown;

            this.txtBusqueda.onKeyPress += TxtBusqueda_onKeyPress;
            this.txtBusqueda.onPxClick += TxtBusqueda_onPxClick;
        }

        public event EventHandler OnDgvDoubleClick;

        private void TxtBusqueda_onPxClick(object sender, EventArgs e)
        {
            CustomTextBox txt = (CustomTextBox)sender;
            if (txt.Texto.Equals(txt.Texto_inicial) | txt.Texto.Equals(""))
                this.BuscarClientes("COMPLETO", "");
            else
                this.BuscarClientes("BUSQUEDA COMPLETA", txt.Texto);
        }

        private void TxtBusqueda_onKeyPress(object sender, KeyPressEventArgs e)
        {
            CustomTextBox txt = (CustomTextBox)sender;
            if ((int)e.KeyChar == (int)Keys.Enter)
            {
                if (txt.Texto.Equals(txt.Texto_inicial) | txt.Texto.Equals(""))
                    this.BuscarClientes("COMPLETO", "");
                else
                    this.BuscarClientes("BUSQUEDA COMPLETA", txt.Texto);
            }
        }

        public void Actualizar()
        {
            this.BuscarClientes("COMPLETO", "");
        }

        private void BtnAgregarCliente_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                FrmAgregarCliente Frm = new FrmAgregarCliente
                {
                    FrmObservarClientes = this,
                    TopLevel = false,
                    FormBorderStyle = FormBorderStyle.None,
                    Dock = DockStyle.Fill
                };
                Frm.OnClienteSuccess += Frm_OnClienteSuccess;
                this.PoperContainer = new PoperContainer(Frm);
                this.PoperContainer.Show(btnAgregarCliente);
                Frm.Show();
            }
        }

        private void Frm_OnClienteSuccess(object sender, EventArgs e)
        {
            this.PoperContainer.Close();
        }

        public ContextMenuDatosPedido menuPedido;
        public FrmAgregarCliente FrmAgregarCliente;
        public bool venta = false;

        private void DgvClientes_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                DataGridViewRow row = this.dgvClientes.CurrentRow;
                if (this.dgvClientes.Enabled & row != null)
                {
                    string rpta = "OK";
                    int fila = this.dgvClientes.CurrentRow.Cells[0].RowIndex;
                    if (this.FrmAgregarCliente != null)
                    {
                        this.FrmAgregarCliente.ObtenerDatos
                            (DatagridString.ReturnValuesOfCells(sender, fila, out rpta));
                        this.Close();
                    }
                    else if (this.menuPedido != null)
                    {
                        this.menuPedido.ObtenerCliente
                            (DatagridString.ReturnValuesOfCells(sender, fila, out rpta));
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    else if (this.venta)
                    {
                        rpta = "OK";
                        this.DialogResult = DialogResult.OK;
                        this.Tag = DatagridString.ReturnValuesOfCells(sender, fila, out rpta);
                    }

                    this.OnDgvDoubleClick?.Invoke(row, e);

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
                Mensajes.MensajeErrorCompleto(this.Name, "DgvClientes_DoubleClick",
                    "Hubo un error con la tabla de datos", ex.Message);
            }
        }

        private void BuscarClientes(string tipo_busqueda, string texto_busqueda)
        {
            try
            {
                DataTable Tabla = NClientes.BuscarClientes(tipo_busqueda, texto_busqueda);
                this.dgvClientes.DataSource = Tabla;
                if (Tabla != null)
                {
                    this.lblResultados.Text = "Se encontraron " + Tabla.Rows.Count + " clientes";
                    this.dgvClientes.Enabled = true;
                    string[] columns_header_text =
                    {
                        "Id cliente", "Nombre", "Teléfono", "Correo electrónico"
                    };
                    this.dgvClientes =
                        DatagridString.ChangeColumnsHeaderText(this.dgvClientes,
                        columns_header_text);
                    bool[] columns_visible =
{
                        false, true, true, true
                    };
                    this.dgvClientes =
                        DatagridString.ChangeColumnsVisible(this.dgvClientes,
                        columns_visible);
                }
                else
                {
                    this.lblResultados.Text = "No se encontraron clientes";
                    this.dgvClientes.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "BuscarClientes",
                    "Hubo un error con la tabla de datos", ex.Message);
            }
        }

        private void FrmObservarClientes_Load(object sender, EventArgs e)
        {
            this.dgvClientes =
                ConfiguracionDatagridview.ConfigurationGrid(this.dgvClientes);
            this.txtBusqueda.Texto_inicial = "Ingrese un texto para buscar";
            this.BuscarClientes("COMPLETO", "");
        }
    }
}
