using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaEntidades.Models
{
    public class Mesas
    {
        public Mesas()
        {

        }

        public Mesas(DataRow row)
        {
            try
            {
                this.Id_mesa = Convert.ToInt32(row["Id_mesa"]);
                this.Num_mesa = Convert.ToInt32(row["Num_mesa"]);
                this.Descripcion_mesa = Convert.ToString(row["Descripcion_mesa"]);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex.Message, null);
            }
        }

        public int Id_mesa { get; set; }

        public int Num_mesa { get; set; }

        public string Descripcion_mesa { get; set; }

        public event EventHandler OnError;
    }
}
