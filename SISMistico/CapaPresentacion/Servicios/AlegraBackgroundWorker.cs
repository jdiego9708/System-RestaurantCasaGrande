using CapaNegocio.Alegra;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CapaPresentacion
{
    public sealed class AlegraBackgroundWorker : IDisposable
    {
        private readonly CancellationTokenSource lifetime=new CancellationTokenSource();
        private readonly System.Windows.Forms.Timer timer=new System.Windows.Forms.Timer {Interval=15000};
        private bool busy;
        public string LastStatus {get;private set;}
        // Instancia en marcha, para que la pantalla de facturar pueda pedir un ciclo sin esperar al temporizador.
        public static AlegraBackgroundWorker Current {get;private set;}
        public AlegraBackgroundWorker()
        {
            Current=this;
            timer.Tick+=async(s,e)=>await Tick();timer.Start();
        }
        // Si ya hay un ciclo en curso no hace nada: el estado se lee de la base de datos.
        public Task RunCycleAsync()=>Tick();
        private async Task Tick()
        {
            if(busy||lifetime.IsCancellationRequested)return;
            busy=true;
            try
            {
                await Task.Run(async()=>
                {
                    await AlegraWorkflow.TickAsync(lifetime.Token).ConfigureAwait(false);
                    var settings=AlegraWorkflow.Settings();
                    if(settings.Enabled)
                    {
                        var pending=AlegraWorkflow.PendingDocuments(settings.AutoPrint, settings.AutoPrintFromUtc);
                        if(pending.Rows.Count>0)
                        {
                            int saleId=Convert.ToInt32(pending.Rows[0]["SaleId"]);
                            try
                            {
                                await AlegraPdfService.DownloadAsync(saleId,lifetime.Token).ConfigureAwait(false);
                                var request=AlegraWorkflow.Request(saleId).Rows[0];
                                var created=DateTime.SpecifyKind(Convert.ToDateTime(request["CreatedAt"]),DateTimeKind.Utc);
                                if(settings.AutoPrint && (!settings.AutoPrintFromUtc.HasValue || created>=settings.AutoPrintFromUtc.Value)
                                    && Convert.ToString(request["PrintStatus"])=="PENDING")
                                    await AlegraPdfService.PrintAsync(saleId,lifetime.Token).ConfigureAwait(false);
                            }
                            catch(Exception ex)
                            {
                                AlegraWorkflow.DocumentFailed(saleId, ex is OperationCanceledException ? "Guardado de PDF cancelado; puede reimprimir desde el detalle de venta." : ex is InvalidOperationException ? ex.Message : "No se pudo descargar o imprimir el PDF electronico. Revise el documento y la cola Windows.");
                                throw;
                            }
                        }
                    }
                },lifetime.Token);
                LastStatus=null;
            }
            catch(Exception ex){LastStatus=ex is InvalidOperationException?ex.Message:"Alegra pendiente: no se pudo completar el ciclo en segundo plano.";}
            finally {busy=false;}
        }
        public void Dispose(){if(Current==this)Current=null;timer.Stop();timer.Dispose();lifetime.Cancel();}
    }
}
