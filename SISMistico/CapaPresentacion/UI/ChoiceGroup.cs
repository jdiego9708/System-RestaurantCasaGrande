using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace CapaPresentacion.UI
{
    /// <summary>
    /// Grupo de opciones excluyentes con tarjetas de radio: tarjeta blanca con encabezado (icono y titulo) y las
    /// opciones como tarjetas tactiles de 40 px que fluyen en filas. La seleccionada se resalta en azul con un
    /// indicador de visto. Cada opcion es un RadioButton real (mismo contenedor = exclusion nativa, flechas del
    /// teclado y accesibilidad). Sustituye a un ComboBox cuando hay pocas opciones y conviene verlas todas.
    /// </summary>
    public class ChoiceGroup : Control
    {
        private const int Pad = 12;
        private const int HeaderHeight = 40;
        private const int OptionHeight = 40;
        private const int Gap = 6;
        private const int CornerRadius = 12;

        private readonly List<ChoiceRadio> options = new List<ChoiceRadio>();
        private string title = string.Empty;
        private string lucideIcon;
        private bool hasError;
        private int layoutWidth = -1;

        /// <summary>Se produce al cambiar la opcion seleccionada.</summary>
        public event EventHandler SelectionChanged;

        public ChoiceGroup()
        {
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint, true);
            this.BackColor = Theme.Surface;
            this.Font = Theme.Body;
            this.Height = HeaderHeight + OptionHeight + Pad;
        }

        public string Title
        {
            get => this.title;
            set { this.title = value ?? string.Empty; this.AccessibleName = this.title; this.Invalidate(); }
        }

        /// <summary>Nombre de un icono Lucide para el encabezado.</summary>
        public string LucideIcon
        {
            get => this.lucideIcon;
            set { this.lucideIcon = value; this.Invalidate(); }
        }

        /// <summary>Marca el grupo en rojo (por ejemplo, cuando falta elegir); se quita al elegir una opcion.</summary>
        public bool HasError
        {
            get => this.hasError;
            set { if (this.hasError != value) { this.hasError = value; this.Invalidate(); } }
        }

        public int OptionCount => this.options.Count;

        /// <summary>Id de la opcion seleccionada, o null si no hay ninguna.</summary>
        public int? SelectedId
        {
            get
            {
                ChoiceRadio selected = this.options.FirstOrDefault(o => o.Checked);
                return selected == null ? (int?)null : selected.OptionId;
            }
            set
            {
                foreach (ChoiceRadio option in this.options)
                    option.Checked = value.HasValue && option.OptionId == value.Value;
            }
        }

        /// <summary>Texto de la opcion seleccionada, o null.</summary>
        public string SelectedText => this.options.FirstOrDefault(o => o.Checked)?.Text;

        /// <summary>Reemplaza las opciones; queda seleccionada la primera, como un ComboBox recien enlazado.</summary>
        public void SetOptions(IEnumerable<KeyValuePair<int, string>> items)
        {
            this.SuspendLayout();
            foreach (ChoiceRadio old in this.options)
            {
                this.Controls.Remove(old);
                old.Dispose();
            }
            this.options.Clear();

            foreach (KeyValuePair<int, string> item in items ?? Enumerable.Empty<KeyValuePair<int, string>>())
            {
                ChoiceRadio radio = new ChoiceRadio { OptionId = item.Key, Text = item.Value };
                radio.CheckedChanged += this.Radio_CheckedChanged;
                this.options.Add(radio);
                this.Controls.Add(radio);
            }
            if (this.options.Count > 0)
                this.options[0].Checked = true;

            this.layoutWidth = -1;
            this.ResumeLayout(false);
            this.LayoutOptions(this.Width);
        }

        private void Radio_CheckedChanged(object sender, EventArgs e)
        {
            if (!((ChoiceRadio)sender).Checked)
                return;
            this.HasError = false;
            this.SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Alto que necesita el grupo con un ancho dado (las opciones se reparten en filas).</summary>
        public int GetPreferredHeight(int width) => this.ComputeLayout(width, false);

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (this.Width != this.layoutWidth)
                this.LayoutOptions(this.Width);
        }

        // Coloca las opciones en filas y ajusta la altura del control.
        private void LayoutOptions(int width)
        {
            if (width <= 0)
                return;
            this.layoutWidth = width;
            int height = this.ComputeLayout(width, true);
            if (this.Height != height)
                this.Height = height;
        }

        // Recorre las opciones calculando su posicion. Con apply=false solo mide.
        private int ComputeLayout(int width, bool apply)
        {
            int available = Math.Max(80, width - Pad * 2);
            int x = Pad, y = HeaderHeight;
            foreach (ChoiceRadio option in this.options)
            {
                int textWidth = TextRenderer.MeasureText(option.Text, Theme.BodyBold).Width;
                int cardWidth = Math.Min(available, Math.Max(116, textWidth + 58));
                if (x > Pad && x + cardWidth > Pad + available)
                {
                    x = Pad;
                    y += OptionHeight + Gap;
                }
                if (apply)
                    option.SetBounds(x, y, cardWidth, OptionHeight);
                x += cardWidth + Gap;
            }
            return y + (this.options.Count > 0 ? OptionHeight : 0) + Pad;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Theme.ContainerBackground(this));
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle card = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
            Color edge = this.hasError ? Theme.Danger : Theme.Border;
            using (GraphicsPath path = Theme.RoundedPath(card, CornerRadius))
            using (SolidBrush fill = new SolidBrush(Theme.Surface))
            using (Pen border = new Pen(edge, this.hasError ? 1.6f : 1f))
            {
                g.FillPath(fill, path);
                g.DrawPath(border, path);
            }

            // Encabezado: insignia con el icono (como las insignias de icono de Domicilios) y el titulo.
            Color tone = this.hasError ? Theme.Danger : Theme.Primary;
            int textLeft = Pad;
            if (!string.IsNullOrEmpty(this.lucideIcon))
            {
                Rectangle badge = new Rectangle(Pad, (HeaderHeight - 26) / 2, 26, 26);
                using (GraphicsPath path = Theme.RoundedPath(badge, 8))
                using (SolidBrush fill = new SolidBrush(Theme.Lighten(tone, 0.88f)))
                    g.FillPath(fill, path);
                LucideIcons.Draw(g, this.lucideIcon, Theme.Darken(tone, 0.15f),
                    new Rectangle(badge.X + 5, badge.Y + 5, 16, 16));
                textLeft = badge.Right + 8;
            }
            const TextFormatFlags flags = TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis
                | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding | TextFormatFlags.VerticalCenter;
            TextRenderer.DrawText(g, this.title, Theme.Heading,
                new Rectangle(textLeft, 0, this.Width - textLeft - 110, HeaderHeight - 4),
                this.hasError ? Theme.Danger : Theme.TextPrimary, flags);

            // Contador a la derecha: cuantas opciones hay.
            string count = this.options.Count + (this.options.Count == 1 ? " opción" : " opciones");
            TextRenderer.DrawText(g, count, Theme.Small,
                new Rectangle(this.Width - Pad - 100, 0, 100, HeaderHeight - 4), Theme.TextMuted,
                flags | TextFormatFlags.Right);

            if (this.options.Count == 0)
                TextRenderer.DrawText(g, "No hay opciones disponibles", Theme.Body,
                    new Rectangle(Pad, HeaderHeight - 6, this.Width - Pad * 2, 30), Theme.TextMuted, flags);
        }

        // Cada opcion es un RadioButton real dibujado como tarjeta.
        private sealed class ChoiceRadio : RadioButton
        {
            private bool hover;

            public int OptionId { get; set; }

            public ChoiceRadio()
            {
                this.SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer
                    | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint, true);
                this.AutoSize = false;
                this.Cursor = Cursors.Hand;
                this.Font = Theme.Body;
                this.BackColor = Theme.Surface;
                this.TextAlign = ContentAlignment.MiddleLeft;
            }

            // El final de un arrastre tactil no cuenta como toque sobre la opcion.
            protected override void OnClick(EventArgs e)
            {
                if (TouchScrollPanel.IsClickSuppressed(this)) return;
                base.OnClick(e);
            }

            protected override void OnMouseEnter(EventArgs e) { this.hover = !TouchScrollPanel.IsTouchInput; this.Invalidate(); base.OnMouseEnter(e); }
            protected override void OnMouseLeave(EventArgs e) { this.hover = false; this.Invalidate(); base.OnMouseLeave(e); }
            protected override void OnCheckedChanged(EventArgs e) { this.Invalidate(); base.OnCheckedChanged(e); }
            protected override void OnGotFocus(EventArgs e) { this.Invalidate(); base.OnGotFocus(e); }
            protected override void OnLostFocus(EventArgs e) { this.Invalidate(); base.OnLostFocus(e); }

            protected override void OnPaint(PaintEventArgs e)
            {
                Graphics g = e.Graphics;
                g.Clear(Theme.Surface);
                g.SmoothingMode = SmoothingMode.AntiAlias;

                Rectangle bounds = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
                Color fillColor = this.Checked ? Theme.Lighten(Theme.Primary, 0.92f)
                    : this.hover ? Theme.SurfaceAlt : Theme.Surface;
                Color borderColor = this.Checked ? Theme.Primary : this.hover ? Theme.BorderStrong : Theme.Border;
                using (GraphicsPath path = Theme.RoundedPath(bounds, 10))
                using (SolidBrush fill = new SolidBrush(fillColor))
                using (Pen border = new Pen(borderColor, this.Checked ? 1.8f : 1f))
                {
                    g.FillPath(fill, path);
                    g.DrawPath(border, path);
                }

                // Indicador: circulo vacio, o circulo azul con visto cuando esta seleccionada.
                Rectangle dot = new Rectangle(13, (this.Height - 20) / 2, 20, 20);
                if (this.Checked)
                {
                    using (SolidBrush fill = new SolidBrush(Theme.Primary))
                        g.FillEllipse(fill, dot);
                    LucideIcons.Draw(g, "check", Color.White, new Rectangle(dot.X + 4, dot.Y + 4, 12, 12));
                }
                else
                {
                    using (SolidBrush fill = new SolidBrush(Theme.Surface))
                    using (Pen ring = new Pen(Theme.BorderStrong, 1.6f))
                    {
                        g.FillEllipse(fill, dot);
                        g.DrawEllipse(ring, dot);
                    }
                }

                TextRenderer.DrawText(g, this.Text, this.Checked ? Theme.BodyBold : Theme.Body,
                    new Rectangle(dot.Right + 10, 0, this.Width - dot.Right - 18, this.Height),
                    this.Enabled ? Theme.TextPrimary : Theme.TextMuted,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis
                    | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);

                if (this.Focused && this.ShowFocusCues)
                    using (GraphicsPath path = Theme.RoundedPath(Rectangle.Inflate(bounds, -3, -3), 8))
                    using (Pen focus = new Pen(Theme.Primary, 1f) { DashStyle = DashStyle.Dot })
                        g.DrawPath(focus, path);
            }
        }
    }
}
