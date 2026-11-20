using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using System.Data.SqlClient;
using CapaEntidades.Models;

namespace CapaDatos
{
    public class DPedidos
    {
        /// <summary>
        /// Ingredientes elegidos en cada plato del pedido (sopa, crema, arroz, acompanantes...), en el orden en que se
        /// eligieron: Id_detalle_pedido y Nombre_ingrediente. Son los mismos que salen impresos en las comandas.
        /// </summary>
        public static DataTable GetIngredientLines(int orderId)
        {
            const string query = @"
SELECT i.Id_detalle_pedido, g.Nombre_ingrediente
FROM Detalle_ingredientes_pedido i
INNER JOIN Ingredientes g ON g.Id_ingrediente = i.Id_ingrediente
WHERE i.Id_pedido = @OrderId
ORDER BY i.Id_detalle_ingrediente_pedido";
            using (SqlConnection con = new SqlConnection(Conexion.Cn))
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.Add("@OrderId", SqlDbType.Int).Value = orderId;
                DataTable table = new DataTable("IngredientLines");
                new SqlDataAdapter(cmd).Fill(table);
                return table;
            }
        }

        /// <summary>
        /// Numero de factura del dia del domicilio (1..N, empieza de nuevo cada dia). Si todavia no lo tiene lo asigna: el
        /// siguiente del dia del pedido, bajo bloqueo para que dos equipos no repitan numero. Null si el pedido no es un
        /// domicilio o si la base aun no tiene la numeracion (script 11): entonces la factura sale sin numero.
        /// </summary>
        public static int? GetOrAssignDeliveryNumber(int orderId)
        {
            const string query = @"
DECLARE @n int = (SELECT DeliveryNumber FROM Pedidos WHERE Id_pedido = @OrderId AND Tipo_pedido = 'DOMICILIO');
IF @n IS NULL AND EXISTS (SELECT 1 FROM Pedidos WHERE Id_pedido = @OrderId AND Tipo_pedido = 'DOMICILIO')
BEGIN
    SET XACT_ABORT ON;
    BEGIN TRAN;
    UPDATE p SET DeliveryNumber = ISNULL((SELECT MAX(x.DeliveryNumber) FROM Pedidos x WITH (UPDLOCK, HOLDLOCK)
                                          WHERE x.Tipo_pedido = 'DOMICILIO' AND x.Fecha_pedido = p.Fecha_pedido), 0) + 1
    FROM Pedidos p WHERE p.Id_pedido = @OrderId AND p.DeliveryNumber IS NULL;
    COMMIT;
    SET @n = (SELECT DeliveryNumber FROM Pedidos WHERE Id_pedido = @OrderId);
END
SELECT @n;";
            for (int attempt = 0; ; attempt++)
            {
                try
                {
                    using (SqlConnection con = new SqlConnection(Conexion.Cn))
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.Add("@OrderId", SqlDbType.Int).Value = orderId;
                        con.Open();
                        object value = cmd.ExecuteScalar();
                        return value == null || value == DBNull.Value ? (int?)null : Convert.ToInt32(value);
                    }
                }
                catch (SqlException ex) when (ex.Number == 207)
                {
                    return null;
                }
                catch (SqlException ex) when ((ex.Number == 2627 || ex.Number == 2601) && attempt < 3)
                {
                    // Otro equipo tomo ese numero justo ahora: se reintenta con el siguiente.
                }
            }
        }

        /// <summary>Guarda la forma de pago elegida al tomar un domicilio. Devuelve "OK" o el motivo del error.</summary>
        public static string SetPaymentMethod(int orderId, string method)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(Conexion.Cn))
                using (SqlCommand cmd = new SqlCommand("UPDATE Pedidos SET Payment_method = @Method WHERE Id_pedido = @OrderId", con))
                {
                    cmd.Parameters.Add("@Method", SqlDbType.VarChar, 20).Value = method;
                    cmd.Parameters.Add("@OrderId", SqlDbType.Int).Value = orderId;
                    con.Open();
                    cmd.ExecuteNonQuery();
                    return "OK";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// Forma de pago guardada del pedido; cadena vacia si no tiene o si la columna aun no existe en la base
        /// (script 09 sin aplicar): en ese caso el domicilio no se factura solo y se factura a mano.
        /// </summary>
        public static string GetPaymentMethod(int orderId)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(Conexion.Cn))
                using (SqlCommand cmd = new SqlCommand("SELECT Payment_method FROM Pedidos WHERE Id_pedido = @OrderId", con))
                {
                    cmd.Parameters.Add("@OrderId", SqlDbType.Int).Value = orderId;
                    con.Open();
                    object value = cmd.ExecuteScalar();
                    return value == null || value == DBNull.Value ? string.Empty : Convert.ToString(value).Trim();
                }
            }
            catch (SqlException)
            {
                return string.Empty;
            }
        }

        #region MENSAJE SQL
        private void SqlCon_InfoMessage(object sender, SqlInfoMessageEventArgs e)
        {
            this.Mensaje_respuesta = e.Message;
        }
        #endregion

        #region VARIABLES
        private string mensaje_respuesta;
        public string Mensaje_respuesta
        {
            get
            {
                return mensaje_respuesta;
            }

            set
            {
                mensaje_respuesta = value;
            }
        }

        public string[] Variables
        {
            get
            {
                return _variables;
            }

            set
            {
                _variables = value;
            }
        }

        private string[] _variables;
        #endregion

        #region CONSTRUCTOR VACIO
        public DPedidos() { }
        #endregion

        #region METODO INSERTAR PEDIDO
        public string InsertarPedido(Pedidos pedido)
        {
            int contador = 0;
            //asignamos a una cadena string la variable rpta y la iniciamos en vacía
            string rpta = "";
            SqlConnection SqlCon = new SqlConnection();
            SqlCon.InfoMessage += new SqlInfoMessageEventHandler(SqlCon_InfoMessage);
            SqlCon.FireInfoMessageEventOnUserErrors = true;
            //Capturador de errores
            try
            {
                SqlCon.ConnectionString = Conexion.Cn;
                SqlCon.Open();
                //establecer comando
                SqlCommand SqlCmd = new SqlCommand
                {
                    Connection = SqlCon,
                    CommandText = "sp_Insertar_pedido",
                    //Indicamos que es un procedimiento almacenado
                    CommandType = CommandType.StoredProcedure
                };

                SqlParameter Id_pedido = new SqlParameter
                {
                    ParameterName = "@Id_pedido",
                    SqlDbType = SqlDbType.Int,
                    Direction = ParameterDirection.Output
                };
                SqlCmd.Parameters.Add(Id_pedido);

                SqlParameter Id_mesa = new SqlParameter
                {
                    ParameterName = "@Id_mesa",
                    SqlDbType = SqlDbType.Int,
                    Value = pedido.Id_mesa,
                };
                SqlCmd.Parameters.Add(Id_mesa);
                contador += 1;

                SqlParameter Id_empleado = new SqlParameter
                {
                    ParameterName = "@Id_empleado",
                    SqlDbType = SqlDbType.Int,
                    Value = pedido.Id_empleado,
                };
                SqlCmd.Parameters.Add(Id_empleado);
                contador += 1;

                SqlParameter Id_cliente = new SqlParameter
                {
                    ParameterName = "@Id_cliente",
                    SqlDbType = SqlDbType.Int,
                    Value = pedido.Id_cliente,
                };
                SqlCmd.Parameters.Add(Id_cliente);
                contador += 1;

                SqlParameter Fecha_pedido = new SqlParameter
                {
                    ParameterName = "@Fecha_pedido",
                    SqlDbType = SqlDbType.Date,
                    Value = pedido.Fecha_pedido,
                };
                SqlCmd.Parameters.Add(Fecha_pedido);

                SqlParameter Hora_pedido = new SqlParameter
                {
                    ParameterName = "@Hora_pedido",
                    SqlDbType = SqlDbType.Time,
                    Value = pedido.Hora_pedido,
                };
                SqlCmd.Parameters.Add(Hora_pedido);

                SqlParameter Tipo_pedido = new SqlParameter
                {
                    ParameterName = "@Tipo_pedido",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 50,
                    Value = pedido.Tipo_pedido
                };
                SqlCmd.Parameters.Add(Tipo_pedido);
                contador += 1;

                SqlParameter Observaciones_pedido = new SqlParameter
                {
                    ParameterName = "@Observaciones_pedido",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 200,
                    Value = pedido.Observaciones_pedido
                };
                SqlCmd.Parameters.Add(Observaciones_pedido);
                contador += 1;

                SqlParameter CantidadClientes = new SqlParameter
                {
                    ParameterName = "@CantidadClientes",
                    SqlDbType = SqlDbType.Int,
                    Value = pedido.CantidadClientes
                };
                SqlCmd.Parameters.Add(CantidadClientes);
                contador += 1;

                // Solo los domicilios llevan direccion; sin el parametro el SP usa NULL (pedido de mesa).
                if (pedido.AddressId.HasValue)
                    SqlCmd.Parameters.Add("@Id_direccion", SqlDbType.Int).Value = pedido.AddressId.Value;

                //SqlParameter Detalles_pedido = new SqlParameter("@Detalle_pedido", TablaDetalle);
                //Detalles_pedido.ParameterName = "@Detalle_pedido";
                //Detalles_pedido.Value = TablaDetalle;
                //SqlCmd.Parameters.Add(Detalles_pedido);

                //Ejecutamos nuestro comando              
                //Se puede ejecutar este metodo pero ya tenemos el mensaje que devuelve sql
                //rpta = SqlCmd.ExecuteNonQuery() >= 1 ? "OK" : "NO se Ingreso el Registro";

                DataSet ds = new DataSet();
                SqlDataAdapter SqlData = new SqlDataAdapter(SqlCmd);
                SqlData.Fill(ds);

                if (ds.Tables.Count > 0)
                {
                    if (ds.Tables[0].Rows[0]["Respuesta"].Equals("OK"))
                    {
                        pedido.Id_pedido = Convert.ToInt32(ds.Tables[0].Rows[0]["Id_pedido"]);
                        rpta = "OK";
                    }
                }
                else
                {
                    rpta = "No se encontraron las tablas correspondientes a la respuesta";
                }
            }
            //Mostramos posible error que tengamos
            catch (SqlException ex)
            {
                rpta = ex.Message;
            }
            catch (Exception ex)
            {
                rpta = ex.Message;
            }
            finally
            {
                //Si la cadena SqlCon esta abierta la cerramos
                if (SqlCon.State == ConnectionState.Open)
                    SqlCon.Close();
            }
            return rpta;
        }
        #endregion

        #region METODO INSERTAR DETALLE PEDIDO
        public string InsertarDetallePedido(Detalle_pedido detalle)
        {
            //asignamos a una cadena string la variable rpta y la iniciamos en vacía
            string rpta = "";
            SqlConnection SqlCon = new SqlConnection();
            SqlCon.InfoMessage += new SqlInfoMessageEventHandler(SqlCon_InfoMessage);
            SqlCon.FireInfoMessageEventOnUserErrors = true;
            //Capturador de errores
            try
            {
                SqlCon.ConnectionString = Conexion.Cn;
                SqlCon.Open();

                SqlCommand SqlCmd = new SqlCommand
                {
                    Connection = SqlCon,
                    CommandText = "sp_Insertar_detalle_pedido",
                    CommandType = CommandType.StoredProcedure
                };

                SqlParameter Id_detalle_pedido = new SqlParameter
                {
                    ParameterName = "@Id_detalle_pedido",
                    SqlDbType = SqlDbType.Int,
                    Direction = ParameterDirection.Output
                };
                SqlCmd.Parameters.Add(Id_detalle_pedido);

                SqlParameter Id_pedido = new SqlParameter
                {
                    ParameterName = "@Id_pedido",
                    SqlDbType = SqlDbType.Int,
                    Value = detalle.Id_pedido,
                };
                SqlCmd.Parameters.Add(Id_pedido);

                SqlParameter Id_tipo = new SqlParameter
                {
                    ParameterName = "@Id_tipo",
                    SqlDbType = SqlDbType.Int,
                    Value = detalle.Id_tipo,
                };
                SqlCmd.Parameters.Add(Id_tipo);

                SqlParameter Tipo = new SqlParameter
                {
                    ParameterName = "@Tipo",
                    SqlDbType = SqlDbType.VarChar,
                    Value = detalle.Tipo,
                };
                SqlCmd.Parameters.Add(Tipo);

                SqlParameter Precio = new SqlParameter
                {
                    ParameterName = "@Precio",
                    SqlDbType = SqlDbType.Decimal,
                    Value = detalle.Precio,
                };
                SqlCmd.Parameters.Add(Precio);

                SqlParameter Cantidad = new SqlParameter
                {
                    ParameterName = "@Cantidad",
                    SqlDbType = SqlDbType.Int,
                    Value = detalle.Cantidad,
                };
                SqlCmd.Parameters.Add(Cantidad);

                SqlParameter Observaciones = new SqlParameter
                {
                    ParameterName = "@Observaciones",
                    SqlDbType = SqlDbType.VarChar,
                    Value = detalle.Observaciones,
                };
                SqlCmd.Parameters.Add(Observaciones);

                //Ejecutamos nuestro comando
                //Se puede ejecutar este metodo pero ya tenemos el mensaje que devuelve sql
                rpta = SqlCmd.ExecuteNonQuery() >= 1 ? "OK" : "NO se Ingreso el Registro";

                if (rpta != "OK")
                {
                    if (this.Mensaje_respuesta != null)
                    {
                        rpta = this.Mensaje_respuesta;
                    }
                }
                else
                {
                    detalle.Id_detalle_pedido = Convert.ToInt32(SqlCmd.Parameters["@Id_detalle_pedido"].Value);
                }
            }
            //Mostramos posible error que tengamos
            catch (SqlException ex)
            {
                rpta = ex.Message;
            }
            catch (Exception ex)
            {
                rpta = ex.Message;
            }
            finally
            {
                //Si la cadena SqlCon esta abierta la cerramos
                if (SqlCon.State == ConnectionState.Open)
                    SqlCon.Close();
            }
            return rpta;
        }
        #endregion

        #region METODO CANCELAR PEDIDO
        public string CancelarPedido(int id_pedido, string observaciones)
        {
            //asignamos a una cadena string la variable rpta y la iniciamos en vacía
            string rpta = "";
            SqlConnection SqlCon = new SqlConnection();
            SqlCon.InfoMessage += new SqlInfoMessageEventHandler(SqlCon_InfoMessage);
            SqlCon.FireInfoMessageEventOnUserErrors = true;
            //Capturador de errores
            try
            {
                SqlCon.ConnectionString = Conexion.Cn;
                SqlCon.Open();
                //establecer comando
                SqlCommand SqlCmd = new SqlCommand
                {
                    Connection = SqlCon,
                    CommandText = "sp_Cancelar_pedido",
                    //Indicamos que es un procedimiento almacenado
                    CommandType = CommandType.StoredProcedure
                };

                SqlParameter Id_pedido = new SqlParameter
                {
                    ParameterName = "@Id_pedido",
                    SqlDbType = SqlDbType.Int,
                    Value = id_pedido,
                };
                SqlCmd.Parameters.Add(Id_pedido);

                SqlParameter Observaciones = new SqlParameter
                {
                    ParameterName = "@Observaciones",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 200,
                    Value = observaciones,
                };
                SqlCmd.Parameters.Add(Observaciones);

                //Ejecutamos nuestro comando
                //Se puede ejecutar este metodo pero ya tenemos el mensaje que devuelve sql
                rpta = SqlCmd.ExecuteNonQuery() >= 1 ? "OK" : "NO se Ingreso el Registro";

                if (rpta != "OK")
                {
                    if (this.Mensaje_respuesta != null)
                    {
                        rpta = this.Mensaje_respuesta;
                    }
                }
            }
            //Mostramos posible error que tengamos
            catch (SqlException ex)
            {
                rpta = ex.Message;
            }
            catch (Exception ex)
            {
                rpta = ex.Message;
            }
            finally
            {
                //Si la cadena SqlCon esta abierta la cerramos
                if (SqlCon.State == ConnectionState.Open)
                    SqlCon.Close();
            }
            return rpta;
        }
        #endregion

        #region METODO CAMBIAR ESTADO PEDIDO
        public string CambiarEstadoPedido(List<string> Variables)
        {
            int contador = 0;
            //asignamos a una cadena string la variable rpta y la iniciamos en vacía
            string rpta = "";
            SqlConnection SqlCon = new SqlConnection();
            SqlCon.InfoMessage += new SqlInfoMessageEventHandler(SqlCon_InfoMessage);
            SqlCon.FireInfoMessageEventOnUserErrors = true;
            //Capturador de errores
            try
            {
                SqlCon.ConnectionString = Conexion.Cn;
                SqlCon.Open();
                //establecer comando
                SqlCommand SqlCmd = new SqlCommand
                {
                    Connection = SqlCon,
                    CommandText = "sp_Cambiar_estado_pedido",
                    //Indicamos que es un procedimiento almacenado
                    CommandType = CommandType.StoredProcedure
                };

                SqlParameter Id_pedido = new SqlParameter
                {
                    ParameterName = "@Id_pedido",
                    SqlDbType = SqlDbType.Int,
                    Value = Convert.ToInt32(Variables[contador])
                };
                SqlCmd.Parameters.Add(Id_pedido);
                contador += 1;

                SqlParameter Estado = new SqlParameter
                {
                    ParameterName = "@Estado",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 50,
                    Value = Variables[contador].Trim().ToUpper()
                };
                SqlCmd.Parameters.Add(Estado);
                contador += 1;

                SqlParameter Id_mesa = new SqlParameter
                {
                    ParameterName = "@Id_mesa",
                    SqlDbType = SqlDbType.Int,
                    Value = Convert.ToInt32(Variables[contador])
                };
                SqlCmd.Parameters.Add(Id_mesa);
                contador += 1;

                //Ejecutamos nuestro comando
                //Se puede ejecutar este metodo pero ya tenemos el mensaje que devuelve sql
                rpta = SqlCmd.ExecuteNonQuery() >= 1 ? "OK" : "NO se Ingreso el Registro";

                if (rpta != "OK")
                {
                    if (this.Mensaje_respuesta != null)
                    {
                        rpta = this.Mensaje_respuesta;
                    }
                }
            }
            //Mostramos posible error que tengamos
            catch (SqlException ex)
            {
                rpta = ex.Message;
            }
            catch (Exception ex)
            {
                rpta = ex.Message;
            }
            finally
            {
                //Si la cadena SqlCon esta abierta la cerramos
                if (SqlCon.State == ConnectionState.Open)
                    SqlCon.Close();
            }
            return rpta;
        }
        #endregion

        #region METODO ACTUALIZAR DETALLE PEDIDO
        public string ActualizarDetallePedido(Detalle_pedido detalle, int id_empleado, string tipo_update)
        {
            int contador = 0;
            //asignamos a una cadena string la variable rpta y la iniciamos en vacía
            string rpta = "";
            SqlConnection SqlCon = new SqlConnection();
            SqlCon.InfoMessage += new SqlInfoMessageEventHandler(SqlCon_InfoMessage);
            SqlCon.FireInfoMessageEventOnUserErrors = true;
            //Capturador de errores
            try
            {
                SqlCon.ConnectionString = Conexion.Cn;
                SqlCon.Open();
                //establecer comando
                SqlCommand SqlCmd = new SqlCommand
                {
                    Connection = SqlCon,
                    CommandText = "sp_Actualizar_detalle_pedido",
                    //Indicamos que es un procedimiento almacenado
                    CommandType = CommandType.StoredProcedure
                };

                SqlParameter Id_detalle_pedidoSalida = new SqlParameter
                {
                    ParameterName = "@Id_detalle_pedidoSalida",
                    SqlDbType = SqlDbType.Int,
                    Direction = ParameterDirection.Output,
                };
                SqlCmd.Parameters.Add(Id_detalle_pedidoSalida);

                SqlParameter Id_detalle_pedido = new SqlParameter
                {
                    ParameterName = "@Id_detalle_pedido",
                    SqlDbType = SqlDbType.Int,
                    Value = detalle.Id_detalle_pedido,
                };
                SqlCmd.Parameters.Add(Id_detalle_pedido);

                SqlParameter Id_pedido = new SqlParameter
                {
                    ParameterName = "@Id_pedido",
                    SqlDbType = SqlDbType.Int,
                    Value = detalle.Id_pedido,
                };
                SqlCmd.Parameters.Add(Id_pedido);
                contador += 1;

                SqlParameter Id_tipo = new SqlParameter
                {
                    ParameterName = "@Id_tipo",
                    SqlDbType = SqlDbType.Int,
                    Value = detalle.Id_tipo,
                };
                SqlCmd.Parameters.Add(Id_tipo);
                contador += 1;

                SqlParameter Tipo = new SqlParameter
                {
                    ParameterName = "@Tipo",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 50,
                    Value = detalle.Tipo,
                };
                SqlCmd.Parameters.Add(Tipo);
                contador += 1;

                SqlParameter Precio = new SqlParameter
                {
                    ParameterName = "@Precio",
                    SqlDbType = SqlDbType.Int,
                    Value = detalle.Precio,
                };
                SqlCmd.Parameters.Add(Precio);
                contador += 1;

                SqlParameter Cantidad = new SqlParameter
                {
                    ParameterName = "@Cantidad",
                    SqlDbType = SqlDbType.Int,
                    Value = detalle.Cantidad,
                };
                SqlCmd.Parameters.Add(Cantidad);
                contador += 1;

                SqlParameter Observaciones = new SqlParameter
                {
                    ParameterName = "@Observaciones",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 500,
                    Value = detalle.Observaciones,
                };
                SqlCmd.Parameters.Add(Observaciones);
                contador += 1;

                SqlParameter Tipo_update = new SqlParameter
                {
                    ParameterName = "@Tipo_update",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 50,
                    Value = tipo_update,
                };
                SqlCmd.Parameters.Add(Tipo_update);
                contador += 1;

                //Ejecutamos nuestro comando
                //Se puede ejecutar este metodo pero ya tenemos el mensaje que devuelve sql
                rpta = SqlCmd.ExecuteNonQuery() >= 1 ? "OK" : "NO";

                if (rpta != "OK")
                {
                    if (this.Mensaje_respuesta != null)
                    {
                        rpta = this.Mensaje_respuesta;
                    }
                }
                else
                {
                    if (detalle.Id_detalle_pedido == 0)
                        detalle.Id_detalle_pedido = Convert.ToInt32(SqlCmd.Parameters["@Id_detalle_pedidoSalida"].Value);
                }
            }
            //Mostramos posible error que tengamos
            catch (SqlException ex)
            {
                rpta = ex.Message;
            }
            catch (Exception ex)
            {
                rpta = ex.Message;
            }
            finally
            {
                //Si la cadena SqlCon esta abierta la cerramos
                if (SqlCon.State == ConnectionState.Open)
                    SqlCon.Close();
            }
            return rpta;
        }
        #endregion

        #region METODO BUSCAR PEDIDOS Y DETALLE
        public static DataTable BuscarPedidosYDetalle(string tipo_busqueda, string texto_busqueda,
            out DataTable DtResultadoDetalle,
            out DataTable dtDetallePlatoDetallado, out string rpta)
        {
            rpta = "OK";
            DtResultadoDetalle = new DataTable("PedidosDetalle");
            dtDetallePlatoDetallado = new DataTable("PlatosDetallados");
            DataSet ds = new DataSet();
            DataTable dtPedidosDatosPrincipales = new DataTable("PedidosDatosPrincipales");
            SqlConnection SqlCon = new SqlConnection();
            try
            {
                /**INICIO BUSCAR PEDIDO DATOS PRINCIPALES**/
                SqlCon.ConnectionString = Conexion.Cn;
                SqlCommand Sqlcmd = new SqlCommand
                {
                    Connection = SqlCon,
                    CommandText = "sp_Buscar_pedidos",
                    CommandType = CommandType.StoredProcedure
                };

                SqlParameter Tipo_busqueda = new SqlParameter
                {
                    ParameterName = "@Tipo_busqueda",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 50,
                    Value = tipo_busqueda.Trim().ToUpper()
                };
                Sqlcmd.Parameters.Add(Tipo_busqueda);

                SqlParameter Texto_busqueda1 = new SqlParameter
                {
                    ParameterName = "@Texto_busqueda1",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 50,
                    Value = texto_busqueda.Trim().ToUpper()
                };
                Sqlcmd.Parameters.Add(Texto_busqueda1);

                SqlParameter Texto_busqueda2 = new SqlParameter
                {
                    ParameterName = "@Texto_busqueda2",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 50,
                    Value = string.Empty,
                };
                Sqlcmd.Parameters.Add(Texto_busqueda2);

                SqlDataAdapter SqlData = new SqlDataAdapter(Sqlcmd);
                SqlData.Fill(ds);

                if (ds.Tables.Count < 1)
                {
                    dtPedidosDatosPrincipales = null;
                    DtResultadoDetalle = null;
                }
                else
                {
                    if (ds.Tables.Count == 1)
                    {
                        dtPedidosDatosPrincipales = ds.Tables[0];
                    }
                    else if (ds.Tables.Count == 2)
                    {
                        dtPedidosDatosPrincipales = ds.Tables[0];
                        DtResultadoDetalle = ds.Tables[1];
                    }
                    else if (ds.Tables.Count == 3)
                    {
                        dtPedidosDatosPrincipales = ds.Tables[0];
                        DtResultadoDetalle = ds.Tables[1];
                        dtDetallePlatoDetallado = ds.Tables[2];
                    }
                }

                /**FIN BUSCAR PEDIDO DATOS PRINCIPALES**/
                /**INICIO BUSCAR PEDIDO DETALLE**/
            }
            catch (SqlException ex)
            {
                dtPedidosDatosPrincipales = null;
                DtResultadoDetalle = null;
                rpta = ex.Message;
            }
            catch (Exception ex)
            {
                dtPedidosDatosPrincipales = null;
                DtResultadoDetalle = null;
                rpta = ex.Message;
            }

            return dtPedidosDatosPrincipales;
        }

        #endregion

        #region METODO BUSCAR PEDIDOS
        public static DataTable BuscarPedidos(string tipo_busqueda, string texto_busqueda)
        {
            DataTable DtResultado = new DataTable("Pedidos");
            SqlConnection SqlCon = new SqlConnection();
            try
            {
                SqlCon.ConnectionString = Conexion.Cn;
                SqlCommand Sqlcmd = new SqlCommand
                {
                    Connection = SqlCon,
                    CommandText = "sp_Buscar_pedidos",
                    CommandType = CommandType.StoredProcedure
                };

                SqlParameter Tipo_busqueda = new SqlParameter
                {
                    ParameterName = "@Tipo_busqueda",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 50,
                    Value = tipo_busqueda.Trim().ToUpper()
                };
                Sqlcmd.Parameters.Add(Tipo_busqueda);

                SqlParameter Texto_busqueda1 = new SqlParameter
                {
                    ParameterName = "@Texto_busqueda1",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 50,
                    Value = texto_busqueda.Trim().ToUpper()
                };
                Sqlcmd.Parameters.Add(Texto_busqueda1);

                SqlParameter Texto_busqueda2 = new SqlParameter
                {
                    ParameterName = "@Texto_busqueda2",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 50,
                    Value = "",
                };
                Sqlcmd.Parameters.Add(Texto_busqueda2);

                SqlDataAdapter SqlData = new SqlDataAdapter(Sqlcmd);
                SqlData.Fill(DtResultado);

                if (DtResultado.Rows.Count < 1)
                {
                    DtResultado = null;
                }
            }
            catch (SqlException)
            {
                DtResultado = null;
            }
            catch (Exception)
            {
                DtResultado = null;
            }

            return DtResultado;
        }

        public async static Task<(string rpta, DataTable dtPedidos)> BuscarPedidos(string tipo_busqueda, string texto_busqueda1, string texto_busqueda2)
        {
            string rpta = "OK";
            DataTable DtResultado = new DataTable("Pedidos");
            SqlConnection SqlCon = new SqlConnection();
            try
            {
                SqlCon.ConnectionString = Conexion.Cn;
                await SqlCon.OpenAsync();
                SqlCommand Sqlcmd = new SqlCommand
                {
                    Connection = SqlCon,
                    CommandText = "sp_Buscar_pedidos",
                    CommandType = CommandType.StoredProcedure
                };

                SqlParameter Tipo_busqueda = new SqlParameter
                {
                    ParameterName = "@Tipo_busqueda",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 50,
                    Value = tipo_busqueda.Trim().ToUpper()
                };
                Sqlcmd.Parameters.Add(Tipo_busqueda);

                SqlParameter Texto_busqueda1 = new SqlParameter
                {
                    ParameterName = "@Texto_busqueda1",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 50,
                    Value = texto_busqueda1.Trim().ToUpper()
                };
                Sqlcmd.Parameters.Add(Texto_busqueda1);

                SqlParameter Texto_busqueda2 = new SqlParameter
                {
                    ParameterName = "@Texto_busqueda2",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 50,
                    Value = texto_busqueda2.Trim().ToUpper()
                };
                Sqlcmd.Parameters.Add(Texto_busqueda2);

                SqlDataAdapter SqlData = new SqlDataAdapter(Sqlcmd);
                SqlData.Fill(DtResultado);

                if (DtResultado.Rows.Count < 1)
                {
                    DtResultado = null;
                }
            }
            catch (SqlException)
            {
                DtResultado = null;
            }
            catch (Exception)
            {
                DtResultado = null;
            }

            return (rpta, DtResultado);
        }
        #endregion

        #region METODO BUSCAR PEDIDOS ELIMINADOS
        public static DataTable BuscarPedidosEliminados(string tipo_busqueda, string texto_busqueda)
        {
            StringBuilder consulta = new StringBuilder();
            consulta.Append("SELECT DISTINCT dp.Id_pedido, dp.Id_usuario_clave_maestra, em.Nombre_empleado as Nombre_empleado_clave_maestra, " +
                "dp.Id_tipo, dp.Tipo, dp.Fecha, CONVERT(varchar(5), dp.Hora) as Hora, dp.Motivo, " +
                "(CASE dp.Tipo WHEN 'PLATO' THEN pl.Nombre_plato " +
                "WHEN 'BEBIDA' THEN be.Nombre_bebida ELSE '' END) AS Nombre, " +
                "(CASE dp.Tipo WHEN 'PLATO' THEN pl.Precio_plato " +
                "WHEN 'BEBIDA' THEN be.Precio_bebida ELSE '' END) AS Precio, " +
                "dp.Id_usuario_sesion, emp.Nombre_empleado as Nombre_empleado_sesion, " +
                "em.Cargo_empleado, " +
                "em.Telefono_empleado " +
                "FROM Delete_pedidos dp " +
                "INNER JOIN Empleados em " +
                "ON(dp.Id_usuario_clave_maestra = em.Id_empleado) " +
                "INNER JOIN Empleados emp " +
                "ON(dp.Id_usuario_sesion = emp.Id_empleado), " +
                "Platos pl, Bebidas be ");
            if (tipo_busqueda.Equals("FECHA"))
            {
                consulta.Append("WHERE (dp.Id_tipo = pl.Id_plato OR " +
                "dp.Id_tipo = be.Id_bebida) and " +
                "Fecha = CONVERT(date, @Fecha)  ");
            }
            else
            {
                consulta.Append("WHERE (dp.Id_tipo = pl.Id_plato OR " +
                "dp.Id_tipo = be.Id_bebida) ");
            }

            DataTable DtResultado = new DataTable("PedidosEliminados");
            SqlConnection SqlCon = new SqlConnection();
            try
            {
                SqlCon.ConnectionString = Conexion.Cn;
                SqlCommand Sqlcmd = new SqlCommand
                {
                    Connection = SqlCon,
                    CommandText = Convert.ToString(consulta),
                    CommandType = CommandType.Text
                };

                SqlParameter Fecha = new SqlParameter
                {
                    ParameterName = "@Fecha",
                    SqlDbType = SqlDbType.Date,
                    Value = texto_busqueda
                };
                Sqlcmd.Parameters.Add(Fecha);

                SqlDataAdapter SqlData = new SqlDataAdapter(Sqlcmd);
                SqlData.Fill(DtResultado);

                if (DtResultado.Rows.Count < 1)
                {
                    DtResultado = null;
                }
            }
            catch (SqlException)
            {
                DtResultado = null;
            }
            catch (Exception)
            {
                DtResultado = null;
            }

            return DtResultado;
        }

        #endregion

        #region METODO BUSCAR PRODUCTOS ELIMINADOS (reporte diario)
        /// <summary>
        /// Productos quitados de un pedido en el rango de fechas, con quién autorizó, cuándo y el motivo.
        /// Las unidades quitadas en el mismo minuto, por el mismo motivo, se agrupan en una fila con su cantidad.
        /// Tabla vacía si no hay ninguno.
        /// </summary>
        public static DataTable BuscarProductosEliminados(DateTime desde, DateTime hasta)
        {
            const string sql =
                "SELECT dp.Id_pedido, dp.Tipo, COALESCE(pl.Nombre_plato, be.Nombre_bebida, '') AS Nombre, " +
                "dp.Fecha, CONVERT(varchar(5), dp.Hora) AS Hora, dp.Motivo, " +
                "(CASE WHEN dp.Id_usuario_clave_maestra = 0 THEN 'Código maestro' ELSE em.Nombre_empleado END) AS Autoriza, " +
                "ses.Nombre_empleado AS Sesion, COUNT(*) AS Cantidad " +
                "FROM Delete_pedidos dp " +
                "LEFT JOIN Platos pl ON dp.Tipo = 'PLATO' AND pl.Id_plato = dp.Id_tipo " +
                "LEFT JOIN Bebidas be ON dp.Tipo = 'BEBIDA' AND be.Id_bebida = dp.Id_tipo " +
                "LEFT JOIN Empleados em ON em.Id_empleado = dp.Id_usuario_clave_maestra " +
                "LEFT JOIN Empleados ses ON ses.Id_empleado = dp.Id_usuario_sesion " +
                "WHERE dp.Fecha BETWEEN @Desde AND @Hasta " +
                "GROUP BY dp.Id_pedido, dp.Tipo, dp.Id_tipo, COALESCE(pl.Nombre_plato, be.Nombre_bebida, ''), " +
                "dp.Fecha, CONVERT(varchar(5), dp.Hora), dp.Motivo, dp.Id_usuario_clave_maestra, em.Nombre_empleado, ses.Nombre_empleado " +
                "ORDER BY dp.Fecha, CONVERT(varchar(5), dp.Hora)";

            DataTable tabla = new DataTable("ProductosEliminados");
            try
            {
                using (SqlConnection cn = new SqlConnection(Conexion.Cn))
                using (SqlCommand cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.Add("@Desde", SqlDbType.Date).Value = desde.Date;
                    cmd.Parameters.Add("@Hasta", SqlDbType.Date).Value = hasta.Date;
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        da.Fill(tabla);
                }
            }
            catch (Exception)
            {
                // El reporte sigue sin esta sección si la consulta falla.
            }
            return tabla;
        }
        #endregion

        #region METODO INSERTAR ELIMINACIÓN COMANDA
        public static string InsertarEliminaciónComanda(int id_pedido, int id_usuario_clave_maestra, int id_usuario_sesion, int id_tipo, string tipo, string observaciones)
        {
            // Con parámetros: la observación la escribe una persona y las comillas no pueden romper (ni inyectar) el SQL.
            const string consulta =
                "INSERT INTO Delete_pedidos (Id_pedido, Id_usuario_clave_maestra, Id_usuario_sesion, Id_tipo, Tipo, Fecha, Hora, Motivo) " +
                "VALUES (@Id_pedido, @Clave, @Sesion, @Id_tipo, @Tipo, @Fecha, @Hora, @Motivo)";
            try
            {
                using (SqlConnection cn = new SqlConnection(Conexion.Cn))
                using (SqlCommand cmd = new SqlCommand(consulta, cn))
                {
                    cmd.Parameters.Add("@Id_pedido", SqlDbType.Int).Value = id_pedido;
                    cmd.Parameters.Add("@Clave", SqlDbType.Int).Value = id_usuario_clave_maestra;
                    cmd.Parameters.Add("@Sesion", SqlDbType.Int).Value = id_usuario_sesion;
                    cmd.Parameters.Add("@Id_tipo", SqlDbType.Int).Value = id_tipo;
                    cmd.Parameters.Add("@Tipo", SqlDbType.VarChar, 50).Value = tipo;
                    cmd.Parameters.Add("@Fecha", SqlDbType.Date).Value = DateTime.Now.Date;
                    cmd.Parameters.Add("@Hora", SqlDbType.Time).Value = DateTime.Now.TimeOfDay;
                    cmd.Parameters.Add("@Motivo", SqlDbType.VarChar, 500).Value = (object)observaciones ?? DBNull.Value;
                    cn.Open();
                    return cmd.ExecuteNonQuery() >= 1 ? "OK" : "ERROR";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        #endregion

        #region METODO INSERTAR DETALLES INGREDIENTES PEDIDO
        public async Task<string> InsertarDetalleIngredientesPedido(List<Detalle_ingredientes_pedido> detalles)
        {
            //asignamos a una cadena string la variable rpta y la iniciamos en vacía
            string rpta = "";
            StringBuilder consulta = new StringBuilder();
            foreach(Detalle_ingredientes_pedido detalle in detalles)
            {
                consulta.Append(string.Format("INSERT INTO Detalle_ingredientes_pedido VALUES ({0}, {1}, {2}, {3}, '{4}'); ",
                    detalle.Id_pedido, detalle.Id_tipo, detalle.Id_ingrediente, detalle.Id_detalle_pedido, detalle.Observaciones));
            }

            SqlConnection SqlCon = new SqlConnection();
            SqlCon.InfoMessage += new SqlInfoMessageEventHandler(SqlCon_InfoMessage);
            SqlCon.FireInfoMessageEventOnUserErrors = true;
            //Capturador de errores
            try
            {
                SqlCon.ConnectionString = Conexion.Cn;
                await SqlCon.OpenAsync();
                //Establecer comando
                SqlCommand SqlCmd = new SqlCommand
                {
                    Connection = SqlCon,
                    CommandText = consulta.ToString(),
                    CommandType = CommandType.Text,
                };

                //Ejecutamos nuestro comando
                //Se puede ejecutar este metodo pero ya tenemos el mensaje que devuelve sql
                rpta = SqlCmd.ExecuteNonQuery() >= 1 ? "OK" : "NO se Ingreso el Registro";

                if (!rpta.Equals("OK"))
                {
                    if (this.Mensaje_respuesta != null)
                    {
                        rpta = this.Mensaje_respuesta;
                    }
                }
            }
            //Mostramos posible error que tengamos
            catch (SqlException ex)
            {
                rpta = ex.Message;
            }
            catch (Exception ex)
            {
                rpta = ex.Message;
            }
            finally
            {
                //Si la cadena SqlCon esta abierta la cerramos
                if (SqlCon.State == ConnectionState.Open)
                    SqlCon.Close();
            }
            return rpta;
        }
        #endregion
    }
}
