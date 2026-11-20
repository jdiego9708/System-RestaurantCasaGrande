using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CapaPresentacion.UI;

namespace CapaPresentacion.Formularios.FormsVentas
{
    // Los indicadores incluyen todos los resultados filtrados de la consulta, antes de paginar.
    internal sealed class SalesMetrics
    {
        public int Count, DiscountCount;
        public decimal Total, Subtotal, Discount, Tips;
        public readonly SortedDictionary<string, decimal> ByType = new SortedDictionary<string, decimal>();
        public readonly int[] HourCounts = new int[24];
        public readonly decimal[] HourTotals = new decimal[24];
        public decimal Average => Count == 0 ? 0 : Total / Count;
        public decimal DiscountShare => Count == 0 ? 0 : 100m * DiscountCount / Count;
        public decimal EffectiveDiscount => Subtotal <= 0 ? 0 : 100m * Discount / Subtotal;

        public void Add(decimal total, decimal subtotal, decimal discount, decimal tips, string type, int hour)
        {
            Count++;
            Total += total;
            Subtotal += subtotal;
            Tips += tips;
            if (discount > 0)
            {
                DiscountCount++;
                Discount += discount <= 100 ? subtotal * discount / 100m : discount;
            }
            type = string.IsNullOrWhiteSpace(type) ? "Sin tipo" : type.Trim().ToUpperInvariant();
            if (!ByType.ContainsKey(type)) ByType[type] = 0;
            ByType[type] += total;
            if (hour >= 0 && hour < 24)
            {
                HourCounts[hour]++;
                HourTotals[hour] += total;
            }
        }
    }

    internal sealed class SalesDashboard : TableLayoutPanel
    {
        private const int KpiCount = 9;
        private const int WideColumns = 10;
        private const int KpiRowHeight = 136;
        private const int ChartsHeight = 164;
        private static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("es-CO");
        private readonly Label[] values = new Label[KpiCount];
        private readonly Label[] details = new Label[KpiCount];
        private readonly MiniSalesChart types = new MiniSalesChart();
        private readonly MiniSalesChart discounts = new MiniSalesChart();
        private readonly MiniSalesChart hours = new MiniSalesChart { Hourly = true };
        private readonly TableLayoutPanel charts = new TableLayoutPanel();
        private readonly ModernButton chartsButton = new ModernButton();
        public ModernButton ChartsButton => chartsButton;
        private readonly Label rappiCash = new SalesMetricBadge { Caption = "Efectivo", Tint = Theme.Success };
        private readonly Label rappiOther = new SalesMetricBadge { Caption = "Otros medios", Tint = Theme.Info };
        private bool chartsExpanded;
        private readonly ModernCard[] kpiCards = new ModernCard[KpiCount];
        private const int RappiCard = 7;
        private const int DeliveryCard = 8;
        public event EventHandler IncomeRequested;
        public event EventHandler ExpensesRequested;
        public event EventHandler SalesRequested;
        public event EventHandler RappiRequested;
        public event EventHandler DeliveriesRequested;

        // Ventas, ingresos, egresos, Rappi y domicilios comparten el listado: solo una vista queda resaltada.
        public void SelectCash(bool income, bool expenses)
        {
            kpiCards[2].AccentColor = income ? Theme.Primary : Color.Empty;
            kpiCards[3].AccentColor = expenses ? Theme.Primary : Color.Empty;
            kpiCards[RappiCard].AccentColor = Color.Empty;
            kpiCards[DeliveryCard].AccentColor = Color.Empty;
            kpiCards[0].AccentColor = income || expenses ? Color.Empty : Theme.Primary;
            kpiCards[1].AccentColor = income || expenses ? Color.Empty : Theme.Primary;
        }

        public void SelectRappi()
        {
            kpiCards[1].AccentColor = Color.Empty;
            kpiCards[0].AccentColor = Color.Empty;
            kpiCards[DeliveryCard].AccentColor = Color.Empty;
            kpiCards[2].AccentColor = Color.Empty;
            kpiCards[3].AccentColor = Color.Empty;
            kpiCards[RappiCard].AccentColor = Theme.Primary;
        }

        public void SelectDeliveries()
        {
            SelectRappi();
            kpiCards[RappiCard].AccentColor = Color.Empty;
            kpiCards[DeliveryCard].AccentColor = Theme.Primary;
        }

        public void UpdateDeliveries(int? count, decimal? total)
        {
            values[DeliveryCard].Text = total.HasValue ? total.Value.ToString("C0", Culture) : "—";
            details[DeliveryCard].Text = count.HasValue
                ? count.Value.ToString("N0", Culture) + (count.Value == 1 ? " domicilio" : " domicilios")
                : "Sin datos de domicilios";
        }

        /// <summary>Ventas de Rappi del periodo: cantidad y valor total en el mismo indicador. Null = sin datos.</summary>
        public void UpdateRappi(int? count, decimal? total, decimal? cash = null, decimal? other = null)
        {
            values[RappiCard].Text = total.HasValue ? total.Value.ToString("C0", Culture) : "—";
            details[RappiCard].Text = count.HasValue
                ? count.Value.ToString("N0", Culture)
                : "Sin datos de Rappi";
            rappiCash.Text = cash.HasValue ? cash.Value.ToString("C0", Culture) : "\u2014";
            rappiOther.Text = other.HasValue ? other.Value.ToString("C0", Culture) : "\u2014";
        }

        private static void WireKpiClick(Control control, EventHandler handler)
        {
            control.Cursor = Cursors.Hand;
            control.Click += handler;
            foreach (Control child in control.Controls) WireKpiClick(child, handler);
        }

        public SalesDashboard()
        {
            Dock = DockStyle.Top;
            ColumnCount = WideColumns;
            RowCount = 3;
            Margin = Padding.Empty;
            for (int i = 0; i < WideColumns; i++) ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / WideColumns));
            RowStyles.Add(new RowStyle(SizeType.Absolute, KpiRowHeight));
            RowStyles.Add(new RowStyle(SizeType.Absolute, 0));
            RowStyles.Add(new RowStyle(SizeType.Absolute, 0));
            string[] titles = { "VENTAS", "TOTAL VENDIDO", "TOTAL INGRESOS", "TOTAL EGRESOS",
                "TICKET PROMEDIO", "TOTAL DE DESCUENTOS", "PROPINAS", "RAPPI", "DOMICILIOS" };
            for (int i = 0; i < titles.Length; i++)
            {
                Color[] accents = { Theme.Primary, Theme.Success, Theme.Info, Theme.Danger,
                    Theme.Primary, Theme.Warning, Theme.Success, Theme.Rappi, Theme.Info };
                ModernCard card = new ModernCard
                {
                    Dock = DockStyle.Fill, Padding = new Padding(12, 10, 12, 10),
                    Margin = new Padding(2),
                };
                TableLayoutPanel content = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill, ColumnCount = 1, RowCount = i == RappiCard ? 2 : 3,
                    Margin = Padding.Empty, Padding = Padding.Empty, BackColor = Color.Transparent,
                };
                content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                content.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
                content.RowStyles.Add(new RowStyle(SizeType.Percent, i == RappiCard ? 100 : 48));
                if (i != RappiCard) content.RowStyles.Add(new RowStyle(SizeType.Percent, 52));
                Label title = new Label
                {
                    Text = titles[i] == "TOTAL DE DESCUENTOS" ? "TOTAL DE\nDESCUENTOS"
                        : titles[i] == "TICKET PROMEDIO" ? "TICKET\nPROMEDIO" : titles[i],
                    Font = Theme.SmallBold, ForeColor = Theme.TextMuted,
                    Dock = DockStyle.Fill, Margin = Padding.Empty, BackColor = Color.Transparent,
                    TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true,
                };
                values[i] = new SalesMetricBadge { Tint = accents[i], Font = Theme.BodyBold };
                details[i] = new SalesMetricBadge { Tint = Theme.Neutral, Font = Theme.Small, Subtle = true };
                content.Controls.Add(title, 0, 0);
                if (i == RappiCard)
                {
                    ((SalesMetricBadge)values[i]).Caption = "Total vendido";
                    ((SalesMetricBadge)details[i]).Caption = "Ventas";
                    details[i].Font = Theme.BodyBold;
                    TableLayoutPanel grid = new TableLayoutPanel
                    {
                        Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2,
                        Margin = Padding.Empty, Padding = Padding.Empty, BackColor = Color.Transparent,
                    };
                    for (int badgeColumn = 0; badgeColumn < 2; badgeColumn++) grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
                    for (int row = 0; row < 2; row++) grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
                    Label[] badges = { values[i], details[i], rappiCash, rappiOther };
                    for (int badge = 0; badge < badges.Length; badge++)
                    {
                        badges[badge].Dock = DockStyle.Fill;
                        badges[badge].Margin = new Padding(badge % 2 == 0 ? 0 : 3, badge < 2 ? 0 : 3, badge % 2 == 0 ? 3 : 0, badge < 2 ? 3 : 0);
                        grid.Controls.Add(badges[badge], badge % 2, badge / 2);
                    }
                    content.Controls.Add(grid, 0, 1);
                }
                else
                {
                    values[i].Dock = details[i].Dock = DockStyle.Fill;
                    values[i].Margin = new Padding(0, 0, 0, 3);
                    details[i].Margin = new Padding(0, 3, 0, 0);
                    content.Controls.Add(values[i], 0, 1);
                    content.Controls.Add(details[i], 0, 2);
                }
                card.Controls.Add(content);
                kpiCards[i] = card;
                if (i == 2) WireKpiClick(card, (sender, args) => IncomeRequested?.Invoke(this, EventArgs.Empty));
                if (i == 3) WireKpiClick(card, (sender, args) => ExpensesRequested?.Invoke(this, EventArgs.Empty));
                if (i == 0 || i == 1) WireKpiClick(card, (sender, args) => SalesRequested?.Invoke(this, EventArgs.Empty));
                if (i == RappiCard) WireKpiClick(card, (sender, args) => RappiRequested?.Invoke(this, EventArgs.Empty));
                if (i == DeliveryCard) WireKpiClick(card, (sender, args) => DeliveriesRequested?.Invoke(this, EventArgs.Empty));
                int column = i < RappiCard ? i : i + (i > RappiCard ? 1 : 0);
                Controls.Add(card, column, 0);
                if (i == RappiCard) SetColumnSpan(card, 2);
            }
            chartsButton.Text = "Gr\u00e1ficas";
            chartsButton.AccessibleName = "Gr\u00e1ficas";
            chartsButton.Kind = ButtonKind.Outline;
            chartsButton.Icon = FontAwesome.Sharp.IconChar.ChevronDown;
            chartsButton.Dock = DockStyle.Left;
            chartsButton.Width = 150;
            chartsButton.Shadow = false;
            chartsButton.Click += (s, e) =>
            {
                chartsExpanded = !chartsExpanded;
                ApplyChartsState();
            };
            charts.Dock = DockStyle.Fill;
            charts.ColumnCount = 3;
            charts.RowCount = 1;
            charts.Margin = Padding.Empty;
            for (int i = 0; i < 3; i++) charts.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / 3));
            charts.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(charts, 0, 2);
            SetColumnSpan(charts, WideColumns);
            AddChart("VENTAS POR TIPO", types, 0);
            AddChart("FACTURAS CON DESCUENTO", discounts, 1);
            AddChart("ACTIVIDAD POR HORA", hours, 2);
            SelectCash(false, false);
            UpdateCashTotals(null, null);
            UpdateRappi(null, null);
            UpdateDeliveries(null, null);
            ApplyChartsState();
            ArrangeKpis();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            if (kpiCards[KpiCount - 1] != null) ArrangeKpis();
        }

        private void ArrangeKpis()
        {
            int columns = ClientSize.Width >= 1250 ? WideColumns : 5;
            if (ColumnCount == columns) return;
            SuspendLayout();
            SetColumnSpan(charts, 1);
            ColumnCount = Math.Max(ColumnCount, columns);
            for (int i = 0; i < KpiCount; i++)
            {
                int slot = i <= RappiCard ? i : i + 1;
                SetCellPosition(kpiCards[i], new TableLayoutPanelCellPosition(slot % columns, slot / columns));
            }
            ColumnCount = columns;
            ColumnStyles.Clear();
            for (int i = 0; i < columns; i++) ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / columns));
            SetColumnSpan(charts, columns);
            RowStyles[1].Height = columns == WideColumns ? 0 : KpiRowHeight;
            ResumeLayout(true);
            ApplyChartsState();
        }

        private void ApplyChartsState()
        {
            SuspendLayout();
            charts.Visible = chartsExpanded;
            RowStyles[2].Height = chartsExpanded ? ChartsHeight : 0;
            Height = KpiRowHeight * (ColumnCount == WideColumns ? 1 : 2) + (chartsExpanded ? ChartsHeight : 0);
            chartsButton.Icon = chartsExpanded ? FontAwesome.Sharp.IconChar.ChevronUp : FontAwesome.Sharp.IconChar.ChevronDown;
            chartsButton.AccessibleDescription = chartsExpanded ? "Ocultar las tres gr\u00e1ficas" : "Mostrar las tres gr\u00e1ficas";
            ResumeLayout(true);
        }

        private void AddChart(string title, MiniSalesChart chart, int column)
        {
            ModernCard card = new ModernCard { Dock = DockStyle.Fill, Padding = new Padding(16, 12, 16, 10) };
            Label caption = new Label { Text = title, Dock = DockStyle.Top, Height = 23, Font = Theme.Small, ForeColor = Theme.TextMuted };
            chart.Dock = DockStyle.Fill;
            card.Controls.Add(chart);
            card.Controls.Add(caption);
            charts.Controls.Add(card, column, 0);
        }

        public void UpdateExpenseTotal(decimal total)
        {
            values[3].Text = total.ToString("C0", Culture);
            details[3].Text = "Sin n\u00f3mina \u00b7 COP";
        }

        public void UpdateIncomeTotal(decimal total)
        {
            values[2].Text = total.ToString("C0", Culture);
            details[2].Text = "Ingresos extras del per\u00edodo";
        }

        public void UpdateCashTotals(decimal? income, decimal? expenses)
        {
            values[2].Text = income.HasValue ? income.Value.ToString("C0", Culture) : "\u2014";
            values[3].Text = expenses.HasValue ? expenses.Value.ToString("C0", Culture) : "\u2014";
            details[2].Text = income.HasValue ? "Ingresos extras del per\u00edodo" : "Sin datos de ingresos";
            details[3].Text = expenses.HasValue ? "Sin n\u00f3mina \u00b7 COP" : "Sin datos de egresos";
        }

        public void UpdateMetrics(SalesMetrics metrics)
        {
            values[0].Text = metrics.Count.ToString("N0", Culture);
            values[1].Text = metrics.Total.ToString("C0", Culture);
            values[4].Text = metrics.Average.ToString("C0", Culture);
            values[5].Text = metrics.Discount.ToString("C0", Culture);
            values[6].Text = metrics.Tips.ToString("C0", Culture);
            details[0].Text = "En esta consulta";
            details[1].Text = "Suma de totales finales";
            details[4].Text = "Por factura \u00b7 COP";
            details[5].Text = metrics.EffectiveDiscount.ToString("N1", Culture) + " % del subtotal";
            details[6].Text = "Promedio " + (metrics.Count == 0 ? 0 : metrics.Tips / metrics.Count).ToString("C0", Culture);
            types.SetData(metrics.ByType.Select(item => new KeyValuePair<string, decimal>(item.Key, item.Value)), "Importe final por tipo de pedido");
            discounts.SetData(new[] {
                new KeyValuePair<string, decimal>("Con descuento", metrics.DiscountCount),
                new KeyValuePair<string, decimal>("Sin descuento", metrics.Count - metrics.DiscountCount) }, "Cantidad de facturas");
            hours.SetHours(metrics.HourCounts, metrics.HourTotals);
        }
    }

    /// <summary>Dato del historial: superficie tintada, borde sutil y valor centrado.</summary>
    internal sealed class SalesMetricBadge : Label
    {
        private Color tint = Theme.Primary;
        private string caption;
        private bool subtle;
        public Color Tint { get => tint; set { tint = value; Invalidate(); } }
        public string Caption { get => caption; set { caption = value; Invalidate(); } }
        public bool Subtle { get => subtle; set { subtle = value; Invalidate(); } }

        public SalesMetricBadge()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            AutoSize = false;
            BackColor = Color.Transparent;
            Font = Theme.BodyBold;
            Text = "\u2014";
            Margin = Padding.Empty;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (Width < 8 || Height < 8) return;
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle bounds = new Rectangle(0, 0, Width - 1, Height - 1);
            using (GraphicsPath path = Theme.RoundedPath(bounds, 8))
            using (SolidBrush fill = new SolidBrush(Theme.Lighten(tint, subtle ? 0.97f : 0.92f)))
            using (Pen border = new Pen(Theme.Lighten(tint, subtle ? 0.91f : 0.82f)))
            {
                g.FillPath(fill, path);
                g.DrawPath(border, path);
            }
            Rectangle text = new Rectangle(5, 2, Width - 10, Height - 4);
            if (!string.IsNullOrEmpty(caption))
            {
                int captionHeight = 15;
                TextRenderer.DrawText(g, caption, Theme.Small,
                    new Rectangle(text.X, text.Y, text.Width, captionHeight), Theme.TextMuted,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding
                    | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
                text.Y += captionHeight;
                text.Height -= captionHeight;
            }
            TextFormatFlags flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
                | TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis;
            flags |= subtle && string.IsNullOrEmpty(caption) ? TextFormatFlags.WordBreak : TextFormatFlags.SingleLine;
            TextRenderer.DrawText(g, Text, Font, text, subtle ? Theme.TextMuted : Theme.Darken(tint, 0.2f), flags);
        }
    }
    internal sealed class MiniSalesChart : Control
    {
        private static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("es-CO");
        private static readonly Color[] Palette = { Theme.Primary, Theme.Info, Theme.Success, Theme.Neutral, Theme.Warning };
        private readonly ToolTip tooltip = new ToolTip();
        private List<KeyValuePair<string, decimal>> data = new List<KeyValuePair<string, decimal>>();
        private int[] hourCounts = new int[24];
        private decimal[] hourTotals = new decimal[24];
        private string description;
        public bool Hourly { get; set; }

        public MiniSalesChart()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = Theme.Surface;
            Font = Theme.Small;
            ForeColor = Theme.TextMuted;
            MouseMove += (s, e) =>
            {
                if (!Hourly || Width <= 0) return;
                int hour = Math.Max(0, Math.Min(23, (e.X - 2) * 24 / Math.Max(1, Width - 4)));
                string text = FormatHour(hour) + " \u00b7 " + hourCounts[hour] + " ventas \u00b7 " + hourTotals[hour].ToString("C0", Culture);
                if (tooltip.GetToolTip(this) != text) tooltip.SetToolTip(this, text);
            };
        }

        public void SetData(IEnumerable<KeyValuePair<string, decimal>> items, string caption)
        {
            data = items.Where(item => item.Value > 0).OrderByDescending(item => item.Value).ToList();
            if (data.Count > 4)
                data = data.Take(3).Concat(new[] { new KeyValuePair<string, decimal>("Otros", data.Skip(3).Sum(item => item.Value)) }).ToList();
            description = caption;
            tooltip.SetToolTip(this, caption + Environment.NewLine + string.Join(Environment.NewLine,
                data.Select(item => item.Key + ": " + item.Value.ToString("N0", Culture))));
            AccessibleName = tooltip.GetToolTip(this);
            Invalidate();
        }

        public void SetHours(int[] counts, decimal[] totals)
        {
            hourCounts = (int[])counts.Clone();
            hourTotals = (decimal[])totals.Clone();
            AccessibleName = "Ventas por hora. " + string.Join("; ", Enumerable.Range(0, 24)
                .Where(hour => counts[hour] > 0).Select(hour => FormatHour(hour) + ": " + counts[hour] + " ventas"));
            Invalidate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) tooltip.Dispose();
            base.Dispose(disposing);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            if (Hourly) { DrawHours(g); return; }
            int diameter = Math.Min(88, Math.Min(Height - 6, Width / 3));
            if (diameter < 12) return;
            Rectangle ring = new Rectangle(2, (Height - diameter) / 2, diameter, diameter);
            decimal total = data.Sum(item => item.Value);
            using (Pen track = new Pen(Theme.Border, 11))
                g.DrawEllipse(track, Rectangle.Inflate(ring, -7, -7));
            float angle = -90;
            for (int i = 0; i < data.Count; i++)
            {
                float sweep = (float)(data[i].Value / total * 360m);
                using (Pen segment = new Pen(Palette[i % Palette.Length], 11))
                    g.DrawArc(segment, Rectangle.Inflate(ring, -7, -7), angle, Math.Max(0.01f, sweep - (data.Count > 1 ? 3 : 0)));
                angle += sweep;
            }
            TextRenderer.DrawText(g, total == 0 ? "\u2014" : "100%", Theme.BodyBold, ring, Theme.TextPrimary,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            int x = ring.Right + 16;
            if (total == 0)
            {
                TextRenderer.DrawText(g, "Sin datos en la consulta", Font, new Rectangle(x, 0, Math.Max(0, Width - x), Height), ForeColor,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
                return;
            }
            int visible = Math.Min(4, data.Count);
            int y = Math.Max(0, (Height - visible * 22 - 18) / 2);
            for (int i = 0; i < visible; i++)
            {
                // La ultima leyenda agrupa el resto para que el grafico siempre quepa.
                decimal value = i == 3 ? data.Skip(3).Sum(item => item.Value) : data[i].Value;
                string name = i == 3 && data.Count > 4 ? "Otros" : data[i].Key;
                using (SolidBrush dot = new SolidBrush(Palette[i % Palette.Length])) g.FillEllipse(dot, x, y + 7, 6, 6);
                TextRenderer.DrawText(g, name + "  " + (value / total * 100m).ToString("N1", Culture) + "%", Font,
                    new Rectangle(x + 12, y, Math.Max(0, Width - x - 12), 22), Theme.TextPrimary,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
                y += 22;
            }
            TextRenderer.DrawText(g, description, Font, new Rectangle(x, y, Math.Max(0, Width - x), 18), ForeColor, TextFormatFlags.EndEllipsis);
        }

        private static string FormatHour(int hour)
        {
            return DateTime.Today.AddHours(hour).ToString("h:mm tt", CultureInfo.InvariantCulture);
        }

        private void DrawHours(Graphics g)
        {
            int max = hourCounts.Max();
            if (max == 0)
            {
                TextRenderer.DrawText(g, "Sin horas registradas", Font, ClientRectangle, ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                return;
            }
            int plotHeight = Math.Max(1, Height - 38);
            float barWidth = (Width - 4f) / 24;
            int peak = Array.IndexOf(hourCounts, max);
            for (int hour = 0; hour < 24; hour++)
            {
                float height = plotHeight * hourCounts[hour] / (float)max;
                if (height <= 0) continue;
                using (SolidBrush brush = new SolidBrush(hour == peak ? Theme.Primary : Theme.Lighten(Theme.Primary, 0.72f)))
                    g.FillRectangle(brush, 2 + hour * barWidth, plotHeight - height, Math.Max(1, barWidth - 3), height);
            }
            for (int hour = 0; hour < 24; hour += 6)
                TextRenderer.DrawText(g, FormatHour(hour), Font, new Point((int)(hour * barWidth), plotHeight + 2), ForeColor);
            TextRenderer.DrawText(g, "Pico " + FormatHour(peak) + " \u00b7 " + max + " ventas", Font,
                new Rectangle(0, Height - 18, Width, 18), Theme.TextPrimary, TextFormatFlags.EndEllipsis);
        }
    }
}
