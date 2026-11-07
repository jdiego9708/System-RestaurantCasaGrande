using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CapaPresentacion.UI
{
    /// <summary>Color semantico de un <see cref="Badge"/>.</summary>
    public enum BadgeKind
    {
        Primary,
        Success,
        Warning,
        Danger,
        Info,
        Neutral,
        Rappi,
    }

    /// <summary>
    /// Etiqueta tipo "pildora" con fondo suave y texto del mismo color. Hereda de Label para poder
    /// sustituir etiquetas del Designer cambiando solo el tipo (Text, Click y ToolTip siguen igual).
    /// </summary>
    public class Badge : Label
    {
        private BadgeKind kind = BadgeKind.Primary;

        public Badge()
        {
            this.AutoSize = false;
            this.Height = 26;
            this.Font = Theme.BodyBold;
            this.TextAlign = ContentAlignment.MiddleCenter;
            this.SetStyle(ControlStyles.SupportsTransparentBackColor | ControlStyles.UserPaint
                | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        }

        public BadgeKind Kind
        {
            get => kind;
            set { kind = value; Invalidate(); }
        }

        /// <summary>Color base de un tipo; las tablas lo usan para pintar badges en celdas.</summary>
        public static Color BaseColor(BadgeKind kind)
        {
            switch (kind)
            {
                case BadgeKind.Success: return Theme.Success;
                case BadgeKind.Warning: return Theme.Warning;
                case BadgeKind.Danger: return Theme.Danger;
                case BadgeKind.Info: return Theme.Info;
                case BadgeKind.Neutral: return Theme.Neutral;
                case BadgeKind.Rappi: return Theme.Rappi;
                default: return Theme.Primary;
            }
        }

        /// <summary>Dibuja la pildora centrada en <paramref name="bounds"/>; la comparten el control y las celdas de tabla.</summary>
        public static void Draw(Graphics g, Rectangle bounds, string text, BadgeKind kind, Font font)
        {
            Color baseColor = BaseColor(kind);
            Color fill = Theme.Lighten(baseColor, 0.86f);
            Color ink = Theme.Darken(baseColor, 0.25f);

            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (GraphicsPath path = Theme.RoundedPath(bounds, bounds.Height / 2))
            using (SolidBrush brush = new SolidBrush(fill))
            using (Pen pen = new Pen(Theme.Lighten(baseColor, 0.6f)))
            {
                g.FillPath(brush, path);
                g.DrawPath(pen, path);
            }

            TextRenderer.DrawText(g, text, font, bounds, ink,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
                | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // El fondo se pinta con el del contenedor para que las esquinas redondeadas no dejen cuadros.
            e.Graphics.Clear(Theme.ContainerBackground(this));
            Rectangle bounds = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
            Draw(e.Graphics, bounds, this.Text, this.kind, this.Font);
        }
    }
}
