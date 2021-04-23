using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaEntidades.Models
{
    public class Pedidos
    {
        public Pedidos()
        {

        }

        public Pedidos(DataRow row)
        {
            try
            {
                this.Id_pedido = Convert.ToInt32(row["Id_pedido"]);
                this.Mesa = new Mesas(row);
                this.Id_empleado = Convert.ToInt32(row["Id_empleado"]);
                this.Empleado = new Empleados(row);
                this.Id_cliente = Convert.ToInt32(row["Id_cliente"]);
                this.Cliente = new Clientes(row);
                this.Estado_pedido = Convert.ToString(row["Estado_pedido"]);
                this.Fecha_pedido = Convert.ToDateTime(row["Fecha_pedido"]);
                this.Hora_pedido = Convert.ToString(row["Hora_pedido"]);
                this.Tipo_pedido = Convert.ToString(row["Tipo_pedido"]);
                this.Observaciones_pedido = Convert.ToString(row["Observaciones_pedido"]);
                this.CantidadClientes = Convert.ToInt32(row["CantidadClientes"]);
            }
            catch (Exception)
            {

            }
        }

        public int Id_pedido { get; set; }

        public int Id_mesa { get; set; }

        public Mesas Mesa { get; set; }

        public int Id_empleado { get; set; }

        public Empleados Empleado { get; set; }

        public int Id_cliente { get; set; }

        public Clientes Cliente { get; set; }

        public string Estado_pedido { get; set; }

        public DateTime Fecha_pedido { get; set; } = DateTime.Now;

        public string Hora_pedido { get; set; }

        public string Tipo_pedido { get; set; }

        public string Observaciones_pedido { get; set; }

        public int CantidadClientes { get; set; }
    }
}
