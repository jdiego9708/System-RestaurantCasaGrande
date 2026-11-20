namespace CapaPresentacion.Formularios.FormsPedido
{
    using CapaPresentacion.UI;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Drawing;
    using System.Windows.Forms;

    /// <summary>Datos del pedido que puede mostrar un <see cref="OrderSummaryBar"/>.</summary>
    [Flags]
    public enum SummaryPart
    {
        Date = 1,
        Time = 2,
        Items = 4,
        Subtotal = 8,
    }

    /// <summary>
    /// Resumen del pedido en badges ordenados de izquierda a derecha. Cada instancia muestra las partes que se
    /// le pidan (<see cref="Parts"/>): el panel Informacion usa una para fecha, hora y productos y otra, mas
    /// grande, para el subtotal. Reemplaza al cuadro de texto "Fecha y hora / Subtotal".
    /// </summary>
    public class OrderSummaryBar : Control
    {
        private const int Gap = 6;
        private const int IconSize = 13;
        private const int PaddingX = 10;

        private const TextFormatFlags TextFlags = TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix
            | TextFormatFlags.NoPadding | TextFormatFlags.VerticalCenter | TextFormatFlags.Left;

        private DateTime moment = DateTime.Now;
        private decimal subtotal;
        private int itemCount;
        private SummaryPart parts = SummaryPart.Date | SummaryPart.Time | SummaryPart.Items;

        public OrderSummaryBar()
        {
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.SupportsTransparentBackColor | ControlStyles.ResizeRedraw, true);
            this.BackColor = Color.Transparent;
            this.Size = new Size(300, 28);
        }

        /// <summary>Que badges se dibujan; el orden es siempre fecha, hora, productos y subtotal.</summary>
        public SummaryPart Parts
        {
            get => this.parts;
            set { this.parts = value; this.Invalidate(); }
        }

        /// <summary>Actualiza los datos mostrados.</summary>
        public void SetSummary(DateTime moment, decimal subtotal, int itemCount)
        {
            this.moment = moment;
            this.subtotal = subtotal;
            this.itemCount = itemCount;
            this.Invalidate();
        }

        private static string Money(decimal value) => value.ToString("C").Replace(",00", "");

        private bool fillWidth = true;

        /// <summary>
        /// True (por defecto): los badges se estiran para ocupar todo el ancho del control, repartiendo el sobrante
        /// entre ellos, de modo que la barra siga llena si el panel crece. False: cada badge mide lo que su texto.
        /// </summary>
        public bool FillWidth
        {
            get => this.fillWidth;
            set { this.fillWidth = value; this.Invalidate(); }
        }

        // Un badge por dibujar: su texto, icono, color y fuente.
        private struct Pill
        {
            public string Text, Icon;
            public BadgeKind Kind;
            public Font Font;
            public int Width;
        }

        private List<Pill> BuildPills()
        {
            List<Pill> pills = new List<Pill>();
            string date = this.moment.ToString("ddd d MMM").Replace(".", "");
            string time = this.moment.ToString("hh:mm tt");
            bool hasDate = this.parts.HasFlag(SummaryPart.Date), hasTime = this.parts.HasFlag(SummaryPart.Time);
            // Fecha y hora juntas en un solo badge para ocupar menos ancho.
            if (hasDate && hasTime)
                pills.Add(new Pill { Text = date + " · " + time, Icon = "calendar", Kind = BadgeKind.Neutral, Font = Theme.SmallBold });
            else if (hasDate)
                pills.Add(new Pill { Text = date, Icon = "calendar", Kind = BadgeKind.Neutral, Font = Theme.SmallBold });
            else if (hasTime)
                pills.Add(new Pill { Text = time, Icon = "clock", Kind = BadgeKind.Neutral, Font = Theme.SmallBold });
            if (this.parts.HasFlag(SummaryPart.Items))
                pills.Add(new Pill
                {
                    Text = this.itemCount + (this.itemCount == 1 ? " producto" : " productos"),
                    Icon = "shopping-basket", Kind = BadgeKind.Primary, Font = Theme.SmallBold,
                });
            if (this.parts.HasFlag(SummaryPart.Subtotal))
            {
                string total = "Subtotal  " + Money(this.subtotal);
                // El subtotal es lo que mas se consulta: letra grande, salvo que la cifra no quepa.
                Font font = this.PillWidth(total, Theme.BodyBold) <= this.Width ? Theme.BodyBold : Theme.SmallBold;
                pills.Add(new Pill { Text = total, Icon = "banknote", Kind = BadgeKind.Success, Font = font });
            }

            for (int i = 0; i < pills.Count; i++)
            {
                Pill pill = pills[i];
                pill.Width = this.PillWidth(pill.Text, pill.Font);
                pills[i] = pill;
            }

            // Reparte el ancho sobrante entre los badges para que la barra llene el espacio disponible.
            if (this.fillWidth && pills.Count > 0)
            {
                int used = pills.Sum(p => p.Width) + Gap * (pills.Count - 1);
                int extra = this.Width - 1 - used;
                if (extra > 0)
                    for (int i = 0; i < pills.Count; i++)
                    {
                        Pill pill = pills[i];
                        // El ultimo recibe el resto de la division para que no queden pixeles sueltos.
                        pill.Width += extra / pills.Count + (i == pills.Count - 1 ? extra % pills.Count : 0);
                        pills[i] = pill;
                    }
            }
            return pills;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            // Los badges se pintan sobre el color del contenedor para que las esquinas no dejen cuadros.
            g.Clear(Theme.ContainerBackground(this));

            int x = 0;
            foreach (Pill pill in this.BuildPills())
                x = this.DrawPill(g, x, pill) + Gap;
        }

        private int PillWidth(string text, Font font) =>
            PaddingX + IconSize + 6 + TextRenderer.MeasureText(text, font, Size.Empty, TextFlags).Width + PaddingX;

        // Dibuja un badge con icono y texto que empieza en 'left'; devuelve su borde derecho.
        // Si el badge es mas ancho que su contenido, el icono y el texto se centran dentro de el.
        private int DrawPill(Graphics g, int left, Pill pill)
        {
            Rectangle bounds = new Rectangle(left, 0, pill.Width, this.Height - 1);

            // El fondo y el borde son los del Badge de Domicilios; el texto va aparte para poder anteponer el icono.
            Badge.Draw(g, bounds, string.Empty, pill.Kind, pill.Font);
            Color ink = Theme.Darken(Badge.BaseColor(pill.Kind), 0.25f);
            int textWidth = TextRenderer.MeasureText(pill.Text, pill.Font, Size.Empty, TextFlags).Width;
            int contentWidth = IconSize + 6 + textWidth;
            int start = bounds.X + Math.Max(PaddingX, (bounds.Width - contentWidth) / 2);
            LucideIcons.Draw(g, pill.Icon, ink,
                new Rectangle(start, bounds.Y + (bounds.Height - IconSize) / 2, IconSize, IconSize));
            TextRenderer.DrawText(g, pill.Text, pill.Font,
                new Rectangle(start + IconSize + 6, bounds.Y, textWidth + 2, bounds.Height), ink, TextFlags);
            return bounds.Right;
        }
    }
}
