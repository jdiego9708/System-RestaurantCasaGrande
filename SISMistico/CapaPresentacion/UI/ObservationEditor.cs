using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CapaPresentacion.UI
{
    /// <summary>
    /// Editor de una nota de texto pensado para mostrarse en un popup: encabezado con icono, titulo y
    /// subtitulo, area de texto con borde que se resalta al escribir y botones Cancelar / Guardar.
    /// Ctrl+Enter guarda y Esc cancela. Quien lo aloja decide que hacer al recibir Saved o Cancelled.
    /// </summary>
    public class ObservationEditor : UserControl
    {
        private const int Margin_ = 16;
        private const int HeaderHeight = 56;
        private const int FooterHeight = 56;

        private readonly TextBox input = new TextBox
        {
            BorderStyle = BorderStyle.None,
            Multiline = true,
            AcceptsReturn = true,
            ScrollBars = ScrollBars.Vertical,
            Font = new Font("Segoe UI", 11F),
            ForeColor = Theme.TextPrimary,
            BackColor = Theme.Surface,
        };
        private readonly MinimalIconButton saveButton = new MinimalIconButton
        {
            MinimalStyle = true,
            LucideIcon = "check",
            Text = "Guardar",
            AccentColor = Theme.Success,
            CornerRadius = 9,
            Size = new Size(118, 38),
            TabStop = false,
        };
        private readonly MinimalIconButton cancelButton = new MinimalIconButton
        {
            MinimalStyle = true,
            LucideIcon = "x",
            Text = "Cancelar",
            CornerRadius = 9,
            Size = new Size(118, 38),
            TabStop = false,
        };

        private string title = "Observaciones";
        private string subtitle = string.Empty;
        private bool inputFocused;

        public event EventHandler Saved;
        public event EventHandler Cancelled;

        public ObservationEditor()
        {
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            this.BackColor = Theme.Surface;
            this.Size = new Size(380, 236);

            this.input.GotFocus += (s, e) => { this.inputFocused = true; this.Invalidate(); };
            this.input.LostFocus += (s, e) => { this.inputFocused = false; this.Invalidate(); };
            this.input.KeyDown += this.Input_KeyDown;
            this.saveButton.Click += (s, e) => this.Saved?.Invoke(this, EventArgs.Empty);
            this.cancelButton.Click += (s, e) => this.Cancelled?.Invoke(this, EventArgs.Empty);

            this.Controls.Add(this.input);
            this.Controls.Add(this.saveButton);
            this.Controls.Add(this.cancelButton);
            this.PerformEditorLayout();
        }

        /// <summary>Texto de la nota.</summary>
        public string Note
        {
            get => this.input.Text;
            set => this.input.Text = value ?? string.Empty;
        }

        public string Title
        {
            get => this.title;
            set { this.title = value ?? string.Empty; this.Invalidate(); }
        }

        /// <summary>Linea gris bajo el titulo; normalmente el nombre del producto.</summary>
        public string Subtitle
        {
            get => this.subtitle;
            set { this.subtitle = value ?? string.Empty; this.Invalidate(); }
        }

        /// <summary>Pone el cursor al final del texto para seguir escribiendo.</summary>
        public void FocusInput()
        {
            this.input.Focus();
            this.input.SelectionStart = this.input.TextLength;
        }

        private Rectangle InputFrame =>
            new Rectangle(Margin_, HeaderHeight, this.Width - Margin_ * 2, this.Height - HeaderHeight - FooterHeight);

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            this.PerformEditorLayout();
        }

        private void PerformEditorLayout()
        {
            Rectangle frame = this.InputFrame;
            this.input.SetBounds(frame.X + 10, frame.Y + 8, Math.Max(20, frame.Width - 20), Math.Max(20, frame.Height - 16));
            int buttonsTop = this.Height - FooterHeight + (FooterHeight - this.saveButton.Height) / 2;
            this.saveButton.Location = new Point(this.Width - Margin_ - this.saveButton.Width, buttonsTop);
            this.cancelButton.Location = new Point(this.saveButton.Left - 8 - this.cancelButton.Width, buttonsTop);
        }

        private void Input_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                e.SuppressKeyPress = true;
                this.Cancelled?.Invoke(this, EventArgs.Empty);
            }
            else if (e.Control && e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                this.Saved?.Invoke(this, EventArgs.Empty);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            // Un clic sobre el marco del texto tambien lleva el cursor a la caja.
            if (this.InputFrame.Contains(e.Location))
                this.input.Focus();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Theme.Surface);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Insignia con el icono de la nota, igual que las insignias de icono de Domicilios.
            Rectangle badge = new Rectangle(Margin_, 12, 32, 32);
            using (GraphicsPath path = Theme.RoundedPath(badge, 9))
            using (SolidBrush fill = new SolidBrush(Theme.Lighten(Theme.Warning, 0.86f)))
                g.FillPath(fill, path);
            LucideIcons.Draw(g, "message-square-text", Theme.Darken(Theme.Warning, 0.25f),
                new Rectangle(badge.X + 7, badge.Y + 7, 18, 18));

            const TextFormatFlags flags = TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis
                | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding | TextFormatFlags.VerticalCenter;
            int textLeft = badge.Right + 10;
            int textWidth = this.Width - textLeft - Margin_;
            bool hasSubtitle = this.subtitle.Length > 0;
            TextRenderer.DrawText(g, this.title, Theme.Heading,
                new Rectangle(textLeft, hasSubtitle ? 10 : 12, textWidth, hasSubtitle ? 20 : 32), Theme.TextPrimary, flags);
            if (hasSubtitle)
                TextRenderer.DrawText(g, this.subtitle, Theme.Small,
                    new Rectangle(textLeft, 30, textWidth, 16), Theme.TextMuted, flags);

            // Marco del area de texto: azul mientras se escribe, gris el resto del tiempo.
            Rectangle frame = this.InputFrame;
            frame.Width -= 1;
            frame.Height -= 1;
            using (GraphicsPath path = Theme.RoundedPath(frame, 10))
            using (SolidBrush fill = new SolidBrush(Theme.Surface))
            using (Pen border = new Pen(this.inputFocused ? Theme.Primary : Theme.BorderStrong, this.inputFocused ? 1.6f : 1f))
            {
                g.FillPath(fill, path);
                g.DrawPath(border, path);
            }

            // Borde exterior del popup (el menu desplegable no trae uno propio).
            using (Pen outline = new Pen(Theme.BorderStrong))
                g.DrawRectangle(outline, 0, 0, this.Width - 1, this.Height - 1);
        }
    }
}
