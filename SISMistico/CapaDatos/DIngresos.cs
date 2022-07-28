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
    public class DIngresos
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
        public DIngresos() { }
        #endregion

        #region METODO INSERTAR INGRESO
        public async Task<(string rpta, int id_ingreso)> InsertarIngreso(Ingresos ingreso)
        {
            int contador = 0;
            int id_ingreso = 0;
            string consulta = "INSERT INTO Ingresos VALUES (@Id_empleado, @Fecha_ingreso, @Valor_ingreso, " +
                "@Descripcion_ingreso, @Estado_ingreso) " +
                "SET @Id_ingreso = SCOPE_IDENTITY(); ";

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

                SqlParameter Id_ingreso = new SqlParameter
                {
                    ParameterName = "@Id_ingreso",
                    SqlDbType = SqlDbType.Int,
                    Direction = ParameterDirection.Output,
                };
                SqlCmd.Parameters.Add(Id_ingreso);

                SqlParameter Id_empleado = new SqlParameter
                {
                    ParameterName = "@Id_empleado",
                    SqlDbType = SqlDbType.Int,
                    Value = ingreso.Id_empleado,
                };
                SqlCmd.Parameters.Add(Id_empleado);

                SqlParameter Fecha_ingreso = new SqlParameter
                {
                    ParameterName = "@Fecha_ingreso",
                    SqlDbType = SqlDbType.Date,
                    Value = ingreso.Fecha_ingreso,
                };
                SqlCmd.Parameters.Add(Fecha_ingreso);
                contador += 1;

                SqlParameter Valor_ingreso = new SqlParameter
                {
                    ParameterName = "@Valor_ingreso",
                    SqlDbType = SqlDbType.Decimal,
                    Value = ingreso.Valor_ingreso,
                };
                SqlCmd.Parameters.Add(Valor_ingreso);
                contador += 1;

                SqlParameter Descripcion_ingreso = new SqlParameter
                {
                    ParameterName = "@Descripcion_ingreso",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 200,
                    Value = ingreso.Descripcion_ingreso,
                };
                SqlCmd.Parameters.Add(Descripcion_ingreso);
                contador += 1;
              
                SqlParameter Estado_ingreso = new SqlParameter
                {
                    ParameterName = "@Estado_ingreso",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 50,
                    Value = ingreso.Estado_ingreso,
                };
                SqlCmd.Parameters.Add(Estado_ingreso);

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
                    id_ingreso = Convert.ToInt32(SqlCmd.Parameters["@Id_ingreso"].Value);
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
            return (rpta, id_ingreso);
        }
        #endregion

        #region METODO EDITAR INGRESO
        public async Task<string> EditarIngreso(int id_ingreso, Ingresos ingreso)
        {
            int contador = 0;
            string consulta = "UPDATE Ingresos SET " +
                "Id_empleado = @Id_empleado, " +
                "Fecha_ingreso = @Fecha_ingreso, " +
                "Valor_ingreso= @Valor_ingreso, " +
                "Descripcion_ingreso = @Descripcion_ingreso, " +
                "Estado_ingreso = @Estado_ingreso " +
                "WHERE Id_ingreso = @Id_ingreso ";

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

                SqlParameter Id_ingreso = new SqlParameter
                {
                    ParameterName = "@Id_egreso",
                    SqlDbType = SqlDbType.Int,
                    Value = id_ingreso,
                };
                SqlCmd.Parameters.Add(Id_ingreso);

                SqlParameter Id_empleado = new SqlParameter
                {
                    ParameterName = "@Id_empleado",
                    SqlDbType = SqlDbType.Int,
                    Value = ingreso.Id_empleado,
                };
                SqlCmd.Parameters.Add(Id_empleado);

                SqlParameter Fecha_ingreso = new SqlParameter
                {
                    ParameterName = "@Fecha_ingreso",
                    SqlDbType = SqlDbType.Date,
                    Value = ingreso.Fecha_ingreso,
                };
                SqlCmd.Parameters.Add(Fecha_ingreso);
                contador += 1;

                SqlParameter Valor_ingreso = new SqlParameter
                {
                    ParameterName = "@Valor_ingreso",
                    SqlDbType = SqlDbType.Decimal,
                    Value = ingreso.Valor_ingreso,
                };
                SqlCmd.Parameters.Add(Valor_ingreso);
                contador += 1;

                SqlParameter Descripcion_ingreso = new SqlParameter
                {
                    ParameterName = "@Descripcion_ingreso",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 200,
                    Value = ingreso.Descripcion_ingreso,
                };
                SqlCmd.Parameters.Add(Descripcion_ingreso);
                contador += 1;

                SqlParameter Estado_ingreso = new SqlParameter
                {
                    ParameterName = "@Estado_ingreso",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 50,
                    Value = ingreso.Estado_ingreso,
                };
                SqlCmd.Parameters.Add(Estado_ingreso);

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

        #region METODO BUSCAR INGRESOS
        public async Task<(string rpta, DataTable dtIngresos)> BuscarIngresos(string tipo_busqueda, string texto_busqueda)
        {
            string rpta = "OK";
            DataTable dtNomina = new DataTable("Ingresos");
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
                    CommandText = "sp_Buscar_ingreso",
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
                    ParameterName = "@Texto_busqueda1",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 50,
                    Value = texto_busqueda.Trim(),
                };
                Sqlcmd.Parameters.Add(Texto_busqueda);

                SqlParameter Texto_busqueda2 = new SqlParameter
                {
                    ParameterName = "@Texto_busqueda2",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 50,
                    Value = ""
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

        public async Task<(string rpta, DataTable dtIngresos)> BuscarIngresos(string tipo_busqueda, string texto_busqueda1, string texto_busqueda2)
        {
            string rpta = "OK";
            DataTable dtNomina = new DataTable("Ingresos");
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
                    CommandText = "sp_Buscar_ingreso",
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

        #endregion
    }
}
