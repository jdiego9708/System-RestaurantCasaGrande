using CapaNegocio.Alegra;
using CapaPresentacion.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace CapaPresentacion.Formularios.FormsVentas
{
    public sealed class FrmAlegraSetup : Form
    {
        private sealed class Option { public string Id { get; set; } public string Name { get; set; } }
        private readonly ComboBox templates=Combo(), cash=Combo(), transfer=Combo(), card=Combo(), delivery=Combo(), packaging=Combo();
        private readonly ComboBox fiscal=new ComboBox { DropDownStyle=ComboBoxStyle.DropDownList };
        private readonly CheckBox enabled=new CheckBox { Text="Activar emisión electrónica automática (producción)",AutoSize=true };
        private readonly CheckBox autoPrint=new CheckBox { Text="Imprimir tambien la factura electronica de 80 mm automaticamente",AutoSize=true };
        private readonly ModernTextBox pdfExecutable=new ModernTextBox(), printer=new ModernTextBox(),tipId=new ModernTextBox();
        private readonly Label status=new Label { AutoSize=true,MaximumSize=new Size(900,0) };
        private readonly DataGridView mappings=new DataGridView();
        private readonly CancellationTokenSource lifetime=new CancellationTokenSource();
        private readonly Dictionary<string,Dictionary<string,object>> remoteItems=new Dictionary<string,Dictionary<string,object>>();
        private readonly Dictionary<string,decimal> taxRates=new Dictionary<string,decimal>();
        private AlegraInvoiceConfiguration settings;
        private string companyId;
        private static ComboBox Combo()=>new ComboBox { DropDownStyle=ComboBoxStyle.DropDownList,DisplayMember="Name",ValueMember="Id",Width=550 };

        public FrmAlegraSetup()
        {
            Text="Configuración Alegra"; ClientSize=new Size(1040,720); MinimumSize=new Size(900,600);
            BackColor=Theme.Background; Font=Theme.Body; StartPosition=FormStartPosition.CenterParent;
            settings=AlegraWorkflow.Settings();
            Panel root=new Panel { Dock=DockStyle.Fill,Padding=new Padding(20) };
            TabControl tabs=new TabControl { Dock=DockStyle.Fill };
            TabPage general=new TabPage("Cuenta, pagos e impresión"), products=new TabPage("Vincular platos y bebidas");
            FlowLayoutPanel fields=new FlowLayoutPanel { Dock=DockStyle.Fill,AutoScroll=true,FlowDirection=FlowDirection.TopDown,WrapContents=false };
            ModernButton load=new ModernButton { Text="Conectar y cargar catálogos",Width=300,Kind=ButtonKind.Primary };
            load.Click+=async(s,e)=>
            {
                load.Enabled=false;
                try { await LoadCatalogs(); status.Text="Catálogos cargados. Seleccione la numeración electrónica y vincule los productos."; }
                catch(Exception ex) { status.Text=ex is InvalidOperationException ? ex.Message : "No se pudieron cargar los catálogos. Revise las credenciales y la conexión."; }
                finally { if(!IsDisposed) load.Enabled=true; }
            };
            fields.Controls.Add(load); fields.Controls.Add(status);
            Add(fields,"Numeración para factura electrónica",templates);
            Add(fields,"Cuenta receptora de efectivo",cash); Add(fields,"Cuenta receptora de transferencias",transfer); Add(fields,"Cuenta receptora de tarjetas",card);
            fiscal.Items.AddRange(new object[]{"CASH","CREDIT_TRANSFER","CREDIT_CARD","DEBIT_CARD","MUTUAL_AGREEMENT"});
            Add(fields,"Medio fiscal para pagos combinados (catálogo Colombia)",fiscal);
            Add(fields,"Producto de Alegra para el cobro del domicilio",delivery); Add(fields,"Producto de Alegra para desechables",packaging);
            Add(fields,"ID del cargo de propina configurado en Alegra (si se cobra)",tipId);
            Add(fields,"SumatraPDF.exe (vacio: usar el incluido con la aplicacion)",pdfExecutable);
            Add(fields,"Impresora Windows (vacio: predeterminada, igual que la factura local)",printer);
            fields.Controls.Add(autoPrint); fields.Controls.Add(enabled);
            general.Controls.Add(fields);
            mappings.Dock=DockStyle.Fill; mappings.AllowUserToAddRows=false; mappings.AllowUserToDeleteRows=false;
            mappings.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill; mappings.StyleGrid();
            mappings.DataError+=(s,e)=>{e.ThrowException=false;status.Text="Revise una vinculación que ya no existe en el catálogo remoto.";};
            products.Controls.Add(mappings);
            tabs.TabPages.Add(general); tabs.TabPages.Add(products);
            FlowLayoutPanel footer=new FlowLayoutPanel { Dock=DockStyle.Bottom,Height=52,FlowDirection=FlowDirection.RightToLeft };
            ModernButton save=new ModernButton { Text="Guardar configuración",Width=235,Kind=ButtonKind.Success };
            save.Click+=(s,e)=>Save(); footer.Controls.Add(save);
            root.Controls.Add(tabs);root.Controls.Add(footer); Controls.Add(root);
            enabled.Checked=settings.Enabled;autoPrint.Checked=settings.AutoPrint;
            pdfExecutable.Text=settings.PdfPrinterExecutable;printer.Text=settings.PrinterName;tipId.Text=settings.TipChargeId;
            fiscal.SelectedItem=settings.FiscalPaymentMethod;
            FormClosed+=(s,e)=>lifetime.Cancel();
        }

        private static void Add(FlowLayoutPanel host,string caption,Control input)
        {
            Panel panel=new Panel { Width=900,Height=70 };
            Label label=new Label { Text=caption,Dock=DockStyle.Top,Height=24 };label.StyleLabel(LabelKind.Caption);
            input.Dock=DockStyle.Top;panel.Controls.Add(input);panel.Controls.Add(label);host.Controls.Add(panel);
        }
        private static List<Option> Options(List<Dictionary<string,object>> values)=>values.Select(value=>new Option {
            Id=AlegraWorkflow.Text(value,"id"),Name=AlegraWorkflow.Text(value,"name")+" "+AlegraWorkflow.Text(value,"prefix")+" ["+AlegraWorkflow.Text(value,"id")+"]" }).ToList();
        private static void Bind(ComboBox combo,List<Option> options,string selected)
        {
            combo.DataSource=new List<Option>(new[]{new Option {Id="",Name="Seleccione…"}}.Concat(options));
            combo.SelectedValue=selected??"";
        }
        private async System.Threading.Tasks.Task LoadCatalogs()
        {
            using(var api=new AlegraClient())
            {
                var company=AlegraWorkflow.Object(await api.GetCompanyAsync(lifetime.Token));
                companyId=AlegraWorkflow.CompanyIdentity(company);
                if(string.IsNullOrWhiteSpace(companyId))throw new InvalidOperationException("La cuenta no devolvió identificador de empresa.");
                if(!string.IsNullOrWhiteSpace(settings.CompanyId)&&settings.CompanyId!=companyId)
                    throw new InvalidOperationException("Las credenciales pertenecen a otra empresa. No se pueden reutilizar estas vinculaciones.");
                var numbering=Options(AlegraWorkflow.List(await api.GetNumberTemplatesAsync(lifetime.Token)));
                var banks=Options(AlegraWorkflow.List(await api.GetBankAccountsAsync(lifetime.Token)));
                var taxes=AlegraWorkflow.List(await api.GetTaxesAsync(lifetime.Token));
                taxRates.Clear();
                foreach(var tax in taxes)
                {
                    if(tax.ContainsKey("percentage")&&decimal.TryParse(Convert.ToString(tax["percentage"]),out decimal percentage))
                        taxRates[AlegraWorkflow.Text(tax,"id")]=percentage;
                }
                remoteItems.Clear();
                for(int start=0;start<30000;start+=30)
                {
                    var page=AlegraWorkflow.List(await api.GetItemsAsync(start,lifetime.Token));
                    foreach(var item in page) remoteItems[AlegraWorkflow.Text(item,"id")]=item;
                    if(page.Count<30)break;
                }
                if(IsDisposed)return;
                Bind(templates,numbering,settings.NumberTemplateId);
                Bind(cash,banks,Account("EFECTIVO"));Bind(transfer,banks,Account("TRANSFERENCIA"));Bind(card,banks,Account("TARJETA"));
                var items=Options(remoteItems.Values.ToList());
                Bind(delivery,items,settings.DeliveryItem?.RemoteId);Bind(packaging,items,settings.PackagingItem?.RemoteId);
                mappings.Columns.Clear();mappings.Rows.Clear();
                mappings.Columns.Add(new DataGridViewTextBoxColumn { Name="Local",HeaderText="Producto local",ReadOnly=true,FillWeight=160 });
                mappings.Columns.Add(new DataGridViewComboBoxColumn { Name="Remote",HeaderText="Producto existente en Alegra",DataSource=new List<Option>(new[]{new Option {Id="",Name="Sin vincular"}}.Concat(items)),DisplayMember="Name",ValueMember="Id",FillWeight=200 });
                foreach(System.Data.DataRow row in AlegraWorkflow.LocalProducts().Rows)
                {
                    string key=Convert.ToString(row["Tipo"])+":"+row["Id"];
                    int index=mappings.Rows.Add(Convert.ToString(row["Nombre"]),settings.Products.ContainsKey(key)?settings.Products[key].RemoteId:"");
                    mappings.Rows[index].Tag=key;
                }
            }
        }
        private string Account(string method)=>settings.PaymentAccounts.ContainsKey(method)?settings.PaymentAccounts[method]:"";
        private AlegraItemMapping Item(string id)
        {
            if(string.IsNullOrWhiteSpace(id))return null;
            if(!remoteItems.TryGetValue(id,out var item))throw new InvalidOperationException("Uno de los productos vinculados ya no está disponible.");
            var mapping=new AlegraItemMapping {RemoteId=id};
            if(item.ContainsKey("tax")&&item["tax"] is object[] taxes)
                foreach(Dictionary<string,object> tax in taxes)
                {
                    string taxId=AlegraWorkflow.Text(tax,"id");
                    if(!taxRates.TryGetValue(taxId,out decimal rate))throw new InvalidOperationException("No se pudo determinar el impuesto de "+AlegraWorkflow.Text(item,"name")+". Revise el catálogo tributario.");
                    mapping.PercentageTaxes.Add(taxId,rate);
                }
            return mapping;
        }
        private void Save()
        {
            try
            {
                if(string.IsNullOrWhiteSpace(companyId))throw new InvalidOperationException("Conecte y cargue los catálogos antes de guardar.");
                if(enabled.Checked&&string.IsNullOrWhiteSpace(Convert.ToString(templates.SelectedValue)))throw new InvalidOperationException("Seleccione la numeración electrónica antes de activar.");
                if(autoPrint.Checked)
                {
                    if(!File.Exists(AlegraPdfService.PrinterExecutable(pdfExecutable.Text))||!Path.GetFileName(AlegraPdfService.PrinterExecutable(pdfExecutable.Text)).Equals("SumatraPDF.exe",StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("Seleccione el ejecutable SumatraPDF.exe instalado.");
                    if(!string.IsNullOrWhiteSpace(printer.Text) && !PrinterSettings.InstalledPrinters.Cast<string>().Contains(printer.Text))throw new InvalidOperationException("Seleccione una impresora instalada en Windows.");
                }
                mappings.EndEdit();
                var updated=new AlegraInvoiceConfiguration {CompanyId=companyId,Enabled=enabled.Checked,AutoPrint=autoPrint.Checked,
                    AutoPrintFromUtc=autoPrint.Checked && !settings.AutoPrint ? DateTime.UtcNow : settings.AutoPrintFromUtc,
                    NumberTemplateId=Convert.ToString(templates.SelectedValue),FiscalPaymentMethod=fiscal.Text,TipChargeId=tipId.Text.Trim(),
                    PdfPrinterExecutable=pdfExecutable.Text.Trim(),PrinterName=printer.Text.Trim(),DeliveryItem=Item(Convert.ToString(delivery.SelectedValue)),PackagingItem=Item(Convert.ToString(packaging.SelectedValue))};
                foreach(DataGridViewRow row in mappings.Rows)
                {
                    var item=Item(Convert.ToString(row.Cells["Remote"].Value));if(item!=null)updated.Products.Add(Convert.ToString(row.Tag),item);
                }
                foreach(var pair in new[]{Tuple.Create("EFECTIVO",cash,"cash"),Tuple.Create("TRANSFERENCIA",transfer,"transfer"),Tuple.Create("TARJETA",card,"credit-card")})
                {
                    updated.PaymentAccounts.Add(pair.Item1,Convert.ToString(pair.Item2.SelectedValue));updated.PaymentMethods.Add(pair.Item1,pair.Item3);
                }
                AlegraWorkflow.SaveSettings(updated);DialogResult=DialogResult.OK;Close();
            }
            catch(Exception ex){status.Text=ex is InvalidOperationException?ex.Message:"No se pudo guardar la configuración. Verifique la migración 07 y las selecciones.";}
        }
    }
}
