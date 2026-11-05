using CapaNegocio;
using CapaPresentacion.UI;
using FontAwesome.Sharp;
using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace CapaPresentacion.Formularios.FormsVentas
{
    public class FrmSaleDetails : Form
    {
        private static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("es-CO");
        private readonly FlowLayoutPanel saleBadges = NewBadges();
        private readonly FlowLayoutPanel customerBadges = NewBadges();
        private readonly FlowLayoutPanel totalsBadges = NewBadges();
        private readonly DataGridView productsGrid = NewGrid();
        private readonly DataGridView paymentsGrid = NewGrid();
        private readonly DataGridView ingredientsGrid = NewGrid();
        private readonly Label heading = new Label();
        private readonly Label paymentSummary = new Label();
        private readonly Label notes = new Label();
        private readonly ModernButton printCommand = new ModernButton();
        private readonly ModernButton printInvoice = new ModernButton();
        private readonly ModernButton reload = new ModernButton();
        private DataRow sale;
        private readonly ModernButton electronicPdf=new ModernButton {Text="PDF de Alegra",Width=150,Kind=ButtonKind.Outline};
        private readonly ModernButton electronicPrint=new ModernButton {Text="Imprimir Alegra",Width=175,Kind=ButtonKind.Outline};
        private readonly Label electronicStatus=new Label {AutoSize=false,Height=48,Dock=DockStyle.Bottom,Font=Theme.Small,ForeColor=Theme.TextMuted};
        private readonly Timer electronicTimer=new Timer {Interval=15000};
        public int Id_venta { get; set; }
        public int Id_pedido { get; private set; }

        public FrmSaleDetails()
        {
            Text = "Detalle de venta";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(960, 680);
            ClientSize = new Size(1240, 840);
            BackColor = Theme.Background;
            Font = Theme.Body;
            DoubleBuffered = true;
            BuildLayout();
            Load += (s, e) => LoadSale();
            KeyPreview = true;
            KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) Close(); };
            electronicTimer.Tick+=(s,e)=>UpdateElectronicStatus();electronicTimer.Start();
            FormClosed+=(s,e)=>electronicTimer.Dispose();
        }

        private static FlowLayoutPanel NewBadges()
        {
            FlowLayoutPanel host = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, AutoScroll = true, WrapContents = true,
                BackColor = Theme.Surface, Padding = new Padding(0, 4, 0, 0),
            };
            host.SizeChanged += (s, e) =>
            {
                foreach (Control badge in host.Controls)
                    badge.MaximumSize = new Size(Math.Max(80, host.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - 8), 0);
            };
            return host;
        }

        private static DataGridView NewGrid()
        {
            DataGridView grid = new DataGridView
            {
                Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false,
                AllowUserToDeleteRows = false, MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells,
            };
            grid.StyleGrid();
            grid.DefaultCellStyle.Padding = new Padding(10, 6, 10, 6);
            grid.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            grid.DefaultCellStyle.NullValue = "—";
            grid.DefaultCellStyle.FormatProvider = Culture;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            grid.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.True;
            grid.ColumnHeadersHeight = 40;
            grid.DataBindingComplete += (s, e) => StyleColumns(grid);
            return grid;
        }

        private static ModernCard Section(string title, Control content)
        {
            ModernCard card = new ModernCard { Dock = DockStyle.Fill };
            Label caption = new Label { Text = title, Dock = DockStyle.Top, Height = 30 };
            caption.StyleLabel(LabelKind.Heading);
            card.Controls.Add(content);
            card.Controls.Add(caption);
            return card;
        }

        private void BuildLayout()
        {
            Panel root = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 16, 20, 16) };
            Panel header = new Panel { Dock = DockStyle.Top, Height = 112 };
            header.Controls.Add(electronicStatus);
            heading.Text = "Detalle de venta";
            heading.AutoSize = true;
            heading.StyleLabel(LabelKind.Title);
            header.Controls.Add(heading);
            Label subtitle = new Label { Text = "Factura, productos y pagos registrados", AutoSize = true, Location = new Point(2, 38) };
            subtitle.StyleLabel(LabelKind.Muted);
            header.Controls.Add(subtitle);
            FlowLayoutPanel footer = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom, Height = 54, Padding = new Padding(0, 8, 0, 0),
                FlowDirection = FlowDirection.RightToLeft, WrapContents = false,
            };
            printCommand.Text = "Reimprimir comanda";
            printCommand.Icon = IconChar.Print;
            printCommand.Kind = ButtonKind.Primary;
            printCommand.Width = 222;
            printCommand.Enabled = false;
            printCommand.Click += (s, e) => PrintDocument(true);
            printInvoice.Text = "Reimprimir factura";
            printInvoice.Icon = IconChar.FileInvoiceDollar;
            printInvoice.Kind = ButtonKind.Outline;
            printInvoice.Width = 212;
            printInvoice.Enabled = false;
            printInvoice.Click += (s, e) => PrintDocument(false);
            reload.Text = "Actualizar";
            reload.Icon = IconChar.ArrowsRotate;
            reload.Kind = ButtonKind.Outline;
            reload.Width = 136;
            reload.Click += (s, e) => LoadSale();
            footer.Controls.Add(printCommand);
            footer.Controls.Add(printInvoice);
            footer.Controls.Add(reload);
            electronicPdf.Click+=async(s,e)=>
            {
                electronicPdf.Enabled=false;
                try { string file=await AlegraPdfService.DownloadAsync(Id_venta,System.Threading.CancellationToken.None);System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(file){UseShellExecute=true}); }
                catch(Exception ex){Mensajes.MensajeInformacion(ex is InvalidOperationException?ex.Message:"No se pudo descargar o abrir el PDF de Alegra.");}
                finally {if(!IsDisposed)UpdateElectronicStatus();}
            };
            electronicPrint.Click+=async(s,e)=>
            {
                electronicPrint.Enabled=false;
                try {await AlegraPdfService.PrintAsync(Id_venta,System.Threading.CancellationToken.None,true);}
                catch(Exception ex){Mensajes.MensajeInformacion(ex is InvalidOperationException?ex.Message:"No se pudo imprimir el PDF de Alegra.");}
                finally {if(!IsDisposed)UpdateElectronicStatus();}
            };
            footer.Controls.Add(electronicPdf);footer.Controls.Add(electronicPrint);
            TableLayoutPanel body = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 3 };
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 66));
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));
            body.RowStyles.Add(new RowStyle(SizeType.Absolute, 156));
            body.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            body.RowStyles.Add(new RowStyle(SizeType.Absolute, 156));
            body.Controls.Add(Section("Información de la venta", saleBadges), 0, 0);
            body.Controls.Add(Section("Cliente y entrega", customerBadges), 1, 0);
            TabControl tabs = new TabControl { Dock = DockStyle.Fill, Font = Theme.BodyBold, Padding = new Point(16, 8) };
            TabPage products = new TabPage("Productos, platos y bebidas") { BackColor = Theme.Surface, Padding = new Padding(4) };
            products.Controls.Add(productsGrid);
            TabPage ingredients = new TabPage("Ingredientes y acompañamientos") { BackColor = Theme.Surface, Padding = new Padding(4) };
            ingredients.Controls.Add(ingredientsGrid);
            tabs.TabPages.Add(products);
            tabs.TabPages.Add(ingredients);
            body.Controls.Add(Section("Detalle del pedido", tabs), 0, 1);
            Panel payments = new Panel { Dock = DockStyle.Fill };
            paymentSummary.Dock = DockStyle.Bottom;
            paymentSummary.Height = 48;
            paymentSummary.Font = Theme.BodyBold;
            paymentSummary.Padding = new Padding(0, 6, 0, 0);
            payments.Controls.Add(paymentsGrid);
            payments.Controls.Add(paymentSummary);
            body.Controls.Add(Section("Métodos de pago", payments), 1, 1);
            body.Controls.Add(Section("Valores de la venta · COP", totalsBadges), 0, 2);
            Panel notesHost = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            notes.Dock = DockStyle.Top;
            notes.AutoSize = true;
            notes.Font = Theme.Body;
            notes.ForeColor = Theme.TextPrimary;
            notesHost.SizeChanged += (s, e) => notes.MaximumSize = new Size(Math.Max(1, notesHost.ClientSize.Width - 20), 0);
            notesHost.Controls.Add(notes);
            body.Controls.Add(Section("Observaciones", notesHost), 1, 2);
            root.Controls.Add(body);
            root.Controls.Add(footer);
            root.Controls.Add(header);
            Controls.Add(root);
        }

        private static void ClearBadges(FlowLayoutPanel host)
        {
            foreach (Control control in host.Controls.Cast<Control>().ToArray()) control.Dispose();
        }

        private static void Badge(FlowLayoutPanel host, string caption, string value, Color accent)
        {
            if (string.IsNullOrWhiteSpace(value)) return;
            Label badge = new Label
            {
                AutoSize = true, MaximumSize = new Size(Math.Max(80, host.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - 8), 0),
                Text = caption + " · " + value, Font = caption == "TOTAL FINAL" ? Theme.BodyBold : Theme.Small,
                ForeColor = Theme.Darken(accent, 0.25f), BackColor = Theme.Lighten(accent, 0.93f),
                Padding = new Padding(8, 5, 8, 5), Margin = new Padding(0, 0, 6, 6), UseMnemonic = false,
            };
            badge.Rounded(7);
            host.Controls.Add(badge);
        }

        private string Value(string name) => sale.Table.Columns.Contains(name) && !sale.IsNull(name)
            ? Convert.ToString(sale[name]) : string.Empty;
        private decimal Amount(string name) => decimal.TryParse(Value(name), out decimal amount) ? amount : 0;
        private static string Money(decimal value) => value.ToString("C0", Culture);
        private static string DateText(string value) => DateTime.TryParse(value, out DateTime date)
            ? date.ToString("dd MMM yyyy", Culture) : value;
        private static string TimeText(string value)
        {
            if (TimeSpan.TryParse(value, out TimeSpan time) && time >= TimeSpan.Zero && time < TimeSpan.FromDays(1))
                return DateTime.Today.Add(time).ToString("hh:mm tt", CultureInfo.InvariantCulture);
            return DateTime.TryParse(value, out DateTime date) ? date.ToString("hh:mm tt", CultureInfo.InvariantCulture) : value;
        }

        private void LoadSale()
        {
            printCommand.Enabled = printInvoice.Enabled = false;
            try { RenderSale(NVentas.GetSaleDetails(Id_venta)); }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(Name, "LoadSale", "No se pudo cargar el detalle de la venta", ex.Message);
            }
        }

        private void RenderSale(DataSet data)
        {
            if (data == null || !data.Tables.Contains("Sale") || data.Tables["Sale"].Rows.Count == 0)
                throw new InvalidOperationException("No se encontró la venta seleccionada.");
            sale = data.Tables["Sale"].Rows[0];
            Id_pedido = Convert.ToInt32(sale["Id_pedido"]);
            Id_venta = Convert.ToInt32(sale["Id_venta"]);
            heading.Text = "Venta #" + Id_venta;
            Text = "Detalle de venta #" + Id_venta;
            ClearBadges(saleBadges);
            ClearBadges(customerBadges);
            ClearBadges(totalsBadges);
            Badge(saleBadges, "Pedido", "#" + Id_pedido, Theme.Primary);
            Badge(saleBadges, "Tipo", Value("Tipo_pedido"), Theme.Info);
            if (Value("Tipo_pedido") != "DOMICILIO") Badge(saleBadges, "Mesa", Value("Num_mesa"), Theme.Neutral);
            Badge(saleBadges, "Venta", Value("Estado"), Value("Estado") == "ACTIVO" ? Theme.Success : Theme.Warning);
            Badge(saleBadges, "Fecha", DateText(Value("Fecha_venta")), Theme.Neutral);
            Badge(saleBadges, "Hora", TimeText(Value("Hora_venta")), Theme.Neutral);
            Badge(saleBadges, "Atendido por", Value("Nombre_empleado"), Theme.Primary);
            Badge(saleBadges, "Personas", Value("CantidadClientes"), Theme.Neutral);
            Badge(saleBadges, "Pedido creado", DateText(Value("Fecha_pedido")) + " " + TimeText(Value("Hora_pedido")), Theme.Neutral);
            Badge(saleBadges, "Estado del pedido", Value("Estado_pedido"), Theme.Neutral);
            Badge(customerBadges, "Cliente", Value("Nombre_cliente"), Theme.Primary);
            Badge(customerBadges, "Teléfono", Value("Telefono_cliente"), Theme.Info);
            Badge(customerBadges, "Correo", Value("Correo_electronico"), Theme.Neutral);
            if (Value("Tipo_pedido") == "DOMICILIO")
            {
                Badge(customerBadges, "Dirección", Value("Direccion_entrega"), Theme.Info);
                Badge(customerBadges, "Barrio", Value("Barrio"), Theme.Info);
                Badge(customerBadges, "Alias", Value("Alias_direccion"), Theme.Neutral);
                Badge(customerBadges, "Referencia", Value("Referencia_entrega"), Theme.Neutral);
                Badge(customerBadges, "Contacto entrega", Value("Telefono_entrega"), Theme.Info);
            }
            foreach (var field in new[] { Tuple.Create("Total_parcial", "Productos"), Tuple.Create("Propina", "Propina"),
                Tuple.Create("Desechables", "Desechables"), Tuple.Create("Domicilio", "Domicilio"), Tuple.Create("Subtotal", "Subtotal") })
                Badge(totalsBadges, field.Item2, Money(Amount(field.Item1)), Theme.Neutral);
            // No se inventa la unidad del descuento: el esquema no distingue porcentaje de valor.
            Badge(totalsBadges, "Descuento registrado", Value("Descuento"), Theme.Warning);
            Badge(totalsBadges, "Bono / cupón", Value("Bono_cupon"), Theme.Warning);
            Badge(totalsBadges, "TOTAL FINAL", Money(Amount("Total_final")), Theme.Success);
            notes.Text = "Venta: " + (string.IsNullOrWhiteSpace(Value("Observaciones")) ? "Sin observaciones" : Value("Observaciones"))
                + Environment.NewLine + Environment.NewLine + "Pedido: "
                + (string.IsNullOrWhiteSpace(Value("Observaciones_pedido")) ? "Sin observaciones" : Value("Observaciones_pedido"));
            BindGrid(productsGrid, data.Tables["Products"]);
            BindGrid(paymentsGrid, data.Tables["Payments"]);
            BindGrid(ingredientsGrid, data.Tables["Ingredients"]);
            decimal paid = data.Tables["Payments"].Rows.Cast<DataRow>().Sum(row => Convert.ToDecimal(row["Valor_pago"]));
            paymentSummary.Text = "Total pagado: " + Money(paid) + Environment.NewLine
                + (paid == Amount("Total_final") ? "Pagos completos" : "Diferencia: " + Money(Amount("Total_final") - paid));
            paymentSummary.ForeColor = paid == Amount("Total_final") ? Theme.Success : Theme.Warning;
            printCommand.Enabled = printInvoice.Enabled = true;
            UpdateElectronicStatus();
        }

        private void UpdateElectronicStatus()
        {
            try
            {
                var table=CapaNegocio.Alegra.AlegraWorkflow.Request(Id_venta);
                electronicPdf.Enabled=electronicPrint.Enabled=false;
                if(table.Rows.Count==0){electronicStatus.Text="Sin solicitud de factura electrónica.";return;}
                var request=table.Rows[0];string state=Convert.ToString(request["Status"]);
                electronicStatus.Text="Alegra: "+state+" · Factura "+Convert.ToString(request["InvoiceNumber"])+" · ID "+Convert.ToString(request["RemoteId"])
                    +" · Impresión: "+Convert.ToString(request["PrintStatus"])+Environment.NewLine+Convert.ToString(request["LastError"]);
                electronicPdf.Enabled=state=="ACCEPTED";
                electronicPrint.Enabled=state=="ACCEPTED" && Convert.ToString(request["PrintStatus"])!="PRINTING";
            }
            catch {electronicStatus.Text="Estado Alegra no disponible. Verifique la migración 07.";}
        }

        private static void BindGrid(DataGridView grid, DataTable table)
        {
            grid.DataSource = table;
            StyleColumns(grid);
        }

        private static void StyleColumns(DataGridView grid)
        {
            foreach (DataGridViewColumn column in grid.Columns)
            {
                string name = column.DataPropertyName;
                column.Visible = !name.StartsWith("Id_", StringComparison.OrdinalIgnoreCase);
                column.MinimumWidth = 86;
                column.FillWeight = 100;
                switch (name)
                {
                    case "Nombre": column.HeaderText = "Producto / plato / bebida"; column.MinimumWidth = 230; column.FillWeight = 280; break;
                    case "Nombre_ingrediente": column.HeaderText = "Ingrediente / acompañamiento"; column.MinimumWidth = 220; column.FillWeight = 220; break;
                    case "Plato": column.HeaderText = "Plato"; column.MinimumWidth = 180; column.FillWeight = 180; break;
                    case "Tipo": column.HeaderText = "Tipo"; column.FillWeight = 80; break;
                    case "Precio": column.HeaderText = "Precio unitario"; goto case "Valor_pago";
                    case "Total": goto case "Valor_pago";
                    case "Valor_pago":
                        if (name == "Valor_pago") column.HeaderText = "Valor pagado";
                        column.DefaultCellStyle.Format = "C0";
                        column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                        column.MinimumWidth = 120;
                        if (name != "Precio") { column.DefaultCellStyle.Font = Theme.BodyBold; column.DefaultCellStyle.ForeColor = Theme.Darken(Theme.Success, 0.25f); }
                        break;
                    case "Cantidad": column.HeaderText = "Cant."; column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter; column.FillWeight = 55; break;
                    case "Metodo_pago": column.HeaderText = "Método"; column.DefaultCellStyle.Font = Theme.BodyBold; break;
                    case "Vaucher": column.HeaderText = "Comprobante"; break;
                    case "Observaciones": column.HeaderText = "Observaciones"; column.MinimumWidth = 150; column.FillWeight = 180; break;
                }
            }
        }

        private void PrintDocument(bool command)
        {
            if (sale == null) return;
            printCommand.Enabled = printInvoice.Enabled = false;
            try
            {
                MensajeEspera.ShowWait(command ? "Reimprimiendo comanda..." : "Reimprimiendo factura...");
                if (command)
                {
                    using (FrmComandas report = new FrmComandas { Id_pedido = Id_pedido })
                    {
                        report.ObtenerReporte(Value("Tipo_pedido") == "DOMICILIO" ? "Casa Grande | DOMICILIO" : "Casa Grande");
                        report.AsignarTablas();
                        report.ImprimirFactura(1);
                    }
                }
                else
                {
                    using (FrmFacturaFinal report = new FrmFacturaFinal { Id_pedido = Id_pedido })
                    {
                        report.AsignarReporte();
                        report.AsignarTablasCuentaFinal(Id_venta);
                        report.ImprimirFactura(1);
                    }
                }
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(Name, "PrintDocument", "No se pudo reimprimir el documento", ex.Message);
            }
            finally
            {
                MensajeEspera.CloseForm();
                printCommand.Enabled = printInvoice.Enabled = true;
            }
        }
    }
}
