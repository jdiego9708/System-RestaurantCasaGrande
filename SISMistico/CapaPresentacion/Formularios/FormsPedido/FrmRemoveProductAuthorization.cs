using CapaPresentacion.Api;
using CapaPresentacion.UI;
using FontAwesome.Sharp;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace CapaPresentacion.Formularios.FormsPedido
{
    // Autorización para una acción destructiva sobre algo ya guardado (quitar un producto de un pedido, cancelar un
    // domicilio): código maestro único de 8 dígitos (el mismo que pide el celular) y observación obligatoria, en
    // el mismo diálogo. Ver ApiConfig.CodigoMaestroValido. Los textos se pueden adaptar a cada acción.
    public sealed class FrmRemoveProductAuthorization : Form
    {
        private readonly TextBox codeBox;
        private readonly TextBox observationBox;
        private readonly Label errorLabel;

        /// <summary>Motivo escrito por quien autoriza (sin espacios en los extremos). Solo válido si el diálogo devolvió OK.</summary>
        public string Observation { get; private set; }

        /// <summary>Autorización para quitar un producto ya guardado (textos originales).</summary>
        public FrmRemoveProductAuthorization(string productName)
            : this("Quitar producto", "Quitar «" + productName + "»", "Quitar producto", IconChar.Trash, "Cancelar")
        {
        }

        /// <summary>Autorización con los textos de la acción que se confirma.</summary>
        /// <param name="windowTitle">Título de la ventana.</param>
        /// <param name="heading">Encabezado: qué se va a hacer y sobre qué.</param>
        /// <param name="acceptText">Texto del botón que confirma.</param>
        /// <param name="acceptIcon">Icono del botón que confirma.</param>
        /// <param name="dismissText">Texto del botón que cierra sin hacer nada.</param>
        public FrmRemoveProductAuthorization(string windowTitle, string heading, string acceptText, IconChar acceptIcon, string dismissText)
        {
            Text = windowTitle;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(480, 380);
            BackColor = Theme.Background;
            Font = Theme.Body;
            Padding = new Padding(Theme.Gap * 2);

            Label title = new Label
            {
                Text = heading,
                Font = Theme.Heading,
                Dock = DockStyle.Top,
                Height = 34,
            };

            Label codeCaption = new Label { Text = "Código maestro (8 dígitos)", Font = Theme.BodyBold, Dock = DockStyle.Top, Height = 24 };
            codeBox = new TextBox
            {
                Dock = DockStyle.Top,
                MaxLength = 8,
                UseSystemPasswordChar = true,
                Font = Theme.Body,
            };
            // Solo dígitos: se filtra al teclear, y el pegado se valida al aceptar.
            codeBox.KeyPress += (s, e) => { if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true; };

            Label observationCaption = new Label
            {
                Text = "Observación (obligatoria)",
                Font = Theme.BodyBold,
                Dock = DockStyle.Top,
                Height = 24,
            };
            observationBox = new TextBox
            {
                Dock = DockStyle.Top,
                Multiline = true,
                Height = 96,
                MaxLength = 200,
                ScrollBars = ScrollBars.Vertical,
                Font = Theme.Body,
            };

            errorLabel = new Label { Dock = DockStyle.Top, Height = 28, ForeColor = Theme.Danger, Text = string.Empty };

            Panel buttons = new Panel { Dock = DockStyle.Bottom, Height = 64, Padding = new Padding(0, Theme.Gap, 0, 0) };
            ModernButton cancel = new ModernButton { Text = dismissText, Kind = ButtonKind.Outline, Icon = IconChar.Xmark, Width = 150, Height = 48, Dock = DockStyle.Left };
            cancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            ModernButton accept = new ModernButton { Text = acceptText, Kind = ButtonKind.Success, Icon = acceptIcon, Width = 210, Height = 48, Dock = DockStyle.Right };
            accept.Click += (s, e) => Accept();
            buttons.Controls.Add(accept);
            buttons.Controls.Add(cancel);

            // Orden de apilado (Dock = Top): el último agregado queda arriba.
            Controls.Add(buttons);
            Controls.Add(errorLabel);
            Controls.Add(observationBox);
            Controls.Add(observationCaption);
            Controls.Add(new Panel { Dock = DockStyle.Top, Height = Theme.Gap });
            Controls.Add(codeBox);
            Controls.Add(codeCaption);
            Controls.Add(title);
            CancelButton = cancel;
            AcceptButton = accept;
            Shown += (s, e) => codeBox.Focus();
        }

        private void Accept()
        {
            string code = codeBox.Text.Trim();
            string observation = observationBox.Text.Trim();

            if (code.Length != 8 || !System.Linq.Enumerable.All(code, char.IsDigit))
            {
                Fail("El código maestro tiene exactamente 8 dígitos.", codeBox);
                return;
            }
            if (!ApiConfig.CodigoMaestroConfigurado)
            {
                Fail("El código maestro no está configurado en este PC (Cambiar codigo maestro, menú Inicio).", codeBox);
                return;
            }
            if (!ApiConfig.CodigoMaestroValido(code))
            {
                codeBox.Clear();
                Fail("El código maestro no es correcto.", codeBox);
                return;
            }
            if (observation.Length == 0)
            {
                Fail("Escriba la observación: es obligatoria.", observationBox);
                return;
            }

            Observation = observation;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void Fail(string message, Control focus)
        {
            errorLabel.Text = message;
            focus.Focus();
        }
    }
}
