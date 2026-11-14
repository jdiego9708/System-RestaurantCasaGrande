using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace CapaNegocio.Alegra
{
    // Consulta el catálogo remoto sin crear productos ni cambiar los vínculos usados al facturar.
    public static class AlegraProductValidation
    {
        public static void PrepareColumns(DataTable products)
        {
            if (!products.Columns.Contains("ExisteAlegra")) products.Columns.Add("ExisteAlegra", typeof(string));
            if (!products.Columns.Contains("PrecioAlegra")) products.Columns.Add("PrecioAlegra", typeof(decimal));
            if (!products.Columns.Contains("ImpuestoAlegra")) products.Columns.Add("ImpuestoAlegra", typeof(decimal));
            if (!products.Columns.Contains("DetalleAlegra")) products.Columns.Add("DetalleAlegra", typeof(string));
            if (!products.Columns.Contains("VinculadoAlegra")) products.Columns.Add("VinculadoAlegra", typeof(bool));
            if (!products.Columns.Contains("SimilarProducts")) products.Columns.Add("SimilarProducts", typeof(List<AlegraSimilarProduct>));
            if (!products.Columns.Contains("ExactProduct")) products.Columns.Add("ExactProduct", typeof(AlegraSimilarProduct));
            foreach (DataRow row in products.Rows)
            {
                row["ExisteAlegra"] = DBNull.Value;
                row["PrecioAlegra"] = DBNull.Value;
                row["ImpuestoAlegra"] = DBNull.Value;
                row["DetalleAlegra"] = "Pendiente de validar con Alegra.";
                row["VinculadoAlegra"] = false;
                row["SimilarProducts"] = DBNull.Value;
                row["ExactProduct"] = DBNull.Value;
            }
        }

        // Producto de Alegra vinculado o coincidente que no tiene unidad de medida: Alegra rechaza timbrar facturas con él.
        public sealed class MissingUnitItem
        {
            public string RemoteId { get; set; }
            public string Name { get; set; }
        }

        public static bool HasUnit(Dictionary<string, object> item)
        {
            var inventory = Value(item, "inventory") as Dictionary<string, object>;
            return inventory != null && !string.IsNullOrWhiteSpace(AlegraWorkflow.Text(inventory, "unit"));
        }

        // Devuelve los productos de Alegra asociados a filas del catálogo que no tienen unidad de medida.
        public static async Task<List<MissingUnitItem>> ValidateAsync(DataTable products, IProgress<string> progress, CancellationToken token)
        {
            var missingUnits = new Dictionary<string, MissingUnitItem>(StringComparer.Ordinal);
            PrepareColumns(products);
            var settings = AlegraWorkflow.Settings();
            using (var api = new AlegraClient())
            {
                token.ThrowIfCancellationRequested();
                progress.Report("Verificando la empresa de Alegra…");
                string company = AlegraWorkflow.CompanyIdentity(AlegraWorkflow.Object(await api.GetCompanyAsync(token).ConfigureAwait(false)));
                if (!string.IsNullOrWhiteSpace(settings.CompanyId) && settings.CompanyId != company)
                    throw new InvalidOperationException("Las credenciales de Alegra pertenecen a otra empresa. Revisa la configuración.");

                var taxes = ParseList(await api.GetTaxesAsync(token).ConfigureAwait(false));
                var taxRates = new Dictionary<string, decimal>();
                foreach (var tax in taxes)
                {
                    decimal rate;
                    if (TryNumber(Value(tax, "percentage"), out rate)) taxRates[AlegraWorkflow.Text(tax, "id")] = rate;
                }
                var items = new Dictionary<string, Dictionary<string, object>>(StringComparer.Ordinal);
                bool complete = false;
                for (int start = 0; start < 30000; start += 30)
                {
                    token.ThrowIfCancellationRequested();
                    progress.Report("Consultando catálogo de Alegra: " + items.Count + " productos recibidos…");
                    var page = ParseList(await api.GetItemsAsync(start, token).ConfigureAwait(false));
                    int previousCount = items.Count;
                    foreach (var item in page)
                    {
                        string id = AlegraWorkflow.Text(item, "id");
                        if (string.IsNullOrWhiteSpace(id)) throw new InvalidOperationException("Alegra devolvió un producto sin identificador.");
                        items[id] = item;
                    }
                    if (page.Count < 30) { complete = true; break; }
                    if (previousCount == items.Count)
                        throw new InvalidOperationException("No se pudo avanzar en el catálogo de Alegra. Vuelve a validar.");
                }
                if (!complete) throw new InvalidOperationException("El catálogo excede el límite de consulta. No se marcaron productos como inexistentes.");

                var byName = items.Values.GroupBy(item => Normalize(AlegraWorkflow.Text(item, "name")))
                    .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.Ordinal);
                foreach (DataRow row in products.Rows)
                {
                    token.ThrowIfCancellationRequested();
                    string key = Convert.ToString(row["Tipo"]) + ":" + Convert.ToString(row["Id"], CultureInfo.InvariantCulture);
                    Dictionary<string, object> match = null;
                    AlegraItemMapping mapping;
                    bool linked = settings.Products != null && settings.Products.TryGetValue(key, out mapping)
                        && mapping != null && !string.IsNullOrWhiteSpace(mapping.RemoteId)
                        && items.TryGetValue(mapping.RemoteId, out match);
                    row["VinculadoAlegra"] = linked;
                    if (!linked)
                    {
                        List<Dictionary<string, object>> matches;
                        if (byName.TryGetValue(Normalize(Convert.ToString(row["Nombre"])), out matches))
                        {
                            if (matches.Count == 1) match = matches[0];
                            else
                            {
                                row["ExisteAlegra"] = "Sí";
                                row["SimilarProducts"] = AlegraSimilarProducts.FindInCatalog(Convert.ToString(row["Nombre"]), matches, taxRates, company);
                                row["DetalleAlegra"] = "Hay varios productos en Alegra con este nombre. Vincula el correcto en la configuración para consultar su precio e impuesto.";
                                continue;
                            }
                        }
                    }
                    if (match == null)
                    {
                        row["ExisteAlegra"] = "No";
                        row["ImpuestoAlegra"] = 0m;
                        row["DetalleAlegra"] = "No se encontró en el catálogo de Alegra por vínculo ni por nombre exacto.";
                        progress.Report("Buscando similares para: " + Convert.ToString(row["Nombre"]));
                        row["SimilarProducts"] = AlegraSimilarProducts.FindInCatalog(Convert.ToString(row["Nombre"]), items.Values, taxRates, company);
                        continue;
                    }
                    row["ExisteAlegra"] = "Sí";
                    row["ExactProduct"] = AlegraSimilarProducts.FindInCatalog(AlegraWorkflow.Text(match, "name"), new[] { match }, taxRates, company).Single();
                    var notes = new List<string> { "Alegra #" + AlegraWorkflow.Text(match, "id") + " · " + AlegraWorkflow.Text(match, "name"),
                        linked ? "Coincidencia por vínculo de facturación." : "Coincidencia por nombre exacto. Esto no crea ni modifica el vínculo de facturación." };
                    decimal price;
                    if (TryPrice(match, out price)) row["PrecioAlegra"] = price;
                    else notes.Add("No se pudo determinar el precio general.");
                    decimal percentage;
                    if (TryTaxes(match, taxRates, out percentage)) row["ImpuestoAlegra"] = percentage;
                    else notes.Add("No se pudo determinar el porcentaje de los impuestos configurados.");
                    notes.Add("Precio general guardado en Alegra. Impuesto: suma de los porcentajes configurados.");
                    if (!HasUnit(match))
                    {
                        string remoteId = AlegraWorkflow.Text(match, "id");
                        missingUnits[remoteId] = new MissingUnitItem { RemoteId = remoteId, Name = AlegraWorkflow.Text(match, "name") };
                        notes.Add("⚠ Sin unidad de medida en Alegra: la factura electrónica no se podrá timbrar con este producto.");
                    }
                    row["DetalleAlegra"] = string.Join(Environment.NewLine, notes);
                }
            }
            return missingUnits.Values.ToList();
        }

        private static object Value(Dictionary<string, object> item, string key)
        { object value; return item.TryGetValue(key, out value) ? value : null; }

        private static string Normalize(string name)
        { return (name ?? "").Trim().Normalize().ToUpperInvariant(); }

        internal static List<Dictionary<string, object>> ParseList(string json)
        {
            object value = new JavaScriptSerializer { MaxJsonLength = 8 * 1024 * 1024 }.DeserializeObject(json);
            var wrapper = value as Dictionary<string, object>;
            if (wrapper != null) value = Value(wrapper, "data");
            var list = value as object[];
            if (list == null || list.Any(item => !(item is Dictionary<string, object>)))
                throw new InvalidOperationException("Alegra devolvió un catálogo inesperado. No se pudo completar la validación.");
            return list.Cast<Dictionary<string, object>>().ToList();
        }

        private static bool TryNumber(object value, out decimal result)
        {
            return decimal.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture),
                NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture, out result);
        }

        internal static bool TryPrice(Dictionary<string, object> item, out decimal result)
        {
            object value = Value(item, "price");
            var list = value as object[];
            if (list == null) return TryNumber(value, out result);
            var prices = list.OfType<Dictionary<string, object>>().ToList();
            var general = prices.FirstOrDefault(price => AlegraWorkflow.Text(price, "idPriceList") == "1")
                ?? prices.FirstOrDefault(price => Normalize(AlegraWorkflow.Text(price, "name")) == "GENERAL")
                ?? prices.FirstOrDefault(price => string.IsNullOrWhiteSpace(AlegraWorkflow.Text(price, "idPriceList")));
            return TryNumber(general == null ? null : Value(general, "price"), out result);
        }

        internal static bool TryTaxes(Dictionary<string, object> item, Dictionary<string, decimal> rates, out decimal result)
        {
            result = 0;
            object value = Value(item, "tax");
            if (value == null) return true;
            var taxes = value as object[];
            if (taxes == null) return false;
            var seen = new HashSet<string>();
            foreach (object entry in taxes)
            {
                var tax = entry as Dictionary<string, object>;
                if (tax == null) return false;
                string id = AlegraWorkflow.Text(tax, "id");
                if (!string.IsNullOrWhiteSpace(id) && !seen.Add(id)) continue;
                decimal percentage;
                if (!TryNumber(Value(tax, "percentage"), out percentage) && !rates.TryGetValue(id, out percentage)) return false;
                result += percentage;
            }
            return true;
        }
    }
}
