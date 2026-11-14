using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CapaEntidades.Models;
using CapaNegocio;
using CapaPresentacion.Formularios.FormsPedido;
using CapaPresentacion.UI;
using FontAwesome.Sharp;

namespace CapaPresentacion.Formularios.FormsDomicilios
{
    /// <summary>
    /// Pagina de Domicilios: KPIs arriba y tres paneles (EN PREPARACION, TERMINADOS, RAPPI).
    /// Los domicilios son pedidos de Pedidos con Tipo_pedido = 'DOMICILIO' e Id_mesa = 0.
    /// Es la pagina de referencia del tema moderno (carpeta UI): para formularios nuevos, copiar este estilo.
    /// </summary>
    public class FrmDeliveries : Form
    {
        #region SINGLETON
        private static FrmDeliveries _instance;

        public static FrmDeliveries GetInstance()
        {
            if (_instance == null || _instance.IsDisposed)
                _instance = new FrmDeliveries();
            return _instance;
        }
        #endregion

        private readonly KpiCard kpiPreparing = new KpiCard
        {
            Caption = "EN PREPARACIÓN", Icon = IconChar.Fire, Accent = Theme.Danger, Dock = DockStyle.Fill,
        };
        private readonly KpiCard kpiFinished = new KpiCard
        {
            Caption = "TERMINADOS", Icon = IconChar.CircleCheck, Accent = Theme.Success, Dock = DockStyle.Fill,
        };
        private readonly KpiCard kpiRappi = new KpiCard
        {
            Caption = "RAPPI", Icon = IconChar.Motorcycle, Accent = Theme.Rappi, Dock = DockStyle.Fill,
        };
        private readonly RappiPanel rappiPanel = new RappiPanel();
        private readonly FlowLayoutPanel listPreparing = NewList();
        private readonly FlowLayoutPanel listFinished = NewList();
        private readonly Badge preparingCount = NewCounter(BadgeKind.Danger);
        private readonly Badge finishedCount = NewCounter(BadgeKind.Success);
        private readonly Label lblSubtitle = new Label();
        private readonly Timer refreshTimer = new Timer { Interval = 10000 };
        // Mueve las cuentas regresivas de las tarjetas; no consulta la base salvo cuando una se agota.
        private readonly Timer clockTimer = new Timer { Interval = 1000 };

        // Huella de lo que se ve; si no cambia, el refresco periodico no reconstruye las tarjetas.
        private string signature = string.Empty;

        private FrmDeliveries()
        {
            this.Text = "Domicilios";
            this.BackColor = Theme.Background;
            this.Font = Theme.Body;
            this.DoubleBuffered = true;

            Panel root = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 16, 20, 16) };

            // Orden de Add: el ultimo agregado se acopla primero, asi que Fill va antes que los Top.
            root.Controls.Add(this.BuildColumns());
            root.Controls.Add(this.BuildKpiRow());
            root.Controls.Add(this.BuildHeader());
            this.Controls.Add(root);

            this.listPreparing.SizeChanged += (s, e) => FitCards(this.listPreparing);
            this.listFinished.SizeChanged += (s, e) => FitCards(this.listFinished);
            this.Load += (s, e) => this.RefreshDeliveries(true);
            this.VisibleChanged += (s, e) => { if (this.Visible) this.RefreshDeliveries(false); };
            // El panel RAPPI lee solo las facturas PDF de su carpeta; el indicador superior cuenta las ordenes de hoy.
            this.rappiPanel.TodayCountChanged += (s, count) => this.kpiRappi.Value = count.ToString();
            this.Load += (s, e) => this.rappiPanel.Start();
            this.refreshTimer.Tick += (s, e) => { if (this.Visible) this.RefreshDeliveries(false); };
            this.refreshTimer.Start();
            this.clockTimer.Tick += this.ClockTimer_Tick;
            this.clockTimer.Start();
            this.FormClosed += (s, e) => { this.refreshTimer.Dispose(); this.clockTimer.Dispose(); this.rappiPanel.Stop(); };
        }

        #region CONSTRUCCION DE LA PANTALLA
        private static FlowLayoutPanel NewList() => new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            BackColor = Theme.Surface,
            Padding = new Padding(0, 4, 0, 4),
        };

        // Contador de pedidos junto al titulo de cada columna.
        private static Badge NewCounter(BadgeKind kind) => new Badge
        {
            Kind = kind,
            Text = "0",
            Size = new Size(40, 24),
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
        };

        private Control BuildHeader()
        {
            Panel header = new Panel { Dock = DockStyle.Top, Height = 64 };

            Label title = new Label { Text = "Domicilios", AutoSize = true, Location = new Point(0, 0) };
            title.StyleLabel(LabelKind.Title);

            this.lblSubtitle.AutoSize = true;
            this.lblSubtitle.Location = new Point(2, 36);
            this.lblSubtitle.StyleLabel(LabelKind.Muted);
            this.lblSubtitle.Text = DateTime.Now.ToString("dddd d 'de' MMMM", new CultureInfo("es-CO"));

            ModernButton refresh = new ModernButton
            {
                Text = "Actualizar",
                Kind = ButtonKind.Outline,
                Icon = IconChar.ArrowsRotate,
                Size = new Size(140, 40),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
            };
            refresh.Click += (s, e) => this.RefreshDeliveries(true);

            FlowLayoutPanel actions = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                Width = 366,
                Padding = new Padding(0, 6, 0, 0),
                WrapContents = false,
            };
            ModernButton newOrder = new ModernButton
            {
                Text = "REALIZAR PEDIDO",
                Kind = ButtonKind.Success,
                Icon = IconChar.Plus,
                Size = new Size(210, 40),
                Margin = new Padding(0, 0, 12, 0),
            };
            newOrder.Click += this.BtnNewOrder_Click;
            refresh.Margin = Padding.Empty;
            refresh.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            actions.Controls.Add(newOrder);
            actions.Controls.Add(refresh);

            header.Controls.Add(title);
            header.Controls.Add(this.lblSubtitle);
            header.Controls.Add(actions);
            return header;
        }

        private Control BuildKpiRow()
        {
            TableLayoutPanel row = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 72,
                ColumnCount = 3,
                RowCount = 1,
            };
            for (int i = 0; i < 3; i++)
                row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / 3));
            row.Controls.Add(this.kpiPreparing, 0, 0);
            row.Controls.Add(this.kpiFinished, 1, 0);
            row.Controls.Add(this.kpiRappi, 2, 0);
            return row;
        }

        private Control BuildColumns()
        {
            TableLayoutPanel columns = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
            };
            for (int i = 0; i < 3; i++)
                columns.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / 3));

            columns.Controls.Add(BuildColumn("EN PREPARACIÓN", IconChar.Fire, Theme.Danger, this.listPreparing,
                this.preparingCount), 0, 0);
            columns.Controls.Add(BuildColumn("TERMINADOS", IconChar.CircleCheck, Theme.Success, this.listFinished,
                this.finishedCount), 1, 0);
            columns.Controls.Add(BuildRappiColumn(), 2, 0);
            return columns;
        }

        private static Control Header(string title, IconChar icon, Color color, Badge counter = null)
        {
            Panel header = new Panel { Dock = DockStyle.Top, Height = 40, BackColor = Theme.Surface };
            if (counter != null)
            {
                // Se agrega antes que el titulo para que quede por encima de su etiqueta, que es mas ancha.
                counter.Location = new Point(header.Width - counter.Width - 4, 8);
                header.Controls.Add(counter);
            }
            IconPictureBox glyph = new IconPictureBox
            {
                IconChar = icon,
                IconColor = color,
                IconSize = 22,
                BackColor = Theme.Surface,
                Size = new Size(28, 28),
                Location = new Point(2, 8),
            };
            Label label = new Label
            {
                Text = title,
                AutoSize = false,
                Location = new Point(34, 0),
                Size = new Size(300, 40),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Theme.Surface,
            };
            label.StyleLabel(LabelKind.Heading);
            header.Controls.Add(label);
            header.Controls.Add(glyph);
            return header;
        }

        private static ModernCard BuildColumn(string title, IconChar icon, Color color, FlowLayoutPanel list,
            Badge counter)
        {
            ModernCard card = new ModernCard { Dock = DockStyle.Fill, AccentColor = color };
            list.BackColor = Theme.Surface;
            card.Controls.Add(list);
            card.Controls.Add(Header(title, icon, color, counter));
            return card;
        }

        private Control BuildRappiColumn()
        {
            ModernCard card = new ModernCard { Dock = DockStyle.Fill, AccentColor = Theme.Rappi };
            card.Controls.Add(this.rappiPanel);
            card.Controls.Add(Header("RAPPI", IconChar.Motorcycle, Theme.Rappi));
            return card;
        }
        #endregion

        #region DATOS
        // Domicilios de hoy. En preparacion = PENDIENTE con tiempo por delante; terminados = facturados y los
        // PENDIENTE que agotaron su tiempo de preparacion (los cancelados no cuentan). Ver DeliveryTiming.
        private static List<Pedidos> Query(string searchType)
        {
            DataTable table = NPedido.BuscarPedidos(searchType, DateTime.Now.ToString("yyyy-MM-dd"));
            List<Pedidos> result = new List<Pedidos>();
            if (table != null)
            {
                foreach (DataRow row in table.Rows)
                    result.Add(new Pedidos(row));
            }
            return result;
        }

        public void RefreshDeliveries(bool force)
        {
            try
            {
                DateTime now = DateTime.Now;
                List<Pedidos> pending = Query("DOMICILIOS PENDIENTES");

                // Los que agotaron el tiempo y tienen forma de pago se facturan solos (venta + estado Facturado), tambien
                // los que vencieron con el programa cerrado. Los demas quedan como "Por facturar" para hacerlo a mano.
                bool autoBilled = false;
                foreach (Pedidos expired in pending.Where(p => DeliveryTiming.StageOf(p, now) == DeliveryStage.AwaitingBilling).ToList())
                {
                    try { autoBilled |= DeliveryAutoBilling.TryBill(expired); }
                    catch (Exception) { /* queda por facturar a mano; el siguiente refresco lo reintenta */ }
                }
                if (autoBilled)
                    pending = Query("DOMICILIOS PENDIENTES");

                // El mas cercano a agotarse va arriba.
                List<Pedidos> preparing = pending
                    .Where(p => DeliveryTiming.StageOf(p, now) == DeliveryStage.Preparing)
                    .OrderBy(p => DeliveryTiming.Remaining(p, now)).ToList();
                // Los que agotaron el tiempo siguen PENDIENTE en la base: van primero para que no se queden sin facturar.
                List<Pedidos> finished = pending
                    .Where(p => DeliveryTiming.StageOf(p, now) == DeliveryStage.AwaitingBilling)
                    .Concat(Query("DOMICILIOS OTROS").Where(p => !"CANCELADO".Equals(p.Estado_pedido)))
                    .ToList();

                this.kpiPreparing.Value = preparing.Count.ToString();
                this.kpiFinished.Value = finished.Count.ToString();
                this.preparingCount.Text = preparing.Count.ToString();
                this.finishedCount.Text = finished.Count.ToString();
                this.lblSubtitle.Text = now.ToString("dddd d 'de' MMMM", new CultureInfo("es-CO"))
                    + "  ·  actualizado " + now.ToString("h:mm tt", CultureInfo.InvariantCulture)
                    + "  ·  preparación " + (int)DeliveryTiming.PreparationTime.TotalMinutes + " min";

                // La huella separa los dos paneles: un pedido que cambia de panel la cambia aunque siga PENDIENTE.
                string newSignature = string.Join("|", preparing.Select(p => p.Id_pedido + p.Estado_pedido))
                    + "#" + string.Join("|", finished.Select(p => p.Id_pedido + p.Estado_pedido));
                if (!force && newSignature == this.signature)
                    return;
                this.signature = newSignature;

                this.Fill(this.listPreparing, preparing, "No hay domicilios en preparación", now);
                this.Fill(this.listFinished, finished, "Aún no hay domicilios terminados hoy", now);
            }
            catch (Exception ex)
            {
                // Se incluye el tipo y la pila: hubo un caso con Message vacio que dejaba el aviso sin detalle.
                // Los temporizadores se detienen mientras el aviso esta abierto para no apilar un aviso por tic.
                this.refreshTimer.Stop();
                this.clockTimer.Stop();
                Mensajes.MensajeErrorCompleto("FrmDeliveries", "RefreshDeliveries",
                    "Hubo un error al cargar los domicilios", ex.GetType().Name + ": " + ex.Message + Environment.NewLine + ex.StackTrace);
                this.refreshTimer.Start();
                this.clockTimer.Start();
            }
        }

        // Cada segundo avanza las cuentas regresivas; cuando una se agota, el pedido pasa a TERMINADOS.
        private void ClockTimer_Tick(object sender, EventArgs e)
        {
            if (!this.Visible)
                return;

            DateTime now = DateTime.Now;
            bool expired = false;
            foreach (DeliveryCard card in this.listPreparing.Controls.OfType<DeliveryCard>())
                expired |= card.Tick(now);
            if (expired)
                this.RefreshDeliveries(true);
        }

        private void Fill(FlowLayoutPanel list, List<Pedidos> orders, string emptyMessage, DateTime now)
        {
            list.SuspendLayout();
            foreach (Control old in list.Controls.Cast<Control>().ToList())
            {
                list.Controls.Remove(old);
                old.Dispose();
            }

            if (orders.Count == 0)
            {
                Label empty = new Label
                {
                    Text = emptyMessage,
                    AutoSize = false,
                    Size = new Size(Math.Max(200, list.ClientSize.Width - 8), 80),
                    TextAlign = ContentAlignment.MiddleCenter,
                };
                empty.StyleLabel(LabelKind.Muted);
                list.Controls.Add(empty);
            }

            foreach (Pedidos order in orders)
            {
                // La tarjeta ocupa el ancho de la columna y calcula sola su altura.
                DeliveryCard card = new DeliveryCard { Width = CardWidth(list) };
                card.Bind(order, DeliveryDetails.Load(order), DeliveryTiming.StageOf(order, now), now);
                card.BillRequested += this.Card_OnBtnNextClick;
                card.Changed += (s, e) => this.RefreshDeliveries(true);
                list.Controls.Add(card);
            }
            list.ResumeLayout();
        }

        // Al cambiar el tamano de la ventana las tarjetas siguen el ancho de su columna.
        private static void FitCards(FlowLayoutPanel list)
        {
            foreach (Control card in list.Controls)
                card.Width = CardWidth(list);
        }

        // Se reserva siempre el ancho de la barra vertical: si no, al aparecer rebasa la lista y sale la horizontal.
        private static int CardWidth(FlowLayoutPanel list) =>
            Math.Max(280, list.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - 4);
        #endregion

        #region ACCIONES
        private void BtnNewOrder_Click(object sender, EventArgs e)
        {
            Clientes customer;
            CustomerAddress address;
            using (FrmDeliveryCustomer picker = new FrmDeliveryCustomer())
            {
                if (picker.ShowDialog(this) != DialogResult.OK)
                    return;
                customer = picker.Customer;
                address = picker.SelectedAddress;
            }

            // Mesa 0 es el comodin de los domicilios; Id_cliente y direccion salen del paso anterior.
            FrmPedido form = new FrmPedido
            {
                StartPosition = FormStartPosition.CenterScreen,
                Numero_mesa = 0,
                Tipo_servicio = "DOMICILIO",
                IsDomicilio = true,
                ClienteSelected = customer,
                AddressId = address.AddressId,
                AddressText = address.Summary,
                MesaSelected = new Mesas { Id_mesa = 0, Num_mesa = 0, Descripcion_mesa = string.Empty },
                WindowState = FormWindowState.Maximized,
            };
            form.FormClosed += (s, args) => this.RefreshDeliveries(true);
            form.Show();
        }

        private void Card_OnBtnNextClick(object sender, EventArgs e)
        {
            Pedidos order = (Pedidos)sender;
            if (!"PENDIENTE".Equals(order.Estado_pedido))
                return;
            FrmFacturarPedido billing = new FrmFacturarPedido
            {
                StartPosition = FormStartPosition.CenterScreen,
                IsPrecuenta = false,
                IsDomicilio = true,
            };
            billing.ObtenerPedido(order.Id_pedido);
            billing.OnFacturarPedidoSuccess += this.Billing_OnFacturarPedidoSuccess;
            billing.Show();
        }

        private void Billing_OnFacturarPedidoSuccess(object sender, EventArgs e)
        {
            int orderId = (int)sender;
            string result = NPedido.CambiarEstadoPedido(new List<string> { orderId.ToString(), "DISPONIBLE", "0" });
            if (!result.Equals("OK"))
                Mensajes.MensajeInformacion("Hubo un error al actualizar el estado del pedido", "Entendido");
            this.RefreshDeliveries(true);
        }
        #endregion
    }
}
