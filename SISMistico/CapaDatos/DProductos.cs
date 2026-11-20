using System;
using System.Data;
using System.Data.SqlClient;

namespace CapaDatos
{
    // Una identidad de producto siempre incluye Tipo + Id; los IDs de ambos catálogos pueden coincidir.
    public static class DProductos
    {
        public static DataTable Listar()
        {
            return Consultar(@"SELECT p.Id_plato Id, 'PLATO' Tipo, p.Nombre_plato Nombre,
                p.Id_tipo_plato CategoriaId, t.Tipo_plato Categoria, p.Precio_plato Precio,
                p.Descripcion_plato Descripcion, p.Estado, p.Plato_carta Carta, p.Plato_detallado Detallado
                FROM dbo.Platos p LEFT JOIN dbo.Tipo_platos t ON t.Id_tipo_plato=p.Id_tipo_plato
                UNION ALL
                SELECT b.Id_bebida, 'BEBIDA', b.Nombre_bebida, b.Id_tipo_bebida,
                t.Tipo_bebida, b.Precio_bebida, b.Descripcion_bebida, b.Estado, '', ''
                FROM dbo.Bebidas b LEFT JOIN dbo.Tipo_bebidas t ON t.Id_tipo_bebida=b.Id_tipo_bebida
                ORDER BY Nombre, Tipo", null);
        }

        public static DataTable Categorias(bool bebida)
        {
            return Consultar(bebida
                ? "SELECT Id_tipo_bebida Id, Tipo_bebida Nombre FROM dbo.Tipo_bebidas ORDER BY Tipo_bebida"
                : "SELECT Id_tipo_plato Id, Tipo_plato Nombre FROM dbo.Tipo_platos ORDER BY Tipo_plato", null);
        }

        private static DataTable Consultar(string sql, Action<SqlParameterCollection> parameters)
        {
            using (var cn = new SqlConnection(Conexion.Cn))
            using (var cmd = new SqlCommand(sql, cn))
            using (var adapter = new SqlDataAdapter(cmd))
            {
                if (parameters != null) parameters(cmd.Parameters);
                var result = new DataTable();
                adapter.Fill(result);
                return result;
            }
        }

        public static void Guardar(int? id, bool bebida, string nombre, int categoria,
            decimal precio, string descripcion, bool activo, bool carta, bool? bebidaOriginal = null, bool ingredientes = false)
        {
            if (string.IsNullOrWhiteSpace(nombre) || nombre.Trim().Length > 200)
                throw new ArgumentException("Escribe un nombre de hasta 200 caracteres.");
            if (precio <= 0 || precio > 99999999999999999.99m || decimal.Round(precio, 2) != precio)
                throw new ArgumentException("El precio debe ser mayor que cero y tener máximo dos decimales.");
            if (descripcion.Length > (bebida ? 200 : 500))
                throw new ArgumentException("La descripción supera la longitud permitida.");
            bool cambiarTipo = id.HasValue && bebidaOriginal.HasValue && bebidaOriginal.Value != bebida;
            string sql;
            if (cambiarTipo)
            {
                // Los pedidos, recetas, inventario y vínculos externos conservan la identidad histórica.
                // Nunca usar el ID de origen para actualizar una fila de la otra tabla: puede ser otro producto.
                sql = bebida
                    ? @"INSERT INTO dbo.Bebidas (Nombre_bebida, Id_tipo_bebida, Precio_bebida, Descripcion_bebida, Estado, Imagen,
                        Precio_trago, Precio_trago_doble, Precio_proveedor, Id_proveedor, Cantidad_unidades, Cantidad_x_unidad, Cantidad_total)
                        SELECT @nombre,@categoria,@precio,@descripcion,@estado,COALESCE(Imagen_plato,'SIN IMAGEN'),0,0,0,0,0,1,0
                        FROM dbo.Platos WITH (UPDLOCK,HOLDLOCK) WHERE Id_plato=@id"
                    : @"INSERT INTO dbo.Platos (Nombre_plato, Id_tipo_plato, Precio_plato, Descripcion_plato, Estado,
                        Imagen_plato, Plato_detallado, Plato_carta)
                        SELECT @nombre,@categoria,@precio,@descripcion,@estado,Imagen,@detallado,@carta
                        FROM dbo.Bebidas WITH (UPDLOCK,HOLDLOCK) WHERE Id_bebida=@id";
            }
            else if (id.HasValue)
                sql = bebida
                    ? "UPDATE dbo.Bebidas SET Nombre_bebida=@nombre, Id_tipo_bebida=@categoria, Precio_bebida=@precio, Descripcion_bebida=@descripcion, Estado=@estado WHERE Id_bebida=@id"
                    : "UPDATE dbo.Platos SET Nombre_plato=@nombre, Id_tipo_plato=@categoria, Precio_plato=@precio, Descripcion_plato=@descripcion, Estado=@estado, Plato_carta=@carta, Plato_detallado=@detallado WHERE Id_plato=@id";
            else
                sql = bebida
                    ? @"INSERT INTO dbo.Bebidas (Nombre_bebida, Id_tipo_bebida, Precio_bebida, Descripcion_bebida, Estado, Imagen,
                        Precio_trago, Precio_trago_doble, Precio_proveedor, Id_proveedor, Cantidad_unidades, Cantidad_x_unidad, Cantidad_total)
                        VALUES (@nombre,@categoria,@precio,@descripcion,@estado,'SIN IMAGEN',0,0,0,0,0,1,0)"
                    : @"INSERT INTO dbo.Platos (Nombre_plato, Id_tipo_plato, Precio_plato, Descripcion_plato, Estado, Imagen_plato, Plato_detallado, Plato_carta)
                        VALUES (@nombre,@categoria,@precio,@descripcion,@estado,'SIN IMAGEN',@detallado,@carta)";
            using (var cn = new SqlConnection(Conexion.Cn))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@nombre", SqlDbType.VarChar, 200).Value = nombre.Trim().ToUpperInvariant();
                cmd.Parameters.Add("@categoria", SqlDbType.Int).Value = categoria;
                var amount = cmd.Parameters.Add("@precio", SqlDbType.Decimal);
                amount.Precision = 19; amount.Scale = 2; amount.Value = precio;
                cmd.Parameters.Add("@descripcion", SqlDbType.VarChar, bebida ? 200 : 500).Value = descripcion.Trim();
                cmd.Parameters.Add("@estado", SqlDbType.VarChar, 50).Value = activo ? "ACTIVO" : "INACTIVO";
                cmd.Parameters.Add("@carta", SqlDbType.VarChar, 50).Value = carta ? "ACTIVO" : "INACTIVO";
                // Con "Habilitar ingredientes", al pedir el plato se abre la ventana para elegir sopa, arroz, acompanante...
                cmd.Parameters.Add("@detallado", SqlDbType.VarChar, 50).Value = ingredientes ? "ACTIVO" : "INACTIVO";
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = (object)id ?? DBNull.Value;
                cn.Open();
                using (var transaction = cn.BeginTransaction())
                {
                    cmd.Transaction = transaction;
                    try
                    {
                        // La categoría debe pertenecer al tipo elegido, incluso si otro usuario la ha retirado.
                        using (var categoryCommand = new SqlCommand(bebida
                            ? "SELECT COUNT(*) FROM dbo.Tipo_bebidas WITH (HOLDLOCK) WHERE Id_tipo_bebida=@categoria"
                            : "SELECT COUNT(*) FROM dbo.Tipo_platos WITH (HOLDLOCK) WHERE Id_tipo_plato=@categoria", cn, transaction))
                        {
                            categoryCommand.Parameters.Add("@categoria", SqlDbType.Int).Value = categoria;
                            if (Convert.ToInt32(categoryCommand.ExecuteScalar()) == 0)
                                throw new InvalidOperationException("Selecciona una categoría válida para el tipo de producto.");
                        }
                        if (cmd.ExecuteNonQuery() == 0)
                            throw new InvalidOperationException("El producto ya no existe. Actualiza el catálogo.");
                        if (cambiarTipo)
                        {
                            using (var archive = new SqlCommand(bebida
                                ? "UPDATE dbo.Platos SET Estado='INACTIVO' WHERE Id_plato=@id"
                                : "UPDATE dbo.Bebidas SET Estado='INACTIVO' WHERE Id_bebida=@id", cn, transaction))
                            {
                                archive.Parameters.Add("@id", SqlDbType.Int).Value = id.Value;
                                if (archive.ExecuteNonQuery() == 0)
                                    throw new InvalidOperationException("No se pudo actualizar el tipo del producto.");
                            }
                        }
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public static void CambiarEstado(int id, bool bebida, bool activo)
        {
            using (var cn = new SqlConnection(Conexion.Cn))
            using (var cmd = new SqlCommand(bebida
                ? "UPDATE dbo.Bebidas SET Estado=@estado WHERE Id_bebida=@id"
                : "UPDATE dbo.Platos SET Estado=@estado WHERE Id_plato=@id", cn))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
                cmd.Parameters.Add("@estado", SqlDbType.VarChar, 50).Value = activo ? "ACTIVO" : "INACTIVO";
                cn.Open();
                if (cmd.ExecuteNonQuery() == 0) throw new InvalidOperationException("El producto ya no existe.");
            }
        }
    }
}
