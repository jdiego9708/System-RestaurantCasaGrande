using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CapaEntidades.Models;
using CapaNegocio;
using CapaPresentacion.UI;
using FontAwesome.Sharp;

namespace CapaPresentacion.Formularios.FormsDomicilios
{
    /// <summary>
    /// Paso previo a un domicilio nuevo, todo en una sola ventana: buscar el cliente escribiendo (nombre o
    /// telefono), crearlo si no existe, y elegir o crear la direccion de entrega.
    ///
    /// Teclado: al abrir, el foco esta en la busqueda; escribir filtra y deja seleccionado el primer cliente;
    /// Enter continua cuando ya hay cliente y direccion; Esc cierra el formulario abierto o la ventana.
    /// </summary>
    public class FrmDeliveryCustomer : Form
    {
        // --- cliente ---
        private readonly ModernTextBox txtSearch = NewInput(IconChar.MagnifyingGlass, "Buscar por nombre o teléfono…");
        private readonly ListBox lstCustomers = NewList(62);
        private readonly Label lblResults = new Label();
        private readonly ModernButton btnNewCustomer = new ModernButton
        {
            Text = "Nuevo cliente", Kind = ButtonKind.Outline, Icon = IconChar.UserPlus,
        };
        private readonly ModernButton btnEditCustomer = new ModernButton
        {
            Text = "Editar", Kind = ButtonKind.Outline, Icon = IconChar.Pen,
        };
        private readonly Panel newCustomerPanel = new Panel { Visible = false };
        private readonly ModernTextBox txtNewName = NewInput(IconChar.User, "Nombre completo");
        private readonly ModernTextBox txtNewPhone = NewInput(IconChar.Phone, "Ej. 3001234567");
        private readonly ModernTextBox txtNewEmail = NewInput(IconChar.Envelope, "correo@ejemplo.com");
        private readonly ModernButton btnSaveCustomer = new ModernButton
        {
            Text = "Guardar cliente", Kind = ButtonKind.Success, Icon = IconChar.Check,
        };
        private readonly ModernButton btnCancelCustomer = new ModernButton { Text = "Volver", Kind = ButtonKind.Outline };

        // --- direccion ---
        private readonly Label lblSelected = new Label();
        private readonly ListBox lstAddresses = NewList(66);
        private readonly Panel addressActions = new Panel();
        private readonly ModernButton btnNewAddress = new ModernButton
        {
            Text = "Nueva", Kind = ButtonKind.Outline, Icon = IconChar.Plus,
        };
        private readonly ModernButton btnEditAddress = new ModernButton
        {
            Text = "Editar", Kind = ButtonKind.Outline, Icon = IconChar.Pen,
        };
        private readonly ModernButton btnRemoveAddress = new ModernButton
        {
            Text = "Quitar", Kind = ButtonKind.Outline, Icon = IconChar.Trash,
        };
        private readonly Panel addressPanel = new Panel { Visible = false };
        private readonly ModernTextBox txtAlias = NewInput(IconChar.Tag, "Casa, Oficina…");
        private readonly ModernTextBox txtAddress = NewInput(IconChar.LocationDot, "Calle, carrera, número…");
        private readonly ModernTextBox txtNeighborhood = NewInput(IconChar.City, "Barrio o sector");
        private readonly ModernTextBox txtReference = NewInput(IconChar.CircleInfo, "Portón, piso, color de la casa…");
        private readonly ModernTextBox txtContactPhone = NewInput(IconChar.Phone, "Teléfono para la entrega");
        private readonly CheckBox chkPrimary = new CheckBox { Text = "Dirección principal", AutoSize = true };
        private readonly ModernButton btnSaveAddress = new ModernButton
        {
            Text = "Guardar dirección", Kind = ButtonKind.Success, Icon = IconChar.Check,
        };
        private readonly ModernButton btnCancelAddress = new ModernButton { Text = "Volver", Kind = ButtonKind.Outline };

        // --- pie ---
        private readonly ModernButton btnContinue = new ModernButton
        {
            Text = "CONTINUAR", Kind = ButtonKind.Success, Icon = IconChar.ArrowRight,
        };
        private readonly ModernButton btnCancel = new ModernButton { Text = "Cancelar", Kind = ButtonKind.Outline };

        private readonly Timer searchTimer = new Timer { Interval = 250 };
        private CustomerAddress editingAddress;
        private Clientes editingCustomer;
        private bool loading;

        public FrmDeliveryCustomer()
        {
            this.Text = "Nuevo domicilio";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ClientSize = new Size(1040, 640);
            this.KeyPreview = true;
            this.BackColor = Theme.Background;
            this.Font = Theme.Body;

            Panel root = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 16, 20, 16) };
            Control body = this.BuildBody();
            Control footer = this.BuildFooter();
            Control header = this.BuildHeader();
            root.Controls.Add(body);
            root.Controls.Add(footer);
            root.Controls.Add(header);
            SetTabOrder(header, body, footer);
            this.Controls.Add(root);

            this.WireEvents();
            this.UpdateState();
        }

        public Clientes Customer { get; private set; }

        public CustomerAddress SelectedAddress { get; private set; }

        #region CONSTRUCCION
        // El orden de agregado por Dock no coincide con el recorrido visual del teclado.
        private static void SetTabOrder(params Control[] controls)
        {
            for (int i = 0; i < controls.Length; i++)
                controls[i].TabIndex = i;
        }

        private static ModernTextBox NewInput(IconChar icon, string placeholder) => new ModernTextBox
        {
            Icon = icon,
            PlaceholderText = placeholder,
            Dock = DockStyle.Top,
        };

        private static ListBox NewList(int itemHeight) => new ListBox
        {
            Dock = DockStyle.Fill,
            DrawMode = DrawMode.OwnerDrawFixed,
            ItemHeight = itemHeight,
            BorderStyle = BorderStyle.None,
            BackColor = Theme.Surface,
            IntegralHeight = false,
        };

        private Control BuildHeader()
        {
            Panel header = new Panel { Dock = DockStyle.Top, Height = 64 };
            Label title = new Label { Text = "Nuevo domicilio", AutoSize = true, Location = new Point(0, 0) };
            title.StyleLabel(LabelKind.Title);
            Label subtitle = new Label
            {
                Text = "Busca al cliente por nombre o teléfono y elige la dirección de entrega",
                AutoSize = true,
                Location = new Point(2, 36),
            };
            subtitle.StyleLabel(LabelKind.Muted);
            header.Controls.Add(title);
            header.Controls.Add(subtitle);
            return header;
        }

        private Control BuildFooter()
        {
            Panel footer = new Panel { Dock = DockStyle.Bottom, Height = 64, Padding = new Padding(0, 14, 0, 0) };
            this.btnContinue.Size = new Size(220, 46);
            this.btnContinue.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.btnCancel.Size = new Size(130, 46);
            this.btnCancel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            footer.Controls.Add(this.btnContinue);
            footer.Controls.Add(this.btnCancel);
            SetTabOrder(this.btnCancel, this.btnContinue);
            footer.SizeChanged += (s, e) =>
            {
                this.btnContinue.Location = new Point(footer.Width - this.btnContinue.Width, 14);
                this.btnCancel.Location = new Point(this.btnContinue.Left - this.btnCancel.Width - 12, 14);
            };
            return footer;
        }

        private Control BuildBody()
        {
            TableLayoutPanel body = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
            Control customerCard = this.BuildCustomerCard();
            Control addressCard = this.BuildAddressCard();
            body.Controls.Add(customerCard, 0, 0);
            body.Controls.Add(addressCard, 1, 0);
            SetTabOrder(customerCard, addressCard);
            return body;
        }

        private static Label StepLabel(string text)
        {
            Label label = new Label { Text = text, Dock = DockStyle.Top, Height = 34 };
            label.StyleLabel(LabelKind.Heading);
            return label;
        }

        private static Control Field(string caption, Control input)
        {
            Panel host = new Panel { Dock = DockStyle.Top, Height = 74 };
            Label label = new Label { Text = caption, Dock = DockStyle.Top, Height = 22 };
            label.StyleLabel(LabelKind.Caption);
            input.Dock = DockStyle.Top;
            host.Controls.Add(input);
            host.Controls.Add(label);
            input.TabIndex = 0;
            return host;
        }

        // Dos campos lado a lado en una misma fila.
        private static Control Row(Control left, Control right)
        {
            TableLayoutPanel row = new TableLayoutPanel { Dock = DockStyle.Top, Height = 74, ColumnCount = 2, RowCount = 1 };
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            left.Dock = DockStyle.Fill;
            left.Margin = new Padding(0, 0, 8, 0);
            right.Dock = DockStyle.Fill;
            right.Margin = new Padding(8, 0, 0, 0);
            row.Controls.Add(left, 0, 0);
            row.Controls.Add(right, 1, 0);
            SetTabOrder(left, right);
            return row;
        }

        // Marca en rojo los obligatorios vacios y lleva el foco al primero; sin ventanas emergentes.
        private static bool Require(params ModernTextBox[] boxes)
        {
            ModernTextBox first = null;
            foreach (ModernTextBox box in boxes)
            {
                if (box.Text.Trim().Length == 0)
                {
                    box.HasError = true;
                    if (first == null)
                        first = box;
                }
            }
            first?.Focus();
            return first == null;
        }

        // Los campos se apilan con Dock = Top: el ultimo agregado queda arriba, por eso se agregan al reves.
        private static void StackFields(Panel panel, params Control[] fieldsTopToBottom)
        {
            for (int i = fieldsTopToBottom.Length - 1; i >= 0; i--)
            {
                fieldsTopToBottom[i].TabIndex = i;
                panel.Controls.Add(fieldsTopToBottom[i]);
            }
        }

        private Control BuildCustomerCard()
        {
            ModernCard card = new ModernCard { Dock = DockStyle.Fill, AccentColor = Theme.Primary };

            // Resultados de la busqueda.
            Panel results = new Panel { Dock = DockStyle.Fill };
            this.lblResults.Dock = DockStyle.Top;
            this.lblResults.Height = 26;
            this.lblResults.StyleLabel(LabelKind.Caption);
            results.Controls.Add(this.lstCustomers);
            results.Controls.Add(this.lblResults);

            // Alta rapida de cliente (reemplaza a los resultados mientras esta abierta).
            this.btnSaveCustomer.Size = new Size(170, 40);
            this.btnCancelCustomer.Size = new Size(90, 40);
            Panel customerButtons = new Panel { Dock = DockStyle.Top, Height = 56, Padding = new Padding(0, 10, 0, 0) };
            this.btnSaveCustomer.Location = new Point(0, 10);
            this.btnCancelCustomer.Location = new Point(180, 10);
            customerButtons.Controls.Add(this.btnSaveCustomer);
            customerButtons.Controls.Add(this.btnCancelCustomer);
            SetTabOrder(this.btnSaveCustomer, this.btnCancelCustomer);
            this.newCustomerPanel.Dock = DockStyle.Fill;
            StackFields(this.newCustomerPanel,
                Field("Nombre *", this.txtNewName),
                Field("Teléfono *", this.txtNewPhone),
                Field("Correo (opcional)", this.txtNewEmail),
                customerButtons);

            Panel buttonRow = new Panel { Dock = DockStyle.Bottom, Height = 52, Padding = new Padding(0, 8, 0, 0) };
            this.btnNewCustomer.Size = new Size(170, 40);
            this.btnNewCustomer.Location = new Point(0, 10);
            this.btnEditCustomer.Size = new Size(120, 40);
            this.btnEditCustomer.Location = new Point(180, 10);
            buttonRow.Controls.Add(this.btnNewCustomer);
            buttonRow.Controls.Add(this.btnEditCustomer);
            SetTabOrder(this.btnNewCustomer, this.btnEditCustomer);

            Panel searchHost = new Panel { Dock = DockStyle.Top, Height = 54 };
            this.txtSearch.Dock = DockStyle.Top;
            searchHost.Controls.Add(this.txtSearch);

            card.Controls.Add(this.newCustomerPanel);
            card.Controls.Add(results);
            card.Controls.Add(buttonRow);
            card.Controls.Add(searchHost);
            card.Controls.Add(StepLabel("1 · Cliente"));
            SetTabOrder(searchHost, results, this.newCustomerPanel, buttonRow);
            return card;
        }

        private Control BuildAddressCard()
        {
            ModernCard card = new ModernCard { Dock = DockStyle.Fill, AccentColor = Theme.Success };

            this.lblSelected.Dock = DockStyle.Top;
            this.lblSelected.Height = 40;
            this.lblSelected.StyleLabel(LabelKind.BodyBold);
            this.lblSelected.Text = "Selecciona un cliente";

            Panel list = new Panel { Dock = DockStyle.Fill };
            list.Controls.Add(this.lstAddresses);

            this.addressActions.Dock = DockStyle.Bottom;
            this.addressActions.Height = 52;
            foreach (ModernButton b in new[] { this.btnNewAddress, this.btnEditAddress, this.btnRemoveAddress })
                b.Size = new Size(120, 40);
            this.btnNewAddress.Location = new Point(0, 10);
            this.btnEditAddress.Location = new Point(130, 10);
            this.btnRemoveAddress.Location = new Point(260, 10);
            this.addressActions.Controls.Add(this.btnNewAddress);
            this.addressActions.Controls.Add(this.btnEditAddress);
            this.addressActions.Controls.Add(this.btnRemoveAddress);
            SetTabOrder(this.btnNewAddress, this.btnEditAddress, this.btnRemoveAddress);

            // Formulario de direccion (reemplaza a la lista mientras esta abierto).
            this.btnSaveAddress.Size = new Size(190, 40);
            this.btnCancelAddress.Size = new Size(90, 40);
            Panel addressButtons = new Panel { Dock = DockStyle.Top, Height = 56, Padding = new Padding(0, 10, 0, 0) };
            this.btnSaveAddress.Location = new Point(0, 10);
            this.btnCancelAddress.Location = new Point(200, 10);
            addressButtons.Controls.Add(this.btnSaveAddress);
            addressButtons.Controls.Add(this.btnCancelAddress);
            SetTabOrder(this.btnSaveAddress, this.btnCancelAddress);
            // La casilla "principal" ocupa el lugar de un campo, alineada con las cajas de su fila.
            Panel primaryHost = new Panel { Dock = DockStyle.Fill };
            this.chkPrimary.Location = new Point(0, 40);
            primaryHost.Controls.Add(this.chkPrimary);
            this.addressPanel.Dock = DockStyle.Fill;
            StackFields(this.addressPanel,
                Field("Dirección *", this.txtAddress),
                Row(Field("Barrio", this.txtNeighborhood), Field("Alias", this.txtAlias)),
                Field("Referencia", this.txtReference),
                Row(Field("Teléfono de contacto", this.txtContactPhone), primaryHost),
                addressButtons);

            card.Controls.Add(this.addressPanel);
            card.Controls.Add(list);
            card.Controls.Add(this.addressActions);
            card.Controls.Add(this.lblSelected);
            card.Controls.Add(StepLabel("2 · Dirección de entrega"));
            SetTabOrder(list, this.addressPanel, this.addressActions);
            return card;
        }
        #endregion

        #region EVENTOS
        private void WireEvents()
        {
            this.Shown += (s, e) =>
            {
                this.txtSearch.Focus();
                this.RunSearch();
            };
            this.FormClosed += (s, e) => this.searchTimer.Dispose();

            // Busqueda en vivo con una pequena espera para no consultar en cada tecla.
            this.txtSearch.TextChanged += (s, e) => { this.searchTimer.Stop(); this.searchTimer.Start(); };
            this.searchTimer.Tick += (s, e) => { this.searchTimer.Stop(); this.RunSearch(); };
            this.txtSearch.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Down && this.lstCustomers.Items.Count > 0)
                {
                    this.lstCustomers.Focus();
                    e.Handled = true;
                }
            };

            this.lstCustomers.DrawItem += this.DrawCustomer;
            this.lstCustomers.SelectedIndexChanged += (s, e) =>
            {
                if (!this.loading && this.lstCustomers.SelectedItem is Clientes picked)
                    this.SelectCustomer(picked);
            };

            this.lstAddresses.DrawItem += this.DrawAddress;
            this.lstAddresses.SelectedIndexChanged += (s, e) => this.UpdateState();
            this.lstAddresses.DoubleClick += (s, e) => this.Accept();

            this.btnNewCustomer.Click += (s, e) => this.ShowNewCustomer(true);
            this.btnEditCustomer.Click += (s, e) => this.ShowNewCustomer(true, this.Customer);
            this.btnCancelCustomer.Click += (s, e) => this.ShowNewCustomer(false);
            this.btnSaveCustomer.Click += (s, e) => this.SaveCustomer();

            this.btnNewAddress.Click += (s, e) => this.ShowAddressForm(new CustomerAddress
            {
                CustomerId = this.Customer.Id_cliente,
                ContactPhone = this.Customer.Telefono_cliente,
            });
            this.btnEditAddress.Click += (s, e) => this.ShowAddressForm(this.lstAddresses.SelectedItem as CustomerAddress);
            this.btnRemoveAddress.Click += (s, e) => this.RemoveAddress();
            this.btnSaveAddress.Click += (s, e) => this.SaveAddress();
            this.btnCancelAddress.Click += (s, e) => this.HideAddressForm();

            this.btnContinue.Click += (s, e) => this.Accept();
            this.btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            // Esc cierra primero el formulario abierto y, si no hay ninguno, la ventana.
            this.KeyDown += (s, e) =>
            {
                if (e.KeyCode != Keys.Escape)
                    return;
                e.Handled = true;
                if (this.addressPanel.Visible)
                    this.HideAddressForm();
                else if (this.newCustomerPanel.Visible)
                    this.ShowNewCustomer(false);
                else
                {
                    this.DialogResult = DialogResult.Cancel;
                    this.Close();
                }
            };
        }

        // Enter hace lo que toca segun lo que este abierto.
        private void UpdateState()
        {
            bool hasAddress = this.Customer != null && this.lstAddresses.SelectedItem != null;
            bool editing = this.addressPanel.Visible || this.newCustomerPanel.Visible;

            this.btnContinue.Enabled = hasAddress && !editing;
            this.btnEditCustomer.Enabled = this.Customer != null;
            this.btnNewAddress.Enabled = this.Customer != null;
            this.btnEditAddress.Enabled = hasAddress;
            this.btnRemoveAddress.Enabled = hasAddress;

            if (this.addressPanel.Visible)
                this.AcceptButton = this.btnSaveAddress;
            else if (this.newCustomerPanel.Visible)
                this.AcceptButton = this.btnSaveCustomer;
            else
                this.AcceptButton = this.btnContinue;
        }
        #endregion

        #region CLIENTES
        private void RunSearch()
        {
            string text = this.txtSearch.Text.Trim();
            DataTable table = NClientes.SearchQuick(text);

            this.loading = true;
            this.lstCustomers.BeginUpdate();
            this.lstCustomers.Items.Clear();
            foreach (DataRow row in table.Rows)
                this.lstCustomers.Items.Add(new Clientes(row));
            this.lstCustomers.EndUpdate();
            this.loading = false;

            int count = this.lstCustomers.Items.Count;
            this.lblResults.Text = text.Length == 0
                ? "Clientes recientes"
                : count == 0 ? "Sin resultados: puedes crear el cliente" : count + (count == 1 ? " resultado" : " resultados");

            // Con texto escrito se deja elegido el primero: lo normal es teclear el telefono y dar Enter.
            if (text.Length > 0 && count > 0)
                this.lstCustomers.SelectedIndex = 0;
            else if (text.Length > 0)
                this.ClearCustomer();
        }

        private void ClearCustomer()
        {
            this.Customer = null;
            this.lblSelected.Text = "Selecciona un cliente";
            this.lstAddresses.Items.Clear();
            this.UpdateState();
        }

        private void SelectCustomer(Clientes customer)
        {
            this.Customer = customer;
            this.lblSelected.Text = customer.Nombre_cliente + "   ·   " + customer.Telefono_cliente;
            this.LoadAddresses(null);

            // Un cliente sin direcciones va directo al formulario de direccion.
            if (this.lstAddresses.Items.Count == 0)
            {
                this.ShowAddressForm(new CustomerAddress
                {
                    CustomerId = customer.Id_cliente,
                    ContactPhone = customer.Telefono_cliente,
                });
            }
        }

        // Con <paramref name="edit"/> el mismo formulario edita nombre, telefono y correo de ese cliente.
        private void ShowNewCustomer(bool show, Clientes edit = null)
        {
            this.newCustomerPanel.Visible = show;
            this.lstCustomers.Parent.Visible = !show;
            this.btnNewCustomer.Visible = !show;
            this.btnEditCustomer.Visible = !show;
            this.editingCustomer = show ? edit : null;
            this.btnSaveCustomer.Text = edit != null ? "Guardar cambios" : "Guardar cliente";

            if (show && edit != null)
            {
                this.txtNewName.Text = edit.Nombre_cliente;
                this.txtNewPhone.Text = edit.Telefono_cliente;
                this.txtNewEmail.Text = edit.Correo_electronico;
                this.txtNewName.Focus();
            }
            else if (show)
            {
                // Lo escrito en la busqueda se aprovecha: si parece telefono va al telefono; si no, al nombre.
                string typed = this.txtSearch.Text.Trim();
                bool isPhone = typed.Length > 0 && typed.Replace(" ", string.Empty).All(char.IsDigit);
                this.txtNewName.Text = isPhone ? string.Empty : typed;
                this.txtNewPhone.Text = isPhone ? typed : string.Empty;
                this.txtNewEmail.Text = string.Empty;
                (isPhone ? this.txtNewName : this.txtNewPhone).Focus();
            }
            else
                this.txtSearch.Focus();
            this.UpdateState();
        }

        // Guarda los cambios del cliente elegido y deja la lista y el encabezado con los datos nuevos.
        private void UpdateCustomer(string name, string phone)
        {
            int id = this.editingCustomer.Id_cliente;

            // Otro cliente con ese mismo telefono seria un duplicado.
            foreach (DataRow row in NClientes.SearchQuick(phone).Rows)
            {
                if (Convert.ToInt32(row["Id_cliente"]) != id
                    && string.Equals(Convert.ToString(row["Telefono_cliente"]).Trim(), phone, StringComparison.Ordinal))
                {
                    this.txtNewPhone.HasError = true;
                    this.txtNewPhone.Focus();
                    Mensajes.MensajeInformacion("Ese teléfono ya pertenece a otro cliente: "
                        + Convert.ToString(row["Nombre_cliente"]));
                    return;
                }
            }

            string result = NClientes.UpdateCustomer(id, name, phone, this.txtNewEmail.Text);
            if (!result.Equals("OK"))
            {
                Mensajes.MensajeErrorForm(result);
                return;
            }

            DataTable updated = NClientes.GetById(id);
            this.ShowNewCustomer(false);
            if (updated == null)
                return;

            Clientes customer = new Clientes(updated.Rows[0]);
            // Se reemplaza el elemento de la lista sin disparar otra seleccion (no hay que recargar direcciones).
            this.loading = true;
            int index = this.lstCustomers.SelectedIndex;
            if (index >= 0)
                this.lstCustomers.Items[index] = customer;
            this.loading = false;
            this.Customer = customer;
            this.lblSelected.Text = customer.Nombre_cliente + "   ·   " + customer.Telefono_cliente;
        }

        private void SaveCustomer()
        {
            if (!Require(this.txtNewName, this.txtNewPhone))
                return;
            string name = this.txtNewName.Text.Trim();
            string phone = this.txtNewPhone.Text.Trim();

            if (this.editingCustomer != null)
            {
                this.UpdateCustomer(name, phone);
                return;
            }

            // Si el telefono ya existe se usa ese cliente en lugar de duplicarlo.
            if (phone.Length > 0)
            {
                foreach (DataRow row in NClientes.SearchQuick(phone).Rows)
                {
                    if (string.Equals(Convert.ToString(row["Telefono_cliente"]).Trim(), phone, StringComparison.Ordinal))
                    {
                        Clientes found = new Clientes(row);
                        Mensajes.MensajeInformacion("Ya existe un cliente con ese teléfono: " + found.Nombre_cliente
                            + ". Se seleccionó ese cliente.");
                        this.ShowNewCustomer(false);
                        this.SelectCustomer(found);
                        return;
                    }
                }
            }

            string result = NClientes.InsertCustomer(name, phone, this.txtNewEmail.Text, out int customerId);
            if (!result.Equals("OK"))
            {
                Mensajes.MensajeErrorForm(result);
                return;
            }

            DataTable created = NClientes.GetById(customerId);
            this.ShowNewCustomer(false);
            if (created != null)
                this.SelectCustomer(new Clientes(created.Rows[0]));
        }
        #endregion

        #region DIRECCIONES
        private void LoadAddresses(int? selectId)
        {
            this.lstAddresses.BeginUpdate();
            this.lstAddresses.Items.Clear();
            foreach (CustomerAddress address in NAddresses.ListByCustomer(this.Customer.Id_cliente))
            {
                int index = this.lstAddresses.Items.Add(address);
                if (selectId.HasValue ? address.AddressId == selectId.Value : address.IsPrimary)
                    this.lstAddresses.SelectedIndex = index;
            }
            if (this.lstAddresses.SelectedIndex < 0 && this.lstAddresses.Items.Count > 0)
                this.lstAddresses.SelectedIndex = 0;
            this.lstAddresses.EndUpdate();
            this.UpdateState();
        }

        private void ShowAddressForm(CustomerAddress address)
        {
            if (address == null)
                return;
            this.editingAddress = address;
            this.txtAlias.Text = address.Alias;
            this.txtAddress.Text = address.Address;
            this.txtNeighborhood.Text = address.Neighborhood;
            this.txtReference.Text = address.Reference;
            this.txtContactPhone.Text = address.ContactPhone;
            this.chkPrimary.Checked = address.IsPrimary;

            this.addressPanel.Visible = true;
            this.lstAddresses.Parent.Visible = false;
            this.addressActions.Visible = false;
            this.txtAddress.Focus();
            this.UpdateState();
        }

        private void HideAddressForm()
        {
            this.addressPanel.Visible = false;
            this.lstAddresses.Parent.Visible = true;
            this.addressActions.Visible = true;
            this.editingAddress = null;
            this.lstAddresses.Focus();
            this.UpdateState();
        }

        private void SaveAddress()
        {
            if (!Require(this.txtAddress))
                return;
            CustomerAddress address = this.editingAddress;
            address.Alias = this.txtAlias.Text;
            address.Address = this.txtAddress.Text;
            address.Neighborhood = this.txtNeighborhood.Text;
            address.Reference = this.txtReference.Text;
            address.ContactPhone = this.txtContactPhone.Text;
            address.IsPrimary = this.chkPrimary.Checked;

            string result = NAddresses.Save(address);
            if (!result.Equals("OK"))
            {
                Mensajes.MensajeErrorForm(result);
                this.txtAddress.Focus();
                return;
            }

            this.HideAddressForm();
            this.LoadAddresses(address.AddressId);
            // Direccion lista: el siguiente Enter continua con el pedido.
            this.btnContinue.Focus();
        }

        private void RemoveAddress()
        {
            CustomerAddress address = this.lstAddresses.SelectedItem as CustomerAddress;
            if (address == null)
                return;
            Mensajes.MensajePregunta("¿Quitar esta dirección del cliente?", "Si", "No", out DialogResult answer);
            if (answer != DialogResult.Yes)
                return;
            string result = NAddresses.Deactivate(address.AddressId);
            if (!result.Equals("OK"))
                Mensajes.MensajeErrorForm(result);
            this.LoadAddresses(null);
            if (this.lstAddresses.Items.Count == 0)
                this.ShowAddressForm(new CustomerAddress { CustomerId = this.Customer.Id_cliente });
        }

        private void Accept()
        {
            this.SelectedAddress = this.lstAddresses.SelectedItem as CustomerAddress;
            if (this.Customer == null || this.SelectedAddress == null
                || this.addressPanel.Visible || this.newCustomerPanel.Visible)
                return;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        #endregion

        #region DIBUJO DE LAS LISTAS
        private static void DrawRowBackground(DrawItemEventArgs e)
        {
            bool selected = (e.State & DrawItemState.Selected) != 0;
            using (SolidBrush back = new SolidBrush(selected ? Theme.Lighten(Theme.Primary, 0.88f) : Theme.Surface))
                e.Graphics.FillRectangle(back, e.Bounds);
            using (Pen line = new Pen(Theme.Border))
                e.Graphics.DrawLine(line, e.Bounds.Left + 8, e.Bounds.Bottom - 1, e.Bounds.Right - 8, e.Bounds.Bottom - 1);
            if (selected)
            {
                using (SolidBrush bar = new SolidBrush(Theme.Primary))
                    e.Graphics.FillRectangle(bar, e.Bounds.Left, e.Bounds.Top, 4, e.Bounds.Height - 1);
            }
        }

        private const TextFormatFlags Flags = TextFormatFlags.Left | TextFormatFlags.NoPadding
            | TextFormatFlags.EndEllipsis | TextFormatFlags.Top;

        private void DrawCustomer(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0)
                return;
            Clientes customer = (Clientes)this.lstCustomers.Items[e.Index];
            DrawRowBackground(e);
            Rectangle b = e.Bounds;
            TextRenderer.DrawText(e.Graphics, customer.Nombre_cliente, Theme.BodyBold,
                new Rectangle(b.X + 14, b.Y + 9, b.Width - 28, 22), Theme.TextPrimary, Flags);
            TextRenderer.DrawText(e.Graphics, customer.Telefono_cliente, Theme.Small,
                new Rectangle(b.X + 14, b.Y + 33, b.Width - 28, 18), Theme.TextMuted, Flags);
        }

        private void DrawAddress(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0)
                return;
            CustomerAddress address = (CustomerAddress)this.lstAddresses.Items[e.Index];
            DrawRowBackground(e);
            Rectangle b = e.Bounds;

            string title = string.IsNullOrWhiteSpace(address.Alias) ? "Dirección" : address.Alias;
            TextRenderer.DrawText(e.Graphics, title, Theme.BodyBold,
                new Rectangle(b.X + 14, b.Y + 7, b.Width - 130, 22), Theme.TextPrimary, Flags);

            if (address.IsPrimary)
            {
                Rectangle chip = new Rectangle(b.Right - 104, b.Y + 9, 90, 20);
                using (SolidBrush fill = new SolidBrush(Theme.Lighten(Theme.Success, 0.82f)))
                    e.Graphics.FillRectangle(fill, chip);
                TextRenderer.DrawText(e.Graphics, "PRINCIPAL", Theme.Small, chip, Theme.Success,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            }

            string line2 = string.IsNullOrWhiteSpace(address.Neighborhood)
                ? address.Address : address.Address + "  ·  " + address.Neighborhood;
            TextRenderer.DrawText(e.Graphics, line2, Theme.Body,
                new Rectangle(b.X + 14, b.Y + 29, b.Width - 28, 20), Theme.TextPrimary, Flags);
            if (!string.IsNullOrWhiteSpace(address.Reference))
                TextRenderer.DrawText(e.Graphics, address.Reference, Theme.Small,
                    new Rectangle(b.X + 14, b.Y + 48, b.Width - 28, 16), Theme.TextMuted, Flags);
        }
        #endregion
    }
}
