using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Script.Serialization;

namespace CapaNegocio.Alegra
{
    public sealed class AlegraItemMapping
    {
        public string RemoteId { get; set; }
        public Dictionary<string, decimal> PercentageTaxes { get; set; } = new Dictionary<string, decimal>();
    }

    public sealed class AlegraInvoiceConfiguration
    {
        public bool Enabled { get; set; }
        public string CompanyId { get; set; }
        public bool AutoPrint { get; set; }
        public string PdfPrinterExecutable { get; set; }
        public string PrinterName { get; set; }
        public string ClientId { get; set; }
        public string NumberTemplateId { get; set; }
        public string FiscalPaymentMethod { get; set; }
        public string TipChargeId { get; set; }
        public AlegraItemMapping DeliveryItem { get; set; }
        public AlegraItemMapping PackagingItem { get; set; }
        public Dictionary<string, AlegraItemMapping> Products { get; set; } = new Dictionary<string, AlegraItemMapping>();
        public Dictionary<string, string> PaymentAccounts { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> PaymentMethods { get; set; } = new Dictionary<string, string>();
    }

    public static class AlegraInvoiceBuilder
    {
        private static decimal Number(object value) => Convert.ToDecimal(value, CultureInfo.InvariantCulture);
        private static string Id(string value, string caption)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new InvalidOperationException("Falta configurar " + caption + ".");
            return value;
        }

        public static string Build(string snapshotJson, AlegraInvoiceConfiguration config, DateTime invoiceDate)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            var serializer = new JavaScriptSerializer();
            var snapshot = (Dictionary<string, object>)serializer.DeserializeObject(snapshotJson);
            object[] values = (object[])snapshot["saleValues"];
            object[] products = (object[])snapshot["products"];
            object[] payments = (object[])snapshot["payments"];
            if (values.Length != 10 || products.Length == 0 || payments.Length == 0)
                throw new InvalidOperationException("El detalle de la solicitud está incompleto.");
            decimal tip = Number(values[2]), packaging = Number(values[6]), delivery = Number(values[7]), total = Number(values[8]);
            if (new[] { tip, packaging, delivery, total }.Any(amount => amount < 0))
                throw new InvalidOperationException("No se admiten cargos ni totales negativos.");
            decimal productsTarget = total - tip - packaging - delivery;
            if (productsTarget < 0) throw new InvalidOperationException("Los cargos superan el total final.");
            var rows = products.Cast<Dictionary<string, object>>().ToList();
            var weights = rows.Select(row => Number(row["Precio"]) * Number(row["Cantidad"])).ToArray();
            decimal[] gross = AlegraAllocation.AllocateGross(weights, productsTarget);
            var items = new List<object>();
            for (int index = 0; index < rows.Count; index++)
            {
                var row = rows[index];
                string key = Convert.ToString(row["Tipo"]) + ":" + Convert.ToString(row["Id_tipo"], CultureInfo.InvariantCulture);
                if (!config.Products.TryGetValue(key, out AlegraItemMapping mapping))
                    throw new InvalidOperationException("Falta vincular el producto: " + Convert.ToString(row["Nombre"]) + ".");
                items.Add(Item(mapping, gross[index], Number(row["Cantidad"]), Convert.ToString(row["Nombre"])));
            }
            if (packaging > 0) items.Add(Item(config.PackagingItem, packaging, 1, "Desechables"));
            if (delivery > 0) items.Add(Item(config.DeliveryItem, delivery, 1, "Domicilio"));
            var remotePayments = new List<object>();
            decimal paid = 0;
            foreach (Dictionary<string, object> payment in payments)
            {
                string localMethod = Convert.ToString(payment["Pago"]);
                if (!config.PaymentAccounts.TryGetValue(localMethod, out string account)
                    || !config.PaymentMethods.TryGetValue(localMethod, out string method))
                    throw new InvalidOperationException("Falta configurar cuenta y método para " + localMethod + ".");
                if (!new[] { "cash", "check", "transfer", "deposit", "credit-card", "debit-card" }.Contains(method))
                    throw new InvalidOperationException("El medio de pago no es compatible con Alegra.");
                decimal amount = Number(payment["Valor_pago"]);
                if (amount < 0) throw new InvalidOperationException("No se admiten pagos negativos.");
                paid += amount;
                remotePayments.Add(new { date = invoiceDate.ToString("yyyy-MM-dd"), account = new { id = Id(account, "cuenta receptora") }, amount, paymentMethod = method });
            }
            if (paid != total) throw new InvalidOperationException("La suma de pagos no coincide con el total final.");
            var invoice = new Dictionary<string, object>
            {
                { "date", invoiceDate.ToString("yyyy-MM-dd") }, { "dueDate", invoiceDate.ToString("yyyy-MM-dd") },
                { "client", Id(config.ClientId, "cliente Alegra") },
                { "numberTemplate", new { id = Id(config.NumberTemplateId, "numeración electrónica") } },
                { "status", "open" }, { "type", "NATIONAL" }, { "operationType", "STANDARD" },
                { "paymentForm", "CASH" }, { "paymentMethod", Id(config.FiscalPaymentMethod, "medio fiscal Colombia") },
                { "items", items }, { "payments", remotePayments },
                { "observations", snapshot.ContainsKey("requestId") ? "CasaGrande solicitud " + snapshot["requestId"] : "CasaGrande pedido " + snapshot["orderId"] }
            };
            if (tip > 0) invoice.Add("additionalCharges", new[] { new { id = Id(config.TipChargeId, "cargo de propina"), amount = tip } });
            // No incluir stamp.generateStamp: primero se persiste y concilia el documento creado.
            return serializer.Serialize(invoice);
        }

        private static object Item(AlegraItemMapping mapping, decimal gross, decimal quantity, string description)
        {
            if (mapping == null) throw new InvalidOperationException("Falta vincular un cargo al catálogo de Alegra.");
            if (quantity <= 0) throw new InvalidOperationException("La cantidad debe ser mayor que cero.");
            if (mapping.PercentageTaxes == null || mapping.PercentageTaxes.Any(tax => tax.Value < 0 || string.IsNullOrWhiteSpace(tax.Key)))
                throw new InvalidOperationException("Revise los impuestos del producto.");
            decimal rate = mapping.PercentageTaxes.Values.Sum() / 100m;
            decimal price = decimal.Round(gross / quantity / (1m + rate), 6, MidpointRounding.AwayFromZero);
            return new { id = Id(mapping.RemoteId, "producto Alegra"), price, quantity, description,
                tax = mapping.PercentageTaxes.Keys.Select(id => new { id }).ToArray() };
        }
    }
}
