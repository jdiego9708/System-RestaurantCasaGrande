using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using CapaDatos;
using CapaEntidades.Models;

namespace CapaNegocio
{
    public class NPedido
    {
        #region INSERTAR PEDIDO
        public static string InsertarPedido(Pedidos pedido)
        {
            DPedidos DPedidos = new DPedidos();
            return DPedidos.InsertarPedido(pedido);
        }

        #endregion

        #region CANCELAR PEDIDO
        public static string CancelarPedido(int id_pedido, string observaciones)
        {
            DPedidos DPedidos = new DPedidos();
            return DPedidos.CancelarPedido(id_pedido, observaciones);
        }

        #endregion

        #region CAMBIAR ESTADO PEDIDO
        public static string CambiarEstadoPedido(List<string> variables)
        {
            DPedidos DPedidos = new DPedidos();
            return DPedidos.CambiarEstadoPedido(variables);
        }

        #endregion

        #region ACTUALIZAR DETALLE PEDIDO
        public static string ActualizarDetallePedido(Detalle_pedido detalle, int id_empleado, string tipo_update)
        {
            DPedidos DPedidos = new DPedidos();
            return DPedidos.ActualizarDetallePedido(detalle, id_empleado, tipo_update);
        }

        #endregion

        #region BUSCAR PEDIDOS

        public static DataTable BuscarPedidosYDetalle(string tipo_busqueda, string texto_busqueda,
            out DataTable TablaDetalle,
            out DataTable dtDetallePlatosDetallado, out string rpta)
        {
            return DPedidos.BuscarPedidosYDetalle(tipo_busqueda, texto_busqueda, 
                out TablaDetalle, out dtDetallePlatosDetallado, out rpta);
        }

        public static DataTable BuscarPedidos(string tipo_busqueda, string texto_busqueda)
        {
            return DPedidos.BuscarPedidos(tipo_busqueda, texto_busqueda);
        }

        public async static Task<(string rpta, DataTable dtPedidos)>BuscarPedidos(string tipo_busqueda, string texto_busqueda1, string texto_busqueda2)
        {
            return await DPedidos.BuscarPedidos(tipo_busqueda, texto_busqueda1, texto_busqueda2);
        }

        public static DataTable BuscarPedidosEliminados(string tipo_busqueda, string texto_busqueda)
        {
            return DPedidos.BuscarPedidosEliminados(tipo_busqueda, texto_busqueda);
        }
        #endregion

        #region INSERTAR ELIMINACIÓN COMANDAS
        public static string InsertarEliminacionComanda(int id_pedido, int id_usuario_clave_maestra, int id_usuario_sesion,
            int id_tipo, string tipo, string observaciones)
        {
            return DPedidos.InsertarEliminaciónComanda(id_pedido, id_usuario_clave_maestra, id_usuario_sesion, id_tipo, tipo, observaciones);
        }
        #endregion

        #region METODO INSERTAR DETALLES INGREDIENTES PEDIDO
        public static async Task<string> InsertarDetalleIngredientesPedido(List<Detalle_ingredientes_pedido> detalles)
        {
            DPedidos DPedidos = new DPedidos();
            return await DPedidos.InsertarDetalleIngredientesPedido(detalles);
        }
        #endregion

        #region INSERTAR DETALLES PEDIDOS
        public static string InsertarDetallePedido(Detalle_pedido detalle)
        {
            DPedidos DPedidos = new DPedidos();
            return DPedidos.InsertarDetallePedido(detalle);
        }
        #endregion
    }
}
