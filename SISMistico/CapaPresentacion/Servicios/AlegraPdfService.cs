using CapaNegocio.Alegra;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace CapaPresentacion
{
    public static class AlegraPdfService
    {
        public static async Task<string> DownloadAsync(int saleId,CancellationToken token)
        {
            var requests=AlegraWorkflow.Request(saleId);
            if(requests.Rows.Count!=1)throw new InvalidOperationException("Esta venta no tiene una solicitud electrónica vinculada.");
            var row=requests.Rows[0];
            if(Convert.ToString(row["Status"])!="ACCEPTED")throw new InvalidOperationException("La factura electrónica todavía no está confirmada como aceptada.");
            string cached=Convert.ToString(row["PdfPath"]);
            string cache=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"CasaGrande","Alegra","PDF");
            Directory.CreateDirectory(cache);
            string file=Path.Combine(cache,saleId+".pdf");
            if(cached==file && File.Exists(file) && IsPdf(file))return file;
            string downloaded=await DownloadDocumentAsync(Convert.ToString(row["RemoteId"]),file,token).ConfigureAwait(false);
            AlegraWorkflow.StorePdf(saleId,downloaded);
            return downloaded;
        }

        public static Task<string> DownloadRemoteAsync(string remoteId,CancellationToken token)
        {
            if(string.IsNullOrWhiteSpace(remoteId))throw new ArgumentException("Seleccione una factura de Alegra.");
            string cache=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"CasaGrande","Alegra","RemotePDF");
            Directory.CreateDirectory(cache);
            return DownloadDocumentAsync(remoteId,Path.Combine(cache,Guid.NewGuid().ToString("N")+".pdf"),token);
        }

        private static async Task<string> DownloadDocumentAsync(string remoteId,string file,CancellationToken token)
        {
            using(var api=new AlegraClient())
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
                    if(!ex.Message.Contains("HTTP 500"))throw;
                    return await RenderReviewAsync(api,remoteId,file,token).ConfigureAwait(false);
                }
                string pdf=AlegraWorkflow.Text(invoice,"pdf");
                if(invoice.ContainsKey("pdf") && invoice["pdf"] is System.Collections.Generic.Dictionary<string,object> value)pdf=AlegraWorkflow.Text(value,"url");
                if(!Uri.TryCreate(pdf,UriKind.Absolute,out Uri uri)||uri.Scheme!="https"||!string.IsNullOrEmpty(uri.UserInfo))
                    return await RenderReviewAsync(api,remoteId,file,token).ConfigureAwait(false);
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

        // Alternativa explícita de revisión: mismo XML y comprobaciones que el diseño revisado.
        // No se utiliza para impresión automática ni se presenta como PDF original de Alegra.
        private static async Task<string> RenderReviewAsync(AlegraClient api,string id,string file,CancellationToken token)
        {
            string script=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Alegra","Render-AlegraFiscalRepresentation.py");
            if(!File.Exists(script))throw new InvalidOperationException("Alegra no pudo generar su PDF (HTTP 500). No se encontró el generador de representación para revisión.");
            string json=await api.GetInvoiceXmlAsync(id,token).ConfigureAwait(false);
            var invoice=AlegraWorkflow.Object(json);
            if(AlegraWorkflow.Text(invoice,"id")!=id)throw new InvalidOperationException("El XML no corresponde a la factura seleccionada.");
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
            string review=Path.Combine(Path.GetDirectoryName(file),Path.GetFileNameWithoutExtension(file)+"-revision.pdf");
            try
            {
                using(var process=Process.Start(new ProcessStartInfo {FileName="python",Arguments="\""+script+"\" \""+xmlPath+"\" \""+jsonPath+"\" \""+review+"\"",UseShellExecute=false,CreateNoWindow=true,WindowStyle=ProcessWindowStyle.Hidden,RedirectStandardOutput=true,RedirectStandardError=true}))
                {
                    Task<string> stdout=process.StandardOutput.ReadToEndAsync(),stderr=process.StandardError.ReadToEndAsync();
                    bool exited=await Task.Run(()=>process.WaitForExit(45000)).ConfigureAwait(false);
                    if(!exited){process.Kill();throw new InvalidOperationException("El generador PDF excedió el tiempo de espera.");}
                    await Task.WhenAll(stdout,stderr).ConfigureAwait(false);
                    token.ThrowIfCancellationRequested();
                    if(process.ExitCode!=0||!File.Exists(review)||!IsPdf(review))throw new InvalidOperationException("No se pudo generar la representación. Verifique Python con reportlab y la coincidencia de XML, CUFE y total.");
                }
            }
            catch(System.ComponentModel.Win32Exception){throw new InvalidOperationException("Alegra devolvió HTTP 500. Para generar la representación de revisión instale Python 3 con reportlab, o descargue el PDF desde Alegra web.");}
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
            if(!File.Exists(settings.PdfPrinterExecutable)||!Path.GetFileName(settings.PdfPrinterExecutable).Equals("SumatraPDF.exe",StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Configure SumatraPDF para imprimir el PDF de Alegra.");
            if(string.IsNullOrWhiteSpace(settings.PrinterName)||settings.PrinterName.Contains("\"")||settings.PrinterName.Contains("\r")||settings.PrinterName.Contains("\n"))
                throw new InvalidOperationException("Configure una impresora térmica válida.");
            string file=await DownloadAsync(saleId,token).ConfigureAwait(false);
            if(Path.GetFileName(file).EndsWith("-revision.pdf",StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Alegra no proporcionó su PDF. La representación de revisión debe abrirse y revisarse manualmente; no se imprimirá automáticamente.");
            if(!AlegraWorkflow.ClaimPrint(saleId,reprint))throw new InvalidOperationException("El documento ya está impreso o en proceso. Actualice el estado antes de reintentar.");
            try
            {
                await PrintFileAsync(file,settings.PdfPrinterExecutable,settings.PrinterName,token).ConfigureAwait(false);
                AlegraWorkflow.MarkPrint(saleId,"PRINTED");
            }
            catch { AlegraWorkflow.MarkPrint(saleId,"UNKNOWN");throw; }
        }

        public static async Task PrintFileAsync(string file,string executable,string printer,CancellationToken token)
        {
            if(!File.Exists(file)||!IsPdf(file))throw new InvalidOperationException("Seleccione un PDF válido.");
            if(!File.Exists(executable)||!Path.GetFileName(executable).Equals("SumatraPDF.exe",StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Configure SumatraPDF.exe para imprimir sin cambiar el PDF original.");
            if(string.IsNullOrWhiteSpace(printer)||printer.IndexOfAny(new[]{'\"','\r','\n'})>=0)throw new InvalidOperationException("La impresora seleccionada no es válida.");
            using(var process=Process.Start(new ProcessStartInfo {
                FileName=executable,Arguments="-silent -print-to \""+printer+"\" -print-settings \"noscale,disable-auto-rotation\" \""+Path.GetFullPath(file)+"\"",
                UseShellExecute=false,CreateNoWindow=true,WindowStyle=ProcessWindowStyle.Hidden }))
            {
                bool exited=await Task.Run(()=>process.WaitForExit(60000),token).ConfigureAwait(false);
                if(!exited||process.ExitCode!=0)throw new InvalidOperationException("La impresión no fue confirmada. Revise la cola de Windows antes de reimprimir.");
            }
        }
    }
}
