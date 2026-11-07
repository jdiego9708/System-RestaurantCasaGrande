using CapaPresentacion.UI;
using FontAwesome.Sharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace CapaPresentacion.Formularios.FormsPedido
{
    // Aviso previo a guardar la venta: hay productos sin vínculo con Alegra. El usuario decide qué hacer.
    //   Yes    = enviar a Alegra solo los productos vinculados (solo si hay al menos uno vinculado)
    //   No     = no enviar esta venta a Alegra; se factura solo en local
    //   Cancel = volver a la venta sin guardar nada
    public sealed class FrmUnlinkedProductsDialog : Form
    {
        public FrmUnlinkedProductsDialog(IList<string> unlinked, int linkedCount, string invoiceTotalText, string saleTotalText)
        {
            bool anyLinked = linkedCount > 0;
            Text = "Productos sin vincular con Alegra";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(840, 520);
            BackColor = Theme.Background;
            Font = Theme.Body;
            Padding = new Padding(Theme.Gap * 2);

            Label title = new Label
            {
                Text = anyLinked ? "Hay productos que no están en Alegra" : "Ningún producto de esta venta está en Alegra",
                Font = Theme.Heading,
                ForeColor = anyLinked ? Theme.TextPrimary : Theme.Danger,
                Dock = DockStyle.Top,
                Height = 34,
            };

            Label explanation = new Label
            {
                Dock = DockStyle.Top,
                Height = 118,
                ForeColor = Theme.TextMuted,
                Text = anyLinked
                    ? "Vinculados: " + linkedCount + "   ·   Sin vincular: " + unlinked.Count + Environment.NewLine
                        + "Puede enviar a Alegra solo los productos vinculados: la factura electrónica será por "
                        + invoiceTotalText + " (la venta es de " + saleTotalText + "). La factura local queda completa, "
                        + "sin cambios. Consulte con su contador si esa factura parcial es válida para esta venta."
                    : "NO SE ENVIARÁ A ALEGRA. SE FACTURARÁ SOLO EN LOCAL." + Environment.NewLine
                        + "Vinculados: 0   ·   Sin vincular: " + unlinked.Count + Environment.NewLine
                        + "Vincule los productos en la configuración de Alegra si quiere facturarlos electrónicamente.",
            };

            ListBox list = new ListBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                IntegralHeight = false,
                SelectionMode = SelectionMode.None,
                BackColor = Theme.Surface,
            };
            foreach (string name in unlinked) list.Items.Add(name);

            Label listCaption = new Label
            {
                Text = "Sin vincular (" + unlinked.Count + "):",
                Font = Theme.BodyBold,
                Dock = DockStyle.Top,
                Height = 26,
            };

            // Volver a la izquierda; a la derecha «No enviar a Alegra» y, a su lado, el botón dinámico.
            Panel buttons = new Panel { Dock = DockStyle.Bottom, Height = 64, Padding = new Padding(0, Theme.Gap, 0, 0) };

            ModernButton back = NewButton("Volver", ButtonKind.Outline, IconChar.ArrowLeft, 140, DialogResult.Cancel);
            back.Dock = DockStyle.Left;

            FlowLayoutPanel right = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
            };
            // Se añade primero el dinámico: con RightToLeft queda en el extremo derecho.
            ModernButton dynamic = anyLinked
                ? NewButton("Enviar a Alegra parcial", ButtonKind.Success, IconChar.PaperPlane, 250, DialogResult.Yes)
                // Sin nada vinculado no hay qué enviar: la continuación factura solo en local.
                : NewButton("Continuar: no se envía a Alegra, solo local", ButtonKind.Success, IconChar.Receipt, 380, DialogResult.No);
            right.Controls.Add(dynamic);
            right.Controls.Add(NewButton("No enviar a Alegra", ButtonKind.Neutral, IconChar.Ban, 210, DialogResult.No));

            buttons.Controls.Add(right);
            buttons.Controls.Add(back);

            Panel listHost = new Panel { Dock = DockStyle.Fill };
            listHost.Controls.Add(list);
            listHost.Controls.Add(listCaption);

            Controls.Add(listHost);
            Controls.Add(buttons);
            Controls.Add(explanation);
            Controls.Add(title);
            CancelButton = back;
        }

        private ModernButton NewButton(string text, ButtonKind kind, IconChar icon, int width, DialogResult result)
        {
            ModernButton button = new ModernButton { Text = text, Kind = kind, Icon = icon, Width = width, Height = 48 };
            button.Click += (s, e) => { DialogResult = result; Close(); };
            return button;
        }
    }
}
