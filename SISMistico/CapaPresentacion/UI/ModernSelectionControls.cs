using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CapaPresentacion.UI
{
    public class ModernComboBox : UserControl
    {
        private readonly ComboBox input;
        public ModernComboBox()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
            Height = 44;
            input = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, FlatStyle = FlatStyle.Flat,
                Font = Theme.Body, BackColor = Theme.Surface, ForeColor = Theme.TextPrimary,
                DrawMode = DrawMode.OwnerDrawFixed, ItemHeight = 25, IntegralHeight = false, DropDownHeight = 260 };
            input.DrawItem += (s, e) =>
            {
                e.DrawBackground();
                if (e.Index >= 0) TextRenderer.DrawText(e.Graphics, input.GetItemText(input.Items[e.Index]), Theme.Body,
                    e.Bounds, (e.State & DrawItemState.Selected) != 0 ? SystemColors.HighlightText : Theme.TextPrimary,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
                e.DrawFocusRectangle();
            };
            input.SelectedIndexChanged += (s, e) => { if (SelectedIndexChanged != null) SelectedIndexChanged(this, e); };
            input.GotFocus += (s, e) => Invalidate();
            input.LostFocus += (s, e) => Invalidate();
            Controls.Add(input);
            Resize += (s, e) => input.SetBounds(12, (Height - input.Height) / 2, Math.Max(20, Width - 24), input.Height);
        }
        public event EventHandler SelectedIndexChanged;
        public ComboBox.ObjectCollection Items { get { return input.Items; } }
        public int SelectedIndex { get { return input.SelectedIndex; } set { input.SelectedIndex = value; } }
        public object SelectedValue { get { return input.SelectedValue; } set { input.SelectedValue = value; } }
        public string DisplayMember { get { return input.DisplayMember; } set { input.DisplayMember = value; } }
        public string ValueMember { get { return input.ValueMember; } set { input.ValueMember = value; } }
        public object DataSource { get { return input.DataSource; } set { input.DataSource = value; } }
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(Theme.ContainerBackground(this));
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (var path = Theme.RoundedPath(new Rectangle(1, 1, Width - 3, Height - 3), 9))
            using (var fill = new SolidBrush(Theme.Surface))
            using (var border = new Pen(ContainsFocus ? Theme.Primary : Theme.BorderStrong, ContainsFocus ? 2 : 1))
            { e.Graphics.FillPath(fill, path); e.Graphics.DrawPath(border, path); }
        }
    }

    public class ModernRadioButton : RadioButton
    {
        public ModernRadioButton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Font = Theme.Body; Height = 42; Width = 150; Cursor = Cursors.Hand; AutoSize = false;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(Theme.ContainerBackground(this));
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var circle = new Rectangle(3, (Height - 22) / 2, 22, 22);
            Color accent = Enabled ? Theme.Primary : Theme.BorderStrong;
            using (var fill = new SolidBrush(Theme.Surface))
            using (var border = new Pen(Checked ? accent : Theme.BorderStrong, 1.8f))
            {
                e.Graphics.FillEllipse(fill, circle);
                e.Graphics.DrawEllipse(border, circle);
            }
            if (Checked)
                using (var fill = new SolidBrush(accent))
                    e.Graphics.FillEllipse(fill, Rectangle.Inflate(circle, -5, -5));
            TextRenderer.DrawText(e.Graphics, Text, Font, new Rectangle(36, 0, Width - 38, Height),
                Enabled ? Theme.TextPrimary : Theme.TextMuted, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
            if (Focused && ShowFocusCues) ControlPaint.DrawFocusRectangle(e.Graphics, Rectangle.Inflate(ClientRectangle, -1, -1));
        }
    }

    public class ModernCheckBox : CheckBox
    {
        public ModernCheckBox()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Font = Theme.Body; Height = 38; Cursor = Cursors.Hand; AutoSize = false;
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(Theme.ContainerBackground(this));
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var box = new Rectangle(2, (Height - 22) / 2, 22, 22);
            using (var path = Theme.RoundedPath(box, 6))
            using (var brush = new SolidBrush(Checked ? Theme.Primary : Theme.Surface))
            using (var pen = new Pen(Checked ? Theme.Primary : Theme.BorderStrong, 1.5f))
            { e.Graphics.FillPath(brush, path); e.Graphics.DrawPath(pen, path); }
            if (Checked)
                using (var pen = new Pen(Color.White, 2))
                    e.Graphics.DrawLines(pen, new[] { new Point(7, box.Y + 11), new Point(11, box.Y + 15), new Point(19, box.Y + 7) });
            TextRenderer.DrawText(e.Graphics, Text, Font, new Rectangle(36, 0, Width - 38, Height),
                Enabled ? Theme.TextPrimary : Theme.TextMuted, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
            if (Focused && ShowFocusCues) ControlPaint.DrawFocusRectangle(e.Graphics, ClientRectangle);
        }
    }
}
