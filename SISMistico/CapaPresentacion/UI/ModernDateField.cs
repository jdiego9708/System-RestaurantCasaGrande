using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;

namespace CapaPresentacion.UI
{
    /// <summary>Campo y calendario propios; el DateTimePicker conserva el valor y las reglas del formulario.</summary>
    internal sealed class ModernDateField : Button
    {
        private readonly DateTimePicker source;
        private readonly ToolStripDropDown popup;
        private readonly CalendarPanel calendar;
        private bool hovered;
        private bool opening;
        private Task preparation;
        private Task<CalendarData> dataTask = CalendarData.GetShared();
        private readonly CultureInfo culture = CultureInfo.GetCultureInfo("es-CO");

        public ModernDateField(DateTimePicker source)
        {
            this.source = source;
            SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            Font = Theme.Body;
            Cursor = Cursors.Hand;
            BackColor = Theme.Surface;
            AccessibleName = source.AccessibleName;
            AccessibleDescription = "Abre el calendario con Enter, Espacio o Alt + flecha abajo.";
            calendar = new CalendarPanel(source, SelectDate);
            var host = new ToolStripControlHost(calendar) { Margin = Padding.Empty, Padding = Padding.Empty, AutoSize = false, Size = calendar.Size };
            popup = new ToolStripDropDown { Padding = new Padding(1), BackColor = Theme.Border, AutoClose = true };
            popup.Items.Add(host);
            popup.Closed += delegate { Invalidate(); };
            source.ValueChanged += SourceChanged;
        }

        private void SourceChanged(object sender, EventArgs e)
        {
            AccessibleDescription = source.Value.ToString("D", culture);
            Invalidate();
        }

        private void SelectDate(DateTime value)
        {
            source.Value = value.Date;
            popup.Close(ToolStripDropDownCloseReason.ItemClicked);
            Focus();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            // Encolar la preparaci?n permite que la ventana termine de abrirse primero.
            BeginInvoke(new Action(async () => await PrepareCalendar()));
        }

        private Task PrepareCalendar()
        {
            if (preparation == null) preparation = PrepareCalendarCore();
            return preparation;
        }

        private async Task PrepareCalendarCore()
        {
            var data = await dataTask;
            if (IsDisposed || Disposing) return;
            calendar.Data = data;
            calendar.RefreshMonth();
            // WinForms exige crear los controles en el hilo de interfaz, sin mostrarlos.
            PrepareHandles(popup);
        }

        private static void PrepareHandles(Control control)
        {
            var handle = control.Handle;
            foreach (Control child in control.Controls) PrepareHandles(child);
        }

        protected override async void OnClick(EventArgs e)
        {
            base.OnClick(e);
            if (popup.Visible) { popup.Close(); return; }
            if (opening) return;
            opening = true;
            try
            {
                var currentData = CalendarData.GetShared();
                if (currentData != dataTask) { dataTask = currentData; preparation = null; }
                await PrepareCalendar();
                if (IsDisposed || Disposing || !ContainsFocus) return;
                calendar.RefreshMonth();
                popup.Show(this, new Point(0, Height + 5));
                calendar.FocusSelected();
                Invalidate();
            }
            finally { opening = false; }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Alt | Keys.Down) || keyData == Keys.F4)
            { OnClick(EventArgs.Empty); return true; }
            return base.ProcessCmdKey(ref msg, keyData);
        }
        protected override void OnMouseEnter(EventArgs e) { hovered = true; base.OnMouseEnter(e); Invalidate(); }
        protected override void OnMouseLeave(EventArgs e) { hovered = false; base.OnMouseLeave(e); Invalidate(); }
        protected override void OnGotFocus(EventArgs e) { base.OnGotFocus(e); Invalidate(); }
        protected override void OnLostFocus(EventArgs e) { base.OnLostFocus(e); Invalidate(); }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(BackColor);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            bool active = Focused || popup.Visible;
            var bounds = new Rectangle(3, 3, Width - 7, Height - 7);
            using (var path = Theme.RoundedPath(bounds, 14))
            using (var fill = new SolidBrush(hovered || active ? Theme.Lighten(Theme.Primary, .97f) : Theme.Surface))
            using (var pen = new Pen(active ? Theme.Primary : Theme.BorderStrong, active ? 2 : 1))
            { g.FillPath(fill, path); g.DrawPath(pen, path); }
            var iconBounds = new Rectangle(14, (Height - 32) / 2, 32, 32);
            using (var path = Theme.RoundedPath(iconBounds, 10))
            using (var brush = new SolidBrush(Theme.Lighten(Theme.Primary, .91f))) g.FillPath(brush, path);
            using (var pen = new Pen(Theme.Primary, 1.6f))
            {
                int x = 22, y = Height / 2 - 8;
                g.DrawRectangle(pen, x, y + 2, 16, 14);
                g.DrawLine(pen, x, y + 7, x + 16, y + 7);
                g.DrawLine(pen, x + 4, y - 1, x + 4, y + 4);
                g.DrawLine(pen, x + 12, y - 1, x + 12, y + 4);
                g.DrawLine(pen, Width - 24, Height / 2 - 2, Width - 20, Height / 2 + 2);
                g.DrawLine(pen, Width - 20, Height / 2 + 2, Width - 16, Height / 2 - 2);
            }
            TextRenderer.DrawText(g, source.Value.ToString("dd MMM yyyy", culture), Theme.BodyBold,
                new Rectangle(56, 7, Width - 90, Height - 14), Enabled ? Theme.TextPrimary : Theme.TextMuted,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) { source.ValueChanged -= SourceChanged; popup.Dispose(); }
            base.Dispose(disposing);
        }

        // Solo datos: se calculan en segundo plano una vez al d?a y se comparten
        // entre Fecha de inicio y Fecha de fin. Ning?n control cruza de hilo.
        private sealed class CalendarData
        {
            private static readonly object Sync = new object();
            private static DateTime cachedDay;
            private static Task<CalendarData> shared;
            public readonly DateTime Minimum;
            public readonly DateTime Maximum;
            public readonly object[] Years;
            public readonly Dictionary<DateTime, string> DayDescriptions;

            private CalendarData(DateTime today)
            {
                Minimum = today.AddYears(-2);
                Maximum = today.AddMonths(1);
                Years = new object[Maximum.Year - Minimum.Year + 1];
                for (int i = 0; i < Years.Length; i++) Years[i] = Minimum.Year + i;
                var culture = CultureInfo.GetCultureInfo("es-CO");
                DayDescriptions = new Dictionary<DateTime, string>();
                for (var date = Minimum; date <= Maximum; date = date.AddDays(1))
                    DayDescriptions.Add(date, date.ToString("D", culture));
            }

            public static Task<CalendarData> GetShared()
            {
                var today = DateTime.Today;
                lock (Sync)
                {
                    if (shared == null || cachedDay != today)
                    {
                        cachedDay = today;
                        shared = Task.Run(() => new CalendarData(today));
                    }
                    return shared;
                }
            }
        }

        private sealed class CalendarButton : ModernButton
        {
            private bool hover;
            protected override void OnMouseEnter(EventArgs e) { hover = true; base.OnMouseEnter(e); }
            protected override void OnMouseLeave(EventArgs e) { hover = false; base.OnMouseLeave(e); }
            protected override void OnPaint(PaintEventArgs e)
            {
                var g = e.Graphics;
                g.Clear(Theme.Surface);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                bool selected = Kind == ButtonKind.Primary;
                Color fill = selected ? Theme.Primary : hover && Enabled ? Theme.Lighten(Theme.Primary, .92f) : Theme.Surface;
                Color text = !Enabled ? Theme.BorderStrong : selected ? Theme.TextOnColor : Theme.TextPrimary;
                var bounds = new Rectangle(1, 1, Width - 3, Height - 3);
                using (var path = Theme.RoundedPath(bounds, CornerRadius))
                using (var brush = new SolidBrush(fill))
                {
                    g.FillPath(brush, path);
                    if (Kind == ButtonKind.Outline || Focused)
                        using (var pen = new Pen(Theme.Primary, 1.3f)) g.DrawPath(pen, path);
                }
                TextRenderer.DrawText(g, Text, Font, ClientRectangle, text,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }

        private sealed class CalendarPanel : UserControl
        {
            private readonly DateTimePicker source;
            private readonly Action<DateTime> select;
            private readonly ComboBox months = new ComboBox();
            private readonly ComboBox years = new ComboBox();
            private readonly ModernButton previous;
            private readonly ModernButton next;
            private readonly ModernButton today;
            private readonly ModernButton[] days = new ModernButton[42];
            private DateTime displayed;
            private bool updating;
            private int firstYear;
            private int lastYear;
            private CalendarData renderedData;
            private DateTime renderedValue;
            private DateTime renderedMinimum;
            private DateTime renderedMaximum;
            private DateTime renderedToday;
            public CalendarData Data { get; set; }
            private DateTime Minimum => source.MinDate.Date > Data.Minimum ? source.MinDate.Date : Data.Minimum;
            private DateTime Maximum => source.MaxDate.Date < Data.Maximum ? source.MaxDate.Date : Data.Maximum;
            private readonly CultureInfo culture = CultureInfo.GetCultureInfo("es-CO");

            public CalendarPanel(DateTimePicker source, Action<DateTime> select)
            {
                this.source = source;
                this.select = select;
                BackColor = Theme.Surface;
                Size = new Size(350, 396);
                Padding = new Padding(16);
                var title = new Label { Text = "SELECCIONAR FECHA", Font = Theme.Small, ForeColor = Theme.TextMuted, AutoSize = true, Location = new Point(20, 16) };
                Controls.Add(title);
                previous = MakeButton("‹", new Rectangle(16, 43, 32, 36), delegate { MoveMonth(-1); });
                next = MakeButton("›", new Rectangle(302, 43, 32, 36), delegate { MoveMonth(1); });
                months.SetBounds(56, 48, 142, 30);
                years.SetBounds(206, 48, 88, 30);
                foreach (var combo in new[] { months, years })
                {
                    combo.DropDownStyle = ComboBoxStyle.DropDownList;
                    combo.FlatStyle = FlatStyle.Flat;
                    combo.Font = Theme.BodyBold;
                    combo.BackColor = Theme.SurfaceAlt;
                    combo.ForeColor = Theme.TextPrimary;
                    Controls.Add(combo);
                    combo.SelectedIndexChanged += delegate
                    {
                        if (updating || years.SelectedItem == null) return;
                        var candidate = new DateTime((int)years.SelectedItem, months.SelectedIndex + 1, 1);
                        var minMonth = new DateTime(Minimum.Year, Minimum.Month, 1);
                        var maxMonth = new DateTime(Maximum.Year, Maximum.Month, 1);
                        displayed = candidate < minMonth ? minMonth : candidate > maxMonth ? maxMonth : candidate;
                        RenderDays();
                    };
                }
                months.AccessibleName = "Mes";
                years.AccessibleName = "Año";
                for (int month = 1; month <= 12; month++) months.Items.Add(culture.TextInfo.ToTitleCase(culture.DateTimeFormat.GetMonthName(month)));
                string[] headings = { "LU", "MA", "MI", "JU", "VI", "SÁ", "DO" };
                for (int i = 0; i < 7; i++)
                    Controls.Add(new Label { Text = headings[i], Font = Theme.Small, ForeColor = Theme.TextMuted,
                        TextAlign = ContentAlignment.MiddleCenter, Bounds = new Rectangle(20 + i * 44, 92, 44, 26) });
                for (int i = 0; i < days.Length; i++)
                {
                    var day = MakeButton("", new Rectangle(22 + i % 7 * 44, 124 + i / 7 * 36, 40, 33), null);
                    day.CornerRadius = 11;
                    day.Shadow = false;
                    day.Click += delegate { if (day.Tag is DateTime) select((DateTime)day.Tag); };
                    days[i] = day;
                }
                today = MakeButton("Hoy", new Rectangle(20, 350, 310, 34), delegate { select(DateTime.Today); });
                today.Kind = ButtonKind.Outline;
                today.AccessibleName = "Seleccionar hoy";
            }

            private ModernButton MakeButton(string text, Rectangle bounds, EventHandler click)
            {
                var button = new CalendarButton { Text = text, Bounds = bounds, Kind = ButtonKind.Neutral, BackColor = Theme.Surface, Font = Theme.BodyBold, CornerRadius = 10 };
                // Los días sin seleccionar se pintan como botones ligeros sobre la superficie.
                button.Kind = ButtonKind.Outline;
                if (click != null) button.Click += click;
                Controls.Add(button);
                return button;
            }

            public void RefreshMonth()
            {
                var selected = source.Value.Date < Minimum ? Minimum : source.Value.Date > Maximum ? Maximum : source.Value.Date;
                var selectedMonth = new DateTime(selected.Year, selected.Month, 1);
                if (renderedData == Data && renderedValue == source.Value.Date
                    && renderedMinimum == Minimum && renderedMaximum == Maximum
                    && renderedToday == DateTime.Today && displayed == selectedMonth) return;
                displayed = selectedMonth;
                SuspendLayout();
                updating = true;
                try
                {
                    // Ambos selectores reutilizan los años del mismo rango precargado.
                    int minYear = Data.Minimum.Year;
                    int maxYear = Data.Maximum.Year;
                    if (firstYear != minYear || lastYear != maxYear)
                    {
                        var items = Data.Years;
                        years.BeginUpdate();
                        try
                        {
                            years.Items.Clear();
                            years.Items.AddRange(items);
                            firstYear = minYear;
                            lastYear = maxYear;
                        }
                        finally { years.EndUpdate(); }
                    }
                    today.Enabled = DateTime.Today >= Minimum && DateTime.Today <= Maximum;
                }
                finally
                {
                    updating = false;
                    ResumeLayout(false);
                }
                RenderDays();
                renderedData = Data;
                renderedValue = source.Value.Date;
                renderedMinimum = Minimum;
                renderedMaximum = Maximum;
                renderedToday = DateTime.Today;
            }

            private void MoveMonth(int delta)
            {
                if (delta < 0 && displayed.Year == 1 && displayed.Month == 1 || delta > 0 && displayed.Year == 9999 && displayed.Month == 12) return;
                var candidate = displayed.AddMonths(delta);
                if (candidate > Maximum || candidate.AddDays(DateTime.DaysInMonth(candidate.Year, candidate.Month) - 1) < Minimum) return;
                displayed = candidate;
                RenderDays();
            }

            private void RenderDays()
            {
                updating = true;
                months.SelectedIndex = displayed.Month - 1;
                years.SelectedIndex = displayed.Year - firstYear;
                updating = false;
                previous.Enabled = displayed > new DateTime(Minimum.Year, Minimum.Month, 1);
                next.Enabled = displayed < new DateTime(Maximum.Year, Maximum.Month, 1);
                int offset = ((int)displayed.DayOfWeek + 6) % 7;
                for (int i = 0; i < days.Length; i++)
                {
                    int number = i - offset + 1;
                    var button = days[i];
                    bool valid = number >= 1 && number <= DateTime.DaysInMonth(displayed.Year, displayed.Month);
                    button.Visible = valid;
                    if (!valid) { button.Tag = null; continue; }
                    var date = displayed.AddDays(number - 1);
                    button.Tag = date;
                    button.Text = number.ToString(culture);
                    string description;
                    bool inWindow = Data.DayDescriptions.TryGetValue(date, out description);
                    button.AccessibleName = inWindow ? description : number.ToString(culture);
                    button.Enabled = inWindow && date >= Minimum && date <= Maximum;
                    button.Kind = date == source.Value.Date ? ButtonKind.Primary : date == DateTime.Today ? ButtonKind.Outline : ButtonKind.Neutral;
                    button.TabStop = button.Enabled;
                }
                Invalidate();
            }

            public void FocusSelected()
            {
                foreach (var day in days)
                    if (day.Tag is DateTime && (DateTime)day.Tag == source.Value.Date && day.Enabled) { day.Focus(); return; }
                months.Focus();
            }

            protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
            {
                var focused = ActiveControl as ModernButton;
                if (focused != null && focused.Tag is DateTime)
                {
                    int delta = keyData == Keys.Left ? -1 : keyData == Keys.Right ? 1 : keyData == Keys.Up ? -7 : keyData == Keys.Down ? 7 : 0;
                    if (delta != 0)
                    {
                        var current = (DateTime)focused.Tag;
                        if (current.Ticks + delta * TimeSpan.TicksPerDay < Minimum.Ticks || current.Ticks + delta * TimeSpan.TicksPerDay > Maximum.Ticks) return true;
                        var target = current.AddDays(delta);
                        displayed = new DateTime(target.Year, target.Month, 1);
                        RenderDays();
                        foreach (var day in days) if (day.Tag is DateTime && (DateTime)day.Tag == target) { day.Focus(); break; }
                        return true;
                    }
                }
                if (keyData == Keys.PageUp || keyData == Keys.PageDown) { MoveMonth(keyData == Keys.PageUp ? -1 : 1); return true; }
                return base.ProcessCmdKey(ref msg, keyData);
            }
        }
    }
}
