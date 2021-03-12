using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using System.Data.SqlClient;

namespace CapaDatos
{
    public class DInsumos
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
        public DInsumos() { }
        #endregion

        #region METODO INSERTAR INSUMOS
        public string InsertarInsumos(List<string> Variables)
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
                    CommandText = "sp_Insertar_insumos",
                    //Indicamos que es un procedimiento almacenado
                    CommandType = CommandType.StoredProcedure
                };

                SqlParameter Id_insumo = new SqlParameter
                {
                    ParameterName = "@Id_insumo",
                    SqlDbType = SqlDbType.Int,
                    Direction = ParameterDirection.Output
                };
                SqlCmd.Parameters.Add(Id_insumo);

                SqlParameter Nombre_insumo = new SqlParameter
                {
                    ParameterName = "@Nombre_insumo",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 50,
                    Value = Variables[contador].Trim().ToUpper()
                };
                SqlCmd.Parameters.Add(Nombre_insumo);
                contador += 1;

                SqlParameter Id_tipo_insumo = new SqlParameter
                {
                    ParameterName = "@Id_tipo_insumo",
                    SqlDbType = SqlDbType.Int,
                    Value = Variables[contador]
                };
                SqlCmd.Parameters.Add(Id_tipo_insumo);
                contador += 1;

                SqlParameter Cantidad_existente = new SqlParameter
                {
                    ParameterName = "@Cantidad_existente",
                    SqlDbType = SqlDbType.Int,
                    Value = Variables[contador]
                };
                SqlCmd.Parameters.Add(Cantidad_existente);
                contador += 1;

                SqlParameter Tipo_medida = new SqlParameter
                {
                    ParameterName = "@Tipo_medida",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 50,
                    Value = Variables[contador].Trim().ToUpper()
                };
                SqlCmd.Parameters.Add(Tipo_medida);
                contador += 1;

                SqlParameter Observaciones = new SqlParameter
                {
                    ParameterName = "@Observaciones",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 500,
                    Value = Variables[contador].Trim().ToUpper()
                };
                SqlCmd.Parameters.Add(Observaciones);
                contador += 1;

                SqlParameter Cantidad_minima = new SqlParameter
                {
                    ParameterName = "@Cantidad_minima",
                    SqlDbType = SqlDbType.Int,
                    Value = Variables[contador]
                };
                SqlCmd.Parameters.Add(Cantidad_minima);
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

        #region METODO EDITAR INSUMOS
        public string EditarInsumos(List<string> Variables)
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
                    CommandText = "sp_Editar_insumos",
                    //Indicamos que es un procedimiento almacenado
                    CommandType = CommandType.StoredProcedure
                };

                SqlParameter Id_insumo = new SqlParameter
                {
                    ParameterName = "@Id_insumo",
                    SqlDbType = SqlDbType.Int,
                    Value = Variables[contador]
                };
                SqlCmd.Parameters.Add(Id_insumo);
                contador += 1;

                SqlParameter Nombre_insumo = new SqlParameter
                {
                    ParameterName = "@Nombre_insumo",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 50,
                    Value = Variables[contador].Trim().ToUpper()
                };
                SqlCmd.Parameters.Add(Nombre_insumo);
                contador += 1;

                SqlParameter Id_tipo_insumo = new SqlParameter
                {
                    ParameterName = "@Id_tipo_insumo",
                    SqlDbType = SqlDbType.Int,
                    Value = Variables[contador]
                };
                SqlCmd.Parameters.Add(Id_tipo_insumo);
                contador += 1;

                SqlParameter Cantidad_existente = new SqlParameter
                {
                    ParameterName = "@Cantidad_existente",
                    SqlDbType = SqlDbType.Int,
                    Value = Variables[contador]
                };
                SqlCmd.Parameters.Add(Cantidad_existente);
                contador += 1;

                SqlParameter Tipo_medida = new SqlParameter
                {
                    ParameterName = "@Tipo_medida",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 50,
                    Value = Variables[contador].Trim().ToUpper()
                };
                SqlCmd.Parameters.Add(Tipo_medida);
                contador += 1;

                SqlParameter Observaciones = new SqlParameter
                {
                    ParameterName = "@Observaciones",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 500,
                    Value = Variables[contador].Trim().ToUpper()
                };
                SqlCmd.Parameters.Add(Observaciones);
                contador += 1;

                SqlParameter Cantidad_minima = new SqlParameter
                {
                    ParameterName = "@Cantidad_minima",
                    SqlDbType = SqlDbType.Int,
                    Value = Variables[contador]
                };
                SqlCmd.Parameters.Add(Cantidad_minima);
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

        #region METODO BUSCAR INSUMOS
        public static DataTable BuscarInsumos(string tipo_busqueda, string texto_busqueda, out string rpta)
        {
            StringBuilder consulta = new StringBuilder();
            consulta.Append("SELECT * FROM Insumos ins INNER JOIN Tipo_insumos tipo " +
                "ON ins.Id_tipo_insumo = tipo.Id_tipo_insumo ");

            if (tipo_busqueda.Equals("NOMBRE"))
            {
                consulta.Append("WHERE ins.Nombre_insumo like '%" + texto_busqueda + "%' ");
            }
            else if (tipo_busqueda.Equals("TIPO"))
            {
                consulta.Append("WHERE tipo.Tipo_insumo = '" + texto_busqueda + "%' ");
            }
            else if (tipo_busqueda.Equals("ID"))
            {
                consulta.Append("WHERE ins.Id_insumo = '" + texto_busqueda + "' ");
            }

            consulta.Append("ORDER BY Id_insumo DESC");
            return Conexion.EjecutarConsultaDt(Convert.ToString(consulta), out rpta);
        }

        #endregion

        #region METODO BUSCAR TIPO INSUMOS
        public static DataTable BuscarTipoInsumos(string tipo_busqueda, string texto_busqueda)
        {
            DataTable DtResultado = new DataTable("Tipo_insumos");
            SqlConnection SqlCon = new SqlConnection();
            try
            {
                SqlCon.ConnectionString = Conexion.Cn;
                SqlCommand Sqlcmd = new SqlCommand
                {
                    Connection = SqlCon,
                    CommandText = "sp_Buscar_tipo_insumos",
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
