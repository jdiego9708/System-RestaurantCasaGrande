using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Web.Script.Serialization;

namespace CapaPresentacion.UI
{
    // Coleccion oficial lucide-static 1.76.0, embebida con su licencia ISC/MIT y mapa completo.
    internal static class LucideIcons
    {
        private static readonly Lazy<IconFont> Source = new Lazy<IconFont>(() => new IconFont());

        public static bool Contains(string name) => name != null && Source.Value.Codes.ContainsKey(name);

        public static void Draw(Graphics graphics, string name, Color color, Rectangle bounds)
        {
            if (string.IsNullOrEmpty(name) || bounds.Width <= 0 || bounds.Height <= 0) return;
            IconFont source = Source.Value;
            if (!source.Codes.TryGetValue(name, out int code)) return;
            using (var font = new Font(source.Family, Math.Min(bounds.Width, bounds.Height), FontStyle.Regular, GraphicsUnit.Pixel))
            using (var brush = new SolidBrush(color))
            using (var format = new StringFormat(StringFormat.GenericTypographic)
            {
                Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center,
                FormatFlags = StringFormatFlags.NoClip,
            })
                graphics.DrawString(char.ConvertFromUtf32(code), font, brush, bounds, format);
        }

        private sealed class IconFont : IDisposable
        {
            private readonly PrivateFontCollection fonts = new PrivateFontCollection();
            private readonly IntPtr memory;
            public readonly FontFamily Family;
            public readonly Dictionary<string, int> Codes;

            public IconFont()
            {
                var assembly = typeof(LucideIcons).Assembly;
                const string prefix = "CapaPresentacion.Resources.Lucide.";
                using (Stream stream = assembly.GetManifestResourceStream(prefix + "lucide.ttf"))
                {
                    if (stream == null) throw new InvalidOperationException("No se encontro la coleccion Lucide embebida.");
                    byte[] bytes = new byte[checked((int)stream.Length)];
                    using (var reader = new BinaryReader(stream)) bytes = reader.ReadBytes(bytes.Length);
                    memory = Marshal.AllocHGlobal(bytes.Length);
                    Marshal.Copy(bytes, 0, memory, bytes.Length);
                    fonts.AddMemoryFont(memory, bytes.Length);
                }
                Family = fonts.Families[0];
                using (var reader = new StreamReader(assembly.GetManifestResourceStream(prefix + "codepoints.json")))
                    Codes = new JavaScriptSerializer().Deserialize<Dictionary<string, int>>(reader.ReadToEnd());
                AppDomain.CurrentDomain.ProcessExit += (s, e) => Dispose();
            }

            public void Dispose()
            {
                Family.Dispose();
                fonts.Dispose();
                Marshal.FreeHGlobal(memory);
            }
        }
    }
}
