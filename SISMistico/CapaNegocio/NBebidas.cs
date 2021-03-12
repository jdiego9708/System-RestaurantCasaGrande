using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CapaDatos;
using System.Data;

namespace CapaNegocio
{
    public class NBebidas
    {
        #region INSERTAR BEBIDA

        public static string InsertarBebida(List<string> variables, out int id_bebida)
        {
            DBebidas DBebidas = new DBebidas
            {
                Variables = variables
            };
            return DBebidas.InsertarBebida(DBebidas, out id_bebida);
        }

        #endregion

        #region EDITAR BEBIDA

        public static string EditarBebida(List<string> variables)
        {
            DBebidas DBebidas = new DBebidas
            {
                Variables = variables
            };
            return DBebidas.EditarBebida(DBebidas);
        }

        #endregion

        #region BUSCAR BEBIDA

        public static DataTable BuscarBebida(string tipo_busqueda, string texto_busqueda,
            string estado_bebida, out string rpta)
        {
            return DBebidas.BuscarBebidas(tipo_busqueda, texto_busqueda, estado_bebida, out rpta);
        }

        #endregion

        #region BUSCAR TIPO BEBIDA

        public static DataTable BuscarTipoBebidas(string tipo_busqueda, string texto_busqueda)
        {
            return DBebidas.BuscarTipoBebidas(tipo_busqueda, texto_busqueda);
        }

        #endregion

        #region INACTIVAR BEBIDA

        public static string InactivarBebida(int id_bebida)
        {
            DBebidas DBebidas = new DBebidas();
            return DBebidas.InactivarBebidas(id_bebida);
        }

        #endregion
    }
}
