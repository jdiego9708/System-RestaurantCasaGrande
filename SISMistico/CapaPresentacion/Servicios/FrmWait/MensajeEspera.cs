using System;
using System.Threading;

namespace CapaPresentacion
{
    public class MensajeEspera
    {
        private delegate void CloseDelegate();
        private static FrmWait frmWait;
        // Se activa cuando la ventana ya se ve: sin esto, un CloseForm inmediato llegaba antes de que el hilo creara
        // la ventana, no la cerraba y quedaba por encima de los dialogos que se abrian despues.
        private static readonly ManualResetEventSlim shown = new ManualResetEventSlim(false);

        static public void ShowWait(string mensaje)
        {
            if (frmWait != null)
                return;

            shown.Reset();
            Thread thread = new Thread(new ThreadStart(() => ShowForm(mensaje)))
            {
                IsBackground = true
            };
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            // Espera acotada: si la ventana no llega a mostrarse, el flujo continua igual.
            shown.Wait(3000);
        }

        static private void ShowForm(string mensaje)
        {
            frmWait = new FrmWait
            {
                StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen,
                Mensaje = mensaje
            };
            frmWait.Shown += (s, e) => shown.Set();
            frmWait.ShowDialog();
        }

        static public void CloseForm()
        {
            try
            {
                if (frmWait == null)
                    return;

                frmWait.Invoke(new CloseDelegate(CloseFormInternal));
            }
            catch (Exception)
            {
                return;
            }
        }

        static private void CloseFormInternal()
        {
            frmWait.Close();
            frmWait = null;
        }
    }
}
