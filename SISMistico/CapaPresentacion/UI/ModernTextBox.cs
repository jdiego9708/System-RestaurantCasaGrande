using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using FontAwesome.Sharp;

namespace CapaPresentacion.UI
{
    /// <summary>
    /// Caja de texto moderna: borde redondeado que se resalta al tomar el foco, icono opcional a la izquierda,
    /// texto de ayuda (placeholder) y estado de error en rojo. Envuelve un TextBox sin borde, asi que Text,
    /// TextChanged, KeyDown y Focus() se usan igual que en una caja normal.
    /// </summary>
    public class ModernTextBox : Control
    {
        private const int EM_SETCUEBANNER = 0x1501;

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, string lParam);

        private readonly TextBox inner = new TextBox
        {
            BorderStyle = BorderStyle.None,
            Font = new Font("Segoe UI", 11F),
            BackColor = Theme.Surface,
            ForeColor = Theme.TextPrimary,
        };

        private IconChar icon = IconChar.None;
        private string placeholder = string.Empty;
        private bool focused;
        private bool hasError;

        public ModernTextBox()
        {
            // El foco lo toma la caja interna, no este contenedor.
            this.SetStyle(ControlStyles.Selectable, false);
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            this.TabStop = false;
            this.Height = 42;
            this.Controls.Add(this.inner);

            this.inner.GotFocus += (s, e) => { this.focused = true; this.Invalidate(); };
            this.inner.LostFocus += (s, e) => { this.focused = false; this.Invalidate(); };
            // Al escribir se quita el error: el usuario ya esta corrigiendo.
            this.inner.TextChanged += (s, e) =>
            {
                this.HasError = false;
                this.OnTextChanged(EventArgs.Empty);
            };
            this.inner.KeyDown += (s, e) => this.OnKeyDown(e);
            this.inner.KeyPress += (s, e) => this.OnKeyPress(e);
        }

        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
        public override string Text
        {
            get => this.inner.Text;
            set => this.inner.Text = value ?? string.Empty;
        }

        [DefaultValue(IconChar.None)]
        public IconChar Icon
        {
            get => this.icon;
            set { this.icon = value; this.LayoutInner(); this.Invalidate(); }
        }

        /// <summary>Texto gris que se ve mientras la caja esta vacia.</summary>
        [DefaultValue("")]
        public string PlaceholderText
        {
            get => this.placeholder;
            set { this.placeholder = value ?? string.Empty; this.ApplyPlaceholder(); }
        }

        /// <summary>Marca el campo en rojo (por ejemplo, un obligatorio vacio); se quita solo al escribir.</summary>
        [DefaultValue(false)]
        public bool HasError
        {
            get => this.hasError;
            set { if (this.hasError != value) { this.hasError = value; this.Invalidate(); } }
        }

        public bool ReadOnly
        {
            get => this.inner.ReadOnly;
            set => this.inner.ReadOnly = value;
        }

        public int MaxLength
        {
            get => this.inner.MaxLength;
            set => this.inner.MaxLength = value;
        }

        /// <summary>Lleva el foco a la caja interna (Control.Focus() no llegaria hasta ella).</summary>
        public new bool Focus() => this.inner.Focus();

        public void SelectAll() => this.inner.SelectAll();

        private void ApplyPlaceholder()
        {
            if (this.inner.IsHandleCreated)
                SendMessage(this.inner.Handle, EM_SETCUEBANNER, (IntPtr)1, this.placeholder);
            else
                this.inner.HandleCreated += (s, e) => SendMessage(this.inner.Handle, EM_SETCUEBANNER, (IntPtr)1, this.placeholder);
        }

        private void LayoutInner()
        {
            int left = this.icon != IconChar.None ? 40 : 14;
            int height = this.inner.PreferredHeight;
            this.inner.SetBounds(left, (this.Height - height) / 2, Math.Max(10, this.Width - left - 14), height);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            this.LayoutInner();
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            this.inner.BackColor = this.Enabled ? Theme.Surface : Theme.SurfaceAlt;
            this.Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            this.inner.Focus();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(this.Parent?.BackColor ?? Theme.Background);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle bounds = new Rectangle(1, 1, this.Width - 3, this.Height - 3);
            Color border = this.hasError ? Theme.Danger : this.focused ? Theme.Primary : Theme.BorderStrong;
            using (GraphicsPath path = Theme.RoundedPath(bounds, 9))
            using (SolidBrush fill = new SolidBrush(this.Enabled ? Theme.Surface : Theme.SurfaceAlt))
            {
                g.FillPath(fill, path);
                if (this.focused && !this.hasError)
                {
                    // Halo suave alrededor del campo con foco.
                    using (Pen halo = new Pen(Color.FromArgb(60, Theme.Primary), 4f))
                        g.DrawPath(halo, path);
                }
                using (Pen pen = new Pen(border, this.focused || this.hasError ? 1.8f : 1.2f))
                    g.DrawPath(pen, path);
            }

            if (this.icon != IconChar.None)
            {
                Color tint = this.hasError ? Theme.Danger : this.focused ? Theme.Primary : Theme.TextMuted;
                using (Bitmap bitmap = this.icon.ToBitmap(tint, 16))
                    g.DrawImage(bitmap, 14, (this.Height - 16) / 2, 16, 16);
            }
        }
    }
}
