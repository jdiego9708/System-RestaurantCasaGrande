using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaEntidades.Models
{
    public class Pedido
    {
        public Pedido()
        {

        }

        public Pedido(DataRow row)
        {
            try
            {
                this.Id_pedido = Convert.ToInt32(row["Id_pedido"]);
                this.Id_mesa = Convert.ToInt32(row["Id_mesa"]);
                this.Id_empleado = Convert.ToInt32(row["Id_empleado"]);
                this.Id_cliente = Convert.ToInt32(row["Id_cliente"]);
                this.Estado_pedido = Convert.ToString(row["Estado_pedido"]);
                this.Fecha_pedido = Convert.ToDateTime(row["Fecha_pedido"]);
                this.Hora_pedido = Convert.ToString(row["Hora_pedido"]);
                this.Tipo_pedido = Convert.ToString(row["Tipo_pedido"]);
                this.Observaciones_pedido = Convert.ToString(row["Observaciones_pedido"]);
            }
            catch (Exception ex)
            {

            }
        }

        public int Id_pedido { get; set; }

        public int Id_mesa { get; set; }

        public int Id_empleado { get; set; }

        public int Id_cliente { get; set; }

        public string Estado_pedido { get; set; }

        public DateTime Fecha_pedido { get; set; } = DateTime.Now;

        public string Hora_pedido { get; set; }

        public string Tipo_pedido { get; set; }

        public string Observaciones_pedido { get; set; }

    }
}
