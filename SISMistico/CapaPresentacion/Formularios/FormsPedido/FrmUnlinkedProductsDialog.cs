using CapaNegocio.Alegra;
using CapaPresentacion.Formularios.FormsProductos;
using CapaPresentacion.UI;
using FontAwesome.Sharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CapaPresentacion.Formularios.FormsPedido
{
    public sealed class FrmUnlinkedProductsDialog : Form
    {
        private readonly DataGridView grid = new DataGridView();
        private readonly Label explanation = new Label();
        private readonly Label status = new Label();
        private readonly Badge counter = new Badge { Kind = BadgeKind.Warning, Width = 235, Height = 30, Margin = new Padding(8, 0, 0, 0) };
        // Antigüedad de la validación con Alegra de la que se toman los datos (caché compartida de la aplicación).
        private readonly Badge cacheAge = new Badge { Kind = BadgeKind.Neutral, Width = 250, Height = 30, Margin = Padding.Empty };
        private readonly System.Windows.Forms.Timer cacheAgeTimer = new System.Windows.Forms.Timer { Interval = 60000 };
        private AlegraProductValidationCache validationCache;
        private readonly ModernButton send = new ModernButton { Kind = ButtonKind.Success, Icon = IconChar.PaperPlane, Width = 240 };
        private readonly ModernButton local = new ModernButton { Text = "No enviar a Alegra", Kind = ButtonKind.Neutral, Width = 205 };
        private readonly ModernButton back = new ModernButton { Text = "Volver", Kind = ButtonKind.Outline, Width = 125 };
        private readonly DataTable rows = new DataTable();
        private readonly Func<string> totalCalculator;
        private readonly string saleTotal;
        private readonly int initialLinked;
        private readonly string initialInvoiceTotal;
        private readonly AlegraProductGridActions actions;
        private bool busy;

        public FrmUnlinkedProductsDialog(IList<string> unlinked, int linkedCount, string invoiceTotalText,
            string saleTotalText, DataTable products = null, Func<string> totalCalculator = null)
        {
            this.totalCalculator = totalCalculator;
            saleTotal = saleTotalText; initialLinked = linkedCount; initialInvoiceTotal = invoiceTotalText;
            Text = "Productos de la factura · Alegra"; StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(1180, 650); MinimumSize = new Size(960, 570);
            MinimizeBox = false; ShowInTaskbar = false; BackColor = Theme.Background; Font = Theme.Body;
            AutoScaleMode = AutoScaleMode.Font; FrmProductos.ApplyWindowIcon(this);
            var root = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(24), ColumnCount = 1, RowCount = 5 };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            var header = new Panel { Dock = DockStyle.Fill };
            header.Controls.Add(new Label { Text = "Completa los productos de tu factura", Font = Theme.Title,
                ForeColor = Theme.TextPrimary, AutoSize = true });
            header.Controls.Add(new Label { Text = "Pulsa Agregar a Alegra, ajusta precio e impuesto y guarda desde la misma fila.",
                ForeColor = Theme.TextMuted, AutoSize = true, Top = 43 });
            root.Controls.Add(header, 0, 0);
            var info = new Panel { Dock = DockStyle.Fill };
            var badges = new FlowLayoutPanel { Dock = DockStyle.Right, AutoSize = true, WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight, Margin = Padding.Empty, Padding = Padding.Empty };
            badges.Controls.Add(cacheAge); badges.Controls.Add(counter);
            explanation.Dock = DockStyle.Fill; explanation.ForeColor = Theme.TextMuted;
            explanation.Padding = new Padding(0, 5, 16, 0);
            info.Controls.Add(explanation); info.Controls.Add(badges); root.Controls.Add(info, 0, 1);
            var card = new ModernCard { Dock = DockStyle.Fill, Padding = new Padding(14) };
            grid.Dock = DockStyle.Fill; grid.AutoGenerateColumns = false; grid.StyleModernGrid();
            grid.AllowUserToDeleteRows = false; grid.RowTemplate.Height = 54; grid.ColumnHeadersHeight = 46;
            Column("Nombre", "PRODUCTO", 30, 170); Column("Tipo", "TIPO", 10, 85);
            Column("Precio", "PRECIO DE VENTA", 17, 145); Column("EstadoAlegra", "ESTADO", 20, 140);
            grid.Columns["Precio"].DefaultCellStyle.Format = "C2";
            grid.Columns["Precio"].DefaultCellStyle.FormatProvider = CultureInfo.GetCultureInfo("es-CO");
            grid.Columns["Precio"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            grid.UseBadgeColumn("ESTADO", value => value == "Vinculado" ? BadgeKind.Success : BadgeKind.Warning);
            rows.Columns.Add("Id", typeof(int)); rows.Columns.Add("Tipo"); rows.Columns.Add("Nombre");
            rows.Columns.Add("Precio", typeof(decimal)); rows.Columns.Add("EstadoAlegra"); rows.Columns.Add("VinculadoAlegra", typeof(bool));
            var settings = AlegraWorkflow.Settings();
            if (products != null)
            {
                var seen = new HashSet<string>();
                foreach (DataRow product in products.Rows)
                {
                    string kind = Convert.ToString(product["Tipo"]); int id = Convert.ToInt32(product["Id_tipo"]);
                    string key = kind + ":" + id.ToString(CultureInfo.InvariantCulture);
                    if (!seen.Add(key) || settings.Products.ContainsKey(key)) continue;
                    rows.Rows.Add(id, kind, product["Nombre"], product["Precio"], "Sin vincular", false);
                }
            }
            else foreach (string name in unlinked) rows.Rows.Add(0, "", name, 0m, "Sin vincular", false);
            // La caché de validación (la misma que puebla el catálogo de Productos) evita consultar Alegra por cada fila:
            // trae existencia, producto exacto y similares. Los productos que no estén en ella se consultan en vivo como antes.
            if (products != null)
            {
                try { validationCache = AlegraProductValidationCache.Shared(); validationCache.Restore(rows); }
                catch (Exception ex) { cacheError = "No se pudo leer la caché de Alegra: " + ex.Message; }
            }
            grid.DataSource = rows;
            actions = new AlegraProductGridActions(grid, message => status.Text = message, SetBusy, RefreshLinks, SaveLinkedToCache);
            if (products == null) { grid.Columns["AgregarAlegra"].Visible = false; grid.Columns["ImpuestoNuevoAlegra"].Visible = false; }
            card.Controls.Add(grid); root.Controls.Add(card, 0, 2);
            status.Dock = DockStyle.Fill; status.ForeColor = Theme.TextMuted; status.Font = Theme.Small;
            status.Padding = new Padding(3, 8, 0, 0); root.Controls.Add(status, 0, 3);
            var footer = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink, FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(0, 10, 0, 0), Margin = Padding.Empty, WrapContents = true };
            foreach (var button in new[] { send, local, back })
            { button.Height = 46; button.Margin = new Padding(8, 0, 0, 6); footer.Controls.Add(button); }
            send.Click += (s, e) => Finish(DialogResult.Yes);
            local.Click += (s, e) => Finish(DialogResult.No);
            back.Click += (s, e) => Finish(DialogResult.Cancel);
            root.Controls.Add(footer, 0, 4); Controls.Add(root); CancelButton = back;
            RefreshCacheAge();
            cacheAgeTimer.Tick += (s, e) => RefreshCacheAge();
            cacheAgeTimer.Start();
            RefreshSummary(settings);
            Shown += async (s, e) =>
            {
                SetBusy(true);
                try { if (products != null) await actions.LoadTaxesAsync(); }
                finally { SetBusy(false); }
            };
            FormClosing += (s, e) => { if (busy) e.Cancel = true; };
            FormClosed += (s, e) => { cacheAgeTimer.Dispose(); actions.Dispose(); rows.Dispose(); };
        }

        private string cacheError = "";

        private void RefreshCacheAge()
        {
            DateTime? validatedAt = validationCache == null ? (DateTime?)null : validationCache.ValidatedAtUtc;
            cacheAge.Text = AlegraProductValidationCache.AgeText(validatedAt);
            cacheAge.Kind = AlegraProductValidationCache.AgeKind(validatedAt);
        }

        // Un producto vinculado desde esta ventana queda también en la caché compartida (sin cambiar su fecha).
        private void SaveLinkedToCache(DataRow row)
        {
            try { if (validationCache != null) validationCache.Merge(new[] { row }); }
            catch (Exception ex) { status.Text = "Producto vinculado. No se pudo actualizar la caché de Alegra: " + ex.Message; }
        }

        private void Column(string property, string caption, int weight, int minimum)
        {
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = property, DataPropertyName = property,
                HeaderText = caption, ReadOnly = true, AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = weight, MinimumWidth = minimum });
        }

        private void SetBusy(bool value)
        { busy = value; send.Enabled = local.Enabled = back.Enabled = !value; grid.Enabled = !value; if (!value) send.Enabled = CanSend(); }

        private bool CanSend()
        { return initialLinked > 0 || rows.Rows.Cast<DataRow>().Any(row => Convert.ToBoolean(row["VinculadoAlegra"])); }

        private Task RefreshLinks()
        {
            RefreshSummary(AlegraWorkflow.Settings());
            return Task.CompletedTask;
        }

        private void RefreshSummary(AlegraInvoiceConfiguration settings)
        {
            int pending = 0;
            foreach (DataRow row in rows.Rows)
            {
                string key = Convert.ToString(row["Tipo"]) + ":" + Convert.ToString(row["Id"], CultureInfo.InvariantCulture);
                bool linked = settings.Products.ContainsKey(key);
                row["VinculadoAlegra"] = linked; row["EstadoAlegra"] = linked ? "Vinculado" : "Sin vincular";
                if (!linked) pending++;
            }
            counter.Text = pending == 0 ? "Todos vinculados" : pending + " pendientes de vincular";
            counter.Kind = pending == 0 ? BadgeKind.Success : BadgeKind.Warning;
            send.Text = pending == 0 ? "Enviar factura completa" : "Enviar a Alegra parcial";
            send.Enabled = !busy && CanSend();
            string amount = initialInvoiceTotal;
            if (totalCalculator != null && CanSend())
            {
                try { amount = totalCalculator(); }
                catch { amount = "el total de los productos vinculados"; }
            }
            explanation.Text = pending == 0
                ? "Todos los productos están vinculados. Puedes continuar con la factura electrónica completa."
                : CanSend() ? "Factura electrónica parcial: " + amount + ". Total de la venta: " + saleTotal + ".\nAgrega los pendientes para enviar la factura completa, o continúa solo con los vinculados."
                : "Todavía no hay productos vinculados para esta factura.\nAgrégalos a Alegra o elige No enviar a Alegra para facturar solo en local.";
            status.Text = string.IsNullOrEmpty(cacheError)
                ? "Pulsa Agregar a Alegra para editar Precio Alegra (sin impuesto) y elegir el impuesto. Después pulsa Guardar."
                : cacheError;
        }

        private void Finish(DialogResult result)
        { if (busy) return; DialogResult = result; Close(); }
    }
}
