using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;

namespace CapaPresentacion.Formularios.FormsPedido
{
    /// <summary>
    /// Miniaturas de las fotos de platos y bebidas. Leer la foto original de cada producto era lo mas lento de
    /// la lista: aqui se reduce una sola vez al tamano en que se muestra y se guarda para toda la sesion, asi
    /// que volver a una pagina o abrir otro pedido ya no toca el disco.
    /// </summary>
    internal static class ProductThumbnails
    {
        // El doble del recuadro de ProductoItem (149 x 140) para que no se vea borrosa con escalado de pantalla.
        private static readonly Size ThumbnailSize = new Size(298, 280);
        private static readonly Dictionary<string, Image> cache = new Dictionary<string, Image>();
        private static readonly object cacheLock = new object();

        /// <summary>Miniatura de la foto, o null si el producto no tiene foto o no se pudo leer.</summary>
        public static Image Get(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return null;
            try
            {
                string folder = Convert.ToString(ConfigurationManager.AppSettings["RUTAIMAGES"]);
                FileInfo file = new FileInfo(Path.Combine(folder, fileName));
                if (!file.Exists)
                    return null;

                // La fecha entra en la clave: si cambian la foto de un producto se vuelve a leer.
                string key = file.FullName.ToUpperInvariant() + "|" + file.LastWriteTimeUtc.Ticks;
                lock (cacheLock)
                {
                    Image cached;
                    if (cache.TryGetValue(key, out cached))
                        return cached;
                }

                Image thumbnail;
                // Se lee por un flujo propio: Image.FromFile deja el fichero bloqueado mientras viva la imagen.
                using (FileStream stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (Image original = Image.FromStream(stream))
                {
                    Bitmap bitmap = new Bitmap(ThumbnailSize.Width, ThumbnailSize.Height);
                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        g.InterpolationMode = InterpolationMode.HighQualityBilinear;
                        g.DrawImage(original, 0, 0, ThumbnailSize.Width, ThumbnailSize.Height);
                    }
                    thumbnail = bitmap;
                }

                lock (cacheLock)
                {
                    Image cached;
                    if (cache.TryGetValue(key, out cached))
                    {
                        // Otro hilo la genero entre tanto: se usa la suya.
                        thumbnail.Dispose();
                        return cached;
                    }
                    cache[key] = thumbnail;
                }
                return thumbnail;
            }
            catch (Exception)
            {
                // Una foto danada o una ruta mal configurada no deben impedir tomar el pedido.
                return null;
            }
        }
    }
}
