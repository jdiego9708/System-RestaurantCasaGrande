using System;
using System.Net;
using System.Threading.Tasks;

namespace CapaPresentacion.Api
{
    /// <summary>
    /// Ciclo de vida del HttpListener. Iniciar() nunca lanza: si la API no puede arrancar, se registra
    /// el motivo y el escritorio sigue funcionando igual que sin ella.
    /// </summary>
    public static class ApiServer
    {
        private static readonly object Candado = new object();
        private static HttpListener _listener;

        public static bool Activo { get; private set; }

        public static bool Iniciar()
        {
            lock (Candado)
            {
                if (Activo)
                    return true;
                if (!ApiConfig.Habilitada)
                    return false;

                try
                {
                    // Sin clave propia solo se admite escuchar en localhost (desarrollo).
                    if (ApiConfig.Clave == ApiConfig.ClavePlaceholder && !ApiConfig.PrefijoLocal)
                    {
                        ApiLog.Error("API no iniciada: ApiKey sigue con el valor de ejemplo y el prefijo no es local. " +
                                     "Define una ApiKey propia en App.config.", null);
                        return false;
                    }

                    ApiLog.Depurar();
                    _listener = new HttpListener();
                    _listener.Prefixes.Add(ApiConfig.Prefijo);
                    _listener.TimeoutManager.HeaderWait = TimeSpan.FromSeconds(10);
                    _listener.TimeoutManager.EntityBody = TimeSpan.FromSeconds(10);
                    _listener.TimeoutManager.IdleConnection = TimeSpan.FromSeconds(60);
                    _listener.Start();
                    Activo = true;
                    ApiUi.Iniciar();   // Iniciar() corre en el hilo de UI (Main): el puente se crea aquí
                    HttpListener escucha = _listener;
                    _ = Task.Run(() => Bucle(escucha));
                    ApiLog.Info("API escuchando en " + ApiConfig.Prefijo);
                    return true;
                }
                catch (HttpListenerException ex)
                {
                    string pista = ex.ErrorCode == 5
                        ? " Acceso denegado: falta la reserva de URL (docs/03 §8.2: netsh http add urlacl) o usa un prefijo localhost."
                        : ex.ErrorCode == 32 || ex.ErrorCode == 183 ? " El puerto ya está en uso." : "";
                    ApiLog.Error("API no iniciada (" + ex.ErrorCode + ")." + pista, ex);
                    Limpiar();
                    return false;
                }
                catch (Exception ex)
                {
                    ApiLog.Error("API no iniciada", ex);
                    Limpiar();
                    return false;
                }
            }
        }

        public static void Detener()
        {
            lock (Candado)
            {
                if (!Activo)
                    return;
                Limpiar();
                ApiLog.Info("API detenida");
            }
        }

        private static void Limpiar()
        {
            Activo = false;
            ApiUi.Detener();
            try { if (_listener != null) _listener.Close(); } catch { }
            _listener = null;
        }

        private static async Task Bucle(HttpListener listener)
        {
            while (listener.IsListening)
            {
                HttpListenerContext ctx;
                try
                {
                    ctx = await listener.GetContextAsync().ConfigureAwait(false);
                }
                catch (HttpListenerException) { break; }
                catch (ObjectDisposedException) { break; }
                catch (InvalidOperationException) { break; }

                HttpListenerContext c = ctx;
                _ = Task.Run(() => ApiRouter.Despachar(c));
            }
        }
    }
}
