using CapaNegocio;
using CapaEntidades.Models;
using CapaPresentacion.Formularios.FormsIngresos;
using CapaPresentacion.Formularios.FormsEgresos;
using CapaPresentacion.Formularios.FormsEstadisticas;
using CapaPresentacion.ReportesFacturas.IngresosTurno;
using CapaPresentacion.ReportesFacturas.GastosTurno;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using CapaPresentacion.UI;
using System.Windows.Forms;

namespace CapaPresentacion.Formularios.FormsVentas
{
    public partial class FrmObservarVentas : Form
    {
        PoperContainer containerResumen;
        ResumenVenta resumenVenta;
        private FrmAlegraInvoices alegraWindow;
        private const int PageSize = 100;
        private DataTable allResults;
        private int currentPage;
        private bool bindingPage;
        private string sortedColumn;
        private bool sortDescending;
        private readonly Button firstPageButton = new Button();
        private readonly Button previousPageButton = new Button();
        private readonly Button nextPageButton = new Button();
        private readonly Button lastPageButton = new Button();
        private readonly Label pageHint = new Label();
        private readonly TableLayoutPanel resultsToolbar = new TableLayoutPanel();
        private int ResultCount => allResults == null ? 0 : allResults.DefaultView.Count;
        private int PageCount => Math.Max(1, (ResultCount + PageSize - 1) / PageSize);
        private const string RowNumberColumn = "ResultRowNumber";
        private readonly Dictionary<int, int> rowNumbers = new Dictionary<int, int>();
        private const string PrintReceiptColumn = "PrintCashReceipt";
        private int hoveredPrintRow = -1;
        private bool printingCashReceipt;
        private bool deleteAccountsAuthorized;
        private bool showingIncome;
        private bool showingExpenses;
        private DateTime cashQueryStart;
        private DateTime cashQueryEnd;
        // Ventas por Rappi (leidas de las facturas PDF). Se comporta como los ingresos y egresos:
        // sin doble clic de detalle ni eliminacion de cuentas.
        private bool showingRappi;
        private bool showingDeliveries;
        private DateTime rappiQueryStart;
        private DateTime rappiQueryEnd;
        private bool ShowingCash => showingIncome || showingExpenses || showingRappi;
        private readonly SalesDashboard dashboard = new SalesDashboard();
        private readonly SalesColumnFilters textFilters;
        private readonly Label resultsHint = new Label();
        private readonly ModernButton newMovementButton = new ModernButton();
        private readonly ModernButton reportButton = new ModernButton();
        private readonly ModernButton baseButton = new ModernButton();
        private ToolStripDropDown basePopup;
        private Label currentBaseLabel;
        private Label baseDayLabel;
        private Label baseStatusLabel;
        private ModernTextBox newBaseInput;
        private ModernButton saveBaseButton;
        private Turno baseShift;
        private bool baseBusy;
        private readonly SalesEmptyState emptySales = new SalesEmptyState();
        private static readonly CultureInfo SalesCulture = CultureInfo.GetCultureInfo("es-CO");

        public FrmObservarVentas()
        {
            InitializeComponent();
            this.BuildModernLayout();
            this.textFilters = new SalesColumnFilters(this.dgvVentas, ApplyResultFilters, () => allResults, ApplyResultSort);
            this.dashboard.IncomeRequested += (s, e) =>
            {
                if (this.btnBuscar.Enabled) BuscarMovimientos(false);
            };
            this.dashboard.ExpensesRequested += (s, e) =>
            {
                if (this.btnBuscar.Enabled) BuscarMovimientos(true);
            };
            this.dashboard.RappiRequested += (s, e) =>
            {
                if (this.btnBuscar.Enabled) BuscarRappi();
            };
            this.dashboard.DeliveriesRequested += (s, e) =>
            {
                if (!this.btnBuscar.Enabled) return;
                showingIncome = showingExpenses = showingRappi = false;
                showingDeliveries = true;
                dashboard.SelectDeliveries();
                BuscarVentas("VENTA FECHAS", "");
            };
            this.dashboard.SalesRequested += (s, e) =>
            {
                if (!this.btnBuscar.Enabled) return;
                showingIncome = false;
                showingExpenses = false;
                showingRappi = false;
                showingDeliveries = false;
                dashboard.SelectCash(false, false);
                BuscarVentas("VENTA FECHAS", "");
            };
            this.Load += FrmObservarVentas_Load;
            this.date1.ValueChanged += Date1_ValueChanged;
            this.btnBuscar.Click += BtnBuscar_Click;
            this.btnResumen.Click += BtnResumen_Click;
            this.dgvVentas.MouseDoubleClick += DgvVentas_DoubleClick;
            this.btnOtrasOpciones.Click += BtnOtrasOpciones_Click;
            this.chkEliminarCuentas.Click += ChkEliminarCuentas_Click;
            this.dgvVentas.CellContentClick += DgvVentas_CellContentClick;
            this.btnEliminarCuentas.Click += BtnEliminarCuentas_Click;
            this.dgvVentas.DataBindingComplete += (s, e) =>
            {
                if (!bindingPage) { StyleSalesColumns(); UpdateSalesSummary(); }
            };
            this.dgvVentas.ColumnHeaderMouseClick += SortResultColumn;
            this.dgvVentas.CellFormatting += FormatSalesCell;
            this.dgvVentas.CellPainting += PaintCashPrintButton;
            this.dgvVentas.CellMouseEnter += (s, e) =>
            {
                bool printCell = ShowingCash && e.RowIndex >= 0 && e.ColumnIndex >= 0
                    && this.dgvVentas.Columns[e.ColumnIndex].Name == PrintReceiptColumn;
                SetPrintHover(printCell ? e.RowIndex : -1);
            };
            this.dgvVentas.CellMouseLeave += (s, e) => SetPrintHover(-1);
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
            // La barra de filtros mide mas que la ventana en pantallas pequenas: se desplaza con el dedo.
            Panel toolbarViewport = new TouchScrollPanel { Dock = DockStyle.Fill, AutoScroll = true, TouchPanning = true };
            TableLayoutPanel toolbar = new TableLayoutPanel
            {
                Dock = DockStyle.Top, Height = 60, ColumnCount = 8, RowCount = 1,
                MinimumSize = new Size(1320, 60), Margin = Padding.Empty,
            };
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 126));
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 146));
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 154));
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 174));
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
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
            ConfigureToolbarButton(this.dashboard.ChartsButton, "Gr\u00e1ficas", ButtonKind.Outline, FontAwesome.Sharp.IconChar.ChevronDown);
            ConfigureToolbarButton(reportButton, "Ver reporte", ButtonKind.Outline, FontAwesome.Sharp.IconChar.FileInvoice);
            reportButton.Click += OpenDailyReport;
            ConfigureToolbarButton(baseButton, "Cambiar base", ButtonKind.Outline, FontAwesome.Sharp.IconChar.ChevronDown);
            baseButton.Click += OpenBasePopup;
            ModernButton invoices = new ModernButton();
            ConfigureToolbarButton(invoices, "Facturas Alegra", ButtonKind.Success, FontAwesome.Sharp.IconChar.FileInvoice);
            // Ventana independiente (sin propietario ni modal): se reutiliza si ya está abierta.
            invoices.Click += (s, e) =>
            {
                if (alegraWindow == null || alegraWindow.IsDisposed) alegraWindow = new FrmAlegraInvoices();
                if (!alegraWindow.Visible) alegraWindow.Show();
                if (alegraWindow.WindowState == FormWindowState.Minimized) alegraWindow.WindowState = FormWindowState.Normal;
                alegraWindow.Activate();
            };
            toolbar.Controls.Add(this.btnBuscar, 2, 0);
            toolbar.Controls.Add(reportButton, 3, 0);
            toolbar.Controls.Add(baseButton, 4, 0);
            toolbar.Controls.Add(this.btnOtrasOpciones, 5, 0);
            toolbar.Controls.Add(this.dashboard.ChartsButton, 6, 0);
            toolbar.Controls.Add(invoices, 7, 0);
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
            resultsToolbar.Dock = DockStyle.Top;
            resultsToolbar.Height = 28;
            resultsToolbar.ColumnCount = 2;
            resultsToolbar.RowCount = 1;
            resultsToolbar.Margin = Padding.Empty;
            resultsToolbar.Padding = new Padding(0, 2, 0, 4);
            resultsToolbar.BackColor = Color.Transparent;
            resultsToolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            resultsToolbar.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            resultsToolbar.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            resultsHint.Dock = DockStyle.Fill;
            resultsHint.TextAlign = ContentAlignment.MiddleLeft;
            resultsHint.AutoEllipsis = true;
            resultsHint.StyleLabel(LabelKind.Caption);
            newMovementButton.Kind = ButtonKind.Primary;
            newMovementButton.Icon = FontAwesome.Sharp.IconChar.Plus;
            newMovementButton.Shadow = false;
            newMovementButton.CornerRadius = 10;
            newMovementButton.Dock = DockStyle.Fill;
            newMovementButton.Width = 174;
            newMovementButton.MinimumSize = new Size(174, 32);
            newMovementButton.Margin = new Padding(12, 0, 0, 0);
            newMovementButton.Font = Theme.BodyBold;
            newMovementButton.Visible = false;
            newMovementButton.Click += NewMovement_Click;
            resultsToolbar.Controls.Add(resultsHint, 0, 0);
            resultsToolbar.Controls.Add(newMovementButton, 1, 0);
            this.dgvVentas.Parent = table;
            this.dgvVentas.Dock = DockStyle.Fill;
            table.Controls.Add(resultsToolbar);
            table.Controls.Add(BuildPager());
            this.StyleSalesGrid();
            this.dgvVentas.Controls.Add(emptySales);
            this.dgvVentas.Layout += (s, e) => PositionEmptySales();
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
            if (showingRappi) { StyleRappiColumns(); return; }
            if (ShowingCash) { StyleCashColumns(); return; }
            foreach (DataGridViewColumn column in this.dgvVentas.Columns)
            {
                column.MinimumWidth = 80;
                column.Width = 124;
                column.DefaultCellStyle.FormatProvider = SalesCulture;
                string name = column.DataPropertyName.Length > 0 ? column.DataPropertyName : column.Name;
                if (name == "Id_venta" || name == "Id_pedido" || name == "Bono_cupon" || name == "Observaciones"
                    || name == "Id_cliente" || name == "Id_mesa" || name == "Id_empleado"
                    || name == "DeliveryNumber" || name == "ReceiptNumber")
                    column.Visible = false;
                if (column.DataPropertyName.Length > 0)
                    column.SortMode = DataGridViewColumnSortMode.Programmatic;
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
                        column.Visible = false;
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
            EnsureRowNumberColumn();
        }

        private void FormatSalesCell(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0
                && this.dgvVentas.Columns[e.ColumnIndex].Name == RowNumberColumn)
            {
                DataGridViewRow row = this.dgvVentas.Rows[e.RowIndex];
                if (showingRappi || !ShowingCash)
                {
                    e.Value = AssignedNumberLabel(row);
                }
                else
                    e.Value = rowNumbers.TryGetValue(e.RowIndex, out int number) ? number.ToString(SalesCulture) : "";
                e.FormattingApplied = true;
                return;
            }
            if (ShowingCash) return;
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

        private IEnumerable<DataRowView> FilteredResults()
        {
            return allResults == null ? Enumerable.Empty<DataRowView>() : allResults.DefaultView.Cast<DataRowView>();
        }

        private static object ResultCell(DataRowView row, string name)
        {
            return row.DataView.Table.Columns.Contains(name) ? row[name] : null;
        }

        private static decimal ResultAmount(DataRowView row, string name)
        {
            object value = ResultCell(row, name);
            return value == null || value == DBNull.Value ? 0 : Convert.ToDecimal(value, SalesCulture);
        }

        private Control BuildPager()
        {
            var footer = new Panel
            {
                Dock = DockStyle.Bottom, Height = 48,
                Padding = new Padding(0, 3, 0, 0), BackColor = Color.Transparent,
            };
            var pager = new FlowLayoutPanel
            {
                Dock = DockStyle.Right, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight, WrapContents = false,
                Margin = Padding.Empty, Padding = Padding.Empty, BackColor = Color.Transparent,
            };
            pageHint.AutoSize = true;
            pageHint.MinimumSize = new Size(0, 40);
            pageHint.TextAlign = ContentAlignment.MiddleRight;
            pageHint.Font = Theme.Small;
            pageHint.ForeColor = Theme.TextMuted;
            pageHint.Margin = new Padding(0, 0, 10, 0);
            pager.Controls.Add(pageHint);
            if (this.components == null) this.components = new System.ComponentModel.Container();
            var hints = new ToolTip(this.components);
            Button[] buttons = { firstPageButton, previousPageButton, nextPageButton, lastPageButton };
            string[] symbols = { "\u00ab", "\u2039", "\u203a", "\u00bb" };
            string[] labels = { "Primera", "Anterior", "Siguiente", "\u00daltima" };
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].Text = symbols[i];
                buttons[i].AccessibleName = labels[i] + " p\u00e1gina";
                hints.SetToolTip(buttons[i], buttons[i].AccessibleName);
                buttons[i].Size = new Size(44, 40); // tamano tactil
                buttons[i].Margin = new Padding(2, 0, 2, 0);
                buttons[i].Padding = Padding.Empty;
                buttons[i].FlatStyle = FlatStyle.Flat;
                buttons[i].FlatAppearance.BorderSize = 0;
                buttons[i].FlatAppearance.MouseOverBackColor = Theme.SurfaceAlt;
                buttons[i].FlatAppearance.MouseDownBackColor = Theme.Border;
                buttons[i].BackColor = Theme.Surface;
                buttons[i].ForeColor = Theme.TextMuted;
                buttons[i].Font = Theme.Heading;
                buttons[i].Cursor = Cursors.Hand;
                buttons[i].UseVisualStyleBackColor = false;
                pager.Controls.Add(buttons[i]);
            }
            firstPageButton.Click += (s, e) => ShowPage(0);
            previousPageButton.Click += (s, e) => ShowPage(currentPage - 1);
            nextPageButton.Click += (s, e) => ShowPage(currentPage + 1);
            lastPageButton.Click += (s, e) => ShowPage(PageCount - 1);
            footer.Controls.Add(pager);
            UpdatePager();
            return footer;
        }

        private void UpdatePager()
        {
            int total = ResultCount;
            int from = total == 0 ? 0 : currentPage * PageSize + 1;
            int to = Math.Min(total, (currentPage + 1) * PageSize);
            pageHint.Text = from.ToString("N0", SalesCulture) + "\u2013" + to.ToString("N0", SalesCulture)
                + " de " + total.ToString("N0", SalesCulture) + "  \u00b7  " + (currentPage + 1) + " / " + PageCount;
            pageHint.AccessibleName = "P\u00e1gina " + (currentPage + 1) + " de " + PageCount
                + ". Registros " + from + " a " + to + " de " + total + ".";
            bool ready = this.btnBuscar.Enabled && !printingCashReceipt;
            firstPageButton.Enabled = previousPageButton.Enabled = ready && currentPage > 0;
            nextPageButton.Enabled = lastPageButton.Enabled = ready && currentPage + 1 < PageCount;
        }

        private void BindResults(DataTable records)
        {
            allResults = records;
            sortedColumn = null;
            sortDescending = false;
            currentPage = 0;
            bindingPage = true;
            try
            {
                this.dgvVentas.DataSource = null;
                this.dgvVentas.Columns.Clear();
                this.chkEliminarCuentas.Checked = false;
                this.btnEliminarCuentas.Visible = false;
            }
            finally { bindingPage = false; }
            ShowPage(0, true);
        }

        private void ApplyResultFilters()
        {
            ShowPage(0, true);
        }

        private void ShowPage(int page, bool refreshSummary = false)
        {
            currentPage = Math.Max(0, Math.Min(page, PageCount - 1));
            bindingPage = true;
            this.dgvVentas.SuspendLayout();
            try
            {
                DataTable slice = allResults == null ? null : allResults.Clone();
                if (slice != null)
                {
                    int start = currentPage * PageSize;
                    for (int i = start; i < Math.Min(start + PageSize, ResultCount); i++)
                        slice.ImportRow(allResults.DefaultView[i].Row);
                }
                // Las selecciones de eliminacion pertenecen solo a la pagina visible.
                if (this.dgvVentas.Columns.Contains("chkEliminar"))
                    foreach (DataGridViewRow row in this.dgvVentas.Rows) row.Cells["chkEliminar"].Value = false;
                this.dgvVentas.DataSource = slice;
                SetPrintHover(-1);
                StyleSalesColumns();
                RestoreSortGlyph();
            }
            finally
            {
                bindingPage = false;
                this.dgvVentas.ResumeLayout();
            }
            if (refreshSummary) UpdateSalesSummary();
            else UpdateRowNumbers();
            UpdatePager();
        }

        private void SortResultColumn(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex < 0 || e.ColumnIndex >= this.dgvVentas.Columns.Count || e.Button != MouseButtons.Left
                || allResults == null || bindingPage || !this.btnBuscar.Enabled) return;
            DataGridViewColumn column = this.dgvVentas.Columns[e.ColumnIndex];
            string name = column.DataPropertyName;
            if (!allResults.Columns.Contains(name) || column.SortMode == DataGridViewColumnSortMode.NotSortable
                || this.textFilters.IsFilterClick(column, e.X)) return;
            ApplyResultSort(name, sortedColumn != name || !sortDescending);
        }

        private void ApplyResultSort(string name, bool descending)
        {
            if (allResults == null || !allResults.Columns.Contains(name) || bindingPage || !this.btnBuscar.Enabled) return;
            allResults.DefaultView.Sort = SalesColumnFilterState.ColumnName(name) + (descending ? " DESC" : " ASC");
            sortedColumn = name;
            sortDescending = descending;
            ShowPage(0);
        }

        private void RestoreSortGlyph()
        {
            // ShowPage recrea las columnas: actualizar solo las que pertenecen al grid actual.
            foreach (DataGridViewColumn column in this.dgvVentas.Columns)
                column.HeaderCell.SortGlyphDirection = column.SortMode == DataGridViewColumnSortMode.Programmatic
                    && column.DataPropertyName == sortedColumn
                    ? (sortDescending ? SortOrder.Descending : SortOrder.Ascending) : SortOrder.None;
        }

        private void EnsureRowNumberColumn()
        {
            if (!this.dgvVentas.Columns.Contains(RowNumberColumn))
            {
                this.dgvVentas.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = RowNumberColumn, HeaderText = "N\u00b0", ReadOnly = true,
                    SortMode = DataGridViewColumnSortMode.NotSortable,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                });
            }
            DataGridViewColumn column = this.dgvVentas.Columns[RowNumberColumn];
            column.Visible = true;
            column.DisplayIndex = 0;
            column.MinimumWidth = 52;
            int numberWidth = 52;
            foreach (DataGridViewRow row in this.dgvVentas.Rows)
            {
                string label = showingRappi || !ShowingCash
                    ? AssignedNumberLabel(row)
                    : ResultCount.ToString(SalesCulture);
                numberWidth = Math.Max(numberWidth, TextRenderer.MeasureText(label, Theme.Small).Width + 24);
            }
            column.Width = numberWidth;
            column.HeaderCell.ToolTipText = showingRappi || !ShowingCash
                ? "Numero asignado al pedido e impreso en su factura"
                : "Consecutivo de todos los resultados de la consulta";
            column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            column.HeaderCell.Style.Padding = Padding.Empty;
            column.DefaultCellStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                Padding = Padding.Empty, ForeColor = Theme.TextMuted, Font = Theme.Small,
            };
        }

        private string AssignedNumberLabel(DataGridViewRow row)
        {
            string field = showingRappi ? "DayNumber" : showingDeliveries ? "DeliveryNumber" : "ReceiptNumber";
            object number = SalesCell(row, field);
            return number == null || number == DBNull.Value ? "—"
                : (showingRappi ? "R - " : "N - ") + Convert.ToString(number, SalesCulture);
        }

        private void UpdateRowNumbers()
        {
            EnsureRowNumberColumn();
            rowNumbers.Clear();
            int number = currentPage * PageSize;
            foreach (DataGridViewRow row in this.dgvVentas.Rows)
                if (!row.IsNewRow && row.Visible) rowNumbers[row.Index] = ++number;
            this.dgvVentas.InvalidateColumn(this.dgvVentas.Columns[RowNumberColumn].Index);
        }

        private void UpdateSalesSummary()
        {
            UpdateMovementAction();
            if (showingRappi) { UpdateRappiSummary(); return; }
            if (ShowingCash) { UpdateCashSummary(); return; }
            UpdateRowNumbers();
            SalesMetrics metrics = new SalesMetrics();
            foreach (DataRowView row in FilteredResults())
            {
                string hourText = Convert.ToString(ResultCell(row, "Hora_venta"));
                int hour = -1;
                if (TimeSpan.TryParse(hourText, out TimeSpan time) && time >= TimeSpan.Zero && time < TimeSpan.FromDays(1))
                    hour = time.Hours;
                else if (DateTime.TryParse(hourText, out DateTime date)) hour = date.Hour;
                metrics.Add(ResultAmount(row, "Total_final"), ResultAmount(row, "Subtotal"),
                    ResultAmount(row, "Descuento"), ResultAmount(row, "Propina"),
                    Convert.ToString(ResultCell(row, "Tipo_pedido")), hour);
            }
            if (showingDeliveries) this.dashboard.UpdateDeliveries(metrics.Count, metrics.Total);
            else this.dashboard.UpdateMetrics(metrics);
            emptySales.RecordName = showingDeliveries ? "ventas por domicilio" : "ventas";
            emptySales.HasQuery = this.dgvVentas.DataSource != null;
            emptySales.Visible = metrics.Count == 0;
            PositionEmptySales();
            emptySales.Invalidate();
            string filterHint = this.textFilters != null && this.textFilters.ActiveCount > 0
                ? " | " + this.textFilters.ActiveCount + " filtros de columna activos" : "";
            resultsHint.Text = this.dgvVentas.DataSource == null ? "Selecciona el per\u00edodo y pulsa Buscar."
                : metrics.Count == 0 ? "No hay ventas para los filtros seleccionados." + filterHint
                : metrics.Count.ToString("N0", SalesCulture) + (showingDeliveries ? " ventas por domicilio" : " ventas")
                    + " en la consulta \u00b7 Importes en COP \u00b7 Doble clic para ver factura" + filterHint;
        }

        private void PositionEmptySales()
        {
            // Mantener accesibles los encabezados y los filtros de columna.
            Rectangle bounds = this.dgvVentas.ClientRectangle;
            int top = this.dgvVentas.ColumnHeadersVisible && this.dgvVentas.Columns.Count > 0
                ? this.dgvVentas.ColumnHeadersHeight + 1 : 1;
            int right = 1;
            int bottom = 1;
            foreach (Control control in this.dgvVentas.Controls)
            {
                if (!control.Visible) continue;
                if (control is VScrollBar) right += control.Width;
                if (control is HScrollBar) bottom += control.Height;
            }
            Rectangle body = new Rectangle(1, top, Math.Max(0, bounds.Width - right - 1),
                Math.Max(0, bounds.Height - top - bottom));
            if (emptySales.Bounds != body) emptySales.Bounds = body;
            if (emptySales.Visible) emptySales.BringToFront();
        }

        private sealed class SalesEmptyState : Control
        {
            public bool HasQuery { get; set; }
            public string RecordName { get; set; } = "ventas";

            public SalesEmptyState()
            {
                SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                    | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
                BackColor = Theme.Surface;
                TabStop = false;
                AccessibleRole = AccessibleRole.StaticText;
                AccessibleName = "No se encontraron resultados. Verifica las fechas y los filtros de columna.";
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                if (Width < 80 || Height < 80) return;
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                int contentWidth = Math.Min(460, Math.Max(1, Width - 40));
                int x = (Width - contentWidth) / 2;
                int y = Math.Max(8, (Height - 202) / 2);
                Rectangle circle = new Rectangle((Width - 76) / 2, y, 76, 76);
                using (SolidBrush background = new SolidBrush(Theme.Lighten(Theme.Primary, 0.94f)))
                    e.Graphics.FillEllipse(background, circle);
                using (Pen border = new Pen(Theme.Lighten(Theme.Primary, 0.85f)))
                    e.Graphics.DrawEllipse(border, circle);
                IconRenderer.Draw(this, e.Graphics, FontAwesome.Sharp.IconChar.Search,
                    Theme.Primary, new Rectangle(circle.X + 22, circle.Y + 22, 32, 32));
                TextRenderer.DrawText(e.Graphics, HasQuery ? "No se encontraron resultados" : "Consulta tus ventas",
                    Theme.Heading, new Rectangle(x, y + 94, contentWidth, 28), Theme.TextPrimary,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
                string message = HasQuery
                    ? "Verifica el rango de fechas y los filtros de columna.\nPrueba con otros criterios para encontrar " + "tus " + RecordName + "."
                    : "Selecciona el rango de fechas y pulsa Buscar\npara ver las ventas de ese per\u00edodo.";
                TextRenderer.DrawText(e.Graphics, message, Theme.Body,
                    new Rectangle(x, y + 132, contentWidth, 64), Theme.TextMuted,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.Top | TextFormatFlags.WordBreak);
            }
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

        private bool AuthorizeAccountDeletion()
        {
            using (var authorization = new AccountDeletionAuthorization())
                return authorization.ShowDialog(this) == DialogResult.OK;
        }

        private sealed class AccountDeletionAuthorization : Form
        {
            public AccountDeletionAuthorization()
            {
                Text = "Autorizar eliminaci\u00f3n de cuentas";
                ClientSize = new Size(460, 280);
                FormBorderStyle = FormBorderStyle.FixedDialog;
                StartPosition = FormStartPosition.CenterParent;
                MaximizeBox = false;
                MinimizeBox = false;
                ShowInTaskbar = false;
                BackColor = Theme.Background;
                Font = Theme.Body;
                var title = new Label
                {
                    Text = "Autorizaci\u00f3n requerida", Font = Theme.Heading,
                    Location = new Point(24, 24), Size = new Size(412, 28),
                };
                var description = new Label
                {
                    Text = "Introduce la clave maestra configurada para\neliminar cuentas del historial de ventas.",
                    ForeColor = Theme.TextMuted, Location = new Point(24, 60), Size = new Size(412, 48),
                };
                var code = new TextBox
                {
                    UseSystemPasswordChar = true, MaxLength = 8, Font = Theme.Heading,
                    Location = new Point(24, 120), Size = new Size(412, 30),
                    AccessibleName = "Clave maestra de ocho d\u00edgitos",
                };
                code.KeyPress += (sender, args) =>
                {
                    if (!char.IsControl(args.KeyChar) && (args.KeyChar < '0' || args.KeyChar > '9')) args.Handled = true;
                };
                var error = new Label
                {
                    ForeColor = Theme.Danger, Location = new Point(24, 157), Size = new Size(412, 42),
                };
                var cancel = new ModernButton
                {
                    Text = "Cancelar", Kind = ButtonKind.Outline, DialogResult = DialogResult.Cancel,
                    Location = new Point(24, 216), Size = new Size(150, 40), Shadow = false,
                };
                var accept = new ModernButton
                {
                    Text = "Autorizar", Kind = ButtonKind.Primary,
                    Location = new Point(286, 216), Size = new Size(150, 40), Shadow = false,
                };
                accept.Click += (sender, args) =>
                {
                    if (!Api.ApiConfig.CodigoMaestroConfigurado)
                        error.Text = "La clave maestra no est\u00e1 configurada en este equipo.";
                    else if (!Api.ApiConfig.CodigoMaestroValido(code.Text))
                        error.Text = "La clave maestra no es correcta. Int\u00e9ntalo de nuevo.";
                    else
                    {
                        code.Clear();
                        DialogResult = DialogResult.OK;
                        Close();
                        return;
                    }
                    code.Clear();
                    code.Focus();
                };
                Controls.AddRange(new Control[] { title, description, code, error, cancel, accept });
                AcceptButton = accept;
                CancelButton = cancel;
                Shown += (sender, args) => code.Focus();
            }
        }

        private void BtnEliminarCuentas_Click(object sender, EventArgs e)
        {
            if (ShowingCash) return;
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
                        // Cada autorizacion permite una sola operacion de eliminacion.
                        if (!deleteAccountsAuthorized && !AuthorizeAccountDeletion()) return;
                        deleteAccountsAuthorized = false;
                        string rpta = "";
                        foreach (DataGridViewRow row2 in Rows)
                        {
                            int id_venta = Convert.ToInt32(row2.Cells["Id_venta"].Value);
                            rpta = NVentas.InactivarVenta(id_venta);
                            if (!rpta.Equals("OK")) break;
                            if (allResults != null)
                            {
                                var original = allResults.Rows.Cast<DataRow>().FirstOrDefault(
                                    item => Convert.ToInt32(item["Id_venta"]) == id_venta);
                                if (original != null) allResults.Rows.Remove(original);
                            }
                        }
                        ShowPage(currentPage, true);

                        if (rpta.Equals("OK"))
                        {
                            Mensajes.MensajeOkForm("Se eliminaron las ventas correctamente");
                            this.chkEliminarCuentas.Checked = false;
                            this.btnEliminarCuentas.Visible = false;
                            if (this.dgvVentas.Columns.Contains("chkEliminar")) this.dgvVentas.Columns.Remove("chkEliminar");

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
            if (ShowingCash)
            {
                if (e.RowIndex >= 0 && e.ColumnIndex >= 0
                    && this.dgvVentas.Columns[e.ColumnIndex].Name == PrintReceiptColumn)
                {
                    if (showingRappi) PrintRappiTicket(this.dgvVentas.Rows[e.RowIndex]);
                    else PrintCashReceipt(this.dgvVentas.Rows[e.RowIndex]);
                }
                return;
            }
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
            if (ShowingCash) return;
            if (this.dgvVentas.DataSource != null)
            {
                CheckBox chk = (CheckBox)sender;
                if (chk.Checked)
                {
                    DataGridViewCheckBoxColumn chkColumn = new DataGridViewCheckBoxColumn();
                    chkColumn.Name = "chkEliminar";
                    chkColumn.HeaderText = "Eliminar";
                    this.dgvVentas.Columns.Add(chkColumn);
                    chkColumn.DisplayIndex = this.dgvVentas.Columns.Contains(RowNumberColumn) ? 1 : 0;
                    EnsureRowNumberColumn();
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
                if (ShowingCash) return;
                if (!this.gbOpciones.Visible)
                {
                    if (!AuthorizeAccountDeletion()) return;
                    deleteAccountsAuthorized = true;
                    this.gbOpciones.Visible = true;
                }
                else
                {
                    deleteAccountsAuthorized = false;
                    this.gbOpciones.Visible = false;
                    this.chkEliminarCuentas.Checked = false;
                    this.btnEliminarCuentas.Visible = false;
                    if (this.dgvVentas.Columns.Contains("chkEliminar"))
                        this.dgvVentas.Columns.Remove("chkEliminar");
                }
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
            menu.Opening += (s, args) =>
            {
                deleteAccounts.Checked = this.gbOpciones.Visible;
                deleteAccounts.Enabled = !ShowingCash;
            };
            menu.Show(this.btnOtrasOpciones, new Point(5, this.btnOtrasOpciones.Height));
        }

        private void DgvVentas_DoubleClick(object sender, MouseEventArgs e)
        {
            if (ShowingCash) return;
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

        private void UpdateMovementAction()
        {
            UpdatePager();
            resultsToolbar.Height = ShowingCash ? 40 : 28;
            newMovementButton.Visible = showingIncome || showingExpenses;
            newMovementButton.Text = showingExpenses ? "Nuevo egreso" : "Nuevo ingreso";
            newMovementButton.AccessibleName = newMovementButton.Text;
            newMovementButton.Enabled = this.btnBuscar.Enabled && !printingCashReceipt;
        }

        private void NewMovement_Click(object sender, EventArgs e)
        {
            if (!ShowingCash || !this.btnBuscar.Enabled || printingCashReceipt) return;
            bool expenses = showingExpenses;
            bool saved = false;
            newMovementButton.Enabled = false;
            try
            {
                if (expenses)
                {
                    using (var editor = new FrmNuevoEgreso { StartPosition = FormStartPosition.CenterParent })
                    {
                        editor.OnEgresoSuccess += (s, args) => saved = true;
                        editor.ShowDialog(this);
                    }
                }
                else
                {
                    using (var editor = new FrmNuevoIngreso { StartPosition = FormStartPosition.CenterParent })
                    {
                        editor.OnIngresoSuccess += (s, args) => saved = true;
                        editor.ShowDialog(this);
                    }
                }
                // Mantener el rango elegido al refrescar tras el registro y su impresion habitual.
                if (saved && !this.IsDisposed) BuscarMovimientos(expenses);
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "NewMovement_Click",
                    "No se pudo abrir el formulario del movimiento", ex.Message);
            }
            finally
            {
                if (!this.IsDisposed) UpdateMovementAction();
            }
        }

        private async void OpenBasePopup(object sender, EventArgs e)
        {
            if (baseBusy) return;
            if (basePopup == null) BuildBasePopup();
            if (basePopup.Visible) { basePopup.Close(); return; }
            basePopup.Show(baseButton, new Point(0, baseButton.Height + 6));
            try { await LoadCurrentBase(); }
            catch (Exception ex) { ShowBaseError(ex.Message); }
        }

        private void BuildBasePopup()
        {
            var panel = new Panel { Size = new Size(354, 284), BackColor = Theme.Surface };
            var title = new Label
            {
                Text = "Base de caja", Font = Theme.Heading, ForeColor = Theme.TextPrimary,
                Location = new Point(18, 16), Size = new Size(318, 28),
            };
            baseDayLabel = new Label
            {
                Font = Theme.Small, ForeColor = Theme.TextMuted,
                Location = new Point(18, 48), Size = new Size(318, 24),
            };
            currentBaseLabel = new Label
            {
                Text = "Consultando base...", Font = Theme.BodyBold, ForeColor = Theme.TextPrimary,
                Location = new Point(18, 80), Size = new Size(318, 28),
            };
            var caption = new Label
            {
                Text = "Nueva base (COP)", Font = Theme.Small, ForeColor = Theme.TextMuted,
                Location = new Point(18, 120), Size = new Size(318, 20),
            };
            newBaseInput = new ModernTextBox
            {
                Icon = FontAwesome.Sharp.IconChar.DollarSign,
                PlaceholderText = "Ej. 150.000", MaxLength = 22,
                Location = new Point(18, 145), Size = new Size(318, 42),
                AccessibleName = "Nueva base en pesos colombianos",
                AccessibleDescription = "Valor en pesos enteros. Usa punto para separar los miles.",
            };
            newBaseInput.KeyPress += (s, e) =>
            {
                if (!char.IsControl(e.KeyChar) && (e.KeyChar < '0' || e.KeyChar > '9')
                    && e.KeyChar != '.') e.Handled = true;
            };
            newBaseInput.Leave += (s, e) =>
            {
                if (TryReadNewBase(out decimal amount)) newBaseInput.Text = amount.ToString("N0", SalesCulture);
            };
            newBaseInput.KeyDown += (s, e) =>
            {
                if (e.KeyCode != Keys.Enter) return;
                e.SuppressKeyPress = true;
                saveBaseButton.PerformClick();
            };
            baseStatusLabel = new Label
            {
                Font = Theme.Small, ForeColor = Theme.TextMuted,
                Location = new Point(18, 193), Size = new Size(318, 34),
            };
            saveBaseButton = new ModernButton
            {
                Text = "Guardar base", Kind = ButtonKind.Primary, Shadow = false, CornerRadius = 10,
                Location = new Point(18, 232), Size = new Size(318, 36), Enabled = false,
            };
            saveBaseButton.Click += SaveCurrentBase;
            panel.Controls.AddRange(new Control[] { title, baseDayLabel, currentBaseLabel, caption,
                newBaseInput, baseStatusLabel, saveBaseButton });
            var host = new ToolStripControlHost(panel)
            {
                AutoSize = false, Size = panel.Size, Margin = Padding.Empty, Padding = Padding.Empty,
            };
            basePopup = new ToolStripDropDown
            {
                Padding = Padding.Empty, BackColor = Theme.Surface, DropShadowEnabled = true,
            };
            basePopup.Items.Add(host);
            if (this.components == null) this.components = new System.ComponentModel.Container();
            this.components.Add(basePopup);
            basePopup.SizeChanged += (s, e) =>
            {
                using (var path = Theme.RoundedPath(basePopup.ClientRectangle, 12))
                {
                    Region previous = basePopup.Region;
                    basePopup.Region = new Region(path);
                    previous?.Dispose();
                }
            };
        }

        private async System.Threading.Tasks.Task LoadCurrentBase()
        {
            baseBusy = true;
            saveBaseButton.Enabled = false;
            newBaseInput.Enabled = false;
            baseShift = null;
            try
            {
                DateTime day = DateTime.Today;
                baseDayLabel.Text = "Hoy, " + day.ToString("dd MMM yyyy", SalesCulture);
                currentBaseLabel.Text = "Consultando base...";
                baseStatusLabel.Text = "";
                baseStatusLabel.ForeColor = Theme.TextMuted;
                var result = await NTurnos.BuscarTurnoDelDia(day);
                if (this.IsDisposed) return;
                if (result.rpta != "OK" || result.dtTurnos == null)
                    throw new InvalidOperationException("No se pudo consultar la base. " + result.rpta);
                if (result.dtTurnos.Rows.Count == 0)
                {
                    currentBaseLabel.Text = "No hay un turno registrado para hoy.";
                    baseStatusLabel.Text = "Inicia un turno para poder guardar su base.";
                    return;
                }
                baseShift = new Turno(result.dtTurnos.Rows[0]);
                currentBaseLabel.Text = "Base actual: " + baseShift.Valor_inicial.ToString("C0", SalesCulture);
                newBaseInput.Text = baseShift.Valor_inicial.ToString("N0", SalesCulture);
                newBaseInput.HasError = false;
                newBaseInput.Enabled = true;
                saveBaseButton.Enabled = true;
                baseButton.AccessibleDescription = currentBaseLabel.Text;
            }
            finally { baseBusy = false; }
        }

        private void ShowBaseError(string message)
        {
            if (this.IsDisposed) return;
            baseStatusLabel.ForeColor = Theme.Danger;
            baseStatusLabel.Text = message;
        }

        private bool TryReadNewBase(out decimal amount)
        {
            amount = 0;
            string text = newBaseInput.Text.Trim();
            // Validar tambien el pegado y evitar interpretar puntos decimales como miles.
            return System.Text.RegularExpressions.Regex.IsMatch(text,
                    @"^(?:[0-9]+|[0-9]{1,3}(?:\.[0-9]{3})+)$")
                && decimal.TryParse(text, NumberStyles.Number, SalesCulture, out amount)
                && amount >= 0 && amount <= 99999999999999999.99m;
        }

        private async void SaveCurrentBase(object sender, EventArgs e)
        {
            if (baseBusy || baseShift == null) return;
            if (!TryReadNewBase(out decimal amount))
            {
                newBaseInput.HasError = true;
                ShowBaseError("Ingresa un valor en pesos enteros. Ejemplo: 150.000.");
                newBaseInput.Focus();
                return;
            }
            if (baseShift.Fecha_turno.Date != DateTime.Today)
            {
                try { await LoadCurrentBase(); }
                catch (Exception ex) { ShowBaseError(ex.Message); }
                return;
            }
            baseBusy = true;
            saveBaseButton.Enabled = false;
            newBaseInput.Enabled = false;
            try
            {
                int shiftId = baseShift.Id_turno;
                string response = await NTurnos.InsertarBase(shiftId, amount);
                if (this.IsDisposed) return;
                if (response != "OK") throw new InvalidOperationException(response);
                // Una nueva lectura confirma la persistencia fuera de la transaccion de escritura.
                await LoadCurrentBase();
                if (this.IsDisposed) return;
                if (baseShift == null || baseShift.Id_turno != shiftId || baseShift.Valor_inicial != amount)
                    throw new InvalidOperationException("La base cambio durante la operacion. Revisa el valor actual.");
                var session = DatosInicioSesion.GetInstancia();
                if (session.Turno != null && session.Turno.Id_turno == shiftId)
                    session.Turno.Valor_inicial = amount;
                baseStatusLabel.ForeColor = Theme.Success;
                baseStatusLabel.Text = "Base guardada y confirmada en la base de datos.";
            }
            catch (Exception ex) { ShowBaseError(ex.Message); }
            finally
            {
                baseBusy = false;
                if (!this.IsDisposed)
                {
                    saveBaseButton.Enabled = baseShift != null;
                    newBaseInput.Enabled = baseShift != null;
                }
            }
        }

        private async void OpenDailyReport(object sender, EventArgs e)
        {
            if (!this.btnBuscar.Enabled || printingCashReceipt) return;
            reportButton.Enabled = false;
            FrmReporteDiario report = null;
            try
            {
                DateTime start = this.date1.Value.Date;
                DateTime end = this.date2.Value.Date;
                report = new FrmReporteDiario { WindowState = FormWindowState.Maximized };
                report.Show(this);
                await report.LoadPeriodo(start, end);
            }
            catch (Exception ex)
            {
                if (report != null && !report.IsDisposed) report.Close();
                MensajeEspera.CloseForm();
                Mensajes.MensajeErrorCompleto(this.Name, "OpenDailyReport",
                    "No se pudo abrir el reporte diario", ex.Message);
            }
            finally
            {
                if (!this.IsDisposed) reportButton.Enabled = true;
            }
        }

        private void BtnBuscar_Click(object sender, EventArgs e)
        {
            if (showingRappi) BuscarRappi();
            else if (ShowingCash) BuscarMovimientos(showingExpenses);
            else this.BuscarVentas("VENTA FECHAS", "");
        }

        private async void BuscarMovimientos(bool expenses)
        {
            deleteAccountsAuthorized = false;
            this.btnBuscar.Enabled = false;
            UpdateMovementAction();
            try
            {
                DateTime start = this.date1.Value.Date;
                DateTime end = this.date2.Value.Date;
                if (start > end)
                {
                    Mensajes.MensajeErrorForm("La fecha inicial no puede ser posterior a la fecha final.");
                    return;
                }
                this.dashboard.UpdateCashTotals(null, null);
                // Actualizar ambos KPIs con la misma captura del rango, incluso al cambiar de vista.
                var incomeTask = NIngresos.BuscarIngresos("RANGO FECHAS",
                    start.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), end.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                var expensesTask = NEgresos.BuscarEgresos(start, end);
                await System.Threading.Tasks.Task.WhenAll(incomeTask, expensesTask);
                if (this.IsDisposed) return;
                var income = await incomeTask;
                var expense = await expensesTask;
                this.dashboard.UpdateCashTotals(
                    SumCashRecords(income.rpta == "OK" ? income.dtIngresos : null, "Valor_ingreso"),
                    SumCashRecords(expense.rpta == "OK" ? expense.dtEgresos : null, "Valor_egreso"));
                DataTable records = expenses ? expense.dtEgresos : income.dtIngresos;
                string response = expenses ? expense.rpta : income.rpta;
                if (response != "OK" || records == null)
                {
                    Mensajes.MensajeErrorForm("No se pudieron consultar los " + (expenses ? "egresos. " : "ingresos. ") + response);
                    return;
                }
                if (income.rpta != "OK" || expense.rpta != "OK")
                    Mensajes.MensajeErrorForm("No se pudo actualizar uno de los totales del per\u00edodo. "
                        + (income.rpta != "OK" ? income.rpta : expense.rpta));
                cashQueryStart = start;
                cashQueryEnd = end;
                showingIncome = !expenses;
                showingExpenses = expenses;
                showingRappi = false;
                showingDeliveries = false;
                RefreshRappiKpi(start, end);
                RefreshProgramSalesKpis(start, end);
                dashboard.SelectCash(showingIncome, showingExpenses);
                this.gbOpciones.Visible = false;
                this.chkEliminarCuentas.Checked = false;
                this.btnEliminarCuentas.Visible = false;
                this.btnResumen.Enabled = false;
                BindResults(records);
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "BuscarMovimientos", "Hubo un error al buscar los " + (expenses ? "egresos" : "ingresos"), ex.Message);
            }
            finally
            {
                if (!this.IsDisposed)
                {
                    this.btnBuscar.Enabled = true;
                    UpdateMovementAction();
                }
            }
        }

        private void StyleCashColumns()
        {
            string suffix = showingExpenses ? "egreso" : "ingreso";
            string[] names = { "Fecha_" + suffix, "Descripcion_" + suffix, "Valor_" + suffix, "Nombre_empleado", "Estado_" + suffix };
            string[] captions = { "Fecha", "Descripci\u00f3n del " + suffix, "Valor del " + suffix, "Registrado por", "Estado" };
            foreach (DataGridViewColumn column in this.dgvVentas.Columns)
            {
                int index = Array.IndexOf(names, column.DataPropertyName);
                column.Visible = index >= 0;
                if (index < 0) continue;
                column.HeaderText = captions[index];
                column.DefaultCellStyle.FormatProvider = SalesCulture;
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                column.MinimumWidth = index == 1 ? 200 : 130;
                column.FillWeight = index == 1 ? 240 : 100;
                column.SortMode = DataGridViewColumnSortMode.Programmatic;
                if (index == 0) column.DefaultCellStyle.Format = "dd MMM yyyy";
                if (index == 2)
                {
                    column.DefaultCellStyle.Format = "C0";
                    column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
                    column.DefaultCellStyle.Font = Theme.BodyBold;
                    column.DefaultCellStyle.ForeColor = Theme.Darken(showingExpenses ? Theme.Danger : Theme.Success, 0.25f);
                }
            }
            for (int i = 0; i < names.Length; i++)
                if (this.dgvVentas.Columns.Contains(names[i])) this.dgvVentas.Columns[names[i]].DisplayIndex = i;
            EnsurePrintColumn("Reimprimir comprobante", "Reimprimir comprobante (1 copia)");
            EnsureRowNumberColumn();
            this.textFilters?.ConfigureColumns();
        }

        // Columna final con el icono de impresora (ingresos, egresos y ventas de Rappi la comparten).
        private void EnsurePrintColumn(string headerTip, string cellTip)
        {
            if (!this.dgvVentas.Columns.Contains(PrintReceiptColumn))
            {
                var print = new DataGridViewButtonColumn
                {
                    Name = PrintReceiptColumn, HeaderText = "", Text = "",
                    UseColumnTextForButtonValue = true, FlatStyle = FlatStyle.Flat,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                    Width = 48, MinimumWidth = 48, Resizable = DataGridViewTriState.False,
                    SortMode = DataGridViewColumnSortMode.NotSortable,
                    DefaultCellStyle = new DataGridViewCellStyle
                    {
                        Alignment = DataGridViewContentAlignment.MiddleCenter, Padding = Padding.Empty,
                    },
                };
                this.dgvVentas.Columns.Add(print);
            }
            DataGridViewColumn column = this.dgvVentas.Columns[PrintReceiptColumn];
            column.HeaderCell.ToolTipText = headerTip;
            column.CellTemplate.ToolTipText = cellTip;
            column.Visible = true;
            column.DisplayIndex = this.dgvVentas.Columns.Count - 1;
        }

        private void SetPrintHover(int rowIndex)
        {
            int previous = hoveredPrintRow;
            hoveredPrintRow = rowIndex;
            this.dgvVentas.Cursor = rowIndex >= 0 ? Cursors.Hand : Cursors.Default;
            if (!this.dgvVentas.Columns.Contains(PrintReceiptColumn)) return;
            int columnIndex = this.dgvVentas.Columns[PrintReceiptColumn].Index;
            if (previous >= 0 && previous < this.dgvVentas.Rows.Count)
                this.dgvVentas.InvalidateCell(columnIndex, previous);
            if (rowIndex >= 0 && rowIndex < this.dgvVentas.Rows.Count)
                this.dgvVentas.InvalidateCell(columnIndex, rowIndex);
        }

        private void PaintCashPrintButton(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (!ShowingCash || e.RowIndex < 0 || e.ColumnIndex < 0
                || this.dgvVentas.Columns[e.ColumnIndex].Name != PrintReceiptColumn) return;
            e.Paint(e.ClipBounds, DataGridViewPaintParts.Background | DataGridViewPaintParts.SelectionBackground);
            bool hovered = e.RowIndex == hoveredPrintRow && !printingCashReceipt;
            Rectangle button = new Rectangle(e.CellBounds.Left + (e.CellBounds.Width - 28) / 2,
                e.CellBounds.Top + (e.CellBounds.Height - 28) / 2, 28, 28);
            if (hovered)
            {
                var state = e.Graphics.Save();
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var fill = new SolidBrush(Theme.Lighten(Theme.Primary, 0.92f)))
                    e.Graphics.FillEllipse(fill, button);
                e.Graphics.Restore(state);
            }
            bool selected = (e.State & DataGridViewElementStates.Selected) != 0;
            Color color = hovered ? Theme.Primary : selected ? e.CellStyle.SelectionForeColor : Theme.TextMuted;
            IconRenderer.Draw(this.dgvVentas, e.Graphics, FontAwesome.Sharp.IconChar.Print, color,
                new Rectangle(button.X + 6, button.Y + 6, 16, 16));
            if ((e.PaintParts & DataGridViewPaintParts.Focus) != 0)
                e.Paint(e.ClipBounds, DataGridViewPaintParts.Focus);
            e.Handled = true;
        }

        private void PrintCashReceipt(DataGridViewRow row)
        {
            if (printingCashReceipt || !this.btnBuscar.Enabled || row.IsNewRow) return;
            printingCashReceipt = true;
            this.btnBuscar.Enabled = false;
            UpdateMovementAction();
            try
            {
                string kind = showingExpenses ? "egreso" : "ingreso";
                // Los datos pertenecen al registro original, no al empleado de la sesion actual.
                string employee = "Nombre de quien genera el " + kind + " "
                    + Convert.ToString(SalesCell(row, "Nombre_empleado")) + Environment.NewLine
                    + "Identificaci\u00f3n " + Convert.ToString(SalesCell(row, "Identificacion_empleado")) + Environment.NewLine;
                string concept = "Concepto/Observaciones del " + kind + ":" + Environment.NewLine
                    + Convert.ToString(SalesCell(row, "Descripcion_" + kind)) + Environment.NewLine;
                string value = Convert.ToDecimal(SalesCell(row, "Valor_" + kind)).ToString("C0", SalesCulture);
                // La fecha almacenada es de tipo date; no se conserva la hora original.
                string date = Convert.ToDateTime(SalesCell(row, "Fecha_" + kind)).ToString("D", SalesCulture);
                string note = "Reimpresi\u00f3n del comprobante #" + Convert.ToString(SalesCell(row, "Id_" + kind));
                if (showingExpenses)
                {
                    using (var receipt = new FrmReporteGastos
                    {
                        FechaHora = date, InformacionEmpleado = employee, InformacionGasto = concept,
                        Valor_gasto = value, Observaciones = note,
                    })
                    {
                        receipt.ObtenerReporte();
                        receipt.ImprimirFactura(1);
                    }
                }
                else
                {
                    using (var receipt = new FrmReporteIngresos
                    {
                        FechaHora = date, InformacionEmpleado = employee, InformacionIngreso = concept,
                        Valor_ingreso = value, Observaciones = note,
                    })
                    {
                        receipt.ObtenerReporte();
                        receipt.ImprimirFactura(1);
                    }
                }
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "PrintCashReceipt",
                    "No se pudo reimprimir el comprobante", ex.Message);
            }
            finally
            {
                printingCashReceipt = false;
                if (!this.IsDisposed)
                {
                    this.btnBuscar.Enabled = true;
                    UpdateMovementAction();
                    this.dgvVentas.Invalidate();
                }
            }
        }

        private void UpdateCashSummary()
        {
            UpdateMovementAction();
            UpdateRowNumbers();
            int count = 0;
            decimal total = 0;
            foreach (DataRowView row in FilteredResults())
            {
                count++;
                total += ResultAmount(row, showingExpenses ? "Valor_egreso" : "Valor_ingreso");
            }
            emptySales.HasQuery = this.dgvVentas.DataSource != null;
            emptySales.RecordName = showingExpenses ? "egresos" : "ingresos";
            emptySales.Visible = count == 0;
            PositionEmptySales();
            emptySales.Invalidate();
            string filters = this.textFilters != null && this.textFilters.ActiveCount > 0
                ? " | " + this.textFilters.ActiveCount + " filtros de columna activos" : "";
            string recordName = showingExpenses ? "egresos" : "ingresos";
            string period = " \u00b7 Del " + cashQueryStart.ToString("dd/MM/yyyy") + " al " + cashQueryEnd.ToString("dd/MM/yyyy");
            resultsHint.Text = count == 0 ? "No hay " + recordName + " para los filtros seleccionados." + period + filters
                : count.ToString("N0", SalesCulture) + " " + recordName + " \u00b7 Total: " + total.ToString("C0", SalesCulture) + period + " \u00b7 COP" + filters;
        }

        private async void BuscarVentas(string tipo_busqueda, string texto_busqueda)
        {
            deleteAccountsAuthorized = false;
            this.btnBuscar.Enabled = false;
            UpdateMovementAction();
            this.dashboard.UpdateCashTotals(null, null);
            try
            {
                DateTime start = this.date1.Value.Date;
                DateTime end = this.date2.Value.Date;
                string fecha1 = this.date1.Value.ToShortDateString();
                string fecha2 = this.date2.Value.ToShortDateString();
                string hora1 = this.ListaHora1.SelectedValue.ToString();
                string hora2 = this.ListaHora2.SelectedValue.ToString();

                DataTable tablaVenta =
                    NVentas.BuscarVenta(tipo_busqueda, texto_busqueda, fecha1, fecha2, hora1, hora2);
                UpdateProgramSalesKpis(tablaVenta);
                BindResults(SelectProgramSales(tablaVenta, showingDeliveries));
                RefreshRappiKpi(start, end);
                // Mismas fuentes y sumas que el Reporte diario; no incluyen ventas ni n\u00f3mina.
                string from = start.ToString("yyyy-MM-dd");
                string to = end.ToString("yyyy-MM-dd");
                var incomeTask = NIngresos.BuscarIngresos("RANGO FECHAS", from, to);
                var expensesTask = NEgresos.BuscarEgresos(start, end);
                await System.Threading.Tasks.Task.WhenAll(incomeTask, expensesTask);
                if (this.IsDisposed) return;
                var income = await incomeTask;
                var expenses = await expensesTask;
                this.dashboard.UpdateCashTotals(SumCashRecords(income.rpta == "OK" ? income.dtIngresos : null, "Valor_ingreso"),
                    SumCashRecords(expenses.rpta == "OK" ? expenses.dtEgresos : null, "Valor_egreso"));
                if (income.rpta != "OK" || expenses.rpta != "OK" || income.dtIngresos == null || expenses.dtEgresos == null)
                    Mensajes.MensajeErrorForm("No se pudieron consultar los ingresos o egresos del per\u00edodo. "
                        + income.rpta + " " + expenses.rpta);

                if (tablaVenta != null)
                {
                    this.btnResumen.Enabled = true;
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
            finally
            {
                if (!this.IsDisposed)
                {
                    this.btnBuscar.Enabled = true;
                    UpdateMovementAction();
                }
            }
        }

        private static DataTable SelectProgramSales(DataTable sales, bool deliveries)
        {
            if (sales == null) return null;
            DataTable selected = sales.Clone();
            foreach (DataRow row in sales.Rows)
            {
                bool isDelivery = sales.Columns.Contains("Tipo_pedido")
                    && "DOMICILIO".Equals(Convert.ToString(row["Tipo_pedido"]).Trim(), StringComparison.OrdinalIgnoreCase);
                if (isDelivery == deliveries) selected.ImportRow(row);
            }
            return selected;
        }

        private void UpdateProgramSalesKpis(DataTable sales)
        {
            DataTable normal = SelectProgramSales(sales, false);
            DataTable deliveries = SelectProgramSales(sales, true);
            SalesMetrics metrics = new SalesMetrics();
            if (normal != null)
                foreach (DataRowView row in normal.DefaultView)
                {
                    TimeSpan time;
                    int hour = TimeSpan.TryParse(Convert.ToString(ResultCell(row, "Hora_venta")), out time) ? time.Hours : -1;
                    metrics.Add(ResultAmount(row, "Total_final"), ResultAmount(row, "Subtotal"),
                        ResultAmount(row, "Descuento"), ResultAmount(row, "Propina"),
                        Convert.ToString(ResultCell(row, "Tipo_pedido")), hour);
                }
            dashboard.UpdateMetrics(metrics);
            dashboard.UpdateDeliveries(deliveries?.Rows.Count, deliveries == null ? (decimal?)null : SumCashRecords(deliveries, "Total_final"));
        }

        private void RefreshProgramSalesKpis(DateTime start, DateTime end)
        {
            UpdateProgramSalesKpis(NVentas.BuscarVenta("VENTA FECHAS", "",
                start.ToString("yyyy-MM-dd"), end.ToString("yyyy-MM-dd"), "00:00", "23:59"));
        }

        // El indicador de Rappi cuenta las facturas PDF en el mismo periodo de la consulta.
        private void RefreshRappiKpi(DateTime start, DateTime end)
        {
            try
            {
                DataTable orders = NRappiOrders.ListRange(start, end);
                this.dashboard.UpdateRappi(orders.Rows.Count, SumCashRecords(orders, "Total"),
                    SumCashRecords(orders, "CashAmount"), SumCashRecords(orders, "OtherPaymentAmount"));
            }
            catch (Exception)
            {
                // Sin la tabla de Rappi (script 10 sin aplicar) o sin conexion: el indicador queda sin datos y no estorba.
                this.dashboard.UpdateRappi(null, null);
            }
        }

        private void BuscarRappi()
        {
            deleteAccountsAuthorized = false;
            this.btnBuscar.Enabled = false;
            UpdateMovementAction();
            try
            {
                DateTime start = this.date1.Value.Date;
                DateTime end = this.date2.Value.Date;
                if (start > end)
                {
                    Mensajes.MensajeErrorForm("La fecha inicial no puede ser posterior a la fecha final.");
                    return;
                }
                DataTable records = NRappiOrders.ListRange(start, end);
                rappiQueryStart = start;
                rappiQueryEnd = end;
                showingIncome = false;
                showingExpenses = false;
                showingRappi = true;
                showingDeliveries = false;
                RefreshProgramSalesKpis(start, end);
                dashboard.SelectRappi();
                this.gbOpciones.Visible = false;
                this.chkEliminarCuentas.Checked = false;
                this.btnEliminarCuentas.Visible = false;
                this.btnResumen.Enabled = false;
                BindResults(records);
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "BuscarRappi",
                    "Hubo un error al consultar las ventas de Rappi. Revise que la base tenga la tabla RappiOrders (script 10).", ex.Message);
            }
            finally
            {
                if (!this.IsDisposed)
                {
                    this.btnBuscar.Enabled = true;
                    UpdateMovementAction();
                }
            }
        }

        // El valor del pedido se separa entre efectivo y otros medios; la forma de pago corresponde al saldo a cobrar.
        private void StyleRappiColumns()
        {
            string[] names = { "OrderDate", "OrderTime", "ExternalId", "CustomerName", "Phone", "PaymentMethod", "CashAmount", "OtherPaymentAmount", "Total" };
            string[] captions = { "Fecha", "Hora", "Orden de Rappi", "Cliente", "Tel\u00e9fono", "Forma de pago", "Efectivo", "Otros medios", "Valor del pedido" };
            foreach (DataGridViewColumn column in this.dgvVentas.Columns)
            {
                int index = Array.IndexOf(names, column.DataPropertyName);
                column.Visible = index >= 0;
                if (index < 0) continue;
                column.HeaderText = captions[index];
                column.DefaultCellStyle.FormatProvider = SalesCulture;
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                column.MinimumWidth = index == 3 ? 180 : 110;
                column.FillWeight = index == 3 ? 200 : 100;
                column.SortMode = DataGridViewColumnSortMode.Programmatic;
                if (index == 0) column.DefaultCellStyle.Format = "dd MMM yyyy";
                if (index == 1)
                {
                    // Igual que la hora del listado de ventas ("04:07 PM"), sin depender de la cultura de Windows.
                    column.DefaultCellStyle.Format = "hh:mm tt";
                    column.DefaultCellStyle.FormatProvider = CultureInfo.InvariantCulture;
                }
                if (index >= 6)
                {
                    column.DefaultCellStyle.Format = "C0";
                    column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
                    column.DefaultCellStyle.Font = Theme.BodyBold;
                    column.DefaultCellStyle.ForeColor = Theme.Darken(Theme.Rappi, 0.25f);
                    column.MinimumWidth = 135;
                    if (column.DataPropertyName == "CashAmount")
                        column.HeaderCell.ToolTipText = "TOTAL A COBRAR cuando la forma de pago es efectivo. No incluye lo ya pagado en Rappi.";
                    else if (column.DataPropertyName == "OtherPaymentAmount")
                        column.HeaderCell.ToolTipText = "Lo pagado a traves de Rappi mas el saldo a cobrar por datafono, transferencia u otro medio distinto de efectivo.";
                }
            }
            for (int i = 0; i < names.Length; i++)
                if (this.dgvVentas.Columns.Contains(names[i])) this.dgvVentas.Columns[names[i]].DisplayIndex = i;
            EnsurePrintColumn("Reimprimir la orden de Rappi", "Reimprimir la orden de Rappi (1 copia)");
            EnsureRowNumberColumn();
            this.textFilters?.ConfigureColumns();
        }

        // Reimprime la orden como ticket termico a partir de lo guardado de su factura PDF (el PDF original es de
        // tamano carta y en un rollo de 58 u 80 mm quedaria ilegible).
        private void PrintRappiTicket(DataGridViewRow row)
        {
            if (printingCashReceipt || !this.btnBuscar.Enabled || row.IsNewRow) return;
            printingCashReceipt = true;
            this.btnBuscar.Enabled = false;
            UpdateMovementAction();
            try
            {
                string externalId = Convert.ToString(SalesCell(row, "ExternalId"));
                RappiOrder order = NRappiOrders.GetByExternalId(externalId);
                if (order == null)
                {
                    Mensajes.MensajeInformacion("No se encontr\u00f3 la orden de Rappi " + externalId + " en la base.", "Entendido");
                    return;
                }
                MensajeEspera.ShowWait("Imprimiendo...");
                try { CapaPresentacion.Formularios.FormsDomicilios.RappiTicketPrinter.Print(order); }
                finally { MensajeEspera.CloseForm(); }
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "PrintRappiTicket",
                    "No se pudo reimprimir la orden de Rappi", ex.Message);
            }
            finally
            {
                printingCashReceipt = false;
                if (!this.IsDisposed)
                {
                    this.btnBuscar.Enabled = true;
                    UpdateMovementAction();
                    this.dgvVentas.Invalidate();
                }
            }
        }

        private void UpdateRappiSummary()
        {
            UpdateMovementAction();
            UpdateRowNumbers();
            int count = 0;
            decimal total = 0, cash = 0, other = 0;
            foreach (DataRowView row in FilteredResults())
            {
                count++;
                total += ResultAmount(row, "Total");
                cash += ResultAmount(row, "CashAmount");
                other += ResultAmount(row, "OtherPaymentAmount");
            }
            this.dashboard.UpdateRappi(count, total, cash, other);
            emptySales.HasQuery = this.dgvVentas.DataSource != null;
            emptySales.RecordName = "ventas de Rappi";
            emptySales.Visible = count == 0;
            PositionEmptySales();
            emptySales.Invalidate();
            string filters = this.textFilters != null && this.textFilters.ActiveCount > 0
                ? " | " + this.textFilters.ActiveCount + " filtros de columna activos" : "";
            string period = " \u00b7 Del " + rappiQueryStart.ToString("dd/MM/yyyy") + " al " + rappiQueryEnd.ToString("dd/MM/yyyy");
            resultsHint.Text = count == 0 ? "No hay ventas de Rappi para los filtros seleccionados." + period + filters
                : count.ToString("N0", SalesCulture) + (count == 1 ? " venta" : " ventas") + " de Rappi \u00b7 Total: "
                    + total.ToString("C0", SalesCulture) + period + " \u00b7 COP" + filters;
        }

        private static decimal? SumCashRecords(DataTable table, string column)
        {
            if (table == null) return null;
            decimal total = 0;
            foreach (DataRow row in table.Rows)
                if (!row.IsNull(column)) total += Convert.ToDecimal(row[column]);
            return total;
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
            this.date2.MinDate = date.Value.Date;
        }

        private void FrmObservarVentas_Load(object sender, EventArgs e)
        {
            this.resumenVenta = new ResumenVenta();
            this.containerResumen = new PoperContainer(resumenVenta);

            // El ayudante configura la misma tabla y la devuelve; no hace falta reasignarla (ahora es TouchDataGridView).
            ConfiguracionDatagridview.ConfigurationGrid(this.dgvVentas);
            this.StyleSalesGrid();
            this.Horas();
            this.date1.Value = DateTime.Today;
            this.date2.Value = DateTime.Today;
            this.BuscarVentas("VENTA FECHAS", "");
        }

    }
}
