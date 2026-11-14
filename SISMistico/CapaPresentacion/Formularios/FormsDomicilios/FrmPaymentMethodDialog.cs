using System;
using System.Drawing;
using System.Windows.Forms;
using CapaPresentacion.UI;
using FontAwesome.Sharp;

namespace CapaPresentacion.Formularios.FormsDomicilios
{
    /// <summary>
    /// Pregunta como pagara el cliente de un domicilio. Se muestra antes de guardar el pedido: con la forma de pago
    /// guardada, el domicilio puede facturarse solo al cumplirse su tiempo. Cerrar la ventana cancela.
    /// </summary>
    public sealed class FrmPaymentMethodDialog : Form
    {
        /// <summary>EFECTIVO, TRANSFERENCIA o TARJETA (los mismos nombres que usa la factura).</summary>
        public string Method { get; private set; }

        public FrmPaymentMethodDialog()
        {
            this.Text = "Forma de pago";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;
            this.BackColor = Theme.Background;
            this.Font = Theme.Body;
            this.ClientSize = new Size(520, 150);

            Label title = new Label
            {
                Text = "¿Cómo pagará el cliente este domicilio?",
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, 14),
                Size = new Size(520, 36),
            };
            title.StyleLabel(LabelKind.Heading);
            this.Controls.Add(title);

            this.AddOption("EFECTIVO", "Efectivo", IconChar.MoneyBill, ButtonKind.Success, 16);
            this.AddOption("TRANSFERENCIA", "Transferencia", IconChar.BuildingColumns, ButtonKind.Primary, 180);
            this.AddOption("TARJETA", "Tarjeta", IconChar.CreditCard, ButtonKind.Outline, 344);
        }

        private void AddOption(string method, string text, IconChar icon, ButtonKind kind, int left)
        {
            ModernButton button = new ModernButton
            {
                Text = text,
                Icon = icon,
                Kind = kind,
                Size = new Size(160, 56),
                Location = new Point(left, 72),
            };
            button.Click += (s, e) =>
            {
                this.Method = method;
                this.DialogResult = DialogResult.OK;
                this.Close();
            };
            this.Controls.Add(button);
        }
    }
}
