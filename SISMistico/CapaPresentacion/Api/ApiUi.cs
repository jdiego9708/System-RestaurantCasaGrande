using System;
using System.Windows.Forms;

namespace CapaPresentacion.Api
{
    /// <summary>
    /// Puente al hilo de la interfaz. Se crea en el hilo de UI (Iniciar() se llama desde Main) y desde ahí
    /// cualquier hilo puede encolar trabajo con Ejecutar() sin esperar. Nunca se toca un control desde el listener.
    /// </summary>
    internal static class ApiUi
    {
        private static Control _invocador;

        public static void Iniciar()
        {
            try
            {
                Control c = new Control();
                IntPtr forzarHandle = c.Handle;   // crea el handle en este hilo (el de UI)
                _invocador = c;
            }
            catch (Exception ex)
            {
                ApiLog.Error("No se pudo crear el puente al hilo de UI", ex);
            }
        }

        /// <summary>True si hay puente al hilo de UI (la API está activa y la interfaz creada).</summary>
        public static bool Disponible
        {
            get
            {
                Control c = _invocador;
                return c != null && !c.IsDisposed && c.IsHandleCreated;
            }
        }

        public static void Detener()
        {
            try { if (_invocador != null) _invocador.Dispose(); } catch { }
            _invocador = null;
        }

        /// <summary>Encola la acción en el hilo de UI y vuelve ya. Una excepción dentro de la acción se registra, no se propaga.</summary>
        public static bool Ejecutar(string nombre, Action accion)
        {
            Control c = _invocador;
            if (c == null || c.IsDisposed || !c.IsHandleCreated)
                return false;
            try
            {
                c.BeginInvoke(new Action(() =>
                {
                    try { accion(); }
                    catch (Exception ex) { ApiLog.Error("Fallo en el hilo de UI (" + nombre + ")", ex); }
                }));
                return true;
            }
            catch (Exception ex)
            {
                ApiLog.Error("No se pudo encolar en el hilo de UI (" + nombre + ")", ex);
                return false;
            }
        }
    }
}
