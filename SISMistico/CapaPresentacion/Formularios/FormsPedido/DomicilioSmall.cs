using CapaEntidades.Models;
using CapaNegocio;
using CapaPresentacion.Formularios.Controles;
using CapaPresentacion.UI;
using FontAwesome.Sharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CapaPresentacion.Formularios.FormsPedido
{
    public partial class DomicilioSmall : UserControl
    {
        Observacion observaciones = new Observacion();
        PoperContainer container;
        private Panel infoHost;
        private readonly List<Control> badges = new List<Control>();
        public int ContentHeight { get; private set; }
        public event EventHandler ContentHeightChanged;
        public DomicilioSmall()
        {
            InitializeComponent();
            this.ApplyModernLayout();
            this.observaciones.btnSave.Click += BtnSave_Click;
            this.observaciones.btnSave.Visible = true;
            this.btnNext.Click += BtnNext_Click;
            this.btnPrint.Click += BtnPrint_Click;
            this.btnAddProductos.Click += BtnAddProductos_Click;
            this.btnCancelar.Click += BtnCancelar_Click;
        }

        // Tarjeta fluida con el tema moderno: franja de estado, badges en cuadricula y botones abajo. Los
        // botones van en un FlowLayoutPanel para que, al ocultarse unos segun el estado, el resto se reacomode.
        private void ApplyModernLayout()
        {
            this.BackColor = Theme.Surface;
            this.ForeColor = Theme.TextPrimary;
            this.Font = Theme.Body;

            this.panel1.Dock = DockStyle.Top;
            this.panel1.Height = 6;

            // El marco "Informacion" del diseno anterior se reemplaza por el texto directo sobre la tarjeta.
            this.groupBox1.Visible = false;
            infoHost = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Surface };
            infoHost.SizeChanged += (s, e) => LayoutBadges();

            FlowLayoutPanel actions = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 40,
                Padding = new Padding(8, 4, 8, 4),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Theme.Surface,
            };
            this.StyleAction(this.btnNext, actions, "Facturar", ButtonKind.Success, IconChar.FileInvoiceDollar, 96);
            this.StyleAction(this.btnAddProductos, actions, "Agregar", ButtonKind.Primary, IconChar.Plus, 88);
            this.StyleAction(this.btnPrint, actions, string.Empty, ButtonKind.Outline, IconChar.Print, 30);
            this.StyleAction(this.btnCancelar, actions, string.Empty, ButtonKind.Danger, IconChar.Xmark, 30);

            this.Controls.Add(infoHost);
            this.Controls.Add(actions);
            this.panel1.BringToFront();
            this.toolTip1.SetToolTip(this.btnNext, "Facturar domicilio");
        }

        // La altura se mide sin crear handles; el contenedor desplaza la lista completa de tarjetas.
        private void LayoutBadges()
        {
            int width = Math.Max(80, infoHost.ClientSize.Width - 16);
            int y = 6;
            for (int i = 0; i < badges.Count; i++)
            {
                Control badge = badges[i];
                bool pair = i < 4 && i + 1 < badges.Count;
                int cellWidth = pair ? (width - 4) / 2 : width;
                int height = BadgeHeight(badge, cellWidth);
                if (pair) height = Math.Max(height, BadgeHeight(badges[i + 1], cellWidth));
                badge.Bounds = new Rectangle(8, y, cellWidth, height);
                if (pair) badges[++i].Bounds = new Rectangle(12 + cellWidth, y, width - cellWidth - 4, height);
                y += height + 4;
            }
            int desired = y + 48;
            if (ContentHeight == desired) return;
            ContentHeight = desired;
            ContentHeightChanged?.Invoke(this, EventArgs.Empty);
        }

        private static int BadgeHeight(Control badge, int width) =>
            TextRenderer.MeasureText(badge.Text, badge.Font, new Size(Math.Max(1, width - 16), int.MaxValue),
                TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix).Height + 8;

        private void AddBadge(string caption, string value, Color accent)
        {
            if (string.IsNullOrWhiteSpace(value)) return;
            Label badge = new Label
            {
                Text = caption + " · " + value,
                Font = Theme.Small,
                ForeColor = Theme.Darken(accent, 0.25f),
                BackColor = Theme.Lighten(accent, 0.93f),
                Padding = new Padding(8, 4, 8, 4),
                UseMnemonic = false,
                AutoSize = false,
            };
            badge.Paint += (s, e) =>
            {
                using (var path = Theme.RoundedPath(new Rectangle(0, 0, badge.Width - 1, badge.Height - 1), 8))
                using (var pen = new Pen(Theme.Lighten(accent, 0.75f)))
                    e.Graphics.DrawPath(pen, path);
            };
            badge.SizeChanged += (s, e) =>
            {
                using (var path = Theme.RoundedPath(new Rectangle(0, 0, badge.Width, badge.Height), 8))
                {
                    Region previous = badge.Region;
                    badge.Region = new Region(path);
                    previous?.Dispose();
                }
            };
            badges.Add(badge);
            infoHost.Controls.Add(badge);
        }

        private void AddDeliveryBadge(string address, string reference)
        {
            string delivery = address ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(reference))
                delivery += " · Referencia: " + reference.Trim();
            AddBadge("DIRECCIÓN", delivery, Theme.Info);
        }

        private static string FormatOrderTime(string value)
        {
            TimeSpan time;
            if (TimeSpan.TryParse(value, System.Globalization.CultureInfo.InvariantCulture, out time)
                && time >= TimeSpan.Zero && time < TimeSpan.FromDays(1))
                return DateTime.Today.Add(time).ToString("hh:mm tt", System.Globalization.CultureInfo.InvariantCulture);
            DateTime date;
            if (DateTime.TryParse(value, System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.NoCurrentDateDefault, out date))
                return date.ToString("hh:mm tt", System.Globalization.CultureInfo.InvariantCulture);
            return value;
        }

        private void StyleAction(ModernButton button, FlowLayoutPanel host, string text, ButtonKind kind,
            IconChar icon, int width)
        {
            button.BackgroundImage = null;
            button.Text = text;
            button.Kind = kind;
            button.Icon = icon;
            button.Size = new Size(width, 32);
            button.Font = Theme.Small;
            button.Margin = new Padding(0, 0, 4, 0);
            host.Controls.Add(button);
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.observaciones.txtObservacion.Text))
            {
                string rpta = NPedido.CancelarPedido(this.Pedido.Id_pedido, this.observaciones.txtObservacion.Text);
                if (rpta.Equals("OK"))
                {
                    Mensajes.MensajeOkForm("Se canceló correctamente el domicilio");
                    this.OnRefresh?.Invoke(this.Pedido, e);
                }
            }
            else
            {
                Mensajes.MensajeInformacion("Debe justificar la cancelación del domicilio", "Entendido");
                container = new PoperContainer(this.observaciones);
                container.Show(this.btnCancelar);
            }
        }

        private void BtnCancelar_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.observaciones.txtObservacion.Text))
                this.observaciones.txtObservacion.Clear();

            Mensajes.MensajePregunta("¿Está seguro que desea cancelar el domicilio?", "Si", "No", out DialogResult dialog);
            if (dialog == DialogResult.Yes)
            {
                container = new PoperContainer(this.observaciones);
                container.Show(this.btnCancelar);
            }
        }

        private void BtnAddProductos_Click(object sender, EventArgs e)
        {
            //Quiere decir que va a agregar más productos
            if (this.Pedido.Estado_pedido.Equals("PENDIENTE"))
            {
                // FrmPedido es el formulario vigente (FrmRealizarPedido ya no guarda pedidos).
                DatosInicioSesion datos = DatosInicioSesion.GetInstancia();
                FrmPedido form = new FrmPedido
                {
                    StartPosition = FormStartPosition.CenterScreen,
                    Numero_mesa = 0,
                    Tipo_servicio = "DOMICILIO",
                    IsDomicilio = true,
                    EmpleadoSelected = datos.EmpleadoClaveMaestra,
                    ClienteSelected = this.Pedido.Cliente,
                    MesaSelected = this.Pedido.Mesa,
                    WindowState = FormWindowState.Maximized,
                    Pedido = this.Pedido,
                    IsEditar = true,
                };
                form.ShowDialog();
                this.AsignarDatos(this.Pedido);
            }
        }

        private void BtnPrint_Click(object sender, EventArgs e)
        {
            if (this.Pedido.Estado_pedido.Equals("PENDIENTE"))
            {
                MensajeEspera.ShowWait("Imprimiendo...");
                //Imprime solo las comandas
                FrmComandas frmComandas = new FrmComandas
                {
                    Id_pedido = this.Pedido.Id_pedido,
                };
                frmComandas.ObtenerReporte("Casa Grande | DOMICILIO");
                frmComandas.AsignarTablas();
                frmComandas.ImprimirFactura(1);
                MensajeEspera.CloseForm();
            }
            else if (this.Pedido.Estado_pedido.Equals("DISPONIBLE"))
            {
                MensajeEspera.ShowWait("Imprimiendo...");
                FrmFacturaFinal frmFacturaFinal = new FrmFacturaFinal
                {
                    Id_pedido = this.Pedido.Id_pedido,
                };
                frmFacturaFinal.AsignarReporte();
                frmFacturaFinal.AsignarTablasCuentaFinal();
                frmFacturaFinal.ImprimirFactura(1);
                MensajeEspera.CloseForm();
            }
        }

        private void BtnNext_Click(object sender, EventArgs e)
        {
            this.OnBtnNextClick?.Invoke(this.Pedido, e);      
        }

        private void AsignarDatos(Pedidos pedido)
        {
            if (pedido.Estado_pedido.Equals("PENDIENTE"))
            {
                this.panel1.BackColor = Theme.Danger;
                this.btnAddProductos.Visible = true;
                this.btnNext.Visible = true;
                this.btnCancelar.Visible = true;
                this.btnPrint.Visible = true;
            }
            else if (pedido.Estado_pedido.Equals("CANCELADO"))
            {
                this.panel1.BackColor = Theme.Warning;
                this.btnAddProductos.Visible = false;
                this.btnNext.Visible = false;
                this.btnCancelar.Visible = false; 
                this.btnPrint.Visible = false;
            }
            else
            {
                this.panel1.BackColor = Theme.Success;
                this.btnAddProductos.Visible = false;
                this.btnNext.Visible = false;
                this.btnCancelar.Visible = false;
                this.btnPrint.Visible = true;
            }

            infoHost.SuspendLayout();
            foreach (Control badge in badges) badge.Dispose();
            badges.Clear();
            AddBadge("PEDIDO", "#" + pedido.Id_pedido, Theme.Primary);
            AddBadge("ESTADO", pedido.Estado_pedido == "PENDIENTE" ? "En preparación" :
                pedido.Estado_pedido == "CANCELADO" ? "Cancelado" : "Terminado", panel1.BackColor);
            AddBadge("FECHA", pedido.Fecha_pedido.ToString("dd MMM yyyy", new System.Globalization.CultureInfo("es-CO")), Theme.Neutral);
            AddBadge("HORA", FormatOrderTime(pedido.Hora_pedido), Theme.Neutral);

            //Obtener los datos principales
            DataTable TablaDatosPrincipales = NPedido.BuscarPedidosYDetalle("ID PEDIDO Y DETALLE",
                Convert.ToString(pedido.Id_pedido), 
                out DataTable dtDetallePedido,
                out DataTable dtDetallePlatosPedido, out string rpta);
            if (TablaDatosPrincipales != null && TablaDatosPrincipales.Rows.Count > 0)
            {
                string cliente = Convert.ToString(TablaDatosPrincipales.Rows[0]["Cliente"]);
                string observaciones = Convert.ToString(TablaDatosPrincipales.Rows[0]["Observaciones_pedido"]);
                // La consulta antigua concatena el telefono al nombre del cliente.
                string[] customerParts = System.Text.RegularExpressions.Regex.Split(cliente,
                    @"\s*-\s*Tel[eé]fono\s*:\s*", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                AddBadge("CLIENTE", customerParts[0], Theme.Primary);

                // Direccion de entrega elegida al crear el pedido (los domicilios viejos no la tienen).
                CustomerAddress address = pedido.AddressId.HasValue ? NAddresses.Get(pedido.AddressId.Value) : null;
                if (address != null)
                {
                    AddDeliveryBadge(address.Summary, address.Reference);
                }
                else if (pedido.Cliente != null)
                {
                    AddDeliveryBadge(pedido.Cliente.Direccion_cliente, pedido.Cliente.Referencia_ubicacion);
                }
                string phone = !string.IsNullOrWhiteSpace(address?.ContactPhone) ? address.ContactPhone :
                    customerParts.Length > 1 ? customerParts[1] : pedido.Cliente?.Telefono_cliente;
                AddBadge("TELÉFONO", phone, Theme.Info);

                if (!string.IsNullOrEmpty(observaciones))
                {
                    AddBadge("OBSERVACIONES", observaciones, Theme.Warning);
                }

                if (dtDetallePedido != null) foreach (DataRow row in dtDetallePedido.Rows)
                {
                    string detail = Convert.ToString(row["Nombre"]);
                    if (!string.IsNullOrEmpty(row["Observaciones"].ToString()))
                    {
                        detail += Environment.NewLine + row["Observaciones"];
                    }
                    AddBadge("× " + row["Cantidad"], detail, Theme.Success);
                }
            }
            else AddBadge("DETALLE", "No se pudo cargar el detalle del pedido. Actualice para reintentar.", Theme.Warning);
            infoHost.ResumeLayout();
            LayoutBadges();
        }

        public event EventHandler OnBtnNextClick;
        public event EventHandler OnRefresh;

        private Pedidos _pedido;

        public Pedidos Pedido
        {
            get => _pedido;
            set
            {
                _pedido = value;
                this.AsignarDatos(value);
            }
        }
    }
}
