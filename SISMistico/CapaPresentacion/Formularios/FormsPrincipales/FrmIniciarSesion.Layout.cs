using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CapaPresentacion.UI;

namespace CapaPresentacion.Formularios.FormsPrincipales
{
    public partial class FrmIniciarSesion
    {
        private readonly List<Font> ownedFonts = new List<Font>();
        private LoginPinEntry pinEntry;
        private PinActionButton submit;
        private Label status;
        private FlowLayoutPanel candidates;

        private Font LoginFont(float size, FontStyle style = FontStyle.Regular)
        {
            var font = new Font("Segoe UI", size, style);
            ownedFonts.Add(font);
            return font;
        }

        private void BuildPinLayout()
        {
            SuspendLayout();
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoScaleDimensions = new SizeF(96, 96);
            ClientSize = new Size(1120, 760);
            MinimumSize = new Size(740, 640);
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = LoginColors.Paper;
            Text = "Casa Grande | Iniciar sesi\u00f3n";
            Name = "FrmIniciarSesion";
            Icon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            var shell = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = Padding.Empty };
            shell.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 44));
            shell.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 56));
            shell.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            var story = new LoginStory { Dock = DockStyle.Fill, Margin = Padding.Empty };
            var right = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty, BackColor = LoginColors.Paper };
            var content = new Panel { Height = 590, Width = 420, BackColor = LoginColors.Paper };
            var badge = new Label
            {
                Text = "ACCESO DEL EQUIPO", Location = new Point(0, 0), Size = new Size(170, 28),
                Font = LoginFont(8.5f, FontStyle.Bold), ForeColor = LoginColors.Ink, BackColor = LoginColors.Lilac,
                TextAlign = ContentAlignment.MiddleCenter,
            };
            badge.Paint += (s, e) =>
            {
                using (var path = Theme.RoundedPath(badge.ClientRectangle, 12))
                {
                    Region previous = badge.Region; badge.Region = new Region(path); previous?.Dispose();
                }
            };
            Label welcome = new Label
            {
                Text = "Casa Grande", Location = new Point(-3, 52), Size = new Size(426, 100),
                Font = LoginFont(29, FontStyle.Bold), ForeColor = LoginColors.Ink,
            };
            Label description = new Label
            {
                Text = "Digita el PIN asignado", Location = new Point(0, 163), Size = new Size(420, 52),
                Font = LoginFont(11.5f), ForeColor = LoginColors.Muted,
            };
            Label caption = new Label
            {
                Text = "TU PIN DE ACCESO", Location = new Point(0, 237), Size = new Size(420, 23),
                Font = LoginFont(9, FontStyle.Bold), ForeColor = LoginColors.Ink,
            };
            pinEntry = new LoginPinEntry { Location = new Point(0, 269), Size = new Size(420, 78), TabIndex = 0 };
            status = new Label
            {
                Location = new Point(1, 355), Size = new Size(418, 44), Font = LoginFont(9.5f),
                ForeColor = LoginColors.Muted, AccessibleRole = AccessibleRole.Alert,
            };
            submit = new PinActionButton
            {
                Text = "Ingresar", Location = new Point(-5, 403), Size = new Size(430, 62),
                Primary = true, TabIndex = 1, AccessibleName = "Ingresar con mi PIN",
            };
            Label keyboard = new Label
            {
                Text = "Tambi\u00e9n puedes presionar Enter", Location = new Point(0, 480), Size = new Size(420, 24),
                TextAlign = ContentAlignment.MiddleCenter, Font = LoginFont(9), ForeColor = LoginColors.Muted,
            };
            Label help = new Label
            {
                Text = "\u00bfNo recuerdas tu PIN?\nPide ayuda a administraci\u00f3n.", Location = new Point(0, 532), Size = new Size(420, 46),
                TextAlign = ContentAlignment.MiddleCenter, Font = LoginFont(9.5f), ForeColor = LoginColors.Muted,
            };
            candidates = new FlowLayoutPanel
            {
                Location = new Point(0, 404), Size = new Size(420, 160), Visible = false,
                AutoScroll = true, FlowDirection = FlowDirection.TopDown, WrapContents = false, TabIndex = 2,
            };
            candidates.VisibleChanged += (s, e) =>
            {
                submit.Visible = !candidates.Visible;
                keyboard.Visible = !candidates.Visible;
                help.Visible = !candidates.Visible;
                AcceptButton = candidates.Visible ? null : submit;
            };
            content.Controls.AddRange(new Control[] { badge, welcome, description, caption, pinEntry, status, submit, keyboard, help, candidates });
            var close = new PinActionButton { Text = "\u00d7", Quiet = true, Size = new Size(38, 38), AccessibleName = "Cerrar aplicaci\u00f3n", TabIndex = 4 };
            var minimize = new PinActionButton { Text = "\u2013", Quiet = true, Size = new Size(38, 38), AccessibleName = "Minimizar", TabIndex = 3 };
            close.Click += (s, e) => Close();
            minimize.Click += (s, e) => WindowState = FormWindowState.Minimized;
            right.Controls.Add(content);
            right.Controls.Add(close);
            right.Controls.Add(minimize);
            shell.Controls.Add(story, 0, 0);
            shell.Controls.Add(right, 1, 0);
            Controls.Add(shell);
            AcceptButton = submit;
            right.Resize += (s, e) =>
            {
                int width = Math.Min(420, Math.Max(300, right.ClientSize.Width - 70));
                content.Width = width;
                content.Location = new Point((right.Width - width) / 2, Math.Max(52, (right.Height - content.Height) / 2));
                foreach (Control child in content.Controls)
                    if (child != badge) child.Width = child == submit ? width + 10 : width;
                close.Location = new Point(right.Width - 54, 12);
                minimize.Location = new Point(right.Width - 96, 12);
                foreach (Control choice in candidates.Controls) choice.Width = candidates.ClientSize.Width - 22;
            };
            Resize += (s, e) =>
            {
                bool compact = ClientSize.Width < 900;
                shell.ColumnStyles[0].SizeType = compact ? SizeType.Absolute : SizeType.Percent;
                shell.ColumnStyles[0].Width = compact ? 0 : 44;
                shell.ColumnStyles[1].Width = compact ? 100 : 56;
                story.Visible = !compact;
                if (Width > 0 && Height > 0 && WindowState != FormWindowState.Minimized)
                {
                    using (var path = Theme.RoundedPath(ClientRectangle, 22))
                    {
                        Region previous = Region; Region = new Region(path); previous?.Dispose();
                    }
                }
            };
            MouseEventHandler drag = (s, e) =>
            {
                if (e.Button != MouseButtons.Left) return;
                ReleaseCapture(); SendMessage(Handle, 0xA1, new IntPtr(2), IntPtr.Zero);
            };
            story.MouseDown += drag;
            right.MouseDown += drag;
            ResumeLayout(true);
            using (var path = Theme.RoundedPath(ClientRectangle, 22)) Region = new Region(path);
        }

        [DllImport("user32.dll")] private static extern bool ReleaseCapture();
        [DllImport("user32.dll")] private static extern IntPtr SendMessage(IntPtr window, int message, IntPtr wParam, IntPtr lParam);
    }

    internal static class LoginColors
    {
        public static readonly Color Paper = Color.FromArgb(253, 252, 249);
        public static readonly Color Sage = Color.FromArgb(231, 239, 230);
        public static readonly Color Ink = Color.FromArgb(42, 64, 55);
        public static readonly Color Muted = Color.FromArgb(105, 120, 110);
        public static readonly Color Primary = Color.FromArgb(65, 101, 81);
        public static readonly Color Lilac = Color.FromArgb(238, 232, 246);
        public static readonly Color Peach = Color.FromArgb(244, 220, 206);
        public static readonly Color Border = Color.FromArgb(208, 221, 208);
        public static readonly Color Error = Color.FromArgb(153, 64, 66);
    }

    internal sealed class LoginStory : Control
    {
        public LoginStory()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = LoginColors.Sage;
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var ink = new SolidBrush(LoginColors.Ink))
            using (var muted = new SolidBrush(LoginColors.Muted))
            using (var small = new Font("Segoe UI", 10f, FontStyle.Bold))
            using (var title = new Font("Georgia", 31f))
            using (var body = new Font("Segoe UI", 11f))
            using (var caption = new Font("Segoe UI", 8.5f))
            {
                int left = 48;
                using (var mark = new Pen(LoginColors.Primary, 1.7f))
                {
                    g.DrawEllipse(mark, left, 46, 36, 36);
                    g.DrawLines(mark, new[] { new Point(left + 9, 65), new Point(left + 18, 56), new Point(left + 27, 65) });
                    g.DrawLine(mark, left + 12, 64, left + 12, 73);
                    g.DrawLine(mark, left + 24, 64, left + 24, 73);
                    g.DrawLine(mark, left + 12, 73, left + 24, 73);
                }
                g.DrawString("", small, ink, left + 50, 55);
                g.DrawString("Sistema de restaurante", title, ink, new RectangleF(left - 2, 136, Width - left - 22, 130));
                g.DrawString("Manizales, Caldas", body, muted, new RectangleF(left, 274, Width - left - 32, 58));
                int size = Math.Min(Width - 96, 290);
                int top = Math.Max(345, Height - size - 110);
                Rectangle arch = new Rectangle((Width - size) / 2, top, size, size);
                using (var archFill = new SolidBrush(Color.FromArgb(245, 242, 223)))
                using (var archPath = Theme.RoundedPath(arch, size / 2)) g.FillPath(archFill, archPath);
                using (var peach = new SolidBrush(LoginColors.Peach)) g.FillEllipse(peach, arch.Right - 86, arch.Top + 24, 104, 104);
                Rectangle plate = new Rectangle(arch.X + 32, arch.Y + 60, size - 64, size - 64);
                using (var shade = new SolidBrush(Color.FromArgb(24, 73, 100, 75))) g.FillEllipse(shade, plate.X + 3, plate.Y + 12, plate.Width, plate.Height);
                using (var white = new SolidBrush(LoginColors.Paper)) g.FillEllipse(white, plate);
                using (var rim = new Pen(LoginColors.Border, 1.5f)) g.DrawEllipse(rim, Rectangle.Inflate(plate, -18, -18));
                using (var leaf = new Pen(LoginColors.Primary, 2f))
                {
                    int cx = plate.X + plate.Width / 2, cy = plate.Y + plate.Height / 2;
                    g.DrawArc(leaf, cx - 18, cy - 20, 38, 40, 40, 250);
                    g.DrawLine(leaf, cx - 10, cy + 13, cx + 12, cy - 14);
                    g.DrawLine(leaf, cx - 9, cy + 5, cx - 9, cy - 8);
                    g.DrawLine(leaf, cx - 1, cy - 3, cx + 11, cy - 3);
                }
                using (var line = new Pen(LoginColors.Primary, 2f))
                {
                    int x = arch.X + 16, y = plate.Y + 24;
                    g.DrawLine(line, x, y, x, y + 102);
                    g.DrawLine(line, x - 5, y, x - 5, y + 22);
                    g.DrawLine(line, x + 5, y, x + 5, y + 22);
                    g.DrawArc(line, x - 5, y + 17, 10, 10, 0, 180);
                    x = arch.Right - 16;
                    g.DrawLine(line, x, y, x, y + 102);
                    g.DrawArc(line, x - 12, y, 12, 54, 90, 180);
                }
                g.DrawString("SOFTWARE HOUSE 2026", caption, muted,
                    new RectangleF(left, Height - 54, Width - 60, 24));
            }
        }
    }

    internal sealed class PinActionButton : Button
    {
        public bool Primary { get; set; }
        public bool Quiet { get; set; }
        private bool hover;
        private bool pressed;
        public PinActionButton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            FlatStyle = FlatStyle.Flat; FlatAppearance.BorderSize = 0;
            Font = new Font("Segoe UI", 11f, FontStyle.Bold);
            Cursor = Cursors.Hand; UseVisualStyleBackColor = false;
        }
        protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { hover = pressed = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e) { pressed = true; Invalidate(); base.OnMouseDown(e); }
        protected override void OnMouseUp(MouseEventArgs e) { pressed = false; Invalidate(); base.OnMouseUp(e); }
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics; g.Clear(Parent?.BackColor ?? LoginColors.Paper); g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle bounds = Rectangle.Inflate(ClientRectangle, Quiet ? -1 : -5, Quiet ? -1 : -5);
            if (bounds.Width < 4 || bounds.Height < 4) return;
            Color fill = Primary ? LoginColors.Primary : Quiet ? LoginColors.Paper : Color.White;
            if (Enabled && (hover || pressed)) fill = Primary ? Theme.Darken(fill, pressed ? 0.16f : 0.08f) : LoginColors.Sage;
            if (!Enabled) fill = Primary ? Theme.Lighten(LoginColors.Primary, 0.5f) : LoginColors.Paper;
            if (Primary && Enabled) Theme.DrawShadow(g, bounds, 17, 4);
            using (var path = Theme.RoundedPath(bounds, Quiet ? 12 : 17))
            using (var brush = new SolidBrush(fill))
            using (var border = new Pen(Primary || Quiet ? fill : LoginColors.Border))
            { g.FillPath(brush, path); g.DrawPath(border, path); }
            Color fore = Primary ? Color.White : LoginColors.Ink;
            TextRenderer.DrawText(g, Text, Font, Rectangle.Inflate(bounds, -8, -2), fore,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            if (Focused && ShowFocusCues) ControlPaint.DrawFocusRectangle(g, Rectangle.Inflate(bounds, -5, -5), fore, fill);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing) Font.Dispose();
            base.Dispose(disposing);
        }
    }

    internal sealed class LoginPinEntry : Control
    {
        private readonly TextBox input = new TextBox();
        private readonly PinActionButton reveal = new PinActionButton();
        private readonly Font pinFont = new Font("Segoe UI", 25f);
        private bool hasError;
        public bool HasError { get => hasError; set { hasError = value; Invalidate(); } }
        public event EventHandler Submit;
        public bool ReadOnly
        {
            get => input.ReadOnly;
            set { input.ReadOnly = value; reveal.Enabled = !value; }
        }
        public override string Text { get => input.Text; set => input.Text = value ?? ""; }
        public LoginPinEntry()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            TabStop = false; Height = 78;
            input.BorderStyle = BorderStyle.None; input.Font = pinFont;
            input.UseSystemPasswordChar = true; input.MaxLength = 10; input.TextAlign = HorizontalAlignment.Center;
            input.BackColor = Color.White; input.ForeColor = LoginColors.Ink; input.TabIndex = 0;
            input.AccessibleName = "PIN de acceso. El mismo codigo que usas en la tablet.";
            input.Enter += (s, e) => Invalidate(); input.Leave += (s, e) => Invalidate();
            input.TextChanged += (s, e) => { HasError = false; Invalidate(); OnTextChanged(EventArgs.Empty); };
            input.KeyPress += (s, e) => { if (!char.IsControl(e.KeyChar) && (e.KeyChar < '0' || e.KeyChar > '9')) e.Handled = true; };
            input.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; Submit?.Invoke(this, EventArgs.Empty); }
                if (e.KeyCode == Keys.Escape) { e.SuppressKeyPress = true; input.Clear(); }
            };
            Font originalRevealFont = reveal.Font;
            reveal.Font = new Font("Segoe UI", 9f);
            originalRevealFont.Dispose();
            reveal.Text = "Mostrar"; reveal.Quiet = true; reveal.TabIndex = 1; reveal.AccessibleName = "Mostrar PIN";
            reveal.Click += (s, e) =>
            {
                input.UseSystemPasswordChar = !input.UseSystemPasswordChar;
                reveal.Text = input.UseSystemPasswordChar ? "Mostrar" : "Ocultar";
                reveal.AccessibleName = reveal.Text + " PIN";
                input.Focus(); input.SelectionStart = input.TextLength;
            };
            Controls.Add(input); Controls.Add(reveal);
            Resize += (s, e) =>
            {
                input.SetBounds(48, Math.Max(8, (Height - input.PreferredHeight) / 2), Math.Max(20, Width - 130), input.PreferredHeight);
                reveal.SetBounds(Width - 76, (Height - 38) / 2, 66, 38);
            };
            Click += (s, e) => input.Focus();
        }
        public void FocusInput() => input.Focus();
        public void SelectAll() { input.Focus(); input.SelectAll(); }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e); Graphics g = e.Graphics;
            g.Clear(Parent?.BackColor ?? LoginColors.Paper); g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle bounds = Rectangle.Inflate(ClientRectangle, -2, -2);
            if (bounds.Width < 4 || bounds.Height < 4) return;
            Color color = HasError ? LoginColors.Error : ContainsFocus ? LoginColors.Primary : LoginColors.Border;
            using (var path = Theme.RoundedPath(bounds, 18))
            using (var brush = new SolidBrush(Color.White))
            using (var pen = new Pen(color, ContainsFocus || HasError ? 2f : 1.2f))
            {
                g.FillPath(brush, path);
                g.DrawPath(pen, path);
            }
            int y = Height / 2;
            using (var pen = new Pen(color, 1.6f))
            {
                g.DrawArc(pen, 22, y - 13, 12, 15, 180, 180);
                g.DrawRectangle(pen, 19, y - 5, 18, 15);
                g.DrawLine(pen, 28, y, 28, y + 4);
            }
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing) pinFont.Dispose();
        }
    }
}
