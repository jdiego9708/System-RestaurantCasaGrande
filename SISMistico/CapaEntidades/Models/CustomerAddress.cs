using System;
using System.Data;

namespace CapaEntidades.Models
{
    /// <summary>
    /// Direccion de entrega de un cliente (tabla Domicilios). Un cliente puede tener varias.
    /// </summary>
    public class CustomerAddress
    {
        public CustomerAddress()
        {
        }

        public CustomerAddress(DataRow row)
        {
            this.AddressId = Convert.ToInt32(row["Id_direccion"]);
            this.CustomerId = Convert.ToInt32(row["Id_cliente"]);
            this.Alias = Convert.ToString(row["Alias"]);
            this.Address = Convert.ToString(row["Direccion"]);
            this.Neighborhood = Convert.ToString(row["Barrio"]);
            this.Reference = Convert.ToString(row["Referencia"]);
            this.ContactPhone = Convert.ToString(row["Telefono_contacto"]);
            this.IsPrimary = Convert.ToBoolean(row["Es_principal"]);
            this.Status = Convert.ToString(row["Estado"]);
        }

        public int AddressId { get; set; }

        public int CustomerId { get; set; }

        public string Alias { get; set; }

        public string Address { get; set; }

        public string Neighborhood { get; set; }

        public string Reference { get; set; }

        public string ContactPhone { get; set; }

        public bool IsPrimary { get; set; }

        public string Status { get; set; }

        /// <summary>Texto de una linea para listas y para la comanda: "OFICINA - CLL 4 # 5-6 (CENTRO)".</summary>
        public string Summary
        {
            get
            {
                string text = string.IsNullOrWhiteSpace(this.Alias) ? this.Address : this.Alias + " - " + this.Address;
                return string.IsNullOrWhiteSpace(this.Neighborhood) ? text : text + " (" + this.Neighborhood + ")";
            }
        }

        public override string ToString() => this.Summary;
    }
}
