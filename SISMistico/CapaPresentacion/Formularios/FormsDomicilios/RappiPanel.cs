using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CapaEntidades.Models;
using CapaNegocio;
using CapaNegocio.Rappi;
using CapaPresentacion.UI;
using FontAwesome.Sharp;

namespace CapaPresentacion.Formularios.FormsDomicilios
{
    /// <summary>Carpeta donde se dejan las facturas PDF de Rappi.</summary>
    public static class RappiFolder
    {
        /// <summary>
        /// La clave opcional <c>RappiInvoicesFolder</c> de appSettings (ruta completa, o relativa a Documentos); sin ella,
        /// Documentos\Casa Grande\FacturasRappi.
        /// </summary>
        public static string Resolve()
        {
            string documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string configured = ConfigurationManager.AppSettings["RappiInvoicesFolder"];
            if (!string.IsNullOrWhiteSpace(configured))
                return Path.IsPathRooted(configured) ? configured : Path.Combine(documents, configured);
            return Path.Combine(documents, "Casa Grande", "FacturasRappi");
        }
    }

    /// <summary>
    /// Cuerpo del panel RAPPI de Domicilios: vigila la carpeta de facturas PDF, importa las nuevas (sin duplicar) y
    /// muestra las ordenes del dia en fichas, con buscador y paginacion. Es informativo: no crea pedidos.
    /// </summary>
    public sealed class RappiPanel : Panel
    {
        private const int PageSize = 10;
        private readonly DeliverySearchBar searchBar;

        private readonly ModernTextBox search = new ModernTextBox
        {
            PlaceholderText = "Buscar por orden, cliente, teléfono, dirección o producto",
            Icon = IconChar.MagnifyingGlass,
            Dock = DockStyle.Fill,
        };
        private readonly FlowLayoutPanel list = new DeliveryTouchList
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            BackColor = Theme.Surface,
            Padding = new Padding(0, 4, 0, 4),
        };
        private readonly ModernButton previous = NewPagerButton("Anterior", IconChar.ChevronLeft);
        private readonly ModernButton next = NewPagerButton("Siguiente", IconChar.ChevronRight);
        private readonly Label pageInfo = new Label { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter };
        private readonly Label status = new Label { Dock = DockStyle.Bottom, Height = 22, TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true };
        private readonly Timer tick = new Timer { Interval = 3000 };
        private readonly Timer searchDelay = new Timer { Interval = 350 };

        private FileSystemWatcher watcher;
        private volatile bool folderChanged;
        private bool importing;
        private bool forceReload;
        private DateTime lastScan = DateTime.MinValue;
        private int page;
        private int total;
        private string signature = string.Empty;
        private string folder;

        /// <summary>Cuantas ordenes de Rappi hay hoy; el formulario lo usa para el indicador superior.</summary>
        public event EventHandler<int> TodayCountChanged;
        public event Action<decimal?, decimal?, decimal?> TodayTotalsChanged;

        public RappiPanel()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Theme.Surface;

            this.searchBar = new DeliverySearchBar(this.search);
            this.searchBar.ViewChanged += (s, e) =>
            {
                this.list.SuspendLayout();
                try
                {
                    foreach (RappiCard card in this.list.Controls.OfType<RappiCard>()) card.CompactView = this.searchBar.CompactView;
                    this.list.AutoScrollPosition = Point.Empty;
                }
                finally { this.list.ResumeLayout(true); }
            };

            TableLayoutPanel pager = new TableLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 58,
                ColumnCount = 3,
                RowCount = 1,
                BackColor = Theme.Surface,
                Padding = new Padding(0, 6, 0, 6),
            };
            pager.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            pager.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            pager.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            this.previous.Dock = DockStyle.Fill;
            this.next.Dock = DockStyle.Fill;
            this.pageInfo.StyleLabel(LabelKind.Muted);
            this.status.StyleLabel(LabelKind.Muted);
            pager.Controls.Add(this.previous, 0, 0);
            pager.Controls.Add(this.pageInfo, 1, 0);
            pager.Controls.Add(this.next, 2, 0);

            // Orden de Add: lo ultimo acoplado se coloca primero, asi que Fill va antes que los bordes.
            this.Controls.Add(this.list);
            this.Controls.Add(pager);
            this.Controls.Add(this.status);
            this.Controls.Add(this.searchBar);

            this.list.SizeChanged += (s, e) => this.FitCards();
            this.previous.Click += (s, e) => this.GoTo(this.page - 1);
            this.next.Click += (s, e) => this.GoTo(this.page + 1);
            this.search.TextChanged += (s, e) => { this.searchDelay.Stop(); this.searchDelay.Start(); };
            this.searchDelay.Tick += (s, e) => { this.searchDelay.Stop(); this.page = 0; this.Reload(true); };
            this.tick.Tick += (s, e) => this.OnTick();
        }

        private static ModernButton NewPagerButton(string text, IconChar icon) => new ModernButton
        {
            Text = text,
            Icon = icon,
            Kind = ButtonKind.Outline,
            CornerRadius = 9,
        };

        /// <summary>Empieza a vigilar la carpeta y a refrescar. Llamar una vez, al mostrar el modulo.</summary>
        public void Start()
        {
            if (this.watcher != null)
                return;
            this.folder = RappiFolder.Resolve();
            try
            {
                // La carpeta se crea para que haya donde dejar las facturas desde el primer dia.
                Directory.CreateDirectory(this.folder);
                this.watcher = new FileSystemWatcher(this.folder, "*.pdf")
                {
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size,
                    EnableRaisingEvents = true,
                };
                FileSystemEventHandler mark = (s, e) => this.folderChanged = true;
                this.watcher.Created += mark;
                this.watcher.Changed += mark;
                this.watcher.Renamed += (s, e) => this.folderChanged = true;
                this.status.Text = "Carpeta vigilada: " + this.folder;
            }
            catch (Exception ex)
            {
                // Sin vigilancia en vivo igual se revisa la carpeta cada pocos segundos.
                this.status.Text = "No se pudo vigilar la carpeta (" + ex.Message + "). Se revisa periodicamente.";
            }
            this.folderChanged = true;
            this.tick.Start();
            this.OnTick();
        }

        /// <summary>
        /// Boton Actualizar de Domicilios: revisa la carpeta ya (sin esperar los 30 s) y reconstruye la lista aunque nada haya cambiado.
        /// </summary>
        public void RefreshNow()
        {
            if (this.watcher == null && this.folder == null)
                return;
            this.folderChanged = true;
            this.forceReload = true;
            this.OnTick();
        }

        public void Stop()
        {
            this.tick.Stop();
            this.searchDelay.Stop();
            if (this.watcher != null)
            {
                this.watcher.EnableRaisingEvents = false;
                this.watcher.Dispose();
                this.watcher = null;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Stop();
                this.tick.Dispose();
                this.searchDelay.Dispose();
            }
            base.Dispose(disposing);
        }

        // Cada 3 s: si la carpeta cambio (o pasaron 30 s) se importa en segundo plano; luego se recarga la lista.
        private void OnTick()
        {
            if (!this.Visible || this.IsDisposed)
                return;
            if (this.importing)
                return;

            bool scan = this.folderChanged || DateTime.UtcNow - this.lastScan > TimeSpan.FromSeconds(30);
            if (!scan)
            {
                this.Reload(false);
                return;
            }

            this.folderChanged = false;
            this.lastScan = DateTime.UtcNow;
            this.importing = true;
            string target = this.folder;
            Task.Run(() =>
            {
                try { return RappiImporter.ImportFolder(target); }
                catch (Exception ex)
                {
                    RappiImportResult failed = new RappiImportResult();
                    failed.Failed.Add(ex.Message);
                    return failed;
                }
            }).ContinueWith(task =>
            {
                if (this.IsDisposed || !this.IsHandleCreated)
                    return;
                this.BeginInvoke((Action)(() =>
                {
                    this.importing = false;
                    RappiImportResult result = task.Result;
                    if (result.Failed.Count > 0)
                        this.status.Text = "No se pudo leer: " + result.Failed[0];
                    else if (result.Imported > 0 || result.Duplicates > 0)
                    {
                        // Lo nuevo queda en la carpeta del dia y las copias en Duplicados (ver RappiImporter).
                        List<string> parts = new List<string>();
                        if (result.Imported > 0)
                            parts.Add(result.Imported + (result.Imported == 1 ? " factura nueva importada" : " facturas nuevas importadas"));
                        if (result.Duplicates > 0)
                            parts.Add(result.Duplicates + (result.Duplicates == 1 ? " duplicada apartada" : " duplicadas apartadas") + " en Duplicados");
                        this.status.Text = string.Join(" · ", parts) + " · " + DateTime.Now.ToString("h:mm tt", System.Globalization.CultureInfo.InvariantCulture);
                    }
                    bool forced = this.forceReload;
                    this.forceReload = false;
                    this.Reload(result.Imported > 0 || forced);
                }));
            });
        }

        private void GoTo(int newPage)
        {
            int pages = Math.Max(1, (int)Math.Ceiling(this.total / (double)PageSize));
            this.page = Math.Max(0, Math.Min(pages - 1, newPage));
            this.Reload(true);
        }

        private void Reload(bool force)
        {
            try
            {
                DateTime today = DateTime.Today;
                int count;
                List<RappiOrder> orders = NRappiOrders.SearchDay(today, this.search.Text, this.page, PageSize, out count);

                // Una busqueda o un borrado pueden dejar la pagina fuera de rango: se vuelve a la ultima valida.
                int pages = Math.Max(1, (int)Math.Ceiling(count / (double)PageSize));
                if (this.page >= pages)
                {
                    this.page = pages - 1;
                    orders = NRappiOrders.SearchDay(today, this.search.Text, this.page, PageSize, out count);
                }

                this.total = count;
                this.pageInfo.Text = count == 0 ? "Sin pedidos" : "Página " + (this.page + 1) + " de " + pages + " · " + count + (count == 1 ? " pedido" : " pedidos");
                this.previous.Enabled = this.page > 0;
                this.next.Enabled = this.page < pages - 1;

                // Totales del dia completo: no se limitan a la pagina ni al buscador.
                System.Data.DataTable dayOrders = NRappiOrders.ListRange(today, today);
                int dayCount = dayOrders.Rows.Count;
                var rows = dayOrders.Rows.Cast<System.Data.DataRow>().ToList();
                Func<string, decimal?> sum = column => rows.Any(row => row.IsNull(column))
                    ? (decimal?)null : rows.Sum(row => Convert.ToDecimal(row[column]));
                this.TodayTotalsChanged?.Invoke(sum("Total"), sum("CashAmount"), sum("OtherPaymentAmount"));
                this.TodayCountChanged?.Invoke(this, dayCount);

                string newSignature = this.page + "|" + this.search.Text + "|" + count + "|" + string.Join(",", orders.Select(o => o.Id));
                if (!force && newSignature == this.signature)
                    return;
                this.signature = newSignature;
                this.Fill(orders);
            }
            catch (Exception ex)
            {
                // El panel no debe tumbar la ventana: el motivo queda en la linea de estado y se reintenta en el proximo tic.
                this.status.Text = "No se pudieron cargar los pedidos de Rappi: " + ex.Message;
            }
        }

        private void Fill(List<RappiOrder> orders)
        {
            this.list.SuspendLayout();
            foreach (Control old in this.list.Controls.Cast<Control>().ToList())
            {
                this.list.Controls.Remove(old);
                old.Dispose();
            }

            if (orders.Count == 0)
            {
                Label empty = new Label
                {
                    Text = string.IsNullOrWhiteSpace(this.search.Text) ? "Aún no hay pedidos de Rappi hoy" : "Ningún pedido coincide con la búsqueda",
                    AutoSize = false,
                    Size = new Size(Math.Max(200, this.list.ClientSize.Width - 8), 80),
                    TextAlign = ContentAlignment.MiddleCenter,
                };
                empty.StyleLabel(LabelKind.Muted);
                this.list.Controls.Add(empty);
            }

            // Cada ficha muestra el numero de su factura: las que aun no lo tienen lo reciben en orden de llegada.
            foreach (RappiOrder order in orders.Where(o => !o.DayNumber.HasValue).OrderBy(o => o.OrderDateTime ?? DateTime.MaxValue))
                order.DayNumber = NRappiOrders.EnsureDayNumber(order.ExternalId);

            foreach (RappiOrder order in orders)
            {
                RappiCard card = new RappiCard { Width = this.CardWidth(), CompactView = this.searchBar.CompactView, GetPrintCopies = () => this.searchBar.Copies };
                card.Bind(order);
                this.list.Controls.Add(card);
            }
            this.list.ResumeLayout();
        }

        private void FitCards()
        {
            foreach (Control card in this.list.Controls)
                card.Width = this.CardWidth();
        }

        // Se reserva el ancho de la barra vertical para que no aparezca una horizontal.
        private int CardWidth() => Math.Max(280, this.list.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - 4);
    }
}
