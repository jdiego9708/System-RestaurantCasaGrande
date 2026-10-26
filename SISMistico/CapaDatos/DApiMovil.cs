using System;
using System.Data;
using System.Data.SqlClient;

namespace CapaDatos
{
    /// <summary>
    /// Acceso a datos de la API móvil. Todo parametrizado y de solo lectura en esta fase.
    /// No usa sp_Login: ese SP cierra y abre turnos (docs/02 §5.18) y la API no debe tocarlos.
    /// </summary>
    public class DApiMovil
    {
        /// <summary>SELECT 1 con timeout corto, para el /ping.</summary>
        public bool ProbarConexion()
        {
            try
            {
                using (SqlConnection cn = new SqlConnection(Conexion.Cn))
                using (SqlCommand cmd = new SqlCommand("SELECT 1", cn) { CommandTimeout = 3 })
                {
                    cn.Open();
                    return Convert.ToInt32(cmd.ExecuteScalar()) == 1;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Busca al empleado por id y contraseña (<paramref name="usarPin"/> = false) o por id y PIN.
        /// Devuelve una fila (sin Password) o una tabla vacía si las credenciales no coinciden.
        /// La comparación usa el collation de la BD, igual que sp_Login en el escritorio.
        /// </summary>
        public DataTable ValidarEmpleado(int idEmpleado, string password, int pin, bool usarPin)
        {
            const string sqlPassword =
                "SELECT Id_empleado, Nombre_empleado, Cargo_empleado, Estado, Codigo_maestro " +
                "FROM Empleados WHERE Id_empleado = @Id AND Password = @Password";
            const string sqlPin =
                "SELECT Id_empleado, Nombre_empleado, Cargo_empleado, Estado, Codigo_maestro " +
                "FROM Empleados WHERE Id_empleado = @Id AND Codigo_maestro = @Pin";

            DataTable tabla = new DataTable("Empleado");
            using (SqlConnection cn = new SqlConnection(Conexion.Cn))
            using (SqlCommand cmd = new SqlCommand(usarPin ? sqlPin : sqlPassword, cn))
            {
                cmd.Parameters.Add("@Id", SqlDbType.Int).Value = idEmpleado;
                if (usarPin)
                    cmd.Parameters.Add("@Pin", SqlDbType.Int).Value = pin;
                else
                    cmd.Parameters.Add("@Password", SqlDbType.VarChar, 50).Value = password ?? string.Empty;

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    da.Fill(tabla);
            }
            return tabla;
        }

        // ------------------------------------------------------------------------------------------
        // Catálogo y mesas (fase 2): todo parametrizado y de solo lectura.
        // ------------------------------------------------------------------------------------------

        public DataTable TiposPlatos()
        {
            return Consultar("SELECT Id_tipo_plato, Tipo_plato FROM Tipo_platos ORDER BY Tipo_plato", null);
        }

        public DataTable TiposBebidas()
        {
            return Consultar("SELECT Id_tipo_bebida, Tipo_bebida FROM Tipo_bebidas ORDER BY Tipo_bebida", null);
        }

        /// <summary>Platos ACTIVO (el mismo filtro que usa el escritorio), con tipo, opcionalmente por tipo, id o texto.</summary>
        public DataTable Platos(int? idTipo, int? idPlato, string texto)
        {
            const string sql =
                "SELECT p.Id_plato, p.Nombre_plato, p.Precio_plato, p.Id_tipo_plato, t.Tipo_plato, " +
                "p.Descripcion_plato, p.Imagen_plato, p.Plato_detallado, p.Plato_carta " +
                "FROM Platos p INNER JOIN Tipo_platos t ON t.Id_tipo_plato = p.Id_tipo_plato " +
                "WHERE p.Estado = 'ACTIVO' " +
                "AND (@Tipo IS NULL OR p.Id_tipo_plato = @Tipo) " +
                "AND (@Id IS NULL OR p.Id_plato = @Id) " +
                "AND (@Q IS NULL OR p.Nombre_plato LIKE @Q ESCAPE '\\') " +
                "ORDER BY t.Tipo_plato, p.Nombre_plato";
            return Consultar(sql, cmd =>
            {
                cmd.Parameters.Add("@Tipo", SqlDbType.Int).Value = (object)idTipo ?? DBNull.Value;
                cmd.Parameters.Add("@Id", SqlDbType.Int).Value = (object)idPlato ?? DBNull.Value;
                cmd.Parameters.Add("@Q", SqlDbType.VarChar, 120).Value = PatronLike(texto);
            });
        }

        public DataTable Bebidas(int? idTipo, string texto)
        {
            const string sql =
                "SELECT b.Id_bebida, b.Nombre_bebida, b.Precio_bebida, b.Id_tipo_bebida, t.Tipo_bebida, " +
                "b.Descripcion_bebida, b.Imagen " +
                "FROM Bebidas b INNER JOIN Tipo_bebidas t ON t.Id_tipo_bebida = b.Id_tipo_bebida " +
                "WHERE b.Estado = 'ACTIVO' " +
                "AND (@Tipo IS NULL OR b.Id_tipo_bebida = @Tipo) " +
                "AND (@Q IS NULL OR b.Nombre_bebida LIKE @Q ESCAPE '\\') " +
                "ORDER BY t.Tipo_bebida, b.Nombre_bebida";
            return Consultar(sql, cmd =>
            {
                cmd.Parameters.Add("@Tipo", SqlDbType.Int).Value = (object)idTipo ?? DBNull.Value;
                cmd.Parameters.Add("@Q", SqlDbType.VarChar, 120).Value = PatronLike(texto);
            });
        }

        /// <summary>Todos los ingredientes (como el escritorio, que no filtra por estado), opcionalmente por tipo.</summary>
        public DataTable Ingredientes(string tipo)
        {
            const string sql =
                "SELECT Id_ingrediente, Tipo_ingrediente, Nombre_ingrediente, Descripcion_ingrediente, Estado_ingrediente " +
                "FROM Ingredientes WHERE (@Tipo IS NULL OR Tipo_ingrediente = @Tipo) " +
                "ORDER BY Tipo_ingrediente, Id_ingrediente";
            return Consultar(sql, cmd =>
                cmd.Parameters.Add("@Tipo", SqlDbType.VarChar, 50).Value = (object)tipo ?? DBNull.Value);
        }

        /// <summary>Clientes activos (sin el comodín 0 "NINGUNO"), máximo 50, por nombre o teléfono.</summary>
        public DataTable Clientes(string texto)
        {
            const string sql =
                "SELECT TOP 50 Id_cliente, Nombre_cliente, Telefono_cliente FROM Clientes " +
                "WHERE Id_cliente > 0 AND Estado_cliente = 'ACTIVO' " +
                "AND (@Q IS NULL OR Nombre_cliente LIKE @Q ESCAPE '\\' OR Telefono_cliente LIKE @Q ESCAPE '\\') " +
                "ORDER BY Nombre_cliente";
            return Consultar(sql, cmd =>
                cmd.Parameters.Add("@Q", SqlDbType.VarChar, 120).Value = PatronLike(texto));
        }

        /// <summary>
        /// Una fila por Num_mesa (si hay números repetidos manda el Id_mesa menor, como el escritorio, que
        /// busca por Num_mesa) con su pedido PENDIENTE/SALIENDO más reciente de la fecha, si lo hay.
        /// </summary>
        public DataTable Mesas(DateTime fecha)
        {
            const string sql =
                "WITH m AS (SELECT Id_mesa, Num_mesa, Descripcion_mesa, " +
                "  ROW_NUMBER() OVER (PARTITION BY Num_mesa ORDER BY Id_mesa) AS rn FROM Mesas WHERE Id_mesa > 0), " +
                "ab AS (SELECT mm.Num_mesa, p.Id_pedido, p.Estado_pedido, p.Hora_pedido, p.CantidadClientes, " +
                "  ROW_NUMBER() OVER (PARTITION BY mm.Num_mesa ORDER BY p.Id_pedido DESC) AS rn " +
                "  FROM Pedidos p INNER JOIN Mesas mm ON mm.Id_mesa = p.Id_mesa " +
                "  WHERE p.Fecha_pedido = @Fecha AND p.Estado_pedido IN ('PENDIENTE', 'SALIENDO') " +
                "  AND ISNULL(p.Tipo_pedido, '') <> 'DOMICILIO') " +
                "SELECT m.Id_mesa, m.Num_mesa, m.Descripcion_mesa, " +
                "  ISNULL(ab.Estado_pedido, 'DISPONIBLE') AS Estado, ISNULL(ab.Id_pedido, 0) AS Id_pedido, " +
                "  ab.Hora_pedido, ISNULL(ab.CantidadClientes, 0) AS CantidadClientes, " +
                "  ISNULL((SELECT SUM(d.Precio * d.Cantidad) FROM Detalle_pedido d WHERE d.Id_pedido = ab.Id_pedido), 0) AS Total " +
                "FROM m LEFT JOIN ab ON ab.Num_mesa = m.Num_mesa AND ab.rn = 1 " +
                "WHERE m.rn = 1 ORDER BY m.Num_mesa";
            return Consultar(sql, cmd =>
                cmd.Parameters.Add("@Fecha", SqlDbType.Date).Value = fecha.Date);
        }

        // ------------------------------------------------------------------------------------------
        // Alta de pedidos (fase 3)
        // ------------------------------------------------------------------------------------------

        /// <summary>La mesa por Id_mesa, con Canonica = 1 si es la de menor Id_mesa de su Num_mesa (la que usa el escritorio).</summary>
        public DataTable MesaPorId(int idMesa)
        {
            const string sql =
                "SELECT m.Id_mesa, m.Num_mesa, " +
                "CASE WHEN m.Id_mesa = (SELECT MIN(x.Id_mesa) FROM Mesas x WHERE x.Num_mesa = m.Num_mesa) THEN 1 ELSE 0 END AS Canonica " +
                "FROM Mesas m WHERE m.Id_mesa = @Id AND m.Id_mesa > 0";
            return Consultar(sql, cmd => cmd.Parameters.Add("@Id", SqlDbType.Int).Value = idMesa);
        }

        /// <summary>Pedido PENDIENTE/SALIENDO más reciente de esa fecha para el Num_mesa (cualquiera de sus Id_mesa).</summary>
        public DataTable PedidoAbiertoDeMesa(int numMesa, DateTime fecha)
        {
            const string sql =
                "SELECT TOP 1 p.Id_pedido, p.Estado_pedido FROM Pedidos p " +
                "INNER JOIN Mesas m ON m.Id_mesa = p.Id_mesa " +
                "WHERE m.Num_mesa = @Num AND p.Fecha_pedido = @Fecha " +
                "AND p.Estado_pedido IN ('PENDIENTE', 'SALIENDO') AND ISNULL(p.Tipo_pedido, '') <> 'DOMICILIO' " +
                "ORDER BY p.Id_pedido DESC";
            return Consultar(sql, cmd =>
            {
                cmd.Parameters.Add("@Num", SqlDbType.Int).Value = numMesa;
                cmd.Parameters.Add("@Fecha", SqlDbType.Date).Value = fecha.Date;
            });
        }

        public bool ClienteActivo(int idCliente)
        {
            DataTable t = Consultar(
                "SELECT COUNT(*) FROM Clientes WHERE Id_cliente = @Id AND Estado_cliente = 'ACTIVO'",
                cmd => cmd.Parameters.Add("@Id", SqlDbType.Int).Value = idCliente);
            return Convert.ToInt32(t.Rows[0][0]) > 0;
        }

        /// <summary>Estado y si es detallado, de los productos cuyos ids van en un JSON array ("[1,2,3]"). Los inexistentes no salen.</summary>
        public DataTable Productos(bool plato, string idsJson)
        {
            string sql = plato
                ? "SELECT p.Id_plato AS Id, p.Estado, p.Plato_detallado AS Detallado " +
                  "FROM OPENJSON(@Ids) WITH (id int '$') j INNER JOIN Platos p ON p.Id_plato = j.id"
                : "SELECT b.Id_bebida AS Id, b.Estado, 'INACTIVO' AS Detallado " +
                  "FROM OPENJSON(@Ids) WITH (id int '$') j INNER JOIN Bebidas b ON b.Id_bebida = j.id";
            return Consultar(sql, cmd => cmd.Parameters.Add("@Ids", SqlDbType.NVarChar, -1).Value = idsJson);
        }

        public DataTable IngredientesPorIds(string idsJson)
        {
            const string sql =
                "SELECT g.Id_ingrediente, g.Tipo_ingrediente, g.Estado_ingrediente " +
                "FROM OPENJSON(@Ids) WITH (id int '$') j INNER JOIN Ingredientes g ON g.Id_ingrediente = j.id";
            return Consultar(sql, cmd => cmd.Parameters.Add("@Ids", SqlDbType.NVarChar, -1).Value = idsJson);
        }

        /// <summary>
        /// Alta atómica con sp_Movil_Insertar_pedido (db/01_sp_Movil_Insertar_pedido.sql). Devuelve el Id_pedido.
        /// Si el SP falla lanza SqlException y no queda nada escrito.
        /// </summary>
        public int InsertarPedidoMovil(int idMesa, int idEmpleado, int idCliente, string tipoPedido,
                                       string observaciones, int cantidadClientes, string itemsJson)
        {
            using (SqlConnection cn = new SqlConnection(Conexion.Cn))
            using (SqlCommand cmd = new SqlCommand("sp_Movil_Insertar_pedido", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 30;
                cmd.Parameters.Add("@Id_mesa", SqlDbType.Int).Value = idMesa;
                cmd.Parameters.Add("@Id_empleado", SqlDbType.Int).Value = idEmpleado;
                cmd.Parameters.Add("@Id_cliente", SqlDbType.Int).Value = idCliente;
                cmd.Parameters.Add("@Tipo_pedido", SqlDbType.VarChar, 50).Value = tipoPedido;
                cmd.Parameters.Add("@Observaciones", SqlDbType.VarChar, 200).Value = observaciones ?? string.Empty;
                cmd.Parameters.Add("@CantidadClientes", SqlDbType.Int).Value = cantidadClientes;
                cmd.Parameters.Add("@Items", SqlDbType.NVarChar, -1).Value = itemsJson;
                SqlParameter salida = cmd.Parameters.Add("@Id_pedido", SqlDbType.Int);
                salida.Direction = ParameterDirection.Output;

                cn.Open();
                cmd.ExecuteNonQuery();
                return Convert.ToInt32(salida.Value);
            }
        }

        /// <summary>Cabecera (tabla 0), líneas (1) e ingredientes (2) de un pedido.</summary>
        public DataSet PedidoCompleto(int idPedido)
        {
            const string sql =
                "SELECT p.Id_pedido, p.Id_mesa, m.Num_mesa, p.Id_cliente, c.Nombre_cliente, p.Id_empleado, e.Nombre_empleado, " +
                "p.Estado_pedido, p.Fecha_pedido, p.Hora_pedido, p.Tipo_pedido, p.Observaciones_pedido, p.CantidadClientes " +
                "FROM Pedidos p LEFT JOIN Mesas m ON m.Id_mesa = p.Id_mesa LEFT JOIN Clientes c ON c.Id_cliente = p.Id_cliente " +
                "LEFT JOIN Empleados e ON e.Id_empleado = p.Id_empleado WHERE p.Id_pedido = @Id; " +
                "SELECT d.Id_detalle_pedido, d.Tipo, d.Id_tipo, COALESCE(pl.Nombre_plato, b.Nombre_bebida) AS Nombre, " +
                "d.Precio, d.Cantidad, d.Observaciones FROM Detalle_pedido d " +
                "LEFT JOIN Platos pl ON d.Tipo = 'PLATO' AND pl.Id_plato = d.Id_tipo " +
                "LEFT JOIN Bebidas b ON d.Tipo = 'BEBIDA' AND b.Id_bebida = d.Id_tipo " +
                "WHERE d.Id_pedido = @Id ORDER BY d.Id_detalle_pedido; " +
                "SELECT i.Id_detalle_pedido, i.Id_tipo, i.Id_ingrediente, g.Tipo_ingrediente, g.Nombre_ingrediente, i.Observaciones " +
                "FROM Detalle_ingredientes_pedido i INNER JOIN Ingredientes g ON g.Id_ingrediente = i.Id_ingrediente " +
                "WHERE i.Id_pedido = @Id ORDER BY i.Id_detalle_ingrediente_pedido";
            DataSet ds = new DataSet();
            using (SqlConnection cn = new SqlConnection(Conexion.Cn))
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@Id", SqlDbType.Int).Value = idPedido;
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    da.Fill(ds);
            }
            return ds;
        }

        /// <summary>
        /// Empleados reales (Id > 0, el comodín 0 "NINGUNO" nunca) cuyo PIN coincide. Sin contraseña.
        /// El PIN 0 no cuenta como PIN: es "sin PIN" escrito como número (se normaliza en db/02).
        /// </summary>
        public DataTable EmpleadosPorPin(int pin)
        {
            const string sql =
                "SELECT Id_empleado, Nombre_empleado, Cargo_empleado, Estado, Codigo_maestro " +
                "FROM Empleados WHERE Id_empleado > 0 AND Codigo_maestro > 0 AND Codigo_maestro = @Pin " +
                "ORDER BY Id_empleado";
            return Consultar(sql, cmd => cmd.Parameters.Add("@Pin", SqlDbType.Int).Value = pin);
        }

        /// <summary>Último turno existente (solo lectura). Tabla vacía si no hay ninguno.</summary>
        public DataTable UltimoTurno()
        {
            DataTable tabla = new DataTable("Turno");
            using (SqlConnection cn = new SqlConnection(Conexion.Cn))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT TOP 1 Id_turno, Fecha_turno, Estado_turno FROM Turnos ORDER BY Id_turno DESC", cn))
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                da.Fill(tabla);
            }
            return tabla;
        }

        private static DataTable Consultar(string sql, Action<SqlCommand> parametros)
        {
            DataTable tabla = new DataTable();
            using (SqlConnection cn = new SqlConnection(Conexion.Cn))
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                if (parametros != null)
                    parametros(cmd);
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    da.Fill(tabla);
            }
            return tabla;
        }

        /// <summary>"%texto%" con los comodines de LIKE escapados (ESCAPE '\'); DBNull si no hay texto.</summary>
        private static object PatronLike(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return DBNull.Value;
            string t = texto.Trim().Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_").Replace("[", "\\[");
            return "%" + t + "%";
        }
    }
}
