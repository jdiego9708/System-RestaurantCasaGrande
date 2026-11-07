using System;
using System.Data;
using System.Globalization;
using System.Text;

namespace CapaPresentacion
{
    /// <summary>
    /// Texto de la tarjeta de una mesa en el mapa del escritorio. Muestra lo mismo que la tarjeta de la app
    /// móvil: estado, comensales, total, hora de apertura en 12 h y mesero (primer nombre + primer apellido).
    /// </summary>
    internal static class TableCardText
    {
        private static readonly CultureInfo Colombia = new CultureInfo("es-CO");

        /// <summary>
        /// Líneas de detalle bajo el número de mesa. <paramref name="apiRow"/> es la fila de
        /// NApiMovil.Mesas (la misma consulta que alimenta al móvil); null en mesas libres.
        /// </summary>
        public static string Detail(string status, DataRow apiRow)
        {
            StringBuilder text = new StringBuilder(StatusLabel(status));
            if (apiRow == null || string.Equals(status, "DISPONIBLE", StringComparison.OrdinalIgnoreCase))
                return text.ToString();

            if (apiRow.Table.Columns.Contains("CantidadClientes") && apiRow["CantidadClientes"] != DBNull.Value)
            {
                int diners = Convert.ToInt32(apiRow["CantidadClientes"]);
                text.AppendLine().Append(diners).Append(diners == 1 ? " comensal" : " comensales");
            }

            if (apiRow["Total"] != DBNull.Value)
                text.AppendLine().Append(Money(Convert.ToDecimal(apiRow["Total"])));

            if (apiRow["Hora_pedido"] is TimeSpan hour)
                text.AppendLine().Append(Time12h(hour));

            string waiter = ShortName(Convert.ToString(apiRow["Nombre_empleado"]));
            if (waiter.Length > 0)
                text.AppendLine().Append(waiter);

            return text.ToString();
        }

        private static string StatusLabel(string status)
        {
            switch ((status ?? string.Empty).Trim().ToUpperInvariant())
            {
                case "DISPONIBLE": return "Libre";
                case "PENDIENTE": return "Pendiente";
                case "SALIENDO": return "Saliendo";
                default: return string.Empty;
            }
        }

        /// <summary>Igual que el móvil: el símbolo va delante, con punto de miles ("$ 49.500").</summary>
        public static string Money(decimal amount)
        {
            return "$ " + Math.Round(amount).ToString("#,##0", Colombia);
        }

        /// <summary>12 horas con AM/PM, siempre igual y sin depender del formato de hora de Windows.</summary>
        public static string Time12h(TimeSpan hour)
        {
            return DateTime.Today.Add(hour).ToString("h:mm tt", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Primer nombre + primer apellido, suponiendo uno o dos nombres seguidos de uno o dos apellidos:
        /// "Juan Diego Duque" -> "Juan Duque", "Maria Paula Casas Ruiz" -> "Maria Casas". Con tres palabras la
        /// última cuenta como apellido. Misma regla que <c>shortPersonName</c> de la app móvil.
        /// </summary>
        public static string ShortName(string fullName)
        {
            string[] parts = (fullName ?? string.Empty).Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return string.Empty;
            if (parts.Length == 1) return parts[0];
            if (parts.Length == 2) return parts[0] + " " + parts[1];
            if (parts.Length == 3) return parts[0] + " " + parts[2];
            return parts[0] + " " + parts[parts.Length - 2];
        }
    }
}
