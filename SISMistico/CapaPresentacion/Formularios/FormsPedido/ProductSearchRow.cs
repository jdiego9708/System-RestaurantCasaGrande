namespace CapaPresentacion.Formularios.FormsPedido
{
    using CapaPresentacion.Formularios.FormsPlatos;
    using CapaPresentacion.UI;
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Windows.Forms;

    /// <summary>
    /// Ficha compacta de un producto en la lista de busqueda. Copia el estilo de las filas de Domicilios
    /// (franja de acento, linea inferior, textos con TextRenderer y badges). Distribucion: foto circular,
    /// nombre completo (hasta donde necesite, en varias filas), una fila de badges con el tipo y el precio,
    /// y a la derecha las acciones editar y agregar. El comentario solo existe en la lista del pedido.
    /// </summary>
    public class ProductSearchRow : UserControl
    {
        private const int MinHeight = 64;
        private const int ImageSize = 38;
        private const int ButtonSize = 36; // tamano tactil
        private const int ButtonGap = 4;
        // El boton de agregar es la accion principal: mismo tamano tactil que los botones de Domicilios.
        private const int AddButtonSize = 44;
        private const int Margin_ = 12;
        private const int BadgeHeight = 22;
        private const int LineGap = 6;

        // Mismas banderas de texto que usa DeliveryCard para texto que se ajusta a varias lineas.
        private const TextFormatFlags WrapFlags = TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix
            | TextFormatFlags.NoPadding | TextFormatFlags.Left | TextFormatFlags.Top;

        private readonly CircleImage image = new CircleImage { Size = new Size(ImageSize, ImageSize) };
        private readonly MinimalIconButton editButton = NewIconButton("pencil");
        private readonly MinimalIconButton addButton = new MinimalIconButton
        {
            MinimalStyle = true,
            LucideIcon = "plus",
            AccentColor = Theme.Success,
            CornerRadius = 9,
            Size = new Size(AddButtonSize, AddButtonSize),
            TabStop = false,
        };
        private readonly ToolTip toolTip = new ToolTip();

        private ProductBinding product;
        private bool hover;

        public event EventHandler OnBtnAddClick;

        public ProductSearchRow()
        {
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            this.BackColor = Theme.Surface;
            this.Font = Theme.Body;
            this.Height = MinHeight;
            this.Margin = Padding.Empty;

            this.toolTip.SetToolTip(this.editButton, "Editar los datos del producto");
            this.toolTip.SetToolTip(this.addButton, "Agregar producto");

            this.image.Cursor = Cursors.Hand;
            this.image.Click += (s, e) => this.RaiseAdd();
            this.addButton.Click += (s, e) => this.RaiseAdd();
            this.editButton.Click += this.EditButton_Click;

            this.Controls.AddRange(new Control[] { this.image, this.editButton, this.addButton });
        }

        public ProductBinding Product
        {
            get => this.product;
            set
            {
                this.product = value;
                // Solo los platos tienen formulario de edicion.
                this.editButton.Visible = value != null
                    && string.Equals(value.Tipo_producto, "PLATO", StringComparison.Ordinal);
                this.PerformRowLayout();
            }
        }

        public Image ImageProduct
        {
            get => this.image.Picture;
            set => this.image.Picture = value;
        }

        private static MinimalIconButton NewIconButton(string lucideName) => new MinimalIconButton
        {
            MinimalStyle = true,
            LucideIcon = lucideName,
            CornerRadius = 9,
            Size = new Size(ButtonSize, ButtonSize),
            TabStop = false,
        };

        // Los badges van en este orden: tipo y precio. El tipo cambia de color para distinguir platos de bebidas.
        private string TypeText => this.product?.Tipo_producto ?? string.Empty;
        private string PriceText => this.product == null ? string.Empty : this.product.Precio.ToString("C").Replace(",00", "");
        private BadgeKind TypeKind =>
            string.Equals(this.TypeText, "PLATO", StringComparison.Ordinal) ? BadgeKind.Primary : BadgeKind.Info;

        private int ActionsWidth =>
            this.editButton.Visible ? ButtonSize + ButtonGap + AddButtonSize : AddButtonSize;

        private int TextLeft => Margin_ + ImageSize + 10;

        private int NameWidth => Math.Max(40, this.ClientSize.Width - this.TextLeft - this.ActionsWidth - Margin_ - 8);

        // Alto del nombre completo: se mide con el ancho disponible para que nunca se recorte.
        private int MeasureName() => string.IsNullOrEmpty(this.product?.Nombre)
            ? 0
            : TextRenderer.MeasureText(this.product.Nombre, Theme.BodyBold, new Size(this.NameWidth, 0), WrapFlags).Height;

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            this.PerformRowLayout();
        }

        // La altura sigue al nombre: una fila de nombre da ficha baja y dos o mas filas la hacen crecer.
        private void PerformRowLayout()
        {
            if (this.product == null || this.ClientSize.Width <= 0)
                return;

            int height = Math.Max(MinHeight, 10 + this.MeasureName() + LineGap + BadgeHeight + 10);
            if (this.Height != height)
            {
                // Cambiar la altura vuelve a lanzar OnResize; la segunda pasada ya no cambia nada.
                this.Height = height;
                return;
            }

            int centerY = height / 2;
            int right = this.ClientSize.Width - Margin_;
            this.addButton.Location = new Point(right - AddButtonSize, centerY - AddButtonSize / 2);
            this.editButton.Location = new Point(this.addButton.Left - ButtonGap - ButtonSize, centerY - ButtonSize / 2);
            this.image.Location = new Point(Margin_, centerY - ImageSize / 2);
            this.Invalidate();
        }

        private void RaiseAdd() => this.OnBtnAddClick?.Invoke(this.product, EventArgs.Empty);

        private void EditButton_Click(object sender, EventArgs e)
        {
            if (this.product == null || !this.product.Tipo_producto.Equals("PLATO"))
                return;

            FrmAgregarPlato form = new FrmAgregarPlato
            {
                StartPosition = FormStartPosition.CenterScreen,
                Plato = (CapaEntidades.Models.Platos)this.product.Product,
            };
            form.OnPlatoSuccess += (s, args) =>
            {
                this.product.Product = (CapaEntidades.Models.Platos)s;
                this.PerformRowLayout();
                this.Invalidate();
            };
            form.ShowDialog();
        }

        protected override void OnMouseEnter(EventArgs e) { this.hover = !TouchScrollPanel.IsTouchInput; this.Invalidate(); base.OnMouseEnter(e); }

        protected override void OnMouseLeave(EventArgs e)
        {
            // Al pasar sobre un hijo el control recibe MouseLeave; se mantiene el realce si el cursor sigue dentro.
            this.hover = this.ClientRectangle.Contains(this.PointToClient(Cursor.Position));
            this.Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(this.hover ? Theme.SurfaceAlt : Theme.Surface);
            if (this.product == null)
                return;

            Color accent = Badge.BaseColor(this.TypeKind);
            // Franja de acento a la izquierda y linea inferior, como las filas de Domicilios.
            using (SolidBrush stripe = new SolidBrush(accent))
                g.FillRectangle(stripe, 0, 10, 3, this.Height - 20);
            using (Pen line = new Pen(Theme.Border))
                g.DrawLine(line, 10, this.Height - 1, this.Width - 8, this.Height - 1);

            // Nombre completo, en tantas filas como necesite.
            int nameHeight = this.MeasureName();
            TextRenderer.DrawText(g, this.product.Nombre, Theme.BodyBold,
                new Rectangle(this.TextLeft, 10, this.NameWidth, nameHeight), Theme.TextPrimary, WrapFlags);

            // Fila de badges: tipo y precio.
            int y = 10 + nameHeight + LineGap;
            int x = this.TextLeft;
            int typeWidth = TextRenderer.MeasureText(this.TypeText, Theme.SmallBold).Width + 20;
            Badge.Draw(g, new Rectangle(x, y, typeWidth, BadgeHeight), this.TypeText, this.TypeKind, Theme.SmallBold);
            x += typeWidth + 6;
            int priceWidth = TextRenderer.MeasureText(this.PriceText, Theme.SmallBold).Width + 20;
            Badge.Draw(g, new Rectangle(x, y, priceWidth, BadgeHeight), this.PriceText, BadgeKind.Success, Theme.SmallBold);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.toolTip.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
