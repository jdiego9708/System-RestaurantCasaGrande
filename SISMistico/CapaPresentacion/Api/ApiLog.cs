using System;
using System.IO;
using System.Text;

namespace CapaPresentacion.Api
{
    /// <summary>Log diario logs\api-yyyyMMdd.log. Nunca lanza: un fallo de log no debe tumbar una petición.</summary>
    public static class ApiLog
    {
        private static readonly object Candado = new object();
        private static string _carpeta;

        public static void Info(string mensaje)
        {
            Escribir("INFO ", mensaje);
        }

        public static void Error(string mensaje, Exception ex)
        {
            // Solo tipo y mensaje: sin stack ni cuerpo de la petición (pueden llevar credenciales).
            Escribir("ERROR", ex == null ? mensaje : mensaje + " | " + ex.GetType().Name + ": " + ex.Message);
        }

        public static void Peticion(string metodo, string ruta, int status, long ms, string ip, string extra)
        {
            Escribir("REQ  ", string.Format("{0} {1} | {2} | {3} ms | ip={4}{5}",
                metodo, ruta, status, ms, ip, string.IsNullOrEmpty(extra) ? "" : " | " + extra));
        }

        /// <summary>Borra los logs de más de 30 días.</summary>
        public static void Depurar()
        {
            try
            {
                string c = Carpeta();
                foreach (string f in Directory.GetFiles(c, "api-*.log"))
                    if (File.GetLastWriteTime(f) < DateTime.Now.AddDays(-30))
                        File.Delete(f);
            }
            catch { }
        }

        private static void Escribir(string nivel, string mensaje)
        {
            try
            {
                string linea = string.Format("{0:yyyy-MM-dd HH:mm:ss} | {1} | {2}{3}",
                    DateTime.Now, nivel, mensaje.Replace("\r", " ").Replace("\n", " "), Environment.NewLine);
                lock (Candado)
                {
                    string archivo = Path.Combine(Carpeta(), "api-" + DateTime.Now.ToString("yyyyMMdd") + ".log");
                    File.AppendAllText(archivo, linea, new UTF8Encoding(false));
                }
            }
            catch { }
        }

        private static string Carpeta()
        {
            if (_carpeta != null)
                return _carpeta;

            string c = ApiConfig.CarpetaLogAbsoluta();
            try
            {
                Directory.CreateDirectory(c);
            }
            catch
            {
                c = Path.Combine(Path.GetTempPath(), "SISMistico-api-logs");
                Directory.CreateDirectory(c);
            }
            _carpeta = c;
            return c;
        }
    }
}
