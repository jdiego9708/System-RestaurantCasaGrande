using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using CapaPresentacion.Formularios.FormsPrincipales;
using CapaPresentacion.UI;
using FontAwesome.Sharp;

namespace CapaPresentacion.Formularios.FormsPedido
{
    public partial class FrmObservarMesas
    {
        private readonly ModernTextBox tableSearch = new ModernTextBox();
        private readonly TableStateIndicator[] tableFilters = new TableStateIndicator[4];
        private readonly Label tableResults = new Label();
        private readonly Label emptyTables = new Label();
        private int selectedTableFilter;
        private bool arrangingTables;

        private void BuildTablesWorkspace()
        {
            SuspendLayout();
            Controls.Clear();
            FormBorderStyle = FormBorderStyle.None;
            BackColor = TableWorkspaceColors.Background;
            DoubleBuffered = true;
            Padding = Padding.Empty;
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3,
                Padding = new Padding(20, 10, 20, 10), Margin = Padding.Empty, BackColor = Color.Transparent,
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            var toolbar = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 7, RowCount = 1,
                Margin = Padding.Empty, Padding = new Padding(0, 2, 0, 8), BackColor = Color.Transparent,
            };
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 104));
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 104));
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 104));
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40));
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            toolbar.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Color[] colors = { TableWorkspaceColors.Ink, TableWorkspaceColors.Available,
                TableWorkspaceColors.Busy, TableWorkspaceColors.Leaving };
            for (int i = 0; i < 4; i++)
            {
                int filter = i;
                tableFilters[i] = new TableStateIndicator
                {
                    Dock = DockStyle.Fill, Margin = new Padding(0, 0, 8, 0), IndicatorColor = colors[i],
                    IndicatorIcon = i == 0 ? IconChar.Table : IconChar.Circle,
                };
                tableFilters[i].Click += (s, e) => { selectedTableFilter = filter; panelMesas.AutoScrollPosition = Point.Empty; UpdateTableWorkspace(); };
                toolbar.Controls.Add(tableFilters[i], i, 0);
            }
            tableSearch.Icon = IconChar.Search;
            tableSearch.PlaceholderText = "Buscar mesa";
            tableSearch.AccessibleName = "Buscar por n?mero de mesa";
            tableSearch.Dock = DockStyle.Fill;
            tableSearch.Margin = new Padding(8, 0, 8, 0);
            tableSearch.TextChanged += (s, e) => { panelMesas.AutoScrollPosition = Point.Empty; UpdateTableWorkspace(); };
            toolbar.Controls.Add(tableSearch, 4, 0);
            StyleTableAction(btnCargar, "");
            ((TableFilterButton)btnCargar).ActionIcon = IconChar.ArrowsRotate;
            btnCargar.AccessibleName = "Actualizar mesas";
            btnCargar.Dock = DockStyle.Fill; btnCargar.Margin = Padding.Empty;
            StyleTableAction(btnDomicilio, "Domicilios");
            btnDomicilio.Width = 150; btnDomicilio.Dock = DockStyle.Fill; btnDomicilio.Margin = new Padding(8, 0, 0, 0);
            toolbar.Controls.Add(btnCargar, 5, 0); toolbar.Controls.Add(btnDomicilio, 6, 0);
            root.Controls.Add(toolbar, 0, 0);
            var content = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty, BackColor = Color.Transparent };
            panelMesas.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            panelMesas.Dock = DockStyle.Fill; panelMesas.Padding = Padding.Empty; panelMesas.BackColor = Color.Transparent;
            panelMesas.AutoScroll = false; panelMesas.Margin = Padding.Empty;
            emptyTables.Text = "No se encontraron mesas.\nRevisa el n\u00famero o cambia el filtro de estado.";
            emptyTables.Dock = DockStyle.Fill; emptyTables.TextAlign = ContentAlignment.MiddleCenter;
            emptyTables.Font = Theme.Body; emptyTables.ForeColor = Theme.TextMuted; emptyTables.BackColor = Color.Transparent;
            content.Controls.Add(panelMesas); content.Controls.Add(emptyTables);
            root.Controls.Add(content, 0, 1);
            var footer = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = Padding.Empty, BackColor = Color.Transparent };
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tableResults.Dock = DockStyle.Fill; tableResults.Font = Theme.Small;
            tableResults.BackColor = Color.Transparent; tableResults.ForeColor = Theme.TextMuted; tableResults.TextAlign = ContentAlignment.MiddleLeft;
            var devices = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.LeftToRight, WrapContents = false,
                Dock = DockStyle.Fill, Margin = Padding.Empty, Padding = new Padding(0, 5, 0, 0), BackColor = Color.Transparent };
            panelDispositivos.Size = new Size(7, 7); panelDispositivos.Margin = new Padding(0, 6, 7, 0);
            lblDispositivos.Font = Theme.Small; lblDispositivos.ForeColor = Theme.TextMuted; lblDispositivos.BackColor = Color.Transparent;
            lblDispositivos.Margin = Padding.Empty;
            devices.Controls.Add(panelDispositivos); devices.Controls.Add(lblDispositivos);
            footer.Controls.Add(tableResults, 0, 0); footer.Controls.Add(devices, 1, 0);
            root.Controls.Add(footer, 0, 2);
            Controls.Add(root);
            panelMesas.SizeChanged += (s, e) => UpdateTableWorkspace();
            UpdateTableWorkspace();
            ResumeLayout(true);
        }

        private Bitmap backdrop;
        private Size backdropParentSize;
        private Point backdropOrigin;

        // Fondo "transparente": wallpaper + velo claro, prerrenderizado una sola vez por tamano.
        // Escalar la imagen completa en cada repintado de cada control hijo hacia que las mesas aparecieran lentamente.
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (AppWallpaper.Image == null || Parent == null || ClientSize.Width <= 0 || ClientSize.Height <= 0)
            { base.OnPaintBackground(e); return; }
            if (backdrop == null || backdrop.Size != ClientSize || backdropParentSize != Parent.ClientSize || backdropOrigin != Location)
            {
                if (backdrop != null) backdrop.Dispose();
                backdrop = new Bitmap(ClientSize.Width, ClientSize.Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                using (var g = Graphics.FromImage(backdrop))
                {
                    g.TranslateTransform(-Left, -Top);
                    AppWallpaper.Draw(g, Parent.ClientRectangle);
                    g.ResetTransform();
                    using (var veil = new SolidBrush(Color.FromArgb(105, TableWorkspaceColors.Background)))
                        g.FillRectangle(veil, 0, 0, backdrop.Width, backdrop.Height);
                }
                backdropParentSize = Parent.ClientSize; backdropOrigin = Location;
            }
            e.Graphics.DrawImageUnscaled(backdrop, 0, 0);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            if (backdrop != null) { backdrop.Dispose(); backdrop = null; }
            base.OnFormClosed(e);
        }

        private static void StyleTableAction(Button button, string caption)
        {
            button.Text = caption; button.Image = button.BackgroundImage = null;
            button.FlatStyle = FlatStyle.Flat; button.FlatAppearance.BorderSize = 0;
            button.Font = Theme.BodyBold; button.ForeColor = TableWorkspaceColors.Ink;
            button.BackColor = Color.White; button.UseVisualStyleBackColor = false;
            button.Cursor = Cursors.Hand; button.TextAlign = ContentAlignment.MiddleCenter;
        }

        private void UpdateTableWorkspace()
        {
            if (arrangingTables || tableFilters[0] == null) return;
            arrangingTables = true;
            panelMesas.SuspendLayout();
            try
            {
                var cards = panelMesas.Controls.OfType<CustomButtonMesa>().OrderBy(card => card.Numero_mesa).ToList();
                int available = cards.Count(card => card.Estado_mesa == "DISPONIBLE");
                int leaving = cards.Count(card => card.Estado_mesa == "SALIENDO");
                int occupied = cards.Count - available - leaving;
                string[] captions = { "Todas", "Libres", "Pendientes", "Saliendo" };
                int[] counts = { cards.Count, available, occupied, leaving };
                for (int i = 0; i < 4; i++) { tableFilters[i].Text = captions[i]; tableFilters[i].Count = counts[i]; tableFilters[i].Selected = selectedTableFilter == i; }
                string query = tableSearch.Text.Trim();
                var visible = cards.Where(card =>
                {
                    bool matches = query.Length == 0 || ("Mesa " + card.Numero_mesa).IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
                    bool state = selectedTableFilter == 0 || selectedTableFilter == 1 && card.Estado_mesa == "DISPONIBLE"
                        || selectedTableFilter == 2 && card.Estado_mesa != "DISPONIBLE" && card.Estado_mesa != "SALIENDO"
                        || selectedTableFilter == 3 && card.Estado_mesa == "SALIENDO";
                    return matches && state;
                }).ToList();
                foreach (var card in cards) card.Visible = visible.Contains(card);
                int shown = visible.Count;
                int width = Math.Max(1, panelMesas.ClientSize.Width);
                int height = Math.Max(1, panelMesas.ClientSize.Height);
                int columns = 1, rows = Math.Max(1, shown), gap = 4;
                double best = double.MinValue;
                // Elegir la distribucion que ofrece la mejor legibilidad dentro del area real del panel.
                for (int candidate = 1; candidate <= Math.Max(1, shown); candidate++)
                {
                    int candidateRows = Math.Max(1, (shown + candidate - 1) / candidate);
                    int candidateGap = Math.Max(1, Math.Min(8, Math.Min(width / candidate, height / candidateRows) / 14));
                    double cellWidth = Math.Max(1, (width - (candidate - 1) * candidateGap) / (double)candidate);
                    double cellHeight = Math.Max(1, (height - (candidateRows - 1) * candidateGap) / (double)candidateRows);
                    double score = Math.Min(cellWidth / 164d, cellHeight / 102d)
                        - .06 * Math.Abs(Math.Log(cellWidth / cellHeight / 1.6));
                    if (score <= best) continue;
                    best = score; columns = candidate; rows = candidateRows; gap = candidateGap;
                }
                int cardWidth = Math.Max(1, Math.Min(220, (width - (columns - 1) * gap) / columns));
                int cardHeight = Math.Max(1, Math.Min(138, (height - (rows - 1) * gap) / rows));
                panelMesas.AutoScrollMinSize = Size.Empty;
                for (int i = 0; i < shown; i++)
                    visible[i].SetBounds(i % columns * (cardWidth + gap), i / columns * (cardHeight + gap), cardWidth, cardHeight);
                emptyTables.Visible = shown == 0;
                if (emptyTables.Visible) emptyTables.BringToFront();
                tableResults.Text = shown + " de " + cards.Count + " mesas \u00b7 Vista del d\u00eda";
            }
            finally { panelMesas.ResumeLayout(true); arrangingTables = false; }
        }
    }

    internal static class TableWorkspaceColors
    {
        public static readonly Color Background = ColorTranslator.FromHtml("#FAF8F3");
        public static readonly Color Ink = ColorTranslator.FromHtml("#493719");
        public static readonly Color Gold = ColorTranslator.FromHtml("#D68C10");
        public static readonly Color Border = ColorTranslator.FromHtml("#E8E0D0");
        public static readonly Color Available = ColorTranslator.FromHtml("#268146");
        public static readonly Color Busy = ColorTranslator.FromHtml("#D0A900");
        public static readonly Color Leaving = ColorTranslator.FromHtml("#7A1636");
    }

    internal sealed class TableStateIndicator : Button
    {
        public Color IndicatorColor { get; set; }
        public IconChar IndicatorIcon { get; set; }
        private int count;
        public int Count { get => count; set { count = value; Invalidate(); } }
        private readonly Font countFont = new Font("Segoe UI", 12f, FontStyle.Bold, GraphicsUnit.Point);
        private bool selected, hovered;
        public bool Selected { get => selected; set { selected = value; Invalidate(); } }
        public TableStateIndicator()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.ResizeRedraw, true);
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold, GraphicsUnit.Point);
            Cursor = Cursors.Hand;
            FlatStyle = FlatStyle.Flat; FlatAppearance.BorderSize = 0;
        }
        protected override void Dispose(bool disposing) { if (disposing) countFont.Dispose(); base.Dispose(disposing); }
        protected override void OnMouseEnter(EventArgs e) { hovered = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { hovered = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnPaintBackground(PaintEventArgs e) { }
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            // El fondo del formulario es transparente: se toma del padre en vez de rellenar con un color fijo.
            ButtonRenderer.DrawParentBackground(g, ClientRectangle, this);
            if (Width < 8 || Height < 8) return;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle bounds = Rectangle.Inflate(ClientRectangle, -2, -2);
            // Color solido y llamativo; el seleccionado lleva aro oscuro y el resto se aclara solo al pasar el raton.
            Color fillColor = hovered && !selected ? Theme.Lighten(IndicatorColor, .12f) : IndicatorColor;
            using (var path = Theme.RoundedPath(bounds, 12))
            using (var fill = new SolidBrush(fillColor))
            {
                g.FillPath(fill, path);
                if (selected)
                {
                    using (var ring = new Pen(Color.White, 3)) g.DrawPath(ring, path);
                    using (var outer = new Pen(TableWorkspaceColors.Ink, 1.5f)) g.DrawPath(outer, Theme.RoundedPath(Rectangle.Inflate(bounds, 2, 2), 14));
                }
            }
            double luminance = 0.299 * fillColor.R + 0.587 * fillColor.G + 0.114 * fillColor.B;
            Color fore = luminance > 150 ? TableWorkspaceColors.Ink : Color.White;
            // Compacto: etiqueta arriba y cantidad grande debajo, ambas centradas.
            int half = Height / 2 - 1;
            var flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding;
            TextRenderer.DrawText(g, Text, Font, new Rectangle(4, 4, Width - 8, half - 2), fore, flags | TextFormatFlags.Bottom);
            TextRenderer.DrawText(g, count.ToString(), countFont, new Rectangle(4, half, Width - 8, Height - half - 4), fore, flags | TextFormatFlags.Top);
            if (Focused && ShowFocusCues) ControlPaint.DrawFocusRectangle(g, Rectangle.Inflate(bounds, -4, -4));
        }
    }

    internal sealed class TableFilterButton : Button
    {
        public IconChar ActionIcon { get; set; }
        private bool selected, hovered;
        public bool Selected { get => selected; set { selected = value; Invalidate(); } }
        public TableFilterButton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Font = Theme.BodyBold; Cursor = Cursors.Hand; FlatStyle = FlatStyle.Flat; FlatAppearance.BorderSize = 0;
        }
        protected override void OnMouseEnter(EventArgs e) { hovered = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { hovered = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnPaint(PaintEventArgs e)
        {
            ButtonRenderer.DrawParentBackground(e.Graphics, ClientRectangle, this); e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            if (Width < 4 || Height < 4) return;
            Rectangle bounds = Rectangle.Inflate(ClientRectangle, -1, -1);
            using (var path = Theme.RoundedPath(bounds, 11))
            using (var fill = new SolidBrush(selected ? Color.FromArgb(255, 244, 214) : hovered ? Color.FromArgb(255, 251, 238) : Color.White))
            using (var border = new Pen(selected ? TableWorkspaceColors.Gold : TableWorkspaceColors.Border))
            { e.Graphics.FillPath(fill, path); e.Graphics.DrawPath(border, path); }
            if (ActionIcon != IconChar.None)
                IconRenderer.Draw(this, e.Graphics, ActionIcon, TableWorkspaceColors.Ink,
                    new Rectangle((Width - 16) / 2, (Height - 16) / 2, 16, 16));
            TextRenderer.DrawText(e.Graphics, Text, Font, Rectangle.Inflate(bounds, -10, 0), TableWorkspaceColors.Ink,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            if (Focused && ShowFocusCues) ControlPaint.DrawFocusRectangle(e.Graphics, Rectangle.Inflate(bounds, -5, -5));
        }
    }
}
