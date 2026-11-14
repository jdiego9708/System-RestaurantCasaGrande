using System;
using System.Configuration;
using System.Globalization;
using CapaEntidades.Models;

namespace CapaPresentacion.Formularios.FormsDomicilios
{
    /// <summary>Etapa en la que se muestra un domicilio dentro de la pagina de Domicilios.</summary>
    public enum DeliveryStage
    {
        /// <summary>Pendiente y con tiempo de preparacion por delante: panel EN PREPARACION, con cuenta regresiva.</summary>
        Preparing,
        /// <summary>Se le agoto el tiempo pero sigue sin facturar: panel TERMINADOS, todavia con sus acciones.</summary>
        AwaitingBilling,
        /// <summary>Facturado: panel TERMINADOS.</summary>
        Billed,
    }

    /// <summary>
    /// Tiempo de preparacion de un domicilio. La etapa se deduce de la fecha y la hora del pedido, sin guardar
    /// nada: asi la cuenta sobrevive a cerrar la aplicacion y todos los equipos ven lo mismo. El pedido que
    /// agota su tiempo sigue PENDIENTE en la base (hay que poder facturarlo); solo cambia de panel.
    /// </summary>
    public static class DeliveryTiming
    {
        private const int DefaultMinutes = 45;

        /// <summary>45 minutos, o lo que diga la clave opcional <c>DeliveryPreparationMinutes</c> de appSettings.</summary>
        public static readonly TimeSpan PreparationTime = ReadPreparationTime();

        private static TimeSpan ReadPreparationTime()
        {
            int minutes;
            string configured = ConfigurationManager.AppSettings["DeliveryPreparationMinutes"];
            if (!int.TryParse(configured, NumberStyles.Integer, CultureInfo.InvariantCulture, out minutes)
                || minutes < 1 || minutes > 24 * 60)
                minutes = DefaultMinutes;
            return TimeSpan.FromMinutes(minutes);
        }

        /// <summary>Momento en que se tomo el pedido; null si la hora guardada no se puede leer.</summary>
        public static DateTime? StartOf(Pedidos order)
        {
            TimeSpan time;
            if (TimeSpan.TryParse(order.Hora_pedido, CultureInfo.InvariantCulture, out time)
                && time >= TimeSpan.Zero && time < TimeSpan.FromDays(1))
                return order.Fecha_pedido.Date.Add(time);
            return null;
        }

        /// <summary>Lo que le queda de preparacion, entre cero y <see cref="PreparationTime"/>.</summary>
        public static TimeSpan Remaining(Pedidos order, DateTime now)
        {
            DateTime? start = StartOf(order);
            // Sin hora legible no se puede cronometrar: se deja en preparacion hasta que se facture.
            if (!start.HasValue)
                return PreparationTime;

            TimeSpan remaining = start.Value + PreparationTime - now;
            if (remaining < TimeSpan.Zero) return TimeSpan.Zero;
            // Un reloj atrasado no debe regalar mas tiempo del previsto.
            return remaining > PreparationTime ? PreparationTime : remaining;
        }

        public static DeliveryStage StageOf(Pedidos order, DateTime now)
        {
            if (!"PENDIENTE".Equals(order.Estado_pedido))
                return DeliveryStage.Billed;
            return Remaining(order, now) > TimeSpan.Zero ? DeliveryStage.Preparing : DeliveryStage.AwaitingBilling;
        }
    }
}
