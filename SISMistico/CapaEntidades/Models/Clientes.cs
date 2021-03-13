using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaEntidades.Models
{
    public class Clientes
    {
        public Clientes()
        {

        }

        public Clientes(DataRow row)
        {
            try
            {
                this.Id_cliente = Convert.ToInt32(row["Id_cliente"]);
                this.Nombre_cliente = Convert.ToString(row["Nombre_cliente"]);
                this.Telefono_cliente = Convert.ToString(row["Telefono_cliente"]);
                this.Correo_electronico = Convert.ToString(row["Correo_electronico"]);
                this.Direccion_cliente = Convert.ToString(row["Direccion_cliente"]);
                this.Referencia_ubicacion = Convert.ToString(row["Referencia_ubicacion"]);
                this.Otras_observaciones = Convert.ToString(row["Otras_observaciones"]);
                this.Estado_cliente = Convert.ToString(row["Estado_cliente"]);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex.Message, null);
            }
        }

        public int Id_cliente { get; set; }

        public string Nombre_cliente { get; set; }

        public string Telefono_cliente { get; set; }

        public string Correo_electronico { get; set; }

        public string Direccion_cliente { get; set; }

        public string Referencia_ubicacion { get; set; }

        public string Otras_observaciones { get; set; }

        public string Estado_cliente { get; set; }

        public event EventHandler OnError;
    }
}
