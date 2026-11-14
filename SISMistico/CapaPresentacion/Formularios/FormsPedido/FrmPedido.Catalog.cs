namespace CapaPresentacion.Formularios.FormsPedido
{
    using CapaPresentacion.Properties;
    using CapaPresentacion.UI;
    using FontAwesome.Sharp;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    // Lista paginada de platos y bebidas. Antes se creaba una tarjeta (ProductoItem) por cada producto del
    // resultado y se leia su foto original; con un tipo de muchos productos la ventana tardaba en responder.
    // Ahora solo existen las tarjetas de la pagina visible y las fotos salen de ProductThumbnails.
    public partial class FrmPedido
    {
        private const int CatalogPageSize = 20;

        private readonly ModernButton previousPageButton = NewPagerButton("Anterior", IconChar.ChevronLeft);
        private readonly ModernButton nextPageButton = NewPagerButton("Siguiente", IconChar.ChevronRight);
        private readonly Label pagerLabel = new Label
        {
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = Theme.Body,
            ForeColor = Theme.TextMuted,
        };

        private List<ProductBinding> catalogResults = new List<ProductBinding>();
        private int catalogPage;
        // Cambia cada vez que se dibuja una pagina; las fotos que llegan de una pagina anterior se descartan.
        private int catalogGeneration;

        private int CatalogPageCount =>
            Math.Max(1, (this.catalogResults.Count + CatalogPageSize - 1) / CatalogPageSize);

        private static ModernButton NewPagerButton(string text, IconChar icon) => new ModernButton
        {
            Text = text,
            Icon = icon,
            Kind = ButtonKind.Outline,
            Font = Theme.BodyBold,
            Size = new Size(118, 34),
            TabStop = false,
        };

        // La barra ocupa la franja inferior del grupo de resultados; la lista se acorta para dejarle sitio.
        private void BuildCatalogPager()
        {
            const int barHeight = 42;
            Panel bar = new Panel
            {
                Location = new Point(this.panelResultados.Left, this.panelResultados.Bottom - barHeight),
                Size = new Size(this.panelResultados.Width, barHeight),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = this.groupBox3.BackColor,
            };
            this.panelResultados.Height -= barHeight;

            this.previousPageButton.Location = new Point(2, 5);
            this.nextPageButton.Location = new Point(bar.Width - this.nextPageButton.Width - 2, 5);
            this.nextPageButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.pagerLabel.Bounds = new Rectangle(this.previousPageButton.Right + 6, 5,
                this.nextPageButton.Left - this.previousPageButton.Right - 12, 34);
            this.pagerLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            this.previousPageButton.Click += (s, e) => this.ShowCatalogPage(this.catalogPage - 1);
            this.nextPageButton.Click += (s, e) => this.ShowCatalogPage(this.catalogPage + 1);

            bar.Controls.Add(this.pagerLabel);
            bar.Controls.Add(this.previousPageButton);
            bar.Controls.Add(this.nextPageButton);
            this.groupBox3.Controls.Add(bar);
            this.UpdateCatalogPager();
        }

        /// <summary>Reemplaza el resultado de la busqueda y muestra su primera pagina.</summary>
        private void ShowCatalog(List<ProductBinding> products)
        {
            this.catalogResults = products ?? new List<ProductBinding>();
            this.ShowCatalogPage(0);
        }

        private void ShowCatalogPage(int page)
        {
            this.catalogPage = Math.Max(0, Math.Min(page, this.CatalogPageCount - 1));
            int generation = ++this.catalogGeneration;

            // Las tarjetas de la pagina anterior se liberan: el panel solo las quita, no las destruye.
            List<UserControl> previous = this.panelResultados.controlsUser.ToList();
            this.panelResultados.SuspendLayout();
            this.panelResultados.clearDataSource();
            foreach (UserControl old in previous)
                old.Dispose();

            List<UserControl> items = new List<UserControl>();
            foreach (ProductBinding product in this.catalogResults
                .Skip(this.catalogPage * CatalogPageSize).Take(CatalogPageSize))
            {
                ProductoItem item = new ProductoItem { Product = product };
                item.OnBtnAddClick += ProductoItem_OnBtnAddClick;
                items.Add(item);
            }

            if (items.Count > 0)
            {
                this.panelResultados.BackgroundImage = null;
                this.panelResultados.AutoScrollPosition = Point.Empty;
                this.panelResultados.AddArrayControl(items);
            }
            else
            {
                this.panelResultados.BackgroundImage = Resources.SIN_IMAGENES;
                this.panelResultados.BackgroundImageLayout = ImageLayout.Center;
            }
            this.panelResultados.ResumeLayout();

            this.UpdateCatalogPager();
            this.LoadCatalogImages(items.Cast<ProductoItem>().ToList(), generation);
        }

        private void UpdateCatalogPager()
        {
            int total = this.catalogResults.Count;
            this.previousPageButton.Enabled = this.catalogPage > 0;
            this.nextPageButton.Enabled = this.catalogPage < this.CatalogPageCount - 1;
            this.pagerLabel.Text = total == 0
                ? "Sin productos"
                : "Página " + (this.catalogPage + 1) + " de " + this.CatalogPageCount + "  ·  "
                    + total + (total == 1 ? " producto" : " productos");
        }

        // Las fotos se preparan fuera del hilo de la ventana y se asignan en el; la lista ya se puede usar
        // mientras llegan.
        private void LoadCatalogImages(List<ProductoItem> items, int generation)
        {
            if (items.Count == 0)
                return;
            Image placeholder = Resources.SIN_IMAGENES;
            // El nombre se lee aqui, en el hilo de la ventana, para no tocar las tarjetas desde el otro hilo.
            List<string> fileNames = items.Select(item => item.Product.NombreImagen).ToList();

            Task.Run(() =>
            {
                for (int i = 0; i < items.Count; i++)
                {
                    if (generation != this.catalogGeneration)
                        return;
                    ProductoItem item = items[i];
                    Image image = ProductThumbnails.Get(fileNames[i]) ?? placeholder;
                    try
                    {
                        this.BeginInvoke((Action)(() =>
                        {
                            if (generation == this.catalogGeneration && !item.IsDisposed)
                                item.ImageProduct = image;
                        }));
                    }
                    catch (InvalidOperationException)
                    {
                        // La ventana se cerro mientras se cargaban las fotos.
                        return;
                    }
                }
            });
        }
    }
}
