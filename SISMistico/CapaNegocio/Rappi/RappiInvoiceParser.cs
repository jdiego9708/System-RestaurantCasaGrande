using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CapaEntidades.Models;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace CapaNegocio.Rappi
{
    /// <summary>
    /// Lee los datos de una factura PDF de Rappi (el comprobante que imprime el restaurante: orden, cliente, productos
    /// y totales). El PDF trae texto real, asi que no hace falta OCR. El analisis va por lineas y etiquetas
    /// ("Fecha orden:", "Total a cobrar al cliente"...) y nunca lanza por un dato que falte: devuelve lo que pudo leer
    /// y marca la orden como incompleta.
    /// </summary>
    public static class RappiInvoiceParser
    {
        // Dos palabras estan en la misma linea si su base difiere menos que esto (en puntos del PDF).
        private const double LineTolerance = 2.5;

        private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

        /// <summary>Texto del PDF ordenado por lineas, de arriba abajo. Lanza si el archivo no es un PDF legible.</summary>
        public static List<string> ReadLines(byte[] pdf)
        {
            List<string> lines = new List<string>();
            using (PdfDocument document = PdfDocument.Open(pdf))
            {
                foreach (Page page in document.GetPages())
                {
                    List<Word> words = page.GetWords().ToList();
                    List<List<Word>> rows = new List<List<Word>>();
                    // Se agrupa por la linea base de la letra (no por el borde inferior de la palabra, que cambia con las
                    // letras que bajan, como la "p" o la "g").
                    foreach (Word word in words.OrderByDescending(w => Baseline(w)))
                    {
                        List<Word> row = rows.LastOrDefault();
                        if (row != null && Math.Abs(Baseline(row[0]) - Baseline(word)) <= LineTolerance)
                            row.Add(word);
                        else
                            rows.Add(new List<Word> { word });
                    }
                    foreach (List<Word> row in rows)
                    {
                        string line = string.Join(" ", row.OrderBy(w => w.BoundingBox.Left).Select(w => w.Text)).Trim();
                        if (line.Length > 0)
                            lines.Add(line);
                    }
                }
            }
            return lines;
        }

        private static double Baseline(Word word) => word.Letters[0].StartBaseLine.Y;

        /// <summary>Lee los datos de la factura. No lanza por datos que falten; si el PDF no se puede abrir, si lanza.</summary>
        public static RappiOrder Parse(byte[] pdf, string sourceFile, string fileHash)
        {
            RappiOrder order = ParseLines(ReadLines(pdf));
            order.SourceFile = sourceFile;
            order.FileHash = fileHash;
            if (string.IsNullOrEmpty(order.ExternalId))
            {
                // Sin ID no hay clave natural: la huella del archivo evita duplicar el mismo PDF.
                order.ExternalId = "HASH:" + (fileHash ?? Guid.NewGuid().ToString("N")).Substring(0, Math.Min(40, (fileHash ?? "x").Length));
                order.IsComplete = false;
            }
            return order;
        }

        /// <summary>Analisis puro sobre las lineas del documento (se prueba sin PDF).</summary>
        public static RappiOrder ParseLines(IList<string> rawLines)
        {
            List<string> lines = JoinHyphenated(rawLines);
            RappiOrder order = new RappiOrder();

            // El nombre del cliente es lo primero que imprime Rappi, antes de "ID ...".
            int idIndex = lines.FindIndex(l => Regex.IsMatch(l, @"^ID\s*\d+", RegexOptions.IgnoreCase));
            if (idIndex > 0)
                order.CustomerName = lines[idIndex - 1].Trim();
            if (idIndex >= 0)
                order.ExternalId = Regex.Match(lines[idIndex], @"\d+").Value;

            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i];
                string value;
                if (TryLabel(line, "Plataforma", out value)) order.Platform = value;
                else if (TryLabel(line, "Fecha orden", out value)) order.OrderDateTime = ParseDate(value);
                else if (TryLabel(line, "Tel[eé]fono", out value)) order.Phone = value;
                else if (TryLabel(line, "M[eé]todo de pago", out value)) order.PaymentMethod = value;
                else if (TryLabel(line, "Direcci[oó]n", out value))
                {
                    // La direccion puede ocupar varias lineas hasta la siguiente etiqueta.
                    StringBuilder address = new StringBuilder(value);
                    while (i + 1 < lines.Count && !IsKnownLabel(lines[i + 1]))
                        address.Append(' ').Append(lines[++i].Trim());
                    order.Address = Regex.Replace(address.ToString(), @"\s+", " ").Trim();
                }
            }

            ParseItems(lines, order);
            ParseTotals(lines, order);

            Match payWith = Regex.Match(string.Join("\n", lines), @"va a pagar con\s*(-?\$?\s*[\d.,]+)", RegexOptions.IgnoreCase);
            if (payWith.Success) order.CashPayWith = Money(payWith.Groups[1].Value);
            Match change = Regex.Match(string.Join("\n", lines), @"llevar\s*(\$\s*[\d.,]+)\s*de cambio", RegexOptions.IgnoreCase);
            if (change.Success) order.ChangeToBring = Money(change.Groups[1].Value);
            Match code = Regex.Match(string.Join("\n", lines), @"C[oó]digo de entrega\s*(\S+)", RegexOptions.IgnoreCase);
            if (code.Success) order.DeliveryCode = code.Groups[1].Value;

            // Sin total o sin productos la ficha se muestra, pero avisando que hay que revisar el PDF.
            order.IsComplete = !string.IsNullOrEmpty(order.ExternalId) && order.Total.HasValue && order.Items.Count > 0;
            return order;
        }

        // Rappi parte las palabras largas con un guion especial al final de la linea ("aparta‐" + "mento").
        private static List<string> JoinHyphenated(IList<string> raw)
        {
            List<string> result = new List<string>();
            foreach (string line in raw)
            {
                string clean = line.Trim();
                if (clean.Length == 0) continue;
                if (result.Count > 0 && (result[result.Count - 1].EndsWith("‐") || result[result.Count - 1].EndsWith("‑")))
                    result[result.Count - 1] = result[result.Count - 1].TrimEnd('‐', '‑') + clean;
                else
                    result.Add(clean);
            }
            return result;
        }

        private static readonly string[] Labels =
        {
            "Restaurante", "Plataforma", "Fecha orden", "Tel[eé]fono", "Direcci[oó]n", "M[eé]todo de pago",
            "DETALLE ORDEN", "Subtotal", "Descuento", "Costo de env[ií]o", "Propina", "Tarifa de servicio",
            "Valor pagado", "Total a cobrar", "QR DE ENTREGA",
        };

        private static bool IsKnownLabel(string line) =>
            Labels.Any(l => Regex.IsMatch(line, "^" + l, RegexOptions.IgnoreCase));

        private static bool TryLabel(string line, string label, out string value)
        {
            Match match = Regex.Match(line, "^" + label + @"\s*:\s*(.*)$", RegexOptions.IgnoreCase);
            value = match.Success ? match.Groups[1].Value.Trim() : null;
            return match.Success;
        }

        private static void ParseItems(List<string> lines, RappiOrder order)
        {
            int start = lines.FindIndex(l => Regex.IsMatch(l, @"^DETALLE ORDEN", RegexOptions.IgnoreCase));
            if (start < 0) return;
            RappiOrderItem previous = null;
            string group = null;
            for (int i = start + 1; i < lines.Count; i++)
            {
                string line = lines[i];
                if (Regex.IsMatch(line, @"^Subtotal", RegexOptions.IgnoreCase)) break;
                if (Regex.IsMatch(line, @"^PRODUCTOS\s*:", RegexOptions.IgnoreCase)) continue;

                Match item = Regex.Match(line, @"^(\d+)\s*x\s*(.+?)\s*(-?\$\s*[\d.]+)\s*$", RegexOptions.IgnoreCase);
                if (item.Success)
                {
                    previous = new RappiOrderItem
                    {
                        Quantity = int.Parse(item.Groups[1].Value, Invariant),
                        Name = item.Groups[2].Value.Trim(),
                        Price = Money(item.Groups[3].Value) ?? 0,
                    };
                    order.Items.Add(previous);
                }
                else if (previous != null)
                {
                    // Menus con opciones: "ELIGE TU ARROZ:" y debajo "1x Arroz de Coco -" (sin precio). Son opciones del
                    // producto, no productos ni parte de su nombre.
                    Match option = Regex.Match(line, @"^\d+\s*x\s*(.+?)\s*-?\s*$", RegexOptions.IgnoreCase);
                    if (line.EndsWith(":"))
                        group = line.TrimEnd(':').Trim();
                    else if (option.Success && group != null)
                        previous.Notes.Add(group + ": " + option.Groups[1].Value.Trim().TrimEnd('-').Trim());
                    else
                        // Linea sin cantidad: sigue el nombre del producto anterior (nombre largo partido).
                        previous.Name += " " + line;
                }
            }
        }

        private static void ParseTotals(List<string> lines, RappiOrder order)
        {
            foreach (string line in lines)
            {
                if (Regex.IsMatch(line, @"^Total a cobrar", RegexOptions.IgnoreCase)) order.Total = LastMoney(line);
                else if (Regex.IsMatch(line, @"^Subtotal", RegexOptions.IgnoreCase)) order.Subtotal = LastMoney(line);
                else if (Regex.IsMatch(line, @"^Descuento", RegexOptions.IgnoreCase)) order.Discount = LastMoney(line);
                else if (Regex.IsMatch(line, @"^Costo de env[ií]o", RegexOptions.IgnoreCase)) order.ShippingCost = LastMoney(line);
                else if (Regex.IsMatch(line, @"^Propina", RegexOptions.IgnoreCase)) order.Tip = LastMoney(line);
                else if (Regex.IsMatch(line, @"^Tarifa de servicio", RegexOptions.IgnoreCase)) order.ServiceFee = LastMoney(line);
                else if (Regex.IsMatch(line, @"^Valor pagado", RegexOptions.IgnoreCase)) order.PaidThroughRappi = LastMoney(line);
            }
        }

        private static int? LastMoney(string line)
        {
            MatchCollection matches = Regex.Matches(line, @"-?\$\s*[\d.]+|-\s*\$\s*[\d.]+");
            return matches.Count == 0 ? null : Money(matches[matches.Count - 1].Value);
        }

        /// <summary>"$45.250" -> 45250; "-$21.850" -> -21850. Los puntos son separador de miles.</summary>
        public static int? Money(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            bool negative = text.Contains("-");
            string digits = Regex.Replace(text, @"[^\d]", string.Empty);
            if (digits.Length == 0) return null;
            int value;
            if (!int.TryParse(digits, NumberStyles.Integer, Invariant, out value)) return null;
            return negative ? -value : value;
        }

        private static DateTime? ParseDate(string text)
        {
            DateTime date;
            string[] formats = { "yyyy-MM-dd HH:mm", "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd", "dd/MM/yyyy HH:mm" };
            return DateTime.TryParseExact(text.Trim(), formats, Invariant, DateTimeStyles.None, out date) ? date : (DateTime?)null;
        }
    }
}
