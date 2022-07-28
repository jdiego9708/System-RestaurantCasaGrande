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
    public class NIngresos
    {
        #region INSERTAR INGRESOS

        public static async Task<(string rpta, int id_ingreso)> InsertarIngresos(Ingresos ingreso)
        {
            DIngresos DIngresos = new DIngresos();
            return await DIngresos.InsertarIngreso(ingreso);
        }

        #endregion

        #region EDITAR INGRESOS

        public static async Task<string> EditarIngreso(int id_ingreso, Ingresos ingreso)
        {
            DIngresos DIngresos = new DIngresos();
            return await DIngresos.EditarIngreso(id_ingreso, ingreso);
        }
        #endregion

        #region BUSCAR INGRESOS

        public static async Task<(string rpta, DataTable dtIngresos)> BuscarIngresos(string tipo_busqueda, string texto_busqueda)
        {
            DIngresos DIngresos = new DIngresos();
            return await DIngresos.BuscarIngresos(tipo_busqueda, texto_busqueda);
        }

        public static async Task<(string rpta, DataTable dtIngresos)> BuscarIngresos(string tipo_busqueda, string texto_busqueda1, string texto_busqueda2)
        {
            DIngresos DIngresos = new DIngresos();
            return await DIngresos.BuscarIngresos(tipo_busqueda, texto_busqueda1, texto_busqueda2);
        }

        #endregion
    }
}
