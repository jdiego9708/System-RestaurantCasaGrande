using System.Globalization;
using System.Text.RegularExpressions;

namespace CapaEntidades.Helpers
{
    public static class OrderNumberSearch
    {
        // N1, N - 1, N°1 y R1 se refieren al consecutivo, no al ID del pedido ni al telefono.
        public static bool TryParse(string text, char prefix, out int number)
        {
            number = 0;
            if (string.IsNullOrWhiteSpace(text)) return false;
            Match match = Regex.Match(text, @"^\s*([NR])\s*[-\u2013\u2014]?\s*[\u00B0\u00BA#]?\s*([0-9]+)\s*$",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            return match.Success && char.ToUpperInvariant(match.Groups[1].Value[0]) == char.ToUpperInvariant(prefix)
                && int.TryParse(match.Groups[2].Value, NumberStyles.None, CultureInfo.InvariantCulture, out number) && number > 0;
        }
    }
}
