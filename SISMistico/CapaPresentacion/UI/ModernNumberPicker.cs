using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;

namespace CapaPresentacion.UI
{
    /// <summary>Selector compacto de 0 a 99, editable y con flechas de incremento.</summary>
    public sealed class ModernNumberPicker : UserControl
    {
        private readonly TextBox input = new TextBox
        {
            BorderStyle = BorderStyle.None, TextAlign = HorizontalAlignment.Center,
            Font = Theme.BodyBold, BackColor = Theme.Surface, ForeColor = Theme.TextPrimary,
            MaxLength = 2, Text = "1", AccessibleName = "Copias",
        };
        private int stepWidth = 22;

        /// <summary>Ancho de cada flecha. 22 px por defecto; 30 o mas para pantallas tactiles.</summary>
        public int StepWidth
        {
            get => stepWidth;
            set { stepWidth = Math.Max(16, value); LayoutInput(); Invalidate(); }
        }
        private int value = 1;
        private int hovered;

        public ModernNumberPicker()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Size = new Size(70, 36);
            MinimumSize = new Size(70, 32);
            AccessibleName = "Copias";
            AccessibleDescription = "Cantidad de copias. Cero omite la impresion.";
            Controls.Add(input);
            input.GotFocus += (s, e) => { input.SelectAll(); Invalidate(); };
            input.LostFocus += (s, e) => { Commit(); Invalidate(); };
            input.KeyPress += (s, e) => e.Handled = !char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar);
            input.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
                {
                    Step(e.KeyCode == Keys.Up ? 1 : -1);
                    e.SuppressKeyPress = true;
                }
                else if (e.KeyCode == Keys.Enter)
                {
                    Commit();
                    input.SelectAll();
                    e.SuppressKeyPress = true;
                }
            };
            LayoutInput();
        }

        public int Value
        {
            get { Commit(); return value; }
            set
            {
                this.value = Math.Max(0, Math.Min(99, value));
                input.Text = this.value.ToString(CultureInfo.InvariantCulture);
                Invalidate();
            }
        }

        private void Commit()
        {
            int parsed;
            if (int.TryParse(input.Text, out parsed)) value = Math.Max(0, Math.Min(99, parsed));
            input.Text = value.ToString(CultureInfo.InvariantCulture);
        }

        private void Step(int amount)
        {
            Value = Value + amount;
            input.Focus();
            input.SelectAll();
        }

        private void LayoutInput()
        {
            input.SetBounds(stepWidth, Math.Max(4, (Height - input.PreferredHeight) / 2), Width - stepWidth * 2, input.PreferredHeight);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (input != null) LayoutInput();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int next = DirectionAt(e.X);
            if (hovered != next) { hovered = next; Invalidate(); }
            Cursor = next == 0 ? Cursors.IBeam : Cursors.Hand;
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            hovered = 0;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left) return;
            int direction = DirectionAt(e.X);
            if (direction != 0) Step(direction);
            else input.Focus();
        }

        private int DirectionAt(int x) => x < stepWidth ? -1 : x >= Width - stepWidth ? 1 : 0;

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Theme.ContainerBackground(this));
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle bounds = new Rectangle(1, 1, Width - 3, Height - 3);
            using (GraphicsPath path = Theme.RoundedPath(bounds, 9))
            using (SolidBrush fill = new SolidBrush(Theme.Surface))
            using (Pen border = new Pen(ContainsFocus ? Theme.Primary : Theme.BorderStrong, ContainsFocus ? 1.6f : 1f))
            {
                g.FillPath(fill, path);
                g.DrawPath(border, path);
            }
            foreach (int direction in new[] { 1, -1 })
            {
                int x = direction == -1 ? stepWidth / 2 + 1 : Width - stepWidth / 2 - 1;
                int y = Height / 2;
                bool limit = direction == 1 ? value == 99 : value == 0;
                if (hovered == direction && !limit)
                    using (GraphicsPath hoverPath = Theme.RoundedPath(new Rectangle(x - 9, 5, 18, Height - 10), 6))
                    using (SolidBrush highlight = new SolidBrush(Theme.Lighten(Theme.Primary, 0.92f)))
                        g.FillPath(highlight, hoverPath);
                using (Pen arrow = new Pen(limit ? Theme.BorderStrong : hovered == direction ? Theme.Primary : Theme.TextMuted, 1.6f))
                    g.DrawLines(arrow, new[] { new Point(x - 3, y + direction), new Point(x, y - 2 * direction), new Point(x + 3, y + direction) });
            }
        }
    }
}
