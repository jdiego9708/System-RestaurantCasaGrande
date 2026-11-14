using CapaPresentacion.UI;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace CapaPresentacion.Formularios.FormsPedido
{
    // Precio personalizado por producto SOLO para la factura electrónica de Alegra: no cambia el pedido, el total
    // cobrado, los pagos ni el recibo local, y tampoco el precio del producto en el catálogo de Alegra.
    // En la API cada línea de la factura lleva su propio items[].price, que es lo que se envía.
    public partial class FrmFacturarPedido
    {
        private const string EditPriceColumn = "EditPrice";
        private const string PriceProperty = "Precio";
        // Precio editado (con impuestos, como el de la columna) por Id de detalle del pedido.
        private readonly Dictionary<int, decimal> priceOverrides = new Dictionary<int, decimal>();
        private int hoverEditRow = -1;

        private void WirePriceEditing()
        {
            // Se registra después del manejador que abre los datos fiscales: si este desmarca la casilla, aquí ya se ve.
            this.opcionesPedido.ElectronicInvoice.CheckedChanged += (s, e) => this.UpdatePriceEditing();
            this.dgvPedido.CellFormatting += this.FormatPriceCell;
            this.dgvPedido.CellPainting += this.PaintEditIcon;
            this.dgvPedido.CellParsing += this.ParsePriceEdit;
            this.dgvPedido.CellEndEdit += (s, e) => this.dgvPedido.ReadOnly = true;
            this.dgvPedido.EditingControlShowing += this.PreparePriceEditor;
            this.dgvPedido.CellClick += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && this.dgvPedido.Columns[e.ColumnIndex].Name == EditPriceColumn)
                    this.BeginPriceEdit(e.RowIndex);
            };
            this.dgvPedido.CellMouseMove += (s, e) =>
            {
                int row = e.ColumnIndex >= 0 && e.RowIndex >= 0 && this.dgvPedido.Columns[e.ColumnIndex].Name == EditPriceColumn ? e.RowIndex : -1;
                this.SetEditHover(row);
            };
            this.dgvPedido.CellMouseLeave += (s, e) => this.SetEditHover(-1);
            this.dgvPedido.CellToolTipTextNeeded += (s, e) =>
            {
                if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
                DataGridViewColumn column = this.dgvPedido.Columns[e.ColumnIndex];
                if (column.Name == EditPriceColumn) e.ToolTipText = "Editar el precio solo para la factura electrónica";
                else if (column.DataPropertyName == PriceProperty && this.priceOverrides.ContainsKey(this.DetailId(e.RowIndex)))
                    e.ToolTipText = "Precio modificado solo para la factura electrónica. Original: "
                        + this.OriginalPrice(e.RowIndex).ToString("C0", CultureInfo.GetCultureInfo("es-CO"));
            };
        }

        // Columna del lápiz: sin datos, va al final de la colección para no desplazar los índices de las columnas
        // enlazadas y se muestra de primera.
        private void EnsurePriceEditColumn()
        {
            if (this.dgvPedido.Columns.Count == 0 || this.dgvPedido.Columns.Contains(EditPriceColumn)) return;
            var column = new DataGridViewTextBoxColumn
            {
                Name = EditPriceColumn, HeaderText = string.Empty, ReadOnly = true, Width = 46,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None, SortMode = DataGridViewColumnSortMode.NotSortable,
                Resizable = DataGridViewTriState.False, MinimumWidth = 46,
            };
            column.DefaultCellStyle.Padding = Padding.Empty;
            this.dgvPedido.Columns.Add(column);
            column.DisplayIndex = 0;
            this.UpdatePriceEditing();
        }

        // Quitar la columna antes de reenlazar los datos: los textos de cabecera se asignan por posición.
        private void RemovePriceEditColumn()
        {
            if (this.dgvPedido.Columns.Contains(EditPriceColumn)) this.dgvPedido.Columns.Remove(EditPriceColumn);
        }

        // El lápiz solo existe con «Solicitar factura electrónica» marcada; al desmarcarla se descartan los precios editados.
        private void UpdatePriceEditing()
        {
            bool enabled = this.opcionesPedido.ElectronicInvoice.Checked && !this.IsPrecuenta;
            if (!enabled && this.priceOverrides.Count > 0)
            {
                this.priceOverrides.Clear();
                this.dgvPedido.Invalidate();
            }
            if (this.dgvPedido.Columns.Contains(EditPriceColumn))
                this.dgvPedido.Columns[EditPriceColumn].Visible = enabled;
        }

        private int DetailId(int rowIndex)
        {
            var view = this.dgvPedido.Rows[rowIndex].DataBoundItem as DataRowView;
            if (view == null) return -1;
            string idColumn = view.Row.Table.Columns.Contains("Id_detalle_pedido") ? "Id_detalle_pedido" : view.Row.Table.Columns[0].ColumnName;
            return Convert.ToInt32(view[idColumn], CultureInfo.InvariantCulture);
        }

        private decimal OriginalPrice(int rowIndex)
        {
            var view = (DataRowView)this.dgvPedido.Rows[rowIndex].DataBoundItem;
            return Convert.ToDecimal(view[PriceProperty], CultureInfo.InvariantCulture);
        }

        private void BeginPriceEdit(int rowIndex)
        {
            DataGridViewColumn price = this.dgvPedido.Columns.Cast<DataGridViewColumn>().FirstOrDefault(c => c.DataPropertyName == PriceProperty);
            if (price == null) return;
            // Solo la celda Precio se puede editar, y solo mientras dura la edición.
            this.dgvPedido.ReadOnly = false;
            foreach (DataGridViewColumn column in this.dgvPedido.Columns) column.ReadOnly = column != price;
            this.dgvPedido.CurrentCell = this.dgvPedido.Rows[rowIndex].Cells[price.Index];
            this.dgvPedido.BeginEdit(true);
        }

        private void PreparePriceEditor(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            var box = e.Control as TextBox;
            if (box == null) return;
            int row = this.dgvPedido.CurrentCell.RowIndex;
            decimal shown = this.priceOverrides.TryGetValue(this.DetailId(row), out decimal custom) ? custom : this.OriginalPrice(row);
            box.Text = decimal.Truncate(shown).ToString(CultureInfo.InvariantCulture);
            box.SelectAll();
            // Solo dígitos (COP sin decimales).
            box.KeyPress -= this.DigitsOnly;
            box.KeyPress += this.DigitsOnly;
        }

        private void DigitsOnly(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true;
        }

        // Enter guarda el valor: se anota el precio de la factura y la celda conserva su valor real del pedido.
        private void ParsePriceEdit(object sender, DataGridViewCellParsingEventArgs e)
        {
            if (this.dgvPedido.Columns[e.ColumnIndex].DataPropertyName != PriceProperty) return;
            var view = (DataRowView)this.dgvPedido.Rows[e.RowIndex].DataBoundItem;
            string digits = new string((Convert.ToString(e.Value) ?? string.Empty).Where(char.IsDigit).ToArray());
            if (digits.Length > 0 && decimal.TryParse(digits, NumberStyles.None, CultureInfo.InvariantCulture, out decimal price) && price > 0)
            {
                int id = this.DetailId(e.RowIndex);
                if (price == this.OriginalPrice(e.RowIndex)) this.priceOverrides.Remove(id);
                else this.priceOverrides[id] = price;
            }
            e.Value = view[PriceProperty];
            e.ParsingApplied = true;
            this.dgvPedido.InvalidateRow(e.RowIndex);
        }

        // Fila con precio modificado: rojo pastel; la celda Precio muestra el valor editado.
        private void FormatPriceCell(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || this.priceOverrides.Count == 0) return;
            if (!this.priceOverrides.TryGetValue(this.DetailId(e.RowIndex), out decimal custom)) return;
            if (e.ColumnIndex >= 0 && this.dgvPedido.Columns[e.ColumnIndex].DataPropertyName == PriceProperty)
            {
                e.Value = custom;
                e.CellStyle.Font = Theme.BodyBold;
            }
            e.CellStyle.BackColor = Theme.Lighten(Theme.Danger, 0.85f);
            e.CellStyle.SelectionBackColor = Theme.Lighten(Theme.Danger, 0.72f);
            e.CellStyle.ForeColor = Theme.TextPrimary;
            e.CellStyle.SelectionForeColor = Theme.TextPrimary;
        }

        private void SetEditHover(int row)
        {
            if (row == this.hoverEditRow) return;
            int previous = this.hoverEditRow;
            this.hoverEditRow = row;
            DataGridViewColumn column = this.dgvPedido.Columns.Contains(EditPriceColumn) ? this.dgvPedido.Columns[EditPriceColumn] : null;
            if (column == null) return;
            this.dgvPedido.Cursor = row >= 0 ? Cursors.Hand : Cursors.Default;
            if (previous >= 0 && previous < this.dgvPedido.RowCount) this.dgvPedido.InvalidateCell(column.Index, previous);
            if (row >= 0 && row < this.dgvPedido.RowCount) this.dgvPedido.InvalidateCell(column.Index, row);
        }

        private void PaintEditIcon(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0 || this.dgvPedido.Columns[e.ColumnIndex].Name != EditPriceColumn) return;
            e.Paint(e.ClipBounds, DataGridViewPaintParts.Background | DataGridViewPaintParts.SelectionBackground | DataGridViewPaintParts.Border);
            bool hovered = e.RowIndex == this.hoverEditRow;
            bool edited = this.priceOverrides.ContainsKey(this.DetailId(e.RowIndex));
            Rectangle button = new Rectangle(e.CellBounds.Left + (e.CellBounds.Width - 28) / 2,
                e.CellBounds.Top + (e.CellBounds.Height - 28) / 2, 28, 28);
            if (hovered)
            {
                var state = e.Graphics.Save();
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var fill = new SolidBrush(Theme.Lighten(Theme.Primary, 0.9f))) e.Graphics.FillEllipse(fill, button);
                e.Graphics.Restore(state);
            }
            Color color = hovered ? Theme.Primary : edited ? Theme.Danger : Theme.TextMuted;
            IconRenderer.Draw(this.dgvPedido, e.Graphics, FontAwesome.Sharp.IconChar.Pencil, color,
                new Rectangle(button.X + 7, button.Y + 7, 14, 14));
            e.Handled = true;
        }

        // Precios editados para el snapshot de la factura electrónica (por Id de detalle del pedido).
        private Dictionary<int, decimal> EditedPrices() => new Dictionary<int, decimal>(this.priceOverrides);
    }
}
