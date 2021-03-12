using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CapaDatos;
using System.Data;

namespace CapaNegocio
{
    public class NMesas
    {
        #region BUSCAR MESAS

        public static DataTable BuscarMesas(string tipo_busqueda, string texto_busqueda)
        {
            return DMesas.BuscarMesas(tipo_busqueda, texto_busqueda);
        }

        #endregion
    }
}
