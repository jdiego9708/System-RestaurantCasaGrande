using CapaNegocio.Alegra;
using CapaPresentacion.UI;
using FontAwesome.Sharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CapaPresentacion.Formularios.FormsPedido
{
    // Espera bloqueante de la factura electrónica: tras guardar la venta local, esta ventana no se puede
    // cerrar hasta que Alegra confirme y la factura electrónica quede impresa/guardada (o falle de forma clara).
    public partial class FrmFacturarPedido
    {
        private const int ElectronicWaitLimitSeconds = 120;
        private const int ElectronicPollMilliseconds = 1500;

        private bool waitingForElectronic;
        private Panel electronicOverlay;
        private Label electronicTitle;
        private Label electronicDetail;
        private ProgressBar electronicProgress;
        private ModernButton electronicKeepWaiting;
        private ModernButton electronicClose;
        private TaskCompletionSource<bool> electronicDecision;

        private enum ElectronicState { Waiting, Done, Failed }

        // Decisión del usuario cuando hay productos sin vínculo: facturar en Alegra solo los vinculados.
        private bool electronicOnlyLinked;

        // Se llama antes de guardar la venta. Devuelve false si el usuario prefiere volver a la pantalla
        // (no se guarda nada); true para continuar, ya sea enviando todo, solo lo vinculado o nada a Alegra.
        private bool ResolveElectronicScope(List<string> values, DataTable payments)
        {
            this.electronicOnlyLinked = false;
            AlegraInvoiceConfiguration settings;
            string preview;
            try
            {
                settings = AlegraWorkflow.Settings();
                preview = this.BuildElectronicRequest(values, payments);
            }
            catch (InvalidOperationException ex)
            {
                Mensajes.MensajeErrorForm(ex.Message);
                return false;
            }

            // Solo se revisan los vínculos (nombre/tipo/id del producto): no depende de importes.
            List<string> unlinked = AlegraInvoiceBuilder.UnlinkedProducts(preview, settings);
            if (unlinked.Count == 0) return true;

            int linkedCount = AlegraInvoiceBuilder.LinkedProducts(preview, settings).Count;
            bool anyLinked = linkedCount > 0;
            string invoiceTotalText = "el total de los productos vinculados";
            string saleTotalText = this.lblTotal.Text;
            if (anyLinked)
            {
                try
                {
                    this.electronicOnlyLinked = true;
                    string partial = this.BuildElectronicRequest(values, payments);
                    decimal total = AlegraInvoiceBuilder.ExpectedTotal(partial, settings);
                    invoiceTotalText = total.ToString("C0", System.Globalization.CultureInfo.GetCultureInfo("es-CO"));
                }
                catch (Exception) { /* solo es el texto informativo: el cálculo real lo hace el envío */ }
                finally { this.electronicOnlyLinked = false; }
            }

            using (var dialog = new FrmUnlinkedProductsDialog(unlinked, linkedCount, invoiceTotalText, saleTotalText))
            {
                DialogResult answer = dialog.ShowDialog(this);
                if (answer == DialogResult.Yes) { this.electronicOnlyLinked = true; return true; }
                if (answer == DialogResult.No)
                {
                    // Desmarcar la casilla limpia también los datos fiscales capturados.
                    this.opcionesPedido.ElectronicInvoice.Checked = false;
                    return true;
                }
                return false;
            }
        }

        // Mientras se espera a Alegra la ventana no se cierra (Alt+F4, X, Escape). El cierre del
        // sistema operativo o de la aplicación sí se respeta.
        private void BlockCloseWhileWaiting(object sender, FormClosingEventArgs e)
        {
            if (this.waitingForElectronic && e.CloseReason == CloseReason.UserClosing) e.Cancel = true;
        }

        // Devuelve null si la factura electrónica quedó emitida e impresa; si no, el motivo.
        private async Task<string> WaitForElectronicInvoiceAsync(int saleId)
        {
            this.waitingForElectronic = true;
            this.ShowElectronicOverlay();
            try
            {
                var settings = await Task.Run(() => AlegraWorkflow.Settings());
                DateTime started = DateTime.UtcNow;
                while (true)
                {
                    await Task.Run(() => AlegraWorkflow.Expedite(saleId));
                    var worker = AlegraBackgroundWorker.Current;
                    if (worker != null) await worker.RunCycleAsync();

                    DataTable request = await Task.Run(() => AlegraWorkflow.Request(saleId));
                    DataRow row = request != null && request.Rows.Count > 0 ? request.Rows[0] : null;
                    string detail;
                    string failure;
                    ElectronicState state = EvaluateElectronicRequest(row, settings, out detail, out failure);

                    if (state == ElectronicState.Done) return null;
                    if (state == ElectronicState.Failed) { await this.AskElectronicDecisionAsync(failure, false); return failure; }

                    this.electronicDetail.Text = detail;

                    // Un diálogo de guardado abierto espera al usuario: no cuenta como retraso de Alegra.
                    bool printing = row != null && Convert.ToString(row["PrintStatus"]) == "PRINTING";
                    if (printing) started = DateTime.UtcNow;
                    else if ((DateTime.UtcNow - started).TotalSeconds > ElectronicWaitLimitSeconds)
                    {
                        string slow = "Alegra está tardando más de lo normal en responder.";
                        if (await this.AskElectronicDecisionAsync(slow, true)) { started = DateTime.UtcNow; continue; }
                        return slow;
                    }
                    await Task.Delay(ElectronicPollMilliseconds);
                }
            }
            finally
            {
                this.waitingForElectronic = false;
                this.electronicOverlay.Visible = false;
            }
        }

        // Estado de la solicitud -> qué mostrar. Separado de la interfaz para poder razonarlo aparte.
        private static ElectronicState EvaluateElectronicRequest(DataRow row, AlegraInvoiceConfiguration settings,
            out string detail, out string failure)
        {
            detail = "Conectando con Alegra...";
            failure = null;
            if (row == null) return ElectronicState.Waiting;

            string status = Convert.ToString(row["Status"]);
            string printStatus = Convert.ToString(row["PrintStatus"]);
            string error = row["LastError"] == DBNull.Value ? null : Convert.ToString(row["LastError"]);
            bool hasPdf = row["PdfPath"] != DBNull.Value && !string.IsNullOrWhiteSpace(Convert.ToString(row["PdfPath"]));

            if (status == "REJECTED" || status == "NEEDS_REVIEW" || status == "UNCERTAIN")
            {
                failure = string.IsNullOrWhiteSpace(error) ? "Alegra no pudo emitir la factura electrónica." : error;
                return ElectronicState.Failed;
            }

            if (status != "ACCEPTED")
            {
                detail = status == "PENDING" ? "Creando la factura en Alegra..." : "Alegra está emitiendo la factura ante la DIAN...";
                return ElectronicState.Waiting;
            }

            // Aceptada por la DIAN: falta la copia/impresión, salvo que la impresión automática no aplique.
            DateTime created = DateTime.SpecifyKind(Convert.ToDateTime(row["CreatedAt"]), DateTimeKind.Utc);
            bool printExpected = settings.AutoPrint && (!settings.AutoPrintFromUtc.HasValue || created >= settings.AutoPrintFromUtc.Value);

            if (printStatus == "PRINTED" || (!printExpected && hasPdf)) return ElectronicState.Done;
            if (printStatus == "CANCELLED" || printStatus == "UNKNOWN")
            {
                failure = "La factura fue aceptada, pero no se confirmó su impresión. Reimprímala desde el detalle de la venta.";
                return ElectronicState.Failed;
            }
            if (!string.IsNullOrWhiteSpace(error))
            {
                failure = "La factura fue aceptada, pero falló la descarga o impresión: " + error;
                return ElectronicState.Failed;
            }

            detail = printStatus == "PRINTING"
                ? "Imprimiendo la factura electrónica. Si aparece el cuadro para guardar el archivo, confírmelo."
                : "Alegra aceptó la factura. Preparando la factura electrónica...";
            return ElectronicState.Waiting;
        }

        // Muestra el problema y espera la decisión del usuario. Devuelve true si decide seguir esperando.
        private async Task<bool> AskElectronicDecisionAsync(string message, bool canKeepWaiting)
        {
            this.electronicTitle.Text = "La factura electrónica no se completó";
            this.electronicTitle.ForeColor = Theme.Danger;
            this.electronicDetail.Text = message + (canKeepWaiting
                ? "\r\nPuede seguir esperando o cerrar: la venta ya quedó registrada y la factura se seguirá intentando."
                : "\r\nLa venta ya quedó registrada. Revise la factura en Observar ventas.");
            this.electronicProgress.Visible = false;
            this.electronicKeepWaiting.Visible = canKeepWaiting;
            this.electronicClose.Visible = true;
            this.electronicDecision = new TaskCompletionSource<bool>();
            bool keepWaiting = await this.electronicDecision.Task;
            if (keepWaiting)
            {
                this.electronicTitle.Text = "Esperando respuesta de Alegra y factura electrónica";
                this.electronicTitle.ForeColor = Theme.TextPrimary;
                this.electronicProgress.Visible = true;
                this.electronicKeepWaiting.Visible = false;
                this.electronicClose.Visible = false;
            }
            return keepWaiting;
        }

        private void ShowElectronicOverlay()
        {
            if (this.electronicOverlay == null) this.BuildElectronicOverlay();
            this.electronicTitle.Text = "Esperando respuesta de Alegra y factura electrónica";
            this.electronicTitle.ForeColor = Theme.TextPrimary;
            this.electronicDetail.Text = "Conectando con Alegra...";
            this.electronicProgress.Visible = true;
            this.electronicKeepWaiting.Visible = false;
            this.electronicClose.Visible = false;
            this.electronicOverlay.Visible = true;
            this.electronicOverlay.BringToFront();
        }

        private void BuildElectronicOverlay()
        {
            this.electronicOverlay = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Background, Visible = false };

            TableLayoutPanel layout = new TableLayoutPanel
            {
                Anchor = AnchorStyles.None,
                AutoSize = true,
                ColumnCount = 1,
                MaximumSize = new Size(760, 0),
                BackColor = Color.Transparent,
            };

            IconPictureBox icon = new IconPictureBox
            {
                IconChar = IconChar.FileInvoice,
                IconColor = Theme.Primary,
                IconSize = 72,
                Size = new Size(72, 72),
                Anchor = AnchorStyles.None,
                BackColor = Color.Transparent,
            };
            this.electronicTitle = new Label
            {
                AutoSize = true,
                Font = Theme.Title,
                ForeColor = Theme.TextPrimary,
                TextAlign = ContentAlignment.MiddleCenter,
                Anchor = AnchorStyles.None,
                MaximumSize = new Size(740, 0),
                Margin = new Padding(0, Theme.Gap, 0, Theme.Gap),
            };
            this.electronicDetail = new Label
            {
                AutoSize = true,
                Font = Theme.Heading,
                ForeColor = Theme.TextMuted,
                TextAlign = ContentAlignment.MiddleCenter,
                Anchor = AnchorStyles.None,
                MaximumSize = new Size(740, 0),
                Margin = new Padding(0, 0, 0, Theme.Gap),
            };
            this.electronicProgress = new ProgressBar
            {
                Style = ProgressBarStyle.Marquee,
                MarqueeAnimationSpeed = 30,
                Width = 420,
                Height = 12,
                Anchor = AnchorStyles.None,
            };

            this.electronicKeepWaiting = new ModernButton
            {
                Text = "Seguir esperando",
                Kind = ButtonKind.Primary,
                Size = new Size(210, 52),
                Anchor = AnchorStyles.None,
                Visible = false,
            };
            this.electronicKeepWaiting.Click += (s, e) => this.electronicDecision?.TrySetResult(true);
            this.electronicClose = new ModernButton
            {
                Text = "Cerrar",
                Kind = ButtonKind.Neutral,
                Size = new Size(210, 52),
                Anchor = AnchorStyles.None,
                Visible = false,
            };
            this.electronicClose.Click += (s, e) => this.electronicDecision?.TrySetResult(false);

            FlowLayoutPanel buttons = new FlowLayoutPanel
            {
                AutoSize = true,
                Anchor = AnchorStyles.None,
                BackColor = Color.Transparent,
                Margin = new Padding(0, Theme.Gap, 0, 0),
            };
            buttons.Controls.Add(this.electronicKeepWaiting);
            buttons.Controls.Add(this.electronicClose);

            layout.Controls.Add(icon);
            layout.Controls.Add(this.electronicTitle);
            layout.Controls.Add(this.electronicDetail);
            layout.Controls.Add(this.electronicProgress);
            layout.Controls.Add(buttons);

            // Se centra en el panel: TableLayoutPanel con Anchor.None dentro de una rejilla de una celda.
            TableLayoutPanel host = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 1 };
            host.Controls.Add(layout, 0, 0);
            this.electronicOverlay.Controls.Add(host);
            this.Controls.Add(this.electronicOverlay);
        }
    }
}
