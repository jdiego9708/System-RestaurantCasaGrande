using System;
using System.Collections.Generic;
using System.IO;

namespace CapaPresentacion.Api
{
    /// <summary>
    /// GET /imagenes/{archivo}: sirve archivos de la carpeta de imágenes. Solo nombres simples con
    /// extensión de imagen; cualquier otra cosa responde 404 sin explicar por qué.
    /// </summary>
    internal static class ApiImagenesHandlers
    {
        private const long MaxBytes = 10L * 1024 * 1024;

        private static readonly Dictionary<string, string> Tipos =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { ".jpg", "image/jpeg" }, { ".jpeg", "image/jpeg" }, { ".png", "image/png" },
                { ".gif", "image/gif" }, { ".webp", "image/webp" }, { ".bmp", "image/bmp" }
            };

        public static ApiResponse Obtener(ApiRequest req)
        {
            string ruta = RutaSegura(req.Parametros["archivo"]);
            if (ruta == null)
                return ApiResponse.Error(404, "NO_ENCONTRADO", "Imagen inexistente.");

            FileInfo info = new FileInfo(ruta);
            if (!info.Exists || info.Length > MaxBytes)
                return ApiResponse.Error(404, "NO_ENCONTRADO", "Imagen inexistente.");

            // FileShare.ReadWrite: el escritorio puede tener el archivo abierto (Image.FromFile lo bloquea).
            byte[] bytes;
            using (FileStream fs = new FileStream(ruta, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                bytes = new byte[fs.Length];
                int leido = 0;
                while (leido < bytes.Length)
                {
                    int n = fs.Read(bytes, leido, bytes.Length - leido);
                    if (n <= 0) break;
                    leido += n;
                }
            }
            return ApiResponse.Archivo(bytes, Tipos[Path.GetExtension(ruta)]);
        }

        /// <summary>URL absoluta de la imagen, o null si no hay imagen (vacío, "SIN IMAGEN") o el archivo no existe.</summary>
        public static string UrlImagen(ApiRequest req, string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre) || nombre.Equals("SIN IMAGEN", StringComparison.OrdinalIgnoreCase))
                return null;
            string ruta = RutaSegura(nombre);
            if (ruta == null || !File.Exists(ruta))
                return null;
            return req.UrlBase + "/imagenes/" + Uri.EscapeDataString(nombre);
        }

        /// <summary>Ruta completa dentro de la carpeta de imágenes, o null si el nombre no es seguro.</summary>
        private static string RutaSegura(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre) || nombre.Length > 150)
                return null;
            if (nombre.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || nombre.Contains(".."))
                return null;
            string ext = Path.GetExtension(nombre);
            if (!Tipos.ContainsKey(ext))
                return null;

            string carpeta = ApiConfig.ImagenesCarpeta;
            if (string.IsNullOrWhiteSpace(carpeta) || !Directory.Exists(carpeta))
                return null;

            string baseCompleta = Path.GetFullPath(carpeta).TrimEnd('\\') + "\\";
            string completa = Path.GetFullPath(Path.Combine(baseCompleta, nombre));
            return completa.StartsWith(baseCompleta, StringComparison.OrdinalIgnoreCase) ? completa : null;
        }
    }
}
