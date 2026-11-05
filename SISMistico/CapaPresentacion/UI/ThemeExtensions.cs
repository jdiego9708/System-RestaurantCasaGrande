using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CapaPresentacion.UI
{
    /// <summary>Tipos de etiqueta del tema.</summary>
    public enum LabelKind
    {
        Title,
        Heading,
        Body,
        BodyBold,
        Muted,
        Caption,
    }

    /// <summary>
    /// Metodos de extension para dar el aspecto moderno a formularios y controles ya existentes (incluidos
    /// los del Designer). Para formularios nuevos conviene usar directamente ModernCard, ModernButton y KpiCard.
    /// </summary>
    public static class ThemeExtensions
    {
        /// <summary>
        /// Aplica el tema a un formulario completo: fondo y fuente base, y recorre sus controles para dejar
        /// botones, etiquetas, cajas de texto y tablas con el estilo del tema. No toca los controles modernos.
        /// </summary>
        public static void ApplyTheme(this Form form)
        {
            form.BackColor = Theme.Background;
            form.Font = Theme.Body;
            StyleChildren(form);
        }

        private static void StyleChildren(Control parent)
        {
            foreach (Control child in parent.Controls)
            {
                if (child is ModernButton || child is ModernCard || child is KpiCard)
                {
                    // Los modernos ya traen su estilo; solo se baja a una tarjeta para estilizar su contenido.
                    if (child is ModernCard)
                        StyleChildren(child);
                    continue;
                }

                if (child is DataGridView grid)
                    grid.StyleGrid();
                else if (child is Button button)
                    button.StyleButton(ButtonKind.Primary);
                else if (child is TextBox textBox)
                {
                    textBox.BorderStyle = BorderStyle.FixedSingle;
                    textBox.Font = Theme.Body;
                    textBox.ForeColor = Theme.TextPrimary;
                }
                else if (child is Label label && !(child.Parent is ModernCard))
                    label.StyleLabel(LabelKind.Body);

                StyleChildren(child);
            }
        }

        /// <summary>Estilo plano de color para un Button comun (sin esquinas redondeadas ni icono).</summary>
        public static void StyleButton(this Button button, ButtonKind kind)
        {
            Color fill;
            switch (kind)
            {
                case ButtonKind.Success: fill = Theme.Success; break;
                case ButtonKind.Danger: fill = Theme.Danger; break;
                case ButtonKind.Warning: fill = Theme.Warning; break;
                case ButtonKind.Neutral: fill = Theme.Neutral; break;
                case ButtonKind.Outline: fill = Theme.Surface; break;
                default: fill = Theme.Primary; break;
            }

            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = kind == ButtonKind.Outline ? 1 : 0;
            button.FlatAppearance.BorderColor = Theme.Primary;
            button.FlatAppearance.MouseOverBackColor = kind == ButtonKind.Outline
                ? Theme.SurfaceAlt : Theme.Darken(fill, 0.10f);
            button.FlatAppearance.MouseDownBackColor = kind == ButtonKind.Outline
                ? Theme.Border : Theme.Darken(fill, 0.22f);
            button.BackColor = fill;
            button.ForeColor = kind == ButtonKind.Outline ? Theme.Primary : Theme.TextOnColor;
            button.Font = Theme.Button;
            button.Cursor = Cursors.Hand;
            button.UseVisualStyleBackColor = false;
        }

        public static void StyleLabel(this Label label, LabelKind kind)
        {
            switch (kind)
            {
                case LabelKind.Title:
                    label.Font = Theme.Title;
                    label.ForeColor = Theme.TextPrimary;
                    break;
                case LabelKind.Heading:
                    label.Font = Theme.Heading;
                    label.ForeColor = Theme.TextPrimary;
                    break;
                case LabelKind.BodyBold:
                    label.Font = Theme.BodyBold;
                    label.ForeColor = Theme.TextPrimary;
                    break;
                case LabelKind.Muted:
                    label.Font = Theme.Body;
                    label.ForeColor = Theme.TextMuted;
                    break;
                case LabelKind.Caption:
                    label.Font = Theme.Small;
                    label.ForeColor = Theme.TextMuted;
                    break;
                default:
                    label.Font = Theme.Body;
                    label.ForeColor = Theme.TextPrimary;
                    break;
            }
        }

        /// <summary>Tabla limpia: sin cuadricula vertical, cabecera de color, filas alternas y seleccion suave.</summary>
        public static void StyleGrid(this DataGridView grid)
        {
            grid.BorderStyle = BorderStyle.None;
            grid.BackgroundColor = Theme.Surface;
            grid.GridColor = Theme.Border;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.EnableHeadersVisualStyles = false;
            grid.RowHeadersVisible = false;
            grid.AllowUserToResizeRows = false;
            grid.RowTemplate.Height = 32;

            grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Theme.Primary;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Theme.TextOnColor;
            grid.ColumnHeadersDefaultCellStyle.Font = Theme.BodyBold;
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Theme.Primary;
            grid.ColumnHeadersHeight = 36;

            grid.DefaultCellStyle.Font = Theme.Body;
            grid.DefaultCellStyle.ForeColor = Theme.TextPrimary;
            grid.DefaultCellStyle.BackColor = Theme.Surface;
            grid.DefaultCellStyle.SelectionBackColor = Theme.Lighten(Theme.Primary, 0.85f);
            grid.DefaultCellStyle.SelectionForeColor = Theme.TextPrimary;
            grid.AlternatingRowsDefaultCellStyle.BackColor = Theme.SurfaceAlt;
        }

        /// <summary>
        /// Tabla de solo lectura estilo "lista": cabecera clara en mayusculas, filas altas sin cuadricula,
        /// seleccion suave y sin borde de foco. Va despues de <c>StyleGrid</c>, que deja la base.
        /// </summary>
        public static void StyleModernGrid(this DataGridView grid)
        {
            grid.StyleGrid();
            grid.ReadOnly = true;
            grid.AllowUserToAddRows = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.RowTemplate.Height = 40;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.ColumnHeadersHeight = 38;

            DataGridViewCellStyle header = grid.ColumnHeadersDefaultCellStyle;
            header.BackColor = Theme.SurfaceAlt;
            header.ForeColor = Theme.TextMuted;
            header.Font = Theme.BodyBold;
            header.SelectionBackColor = Theme.SurfaceAlt;
            header.SelectionForeColor = Theme.TextMuted;
            header.Padding = new Padding(10, 0, 10, 0);
            header.WrapMode = DataGridViewTriState.False;

            grid.DefaultCellStyle.Padding = new Padding(10, 0, 10, 0);
            grid.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
            grid.DefaultCellStyle.SelectionBackColor = Theme.Lighten(Theme.Primary, 0.9f);
            grid.DefaultCellStyle.SelectionForeColor = Theme.TextPrimary;
            grid.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle(grid.DefaultCellStyle);
            grid.RowsDefaultCellStyle = new DataGridViewCellStyle(grid.DefaultCellStyle);
            grid.AlternatingRowsDefaultCellStyle.BackColor = Theme.Surface;
        }

        /// <summary>
        /// Pinta como badge (pildora de color) las celdas de la columna cuyo encabezado es
        /// <paramref name="headerText"/>. <paramref name="pick"/> decide el color segun el texto de la celda.
        /// </summary>
        public static void UseBadgeColumn(this DataGridView grid, string headerText, Func<string, BadgeKind> pick)
        {
            grid.CellPainting += (s, e) =>
            {
                if (e.RowIndex < 0 || e.ColumnIndex < 0 || e.Value == null) return;
                if (grid.Columns[e.ColumnIndex].HeaderText != headerText) return;

                string text = Convert.ToString(e.Value);
                e.PaintBackground(e.ClipBounds, true);

                Size size = TextRenderer.MeasureText(text, Theme.Small, Size.Empty, TextFormatFlags.NoPadding);
                int width = Math.Min(e.CellBounds.Width - 12, size.Width + 22);
                Rectangle pill = new Rectangle(
                    e.CellBounds.X + (e.CellBounds.Width - width) / 2,
                    e.CellBounds.Y + (e.CellBounds.Height - 24) / 2, width, 24);
                Badge.Draw(e.Graphics, pill, text, pick(text), Theme.Small);
                e.Handled = true;
            };
        }

        /// <summary>
        /// Esquinas redondeadas para cualquier control (recorta su region). Sirve para botones, paneles o
        /// imagenes ya existentes; se vuelve a calcular cuando el control cambia de tamano.
        /// </summary>
        public static void Rounded(this Control control, int radius)
        {
            void Apply()
            {
                if (control.Width < 2 || control.Height < 2)
                    return;
                using (GraphicsPath path = Theme.RoundedPath(new Rectangle(0, 0, control.Width, control.Height), radius))
                {
                    control.Region?.Dispose();
                    control.Region = new Region(path);
                }
            }

            control.SizeChanged += (s, e) => Apply();
            Apply();
        }
    }
}
