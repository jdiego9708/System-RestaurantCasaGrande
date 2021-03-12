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
