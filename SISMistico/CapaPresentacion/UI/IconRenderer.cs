using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using FontAwesome.Sharp;

namespace CapaPresentacion.UI
{
    internal static class IconRenderer
    {
        // La primera carga de fuentes puede procesar mensajes de Windows y provocar otro Paint.
        // No se debe volver a entrar en FontAwesome mientras su Lazy<FontFamily> se inicializa.
        [ThreadStatic] private static bool creatingBitmap;
        [ThreadStatic] private static Dictionary<Tuple<IconChar, int, int>, Bitmap> cache;
        [ThreadStatic] private static HashSet<Control> waiting;

        public static void Draw(Control owner, Graphics graphics, IconChar icon, Color color, Rectangle bounds)
        {
            if (icon == IconChar.None || bounds.Width <= 0 || bounds.Height <= 0) return;
            if (cache == null) cache = new Dictionary<Tuple<IconChar, int, int>, Bitmap>();
            var key = Tuple.Create(icon, color.ToArgb(), bounds.Width);
            Bitmap bitmap;
            if (!cache.TryGetValue(key, out bitmap))
            {
                if (creatingBitmap)
                {
                    if (waiting == null) waiting = new HashSet<Control>();
                    waiting.Add(owner);
                    return;
                }
                creatingBitmap = true;
                try
                {
                    bitmap = icon.ToBitmap(color, bounds.Width);
                    if (cache.Count >= 128)
                    {
                        foreach (Bitmap previous in cache.Values) previous.Dispose();
                        cache.Clear();
                    }
                    cache.Add(key, bitmap);
                }
                finally
                {
                    creatingBitmap = false;
                    if (waiting != null)
                    {
                        foreach (Control control in waiting)
                            if (!control.IsDisposed && control.IsHandleCreated) control.Invalidate();
                        waiting.Clear();
                    }
                }
            }
            graphics.DrawImage(bitmap, bounds);
        }
    }
}
