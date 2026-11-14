using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;
using CapaEntidades.Models;
using CapaPresentacion.UI;
using FontAwesome.Sharp;

namespace CapaPresentacion.Formularios.FormsDomicilios
{
    /// <summary>
    /// Ficha informativa de una orden de Rappi (leida de su factura PDF). Se dibuja entera; plegada muestra lo esencial
    /// (orden, hora, cliente, total) y con Expandir el detalle: contacto, productos y desglose de valores. La altura sale
    /// del contenido, igual que <see cref="DeliveryCard"/>.
    /// </summary>
    public class RappiCard : Control
    {
        private const int ShadowDepth = 4;
        private const int AccentWidth = 5;
        private const int PaddingX = 16;
        private const int ButtonHeight = 30;
        private const int ChipWidth = 36;
        private const TextFormatFlags Wrapped = TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix
            | TextFormatFlags.NoPadding | TextFormatFlags.Left | TextFormatFlags.Top;
        private const TextFormatFlags SingleLine = TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding
            | TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis;
        private static readonly CultureInfo Colombia = CultureInfo.GetCultureInfo("es-CO");

        // Ordenes expandidas por el usuario; sobrevive a la reconstruccion de la lista en cada recarga.
        private static readonly HashSet<string> Expanded = new HashSet<string>();

        private readonly ModernButton expandButton = new ModernButton
        {
            Text = "Expandir",
            Kind = ButtonKind.Outline,
            Icon = IconChar.ChevronDown,
            Font = Theme.BodyBold,
            CornerRadius = 9,
            Size = new Size(104, ButtonHeight),
            TabStop = false,
        };

        private RappiOrder order;
        private bool expanded;
        private int buttonTop;
        private int laidOutWidth = -1;
        private bool composing;

        public RappiCard()
        {
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            this.BackColor = Theme.Surface;
            this.Font = Theme.Body;
            this.Size = new Size(380, 120);
            this.Margin = new Padding(0, 0, 0, 2);
            this.Controls.Add(this.expandButton);
            this.expandButton.Click += (s, e) =>
            {
                this.expanded = !this.expanded;
                if (this.expanded) Expanded.Add(this.order.ExternalId); else Expanded.Remove(this.order.ExternalId);
                this.UpdateButton();
                this.Relayout();
            };
        }

        public RappiOrder Order => this.order;

        public void Bind(RappiOrder order)
        {
            this.order = order;
            this.expanded = Expanded.Contains(order.ExternalId);
            this.UpdateButton();
            this.Relayout();
        }

        private void UpdateButton()
        {
            this.expandButton.Text = this.expanded ? "Contraer" : "Expandir";
            this.expandButton.Icon = this.expanded ? IconChar.ChevronUp : IconChar.ChevronDown;
        }

        #region DISTRIBUCION Y DIBUJO
        private Rectangle SurfaceBounds => new Rectangle(ShadowDepth, ShadowDepth,
            this.Width - ShadowDepth * 2 - 1, this.Height - ShadowDepth * 2 - 1);

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (this.Width != this.laidOutWidth)
                this.Relayout();
        }

        private void Relayout()
        {
            if (this.order == null || this.composing)
                return;
            this.composing = true;
            try
            {
                this.laidOutWidth = this.Width;
                int height = this.Compose(null);
                Rectangle surface = this.SurfaceBounds;
                this.expandButton.Location = new Point(surface.Right - PaddingX - this.expandButton.Width, this.buttonTop);
                if (this.Height != height)
                    this.Height = height;
                this.Invalidate();
            }
            finally
            {
                this.composing = false;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Theme.ContainerBackground(this));
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle surface = this.SurfaceBounds;
            if (surface.Width < 40 || surface.Height < 40 || this.order == null)
                return;

            Theme.DrawShadow(g, surface, Theme.Radius, ShadowDepth);
            using (GraphicsPath path = Theme.RoundedPath(surface, Theme.Radius))
            using (SolidBrush fill = new SolidBrush(Theme.Surface))
            using (Pen border = new Pen(Theme.Border))
            {
                g.FillPath(fill, path);
                g.DrawPath(border, path);
                GraphicsState state = g.Save();
                g.SetClip(path);
                using (SolidBrush bar = new SolidBrush(Theme.Rappi))
                    g.FillRectangle(bar, surface.X, surface.Y, AccentWidth, surface.Height + 1);
                g.Restore(state);
            }
            this.Compose(g);
        }

        /// <summary>Un solo recorrido para medir y dibujar (con g null solo mide). Devuelve la altura total.</summary>
        private int Compose(Graphics g)
        {
            Rectangle surface = this.SurfaceBounds;
            int left = surface.X + AccentWidth + PaddingX;
            int right = surface.Right - PaddingX;
            int fullWidth = Math.Max(60, right - left);
            int y = surface.Y + 8;

            // Numero de orden y hora a la derecha.
            string number = "#" + this.order.ExternalId;
            string time = this.order.OrderDateTime.HasValue
                ? this.order.OrderDateTime.Value.ToString("h:mm tt", CultureInfo.InvariantCulture) : "sin hora";
            int timeWidth = LineWidth(time, Theme.BodyBold);
            DrawText(g, number, Theme.Heading, Theme.TextPrimary, new Rectangle(left, y, fullWidth - timeWidth - 8, 24), SingleLine);
            DrawText(g, time, Theme.BodyBold, Theme.TextMuted, new Rectangle(right - timeWidth, y, timeWidth, 24),
                SingleLine | TextFormatFlags.Right);
            y += 28;

            if (!string.IsNullOrWhiteSpace(this.order.CustomerName))
                y += DrawText(g, this.order.CustomerName.Trim(), Theme.BodyBold, Theme.TextPrimary,
                    new Rectangle(left, y, fullWidth, 0), Wrapped) + 3;

            // Plegada: total, forma de pago y numero de productos.
            string summary = (this.order.OrderValue.HasValue ? "Pedido " + Money(this.order.OrderValue.Value) : "Valor sin leer")
                + (string.IsNullOrWhiteSpace(this.order.PaymentMethod) ? string.Empty : " · " + this.order.PaymentMethod)
                + (this.order.Items.Count > 0 ? " · " + this.order.Items.Count + (this.order.Items.Count == 1 ? " producto" : " productos") : string.Empty);
            y += IconRow(g, IconChar.MoneyBill, summary, left, y, fullWidth);

            if (this.expanded)
                y = this.ComposeDetail(g, left, right, fullWidth, y + 4);

            y += 2;
            this.buttonTop = y;
            y += ButtonHeight + 8;
            return y + ShadowDepth + 1;
        }

        private int ComposeDetail(Graphics g, int left, int right, int fullWidth, int y)
        {
            Divider(g, left, right, y);
            y += 8;

            y += IconRow(g, IconChar.Phone, this.order.Phone, left, y, fullWidth);
            y += IconRow(g, IconChar.LocationDot, this.order.Address, left, y, fullWidth);
            if (!string.IsNullOrWhiteSpace(this.order.PaymentMethod) && this.order.CashPayWith.HasValue && this.order.CashPayWith.Value > 0)
                y += IconRow(g, IconChar.HandHoldingDollar, "Paga con " + Money(this.order.CashPayWith.Value)
                    + (this.order.ChangeToBring.HasValue ? " · llevar " + Money(this.order.ChangeToBring.Value) + " de cambio" : string.Empty), left, y, fullWidth);
            if (!string.IsNullOrWhiteSpace(this.order.DeliveryCode))
                y += IconRow(g, IconChar.Key, "Código de entrega " + this.order.DeliveryCode, left, y, fullWidth);
            y += 4;

            // Productos: cantidad en una ficha, nombre y valor de la linea.
            foreach (RappiOrderItem item in this.order.Items)
            {
                int priceWidth = item.Price > 0 ? LineWidth(Money(item.Price), Theme.Body) : 0;
                int nameWidth = fullWidth - ChipWidth - 10 - priceWidth - 8;
                int nameHeight = DrawText(g, item.Name, Theme.Body, Theme.TextPrimary,
                    new Rectangle(left + ChipWidth + 10, y + 2, Math.Max(40, nameWidth), 0), Wrapped);
                DrawChip(g, new Rectangle(left, y, ChipWidth, 22), item.Quantity + "×");
                if (priceWidth > 0)
                    DrawText(g, Money(item.Price), Theme.Body, Theme.TextMuted, new Rectangle(right - priceWidth, y + 2, priceWidth, 20),
                        SingleLine | TextFormatFlags.Right);
                int rowHeight = Math.Max(22, nameHeight + 2);
                foreach (string note in item.Notes)
                    rowHeight += DrawText(g, note, Theme.Small, Theme.TextMuted,
                        new Rectangle(left + ChipWidth + 10, y + rowHeight, Math.Max(40, nameWidth), 0), Wrapped) + 1;
                y += rowHeight + 4;
            }
            if (!this.order.IsComplete)
                y += DrawText(g, "Datos incompletos: revise el PDF de la factura.", Theme.Small,
                    Theme.Darken(Theme.Warning, 0.35f), new Rectangle(left, y, fullWidth, 0), Wrapped) + 6;

            // Desglose de valores, como en la factura.
            y += 2;
            Divider(g, left, right, y);
            y += 8;
            y += ValueRow(g, "Subtotal", this.order.Subtotal, left, right, y, false);
            y += ValueRow(g, "Descuento por oferta", this.order.Discount, left, right, y, false);
            y += ValueRow(g, "Costo de envío", this.order.ShippingCost, left, right, y, false);
            y += ValueRow(g, "Propina", this.order.Tip, left, right, y, false);
            y += ValueRow(g, "Tarifa de servicio", this.order.ServiceFee, left, right, y, false);
            y += ValueRow(g, "Pagado a través de Rappi", this.order.PaidThroughRappi, left, right, y, false);
            y += ValueRow(g, "Total a cobrar", this.order.Total, left, right, y, true);
            return y + 4;
        }

        private static int ValueRow(Graphics g, string label, int? value, int left, int right, int y, bool bold)
        {
            if (!value.HasValue)
                return 0;
            Font font = bold ? Theme.BodyBold : Theme.Small;
            string text = (value.Value < 0 ? "-" : string.Empty) + Money(Math.Abs(value.Value));
            int width = LineWidth(text, font);
            DrawText(g, label, font, bold ? Theme.TextPrimary : Theme.TextMuted, new Rectangle(left, y, right - left - width - 8, 18), SingleLine);
            DrawText(g, text, font, bold ? Theme.TextPrimary : Theme.Neutral, new Rectangle(right - width, y, width, 18), SingleLine | TextFormatFlags.Right);
            return 19;
        }

        private static string Money(int value) => "$" + value.ToString("N0", Colombia);

        private static int LineWidth(string text, Font font) =>
            TextRenderer.MeasureText(text, font, new Size(int.MaxValue, int.MaxValue),
                TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine).Width + 2;

        private static int Measure(string text, Font font, int width) =>
            TextRenderer.MeasureText(text, font, new Size(Math.Max(1, width), int.MaxValue), Wrapped).Height;

        private static int DrawText(Graphics g, string text, Font font, Color color, Rectangle bounds, TextFormatFlags flags)
        {
            if (bounds.Height == 0)
                bounds.Height = Measure(text, font, bounds.Width);
            if (g != null)
                TextRenderer.DrawText(g, text, font, bounds, color, flags);
            return bounds.Height;
        }

        private int IconRow(Graphics g, IconChar icon, string text, int left, int y, int width)
        {
            if (string.IsNullOrWhiteSpace(text))
                return 0;
            if (g != null)
                IconRenderer.Draw(this, g, icon, Theme.TextMuted, new Rectangle(left + 1, y + 2, 13, 13));
            int height = DrawText(g, text.Trim(), Theme.Small, Theme.Neutral, new Rectangle(left + 22, y, width - 22, 0), Wrapped);
            return Math.Max(17, height) + 5;
        }

        private static void Divider(Graphics g, int left, int right, int y)
        {
            if (g == null) return;
            using (Pen pen = new Pen(Theme.Border))
                g.DrawLine(pen, left, y, right, y);
        }

        private static void DrawChip(Graphics g, Rectangle bounds, string text)
        {
            if (g == null) return;
            using (GraphicsPath path = Theme.RoundedPath(bounds, 7))
            using (SolidBrush fill = new SolidBrush(Theme.Lighten(Theme.Rappi, 0.88f)))
                g.FillPath(fill, path);
            TextRenderer.DrawText(g, text, Theme.SmallBold, bounds, Theme.Darken(Theme.Rappi, 0.2f),
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
        }
        #endregion
    }
}
