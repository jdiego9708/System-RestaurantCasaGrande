using CapaNegocio;
using CapaPresentacion.Formularios.FormsClientes;
using CapaPresentacion.Formularios.FormsEmpleados;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;

namespace CapaPresentacion.Formularios.FormsVentas
{
    public partial class FrmObservarVentas : Form
    {
        PoperContainer containerFiltros;
        Filtrosventa filtrosventa;

        PoperContainer containerResumen;
        ResumenVenta resumenVenta;

        public FrmObservarVentas()
        {
            InitializeComponent();
            this.Load += FrmObservarVentas_Load;
            this.date1.ValueChanged += Date1_ValueChanged;
            this.btnFiltros.Click += BtnFiltros_Click;
            this.btnBuscar.Click += BtnBuscar_Click;
            this.btnResumen.Click += BtnResumen_Click;
            this.dgvVentas.DoubleClick += DgvVentas_DoubleClick;
            this.btnOtrasOpciones.Click += BtnOtrasOpciones_Click;
            this.chkEliminarCuentas.Click += ChkEliminarCuentas_Click;
            this.dgvVentas.CellContentClick += DgvVentas_CellContentClick;
            this.btnEliminarCuentas.Click += BtnEliminarCuentas_Click;
        }

        private void BtnEliminarCuentas_Click(object sender, EventArgs e)
        {
            if (this.dgvVentas.Rows.Count > 0 & this.chkEliminarCuentas.Checked)
            {
                List<DataGridViewRow> Rows = new List<DataGridViewRow>();
                foreach (DataGridViewRow row in this.dgvVentas.Rows)
                {
                    if (Convert.ToBoolean(row.Cells["chkEliminar"].Value))
                    {
                        Rows.Add(row);
                    }
                }

                if (Rows.Count > 0)
                {
                    Mensajes.MensajePregunta("¿Seguro desea eliminar " + Rows.Count + " cuentas seleccionadas?",
                        "Eliminar", "Cancelar", out DialogResult dialog);
                    if (dialog == DialogResult.Yes)
                    {
                        string rpta = "";
                        foreach (DataGridViewRow row2 in Rows)
                        {
                            int id_venta = Convert.ToInt32(row2.Cells["Id_venta"].Value);
                            rpta = NVentas.InactivarVenta(id_venta);
                            if (!rpta.Equals("OK"))
                                break;
                        }

                        if (rpta.Equals("OK"))
                        {
                            Mensajes.MensajeOkForm("Se eliminaron las ventas correctamente");
                            foreach (DataGridViewRow row in this.dgvVentas.Rows)
                            {
                                if (Rows.Contains(row))
                                    this.dgvVentas.Rows.Remove(row);
                            }
                            this.chkEliminarCuentas.Checked = false;

                        }
                        else
                        {
                            Mensajes.MensajeErrorCompleto(this.Name, "BtnEliminarCuentas_Click",
                                "Hubo  un error al eliminar cuentas", rpta);
                        }
                    }     
                }
                else
                {
                    Mensajes.MensajeInformacion("No hay ventas seleccionadas", "Entendido");
                }
            }
        }

        private void DgvVentas_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (this.chkEliminarCuentas.Checked)
                {
                    DataGridViewRow row = dgvVentas.CurrentRow;
                    if (e.ColumnIndex == this.dgvVentas.Columns["chkEliminar"].Index)
                    {
                        if (row.Cells["chkEliminar"].Value != null && (bool)row.Cells["chkEliminar"].Value)
                        {
                            row.Cells["chkEliminar"].Value = false;
                            row.Cells["chkEliminar"].Value = null;
                        }
                        else if (row.Cells["chkEliminar"].Value == null)
                        {
                            row.Cells["chkEliminar"].Value = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "DgvVentas_CellContentClick",
                    "Hubo un error con la tabla de datos", ex.Message);
            }
        }

        private void ChkEliminarCuentas_Click(object sender, EventArgs e)
        {
            if (this.dgvVentas.DataSource != null)
            {
                CheckBox chk = (CheckBox)sender;
                if (chk.Checked)
                {
                    DataGridViewCheckBoxColumn chkColumn = new DataGridViewCheckBoxColumn();
                    chkColumn.Name = "chkEliminar";
                    chkColumn.HeaderText = "Eliminar";
                    chkColumn.DisplayIndex = 0;
                    this.dgvVentas.Columns.Add(chkColumn);
                    this.btnEliminarCuentas.Visible = true;
                }
                else
                {
                    this.btnEliminarCuentas.Visible = false;
                    foreach (DataGridViewColumn column in this.dgvVentas.Columns)
                    {
                        if (column.Name.Equals("chkEliminar"))
                        {
                            this.dgvVentas.Columns.Remove(column);
                            break;
                        }
                    }
                }
            }
        }

        private void BtnOtrasOpciones_Click(object sender, EventArgs e)
        {
            if (this.gbOpciones.Visible)
            {
                this.gbOpciones.Visible = false;
                this.chkEliminarCuentas.Checked = false;
            }
            else
                this.gbOpciones.Visible = true;
        }

        private void DgvVentas_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                DataGridViewRow row = this.dgvVentas.CurrentRow;
                if (row != null)
                {
                    FrmDetalleVenta venta = new FrmDetalleVenta();
                    venta.Id_venta = Convert.ToInt32(row.Cells["Id_venta"].Value);
                    venta.Row_venta = row;
                    venta.StartPosition = FormStartPosition.CenterScreen;
                    venta.Show();
                }

            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "DgvVentas_DoubleClick",
                    "Hubo un error con la tabla de datos", ex.Message);
            }
        }

        private void BtnResumen_Click(object sender, EventArgs e)
        {
            if (this.dgvVentas.DataSource != null)
            {
                this.containerResumen.Show(btnResumen);
            }
        }

        private void BtnBuscar_Click(object sender, EventArgs e)
        {
            string tipo = this.filtrosventa.ObtenerValorPanel(this.filtrosventa.panel1);
            if (tipo.Equals("NINGUNO"))
            {
                this.BuscarVentas("VENTA FECHAS", "");
            }
            else if (tipo.Equals("MESA"))
            {
                this.BuscarVentas("MESA", Convert.ToString(this.filtrosventa.listaMesas.SelectedValue));
            }
            else
            {
                this.BuscarVentas(tipo, Convert.ToString(this.filtrosventa.txtTipo.Tag));
            }
        }

        private void BtnSeleccione_Click(object sender, EventArgs e)
        {
            string tipo = this.filtrosventa.ObtenerValorPanel(this.filtrosventa.panel1);
            if (!tipo.Equals(""))
            {
                switch (tipo.ToUpper())
                {
                    case "NINGUNO":
                        this.filtrosventa.btnSeleccione.Enabled = false;
                        this.filtrosventa.txtTipo.Enabled = false;
                        break;
                    case "CLIENTE":
                        FrmObservarClientes frmObservarClientes = new FrmObservarClientes();
                        frmObservarClientes.StartPosition = FormStartPosition.CenterScreen;
                        frmObservarClientes.venta = true;
                        frmObservarClientes.FormClosed += FrmObservarClientes_FormClosed;
                        frmObservarClientes.Show();
                        break;
                    case "EMPLEADO":
                        FrmObservarEmpleados frmObservarEmpleados = new FrmObservarEmpleados();
                        frmObservarEmpleados.StartPosition = FormStartPosition.CenterScreen;
                        frmObservarEmpleados.venta = true;
                        frmObservarEmpleados.FormClosed += FrmObservarEmpleados_FormClosed;
                        frmObservarEmpleados.Show();
                        break;
                    case "MESA":
                        this.filtrosventa.btnSeleccione.Enabled = true;
                        this.filtrosventa.btnSeleccione.Text = "Seleccione un empleado";
                        this.filtrosventa.txtTipo.Enabled = true;
                        this.filtrosventa.txtTipo.Text = "Información empleado";
                        break;
                }
                this.filtrosventa.btnSeleccione.Tag = tipo.ToUpper();
            }
        }

        private void BtnFiltros_Click(object sender, EventArgs e)
        {
            this.containerFiltros.Show(this.btnFiltros);
        }

        private void BuscarVentas(string tipo_busqueda, string texto_busqueda)
        {
            try
            {
                string fecha1 = this.date1.Value.ToShortDateString();
                string fecha2 = this.date2.Value.ToShortDateString();
                string hora1 = this.ListaHora1.SelectedValue.ToString();
                string hora2 = this.ListaHora2.SelectedValue.ToString();

                DataTable tablaVenta =
                    NVentas.BuscarVenta(tipo_busqueda, texto_busqueda, fecha1, fecha2, hora1, hora2);
                this.dgvVentas.DataSource = tablaVenta;
                if (tablaVenta != null)
                {
                    this.btnResumen.Enabled = true;
                    string[] columns_header =
                    {
                        "Id venta", "Id pedido", "Fecha", "Hora", "Total parcial", "Propina", "Subtotal",
                        "Descuento", "Bono o cupón", "Total", "Observaciones", "Id cliente", "Nombre cliente",
                        "Id mesa", "Mesa", "Id empleado", "Empleado"
                    };
                    bool[] columns_visible =
                    {
                        false, false, true, true, true, true, true, true,
                        false, true, false, false, true, false, true, false, true
                    };
                    this.dgvVentas =
                        DatagridString.ChangeHeaderTextAndVisible(this.dgvVentas, columns_header, columns_visible);

                    //this.dgvVentas.Columns["Hora_venta"].DefaultCellStyle.Format = "HH:mm";

                    this.resumenVenta.ObtenerVenta(Convert.ToDateTime(fecha1), Convert.ToDateTime(fecha2),
                        hora1, hora2);
                }
                else
                {
                    this.btnResumen.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "BuscarVentas", "Hubo un error al buscar una venta", ex.Message);
            }
        }

        private void Horas()
        {
            DataTable tablaHorasInicio = new DataTable("TablaHoras");
            tablaHorasInicio.Columns.Add("Hora", typeof(string));
            tablaHorasInicio.Columns.Add("Value_hora", typeof(string));

            DataTable tablaHorasFin = new DataTable("TablaHoras");
            tablaHorasFin.Columns.Add("Hora", typeof(string));
            tablaHorasFin.Columns.Add("Value_hora", typeof(string));

            bool am = true;
            int contador = 1;
            int contador_horas = 1;

            while (contador_horas <= 24)
            {
                DataRow row1 = tablaHorasInicio.NewRow();
                DataRow row2 = tablaHorasFin.NewRow();
                if (am)
                {
                    row1["Hora"] = contador + ":00 am";
                    row1["Value_hora"] = contador_horas + ":00";

                    row2["Hora"] = contador + ":00 am";
                    row2["Value_hora"] = contador_horas + ":00";
                }
                else
                {
                    row1["Hora"] = contador + ":00 pm";
                    row1["Value_hora"] = contador_horas + ":00";

                    row2["Hora"] = contador + ":00 pm";
                    row2["Value_hora"] = contador_horas + ":00";
                }
                tablaHorasInicio.Rows.Add(row1);
                tablaHorasFin.Rows.Add(row2);
                contador += 1;
                contador_horas += 1;
                if (contador == 13)
                {
                    am = false;
                    contador = 1;
                }
            }
            this.ListaHora1.DataSource = tablaHorasInicio;
            this.ListaHora1.ValueMember = "Value_hora";
            this.ListaHora1.DisplayMember = "Hora";

            this.ListaHora2.DataSource = tablaHorasFin;
            this.ListaHora2.ValueMember = "Value_hora";
            this.ListaHora2.DisplayMember = "Hora";
        }

        private void Date1_ValueChanged(object sender, EventArgs e)
        {
            DateTimePicker date = (DateTimePicker)sender;
            this.date2.MinDate = date.Value;
        }

        private void FrmObservarVentas_Load(object sender, EventArgs e)
        {
            this.resumenVenta = new ResumenVenta();
            this.containerResumen = new PoperContainer(resumenVenta);

            this.filtrosventa = new Filtrosventa();
            this.filtrosventa.rdNinguno.Checked = true;
            this.filtrosventa.btnSeleccione.Click += BtnSeleccione_Click;
            this.containerFiltros = new PoperContainer(filtrosventa);
            this.dgvVentas =
                ConfiguracionDatagridview.ConfigurationGrid(this.dgvVentas);
            this.Horas();
        }

        private void FrmObservarClientes_FormClosed(object sender, FormClosedEventArgs e)
        {
            Form frm = sender as FrmObservarClientes;

            if (frm.DialogResult == DialogResult.OK)
            {
                List<string> datosCliente = (List<string>)frm.Tag;

                this.filtrosventa.txtTipo.Text = datosCliente[1];
                this.filtrosventa.txtTipo.Tag = datosCliente[0];
                this.Tag = datosCliente;
            }
        }

        private void FrmObservarEmpleados_FormClosed(object sender, FormClosedEventArgs e)
        {
            Form frm = sender as FrmObservarEmpleados;

            if (frm.DialogResult == DialogResult.OK)
            {
                List<string> datosEmpleado = (List<string>)frm.Tag;
                this.filtrosventa.txtTipo.Text = datosEmpleado[1];
                this.filtrosventa.txtTipo.Tag = datosEmpleado[0];
                this.Tag = datosEmpleado;
            }
        }

    }
}
