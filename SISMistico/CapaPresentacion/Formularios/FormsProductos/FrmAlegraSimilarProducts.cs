using CapaNegocio.Alegra;
using CapaPresentacion.UI;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace CapaPresentacion.Formularios.FormsProductos
{
    internal sealed class FrmAlegraSimilarProducts : Form
    {
        public AlegraSimilarProduct SelectedProduct { get; private set; }

        public FrmAlegraSimilarProducts(string localName, List<AlegraSimilarProduct> products, bool relinking = false)
        {
            Text = relinking ? "RE-VINCULAR ESTE PRODUCTO" : "Productos similares en Alegra";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(1050, 580); MinimumSize = new Size(850, 480);
            BackColor = Theme.Background; Font = Theme.Body; AutoScaleMode = AutoScaleMode.Font;
            MinimizeBox = false; ShowInTaskbar = false; FrmProductos.ApplyWindowIcon(this);
            var root = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(24), ColumnCount = 1, RowCount = 3 };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 90));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
            var header = new Panel { Dock = DockStyle.Fill };
            header.Controls.Add(new Label { Text = Text, Font = Theme.Title,
                ForeColor = Theme.TextPrimary, AutoSize = true });
            header.Controls.Add(new Label { Text = products.Count + " coincidencias para: " + localName,
                Top = 43, Height = 38, Dock = DockStyle.Bottom, ForeColor = Theme.TextMuted, AutoEllipsis = true });
            root.Controls.Add(header, 0, 0);
            var card = new ModernCard { Dock = DockStyle.Fill, Padding = new Padding(14) };
            var grid = new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = false };
            grid.StyleModernGrid(); grid.AllowUserToDeleteRows = false; grid.RowTemplate.Height = 52;
            grid.ColumnHeadersHeight = 44;
            grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Id", HeaderText = "ID ALEGRA", Width = 95, ReadOnly = true });
            grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Name", HeaderText = "PRODUCTO EN ALEGRA",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, MinimumWidth = 230, ReadOnly = true });
            grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Price", HeaderText = "PRECIO ALEGRA", Width = 165, ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "C2", FormatProvider = CultureInfo.GetCultureInfo("es-CO"),
                    NullValue = "—", Alignment = DataGridViewContentAlignment.MiddleRight } });
            grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "TaxPercentage", HeaderText = "IMPUESTO", Width = 115, ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "0.##'%'", NullValue = "—" } });
            grid.Columns.Add(new DataGridViewButtonColumn { Name = "Select", HeaderText = "ACCIÓN", Text = "Seleccionar producto",
                UseColumnTextForButtonValue = true, Width = 185, FlatStyle = FlatStyle.Flat,
                DefaultCellStyle = new DataGridViewCellStyle { ForeColor = Theme.Primary, Font = Theme.BodyBold } });
            grid.DataSource = products;
            grid.CellPainting += PaintSelectionButton;
            grid.CellMouseEnter += (s, e) => { if (e.RowIndex >= 0) grid.InvalidateRow(e.RowIndex); };
            grid.CellMouseLeave += (s, e) => { if (e.RowIndex >= 0) grid.InvalidateRow(e.RowIndex); };
            grid.CellMouseDown += (s, e) => { if (e.RowIndex >= 0) grid.InvalidateRow(e.RowIndex); };
            grid.CellMouseUp += (s, e) => { if (e.RowIndex >= 0) grid.InvalidateRow(e.RowIndex); };
            grid.CellContentClick += (s, e) =>
            {
                if (e.RowIndex < 0 || e.ColumnIndex < 0 || grid.Columns[e.ColumnIndex].Name != "Select") return;
                SelectedProduct = grid.Rows[e.RowIndex].DataBoundItem as AlegraSimilarProduct;
                if (SelectedProduct != null) { DialogResult = DialogResult.OK; Close(); }
            };
            card.Controls.Add(grid); root.Controls.Add(card, 0, 1);
            var footer = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(0, 14, 0, 0) };
            var cancel = new ModernButton { Text = "Volver sin seleccionar", Kind = ButtonKind.Outline,
                Width = 235, DialogResult = DialogResult.Cancel };
            footer.Controls.Add(cancel); root.Controls.Add(footer, 0, 2); Controls.Add(root); CancelButton = cancel;
        }

        private static void PaintSelectionButton(object sender, DataGridViewCellPaintingEventArgs e)
        {
            var grid = (DataGridView)sender;
            if (e.RowIndex < 0 || e.ColumnIndex < 0 || grid.Columns[e.ColumnIndex].Name != "Select") return;
            e.PaintBackground(e.ClipBounds, true);
            float scale = e.Graphics.DpiX / 96f;
            int inset = (int)(8 * scale);
            int height = System.Math.Min((int)(34 * scale), e.CellBounds.Height - 12);
            var bounds = new Rectangle(e.CellBounds.X + inset,
                e.CellBounds.Y + (e.CellBounds.Height - height) / 2, e.CellBounds.Width - inset * 2, height);
            bool hover = bounds.Contains(grid.PointToClient(Cursor.Position));
            Color fill = !grid.Enabled ? Theme.Lighten(Theme.Primary, .55f)
                : hover ? Theme.Darken(Theme.Primary, Control.MouseButtons == MouseButtons.Left ? .22f : .10f)
                : Theme.Primary;
            var smoothing = e.Graphics.SmoothingMode;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using (var path = Theme.RoundedPath(bounds, (int)(10 * scale)))
            using (var brush = new SolidBrush(fill))
                e.Graphics.FillPath(brush, path);
            e.Graphics.SmoothingMode = smoothing;
            TextRenderer.DrawText(e.Graphics, "Seleccionar producto", Theme.Button, bounds, Theme.TextOnColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            if (grid.Focused && grid.CurrentCell == grid.Rows[e.RowIndex].Cells[e.ColumnIndex])
                ControlPaint.DrawFocusRectangle(e.Graphics, Rectangle.Inflate(bounds, -4, -4), Theme.TextOnColor, fill);
            e.Handled = true;
        }
    }
}
