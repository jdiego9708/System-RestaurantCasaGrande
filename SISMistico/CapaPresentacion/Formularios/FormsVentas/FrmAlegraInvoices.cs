using CapaNegocio.Alegra;
using CapaPresentacion.UI;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CapaPresentacion.Formularios.FormsVentas
{
    // Ventana independiente (no modal) con las facturas de la cuenta Alegra, de la más reciente a la más antigua.
    // Solo consulta, descarga e imprime documentos existentes: nunca crea ni emite facturas.
    public sealed class FrmAlegraInvoices : Form
    {
        private const int PageSize = 30;
        private const string StateColumn = "StatePin";
        private const string PdfColumn = "OpenPdf";
        private const string PrintColumn = "PrintInvoice";
        private static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("es-CO");

        private readonly DateTimePicker from = new DateTimePicker { Format = DateTimePickerFormat.Short };
        private readonly DateTimePicker to = new DateTimePicker { Format = DateTimePickerFormat.Short };
        private readonly DataGridView grid = new DataGridView();
        private readonly Label status = new Label { Dock = DockStyle.Bottom, Height = 30 };
        private readonly Label pageHint = new Label();
        private readonly ModernButton search = new ModernButton();
        private readonly Button firstButton = new Button();
        private readonly Button previousButton = new Button();
        private readonly Button nextButton = new Button();
        private readonly CancellationTokenSource lifetime = new CancellationTokenSource();
        // Páginas ya consultadas del período actual, por posición de inicio. Buscar las descarta.
        private readonly Dictionary<int, DataTable> pageCache = new Dictionary<int, DataTable>();
        private SalesColumnFilters columnFilters;
        private int offset;
        private int pageCount;
        private DateTime queryFrom, queryTo;
        private bool busy;
        private int hoverRow = -1, hoverColumn = -1;

        public FrmAlegraInvoices()
        {
            Text = "Facturas de Alegra";
            ClientSize = new Size(1280, 760);
            MinimumSize = new Size(900, 560);
            StartPosition = FormStartPosition.CenterScreen;
            ShowInTaskbar = true;
            this.ApplyTheme();
            from.Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            to.Value = DateTime.Today;
            from.ValueChanged += (s, e) => to.MinDate = from.Value.Date;

            Panel root = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 16, 20, 16) };
            Panel header = new Panel { Dock = DockStyle.Top, Height = 72 };
            Label title = new Label { Text = "Facturas de Alegra", AutoSize = true };
            title.StyleLabel(LabelKind.Title);
            Label subtitle = new Label
            {
                Text = "Cuenta completa, de la más reciente a la más antigua. Doble clic para ver el detalle.",
                AutoSize = true, Location = new Point(2, 38),
            };
            subtitle.StyleLabel(LabelKind.Muted);
            header.Controls.Add(title);
            header.Controls.Add(subtitle);

            ModernCard filters = new ModernCard { Dock = DockStyle.Top, Height = 78, Padding = new Padding(12, 8, 12, 8) };
            TableLayoutPanel toolbar = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, Margin = Padding.Empty };
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            toolbar.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Control fromField = CreateDateField("Fecha de inicio", from);
            Control toField = CreateDateField("Fecha de fin", to);
            fromField.Dock = toField.Dock = DockStyle.Fill;
            search.Text = "Buscar";
            search.Kind = ButtonKind.Primary;
            search.Icon = FontAwesome.Sharp.IconChar.Search;
            search.Shadow = false;
            search.CornerRadius = 10;
            search.Dock = DockStyle.Fill;
            search.Margin = new Padding(0, 20, 0, 0);
            search.Font = Theme.BodyBold;
            toolbar.Controls.Add(fromField, 0, 0);
            toolbar.Controls.Add(toField, 1, 0);
            toolbar.Controls.Add(search, 2, 0);
            filters.Controls.Add(toolbar);

            ModernCard card = new ModernCard { Dock = DockStyle.Fill };
            BuildGrid();
            // Filtros de cabecera en las columnas de texto (factura, cliente, identificación), igual que en Ventas.
            // El DataTable enlazado aplica el RowFilter solo; no hay nada más que refrescar.
            columnFilters = new SalesColumnFilters(grid, () => { });
            card.Controls.Add(grid);
            card.Controls.Add(BuildPager());
            status.StyleLabel(LabelKind.Caption);
            status.TextAlign = ContentAlignment.MiddleLeft;

            root.Controls.Add(card);
            card.BringToFront();
            root.Controls.Add(status);
            root.Controls.Add(filters);
            root.Controls.Add(header);
            Controls.Add(root);

            search.Click += async (s, e) => await Search();
            firstButton.Click += async (s, e) => await LoadPage(0);
            previousButton.Click += async (s, e) => await LoadPage(Math.Max(0, offset - PageSize));
            nextButton.Click += async (s, e) => await LoadPage(offset + PageSize);
            FormClosed += (s, e) => lifetime.Cancel();
            // Consulta inicial al abrir: el mes en curso, lo más reciente primero.
            Shown += async (s, e) => await Search();
            UpdateControls();
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
            field.Controls.Add(new ModernDateField(picker) { Dock = DockStyle.Bottom, Height = 40, AccessibleName = captionText });
            field.Controls.Add(caption);
            return field;
        }

        private void BuildGrid()
        {
            grid.Dock = DockStyle.Fill;
            grid.AutoGenerateColumns = false;
            grid.StyleGrid();
            grid.RowsDefaultCellStyle = new DataGridViewCellStyle();
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.ReadOnly = true;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false;
            grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            grid.RowTemplate.Height = 40;
            grid.DefaultCellStyle.Padding = new Padding(6, 4, 6, 4);
            grid.DefaultCellStyle.NullValue = "—";
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.ColumnHeadersHeight = 44;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Theme.SurfaceAlt;
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Theme.SurfaceAlt;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Theme.TextPrimary;
            grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = Theme.TextPrimary;
            grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(6, 0, 6, 0);
            grid.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            AddColumn("Id", "ID", 0, false);
            AddColumn("Status", "Status", 0, false);
            AddColumn("StateKind", "StateKind", 0, false);
            AddColumn("StateTip", "StateTip", 0, false);
            AddColumn("Number", "Factura", 85, true);
            AddColumn("Date", "Fecha", 170, true);
            AddColumn("Client", "Cliente", 150, true);
            AddColumn("Identification", "Identificación", 140, true);
            AddColumn("Subtotal", "Subtotal", 90, true, "C0");
            AddColumn("Tax", "Impuestos", 100, true, "C0");
            AddColumn("Total", "Total", 110, true, "C0");
            foreach (string money in new[] { "Subtotal", "Tax", "Total" })
            {
                grid.Columns[money].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                grid.Columns[money].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
            grid.Columns["Total"].HeaderText = "Total con impuestos";
            grid.Columns["Total"].DefaultCellStyle.Font = Theme.BodyBold;
            // Columnas de iconos: pin de estado, abrir PDF e imprimir.
            foreach (string name in new[] { StateColumn, PdfColumn, PrintColumn })
            {
                var icon = new DataGridViewTextBoxColumn
                {
                    Name = name, HeaderText = name == StateColumn ? "Estado" : name == PdfColumn ? "PDF" : "Imprimir", ReadOnly = true,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.None, Width = name == PrintColumn ? 84 : name == StateColumn ? 100 : 76,
                    // El pin se pinta encima; enlazar la columna al nombre del estado habilita el filtro de cabecera.
                    DataPropertyName = name == StateColumn ? "StateTip" : string.Empty,
                    SortMode = DataGridViewColumnSortMode.NotSortable,
                };
                icon.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                icon.HeaderCell.Style.Padding = Padding.Empty;
                icon.DefaultCellStyle.Padding = Padding.Empty;
                grid.Columns.Add(icon);
            }
            grid.Columns["Client"].FillWeight = 160;
            grid.Columns["Total"].MinimumWidth = 165;

            grid.CellPainting += PaintIconCell;
            grid.CellMouseMove += (s, e) => SetHover(e.RowIndex, IsIconColumn(e.ColumnIndex) ? e.ColumnIndex : -1);
            grid.CellMouseLeave += (s, e) => SetHover(-1, -1);
            grid.CellClick += async (s, e) =>
            {
                if (e.RowIndex < 0 || !IsIconColumn(e.ColumnIndex) || !CanUseRow(e.RowIndex)) return;
                await Document(grid.Columns[e.ColumnIndex].Name == PrintColumn, e.RowIndex);
            };
            grid.CellDoubleClick += async (s, e) => { if (e.RowIndex >= 0 && !IsIconColumn(e.ColumnIndex)) await Details(); };
            grid.DataBindingComplete += (s, e) => UpdateIconTooltips();
            grid.CellFormatting += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.Value is DateTime && grid.Columns[e.ColumnIndex].Name == "Date")
                { e.Value = FormatInvoiceDate((DateTime)e.Value); e.FormattingApplied = true; }
            };
        }

        // Formato "11 Nov. 2026 - 10:30 pm"; si Alegra no informó la hora se muestra solo la fecha.
        private static string FormatInvoiceDate(DateTime value)
        {
            string month = Culture.DateTimeFormat.AbbreviatedMonthNames[value.Month - 1];
            month = char.ToUpper(month[0], Culture) + month.Substring(1);
            string text = value.Day.ToString("00") + " " + month + " " + value.Year;
            if (value.TimeOfDay == TimeSpan.Zero) return text;
            return text + " - " + (value.Hour % 12 == 0 ? 12 : value.Hour % 12).ToString("00") + ":" + value.Minute.ToString("00")
                + (value.Hour < 12 ? " am" : " pm");
        }

        // Alegra entrega "datetime" con la hora de emisión; "date" solo trae el día.
        private static DateTime InvoiceMoment(Dictionary<string, object> invoice)
        {
            DateTime moment;
            if (DateTime.TryParse(AlegraWorkflow.Text(invoice, "datetime"), CultureInfo.InvariantCulture, DateTimeStyles.None, out moment)) return moment;
            DateTime.TryParse(AlegraWorkflow.Text(invoice, "date"), CultureInfo.InvariantCulture, DateTimeStyles.None, out moment);
            return moment;
        }

        private void AddColumn(string name, string header, int minimumWidth, bool visible, string format = null)
        {
            var column = new DataGridViewTextBoxColumn
            {
                Name = name, DataPropertyName = name, HeaderText = header, Visible = visible,
                SortMode = DataGridViewColumnSortMode.NotSortable, MinimumWidth = Math.Max(minimumWidth, 10),
            };
            if (format != null) { column.DefaultCellStyle.Format = format; column.DefaultCellStyle.FormatProvider = Culture; }
            grid.Columns.Add(column);
        }

        private bool IsIconColumn(int index)
        {
            if (index < 0) return false;
            string name = grid.Columns[index].Name;
            return name == PdfColumn || name == PrintColumn;
        }

        // Los borradores no tienen PDF ni representación para imprimir.
        private bool CanUseRow(int rowIndex)
        {
            if (busy || rowIndex < 0 || rowIndex >= grid.Rows.Count) return false;
            return Convert.ToString(grid.Rows[rowIndex].Cells["Status"].Value) != "draft";
        }

        private void SetHover(int row, int column)
        {
            if (row == hoverRow && column == hoverColumn) return;
            int oldRow = hoverRow, oldColumn = hoverColumn;
            hoverRow = row; hoverColumn = column;
            grid.Cursor = column >= 0 && CanUseRow(row) ? Cursors.Hand : Cursors.Default;
            if (oldRow >= 0 && oldColumn >= 0 && oldRow < grid.Rows.Count) grid.InvalidateCell(oldColumn, oldRow);
            if (row >= 0 && column >= 0 && row < grid.Rows.Count) grid.InvalidateCell(column, row);
        }

        private void UpdateIconTooltips()
        {
            foreach (DataGridViewRow row in grid.Rows)
            {
                row.Cells[StateColumn].ToolTipText = Convert.ToString(row.Cells["StateTip"].Value);
                bool draft = Convert.ToString(row.Cells["Status"].Value) == "draft";
                row.Cells[PdfColumn].ToolTipText = draft ? "Los borradores no tienen PDF." : "Abrir el PDF de Alegra";
                row.Cells[PrintColumn].ToolTipText = draft ? "Los borradores no se imprimen." : "Imprimir la factura de 80 mm";
            }
        }

        private void PaintIconCell(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && grid.Columns[e.ColumnIndex].Name == StateColumn)
            {
                PaintStatePin(e);
                return;
            }
            if (e.RowIndex < 0 || !IsIconColumn(e.ColumnIndex)) return;
            bool isPdf = grid.Columns[e.ColumnIndex].Name == PdfColumn;
            e.Paint(e.ClipBounds, DataGridViewPaintParts.Background | DataGridViewPaintParts.SelectionBackground | DataGridViewPaintParts.Border);
            bool enabled = CanUseRow(e.RowIndex);
            bool hovered = enabled && e.RowIndex == hoverRow && e.ColumnIndex == hoverColumn;
            Color accent = isPdf ? Theme.Danger : Theme.Primary;
            Rectangle button = new Rectangle(e.CellBounds.Left + (e.CellBounds.Width - 30) / 2,
                e.CellBounds.Top + (e.CellBounds.Height - 30) / 2, 30, 30);
            if (hovered)
            {
                var state = e.Graphics.Save();
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var fill = new SolidBrush(Theme.Lighten(accent, 0.9f))) e.Graphics.FillEllipse(fill, button);
                e.Graphics.Restore(state);
            }
            Color color = !enabled ? Theme.Border : hovered ? accent : isPdf ? Theme.Lighten(accent, 0.2f) : Theme.TextMuted;
            IconRenderer.Draw(grid, e.Graphics, isPdf ? FontAwesome.Sharp.IconChar.FilePdf : FontAwesome.Sharp.IconChar.Print,
                color, new Rectangle(button.X + 6, button.Y + 6, 18, 18));
            e.Handled = true;
        }

        // Pin de color: verde emitida, amarillo pendiente, rojo rechazada u otro estado. El nombre sale en el tooltip.
        private void PaintStatePin(DataGridViewCellPaintingEventArgs e)
        {
            e.Paint(e.ClipBounds, DataGridViewPaintParts.Background | DataGridViewPaintParts.SelectionBackground | DataGridViewPaintParts.Border);
            string kind = Convert.ToString(grid.Rows[e.RowIndex].Cells["StateKind"].Value);
            Color color = kind == "ok" ? Theme.Success : kind == "pending" ? Theme.Warning : Theme.Danger;
            int size = 14;
            Rectangle pin = new Rectangle(e.CellBounds.Left + (e.CellBounds.Width - size) / 2, e.CellBounds.Top + (e.CellBounds.Height - size) / 2, size, size);
            var state = e.Graphics.Save();
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using (var halo = new SolidBrush(Theme.Lighten(color, 0.8f))) e.Graphics.FillEllipse(halo, Rectangle.Inflate(pin, 4, 4));
            using (var fill = new SolidBrush(color)) e.Graphics.FillEllipse(fill, pin);
            e.Graphics.Restore(state);
            e.Handled = true;
        }

        private Control BuildPager()
        {
            var footer = new Panel { Dock = DockStyle.Bottom, Height = 34, Padding = new Padding(0, 3, 0, 0), BackColor = Color.Transparent };
            var pager = new FlowLayoutPanel
            {
                Dock = DockStyle.Right, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight, WrapContents = false,
                Margin = Padding.Empty, Padding = Padding.Empty, BackColor = Color.Transparent,
            };
            pageHint.AutoSize = true;
            pageHint.MinimumSize = new Size(0, 28);
            pageHint.TextAlign = ContentAlignment.MiddleRight;
            pageHint.Font = Theme.Small;
            pageHint.ForeColor = Theme.TextMuted;
            pageHint.Margin = new Padding(0, 0, 10, 0);
            pager.Controls.Add(pageHint);
            var hints = new ToolTip();
            Disposed += (s, e) => hints.Dispose();
            Button[] buttons = { firstButton, previousButton, nextButton };
            string[] symbols = { "«", "‹", "›" };
            string[] labels = { "Primera página", "Página anterior", "Página siguiente" };
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].Text = symbols[i];
                buttons[i].AccessibleName = labels[i];
                hints.SetToolTip(buttons[i], labels[i]);
                buttons[i].Size = new Size(28, 28);
                buttons[i].Margin = new Padding(1, 0, 1, 0);
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
            footer.Controls.Add(pager);
            return footer;
        }

        private void UpdateControls()
        {
            search.Enabled = !busy;
            from.Enabled = to.Enabled = !busy;
            firstButton.Enabled = previousButton.Enabled = !busy && offset > 0;
            // La API no informa el total: si la página vino llena, puede haber otra.
            nextButton.Enabled = !busy && pageCount == PageSize;
            int page = offset / PageSize + 1;
            pageHint.Text = pageCount == 0 ? "Sin resultados" : "Página " + page + "  ·  " + pageCount + " facturas";
            grid.Invalidate();
        }

        // Clasifica la factura para el pin: "ok" emitida, "pending" pendiente (se puede volver a enviar a emitir),
        // "error" rechazada, anulada u otro estado. Devuelve también el nombre que se ve en el tooltip.
        private static string ClassifyState(string commercial, string emission, out string name)
        {
            if (commercial == "void") { name = "Anulada"; return "error"; }
            switch (emission)
            {
                case "STAMPED_AND_ACCEPTED": case "ACCEPTED":
                    name = "Emitida y aceptada por la DIAN"; return "ok";
                case "STAMPED_AND_ACCEPTED_WITH_OBSERVATIONS":
                    name = "Emitida y aceptada por la DIAN con observaciones"; return "ok";
                case "PENDING":
                    name = "Pendiente de emitir"; return "pending";
                case "STAMPED_AND_WAITING_RESPONSE":
                    name = "Enviada a la DIAN, esperando respuesta"; return "pending";
                case "STAMPED_AND_REJECTED": case "REJECTED":
                    name = "Rechazada por la DIAN"; return "error";
            }
            if (string.IsNullOrWhiteSpace(emission))
            {
                // El listado de Alegra puede omitir el estado electrónico.
                name = commercial == "draft" ? "Borrador: pendiente de emitir" : "Pendiente: el listado no informa el estado electrónico";
                return "pending";
            }
            name = "Estado: " + emission;
            return "error";
        }

        private static string IdentificationText(Dictionary<string, object> client)
        {
            var identification = client != null && client.ContainsKey("identificationObject") ? client["identificationObject"] as Dictionary<string, object> : null;
            string number = AlegraWorkflow.Text(identification, "number");
            if (string.IsNullOrWhiteSpace(number)) number = AlegraWorkflow.Text(client, "identification");
            if (string.IsNullOrWhiteSpace(number)) return string.Empty;
            string type = AlegraWorkflow.Text(identification, "type");
            string digit = AlegraWorkflow.Text(identification, "dv");
            return (string.IsNullOrWhiteSpace(type) ? "" : type + " ") + number + (type == "NIT" && !string.IsNullOrWhiteSpace(digit) ? "-" + digit : "");
        }

        // Los importes de la API usan punto decimal; se leen sin depender de la cultura de Windows.
        private static decimal Amount(Dictionary<string, object> invoice, string key)
        {
            decimal.TryParse(AlegraWorkflow.Text(invoice, key), NumberStyles.Float, CultureInfo.InvariantCulture, out decimal value);
            return value;
        }

        // Muestra una página ya consultada sin volver a llamar a Alegra.
        private void ShowCachedPage(int start)
        {
            DataTable table = pageCache[start];
            // Al cambiar de página los filtros de columna se reinician; la tabla guardada no debe conservar el anterior.
            table.DefaultView.RowFilter = string.Empty;
            table.DefaultView.Sort = string.Empty;
            foreach (DataGridViewColumn column in grid.Columns) column.HeaderCell.SortGlyphDirection = SortOrder.None;
            grid.DataSource = table; offset = start; pageCount = table.Rows.Count;
            status.Text = pageCount == 0
                ? (start == 0 ? "No hay facturas en el período seleccionado." : "No hay más facturas en el período seleccionado.")
                : "Importes en COP · Pase el cursor sobre el pin de estado para ver su nombre: verde emitida, amarillo pendiente, rojo rechazada u otro.";
        }

        // Nueva búsqueda: descarta las páginas guardadas y consulta Alegra otra vez.
        private async Task Search()
        {
            if (busy) return;
            queryFrom = from.Value.Date; queryTo = to.Value.Date;
            pageCache.Clear();
            await LoadPage(0);
        }

        private async Task LoadPage(int start)
        {
            if (busy) return;
            if (pageCache.ContainsKey(start)) { ShowCachedPage(start); UpdateControls(); return; }
            busy = true; UpdateControls(); status.Text = "Consultando la cuenta de Alegra…";
            try
            {
                using (var api = new AlegraClient())
                {
                    var page = AlegraWorkflow.List(await api.GetInvoicesAsync(queryFrom, queryTo, start, lifetime.Token));
                    if (IsDisposed) return;
                    DataTable table = new DataTable();
                    foreach (string field in new[] { "Id", "Number", "Client", "Identification", "Status", "StateKind", "StateTip" }) table.Columns.Add(field);
                    table.Columns.Add("Date", typeof(DateTime));
                    foreach (string field in new[] { "Subtotal", "Tax", "Total" }) table.Columns.Add(field, typeof(decimal));
                    var rows = new List<Tuple<DateTime, long, object[]>>();
                    foreach (var invoice in page)
                    {
                        var number = invoice.ContainsKey("numberTemplate") ? invoice["numberTemplate"] as Dictionary<string, object> : null;
                        var client = invoice.ContainsKey("client") ? invoice["client"] as Dictionary<string, object> : null;
                        string id = AlegraWorkflow.Text(invoice, "id");
                        string state = AlegraWorkflow.Text(invoice, "status");
                        DateTime date = InvoiceMoment(invoice);
                        long.TryParse(id, out long numericId);
                        string kind = ClassifyState(state, AlegraWorkflow.Emission(invoice), out string stateName);
                        rows.Add(Tuple.Create(date, numericId, new object[]
                        {
                            id, AlegraWorkflow.Text(number, "fullNumber"), AlegraWorkflow.Text(client, "name"), IdentificationText(client),
                            state, kind, stateName, date == default(DateTime) ? (object)DBNull.Value : date,
                            Amount(invoice, "subtotal"), Amount(invoice, "tax"), Amount(invoice, "total"),
                        }));
                    }
                    // Más reciente primero; dentro de un mismo día, el ID mayor es la factura creada después.
                    foreach (var row in rows.OrderByDescending(r => r.Item1).ThenByDescending(r => r.Item2)) table.Rows.Add(row.Item3);
                    pageCache[start] = table;
                    ShowCachedPage(start);
                }
            }
            catch (OperationCanceledException) { if (!IsDisposed) status.Text = "Consulta cancelada o tiempo de espera agotado."; }
            catch (Exception ex) { if (!IsDisposed) status.Text = ex is InvalidOperationException || ex is ArgumentException ? ex.Message : "No se pudo consultar Alegra. Revise la conexión."; }
            finally { busy = false; if (!IsDisposed) UpdateControls(); }
        }

        private async Task Document(bool printing, int rowIndex)
        {
            if (busy || rowIndex < 0) return;
            string id = Convert.ToString(grid.Rows[rowIndex].Cells["Id"].Value);
            busy = true; UpdateControls();
            status.Text = printing ? "Preparando la factura de 80 mm para imprimir…" : "Consultando y descargando el PDF oficial…";
            try
            {
                if (printing)
                {
                    await AlegraPdfService.PrintRemoteAsync(id, lifetime.Token);
                    if (!IsDisposed) status.Text = "Factura enviada a la impresora. Compruebe el formato físico de 80 mm.";
                }
                else
                {
                    string file = await AlegraPdfService.DownloadRemoteAsync(id, lifetime.Token);
                    if (IsDisposed) return;
                    bool review = Path.GetFileName(file).EndsWith("-revision.pdf", StringComparison.OrdinalIgnoreCase);
                    Process.Start(new ProcessStartInfo(file) { UseShellExecute = true });
                    status.Text = review ? "Representación compacta para revisión abierta: Alegra no pudo proporcionar su PDF." : "PDF de Alegra abierto.";
                }
            }
            catch (OperationCanceledException) { if (!IsDisposed) status.Text = "Operación cancelada o tiempo de espera agotado."; }
            catch (Exception ex) { if (!IsDisposed) status.Text = ex is InvalidOperationException || ex is ArgumentException ? ex.Message : "No se pudo descargar o imprimir la factura de Alegra."; }
            finally { busy = false; if (!IsDisposed) UpdateControls(); }
        }

        private async Task Details()
        {
            if (busy || grid.CurrentRow == null) return;
            string id = Convert.ToString(grid.CurrentRow.Cells["Id"].Value);
            busy = true; UpdateControls(); status.Text = "Consultando detalle de factura…";
            var loading = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Background };
            var indicator = new ModernCard { Size = new Size(380, 140), AccentColor = Theme.Primary };
            indicator.Controls.Add(new Label { Text = "Cargando detalle de factura…", Dock = DockStyle.Top, Height = 55, TextAlign = ContentAlignment.MiddleCenter, Font = Theme.BodyBold });
            indicator.Controls.Add(new ProgressBar { Dock = DockStyle.Bottom, Height = 8, Style = ProgressBarStyle.Marquee, MarqueeAnimationSpeed = 25 });
            loading.Controls.Add(indicator);
            EventHandler center = (s, e) => indicator.Location = new Point(Math.Max(0, (loading.ClientSize.Width - indicator.Width) / 2), Math.Max(0, (loading.ClientSize.Height - indicator.Height) / 2));
            loading.Resize += center; Controls.Add(loading); loading.BringToFront(); center(null, EventArgs.Empty); loading.Refresh();
            try
            {
                await Task.Yield();
                using (var api = new AlegraClient())
                {
                    var invoice = AlegraWorkflow.Object(await api.GetInvoiceDetailsAsync(id, lifetime.Token));
                    if (IsDisposed) return;
                    if (AlegraWorkflow.Text(invoice, "id") != id) throw new InvalidOperationException("La respuesta no corresponde a la factura seleccionada.");
                    using (var detail = new FrmAlegraInvoiceDetails(invoice))
                    {
                        loading.Hide();
                        detail.ShowDialog(this);
                    }
                    if (!IsDisposed) status.Text = "Detalle de factura consultado.";
                }
            }
            catch (OperationCanceledException) { if (!IsDisposed) status.Text = "Consulta cancelada."; }
            catch (Exception ex) { if (!IsDisposed) status.Text = ex is InvalidOperationException ? ex.Message : "No se pudo consultar el detalle de la factura."; }
            finally { loading.Resize -= center; loading.Dispose(); busy = false; if (!IsDisposed) UpdateControls(); }
        }
    }
}
