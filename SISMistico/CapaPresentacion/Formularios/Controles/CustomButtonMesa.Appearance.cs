using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using CapaPresentacion.Formularios.FormsPedido;
using CapaPresentacion.UI;

namespace CapaPresentacion.Formularios
{
    internal sealed class TableCardButton : Button
    {
        private readonly CustomButtonMesa table;
        private readonly ToolTip detailsTip = new ToolTip { InitialDelay = 300, ReshowDelay = 100, AutoPopDelay = 15000 };
        private Font numberFont, amountFont, detailFont;
        private float fontScale = -1;
        private bool hovered, pressed;

        public TableCardButton(CustomButtonMesa table)
        {
            this.table = table;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.ResizeRedraw | ControlStyles.Opaque, true);
            FlatStyle = FlatStyle.Flat; FlatAppearance.BorderSize = 0;
            UseVisualStyleBackColor = false; Cursor = Cursors.Hand;
            AccessibleRole = AccessibleRole.PushButton;
        }
        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            detailsTip?.SetToolTip(this, Text);
            AccessibleName = Text;
            Invalidate();
        }
        protected override void OnMouseEnter(EventArgs e) { hovered = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { hovered = pressed = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e) { pressed = e.Button == MouseButtons.Left; Invalidate(); base.OnMouseDown(e); }
        protected override void OnMouseUp(MouseEventArgs e) { pressed = false; Invalidate(); base.OnMouseUp(e); }
        protected override void OnMouseCaptureChanged(EventArgs e) { pressed = false; Invalidate(); base.OnMouseCaptureChanged(e); }
        protected override void OnPaintBackground(PaintEventArgs e) { }

        private void UpdateFonts()
        {
            float scale = (float)Math.Max(.55, Math.Min(1.12, Math.Min(Width / 164d, Height / 102d)));
            scale = (float)Math.Round(scale, 2);
            if (fontScale == scale) return;
            numberFont?.Dispose(); amountFont?.Dispose(); detailFont?.Dispose();
            numberFont = new Font("Georgia", Math.Max(11, 20 * scale), FontStyle.Bold, GraphicsUnit.Pixel);
            amountFont = new Font("Segoe UI", Math.Max(9, 14 * scale), FontStyle.Bold, GraphicsUnit.Pixel);
            detailFont = new Font("Segoe UI", Math.Max(8, 11 * scale), FontStyle.Regular, GraphicsUnit.Pixel);
            fontScale = scale;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            ButtonRenderer.DrawParentBackground(g, ClientRectangle, this); // transparente: toma el fondo del padre (wallpaper)
            if (Width < 6 || Height < 6) return;
            UpdateFonts();
            g.SmoothingMode = SmoothingMode.AntiAlias;
            bool free = string.Equals(table.Estado_mesa, "DISPONIBLE", StringComparison.OrdinalIgnoreCase);
            bool leaving = string.Equals(table.Estado_mesa, "SALIENDO", StringComparison.OrdinalIgnoreCase);
            Color accent = leaving ? Color.White : free ? Color.FromArgb(28, 95, 51) : Color.FromArgb(101, 77, 0);
            Color cardColor = free ? Color.FromArgb(191, 232, 200) : leaving ? Color.FromArgb(122, 22, 54) : Color.FromArgb(255, 235, 59);
            Color ink = leaving ? Color.White : TableWorkspaceColors.Ink;
            Color muted = leaving ? Color.FromArgb(245, 219, 227) : Color.FromArgb(83, 80, 57);
            string state = free ? "Libre" : leaving ? "Saliendo" : "Pendiente";
            Rectangle surface = Rectangle.Inflate(ClientRectangle, -1, -1);
            using (var path = Theme.RoundedPath(surface, Math.Min(11, Math.Max(3, Height / 9))))
            using (var fill = new SolidBrush(pressed ? Theme.Darken(cardColor, .06f) : hovered ? Theme.Lighten(cardColor, .05f) : cardColor))
            using (var border = new Pen(Theme.Darken(cardColor, hovered ? .28f : .13f), hovered ? 1.5f : 1f))
            { g.FillPath(fill, path); g.DrawPath(border, path); }
            int pad = Math.Max(5, Math.Min(12, Width / 16));
            int line = Math.Max(14, (int)Math.Ceiling(numberFont.GetHeight(g)) + 2);
            int top = Math.Max(4, Math.Min(10, Height / 10));
            bool inlineState = Width >= 145 && Height >= 64;
            int stateWidth = inlineState ? Math.Min(72, TextRenderer.MeasureText(g, state, detailFont).Width + 12) : 0;
            DrawLine(g, "Mesa " + table.Numero_mesa.ToString("00"), numberFont, ink,
                new Rectangle(pad, top, Math.Max(1, Width - pad * 2 - stateWidth), line));
            if (inlineState)
            {
                Rectangle badge = new Rectangle(Width - pad - stateWidth, top + 2, stateWidth, Math.Max(15, line - 4));
                using (var path = Theme.RoundedPath(badge, 6))
                using (var fill = new SolidBrush(Color.FromArgb(leaving ? 32 : 125, Color.White))) g.FillPath(fill, path);
                TextRenderer.DrawText(g, state, detailFont, badge, accent,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
            }
            string[] lines = (table.Details ?? "").Split(new[] { (char)13, (char)10 }, StringSplitOptions.RemoveEmptyEntries);
            string amount = lines.FirstOrDefault(item => item.TrimStart().StartsWith("$", StringComparison.Ordinal)) ?? "Pedido abierto";
            int y = top + line + 3;
            int smallLine = Math.Max(13, (int)Math.Ceiling(detailFont.GetHeight(g)) + 1);
            if (!inlineState && y + smallLine < Height - 3)
            {
                DrawLine(g, state, detailFont, accent, new Rectangle(pad, y, Width - pad * 2, smallLine));
                y += smallLine;
            }
            int moneyLine = Math.Max(15, (int)Math.Ceiling(amountFont.GetHeight(g)) + 2);
            if (y + moneyLine <= Height - 4)
            {
                DrawLine(g, free ? "Disponible" : amount, amountFont, ink,
                    new Rectangle(pad, y, Width - pad * 2, moneyLine));
                y += moneyLine + 3;
            }
            if (!free)
            {
                var details = lines.Skip(1).Where(item => !item.TrimStart().StartsWith("$", StringComparison.Ordinal)).ToArray();
                foreach (string detail in details)
                {
                    if (y + smallLine > Height - 5) break;
                    DrawLine(g, detail, detailFont, muted, new Rectangle(pad, y, Width - pad * 2, smallLine));
                    y += smallLine;
                }
            }
            else if (y + smallLine <= Height - 5)
                DrawLine(g, "Abrir pedido", detailFont, accent, new Rectangle(pad, y, Width - pad * 2, smallLine));
            if (Focused && ShowFocusCues) ControlPaint.DrawFocusRectangle(g, Rectangle.Inflate(surface, -4, -4));
        }
        private static void DrawLine(Graphics graphics, string text, Font font, Color color, Rectangle bounds)
        {
            if (bounds.Width <= 0 || bounds.Height <= 0) return;
            TextRenderer.DrawText(graphics, text, font, bounds, color,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing) { numberFont?.Dispose(); amountFont?.Dispose(); detailFont?.Dispose(); detailsTip.Dispose(); }
            base.Dispose(disposing);
        }
    }
}
