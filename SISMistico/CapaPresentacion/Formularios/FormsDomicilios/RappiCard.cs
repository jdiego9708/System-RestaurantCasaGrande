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
        private const int ButtonHeight = 44;
        private const int ChipWidth = 36;
        private bool compactView;
        private bool compactExpanded;
        private readonly ToolTip hints = new ToolTip();

        public bool CompactView
        {
            get => compactView;
            set
            {
                if (compactView == value) return;
                compactView = value;
                compactExpanded = false;
                UpdateButton();
                Relayout();
            }
        }
        private const TextFormatFlags Wrapped = TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix
            | TextFormatFlags.NoPadding | TextFormatFlags.Left | TextFormatFlags.Top;
        private const TextFormatFlags SingleLine = TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding
            | TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis;
        private static readonly CultureInfo Colombia = CultureInfo.GetCultureInfo("es-CO");

        // Ordenes expandidas por el usuario; sobrevive a la reconstruccion de la lista en cada recarga.
        private static readonly HashSet<string> Expanded = new HashSet<string>();

        private readonly MinimalIconButton expandButton = new DeliveryTouchButton
        {
            Text = "Expandir",
            Kind = ButtonKind.Outline,
            Icon = IconChar.ChevronDown,
            MinimalStyle = true, LucideIcon = "chevron-down",
            Font = Theme.BodyBold,
            CornerRadius = 9,
            Size = new Size(104, ButtonHeight),
            TabStop = false,
        };

        private readonly MinimalIconButton printButton = new DeliveryTouchButton
        {
            Text = "Imprimir",
            Kind = ButtonKind.Outline,
            Icon = IconChar.Print,
            MinimalStyle = true, LucideIcon = "printer",
            Font = Theme.BodyBold,
            CornerRadius = 9,
            Size = new Size(104, ButtonHeight),
            TabStop = false,
        };

        // Misma cuenta regresiva de los pedidos en preparacion: el tiempo corre desde la hora de la orden en Rappi. Al agotarse
        // el anillo desaparece (en Rappi no hay paso de facturar). Cada ficha lleva su propio reloj de un segundo.
        private readonly CountdownRing ring = new CountdownRing { Visible = false };
        private readonly Timer clock = new Timer { Interval = 1000 };

        private RappiOrder order;
        private bool expanded;
        private bool printing;
        private bool hasRing;
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
            this.ring.Total = DeliveryTiming.PreparationTime;
            this.Controls.Add(this.ring);
            this.clock.Tick += (s, e) => this.ClockTick();
            this.Controls.Add(this.expandButton);
            this.Controls.Add(this.printButton);
            this.printButton.Click += (s, e) => this.PrintTicket();
            this.expandButton.Click += (s, e) =>
            {
                if (this.compactView)
                {
                    this.compactExpanded = !this.compactExpanded;
                    this.UpdateButton();
                    this.Relayout();
                    return;
                }
                this.expanded = !this.expanded;
                if (this.expanded) Expanded.Add(this.order.ExternalId); else Expanded.Remove(this.order.ExternalId);
                this.UpdateButton();
                this.Relayout();
            };
            this.printButton.AccessibleName = "Imprimir orden de Rappi";
            this.hints.SetToolTip(this.printButton, this.printButton.AccessibleName);
        }

        public Func<int> GetPrintCopies { get; set; }
        public RappiOrder Order => this.order;

        private string CustomerContact
        {
            get
            {
                string name = this.order?.CustomerName?.Trim() ?? string.Empty;
                string phone = this.order?.Phone?.Trim() ?? string.Empty;
                return name.Length == 0 ? phone : phone.Length == 0 ? name : name + " - " + phone;
            }
        }

        private string ContactText => string.IsNullOrWhiteSpace(this.order?.Phone)
            ? string.Empty : "Contacto: " + this.order.Phone.Trim();

        private string NumberBadge => this.order?.DayNumber.HasValue == true ? "R-" + this.order.DayNumber.Value : null;

        private string CountdownText
        {
            get
            {
                TimeSpan? remaining = this.RemainingTime(DateTime.Now);
                return remaining.HasValue && remaining.Value > TimeSpan.Zero ? DeliveryListLayout.Time(remaining.Value) : string.Empty;
            }
        }

        private string TotalText => "Total: " + (this.order.OrderValue.HasValue ? Money(this.order.OrderValue.Value) : "—");
        private string CashText => this.order.CashAmount == 0 ? string.Empty
            : "Efectivo: " + (this.order.CashAmount.HasValue ? Money(this.order.CashAmount.Value) : "—");

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.clock.Dispose();
                this.hints.Dispose();
            }
            base.Dispose(disposing);
        }

        // Lo que le queda a la orden; null si no tiene hora legible (entonces no hay cuenta).
        private TimeSpan? RemainingTime(DateTime now)
        {
            if (this.order == null || !this.order.OrderDateTime.HasValue)
                return null;
            TimeSpan remaining = this.order.OrderDateTime.Value + DeliveryTiming.PreparationTime - now;
            if (remaining < TimeSpan.Zero) return TimeSpan.Zero;
            return remaining > DeliveryTiming.PreparationTime ? DeliveryTiming.PreparationTime : remaining;
        }

        private void UpdateRing()
        {
            TimeSpan? remaining = this.RemainingTime(DateTime.Now);
            this.hasRing = remaining.HasValue && remaining.Value > TimeSpan.Zero;
            this.ring.Visible = this.hasRing && !this.compactView;
            if (this.hasRing)
            {
                this.ring.Remaining = remaining.Value;
                this.clock.Start();
            }
            else
            {
                this.clock.Stop();
            }
        }

        private void ClockTick()
        {
            TimeSpan? remaining = this.RemainingTime(DateTime.Now);
            if (remaining.HasValue && remaining.Value > TimeSpan.Zero)
            {
                this.ring.Remaining = remaining.Value;
                if (this.compactView) this.Relayout();
                return;
            }
            // Se agoto: sin anillo la ficha vuelve a usar todo el ancho.
            this.UpdateRing();
            this.Relayout();
        }

        // Imprime la factura guardada de la orden como ticket de 80 mm, con la numeracion del dia propia de Rappi
        // (separada de la de los domicilios del programa).
        private void PrintTicket()
        {
            if (this.printing || this.order == null)
                return;
            int copies = Math.Max(0, Math.Min(99, this.GetPrintCopies?.Invoke() ?? 1));
            if (copies == 0) return;
            this.printing = true;
            try
            {
                MensajeEspera.ShowWait("Imprimiendo...");
                try { RappiTicketPrinter.Print(this.order, copies); }
                finally { MensajeEspera.CloseForm(); }
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto("RappiCard", "PrintTicket", "No se pudo imprimir la orden de Rappi", ex.Message);
            }
            finally
            {
                this.printing = false;
            }
        }

        public void Bind(RappiOrder order)
        {
            this.order = order;
            this.expanded = Expanded.Contains(order.ExternalId);
            this.hints.SetToolTip(this, this.CustomerContact);
            this.UpdateRing();
            this.UpdateButton();
            this.Relayout();
        }

        private void UpdateButton()
        {
            bool detail = this.compactView ? this.compactExpanded : this.expanded;
            this.expandButton.Text = this.compactView ? string.Empty : detail ? "Contraer" : "Expandir";
            this.expandButton.Icon = detail ? IconChar.ChevronUp : IconChar.ChevronDown;
            this.expandButton.LucideIcon = detail ? "chevron-up" : "chevron-down";
            this.expandButton.AccessibleName = detail ? "Contraer detalle" : "Expandir detalle";
            this.hints.SetToolTip(this.expandButton, this.expandButton.AccessibleName);
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
                this.ring.Visible = this.hasRing && !this.compactView;
                this.printButton.Text = this.compactView ? string.Empty : "Imprimir";
                if (this.compactView)
                {
                    this.expandButton.Bounds = new Rectangle(this.Width - 100, 14, 44, 44);
                    this.printButton.Bounds = new Rectangle(this.Width - 52, 14, 44, 44);
                    this.Height = this.ComposeCompact(null);
                    this.Invalidate();
                    return;
                }
                this.expandButton.Size = this.printButton.Size = new Size(104, ButtonHeight);
                int height = this.Compose(null);
                Rectangle surface = this.SurfaceBounds;
                this.ring.Location = new Point(surface.Right - PaddingX - this.ring.Width, surface.Y + 8 + (this.ContactText.Length > 0 ? 30 : 0));
                this.expandButton.Location = new Point(surface.Right - PaddingX - this.expandButton.Width, this.buttonTop);
                this.printButton.Location = new Point(this.expandButton.Left - 8 - this.printButton.Width, this.buttonTop);
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
            if (this.compactView && this.order != null)
            {
                this.ComposeCompact(g);
                return;
            }

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
            // Junto al anillo el texto es mas angosto; debajo de el vuelve a ocupar todo el ancho.
            int headWidth = this.hasRing ? Math.Max(60, fullWidth - this.ring.Width - 12) : fullWidth;
            int y = surface.Y + 8;

            // Numero de orden y hora a la derecha.
            string number = "#" + this.order.ExternalId;
            string time = this.order.OrderDateTime.HasValue
                ? this.order.OrderDateTime.Value.ToString("h:mm tt", CultureInfo.InvariantCulture) : "sin hora";
            int timeWidth = LineWidth(time, Theme.BodyBold);
            int numberWidth = LineWidth(number, Theme.Heading);
            if (this.ContactText.Length > 0)
            {
                int contactWidth = Math.Min(fullWidth - 60, LineWidth(this.order.Phone.Trim(), Theme.Small) + 18);
                int orderWidth = Math.Max(30, Math.Min(numberWidth + 2, fullWidth - contactWidth - 10));
                DrawText(g, number, Theme.Heading, Theme.TextPrimary, new Rectangle(left, y, orderWidth, 24), SingleLine);
                DeliveryListLayout.DrawContact(g, this.order.Phone,
                    new Rectangle(left + orderWidth + 10, y, fullWidth - orderWidth - 10, 24));
            }
            else
            {
                DrawText(g, number, Theme.Heading, Theme.TextPrimary, new Rectangle(left, y, Math.Max(30, headWidth - timeWidth - 8), 24), SingleLine);
                DrawText(g, time, Theme.BodyBold, Theme.TextMuted, new Rectangle(left + headWidth - timeWidth, y, timeWidth, 24),
                    SingleLine | TextFormatFlags.Right);
            }
            y += 28;

            string customer = string.IsNullOrWhiteSpace(this.order.CustomerName) ? "Sin cliente" : this.order.CustomerName.Trim();
            if (!string.IsNullOrWhiteSpace(this.order.PaymentMethod)) customer += " · " + this.order.PaymentMethod.Trim();
            y += DrawText(g, customer, Theme.BodyBold, Theme.TextPrimary,
                    new Rectangle(left, y, headWidth, 0), Wrapped) + 3;

            string summary = "Pedido a las " + time
                + (this.order.Items.Count > 0 ? " · " + this.order.Items.Count + (this.order.Items.Count == 1 ? " producto" : " productos") : string.Empty);
            y += IconRow(g, IconChar.Clock, summary, left, y, headWidth);
            if (this.hasRing)
                y = Math.Max(y, surface.Y + 8 + (this.ContactText.Length > 0 ? 30 : 0) + this.ring.Height + 2);
            y += 4;
            y += this.ComposePaymentSummary(g, left, y, fullWidth, string.Empty);

            // Numeracion del ticket en una fila propia, debajo del resumen y fuera del area del contador.
            if (this.order.DayNumber.HasValue)
            {
                string tag = this.NumberBadge;
                int tagWidth = LineWidth(tag, Theme.SmallBold) + 18;
                DrawChip(g, new Rectangle(left, y + 2, tagWidth, 22), tag);
                y += 28;
            }
            if (this.expanded)
                y = this.ComposeDetail(g, left, right, fullWidth, y + 4);

            y += 2;
            this.buttonTop = y;
            y += ButtonHeight + 8;
            return y + ShadowDepth + 1;
        }

        private int ComposeCompact(Graphics g)
        {
            string number = "#" + this.order.ExternalId;
            string timer = this.CountdownText;
            string paymentTimer = this.ContactText.Length > 0 ? timer : string.Empty;
            int paymentsHeight = this.ComposePaymentSummary(null, 12, 64, this.Width - 24, paymentTimer);
            int rowHeight = Math.Max(96, 64 + paymentsHeight + 8);
            DeliveryListLayout.Draw(g, this.Width, Theme.Rappi, number, timer,
                string.IsNullOrWhiteSpace(this.order.CustomerName) ? "Sin cliente" : this.order.CustomerName.Trim(),
                this.order.PaymentMethod?.Trim() ?? string.Empty, headerContact: this.order.Phone,
                rowHeight: rowHeight, moveTimerToSummary: false, numberBadge: this.NumberBadge, headerPhoneIcon: true);
            this.ComposePaymentSummary(g, 12, 64, this.Width - 24, paymentTimer);
            if (!this.compactExpanded) return rowHeight;
            int y = rowHeight + 4;
            y += DrawText(g, "Orden #" + this.order.ExternalId, Theme.SmallBold, Theme.TextPrimary,
                new Rectangle(12, y, this.Width - 24, 0), Wrapped) + 6;
            return this.ComposeDetail(g, 12, this.Width - 12, this.Width - 24, y) + 8;
        }

        private int ComposePaymentSummary(Graphics g, int left, int y, int width, string countdown)
        {
            int timerWidth = string.IsNullOrEmpty(countdown) ? 0 : LineWidth(countdown, Theme.SmallBold) + 10;
            int available = Math.Max(40, width - timerWidth);
            string total = this.TotalText;
            string cash = this.CashText;
            bool hasCash = cash.Length > 0;
            const string separator = " - ";
            int totalWidth = LineWidth(total, Theme.Small);
            int separatorWidth = LineWidth(separator, Theme.Small);
            int cashWidth = hasCash ? LineWidth(cash, Theme.SmallBold) : 0;
            bool wrap = hasCash && totalWidth + separatorWidth + cashWidth > available;
            DrawText(g, total, Theme.Small, Theme.TextPrimary, new Rectangle(left, y, Math.Min(totalWidth, available), 22), SingleLine);
            int cashLeft = wrap ? left : left + totalWidth + separatorWidth;
            if (hasCash && !wrap)
                DrawText(g, separator, Theme.Small, Theme.TextMuted, new Rectangle(left + totalWidth, y, separatorWidth, 22), SingleLine);
            if (hasCash)
                DrawText(g, cash, Theme.SmallBold, Theme.Danger,
                    new Rectangle(cashLeft, wrap ? y + 22 : y, Math.Max(10, available - (cashLeft - left)), 22), SingleLine);
            if (timerWidth > 0)
                DrawText(g, countdown, Theme.SmallBold, this.ring.CurrentColor,
                    new Rectangle(left + width - timerWidth, y, timerWidth, 22), SingleLine | TextFormatFlags.Right);
            return wrap ? 44 : 22;
        }

        private int ComposeDetail(Graphics g, int left, int right, int fullWidth, int y)
        {
            Divider(g, left, right, y);
            y += 8;

            y += DeliveryListLayout.DrawContact(g, this.order.Phone, new Rectangle(left, y, fullWidth, 22));
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
