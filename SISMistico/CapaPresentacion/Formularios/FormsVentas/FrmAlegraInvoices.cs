using CapaNegocio.Alegra;
using CapaPresentacion.UI;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CapaPresentacion.Formularios.FormsVentas
{
    // Consulta remota independiente de Ventas local. Solo GET y descarga de documentos existentes.
    public sealed class FrmAlegraInvoices : Form
    {
        private readonly DateTimePicker from=new DateTimePicker {Format=DateTimePickerFormat.Short,Width=125};
        private readonly DateTimePicker to=new DateTimePicker {Format=DateTimePickerFormat.Short,Width=125};
        private readonly DataGridView grid=new DataGridView();
        private readonly Label status=new Label {Dock=DockStyle.Bottom,Height=42,ForeColor=Theme.TextMuted};
        private readonly ModernButton search=Button("Consultar",120),previous=Button("Anterior",110),next=Button("Siguiente",110),open=Button("Abrir PDF",130),print=Button("Imprimir PDF",150);
        private readonly CancellationTokenSource lifetime=new CancellationTokenSource();
        private int offset;
        private DateTime queryFrom,queryTo;
        private bool busy;
        private int pageCount;
        public FrmAlegraInvoices()
        {
            Text="Facturas de Alegra";ClientSize=new Size(1160,700);MinimumSize=new Size(850,550);
            StartPosition=FormStartPosition.CenterParent;BackColor=Theme.Background;Font=Theme.Body;
            from.Value=new DateTime(DateTime.Today.Year,DateTime.Today.Month,1);to.Value=DateTime.Today;
            Panel root=new Panel {Dock=DockStyle.Fill,Padding=new Padding(20)};
            Label title=new Label {Text="Facturas de Alegra",Dock=DockStyle.Top,Height=42};title.StyleLabel(LabelKind.Title);
            Label subtitle=new Label {Text="Cuenta remota completa · Incluye facturas creadas fuera de Casa Grande · Consultar no crea ni emite documentos",Dock=DockStyle.Top,Height=32};subtitle.StyleLabel(LabelKind.Caption);
            FlowLayoutPanel filters=new FlowLayoutPanel {Dock=DockStyle.Top,Height=54,WrapContents=false};
            filters.Controls.Add(new Label {Text="Desde",AutoSize=true,Margin=new Padding(0,10,6,0)});filters.Controls.Add(from);
            filters.Controls.Add(new Label {Text="Hasta",AutoSize=true,Margin=new Padding(12,10,6,0)});filters.Controls.Add(to);filters.Controls.Add(search);
            ModernCard card=new ModernCard {Dock=DockStyle.Fill};
            grid.Dock=DockStyle.Fill;grid.ReadOnly=true;grid.AllowUserToAddRows=false;grid.AllowUserToDeleteRows=false;
            grid.MultiSelect=false;grid.SelectionMode=DataGridViewSelectionMode.FullRowSelect;grid.StyleGrid();
            grid.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill;grid.DefaultCellStyle.Padding=new Padding(8,5,8,5);
            card.Controls.Add(grid);
            FlowLayoutPanel footer=new FlowLayoutPanel {Dock=DockStyle.Bottom,Height=52,WrapContents=false};
            footer.Controls.Add(previous);footer.Controls.Add(next);footer.Controls.Add(open);footer.Controls.Add(print);
            root.Controls.Add(card);root.Controls.Add(status);root.Controls.Add(footer);root.Controls.Add(filters);root.Controls.Add(subtitle);root.Controls.Add(title);Controls.Add(root);
            search.Click+=async(s,e)=>{queryFrom=from.Value.Date;queryTo=to.Value.Date;await LoadPage(0);};
            previous.Click+=async(s,e)=>await LoadPage(Math.Max(0,offset-30));next.Click+=async(s,e)=>await LoadPage(offset+30);
            open.Click+=async(s,e)=>await Document(false);print.Click+=async(s,e)=>await Document(true);
            grid.SelectionChanged+=(s,e)=>UpdateButtons();
            grid.CellDoubleClick+=async(s,e)=>{if(e.RowIndex>=0)await Details();};
            FormClosed+=(s,e)=>lifetime.Cancel();UpdateButtons();
            status.Text="Seleccione el período y pulse Consultar. Los borradores se pueden consultar, pero no tienen PDF de factura.";
        }
        private static ModernButton Button(string text,int width)=>new ModernButton {Text=text,Width=width,Height=38,Kind=ButtonKind.Outline,Margin=new Padding(4,0,6,0)};
        private void UpdateButtons()
        {
            search.Enabled=!busy;from.Enabled=to.Enabled=!busy;
            previous.Enabled=!busy&&offset>0;next.Enabled=!busy&&pageCount==30;
            bool selected=grid.CurrentRow!=null;
            open.Enabled=print.Enabled=!busy&&selected&&Convert.ToString(grid.CurrentRow.Cells["Status"].Value)!="draft";
        }
        private async Task LoadPage(int start)
        {
            if(busy)return;busy=true;UpdateButtons();status.Text="Consultando la cuenta de Alegra…";
            try
            {
                using(var api=new AlegraClient())
                {
                    var page=AlegraWorkflow.List(await api.GetInvoicesAsync(queryFrom,queryTo,start,lifetime.Token));
                    if(IsDisposed)return;
                    DataTable table=new DataTable();
                    foreach(string field in new[]{"Id","Number","Date","Client","Status","Emission"})table.Columns.Add(field);
                    table.Columns.Add("Total",typeof(decimal));
                    foreach(var invoice in page)
                    {
                        var number=invoice.ContainsKey("numberTemplate")?invoice["numberTemplate"] as Dictionary<string,object>:null;
                        var client=invoice.ContainsKey("client")?invoice["client"] as Dictionary<string,object>:null;
                        string emission=AlegraWorkflow.Emission(invoice);
                        decimal.TryParse(AlegraWorkflow.Text(invoice,"total"),NumberStyles.Number,CultureInfo.CurrentCulture,out decimal amount);
                        table.Rows.Add(AlegraWorkflow.Text(invoice,"id"),AlegraWorkflow.Text(number,"fullNumber"),AlegraWorkflow.Text(invoice,"date"),
                            AlegraWorkflow.Text(client,"name"),AlegraWorkflow.Text(invoice,"status"),string.IsNullOrWhiteSpace(emission)?"No informado en listado":emission,amount);
                    }
                    grid.DataSource=table;offset=start;pageCount=page.Count;
                    string[] captions={"ID Alegra","Factura","Fecha","Cliente","Estado comercial","Emisión electrónica","Total COP"};
                    for(int i=0;i<captions.Length;i++)grid.Columns[i].HeaderText=captions[i];
                    grid.Columns["Id"].MinimumWidth=100;grid.Columns["Client"].FillWeight=160;grid.Columns["Emission"].FillWeight=160;
                    grid.Columns["Total"].DefaultCellStyle.Format="C0";grid.Columns["Total"].DefaultCellStyle.FormatProvider=CultureInfo.GetCultureInfo("es-CO");
                    grid.Columns["Total"].DefaultCellStyle.Alignment=DataGridViewContentAlignment.MiddleRight;grid.Columns["Total"].DefaultCellStyle.Font=Theme.BodyBold;
                    status.Text=page.Count==0?"No hay más facturas en el período seleccionado.":"Página "+(offset/30+1)+" · "+page.Count+" documentos. El listado puede omitir el estado electrónico; al abrir el PDF se consulta la factura individual.";
                }
            }
            catch(OperationCanceledException){if(!IsDisposed)status.Text="Consulta cancelada o tiempo de espera agotado.";}
            catch(Exception ex){if(!IsDisposed)status.Text=ex is InvalidOperationException||ex is ArgumentException?ex.Message:"No se pudo consultar Alegra. Revise la conexión.";}
            finally{busy=false;if(!IsDisposed)UpdateButtons();}
        }
        private async Task Document(bool printing)
        {
            if(busy||grid.CurrentRow==null)return;
            string id=Convert.ToString(grid.CurrentRow.Cells["Id"].Value);
            busy=true;UpdateButtons();status.Text="Consultando y descargando el PDF oficial…";
            try
            {
                string file=await AlegraPdfService.DownloadRemoteAsync(id,lifetime.Token);
                if(IsDisposed)return;
                bool review=Path.GetFileName(file).EndsWith("-revision.pdf",StringComparison.OrdinalIgnoreCase);
                if(!printing){Process.Start(new ProcessStartInfo(file){UseShellExecute=true});status.Text=review?"Representación compacta para revisión abierta: Alegra no pudo proporcionar su PDF.":"PDF de Alegra abierto.";}
                else
                {
                    if(review && MessageBox.Show(this,"Alegra no pudo proporcionar su PDF. Se generó una representación del XML marcada para revisión contable. ¿Desea imprimir esa representación?","Representación de revisión",MessageBoxButtons.YesNo,MessageBoxIcon.Information)!=DialogResult.Yes)return;
                    var settings=AlegraWorkflow.Settings();
                    string executable=settings.PdfPrinterExecutable;
                    if(!File.Exists(executable))
                    {
                        using(var choose=new OpenFileDialog {Title="Seleccione SumatraPDF.exe para imprimir el PDF original",Filter="SumatraPDF|SumatraPDF.exe",CheckFileExists=true})
                        {if(choose.ShowDialog(this)!=DialogResult.OK)return;executable=choose.FileName;}
                    }
                    using(var dialog=new PrintDialog {UseEXDialog=true,AllowSomePages=false})
                    {
                        dialog.PrinterSettings=new PrinterSettings();
                        if(dialog.ShowDialog(this)!=DialogResult.OK)return;
                        status.Text="Enviando PDF a la impresora seleccionada…";
                        await AlegraPdfService.PrintFileAsync(file,executable,dialog.PrinterSettings.PrinterName,lifetime.Token);
                        if(!IsDisposed)status.Text="PDF enviado a impresión sin escalar. Compruebe el formato físico de 80 mm.";
                    }
                }
            }
            catch(OperationCanceledException){if(!IsDisposed)status.Text="Descarga cancelada o tiempo de espera agotado.";}
            catch(Exception ex){if(!IsDisposed)status.Text=ex is InvalidOperationException?ex.Message:"No se pudo descargar o imprimir el PDF de Alegra.";}
            finally{busy=false;if(!IsDisposed)UpdateButtons();}
        }
        private async Task Details()
        {
            if(busy||grid.CurrentRow==null)return;
            string id=Convert.ToString(grid.CurrentRow.Cells["Id"].Value);
            busy=true;UpdateButtons();status.Text="Consultando detalle de factura…";
            var loading=new Panel {Dock=DockStyle.Fill,BackColor=Theme.Background};
            var indicator=new ModernCard {Size=new Size(380,140),AccentColor=Theme.Primary};
            indicator.Controls.Add(new Label {Text="Cargando detalle de factura…",Dock=DockStyle.Top,Height=55,TextAlign=ContentAlignment.MiddleCenter,Font=Theme.BodyBold});
            indicator.Controls.Add(new ProgressBar {Dock=DockStyle.Bottom,Height=8,Style=ProgressBarStyle.Marquee,MarqueeAnimationSpeed=25});
            loading.Controls.Add(indicator);
            EventHandler center=(s,e)=>indicator.Location=new Point(Math.Max(0,(loading.ClientSize.Width-indicator.Width)/2),Math.Max(0,(loading.ClientSize.Height-indicator.Height)/2));
            loading.Resize+=center;Controls.Add(loading);loading.BringToFront();center(null,EventArgs.Empty);loading.Refresh();
            try
            {
                await Task.Yield();
                using(var api=new AlegraClient())
                {
                    var invoice=AlegraWorkflow.Object(await api.GetInvoiceDetailsAsync(id,lifetime.Token));
                    if(IsDisposed)return;
                    if(AlegraWorkflow.Text(invoice,"id")!=id)throw new InvalidOperationException("La respuesta no corresponde a la factura seleccionada.");
                    using(var detail=new FrmAlegraInvoiceDetails(invoice))
                    {
                        loading.Hide();
                        detail.ShowDialog(this);
                    }
                    if(!IsDisposed)status.Text="Detalle de factura consultado.";
                }
            }
            catch(OperationCanceledException){if(!IsDisposed)status.Text="Consulta cancelada.";}
            catch(Exception ex){if(!IsDisposed)status.Text=ex is InvalidOperationException?ex.Message:"No se pudo consultar el detalle de la factura.";}
            finally{loading.Resize-=center;loading.Dispose();busy=false;if(!IsDisposed)UpdateButtons();}
        }
    }
}
