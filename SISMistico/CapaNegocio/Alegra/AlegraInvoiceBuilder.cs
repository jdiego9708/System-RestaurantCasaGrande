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
        public bool AutoPrint { get; set; } = true;
        public DateTime? AutoPrintFromUtc { get; set; }
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
        private static decimal Number(object value)
        {
            if(!(value is string text))return Convert.ToDecimal(value,CultureInfo.InvariantCulture);
            var style=NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite;
            // Version 1 podia contener decimales locales sin agrupacion, como 43800,00.
            // Nunca interpretar una coma como separador de miles.
            var culture=text.Contains(",") ? CultureInfo.GetCultureInfo("es-CO") : CultureInfo.InvariantCulture;
            if(!decimal.TryParse(text,style,culture,out decimal result))
                throw new InvalidOperationException("Importe electronico invalido; revise la solicitud y sus pagos guardados.");
            return result;
        }
        private static string Id(string value, string caption)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new InvalidOperationException("Falta configurar " + caption + ".");
            return value;
        }

        public static string Build(string snapshotJson, AlegraInvoiceConfiguration config, DateTime invoiceDate)
        {
            return BuildCore(snapshotJson, config, invoiceDate, false, out decimal invoiceTotal, out List<string> excluded);
        }

        // Total que debe tener la factura en Alegra. Es el total de la venta, salvo que el usuario haya elegido
        // enviar solo los productos vinculados: entonces es la suma de esos productos más los cargos.
        public static decimal ExpectedTotal(string snapshotJson, AlegraInvoiceConfiguration config)
        {
            BuildCore(snapshotJson, config, DateTime.Today, true, out decimal invoiceTotal, out List<string> excluded);
            return invoiceTotal;
        }

        // Productos que quedarían fuera de la factura (solo con «enviar solo los vinculados»).
        public static List<string> ExcludedProducts(string snapshotJson, AlegraInvoiceConfiguration config)
        {
            BuildCore(snapshotJson, config, DateTime.Today, true, out decimal invoiceTotal, out List<string> excluded);
            return excluded;
        }

        // Nombres de los productos de la venta que no tienen vínculo con Alegra. No lanza por ellos.
        public static List<string> UnlinkedProducts(string snapshotJson, AlegraInvoiceConfiguration config)
        {
            var snapshot = (Dictionary<string, object>)new JavaScriptSerializer().DeserializeObject(snapshotJson);
            var names = new List<string>();
            foreach (Dictionary<string, object> row in ((object[])snapshot["products"]).Cast<Dictionary<string, object>>())
            {
                string key = Convert.ToString(row["Tipo"]) + ":" + Convert.ToString(row["Id_tipo"], CultureInfo.InvariantCulture);
                string name = Convert.ToString(row["Nombre"]);
                if (!config.Products.ContainsKey(key) && !names.Contains(name)) names.Add(name);
            }
            return names;
        }

        // Nombres de los productos de la venta que sí tienen vínculo con Alegra.
        public static List<string> LinkedProducts(string snapshotJson, AlegraInvoiceConfiguration config)
        {
            var snapshot = (Dictionary<string, object>)new JavaScriptSerializer().DeserializeObject(snapshotJson);
            var names = new List<string>();
            foreach (Dictionary<string, object> row in ((object[])snapshot["products"]).Cast<Dictionary<string, object>>())
            {
                string key = Convert.ToString(row["Tipo"]) + ":" + Convert.ToString(row["Id_tipo"], CultureInfo.InvariantCulture);
                string name = Convert.ToString(row["Nombre"]);
                if (config.Products.ContainsKey(key) && !names.Contains(name)) names.Add(name);
            }
            return names;
        }

        // Verdadero si al menos un producto de la venta tiene vínculo con Alegra.
        public static bool AnyLinked(string snapshotJson, AlegraInvoiceConfiguration config)
        {
            var snapshot = (Dictionary<string, object>)new JavaScriptSerializer().DeserializeObject(snapshotJson);
            return ((object[])snapshot["products"]).Cast<Dictionary<string, object>>().Any(row =>
                config.Products.ContainsKey(Convert.ToString(row["Tipo"]) + ":" + Convert.ToString(row["Id_tipo"], CultureInfo.InvariantCulture)));
        }

        private static string BuildCore(string snapshotJson, AlegraInvoiceConfiguration config, DateTime invoiceDate,
            bool totalOnly, out decimal invoiceTotal, out List<string> excluded)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            excluded = new List<string>();
            invoiceTotal = 0;
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
            // Decisión tomada al terminar la venta: facturar en Alegra solo los productos que tienen vínculo.
            bool onlyLinked = snapshot.ContainsKey("onlyLinked") && Convert.ToBoolean(snapshot["onlyLinked"]);
            decimal includedGross = 0;
            for (int index = 0; index < rows.Count; index++)
            {
                var row = rows[index];
                string key = Convert.ToString(row["Tipo"]) + ":" + Convert.ToString(row["Id_tipo"], CultureInfo.InvariantCulture);
                if (!config.Products.TryGetValue(key, out AlegraItemMapping mapping))
                {
                    if (!onlyLinked) throw new InvalidOperationException("Falta vincular el producto: " + Convert.ToString(row["Nombre"]) + ".");
                    excluded.Add(Convert.ToString(row["Nombre"]));
                    continue;
                }
                items.Add(Item(mapping, gross[index], Number(row["Cantidad"]), Convert.ToString(row["Nombre"])));
                includedGross += gross[index];
            }
            if (items.Count == 0) throw new InvalidOperationException("Ningún producto de la venta está vinculado con Alegra.");
            if (packaging > 0) items.Add(Item(config.PackagingItem, packaging, 1, "Desechables"));
            if (delivery > 0) items.Add(Item(config.DeliveryItem, delivery, 1, "Domicilio"));
            // Los cargos (propina, desechables, domicilio) siempre van completos; los productos omitidos no.
            invoiceTotal = includedGross + tip + packaging + delivery;
            decimal[] paymentAmounts = payments.Cast<Dictionary<string, object>>().Select(item => Number(item["Valor_pago"])).ToArray();
            if (invoiceTotal != total)
            {
                // Factura parcial: los pagos de la factura se reparten en proporción a lo que se pagó realmente.
                try { paymentAmounts = AlegraAllocation.AllocateGross(paymentAmounts, invoiceTotal); }
                catch (ArgumentException) { throw new InvalidOperationException("No se pudo repartir el total de la factura parcial entre los pagos."); }
            }
            var remotePayments = new List<object>();
            decimal paid = 0;
            // La cabecera lleva un solo medio fiscal: el del pago de mayor valor (en un empate, el primero).
            string headerFiscalMethod = null;
            decimal largestPayment = -1;
            for (int paymentIndex = 0; paymentIndex < payments.Length; paymentIndex++)
            {
                var payment = (Dictionary<string, object>)payments[paymentIndex];
                string localMethod = Convert.ToString(payment["Pago"]);
                if (!config.PaymentAccounts.TryGetValue(localMethod, out string account)
                    || !config.PaymentMethods.TryGetValue(localMethod, out string method))
                    throw new InvalidOperationException("Falta configurar cuenta y método para " + localMethod + ".");
                if (!new[] { "cash", "check", "transfer", "deposit", "credit-card", "debit-card" }.Contains(method))
                    throw new InvalidOperationException("El medio de pago no es compatible con Alegra.");
                if (Number(payment["Valor_pago"]) < 0) throw new InvalidOperationException("No se admiten pagos negativos.");
                decimal amount = paymentAmounts[paymentIndex];
                paid += amount;
                if (amount > largestPayment) { largestPayment = amount; headerFiscalMethod = FiscalMethodFor(method, config.FiscalPaymentMethod); }
                remotePayments.Add(new { date = invoiceDate.ToString("yyyy-MM-dd"), account = new { id = Id(account, "cuenta receptora") }, amount, paymentMethod = method });
            }
            if (paid != invoiceTotal) throw new InvalidOperationException("La suma de pagos no coincide con el total final.");
            if (totalOnly) return null;
            var invoice = new Dictionary<string, object>
            {
                { "date", invoiceDate.ToString("yyyy-MM-dd") }, { "dueDate", invoiceDate.ToString("yyyy-MM-dd") },
                { "client", Id(config.ClientId, "cliente Alegra") },
                { "numberTemplate", new { id = Id(config.NumberTemplateId, "numeración electrónica") } },
                { "status", "open" }, { "type", "NATIONAL" }, { "operationType", "STANDARD" },
                { "paymentForm", "CASH" }, { "paymentMethod", Id(headerFiscalMethod, "medio fiscal Colombia") },
                { "items", items }, { "payments", remotePayments },
                { "observations", snapshot.ContainsKey("requestId") ? "CasaGrande solicitud " + snapshot["requestId"] : "CasaGrande pedido " + snapshot["orderId"] }
            };
            if (tip > 0) invoice.Add("additionalCharges", new[] { new { id = Id(config.TipChargeId, "cargo de propina"), amount = tip } });
            // No incluir stamp.generateStamp: primero se persiste y concilia el documento creado.
            return serializer.Serialize(invoice);
        }

        // Medio de pago fiscal de la cabecera (códigos Colombia) a partir del medio de cada pago. Antes la cabecera
        // llevaba siempre FiscalPaymentMethod (CASH) y una transferencia salía como efectivo. Cheque y consignación
        // no tienen equivalente configurado: usan el valor de respaldo de la configuración.
        private static string FiscalMethodFor(string paymentMethod, string fallback)
        {
            switch (paymentMethod)
            {
                case "cash": return "CASH";
                // Decisión del usuario (6-nov-2026): una transferencia a la cuenta del restaurante se declara
                // como «Consignación bancaria» del catálogo de medios de pago de Colombia.
                case "transfer": return "BANK_DEPOSIT";
                case "credit-card": return "CREDIT_CARD";
                case "debit-card": return "DEBIT_CARD";
                default: return fallback;
            }
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
