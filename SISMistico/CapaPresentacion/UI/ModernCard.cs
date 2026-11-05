using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CapaPresentacion.UI
{
    /// <summary>
    /// Panel tipo tarjeta: superficie blanca con esquinas redondeadas, sombra suave y, si se pide, una franja
    /// de color en el borde superior. Los hijos se colocan dentro de <see cref="Control.Padding"/>.
    /// </summary>
    public class ModernCard : Panel
    {
        private const int ShadowDepth = 4;

        private int cornerRadius = Theme.Radius;
        private Color accentColor = Color.Empty;
        private bool shadow = true;

        public ModernCard()
        {
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.ResizeRedraw, true);
            this.BackColor = Theme.Surface;
            // El hueco de la sombra es parte del control; los hijos arrancan despues de el.
            this.Padding = new Padding(ShadowDepth + 10);
            // El margen descuenta el hueco de la sombra para que el borde visible quede alineado con otros controles.
            this.Margin = new Padding(Theme.Gap / 2 - ShadowDepth);
        }

        [DefaultValue(Theme.Radius)]
        public int CornerRadius
        {
            get => this.cornerRadius;
            set { this.cornerRadius = value; this.Invalidate(); }
        }

        /// <summary>Color de la franja superior; vacio = sin franja.</summary>
        public Color AccentColor
        {
            get => this.accentColor;
            set { this.accentColor = value; this.Invalidate(); }
        }

        [DefaultValue(true)]
        public bool Shadow
        {
            get => this.shadow;
            set { this.shadow = value; this.Invalidate(); }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // El fondo real de la tarjeta lo dibuja OnPaint; aqui solo se limpia con el color del contenedor.
            e.Graphics.Clear(this.Parent?.BackColor ?? Theme.Background);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle surface = new Rectangle(ShadowDepth, ShadowDepth,
                this.Width - ShadowDepth * 2 - 1, this.Height - ShadowDepth * 2 - 1);
            if (surface.Width < 4 || surface.Height < 4)
                return;

            if (this.shadow)
                Theme.DrawShadow(g, surface, this.cornerRadius, ShadowDepth);

            using (GraphicsPath path = Theme.RoundedPath(surface, this.cornerRadius))
            using (SolidBrush fill = new SolidBrush(this.BackColor))
            using (Pen border = new Pen(Theme.Border))
            {
                g.FillPath(fill, path);
                g.DrawPath(border, path);

                if (this.accentColor != Color.Empty)
                {
                    // Franja superior recortada con la forma de la tarjeta para respetar las esquinas.
                    GraphicsState state = g.Save();
                    g.SetClip(path);
                    using (SolidBrush accent = new SolidBrush(this.accentColor))
                        g.FillRectangle(accent, surface.X, surface.Y, surface.Width + 1, 5);
                    g.Restore(state);
                }
            }
        }
    }
}
