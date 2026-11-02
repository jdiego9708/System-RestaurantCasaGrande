#if DEBUG
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace CapaPresentacion
{
    /// <summary>
    /// SOLO DEPURACIÓN. Registra en logs\form-close-trace.log quién cierra u oculta cada ventana del escritorio:
    /// la pila completa en el momento del FormClosing y cuando una ventana pasa a no visible. Sirve para saber qué
    /// cierra una ventana del escritorio cuando llega un pedido del móvil. No existe en la compilación Release.
    /// </summary>
    public static class FormCloseTracer
    {
        private static readonly HashSet<Form> Vistos = new HashSet<Form>();
        private static readonly object Candado = new object();
        private static Timer _timer;
        private static string _ruta;

        public static void Iniciar()
        {
            string carpeta = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            Directory.CreateDirectory(carpeta);
            _ruta = Path.Combine(carpeta, "form-close-trace.log");
            Escribir("=== Trazador de cierre de ventanas iniciado ===");

            _timer = new Timer { Interval = 300 };
            _timer.Tick += (s, e) => Revisar();
            _timer.Start();
        }

        private static void Revisar()
        {
            foreach (Form f in Application.OpenForms.Cast<Form>().ToArray())
            {
                if (!Vistos.Add(f))
                    continue;

                string nombre = f.GetType().Name + " '" + f.Text + "'";
                Escribir("ABRE   " + nombre);
                f.FormClosing += (s, e) =>
                    Escribir("CIERRA " + nombre + " razón=" + e.CloseReason + " cancelado=" + e.Cancel +
                             Environment.NewLine + Environment.StackTrace);
                f.VisibleChanged += (s, e) =>
                {
                    if (!f.Visible && !f.IsDisposed)
                        Escribir("OCULTA " + nombre + Environment.NewLine + Environment.StackTrace);
                };
                f.Disposed += (s, e) => Escribir("DISPOSE " + nombre);
            }
        }

        private static void Escribir(string texto)
        {
            lock (Candado)
            {
                try { File.AppendAllText(_ruta, DateTime.Now.ToString("HH:mm:ss.fff") + " " + texto + Environment.NewLine); }
                catch (Exception) { }
            }
        }
    }
}
#endif
