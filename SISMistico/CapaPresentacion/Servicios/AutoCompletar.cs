using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Forms;
using System.Data;
using CapaNegocio;

namespace CapaPresentacion
{
    public class AutoCompletar
    {
        public static AutoCompleteStringCollection AutoCompleteBebida(DataTable Tabla)
        {
            AutoCompleteStringCollection source = new AutoCompleteStringCollection();
            try
            {
                for (int i = 0; i <= Tabla.Columns.Count - 1; i++)
                {
                    source.Add(Convert.ToString(Tabla.Rows[0][i]));
                }
            }
            catch (Exception)
            {
                source = null;
            }
            return source;
        }

        public static AutoCompleteStringCollection AutoCompletePlato(DataTable Tabla)
        {
            AutoCompleteStringCollection source = new AutoCompleteStringCollection();
            try
            {
                for (int i = 0; i <= Tabla.Columns.Count - 1; i++)
                {
                    source.Add(Convert.ToString(Tabla.Rows[0][i]));
                }
            }
            catch (Exception)
            {
                source = null;
            }
            return source;
        }
    }
}
