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
        public AlegraBackgroundWorker()
        {
            timer.Tick+=async(s,e)=>await Tick();timer.Start();
        }
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
                    if(settings.Enabled&&settings.AutoPrint)
                    {
                        var pending=AlegraWorkflow.PendingPrints();
                        if(pending.Rows.Count>0)await AlegraPdfService.PrintAsync(Convert.ToInt32(pending.Rows[0]["SaleId"]),lifetime.Token).ConfigureAwait(false);
                    }
                },lifetime.Token);
                LastStatus=null;
            }
            catch(Exception ex){LastStatus=ex is InvalidOperationException?ex.Message:"Alegra pendiente: no se pudo completar el ciclo en segundo plano.";}
            finally {busy=false;}
        }
        public void Dispose(){timer.Stop();timer.Dispose();lifetime.Cancel();}
    }
}
