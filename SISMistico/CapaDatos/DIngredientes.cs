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
    public class DIngredientes
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
        public DIngredientes() { }
        #endregion

        #region METODO INSERTAR INGREDIENTE
        public async Task<(string rpta, int id)> InsertarIngrediente(Ingredientes ing)
        {
            int id = 0;
            //asignamos a una cadena string la variable rpta y la iniciamos en vacía
            string rpta = "";
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
                    CommandText = "sp_Insertar_ingrediente",
                    CommandType = CommandType.StoredProcedure,
                };

                SqlParameter Id_ingrediente = new SqlParameter
                {
                    ParameterName = "@Id_ingrediente",
                    SqlDbType = SqlDbType.Int,
                    Direction = ParameterDirection.Output,
                };
                SqlCmd.Parameters.Add(Id_ingrediente);

                SqlParameter Tipo_ingrediente = new SqlParameter
                {
                    ParameterName = "@Tipo_ingrediente",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 50,
                    Value = ing.Tipo_ingrediente,
                };
                SqlCmd.Parameters.Add(Tipo_ingrediente);

                SqlParameter Nombre_ingrediente = new SqlParameter
                {
                    ParameterName = "@Nombre_ingrediente",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 50,
                    Value = ing.Nombre_ingrediente,
                };
                SqlCmd.Parameters.Add(Nombre_ingrediente);

                SqlParameter Descripcion_ingrediente = new SqlParameter
                {
                    ParameterName = "@Descripcion_ingrediente",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 200,
                    Value = ing.Descripcion_ingrediente,
                };
                SqlCmd.Parameters.Add(Descripcion_ingrediente);

                SqlParameter Estado_ingrediente = new SqlParameter
                {
                    ParameterName = "@Estado_ingrediente",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 50,
                    Value = ing.Estado_ingrediente,
                };
                SqlCmd.Parameters.Add(Estado_ingrediente);

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
                else
                {
                    id = Convert.ToInt32(SqlCmd.Parameters["@Id_ingrediente"].Value);
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
            return (rpta, id);
        }
        #endregion

        #region METODO EDITAR INGREDIENTE
        public async Task<string> EditarIngrediente(int id_ingrediente, Ingredientes ing)
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
                await SqlCon.OpenAsync();
                //Establecer comando
                SqlCommand SqlCmd = new SqlCommand
                {
                    Connection = SqlCon,
                    CommandText = "sp_Editar_ingrediente",
                    CommandType = CommandType.StoredProcedure,
                };

                SqlParameter Id_ingrediente = new SqlParameter
                {
                    ParameterName = "@Id_ingrediente",
                    SqlDbType = SqlDbType.Int,
                    Value = id_ingrediente,
                };
                SqlCmd.Parameters.Add(Id_ingrediente);

                SqlParameter Tipo_ingrediente = new SqlParameter
                {
                    ParameterName = "@Tipo_ingrediente",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 50,
                    Value = ing.Tipo_ingrediente,
                };
                SqlCmd.Parameters.Add(Tipo_ingrediente);

                SqlParameter Nombre_ingrediente = new SqlParameter
                {
                    ParameterName = "@Nombre_ingrediente",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 50,
                    Value = ing.Nombre_ingrediente,
                };
                SqlCmd.Parameters.Add(Nombre_ingrediente);

                SqlParameter Descripcion_ingrediente = new SqlParameter
                {
                    ParameterName = "@Descripcion_ingrediente",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 200,
                    Value = ing.Descripcion_ingrediente,
                };
                SqlCmd.Parameters.Add(Descripcion_ingrediente);

                SqlParameter Estado_ingrediente = new SqlParameter
                {
                    ParameterName = "@Estado_ingrediente",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 50,
                    Value = ing.Estado_ingrediente,
                };
                SqlCmd.Parameters.Add(Estado_ingrediente);

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

        #region METODO BUSCAR INGREDIENTE
        public async Task<(string rpta, DataTable dtIngredientes)> BuscarIngredientes(string tipo_busqueda, string texto_busqueda)
        {
            string rpta = "OK";
            DataTable dt = new DataTable("Ingredientes");
            SqlConnection SqlCon = new SqlConnection();
            SqlCon.InfoMessage += new SqlInfoMessageEventHandler(SqlCon_InfoMessage);
            SqlCon.FireInfoMessageEventOnUserErrors = true;
            try
            {
                SqlCon.ConnectionString = Conexion.Cn;
                await SqlCon.OpenAsync();
                SqlCommand Sqlcmd = new SqlCommand
                {
                    Connection = SqlCon,
                    CommandText = "sp_Buscar_ingredientes",
                    CommandType = CommandType.StoredProcedure,
                };

                SqlParameter Tipo_busqueda = new SqlParameter
                {
                    ParameterName = "@Tipo_busqueda",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 50,
                    Value = tipo_busqueda.Trim(),
                };
                Sqlcmd.Parameters.Add(Tipo_busqueda);

                SqlParameter Texto_busqueda = new SqlParameter
                {
                    ParameterName = "@Texto_busqueda",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 50,
                    Value = texto_busqueda.Trim(),
                };
                Sqlcmd.Parameters.Add(Texto_busqueda);

                SqlDataAdapter SqlData = new SqlDataAdapter(Sqlcmd);
                SqlData.Fill(dt);

                if (dt.Rows.Count < 1)
                {
                    dt = null;
                }
            }
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
                if (SqlCon.State == ConnectionState.Open)
                    SqlCon.Close();
            }

            return (rpta, dt);
        }

        public DataTable BuscarIngredientes(string tipo_busqueda, string texto_busqueda, out string rpta)
        {
            rpta = "OK";
            DataTable dt = new DataTable("Ingredientes");
            SqlConnection SqlCon = new SqlConnection();
            SqlCon.InfoMessage += new SqlInfoMessageEventHandler(SqlCon_InfoMessage);
            SqlCon.FireInfoMessageEventOnUserErrors = true;
            try
            {
                SqlCon.ConnectionString = Conexion.Cn;
                SqlCon.Open();
                SqlCommand Sqlcmd = new SqlCommand
                {
                    Connection = SqlCon,
                    CommandText = "sp_Buscar_ingredientes",
                    CommandType = CommandType.StoredProcedure,
                };

                SqlParameter Tipo_busqueda = new SqlParameter
                {
                    ParameterName = "@Tipo_busqueda",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 50,
                    Value = tipo_busqueda.Trim(),
                };
                Sqlcmd.Parameters.Add(Tipo_busqueda);

                SqlParameter Texto_busqueda = new SqlParameter
                {
                    ParameterName = "@Texto_busqueda",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 50,
                    Value = texto_busqueda.Trim(),
                };
                Sqlcmd.Parameters.Add(Texto_busqueda);

                SqlDataAdapter SqlData = new SqlDataAdapter(Sqlcmd);
                SqlData.Fill(dt);

                if (dt.Rows.Count < 1)
                {
                    dt = null;
                }
            }
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
                if (SqlCon.State == ConnectionState.Open)
                    SqlCon.Close();
            }

            return dt;
        }

        #endregion
    }
}
