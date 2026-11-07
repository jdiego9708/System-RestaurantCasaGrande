using CapaNegocio.Alegra;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CapaPresentacion
{
    public static class AlegraPdfService
    {
        public static async Task<string> DownloadAsync(int saleId,CancellationToken token,bool refresh=false)
        {
            var requests=AlegraWorkflow.Request(saleId);
            if(requests.Rows.Count!=1)throw new InvalidOperationException("Esta venta no tiene una solicitud electrónica vinculada.");
            var row=requests.Rows[0];
            if(Convert.ToString(row["Status"])!="ACCEPTED")throw new InvalidOperationException("La factura electrónica todavía no está confirmada como aceptada.");
            string cached=Convert.ToString(row["PdfPath"]);
            string cache=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"CasaGrande","Alegra","PDF");
            Directory.CreateDirectory(cache);
            string file=Path.Combine(cache,saleId+"-electronica.pdf");
            if(!refresh && cached==file && File.Exists(file) && IsPdf(file))return file;
            string downloaded;
            using (var api = new AlegraClient(new HttpClientHandler { AllowAutoRedirect = false }, AlegraCredentials.FromEmissionEnvironment()))
            {
                var settings = AlegraWorkflow.Settings();
                var company = AlegraWorkflow.Object(await api.GetCompanyAsync(token).ConfigureAwait(false));
                if (AlegraWorkflow.CompanyIdentity(company) != settings.CompanyId)
                    throw new InvalidOperationException("La credencial PDF pertenece a otra empresa.");
                var invoice = AlegraWorkflow.Object(await api.GetInvoiceAsync(Convert.ToString(row["RemoteId"]), token).ConfigureAwait(false));
                // Total esperado: el de la venta o, en una factura parcial, el de los productos vinculados.
                decimal total = AlegraInvoiceBuilder.ExpectedTotal(Convert.ToString(row["RequestJson"]), settings);
                if (!invoice.ContainsKey("total") || Math.Abs(Convert.ToDecimal(invoice["total"]) - total) > 0.01m)
                    throw new InvalidOperationException("El total de la factura electronica no coincide con lo que debía facturarse en Alegra.");
                downloaded = await DownloadDocumentAsync(Convert.ToString(row["RemoteId"]), file, token, true).ConfigureAwait(false);
            }
            AlegraWorkflow.StorePdf(saleId,downloaded);
            return downloaded;
        }

        public static async Task<string> RenderForPrintAsync(int saleId, CancellationToken token, bool refresh=false)
        {
            string official = await DownloadAsync(saleId,token,refresh).ConfigureAwait(false);
            var row = AlegraWorkflow.Request(saleId).Rows[0];
            var settings = AlegraWorkflow.Settings();
            using (var api = new AlegraClient(new HttpClientHandler {AllowAutoRedirect=false}, AlegraCredentials.FromEmissionEnvironment()))
            {
                var company = AlegraWorkflow.Object(await api.GetCompanyAsync(token).ConfigureAwait(false));
                if (AlegraWorkflow.CompanyIdentity(company) != settings.CompanyId)
                    throw new InvalidOperationException("La credencial de impresion pertenece a otra empresa.");
                string file=Path.Combine(Path.GetDirectoryName(official),saleId+"-80mm.pdf");
                return await RenderReviewAsync(api,Convert.ToString(row["RemoteId"]),file,token,true,official).ConfigureAwait(false);
            }
        }

        public static Task<string> DownloadRemoteAsync(string remoteId,CancellationToken token)
        {
            if(string.IsNullOrWhiteSpace(remoteId))throw new ArgumentException("Seleccione una factura de Alegra.");
            string cache=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"CasaGrande","Alegra","RemotePDF");
            Directory.CreateDirectory(cache);
            return DownloadDocumentAsync(remoteId,Path.Combine(cache,Guid.NewGuid().ToString("N")+".pdf"),token);
        }

        private static async Task<string> DownloadDocumentAsync(string remoteId,string file,CancellationToken token,bool requireOfficial=false)
        {
            using(var api=new AlegraClient(new HttpClientHandler { AllowAutoRedirect=false }, AlegraCredentials.FromEmissionEnvironment()))
            {
                var invoice=AlegraWorkflow.Object(await api.GetInvoiceAsync(remoteId,token).ConfigureAwait(false));
                if(AlegraWorkflow.Text(invoice,"id")!=remoteId)throw new InvalidOperationException("El documento recibido no coincide con la factura seleccionada.");
                if(AlegraWorkflow.Text(invoice,"status")=="draft")throw new InvalidOperationException("Los borradores no disponen de PDF en la consulta de factura. Use una factura emitida para comprobar su formato.");
                string emission=AlegraWorkflow.Emission(invoice);
                if(!new[]{"ACCEPTED","STAMPED_AND_ACCEPTED","STAMPED_AND_ACCEPTED_WITH_OBSERVATIONS"}.Contains(emission))
                    throw new InvalidOperationException("Alegra todavía no confirmó la aceptación electrónica del documento.");
                try
                {
                    invoice=AlegraWorkflow.Object(await api.GetInvoicePdfAsync(remoteId,token).ConfigureAwait(false));
                }
                catch(InvalidOperationException ex)
                {
                    if(requireOfficial || !ex.Message.Contains("HTTP 500"))throw;
                    return await RenderReviewAsync(api,remoteId,file,token).ConfigureAwait(false);
                }
                string pdf=AlegraWorkflow.Text(invoice,"pdf");
                if(invoice.ContainsKey("pdf") && invoice["pdf"] is System.Collections.Generic.Dictionary<string,object> value)pdf=AlegraWorkflow.Text(value,"url");
                if(!Uri.TryCreate(pdf,UriKind.Absolute,out Uri uri)||uri.Scheme!="https"||!string.IsNullOrEmpty(uri.UserInfo))
                {
                    if(requireOfficial)throw new InvalidOperationException("Alegra no proporciono el PDF oficial necesario para contrastar fecha de validacion y software.");
                    return await RenderReviewAsync(api,remoteId,file,token).ConfigureAwait(false);
                }
                // Cliente separado: nunca enviar Authorization de Alegra al almacenamiento del PDF.
                using(var handler=new HttpClientHandler {AllowAutoRedirect=false})
                using(var client=new HttpClient(handler) {Timeout=TimeSpan.FromSeconds(45)})
                {
                    for(int redirect=0;redirect<4;redirect++)
                    {
                        await CheckPublicHost(uri).ConfigureAwait(false);
                        using(var response=await client.GetAsync(uri,HttpCompletionOption.ResponseHeadersRead,token).ConfigureAwait(false))
                        {
                            if((int)response.StatusCode>=300 && (int)response.StatusCode<400)
                            {
                                if(response.Headers.Location==null)throw new InvalidOperationException("Redirección PDF inválida.");
                                uri=new Uri(uri,response.Headers.Location);continue;
                            }
                            response.EnsureSuccessStatusCode();
                            if(response.Content.Headers.ContentLength>20*1024*1024)throw new InvalidOperationException("El PDF excede el tamaño máximo permitido.");
                            string temp=file+"."+Guid.NewGuid().ToString("N")+".tmp";
                            try
                            {
                                using(var input=await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                                using(var output=new FileStream(temp,FileMode.CreateNew,FileAccess.Write,FileShare.None))
                                {
                                    byte[] buffer=new byte[81920];int count;long total=0;
                                    while((count=await input.ReadAsync(buffer,0,buffer.Length,token).ConfigureAwait(false))>0)
                                    {
                                        total+=count;if(total>20*1024*1024)throw new InvalidOperationException("El PDF excede el tamaño máximo permitido.");
                                        await output.WriteAsync(buffer,0,count,token).ConfigureAwait(false);
                                    }
                                }
                                if(!IsPdf(temp))throw new InvalidOperationException("La descarga no es un archivo PDF válido.");
                                File.Copy(temp,file,true);
                                return file;
                            }
                            finally {if(File.Exists(temp))File.Delete(temp);}
                        }
                    }
                }
            }
            throw new InvalidOperationException("Demasiadas redirecciones al descargar el PDF.");
        }
        private static bool IsPdf(string file)
        {
            using(var stream=File.OpenRead(file)) {byte[] signature=new byte[5];return stream.Read(signature,0,5)==5 && System.Text.Encoding.ASCII.GetString(signature)=="%PDF-";}
        }

        // Renderiza desde el XML real; receipt=true usa los datos de validacion del PDF oficial.
        // La descarga oficial y la representacion termica se conservan como archivos separados.
        private static async Task<string> RenderReviewAsync(AlegraClient api,string id,string file,CancellationToken token,bool receipt=false,string official=null)
        {
            string script=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Alegra","Render-AlegraFiscalRepresentation.py");
            string renderer = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Alegra","Tools","Renderer","CasaGrandeFiscalPdf.exe");
            if(!File.Exists(renderer) && !File.Exists(script))throw new InvalidOperationException("No se encontro el generador de PDF de 80 mm. Actualice la instalacion.");
            string json=await api.GetInvoiceXmlAsync(id,token).ConfigureAwait(false);
            var invoice=AlegraWorkflow.Object(json);
            if(AlegraWorkflow.Text(invoice,"id")!=id)throw new InvalidOperationException("El XML no corresponde a la factura seleccionada.");
            if (!new[] {"ACCEPTED","STAMPED_AND_ACCEPTED","STAMPED_AND_ACCEPTED_WITH_OBSERVATIONS"}.Contains(AlegraWorkflow.Emission(invoice)))
                throw new InvalidOperationException("La factura aun no esta aceptada; no se imprime.");
            if(!Uri.TryCreate(AlegraWorkflow.Text(invoice,"xml"),UriKind.Absolute,out Uri uri))throw new InvalidOperationException("Alegra no devolvió el XML necesario para generar el PDF.");
            await CheckPublicHost(uri).ConfigureAwait(false);
            string xmlPath=file+".xml",jsonPath=file+".json";
            using(var handler=new HttpClientHandler {AllowAutoRedirect=false})
            using(var client=new HttpClient(handler) {Timeout=TimeSpan.FromSeconds(45)})
            using(var response=await client.GetAsync(uri,HttpCompletionOption.ResponseHeadersRead,token).ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();
                using(var input=await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                using(var output=File.Create(xmlPath))
                {
                    byte[] buffer=new byte[81920];int count;long total=0;
                    while((count=await input.ReadAsync(buffer,0,buffer.Length,token).ConfigureAwait(false))>0)
                    {total+=count;if(total>20*1024*1024)throw new InvalidOperationException("El XML excede el tamaño permitido.");await output.WriteAsync(buffer,0,count,token).ConfigureAwait(false);}
                }
            }
            // No persistir enlaces temporales firmados en el detalle utilizado por el renderer.
            invoice.Remove("xml");invoice.Remove("pdf");
            File.WriteAllText(jsonPath,new System.Web.Script.Serialization.JavaScriptSerializer {MaxJsonLength=8*1024*1024}.Serialize(invoice),System.Text.Encoding.UTF8);
            string review=receipt ? file : Path.Combine(Path.GetDirectoryName(file),Path.GetFileNameWithoutExtension(file)+"-revision.pdf");
            string tempPdf=Path.Combine(Path.GetDirectoryName(file),Guid.NewGuid().ToString("N")+".pdf");
            string arguments=(File.Exists(renderer) ? "" : "\""+script+"\" ")
                + "\""+xmlPath+"\" \""+jsonPath+"\" \""+tempPdf+"\""
                + (receipt ? " --receipt --official-pdf \""+official+"\"" : "");
            try
            {
                using(var process=Process.Start(new ProcessStartInfo {FileName=File.Exists(renderer) ? renderer : "python",Arguments=arguments,UseShellExecute=false,CreateNoWindow=true,WindowStyle=ProcessWindowStyle.Hidden,RedirectStandardOutput=true,RedirectStandardError=true}))
                {
                    Task<string> stdout=process.StandardOutput.ReadToEndAsync(),stderr=process.StandardError.ReadToEndAsync();
                    bool exited=await Task.Run(()=>process.WaitForExit(45000)).ConfigureAwait(false);
                    if(!exited){process.Kill();throw new InvalidOperationException("El generador PDF excedió el tiempo de espera.");}
                    await Task.WhenAll(stdout,stderr).ConfigureAwait(false);
                    token.ThrowIfCancellationRequested();
                    if(process.ExitCode!=0||!File.Exists(tempPdf)||!IsPdf(tempPdf))throw new InvalidOperationException("No se pudo generar el PDF de 80 mm. Revise coincidencia de numero, CUFE, total y datos de validacion del XML/PDF oficial.");
                }
            }
            catch(System.ComponentModel.Win32Exception){throw new InvalidOperationException("No se pudo iniciar el generador PDF. Actualice la instalacion con las herramientas portables de Alegra.");}
            File.Copy(tempPdf,review,true);
            File.Delete(tempPdf);
            string report=Path.ChangeExtension(tempPdf,".revision.json");
            if(File.Exists(report)) { File.Copy(report,Path.ChangeExtension(review,".revision.json"),true); File.Delete(report); }
            return review;
        }
        private static async Task CheckPublicHost(Uri uri)
        {
            if(uri.Scheme!="https"||uri.IsLoopback||uri.Port!=443||!string.IsNullOrEmpty(uri.UserInfo))throw new InvalidOperationException("La URL del PDF no es válida.");
            foreach(var ip in await Dns.GetHostAddressesAsync(uri.Host).ConfigureAwait(false))
            {
                byte[] b=ip.GetAddressBytes();
                if(IPAddress.IsLoopback(ip)||ip.IsIPv6LinkLocal||ip.IsIPv6SiteLocal||ip.IsIPv4MappedToIPv6
                    || (b.Length==4 && (b[0]==0||b[0]==10||b[0]==127||b[0]>=224||b[0]==169&&b[1]==254||b[0]==192&&b[1]==168||b[0]==172&&b[1]>=16&&b[1]<=31))
                    ||(b.Length==16&&(b[0]&0xfe)==0xfc))throw new InvalidOperationException("La URL del PDF apunta a una dirección no pública.");
            }
        }

        public static async Task PrintAsync(int saleId,CancellationToken token,bool reprint=false)
        {
            var settings=AlegraWorkflow.Settings();
            string executable = PrinterExecutable(settings.PdfPrinterExecutable);
            string printer = string.IsNullOrWhiteSpace(settings.PrinterName) ? new System.Drawing.Printing.PrinterSettings().PrinterName : settings.PrinterName;
            if(!File.Exists(executable)||!Path.GetFileName(executable).Equals("SumatraPDF.exe",StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Configure SumatraPDF para imprimir el PDF de Alegra.");
            if(string.IsNullOrWhiteSpace(printer)||printer.Contains("\"")||printer.Contains("\r")||printer.Contains("\n"))
                throw new InvalidOperationException("Configure una impresora térmica válida.");
            string file=await RenderForPrintAsync(saleId,token,reprint).ConfigureAwait(false);
            if(Path.GetFileName(file).EndsWith("-revision.pdf",StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Alegra no proporcionó su PDF. La representación de revisión debe abrirse y revisarse manualmente; no se imprimirá automáticamente.");
            var printerSettings = new System.Drawing.Printing.PrinterSettings { PrinterName=printer };
            if (!printerSettings.IsValid)throw new InvalidOperationException("Seleccione una impresora instalada en Windows.");
            if(!AlegraWorkflow.ClaimPrint(saleId,reprint))throw new InvalidOperationException("El documento ya está impreso o en proceso. Actualice el estado antes de reintentar.");
            try
            {
                await PrintFileAsync(file,executable,printer,token).ConfigureAwait(false);
                AlegraWorkflow.MarkPrint(saleId,"PRINTED");
            }
            catch (OperationCanceledException) { AlegraWorkflow.MarkPrint(saleId,"CANCELLED");throw; }
            catch { AlegraWorkflow.MarkPrint(saleId,"UNKNOWN");throw; }
        }

        public static string PrinterExecutable(string configured)
        {
            return string.IsNullOrWhiteSpace(configured)
                ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Alegra", "Tools", "SumatraPDF.exe")
                : configured;
        }

        private sealed class DialogOwner : IWin32Window
        {
            public IntPtr Handle { get; private set; }
            public DialogOwner(IntPtr handle) { Handle=handle; }
        }

        // Mantiene el PDF oficial intacto y muestra el dialogo Windows de guardado.
        // Hilo STA propio: la descarga/emision siguen fuera del hilo de la interfaz.
        private static Task SaveVirtualPdfAsync(string file, CancellationToken token)
        {
            var completion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            IntPtr owner = Process.GetCurrentProcess().MainWindowHandle;
            var thread = new Thread(() =>
            {
                try
                {
                    token.ThrowIfCancellationRequested();
                    using (var dialog = new SaveFileDialog
                    {
                        Title="Guardar factura electronica de Alegra", Filter="Factura electronica PDF (*.pdf)|*.pdf",
                        FileName=Path.GetFileName(file), DefaultExt="pdf", AddExtension=true, OverwritePrompt=true,
                    })
                    {
                        var result = owner == IntPtr.Zero ? dialog.ShowDialog() : dialog.ShowDialog(new DialogOwner(owner));
                        if (result != DialogResult.OK) { completion.TrySetCanceled(); return; }
                        token.ThrowIfCancellationRequested();
                        if (!string.Equals(Path.GetFullPath(file), Path.GetFullPath(dialog.FileName), StringComparison.OrdinalIgnoreCase))
                            File.Copy(file, dialog.FileName, true);
                        completion.TrySetResult(true);
                    }
                }
                catch (OperationCanceledException) { completion.TrySetCanceled(); }
                catch (Exception ex) { completion.TrySetException(ex); }
            });
            thread.IsBackground=true;
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            return completion.Task;
        }

        public static async Task PrintFileAsync(string file,string executable,string printer,CancellationToken token)
        {
            if(!File.Exists(file)||!IsPdf(file))throw new InvalidOperationException("Seleccione un PDF válido.");
            if(!File.Exists(executable)||!Path.GetFileName(executable).Equals("SumatraPDF.exe",StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Configure SumatraPDF.exe para imprimir sin cambiar el PDF original.");
            if(string.IsNullOrWhiteSpace(printer)||printer.IndexOfAny(new[]{'\"','\r','\n'})>=0)throw new InvalidOperationException("La impresora seleccionada no es válida.");
            if (printer.Equals("Microsoft Print to PDF", StringComparison.OrdinalIgnoreCase) || printer.Equals("Adobe PDF", StringComparison.OrdinalIgnoreCase))
            {
                await SaveVirtualPdfAsync(file, token).ConfigureAwait(false);
                return;
            }
            bool interactive = printer.IndexOf("PDF", StringComparison.OrdinalIgnoreCase) >= 0 || printer.IndexOf("OneNote", StringComparison.OrdinalIgnoreCase) >= 0;
            using(var process=Process.Start(new ProcessStartInfo {
                FileName=executable,Arguments=(interactive ? "" : "-silent ") + "-print-to \""+printer+"\" -print-settings \"noscale,disable-auto-rotation\" \""+Path.GetFullPath(file)+"\"",
                UseShellExecute=false,CreateNoWindow=!interactive,WindowStyle=interactive ? ProcessWindowStyle.Normal : ProcessWindowStyle.Hidden }))
            {
                await Task.Run(()=>process.WaitForExit(),token).ConfigureAwait(false);
                if(process.ExitCode!=0)throw new InvalidOperationException("La impresión no fue confirmada. Revise la cola de Windows antes de reimprimir.");
            }
        }
    }
}
