using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaPresentacion.Formularios.FormsPedido.Platos
{
    public class EPlatos
    {
        private int _id_plato;
        private string _nombre_plato;
        private int _precio_plato;
        private string _imagen_plato;
        private string _descripcion_plato;
        private int _id_tipo_plato;
        private string _tipo_plato;

        private int _id_pedido;
        private int _cantidad_pedido;
        private int _precio_cobrar;
        private string _observaciones_pedido;

        private int _cantidad_pedido_editar;

        public int Id_plato { get => _id_plato; set => _id_plato = value; }
        public string Nombre_plato { get => _nombre_plato; set => _nombre_plato = value; }
        public int Precio_plato { get => _precio_plato; set => _precio_plato = value; }
        public string Imagen_plato { get => _imagen_plato; set => _imagen_plato = value; }
        public string Descripcion_plato { get => _descripcion_plato; set => _descripcion_plato = value; }
        public int Id_tipo_plato { get => _id_tipo_plato; set => _id_tipo_plato = value; }
        public string Tipo_plato { get => _tipo_plato; set => _tipo_plato = value; }

        public int Cantidad_pedido { get => _cantidad_pedido; set => _cantidad_pedido = value; }
        public int Precio_cobrar { get => _precio_cobrar; set => _precio_cobrar = value; }
        public string Observaciones_pedido { get => _observaciones_pedido; set => _observaciones_pedido = value; }
        public int Id_pedido { get => _id_pedido; set => _id_pedido = value; }
        public int Cantidad_pedido_editar { get => _cantidad_pedido_editar; set => _cantidad_pedido_editar = value; }
    }
}
