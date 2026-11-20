using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CapaPresentacion.UI;
using FontAwesome.Sharp;

namespace CapaPresentacion.Formularios.FormsPrincipales
{
    public partial class FrmPrincipal
    {
        private MainNavigationButton[] navigationButtons;
        private Label navigationTitle;
        private Label navigationDate;
        private bool arrangingNavigation;
        private bool updatingContentSpace;
        private FlowLayoutPanel navigationItems;
        private TableLayoutPanel navigationShell;
        private TableLayoutPanel navigationSidebar;
        private Panel navigationBrand;
        private Label navigationFooter;
        private MainNavigationButton menuToggle;
        private bool navigationCollapsed;
        private const int ExpandedSidebarWidth = 248;
        private const int CollapsedSidebarWidth = 80;

        private void BuildModernNavigation()
        {
            SuspendLayout();
            MinimumSize = new Size(1040, 720);
            // Reutilizar cada control una sola vez y eliminar las coordenadas del diseno heredado.
            Controls.Clear();
            panelBotones.Controls.Clear();
            panelBarraSuperior.Controls.Clear();
            panelBotones.Anchor = panelBarraSuperior.Anchor = panel1.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            panelBotones.Dock = panelBarraSuperior.Dock = panel1.Dock = DockStyle.Fill;
            panelBotones.Margin = panelBarraSuperior.Margin = panel1.Margin = Padding.Empty;
            var shell = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1,
                Margin = Padding.Empty, Padding = Padding.Empty,
            };
            shell.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 248));
            shell.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            shell.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            navigationShell = shell;
            shell.Controls.Add(panelBotones, 0, 0);
            // Sin barra superior: el contenido ocupa todo el alto y el boton de cuenta flota sobre el fondo.
            shell.Controls.Add(panel1, 1, 0);
            // Franja superior libre (solo fondo) para que el boton flotante no tape la barra de herramientas de los formularios hijos.
            panel1.Padding = new Padding(0, 48, 0, 0);
            Controls.Add(shell);

            var sidebar = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3,
                Margin = Padding.Empty, Padding = Padding.Empty, BackColor = Color.Transparent,
            };
            sidebar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            sidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 140));
            sidebar.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            sidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
            navigationSidebar = sidebar;
            var brand = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty, BackColor = Color.Transparent };
            lblMistico.Text = "Casa Grande"; lblMistico.AutoSize = false;
            lblMistico.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            lblMistico.SetBounds(23, 22, 212, 40);
            SetNavigationFont(lblMistico, "Georgia", 20f, FontStyle.Bold);
            lblMistico.ForeColor = MainNavigationColors.Ink;
            lblMistico.BackColor = Color.Transparent;
            brand.Controls.Add(lblMistico);
            var brandCaption = new Label
            {
                Text = "R E S T A U R A N T E", Location = new Point(26, 67), Size = new Size(210, 18),
                ForeColor = MainNavigationColors.Bronze, BackColor = Color.Transparent,
            };
            SetNavigationFont(brandCaption, "Trebuchet MS", 8f, FontStyle.Bold);
            brand.Controls.Add(brandCaption);
            brand.Controls.Add(new MainNavigationDivider { Location = new Point(24, 91), Size = new Size(200, 8) });
            var sectionCaption = new Label
            {
                Text = "Espacio de trabajo", Location = new Point(25, 111), Size = new Size(158, 24),
                ForeColor = MainNavigationColors.Ink, BackColor = Color.Transparent,
            };
            SetNavigationFont(sectionCaption, "Trebuchet MS", 10f, FontStyle.Bold);
            brand.Controls.Add(sectionCaption);
            navigationBrand = brand;
            navigationFooter = label1;
            // Boton hamburguesa: contrae el panel a solo iconos y lo vuelve a expandir.
            menuToggle = new MainNavigationButton { Size = new Size(44, 38), TabStop = false };
            ConfigureNavigation(menuToggle, IconChar.Bars, "Contraer men\u00fa");
            menuToggle.IconOnly = true; menuToggle.Dock = DockStyle.None;
            menuToggle.Location = new Point(192, 98);
            menuToggle.Click += (sender, args) => SetNavigationCollapsed(!navigationCollapsed);
            brand.Controls.Add(menuToggle);
            menuToggle.BringToFront();
            navigationItems = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false,
                AutoScroll = true, Margin = Padding.Empty, Padding = new Padding(14, 4, 0, 0), BackColor = Color.Transparent,
            };
            label1.AutoSize = false;
            label1.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            label1.Dock = DockStyle.Fill;
            label1.Margin = new Padding(0, 8, 0, 0);
            SetNavigationFont(label1, "Trebuchet MS", 8f, FontStyle.Regular);
            label1.ForeColor = MainNavigationColors.Bronze; label1.BackColor = Color.Transparent;
            var footer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2,
                BackColor = Color.Transparent, Padding = new Padding(24, 0, 18, 8), Margin = Padding.Empty,
            };
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            footer.RowStyles.Add(new RowStyle(SizeType.Absolute, 8));
            footer.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            footer.Controls.Add(new MainNavigationDivider { Dock = DockStyle.Fill, Margin = Padding.Empty }, 0, 0);
            footer.Controls.Add(label1, 0, 1);
            sidebar.Controls.Add(brand, 0, 0);
            sidebar.Controls.Add(navigationItems, 0, 1);
            sidebar.Controls.Add(footer, 0, 2);
            panelBotones.Controls.Add(sidebar);

            ConfigureNavigation(btnMesasPedidos, IconChar.Utensils, "Mesas y pedidos");
            ConfigureNavigation(btnDomicilios, IconChar.Truck, "Domicilios");
            ConfigureNavigation(btnAdministracion, IconChar.ChartLine, "Administraci\u00f3n");
            ConfigureNavigation(btnPlatos, IconChar.BoxOpen, "Productos");
            ConfigureNavigation(btnBebidas, IconChar.MugHot, "Acompa\u00f1antes");
            ConfigureNavigation(btnClientes, IconChar.AddressBook, "Clientes", true);
            ConfigureNavigation(btnEmpleados, IconChar.Users, "Empleados", true);
            ConfigureNavigation(btnInsumos, IconChar.BoxesStacked, "Insumos", true);
            ConfigureNavigation(btnReservas, IconChar.CalendarDays, "Reservas");
            navigationButtons = new[] { btnMesasPedidos, btnDomicilios, btnAdministracion, btnPlatos,
                btnBebidas, btnClientes, btnEmpleados, btnInsumos, btnReservas }.Cast<MainNavigationButton>().ToArray();
            foreach (var button in navigationButtons)
            {
                button.Dock = DockStyle.None;
                button.AutoSize = false;
                button.Height = 46;
                button.Margin = new Padding(0, 0, 0, 8);
                navigationItems.Controls.Add(button);
                button.Click += (sender, args) => {
                    var selected = (MainNavigationButton)sender;
                    foreach (var item in navigationButtons) item.Active = item == selected;
                    navigationTitle.Text = selected.Text;
                    UpdateContentSpace();
                };
            }
            ConfigureNavigation(btnUsuario, IconChar.User, btnUsuario.Text, true);
            var account = (MainNavigationButton)btnUsuario;
            account.Toolbar = account.Surface = true; account.AccessibleName = "Opciones de mi cuenta";
            ConfigureNavigation(btnAdministracionAvanzada, IconChar.Gear, "Configuraci\u00f3n avanzada");
            var settings = (MainNavigationButton)btnAdministracionAvanzada;
            settings.Toolbar = settings.IconOnly = true; settings.AccessibleName = "Configuraci\u00f3n avanzada";
            if (components == null) components = new System.ComponentModel.Container();
            var hints = new ToolTip(components);
            hints.SetToolTip(settings, settings.AccessibleName); hints.SetToolTip(account, account.AccessibleName);
            hints.SetToolTip(menuToggle, "Contraer o expandir el menú");
            foreach (var button in navigationButtons) hints.SetToolTip(button, button.Text);
            navigationTitle = new Label { Text = "Panel de control", Font = Theme.Heading, ForeColor = MainNavigationColors.Ink,
                BackColor = Color.Transparent, AutoEllipsis = true, Dock = DockStyle.Fill, Margin = Padding.Empty };
            navigationDate = new Label { Text = DateTime.Today.ToString("dddd \u00b7 dd MMMM yyyy", CultureInfo.GetCultureInfo("es-CO")),
                Font = Theme.Small, ForeColor = MainNavigationColors.Bronze, BackColor = Color.Transparent,
                AutoEllipsis = true, Dock = DockStyle.Fill, Margin = Padding.Empty };
            SetNavigationFont(navigationTitle, "Georgia", 16f, FontStyle.Regular);
            SetNavigationFont(navigationDate, "Trebuchet MS", 9f, FontStyle.Regular);
            // El boton de ajustes avanzados deja de mostrarse; el control se conserva por su logica existente.
            settings.Visible = false;
            account.Dock = DockStyle.None; account.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            account.Floating = true; account.Height = 40;
            Controls.Add(account);
            account.BringToFront();
            // La franja depende del formulario visible, tambien al abrirlo por otro flujo o volver al frente.
            panel1.ControlAdded += (sender, args) =>
            {
                Form child = args.Control as Form;
                if (child != null)
                {
                    child.VisibleChanged += (s, e) => UpdateContentSpace();
                    child.Activated += (s, e) => UpdateContentSpace();
                    child.FormClosed += (s, e) => UpdateContentSpace();
                }
                UpdateContentSpace();
            };
            panel1.ControlRemoved += (sender, args) => UpdateContentSpace();
            panel1.Layout += (sender, args) => UpdateContentSpace();
            navigationItems.SizeChanged += (sender, args) => LayoutModernNavigation();
            Shown += (sender, args) => { LayoutModernNavigation(); UpdateFloatingAccount(); };
            LayoutModernNavigation();
            ResumeLayout(true);
        }

        // Contrae el panel izquierdo a una columna de iconos (menu hamburguesa) o lo expande de nuevo.
        private void SetNavigationCollapsed(bool collapsed)
        {
            if (navigationShell == null) return;
            navigationCollapsed = collapsed;
            SuspendLayout();
            navigationShell.ColumnStyles[0].Width = collapsed ? CollapsedSidebarWidth : ExpandedSidebarWidth;
            navigationSidebar.RowStyles[0].Height = collapsed ? 72 : 140;
            foreach (Control child in navigationBrand.Controls)
                if (child != menuToggle) child.Visible = !collapsed;
            navigationFooter.Visible = !collapsed;
            menuToggle.Location = collapsed ? new Point((CollapsedSidebarWidth - menuToggle.Width) / 2 - 4, 18) : new Point(192, 98);
            menuToggle.Text = collapsed ? "Expandir men\u00fa" : "Contraer men\u00fa";
            navigationItems.Padding = new Padding(collapsed ? 8 : 14, 4, 0, 0);
            foreach (var button in navigationButtons) button.IconOnly = collapsed;
            ResumeLayout(true);
            LayoutModernNavigation();
            foreach (var button in navigationButtons) button.Invalidate();
        }

        private void UpdateContentSpace()
        {
            if (updatingContentSpace) return;
            // Controls enumera los hijos en orden Z: el primero visible es el que ocupa el frente.
            Form active = panel1.Controls.OfType<Form>()
                .FirstOrDefault(child => child.Visible && child.WindowState != FormWindowState.Minimized);
            SetFullSpace(active is Formularios.FormsDomicilios.FrmDeliveries);
        }

        // El boton flotante y su reserva de espacio cambian juntos.
        private void SetFullSpace(bool full)
        {
            if (updatingContentSpace) return;
            updatingContentSpace = true;
            try
            {
                btnUsuario.Visible = !full;
                Padding padding = full ? Padding.Empty : new Padding(0, 48, 0, 0);
                if (panel1.Padding != padding) panel1.Padding = padding;
            }
            finally { updatingContentSpace = false; }
        }
        // Texto del boton flotante: nombre del empleado + fecha de hoy; se reajusta ancho y posicion.
        private void UpdateFloatingAccount()
        {
            var account = btnUsuario as MainNavigationButton;
            if (account == null || !account.Floating) return;
            string date = DateTime.Today.ToString("ddd dd MMM yyyy", CultureInfo.GetCultureInfo("es-CO"));
            string name = btnUsuario.Text.Split(new[] { " · " }, StringSplitOptions.None)[0];
            account.Text = name + " · " + date;
            int textWidth = TextRenderer.MeasureText(account.Text, account.Font).Width;
            account.Width = Math.Min(Math.Max(220, textWidth + 49 + 29), Math.Max(220, ClientSize.Width - 300));
            account.Location = new Point(ClientSize.Width - account.Width - 22, 4);
        }

        private static void SetNavigationFont(Control owner, string family, float size, FontStyle style)
        {
            var font = new Font(family, size, style, GraphicsUnit.Point);
            owner.Font = font;
            owner.Disposed += (sender, args) => font.Dispose();
        }

        private static void ConfigureNavigation(Button button, IconChar icon, string text, bool submenu = false)
        {
            var modern = (MainNavigationButton)button;
            modern.Image = null; modern.Text = text; modern.NavigationIcon = icon; modern.Submenu = submenu;
            modern.Font = Theme.BodyBold; modern.ForeColor = MainNavigationColors.Ink;
            modern.BackColor = Theme.Lighten(MainNavigationColors.Pale, .60f);
            modern.FlatStyle = FlatStyle.Flat; modern.FlatAppearance.BorderSize = 0; modern.UseVisualStyleBackColor = false;
            modern.Anchor = AnchorStyles.Top | AnchorStyles.Left; modern.Cursor = Cursors.Hand; modern.AccessibleName = text;
        }

        private void LayoutModernNavigation()
        {
            if (navigationItems == null || navigationButtons == null || arrangingNavigation) return;
            arrangingNavigation = true;
            navigationItems.SuspendLayout();
            try
            {
                int width = Math.Max(navigationCollapsed ? 40 : 120, navigationItems.ClientSize.Width
                    - navigationItems.Padding.Horizontal - SystemInformation.VerticalScrollBarWidth - 8);
                foreach (var button in navigationButtons)
                    button.Size = new Size(width, 46);
            }
            finally
            {
                navigationItems.ResumeLayout(true);
                arrangingNavigation = false;
            }
        }
    }

    // Fondo de pantalla compartido: los formularios hijos lo pintan para verse "transparentes" sobre el.
    internal static class AppWallpaper
    {
        public static Bitmap Image;

        // Ajuste "cubrir" sobre el area dada; igual que el panel principal.
        public static void Draw(Graphics g, Rectangle area)
        {
            if (Image == null || area.Width <= 0 || area.Height <= 0) return;
            float scale = Math.Max((float)area.Width / Image.Width, (float)area.Height / Image.Height);
            float width = Image.Width * scale, height = Image.Height * scale;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(Image, area.X + (area.Width - width) / 2, area.Y + (area.Height - height) / 2, width, height);
        }
    }

    internal static class MainNavigationColors
    {
        // Paleta verde alineada con LoginColors (login moderno); los nombres se conservan para no tocar los usos.
        public static readonly Color Pale = LoginColors.Sage;
        public static readonly Color Gold = Color.FromArgb(143, 176, 153);
        public static readonly Color Amber = Color.FromArgb(104, 140, 120);
        public static readonly Color Ochre = LoginColors.Primary;
        public static readonly Color Bronze = LoginColors.Primary;
        public static readonly Color Ink = Theme.Darken(Bronze, .44f);
    }

    internal sealed class MainNavigationPanel : Panel
    {
        private readonly bool horizontal;
        public MainNavigationPanel(bool horizontal) {
            this.horizontal = horizontal;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        }
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            PaintSurface(e.Graphics, Point.Empty);
        }

        internal void PaintSurface(Graphics graphics, Point offset)
        {
            if (Width <= 0 || Height <= 0) return;
            // Mismo degradado, con coordenadas del panel incluso al pintar el fondo de un boton.
            Rectangle surface = new Rectangle(-offset.X, -offset.Y, Width, Height);
            using (var gradient = new LinearGradientBrush(surface, Color.White, Color.White,
                horizontal ? LinearGradientMode.Horizontal : LinearGradientMode.Vertical))
            {
                gradient.InterpolationColors = new ColorBlend {
                    Colors = horizontal
                        ? new[] { Theme.Lighten(MainNavigationColors.Pale, .57f), Theme.Lighten(MainNavigationColors.Gold, .77f), Theme.Lighten(MainNavigationColors.Amber, .90f) }
                        : new[] { Theme.Lighten(MainNavigationColors.Pale, .44f), Theme.Lighten(MainNavigationColors.Gold, .73f), Theme.Lighten(MainNavigationColors.Amber, .88f) },
                    Positions = new[] { 0f, .52f, 1f } };
                graphics.FillRectangle(gradient, surface);
            }
            using (var border = new Pen(Color.FromArgb(42, MainNavigationColors.Ochre))) {
                if (horizontal) graphics.DrawLine(border, surface.Left, surface.Bottom - 1, surface.Right, surface.Bottom - 1);
                else graphics.DrawLine(border, surface.Right - 1, surface.Top, surface.Right - 1, surface.Bottom);
            }
        }
    }

    internal sealed class MainNavigationDivider : Control
    {
        public MainNavigationDivider()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.Opaque, true);
            TabStop = false;
        }
        protected override void OnPaintBackground(PaintEventArgs e) { }
        protected override void OnPaint(PaintEventArgs e)
        {
            Point offset = Point.Empty;
            Control ancestor = this;
            while (ancestor != null && !(ancestor is MainNavigationPanel))
            {
                offset.Offset(ancestor.Left, ancestor.Top);
                ancestor = ancestor.Parent;
            }
            var panel = ancestor as MainNavigationPanel;
            if (panel != null) panel.PaintSurface(e.Graphics, offset);
            else e.Graphics.Clear(Theme.Lighten(MainNavigationColors.Pale, .7f));
            if (Width < 4 || Height < 2) return;
            using (var gradient = new LinearGradientBrush(ClientRectangle, Color.Transparent, Color.Transparent, 0f))
            {
                gradient.InterpolationColors = new ColorBlend
                {
                    Colors = new[] { Color.FromArgb(0, MainNavigationColors.Ochre),
                        Color.FromArgb(110, MainNavigationColors.Ochre), Color.FromArgb(0, MainNavigationColors.Ochre) },
                    Positions = new[] { 0f, .5f, 1f },
                };
                e.Graphics.FillRectangle(gradient, 0, Height / 2, Width, 1);
            }
        }
    }

    internal sealed class MainNavigationButton : Button
    {
        public IconChar NavigationIcon { get; set; }
        public bool Submenu { get; set; }
        public bool Toolbar { get; set; }
        public bool Surface { get; set; }
        public bool Floating { get; set; }
        public bool IconOnly { get; set; }
        private bool active, hovered, pressed;
        public bool Active { get => active; set { active = value; Invalidate(); } }
        public MainNavigationButton() {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.ResizeRedraw | ControlStyles.Opaque, true);
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            UseVisualStyleBackColor = false;
            BackColor = Theme.Lighten(MainNavigationColors.Pale, .60f);
        }
        protected override void OnMouseEnter(EventArgs e) { hovered = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { hovered = pressed = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e) { pressed = true; Invalidate(); base.OnMouseDown(e); }
        protected override void OnMouseUp(MouseEventArgs e) { pressed = false; Invalidate(); base.OnMouseUp(e); }
        protected override void OnEnabledChanged(EventArgs e) { hovered = pressed = false; Invalidate(); base.OnEnabledChanged(e); }
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // No usar el fondo nativo de Button ni la transparencia simulada de sus padres.
            // OnPaint reconstruye todo el fondo antes de dibujar cada estado.
        }

        private void PaintNavigationBackground(Graphics graphics)
        {
            Point offset = Point.Empty;
            Control ancestor = this;
            while (ancestor != null && !(ancestor is MainNavigationPanel))
            {
                offset.Offset(ancestor.Left, ancestor.Top);
                ancestor = ancestor.Parent;
            }
            var panel = ancestor as MainNavigationPanel;
            if (panel != null) panel.PaintSurface(graphics, offset);
            else graphics.Clear(BackColor);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (!Floating || Width < 8 || Height < 8) return;
            // Esquinas redondeadas reales: el boton flota sobre el fondo y no tiene panel que lo pinte.
            using (var path = Theme.RoundedPath(new Rectangle(0, 0, Width, Height), 12)) Region = new Region(path);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (Floating) e.Graphics.Clear(Color.FromArgb(246, 249, 246));
            else PaintNavigationBackground(e.Graphics);
            if (Width < 8 || Height < 8) return;
            var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle bounds = Rectangle.Inflate(ClientRectangle, -1, -1);
            bool highlight = Enabled && (hovered || pressed);
            if (active || highlight || Surface)
            {
                Color fill = active ? Color.FromArgb(236, Color.White)
                    : highlight ? Color.FromArgb(pressed ? 204 : 150, Color.White) : Color.FromArgb(130, Color.White);
                using (var path = Theme.RoundedPath(bounds, 12))
                using (var brush = new SolidBrush(fill))
                using (var border = new Pen(Color.FromArgb(active ? 80 : 30, MainNavigationColors.Ochre)))
                { g.FillPath(brush, path); g.DrawPath(border, path); }
            }
            if (active)
            {
                var mark = new Rectangle(7, Height / 2 - 9, 3, 18);
                using (var path = Theme.RoundedPath(mark, 2))
                using (var accent = new LinearGradientBrush(mark, MainNavigationColors.Gold, MainNavigationColors.Ochre,
                    LinearGradientMode.Vertical)) g.FillPath(accent, path);
            }
            Color ink = Enabled ? MainNavigationColors.Ink : Theme.Lighten(MainNavigationColors.Bronze, .48f);
            Color icon = Enabled ? MainNavigationColors.Bronze : Theme.Lighten(MainNavigationColors.Bronze, .54f);
            int iconX = IconOnly ? (Width - 18) / 2 : 19;
            IconRenderer.Draw(this, g, NavigationIcon, icon, new Rectangle(iconX, (Height - 18) / 2, 18, 18));
            if (!IconOnly)
            {
                int textX = Toolbar ? 49 : 51;
                TextRenderer.DrawText(g, Text, Font, new Rectangle(textX, 0, Math.Max(1, Width - textX - (Submenu ? 29 : 12)), Height), ink,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);
                if (Submenu) IconRenderer.Draw(this, g, Toolbar ? IconChar.ChevronDown : IconChar.ChevronRight, icon,
                    new Rectangle(Width - 24, (Height - 10) / 2, 10, 10));
            }
            if (Focused && ShowFocusCues) ControlPaint.DrawFocusRectangle(g, Rectangle.Inflate(bounds, -5, -5), ink,
                Theme.Lighten(MainNavigationColors.Pale, .7f));
        }
    }
}
