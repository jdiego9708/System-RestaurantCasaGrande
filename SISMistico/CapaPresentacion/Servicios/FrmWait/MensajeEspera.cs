using System;
using System.Threading;

namespace CapaPresentacion
{
    public class MensajeEspera
    {
        private delegate void CloseDelegate();
        private static FrmWait frmWait;

        static public void ShowWait(string mensaje)
        {
            if (frmWait != null)
                return;

            Thread thread = new Thread(new ThreadStart(() => ShowForm(mensaje)))
            {
                IsBackground = true
            };
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        static private void ShowForm(string mensaje)
        {
            frmWait = new FrmWait
            {
                StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen,
                Mensaje = mensaje
            };
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
