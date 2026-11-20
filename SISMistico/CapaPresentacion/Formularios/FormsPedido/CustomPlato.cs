namespace CapaPresentacion.Formularios.FormsPedido
{
    using CapaNegocio;
    using CapaPresentacion.UI;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Drawing;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    /// <summary>
    /// Contenido del dialogo "Detalle del plato": una tarjeta de opciones (radio) por cada ingrediente a elegir
    /// -sopa/crema, arroz, acompanante y bebida-, un encabezado con el nombre del plato y un pie con las acciones
    /// Cancelar y Agregar. Antes eran cuatro listas desplegables. La zona de opciones se desplaza con el dedo.
    /// </summary>
    public partial class CustomPlato : UserControl
    {
        private const int HeaderHeight = 54;
        private const int FooterHeight = 62;
        private const int ContentPad = 12;

        private readonly TouchScrollPanel scroll = new TouchScrollPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = Theme.Background,
            TouchPanning = true,
        };
        private readonly Panel header = new Panel { Dock = DockStyle.Top, Height = HeaderHeight, BackColor = Theme.Surface };
        private readonly Panel footer = new Panel { Dock = DockStyle.Bottom, Height = FooterHeight, BackColor = Theme.Surface };
        private readonly Label titleLabel = new Label
        {
            AutoSize = false,
            AutoEllipsis = true,
            Font = Theme.Heading,
            ForeColor = Theme.TextPrimary,
            TextAlign = ContentAlignment.BottomLeft,
        };
        private readonly Label subtitleLabel = new Label
        {
            AutoSize = false,
            Text = "Elige una opción en cada grupo",
            Font = Theme.Small,
            ForeColor = Theme.TextMuted,
            TextAlign = ContentAlignment.TopLeft,
        };
        private readonly MinimalIconButton saveButton = new MinimalIconButton
        {
            MinimalStyle = true,
            LucideIcon = "check",
            Text = "Agregar al pedido",
            AccentColor = Theme.Success,
            CornerRadius = 10,
            Font = Theme.BodyBold,
            Size = new Size(180, 44),
            TabStop = true,
            AccessibleName = "Agregar al pedido",
        };
        private readonly MinimalIconButton cancelButton = new MinimalIconButton
        {
            MinimalStyle = true,
            LucideIcon = "x",
            Text = "Cancelar",
            CornerRadius = 10,
            Font = Theme.BodyBold,
            Size = new Size(120, 44),
            TabStop = true,
            AccessibleName = "Cancelar",
        };

        private readonly ChoiceGroup soupGroup = NewGroup("Sopa / Crema", "soup");
        private readonly ChoiceGroup riceGroup = NewGroup("Arroz", "wheat");
        private readonly ChoiceGroup sideGroup = NewGroup("Acompañantes", "salad");
        private readonly ChoiceGroup drinkGroup = NewGroup("Bebida", "cup-soda");

        public CustomPlato()
        {
            this.InitializeComponent();
            this.BackColor = Theme.Background;
            this.Font = Theme.Body;

            this.header.Controls.Add(this.subtitleLabel);
            this.header.Controls.Add(this.titleLabel);
            this.header.Paint += (s, e) =>
            {
                // Linea inferior fina que separa el encabezado de las opciones.
                using (Pen line = new Pen(Theme.Border))
                    e.Graphics.DrawLine(line, 0, this.header.Height - 1, this.header.Width, this.header.Height - 1);
            };
            this.header.Resize += (s, e) => this.LayoutHeader();

            this.footer.Controls.Add(this.cancelButton);
            this.footer.Controls.Add(this.saveButton);
            this.footer.Paint += (s, e) =>
            {
                using (Pen line = new Pen(Theme.Border))
                    e.Graphics.DrawLine(line, 0, 0, this.footer.Width, 0);
            };
            this.footer.Resize += (s, e) => this.LayoutFooter();

            this.scroll.Controls.AddRange(new Control[] { this.soupGroup, this.riceGroup, this.sideGroup, this.drinkGroup });
            this.scroll.Resize += (s, e) => this.LayoutGroups();

            // El orden de adicion importa para Dock: primero el relleno, luego los bordes.
            this.Controls.Add(this.scroll);
            this.Controls.Add(this.footer);
            this.Controls.Add(this.header);

            this.saveButton.Click += this.BtnSave_Click;
            this.cancelButton.Click += (s, e) => this.OnBtnCancelClick?.Invoke(this, EventArgs.Empty);
            this.LayoutHeader();
            this.LayoutFooter();
        }

        public event EventHandler OnBtnSaveClick;
        public event EventHandler OnBtnCancelClick;

        /// <summary>Nombre del plato, que se muestra como titulo del encabezado.</summary>
        public string DishName
        {
            get => this.titleLabel.Text;
            set => this.titleLabel.Text = value ?? string.Empty;
        }

        private static ChoiceGroup NewGroup(string title, string icon) => new ChoiceGroup { Title = title, LucideIcon = icon };

        private void LayoutHeader()
        {
            int width = this.header.Width - ContentPad * 2;
            this.titleLabel.Bounds = new Rectangle(ContentPad, 4, width, 28);
            this.subtitleLabel.Bounds = new Rectangle(ContentPad, 32, width, 18);
        }

        private void LayoutFooter()
        {
            int top = (FooterHeight - this.saveButton.Height) / 2;
            this.saveButton.Location = new Point(this.footer.Width - ContentPad - this.saveButton.Width, top);
            this.cancelButton.Location = new Point(this.saveButton.Left - 10 - this.cancelButton.Width, top);
        }

        // Los grupos se apilan a todo el ancho util; cada uno decide su alto segun cuantas filas necesite.
        private bool layingOut;

        private void LayoutGroups()
        {
            // Cambiar el alto de contenido muestra u oculta la barra y vuelve a lanzar Resize: se evita el bucle.
            if (this.layingOut)
                return;
            this.layingOut = true;
            try
            {
                this.DoLayoutGroups();
            }
            finally { this.layingOut = false; }
        }

        private void DoLayoutGroups()
        {
            int width = Math.Max(200, this.scroll.ClientSize.Width - ContentPad * 2);
            int y = ContentPad + this.scroll.AutoScrollPosition.Y;
            foreach (ChoiceGroup group in new[] { this.soupGroup, this.riceGroup, this.sideGroup, this.drinkGroup })
            {
                group.SetBounds(ContentPad, y, width, group.GetPreferredHeight(width));
                y += group.Height + 8;
            }
            // Margen inferior para que el ultimo grupo no quede pegado al pie al desplazar.
            this.scroll.AutoScrollMinSize = new Size(0, y - this.scroll.AutoScrollPosition.Y + ContentPad - 8);
        }

        // Cada grupo debe tener una opcion elegida; se conservan los mensajes de antes.
        private bool TryReadChoice(ChoiceGroup group, string missingMessage, out ProductDetalleBinding detail)
        {
            detail = null;
            if (!group.SelectedId.HasValue)
            {
                group.HasError = true;
                Mensajes.MensajeInformacion(missingMessage);
                return false;
            }

            DataRow[] rows = this.DtIngredientes.Select(string.Format("Id_ingrediente = {0}", group.SelectedId.Value));
            if (rows.Length == 0)
            {
                Mensajes.MensajeInformacion("Hubo un error obteniendo el tipo de ingrediente");
                return false;
            }

            detail = new ProductDetalleBinding
            {
                Id_ingrediente = group.SelectedId.Value,
                Ingrediente = new CapaEntidades.Models.Ingredientes(rows[0]),
                Observaciones = string.Empty,
            };
            return true;
        }

        private bool Comprobaciones(out List<ProductDetalleBinding> detalles)
        {
            detalles = new List<ProductDetalleBinding>();
            // Mismo orden que antes: sopa/crema, arroz, acompañante y bebida.
            (ChoiceGroup group, string message)[] required =
            {
                (this.soupGroup, "Compruebe la sopa/crema seleccionada"),
                (this.riceGroup, "Compruebe el arroz seleccionado"),
                (this.sideGroup, "Compruebe el acompañante seleccionado"),
                (this.drinkGroup, "Compruebe la bebida seleccionada"),
            };
            foreach ((ChoiceGroup group, string message) item in required)
            {
                if (!this.TryReadChoice(item.group, item.message, out ProductDetalleBinding detail))
                    return false;
                detalles.Add(detail);
            }
            return true;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (this.Comprobaciones(out List<ProductDetalleBinding> detalles))
                this.OnBtnSaveClick?.Invoke(detalles, e);
        }

        // Opciones de un tipo de ingrediente, tomadas de la tabla de ingredientes.
        private static List<KeyValuePair<int, string>> OptionsOf(DataTable table, string type)
        {
            DataView view = new DataView(table) { RowFilter = string.Format("Tipo_ingrediente = '{0}'", type) };
            return view.Cast<DataRowView>()
                .Select(row => new KeyValuePair<int, string>(
                    Convert.ToInt32(row["Id_ingrediente"]), Convert.ToString(row["Nombre_ingrediente"])))
                .ToList();
        }

        public async Task LoadIngredientes()
        {
            try
            {
                var (rpta, dtIngredientes) =
                    await NIngredientes.BuscarIngredientes("COMPLETO", "");
                if (dtIngredientes != null)
                {
                    this.DtIngredientes = dtIngredientes;
                    this.soupGroup.SetOptions(OptionsOf(dtIngredientes, "SOPA"));
                    this.riceGroup.SetOptions(OptionsOf(dtIngredientes, "ARROZ"));
                    this.sideGroup.SetOptions(OptionsOf(dtIngredientes, "ACOMPANANTE"));
                    this.drinkGroup.SetOptions(OptionsOf(dtIngredientes, "BEBIDA"));
                    this.LayoutGroups();
                }
                else
                    if (!rpta.Equals("OK"))
                    throw new Exception(rpta);
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "LoadIngredientes",
                    "Hubo un error cargando los ingredientes", ex.Message);
            }
        }

        public bool IsEditar { get; set; }

        public DataTable DtIngredientes { get; set; }

        public bool IsEnabledBebida { get; set; }

        public bool IsEnabledSopa { get; set; }
    }
}
