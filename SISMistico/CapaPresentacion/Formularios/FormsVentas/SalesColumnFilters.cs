using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CapaPresentacion.UI;

namespace CapaPresentacion.Formularios.FormsVentas
{
    internal sealed class SalesColumnFilterState
    {
        public readonly Dictionary<string, HashSet<string>> Selected = new Dictionary<string, HashSet<string>>();
        public static readonly StringComparer Comparer = StringComparer.CurrentCultureIgnoreCase;

        public static string ColumnName(string name) => "[" + name.Replace("\\", "\\\\").Replace("]", "\\]") + "]";

        public static List<string> Values(DataTable table, string column)
        {
            return table.Rows.Cast<DataRow>().Where(row => row.RowState != DataRowState.Deleted)
                .Select(row => Convert.ToString(row[column], CultureInfo.CurrentCulture))
                .Distinct(Comparer).OrderBy(value => value, Comparer).ToList();
        }

        public string Expression()
        {
            return string.Join(" AND ", Selected.Select(filter => filter.Value.Count == 0 ? "1 = 0"
                : "ISNULL(CONVERT(" + ColumnName(filter.Key) + ", 'System.String'), '') IN ("
                    + string.Join(",", filter.Value.Select(value => "'" + value.Replace("'", "''") + "'")) + ")"));
        }
    }

    internal sealed class SalesColumnFilters : IDisposable
    {
        private readonly DataGridView grid;
        private readonly Action changed;
        private readonly SalesColumnFilterState state = new SalesColumnFilterState();
        private DataTable source;
        private ToolStripDropDown popup;
        public int ActiveCount => state.Selected.Count;

        public SalesColumnFilters(DataGridView grid, Action changed)
        {
            this.grid = grid;
            this.changed = changed;
            grid.ColumnHeaderMouseClick += OpenFilter;
            grid.CellPainting += PaintHeader;
            grid.DataSourceChanged += SourceChanged;
            grid.DataBindingComplete += BindingComplete;
            grid.Resize += RoundGrid;
            grid.Disposed += GridDisposed;
            RoundGrid(grid, EventArgs.Empty);
        }

        private void SourceChanged(object sender, EventArgs e)
        {
            DataTable next = grid.DataSource as DataTable;
            if (ReferenceEquals(source, next)) return;
            source = next;
            state.Selected.Clear();
            if (popup != null) { popup.Dispose(); popup = null; }
            ConfigureColumns();
        }

        private void BindingComplete(object sender, DataGridViewBindingCompleteEventArgs e) => ConfigureColumns();

        public void ConfigureColumns()
        {
            SourceChangedIfNeeded();
            foreach (DataGridViewColumn column in grid.Columns)
            {
                if (!IsTextColumn(column)) continue;
                column.SortMode = DataGridViewColumnSortMode.Programmatic;
                column.HeaderCell.Style.Padding = new Padding(10, 0, 30, 0);
                column.HeaderCell.ToolTipText = state.Selected.ContainsKey(column.DataPropertyName)
                    ? "Filtro activo. Haz clic para modificarlo o quitarlo."
                    : "Haz clic para buscar y filtrar los valores de esta columna.";
            }
            grid.Invalidate();
        }

        private void SourceChangedIfNeeded()
        {
            if (!ReferenceEquals(source, grid.DataSource as DataTable)) SourceChanged(grid, EventArgs.Empty);
        }

        private bool IsTextColumn(DataGridViewColumn column)
        {
            string name = column.DataPropertyName;
            return source != null && column.Visible && source.Columns.Contains(name)
                && source.Columns[name].DataType == typeof(string)
                && name != "Hora_venta" && name != "Fecha_venta";
        }

        private void OpenFilter(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex < 0 || e.Button != MouseButtons.Left) return;
            DataGridViewColumn column = grid.Columns[e.ColumnIndex];
            if (!IsTextColumn(column)) return;
            if (popup != null) popup.Dispose();
            string key = column.DataPropertyName;
            List<string> options = SalesColumnFilterState.Values(source, key);
            HashSet<string> selected;
            if (!state.Selected.TryGetValue(key, out selected)) selected = new HashSet<string>(options, SalesColumnFilterState.Comparer);
            ColumnFilterPanel panel = new ColumnFilterPanel(column.HeaderText, options, selected);
            ToolStripControlHost host = new ToolStripControlHost(panel)
            {
                AutoSize = false, Size = panel.Size, Margin = Padding.Empty, Padding = Padding.Empty,
            };
            popup = new ToolStripDropDown
            {
                AutoSize = true, Padding = new Padding(1), BackColor = Theme.Surface,
                DropShadowEnabled = true, AutoClose = true,
            };
            popup.Items.Add(host);
            popup.Opened += (s, args) => panel.FocusSearch();
            panel.Apply += (s, args) =>
            {
                if (panel.Selected.SetEquals(options)) state.Selected.Remove(key);
                else state.Selected[key] = new HashSet<string>(panel.Selected, SalesColumnFilterState.Comparer);
                ApplyFilters();
                popup.Close();
            };
            panel.Clear += (s, args) =>
            {
                state.Selected.Remove(key);
                ApplyFilters();
                popup.Close();
            };
            panel.Sort += (s, args) =>
            {
                source.DefaultView.Sort = SalesColumnFilterState.ColumnName(key) + (panel.SortDescending ? " DESC" : " ASC");
                foreach (DataGridViewColumn other in grid.Columns) other.HeaderCell.SortGlyphDirection = SortOrder.None;
                column.HeaderCell.SortGlyphDirection = panel.SortDescending ? SortOrder.Descending : SortOrder.Ascending;
                popup.Close();
            };
            popup.SizeChanged += (s, args) =>
            {
                Region old = popup.Region;
                using (GraphicsPath path = Theme.RoundedPath(new Rectangle(0, 0, popup.Width, popup.Height), 14))
                    popup.Region = new Region(path);
                old?.Dispose();
            };
            Rectangle header = grid.GetCellDisplayRectangle(column.Index, -1, true);
            popup.Show(grid, new Point(Math.Max(0, header.Left), header.Bottom + 4));
        }

        private void ApplyFilters()
        {
            source.DefaultView.RowFilter = state.Expression();
            ConfigureColumns();
            changed();
        }

        private void PaintHeader(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex != -1 || e.ColumnIndex < 0 || !IsTextColumn(grid.Columns[e.ColumnIndex])) return;
            e.Paint(e.ClipBounds, DataGridViewPaintParts.All);
            bool active = state.Selected.ContainsKey(grid.Columns[e.ColumnIndex].DataPropertyName);
            int x = e.CellBounds.Right - 23, y = e.CellBounds.Top + (e.CellBounds.Height - 13) / 2;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            if (active)
                using (SolidBrush tint = new SolidBrush(Theme.Lighten(Theme.Primary, 0.88f)))
                    e.Graphics.FillEllipse(tint, x - 5, y - 5, 25, 25);
            using (Pen pen = new Pen(active ? Theme.Primary : Theme.TextMuted, 1.5f))
            {
                e.Graphics.DrawLines(pen, new[] { new Point(x, y), new Point(x + 14, y), new Point(x + 9, y + 6),
                    new Point(x + 9, y + 12), new Point(x + 5, y + 10), new Point(x + 5, y + 6), new Point(x, y) });
            }
            e.Handled = true;
        }

        private void RoundGrid(object sender, EventArgs e)
        {
            if (grid.Width < 4 || grid.Height < 4) return;
            Region old = grid.Region;
            using (GraphicsPath path = Theme.RoundedPath(new Rectangle(0, 0, grid.Width, grid.Height), 14))
                grid.Region = new Region(path);
            old?.Dispose();
        }

        private void GridDisposed(object sender, EventArgs e) => Dispose();

        public void Dispose()
        {
            grid.ColumnHeaderMouseClick -= OpenFilter;
            grid.CellPainting -= PaintHeader;
            grid.DataSourceChanged -= SourceChanged;
            grid.DataBindingComplete -= BindingComplete;
            grid.Resize -= RoundGrid;
            grid.Disposed -= GridDisposed;
            popup?.Dispose();
            popup = null;
        }
    }

    internal sealed class ColumnFilterPanel : UserControl
    {
        private readonly List<string> options;
        private readonly ModernTextBox search = new ModernTextBox();
        private readonly FilterValueList list = new FilterValueList();
        private readonly Label count = new Label();
        private readonly Label empty = new Label();
        public readonly HashSet<string> Selected;
        public bool SortDescending { get; private set; }
        public event EventHandler Apply;
        public event EventHandler Clear;
        public event EventHandler Sort;

        public ColumnFilterPanel(string title, List<string> options, HashSet<string> selected)
        {
            this.options = options;
            Selected = new HashSet<string>(selected, SalesColumnFilterState.Comparer);
            list.SelectedValues = Selected;
            Size = new Size(332, 458);
            BackColor = Theme.Surface;
            Padding = new Padding(18);
            Label heading = new Label { Text = title, Location = new Point(18, 16), Size = new Size(296, 27), Font = Theme.Heading, ForeColor = Theme.TextPrimary, AutoEllipsis = true };
            search.Location = new Point(18, 52);
            search.Size = new Size(296, 40);
            search.PlaceholderText = "Buscar valores...";
            search.Icon = FontAwesome.Sharp.IconChar.Search;
            search.AccessibleName = "Buscar valores de " + title;
            FlowLayoutPanel tools = new FlowLayoutPanel { Location = new Point(18, 102), Size = new Size(296, 28), WrapContents = false };
            AddLink(tools, "Todos", () => { Selected.UnionWith(options); RefreshSelection(); });
            AddLink(tools, "Ninguno", () => { Selected.Clear(); RefreshSelection(); });
            AddLink(tools, "Solo resultados", () =>
            {
                Selected.Clear();
                Selected.UnionWith(list.Items.Cast<string>());
                RefreshSelection();
            });
            list.Location = new Point(18, 138);
            list.Size = new Size(296, 208);
            list.AccessibleName = "Valores disponibles. Espacio para seleccionar.";
            list.Changed += (s, e) => RefreshSelection();
            empty.Text = "No hay coincidencias";
            empty.Font = Theme.Body;
            empty.ForeColor = Theme.TextMuted;
            empty.TextAlign = ContentAlignment.MiddleCenter;
            empty.Bounds = list.Bounds;
            count.Location = new Point(18, 350);
            count.Size = new Size(296, 24);
            count.Font = Theme.Small;
            count.ForeColor = Theme.TextMuted;
            FlowLayoutPanel sort = new FlowLayoutPanel { Location = new Point(18, 377), Size = new Size(296, 25), WrapContents = false };
            AddLink(sort, "Ordenar A - Z", () => { SortDescending = false; Sort?.Invoke(this, EventArgs.Empty); });
            AddLink(sort, "Z - A", () => { SortDescending = true; Sort?.Invoke(this, EventArgs.Empty); });
            ModernButton clear = new ModernButton { Text = "Quitar filtro", Kind = ButtonKind.Outline, Shadow = true, CornerRadius = 12, Location = new Point(13, 405), Size = new Size(148, 44), Font = Theme.BodyBold };
            ModernButton apply = new ModernButton { Text = "Aplicar", Kind = ButtonKind.Primary, Shadow = true, CornerRadius = 12, Location = new Point(165, 405), Size = new Size(154, 44), Font = Theme.BodyBold };
            clear.Click += (s, e) => Clear?.Invoke(this, EventArgs.Empty);
            apply.Click += (s, e) => Apply?.Invoke(this, EventArgs.Empty);
            search.TextChanged += (s, e) => RefreshOptions();
            search.KeyDown += ApplyOnEnter;
            list.KeyDown += ApplyOnEnter;
            Controls.AddRange(new Control[] { heading, search, tools, list, empty, count, sort, clear, apply });
            RefreshOptions();
        }

        public void FocusSearch() => search.Focus();

        private void ApplyOnEnter(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            e.SuppressKeyPress = true;
            Apply?.Invoke(this, EventArgs.Empty);
        }

        private static void AddLink(FlowLayoutPanel parent, string text, Action click)
        {
            LinkLabel link = new LinkLabel { Text = text, AutoSize = true, Font = Theme.Small, LinkColor = Theme.Primary,
                ActiveLinkColor = Theme.Primary, LinkBehavior = LinkBehavior.HoverUnderline, Margin = new Padding(0, 3, 14, 0) };
            link.LinkClicked += (s, e) => click();
            parent.Controls.Add(link);
        }

        private void RefreshOptions()
        {
            list.BeginUpdate();
            list.Items.Clear();
            foreach (string value in options)
                if (CultureInfo.CurrentCulture.CompareInfo.IndexOf(value.Length == 0 ? "(Sin valor)" : value,
                    search.Text.Trim(), CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0) list.Items.Add(value);
            list.EndUpdate();
            empty.Visible = list.Items.Count == 0;
            list.Visible = !empty.Visible;
            RefreshSelection();
        }

        private void RefreshSelection()
        {
            count.Text = Selected.Count + " de " + options.Count + " seleccionados";
            list.Invalidate();
        }
    }

    internal sealed class FilterValueList : ListBox
    {
        public HashSet<string> SelectedValues;
        public event EventHandler Changed;
        public FilterValueList()
        {
            DrawMode = DrawMode.OwnerDrawFixed;
            ItemHeight = 34;
            IntegralHeight = false;
            BorderStyle = BorderStyle.None;
            Font = Theme.Body;
            BackColor = Theme.Surface;
            ForeColor = Theme.TextPrimary;
        }
        private void Toggle(int index)
        {
            if (index < 0 || index >= Items.Count) return;
            string value = (string)Items[index];
            if (!SelectedValues.Add(value)) SelectedValues.Remove(value);
            Changed?.Invoke(this, EventArgs.Empty);
        }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left) Toggle(IndexFromPoint(e.Location));
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space) { Toggle(SelectedIndex); e.Handled = true; e.SuppressKeyPress = true; }
            base.OnKeyDown(e);
        }
        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            bool check = SelectedValues.Contains((string)Items[e.Index]);
            bool focus = (e.State & DrawItemState.Selected) != 0;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (SolidBrush background = new SolidBrush(focus ? Theme.SurfaceAlt : Theme.Surface)) e.Graphics.FillRectangle(background, e.Bounds);
            Rectangle box = new Rectangle(e.Bounds.X + 4, e.Bounds.Y + 8, 17, 17);
            using (GraphicsPath path = Theme.RoundedPath(box, 5))
            using (SolidBrush fill = new SolidBrush(check ? Theme.Primary : Theme.Surface))
            using (Pen border = new Pen(check ? Theme.Primary : Theme.BorderStrong))
            {
                e.Graphics.FillPath(fill, path);
                e.Graphics.DrawPath(border, path);
            }
            if (check)
                using (Pen tick = new Pen(Color.White, 1.8f)) e.Graphics.DrawLines(tick,
                    new[] { new Point(box.X + 4, box.Y + 8), new Point(box.X + 7, box.Y + 11), new Point(box.X + 13, box.Y + 5) });
            string text = (string)Items[e.Index];
            TextRenderer.DrawText(e.Graphics, text.Length == 0 ? "(Sin valor)" : text, Font,
                new Rectangle(e.Bounds.X + 32, e.Bounds.Y, e.Bounds.Width - 36, e.Bounds.Height), Theme.TextPrimary,
                TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            if (Focused && focus) ControlPaint.DrawFocusRectangle(e.Graphics, Rectangle.Inflate(e.Bounds, -1, -1));
        }
    }
}
