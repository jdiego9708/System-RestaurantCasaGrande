using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CapaNegocio;
using CapaNegocio.Alegra;
using CapaPresentacion.UI;
using FontAwesome.Sharp;

namespace CapaPresentacion.Formularios.FormsProductos
{
    public class FrmProductos : Form
    {
        private readonly ModernTextBox search = new ModernTextBox { PlaceholderText = "Buscar por nombre o categoría…", Icon = IconChar.MagnifyingGlass };
        private readonly FlowLayoutPanel type = new FlowLayoutPanel { WrapContents = false };
        private readonly FlowLayoutPanel state = new FlowLayoutPanel { WrapContents = false };
        private readonly ModernRadioButton plates = new ModernRadioButton { Text = "Platos", Width = 96 };
        private readonly ModernRadioButton beverages = new ModernRadioButton { Text = "Bebidas", Width = 108 };
        private readonly ModernRadioButton allTypes = new ModernRadioButton { Text = "Ambos", Width = 96, Checked = true };
        private readonly ModernRadioButton activeProducts = new ModernRadioButton { Text = "Activos", Width = 100, Checked = true };
        private readonly ModernRadioButton inactiveProducts = new ModernRadioButton { Text = "Inactivos", Width = 112 };
        private readonly ModernRadioButton allStates = new ModernRadioButton { Text = "Ambos", Width = 96 };
        private readonly DataGridView grid = new DataGridView();
        private readonly Label summary = new Label();
        private readonly Label status = new Label();
        private readonly ModernButton edit = new ModernButton { Text = "Editar producto", Kind = ButtonKind.Outline, Width = 170, Icon = IconChar.Pen };
        private readonly ModernButton toggle = new ModernButton { Text = "Inactivar", Kind = ButtonKind.Outline, Width = 155 };
        private readonly ModernButton create = new ModernButton { Text = "Nuevo producto", Width = 190, Icon = IconChar.Plus };
        private readonly ModernButton refresh = new ModernButton { Text = "Actualizar", Kind = ButtonKind.Outline, Width = 140 };
        private readonly ModernButton validateAlegra = new ModernButton { Text = "VALIDAR PRODUCTOS CON ALEGRA",
            Kind = ButtonKind.Primary, Width = 340, Icon = IconChar.Check };
        private readonly ModernCard alegraLoading = new ModernCard { Visible = false, Height = 112,
            AccentColor = Theme.Primary, Padding = new Padding(16, 12, 16, 12), Shadow = false };
        private readonly Label alegraLoadingDetail = new Label { ForeColor = Theme.TextMuted, Font = Theme.Small,
            Dock = DockStyle.Fill, AutoEllipsis = true, TextAlign = ContentAlignment.MiddleLeft };
        private readonly ProgressBar alegraProgress = new ProgressBar { Dock = DockStyle.Fill,
            Style = ProgressBarStyle.Marquee, MarqueeAnimationSpeed = 0 };
        private DataTable products;
        private bool busy;
        private CancellationTokenSource validationCancellation;
        private string alegraStatus = "";
        private AlegraProductGridActions alegraActions;
        private AlegraProductValidationCache validationCache;
        private DateTime? lastValidatedAtUtc;
        private readonly Label lastValidation = new Label { AutoSize = true, Margin = new Padding(10, 5, 0, 0),
            ForeColor = Theme.TextMuted, Font = Theme.Small, Text = "Última validación de Alegra: sin validación guardada." };
        // Badge con el tiempo transcurrido desde la última validación; se refresca cada minuto.
        private readonly Badge validationAge = new Badge { Width = 250, Height = 26, Margin = Padding.Empty, Kind = BadgeKind.Neutral,
            Text = AlegraProductValidationCache.AgeText(null) };
        private readonly System.Windows.Forms.Timer validationAgeTimer = new System.Windows.Forms.Timer { Interval = 60000 };

        private void ShowLastValidation(DateTime? utc)
        {
            lastValidatedAtUtc = utc;
            validationAge.Text = AlegraProductValidationCache.AgeText(utc);
            validationAge.Kind = AlegraProductValidationCache.AgeKind(utc);
            lastValidation.Text = utc.HasValue ? "Última validación de Alegra: "
                + utc.Value.ToLocalTime().ToString("dd/MM/yyyy hh:mm tt", CultureInfo.GetCultureInfo("en-US"))
                : "Última validación de Alegra: sin validación guardada.";
        }

        public FrmProductos()
        {
            Text = "Productos"; BackColor = Theme.Background; Font = Theme.Body;
            AutoScaleMode = AutoScaleMode.Font; StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            ApplyWindowIcon(this);
            ClientSize = new Size(1100, 740); MinimumSize = new Size(920, 620);
            var root = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(26), ColumnCount = 1, RowCount = 5 };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 82));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 104));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            var header = new Panel { Dock = DockStyle.Fill };
            header.Controls.Add(new Label { Text = "Productos", AutoSize = true, Font = Theme.Title, ForeColor = Theme.TextPrimary });
            header.Controls.Add(new Label { Text = "Tu catálogo de platos y bebidas, en un solo lugar.", AutoSize = true,
                Top = 43, ForeColor = Theme.TextMuted });
            create.Anchor = AnchorStyles.Top | AnchorStyles.Right; create.Top = 10;
            header.Controls.Add(create); header.Resize += (s, e) => create.Left = header.Width - create.Width;
            root.Controls.Add(header, 0, 0);
            var filters = new ModernCard { Dock = DockStyle.Fill, Padding = new Padding(18, 10, 18, 12) };
            var filterLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 2,
                Margin = Padding.Empty, Padding = Padding.Empty };
            filterLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
            filterLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            filterLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            filterLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 312));
            filterLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 320));
            foreach (var radio in new[] { plates, beverages, allTypes, activeProducts, inactiveProducts, allStates })
                radio.Margin = Padding.Empty;
            type.Controls.AddRange(new Control[] { plates, beverages, allTypes });
            state.Controls.AddRange(new Control[] { activeProducts, inactiveProducts, allStates });
            string[] labels = { "BUSCAR PRODUCTOS", "TIPO", "ESTADO" };
            Control[] inputs = { search, type, state };
            for (int i = 0; i < inputs.Length; i++)
            {
                filterLayout.Controls.Add(new Label { Text = labels[i], Font = Theme.Small, ForeColor = Theme.TextMuted,
                    Dock = DockStyle.Fill, AutoSize = false, Margin = Padding.Empty, TextAlign = ContentAlignment.MiddleLeft }, i, 0);
                inputs[i].Dock = DockStyle.Top; inputs[i].Height = 42;
                inputs[i].Margin = new Padding(0, 0, i == 2 ? 0 : 12, 0);
                filterLayout.Controls.Add(inputs[i], i, 1);
            }
            filters.Controls.Add(filterLayout); root.Controls.Add(filters, 0, 1);
            summary.Dock = DockStyle.Fill; summary.TextAlign = ContentAlignment.MiddleLeft;
            summary.Font = Theme.BodyBold; summary.ForeColor = Theme.TextMuted; root.Controls.Add(summary, 0, 2);
            var card = new ModernCard { Dock = DockStyle.Fill, Padding = new Padding(14) };
            grid.Dock = DockStyle.Fill; grid.StyleModernGrid(); grid.AutoGenerateColumns = false;
            grid.ReadOnly = true; grid.AllowUserToAddRows = false; grid.AllowUserToDeleteRows = false;
            grid.MultiSelect = false; grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.RowHeadersVisible = false; grid.RowTemplate.Height = 48;
            grid.ColumnHeadersHeight = 46;
            grid.UseBadgeColumn("ESTADO", value => value == "ACTIVO" ? BadgeKind.Success : BadgeKind.Neutral);
            AddColumn("Nombre", "PRODUCTO", 32); AddColumn("Tipo", "TIPO", 12);
            AddColumn("Categoria", "CATEGORÍA", 24); AddColumn("Precio", "PRECIO (COP)", 17);
            AddColumn("Estado", "ESTADO", 15);
            grid.Columns["Precio"].DefaultCellStyle.Format = "C2";
            grid.Columns["Precio"].DefaultCellStyle.FormatProvider = CultureInfo.GetCultureInfo("es-CO");
            grid.Columns["Precio"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            grid.CellFormatting += (s, e) =>
            {
                if (e.ColumnIndex == grid.Columns["Estado"].Index && e.Value != null)
                {
                    bool enabled = Convert.ToString(e.Value) == "ACTIVO";
                    e.CellStyle.ForeColor = enabled ? Theme.Success : Theme.TextMuted;
                    e.CellStyle.BackColor = Theme.Lighten(enabled ? Theme.Success : Theme.Neutral, .94f);
                    e.CellStyle.SelectionForeColor = e.CellStyle.ForeColor;
                    e.CellStyle.SelectionBackColor = e.CellStyle.BackColor;
                }
                if (grid.Columns[e.ColumnIndex].Name == "ExisteAlegra" && e.Value != null && e.Value != DBNull.Value)
                {
                    bool exists = Convert.ToString(e.Value) == "Sí";
                    e.CellStyle.ForeColor = exists ? Theme.Success : Theme.Danger;
                    e.CellStyle.SelectionForeColor = e.CellStyle.ForeColor;
                    e.CellStyle.BackColor = Theme.Lighten(exists ? Theme.Success : Theme.Danger, .94f);
                    e.CellStyle.SelectionBackColor = e.CellStyle.BackColor;
                }
            };
            grid.CellToolTipTextNeeded += (s, e) =>
            {
                if (e.RowIndex < 0 || e.ColumnIndex < 0 || !grid.Columns[e.ColumnIndex].Name.EndsWith("Alegra")) return;
                var row = grid.Rows[e.RowIndex].DataBoundItem as DataRowView;
                if (row != null && row.Row.Table.Columns.Contains("DetalleAlegra"))
                    e.ToolTipText = Convert.ToString(row["DetalleAlegra"]);
            };
            card.Controls.Add(grid); root.Controls.Add(card, 0, 3);
            var footer = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 4, ColumnCount = 1,
                AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Margin = Padding.Empty };
            footer.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            footer.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            footer.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            footer.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(0, 12, 0, 8),
                AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = false, AutoScroll = true, Margin = Padding.Empty };
            foreach (var button in new[] { edit, toggle, refresh, validateAlegra })
            {
                button.Height = 44;
                button.Margin = new Padding(0, 0, 8, 6);
            }
            actions.Controls.Add(edit); actions.Controls.Add(toggle); actions.Controls.Add(refresh);
            actions.Controls.Add(validateAlegra);
            footer.Controls.Add(actions, 0, 0);
            var loadingContent = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4,
                Margin = Padding.Empty, Padding = Padding.Empty };
            loadingContent.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
            loadingContent.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
            loadingContent.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            loadingContent.RowStyles.Add(new RowStyle(SizeType.Absolute, 8));
            loadingContent.Controls.Add(new Label { Text = "Consultando productos en Alegra…", Font = Theme.BodyBold,
                ForeColor = Theme.Primary, Dock = DockStyle.Fill, Margin = Padding.Empty }, 0, 0);
            loadingContent.Controls.Add(new Label { Text = "Espera un momento mientras se cargan los productos.",
                Font = Theme.Small, ForeColor = Theme.TextPrimary, Dock = DockStyle.Fill, Margin = Padding.Empty }, 0, 1);
            alegraLoadingDetail.Margin = Padding.Empty; alegraProgress.Margin = Padding.Empty;
            loadingContent.Controls.Add(alegraLoadingDetail, 0, 2);
            loadingContent.Controls.Add(alegraProgress, 0, 3);
            alegraLoading.Controls.Add(loadingContent); alegraLoading.Dock = DockStyle.Top;
            alegraLoading.Margin = new Padding(0, 0, 0, 8);
            footer.Controls.Add(alegraLoading, 0, 1);
            status.Dock = DockStyle.Fill; status.AutoSize = true; status.Margin = new Padding(0, 0, 0, 4);
            status.ForeColor = Theme.TextMuted; status.Font = Theme.Small;
            footer.Controls.Add(status, 0, 2); root.Controls.Add(footer, 0, 4); Controls.Add(root);
            var validationRow = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight, Margin = new Padding(0, 0, 0, 4), Padding = Padding.Empty };
            validationRow.Controls.Add(validationAge); validationRow.Controls.Add(lastValidation);
            footer.Controls.Add(validationRow, 0, 3);
            validationAgeTimer.Tick += (s, e) => ShowLastValidation(lastValidatedAtUtc);
            validationAgeTimer.Start();
            search.TextChanged += (s, e) => Filter();
            foreach (var radio in new[] { plates, beverages, allTypes, activeProducts, inactiveProducts, allStates })
                radio.CheckedChanged += (s, e) => { if (((RadioButton)s).Checked) Filter(); };
            grid.SelectionChanged += (s, e) => UpdateActions();
            grid.CellDoubleClick += async (s, e) =>
            {
                if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && grid.Columns[e.ColumnIndex].Name != "AgregarAlegra"
                    && grid.Columns[e.ColumnIndex].Name != "ImpuestoNuevoAlegra"
                    && grid.Columns[e.ColumnIndex].Name != "SimilaresAlegra"
                    && grid.Columns[e.ColumnIndex].Name != "ProductoRelacionadoAlegra") await OpenEditor(true);
            };
            create.Click += async (s, e) => await OpenEditor(false);
            edit.Click += async (s, e) => await OpenEditor(true);
            refresh.Click += async (s, e) => await Reload();
            toggle.Click += async (s, e) => await ChangeState();
            validateAlegra.Click += async (s, e) =>
            {
                if (validationCancellation != null) { validationCancellation.Cancel(); return; }
                await ValidateWithAlegra();
            };
            Shown += async (s, e) => await Reload();
            FormClosing += (s, e) =>
            {
                if (!busy) return;
                if (validationCancellation != null) validationCancellation.Cancel();
                e.Cancel = true;
            };
            UpdateActions();
            FormClosed += (s, e) => { validationAgeTimer.Dispose(); if (alegraActions != null) alegraActions.Dispose(); };
        }

        internal static void ApplyWindowIcon(Form form)
        {
            var resources = new System.ComponentModel.ComponentResourceManager(typeof(FormsPrincipales.FrmPrincipal));
            var applicationIcon = resources.GetObject("$this.Icon") as System.Drawing.Icon;
            if (applicationIcon == null) return;
            var windowIcon = (System.Drawing.Icon)applicationIcon.Clone();
            form.Icon = windowIcon;
            form.Disposed += (s, e) => windowIcon.Dispose();
        }

        private void AddColumn(string field, string title, int weight)
        {
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = field, DataPropertyName = field,
                HeaderText = title, ReadOnly = true, AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = weight });
        }
        private DataRow Selected
        { get { var row = grid.CurrentRow == null ? null : grid.CurrentRow.DataBoundItem as DataRowView; return row == null ? null : row.Row; } }
        private void UpdateActions()
        {
            var row = Selected;
            edit.Enabled = toggle.Enabled = !busy && row != null;
            create.Enabled = refresh.Enabled = !busy;
            validateAlegra.Enabled = validationCancellation != null || (!busy && grid.Rows.Count > 0);
            grid.Enabled = !busy;
            search.Enabled = type.Enabled = state.Enabled = !busy;
            toggle.Text = row != null && Convert.ToString(row["Estado"]) == "ACTIVO" ? "Inactivar" : "Activar";
        }
        private void Filter()
        {
            if (products == null) return;
            // Filtrado literal en memoria: nombres con comillas, %, [] y * no se interpretan como expresiones.
            string query = search.Text.Trim();
            int count = 0, enabled = 0;
            DataTable visible = products.Clone();
            foreach (DataRow row in products.Rows)
            {
                string kind = Convert.ToString(row["Tipo"]), availability = Convert.ToString(row["Estado"]);
                if (plates.Checked && kind != "PLATO" || beverages.Checked && kind != "BEBIDA") continue;
                if (activeProducts.Checked && availability != "ACTIVO" || inactiveProducts.Checked && availability != "INACTIVO") continue;
                if (query.Length > 0 && (Convert.ToString(row["Nombre"]) + " " + Convert.ToString(row["Categoria"]))
                    .IndexOf(query, StringComparison.CurrentCultureIgnoreCase) < 0) continue;
                visible.ImportRow(row); count++; if (availability == "ACTIVO") enabled++;
            }
            var previous = grid.DataSource as DataTable;
            grid.DataSource = visible; if (previous != null) previous.Dispose();
            summary.Text = count + " productos  ·  " + enabled + " activos  ·  " + (count - enabled) + " inactivos";
            if (!busy)
                status.Text = string.IsNullOrWhiteSpace(alegraStatus)
                    ? (count == 0 ? "No hay productos con estos filtros. Puedes cambiar la búsqueda o crear un producto." : "Selecciona un producto para editarlo o cambiar su estado. Doble clic para editar.")
                    : alegraStatus;
            UpdateActions();
        }
        private async Task Reload()
        {
            if (busy) return;
            busy = true; UpdateActions(); status.Text = "Cargando catálogo…";
            try
            {
                var result = await Task.Run(() => NProductos.Listar());
                DateTime? restored = null;
                string cacheWarning = "";
                try
                {
                    // Caché de aplicación compartida con la ventana de productos de la factura.
                    validationCache = AlegraProductValidationCache.Shared();
                    restored = validationCache.Restore(result);
                }
                catch (Exception ex) { cacheWarning = "No se pudo cargar la caché de Alegra: " + ex.Message; }
                ShowLastValidation(restored);
                alegraStatus = restored.HasValue ? "Mostrando la última validación guardada de Alegra. Valida para actualizar desde la nube." : cacheWarning;
                if (restored.HasValue) AddAlegraColumns();
                ShowAlegraColumns(restored.HasValue);
                if (products != null) products.Dispose(); products = result; Filter();
                status.Text = string.IsNullOrWhiteSpace(alegraStatus) ? "Catálogo actualizado." : alegraStatus;
            }
            catch (Exception ex) { status.Text = "No se pudo cargar el catálogo: " + ex.Message; }
            finally { busy = false; UpdateActions(); }
        }

        private void AddAlegraColumns()
        {
            if (grid.Columns.Contains("ExisteAlegra")) return;
            AddColumn("ExisteAlegra", "Existe", 10);
            AddColumn("PrecioAlegra", "Precio Alegra", 18);
            AddColumn("ImpuestoAlegra", "Impuesto", 18);
            grid.Columns["ExisteAlegra"].MinimumWidth = 80;
            grid.Columns["PrecioAlegra"].MinimumWidth = 140;
            grid.Columns["ImpuestoAlegra"].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            grid.Columns["ImpuestoAlegra"].MinimumWidth = 80;
            grid.Columns["PrecioAlegra"].DefaultCellStyle.Format = "C2";
            grid.Columns["PrecioAlegra"].DefaultCellStyle.FormatProvider = CultureInfo.GetCultureInfo("es-CO");
            grid.Columns["PrecioAlegra"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            // El API entrega porcentajes como 8, no como 0.08.
            grid.Columns["ImpuestoAlegra"].DefaultCellStyle.Format = "0.##'%'";
            grid.Columns["ImpuestoAlegra"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            grid.Columns["PrecioAlegra"].DefaultCellStyle.NullValue = "—";
            grid.Columns["ImpuestoAlegra"].DefaultCellStyle.NullValue = "—";
            grid.Columns["ExisteAlegra"].DefaultCellStyle.NullValue = "—";
            EnsureAlegraActions();
            grid.Columns["ImpuestoNuevoAlegra"].DisplayIndex = grid.Columns.Count - 4;
            grid.Columns["ProductoRelacionadoAlegra"].DisplayIndex = grid.Columns.Count - 3;
            grid.Columns["AgregarAlegra"].DisplayIndex = grid.Columns.Count - 2;
            grid.Columns["SimilaresAlegra"].DisplayIndex = grid.Columns.Count - 1;
        }

        private void EnsureAlegraActions()
        {
            if (alegraActions == null)
                alegraActions = new AlegraProductGridActions(grid, message => status.Text = message,
                    value => { busy = value; UpdateActions(); }, () => Task.CompletedTask, ProductConfirmed);
        }

        private void ProductConfirmed(DataRow confirmed)
        {
            if (products == null) return;
            string kind = Convert.ToString(confirmed["Tipo"]);
            int id = Convert.ToInt32(confirmed["Id"]);
            DataRow target = products.Rows.Cast<DataRow>().FirstOrDefault(row =>
                Convert.ToString(row["Tipo"]) == kind && Convert.ToInt32(row["Id"]) == id);
            if (target == null) return;
            target.BeginEdit();
            try
            {
                foreach (string column in new[] { "ExisteAlegra", "PrecioAlegra", "ImpuestoAlegra", "DetalleAlegra",
                    "VinculadoAlegra", "SimilarProducts", "ExactProduct" }) target[column] = confirmed[column];
                target.EndEdit();
            }
            catch { target.CancelEdit(); throw; }
            // Guardar el estado sin atribuir una nueva validacion completa al resto del catalogo.
            if (validationCache != null && lastValidatedAtUtc.HasValue)
            {
                try { validationCache.Merge(new[] { target }); }
                catch (Exception ex)
                {
                    status.Text = "Producto vinculado y actualizado en memoria. No se pudo guardar la cach\u00e9: " + ex.Message;
                }
            }
        }

        private void ShowAlegraColumns(bool visible)
        {
            foreach (string name in new[] { "ExisteAlegra", "PrecioAlegra", "ImpuestoAlegra", "AgregarAlegra" })
                if (grid.Columns.Contains(name)) grid.Columns[name].Visible = visible;
            // El impuesto de alta se muestra únicamente al comenzar a agregar un producto.
            if (grid.Columns.Contains("ImpuestoNuevoAlegra")) grid.Columns["ImpuestoNuevoAlegra"].Visible = false;
            if (grid.Columns.Contains("ProductoRelacionadoAlegra")) grid.Columns["ProductoRelacionadoAlegra"].Visible = false;
            if (grid.Columns.Contains("SimilaresAlegra")) grid.Columns["SimilaresAlegra"].Visible = visible;
        }

        private async Task ValidateWithAlegra()
        {
            var visible = grid.DataSource as DataTable;
            if (busy || products == null || visible == null || visible.Rows.Count == 0) return;
            busy = true;
            validationCancellation = new CancellationTokenSource();
            var cancellation = validationCancellation;
            validateAlegra.Text = "Cancelar validación";
            ShowAlegraColumns(false);
            alegraLoadingDetail.Text = "Conectando con Alegra…";
            alegraLoading.Visible = true; alegraProgress.MarqueeAnimationSpeed = 25;
            grid.UseWaitCursor = true;
            UpdateActions(); status.Text = "";
            var progress = new Progress<string>(message =>
            {
                if (!IsDisposed && validationCancellation == cancellation) alegraLoadingDetail.Text = message;
            });
            // Aplicar resultados completos evita confundir consultas interrumpidas con productos inexistentes.
            using (var snapshot = visible.Copy())
            {
                try
                {
                    if (validationCache == null) throw new InvalidOperationException("No se pudo preparar la caché. Actualiza el catálogo e inténtalo otra vez.");
                    validationCache.Clear();
                    AlegraProductValidation.PrepareColumns(products);
                    ShowLastValidation(null);
                    var missingUnits = await Task.Run(() => AlegraProductValidation.ValidateAsync(snapshot, progress, cancellation.Token));
                    cancellation.Token.ThrowIfCancellationRequested();
                    ApplyValidationResults(products, snapshot);
                    int found = 0, review = 0;
                    foreach (DataRow row in snapshot.Rows)
                    {
                        if (Convert.ToString(row["ExisteAlegra"]) == "Sí")
                        {
                            found++;
                            if (row.IsNull("PrecioAlegra") || row.IsNull("ImpuestoAlegra")) review++;
                        }
                    }
                    alegraStatus = "Validación Alegra " + DateTime.Now.ToString("HH:mm") + ": " + found + " existen · "
                        + (snapshot.Rows.Count - found) + " no encontrados"
                        + (review > 0 ? " · " + review + " por revisar (pasa el cursor por las celdas)" : "")
                        + ". " + snapshot.Rows.Count + " productos validados con los filtros actuales. Precio general e impuestos actuales de Alegra.";
                    AddAlegraColumns();
                    ShowAlegraColumns(true);
                    DateTime validatedAt = DateTime.UtcNow;
                    try
                    {
                        validationCache.Save(snapshot, validatedAt);
                        ShowLastValidation(validatedAt);
                    }
                    catch (Exception ex) { alegraStatus += " No se pudo guardar la caché: " + ex.Message; }
                    if (missingUnits.Count > 0) alegraStatus += await OfferUnitFix(missingUnits, cancellation.Token);
                }
                catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
                { alegraStatus = "Validación cancelada. Vuelve a validar para consultar los datos de Alegra."; }
                catch (OperationCanceledException)
                { alegraStatus = "Alegra tardó demasiado en responder. Vuelve a intentar la validación."; }
                catch (Exception ex) { alegraStatus = "No se pudo validar con Alegra: " + ex.Message; }
                finally
                {
                    validationCancellation = null; cancellation.Dispose(); busy = false;
                    alegraProgress.MarqueeAnimationSpeed = 0; alegraLoading.Visible = false;
                    grid.UseWaitCursor = false;
                    validateAlegra.Text = "VALIDAR PRODUCTOS CON ALEGRA";
                    Filter(); UpdateActions();
                }
            }
        }
        // Alegra no timbra facturas electrónicas con productos sin unidad de medida (código 3051): se ofrece corregirlos.
        private async Task<string> OfferUnitFix(System.Collections.Generic.List<AlegraProductValidation.MissingUnitItem> missing, CancellationToken token)
        {
            string names = string.Join("\n", missing.Take(10).Select(item => " • " + item.Name + " (Alegra #" + item.RemoteId + ")"))
                + (missing.Count > 10 ? "\n … y " + (missing.Count - 10) + " más" : "");
            if (MessageBox.Show(this, missing.Count + " producto(s) de Alegra no tienen unidad de medida y la factura electrónica no se podrá timbrar con ellos:\n\n"
                + names + "\n\n¿Asignarles la unidad \"unit\" en Alegra ahora?", "Unidad de medida faltante",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return " · " + missing.Count + " producto(s) sin unidad de medida en Alegra (sin corregir).";
            try
            {
                var results = await Task.Run(() => AlegraProductRegistration.FixMissingUnitsAsync(missing, token));
                MessageBox.Show(this, string.Join("\n", results) + "\n\nVuelve a validar para actualizar el detalle.",
                    "Unidad de medida", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return " · Unidad de medida: " + results.Count(line => line.EndsWith("unidad asignada.")) + " de " + missing.Count + " corregidos.";
            }
            catch (OperationCanceledException) { return " · Corrección de unidades cancelada."; }
        }

        private static void ApplyValidationResults(DataTable catalog, DataTable validated)
        {
            if (!catalog.Columns.Contains("ExisteAlegra")) AlegraProductValidation.PrepareColumns(catalog);
            // Los IDs pueden coincidir entre platos y bebidas; conservar los resultados fuera del filtro.
            var byProduct = catalog.Rows.Cast<DataRow>().ToDictionary(
                row => Convert.ToString(row["Tipo"]) + ":" + Convert.ToString(row["Id"], CultureInfo.InvariantCulture));
            foreach (DataRow result in validated.Rows)
            {
                string key = Convert.ToString(result["Tipo"]) + ":" + Convert.ToString(result["Id"], CultureInfo.InvariantCulture);
                DataRow target;
                if (!byProduct.TryGetValue(key, out target)) continue;
                foreach (string column in new[] { "ExisteAlegra", "PrecioAlegra", "ImpuestoAlegra", "DetalleAlegra", "VinculadoAlegra", "SimilarProducts", "ExactProduct" })
                    target[column] = result[column];
            }
        }
        private async Task OpenEditor(bool editing)
        {
            if (busy || editing && Selected == null) return;
            using (var form = new FrmProducto(editing ? Selected : null))
                if (form.ShowDialog(this) == DialogResult.OK) await Reload();
        }
        private async Task ChangeState()
        {
            var row = Selected; if (busy || row == null) return;
            bool activate = Convert.ToString(row["Estado"]) != "ACTIVO";
            if (MessageBox.Show(this, (activate ? "¿Activar " : "¿Inactivar ") + Convert.ToString(row["Nombre"]) + "?\n" +
                (activate ? "Volverá a estar disponible para nuevos pedidos." : "Dejará de estar disponible para nuevos pedidos. Los pedidos existentes conservan sus datos."),
                "Estado del producto", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            int id = Convert.ToInt32(row["Id"]); bool beverage = Convert.ToString(row["Tipo"]) == "BEBIDA";
            busy = true; UpdateActions();
            try { await Task.Run(() => NProductos.CambiarEstado(id, beverage, activate)); }
            catch (Exception ex) { status.Text = "No se pudo cambiar el estado: " + ex.Message; busy = false; UpdateActions(); return; }
            busy = false; await Reload();
        }
    }
}
