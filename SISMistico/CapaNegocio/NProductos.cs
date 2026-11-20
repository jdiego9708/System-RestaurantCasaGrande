using System.Data;
using CapaDatos;

namespace CapaNegocio
{
    public static class NProductos
    {
        public static string DatabaseIdentity()
        {
            var connection = new System.Data.SqlClient.SqlConnectionStringBuilder(Conexion.Cn);
            return connection.DataSource + "|" + connection.InitialCatalog;
        }
        public static DataTable Listar() { return DProductos.Listar(); }
        public static DataTable Categorias(bool bebida) { return DProductos.Categorias(bebida); }
        public static void Guardar(int? id, bool bebida, string nombre, int categoria, decimal precio,
            string descripcion, bool activo, bool carta, bool? bebidaOriginal = null, bool ingredientes = false)
        { DProductos.Guardar(id, bebida, nombre, categoria, precio, descripcion, activo, carta, bebidaOriginal, ingredientes); }
        public static void CambiarEstado(int id, bool bebida, bool activo)
        { DProductos.CambiarEstado(id, bebida, activo); }
    }
}
