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

namespace CapaPresentacion.Formularios.FormsEmpleados
{
    public partial class FrmObservarEmpleados : Form
    {
        public FrmObservarEmpleados()
        {
            InitializeComponent();
            this.Load += FrmObservarEmpleados_Load;
            this.txtBusqueda.Click += TxtBusqueda_Click;
            this.dgvEmpleados.DoubleClick += DgvEmpleados_DoubleClick;
            this.btnConfiguracionColumnas.Click += BtnConfiguracionColumnas_Click;
            this.txtBusqueda.onKeyPress += TxtBusqueda_onKeyPress;
            this.btnBuscar.Click += BtnBuscar_Click;
        }

        private void BtnBuscar_Click(object sender, EventArgs e)
        {
            string estado = this.rdActivo.Checked ? "ACTIVO" : "INACTIVO";
            string tipo_busqueda = "COMPLETO " + estado;
            if (this.txtBusqueda.Texto.Equals("") | 
                this.txtBusqueda.Texto.Equals(this.txtBusqueda.Texto_inicial))
            {
                this.BuscarEmpleados(tipo_busqueda, "");
            }
            else
            {
                tipo_busqueda = "BUSQUEDA COMPLETA " + estado;
                this.BuscarEmpleados(tipo_busqueda, this.txtBusqueda.Texto);
            }
        }

        private void TxtBusqueda_onKeyPress(object sender, KeyPressEventArgs e)
        {
            if ((int)e.KeyChar == (int)Keys.Enter)
            {
                this.btnBuscar.PerformClick();
            }
        }

        private void BtnConfiguracionColumnas_Click(object sender, EventArgs e)
        {
            if (this.panelColumnas.Visible)
                this.panelColumnas.Visible = false;
            else
                this.panelColumnas.Visible = true;
        }

        public FrmAgregarEmpleado FrmAgregarEmpleado;
        public bool venta = false;
        public bool facturar_pedido = false;

        private void DgvEmpleados_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                if (this.dgvEmpleados.Enabled)
                {
                    int fila = this.dgvEmpleados.CurrentRow.Cells[0].RowIndex;
                    string rpta = "SIN ACCION";
                    if (this.FrmAgregarEmpleado != null)
                    {
                        rpta = "";
                        this.FrmAgregarEmpleado.ObtenerDatos
                            (DatagridString.ReturnValuesOfCells(sender, fila, out rpta));
                    }
                    else if (this.venta)
                    {
                        rpta = "";
                        this.Tag = DatagridString.ReturnValuesOfCells(sender, fila, out rpta);
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    else if (this.facturar_pedido)
                    {
                        rpta = "";
                        this.Tag = DatagridString.ReturnValuesOfCells(sender, fila, out rpta);
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }

                    if (this.OnDataRow != null)
                    {
                        this.OnDataRow?.Invoke(dgvEmpleados.CurrentRow, e);
                        this.Close();
                    }

                    if (rpta.Equals("OK"))
                    {
                        this.Close();
                    }
                    else if (!rpta.Equals("SIN ACCION"))
                    {
                        throw new Exception(rpta);
                    }
                }
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "DgvEmpleados_DoubleClick",
                    "Hubo un error con la tabla de datos", ex.Message);
            }
        }

        private void TxtBusqueda_Click(object sender, EventArgs e)
        {
            CustomTextBox txt = (CustomTextBox)sender;
            if (txt.Texto.Equals("Ingrese un texto"))
            {
                this.txtBusqueda.txtBusqueda.Clear();
            }
        }

        private void BuscarEmpleados(string tipo_busqueda, string texto_busqueda)
        {
            try
            {
                string rpta;
                DataTable Tabla =
                    NEmpleados.BuscarEmpleados(tipo_busqueda, texto_busqueda, out rpta);
                this.dgvEmpleados.DataSource = Tabla;
                if (Tabla != null)
                {
                    this.dgvEmpleados.Enabled = true;
                    this.lblResultados.Text = "Se encontraron " + Tabla.Rows.Count + " empleados";
                    string[] columns_header_text =
                    {
                        "Id empleado", "Nombre", "Teléfono", "Correo electrónico", "Cargo", "Contraseña", "Clave maestra", "Estado"
                    };
                    this.dgvEmpleados =
                        DatagridString.ChangeColumnsHeaderText(this.dgvEmpleados, columns_header_text);
                    bool[] columns_visible =
{
                        false, true, true, true, true, false, false, false, false
                    };
                    this.dgvEmpleados =
                        DatagridString.ChangeColumnsVisible(this.dgvEmpleados, columns_visible);
                    this.CargarPanelColumnas(this.dgvEmpleados);
                }
                else
                {
                    this.dgvEmpleados.Enabled = false;
                    this.lblResultados.Text = "No se encontraron empleados";
                }
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "BuscarEmpleados",
                    "Hubo un error al buscar un empleado", ex.Message);
            }
        }

        private void FrmObservarEmpleados_Load(object sender, EventArgs e)
        {
            this.dgvEmpleados =
                ConfiguracionDatagridview.ConfigurationGrid(this.dgvEmpleados);
            this.BuscarEmpleados("COMPLETO ACTIVO", "");
            this.txtBusqueda.Texto_inicial = "Ingrese un texto a buscar";
        }

        private void CargarPanelColumnas(DataGridView dgv)
        {
            int locationX = 0;
            int locationY = 0;

            foreach (DataGridViewColumn column in dgv.Columns)
            {
                CheckBox chk = new CheckBox();
                chk.AutoEllipsis = true;
                chk.Text = column.HeaderText;
                chk.Name = column.Name;
                chk.Tag = column;
                chk.Checked = column.Visible;
                chk.Location = new Point(locationX, locationY);
                chk.CheckedChanged += Chk_CheckedChanged;
                locationX = chk.Location.X + chk.Width;
                this.panelColumnas.Controls.Add(chk);
            }
        }

        private void Chk_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox chk = (CheckBox)sender;
            foreach (DataGridViewColumn column in this.dgvEmpleados.Columns)
            {
                if (column.Name.Equals(chk.Name))
                {
                    column.Visible = chk.Checked;
                    this.dgvEmpleados.Refresh();
                    break;
                }
            }
        }

        public event EventHandler OnDataRow;
    }
}
