using CapaNegocio.Alegra;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace CapaPresentacion.UI
{
    /// <summary>Indicador del vínculo guardado; no representa el estado de emisión.</summary>
    internal static class AlegraLinkIndicator
    {
        public static void Attach(DataGridView grid, string nameColumn, string idColumn, string kind)
        {
            var linkedIds = new HashSet<string>(StringComparer.Ordinal);
            Action refresh = () =>
            {
                linkedIds.Clear();
                var products = AlegraWorkflow.Settings().Products;
                if (products != null)
                    foreach (var pair in products)
                        if (pair.Key.StartsWith(kind + ":", StringComparison.Ordinal)
                            && pair.Value != null && !string.IsNullOrWhiteSpace(pair.Value.RemoteId))
                            linkedIds.Add(pair.Key.Substring(kind.Length + 1));
                grid.Invalidate();
            };
            Func<int, bool> isLinked = row => row >= 0 && row < grid.Rows.Count
                && grid.Columns.Contains(idColumn)
                && linkedIds.Contains(Convert.ToString(grid.Rows[row].Cells[idColumn].Value,
                    System.Globalization.CultureInfo.InvariantCulture));
            grid.DataBindingComplete += (s, e) => refresh();
            grid.VisibleChanged += (s, e) => { if (grid.Visible) refresh(); };
            grid.CellFormatting += (s, e) =>
            {
                if (e.ColumnIndex < 0 || grid.Columns[e.ColumnIndex].Name != nameColumn || !isLinked(e.RowIndex)) return;
                e.CellStyle = new DataGridViewCellStyle(e.CellStyle);
                Padding padding = e.CellStyle.Padding;
                padding.Left += (int)Math.Round(14 * grid.DeviceDpi / 96.0);
                e.CellStyle.Padding = padding;
            };
            grid.CellPainting += (s, e) =>
            {
                if (e.ColumnIndex < 0 || grid.Columns[e.ColumnIndex].Name != nameColumn || !isLinked(e.RowIndex)) return;
                e.Paint(e.ClipBounds, e.PaintParts);
                int diameter = Math.Max(4, (int)Math.Round(6 * grid.DeviceDpi / 96.0));
                int inset = (int)Math.Round(5 * grid.DeviceDpi / 96.0);
                using (var brush = new SolidBrush(Color.FromArgb(75, 147, 101)))
                    e.Graphics.FillEllipse(brush, e.CellBounds.Left + inset,
                        e.CellBounds.Top + (e.CellBounds.Height - diameter) / 2, diameter, diameter);
                e.Handled = true;
            };
            grid.CellToolTipTextNeeded += (s, e) =>
            {
                if (e.ColumnIndex >= 0 && grid.Columns[e.ColumnIndex].Name == nameColumn && isLinked(e.RowIndex))
                    e.ToolTipText = "Vinculado con Alegra";
            };
        }
    }
}
