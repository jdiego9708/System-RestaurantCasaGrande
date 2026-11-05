using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CapaEntidades.Models;

namespace CapaDatos
{
    /// <summary>Acceso a la tabla Domicilios (direcciones de entrega de los clientes).</summary>
    public static class DAddresses
    {
        /// <summary>Direcciones activas del cliente, la principal primero. Lista vacia si no tiene.</summary>
        public static List<CustomerAddress> ListByCustomer(int customerId)
        {
            List<CustomerAddress> result = new List<CustomerAddress>();
            using (SqlConnection con = new SqlConnection(Conexion.Cn))
            using (SqlCommand cmd = new SqlCommand("sp_Domicilios_listar", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Id_cliente", SqlDbType.Int).Value = customerId;
                DataTable table = new DataTable();
                new SqlDataAdapter(cmd).Fill(table);
                foreach (DataRow row in table.Rows)
                    result.Add(new CustomerAddress(row));
            }
            return result;
        }

        /// <summary>Una direccion por Id aunque este inactiva (un pedido viejo puede apuntar a ella). Null si no existe.</summary>
        public static CustomerAddress Get(int addressId)
        {
            using (SqlConnection con = new SqlConnection(Conexion.Cn))
            using (SqlCommand cmd = new SqlCommand("sp_Domicilio_obtener", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Id_direccion", SqlDbType.Int).Value = addressId;
                DataTable table = new DataTable();
                new SqlDataAdapter(cmd).Fill(table);
                return table.Rows.Count == 0 ? null : new CustomerAddress(table.Rows[0]);
            }
        }

        /// <summary>
        /// Crea (AddressId = 0) o edita una direccion. Devuelve "OK" o el mensaje de error; el Id queda en
        /// <paramref name="address"/>.
        /// </summary>
        public static string Save(CustomerAddress address)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(Conexion.Cn))
                using (SqlCommand cmd = new SqlCommand("sp_Domicilio_guardar", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    SqlParameter id = cmd.Parameters.Add("@Id_direccion", SqlDbType.Int);
                    id.Direction = ParameterDirection.InputOutput;
                    id.Value = address.AddressId;
                    cmd.Parameters.Add("@Id_cliente", SqlDbType.Int).Value = address.CustomerId;
                    cmd.Parameters.Add("@Alias", SqlDbType.VarChar, 50).Value = Upper(address.Alias);
                    cmd.Parameters.Add("@Direccion", SqlDbType.VarChar, 200).Value = Upper(address.Address);
                    cmd.Parameters.Add("@Barrio", SqlDbType.VarChar, 100).Value = Upper(address.Neighborhood);
                    cmd.Parameters.Add("@Referencia", SqlDbType.VarChar, 200).Value = Upper(address.Reference);
                    cmd.Parameters.Add("@Telefono_contacto", SqlDbType.VarChar, 50).Value = Upper(address.ContactPhone);
                    cmd.Parameters.Add("@Es_principal", SqlDbType.Bit).Value = address.IsPrimary;

                    DataTable table = new DataTable();
                    new SqlDataAdapter(cmd).Fill(table);
                    address.AddressId = Convert.ToInt32(table.Rows[0]["Id_direccion"]);
                    return "OK";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>Inactiva la direccion (no se borra: los pedidos viejos siguen apuntando a ella).</summary>
        public static string Deactivate(int addressId)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(Conexion.Cn))
                using (SqlCommand cmd = new SqlCommand("sp_Domicilio_inactivar", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Id_direccion", SqlDbType.Int).Value = addressId;
                    con.Open();
                    cmd.ExecuteNonQuery();
                    return "OK";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        // Los datos del restaurante se guardan en mayusculas; el vacio se guarda como cadena vacia.
        private static string Upper(string text) => (text ?? string.Empty).Trim().ToUpperInvariant();
    }
}
