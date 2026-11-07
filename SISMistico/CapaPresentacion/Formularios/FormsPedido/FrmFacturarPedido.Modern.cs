using CapaPresentacion.UI;
using FontAwesome.Sharp;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace CapaPresentacion.Formularios.FormsPedido
{
    // Aspecto moderno de la ventana de facturar: se reorganizan los controles del Designer en tarjetas
    // (cabecera, detalle, totales, opciones) y se usan los badges y la tabla del tema (carpeta UI).
    public partial class FrmFacturarPedido
    {
        private Badge modeBadge;
        private ModernCard headerCard;

        private void ApplyModernLayout()
        {
            this.SuspendLayout();
            this.BackColor = Theme.Background;
            this.Font = Theme.Body;
            this.Padding = new Padding(Theme.Gap);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.Controls.Clear();

            // El boton de descuentos queda oculto: las opciones viven embebidas en la columna derecha.
            this.btnDescuentos.Visible = false;
            this.Controls.Add(this.btnDescuentos);

            Panel left = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            Panel right = new Panel
            {
                Dock = DockStyle.Right,
                Width = 660,
                BackColor = Color.Transparent,
                Padding = new Padding(Theme.Gap, 0, 0, 0),
            };
            this.Controls.Add(left);
            this.Controls.Add(right);

            BuildRightColumn(right);
            BuildLeftColumn(left);

            this.dgvPedido.StyleModernGrid();
            this.dgvPedido.UseBadgeColumn("Tipo", PickTypeBadge);
            this.ResumeLayout(true);
        }

        #region COLUMNA DERECHA

        private void BuildRightColumn(Panel right)
        {
            TableLayoutPanel actions = new TableLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 82,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(0, Theme.Gap, 0, 0),
                BackColor = Color.Transparent,
            };
            actions.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 132));
            actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            this.groupBox6.Dock = DockStyle.Fill;
            this.groupBox6.Margin = new Padding(0, 0, Theme.Gap, 0);
            this.groupBox6.BackColor = Theme.Surface;
            this.groupBox6.ForeColor = Theme.TextMuted;
            this.groupBox6.Font = Theme.BodyBold;
            this.numericCantidadFacturas.BackColor = Theme.Surface;
            this.numericCantidadFacturas.Left = 24;

            this.btnTerminar.Image = null;
            this.btnTerminar.Kind = ButtonKind.Success;
            this.btnTerminar.Icon = IconChar.Check;
            this.btnTerminar.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            this.btnTerminar.Dock = DockStyle.Fill;
            this.btnTerminar.Margin = Padding.Empty;
            this.btnTerminar.BackColor = Theme.Background;

            actions.Controls.Add(this.groupBox6, 0, 0);
            actions.Controls.Add(this.btnTerminar, 1, 0);

            ModernCard discounts = new ModernCard { Dock = DockStyle.Fill, AccentColor = Theme.Warning };
            Label title = new Label { Text = "Descuentos y opciones", Dock = DockStyle.Top, Height = 32 };
            title.StyleLabel(LabelKind.Heading);

            this.panelDescuentos.Dock = DockStyle.Fill;
            this.panelDescuentos.BackColor = Theme.Surface;
            discounts.Controls.Add(this.panelDescuentos);
            discounts.Controls.Add(title);

            right.Controls.Add(discounts);
            right.Controls.Add(actions);
        }

        #endregion

        #region COLUMNA IZQUIERDA

        private void BuildLeftColumn(Panel left)
        {
            ModernCard detail = new ModernCard { Dock = DockStyle.Fill };
            this.label4.Text = "Detalle del pedido";
            this.label4.AutoSize = false;
            this.label4.Dock = DockStyle.Top;
            this.label4.Height = 32;
            this.label4.StyleLabel(LabelKind.Heading);
            this.dgvPedido.Dock = DockStyle.Fill;
            detail.Controls.Add(this.dgvPedido);
            detail.Controls.Add(this.label4);

            this.headerCard = BuildHeaderCard();

            Panel bottom = new Panel { Dock = DockStyle.Bottom, Height = 146, BackColor = Color.Transparent };
            bottom.Controls.Add(BuildTotalsCard());
            bottom.Controls.Add(BuildOptionsCard());

            left.Controls.Add(detail);
            left.Controls.Add(this.headerCard);
            left.Controls.Add(bottom);
        }

        private ModernCard BuildHeaderCard()
        {
            ModernCard card = new ModernCard { Dock = DockStyle.Top, Height = 150, AccentColor = Theme.Primary };

            FlowLayoutPanel infoRow = NewRow(40);
            FlowLayoutPanel peopleRow = NewRow(36);

            this.modeBadge = new Badge { Width = 120, Margin = new Padding(0, 0, 0, 0) };

            AddCaption(infoRow, this.label1, "Pedido");
            AddBadge(infoRow, this.lblIdPedido, 76, BadgeKind.Primary);
            AddCaption(infoRow, this.label3, "Fecha");
            AddBadge(infoRow, this.lblFecha, 104, BadgeKind.Neutral);
            AddBadge(infoRow, this.lblMesa, 124, BadgeKind.Info);
            infoRow.Controls.Add(this.modeBadge);

            AddCaption(peopleRow, this.label6, "Cliente");
            AddLink(peopleRow, this.lblCliente);
            AddCaption(peopleRow, this.label9, "Mesero");
            AddLink(peopleRow, this.lblMesero);

            this.txtCantidadPedidosCliente.Dock = DockStyle.Bottom;
            this.txtCantidadPedidosCliente.Height = 30;
            this.txtCantidadPedidosCliente.Kind = BadgeKind.Success;
            this.txtCantidadPedidosCliente.TextAlign = ContentAlignment.MiddleLeft;

            card.Controls.Add(this.txtCantidadPedidosCliente);
            card.Controls.Add(peopleRow);
            card.Controls.Add(infoRow);
            return card;
        }

        private ModernCard BuildTotalsCard()
        {
            ModernCard card = new ModernCard { Dock = DockStyle.Fill, AccentColor = Theme.Success };

            Panel partialRow = NewTotalRow(this.label12, this.lblTotalParcial, 28, Theme.BodyBold, Theme.TextPrimary);
            Panel subRow = this.panelSubTotal;
            subRow.Controls.Clear();
            subRow.Dock = DockStyle.Top;
            subRow.Height = 28;
            subRow.BackColor = Theme.Surface;
            FillTotalRow(subRow, this.label5, this.lblSubTotal, Theme.BodyBold, Theme.TextPrimary);
            Panel totalRow = NewTotalRow(this.label11, this.lblTotal, 48,
                new Font("Segoe UI", 20F, FontStyle.Bold), Theme.Success);

            this.label12.Text = "Total parcial";
            this.label5.Text = "Subtotal";
            this.label11.Text = "TOTAL FINAL";

            card.Controls.Add(totalRow);
            card.Controls.Add(subRow);
            card.Controls.Add(partialRow);
            return card;
        }

        private ModernCard BuildOptionsCard()
        {
            ModernCard card = new ModernCard { Dock = DockStyle.Right, Width = 372, AccentColor = Theme.Info };

            this.panelTipoPedido.Dock = DockStyle.Fill;
            this.panelTipoPedido.BackColor = Theme.Surface;
            this.panelTipoPedido.Font = Theme.Body;
            this.metroTile1.Visible = false;

            this.label2.Text = "Opciones de factura";
            this.label2.StyleLabel(LabelKind.Heading);
            this.label2.Location = new Point(0, 0);

            Control[] choices = { this.rdImprimir, this.rdAmbas, this.rdNinguna, this.rdCorreo, this.chkRecordarOpcion };
            foreach (Control choice in choices)
            {
                choice.Font = Theme.Body;
                choice.ForeColor = Theme.TextPrimary;
                choice.BackColor = Theme.Surface;
            }
            this.rdImprimir.Location = new Point(0, 36);
            this.rdAmbas.Location = new Point(100, 36);
            this.rdNinguna.Location = new Point(180, 36);
            this.rdCorreo.Location = new Point(0, 68);
            this.chkRecordarOpcion.Location = new Point(190, 68);

            card.Controls.Add(this.panelTipoPedido);
            return card;
        }

        #endregion

        #region AYUDAS DE LAYOUT

        private static FlowLayoutPanel NewRow(int height)
        {
            return new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = height,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.Transparent,
            };
        }

        private static void AddCaption(FlowLayoutPanel row, Label label, string text)
        {
            label.Text = text;
            label.AutoSize = true;
            label.Margin = new Padding(0, 5, 6, 0);
            label.StyleLabel(LabelKind.Muted);
            row.Controls.Add(label);
        }

        private static void AddBadge(FlowLayoutPanel row, Badge badge, int width, BadgeKind kind)
        {
            badge.Width = width;
            badge.Height = 26;
            badge.Kind = kind;
            badge.Margin = new Padding(0, 0, 14, 0);
            row.Controls.Add(badge);
        }

        // El cliente y el mesero abren un dialogo al pulsarlos; se muestran como enlaces.
        private static void AddLink(FlowLayoutPanel row, Label label)
        {
            label.AutoSize = true;
            label.Margin = new Padding(0, 3, 24, 0);
            label.Font = new Font(Theme.BodyBold, FontStyle.Underline);
            label.ForeColor = Theme.Primary;
            row.Controls.Add(label);
        }

        private static Panel NewTotalRow(Label caption, Label value, int height, Font valueFont, Color valueColor)
        {
            Panel row = new Panel { Dock = DockStyle.Top, Height = height, BackColor = Theme.Surface };
            FillTotalRow(row, caption, value, valueFont, valueColor);
            return row;
        }

        // Titulo a la izquierda y valor alineado a la derecha: la columna de cifras queda a ras.
        private static void FillTotalRow(Panel row, Label caption, Label value, Font valueFont, Color valueColor)
        {
            value.AutoSize = false;
            value.Dock = DockStyle.None;
            value.Anchor = AnchorStyles.None;
            value.TextAlign = ContentAlignment.MiddleRight;
            value.Font = valueFont;
            value.ForeColor = valueColor;

            caption.AutoSize = false;
            caption.Dock = DockStyle.Left;
            caption.Width = 100;
            caption.TextAlign = ContentAlignment.MiddleLeft;
            caption.StyleLabel(LabelKind.Muted);

            row.Controls.Add(value);
            row.Controls.Add(caption);
            Action arrangeAmount = () => value.SetBounds(caption.Width, 0,
                Math.Max(0, row.ClientSize.Width - caption.Width), row.ClientSize.Height);
            row.SizeChanged += (s, e) => arrangeAmount();
            arrangeAmount();

            // Evitar que el simbolo monetario quede en una linea y el importe se recorte debajo.
            Font fittedFont = null;
            Action fitAmount = () =>
            {
                if (value.ClientSize.Width <= 0) return;
                Font nextFont = valueFont;
                float size = valueFont.Size;
                while (size > 8 && TextRenderer.MeasureText(value.Text, nextFont, Size.Empty,
                    TextFormatFlags.SingleLine).Width > value.ClientSize.Width)
                {
                    if (nextFont != valueFont) nextFont.Dispose();
                    size -= 0.5f;
                    nextFont = new Font(valueFont.FontFamily, size, valueFont.Style);
                }
                value.Font = nextFont;
                if (fittedFont != null) fittedFont.Dispose();
                fittedFont = nextFont == valueFont ? null : nextFont;
            };
            value.TextChanged += (s, e) => fitAmount();
            value.SizeChanged += (s, e) => fitAmount();
            value.Disposed += (s, e) => { if (fittedFont != null) fittedFont.Dispose(); };
            fitAmount();
        }

        #endregion

        #region TABLA Y BADGES

        private static BadgeKind PickTypeBadge(string type)
        {
            switch ((type ?? "").Trim().ToUpperInvariant())
            {
                case "PLATO": return BadgeKind.Primary;
                case "BEBIDA": return BadgeKind.Info;
                case "ADICION": return BadgeKind.Warning;
                default: return BadgeKind.Neutral;
            }
        }

        // Columnas del detalle: el nombre se reparte el espacio libre, las cifras van a la derecha.
        private void FormatOrderColumns()
        {
            if (this.dgvPedido.Columns.Count == 0) return;

            this.dgvPedido.Dock = DockStyle.Fill;
            this.dgvPedido.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

            foreach (DataGridViewColumn column in this.dgvPedido.Columns)
            {
                if (!column.Visible) continue;

                switch (column.HeaderText)
                {
                    case "Nombre":
                    case "Observaciones":
                        column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                        column.FillWeight = column.HeaderText == "Nombre" ? 160 : 100;
                        break;
                    case "Precio":
                    case "Total":
                        column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                        column.DefaultCellStyle.Format = "C0";
                        column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                        column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
                        if (column.HeaderText == "Total")
                            column.DefaultCellStyle.Font = Theme.BodyBold;
                        break;
                    case "Cantidad":
                        column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                        column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                        column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                        break;
                    case "Tipo":
                        column.Width = 110;
                        column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                        break;
                    default:
                        column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                        break;
                }
            }
        }

        // Etiquetas que dependen de como se abrio la ventana (se asignan despues del constructor).
        private void UpdateModeBadges()
        {
            bool delivery = this.IsDomicilio || string.Equals(this.Mesa, "DOMICILIO", StringComparison.OrdinalIgnoreCase);
            this.lblMesa.Kind = delivery ? BadgeKind.Rappi : BadgeKind.Info;

            if (this.IsPrecuenta)
            {
                this.modeBadge.Text = "PRECUENTA";
                this.modeBadge.Kind = BadgeKind.Warning;
                this.Text = "Precuenta";
            }
            else
            {
                this.modeBadge.Text = "FACTURA";
                this.modeBadge.Kind = BadgeKind.Success;
                this.Text = "Facturar pedido";
            }
        }

        #endregion
    }
}
