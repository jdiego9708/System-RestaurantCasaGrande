using System;
using System.Configuration;
using System.Drawing.Printing;
using System.Globalization;
using System.Linq;

namespace CapaPresentacion
{
    public enum PaperKind
    {
        Unknown,
        Roll58,
        Roll80,
        Sheet,
    }

    // Papel de la impresora que se va a usar, calculado en el momento de imprimir.
    public sealed class PrinterPaperInfo
    {
        public string PrinterName { get; set; }
        public PaperKind Kind { get; set; }
        // Ancho imprimible que anuncia el driver, en centésimas de pulgada (como PaperSize.Width).
        public int PrintableWidth { get; set; }
        public string Source { get; set; }

        public double PrintableMm { get { return PrintableWidth * 0.254; } }
        public bool IsRoll { get { return Kind == PaperKind.Roll58 || Kind == PaperKind.Roll80; } }
        // Etiqueta para nombres de archivo y mensajes: "58mm" o "80mm".
        public string Label { get { return Kind == PaperKind.Roll58 ? "58mm" : "80mm"; } }

        // Ancho imprimible en milímetros para el generador de la factura electrónica (72 si no se pudo medir).
        public double RendererMm
        {
            get { return IsRoll && PrintableWidth > 0 ? Math.Round(Math.Min(PrintableMm, 80), 1) : 72; }
        }

        public override string ToString()
        {
            return PrinterName + ": " + Kind + " (" + Math.Round(PrintableMm, 1).ToString("0.#", CultureInfo.InvariantCulture)
                + " mm imprimibles, " + Source + ")";
        }
    }

    // Detecta el ancho de papel de la impresora conectada cada vez que se imprime: ni 58 ni 80 mm van fijos en el código.
    // El driver informa el ancho imprimible (XP-58: 48 mm; POS-80: 72 mm). Solo si hace falta forzarlo, la clave
    // AnchoPapelMm de App.config admite "58" u "80" ("auto" o vacío = detectar).
    public static class PrinterPaper
    {
        // Los rollos de tickets miden menos de 4 pulgadas; una hoja (A4, Carta) mide más.
        public const int RollMaxWidth = 400;
        private const int Roll58MaxWidth = 220;  // hasta 56 mm imprimibles
        private const int Roll80MaxWidth = 354;  // hasta 90 mm imprimibles

        public static PrinterPaperInfo Detect(string printerName = null)
        {
            var settings = new PrinterSettings();
            if (!string.IsNullOrWhiteSpace(printerName)) settings.PrinterName = printerName;
            var info = new PrinterPaperInfo { PrinterName = settings.PrinterName };

            string forced = (ConfigurationManager.AppSettings["AnchoPapelMm"] ?? "auto").Trim();
            if (forced == "58") return Forced(info, PaperKind.Roll58, 189);
            if (forced == "80") return Forced(info, PaperKind.Roll80, 283);

            if (!settings.IsValid) { info.Kind = PaperKind.Unknown; info.PrintableWidth = 283; info.Source = "impresora no valida"; return info; }

            int width = settings.DefaultPageSettings.PaperSize.Width;
            var rolls = settings.PaperSizes.Cast<PaperSize>().Select(p => p.Width).Where(w => w > 0 && w <= RollMaxWidth).ToList();
            // Un driver de rollo puede tener una hoja grande como predeterminada: se usa el ancho de sus formatos de rollo.
            if ((width <= 0 || width > RollMaxWidth) && rolls.Count > 0) width = rolls.Max();
            info.PrintableWidth = width;
            info.Source = "driver";
            info.Kind = Classify(width);
            return info;
        }

        public static PaperKind Classify(int printableWidth)
        {
            if (printableWidth <= 0) return PaperKind.Unknown;
            if (printableWidth <= Roll58MaxWidth) return PaperKind.Roll58;
            if (printableWidth <= Roll80MaxWidth) return PaperKind.Roll80;
            return PaperKind.Sheet;
        }

        private static PrinterPaperInfo Forced(PrinterPaperInfo info, PaperKind kind, int width)
        {
            info.Kind = kind;
            info.PrintableWidth = width;
            info.Source = "forzado en AnchoPapelMm";
            return info;
        }
    }
}
