using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CapaDatos;
using System.Data;
using CapaEntidades.Models;

namespace CapaNegocio
{
    public class NIngredientes
    {
        #region INSERTAR INGREDIENTE

        public static async Task<(string rpta, int id)> InsertarIngrediente(Ingredientes ing)
        {
            DIngredientes DIngredientes = new DIngredientes();
            return await DIngredientes.InsertarIngrediente(ing);
        }

        #endregion

        #region EDITAR INGREDIENTE

        public static async Task<string> EditarIngrediente(int id_ingrediente, Ingredientes ing)
        {
            DIngredientes DIngredientes = new DIngredientes();
            return await DIngredientes.EditarIngrediente(id_ingrediente, ing);
        }

        #endregion

        #region BUSCAR INGREDIENTES

        public static async Task<(string rpta, DataTable dtIngrediente)> BuscarIngredientes(string tipo_busqueda, string texto_busqueda)
        {
            DIngredientes DIngredientes = new DIngredientes();
            return await DIngredientes.BuscarIngredientes(tipo_busqueda, texto_busqueda);
        }

        public static DataTable BuscarIngredientes(string tipo_busqueda, string texto_busqueda, out string rpta)
        {
            DIngredientes DIngredientes = new DIngredientes();
            return DIngredientes.BuscarIngredientes(tipo_busqueda, texto_busqueda, out rpta);
        }

        #endregion        
    }
}
