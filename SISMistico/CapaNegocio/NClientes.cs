using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CapaDatos;
using System.Data;

namespace CapaNegocio
{
    public class NClientes
    {
        #region INSERTAR CLIENTE

        public static string InsertarCliente(string[] variables)
        {
            DClientes DClientes = new DClientes();
            DClientes.Variables = variables;
            return DClientes.InsertarClientes(DClientes);
        }

        #endregion

        #region BUSQUEDA RAPIDA

        public static DataTable SearchQuick(string text) => DClientes.SearchQuick(text);

        public static DataTable GetById(int customerId) => DClientes.GetById(customerId);

        #endregion

        #region EDITAR CLIENTE BASICO

        /// <summary>Cambia nombre, telefono y correo de un cliente. Devuelve "OK" o el motivo del rechazo.</summary>
        public static string UpdateCustomer(int customerId, string name, string phone, string email)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(phone))
                return "El nombre y el teléfono del cliente son obligatorios";
            return DClientes.UpdateCustomer(customerId, name, phone, email);
        }

        #endregion

        #region INSERTAR CLIENTE RAPIDO

        /// <summary>Crea un cliente con nombre y telefono (correo opcional) y devuelve "OK" y su Id.</summary>
        public static string InsertCustomer(string name, string phone, string email, out int customerId)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                customerId = 0;
                return "El nombre del cliente es obligatorio";
            }
            if (string.IsNullOrWhiteSpace(phone))
            {
                customerId = 0;
                return "El teléfono del cliente es obligatorio";
            }
            return DClientes.InsertCustomer(name, phone, email, out customerId);
        }

        #endregion

        #region EDITAR CLIENTE

        public static string EditarCliente(string[] variables)
        {
            DClientes DClientes = new DClientes();
            DClientes.Variables = variables;
            return DClientes.EditarClientes(DClientes);
        }

        #endregion

        #region BUSCAR CLIENTES

        public static DataTable BuscarClientes(string tipo_busqueda, string texto_busqueda)
        {
            return DClientes.BuscarClientes(tipo_busqueda, texto_busqueda);
        }

        #endregion
    }
}
