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
    public class NTurnos
    {
        #region BUSCAR TURNOS

        public static async Task<(string rpta, DataTable dtTurnos)> BuscarTurnos(string tipo_busqueda, string texto_busqueda1,
            string texto_busqueda2)
        {
            DEgresos DEgresos = new DEgresos();
            return await DEgresos.BuscarEgreso(tipo_busqueda, texto_busqueda1, texto_busqueda2);
        }

        #endregion
    }
}
