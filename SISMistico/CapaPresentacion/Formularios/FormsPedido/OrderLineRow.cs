namespace CapaPresentacion.Formularios.FormsPedido
{
    using CapaPresentacion.Formularios.FormsPlatos;
    using CapaPresentacion.UI;
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Linq;
    using System.Windows.Forms;

    /// <summary>
    /// Linea de producto en la lista Pedido (una columna). Comparte el estilo de ProductSearchRow (franja de
    /// acento, linea inferior, foto circular y badges) y ordena todo en un maximo de tres filas:
    ///   1. nombre completo;
    ///   2. badges de precio y cantidad, y el total de la linea;
    ///   3. (solo si hay datos) ingredientes elegidos y, a su derecha, la nota de observaciones.
    /// A la derecha, centradas, van las acciones: editar, observaciones y quitar (o agregar).
    /// </summary>
    public class OrderLineRow : UserControl
    {
        private const int MinHeight = 64;
        private const int ImageSize = 38;
        // Tamanos tactiles: 36 px las acciones secundarias y 40 px la principal (quitar o agregar).
        private const int ButtonSize = 36;
        private const int PrimarySize = 40;
        private const int ButtonGap = 4;
        private const int Margin_ = 12;
        private const int BadgeHeight = 22;
        private const int DetailHeight = 20;
        private const int LineGap = 5;

        private const TextFormatFlags WrapFlags = TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix
            | TextFormatFlags.NoPadding | TextFormatFlags.Left | TextFormatFlags.Top;
        private const TextFormatFlags LineFlags = TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis
            | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding | TextFormatFlags.VerticalCenter;

        private readonly CircleImage image = new CircleImage { Size = new Size(ImageSize, ImageSize) };
        private readonly MinimalIconButton editButton = NewIconButton("pencil");
        private readonly MinimalIconButton commentButton = NewIconButton("message-square");
        private readonly MinimalIconButton removeButton = NewIconButton("trash-2", Theme.Danger);
        private readonly MinimalIconButton addButton = NewIconButton("plus", Theme.Success);
        private readonly ToolTip toolTip = new ToolTip();

        private ProductBinding product;
        private bool hover;
        // Medidas calculadas en PerformRowLayout; el dibujo las reutiliza en vez de volver a medir.
        private int nameHeight;
        private string ingredientsText = string.Empty;

        public event EventHandler OnBtnAddClick;
        public event EventHandler OnBtnRemoveClick;

        public OrderLineRow()
        {
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            this.BackColor = Theme.Surface;
            this.Font = Theme.Body;
            this.Height = MinHeight;
            this.Margin = Padding.Empty;

            this.removeButton.Size = this.addButton.Size = new Size(PrimarySize, PrimarySize);
            this.toolTip.SetToolTip(this.editButton, "Editar los datos del producto");
            this.toolTip.SetToolTip(this.commentButton, "Observaciones del producto");
            this.toolTip.SetToolTip(this.removeButton, "Quitar una unidad del pedido");
            this.toolTip.SetToolTip(this.addButton, "Agregar producto");

            this.image.Click += (s, e) => this.RaiseAdd();
            this.addButton.Click += (s, e) => this.RaiseAdd();
            this.removeButton.Click += (s, e) => this.OnBtnRemoveClick?.Invoke(this.product, EventArgs.Empty);
            this.commentButton.Click += this.CommentButton_Click;
            this.editButton.Click += this.EditButton_Click;

            this.Controls.AddRange(new Control[]
            {
                this.image, this.editButton, this.commentButton, this.removeButton, this.addButton,
            });
        }

        public ProductBinding Product
        {
            get => this.product;
            set
            {
                this.product = value;
                this.ApplyProductState();
                this.PerformRowLayout();
            }
        }

        public Image ImageProduct
        {
            get => this.image.Picture;
            set => this.image.Picture = value;
        }

        private static MinimalIconButton NewIconButton(string lucideName, Color? accent = null) => new MinimalIconButton
        {
            MinimalStyle = true,
            LucideIcon = lucideName,
            AccentColor = accent ?? Color.Empty,
            CornerRadius = 9,
            Size = new Size(ButtonSize, ButtonSize),
            TabStop = false,
        };

        // Una linea ya guardada o agregada al pedido se quita; una que aun no lo esta se agrega (como ProductoItem).
        private bool ShowsRemove => this.product != null && (this.product.IsAddBD || this.product.IsEditar);

        private void ApplyProductState()
        {
            if (this.product == null)
                return;
            this.editButton.Visible = string.Equals(this.product.Tipo_producto, "PLATO", StringComparison.Ordinal);
            this.removeButton.Visible = this.ShowsRemove;
            this.addButton.Visible = !this.ShowsRemove;
            this.image.Enabled = !this.ShowsRemove;
            this.image.Cursor = this.ShowsRemove ? Cursors.Default : Cursors.Hand;
        }

        private string PriceText => this.product == null ? string.Empty : FormatMoney(this.product.Precio);
        private string QuantityText => "× " + Math.Max(1, this.product?.Cantidad ?? 1);
        private BadgeKind TypeKind =>
            string.Equals(this.product?.Tipo_producto, "PLATO", StringComparison.Ordinal) ? BadgeKind.Primary : BadgeKind.Info;

        private static string FormatMoney(decimal value) => value.ToString("C").Replace(",00", "");

        private bool HasNote => !string.IsNullOrWhiteSpace(this.product?.Observaciones);
        private bool HasThirdRow => this.ingredientsText.Length > 0 || this.HasNote;

        private int ActionsWidth =>
            (this.editButton.Visible ? ButtonSize + ButtonGap : 0) + ButtonSize + ButtonGap + PrimarySize;

        private int TextLeft => Margin_ + ImageSize + 10;
        private int TextWidth => Math.Max(60, this.ClientSize.Width - this.TextLeft - this.ActionsWidth - Margin_ - 8);

        // Ingredientes elegidos para el plato en una sola linea: "Tipo: ingrediente  ·  Tipo: ingrediente".
        private string BuildIngredientsText()
        {
            if (this.product?.ProductDetalles == null
                || !string.Equals(this.product.Tipo_producto, "PLATO", StringComparison.Ordinal))
                return string.Empty;
            return string.Join("  ·  ", this.product.ProductDetalles
                .Where(d => d?.Ingrediente != null)
                .Select(d => d.Ingrediente.Tipo_ingrediente + ": " + d.Ingrediente.Nombre_ingrediente));
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            this.PerformRowLayout();
        }

        // La altura suma el nombre, los badges y, si hay datos, la tercera fila; las acciones se centran.
        private void PerformRowLayout()
        {
            if (this.product == null || this.ClientSize.Width <= 0)
                return;

            this.nameHeight = string.IsNullOrEmpty(this.product.Nombre)
                ? 0
                : TextRenderer.MeasureText(this.product.Nombre, Theme.BodyBold, new Size(this.TextWidth, 0), WrapFlags).Height;
            this.ingredientsText = this.BuildIngredientsText();

            int content = 10 + this.nameHeight + LineGap + BadgeHeight
                + (this.HasThirdRow ? LineGap + DetailHeight : 0) + 10;
            int height = Math.Max(MinHeight, content);
            if (this.Height != height)
            {
                // Cambiar la altura vuelve a lanzar OnResize; la segunda pasada ya no cambia nada.
                this.Height = height;
                return;
            }

            int centerY = height / 2;
            int right = this.ClientSize.Width - Margin_;
            MinimalIconButton primary = this.ShowsRemove ? this.removeButton : this.addButton;
            primary.Location = new Point(right - PrimarySize, centerY - PrimarySize / 2);
            this.commentButton.Location = new Point(primary.Left - ButtonGap - ButtonSize, centerY - ButtonSize / 2);
            this.editButton.Location = new Point(this.commentButton.Left - ButtonGap - ButtonSize, centerY - ButtonSize / 2);
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

        // Popup moderno de observaciones. Se guarda al pulsar Guardar o al hacer clic fuera (como antes);
        // Cancelar o Esc descartan lo escrito.
        private void CommentButton_Click(object sender, EventArgs e)
        {
            if (this.product == null)
                return;

            ObservationEditor editor = new ObservationEditor
            {
                Title = "Observaciones",
                Subtitle = this.product.Nombre,
                Note = this.product.Observaciones ?? string.Empty,
            };
            PoperContainer popup = new PoperContainer(editor);
            bool cancelled = false;
            editor.Saved += (s, args) => popup.Close();
            editor.Cancelled += (s, args) => { cancelled = true; popup.Close(); };
            popup.Closed += (s, args) =>
            {
                if (!cancelled)
                {
                    this.product.Observaciones = editor.Note.Trim();
                    this.PerformRowLayout();
                    this.Invalidate();
                }
                // El editor se libera fuera del evento de cierre del popup que lo contiene.
                this.BeginInvoke((Action)(() => editor.Dispose()));
            };
            popup.Show(this.commentButton);
            editor.FocusInput();
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
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Franja de acento (azul plato, turquesa bebida) y linea inferior, como las filas de Domicilios.
            using (SolidBrush stripe = new SolidBrush(Badge.BaseColor(this.TypeKind)))
                g.FillRectangle(stripe, 0, 10, 3, this.Height - 20);
            using (Pen line = new Pen(Theme.Border))
                g.DrawLine(line, 10, this.Height - 1, this.Width - 8, this.Height - 1);

            int x = this.TextLeft;
            int y = 10;

            // Fila 1: nombre completo.
            TextRenderer.DrawText(g, this.product.Nombre, Theme.BodyBold,
                new Rectangle(x, y, this.TextWidth, this.nameHeight), Theme.TextPrimary, WrapFlags);
            y += this.nameHeight + LineGap;

            // Fila 2: precio unitario, cantidad y, si hay mas de una unidad y cabe, el total de la linea.
            int bx = x;
            bx += this.DrawBadge(g, bx, y, this.PriceText, BadgeKind.Success) + 6;
            bx += this.DrawBadge(g, bx, y, this.QuantityText, BadgeKind.Neutral) + 8;
            if (this.product.Cantidad > 1)
            {
                string total = "= " + FormatMoney(this.product.Precio * this.product.Cantidad);
                int available = x + this.TextWidth - bx;
                // El total solo se dibuja si cabe entero: una cifra recortada con "..." confundiria.
                if (TextRenderer.MeasureText(total, Theme.SmallBold).Width <= available)
                    TextRenderer.DrawText(g, total, Theme.SmallBold, new Rectangle(bx, y, available, BadgeHeight),
                        Theme.TextPrimary, LineFlags);
            }
            y += BadgeHeight;

            // Fila 3: ingredientes (izquierda) y nota de observaciones (derecha), cada uno en una sola linea.
            if (!this.HasThirdRow)
                return;
            y += LineGap;
            int rowWidth = this.TextWidth;
            bool hasIngredients = this.ingredientsText.Length > 0;
            int noteWidth = this.HasNote ? (hasIngredients ? (int)(rowWidth * 0.46f) : rowWidth) : 0;
            int ingredientsWidth = rowWidth - noteWidth - (noteWidth > 0 && hasIngredients ? 6 : 0);

            if (hasIngredients)
                TextRenderer.DrawText(g, this.ingredientsText, Theme.Small,
                    new Rectangle(x, y, ingredientsWidth, DetailHeight), Theme.TextMuted, LineFlags);

            if (noteWidth > 0)
            {
                Rectangle box = new Rectangle(x + rowWidth - noteWidth, y, noteWidth, DetailHeight);
                using (GraphicsPath path = Theme.RoundedPath(box, 8))
                using (SolidBrush fill = new SolidBrush(Theme.Lighten(Theme.Warning, 0.90f)))
                    g.FillPath(fill, path);
                LucideIcons.Draw(g, "message-square-text", Theme.Darken(Theme.Warning, 0.25f),
                    new Rectangle(box.X + 6, box.Y + 4, 12, 12));
                TextRenderer.DrawText(g, this.product.Observaciones.Trim(), Theme.Small,
                    new Rectangle(box.X + 24, box.Y, box.Width - 30, box.Height),
                    Theme.Darken(Theme.Warning, 0.50f), LineFlags);
            }
        }

        // Dibuja un badge del ancho de su texto y devuelve ese ancho para colocar el siguiente.
        private int DrawBadge(Graphics g, int x, int y, string text, BadgeKind kind)
        {
            int width = TextRenderer.MeasureText(text, Theme.SmallBold).Width + 20;
            Badge.Draw(g, new Rectangle(x, y, width, BadgeHeight), text, kind, Theme.SmallBold);
            return width;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                this.toolTip.Dispose();
            base.Dispose(disposing);
        }
    }
}
