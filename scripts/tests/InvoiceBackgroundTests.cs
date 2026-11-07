using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Globalization;
using System.Reflection;
using System.Windows.Forms;
using CapaPresentacion.Formularios.FormsPedido;
using CapaPresentacion.UI;

internal static class InvoiceBackgroundTests
{
    // No ejecutar el Load de facturacion ni consultar pedidos durante la prueba de dibujo.
    private sealed class InvoicePreview : FrmFacturarPedido
    {
        protected override void OnLoad(EventArgs e) { }
    }

    [STAThread]
    private static int Main(string[] args)
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        try
        {
            using (var root = new Panel { BackColor = Theme.Background })
            using (var transparent = new Panel { BackColor = Color.Transparent })
            using (var nested = new FlowLayoutPanel { BackColor = Color.Transparent })
            {
                root.Controls.Add(transparent);
                transparent.Controls.Add(nested);
                Control[] controls = {
                    new ModernCard(), new Badge { Text = "FACTURA" },
                    new ModernButton { Text = "Terminar" }, new ModernTextBox(), new KpiCard()
                };
                foreach (Control control in controls)
                {
                    using (control)
                    {
                        control.Size = new Size(180, 80);
                        nested.Controls.Add(control);
                        CheckCorner(control, Theme.Background);
                        // El primer contenedor opaco debe prevalecer sobre el fondo del formulario.
                        nested.BackColor = Theme.Surface;
                        CheckCorner(control, Theme.Surface);
                        nested.BackColor = Color.Transparent;
                        nested.Controls.Remove(control);
                    }
                }
            }

            using (var form = new InvoicePreview())
            using (var bitmap = new Bitmap(form.Width, form.Height))
            {
                var options = (DescuentosOpcionesPedido)typeof(FrmFacturarPedido).GetField("opcionesPedido", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(form);
                var optionsPanel = (Panel)typeof(FrmFacturarPedido).GetField("panelDescuentos", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(form);
                options.frmFacturarPedido = form;
                options.Total_parcial = 83000;
                options.Dock = DockStyle.Top;
                optionsPanel.Controls.Add(options);
                form.ShowInTaskbar = false;
                form.StartPosition = FormStartPosition.Manual;
                form.Location = new Point(-30000, -30000);
                form.Show();
                foreach (int finalTotal in new[] { 0, 62250, 90000, int.MaxValue })
                {
                    form.ObtenerTotales(83000, 83000, finalTotal);
                    Application.DoEvents();
                    CheckTotal(form, finalTotal);
                }
                form.ObtenerTotales(83000, 83000, 83000);
                Application.DoEvents();
                CheckTotal(form, 83000);
                Rectangle checkboxBounds = optionsPanel.RectangleToClient(options.RectangleToScreen(options.ElectronicInvoice.Bounds));
                if (!optionsPanel.ClientRectangle.Contains(checkboxBounds))
                    throw new InvalidOperationException("La casilla de factura electronica debe verse sin desplazar el panel: " + checkboxBounds + "; panel=" + optionsPanel.ClientRectangle);
                form.PerformLayout();
                form.DrawToBitmap(bitmap, new Rectangle(Point.Empty, bitmap.Size));
                bitmap.Save(Path.Combine(args[0], "facturar-fondos.png"), ImageFormat.Png);
                form.Close();
            }
            Console.WriteLine("OK: 10 comprobaciones de fondos y 5 totales visibles (cero, descuento, cargo y maximo), sin ventas ni impresion.");
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }

    private static void CheckCorner(Control control, Color expected)
    {
        using (var bitmap = new Bitmap(control.Width, control.Height))
        {
            control.DrawToBitmap(bitmap, control.ClientRectangle);
            Color actual = bitmap.GetPixel(0, 0);
            if (actual.ToArgb() != expected.ToArgb())
                throw new InvalidOperationException(control.GetType().Name + ": fondo de esquina incorrecto: " + actual);
        }
    }

    private static void CheckTotal(FrmFacturarPedido form, int expected)
    {
        var label = (Label)typeof(FrmFacturarPedido).GetField("lblTotal", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(form);
        string expectedText = expected.ToString("C0", CultureInfo.GetCultureInfo("es-CO"));
        if (label.Text != expectedText || form.Total_final != expected || Convert.ToInt32(label.Tag) != expected)
            throw new InvalidOperationException("El total visible debe coincidir con el importe final de la venta.");
        Size text = TextRenderer.MeasureText(label.Text, label.Font, Size.Empty, TextFormatFlags.SingleLine);
        if (text.Width > label.ClientSize.Width || text.Height > label.ClientSize.Height)
        {
            foreach (Control sibling in label.Parent.Controls)
                Console.WriteLine(sibling.Name + " " + sibling.Bounds + " " + sibling.Dock);
            throw new InvalidOperationException("El importe final no cabe completo en la tarjeta: " + expectedText
                + "; texto=" + text + "; etiqueta=" + label.ClientSize + "; fuente=" + label.Font.Size
                + "; dock=" + label.Dock + "; padre=" + label.Parent.Size + "; autosize=" + label.AutoSize
                + "; max=" + label.MaximumSize + "; min=" + label.MinimumSize);
        }
    }
}
