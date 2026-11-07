using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using FontAwesome.Sharp;

namespace CapaPresentacion.UI
{
    /// <summary>
    /// Indicador grande: degradado del color de acento, circulo con icono a la izquierda, titulo y valor.
    /// Se dibuja entero (sin controles hijos), asi que se puede acoplar y cambiar de tamano sin tocar nada mas.
    /// </summary>
    public class KpiCard : Control
    {
        private string caption = string.Empty;
        private string value = "0";
        private IconChar icon = IconChar.None;
        private Color accent = Theme.Primary;

        public KpiCard()
        {
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
            this.Size = new Size(260, 92);
            this.Margin = new Padding(Theme.Gap / 2);
        }

        public string Caption
        {
            get => this.caption;
            set { this.caption = value; this.Invalidate(); }
        }

        [DefaultValue("0")]
        public string Value
        {
            get => this.value;
            set { this.value = value; this.Invalidate(); }
        }

        [DefaultValue(IconChar.None)]
        public IconChar Icon
        {
            get => this.icon;
            set { this.icon = value; this.Invalidate(); }
        }

        public Color Accent
        {
            get => this.accent;
            set { this.accent = value; this.Invalidate(); }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Theme.ContainerBackground(this));
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            Rectangle bounds = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
            if (bounds.Width < 8 || bounds.Height < 8)
                return;

            using (GraphicsPath path = Theme.RoundedPath(bounds, Theme.Radius))
            using (LinearGradientBrush brush = new LinearGradientBrush(bounds,
                       this.accent, Theme.Darken(this.accent, 0.28f), LinearGradientMode.ForwardDiagonal))
            {
                g.FillPath(brush, path);
            }

            // Circulo translucido con el icono.
            int circle = this.Height - 36;
            Rectangle circleBounds = new Rectangle(18, (this.Height - circle) / 2, circle, circle);
            using (SolidBrush glass = new SolidBrush(Color.FromArgb(55, 255, 255, 255)))
                g.FillEllipse(glass, circleBounds);
            if (this.icon != IconChar.None)
            {
                int iconSize = circle / 2;
                IconRenderer.Draw(this, g, this.icon, Color.White,
                    new Rectangle(circleBounds.X + (circle - iconSize) / 2,
                        circleBounds.Y + (circle - iconSize) / 2, iconSize, iconSize));
            }

            int textX = circleBounds.Right + 16;
            int textWidth = this.Width - textX - 12;
            TextRenderer.DrawText(g, this.value, Theme.Kpi,
                new Rectangle(textX, 6, textWidth, this.Height - 34), Color.White,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            TextRenderer.DrawText(g, this.caption, Theme.BodyBold,
                new Rectangle(textX, this.Height - 32, textWidth, 22), Color.FromArgb(235, 255, 255, 255),
                TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis);
        }
    }
}
