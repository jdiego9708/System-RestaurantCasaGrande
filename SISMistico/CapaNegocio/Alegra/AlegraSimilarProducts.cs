using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CapaNegocio.Alegra
{
    public sealed class AlegraSimilarProduct
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public decimal? Price { get; set; }
        public decimal? TaxPercentage { get; set; }
        public string CompanyId { get; set; }
        public decimal Similarity { get; set; }
    }

    public static class AlegraSimilarProducts
    {
        public static async Task<List<AlegraSimilarProduct>> FindAsync(string name, CancellationToken token)
        {
            var settings = AlegraWorkflow.Settings();
            var rates = (await AlegraProductRegistration.TaxesAsync(token).ConfigureAwait(false))
                .Where(tax => tax.Id.Length > 0).GroupBy(tax => tax.Id).ToDictionary(group => group.Key, group => group.First().Percentage);
            using (var api = new AlegraClient())
            {
                string company = AlegraWorkflow.CompanyIdentity(AlegraWorkflow.Object(await api.GetCompanyAsync(token).ConfigureAwait(false)));
                if (!string.IsNullOrWhiteSpace(settings.CompanyId) && settings.CompanyId != company)
                    throw new InvalidOperationException("Las credenciales pertenecen a otra empresa de Alegra.");
                var result = new List<AlegraSimilarProduct>();
                var seen = new HashSet<string>();
                string normalized = Normalize(name);
                for (int start = 0; start < 30000; start += 30)
                {
                    token.ThrowIfCancellationRequested();
                    var page = AlegraProductValidation.ParseList(await api.GetItemsAsync(start, token).ConfigureAwait(false));
                    int previous = seen.Count;
                    foreach (var item in page)
                    {
                        string id = AlegraWorkflow.Text(item, "id");
                        if (string.IsNullOrWhiteSpace(id)) throw new InvalidOperationException("Alegra devolvió un producto sin identificador.");
                        if (!seen.Add(id)) continue;
                        string remoteName = AlegraWorkflow.Text(item, "name");
                        decimal score = Score(normalized, Normalize(remoteName));
                        if (score < .6m) continue;
                        decimal price, percentage;
                        result.Add(new AlegraSimilarProduct { Id = id, Name = remoteName, CompanyId = company, Similarity = score,
                            Price = AlegraProductValidation.TryPrice(item, out price) ? (decimal?)price : null,
                            TaxPercentage = AlegraProductValidation.TryTaxes(item, rates, out percentage) ? (decimal?)percentage : null });
                    }
                    if (page.Count < 30) return result.OrderByDescending(item => item.Similarity).ThenBy(item => item.Name).ToList();
                    if (seen.Count == previous) throw new InvalidOperationException("No se pudo completar la búsqueda de productos similares. Reintenta la acción.");
                }
                throw new InvalidOperationException("El catálogo excede el límite de consulta. No se pudo completar la búsqueda de similares.");
            }
        }

        internal static List<AlegraSimilarProduct> FindInCatalog(string name,
            IEnumerable<Dictionary<string, object>> items, Dictionary<string, decimal> rates, string company)
        {
            string normalized = Normalize(name);
            var result = new List<AlegraSimilarProduct>();
            foreach (var item in items)
            {
                string remoteName = AlegraWorkflow.Text(item, "name");
                decimal score = Score(normalized, Normalize(remoteName));
                if (score < .6m) continue;
                decimal price, percentage;
                result.Add(new AlegraSimilarProduct { Id = AlegraWorkflow.Text(item, "id"), Name = remoteName,
                    CompanyId = company, Similarity = score,
                    Price = AlegraProductValidation.TryPrice(item, out price) ? (decimal?)price : null,
                    TaxPercentage = AlegraProductValidation.TryTaxes(item, rates, out percentage) ? (decimal?)percentage : null });
            }
            return result.OrderByDescending(item => item.Similarity).ThenBy(item => item.Name).ToList();
        }

        private static string Normalize(string name)
        {
            var text = new StringBuilder();
            foreach (char value in (name ?? "").Normalize(NormalizationForm.FormD))
            {
                if (CharUnicodeInfo.GetUnicodeCategory(value) == UnicodeCategory.NonSpacingMark) continue;
                text.Append(char.IsLetterOrDigit(value) ? char.ToUpperInvariant(value) : ' ');
            }
            return string.Join(" ", text.ToString().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
        }

        private static decimal Score(string first, string second)
        {
            if (first.Length == 0 || second.Length == 0) return 0;
            if (first == second) return 1;
            if (Math.Min(first.Length, second.Length) >= 4 && (first.Contains(second) || second.Contains(first))) return .95m;
            var ignored = new HashSet<string> { "DE", "DEL", "LA", "EL", "LOS", "LAS", "CON", "SIN", "AL", "EN", "Y", "PLATO", "BEBIDA", "PORCION" };
            var left = new HashSet<string>(first.Split(' ').Where(word => word.Length >= 3 && !ignored.Contains(word)));
            var right = new HashSet<string>(second.Split(' ').Where(word => word.Length >= 3 && !ignored.Contains(word)));
            int common = left.Intersect(right).Count();
            decimal overlap = left.Count + right.Count == 0 ? 0 : 2m * common / (left.Count + right.Count);
            if (common > 0) overlap = Math.Max(.6m, overlap);
            // Distancia de edición para diferencias pequeñas de escritura, además de palabras compartidas.
            if (Math.Max(first.Length, second.Length) > 200) return overlap;
            var previous = Enumerable.Range(0, second.Length + 1).ToArray();
            for (int i = 1; i <= first.Length; i++)
            {
                var next = new int[second.Length + 1]; next[0] = i;
                for (int j = 1; j <= second.Length; j++)
                    next[j] = Math.Min(Math.Min(next[j - 1] + 1, previous[j] + 1), previous[j - 1] + (first[i - 1] == second[j - 1] ? 0 : 1));
                previous = next;
            }
            decimal spelling = 1m - (decimal)previous[second.Length] / Math.Max(first.Length, second.Length);
            return Math.Max(overlap, spelling >= .75m ? spelling : 0);
        }
    }
}
