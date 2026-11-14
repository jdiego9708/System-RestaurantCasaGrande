using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using Microsoft.Reporting.WinForms;

namespace CapaPresentacion
{
    // Los tickets (comandas, facturas, precuentas, ingresos y egresos) se diseñaron para rollo de 80 mm (cuerpo de
    // 7,8 cm). Para un rollo más estrecho, como el de 58 mm (48 mm imprimibles), no se reduce la imagen terminada,
    // que dejaría el texto diminuto: se reajusta el diseño al ancho real. Todos los anchos y posiciones
    // horizontales se escalan y las fuentes bajan un poco, de modo que los nombres largos pasan a varias líneas.
    internal static class ReceiptLayoutAdapter
    {
        // En 58 mm las fuentes son un 15 % menores; más abajo la térmica de 203 ppp ya no se lee bien.
        private const double FontScale = 0.85;
        private const double MinimumFontPoints = 6.5;
        // Holgura entre el cuerpo y el borde de la página (los 7,8 cm del diseño sobre 8,0 cm de hoja).
        private const double SlackCm = 0.2;
        // Si el diseño ya cabe (hasta un 3 % de diferencia) no se toca.
        private const double FitTolerance = 0.97;
        private static readonly Regex Measure = new Regex(@"^\s*(-?\d+(?:\.\d+)?)\s*(cm|mm|in|pt|pc)\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        private static readonly HashSet<string> BorderParents = new HashSet<string>
        {
            "Border", "TopBorder", "BottomBorder", "LeftBorder", "RightBorder",
        };

        // Devuelve un informe nuevo con el diseño reajustado al ancho imprimible (centésimas de pulgada), o null si no
        // hace falta o no se puede (el llamador conserva entonces el informe original y lo reduce como antes).
        // El llamador debe liberar el informe devuelto.
        public static LocalReport Adapt(LocalReport source, int printableWidthHundredths)
        {
            XmlDocument xml = LoadDefinition(source);
            if (xml == null) return null;
            var ns = new XmlNamespaceManager(xml.NameTable);
            ns.AddNamespace("r", xml.DocumentElement.NamespaceURI);
            double designCm = DesignWidthCm(xml, ns);
            double targetCm = printableWidthHundredths / 100.0 * 2.54 - SlackCm;
            if (designCm <= 0 || targetCm <= 0 || targetCm >= designCm * FitTolerance) return null;

            Scale(xml, targetCm / designCm);
            ScaleImages(xml, targetCm / designCm);
            CompactTableHeaders(xml);
            RebalanceDetailColumns(xml);

            var adapted = new LocalReport();
            try
            {
                using (var stream = new MemoryStream())
                {
                    xml.Save(stream);
                    stream.Position = 0;
                    adapted.LoadReportDefinition(stream);
                }
                adapted.EnableExternalImages = source.EnableExternalImages;
                adapted.EnableHyperlinks = source.EnableHyperlinks;
                foreach (ReportDataSource dataSource in source.DataSources)
                    adapted.DataSources.Add(new ReportDataSource(dataSource.Name, dataSource.Value));
                var parameters = new List<ReportParameter>();
                foreach (ReportParameterInfo info in source.GetParameters())
                    if (info.Values != null && info.Values.Count > 0) parameters.Add(new ReportParameter(info.Name, info.Values.ToArray()));
                if (parameters.Count > 0) adapted.SetParameters(parameters);
                return adapted;
            }
            catch
            {
                adapted.Dispose();
                throw;
            }
        }

        private static XmlDocument LoadDefinition(LocalReport report)
        {
            var xml = new XmlDocument { XmlResolver = null };
            if (!string.IsNullOrEmpty(report.ReportEmbeddedResource))
            {
                using (Stream stream = typeof(ControladorImpresion).Assembly.GetManifestResourceStream(report.ReportEmbeddedResource))
                {
                    if (stream == null) return null;
                    xml.Load(stream);
                    return xml;
                }
            }
            if (!string.IsNullOrEmpty(report.ReportPath) && File.Exists(report.ReportPath))
            {
                xml.Load(report.ReportPath);
                return xml;
            }
            return null;
        }

        // Ancho del cuerpo del informe, en cm (esquema 2016: ReportSection/Width; esquemas anteriores: Report/Width).
        private static double DesignWidthCm(XmlDocument xml, XmlNamespaceManager ns)
        {
            XmlNode width = xml.SelectSingleNode("//r:ReportSection/r:Width", ns) ?? xml.SelectSingleNode("/r:Report/r:Width", ns);
            double cm;
            return width != null && TryParseCm(width.InnerText, out cm) ? cm : 0;
        }

        // Las imagenes (el logo) se ajustan con FitProportional: al estrechar el diseño su ancho baja con el factor, pero
        // su alto no, y la imagen quedaba centrada en una caja mas alta que ella (un hueco grande sobre y bajo el logo).
        // El alto baja igual que el ancho y lo que hay debajo de la imagen sube lo que ella se achico.
        private static void ScaleImages(XmlDocument xml, double factor)
        {
            foreach (XmlElement image in xml.SelectNodes("//*").Cast<XmlElement>().Where(e => e.LocalName == "Image").ToList())
            {
                XmlElement items = image.ParentNode as XmlElement;
                XmlElement body = items == null ? null : items.ParentNode as XmlElement;
                if (items == null || items.LocalName != "ReportItems" || body == null || body.LocalName != "Body") continue;

                XmlElement top = ChildNamed(image, "Top");
                XmlElement height = ChildNamed(image, "Height");
                double topCm, heightCm;
                if (height == null || !TryParseCm(height.InnerText, out heightCm)) continue;
                topCm = 0;
                if (top != null && !TryParseCm(top.InnerText, out topCm)) continue;

                double shrink = heightCm - heightCm * factor;
                height.InnerText = FormatCm(heightCm * factor);
                // Lo que empieza por debajo de la mitad de la imagen se considera debajo de ella (el titulo puede
                // montarse unos milimetros sobre su borde inferior).
                double belowFrom = topCm + heightCm / 2;
                foreach (XmlElement sibling in items.ChildNodes.OfType<XmlElement>())
                {
                    XmlElement siblingTop = ChildNamed(sibling, "Top");
                    double value;
                    if (sibling == image || siblingTop == null || !TryParseCm(siblingTop.InnerText, out value)) continue;
                    if (value > belowFrom)
                        siblingTop.InnerText = FormatCm(value - shrink);
                }
                XmlElement bodyHeight = ChildNamed(body, "Height");
                double bodyCm;
                if (bodyHeight != null && TryParseCm(bodyHeight.InnerText, out bodyCm))
                    bodyHeight.InnerText = FormatCm(bodyCm - shrink);
            }
        }

        private static XmlElement ChildNamed(XmlElement parent, string name) =>
            parent.ChildNodes.OfType<XmlElement>().FirstOrDefault(e => e.LocalName == name);

        private static string FormatCm(double value) =>
            value.ToString("0.#####", CultureInfo.InvariantCulture) + "cm";

        private static void Scale(XmlDocument xml, double factor)
        {
            foreach (XmlElement element in xml.SelectNodes("//*").Cast<XmlElement>().ToList())
            {
                string name = element.LocalName;
                if (name == "Width" || name == "Left")
                {
                    // El grosor de un borde es un "Width" que no es horizontal ni debe escalarse.
                    var parent = element.ParentNode as XmlElement;
                    if (parent != null && BorderParents.Contains(parent.LocalName)) continue;
                    Match match = Measure.Match(element.InnerText);
                    if (!match.Success) continue;
                    double value = double.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture) * factor;
                    element.InnerText = value.ToString("0.#####", CultureInfo.InvariantCulture) + match.Groups[2].Value.ToLowerInvariant();
                }
                else if (name == "FontSize")
                {
                    Match match = Measure.Match(element.InnerText);
                    if (!match.Success || !string.Equals(match.Groups[2].Value, "pt", StringComparison.OrdinalIgnoreCase)) continue;
                    double points = double.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
                    double scaled = Math.Max(Math.Min(points, MinimumFontPoints), Math.Round(points * FontScale * 2) / 2);
                    element.InnerText = scaled.ToString("0.#", CultureInfo.InvariantCulture) + "pt";
                }
            }
        }

        // Encabezados de la tabla de detalle en papel estrecho: texto corto (símbolo) y letra pequeña, para que no
        // ocupen varias líneas. Precio = $, cantidad = #, total de la línea = Σ.
        private static readonly Dictionary<string, string> HeaderSymbols = new Dictionary<string, string>
        {
            { "Precio", "$" },
            { "Cant.", "#" },
            { "Total", "Σ" },
        };
        private const string CompactHeaderFont = "6.5pt";

        private static void CompactTableHeaders(XmlDocument xml)
        {
            foreach (XmlElement value in xml.SelectNodes("//*").Cast<XmlElement>().Where(e => e.LocalName == "Value").ToList())
            {
                string text = value.InnerText.Trim();
                string symbol;
                bool isSymbol = HeaderSymbols.TryGetValue(text, out symbol);
                if (!isSymbol && text != "Nombre") continue;
                if (isSymbol) value.InnerText = symbol;
                var run = value.ParentNode as XmlElement;
                if (run == null || run.LocalName != "TextRun") continue;
                foreach (XmlElement style in run.ChildNodes.OfType<XmlElement>().Where(e => e.LocalName == "Style"))
                {
                    XmlElement size = style.ChildNodes.OfType<XmlElement>().FirstOrDefault(e => e.LocalName == "FontSize");
                    if (size == null)
                    {
                        // Algunas plantillas (la precuenta) no declaran tamaño y heredan el del informe: se fija uno.
                        // Orden del esquema: FontFamily, FontSize, FontStyle, FontWeight...
                        size = xml.CreateElement(style.Prefix, "FontSize", style.NamespaceURI);
                        XmlElement family = style.ChildNodes.OfType<XmlElement>().FirstOrDefault(e => e.LocalName == "FontFamily");
                        if (family != null) style.InsertAfter(size, family);
                        else style.PrependChild(size);
                    }
                    size.InnerText = CompactHeaderFont;
                }
            }
        }

        // Con encabezados cortos, los importes son lo que fija el ancho de las columnas: la tabla Nombre / $ / # / Σ
        // se reparte para que "$19800" y "$119800" quepan en una línea y el nombre use el resto.
        private static readonly double[] DetailColumnShare = { 0.38, 0.24, 0.09, 0.29 };

        private static void RebalanceDetailColumns(XmlDocument xml)
        {
            foreach (XmlElement tablix in xml.SelectNodes("//*").Cast<XmlElement>().Where(e => e.LocalName == "Tablix").ToList())
            {
                var columns = tablix.GetElementsByTagName("TablixColumn").Cast<XmlElement>().ToList();
                var firstRow = tablix.GetElementsByTagName("TablixRow").Cast<XmlElement>().FirstOrDefault();
                if (columns.Count != DetailColumnShare.Length || firstRow == null) continue;
                var headers = firstRow.GetElementsByTagName("TablixCell").Cast<XmlElement>()
                    .Select(cell => cell.GetElementsByTagName("Value").Cast<XmlElement>().Select(v => v.InnerText.Trim()).FirstOrDefault()).ToList();
                if (!headers.SequenceEqual(new[] { "Nombre", "$", "#", "Σ" })) continue;
                var widths = columns.Select(c => c.GetElementsByTagName("Width").Cast<XmlElement>().FirstOrDefault()).ToList();
                double total = 0;
                bool ok = true;
                foreach (XmlElement width in widths)
                {
                    double cm;
                    if (width == null || !TryParseCm(width.InnerText, out cm)) { ok = false; break; }
                    total += cm;
                }
                if (!ok) continue;
                for (int i = 0; i < widths.Count; i++)
                    widths[i].InnerText = (total * DetailColumnShare[i]).ToString("0.#####", CultureInfo.InvariantCulture) + "cm";
            }
        }

        private static bool TryParseCm(string text, out double cm)
        {
            cm = 0;
            Match match = Measure.Match(text ?? string.Empty);
            if (!match.Success) return false;
            double value = double.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
            switch (match.Groups[2].Value.ToLowerInvariant())
            {
                case "cm": cm = value; break;
                case "mm": cm = value / 10; break;
                case "in": cm = value * 2.54; break;
                case "pt": cm = value * 2.54 / 72; break;
                case "pc": cm = value * 2.54 / 6; break;
            }
            return true;
        }
    }
}
