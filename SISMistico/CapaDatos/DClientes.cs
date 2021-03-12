using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using System.Data.SqlClient;

namespace CapaDatos
{
    public class DClientes
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
        public DClientes() { }
        #endregion

        #region METODO INSERTAR CLIENTES
        public string InsertarClientes(DClientes DClientes)
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
                    CommandText = "sp_Insertar_cliente",
                    //Indicamos que es un procedimiento almacenado
                    CommandType = CommandType.StoredProcedure
                };

                SqlParameter Id_cliente = new SqlParameter
                {
                    ParameterName = "@Id_cliente",
                    SqlDbType = SqlDbType.Int,
                    Direction = ParameterDirection.Output
                };
                SqlCmd.Parameters.Add(Id_cliente);

                SqlParameter Nombre_cliente = new SqlParameter
                {
                    ParameterName = "@Nombre_cliente",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 500,
                    Value = DClientes.Variables[contador].Trim().ToUpper()
                };
                SqlCmd.Parameters.Add(Nombre_cliente);
                contador += 1;

                SqlParameter Telefono_cliente = new SqlParameter
                {
                    ParameterName = "@Telefono_cliente",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 50,
                    Value = DClientes.Variables[contador].Trim().ToUpper()
                };
                SqlCmd.Parameters.Add(Telefono_cliente);
                contador += 1;

                SqlParameter Correo_electronico = new SqlParameter
                {
                    ParameterName = "@Correo_electronico",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 500,
                    Value = DClientes.Variables[contador].Trim().ToUpper()
                };
                SqlCmd.Parameters.Add(Correo_electronico);
                contador += 1;

                SqlParameter Direccion_cliente = new SqlParameter
                {
                    ParameterName = "@Direccion_cliente",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 100,
                    Value = DClientes.Variables[contador].Trim().ToUpper()
                };
                SqlCmd.Parameters.Add(Direccion_cliente);
                contador += 1;

                SqlParameter Referencia_ubicacion = new SqlParameter
                {
                    ParameterName = "@Referencia_ubicacion",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 100,
                    Value = DClientes.Variables[contador].Trim().ToUpper()
                };
                SqlCmd.Parameters.Add(Referencia_ubicacion);
                contador += 1;

                SqlParameter Otras_observaciones = new SqlParameter
                {
                    ParameterName = "@Otras_observaciones",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 200,
                    Value = DClientes.Variables[contador].Trim().ToUpper()
                };
                SqlCmd.Parameters.Add(Otras_observaciones);
                contador += 1;

                SqlParameter Estado_cliente = new SqlParameter
                {
                    ParameterName = "@Estado_cliente",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 50,
                    Value = DClientes.Variables[contador].Trim().ToUpper()
                };
                SqlCmd.Parameters.Add(Estado_cliente);
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

        #region METODO EDITAR CLIENTES
        public string EditarClientes(DClientes DClientes)
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
                    CommandText = "sp_Editar_cliente",
                    //Indicamos que es un procedimiento almacenado
                    CommandType = CommandType.StoredProcedure
                };

                SqlParameter Id_cliente = new SqlParameter
                {
                    ParameterName = "@Id_cliente",
                    SqlDbType = SqlDbType.Int,
                    Value = DClientes.Variables[contador].Trim().ToUpper()
                };
                SqlCmd.Parameters.Add(Id_cliente);
                contador += 1;

                SqlParameter Nombre_cliente = new SqlParameter
                {
                    ParameterName = "@Nombre_cliente",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 500,
                    Value = DClientes.Variables[contador].Trim().ToUpper()
                };
                SqlCmd.Parameters.Add(Nombre_cliente);
                contador += 1;

                SqlParameter Telefono_cliente = new SqlParameter
                {
                    ParameterName = "@Telefono_cliente",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 50,
                    Value = DClientes.Variables[contador].Trim().ToUpper()
                };
                SqlCmd.Parameters.Add(Telefono_cliente);
                contador += 1;

                SqlParameter Correo_electronico = new SqlParameter
                {
                    ParameterName = "@Correo_electronico",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 500,
                    Value = DClientes.Variables[contador].Trim().ToUpper()
                };
                SqlCmd.Parameters.Add(Correo_electronico);
                contador += 1;

                SqlParameter Direccion_cliente = new SqlParameter
                {
                    ParameterName = "@Direccion_cliente",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 100,
                    Value = DClientes.Variables[contador].Trim().ToUpper()
                };
                SqlCmd.Parameters.Add(Direccion_cliente);
                contador += 1;

                SqlParameter Referencia_ubicacion = new SqlParameter
                {
                    ParameterName = "@Referencia_ubicacion",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 100,
                    Value = DClientes.Variables[contador].Trim().ToUpper()
                };
                SqlCmd.Parameters.Add(Referencia_ubicacion);
                contador += 1;

                SqlParameter Otras_observaciones = new SqlParameter
                {
                    ParameterName = "@Otras_observaciones",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 200,
                    Value = DClientes.Variables[contador].Trim().ToUpper()
                };
                SqlCmd.Parameters.Add(Otras_observaciones);
                contador += 1;

                SqlParameter Estado_cliente = new SqlParameter
                {
                    ParameterName = "@Estado_cliente",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 50,
                    Value = DClientes.Variables[contador].Trim().ToUpper()
                };
                SqlCmd.Parameters.Add(Estado_cliente);
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

        #region METODO BUSCAR CLIENTES
        public static DataTable BuscarClientes(string tipo_busqueda, string texto_busqueda)
        {
            DataTable DtResultado = new DataTable("Clientes");
            SqlConnection SqlCon = new SqlConnection();
            try
            {
                SqlCon.ConnectionString = Conexion.Cn;
                SqlCommand Sqlcmd = new SqlCommand
                {
                    Connection = SqlCon,
                    CommandText = "sp_Buscar_clientes",
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
    }
}
