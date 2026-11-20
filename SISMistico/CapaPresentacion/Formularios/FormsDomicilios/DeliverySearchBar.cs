using System;
using System.Drawing;
using System.Windows.Forms;
using CapaPresentacion.UI;
using FontAwesome.Sharp;

namespace CapaPresentacion.Formularios.FormsDomicilios
{
    internal sealed class DeliverySearchBar : TableLayoutPanel
    {
        private readonly ModernNumberPicker copies = new ModernNumberPicker();
        private readonly MinimalIconButton viewButton = new MinimalIconButton
        {
            Icon = IconChar.List, Text = string.Empty, Kind = ButtonKind.Outline,
            Size = new Size(44, 44), CornerRadius = 9, Margin = new Padding(8, 0, 0, 0),
            Anchor = AnchorStyles.None,
        };
        private readonly ToolTip hints = new ToolTip();
        public int Copies => copies.Value;
        public bool CompactView { get; private set; } = true;
        public event EventHandler ViewChanged;

        public DeliverySearchBar(ModernTextBox search)
        {
            Dock = DockStyle.Top;
            Height = 56;
            Padding = new Padding(0, 4, 0, 4);
            Margin = Padding.Empty;
            BackColor = Theme.Surface;
            ColumnCount = 4;
            RowCount = 1;
            ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 48));
            ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 72));
            ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 52));
            RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            search.Dock = DockStyle.Fill;
            search.Margin = new Padding(0, 0, 8, 0);
            Label label = new Label
            {
                Text = "Copias", Dock = DockStyle.Fill, Font = Theme.Small,
                ForeColor = Theme.TextMuted, TextAlign = ContentAlignment.MiddleLeft,
                Margin = Padding.Empty,
            };
            copies.Anchor = AnchorStyles.Left;
            copies.Height = 44;
            copies.Margin = Padding.Empty;
            Controls.Add(search, 0, 0);
            Controls.Add(label, 1, 0);
            Controls.Add(copies, 2, 0);
            Controls.Add(viewButton, 3, 0);
            UpdateViewButton();
            viewButton.Click += (s, e) =>
            {
                CompactView = !CompactView;
                UpdateViewButton();
                ViewChanged?.Invoke(this, EventArgs.Empty);
            };
        }

        private void UpdateViewButton()
        {
            viewButton.Icon = CompactView ? IconChar.ThLarge : IconChar.List;
            viewButton.LucideIcon = CompactView ? "layout-grid" : "list";
            viewButton.AccessibleName = CompactView ? "Cambiar a vista de fichas" : "Cambiar a lista compacta";
            viewButton.AccessibleDescription = CompactView ? "Vista actual: lista compacta" : "Vista actual: fichas";
            hints.SetToolTip(viewButton, viewButton.AccessibleName);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) hints.Dispose();
            base.Dispose(disposing);
        }

    }
}
