using System.ComponentModel;
using System.Drawing;
using System.Globalization;
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
        private static readonly Font ValueFont = new Font(Theme.Kpi.FontFamily, 20F, FontStyle.Bold);
        private string caption = string.Empty;
        private string value = "0";
        private IconChar icon = IconChar.None;
        private Color accent = Theme.Primary;
        private string[] paymentTotals;

        public void SetPaymentTotals(decimal? total, decimal? cash, decimal? other)
        {
            CultureInfo culture = CultureInfo.GetCultureInfo("es-CO");
            paymentTotals = new[]
            {
                total.HasValue ? total.Value.ToString("C0", culture) : "\u2014",
                cash.HasValue ? cash.Value.ToString("C0", culture) : "\u2014",
                other.HasValue ? other.Value.ToString("C0", culture) : "\u2014",
            };
            AccessibleDescription = "Total: " + paymentTotals[0] + "; Efectivo: " + paymentTotals[1] + "; Otros medios: " + paymentTotals[2];
            Invalidate();
        }

        public KpiCard()
        {
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
            this.Size = new Size(260, 64);
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

            if (paymentTotals != null)
            {
                DrawPaymentSummary(g);
                return;
            }

            // Circulo translucido con el icono.
            int circle = this.Height - 22;
            Rectangle circleBounds = new Rectangle(14, (this.Height - circle) / 2, circle, circle);
            using (SolidBrush glass = new SolidBrush(Color.FromArgb(55, 255, 255, 255)))
                g.FillEllipse(glass, circleBounds);
            if (this.icon != IconChar.None)
            {
                int iconSize = circle / 2;
                IconRenderer.Draw(this, g, this.icon, Color.White,
                    new Rectangle(circleBounds.X + (circle - iconSize) / 2,
                        circleBounds.Y + (circle - iconSize) / 2, iconSize, iconSize));
            }

            int textX = circleBounds.Right + 12;
            int textWidth = this.Width - textX - 10;
            // Version compacta: valor de 20 pt y titulo pequeno debajo, en unos 60 px de alto.
            TextRenderer.DrawText(g, this.value, ValueFont,
                new Rectangle(textX, 4, textWidth, this.Height - 26), Color.White,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            TextRenderer.DrawText(g, this.caption, Theme.SmallBold,
                new Rectangle(textX, this.Height - 24, textWidth, 18), Color.FromArgb(235, 255, 255, 255),
                TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis);
        }
        private void DrawPaymentSummary(Graphics g)
        {
            const TextFormatFlags flags = TextFormatFlags.Left | TextFormatFlags.VerticalCenter
                | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine;
            int countWidth = TextRenderer.MeasureText(value, Theme.Heading, Size.Empty, flags).Width + 2;
            int titleLeft = 10 + countWidth + 6;
            int captionWidth = TextRenderer.MeasureText(caption, Theme.SmallBold, Size.Empty, flags).Width + 2;
            TextRenderer.DrawText(g, value, Theme.Heading, new Rectangle(10, 0, countWidth, Height), Color.White, flags);
            TextRenderer.DrawText(g, caption, Theme.SmallBold, new Rectangle(titleLeft, 0, captionWidth, Height), Color.White, flags);
            int badgesLeft = titleLeft + captionWidth + 10;
            int badgesWidth = Width - badgesLeft - 8;
            string[] labels = { "Total", "Efectivo", "Otros medios" };
            for (int index = 0; index < labels.Length; index++)
            {
                int left = badgesLeft + index * badgesWidth / 3;
                int right = badgesLeft + (index + 1) * badgesWidth / 3;
                Rectangle badge = new Rectangle(left, (Height - 36) / 2, System.Math.Max(8, right - left - 4), 36);
                using (GraphicsPath path = Theme.RoundedPath(badge, 7))
                using (SolidBrush glass = new SolidBrush(Color.FromArgb(32, Color.White)))
                    g.FillPath(glass, path);
                TextRenderer.DrawText(g, labels[index], Theme.Small, new Rectangle(badge.X + 4, badge.Y + 1, badge.Width - 8, 15), Color.FromArgb(225, Color.White),
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis);
                TextRenderer.DrawText(g, paymentTotals[index], Theme.SmallBold, new Rectangle(badge.X + 3, badge.Y + 16, badge.Width - 6, 18), Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis);
            }
        }
    }
}
