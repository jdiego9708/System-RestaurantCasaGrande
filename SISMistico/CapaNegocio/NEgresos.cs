using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CapaDatos;
using System.Data;
using CapaEntidades.Models;
using CapaEntidades.Helpers;

namespace CapaNegocio
{
    public class NEgresos
    {
        #region INSERTAR EGRESOS

        public static async Task<(string rpta, int id_egreso)> InsertarEgresos(Egresos egreso)
        {
            DEgresos DEgresos = new DEgresos();
            return await DEgresos.InsertarEgreso(egreso);
        }

        #endregion

        #region EDITAR EGRESOS

        public static async Task<string> EditarEgreso(int id_egreso, Egresos egreso)
        {
            DEgresos DEgresos = new DEgresos();
            return await DEgresos.EditarEgreso(id_egreso, egreso);
        }
        #endregion

        #region BUSCAR EGRESOS

        public static async Task<(string rpta, DataTable dtEgresos)> BuscarEgresos(string tipo_busqueda, string texto_busqueda)
        {
            DEgresos DEgresos = new DEgresos();
            return await DEgresos.BuscarEgreso(tipo_busqueda, texto_busqueda);
        }

        public static async Task<(string rpta, DataTable dtEgresos)> BuscarEgresos(string tipo_busqueda, string texto_busqueda1, string texto_busqueda2)
        {
            DEgresos DEgresos = new DEgresos();
            return await DEgresos.BuscarEgreso(tipo_busqueda, texto_busqueda1, texto_busqueda2);
        }

        public static async Task<(string rpta, DataTable dtEgresos)> BuscarEgresos(ModelHelperSearch modelSearch)
        {
            DEgresos DEgresos = new DEgresos();
            return await DEgresos.BuscarEgreso(modelSearch);
        }

        #endregion
    }
}
