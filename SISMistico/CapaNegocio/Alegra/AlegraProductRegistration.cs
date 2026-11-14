using CapaDatos;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace CapaNegocio.Alegra
{
    public sealed class AlegraTaxOption
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public decimal Percentage { get; set; }
    }

    public sealed class AlegraProductRegistrationResult
    {
        public string Message { get; set; }
        public AlegraSimilarProduct Product { get; set; }
    }

    public static class AlegraProductRegistration
    {
        public static async Task<List<AlegraTaxOption>> TaxesAsync(CancellationToken token)
        {
            using (var api = new AlegraClient())
            {
                var result = new List<AlegraTaxOption> { new AlegraTaxOption { Id = "", Name = "Sin impuesto (0%)" } };
                foreach (var tax in AlegraWorkflow.List(await api.GetTaxesAsync(token).ConfigureAwait(false)))
                {
                    decimal rate;
                    object value;
                    // Alegra rechaza crear productos con impuestos inactivos: no se ofrecen en la lista.
                    object status;
                    if (tax.TryGetValue("status", out status) && string.Equals(Convert.ToString(status), "inactive", StringComparison.OrdinalIgnoreCase)) continue;
                    if (!tax.TryGetValue("percentage", out value) || !decimal.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture),
                        NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                        CultureInfo.InvariantCulture, out rate) || rate < 0) continue;
                    result.Add(new AlegraTaxOption { Id = AlegraWorkflow.Text(tax, "id"), Percentage = rate,
                        Name = AlegraWorkflow.Text(tax, "name") + " (" + rate.ToString("0.##", CultureInfo.InvariantCulture) + "%)" });
                }
                return result;
            }
        }

        // Asigna la unidad de medida a productos de Alegra que no la tienen. Relee cada producto antes y después del cambio.
        // Devuelve una línea por producto con el resultado.
        public static async Task<List<string>> FixMissingUnitsAsync(IEnumerable<AlegraProductValidation.MissingUnitItem> items, CancellationToken token)
        {
            var results = new List<string>();
            using (var api = new AlegraClient())
            {
                foreach (var entry in items)
                {
                    token.ThrowIfCancellationRequested();
                    string label = entry.Name + " (#" + entry.RemoteId + ")";
                    try
                    {
                        string json = await api.GetItemOrNullAsync(entry.RemoteId, token).ConfigureAwait(false);
                        if (json == null) { results.Add(label + ": ya no existe en Alegra."); continue; }
                        if (AlegraProductValidation.HasUnit(AlegraWorkflow.Object(json))) { results.Add(label + ": ya tenía unidad."); continue; }
                        var response = await api.UpdateItemAsync(entry.RemoteId,
                            new JavaScriptSerializer().Serialize(new { inventory = new { unit = "unit" } }), token).ConfigureAwait(false);
                        if (!response.Success) { results.Add(label + ": Alegra rechazó el cambio (HTTP " + response.StatusCode + "). " + response.Json); continue; }
                        string after = await api.GetItemOrNullAsync(entry.RemoteId, token).ConfigureAwait(false);
                        results.Add(label + (after != null && AlegraProductValidation.HasUnit(AlegraWorkflow.Object(after))
                            ? ": unidad asignada." : ": Alegra respondió OK pero la unidad no quedó guardada. Revísalo en Alegra."));
                    }
                    catch (OperationCanceledException) { throw; }
                    catch (Exception ex) { results.Add(label + ": " + ex.Message); }
                }
            }
            return results;
        }

        public static async Task<string> RegisterAsync(string kind, int id, string name, decimal price,
            AlegraTaxOption selectedTax, CancellationToken token, string relatedRemoteId = null, string relatedCompanyId = null)
        {
            var result = await RegisterConfirmedAsync(kind, id, name, price, selectedTax, token,
                relatedRemoteId, relatedCompanyId).ConfigureAwait(false);
            return result.Message;
        }

        public static async Task<AlegraProductRegistrationResult> RegisterConfirmedAsync(string kind, int id, string name, decimal price,
            AlegraTaxOption selectedTax, CancellationToken token, string relatedRemoteId = null, string relatedCompanyId = null)
        {
            if (kind != "PLATO" && kind != "BEBIDA") throw new ArgumentException("Tipo de producto inválido.");
            string key = kind + ":" + id.ToString(CultureInfo.InvariantCulture);
            string reference = "CG-" + kind + "-" + id.ToString(CultureInfo.InvariantCulture);
            if (string.IsNullOrWhiteSpace(DAlegra.GetSettings()))
                throw new InvalidOperationException("Guarda primero la configuración de Alegra para poder vincular productos.");
            var settings = AlegraWorkflow.Settings();
            using (var api = new AlegraClient())
            {
                string company = AlegraWorkflow.CompanyIdentity(AlegraWorkflow.Object(await api.GetCompanyAsync(token).ConfigureAwait(false)));
                if (!string.IsNullOrWhiteSpace(settings.CompanyId) && settings.CompanyId != company)
                    throw new InvalidOperationException("La cuenta conectada pertenece a otra empresa de Alegra.");
                Dictionary<string, object> remote = null;
                if (!string.IsNullOrWhiteSpace(relatedRemoteId))
                {
                    if (string.IsNullOrWhiteSpace(relatedCompanyId) || relatedCompanyId != company)
                        throw new InvalidOperationException("El producto seleccionado pertenece a otra empresa. Vuelve a buscar los similares.");
                    string relatedJson = await api.GetItemOrNullAsync(relatedRemoteId, token).ConfigureAwait(false);
                    if (relatedJson == null) throw new InvalidOperationException("El producto seleccionado ya no existe en Alegra. Selecciona otro similar.");
                    remote = AlegraWorkflow.Object(relatedJson);
                    if (AlegraWorkflow.Text(remote, "id") != relatedRemoteId)
                        throw new InvalidOperationException("Alegra no confirmó el producto seleccionado.");
                }
                AlegraItemMapping previous;
                if (remote == null && settings.Products != null && settings.Products.TryGetValue(key, out previous) && previous != null && !string.IsNullOrWhiteSpace(previous.RemoteId))
                {
                    string json = await api.GetItemOrNullAsync(previous.RemoteId, token).ConfigureAwait(false);
                    if (json != null) remote = AlegraWorkflow.Object(json);
                }
                if (remote == null)
                {
                    var candidates = AlegraWorkflow.List(await api.FindItemsByReferenceAsync(reference, token).ConfigureAwait(false))
                        .Where(item => AlegraWorkflow.Text(item, "reference") == reference).ToList();
                    if (candidates.Count == 0)
                        candidates = AlegraWorkflow.List(await api.FindItemsAsync(name.Trim(), token).ConfigureAwait(false))
                            .Where(item => string.Equals(AlegraWorkflow.Text(item, "name").Trim(), name.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();
                    if (candidates.Count > 1) throw new InvalidOperationException("Hay varios productos con este nombre en Alegra. Vincula el correcto desde la configuración de Alegra.");
                    remote = candidates.SingleOrDefault();
                }
                bool created = false;
                if (remote == null)
                {
                    if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 150)
                        throw new InvalidOperationException("Alegra admite nombres de hasta 150 caracteres. Edita el nombre del producto antes de agregarlo.");
                    if (selectedTax == null) throw new InvalidOperationException("Selecciona en la fila el impuesto del producto, o Sin impuesto (0%).");
                    if (price <= 0 || decimal.Round(price, 6) != price)
                        throw new InvalidOperationException("El Precio Alegra debe ser mayor que cero y tener hasta seis decimales.");
                    // Releer el catálogo para no usar un porcentaje obsoleto o alterado en pantalla.
                    var catalog = await TaxesAsync(token).ConfigureAwait(false);
                    selectedTax = catalog.FirstOrDefault(tax => tax.Id == selectedTax.Id)
                        ?? throw new InvalidOperationException("El impuesto elegido ya no está disponible. Actualiza la lista.");
                    var body = new { name = name.Trim(), reference,
                        price,
                        // La emisión electrónica exige unidad de medida en cada ítem; sin ella Alegra rechaza el timbrado (código 3051).
                        inventory = new { unit = "unit" },
                        tax = selectedTax.Id.Length == 0 ? new object[0] : new object[] { new { id = selectedTax.Id } } };
                    AlegraResponse response;
                    try { response = await api.CreateItemAsync(new JavaScriptSerializer().Serialize(body), token).ConfigureAwait(false); }
                    catch (Exception ex) when (ex is System.Net.Http.HttpRequestException || ex is TaskCanceledException)
                    { throw new InvalidOperationException("No se pudo confirmar si Alegra creó el producto. Vuelve a agregarlo: se buscará su referencia antes de intentar crearlo nuevamente.", ex); }
                    remote = AlegraWorkflow.Object(response.Json);
                    if (!response.Success || string.IsNullOrWhiteSpace(AlegraWorkflow.Text(remote, "id")))
                        throw new InvalidOperationException("Alegra no confirmó la creación. " + AlegraWorkflow.Text(remote, "message"));
                    created = true;
                }
                // Confirmar los impuestos guardados; el POST puede devolver solo parte del producto.
                string details = await api.GetItemOrNullAsync(AlegraWorkflow.Text(remote, "id"), token).ConfigureAwait(false);
                if (details == null) throw new InvalidOperationException("No se pudo confirmar el producto de Alegra. Vuelve a vincularlo.");
                remote = AlegraWorkflow.Object(details);
                var mapping = new AlegraItemMapping { RemoteId = AlegraWorkflow.Text(remote, "id") };
                if (string.IsNullOrWhiteSpace(mapping.RemoteId)) throw new InvalidOperationException("Alegra no devolvió el identificador del producto.");
                object taxes;
                if (remote.TryGetValue("tax", out taxes) && taxes != null)
                {
                    var rates = await TaxesAsync(token).ConfigureAwait(false);
                    var list = taxes as object[] ?? throw new InvalidOperationException("No se pudieron determinar los impuestos del producto remoto.");
                    foreach (var entry in list)
                    {
                        var tax = entry as Dictionary<string, object>;
                        string taxId = tax == null ? Convert.ToString(entry) : AlegraWorkflow.Text(tax, "id");
                        var rate = rates.FirstOrDefault(option => option.Id == taxId);
                        if (rate == null) throw new InvalidOperationException("No se pudo determinar el impuesto del producto en Alegra. No se guardó el vínculo.");
                        mapping.PercentageTaxes[taxId] = rate.Percentage;
                    }
                }
                else if (created && selectedTax.Id.Length > 0)
                    mapping.PercentageTaxes[selectedTax.Id] = selectedTax.Percentage;
                DAlegra.SaveProductMapping(key, new JavaScriptSerializer().Serialize(mapping), company);
                decimal confirmedPrice;
                bool hasPrice = AlegraProductValidation.TryPrice(remote, out confirmedPrice);
                var confirmed = new AlegraSimilarProduct
                {
                    Id = mapping.RemoteId, Name = AlegraWorkflow.Text(remote, "name"), CompanyId = company,
                    Price = hasPrice ? (decimal?)confirmedPrice : created ? (decimal?)price : null,
                    TaxPercentage = mapping.PercentageTaxes.Values.Sum(), Similarity = 1m,
                };
                return new AlegraProductRegistrationResult
                {
                    Message = created ? "Agregado y vinculado" : "Vinculado con Alegra",
                    Product = confirmed,
                };
            }
        }
    }
}
