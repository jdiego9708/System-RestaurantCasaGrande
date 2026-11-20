using System;
using System.Collections.Generic;
using System.Data;

namespace CapaEntidades.Models
{
    /// <summary>Un producto de una orden de Rappi: "2x BURGER CLASICA $55.000".</summary>
    public sealed class RappiOrderItem
    {
        public int Quantity { get; set; }

        public string Name { get; set; }

        /// <summary>Valor de la linea (cantidad por precio unitario), tal como lo imprime Rappi.</summary>
        public int Price { get; set; }

        /// <summary>Opciones elegidas del producto ("ELIGE TU ARROZ: Arroz de Coco"); no son productos aparte.</summary>
        public List<string> Notes { get; } = new List<string>();
    }

    /// <summary>
    /// Orden de Rappi leida de una factura PDF (tabla RappiOrders). Es informativa: no es un pedido del restaurante
    /// y no pasa por el flujo de preparacion ni de facturacion.
    /// </summary>
    public class RappiOrder
    {
        /// <summary>ID de orden de Rappi; es la clave contra duplicados. "HASH:..." si el PDF no se pudo leer del todo.</summary>
        public string ExternalId { get; set; }

        public string CustomerName { get; set; }

        public string Phone { get; set; }

        public string Address { get; set; }

        public string PaymentMethod { get; set; }

        public string Platform { get; set; }

        public DateTime? OrderDateTime { get; set; }

        public List<RappiOrderItem> Items { get; } = new List<RappiOrderItem>();

        public int? Subtotal { get; set; }

        /// <summary>Descuento por oferta; negativo tal como lo imprime Rappi.</summary>
        public int? Discount { get; set; }

        public int? ShippingCost { get; set; }

        public int? Tip { get; set; }

        public int? ServiceFee { get; set; }

        public int? PaidThroughRappi { get; set; }

        /// <summary>Total a cobrar al cliente (en efectivo o datafono; 0 si ya lo pago todo a traves de Rappi).</summary>
        public int? Total { get; set; }

        /// <summary>
        /// Valor del pedido: lo que a cobrar mas lo que ya pago el cliente a traves de Rappi. Es el valor de la venta
        /// aunque se haya pagado con RappiCard (entonces el total a cobrar es 0).
        /// </summary>
        public int? OrderValue => this.Total.HasValue ? this.Total.Value + Math.Abs(this.PaidThroughRappi ?? 0) : (int?)null;

        private bool CollectInCash => string.Equals(this.PaymentMethod?.Trim(), "Efectivo", StringComparison.OrdinalIgnoreCase)
            || string.Equals(this.PaymentMethod?.Trim(), "Cash", StringComparison.OrdinalIgnoreCase);

        /// <summary>Saldo a cobrar en efectivo, sin incluir lo ya pagado en Rappi ni el billete usado para pagar.</summary>
        public int? CashAmount => !this.Total.HasValue || (this.Total.Value != 0 && string.IsNullOrWhiteSpace(this.PaymentMethod))
            ? (int?)null : this.CollectInCash ? this.Total.Value : 0;

        /// <summary>Lo pagado en Rappi mas el saldo a cobrar cuando la forma de pago es distinta de efectivo.</summary>
        public int? OtherPaymentAmount => !this.Total.HasValue || (this.Total.Value != 0 && string.IsNullOrWhiteSpace(this.PaymentMethod))
            ? (int?)null : Math.Abs(this.PaidThroughRappi ?? 0) + (this.CollectInCash ? 0 : this.Total.Value);

        /// <summary>Con cuanto va a pagar el cliente en efectivo.</summary>
        public int? CashPayWith { get; set; }

        public int? ChangeToBring { get; set; }

        public string DeliveryCode { get; set; }

        /// <summary>False si faltaron datos clave (ID, total, productos o forma de cobro); la ficha lo avisa.</summary>
        public bool IsComplete { get; set; } = true;

        public string SourceFile { get; set; }

        public string FileHash { get; set; }

        public DateTime ImportedAt { get; set; }

        /// <summary>Numero de factura del dia en la numeracion de Rappi (1..N, empieza de nuevo cada dia); null sin numerar.</summary>
        public int? DayNumber { get; set; }

        public int Id { get; set; }

        /// <summary>Cuando se hizo la orden; si la factura no traia fecha, cuando se importo.</summary>
        public DateTime EffectiveDate => this.OrderDateTime ?? this.ImportedAt;

        /// <summary>Texto guardado: una linea por producto.</summary>
        public string ItemsText
        {
            get
            {
                List<string> lines = new List<string>();
                foreach (RappiOrderItem item in this.Items)
                {
                    lines.Add(item.Quantity + "x " + item.Name + " $" + item.Price.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("es-CO")));
                    foreach (string note in item.Notes)
                        lines.Add("> " + note);
                }
                return string.Join("\n", lines);
            }
        }

        public static RappiOrder FromRow(DataRow row)
        {
            RappiOrder order = new RappiOrder
            {
                Id = Convert.ToInt32(row["Id"]),
                ExternalId = Convert.ToString(row["ExternalId"]),
                CustomerName = Text(row, "CustomerName"),
                Phone = Text(row, "Phone"),
                Address = Text(row, "Address"),
                PaymentMethod = Text(row, "PaymentMethod"),
                Platform = Text(row, "Platform"),
                OrderDateTime = row["OrderDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["OrderDateTime"]),
                Subtotal = Number(row, "Subtotal"),
                Discount = Number(row, "Discount"),
                ShippingCost = Number(row, "ShippingCost"),
                Tip = Number(row, "Tip"),
                ServiceFee = Number(row, "ServiceFee"),
                PaidThroughRappi = Number(row, "PaidThroughRappi"),
                Total = Number(row, "Total"),
                CashPayWith = Number(row, "CashPayWith"),
                ChangeToBring = Number(row, "ChangeToBring"),
                DeliveryCode = Text(row, "DeliveryCode"),
                IsComplete = Convert.ToBoolean(row["IsComplete"]),
                SourceFile = Text(row, "SourceFile"),
                FileHash = Text(row, "FileHash"),
                ImportedAt = Convert.ToDateTime(row["ImportedAt"]),
                DayNumber = row.Table.Columns.Contains("DayNumber") ? Number(row, "DayNumber") : null,
            };

            // ProductsText: "2x NOMBRE $55.000" por linea (el mismo formato que imprime la factura).
            string products = Text(row, "ProductsText");
            if (!string.IsNullOrEmpty(products))
            {
                foreach (string line in products.Split('\n'))
                {
                    // "> opcion" es una opcion del producto anterior.
                    if (line.StartsWith("> ") && order.Items.Count > 0)
                    {
                        order.Items[order.Items.Count - 1].Notes.Add(line.Substring(2).Trim());
                        continue;
                    }
                    System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(
                        line.Trim(), @"^(\d+)x\s+(.+?)\s+\$([\d.]+)$");
                    if (match.Success)
                        order.Items.Add(new RappiOrderItem
                        {
                            Quantity = int.Parse(match.Groups[1].Value),
                            Name = match.Groups[2].Value,
                            Price = int.Parse(match.Groups[3].Value.Replace(".", string.Empty)),
                        });
                    else if (line.Trim().Length > 0)
                        order.Items.Add(new RappiOrderItem { Quantity = 1, Name = line.Trim(), Price = 0 });
                }
            }
            return order;
        }

        private static string Text(DataRow row, string column) =>
            row[column] == DBNull.Value ? null : Convert.ToString(row[column]);

        private static int? Number(DataRow row, string column) =>
            row[column] == DBNull.Value ? (int?)null : Convert.ToInt32(row[column]);
    }
}
