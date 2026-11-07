using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using FontAwesome.Sharp;

namespace CapaPresentacion.UI
{
    /// <summary>Aspecto del <see cref="ModernButton"/>.</summary>
    public enum ButtonKind
    {
        Primary,
        Success,
        Danger,
        Warning,
        Neutral,
        /// <summary>Fondo transparente con borde; para acciones secundarias.</summary>
        Outline,
    }

    /// <summary>
    /// Boton con esquinas redondeadas, color por tipo, efecto al pasar el mouse / pulsar e icono opcional
    /// (FontAwesome.Sharp). Sustituye al Button plano en los formularios nuevos.
    /// </summary>
    public class ModernButton : Button
    {
        private ButtonKind kind = ButtonKind.Primary;
        private IconChar icon = IconChar.None;
        private int cornerRadius = 10;
        private bool hover;
        private bool pressed;
        private bool shadow;

        [DefaultValue(false)]
        public bool Shadow
        {
            get => this.shadow;
            set { this.shadow = value; this.Invalidate(); }
        }

        public ModernButton()
        {
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.ResizeRedraw, true);
            this.FlatStyle = FlatStyle.Flat;
            this.FlatAppearance.BorderSize = 0;
            this.Cursor = Cursors.Hand;
            this.Font = Theme.Button;
            this.Height = 40;
            this.BackColor = Theme.Background;
        }

        [DefaultValue(ButtonKind.Primary)]
        public ButtonKind Kind
        {
            get => this.kind;
            set { this.kind = value; this.Invalidate(); }
        }

        [DefaultValue(IconChar.None)]
        public IconChar Icon
        {
            get => this.icon;
            set { this.icon = value; this.Invalidate(); }
        }

        [DefaultValue(10)]
        public int CornerRadius
        {
            get => this.cornerRadius;
            set { this.cornerRadius = value; this.Invalidate(); }
        }

        private Color BaseColor()
        {
            switch (this.kind)
            {
                case ButtonKind.Success: return Theme.Success;
                case ButtonKind.Danger: return Theme.Danger;
                case ButtonKind.Warning: return Theme.Warning;
                case ButtonKind.Neutral: return Theme.Neutral;
                case ButtonKind.Outline: return Theme.Surface;
                default: return Theme.Primary;
            }
        }

        private Color ForeColorForKind() => this.kind == ButtonKind.Outline ? Theme.Primary : Theme.TextOnColor;

        protected override void OnMouseEnter(EventArgs e) { this.hover = true; this.Invalidate(); base.OnMouseEnter(e); }

        protected override void OnMouseLeave(EventArgs e)
        {
            this.hover = false;
            this.pressed = false;
            this.Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs mevent) { this.pressed = true; this.Invalidate(); base.OnMouseDown(mevent); }

        protected override void OnMouseUp(MouseEventArgs mevent) { this.pressed = false; this.Invalidate(); base.OnMouseUp(mevent); }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Theme.ContainerBackground(this));
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            Color fill = this.BaseColor();
            Color fore = this.ForeColorForKind();
            if (!this.Enabled)
            {
                fill = this.kind == ButtonKind.Outline ? Theme.SurfaceAlt : Theme.Lighten(fill, 0.55f);
                fore = this.kind == ButtonKind.Outline ? Theme.TextMuted : Color.White;
            }
            else if (this.pressed)
                fill = this.kind == ButtonKind.Outline ? Theme.Border : Theme.Darken(fill, 0.22f);
            else if (this.hover)
                fill = this.kind == ButtonKind.Outline ? Theme.SurfaceAlt : Theme.Darken(fill, 0.10f);

            Rectangle bounds = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
            if (this.shadow)
            {
                bounds.Inflate(-5, -5);
                Theme.DrawShadow(g, bounds, this.cornerRadius, this.pressed ? 2 : 4);
            }
            using (GraphicsPath path = Theme.RoundedPath(bounds, this.cornerRadius))
            using (SolidBrush brush = new SolidBrush(fill))
            {
                g.FillPath(brush, path);
                if (this.kind == ButtonKind.Outline)
                {
                    using (Pen pen = new Pen(this.Enabled ? Theme.Primary : Theme.Border, 1.5f))
                        g.DrawPath(pen, path);
                }
            }

            if (this.Focused && this.ShowFocusCues)
            {
                Rectangle focus = Rectangle.Inflate(bounds, -4, -4);
                ControlPaint.DrawFocusRectangle(g, focus, fore, fill);
            }

            // Icono + texto centrados como un solo bloque.
            const int iconSize = 18;
            const int spacing = 8;
            bool hasIcon = this.icon != IconChar.None;
            Size textSize = TextRenderer.MeasureText(g, this.Text, this.Font);
            int contentWidth = textSize.Width + (hasIcon ? iconSize + (this.Text.Length > 0 ? spacing : 0) : 0);
            int x = (this.Width - contentWidth) / 2;

            if (hasIcon)
            {
                IconRenderer.Draw(this, g, this.icon, fore,
                    new Rectangle(x, (this.Height - iconSize) / 2, iconSize, iconSize));
                x += iconSize + spacing;
            }
            TextRenderer.DrawText(g, this.Text, this.Font,
                new Rectangle(x, 0, textSize.Width + 2, this.Height), fore,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.NoPadding);
        }
    }
}
