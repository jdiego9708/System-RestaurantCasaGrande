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
        private readonly ComboBox documentType = new FiscalComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly ComboBox personType = new FiscalComboBox { DropDownStyle = ComboBoxStyle.DropDownList, DisplayMember = "Name", ValueMember = "Code" };
        private readonly ComboBox regime = new FiscalComboBox { DropDownStyle = ComboBoxStyle.DropDownList, DisplayMember = "Name", ValueMember = "Code" };
        private readonly CancellationTokenSource lifetime = new CancellationTokenSource();
        private readonly DataGridView remoteContacts = new DataGridView
        {
            Width = 500, Height = 126, ReadOnly = true, AllowUserToAddRows = false,
            AllowUserToDeleteRows = false, AllowUserToResizeRows = false,
            RowHeadersVisible = false, AutoGenerateColumns = false, MultiSelect = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            BackgroundColor = Theme.Surface, BorderStyle = BorderStyle.None,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            Margin = new Padding(0, 0, 0, 8), Visible = false,
        };
        private sealed class FiscalOption { public string Name { get; set; } public string Code { get; set; } }
        private readonly Dictionary<Control, Label> fieldLabels = new Dictionary<Control, Label>();
        private readonly Label contactNotice = new Label { AutoSize = true, MaximumSize = new Size(500, 0), ForeColor = Theme.TextPrimary, Margin = new Padding(0, 0, 0, 8), Visible = false };
        private readonly Label resultsNotice = new Label
        {
            AutoSize = true, MaximumSize = new Size(500, 0), ForeColor = Theme.TextMuted,
            Margin = new Padding(0, 0, 0, 8), Visible = false,
        };
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
            personType.Items.AddRange(new object[]
            {
                new FiscalOption { Name = "Persona natural", Code = "PERSON_ENTITY" },
                new FiscalOption { Name = "Persona jurídica", Code = "LEGAL_ENTITY" },
            });
            personType.SelectedIndex = 0;
            ModernTextBox lookupIdentification = new ModernTextBox();
            fields.Add("Identificación", lookupIdentification);
            lookupIdentification.TextChanged += (s, e) => selectedRemoteId = null;
            documentType.SelectedIndexChanged += (s, e) => selectedRemoteId = null;
            personType.Width = 158;
            documentType.Width = 158;
            regime.Width = 158;
            personType.DropDownWidth = 240;
            documentType.DropDownWidth = 180;
            regime.DropDownWidth = 340;
            FlowLayoutPanel fiscalTypesRow = new FlowLayoutPanel
            {
                Width = 500, Height = 68, WrapContents = false, Margin = Padding.Empty,
            };
            AddField(fiscalTypesRow, "Tipo de persona", personType, 158);
            AddField(fiscalTypesRow, "Tipo de documento", documentType, 158);
            regime.Items.AddRange(new object[]
            {
                new FiscalOption { Name = "Régimen común", Code = "COMMON_REGIME" },
                new FiscalOption { Name = "Régimen simplificado", Code = "SIMPLIFIED_REGIME" },
                new FiscalOption { Name = "Impuesto nacional al consumo", Code = "NATIONAL_CONSUMPTION_TAX" },
                new FiscalOption { Name = "Responsable de IVA (INC)", Code = "INC_IVA_RESPONSIBLE" },
            });
            AddField(fiscalTypesRow, "Régimen fiscal del receptor", regime, 168);
            content.Controls.Add(fiscalTypesRow);
            var inputOrder = new[] { "Dígito de verificación (NIT)", "Nombre o razón social", "Primer nombre", "Segundo nombre", "Apellidos", "Correo electrónico", "Teléfono", "Dirección fiscal", "Departamento", "Municipio", "País" };
            foreach (var field in inputOrder)
            {
                ModernTextBox input = new ModernTextBox();
                fields.Add(field, input);
            }
            FlowLayoutPanel identificationRow = new FlowLayoutPanel
            {
                Width = 500, Height = 68, WrapContents = false, Margin = Padding.Empty,
            };
            lookupIdentification.Width = 258;
            AddField(identificationRow, "Documento", lookupIdentification, 258);
            ModernButton find = new ModernButton { Text = "Consultar cliente en Alegra", Width = 234, Height = 40, Kind = ButtonKind.Outline };
            find.Margin = new Padding(0, 22, 0, 0);
            find.TabIndex = 1;
            identificationRow.Controls.Add(find);
            content.Controls.Add(identificationRow);
            content.Controls.Add(resultsNotice);
            ConfigureContactsGrid();
            remoteContacts.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ContactName", HeaderText = "Clientes encontrados", FillWeight = 75,
                SortMode = DataGridViewColumnSortMode.NotSortable,
            });
            remoteContacts.Columns.Add(new DataGridViewButtonColumn
            {
                Name = "SelectContact", HeaderText = "", Text = "Seleccionar",
                UseColumnTextForButtonValue = true, Width = 140,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
            });
            content.Controls.Add(remoteContacts);
            content.Controls.Add(contactNotice);
            FlowLayoutPanel businessNameRow = new FlowLayoutPanel
            {
                Width = 500, Height = 68, WrapContents = false, Margin = Padding.Empty,
            };
            AddField(businessNameRow, "Nombre o razón social", fields["Nombre o razón social"]);
            AddField(businessNameRow, "Primer nombre", fields["Primer nombre"]);
            foreach (Control control in businessNameRow.Controls) control.Width = 242;
            content.Controls.Add(businessNameRow);
            FlowLayoutPanel namesRow = new FlowLayoutPanel
            {
                Width = 500, Height = 68, WrapContents = false, Margin = Padding.Empty,
            };
            AddField(namesRow, "Segundo nombre", fields["Segundo nombre"]);
            AddField(namesRow, "Primer apellido", fields["Apellidos"]);
            foreach (Control control in namesRow.Controls) control.Width = 242;
            content.Controls.Add(namesRow);
            FlowLayoutPanel taxEmailRow = new FlowLayoutPanel
            {
                Width = 500, Height = 68, WrapContents = false, Margin = Padding.Empty,
            };
            Panel verificationField = new Panel { Width = 64, Height = 68, Margin = new Padding(0, 0, 8, 0) };
            Label verificationLabel = new Label { Tag = "DV (NIT)", Text = "DV (NIT)", Dock = DockStyle.Top, Height = 22 };
            verificationLabel.StyleLabel(LabelKind.Caption);
            fieldLabels.Add(fields["Dígito de verificación (NIT)"], verificationLabel);
            fields["Dígito de verificación (NIT)"].Dock = DockStyle.Top;
            verificationField.Controls.Add(fields["Dígito de verificación (NIT)"]);
            verificationField.Controls.Add(verificationLabel);
            taxEmailRow.Controls.Add(verificationField);
            Panel emailField = new Panel { Width = 428, Height = 68, Margin = Padding.Empty };
            Label emailLabel = new Label { Tag = "Correo electrónico", Text = "Correo electrónico", Dock = DockStyle.Top, Height = 22 };
            emailLabel.StyleLabel(LabelKind.Caption);
            fieldLabels.Add(fields["Correo electrónico"], emailLabel);
            fields["Correo electrónico"].Dock = DockStyle.Top;
            emailField.Controls.Add(fields["Correo electrónico"]);
            emailField.Controls.Add(emailLabel);
            taxEmailRow.Controls.Add(emailField);
            content.Controls.Add(taxEmailRow);
            FlowLayoutPanel phoneAddressRow = new FlowLayoutPanel
            {
                Width = 500, Height = 68, WrapContents = false, Margin = Padding.Empty,
            };
            AddField(phoneAddressRow, "Teléfono", fields["Teléfono"]);
            AddField(phoneAddressRow, "Dirección", fields["Dirección fiscal"]);
            foreach (Control control in phoneAddressRow.Controls) control.Width = 242;
            content.Controls.Add(phoneAddressRow);
            FlowLayoutPanel locationRow = new FlowLayoutPanel
            {
                Width = 500, Height = 68, WrapContents = false, Margin = Padding.Empty,
            };
            AddField(locationRow, "Departamento", fields["Departamento"]);
            AddField(locationRow, "Municipio", fields["Municipio"]);
            AddField(locationRow, "País", fields["País"]);
            foreach (Control control in locationRow.Controls) control.Width = 158;
            content.Controls.Add(locationRow);
            fields["Nombre o razón social"].Text = customerName;
            fields["Correo electrónico"].Text = email;
            fields["País"].Text = "Colombia";
            fields["Departamento"].Text = "Caldas";
            fields["Municipio"].Text = "Manizales";
            remoteContacts.CellContentClick += (s, e) =>
            {
                if (e.RowIndex < 0 || e.ColumnIndex != remoteContacts.Columns["SelectContact"].Index) return;
                var contact = remoteContacts.Rows[e.RowIndex].Tag as Dictionary<string, object>;
                if (contact == null) return;
                var identification=contact.ContainsKey("identificationObject") ? contact["identificationObject"] as Dictionary<string,object> : null;
                if(identification==null)return;
                documentType.SelectedItem=AlegraWorkflow.Text(identification,"type");
                fields["Identificación"].Text=AlegraWorkflow.Text(identification,"number");
                fields["Dígito de verificación (NIT)"].Text=AlegraWorkflow.Text(identification,"dv");
                SelectFiscalOption(personType, AlegraWorkflow.Text(contact,"kindOfPerson"));
                SelectFiscalOption(regime, AlegraWorkflow.Text(contact,"regime"));
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
                remoteContacts.Invalidate();
                contactNotice.Text = "Cliente seleccionado: " + AlegraWorkflow.Text(contact, "name");
                contactNotice.ForeColor = Theme.Success;
                contactNotice.Visible = true;
                error.Text = string.Empty;
            };
            find.Click += async (s, e) =>
            {
                find.Enabled = false;
                remoteContacts.Rows.Clear();
                remoteContacts.Visible = false;
                resultsNotice.Visible = false;
                contactNotice.Visible = false;
                error.Text = string.Empty;
                try
                {
                    using (var api = new AlegraClient())
                    {
                        string json = await api.FindContactsAsync(lookupIdentification.Text, lifetime.Token);
                        if (IsDisposed) return;
                        var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                        object parsed = serializer.DeserializeObject(json);
                        object[] contacts = parsed as object[];
                        if (contacts == null && parsed is Dictionary<string, object> wrapper && wrapper.ContainsKey("data")) contacts = wrapper["data"] as object[];
                        resultsNotice.Visible = true;
                        resultsNotice.Text = contacts == null || contacts.Length == 0 ? "No se encontró un contacto en Alegra con esa identificación."
                            : "Consulta correcta: " + contacts.Length + " contacto(s). Confirme los datos fiscales antes de continuar.";
                        if (contacts != null) foreach (object item in contacts)
                        {
                            var contact = item as Dictionary<string, object>;
                            if (contact == null) continue;
                            int row = remoteContacts.Rows.Add(AlegraWorkflow.Text(contact, "name"));
                            remoteContacts.Rows[row].Tag = contact;
                        }
                        remoteContacts.Height = remoteContacts.ColumnHeadersHeight + remoteContacts.RowTemplate.Height * Math.Min(3, remoteContacts.Rows.Count) + 2;
                        remoteContacts.Visible = remoteContacts.Rows.Count > 0;
                        remoteContacts.ClearSelection();
                    }
                }
                catch (OperationCanceledException) { if (!IsDisposed) error.Text = "Consulta cancelada o tiempo de espera agotado."; }
                catch (Exception ex) { if (!IsDisposed) error.Text = ex is InvalidOperationException || ex is ArgumentException ? ex.Message : "No se pudo consultar Alegra. Revise la conexión."; }
                finally { if (!IsDisposed) find.Enabled = true; }
            };
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
            personType.SelectedIndexChanged += (s, e) => UpdateRequiredLabels();
            documentType.SelectedIndexChanged += (s, e) => UpdateRequiredLabels();
            UpdateRequiredLabels();
            FormClosed += (s, e) => lifetime.Cancel();
        }

        // Conserva navegacion, accesibilidad y apertura nativas; personaliza la superficie y las opciones.
        private sealed class FiscalComboBox : ComboBox
        {
            private bool hover;
            public FiscalComboBox()
            {
                DrawMode = DrawMode.OwnerDrawFixed;
                FlatStyle = FlatStyle.Flat;
                Font = Theme.Body;
                BackColor = Theme.Surface;
                ForeColor = Theme.TextPrimary;
                ItemHeight = 30;
                IntegralHeight = false;
                DropDownHeight = 240;
            }

            protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
            protected override void OnMouseLeave(EventArgs e) { hover = false; Invalidate(); base.OnMouseLeave(e); }
            protected override void OnGotFocus(EventArgs e) { base.OnGotFocus(e); Invalidate(); }
            protected override void OnLostFocus(EventArgs e) { base.OnLostFocus(e); Invalidate(); }
            protected override void OnDropDown(EventArgs e) { base.OnDropDown(e); Invalidate(); }
            protected override void OnDropDownClosed(EventArgs e) { base.OnDropDownClosed(e); Invalidate(); }
            protected override void OnSelectedIndexChanged(EventArgs e) { base.OnSelectedIndexChanged(e); Invalidate(); }

            protected override void OnDrawItem(DrawItemEventArgs e)
            {
                if (e.Index < 0) return;
                bool selected = (e.State & DrawItemState.Selected) != 0;
                using (var background = new SolidBrush(selected ? Theme.Lighten(Theme.Primary, 0.91f) : Theme.Surface))
                    e.Graphics.FillRectangle(background, e.Bounds);
                Rectangle text = Rectangle.Inflate(e.Bounds, -12, 0);
                TextRenderer.DrawText(e.Graphics, GetItemText(Items[e.Index]), Font, text,
                    selected ? Theme.Primary : Theme.TextPrimary,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            }

            protected override void WndProc(ref Message m)
            {
                base.WndProc(ref m);
                if (m.Msg == 0x000F) // WM_PAINT
                {
                    using (Graphics graphics = CreateGraphics()) PaintSurface(graphics);
                }
                else if ((m.Msg == 0x0318 || m.Msg == 0x0317) && m.WParam != IntPtr.Zero) // WM_PRINTCLIENT / WM_PRINT
                {
                    using (Graphics graphics = Graphics.FromHdc(m.WParam)) PaintSurface(graphics);
                }
            }

            private void PaintSurface(Graphics graphics)
            {
                if (Width < 2 || Height < 2) return;
                float scale = graphics.DpiX / 96f;
                using (var background = new SolidBrush(Theme.ContainerBackground(this)))
                    graphics.FillRectangle(background, ClientRectangle);
                var previous = graphics.SmoothingMode;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                Rectangle bounds = new Rectangle(1, 1, Width - 3, Height - 3);
                using (var path = Theme.RoundedPath(bounds, (int)(9 * scale)))
                using (var fill = new SolidBrush(Enabled ? Theme.Surface : Theme.SurfaceAlt))
                using (var border = new Pen(Focused || DroppedDown ? Theme.Primary : hover ? Theme.TextMuted : Theme.Border,
                    (Focused || DroppedDown ? 1.5f : 1f) * scale))
                {
                    graphics.FillPath(fill, path);
                    graphics.DrawPath(border, path);
                }
                int arrowX = Width - (int)(18 * scale);
                int centerY = Height / 2;
                int delta = (int)(4 * scale);
                using (var pen = new Pen(Focused || hover ? Theme.Primary : Theme.TextMuted, 1.5f * scale))
                {
                    pen.StartCap = pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                    graphics.DrawLines(pen, new[] { new Point(arrowX - delta, centerY - 2), new Point(arrowX, centerY + 2), new Point(arrowX + delta, centerY - 2) });
                }
                graphics.SmoothingMode = previous;
                TextRenderer.DrawText(graphics, SelectedIndex >= 0 ? GetItemText(SelectedItem) : "Seleccionar...", Font,
                    new Rectangle((int)(11 * scale), 0, Math.Max(1, Width - (int)(40 * scale)), Height),
                    Enabled && SelectedIndex >= 0 ? Theme.TextPrimary : Theme.TextMuted,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            }
        }

        private void ConfigureContactsGrid()
        {
            remoteContacts.StyleModernGrid();
            remoteContacts.BorderStyle = BorderStyle.None;
            remoteContacts.AllowUserToResizeColumns = false;
            remoteContacts.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            remoteContacts.ColumnHeadersHeight = 42;
            remoteContacts.RowTemplate.Height = 54;
            remoteContacts.DefaultCellStyle.Font = Theme.BodyBold;
            remoteContacts.DefaultCellStyle.Padding = new Padding(14, 0, 10, 0);
            remoteContacts.ColumnHeadersDefaultCellStyle.ForeColor = Theme.TextPrimary;
            remoteContacts.ColumnHeadersDefaultCellStyle.SelectionForeColor = Theme.TextPrimary;
            remoteContacts.ColumnHeadersDefaultCellStyle.Padding = new Padding(14, 0, 10, 0);
            remoteContacts.CellPainting += PaintContactAction;
            remoteContacts.CellMouseEnter += (s, e) =>
            {
                remoteContacts.Cursor = e.RowIndex >= 0 && e.ColumnIndex == 1 ? Cursors.Hand : Cursors.Default;
                remoteContacts.Invalidate();
            };
            remoteContacts.CellMouseLeave += (s, e) => remoteContacts.Invalidate();
            remoteContacts.MouseLeave += (s, e) => { remoteContacts.Cursor = Cursors.Default; remoteContacts.Invalidate(); };
            remoteContacts.MouseDown += (s, e) => remoteContacts.Invalidate();
            remoteContacts.MouseUp += (s, e) => remoteContacts.Invalidate();
            remoteContacts.Resize += (s, e) =>
            {
                if (remoteContacts.Width < 1 || remoteContacts.Height < 1) return;
                using (var path = Theme.RoundedPath(remoteContacts.ClientRectangle, Theme.Radius))
                {
                    Region previous = remoteContacts.Region;
                    remoteContacts.Region = new Region(path);
                    previous?.Dispose();
                }
            };
        }

        private void PaintContactAction(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != 1) return;
            e.PaintBackground(e.ClipBounds, true);
            float scale = e.Graphics.DpiX / 96f;
            int inset = (int)(10 * scale);
            int height = Math.Min((int)(34 * scale), e.CellBounds.Height - 12);
            Rectangle button = new Rectangle(e.CellBounds.X + inset,
                e.CellBounds.Y + (e.CellBounds.Height - height) / 2, e.CellBounds.Width - inset * 2, height);
            var contact = remoteContacts.Rows[e.RowIndex].Tag as Dictionary<string, object>;
            bool selected = !string.IsNullOrEmpty(selectedRemoteId) && AlegraWorkflow.Text(contact, "id") == selectedRemoteId;
            Point mouse = remoteContacts.PointToClient(Cursor.Position);
            bool hover = button.Contains(mouse);
            Color fill = selected ? Theme.Lighten(Theme.Success, 0.9f) : Theme.Primary;
            if (hover) fill = Theme.Darken(fill, Control.MouseButtons == MouseButtons.Left ? 0.18f : 0.08f);
            var smoothing = e.Graphics.SmoothingMode;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using (var path = Theme.RoundedPath(button, (int)(9 * scale)))
            using (var brush = new SolidBrush(fill))
                e.Graphics.FillPath(brush, path);
            e.Graphics.SmoothingMode = smoothing;
            TextRenderer.DrawText(e.Graphics, selected ? "Seleccionado" : "Seleccionar", Theme.Small,
                button, selected ? Theme.Darken(Theme.Success, 0.2f) : Color.White,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
            if (remoteContacts.Focused && remoteContacts.CurrentCell == remoteContacts.Rows[e.RowIndex].Cells[e.ColumnIndex])
                ControlPaint.DrawFocusRectangle(e.Graphics, Rectangle.Inflate(button, -4, -4), selected ? Theme.Success : Color.White, fill);
            e.Handled = true;
        }

        private void AddField(FlowLayoutPanel host, string caption, Control input, int width = 500)
        {
            Panel field = new Panel { Width = width, Height = 68, Margin = new Padding(0, 0, 8, 0) };
            Label label = new Label { Tag = caption, Text = caption, Dock = DockStyle.Top, Height = 22 };
            label.StyleLabel(LabelKind.Caption);
            fieldLabels.Add(input, label);
            input.Dock = DockStyle.Top;
            field.Controls.Add(input);
            field.Controls.Add(label);
            field.TabIndex = host.Controls.Count;
            host.Controls.Add(field);
        }

        private static void SelectFiscalOption(ComboBox combo, string code)
        {
            for (int i = 0; i < combo.Items.Count; i++)
            {
                FiscalOption option = combo.Items[i] as FiscalOption;
                if (option != null && string.Equals(option.Code, code, StringComparison.Ordinal))
                {
                    combo.SelectedIndex = i;
                    return;
                }
            }
            combo.SelectedIndex = -1;
        }

        private bool IsRequired(string key)
        {
            bool natural = (personType.SelectedItem as FiscalOption)?.Code == "PERSON_ENTITY";
            bool foreign = documentType.Text == "PP" || documentType.Text == "DIE" || documentType.Text == "FOREIGN_NIT";
            return !(key == "Teléfono" || key == "Segundo nombre"
                || (key == "Dígito de verificación (NIT)" && documentType.Text != "NIT")
                || (key == "Nombre o razón social" && natural)
                || ((key == "Primer nombre" || key == "Apellidos") && !natural)
                || ((key == "Departamento" || key == "Municipio" || key == "Dirección fiscal") && foreign));
        }

        private void UpdateRequiredLabels()
        {
            foreach (var field in fields)
            {
                Label label = fieldLabels[field.Value];
                label.Text = Convert.ToString(label.Tag) + (IsRequired(field.Key) ? " *" : string.Empty);
            }
            foreach (var combo in new[] { personType, documentType, regime })
                fieldLabels[combo].Text = Convert.ToString(fieldLabels[combo].Tag) + " *";
        }

        private void Save()
        {
            if (regime.SelectedIndex < 0) { error.Text = "Seleccione el régimen fiscal del receptor."; regime.Focus(); return; }
            foreach (var field in fields)
            {
                if (!IsRequired(field.Key)) continue;
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
            FiscalData.Add("kindOfPerson", Convert.ToString(personType.SelectedValue));
            FiscalData.Add("regime", Convert.ToString(regime.SelectedValue));
            if(!string.IsNullOrWhiteSpace(selectedRemoteId))FiscalData.Add("remoteClientId",selectedRemoteId);
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
