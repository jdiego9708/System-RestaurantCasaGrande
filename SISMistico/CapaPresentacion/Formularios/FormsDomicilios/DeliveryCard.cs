using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CapaEntidades.Models;
using CapaNegocio;
using CapaPresentacion.Formularios.Controles;
using CapaPresentacion.Formularios.FormsPedido;
using CapaPresentacion.UI;
using FontAwesome.Sharp;

namespace CapaPresentacion.Formularios.FormsDomicilios
{
    /// <summary>Un producto del domicilio tal como se muestra en la tarjeta.</summary>
    public sealed class DeliveryItem
    {
        public string Quantity { get; set; }
        public string Name { get; set; }
        public string Notes { get; set; }
    }

    public sealed class DeliveryPayment
    {
        public string Method { get; set; }
        public decimal? Amount { get; set; }
    }

    /// <summary>
    /// Lo que la tarjeta muestra ademas del pedido: cliente, entrega y productos. Va aparte de la tarjeta para
    /// que el dibujo no dependa de la base de datos (las pruebas de renderizado la llenan a mano).
    /// </summary>
    public sealed class DeliveryDetails
    {
        public string CustomerName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Notes { get; set; }
        /// <summary>Numero del dia de la factura de domicilio (el mismo que sale impreso); null si todavia no tiene.</summary>
        public int? Number { get; set; }
        public List<DeliveryItem> Items { get; } = new List<DeliveryItem>();
        /// <summary>False si la consulta del detalle fallo; la tarjeta lo avisa en vez de quedar vacia.</summary>
        public bool Loaded { get; set; }
        public bool PaymentsLoaded { get; set; }
        public bool PaymentsFinal { get; set; }
        public List<DeliveryPayment> Payments { get; } = new List<DeliveryPayment>();

        public static DeliveryDetails Load(Pedidos order)
        {
            DeliveryDetails details = new DeliveryDetails();
            DataTable main = NPedido.BuscarPedidosYDetalle("ID PEDIDO Y DETALLE", Convert.ToString(order.Id_pedido),
                out DataTable items, out DataTable _, out string _);
            if (main == null || main.Rows.Count == 0)
                return details;

            details.Loaded = true;
            details.Number = NPedido.GetOrAssignDeliveryNumber(order.Id_pedido);
            details.Notes = Convert.ToString(main.Rows[0]["Observaciones_pedido"]);

            // La consulta antigua concatena el telefono al nombre del cliente.
            string[] customerParts = Regex.Split(Convert.ToString(main.Rows[0]["Cliente"]),
                @"\s*-\s*Tel[eé]fono\s*:\s*", RegexOptions.IgnoreCase);
            details.CustomerName = customerParts[0];

            // Direccion de entrega elegida al crear el pedido (los domicilios viejos no la tienen).
            CustomerAddress address = order.AddressId.HasValue ? NAddresses.Get(order.AddressId.Value) : null;
            string reference = address != null ? address.Reference : order.Cliente?.Referencia_ubicacion;
            details.Address = address != null ? address.Summary : order.Cliente?.Direccion_cliente;
            if (!string.IsNullOrWhiteSpace(details.Address) && !string.IsNullOrWhiteSpace(reference))
                details.Address += " · Ref: " + reference.Trim();
            details.Phone = !string.IsNullOrWhiteSpace(address?.ContactPhone) ? address.ContactPhone :
                customerParts.Length > 1 ? customerParts[1] : order.Cliente?.Telefono_cliente;

            if (items != null)
            {
                foreach (DataRow row in items.Rows)
                {
                    details.Items.Add(new DeliveryItem
                    {
                        Quantity = Convert.ToString(row["Cantidad"]),
                        Name = Convert.ToString(row["Nombre"]),
                        Notes = Convert.ToString(row["Observaciones"]),
                    });
                }
            }
            return details;
        }
    }

    /// <summary>
    /// Tarjeta de un domicilio. Se dibuja entera (superficie, franja de estado, textos e iconos); los unicos
    /// controles hijos son el anillo de la cuenta regresiva y los botones. La altura sale del contenido: al
    /// cambiar el ancho se recalcula sola.
    /// </summary>
    public class DeliveryCard : Control
    {
        private const int ShadowDepth = 4;
        private const int AccentWidth = 5;
        private const int PaddingX = 16;
        private const int ButtonHeight = 44;
        private const int ExpandWidth = 104;
        private bool compactView;
        private bool compactExpanded;
        private readonly ModernButton moreButton = NewAction(string.Empty, ButtonKind.Outline, IconChar.EllipsisH);
        private readonly ContextMenuStrip compactMenu = new ContextMenuStrip();

        public bool CompactView
        {
            get => compactView;
            set
            {
                if (compactView == value) return;
                compactView = value;
                compactExpanded = false;
                compactMenu.Close();
                UpdateExpandButton();
                Relayout();
            }
        }
        // Pedidos que el usuario expandio; sobrevive a la reconstruccion de tarjetas en cada refresco.
        private static readonly HashSet<int> ExpandedOrders = new HashSet<int>();
        private const int ChipWidth = 36;
        private const TextFormatFlags Wrapped = TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix
            | TextFormatFlags.NoPadding | TextFormatFlags.Left | TextFormatFlags.Top;
        private const TextFormatFlags SingleLine = TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding
            | TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis;

        private readonly CountdownRing ring = new CountdownRing { Visible = false };
        private readonly ModernButton billButton = NewAction("Facturar", ButtonKind.Success, IconChar.FileInvoiceDollar);
        private readonly ModernButton addButton = NewAction("Agregar", ButtonKind.Primary, IconChar.Plus);
        private readonly ModernButton printButton = NewAction(string.Empty, ButtonKind.Outline, IconChar.Print);
        private readonly ModernButton cancelButton = NewAction(string.Empty, ButtonKind.Danger, IconChar.Xmark);
        private readonly ModernButton finishButton = NewAction(string.Empty, ButtonKind.Success, IconChar.CircleCheck);
        private readonly ModernButton expandButton = NewAction("Expandir", ButtonKind.Outline, IconChar.ChevronDown);
        private readonly ToolTip toolTip = new ToolTip();
        private readonly Observacion cancelNote = new Observacion();
        private PoperContainer cancelPopup;

        private Pedidos order;
        private DeliveryDetails details = new DeliveryDetails();
        private DeliveryStage stage;
        private Color accent = Theme.Success;
        private int buttonsTop;
        private int laidOutWidth = -1;
        private bool composing;
        private bool expanded;

        /// <summary>Se pidio facturar; el emisor es el <see cref="Pedidos"/> de la tarjeta.</summary>
        public event EventHandler BillRequested;
        /// <summary>Se pidio pasarlo a TERMINADOS ya (como si se agotara el tiempo), sin abrir la ventana de facturar.</summary>
        public event EventHandler FinishRequested;
        /// <summary>El pedido cambio (cancelado o con productos nuevos) y la lista debe recargarse.</summary>
        public event EventHandler Changed;

        public DeliveryCard()
        {
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            // Fondo que ven los hijos (anillo y botones): el de la superficie, no el de la lista.
            this.BackColor = Theme.Surface;
            this.Font = Theme.Body;
            this.Size = new Size(380, 130);
            this.Margin = new Padding(0, 0, 0, 2);
            this.ring.Total = DeliveryTiming.PreparationTime;

            this.Controls.Add(this.ring);
            this.Controls.Add(this.billButton);
            this.Controls.Add(this.addButton);
            this.Controls.Add(this.printButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.finishButton);
            this.Controls.Add(this.expandButton);
            this.Controls.Add(this.moreButton);
            this.moreButton.Visible = false;
            this.moreButton.AccessibleName = "Acciones del domicilio";
            this.toolTip.SetToolTip(this.moreButton, "Acciones del domicilio");
            this.moreButton.Click += (s, e) => this.ShowCompactActions();
            this.expandButton.Click += this.ExpandButton_Click;

            this.toolTip.SetToolTip(this.billButton, "Facturar domicilio");
            this.toolTip.SetToolTip(this.addButton, "Agregar platos o bebidas al domicilio");
            this.toolTip.SetToolTip(this.cancelButton, "Cancelar el domicilio");
            this.toolTip.SetToolTip(this.finishButton, "Pasar a TERMINADOS: se factura ya, como si se acabara el tiempo");

            this.billButton.Click += (s, e) => this.BillRequested?.Invoke(this.order, e);
            this.finishButton.Click += (s, e) => this.FinishRequested?.Invoke(this.order, e);
            this.addButton.Click += this.AddButton_Click;
            this.printButton.Click += this.PrintButton_Click;
            this.cancelButton.Click += this.CancelButton_Click;
            this.cancelNote.btnSave.Visible = true;
            this.cancelNote.btnSave.Click += this.CancelNote_Save;
        }

        public Func<int> GetPrintCopies { get; set; }
        public Pedidos Order => this.order;

        public DeliveryStage Stage => this.stage;

        private string ContactText => string.IsNullOrWhiteSpace(this.details?.Phone)
            ? string.Empty : "Contacto: " + this.details.Phone.Trim();

        private static ModernButton NewAction(string text, ButtonKind kind, IconChar icon) => new DeliveryTouchButton
        {
            Text = text,
            Kind = kind,
            Icon = icon,
            MinimalStyle = icon == IconChar.Print || icon == IconChar.ChevronDown || icon == IconChar.EllipsisH,
            LucideIcon = icon == IconChar.Print ? "printer" : icon == IconChar.ChevronDown ? "chevron-down" : icon == IconChar.EllipsisH ? "ellipsis" : null,
            Font = Theme.BodyBold,
            CornerRadius = 9,
            Size = new Size(ButtonHeight, ButtonHeight),
            TabStop = false,
        };

        /// <summary>Asigna el pedido, su detalle y la etapa en la que se muestra.</summary>
        public void Bind(Pedidos order, DeliveryDetails details, DeliveryStage stage, DateTime now)
        {
            this.order = order;
            this.details = details ?? new DeliveryDetails();
            this.stage = stage;
            this.expanded = ExpandedOrders.Contains(order.Id_pedido);
            this.UpdateExpandButton();

            bool pending = stage != DeliveryStage.Billed;
            this.billButton.Visible = pending;
            this.addButton.Visible = pending;
            this.cancelButton.Visible = pending;
            // Solo mientras se prepara: al agotarse el tiempo el pedido ya esta en TERMINADOS.
            this.finishButton.Visible = stage == DeliveryStage.Preparing;
            this.toolTip.SetToolTip(this.printButton,
                pending ? "Imprimir las comandas nuevamente" : "Imprimir la factura nuevamente");

            this.ring.Visible = stage == DeliveryStage.Preparing;
            this.ring.Remaining = DeliveryTiming.Remaining(order, now);
            this.accent = this.AccentForStage();
            this.Relayout();
        }

        /// <summary>
        /// Avanza la cuenta regresiva. Devuelve true cuando el tiempo se agoto y la tarjeta debe pasar a TERMINADOS.
        /// </summary>
        public bool Tick(DateTime now)
        {
            if (this.order == null || this.stage != DeliveryStage.Preparing)
                return false;

            TimeSpan remaining = DeliveryTiming.Remaining(this.order, now);
            this.ring.Remaining = remaining;
            if (this.compactView) this.Invalidate();
            Color color = this.ring.CurrentColor;
            if (color != this.accent)
            {
                // La franja y la pildora de estado siguen el color del anillo.
                this.accent = color;
                this.Invalidate(new Rectangle(0, 0, ShadowDepth + AccentWidth + 2, this.Height));
                this.Invalidate(new Rectangle(0, 0, this.Width, 48));
            }
            return remaining <= TimeSpan.Zero;
        }

        private void ExpandButton_Click(object sender, EventArgs e)
        {
            if (this.compactView)
            {
                this.compactExpanded = !this.compactExpanded;
                this.UpdateExpandButton();
                this.Relayout();
                return;
            }
            this.expanded = !this.expanded;
            if (this.expanded)
                ExpandedOrders.Add(this.order.Id_pedido);
            else
                ExpandedOrders.Remove(this.order.Id_pedido);
            this.UpdateExpandButton();
            this.Relayout();
        }

        private void UpdateExpandButton()
        {
            bool detail = this.compactView ? this.compactExpanded : this.expanded;
            this.expandButton.Text = this.compactView ? string.Empty : detail ? "Contraer" : "Expandir";
            this.expandButton.Icon = detail ? IconChar.ChevronUp : IconChar.ChevronDown;
            ((MinimalIconButton)this.expandButton).LucideIcon = detail ? "chevron-up" : "chevron-down";
            this.expandButton.AccessibleName = detail ? "Contraer detalle" : "Expandir detalle";
            this.toolTip.SetToolTip(this.expandButton, this.expandButton.AccessibleName);
        }

        private void ShowCompactActions()
        {
            this.BuildCompactActions();
            this.compactMenu.Show(this.moreButton, new Point(0, this.moreButton.Height));
        }

        private void BuildCompactActions()
        {
            while (this.compactMenu.Items.Count > 0) this.compactMenu.Items[0].Dispose();
            if (this.stage != DeliveryStage.Billed)
            {
                this.compactMenu.Items.Add(DeliveryListLayout.Action("Facturar", (s, e) => this.BillRequested?.Invoke(this.order, e)));
                this.compactMenu.Items.Add(DeliveryListLayout.Action("Agregar productos", this.AddButton_Click));
                if (this.stage == DeliveryStage.Preparing)
                    this.compactMenu.Items.Add(DeliveryListLayout.Action("Pasar a terminados", (s, e) => this.FinishRequested?.Invoke(this.order, e)));
            }
            this.compactMenu.Items.Add(DeliveryListLayout.Action(this.stage == DeliveryStage.Billed ? "Imprimir factura" : "Imprimir comandas", this.PrintButton_Click));
            if (this.stage != DeliveryStage.Billed)
                this.compactMenu.Items.Add(DeliveryListLayout.Action("Cancelar domicilio", this.CancelButton_Click));
        }

        private Color AccentForStage()
        {
            switch (this.stage)
            {
                case DeliveryStage.Preparing: return this.ring.CurrentColor;
                case DeliveryStage.AwaitingBilling: return Theme.Warning;
                default: return Theme.Success;
            }
        }

        private string StageText()
        {
            switch (this.stage)
            {
                case DeliveryStage.Preparing: return "En preparación";
                case DeliveryStage.AwaitingBilling: return "Por facturar";
                default: return "Facturado";
            }
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
                bool pendingStage = this.stage != DeliveryStage.Billed;
                this.billButton.Visible = this.addButton.Visible = this.cancelButton.Visible = pendingStage && !this.compactView;
                this.finishButton.Visible = this.stage == DeliveryStage.Preparing && !this.compactView;
                this.printButton.Visible = !this.compactView || !pendingStage;
                this.moreButton.Visible = this.compactView && pendingStage;
                this.ring.Visible = this.stage == DeliveryStage.Preparing && !this.compactView;
                if (this.compactView)
                {
                    this.expandButton.Bounds = new Rectangle(this.Width - 100, 14, 44, 44);
                    this.moreButton.Bounds = new Rectangle(this.Width - 52, 14, 44, 44);
                    this.printButton.Bounds = new Rectangle(this.Width - 52, 14, 44, 44);
                    this.Height = this.ComposeCompact(null);
                    this.Invalidate();
                    return;
                }
                int height = this.Compose(null);

                Rectangle surface = this.SurfaceBounds;
                int left = surface.X + AccentWidth + PaddingX;
                int right = surface.Right - PaddingX;
                this.ring.Location = new Point(right - this.ring.Width, surface.Y + 8 + (this.ContactText.Length > 0 ? 30 : 0));

                // Cobrar/agregar/terminar arriba; imprimir/cancelar/expandir abajo. Blancos tactiles de 44 px.
                bool pending = this.stage != DeliveryStage.Billed;
                int secondaryTop = this.buttonsTop + (pending ? ButtonHeight + 6 : 0);
                int x = right - ExpandWidth;
                this.expandButton.Bounds = new Rectangle(x, secondaryTop, ExpandWidth, ButtonHeight);
                x -= 6;
                if (this.cancelButton.Visible)
                {
                    x -= ButtonHeight;
                    this.cancelButton.Location = new Point(x, secondaryTop);
                    x -= 6;
                }
                x -= ButtonHeight;
                this.printButton.Location = new Point(x, secondaryTop);

                int primaryRight = right;
                if (this.finishButton.Visible)
                {
                    this.finishButton.Location = new Point(right - ButtonHeight, this.buttonsTop);
                    primaryRight -= ButtonHeight + 6;
                }
                int textWidth = Math.Min(116, (primaryRight - left - 6) / 2);
                bool compact = textWidth < 92;
                this.billButton.Text = compact ? string.Empty : "Facturar";
                this.addButton.Text = compact ? string.Empty : "Agregar";
                if (compact) textWidth = ButtonHeight;
                this.billButton.Bounds = new Rectangle(left, this.buttonsTop, textWidth, ButtonHeight);
                this.addButton.Bounds = new Rectangle(left + textWidth + 6, this.buttonsTop, textWidth, ButtonHeight);

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
            // Fuera de la superficie se ve la lista: se limpia con su color, no con el blanco de la tarjeta.
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

                // Franja lateral recortada con la forma de la tarjeta para respetar las esquinas.
                GraphicsState state = g.Save();
                g.SetClip(path);
                using (SolidBrush bar = new SolidBrush(this.accent))
                    g.FillRectangle(bar, surface.X, surface.Y, AccentWidth, surface.Height + 1);
                g.Restore(state);
            }

            this.Compose(g);
        }

        /// <summary>
        /// Recorre el contenido de arriba abajo. Con <paramref name="g"/> lo dibuja; con null solo mide. Es un
        /// unico recorrido para que lo medido y lo dibujado no puedan desajustarse. Devuelve la altura total.
        /// </summary>
        private int Compose(Graphics g)
        {
            Rectangle surface = this.SurfaceBounds;
            int left = surface.X + AccentWidth + PaddingX;
            int right = surface.Right - PaddingX;
            bool hasRing = this.stage == DeliveryStage.Preparing;
            // Junto al anillo el texto es mas angosto; debajo de el vuelve a ocupar todo el ancho.
            int headWidth = Math.Max(60, (hasRing ? right - this.ring.Width - 12 : right) - left);
            int fullWidth = Math.Max(60, right - left);
            int y = surface.Y + 8;

            // Numero de pedido y pildora de estado.
            string number = "#" + this.order.Id_pedido;
            Size numberSize = new Size(LineWidth(number, Theme.Heading), 24);
            DrawText(g, number, Theme.Heading, Theme.TextPrimary, new Rectangle(left, y, numberSize.Width + 2, 24), SingleLine);
            bool hasContact = this.ContactText.Length > 0;
            if (hasContact)
            {
                int contactLeft = left + numberSize.Width + 10;
                DeliveryListLayout.DrawContact(g, this.details.Phone,
                    new Rectangle(contactLeft, y, Math.Max(10, right - contactLeft), 24));
                y += 28;
            }

            // Numero de la factura ligada (el mismo que lleva impreso arriba a la derecha), junto al numero del pedido.
            int tagWidth = 0;
            if (this.details.Number.HasValue)
            {
                string tag = "N° " + this.details.Number.Value;
                tagWidth = LineWidth(tag, Theme.SmallBold) + 18;
                DrawChip(g, new Rectangle(hasContact ? left : left + numberSize.Width + 10, y + 1, tagWidth, 22), tag);
            }

            string stageText = this.StageText();
            int pillWidth = LineWidth(stageText, Theme.SmallBold) + 32;
            int pillX = (hasContact ? left : left + numberSize.Width + 10) + (tagWidth > 0 ? tagWidth + 8 : 0);
            if (pillX + pillWidth > left + headWidth)
            {
                // No cabe al lado del numero: baja a su propia linea.
                y += 26;
                pillX = left;
            }
            DrawPill(g, new Rectangle(pillX, y + 1, pillWidth, 22), stageText, this.accent);
            y += 28;

            if (!string.IsNullOrWhiteSpace(this.details.CustomerName))
                y += DrawText(g, this.details.CustomerName.Trim(), Theme.BodyBold, Theme.TextPrimary,
                    new Rectangle(left, y, headWidth, 0), Wrapped) + 3;

            // Plegada: solo la hora y el numero de productos; el resto aparece con Expandir.
            string summary = "Pedido a las " + FormatOrderTime(this.order.Hora_pedido);
            if (!this.expanded && this.details.Items.Count > 0)
                summary += " · " + this.details.Items.Count + (this.details.Items.Count == 1 ? " producto" : " productos");
            if (this.expanded)
            {
                y += this.IconRow(g, IconChar.LocationDot, this.details.Address, left, y, headWidth);
                y += DeliveryListLayout.DrawContact(g, this.details.Phone, new Rectangle(left, y, headWidth, 22));
            }
            y += this.IconRow(g, IconChar.Clock, summary, left, y, headWidth);

            if (hasRing)
                y = Math.Max(y, surface.Y + 8 + (hasContact ? 30 : 0) + this.ring.Height + 2);

            if (this.expanded)
                y = this.ComposeDetail(g, left, right, fullWidth, y + 4);

            y += 2;
            this.buttonsTop = y;
            if (this.stage != DeliveryStage.Billed) y += ButtonHeight + 6;
            y += ButtonHeight + 8;
            return y + ShadowDepth + 1;
        }

        private int ComposeCompact(Graphics g)
        {
            // Misma distribucion en todas las etapas (la de "Terminados"): insignia del numero del dia, #pedido y
            // cliente en la primera fila; telefono y direccion en la segunda, con el estado a la derecha. En
            // preparacion ese estado es el temporizador (en el color del anillo).
            string status;
            if (this.stage == DeliveryStage.Preparing)
                status = DeliveryListLayout.Time(this.ring.Remaining);
            else
                status = this.stage == DeliveryStage.Billed ? string.Empty : this.StageText();
            DeliveryListLayout.DrawFinished(g, this.Width, this.accent,
                this.details.Number.HasValue ? "N" + this.details.Number.Value : null,
                "#" + this.order.Id_pedido, string.IsNullOrWhiteSpace(this.details.CustomerName) ? "Sin cliente" : this.details.CustomerName.Trim(),
                this.details.Phone, this.details.Address, status);
            if (!this.compactExpanded) return DeliveryListLayout.Height;
            int y = DeliveryListLayout.Height + 4;
            int right = this.Width - 12;
            y += DrawText(g, "Pedido #" + this.order.Id_pedido + " · " + FormatOrderTime(this.order.Hora_pedido), Theme.SmallBold,
                Theme.TextPrimary, new Rectangle(12, y, right - 12, 0), Wrapped) + 6;
            y += this.IconRow(g, IconChar.LocationDot, this.details.Address, 12, y, right - 12);
            y += DeliveryListLayout.DrawContact(g, this.details.Phone, new Rectangle(12, y, right - 12, 22));
            return this.ComposeDetail(g, 12, right, right - 12, y + 4) + 8;
        }

        // Productos y observaciones; solo se recorre con la tarjeta expandida.
        private int ComposeDetail(Graphics g, int left, int right, int fullWidth, int y)
        {
            Divider(g, left, right, y);
            y += 8;

            y += DrawText(g, this.details.PaymentsLoaded && !this.details.PaymentsFinal ? "Medio de pago previsto" : "Medios de pago", Theme.SmallBold,
                Theme.TextPrimary, new Rectangle(left, y, fullWidth, 0), Wrapped) + 4;
            if (!this.details.PaymentsLoaded || this.details.Payments.Count == 0)
                y += DrawText(g, this.details.PaymentsLoaded ? "Sin medio de pago asignado" : "No se pudieron consultar los medios de pago",
                    Theme.Small, Theme.TextMuted, new Rectangle(left, y, fullWidth, 0), Wrapped) + 4;
            foreach (DeliveryPayment payment in this.details.Payments)
            {
                string amount = payment.Amount.HasValue ? payment.Amount.Value.ToString("C0", CultureInfo.GetCultureInfo("es-CO")) : string.Empty;
                int amountWidth = amount.Length == 0 ? 0 : LineWidth(amount, Theme.SmallBold) + 8;
                int height = Math.Max(22, DrawText(g, payment.Method, Theme.Small, Theme.TextMuted,
                    new Rectangle(left, y, Math.Max(40, fullWidth - amountWidth), 0), Wrapped));
                if (amountWidth > 0)
                    DrawText(g, amount, Theme.SmallBold, Theme.TextPrimary, new Rectangle(right - amountWidth, y, amountWidth, height),
                        SingleLine | TextFormatFlags.Right);
                y += height + 2;
            }
            y += 4;
            Divider(g, left, right, y);
            y += 8;

            // Productos: cantidad en una ficha y el nombre al lado.
            foreach (DeliveryItem item in this.details.Items)
            {
                int nameWidth = fullWidth - ChipWidth - 10;
                int nameHeight = DrawText(g, item.Name, Theme.Body, Theme.TextPrimary,
                    new Rectangle(left + ChipWidth + 10, y + 2, nameWidth, 0), Wrapped);
                DrawChip(g, new Rectangle(left, y, ChipWidth, 22), item.Quantity + "×");
                int rowHeight = Math.Max(22, nameHeight + 2);
                if (!string.IsNullOrWhiteSpace(item.Notes))
                    rowHeight += DrawText(g, item.Notes.Trim(), Theme.Small, Theme.TextMuted,
                        new Rectangle(left + ChipWidth + 10, y + rowHeight + 1, nameWidth, 0), Wrapped) + 1;
                y += rowHeight + 4;
            }
            if (!this.details.Loaded)
                y += DrawText(g, "No se pudo cargar el detalle del pedido. Actualice para reintentar.", Theme.Small,
                    Theme.Darken(Theme.Warning, 0.35f), new Rectangle(left, y, fullWidth, 0), Wrapped) + 6;

            // Observaciones del pedido, en un recuadro suave para que no se confundan con un producto.
            if (!string.IsNullOrWhiteSpace(this.details.Notes))
            {
                string notes = this.details.Notes.Trim();
                int notesWidth = fullWidth - 42;
                int notesHeight = Measure(notes, Theme.Small, notesWidth);
                Rectangle box = new Rectangle(left, y + 2, fullWidth, Math.Max(18, notesHeight) + 14);
                if (g != null)
                {
                    using (GraphicsPath path = Theme.RoundedPath(box, 8))
                    using (SolidBrush fill = new SolidBrush(Theme.Lighten(Theme.Warning, 0.90f)))
                        g.FillPath(fill, path);
                    IconRenderer.Draw(this, g, IconChar.NoteSticky, Theme.Darken(Theme.Warning, 0.25f),
                        new Rectangle(box.X + 10, box.Y + 8, 14, 14));
                }
                DrawText(g, notes, Theme.Small, Theme.Darken(Theme.Warning, 0.50f),
                    new Rectangle(box.X + 32, box.Y + 7, notesWidth, 0), Wrapped);
                y = box.Bottom + 6;
            }
            return y;
        }

        // Ancho de una linea sin recortar. No se mide con SingleLine: su EndEllipsis, sin un ancho propuesto,
        // devuelve el ancho del texto ya recortado y el numero salia como "#4...".
        private static int LineWidth(string text, Font font) =>
            TextRenderer.MeasureText(text, font, new Size(int.MaxValue, int.MaxValue),
                TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine).Width + 2;

        private static int Measure(string text, Font font, int width) =>
            TextRenderer.MeasureText(text, font, new Size(Math.Max(1, width), int.MaxValue), Wrapped).Height;

        /// <summary>Dibuja (si hay donde) y devuelve la altura que ocupa; con altura 0 en el rectangulo, la mide.</summary>
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
            int height = DrawText(g, text.Trim(), Theme.Small, Theme.Neutral,
                new Rectangle(left + 22, y, width - 22, 0), Wrapped);
            return Math.Max(17, height) + 5;
        }

        private static void Divider(Graphics g, int left, int right, int y)
        {
            if (g == null) return;
            using (Pen pen = new Pen(Theme.Border))
                g.DrawLine(pen, left, y, right, y);
        }

        // Pildora de estado: punto de color y texto sobre un fondo suave del mismo tono.
        private static void DrawPill(Graphics g, Rectangle bounds, string text, Color color)
        {
            if (g == null) return;
            using (GraphicsPath path = Theme.RoundedPath(bounds, bounds.Height / 2))
            using (SolidBrush fill = new SolidBrush(Theme.Lighten(color, 0.88f)))
            using (SolidBrush dot = new SolidBrush(color))
            {
                g.FillPath(fill, path);
                g.FillEllipse(dot, bounds.X + 9, bounds.Y + bounds.Height / 2 - 3, 7, 7);
            }
            TextRenderer.DrawText(g, text, Theme.SmallBold,
                new Rectangle(bounds.X + 21, bounds.Y, bounds.Width - 24, bounds.Height),
                Theme.Darken(color, 0.35f), SingleLine);
        }

        private static void DrawChip(Graphics g, Rectangle bounds, string text)
        {
            if (g == null) return;
            using (GraphicsPath path = Theme.RoundedPath(bounds, 7))
            using (SolidBrush fill = new SolidBrush(Theme.Lighten(Theme.Primary, 0.90f)))
                g.FillPath(fill, path);
            TextRenderer.DrawText(g, text, Theme.SmallBold, bounds, Theme.Darken(Theme.Primary, 0.15f),
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding
                | TextFormatFlags.NoPrefix);
        }

        private static string FormatOrderTime(string value)
        {
            TimeSpan time;
            if (TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out time)
                && time >= TimeSpan.Zero && time < TimeSpan.FromDays(1))
                return DateTime.Today.Add(time).ToString("h:mm tt", CultureInfo.InvariantCulture);
            return value;
        }
        #endregion

        #region ACCIONES
        private void AddButton_Click(object sender, EventArgs e)
        {
            if (!"PENDIENTE".Equals(this.order.Estado_pedido))
                return;

            // FrmPedido es el formulario vigente (FrmRealizarPedido ya no guarda pedidos).
            DatosInicioSesion session = DatosInicioSesion.GetInstancia();
            FrmPedido form = new FrmPedido
            {
                StartPosition = FormStartPosition.CenterScreen,
                Numero_mesa = 0,
                Tipo_servicio = "DOMICILIO",
                IsDomicilio = true,
                EmpleadoSelected = session.EmpleadoClaveMaestra,
                ClienteSelected = this.order.Cliente,
                MesaSelected = this.order.Mesa,
                WindowState = FormWindowState.Maximized,
                Pedido = this.order,
                IsEditar = true,
            };
            form.ShowDialog();
            this.Changed?.Invoke(this.order, EventArgs.Empty);
        }

        private void PrintButton_Click(object sender, EventArgs e)
        {
            int copies = Math.Max(0, Math.Min(99, this.GetPrintCopies?.Invoke() ?? 1));
            if (copies == 0) return;
            MensajeEspera.ShowWait("Imprimiendo...");
            try
            {
                if ("PENDIENTE".Equals(this.order.Estado_pedido))
                {
                    // Sin facturar solo hay comandas que reimprimir.
                    FrmComandas kitchenTickets = new FrmComandas { Id_pedido = this.order.Id_pedido };
                    kitchenTickets.ObtenerReporte("Casa Grande | DOMICILIO");
                    kitchenTickets.AsignarTablas();
                    kitchenTickets.ImprimirFactura(copies);
                }
                else
                {
                    FrmFacturaFinal invoice = new FrmFacturaFinal { Id_pedido = this.order.Id_pedido };
                    invoice.AsignarReporte();
                    invoice.AsignarTablasCuentaFinal();
                    invoice.ImprimirFactura(copies);
                }
            }
            finally
            {
                MensajeEspera.CloseForm();
            }
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            // Un domicilio en preparacion ya esta guardado y en cocina: cancelarlo exige el codigo maestro y un
            // comentario obligatorio, igual que quitar un producto ya guardado de un pedido.
            if (this.stage == DeliveryStage.Preparing)
            {
                this.CancelWithAuthorization();
                return;
            }

            this.cancelNote.txtObservacion.Clear();
            Mensajes.MensajePregunta("¿Está seguro que desea cancelar el domicilio?", "Si", "No", out DialogResult answer);
            if (answer == DialogResult.Yes)
                this.ShowCancelNote();
        }

        private void CancelWithAuthorization()
        {
            string observation;
            using (CapaPresentacion.Formularios.FormsPedido.FrmRemoveProductAuthorization authorization =
                new CapaPresentacion.Formularios.FormsPedido.FrmRemoveProductAuthorization(
                    "Cancelar domicilio", "Cancelar el domicilio #" + this.order.Id_pedido,
                    "Cancelar domicilio", IconChar.Trash, "Volver"))
            {
                if (authorization.ShowDialog(this.FindForm()) != DialogResult.OK)
                    return;
                observation = authorization.Observation;
            }

            string result = NPedido.CancelarPedido(this.order.Id_pedido, observation);
            if (result.Equals("OK"))
            {
                Mensajes.MensajeOkForm("Se canceló correctamente el domicilio");
                this.Changed?.Invoke(this.order, EventArgs.Empty);
            }
            else
                Mensajes.MensajeInformacion("No se pudo cancelar el domicilio: " + result, "Entendido");
        }

        private void ShowCancelNote()
        {
            this.cancelPopup = new PoperContainer(this.cancelNote);
            this.cancelPopup.Show(this.compactView ? this.moreButton : this.cancelButton);
        }

        private void CancelNote_Save(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.cancelNote.txtObservacion.Text))
            {
                Mensajes.MensajeInformacion("Debe justificar la cancelación del domicilio", "Entendido");
                this.ShowCancelNote();
                return;
            }

            string result = NPedido.CancelarPedido(this.order.Id_pedido, this.cancelNote.txtObservacion.Text);
            if (result.Equals("OK"))
            {
                Mensajes.MensajeOkForm("Se canceló correctamente el domicilio");
                this.Changed?.Invoke(this.order, EventArgs.Empty);
            }
        }
        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.toolTip.Dispose();
                this.compactMenu.Dispose();
                this.cancelPopup?.Dispose();
                this.cancelNote.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
