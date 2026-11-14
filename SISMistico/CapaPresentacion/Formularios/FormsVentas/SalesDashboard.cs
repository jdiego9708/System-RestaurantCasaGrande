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
        private const int KpiRowHeight = 78;
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
        private bool chartsExpanded;
        private readonly ModernCard[] kpiCards = new ModernCard[KpiCount];
        private const int RappiCard = 8;
        public event EventHandler IncomeRequested;
        public event EventHandler ExpensesRequested;
        public event EventHandler SalesRequested;
        public event EventHandler RappiRequested;

        // Ventas, ingresos, egresos y Rappi son las cuatro vistas del listado: solo una queda resaltada.
        public void SelectCash(bool income, bool expenses)
        {
            kpiCards[2].AccentColor = income ? Theme.Primary : Color.Empty;
            kpiCards[3].AccentColor = expenses ? Theme.Primary : Color.Empty;
            kpiCards[RappiCard].AccentColor = Color.Empty;
            kpiCards[1].AccentColor = income || expenses ? Color.Empty : Theme.Primary;
        }

        public void SelectRappi()
        {
            kpiCards[1].AccentColor = Color.Empty;
            kpiCards[2].AccentColor = Color.Empty;
            kpiCards[3].AccentColor = Color.Empty;
            kpiCards[RappiCard].AccentColor = Theme.Primary;
        }

        /// <summary>Ventas de Rappi del periodo: cantidad y valor total en el mismo indicador. Null = sin datos.</summary>
        public void UpdateRappi(int? count, decimal? total)
        {
            values[RappiCard].Text = total.HasValue ? total.Value.ToString("C0", Culture) : "—";
            details[RappiCard].Text = count.HasValue
                ? count.Value.ToString("N0", Culture) + (count.Value == 1 ? " venta" : " ventas") + " por Rappi"
                : "Sin datos de Rappi";
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
            ColumnCount = KpiCount;
            RowCount = 2;
            Margin = Padding.Empty;
            for (int i = 0; i < KpiCount; i++) ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / KpiCount));
            RowStyles.Add(new RowStyle(SizeType.Absolute, KpiRowHeight));
            RowStyles.Add(new RowStyle(SizeType.Absolute, 0));
            string[] titles = { "VENTAS", "TOTAL VENDIDO", "TOTAL INGRESOS", "TOTAL EGRESOS",
                "TICKET PROMEDIO", "DESCONTADO", "CON DESCUENTO", "PROPINAS", "RAPPI" };
            for (int i = 0; i < titles.Length; i++)
            {
                // Reservar el borde, la sombra y las esquinas antes de distribuir el texto.
                ModernCard card = new ModernCard
                {
                    Dock = DockStyle.Fill, Padding = new Padding(12, 10, 12, 10),
                    Margin = new Padding(2),
                };
                TableLayoutPanel content = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3,
                    Margin = Padding.Empty, Padding = Padding.Empty, BackColor = Color.Transparent,
                };
                content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                content.RowStyles.Add(new RowStyle(SizeType.Percent, 30));
                content.RowStyles.Add(new RowStyle(SizeType.Percent, 40));
                content.RowStyles.Add(new RowStyle(SizeType.Percent, 30));
                Label title = new Label { Text = titles[i], Font = Theme.Small, ForeColor = Theme.TextMuted };
                values[i] = new Label { Font = Theme.BodyBold, ForeColor = i == 1 || i == 2 ? Theme.Success : i == RappiCard ? Theme.Rappi : Theme.TextPrimary };
                details[i] = new Label { Font = Theme.Small, ForeColor = Theme.TextMuted };
                Label[] lines = { title, values[i], details[i] };
                for (int line = 0; line < lines.Length; line++)
                {
                    lines[line].Dock = DockStyle.Fill;
                    lines[line].Margin = Padding.Empty;
                    lines[line].BackColor = Color.Transparent;
                    lines[line].BorderStyle = BorderStyle.None;
                    lines[line].AutoSize = false;
                    lines[line].AutoEllipsis = true;
                    lines[line].TextAlign = ContentAlignment.MiddleLeft;
                    content.Controls.Add(lines[line], 0, line);
                }
                card.Controls.Add(content);
                kpiCards[i] = card;
                if (i == 2) WireKpiClick(card, (sender, args) => IncomeRequested?.Invoke(this, EventArgs.Empty));
                if (i == 3) WireKpiClick(card, (sender, args) => ExpensesRequested?.Invoke(this, EventArgs.Empty));
                if (i == 0 || i == 1) WireKpiClick(card, (sender, args) => SalesRequested?.Invoke(this, EventArgs.Empty));
                if (i == RappiCard) WireKpiClick(card, (sender, args) => RappiRequested?.Invoke(this, EventArgs.Empty));
                Controls.Add(card, i, 0);
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
            Controls.Add(charts, 0, 1);
            SetColumnSpan(charts, KpiCount);
            AddChart("VENTAS POR TIPO", types, 0);
            AddChart("FACTURAS CON DESCUENTO", discounts, 1);
            AddChart("ACTIVIDAD POR HORA", hours, 2);
            SelectCash(false, false);
            UpdateCashTotals(null, null);
            UpdateRappi(null, null);
            ApplyChartsState();
        }

        private void ApplyChartsState()
        {
            SuspendLayout();
            charts.Visible = chartsExpanded;
            RowStyles[1].Height = chartsExpanded ? ChartsHeight : 0;
            Height = KpiRowHeight + (chartsExpanded ? ChartsHeight : 0);
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
            details[3].Text = "Egresos del per\u00edodo (sin n\u00f3mina)";
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
            details[3].Text = expenses.HasValue ? "Egresos del per\u00edodo (sin n\u00f3mina)" : "Sin datos de egresos";
        }

        public void UpdateMetrics(SalesMetrics metrics)
        {
            values[0].Text = metrics.Count.ToString("N0", Culture);
            values[1].Text = metrics.Total.ToString("C0", Culture);
            values[4].Text = metrics.Average.ToString("C0", Culture);
            values[5].Text = metrics.Discount.ToString("C0", Culture);
            values[6].Text = metrics.DiscountShare.ToString("N1", Culture) + " %";
            values[7].Text = metrics.Tips.ToString("C0", Culture);
            details[0].Text = "En esta consulta";
            details[1].Text = "Suma de totales finales";
            details[4].Text = "Por factura \u00b7 COP";
            details[5].Text = metrics.EffectiveDiscount.ToString("N1", Culture) + " % del subtotal";
            details[6].Text = metrics.DiscountCount.ToString("N0", Culture) + " de " + metrics.Count.ToString("N0", Culture) + " facturas";
            details[7].Text = "Promedio " + (metrics.Count == 0 ? 0 : metrics.Tips / metrics.Count).ToString("C0", Culture);
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
