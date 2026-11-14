using CapaNegocio.Alegra;
using CapaPresentacion.UI;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CapaPresentacion.Formularios.FormsProductos
{
    // Acciones compartidas por el catálogo y la resolución de productos al facturar.
    internal sealed class AlegraProductGridActions : IDisposable
    {
        private readonly DataGridView grid;
        private readonly Action<string> status;
        private readonly Action<bool> setBusy;
        private readonly Func<Task> refresh;
        private readonly Action<DataRow> productUpdated;
        private readonly CancellationTokenSource lifetime = new CancellationTokenSource();
        private List<AlegraTaxOption> taxes;
        private bool working;
        private DataGridViewRow editingRow;
        private object previousPrice;
        private object previousTax;
        private string operationText = "";
        private List<AlegraSimilarProduct> similarProducts = new List<AlegraSimilarProduct>();
        private AlegraSimilarProduct relatedProduct;
        private bool similarSearchFailed;

        public AlegraProductGridActions(DataGridView grid, Action<string> status, Action<bool> setBusy, Func<Task> refresh, Action<DataRow> productUpdated = null)
        {
            this.grid = grid; this.status = status; this.setBusy = setBusy; this.refresh = refresh; this.productUpdated = productUpdated;
            grid.ReadOnly = false;
            grid.EditMode = DataGridViewEditMode.EditOnEnter;
            foreach (DataGridViewColumn column in grid.Columns) column.ReadOnly = true;
            if (!grid.Columns.Contains("PrecioAlegra"))
                grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "PrecioAlegra", HeaderText = "Precio Alegra",
                    ReadOnly = true, MinimumWidth = 145, AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                    DefaultCellStyle = new DataGridViewCellStyle { Format = "C2", FormatProvider = CultureInfo.GetCultureInfo("es-CO"),
                        NullValue = "—", Alignment = DataGridViewContentAlignment.MiddleRight } });
            grid.Columns.Add(new DataGridViewComboBoxColumn { Name = "ImpuestoNuevoAlegra", HeaderText = "Impuesto al agregar",
                DisplayMember = "Name", ValueMember = "Id", FlatStyle = FlatStyle.Flat, ReadOnly = false, Visible = false,
                MinimumWidth = 195, AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 24,
                DefaultCellStyle = new DataGridViewCellStyle { NullValue = "Seleccionar impuesto", BackColor = Theme.SurfaceAlt } });
            grid.Columns.Add(new DataGridViewButtonColumn { Name = "AgregarAlegra", HeaderText = "Acción en Alegra",
                Text = "Agregar a Alegra", UseColumnTextForButtonValue = true, FlatStyle = FlatStyle.Flat,
                Width = 180, MinimumWidth = 160, ReadOnly = true });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "ProductoRelacionadoAlegra", HeaderText = "Producto relacionado en Alegra",
                Width = 240, MinimumWidth = 180, ReadOnly = true, Visible = false,
                DefaultCellStyle = new DataGridViewCellStyle { ForeColor = Theme.Info, BackColor = Theme.Lighten(Theme.Info, .94f) } });
            grid.Columns.Add(new DataGridViewButtonColumn { Name = "SimilaresAlegra", HeaderText = "Similares", Width = 88,
                MinimumWidth = 80, ReadOnly = true, Visible = false, FlatStyle = FlatStyle.Flat });
            grid.Columns["ProductoRelacionadoAlegra"].DisplayIndex = grid.Columns["AgregarAlegra"].DisplayIndex;
            grid.Columns["AgregarAlegra"].DefaultCellStyle = new DataGridViewCellStyle {
                Font = Theme.BodyBold, ForeColor = Theme.Primary, BackColor = Theme.Lighten(Theme.Primary, .94f),
                SelectionBackColor = Theme.Lighten(Theme.Primary, .88f), Padding = new Padding(8) };
            grid.CurrentCellDirtyStateChanged += CommitSelection;
            grid.CellEnter += EnterCell;
            grid.CellContentClick += Click;
            grid.CellFormatting += Format;
            grid.CellPainting += PaintAction;
            grid.DataError += DataError;
            grid.CellBeginEdit += BeginEdit;
            grid.CellValidating += ValidateCell;
            grid.CellParsing += ParseCell;
            grid.DataSourceChanged += SourceChanged;
        }

        private bool IsLinked(DataGridViewRow row)
        {
            var view = row.DataBoundItem as DataRowView;
            return view != null && view.Row.Table.Columns.Contains("VinculadoAlegra")
                && !view.Row.IsNull("VinculadoAlegra") && Convert.ToBoolean(view["VinculadoAlegra"]);
        }

        private string ActionText(DataGridViewRow row)
        {
            if (ReferenceEquals(row, editingRow)) return working ? operationText
                : IsLinked(row) ? "Guardar re-vínculo" : relatedProduct == null ? "Guardar" : "Guardar Relacionado";
            var view = row.DataBoundItem as DataRowView;
            bool exists = view != null && view.Row.Table.Columns.Contains("ExisteAlegra") && Convert.ToString(view["ExisteAlegra"]) == "Sí";
            return IsLinked(row) ? "Vinculado" : exists ? "Vincular con Alegra" : "Agregar a Alegra";
        }

        private List<AlegraSimilarProduct> CachedSimilar(DataGridViewRow row)
        {
            var view = row.DataBoundItem as DataRowView;
            return view != null && view.Row.Table.Columns.Contains("SimilarProducts")
                ? view["SimilarProducts"] as List<AlegraSimilarProduct> : null;
        }

        private bool HasSimilarResults()
        {
            return grid.Rows.Cast<DataGridViewRow>().Any(row => CachedSimilar(row) != null);
        }

        private void SourceChanged(object sender, EventArgs e)
        {
            editingRow = null; previousPrice = null; previousTax = null;
            relatedProduct = null; similarProducts = new List<AlegraSimilarProduct>(); similarSearchFailed = false;
            grid.Columns["ImpuestoNuevoAlegra"].Visible = false;
            grid.Columns["ProductoRelacionadoAlegra"].Visible = false;
            grid.Columns["SimilaresAlegra"].Visible = HasSimilarResults();
            grid.Columns["PrecioAlegra"].ReadOnly = true;
        }

        private void ResetDraft(bool restore)
        {
            if (editingRow != null && editingRow.DataGridView == grid)
            {
                grid.CancelEdit();
                if (restore) editingRow.Cells["PrecioAlegra"].Value = previousPrice;
                if (restore && grid.Columns.Contains("ImpuestoAlegra")) editingRow.Cells["ImpuestoAlegra"].Value = previousTax;
                editingRow.Cells["PrecioAlegra"].ReadOnly = true;
                editingRow.Cells["PrecioAlegra"].Style = new DataGridViewCellStyle();
                editingRow.Cells["ImpuestoNuevoAlegra"].Value = null;
                editingRow.Cells["ImpuestoNuevoAlegra"].ReadOnly = true;
                editingRow.Cells["ProductoRelacionadoAlegra"].Value = null;
                editingRow.Cells["PrecioAlegra"].ToolTipText = "";
            }
            editingRow = null; previousPrice = null; previousTax = null;
            relatedProduct = null; similarProducts = new List<AlegraSimilarProduct>(); similarSearchFailed = false;
            grid.Columns["ImpuestoNuevoAlegra"].Visible = false;
            grid.Columns["ProductoRelacionadoAlegra"].Visible = false;
            grid.Columns["SimilaresAlegra"].Visible = HasSimilarResults();
            grid.Columns["PrecioAlegra"].ReadOnly = true;
            grid.Invalidate();
        }

        private void BeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            string column = grid.Columns[e.ColumnIndex].Name;
            e.Cancel = working || relatedProduct != null || !ReferenceEquals(grid.Rows[e.RowIndex], editingRow)
                || (column != "PrecioAlegra" && column != "ImpuestoNuevoAlegra");
        }

        private static bool ReadPrice(object value, out decimal result)
        {
            if (value is decimal) { result = (decimal)value; return true; }
            return decimal.TryParse(Convert.ToString(value, CultureInfo.GetCultureInfo("es-CO")),
                NumberStyles.Currency, CultureInfo.GetCultureInfo("es-CO"), out result);
        }

        private void ValidateCell(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (working || relatedProduct != null || e.RowIndex < 0 || !ReferenceEquals(grid.Rows[e.RowIndex], editingRow)
                || grid.Columns[e.ColumnIndex].Name != "PrecioAlegra") return;
            decimal value;
            if (!ReadPrice(e.FormattedValue, out value) || value <= 0 || decimal.Round(value, 6) != value)
            {
                e.Cancel = true;
                status("Escribe un Precio Alegra mayor que cero, con hasta seis decimales.");
            }
        }

        private void ParseCell(object sender, DataGridViewCellParsingEventArgs e)
        {
            if (e.ColumnIndex < 0 || grid.Columns[e.ColumnIndex].Name != "PrecioAlegra") return;
            decimal value;
            if (ReadPrice(e.Value, out value)) { e.Value = value; e.ParsingApplied = true; }
        }

        private void PaintAction(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            string columnName = grid.Columns[e.ColumnIndex].Name;
            if (columnName == "SimilaresAlegra")
            {
                e.PaintBackground(e.ClipBounds, true);
                bool draft = ReferenceEquals(grid.Rows[e.RowIndex], editingRow);
                var candidates = draft ? similarProducts : CachedSimilar(grid.Rows[e.RowIndex]);
                if (draft || candidates != null)
                {
                    var badgeBounds = Rectangle.Inflate(e.CellBounds, -9, -9);
                    bool failed = draft && similarSearchFailed;
                    bool available = !working && grid.Enabled && (failed || candidates.Count > 0);
                    using (var path = Theme.RoundedPath(badgeBounds, 8))
                    using (var fill = new SolidBrush(Theme.Lighten(available ? Theme.Info : Theme.Neutral, .90f)))
                        e.Graphics.FillPath(fill, path);
                    TextRenderer.DrawText(e.Graphics, draft && working ? "…" : failed ? "!" : candidates.Count.ToString(),
                        Theme.BodyBold, badgeBounds, available ? Theme.Info : Theme.TextMuted,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }
                e.Handled = true; return;
            }
            if (columnName != "AgregarAlegra") return;
            e.PaintBackground(e.ClipBounds, true);
            var view = grid.Rows[e.RowIndex].DataBoundItem as DataRowView;
            bool linked = view != null && view.Row.Table.Columns.Contains("VinculadoAlegra")
                && !view.Row.IsNull("VinculadoAlegra") && Convert.ToBoolean(view["VinculadoAlegra"]);
            var bounds = Rectangle.Inflate(e.CellBounds, -8, -9);
            var previous = e.Graphics.SmoothingMode;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using (var path = Theme.RoundedPath(bounds, 8))
            using (var fill = new SolidBrush(linked ? Theme.Lighten(Theme.Success, .90f)
                : working || !grid.Enabled ? Theme.Lighten(Theme.Primary, .55f) : Theme.Primary))
                e.Graphics.FillPath(fill, path);
            TextRenderer.DrawText(e.Graphics, ActionText(grid.Rows[e.RowIndex]), Theme.Small, bounds,
                linked ? Theme.Success : Theme.TextOnColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            e.Graphics.SmoothingMode = previous;
            e.Handled = true;
        }

        private async void EnterCell(object sender, DataGridViewCellEventArgs e)
        {
            if (!working && e.RowIndex >= 0 && e.ColumnIndex >= 0 && grid.Columns[e.ColumnIndex].Name == "ImpuestoNuevoAlegra")
                await LoadTaxesAsync();
        }

        public async Task LoadTaxesAsync()
        {
            if (taxes != null) return;
            try
            {
                taxes = await AlegraProductRegistration.TaxesAsync(lifetime.Token);
                if (!grid.IsDisposed) ((DataGridViewComboBoxColumn)grid.Columns["ImpuestoNuevoAlegra"]).DataSource = taxes;
            }
            catch (OperationCanceledException) { }
            catch (Exception ex) { status("No se cargaron los impuestos de Alegra: " + ex.Message); }
        }

        private void CommitSelection(object sender, EventArgs e)
        {
            if (grid.IsCurrentCellDirty && grid.CurrentCell is DataGridViewComboBoxCell)
                grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        private void DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            if (e.ColumnIndex >= 0 && grid.Columns[e.ColumnIndex].Name == "ImpuestoNuevoAlegra")
            { e.ThrowException = false; status("Selecciona un impuesto válido de Alegra."); }
        }

        private void Format(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (grid.Columns[e.ColumnIndex].Name == "SimilaresAlegra")
            {
                e.Value = ReferenceEquals(grid.Rows[e.RowIndex], editingRow)
                    ? (working ? "…" : similarSearchFailed ? "!" : similarProducts.Count.ToString())
                    : CachedSimilar(grid.Rows[e.RowIndex]) == null ? "" : CachedSimilar(grid.Rows[e.RowIndex]).Count.ToString();
                e.FormattingApplied = true; return;
            }
            if (grid.Columns[e.ColumnIndex].Name != "AgregarAlegra") return;
            var view = grid.Rows[e.RowIndex].DataBoundItem as DataRowView;
            bool linked = view != null && view.Row.Table.Columns.Contains("VinculadoAlegra")
                && !view.Row.IsNull("VinculadoAlegra") && Convert.ToBoolean(view["VinculadoAlegra"]);
            e.Value = ActionText(grid.Rows[e.RowIndex]); e.FormattingApplied = true;
            e.CellStyle.ForeColor = linked ? Theme.Success : Theme.Primary;
            e.CellStyle.SelectionForeColor = e.CellStyle.ForeColor;
        }

        private async void Click(object sender, DataGridViewCellEventArgs e)
        {
            if (working || !grid.Enabled || e.RowIndex < 0 || e.ColumnIndex < 0) return;
            string action = grid.Columns[e.ColumnIndex].Name;
            if (action == "SimilaresAlegra")
            {
                var selectedRow = grid.Rows[e.RowIndex];
                if (!ReferenceEquals(selectedRow, editingRow))
                {
                    var cached = CachedSimilar(selectedRow);
                    if (cached == null || cached.Count == 0 || IsLinked(selectedRow)) return;
                    ResetDraft(true);
                    PrepareDraft(selectedRow);
                    similarProducts = cached;
                }
                if (similarSearchFailed) { await SearchSimilar(); return; }
                if (similarProducts.Count == 0) return;
                using (var dialog = new FrmAlegraSimilarProducts(Convert.ToString(((DataRowView)editingRow.DataBoundItem)["Nombre"]), similarProducts, IsLinked(editingRow)))
                {
                    if (dialog.ShowDialog(grid.FindForm()) != DialogResult.OK) return;
                    SelectRelatedProduct(dialog.SelectedProduct);
                    status("Seleccionado: " + relatedProduct.Name + ". Pulsa " + ActionText(editingRow) + " para confirmar el vínculo.");
                    grid.Invalidate();
                }
                return;
            }
            if (action != "AgregarAlegra") return;
            var row = grid.Rows[e.RowIndex]; var view = row.DataBoundItem as DataRowView;
            if (view == null) return;
            if (IsLinked(row) && !ReferenceEquals(row, editingRow))
            {
                await ChooseReplacement(row);
                return;
            }
            if (!ReferenceEquals(row, editingRow))
            {
                ResetDraft(true);
                PrepareDraft(row);
                working = true; operationText = "Cargando…"; setBusy(true); grid.Invalidate();
                try
                {
                    await LoadTaxesAsync();
                    operationText = "Buscando similares…"; grid.Invalidate();
                    await FindSimilarForDraft();
                    var exact = view.Row.Table.Columns.Contains("ExactProduct") ? view["ExactProduct"] as AlegraSimilarProduct : null;
                    if (exact != null) SelectRelatedProduct(exact);
                }
                catch (Exception ex) { similarSearchFailed = true; status("No se pudo completar la búsqueda de similares: " + ex.Message); }
                finally { working = false; setBusy(false); grid.Invalidate(); }
                if (!similarSearchFailed)
                    status(relatedProduct != null ? "Producto existente: " + relatedProduct.Name + ". Pulsa Guardar Relacionado para confirmar el vínculo."
                        : view.Row.Table.Columns.Contains("ExisteAlegra") && Convert.ToString(view["ExisteAlegra"]) == "Sí"
                        ? "El producto existe en Alegra. Selecciona el correcto desde Similares para confirmar su vínculo."
                        : similarProducts.Count + " productos similares encontrados. Usa el botón de cantidad para seleccionarlos, o edita precio e impuesto para crear uno nuevo.");
                grid.CurrentCell = row.Cells["PrecioAlegra"]; grid.BeginEdit(true);
                return;
            }
            string kind = Convert.ToString(view["Tipo"]), name = Convert.ToString(view["Nombre"]);
            int id = Convert.ToInt32(view["Id"]);
            if (similarSearchFailed)
            { status("Completa la búsqueda de similares antes de guardar. Pulsa el botón ! para reintentar."); return; }
            if (relatedProduct == null && view.Row.Table.Columns.Contains("ExisteAlegra") && Convert.ToString(view["ExisteAlegra"]) == "Sí")
            { status("El producto ya existe en Alegra. Selecciona el correcto desde Similares para vincularlo."); return; }
            decimal price = 0;
            if (!grid.EndEdit() || (relatedProduct == null && (!ReadPrice(row.Cells["PrecioAlegra"].Value, out price) || price <= 0 || decimal.Round(price, 6) != price)))
            { status("Escribe un Precio Alegra mayor que cero, con hasta seis decimales."); return; }
            string selectedTaxId = row.Cells["ImpuestoNuevoAlegra"].Value as string;
            if (relatedProduct == null && selectedTaxId == null)
            { status("Selecciona un impuesto en la fila, o Sin impuesto (0%), antes de guardar."); return; }
            working = true; operationText = "Guardando…"; setBusy(true); grid.Enabled = false; grid.Invalidate();
            try
            {
                await LoadTaxesAsync();
                string taxId = row.Cells["ImpuestoNuevoAlegra"].Value as string;
                var selected = taxes == null || taxId == null ? null : taxes.FirstOrDefault(tax => tax.Id == taxId);
                var related = relatedProduct;
                status("Agregando o vinculando " + name + " con Alegra…");
                var result = await Task.Run(() => AlegraProductRegistration.RegisterConfirmedAsync(kind, id, name, price, selected,
                    lifetime.Token, related == null ? null : related.Id, related == null ? null : related.CompanyId));
                if (grid.IsDisposed) return;
                ResetDraft(false);
                ApplyConfirmedProduct(row, result.Product);
                status(name + ": " + result.Message + ".");
                // El catalogo recibe solo la fila confirmada; no vuelve a consultar ni validar Alegra.
                productUpdated?.Invoke(view.Row);
                setBusy(false);
                await refresh();
            }
            catch (OperationCanceledException) { status("Operación cancelada. Vuelve a consultar el producto antes de agregarlo otra vez."); }
            catch (Exception ex) { status(ex.Message); }
            finally { working = false; if (!grid.IsDisposed) { grid.Enabled = true; setBusy(false); grid.Invalidate(); } }
        }

        private void ApplyConfirmedProduct(DataGridViewRow row, AlegraSimilarProduct product)
        {
            var view = (DataRowView)row.DataBoundItem;
            var values = new Dictionary<string, object>
            {
                { "VinculadoAlegra", true }, { "ExisteAlegra", "S\u00ed" },
                { "PrecioAlegra", (object)product.Price ?? DBNull.Value },
                { "ImpuestoAlegra", (object)product.TaxPercentage ?? DBNull.Value },
                { "ExactProduct", product },
                { "SimilarProducts", new List<AlegraSimilarProduct> { product } },
                { "DetalleAlegra", "Alegra #" + product.Id + " \u00b7 " + product.Name
                    + Environment.NewLine + "Producto confirmado y vinculado con Alegra." },
            };
            view.Row.BeginEdit();
            try
            {
                foreach (var entry in values)
                {
                    if (view.Row.Table.Columns.Contains(entry.Key)) view[entry.Key] = entry.Value;
                    else if (grid.Columns.Contains(entry.Key)) row.Cells[entry.Key].Value = entry.Value;
                }
                view.Row.EndEdit();
            }
            catch { view.Row.CancelEdit(); throw; }
            if (row.Index >= 0) grid.InvalidateRow(row.Index);
        }

        private async Task ChooseReplacement(DataGridViewRow row)
        {
            var view = (DataRowView)row.DataBoundItem;
            string name = Convert.ToString(view["Nombre"]);
            ResetDraft(true);
            working = true; setBusy(true); grid.Invalidate();
            status("Consultando productos de Alegra para re-vincular " + name + "…");
            List<AlegraSimilarProduct> candidates = null;
            try
            {
                candidates = await Task.Run(() => AlegraSimilarProducts.FindAsync(name, lifetime.Token));
            }
            catch (OperationCanceledException) { status("Consulta cancelada. Se conserva el vínculo actual."); }
            catch (Exception ex) { status("No se pudo consultar Alegra. Se conserva el vínculo actual. " + ex.Message); }
            finally { working = false; if (!grid.IsDisposed) { setBusy(false); grid.Invalidate(); } }
            if (grid.IsDisposed || candidates == null) return;
            using (var dialog = new FrmAlegraSimilarProducts(name, candidates, true))
            {
                if (dialog.ShowDialog(grid.FindForm()) != DialogResult.OK)
                { status("Se conserva el vínculo actual de " + name + "."); return; }
                PrepareDraft(row);
                similarProducts = candidates;
                grid.Columns["SimilaresAlegra"].Visible = true;
                SelectRelatedProduct(dialog.SelectedProduct);
                status("Seleccionado: " + relatedProduct.Name + ". Pulsa Guardar re-vínculo para reemplazar el vínculo actual.");
                grid.Invalidate();
            }
        }

        private void PrepareDraft(DataGridViewRow row)
        {
            var view = (DataRowView)row.DataBoundItem;
            editingRow = row; previousPrice = row.Cells["PrecioAlegra"].Value;
            previousTax = grid.Columns.Contains("ImpuestoAlegra") ? row.Cells["ImpuestoAlegra"].Value : null;
            if (previousPrice == null || previousPrice == DBNull.Value) row.Cells["PrecioAlegra"].Value = view["Precio"];
            grid.Columns["ImpuestoNuevoAlegra"].Visible = true;
            grid.Columns["PrecioAlegra"].ReadOnly = false;
            foreach (DataGridViewRow other in grid.Rows)
            {
                bool selected = ReferenceEquals(other, row);
                other.Cells["PrecioAlegra"].ReadOnly = !selected;
                other.Cells["ImpuestoNuevoAlegra"].ReadOnly = !selected;
                ((DataGridViewComboBoxCell)other.Cells["ImpuestoNuevoAlegra"]).DisplayStyle = selected
                    ? DataGridViewComboBoxDisplayStyle.DropDownButton : DataGridViewComboBoxDisplayStyle.Nothing;
            }
            row.Cells["PrecioAlegra"].Style = new DataGridViewCellStyle { Format = "0.######",
                FormatProvider = CultureInfo.GetCultureInfo("es-CO"), BackColor = Theme.Lighten(Theme.Primary, .94f) };
        }

        private void SelectRelatedProduct(AlegraSimilarProduct product)
        {
            grid.CancelEdit(); relatedProduct = product;
            grid.Columns["ProductoRelacionadoAlegra"].Visible = true;
            editingRow.Cells["ProductoRelacionadoAlegra"].Value = product.Name + " (#" + product.Id + ")";
            editingRow.Cells["ProductoRelacionadoAlegra"].ToolTipText = product.Name;
            editingRow.Cells["PrecioAlegra"].Value = (object)product.Price ?? DBNull.Value;
            editingRow.Cells["PrecioAlegra"].ReadOnly = true;
            editingRow.Cells["PrecioAlegra"].ToolTipText = "Precio del producto existente en Alegra. Al relacionar se conservan su precio e impuestos.";
            grid.Columns["ImpuestoNuevoAlegra"].Visible = false;
            if (grid.Columns.Contains("ImpuestoAlegra"))
                editingRow.Cells["ImpuestoAlegra"].Value = (object)product.TaxPercentage ?? DBNull.Value;
        }

        private async Task FindSimilarForDraft()
        {
            string name = Convert.ToString(((DataRowView)editingRow.DataBoundItem)["Nombre"]);
            grid.Columns["SimilaresAlegra"].Visible = true;
            status("Buscando productos con nombres similares en Alegra. Espera un momento…");
            similarProducts = CachedSimilar(editingRow)
                ?? await Task.Run(() => AlegraSimilarProducts.FindAsync(name, lifetime.Token));
            similarSearchFailed = false;
            editingRow.Cells["SimilaresAlegra"].ToolTipText = similarProducts.Count + " productos similares. Pulsa para verlos y seleccionar uno.";
        }

        private async Task SearchSimilar()
        {
            working = true; operationText = "Buscando similares…"; setBusy(true); grid.Invalidate();
            try { await FindSimilarForDraft(); status(similarProducts.Count + " productos similares encontrados."); }
            catch (Exception ex) { similarSearchFailed = true; status("No se pudo buscar: " + ex.Message); }
            finally { working = false; setBusy(false); grid.Invalidate(); }
        }

        public void Dispose()
        {
            lifetime.Cancel(); lifetime.Dispose();
            grid.CurrentCellDirtyStateChanged -= CommitSelection;
            grid.CellEnter -= EnterCell;
            grid.CellContentClick -= Click; grid.CellFormatting -= Format; grid.DataError -= DataError;
            grid.CellPainting -= PaintAction;
            grid.CellBeginEdit -= BeginEdit; grid.CellValidating -= ValidateCell; grid.CellParsing -= ParseCell;
            grid.DataSourceChanged -= SourceChanged;
        }
    }
}
