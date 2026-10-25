using System;
using System.Data;

using CapaDatos;

namespace CapaNegocio
{
    /// <summary>Empleado autenticado por la API móvil (nunca incluye contraseña ni PIN).</summary>
    public class EmpleadoMovil
    {
        public int IdEmpleado { get; set; }
        public string Nombre { get; set; }
        public string Cargo { get; set; }
        public string Estado { get; set; }
        public bool TienePin { get; set; }

        public bool EstaActivo
        {
            get { return string.Equals(Estado, "ACTIVO", StringComparison.OrdinalIgnoreCase); }
        }
    }

    /// <summary>Último turno existente, solo lectura.</summary>
    public class TurnoMovil
    {
        public int IdTurno { get; set; }
        public DateTime Fecha { get; set; }
        public string Estado { get; set; }
    }

    public class NApiMovil
    {
        private static readonly DApiMovil Datos = new DApiMovil();

        public static bool ProbarConexion()
        {
            return Datos.ProbarConexion();
        }

        /// <summary>Devuelve el empleado si las credenciales coinciden; null si no.</summary>
        public static EmpleadoMovil ValidarEmpleado(int idEmpleado, string password, int pin, bool usarPin)
        {
            DataTable t = Datos.ValidarEmpleado(idEmpleado, password, pin, usarPin);
            if (t.Rows.Count == 0)
                return null;

            DataRow r = t.Rows[0];
            return new EmpleadoMovil
            {
                IdEmpleado = Convert.ToInt32(r["Id_empleado"]),
                Nombre = Convert.ToString(r["Nombre_empleado"]),
                Cargo = Convert.ToString(r["Cargo_empleado"]),
                Estado = Convert.ToString(r["Estado"]),
                TienePin = r["Codigo_maestro"] != DBNull.Value
            };
        }

        // Catálogo y mesas: tablas tal cual; el mapeo a JSON lo hace la capa Api.
        public static DataTable TiposPlatos() { return Datos.TiposPlatos(); }
        public static DataTable TiposBebidas() { return Datos.TiposBebidas(); }
        public static DataTable Platos(int? idTipo, int? idPlato, string texto) { return Datos.Platos(idTipo, idPlato, texto); }
        public static DataTable Bebidas(int? idTipo, string texto) { return Datos.Bebidas(idTipo, texto); }
        public static DataTable Ingredientes(string tipo) { return Datos.Ingredientes(tipo); }
        public static DataTable Clientes(string texto) { return Datos.Clientes(texto); }
        public static DataTable Mesas(DateTime fecha) { return Datos.Mesas(fecha); }

        // Alta de pedidos
        public static DataTable MesaPorId(int idMesa) { return Datos.MesaPorId(idMesa); }
        public static DataTable PedidoAbiertoDeMesa(int numMesa, DateTime fecha) { return Datos.PedidoAbiertoDeMesa(numMesa, fecha); }
        public static bool ClienteActivo(int idCliente) { return Datos.ClienteActivo(idCliente); }
        public static DataTable Productos(bool plato, string idsJson) { return Datos.Productos(plato, idsJson); }
        public static DataTable IngredientesPorIds(string idsJson) { return Datos.IngredientesPorIds(idsJson); }
        public static DataSet PedidoCompleto(int idPedido) { return Datos.PedidoCompleto(idPedido); }

        public static int InsertarPedidoMovil(int idMesa, int idEmpleado, int idCliente, string tipoPedido,
                                              string observaciones, int cantidadClientes, string itemsJson)
        {
            return Datos.InsertarPedidoMovil(idMesa, idEmpleado, idCliente, tipoPedido, observaciones, cantidadClientes, itemsJson);
        }

        /// <summary>Último turno o null si la tabla está vacía.</summary>
        public static TurnoMovil UltimoTurno()
        {
            DataTable t = Datos.UltimoTurno();
            if (t.Rows.Count == 0)
                return null;

            DataRow r = t.Rows[0];
            return new TurnoMovil
            {
                IdTurno = Convert.ToInt32(r["Id_turno"]),
                Fecha = Convert.ToDateTime(r["Fecha_turno"]),
                Estado = Convert.ToString(r["Estado_turno"])
            };
        }
    }
}
