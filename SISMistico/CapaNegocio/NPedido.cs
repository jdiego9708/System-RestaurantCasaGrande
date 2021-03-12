using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using CapaDatos;

namespace CapaNegocio
{
    public class NPedido
    {
        #region INSERTAR PEDIDO
        public static string InsertarPedido(List<string> variables, 
            out int id_pedido, DataTable Detalles)
        {
            DPedidos DPedidos = new DPedidos();
            return DPedidos.InsertarPedido(variables, Detalles, out id_pedido);
        }

        #endregion

        #region CANCELAR DOMICILIO
        public static string CancelarDomicilio(int id_pedido, string observaciones)
        {
            DPedidos DPedidos = new DPedidos();
            return DPedidos.CancelarDomicilio(id_pedido, observaciones);
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
        public static string ActualizarDetallePedido(List<string> variables)
        {
            DPedidos DPedidos = new DPedidos();
            return DPedidos.ActualizarDetallePedido(variables);
        }

        #endregion

        #region BUSCAR PEDIDOS

        public static DataTable BuscarPedidosYDetalle(string tipo_busqueda, string texto_busqueda,
            out DataTable TablaDetalle, out string rpta)
        {
            return DPedidos.BuscarPedidosYDetalle(tipo_busqueda, texto_busqueda, out TablaDetalle, out rpta);
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
    }
}
