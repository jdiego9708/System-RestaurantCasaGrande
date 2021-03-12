﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using System.Data.SqlClient;

namespace CapaDatos
{
    public class DPedidos
    {

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
        public string InsertarPedido(List<string> Variables, DataTable Detalles,
            out int id_pedido)
        {
            DataTable TablaDetalle = Detalles;
            int rows = TablaDetalle.Rows.Count;
            id_pedido = 0;
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
                    Value = Convert.ToInt32(Variables[contador])
                };
                SqlCmd.Parameters.Add(Id_mesa);
                contador += 1;

                SqlParameter Id_empleado = new SqlParameter
                {
                    ParameterName = "@Id_empleado",
                    SqlDbType = SqlDbType.Int,
                    Value = Convert.ToInt32(Variables[contador])
                };
                SqlCmd.Parameters.Add(Id_empleado);
                contador += 1;

                SqlParameter Id_cliente = new SqlParameter
                {
                    ParameterName = "@Id_cliente",
                    SqlDbType = SqlDbType.Int,
                    Value = Convert.ToInt32(Variables[contador])
                };
                SqlCmd.Parameters.Add(Id_cliente);
                contador += 1;

                SqlParameter Id_usuario = new SqlParameter
                {
                    ParameterName = "@Id_usuario",
                    SqlDbType = SqlDbType.Int,
                    Value = Convert.ToInt32(Variables[contador])
                };
                SqlCmd.Parameters.Add(Id_usuario);
                contador += 1;

                SqlParameter Fecha_pedido = new SqlParameter
                {
                    ParameterName = "@Fecha_pedido",
                    SqlDbType = SqlDbType.Date,
                    Value = DateTime.Now.ToString("yyyy-MM-dd"),
                };
                SqlCmd.Parameters.Add(Fecha_pedido);

                SqlParameter Hora_pedido = new SqlParameter
                {
                    ParameterName = "@Hora_pedido",
                    SqlDbType = SqlDbType.Time,
                    Value = DateTime.Now.ToString("HH:mm"),
                };
                SqlCmd.Parameters.Add(Hora_pedido);

                SqlParameter Tipo_pedido = new SqlParameter
                {
                    ParameterName = "@Tipo_pedido",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 50,
                    Value = Variables[contador]
                };
                SqlCmd.Parameters.Add(Tipo_pedido);
                contador += 1;

                SqlParameter Observaciones_pedido = new SqlParameter
                {
                    ParameterName = "@Observaciones_pedido",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 200,
                    Value = Variables[contador]
                };
                SqlCmd.Parameters.Add(Observaciones_pedido);
                contador += 1;

                SqlParameter Detalles_pedido = new SqlParameter("@Detalle_pedido", TablaDetalle);
                //Detalles_pedido.ParameterName = "@Detalle_pedido";
                //Detalles_pedido.Value = TablaDetalle;
                SqlCmd.Parameters.Add(Detalles_pedido);

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
                    id_pedido = Convert.ToInt32(SqlCmd.Parameters["@Id_pedido"].Value);
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

        #region METODO CANCELAR DOMICILIO
        public string CancelarDomicilio(int id_pedido, string observaciones)
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
        public string ActualizarDetallePedido(List<string> Variables)
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

                SqlParameter Id_pedido = new SqlParameter
                {
                    ParameterName = "@Id_pedido",
                    SqlDbType = SqlDbType.Int,
                    Value = Convert.ToInt32(Variables[contador])
                };
                SqlCmd.Parameters.Add(Id_pedido);
                contador += 1;

                SqlParameter Id_tipo = new SqlParameter
                {
                    ParameterName = "@Id_tipo",
                    SqlDbType = SqlDbType.Int,
                    Value = Convert.ToInt32(Variables[contador])
                };
                SqlCmd.Parameters.Add(Id_tipo);
                contador += 1;

                SqlParameter Tipo = new SqlParameter
                {
                    ParameterName = "@Tipo",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 50,
                    Value = Convert.ToString(Variables[contador])
                };
                SqlCmd.Parameters.Add(Tipo);
                contador += 1;

                SqlParameter Precio = new SqlParameter
                {
                    ParameterName = "@Precio",
                    SqlDbType = SqlDbType.Int,
                    Value = Convert.ToInt32(Variables[contador])
                };
                SqlCmd.Parameters.Add(Precio);
                contador += 1;

                SqlParameter Cantidad = new SqlParameter
                {
                    ParameterName = "@Cantidad",
                    SqlDbType = SqlDbType.Int,
                    Value = Convert.ToInt32(Variables[contador])
                };
                SqlCmd.Parameters.Add(Cantidad);
                contador += 1;

                SqlParameter Observaciones = new SqlParameter
                {
                    ParameterName = "@Observaciones",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 500,
                    Value = Convert.ToString(Variables[contador])
                };
                SqlCmd.Parameters.Add(Observaciones);
                contador += 1;

                SqlParameter Id_usuario = new SqlParameter
                {
                    ParameterName = "@Id_usuario",
                    SqlDbType = SqlDbType.Int,
                    Value = Convert.ToInt32(Variables[contador])
                };
                SqlCmd.Parameters.Add(Id_usuario);
                contador += 1;

                SqlParameter Tipo_update = new SqlParameter
                {
                    ParameterName = "@Tipo_update",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 50,
                    Value = Convert.ToString(Variables[contador])
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
            out DataTable DtResultadoDetalle, out string rpta)
        {
            rpta = "OK";
            DtResultadoDetalle = new DataTable("PedidosDetalle");
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
                    dtPedidosDatosPrincipales = ds.Tables[0];
                    DtResultadoDetalle = ds.Tables[1];
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

        #region METODO INSERTAR ELIMINACIÓN COMANDA
        public static string InsertarEliminaciónComanda(int id_pedido, int id_usuario_clave_maestra, int id_usuario_sesion, int id_tipo, string tipo, string observaciones)
        {
            string consulta = "INSERT INTO Delete_pedidos VALUES ('" + id_pedido + "','" + id_usuario_clave_maestra + "','" + id_usuario_sesion + "','" +
                id_tipo + "','" + tipo + "','" + DateTime.Now.ToString("yyyy-MM-dd") + "','" + DateTime.Now.ToString("HH:mm") + "','" + observaciones + "')";

            return Conexion.EjecutarConsultaCadena(consulta, false);
        }
        #endregion
    }
}
