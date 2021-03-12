
using System;
using System.Data;
using System.Data.SqlClient;

namespace CapaDatos
{
    public class DReservas
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

        #endregion

        #region CONSTRUCTOR VACIO
        public DReservas() { }
        #endregion

        #region METODO INSERTAR RESERVA
        public string InsertarReserva(out int id_reserva, int id_mesa,
            int id_cliente, string fecha, string hora, string observaciones)
        {
            id_reserva = 0;
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
                    CommandText = "sp_Insertar_reserva",
                    //Indicamos que es un procedimiento almacenado
                    CommandType = CommandType.StoredProcedure
                };

                SqlParameter Id_reserva = new SqlParameter
                {
                    ParameterName = "@Id_reserva",
                    SqlDbType = SqlDbType.Int,
                    Direction = ParameterDirection.Output
                };
                SqlCmd.Parameters.Add(Id_reserva);

                SqlParameter Id_mesa = new SqlParameter
                {
                    ParameterName = "@Id_mesa",
                    SqlDbType = SqlDbType.Int,
                    Value = id_mesa
                };
                SqlCmd.Parameters.Add(Id_mesa);

                SqlParameter Id_cliente = new SqlParameter
                {
                    ParameterName = "@Id_cliente",
                    SqlDbType = SqlDbType.Int,
                    Value = id_cliente
                };
                SqlCmd.Parameters.Add(Id_cliente);

                SqlParameter Fecha = new SqlParameter
                {
                    ParameterName = "@Fecha",
                    SqlDbType = SqlDbType.Date,
                    Value = fecha
                };
                SqlCmd.Parameters.Add(Fecha);

                SqlParameter Hora = new SqlParameter
                {
                    ParameterName = "@Hora",
                    SqlDbType = SqlDbType.Time,
                    Value = hora
                };
                SqlCmd.Parameters.Add(Hora);

                SqlParameter Observaciones = new SqlParameter
                {
                    ParameterName = "@Observaciones",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 50,
                    Value = observaciones
                };
                SqlCmd.Parameters.Add(Observaciones);

                //Ejecutamos nuestro comando
                //Se puede ejecutar este metodo pero ya tenemos el mensaje que devuelve sql
                rpta = SqlCmd.ExecuteNonQuery() >= 1 ? "OK" : "NO se Ingreso el Registro";

                if (!rpta.Equals("OK"))
                {
                    if (this.Mensaje_respuesta != null)
                        rpta = this.Mensaje_respuesta;
                }
                else
                {
                    id_reserva = Convert.ToInt32(SqlCmd.Parameters["@Id_reserva"].Value);
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

        #region METODO EDITAR RESERVA
        public string EditarReserva(int id_reserva, int id_mesa,
            int id_cliente, string fecha, string hora, string observaciones, string estado)
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
                    CommandText = "sp_Editar_reserva",
                    //Indicamos que es un procedimiento almacenado
                    CommandType = CommandType.StoredProcedure
                };

                SqlParameter Id_reserva = new SqlParameter
                {
                    ParameterName = "@Id_reserva",
                    SqlDbType = SqlDbType.Int,
                    Value = id_reserva
                };
                SqlCmd.Parameters.Add(Id_reserva);

                SqlParameter Id_mesa = new SqlParameter
                {
                    ParameterName = "@Id_mesa",
                    SqlDbType = SqlDbType.Int,
                    Value = id_mesa
                };
                SqlCmd.Parameters.Add(Id_mesa);

                SqlParameter Id_cliente = new SqlParameter
                {
                    ParameterName = "@Id_cliente",
                    SqlDbType = SqlDbType.Int,
                    Value = id_cliente
                };
                SqlCmd.Parameters.Add(Id_cliente);

                SqlParameter Fecha = new SqlParameter
                {
                    ParameterName = "@Fecha",
                    SqlDbType = SqlDbType.Date,
                    Value = fecha
                };
                SqlCmd.Parameters.Add(Fecha);

                SqlParameter Hora = new SqlParameter
                {
                    ParameterName = "@Hora",
                    SqlDbType = SqlDbType.Time,
                    Value = hora
                };
                SqlCmd.Parameters.Add(Hora);

                SqlParameter Observaciones = new SqlParameter
                {
                    ParameterName = "@Observaciones",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 50,
                    Value = observaciones
                };
                SqlCmd.Parameters.Add(Observaciones);

                SqlParameter Estado = new SqlParameter
                {
                    ParameterName = "@Estado",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 50,
                    Value = estado
                };
                SqlCmd.Parameters.Add(Estado);

                //Ejecutamos nuestro comando
                //Se puede ejecutar este metodo pero ya tenemos el mensaje que devuelve sql
                rpta = SqlCmd.ExecuteNonQuery() >= 1 ? "OK" : "NO se Ingreso el Registro";

                if (!rpta.Equals("OK"))
                {
                    if (this.Mensaje_respuesta != null)
                        rpta = this.Mensaje_respuesta;
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

        #region METODO BUSCAR RESERVAS
        public DataTable BuscarReservas(string tipo_busqueda, string texto_busqueda, string estado,
            out string rpta)
        {
            rpta = "OK";
            DataTable DtResultado = new DataTable("Reservas");
            SqlConnection SqlCon = new SqlConnection();
            SqlCon.InfoMessage += new SqlInfoMessageEventHandler(SqlCon_InfoMessage);
            SqlCon.FireInfoMessageEventOnUserErrors = true;
            try
            {
                SqlCon.ConnectionString = Conexion.Cn;
                SqlCommand Sqlcmd = new SqlCommand
                {
                    Connection = SqlCon,
                    CommandText = "sp_Buscar_reservas",
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

                SqlParameter Texto_busqueda = new SqlParameter
                {
                    ParameterName = "@Texto_busqueda",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 50,
                    Value = texto_busqueda.Trim().ToUpper()
                };
                Sqlcmd.Parameters.Add(Texto_busqueda);

                SqlParameter Estado = new SqlParameter
                {
                    ParameterName = "@Estado",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 50,
                    Value = estado.Trim().ToUpper()
                };
                Sqlcmd.Parameters.Add(Estado);

                SqlDataAdapter SqlData = new SqlDataAdapter(Sqlcmd);
                SqlData.Fill(DtResultado);

                if (DtResultado.Rows.Count < 1)
                {
                    DtResultado = null;
                }
            }
            catch (SqlException ex)
            {
                rpta = ex.Message;
                DtResultado = null;
            }
            catch (Exception ex)
            {
                rpta = ex.Message;
                DtResultado = null;
            }

            return DtResultado;
        }

        #endregion
    }
}
