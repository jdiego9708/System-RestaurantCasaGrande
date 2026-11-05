using CapaNegocio;
using CapaPresentacion.Formularios.FormsClientes;
using CapaPresentacion.Formularios.FormsEmpleados;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using CapaPresentacion.UI;
using System.Windows.Forms;

namespace CapaPresentacion.Formularios.FormsVentas
{
    public partial class FrmObservarVentas : Form
    {
        PoperContainer containerFiltros;
        Filtrosventa filtrosventa;

        PoperContainer containerResumen;
        ResumenVenta resumenVenta;
        private readonly Label salesCount = new Label();
        private readonly Label salesTotal = new Label();
        private readonly Label salesAverage = new Label();
        private readonly Label resultsHint = new Label();
        private static readonly CultureInfo SalesCulture = CultureInfo.GetCultureInfo("es-CO");

        public FrmObservarVentas()
        {
            InitializeComponent();
            this.BuildModernLayout();
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
            this.dgvVentas.DataBindingComplete += (s, e) => { StyleSalesColumns(); UpdateSalesSummary(); };
            this.dgvVentas.RowsRemoved += (s, e) => UpdateSalesSummary();
            this.dgvVentas.CellFormatting += FormatSalesCell;
        }

        private void BuildModernLayout()
        {
            this.SuspendLayout();
            this.ApplyTheme();
            this.MinimumSize = new Size(820, 560);
            this.ClientSize = new Size(1180, 720);
            Panel root = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 16, 20, 16) };
            Panel header = new Panel { Dock = DockStyle.Top, Height = 72 };
            Label title = new Label { Text = "Historial de ventas", AutoSize = true };
            title.StyleLabel(LabelKind.Title);
            Label subtitle = new Label
            {
                Text = "Consulta tus ventas por fecha y abre cada factura con doble clic.",
                AutoSize = true, Location = new Point(2, 38),
            };
            subtitle.StyleLabel(LabelKind.Muted);
            header.Controls.Add(title);
            header.Controls.Add(subtitle);

            ModernCard filters = new ModernCard { Dock = DockStyle.Top, Height = 190 };
            FlowLayoutPanel dates = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 64, WrapContents = false };
            foreach (var entry in new[] { Tuple.Create("Desde", this.date1), Tuple.Create("Hasta", this.date2) })
            {
                Panel field = new Panel { Width = 272, Height = 58, Margin = new Padding(0, 0, 16, 0) };
                Label caption = new Label { Text = entry.Item1, Dock = DockStyle.Top, Height = 22 };
                caption.StyleLabel(LabelKind.Caption);
                entry.Item2.Format = DateTimePickerFormat.Custom;
                entry.Item2.CustomFormat = "dddd, dd MMM yyyy";
                entry.Item2.Font = Theme.Body;
                entry.Item2.Dock = DockStyle.Top;
                field.Controls.Add(entry.Item2);
                field.Controls.Add(caption);
                dates.Controls.Add(field);
            }
            FlowLayoutPanel actions = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = true };
            foreach (Button button in new[] { this.btnBuscar, this.btnFiltros, this.btnOtrasOpciones })
            {
                button.Image = null;
                button.Size = new Size(button == this.btnBuscar ? 120 : 160, 38);
                button.TextAlign = ContentAlignment.MiddleCenter;
                button.Margin = new Padding(0, 0, 10, 0);
                button.StyleButton(button == this.btnBuscar ? ButtonKind.Primary : ButtonKind.Outline);
                actions.Controls.Add(button);
            }
            this.btnFiltros.Text = "Filtros adicionales";
            ModernButton alegra = new ModernButton { Text = "Configurar Alegra", Width = 175, Height = 38, Kind = ButtonKind.Outline };
            alegra.Click += (s,e) => { try { using (var setup = new FrmAlegraSetup()) setup.ShowDialog(this); } catch (Exception) { Mensajes.MensajeInformacion("No se pudo abrir la configuración Alegra. Aplique la migración 07 de base de datos."); } };
            actions.Controls.Add(alegra);
            ModernButton invoices = new ModernButton { Text = "Facturas de Alegra", Width = 185, Height = 38, Kind = ButtonKind.Primary };
            invoices.Click += (s,e) => { var window = new FrmAlegraInvoices(); window.Show(this); };
            actions.Controls.Add(invoices);
            this.btnOtrasOpciones.Text = "Otras opciones";
            filters.Controls.Add(actions);
            filters.Controls.Add(dates);

            TableLayoutPanel summary = new TableLayoutPanel
            {
                Dock = DockStyle.Top, Height = 84, ColumnCount = 3, RowCount = 1,
            };
            Label[] values = { salesCount, salesTotal, salesAverage };
            for (int i = 0; i < values.Length; i++)
            {
                summary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / 3));
                ModernCard card = new ModernCard { Dock = DockStyle.Fill, Padding = new Padding(14, 10, 14, 8) };
                values[i].Dock = DockStyle.Fill;
                values[i].Font = Theme.Heading;
                values[i].ForeColor = i == 1 ? Theme.Success : Theme.TextPrimary;
                values[i].TextAlign = ContentAlignment.MiddleLeft;
                card.Controls.Add(values[i]);
                summary.Controls.Add(card, i, 0);
            }

            this.gbOpciones.Parent = root;
            this.gbOpciones.Dock = DockStyle.Top;
            this.gbOpciones.Height = 64;
            this.gbOpciones.Font = Theme.Small;
            this.gbOpciones.ForeColor = Theme.TextMuted;
            this.btnEliminarCuentas.Image = null;
            this.btnEliminarCuentas.StyleButton(ButtonKind.Danger);
            this.btnEliminarCuentas.TextAlign = ContentAlignment.MiddleCenter;

            ModernCard table = new ModernCard { Dock = DockStyle.Fill };
            resultsHint.Dock = DockStyle.Top;
            resultsHint.Height = 32;
            resultsHint.StyleLabel(LabelKind.Caption);
            this.dgvVentas.Parent = table;
            this.dgvVentas.Dock = DockStyle.Fill;
            table.Controls.Add(resultsHint);
            this.StyleSalesGrid();
            root.Controls.Add(table);
            table.BringToFront();
            root.Controls.Add(summary);
            root.Controls.Add(filters);
            root.Controls.Add(header);
            this.Controls.Add(root);
            // Los controles heredados de hora permanecen disponibles para las consultas existentes.
            this.groupBox1.Visible = false;
            this.btnResumen.Visible = false;
            this.UpdateSalesSummary();
            this.ResumeLayout(true);
        }

        private void StyleSalesGrid()
        {
            this.dgvVentas.StyleGrid();
            this.dgvVentas.Dock = DockStyle.Fill;
            this.dgvVentas.RowsDefaultCellStyle = new DataGridViewCellStyle();
            this.dgvVentas.AllowUserToAddRows = false;
            this.dgvVentas.AllowUserToDeleteRows = false;
            this.dgvVentas.ReadOnly = true;
            this.dgvVentas.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvVentas.MultiSelect = false;
            this.dgvVentas.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            this.dgvVentas.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            this.dgvVentas.RowTemplate.Height = 40;
            this.dgvVentas.DefaultCellStyle.Padding = new Padding(10, 4, 10, 4);
            this.dgvVentas.DefaultCellStyle.NullValue = "—";
            this.dgvVentas.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvVentas.ColumnHeadersHeight = 44;
            this.dgvVentas.ColumnHeadersDefaultCellStyle.Padding = new Padding(10, 0, 10, 0);
            this.dgvVentas.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;
        }

        private void StyleSalesColumns()
        {
            foreach (DataGridViewColumn column in this.dgvVentas.Columns)
            {
                column.MinimumWidth = 80;
                column.Width = 124;
                column.DefaultCellStyle.FormatProvider = SalesCulture;
                string name = column.DataPropertyName.Length > 0 ? column.DataPropertyName : column.Name;
                switch (name)
                {
                    case "Fecha_venta": column.HeaderText = "Fecha"; column.Width = 124; break;
                    case "Hora_venta": column.HeaderText = "Hora"; column.Width = 110; break;
                    case "Nombre_cliente":
                        column.HeaderText = "Cliente"; column.MinimumWidth = 190; column.Width = 220; break;
                    case "Nombre_empleado":
                        column.HeaderText = "Atendido por"; column.MinimumWidth = 170; column.Width = 190; break;
                    case "Num_mesa": column.HeaderText = "Mesa"; column.Width = 86;
                        column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter; break;
                    case "Tipo_pedido":
                        column.HeaderText = "Tipo";
                        column.Visible = true;
                        column.Width = 136;
                        column.DefaultCellStyle.Font = Theme.BodyBold;
                        break;
                    case "Total_parcial": case "Propina": case "Subtotal": case "Total_final":
                    case "Desechables": case "Domicilio":
                        column.DefaultCellStyle.Format = "C0";
                        column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                        column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
                        column.Width = 140;
                        if (name == "Total_parcial") column.HeaderText = "Total parcial";
                        if (name == "Total_final")
                        {
                            column.HeaderText = "Total final";
                            column.DefaultCellStyle.Font = Theme.BodyBold;
                            column.DefaultCellStyle.ForeColor = Theme.Darken(Theme.Success, 0.25f);
                            column.DefaultCellStyle.BackColor = Theme.Lighten(Theme.Success, 0.96f);
                        }
                        break;
                    case "Descuento": column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                        column.HeaderCell.ToolTipText = "Descuento registrado en porcentaje o en valor, según la venta."; break;
                }
            }
            int position = 0;
            foreach (string name in new[] { "Fecha_venta", "Hora_venta", "Nombre_cliente", "Num_mesa",
                "Tipo_pedido", "Total_final", "Nombre_empleado", "Total_parcial", "Propina", "Subtotal", "Descuento" })
            {
                if (this.dgvVentas.Columns.Contains(name))
                    this.dgvVentas.Columns[name].DisplayIndex = position++;
            }
        }

        private void FormatSalesCell(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.Value == null || e.Value == DBNull.Value || e.ColumnIndex < 0) return;
            string name = this.dgvVentas.Columns[e.ColumnIndex].DataPropertyName;
            if (name == "Tipo_pedido")
            {
                string type = Convert.ToString(e.Value).Trim().ToUpperInvariant();
                Color accent = type == "DOMICILIO" ? Theme.Info : Theme.Neutral;
                e.CellStyle.ForeColor = Theme.Darken(accent, 0.25f);
                e.CellStyle.BackColor = Theme.Lighten(accent, 0.94f);
            }
            else if ((name == "Total_parcial" || name == "Propina" || name == "Subtotal" || name == "Total_final"
                || name == "Desechables" || name == "Domicilio")
                && decimal.TryParse(Convert.ToString(e.Value), out decimal amount))
            {
                e.Value = amount.ToString("C0", SalesCulture);
                e.FormattingApplied = true;
            }
            else if (name == "Fecha_venta" && DateTime.TryParse(Convert.ToString(e.Value), out DateTime date))
            {
                e.Value = date.ToString("dd MMM yyyy", SalesCulture);
                e.FormattingApplied = true;
            }
            else if (name == "Hora_venta")
            {
                if (TimeSpan.TryParse(Convert.ToString(e.Value), out TimeSpan time)
                    && time >= TimeSpan.Zero && time < TimeSpan.FromDays(1))
                {
                    e.Value = DateTime.Today.Add(time).ToString("hh:mm tt", CultureInfo.InvariantCulture);
                    e.FormattingApplied = true;
                }
                else if (DateTime.TryParse(Convert.ToString(e.Value), out DateTime hour))
                {
                    e.Value = hour.ToString("hh:mm tt", CultureInfo.InvariantCulture);
                    e.FormattingApplied = true;
                }
            }
        }

        private void UpdateSalesSummary()
        {
            int count = 0;
            decimal total = 0;
            foreach (DataGridViewRow row in this.dgvVentas.Rows)
            {
                if (row.IsNewRow) continue;
                count++;
                if (this.dgvVentas.Columns.Contains("Total_final")
                    && decimal.TryParse(Convert.ToString(row.Cells["Total_final"].Value), out decimal amount))
                    total += amount;
            }
            salesCount.Text = "VENTAS EN LA CONSULTA\n" + count.ToString("N0", SalesCulture);
            salesTotal.Text = "TOTAL VENDIDO\n" + total.ToString("C0", SalesCulture);
            salesAverage.Text = "PROMEDIO POR VENTA\n" + (count == 0 ? 0 : total / count).ToString("C0", SalesCulture);
            resultsHint.Text = this.dgvVentas.DataSource == null ? "Selecciona el período y pulsa Buscar."
                : count == 0 ? "No hay ventas para los filtros seleccionados."
                : count.ToString("N0", SalesCulture) + " ventas · Doble clic para ver la factura · Importes en COP";
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
                    FrmSaleDetails venta = new FrmSaleDetails();
                    venta.Id_venta = Convert.ToInt32(row.Cells["Id_venta"].Value);
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
                    this.StyleSalesColumns();

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
            this.StyleSalesGrid();
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
