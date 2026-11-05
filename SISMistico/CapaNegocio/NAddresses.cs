using System.Collections.Generic;
using CapaDatos;
using CapaEntidades.Models;

namespace CapaNegocio
{
    /// <summary>Direcciones de entrega de los clientes (domicilios).</summary>
    public static class NAddresses
    {
        public static List<CustomerAddress> ListByCustomer(int customerId) => DAddresses.ListByCustomer(customerId);

        public static CustomerAddress Get(int addressId) => DAddresses.Get(addressId);

        /// <summary>Valida lo minimo y guarda. Devuelve "OK" o el motivo del rechazo.</summary>
        public static string Save(CustomerAddress address)
        {
            if (address.CustomerId <= 0)
                return "Seleccione un cliente antes de guardar la direccion";
            if (string.IsNullOrWhiteSpace(address.Address))
                return "La direccion es obligatoria";
            return DAddresses.Save(address);
        }

        public static string Deactivate(int addressId) => DAddresses.Deactivate(addressId);
    }
}
