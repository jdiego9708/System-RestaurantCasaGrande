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
    // Los valores siempre proceden de las mismas filas que se muestran en la tabla.
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
        private const int KpiRowHeight = 80;
        private const int ChartsHeight = 164;
        private static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("es-CO");
        private readonly Label[] values = new Label[6];
        private readonly Label[] details = new Label[6];
        private readonly MiniSalesChart types = new MiniSalesChart();
        private readonly MiniSalesChart discounts = new MiniSalesChart();
        private readonly MiniSalesChart hours = new MiniSalesChart { Hourly = true };

        public SalesDashboard()
        {
            Dock = DockStyle.Top;
            Height = KpiRowHeight + ChartsHeight;
            ColumnCount = 6;
            RowCount = 2;
            Margin = Padding.Empty;
            RowStyles.Add(new RowStyle(SizeType.Absolute, KpiRowHeight));
            RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            string[] titles = { "VENTAS", "TOTAL VENDIDO", "TICKET PROMEDIO", "TOTAL DESCONTADO", "CON DESCUENTO", "PROPINAS" };
            for (int i = 0; i < 6; i++)
            {
                ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / 6));
                ModernCard card = new ModernCard { Dock = DockStyle.Fill, Padding = new Padding(14, 8, 14, 8) };
                Label title = new Label { Text = titles[i], Dock = DockStyle.Top, Height = 18, Font = Theme.Small, ForeColor = Theme.TextMuted };
                values[i] = new Label { Dock = DockStyle.Top, Height = 24, Font = Theme.Heading, ForeColor = i == 1 ? Theme.Success : Theme.TextPrimary, AutoEllipsis = true };
                details[i] = new Label { Dock = DockStyle.Bottom, Height = 18, Font = Theme.Small, ForeColor = Theme.TextMuted, AutoEllipsis = true };
                card.Controls.Add(values[i]);
                card.Controls.Add(title);
                card.Controls.Add(details[i]);
                Controls.Add(card, i, 0);
            }
            AddChart("VENTAS POR TIPO", types, 0);
            AddChart("FACTURAS CON DESCUENTO", discounts, 2);
            AddChart("ACTIVIDAD POR HORA", hours, 4);
            SizeChanged += (s, e) => ArrangeCards();
        }

        private void ArrangeCards()
        {
            bool compact = Width < 1080;
            int columns = compact ? 3 : 6;
            if (ColumnCount == columns) return;
            SuspendLayout();
            Control[] cards = Controls.Cast<Control>().ToArray();
            foreach (Control card in cards) SetColumnSpan(card, 1);
            ColumnCount = columns;
            RowCount = compact ? 3 : 2;
            ColumnStyles.Clear();
            RowStyles.Clear();
            for (int i = 0; i < columns; i++) ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / columns));
            RowStyles.Add(new RowStyle(SizeType.Absolute, KpiRowHeight));
            if (compact) RowStyles.Add(new RowStyle(SizeType.Absolute, KpiRowHeight));
            RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            for (int i = 0; i < 6; i++) SetCellPosition(cards[i], new TableLayoutPanelCellPosition(i % columns, i / columns));
            for (int i = 6; i < 9; i++)
            {
                SetCellPosition(cards[i], new TableLayoutPanelCellPosition((i - 6) * (compact ? 1 : 2), compact ? 2 : 1));
                SetColumnSpan(cards[i], compact ? 1 : 2);
            }
            Height = KpiRowHeight * (compact ? 2 : 1) + ChartsHeight;
            ResumeLayout(true);
        }

        private void AddChart(string title, MiniSalesChart chart, int column)
        {
            ModernCard card = new ModernCard { Dock = DockStyle.Fill, Padding = new Padding(16, 12, 16, 10) };
            Label caption = new Label { Text = title, Dock = DockStyle.Top, Height = 23, Font = Theme.Small, ForeColor = Theme.TextMuted };
            chart.Dock = DockStyle.Fill;
            card.Controls.Add(chart);
            card.Controls.Add(caption);
            Controls.Add(card, column, 1);
            SetColumnSpan(card, 2);
        }

        public void UpdateMetrics(SalesMetrics metrics)
        {
            values[0].Text = metrics.Count.ToString("N0", Culture);
            values[1].Text = metrics.Total.ToString("C0", Culture);
            values[2].Text = metrics.Average.ToString("C0", Culture);
            values[3].Text = metrics.Discount.ToString("C0", Culture);
            values[4].Text = metrics.DiscountShare.ToString("N1", Culture) + " %";
            values[5].Text = metrics.Tips.ToString("C0", Culture);
            details[0].Text = "En esta consulta";
            details[1].Text = "Suma de totales finales";
            details[2].Text = "Por factura \u00b7 COP";
            details[3].Text = metrics.EffectiveDiscount.ToString("N1", Culture) + " % del subtotal";
            details[4].Text = metrics.DiscountCount.ToString("N0", Culture) + " de " + metrics.Count.ToString("N0", Culture) + " facturas";
            details[5].Text = "Promedio " + (metrics.Count == 0 ? 0 : metrics.Tips / metrics.Count).ToString("C0", Culture);
            types.SetData(metrics.ByType.Select(item => new KeyValuePair<string, decimal>(item.Key, item.Value)), "Importe final por tipo de pedido");
            discounts.SetData(new[] {
                new KeyValuePair<string, decimal>("Con descuento", metrics.DiscountCount),
                new KeyValuePair<string, decimal>("Sin descuento", metrics.Count - metrics.DiscountCount) }, "Cantidad de facturas");
            hours.SetHours(metrics.HourCounts, metrics.HourTotals);
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
