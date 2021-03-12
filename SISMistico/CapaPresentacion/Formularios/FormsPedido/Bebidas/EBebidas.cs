using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaPresentacion.Formularios.FormsPedido.Bebidas
{
    public class EBebidas
    {
        private int _id_bebida;
        private string _nombre_bebida;
        private int _precio_bebida;
        private int _precio_trago;
        private int _precio_trago_doble;
        private int _precio_proveedor;
        private int _id_proveedor;
        private string _imagen;
        private int _cantidad_unidades;
        private int _cantidad_x_unidad;
        private int _cantidad_total;
        private int _id_tipo_bebida;
        private string _tipo_bebida;

        private int _precio_cobrar;
        private int _cantidad_pedido;
        private string _observaciones_pedido;
        private int _id_pedido;
        private int _cantidad_pedido_editar;

        public int Id_bebida { get => _id_bebida; set => _id_bebida = value; }
        public string Nombre_bebida { get => _nombre_bebida; set => _nombre_bebida = value; }
        public int Precio_bebida { get => _precio_bebida; set => _precio_bebida = value; }
        public int Precio_trago { get => _precio_trago; set => _precio_trago = value; }
        public int Precio_trago_doble { get => _precio_trago_doble; set => _precio_trago_doble = value; }
        public int Precio_proveedor { get => _precio_proveedor; set => _precio_proveedor = value; }
        public int Id_proveedor { get => _id_proveedor; set => _id_proveedor = value; }
        public string Imagen { get => _imagen; set => _imagen = value; }
        public int Cantidad_unidades { get => _cantidad_unidades; set => _cantidad_unidades = value; }
        public int Cantidad_x_unidad { get => _cantidad_x_unidad; set => _cantidad_x_unidad = value; }
        public int Cantidad_total { get => _cantidad_total; set => _cantidad_total = value; }
        public int Id_tipo_bebida { get => _id_tipo_bebida; set => _id_tipo_bebida = value; }
        public string Tipo_bebida { get => _tipo_bebida; set => _tipo_bebida = value; }
        public string Observaciones_pedido { get => _observaciones_pedido; set => _observaciones_pedido = value; }
        public int Cantidad_pedido { get => _cantidad_pedido; set => _cantidad_pedido = value; }
        public int Precio_cobrar { get => _precio_cobrar; set => _precio_cobrar = value; }
        public int Id_pedido { get => _id_pedido; set => _id_pedido = value; }
        public int Cantidad_pedido_editar { get => _cantidad_pedido_editar; set => _cantidad_pedido_editar = value; }
    }
}
