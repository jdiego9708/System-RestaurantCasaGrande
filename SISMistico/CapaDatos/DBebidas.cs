using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.SqlClient;
using System.Data;

namespace CapaDatos
{
    public class DBebidas
    {
        #region MENSAJE DE SQL
        private void SqlCon_InfoMessage(object sender, SqlInfoMessageEventArgs e)
        {
            this.Mensaje_respuesta = e.Message;
        }
        #endregion

        #region CONSTRUCTOR
        public DBebidas()
        {

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

        public List<string> Variables { get => variables; set => variables = value; }

        private List<string> variables;
        #endregion

        #region METODO INSERTAR BEBIDA
        public string InsertarBebida(DBebidas DBebidas, out int id_bebida)
        {
            id_bebida = 0;
            int contador = 1;
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
                    CommandText = "sp_Insertar_bebidas",
                    //Indicamos que es un procedimiento almacenado
                    CommandType = CommandType.StoredProcedure
                };

                SqlParameter Id_bebida = new SqlParameter
                {
                    ParameterName = "@Id_bebida",
                    SqlDbType = SqlDbType.Int,
                    Direction = ParameterDirection.Output
                };
                SqlCmd.Parameters.Add(Id_bebida);

                SqlParameter Nombre_bebida = new SqlParameter
                {
                    ParameterName = "@Nombre_bebida",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 200,
                    Value = DBebidas.Variables[contador].Trim().ToUpper()
                };
                SqlCmd.Parameters.Add(Nombre_bebida);
                contador += 1;

                SqlParameter Descripcion_bebida = new SqlParameter
                {
                    ParameterName = "@Descripcion_bebida",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 500,
                    Value = DBebidas.Variables[contador].Trim().ToUpper()
                };
                SqlCmd.Parameters.Add(Descripcion_bebida);
                contador += 1;

                SqlParameter Precio_bebida = new SqlParameter
                {
                    ParameterName = "@Precio_bebida",
                    SqlDbType = SqlDbType.Decimal,
                    Value = DBebidas.Variables[contador],
                };
                SqlCmd.Parameters.Add(Precio_bebida);
                contador += 1;

                SqlParameter Precio_trago = new SqlParameter
                {
                    ParameterName = "@Precio_trago",
                    SqlDbType = SqlDbType.Decimal,
                    Value = DBebidas.Variables[contador]
                };
                SqlCmd.Parameters.Add(Precio_trago);
                contador += 1;

                SqlParameter Precio_trago_doble = new SqlParameter
                {
                    ParameterName = "@Precio_trago_doble",
                    SqlDbType = SqlDbType.Decimal,
                    Value = DBebidas.Variables[contador]
                };
                SqlCmd.Parameters.Add(Precio_trago_doble);
                contador += 1;

                SqlParameter Precio_proveedor = new SqlParameter
                {
                    ParameterName = "@Precio_proveedor",
                    SqlDbType = SqlDbType.Decimal,
                    Value = DBebidas.Variables[contador]
                };
                SqlCmd.Parameters.Add(Precio_proveedor);
                contador += 1;

                SqlParameter Id_proveedor = new SqlParameter
                {
                    ParameterName = "@Id_proveedor",
                    SqlDbType = SqlDbType.Int,
                    Value = DBebidas.Variables[contador]
                };
                SqlCmd.Parameters.Add(Id_proveedor);
                contador += 1;

                SqlParameter Imagen = new SqlParameter
                {
                    ParameterName = "@Imagen",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 8000,
                    Value = DBebidas.Variables[contador]
                };
                SqlCmd.Parameters.Add(Imagen);
                contador += 1;

                SqlParameter Tipo_bebida = new SqlParameter
                {
                    ParameterName = "@Id_tipo_bebida",
                    SqlDbType = SqlDbType.Int,
                    Value = DBebidas.Variables[contador]
                };
                SqlCmd.Parameters.Add(Tipo_bebida);
                contador += 1;

                SqlParameter Cantidad_unidades = new SqlParameter
                {
                    ParameterName = "@Cantidad_unidades",
                    SqlDbType = SqlDbType.Int,
                    Value = DBebidas.Variables[contador]
                };
                SqlCmd.Parameters.Add(Cantidad_unidades);
                contador += 1;

                SqlParameter Cantidad_x_unidades = new SqlParameter
                {
                    ParameterName = "@Cantidad_x_unidades",
                    SqlDbType = SqlDbType.Int,
                    Value = DBebidas.Variables[contador]
                };
                SqlCmd.Parameters.Add(Cantidad_x_unidades);
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
                else
                {
                    id_bebida = Convert.ToInt32(SqlCmd.Parameters["@Id_bebida"].Value);
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

        #region METODO EDITAR BEBIDA
        public string EditarBebida(DBebidas DBebidas)
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
                    CommandText = "sp_Editar_bebidas",
                    //Indicamos que es un procedimiento almacenado
                    CommandType = CommandType.StoredProcedure
                };

                SqlParameter Id_bebida = new SqlParameter
                {
                    ParameterName = "@Id_bebida",
                    SqlDbType = SqlDbType.Int,
                    Value = DBebidas.Variables[contador].Trim()
                };
                SqlCmd.Parameters.Add(Id_bebida);
                contador += 1;

                SqlParameter Nombre_bebida = new SqlParameter
                {
                    ParameterName = "@Nombre_bebida",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 200,
                    Value = DBebidas.Variables[contador].Trim().ToUpper()
                };
                SqlCmd.Parameters.Add(Nombre_bebida);
                contador += 1;

                SqlParameter Descripcion_bebida = new SqlParameter
                {
                    ParameterName = "@Descripcion_bebida",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 500,
                    Value = DBebidas.Variables[contador].Trim().ToUpper()
                };
                SqlCmd.Parameters.Add(Descripcion_bebida);
                contador += 1;

                SqlParameter Precio_bebida = new SqlParameter
                {
                    ParameterName = "@Precio_bebida",
                    SqlDbType = SqlDbType.Decimal,
                    Value = DBebidas.Variables[contador]
                };
                SqlCmd.Parameters.Add(Precio_bebida);
                contador += 1;

                SqlParameter Precio_trago = new SqlParameter
                {
                    ParameterName = "@Precio_trago",
                    SqlDbType = SqlDbType.Decimal,
                    Value = DBebidas.Variables[contador].Trim().ToUpper()
                };
                SqlCmd.Parameters.Add(Precio_trago);
                contador += 1;

                SqlParameter Precio_trago_doble = new SqlParameter
                {
                    ParameterName = "@Precio_trago_doble",
                    SqlDbType = SqlDbType.Decimal,
                    Value = DBebidas.Variables[contador]
                };
                SqlCmd.Parameters.Add(Precio_trago_doble);
                contador += 1;

                SqlParameter Precio_proveedor = new SqlParameter
                {
                    ParameterName = "@Precio_proveedor",
                    SqlDbType = SqlDbType.Decimal,
                    Value = DBebidas.Variables[contador]
                };
                SqlCmd.Parameters.Add(Precio_proveedor);
                contador += 1;

                SqlParameter Id_proveedor = new SqlParameter
                {
                    ParameterName = "@Id_proveedor",
                    SqlDbType = SqlDbType.Int,
                    Value = DBebidas.Variables[contador].Trim().ToUpper()
                };
                SqlCmd.Parameters.Add(Id_proveedor);
                contador += 1;

                SqlParameter Imagen = new SqlParameter
                {
                    ParameterName = "@Imagen",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 8000,
                    Value = DBebidas.Variables[contador].ToUpper()
                };
                SqlCmd.Parameters.Add(Imagen);
                contador += 1;

                SqlParameter Tipo_bebida = new SqlParameter
                {
                    ParameterName = "@Id_tipo_bebida",
                    SqlDbType = SqlDbType.Int,
                    Value = DBebidas.Variables[contador]
                };
                SqlCmd.Parameters.Add(Tipo_bebida);
                contador += 1;

                SqlParameter Cantidad_unidades = new SqlParameter
                {
                    ParameterName = "@Cantidad_unidades",
                    SqlDbType = SqlDbType.Int,
                    Value = DBebidas.Variables[contador]
                };
                SqlCmd.Parameters.Add(Cantidad_unidades);
                contador += 1;

                SqlParameter Cantidad_x_unidades = new SqlParameter
                {
                    ParameterName = "@Cantidad_x_unidades",
                    SqlDbType = SqlDbType.Int,
                    Value = DBebidas.Variables[contador]
                };
                SqlCmd.Parameters.Add(Cantidad_x_unidades);
                contador += 1;

                //Ejecutamos nuestro comando
                //Se puede ejecutar este metodo pero ya tenemos el mensaje que devuelve sql
                rpta = SqlCmd.ExecuteNonQuery() >= 1 ? "OK" : "NO se Ingreso el Registro";

                if (rpta != "OK")
                {
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

        #region METODO BUSCAR BEBIDAS
        public static DataTable BuscarBebidas(string tipo_busqueda, string texto_busqueda, string estado_bebida,
            out string rpta)
        {
            StringBuilder consulta = new StringBuilder();
            consulta.Append("SELECT * FROM Bebidas be INNER JOIN Tipo_bebidas tpb ON be.Id_tipo_bebida= tpb.Id_tipo_bebida ");

            if (tipo_busqueda.Equals("COMPLETO"))
            {
                consulta.Append("WHERE be.Estado = '" + estado_bebida + "' ");
            }
            else if (tipo_busqueda.Equals("TIPO BEBIDA"))
            {
                consulta.Append("WHERE tpb.Tipo_bebida = '" + texto_busqueda + "' and be.Estado = '" + estado_bebida + "' ");
            }
            else if (tipo_busqueda.Equals("ID BEBIDA"))
            {
                consulta.Append("WHERE be.Id_bebida = '" + texto_busqueda + "' and be.Estado = '" + estado_bebida + "' ");
            }
            else if (tipo_busqueda.Equals("NOMBRE"))
            {
                consulta.Append("WHERE be.Nombre_bebida like '%" + texto_busqueda + "%' and be.Estado = '" + estado_bebida + "' ");
            }
            else if (tipo_busqueda.Equals("ID TIPO BEBIDA"))
            {
                consulta.Append("WHERE tpb.Id_tipo_bebida = '" + texto_busqueda + "' and be.Estado = '" + estado_bebida + "' ");
            }

            consulta.Append("ORDER BY Id_bebida DESC");
            return Conexion.EjecutarConsultaDt(Convert.ToString(consulta), out rpta);
        }

        #endregion

        #region METODO BUSCAR TIPO BEBIDAS
        public static DataTable BuscarTipoBebidas(string tipo_busqueda, string texto_busqueda)
        {
            DataTable DtResultado = new DataTable("TipoBebidas");
            SqlConnection SqlCon = new SqlConnection();
            try
            {
                SqlCon.ConnectionString = Conexion.Cn;
                SqlCommand Sqlcmd = new SqlCommand
                {
                    Connection = SqlCon,
                    CommandText = "sp_Buscar_tipo_bebidas",
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
            catch (SqlException ex)
            {
                DErrores DErrores = DErrores.GetInstancia();
                DErrores.NewError("Error of application SqlException", "DBebidas",
                    "BuscarBebidas", ex.Message, Convert.ToString(DateTime.Now));
                DtResultado = null;
            }
            catch (Exception ex)
            {
                DErrores DErrores = DErrores.GetInstancia();
                DErrores.NewError("Error of application Exception", "DBebidas",
                    "BuscarBebidas", ex.Message, Convert.ToString(DateTime.Now));
                DtResultado = null;
            }
            return DtResultado;
        }

        #endregion

        #region METODO INACTIVAR BEBIDAS
        public string InactivarBebidas(int id_bebida)
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
                    CommandText = "sp_Inactivar_bebidas",
                    //Indicamos que es un procedimiento almacenado
                    CommandType = CommandType.StoredProcedure
                };

                SqlParameter Id_bebida = new SqlParameter
                {
                    ParameterName = "@Id_bebida",
                    SqlDbType = SqlDbType.Int,
                    Value = id_bebida
                };
                SqlCmd.Parameters.Add(Id_bebida);

                //Ejecutamos nuestro comando
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
    }
}
