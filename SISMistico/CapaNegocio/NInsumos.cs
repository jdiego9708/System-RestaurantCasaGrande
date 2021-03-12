using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CapaDatos;
using System.Data;

namespace CapaNegocio
{
    public class NInsumos
    {
        #region INSERTAR INSUMOS

        public static string InsertarInsumos(List<string> vs)
        {
            DInsumos DInsumos = new DInsumos();
            return DInsumos.InsertarInsumos(vs);
        }

        #endregion

        #region EDITAR INSUMOS

        public static string EditarInsumos(List<string> vs)
        {
            DInsumos DInsumos = new DInsumos();
            return DInsumos.EditarInsumos(vs);
        }

        #endregion

        #region BUSCAR INSUMOS

        public static DataTable BuscarInsumos(string tipo_busqueda, string texto_busqueda, out string rpta)
        {
            return DInsumos.BuscarInsumos(tipo_busqueda, texto_busqueda, out rpta);
        }

        #endregion

        #region BUSCAR TIPO INSUMOS

        public static DataTable BuscarTipoInsumos(string tipo_busqueda, string texto_busqueda)
        {
            return DInsumos.BuscarTipoInsumos(tipo_busqueda, texto_busqueda);
        }

        #endregion
    }
}
