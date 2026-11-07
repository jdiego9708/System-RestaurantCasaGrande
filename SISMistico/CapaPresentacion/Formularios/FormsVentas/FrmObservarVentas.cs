using CapaNegocio;
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
        PoperContainer containerResumen;
        ResumenVenta resumenVenta;
        private readonly SalesDashboard dashboard = new SalesDashboard();
        private readonly SalesColumnFilters textFilters;
        private readonly Label resultsHint = new Label();
        private static readonly CultureInfo SalesCulture = CultureInfo.GetCultureInfo("es-CO");

        public FrmObservarVentas()
        {
            InitializeComponent();
            this.BuildModernLayout();
            this.textFilters = new SalesColumnFilters(this.dgvVentas, UpdateSalesSummary);
            this.Load += FrmObservarVentas_Load;
            this.date1.ValueChanged += Date1_ValueChanged;
            this.btnBuscar.Click += BtnBuscar_Click;
            this.btnResumen.Click += BtnResumen_Click;
            this.dgvVentas.MouseDoubleClick += DgvVentas_DoubleClick;
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
            this.MinimumSize = new Size(820, 740);
            this.ClientSize = new Size(1320, 880);
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

            ModernCard filters = new ModernCard
            {
                Dock = DockStyle.Top, Height = 78,
                Padding = new Padding(12, 8, 12, 8),
            };
            Panel toolbarViewport = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            TableLayoutPanel toolbar = new TableLayoutPanel
            {
                Dock = DockStyle.Top, Height = 60, ColumnCount = 5, RowCount = 1,
                MinimumSize = new Size(1180, 60), Margin = Padding.Empty,
            };
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 126));
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 174));
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 194));
            toolbar.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            int dateColumn = 0;
            foreach (var entry in new[] { Tuple.Create("Fecha de inicio", this.date1), Tuple.Create("Fecha de fin", this.date2) })
            {
                Control field = CreateDateField(entry.Item1, entry.Item2);
                field.Dock = DockStyle.Fill;
                toolbar.Controls.Add(field, dateColumn++, 0);
            }
            ConfigureToolbarButton(this.btnBuscar, "Buscar", ButtonKind.Primary, FontAwesome.Sharp.IconChar.Search);
            ConfigureToolbarButton(this.btnOtrasOpciones, "Otras opciones", ButtonKind.Outline, FontAwesome.Sharp.IconChar.ChevronDown);
            ModernButton invoices = new ModernButton();
            ConfigureToolbarButton(invoices, "Facturas Alegra", ButtonKind.Success, FontAwesome.Sharp.IconChar.FileInvoice);
            invoices.Click += (s, e) => { var window = new FrmAlegraInvoices(); window.Show(this); };
            toolbar.Controls.Add(this.btnBuscar, 2, 0);
            toolbar.Controls.Add(this.btnOtrasOpciones, 3, 0);
            toolbar.Controls.Add(invoices, 4, 0);
            toolbarViewport.Controls.Add(toolbar);
            filters.Controls.Add(toolbarViewport);
            filters.SizeChanged += (s, e) =>
            {
                // Reservar espacio para el desplazamiento horizontal solo en ventanas estrechas.
                int height = 78 + (filters.ClientSize.Width - filters.Padding.Horizontal < toolbar.MinimumSize.Width
                    ? SystemInformation.HorizontalScrollBarHeight : 0);
                if (filters.Height != height) filters.Height = height;
            };

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
            root.Controls.Add(this.dashboard);
            root.Controls.Add(filters);
            root.Controls.Add(header);
            this.Controls.Add(root);
            // Los controles heredados de hora permanecen disponibles para las consultas existentes.
            this.groupBox1.Visible = false;
            this.btnResumen.Visible = false;
            this.UpdateSalesSummary();
            this.ResumeLayout(true);
        }

        private static void ConfigureToolbarButton(ModernButton button, string text, ButtonKind kind, FontAwesome.Sharp.IconChar icon)
        {
            button.Text = text;
            button.Image = null;
            button.Kind = kind;
            button.Icon = icon;
            button.Shadow = false;
            button.CornerRadius = 10;
            button.Dock = DockStyle.Fill;
            button.Margin = new Padding(0, 20, 6, 0);
            button.Font = Theme.BodyBold;
            button.AccessibleName = text;
        }

        private static Control CreateDateField(string captionText, DateTimePicker picker)
        {
            Panel field = new Panel { Width = 330, Height = 60, Margin = new Padding(0, 0, 12, 0) };
            Label caption = new Label { Text = captionText, Dock = DockStyle.Top, Height = 20 };
            caption.StyleLabel(LabelKind.Caption);
            picker.AccessibleName = captionText;
            picker.Visible = false;
            picker.TabStop = false;
            field.Controls.Add(picker);
            ModernDateField input = new ModernDateField(picker)
            {
                Dock = DockStyle.Bottom, Height = 40,
                AccessibleName = captionText,
            };
            field.Controls.Add(input);
            field.Controls.Add(caption);
            return field;
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
            this.dgvVentas.ColumnHeadersDefaultCellStyle.BackColor = Theme.SurfaceAlt;
            this.dgvVentas.ColumnHeadersDefaultCellStyle.SelectionBackColor = Theme.SurfaceAlt;
            this.dgvVentas.ColumnHeadersDefaultCellStyle.ForeColor = Theme.TextPrimary;
            this.dgvVentas.ColumnHeadersDefaultCellStyle.SelectionForeColor = Theme.TextPrimary;
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
                    case "Factura_electronica":
                        column.HeaderText = "Factura electr\u00f3nica"; column.Width = 165; column.Visible = true;
                        column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter; break;
                    case "Numero_factura_electronica":
                        column.HeaderText = "N\u00famero"; column.Width = 135; column.Visible = true; break;
                    case "Estado_factura_electronica":
                        column.HeaderText = "Estado electr\u00f3nico"; column.Width = 195; column.Visible = true;
                        column.DefaultCellStyle.Font = Theme.BodyBold; break;
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
                        column.HeaderCell.ToolTipText = "De 0 a 100: porcentaje. Mayor que 100: pesos colombianos."; break;
                }
            }
            this.textFilters?.ConfigureColumns();
            int position = 0;
            foreach (string name in new[] { "Fecha_venta", "Hora_venta", "Nombre_cliente", "Num_mesa",
                "Tipo_pedido", "Total_final", "Nombre_empleado", "Total_parcial", "Propina", "Subtotal", "Descuento" })
            {
                if (this.dgvVentas.Columns.Contains(name))
                    this.dgvVentas.Columns[name].DisplayIndex = position++;
            }
            foreach (string name in new[] { "Factura_electronica", "Numero_factura_electronica", "Estado_factura_electronica" })
            {
                if (this.dgvVentas.Columns.Contains(name))
                    this.dgvVentas.Columns[name].DisplayIndex = this.dgvVentas.Columns.Count - 1;
            }
        }

        private void FormatSalesCell(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex < 0 || e.RowIndex < 0) return;
            string electronicInvoice = Convert.ToString(SalesCell(this.dgvVentas.Rows[e.RowIndex], "Factura_electronica")).Trim();
            bool hasElectronicInvoice = electronicInvoice.Equals("SI", StringComparison.OrdinalIgnoreCase)
                || electronicInvoice.Equals("SÍ", StringComparison.OrdinalIgnoreCase)
                || electronicInvoice.Equals("True", StringComparison.OrdinalIgnoreCase)
                || electronicInvoice == "1";
            bool hasDiscount = this.dgvVentas.Columns.Contains("Descuento")
                && decimal.TryParse(Convert.ToString(this.dgvVentas.Rows[e.RowIndex].Cells["Descuento"].Value),
                    out decimal rowDiscount) && rowDiscount != 0;
            if (e.Value == null || e.Value == DBNull.Value)
            {
                if (hasElectronicInvoice) ApplyElectronicInvoiceStyle(e.CellStyle);
                else if (hasDiscount) ApplyDiscountStyle(e.CellStyle);
                return;
            }
            string name = this.dgvVentas.Columns[e.ColumnIndex].DataPropertyName;
            if (name == "Tipo_pedido")
            {
                string type = Convert.ToString(e.Value).Trim().ToUpperInvariant();
                Color accent = type == "DOMICILIO" ? Theme.Info : Theme.Neutral;
                e.CellStyle.ForeColor = Theme.Darken(accent, 0.25f);
                e.CellStyle.BackColor = Theme.Lighten(accent, 0.94f);
            }
            else if (name == "Descuento" && decimal.TryParse(Convert.ToString(e.Value), out decimal discount))
            {
                e.Value = discount >= 0 && discount <= 100
                    ? discount.ToString("0.############################", SalesCulture) + "%"
                    : "$" + discount.ToString("N0", SalesCulture);
                e.FormattingApplied = true;
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
            // Se aplica al final para cubrir también los fondos de Tipo y Total final.
            if (hasElectronicInvoice) ApplyElectronicInvoiceStyle(e.CellStyle);
            else if (hasDiscount) ApplyDiscountStyle(e.CellStyle);
        }

        private static void ApplyElectronicInvoiceStyle(DataGridViewCellStyle style)
        {
            style.BackColor = Theme.Lighten(Theme.Success, 0.87f);
            style.SelectionBackColor = Theme.Lighten(Theme.Success, 0.73f);
            style.ForeColor = Theme.TextPrimary;
            style.SelectionForeColor = Theme.TextPrimary;
        }

        private static void ApplyDiscountStyle(DataGridViewCellStyle style)
        {
            style.BackColor = Theme.Lighten(Theme.Danger, 0.85f);
            style.SelectionBackColor = Theme.Lighten(Theme.Danger, 0.72f);
            style.ForeColor = Theme.TextPrimary;
            style.SelectionForeColor = Theme.TextPrimary;
        }

        private void UpdateSalesSummary()
        {
            SalesMetrics metrics = new SalesMetrics();
            foreach (DataGridViewRow row in this.dgvVentas.Rows)
            {
                if (row.IsNewRow || !row.Visible) continue;
                string hourText = Convert.ToString(SalesCell(row, "Hora_venta"));
                int hour = -1;
                if (TimeSpan.TryParse(hourText, out TimeSpan time) && time >= TimeSpan.Zero && time < TimeSpan.FromDays(1))
                    hour = time.Hours;
                else if (DateTime.TryParse(hourText, out DateTime date)) hour = date.Hour;
                metrics.Add(SalesAmount(row, "Total_final"), SalesAmount(row, "Subtotal"),
                    SalesAmount(row, "Descuento"), SalesAmount(row, "Propina"),
                    Convert.ToString(SalesCell(row, "Tipo_pedido")), hour);
            }
            this.dashboard.UpdateMetrics(metrics);
            string filterHint = this.textFilters != null && this.textFilters.ActiveCount > 0
                ? " | " + this.textFilters.ActiveCount + " filtros de columna activos" : "";
            resultsHint.Text = this.dgvVentas.DataSource == null ? "Selecciona el per\u00edodo y pulsa Buscar."
                : metrics.Count == 0 ? "No hay ventas para los filtros seleccionados." + filterHint
                : metrics.Count.ToString("N0", SalesCulture) + " ventas en la consulta \u00b7 Indicadores sobre estas filas \u00b7 Importes en COP \u00b7 Doble clic para ver factura" + filterHint;
        }

        private object SalesCell(DataGridViewRow row, string column)
        {
            return this.dgvVentas.Columns.Contains(column) ? row.Cells[column].Value : null;
        }

        private decimal SalesAmount(DataGridViewRow row, string column)
        {
            object value = SalesCell(row, column);
            return value == null || value == DBNull.Value ? 0 : Convert.ToDecimal(value, SalesCulture);
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
            if (this.btnOtrasOpciones.ContextMenuStrip != null)
            {
                this.btnOtrasOpciones.ContextMenuStrip.Show(this.btnOtrasOpciones,
                    new Point(5, this.btnOtrasOpciones.Height));
                return;
            }
            ContextMenuStrip menu = new ContextMenuStrip
            {
                Font = Theme.Body, BackColor = Theme.Surface, ForeColor = Theme.TextPrimary,
                ShowImageMargin = false, Padding = new Padding(6),
            };
            ToolStripMenuItem deleteAccounts = new ToolStripMenuItem("Eliminar cuentas")
            {
                Checked = this.gbOpciones.Visible, Padding = new Padding(12, 8, 12, 8),
            };
            deleteAccounts.Click += (s, args) =>
            {
                this.gbOpciones.Visible = !this.gbOpciones.Visible;
                if (!this.gbOpciones.Visible) this.chkEliminarCuentas.Checked = false;
            };
            ToolStripMenuItem alegra = new ToolStripMenuItem("Configurar Alegra")
            {
                Padding = new Padding(12, 8, 12, 8),
            };
            alegra.Click += (s, args) =>
            {
                try { using (var setup = new FrmAlegraSetup()) setup.ShowDialog(this); }
                catch (Exception)
                {
                    Mensajes.MensajeInformacion("No se pudo abrir la configuraci\u00f3n Alegra. Aplique la migraci\u00f3n 07 de base de datos.");
                }
            };
            menu.Items.Add(deleteAccounts);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(alegra);
            if (this.components == null) this.components = new System.ComponentModel.Container();
            this.components.Add(menu);
            this.btnOtrasOpciones.ContextMenuStrip = menu;
            menu.Opening += (s, args) => deleteAccounts.Checked = this.gbOpciones.Visible;
            menu.Show(this.btnOtrasOpciones, new Point(5, this.btnOtrasOpciones.Height));
        }

        private void DgvVentas_DoubleClick(object sender, MouseEventArgs e)
        {
            try
            {
                if (this.dgvVentas.HitTest(e.X, e.Y).RowIndex < 0) return;
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
            this.BuscarVentas("VENTA FECHAS", "");
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

            this.dgvVentas =
                ConfiguracionDatagridview.ConfigurationGrid(this.dgvVentas);
            this.StyleSalesGrid();
            this.Horas();
        }

    }
}
