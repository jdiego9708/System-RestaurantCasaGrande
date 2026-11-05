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

        #region BUSQUEDA RAPIDA DE CLIENTES
        /// <summary>
        /// Clientes activos cuyo nombre o telefono contiene el texto (los 50 mas recientes si esta vacio). Nunca
        /// devuelve null: sin resultados, la tabla queda vacia. No usa sp_Buscar_clientes porque ese falla al final.
        /// </summary>
        public static DataTable SearchQuick(string text)
        {
            DataTable table = new DataTable("Clientes");
            using (SqlConnection con = new SqlConnection(Conexion.Cn))
            using (SqlCommand cmd = new SqlCommand("sp_Clientes_buscar_rapido", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Texto", SqlDbType.VarChar, 50).Value = (text ?? string.Empty).Trim().ToUpper();
                new SqlDataAdapter(cmd).Fill(table);
            }
            return table;
        }

        /// <summary>Un cliente por Id, o null si no existe.</summary>
        public static DataTable GetById(int customerId)
        {
            DataTable table = new DataTable("Clientes");
            using (SqlConnection con = new SqlConnection(Conexion.Cn))
            using (SqlCommand cmd = new SqlCommand("sp_Cliente_obtener", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Id_cliente", SqlDbType.Int).Value = customerId;
                new SqlDataAdapter(cmd).Fill(table);
            }
            return table.Rows.Count == 0 ? null : table;
        }
        #endregion

        #region METODO EDITAR CLIENTE BASICO
        /// <summary>Cambia nombre, telefono y correo del cliente sin tocar el resto de sus datos. Devuelve "OK" o el error.</summary>
        public static string UpdateCustomer(int customerId, string name, string phone, string email)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(Conexion.Cn))
                using (SqlCommand cmd = new SqlCommand("sp_Cliente_editar_basico", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Id_cliente", SqlDbType.Int).Value = customerId;
                    cmd.Parameters.Add("@Nombre_cliente", SqlDbType.VarChar, 500).Value = name.Trim().ToUpper();
                    cmd.Parameters.Add("@Telefono_cliente", SqlDbType.VarChar, 50).Value = phone.Trim();
                    cmd.Parameters.Add("@Correo_electronico", SqlDbType.VarChar, 500).Value =
                        string.IsNullOrWhiteSpace(email) ? (object)DBNull.Value : email.Trim().ToUpper();
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
        #endregion

        #region METODO INSERTAR CLIENTE RAPIDO
        /// <summary>
        /// Crea un cliente con lo minimo que necesita un domicilio (nombre, telefono y correo opcional) y devuelve
        /// su Id. La direccion no va aqui: vive en Domicilios, donde un cliente puede tener varias.
        /// </summary>
        public static string InsertCustomer(string name, string phone, string email, out int customerId)
        {
            customerId = 0;
            try
            {
                using (SqlConnection con = new SqlConnection(Conexion.Cn))
                using (SqlCommand cmd = new SqlCommand("sp_Insertar_cliente", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    SqlParameter id = cmd.Parameters.Add("@Id_cliente", SqlDbType.Int);
                    id.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@Nombre_cliente", SqlDbType.VarChar, 500).Value = name.Trim().ToUpper();
                    cmd.Parameters.Add("@Telefono_cliente", SqlDbType.VarChar, 50).Value = phone.Trim();
                    cmd.Parameters.Add("@Correo_electronico", SqlDbType.VarChar, 500).Value =
                        string.IsNullOrWhiteSpace(email) ? (object)DBNull.Value : email.Trim().ToUpper();
                    // Los reportes concatenan estos campos solo cuando no son NULL.
                    cmd.Parameters.Add("@Direccion_cliente", SqlDbType.VarChar, 100).Value = DBNull.Value;
                    cmd.Parameters.Add("@Referencia_ubicacion", SqlDbType.VarChar, 100).Value = DBNull.Value;
                    cmd.Parameters.Add("@Otras_observaciones", SqlDbType.VarChar, 100).Value = DBNull.Value;
                    cmd.Parameters.Add("@Estado_cliente", SqlDbType.VarChar, 50).Value = "ACTIVO";
                    con.Open();
                    cmd.ExecuteNonQuery();
                    customerId = Convert.ToInt32(id.Value);
                    return "OK";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
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
