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
    public class DNomina
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
        public DNomina() { }
        #endregion

        #region METODO INSERTAR NOMINA
        public async Task<(string rpta, int id_empleado)> InsertarNomina(EmpleadoNominaBinding empleadoNomina)
        {
            int contador = 0;
            int id_nomina = 0;
            string consulta = "INSERT INTO Nomina_empleado VALUES (@Id_empleado, @Fecha_nomina, @Turno, " +
                "@Servicios, @Platos, @Otros_ingresos, @Egresos, @Total_nomina, @Estado_nomina, @Observaciones) " +
                "SET @Id_nomina_empleado = SCOPE_IDENTITY(); ";

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

                SqlParameter Id_nomina_empleado = new SqlParameter
                {
                    ParameterName = "@Id_nomina_empleado",
                    SqlDbType = SqlDbType.Int,
                    Direction = ParameterDirection.Output,
                };
                SqlCmd.Parameters.Add(Id_nomina_empleado);

                SqlParameter Id_empleado = new SqlParameter
                {
                    ParameterName = "@Id_empleado",
                    SqlDbType = SqlDbType.Int,
                    Value = empleadoNomina.Id_empleado,
                };
                SqlCmd.Parameters.Add(Id_empleado);

                SqlParameter Fecha_nomina = new SqlParameter
                {
                    ParameterName = "@Fecha_nomina",
                    SqlDbType = SqlDbType.Date,
                    Value = empleadoNomina.Fecha_nomina,
                };
                SqlCmd.Parameters.Add(Fecha_nomina);
                contador += 1;

                SqlParameter Turno = new SqlParameter
                {
                    ParameterName = "@Turno",
                    SqlDbType = SqlDbType.Decimal,
                    Value = empleadoNomina.Turno,
                };
                SqlCmd.Parameters.Add(Turno);
                contador += 1;

                SqlParameter Servicios = new SqlParameter
                {
                    ParameterName = "@Servicios",
                    SqlDbType = SqlDbType.Decimal,
                    Value = empleadoNomina.Servicios,
                };
                SqlCmd.Parameters.Add(Servicios);
                contador += 1;

                SqlParameter Platos = new SqlParameter
                {
                    ParameterName = "@Platos",
                    SqlDbType = SqlDbType.Decimal,
                    Value = empleadoNomina.Platos,
                };
                SqlCmd.Parameters.Add(Platos);
                contador += 1;

                SqlParameter Otros_ingresos = new SqlParameter
                {
                    ParameterName = "@Otros_ingresos",
                    SqlDbType = SqlDbType.Decimal,
                    Value = empleadoNomina.Otros_ingresos,
                };
                SqlCmd.Parameters.Add(Otros_ingresos);
                contador += 1;

                SqlParameter Egresos = new SqlParameter
                {
                    ParameterName = "@Egresos",
                    SqlDbType = SqlDbType.Decimal,
                    Value = empleadoNomina.Egresos,
                };
                SqlCmd.Parameters.Add(Egresos);
                contador += 1;

                SqlParameter Total_nomina = new SqlParameter
                {
                    ParameterName = "@Total_nomina",
                    SqlDbType = SqlDbType.Decimal,
                    Value = empleadoNomina.Total_nomina,
                };
                SqlCmd.Parameters.Add(Total_nomina);
                contador += 1;
                
                SqlParameter Estado_nomina = new SqlParameter
                {
                    ParameterName = "@Estado_nomina",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 50,
                    Value = empleadoNomina.Estado_nomina,
                };
                SqlCmd.Parameters.Add(Estado_nomina);

                SqlParameter Observaciones = new SqlParameter
                {
                    ParameterName = "@Observaciones",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 50,
                    Value = empleadoNomina.Observaciones,
                };
                SqlCmd.Parameters.Add(Observaciones);

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
                    id_nomina = Convert.ToInt32(SqlCmd.Parameters["@Id_nomina_empleado"].Value);
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
            return (rpta, id_nomina);
        }
        #endregion

        #region METODO EDITAR NOMINA
        public async Task<string> EditarNomina(int id_nomina_empleado, EmpleadoNominaBinding empleadoNomina)
        {
            int contador = 0;
            string consulta = "UPDATE Nomina_empleado SET " +
                "Id_empleado = @Id_empleado, " +
                "Fecha_nomina = @Fecha_nomina, " +
                "Salario = @Salario, " +
                "Propinas = @Propinas, " +
                "Otros_ingresos = @Otros_ingresos, " +
                "Egresos = @Egresos, " +
                "Total_nomina = @Total_nomina, " +
                "Estado_nomina = @Estado_nomina, " +
                "Observaciones = @Observaciones " +
                "WHERE Id_nomina_empleado = @Id_nomina_empleado ";

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

                SqlParameter Id_nomina_empleado = new SqlParameter
                {
                    ParameterName = "@Id_nomina_empleado",
                    SqlDbType = SqlDbType.Int,
                    Value = id_nomina_empleado,
                };
                SqlCmd.Parameters.Add(Id_nomina_empleado);

                SqlParameter Id_empleado = new SqlParameter
                {
                    ParameterName = "@Id_empleado",
                    SqlDbType = SqlDbType.Int,
                    Value = empleadoNomina.Id_empleado,
                };
                SqlCmd.Parameters.Add(Id_empleado);

                SqlParameter Fecha_nomina = new SqlParameter
                {
                    ParameterName = "@Fecha_nomina",
                    SqlDbType = SqlDbType.Date,
                    Value = empleadoNomina.Fecha_nomina,
                };
                SqlCmd.Parameters.Add(Fecha_nomina);
                contador += 1;

                SqlParameter Salario = new SqlParameter
                {
                    ParameterName = "@Salario",
                    SqlDbType = SqlDbType.Decimal,
                    Value = empleadoNomina.Turno,
                };
                SqlCmd.Parameters.Add(Salario);
                contador += 1;

                SqlParameter Propinas = new SqlParameter
                {
                    ParameterName = "@Propinas",
                    SqlDbType = SqlDbType.Decimal,
                    Value = empleadoNomina.Servicios,
                };
                SqlCmd.Parameters.Add(Propinas);
                contador += 1;

                SqlParameter Otros_ingresos = new SqlParameter
                {
                    ParameterName = "@Otros_ingresos",
                    SqlDbType = SqlDbType.Decimal,
                    Value = empleadoNomina.Turno,
                };
                SqlCmd.Parameters.Add(Otros_ingresos);
                contador += 1;

                SqlParameter Egresos = new SqlParameter
                {
                    ParameterName = "@Egresos",
                    SqlDbType = SqlDbType.Decimal,
                    Value = empleadoNomina.Egresos,
                };
                SqlCmd.Parameters.Add(Egresos);
                contador += 1;

                SqlParameter Total_nomina = new SqlParameter
                {
                    ParameterName = "@Total_nomina",
                    SqlDbType = SqlDbType.Decimal,
                    Value = empleadoNomina.Total_nomina,
                };
                SqlCmd.Parameters.Add(Total_nomina);
                contador += 1;

                SqlParameter Estado_nomina = new SqlParameter
                {
                    ParameterName = "@Estado_nomina",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 50,
                    Value = empleadoNomina.Estado_nomina,
                };
                SqlCmd.Parameters.Add(Estado_nomina);

                SqlParameter Observaciones = new SqlParameter
                {
                    ParameterName = "@Observaciones",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 50,
                    Value = empleadoNomina.Observaciones,
                };
                SqlCmd.Parameters.Add(Observaciones);

                //Ejecutamos nuestro comando
                //Se puede ejecutar este metodo pero ya tenemos el mensaje que devuelve sql
                rpta = SqlCmd.ExecuteNonQuery() >= 1 ? "OK" : "NO se actualizó el registro";

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

        #region METODO BUSCAR NOMINA
        public DataTable BuscarNomina(string tipo_busqueda, string texto_busqueda, out string rpta)
        {
            rpta = "OK";
            DataTable dtNomina = new DataTable("NominaEmpleado");
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
                    CommandText = "sp_Buscar_nomina",
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

                SqlParameter Fecha = new SqlParameter
                {
                    ParameterName = "@Fecha",
                    SqlDbType = SqlDbType.Date,
                    Value = DateTime.Now.ToString("yyyy-MM-dd"),
                };
                Sqlcmd.Parameters.Add(Fecha);

                SqlDataAdapter SqlData = new SqlDataAdapter(Sqlcmd);
                SqlData.Fill(dtNomina);                
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

            return dtNomina;
        }

        public DataTable BuscarNomina(string tipo_busqueda, string texto_busqueda1,
            string texto_busqueda2, out string rpta)
        {
            rpta = "OK";
            DataTable dtNomina = new DataTable("NominaEmpleado");
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
                    CommandText = "sp_Buscar_nomina",
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

                SqlParameter Fecha = new SqlParameter
                {
                    ParameterName = "@Fecha",
                    SqlDbType = SqlDbType.Date,
                    Value = DateTime.Now.ToString("yyyy-MM-dd"),
                };
                Sqlcmd.Parameters.Add(Fecha);

                SqlDataAdapter SqlData = new SqlDataAdapter(Sqlcmd);
                SqlData.Fill(dtNomina);
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

            return dtNomina;
        }
        #endregion

        #region METODO ESTADÍSTICAS DIARIAS
        public async Task<(string rpta, DataTable dtEstadistica, DataTable dtDetalle, DataTable dtPagos)> BuscarEstadistica(int id_turno, string fecha)
        {
            string rpta = "OK";
            DataSet dsResultados = new DataSet();
            DataTable dtNomina = new DataTable("EstadisticaDiaria");
            DataTable dtDetalles = new DataTable("Detalles");
            DataTable dtPagos = new DataTable("Pagos");
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
                    CommandText = "sp_Estadisticas_diarias",
                    CommandType = CommandType.StoredProcedure,
                };

                SqlParameter Id_turno = new SqlParameter
                {
                    ParameterName = "@Id_turno",
                    SqlDbType = SqlDbType.Int,
                    Value = id_turno,
                };
                Sqlcmd.Parameters.Add(Id_turno);

                SqlParameter Fecha = new SqlParameter
                {
                    ParameterName = "@Fecha",
                    SqlDbType = SqlDbType.Date,
                    Value = fecha.Trim(),
                };
                Sqlcmd.Parameters.Add(Fecha);

                SqlDataAdapter SqlData = new SqlDataAdapter(Sqlcmd);
                SqlData.Fill(dsResultados);

                if (dsResultados.Tables.Count < 0)
                {
                    dtDetalles = null;
                    dtNomina = null;
                }
                else
                {
                    dtNomina = dsResultados.Tables[0];
                    dtDetalles = dsResultados.Tables[1];
                    dtPagos = dsResultados.Tables[2];
                }
            }
            catch (SqlException ex)
            {
                dtDetalles = null;
                dtNomina = null;
                dtPagos = null;
                rpta = ex.Message;
            }
            catch (Exception ex)
            {
                dtDetalles = null;
                dtNomina = null;
                dtPagos = null;
                rpta = ex.Message;
            }
            finally
            {
                if (SqlCon.State == ConnectionState.Open)
                    SqlCon.Close();
            }

            return (rpta, dtNomina, dtDetalles, dtPagos);
        }

        public async Task<(string rpta, DataTable dtEstadistica, DataTable dtDetalle, DataTable dtPagos)> BuscarEstadistica(string fecha1, string fecha2)
        {
            string rpta = "OK";
            DataSet dsResultados = new DataSet();
            DataTable dtNomina = new DataTable("EstadisticaDiaria");
            DataTable dtDetalles = new DataTable("Detalles");
            DataTable dtPagos = new DataTable("Pagos");
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
                    CommandText = "sp_Estadisticas_fechas",
                    CommandType = CommandType.StoredProcedure,
                };

                SqlParameter Fecha1 = new SqlParameter
                {
                    ParameterName = "@Fecha1",
                    SqlDbType = SqlDbType.Date,
                    Value = fecha1,
                };
                Sqlcmd.Parameters.Add(Fecha1);

                SqlParameter Fecha2 = new SqlParameter
                {
                    ParameterName = "@Fecha2",
                    SqlDbType = SqlDbType.Date,
                    Value = fecha2.Trim(),
                };
                Sqlcmd.Parameters.Add(Fecha2);

                SqlDataAdapter SqlData = new SqlDataAdapter(Sqlcmd);
                SqlData.Fill(dsResultados);

                if (dsResultados.Tables.Count < 0)
                {
                    dtDetalles = null;
                    dtNomina = null;
                }
                else
                {
                    dtNomina = dsResultados.Tables[0];
                    dtDetalles = dsResultados.Tables[1];
                    dtPagos = dsResultados.Tables[2];
                }
            }
            catch (SqlException ex)
            {
                dtDetalles = null;
                dtNomina = null;
                dtPagos = null;
                rpta = ex.Message;
            }
            catch (Exception ex)
            {
                dtDetalles = null;
                dtNomina = null;
                dtPagos = null;
                rpta = ex.Message;
            }
            finally
            {
                if (SqlCon.State == ConnectionState.Open)
                    SqlCon.Close();
            }

            return (rpta, dtNomina, dtDetalles, dtPagos);
        }

        #endregion
    }
}
