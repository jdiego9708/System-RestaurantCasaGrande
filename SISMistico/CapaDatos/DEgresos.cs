using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using System.Data.SqlClient;
using CapaEntidades.Models;
using CapaEntidades.Helpers;

namespace CapaDatos
{
    public class DEgresos
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
        public DEgresos() { }
        #endregion

        #region METODO INSERTAR EGRESO
        public async Task<(string rpta, int id_egreso)> InsertarEgreso(Egresos egreso)
        {
            int contador = 0;
            int id_egreso = 0;
            string consulta = "INSERT INTO Egresos VALUES (@Id_empleado, @Fecha_egreso, @Valor_egreso, " +
                "@Descripcion_egreso, @Estado_egreso) " +
                "SET @Id_egreso = SCOPE_IDENTITY(); ";

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
                    CommandText = consulta,
                    CommandType = CommandType.Text,
                };

                SqlParameter Id_egreso = new SqlParameter
                {
                    ParameterName = "@Id_egreso",
                    SqlDbType = SqlDbType.Int,
                    Direction = ParameterDirection.Output,
                };
                SqlCmd.Parameters.Add(Id_egreso);

                SqlParameter Id_empleado = new SqlParameter
                {
                    ParameterName = "@Id_empleado",
                    SqlDbType = SqlDbType.Int,
                    Value = egreso.Id_empleado,
                };
                SqlCmd.Parameters.Add(Id_empleado);

                SqlParameter Fecha_egreso = new SqlParameter
                {
                    ParameterName = "@Fecha_egreso",
                    SqlDbType = SqlDbType.Date,
                    Value = egreso.Fecha_egreso,
                };
                SqlCmd.Parameters.Add(Fecha_egreso);
                contador += 1;

                SqlParameter Valor_egreso = new SqlParameter
                {
                    ParameterName = "@Valor_egreso",
                    SqlDbType = SqlDbType.Decimal,
                    Value = egreso.Valor_egreso,
                };
                SqlCmd.Parameters.Add(Valor_egreso);
                contador += 1;

                SqlParameter Descripcion_egreso = new SqlParameter
                {
                    ParameterName = "@Descripcion_egreso",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 200,
                    Value = egreso.Descripcion_egreso,
                };
                SqlCmd.Parameters.Add(Descripcion_egreso);
                contador += 1;
              
                SqlParameter Estado_egreso = new SqlParameter
                {
                    ParameterName = "@Estado_egreso",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 50,
                    Value = egreso.Estado_egreso,
                };
                SqlCmd.Parameters.Add(Estado_egreso);

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
                    id_egreso = Convert.ToInt32(SqlCmd.Parameters["@Id_egreso"].Value);
                }
            }
            //Mostramos posible error que tengamos
            catch (SqlException ex)
            {
                rpta = ex.Message;
            }
            catch(Exception ex)
            {
                rpta = ex.Message;
            }
            finally
            {
                //Si la cadena SqlCon esta abierta la cerramos
                if (SqlCon.State == ConnectionState.Open)
                    SqlCon.Close();
            }
            return (rpta, id_egreso);
        }
        #endregion

        #region METODO EDITAR EGRESO
        public async Task<string> EditarEgreso(int id_egreso, Egresos egreso)
        {
            int contador = 0;
            string consulta = "UPDATE Egresos SET " +
                "Id_empleado = @Id_empleado, " +
                "Fecha_egreso = @Fecha_egreso, " +
                "Valor_egreso= @Valor_egreso, " +
                "Descripcion_egreso = @Descripcion_egreso, " +
                "Estado_egreso = @Estado_egreso " +
                "WHERE Id_egreso = @Id_egreso ";

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
                    CommandText = consulta,
                    CommandType = CommandType.Text,
                };

                SqlParameter Id_egreso = new SqlParameter
                {
                    ParameterName = "@Id_egreso",
                    SqlDbType = SqlDbType.Int,
                    Value = id_egreso,
                };
                SqlCmd.Parameters.Add(Id_egreso);

                SqlParameter Id_empleado = new SqlParameter
                {
                    ParameterName = "@Id_empleado",
                    SqlDbType = SqlDbType.Int,
                    Value = egreso.Id_empleado,
                };
                SqlCmd.Parameters.Add(Id_empleado);

                SqlParameter Fecha_egreso = new SqlParameter
                {
                    ParameterName = "@Fecha_egreso",
                    SqlDbType = SqlDbType.Date,
                    Value = egreso.Fecha_egreso,
                };
                SqlCmd.Parameters.Add(Fecha_egreso);
                contador += 1;

                SqlParameter Valor_egreso = new SqlParameter
                {
                    ParameterName = "@Valor_egreso",
                    SqlDbType = SqlDbType.Decimal,
                    Value = egreso.Valor_egreso,
                };
                SqlCmd.Parameters.Add(Valor_egreso);
                contador += 1;

                SqlParameter Descripcion_egreso = new SqlParameter
                {
                    ParameterName = "@Descripcion_egreso",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 200,
                    Value = egreso.Descripcion_egreso,
                };
                SqlCmd.Parameters.Add(Descripcion_egreso);
                contador += 1;

                SqlParameter Estado_egreso = new SqlParameter
                {
                    ParameterName = "@Estado_egreso",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 50,
                    Value = egreso.Estado_egreso,
                };
                SqlCmd.Parameters.Add(Estado_egreso);

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

        #region METODO BUSCAR EGRESOS
        public async Task<(string rpta, DataTable dtEgresos)> BuscarEgreso(string tipo_busqueda, string texto_busqueda)
        {
            string rpta = "OK";
            DataTable dtNomina = new DataTable("Egresos");
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
                    CommandText = "sp_Buscar_egreso",
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

                SqlParameter Texto_busqueda1 = new SqlParameter
                {
                    ParameterName = "@Texto_busqueda1",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 50,
                    Value = texto_busqueda.Trim(),
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
                await Task.Run(() => SqlData.Fill(dtNomina));                
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

            return (rpta, dtNomina);
        }

        public async Task<(string rpta, DataTable dtEgresos)> BuscarEgreso(string tipo_busqueda, string texto_busqueda1, string texto_busqueda2)
        {
            string rpta = "OK";
            DataTable dtNomina = new DataTable("Egresos");
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
                    CommandText = "sp_Buscar_egreso",
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

                SqlParameter Texto_busqueda1 = new SqlParameter
                {
                    ParameterName = "@Texto_busqueda1",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 50,
                    Value = texto_busqueda1.Trim(),
                };
                Sqlcmd.Parameters.Add(Texto_busqueda1);

                SqlParameter Texto_busqueda2 = new SqlParameter
                {
                    ParameterName = "@Texto_busqueda2",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 50,
                    Value = texto_busqueda2.Trim(),
                };
                Sqlcmd.Parameters.Add(Texto_busqueda2);

                SqlDataAdapter SqlData = new SqlDataAdapter(Sqlcmd);
                await Task.Run(() => SqlData.Fill(dtNomina));
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

            return (rpta, dtNomina);
        }

        public async Task<(string rpta, DataTable dtEgresos)> BuscarEgreso(ModelHelperSearch modelSearch)
        {
            string rpta = "OK";
            DataTable dtNomina = new DataTable("Egresos");
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
                    CommandText = "sp_Buscar_egreso",
                    CommandType = CommandType.StoredProcedure,
                };

                foreach (ParameterSQLModel param in modelSearch.ParametrosBusqueda)
                {
                    SqlParameter sqlParameter = new SqlParameter
                    {
                        ParameterName = param.ParameterName,
                        SqlDbType = ValidationType.ConvertTypeParameter(param.ParameterValue),
                        Value = param.ParameterValue,
                    };

                    Sqlcmd.Parameters.Add(sqlParameter);
                }

                SqlDataAdapter SqlData = new SqlDataAdapter(Sqlcmd);
                await Task.Run(() => SqlData.Fill(dtNomina));
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

            return (rpta, dtNomina);
        }

        #endregion
    }
}
