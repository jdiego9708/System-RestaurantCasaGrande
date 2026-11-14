using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CapaPresentacion.UI
{
    /// <summary>
    /// Cuenta regresiva circular: un anillo que se vacia y el tiempo restante (mm:ss) en el centro. El color
    /// pasa de verde a ambar y de ambar a rojo de forma continua segun lo que queda. No tiene temporizador
    /// propio: quien lo usa asigna <see cref="Remaining"/> (un solo Timer mueve todos los anillos de la pantalla).
    /// </summary>
    public class CountdownRing : Control
    {
        /// <summary>Por debajo de esta fraccion el centro del anillo parpadea para llamar la atencion.</summary>
        private const double UrgentFraction = 0.12;

        private TimeSpan total = TimeSpan.FromMinutes(45);
        private TimeSpan remaining = TimeSpan.FromMinutes(45);

        public CountdownRing()
        {
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            this.SetStyle(ControlStyles.Selectable, false);
            this.Size = new Size(76, 76);
            this.TabStop = false;
        }

        /// <summary>Duracion completa de la cuenta; define que fraccion del anillo queda llena.</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public TimeSpan Total
        {
            get => this.total;
            set { this.total = value; this.Invalidate(); }
        }

        /// <summary>Tiempo que falta; se recorta a [0, <see cref="Total"/>].</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public TimeSpan Remaining
        {
            get => this.remaining;
            set
            {
                if (value < TimeSpan.Zero) value = TimeSpan.Zero;
                if (value > this.total) value = this.total;
                // Solo se repinta cuando cambia el segundo que se ve.
                bool changed = DisplaySeconds(value) != DisplaySeconds(this.remaining);
                this.remaining = value;
                if (changed) this.Invalidate();
            }
        }

        /// <summary>Fraccion que queda: 1 = recien empezado, 0 = agotado.</summary>
        [Browsable(false)]
        public double Fraction => this.total.Ticks <= 0 ? 0 : (double)this.remaining.Ticks / this.total.Ticks;

        /// <summary>Color actual del anillo; la tarjeta que lo contiene lo usa para su franja y su estado.</summary>
        [Browsable(false)]
        public Color CurrentColor => ColorFor(this.Fraction);

        /// <summary>Verde con todo el tiempo, ambar a la mitad y rojo al agotarse, mezclando entre medias.</summary>
        public static Color ColorFor(double fraction)
        {
            fraction = Math.Max(0, Math.Min(1, fraction));
            return fraction >= 0.5
                ? Mix(Theme.Warning, Theme.Success, (fraction - 0.5) * 2)
                : Mix(Theme.Danger, Theme.Warning, fraction * 2);
        }

        private static Color Mix(Color from, Color to, double amount) => Color.FromArgb(
            (int)Math.Round(from.R + (to.R - from.R) * amount),
            (int)Math.Round(from.G + (to.G - from.G) * amount),
            (int)Math.Round(from.B + (to.B - from.B) * amount));

        // Se redondea hacia arriba para que la cuenta arranque en 45:00 y solo marque 0:00 al agotarse.
        private static long DisplaySeconds(TimeSpan value) => (long)Math.Ceiling(value.TotalSeconds);

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Theme.ContainerBackground(this));
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int side = Math.Min(this.Width, this.Height);
            if (side < 24)
                return;

            double fraction = this.Fraction;
            Color color = ColorFor(fraction);
            float stroke = Math.Max(5f, side / 12f);
            RectangleF ring = new RectangleF((this.Width - side) / 2f + stroke / 2 + 1,
                (this.Height - side) / 2f + stroke / 2 + 1, side - stroke - 2, side - stroke - 2);

            long seconds = DisplaySeconds(this.remaining);
            bool urgent = fraction <= UrgentFraction;
            if (urgent && seconds % 2 == 0)
            {
                using (SolidBrush glow = new SolidBrush(Theme.Lighten(color, 0.88f)))
                    g.FillEllipse(glow, ring);
            }

            using (Pen track = new Pen(Theme.Lighten(color, 0.82f), stroke))
                g.DrawEllipse(track, ring);

            float sweep = (float)(360 * fraction);
            if (sweep > 0.5f)
            {
                using (Pen progress = new Pen(color, stroke) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                    g.DrawArc(progress, ring, -90, sweep);
            }

            string time = (seconds / 60) + ":" + (seconds % 60).ToString("00");
            int middle = this.Height / 2;
            TextRenderer.DrawText(g, time, Theme.Heading, new Rectangle(0, middle - 17, this.Width, 24),
                Theme.Darken(color, 0.25f),
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            TextRenderer.DrawText(g, "MIN", Theme.Tiny, new Rectangle(0, middle + 6, this.Width, 12),
                Theme.TextMuted,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        }
    }
}
