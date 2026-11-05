using CapaPresentacion.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Mail;
using System.Windows.Forms;
using CapaNegocio.Alegra;
using System.Threading;

namespace CapaPresentacion.Formularios.FormsVentas
{
    public sealed class FrmElectronicInvoiceCustomer : Form
    {
        private readonly Dictionary<string, ModernTextBox> fields = new Dictionary<string, ModernTextBox>();
        private readonly ComboBox documentType = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly ComboBox personType = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly ComboBox regime = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly CancellationTokenSource lifetime = new CancellationTokenSource();
        private readonly ComboBox remoteContacts = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, DisplayMember = "Name", Width = 500 };
        private sealed class ContactChoice { public string Name { get; set; } public Dictionary<string, object> Contact { get; set; } }
        private string selectedRemoteId;
        private readonly Label error = new Label { AutoSize = true, ForeColor = Theme.Danger, MaximumSize = new Size(500, 0) };
        public Dictionary<string, string> FiscalData { get; private set; }

        public FrmElectronicInvoiceCustomer(string customerName, string email)
        {
            Text = "Datos para factura electrónica";
            ClientSize = new Size(570, 680);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            Font = Theme.Body;
            BackColor = Theme.Background;
            Panel root = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
            FlowLayoutPanel content = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, FlowDirection = FlowDirection.TopDown, WrapContents = false };
            Label title = new Label { Text = "Factura electrónica · Datos del receptor", AutoSize = true };
            title.StyleLabel(LabelKind.Heading);
            content.Controls.Add(title);
            documentType.Items.AddRange(new object[] { "CC", "NIT", "CE", "PP", "TI", "TE", "RC", "DIE", "NUIP", "FOREIGN_NIT" });
            documentType.SelectedIndex = 0;
            personType.Items.AddRange(new object[] { "PERSON_ENTITY", "LEGAL_ENTITY" });
            personType.SelectedIndex = 0;
            AddField(content, "Tipo de documento", documentType);
            AddField(content, "Tipo de persona", personType);
            regime.Items.AddRange(new object[] { "COMMON_REGIME", "SIMPLIFIED_REGIME", "NATIONAL_CONSUMPTION_TAX", "INC_IVA_RESPONSIBLE" });
            AddField(content, "Régimen fiscal del receptor", regime);
            foreach (var field in new[] { "Identificación", "Dígito de verificación (NIT)", "Nombre o razón social", "Primer nombre", "Segundo nombre", "Apellidos", "Correo electrónico", "Teléfono", "Dirección fiscal", "Departamento", "Municipio", "País" })
            {
                ModernTextBox input = new ModernTextBox();
                fields.Add(field, input);
                AddField(content, field, input);
            }
            fields["Nombre o razón social"].Text = customerName;
            fields["Correo electrónico"].Text = email;
            fields["País"].Text = "Colombia";
            fields["Identificación"].TextChanged += (s,e) => selectedRemoteId = null;
            documentType.SelectedIndexChanged += (s,e) => selectedRemoteId = null;
            remoteContacts.SelectedIndexChanged += (s,e) =>
            {
                if (!(remoteContacts.SelectedItem is ContactChoice choice)) return;
                var contact=choice.Contact;
                var identification=contact.ContainsKey("identificationObject") ? contact["identificationObject"] as Dictionary<string,object> : null;
                if(identification==null)return;
                documentType.SelectedItem=AlegraWorkflow.Text(identification,"type");
                fields["Identificación"].Text=AlegraWorkflow.Text(identification,"number");
                fields["Dígito de verificación (NIT)"].Text=AlegraWorkflow.Text(identification,"dv");
                personType.SelectedItem=AlegraWorkflow.Text(contact,"kindOfPerson");
                regime.SelectedItem=AlegraWorkflow.Text(contact,"regime");
                fields["Nombre o razón social"].Text=AlegraWorkflow.Text(contact,"name");
                fields["Correo electrónico"].Text=AlegraWorkflow.Text(contact,"email");
                fields["Teléfono"].Text=AlegraWorkflow.Text(contact,"phonePrimary");
                if(contact.ContainsKey("nameObject") && contact["nameObject"] is Dictionary<string,object> names)
                {
                    fields["Primer nombre"].Text=AlegraWorkflow.Text(names,"firstName"); fields["Segundo nombre"].Text=AlegraWorkflow.Text(names,"secondName"); fields["Apellidos"].Text=AlegraWorkflow.Text(names,"lastName");
                }
                if(contact.ContainsKey("address") && contact["address"] is Dictionary<string,object> address)
                {
                    fields["Dirección fiscal"].Text=AlegraWorkflow.Text(address,"address");fields["Departamento"].Text=AlegraWorkflow.Text(address,"department");fields["Municipio"].Text=AlegraWorkflow.Text(address,"city");fields["País"].Text=AlegraWorkflow.Text(address,"country");
                }
                selectedRemoteId=AlegraWorkflow.Text(contact,"id");
                error.Text="Contacto existente seleccionado. Sus datos remotos no se modificarán automáticamente.";
            };
            ModernButton find = new ModernButton { Text = "Consultar cliente en Alegra", Width = 300, Kind = ButtonKind.Outline };
            find.Click += async (s, e) =>
            {
                find.Enabled = false;
                try
                {
                    using (var api = new AlegraClient())
                    {
                        string json = await api.FindContactsAsync(fields["Identificación"].Text, lifetime.Token);
                        if (IsDisposed) return;
                        var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                        object parsed = serializer.DeserializeObject(json);
                        object[] contacts = parsed as object[];
                        if (contacts == null && parsed is Dictionary<string, object> wrapper && wrapper.ContainsKey("data")) contacts = wrapper["data"] as object[];
                        error.Text = contacts == null || contacts.Length == 0 ? "No se encontró un contacto en Alegra con esa identificación."
                            : "Consulta correcta: " + contacts.Length + " contacto(s). Confirme los datos fiscales antes de guardar.";
                        remoteContacts.Items.Clear();
                        if(contacts!=null)foreach(Dictionary<string,object> contact in contacts)
                            remoteContacts.Items.Add(new ContactChoice {Name=AlegraWorkflow.Text(contact,"name")+" ["+AlegraWorkflow.Text(contact,"id")+"]",Contact=contact});
                    }
                }
                catch (OperationCanceledException) { if (!IsDisposed) error.Text = "Consulta cancelada o tiempo de espera agotado."; }
                catch (Exception ex) { if (!IsDisposed) error.Text = ex is InvalidOperationException || ex is ArgumentException ? ex.Message : "No se pudo consultar Alegra. Revise la conexión."; }
                finally { if (!IsDisposed) find.Enabled = true; }
            };
            content.Controls.Add(find);
            AddField(content,"Seleccione un contacto existente (opcional)",remoteContacts);
            content.Controls.Add(error);
            FlowLayoutPanel actions = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 48, FlowDirection = FlowDirection.RightToLeft };
            ModernButton save = new ModernButton { Text = "Confirmar datos", Width = 180, Kind = ButtonKind.Success };
            ModernButton cancel = new ModernButton { Text = "Cancelar", Width = 120, Kind = ButtonKind.Outline };
            save.Click += (s, e) => Save();
            cancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            actions.Controls.Add(save);
            actions.Controls.Add(cancel);
            root.Controls.Add(content);
            root.Controls.Add(actions);
            Controls.Add(root);
            CancelButton = cancel;
            FormClosed += (s, e) => lifetime.Cancel();
        }

        private static void AddField(FlowLayoutPanel host, string caption, Control input)
        {
            Panel field = new Panel { Width = 500, Height = 68 };
            Label label = new Label { Text = caption, Dock = DockStyle.Top, Height = 22 };
            label.StyleLabel(LabelKind.Caption);
            input.Dock = DockStyle.Top;
            field.Controls.Add(input);
            field.Controls.Add(label);
            field.TabIndex = host.Controls.Count;
            host.Controls.Add(field);
        }

        private void Save()
        {
            if (regime.SelectedIndex < 0) { error.Text = "Seleccione el régimen fiscal del receptor."; regime.Focus(); return; }
            foreach (var field in fields)
            {
                bool natural = personType.Text == "PERSON_ENTITY";
                bool foreign = documentType.Text == "PP" || documentType.Text == "DIE" || documentType.Text == "FOREIGN_NIT";
                if (field.Key == "Teléfono" || field.Key == "Segundo nombre"
                    || (field.Key == "Dígito de verificación (NIT)" && documentType.Text != "NIT")
                    || (field.Key == "Nombre o razón social" && natural)
                    || ((field.Key == "Primer nombre" || field.Key == "Apellidos") && !natural)
                    || ((field.Key == "Departamento" || field.Key == "Municipio" || field.Key == "Dirección fiscal") && foreign)) continue;
                if (string.IsNullOrWhiteSpace(field.Value.Text))
                {
                    error.Text = "Complete: " + field.Key;
                    field.Value.HasError = true;
                    field.Value.Focus();
                    return;
                }
            }
            try { new MailAddress(fields["Correo electrónico"].Text.Trim()); }
            catch (FormatException) { error.Text = "Revise el correo electrónico."; return; }
            string id = fields["Identificación"].Text.Trim();
            if ((documentType.Text == "NIT" || documentType.Text == "CC" || documentType.Text == "TI")
                && !System.Linq.Enumerable.All(id, char.IsDigit)) { error.Text = "La identificación debe contener solo números, sin puntos ni guiones."; return; }
            string dv = fields["Dígito de verificación (NIT)"].Text.Trim();
            if (documentType.Text == "NIT" && (dv.Length != 1 || !char.IsDigit(dv[0]))) { error.Text = "El dígito de verificación del NIT debe tener un número."; return; }
            FiscalData = new Dictionary<string, string>();
            foreach (var field in fields) FiscalData.Add(field.Key, field.Value.Text.Trim());
            FiscalData.Add("identificationType", documentType.Text);
            FiscalData.Add("kindOfPerson", personType.Text);
            FiscalData.Add("regime", regime.Text);
            if(!string.IsNullOrWhiteSpace(selectedRemoteId))FiscalData.Add("remoteClientId",selectedRemoteId);
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
