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
    public class DTurnos
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
        public DTurnos() { }
        #endregion

        #region METODO INSERTAR TURNO
        public Task<string> InsertarTurno(Turno turno)
        {
            string rpta = string.Empty;
            SqlConnection SqlCon = new SqlConnection(Conexion.Cn);
            try
            {
                SqlCon.InfoMessage += new SqlInfoMessageEventHandler(SqlCon_InfoMessage);
                SqlCon.FireInfoMessageEventOnUserErrors = true;
                SqlCon.Open();

                SqlCommand SqlCmd = new SqlCommand()
                {
                    Connection = SqlCon,
                    CommandText = "sp_Turnos_i",
                    CommandType = CommandType.StoredProcedure
                };

                SqlParameter Id_turno = new SqlParameter()
                {
                    ParameterName = "@Id_turno",
                    SqlDbType = SqlDbType.Int,
                    Direction = ParameterDirection.Output,
                };
                SqlCmd.Parameters.Add(Id_turno);

                SqlParameter Fecha_turno = new SqlParameter()
                {
                    ParameterName = "@Fecha_turno",
                    SqlDbType = SqlDbType.Date,
                    Value = turno.Fecha_turno,
                };
                SqlCmd.Parameters.Add(Fecha_turno);

                SqlParameter Hora_turno = new SqlParameter()
                {
                    ParameterName = "@Hora_turno",
                    SqlDbType = SqlDbType.Time,
                    Value = turno.Hora_turno,
                };
                SqlCmd.Parameters.Add(Hora_turno);

                SqlParameter Valor_inicial = new SqlParameter()
                {
                    ParameterName = "@Valor_inicial",
                    SqlDbType = SqlDbType.Decimal,
                    Value = turno.Valor_inicial,
                };
                SqlCmd.Parameters.Add(Valor_inicial);

                SqlParameter Total_ingresos = new SqlParameter()
                {
                    ParameterName = "@Total_ingresos",
                    SqlDbType = SqlDbType.Decimal,
                    Value = turno.Total_ingresos,
                };
                SqlCmd.Parameters.Add(Total_ingresos);

                SqlParameter Total_egresos = new SqlParameter()
                {
                    ParameterName = "@Total_egresos",
                    SqlDbType = SqlDbType.Decimal,
                    Value = turno.Total_egresos,
                };
                SqlCmd.Parameters.Add(Total_egresos);

                SqlParameter Total_ventas = new SqlParameter()
                {
                    ParameterName = "@Total_ventas",
                    SqlDbType = SqlDbType.Decimal,
                    Value = turno.Total_ventas,
                };
                SqlCmd.Parameters.Add(Total_ventas);

                SqlParameter Total_nomina = new SqlParameter()
                {
                    ParameterName = "@Total_nomina",
                    SqlDbType = SqlDbType.Decimal,
                    Value = turno.Total_nomina,
                };
                SqlCmd.Parameters.Add(Total_nomina);

                SqlParameter @Total_turno = new SqlParameter()
                {
                    ParameterName = "@Total_turno",
                    SqlDbType = SqlDbType.Decimal,
                    Value = turno.Total_turno,
                };
                SqlCmd.Parameters.Add(Total_turno);

                SqlParameter Estado_turno = new SqlParameter()
                {
                    ParameterName = "@Estado_turno",
                    SqlDbType = SqlDbType.VarChar,
                    Value = turno.Estado_turno,
                };
                SqlCmd.Parameters.Add(Estado_turno);

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
                    turno.Id_turno = Convert.ToInt32(SqlCmd.Parameters["@Id_turno"].Value);
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
            return Task.FromResult(rpta);
        }
        #endregion

        #region METODO BUSCAR TURNOS
        public Task<(string rpta, DataTable dtTurnos)> BuscarTurnos(string tipo_busqueda, string texto_busqueda1,
            string texto_busqueda2)
        {
            string rpta = "OK";
            DataTable dtTurnos = new DataTable("Turnos");
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
                    CommandText = "sp_Buscar_turnos",
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
                SqlData.Fill(dtTurnos);
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

            return Task.FromResult((rpta, dtTurnos));
        }

        #endregion

        #region METODO CAMBIAR BASE
        public Task<string> InsertarBase(int id_turno, decimal base_nueva)
        {
            string rpta = string.Empty;
            SqlConnection SqlCon = new SqlConnection(Conexion.Cn);
            try
            {
                SqlCon.InfoMessage += new SqlInfoMessageEventHandler(SqlCon_InfoMessage);
                SqlCon.FireInfoMessageEventOnUserErrors = true;
                SqlCon.Open();

                SqlCommand SqlCmd = new SqlCommand()
                {
                    Connection = SqlCon,
                    CommandText = "sp_Base_turno_u",
                    CommandType = CommandType.StoredProcedure
                };

                SqlParameter Id_turno = new SqlParameter()
                {
                    ParameterName = "@Id_turno",
                    SqlDbType = SqlDbType.Int,
                    Value = id_turno,
                };
                SqlCmd.Parameters.Add(Id_turno);

                SqlParameter Base_nueva = new SqlParameter()
                {
                    ParameterName = "@Base_nueva",
                    SqlDbType = SqlDbType.Decimal,
                    Value = base_nueva,
                };
                SqlCmd.Parameters.Add(Base_nueva);

                rpta = SqlCmd.ExecuteNonQuery() >= 1 ? "OK" : "NO";

                if (rpta != "OK")
                {
                    if (this.Mensaje_respuesta != null)
                    {
                        rpta = this.Mensaje_respuesta;
                    }
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
            return Task.FromResult(rpta);
        }
        #endregion

        #region METODO EDITAR ESTADO TURNO
        public Task<string> EditarEstadoTurno(int id_turno, string estado)
        {
            string rpta = string.Empty;
            SqlConnection SqlCon = new SqlConnection(Conexion.Cn);
            try
            {
                SqlCon.InfoMessage += new SqlInfoMessageEventHandler(SqlCon_InfoMessage);
                SqlCon.FireInfoMessageEventOnUserErrors = true;
                SqlCon.Open();

                SqlCommand SqlCmd = new SqlCommand()
                {
                    Connection = SqlCon,
                    CommandText = "sp_Estado_turno_u",
                    CommandType = CommandType.StoredProcedure
                };

                SqlParameter Id_turno = new SqlParameter()
                {
                    ParameterName = "@Id_turno",
                    SqlDbType = SqlDbType.Int,
                    Value = id_turno,
                };
                SqlCmd.Parameters.Add(Id_turno);

                SqlParameter Estado_turno = new SqlParameter()
                {
                    ParameterName = "@Estado_turno",
                    SqlDbType = SqlDbType.VarChar,
                    Value = estado,
                };
                SqlCmd.Parameters.Add(Estado_turno);

                rpta = SqlCmd.ExecuteNonQuery() >= 1 ? "OK" : "NO";

                if (rpta != "OK")
                {
                    if (this.Mensaje_respuesta != null)
                    {
                        rpta = this.Mensaje_respuesta;
                    }
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
            return Task.FromResult(rpta);
        }
        #endregion
    }
}
