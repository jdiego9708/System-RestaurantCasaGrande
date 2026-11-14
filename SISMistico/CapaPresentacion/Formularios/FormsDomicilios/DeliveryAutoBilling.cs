using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using CapaEntidades.Models;
using CapaNegocio;

namespace CapaPresentacion.Formularios.FormsDomicilios
{
    /// <summary>
    /// Factura sola un domicilio cuando se le agota el tiempo de preparacion: registra la venta con la forma de pago
    /// elegida al tomar el pedido y lo deja DISPONIBLE (Facturado). Si el pedido no tiene forma de pago guardada,
    /// se factura como efectivo.
    /// </summary>
    public static class DeliveryAutoBilling
    {
        private const string BilledState = "DISPONIBLE";
        private const string DefaultMethod = "EFECTIVO";

        /// <summary>
        /// Intenta facturar el pedido. Devuelve true si quedo facturado (ahora o por otro equipo que se adelanto).
        /// </summary>
        public static bool TryBill(Pedidos order)
        {
            // Un domicilio sin forma de pago guardada (anterior a la pregunta al crearlo) se factura como EFECTIVO y la
            // venta lo deja anotado en sus observaciones, para que se pueda corregir.
            string method = NPedido.GetPaymentMethod(order.Id_pedido);
            bool assumed = string.IsNullOrEmpty(method);
            if (assumed)
                method = DefaultMethod;

            DataTable items;
            DataTable main = NPedido.BuscarPedidosYDetalle("ID PEDIDO Y DETALLE", Convert.ToString(order.Id_pedido),
                out items, out DataTable _, out string _);
            if (main == null || items == null)
                return false;

            // Igual que FrmFacturarPedido.SumarPrecios: el total parcial es la suma de la columna Total.
            int total = 0;
            foreach (DataRow row in items.Rows)
                total += Convert.ToInt32(row["Total"], CultureInfo.InvariantCulture);
            if (total <= 0)
                return false;

            string totalText = total.ToString(CultureInfo.InvariantCulture);

            // Mismo orden que FrmFacturarPedido.Variables: sin propina, descuento, cupon, desechables ni domicilio.
            List<string> variables = new List<string>
            {
                Convert.ToString(order.Id_pedido), totalText, "0", totalText, "0", string.Empty, "0", "0", totalText,
                assumed ? "Facturado automáticamente al cumplirse el tiempo; forma de pago no registrada, se asumió efectivo"
                        : "Facturado automáticamente al cumplirse el tiempo de preparación",
            };

            DataTable payments = new DataTable();
            payments.Columns.Add("Pago", typeof(string));
            payments.Columns.Add("Valor_pago", typeof(string));
            payments.Columns.Add("Vaucher", typeof(string));
            payments.Columns.Add("Observaciones", typeof(string));
            payments.Rows.Add(method, totalText, string.Empty, string.Empty);

            string result = NVentas.InsertarVenta(variables, out int _, payments);
            // Si otro equipo ya facturo el pedido, solo falta dejarlo en su estado final.
            bool alreadyBilled = result != null && result.EndsWith("ya fue facturado.", StringComparison.Ordinal);
            if (!result.Equals("OK") && !alreadyBilled)
                return false;

            return NPedido.CambiarEstadoPedido(new List<string> { order.Id_pedido.ToString(), BilledState, "0" })
                .Equals("OK");
        }
    }
}
