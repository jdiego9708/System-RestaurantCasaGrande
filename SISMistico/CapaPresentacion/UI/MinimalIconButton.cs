using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CapaPresentacion.UI
{
    // Variante optativa: no cambia el estilo de los botones existentes en otras ventanas.
    public class MinimalIconButton : ModernButton
    {
        private bool hover;
        private bool pressed;
        private bool minimalStyle = true;
        private string lucideIcon;
        public bool MinimalStyle
        {
            get => minimalStyle;
            set { if (minimalStyle == value) return; minimalStyle = value; Invalidate(); }
        }
        private Color accentColor = Color.Empty;
        /// <summary>Color de relleno opcional para una accion principal; vacio mantiene el estilo discreto.</summary>
        public Color AccentColor
        {
            get => accentColor;
            set { if (accentColor == value) return; accentColor = value; Invalidate(); }
        }
        public string LucideIcon
        {
            get => lucideIcon;
            set { if (lucideIcon == value) return; lucideIcon = value; Invalidate(); }
        }

        // Levantar el dedo al terminar de desplazar un panel tactil no debe pulsar el boton.
        protected override void OnClick(EventArgs e)
        {
            if (TouchScrollPanel.IsClickSuppressed(this)) return;
            base.OnClick(e);
        }

        protected override void OnMouseEnter(EventArgs e) { hover = !TouchScrollPanel.IsTouchInput; base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { hover = pressed = false; base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e) { pressed = true; base.OnMouseDown(e); }
        protected override void OnMouseUp(MouseEventArgs e) { pressed = false; base.OnMouseUp(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (!MinimalStyle) { base.OnPaint(e); return; }
            Graphics g = e.Graphics;
            g.Clear(Theme.ContainerBackground(this));
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            Color fill = pressed ? Theme.Border : hover || !Enabled ? Theme.SurfaceAlt : Theme.Surface;
            Color fore = Enabled && hover ? Theme.TextPrimary : Theme.TextMuted;
            if (!accentColor.IsEmpty)
            {
                fill = pressed ? Theme.Darken(accentColor, 0.25f) : hover ? Theme.Darken(accentColor, 0.1f) : accentColor;
                fore = Theme.TextOnColor;
            }
            Rectangle bounds = new Rectangle(1, 1, Width - 3, Height - 3);
            using (var path = Theme.RoundedPath(bounds, CornerRadius))
            using (var brush = new SolidBrush(fill))
            using (var border = new Pen(Enabled ? Theme.BorderStrong : Theme.Border, 1f))
            {
                g.FillPath(brush, path);
                g.DrawPath(border, path);
            }
            const int iconSize = 18;
            int textWidth = string.IsNullOrEmpty(Text) ? 0 : TextRenderer.MeasureText(Text, Font).Width;
            int spacing = textWidth > 0 ? 6 : 0;
            int left = (Width - iconSize - spacing - textWidth) / 2;
            LucideIcons.Draw(g, LucideIcon, fore, new Rectangle(left, (Height - iconSize) / 2, iconSize, iconSize));
            if (textWidth > 0)
                TextRenderer.DrawText(g, Text, Font, new Rectangle(left + iconSize + spacing, 0, textWidth, Height), fore,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
            if (Focused && ShowFocusCues)
                ControlPaint.DrawFocusRectangle(g, Rectangle.Inflate(bounds, -4, -4), fore, fill);
        }
    }
}
