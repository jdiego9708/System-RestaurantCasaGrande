using CapaDatos;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaNegocio
{
    public class NNotas
    {
        #region INSERTAR NOTA

        public static string InsertarNota(List<string> variables, out int id_nota)
        {
            return DNotas.InsertarNota(variables, out id_nota);
        }

        #endregion

        #region EDITAR NOTA

        public static string EditarNota(int id_nota, List<string> variables)
        {
            return DNotas.EditarNota(id_nota, variables);
        }

        #endregion

        #region BUSCAR NOTAS

        public static DataTable BuscarNotas(string tipo_busqueda, string texto_busqueda, out string rpta)
        {
            return DNotas.BuscarNotas(tipo_busqueda, texto_busqueda, out rpta);
        }

        #endregion
    }
}
