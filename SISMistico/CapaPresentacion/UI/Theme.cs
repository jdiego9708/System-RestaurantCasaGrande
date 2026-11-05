using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace CapaPresentacion.UI
{
    /// <summary>
    /// Paleta, tipografia y utilidades de dibujo compartidas por los controles modernos (carpeta UI).
    /// Todo formulario nuevo debe sacar de aqui sus colores y fuentes en vez de escribirlos a mano.
    /// </summary>
    public static class Theme
    {
        #region COLORES
        public static readonly Color Background = Color.FromArgb(243, 245, 249);
        public static readonly Color Surface = Color.White;
        public static readonly Color SurfaceAlt = Color.FromArgb(248, 250, 252);
        public static readonly Color Border = Color.FromArgb(226, 232, 240);
        /// <summary>Borde de los campos de entrada: mas marcado que <see cref="Border"/> para que se vean sobre blanco.</summary>
        public static readonly Color BorderStrong = Color.FromArgb(203, 213, 225);

        public static readonly Color TextPrimary = Color.FromArgb(30, 41, 59);
        public static readonly Color TextMuted = Color.FromArgb(100, 116, 139);
        public static readonly Color TextOnColor = Color.White;

        public static readonly Color Primary = Color.FromArgb(37, 99, 235);
        public static readonly Color Success = Color.FromArgb(22, 163, 74);
        public static readonly Color Warning = Color.FromArgb(245, 158, 11);
        public static readonly Color Danger = Color.FromArgb(220, 38, 38);
        public static readonly Color Info = Color.FromArgb(8, 145, 178);
        public static readonly Color Neutral = Color.FromArgb(71, 85, 105);
        public static readonly Color Rappi = Color.FromArgb(255, 68, 31);
        #endregion

        #region TIPOGRAFIA
        private const string Family = "Segoe UI";

        public static readonly Font Title = new Font(Family, 20F, FontStyle.Bold);
        public static readonly Font Heading = new Font(Family, 12F, FontStyle.Bold);
        public static readonly Font Body = new Font(Family, 10F);
        public static readonly Font BodyBold = new Font(Family, 10F, FontStyle.Bold);
        public static readonly Font Small = new Font(Family, 9F);
        public static readonly Font Kpi = new Font(Family, 30F, FontStyle.Bold);
        public static readonly Font Button = new Font(Family, 10.5F, FontStyle.Bold);
        #endregion

        #region MEDIDAS
        public const int Radius = 12;
        public const int Gap = 12;
        #endregion

        #region UTILIDADES DE COLOR
        /// <summary>Oscurece un color (0 = igual, 1 = negro); se usa para hover y pulsado.</summary>
        public static Color Darken(Color color, float amount)
        {
            amount = Math.Max(0f, Math.Min(1f, amount));
            return Color.FromArgb(color.A,
                (int)(color.R * (1 - amount)), (int)(color.G * (1 - amount)), (int)(color.B * (1 - amount)));
        }

        /// <summary>Aclara un color (0 = igual, 1 = blanco); se usa para fondos suaves y estado deshabilitado.</summary>
        public static Color Lighten(Color color, float amount)
        {
            amount = Math.Max(0f, Math.Min(1f, amount));
            return Color.FromArgb(color.A,
                color.R + (int)((255 - color.R) * amount),
                color.G + (int)((255 - color.G) * amount),
                color.B + (int)((255 - color.B) * amount));
        }
        #endregion

        #region UTILIDADES DE DIBUJO
        /// <summary>Rectangulo con esquinas redondeadas; <paramref name="radius"/> es el radio de cada esquina.</summary>
        public static GraphicsPath RoundedPath(Rectangle bounds, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int d = Math.Max(1, Math.Min(radius * 2, Math.Min(bounds.Width, bounds.Height)));
            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        /// <summary>Sombra suave: varias capas casi transparentes y cada vez mas grandes alrededor de la superficie.</summary>
        public static void DrawShadow(Graphics g, Rectangle surface, int radius, int depth)
        {
            for (int i = depth; i >= 1; i--)
            {
                Rectangle layer = Rectangle.Inflate(surface, i, i);
                layer.Offset(0, 1);
                using (GraphicsPath path = RoundedPath(layer, radius + i))
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(Math.Max(2, 22 / i), 15, 23, 42)))
                {
                    g.FillPath(brush, path);
                }
            }
        }
        #endregion
    }
}
