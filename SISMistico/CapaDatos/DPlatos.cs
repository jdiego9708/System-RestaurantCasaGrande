using System;
using System.Collections.Generic;

using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace CapaDatos
{
    public class DPlatos
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
        public DPlatos() { }
        #endregion

        #region METODO INSERTAR PLATOS
        public string InsertarPlatos(DPlatos DPlatos, out int id_plato)
        {
            id_plato = 0;
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
                    CommandText = "sp_Insertar_plato",
                    //Indicamos que es un procedimiento almacenado
                    CommandType = CommandType.StoredProcedure
                };

                SqlParameter Id_plato = new SqlParameter
                {
                    ParameterName = "@Id_plato",
                    SqlDbType = SqlDbType.Int,
                    Direction = ParameterDirection.Output
                };
                SqlCmd.Parameters.Add(Id_plato);

                SqlParameter Nombre_plato = new SqlParameter
                {
                    ParameterName = "@Nombre_plato",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 200,
                    Value = DPlatos.Variables[contador].Trim().ToUpper()
                };
                SqlCmd.Parameters.Add(Nombre_plato);
                contador += 1;

                SqlParameter Id_tipo_plato = new SqlParameter
                {
                    ParameterName = "@Id_tipo_plato",
                    SqlDbType = SqlDbType.Int,
                    Value = DPlatos.Variables[contador]
                };
                SqlCmd.Parameters.Add(Id_tipo_plato);
                contador += 1;

                SqlParameter Precio_plato = new SqlParameter
                {
                    ParameterName = "@Precio_plato",
                    SqlDbType = SqlDbType.Int,
                    Value = DPlatos.Variables[contador]
                };
                SqlCmd.Parameters.Add(Precio_plato);
                contador += 1;

                SqlParameter Imagen = new SqlParameter
                {
                    ParameterName = "@Imagen_plato",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 8000,
                    Value = DPlatos.Variables[contador].Trim()
                };
                SqlCmd.Parameters.Add(Imagen);
                contador += 1;

                SqlParameter Descripcion_plato = new SqlParameter
                {
                    ParameterName = "@Descripcion_plato",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 500,
                    Value = DPlatos.Variables[contador].Trim()
                };
                SqlCmd.Parameters.Add(Descripcion_plato);
                contador += 1;

                SqlParameter Estado_plato = new SqlParameter
                {
                    ParameterName = "@Estado_plato",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 50,
                    Value = DPlatos.Variables[contador].Trim()
                };
                SqlCmd.Parameters.Add(Estado_plato);
                contador += 1;

                SqlParameter Plato_detallado = new SqlParameter
                {
                    ParameterName = "@Plato_detallado",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 50,
                    Value = DPlatos.Variables[contador].Trim()
                };
                SqlCmd.Parameters.Add(Plato_detallado);
                contador += 1;

                SqlParameter Plato_carta = new SqlParameter
                {
                    ParameterName = "@Plato_carta",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 50,
                    Value = DPlatos.Variables[contador].Trim()
                };
                SqlCmd.Parameters.Add(Plato_carta);

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
                    id_plato = Convert.ToInt32(SqlCmd.Parameters["@Id_plato"].Value);
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

        #region METODO EDITAR PLATOS
        public string EditarPlatos(DPlatos DPlatos)
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
                    CommandText = "sp_Editar_platos",
                    //Indicamos que es un procedimiento almacenado
                    CommandType = CommandType.StoredProcedure
                };

                int contador = 0;
                SqlParameter Id_plato = new SqlParameter
                {
                    ParameterName = "@Id_plato",
                    SqlDbType = SqlDbType.Int,
                    Value = DPlatos.Variables[contador]
                };
                SqlCmd.Parameters.Add(Id_plato);
                contador += 1;

                SqlParameter Nombre_plato = new SqlParameter
                {
                    ParameterName = "@Nombre_plato",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 200,
                    Value = DPlatos.Variables[contador].Trim().ToUpper()
                };
                SqlCmd.Parameters.Add(Nombre_plato);
                contador += 1;

                SqlParameter Id_tipo_plato = new SqlParameter
                {
                    ParameterName = "@Id_tipo_plato",
                    SqlDbType = SqlDbType.Int,
                    Value = DPlatos.Variables[contador]
                };
                SqlCmd.Parameters.Add(Id_tipo_plato);
                contador += 1;

                SqlParameter Precio_plato = new SqlParameter
                {
                    ParameterName = "@Precio_plato",
                    SqlDbType = SqlDbType.Decimal,
                    Value = DPlatos.Variables[contador]
                };
                SqlCmd.Parameters.Add(Precio_plato);
                contador += 1;

                SqlParameter Imagen = new SqlParameter
                {
                    ParameterName = "@Imagen_plato",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 8000,
                    Value = DPlatos.Variables[contador].Trim()
                };
                SqlCmd.Parameters.Add(Imagen);
                contador += 1;

                SqlParameter Descripcion_plato = new SqlParameter
                {
                    ParameterName = "@Descripcion_plato",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 50,
                    Value = DPlatos.Variables[contador].Trim()
                };
                SqlCmd.Parameters.Add(Descripcion_plato);
                contador += 1;

                SqlParameter Estado_plato = new SqlParameter
                {
                    ParameterName = "@Estado_plato",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 50,
                    Value = DPlatos.Variables[contador].Trim()
                };
                SqlCmd.Parameters.Add(Estado_plato);
                contador += 1;

                SqlParameter Plato_detallado = new SqlParameter
                {
                    ParameterName = "@Plato_detallado",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 50,
                    Value = DPlatos.Variables[contador].Trim()
                };
                SqlCmd.Parameters.Add(Plato_detallado);

                SqlParameter Plato_carta = new SqlParameter
                {
                    ParameterName = "@Plato_carta",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 50,
                    Value = DPlatos.Variables[contador].Trim()
                };
                SqlCmd.Parameters.Add(Plato_carta);

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

        #region METODO BUSCAR PLATOS
        public static DataTable BuscarPlatos(string tipo_busqueda, string texto_busqueda, 
            string estado_plato, out string rpta)
        {
            StringBuilder consulta = new StringBuilder();
            consulta.Append("SELECT * FROM Platos pl " +
                "INNER JOIN Tipo_platos tpl ON pl.Id_tipo_plato = tpl.Id_tipo_plato ");

            if (tipo_busqueda.Equals("COMPLETO"))
            {
                consulta.Append("WHERE pl.Estado = '" + estado_plato + "' ");
            }
            else if (tipo_busqueda.Equals("TIPO DE PLATO"))
            {
                consulta.Append("WHERE tpl.Tipo_plato = '" + texto_busqueda + "' and pl.Estado = '" + estado_plato + "' ");
            }
            else if (tipo_busqueda.Equals("ID PLATO"))
            {
                consulta.Append("WHERE pl.Id_plato = '" + texto_busqueda + "' and pl.Estado = '" + estado_plato + "' ");
            }
            else if (tipo_busqueda.Equals("NOMBRE"))
            {
                consulta.Append("WHERE pl.Nombre_plato like '%" + texto_busqueda + "%' and pl.Estado = '" + estado_plato + "' ");
            }
            else if (tipo_busqueda.Equals("ID TIPO PLATO"))
            {
                consulta.Append("WHERE tpl.Id_tipo_plato = " + texto_busqueda + " and pl.Estado = '" + estado_plato + "' ");
            }

            consulta.Append("ORDER BY Id_plato DESC");
            return Conexion.EjecutarConsultaDt(Convert.ToString(consulta), out rpta);
        }

        #endregion

        #region METODO BUSCAR TIPO PLATOS
        public static DataTable BuscarTipoPlatos(string tipo_busqueda, string texto_busqueda)
        {
            DataTable DtResultado = new DataTable("TipoPlatos");
            try
            {
                SqlConnection SqlCon = new SqlConnection
                {
                    ConnectionString = Conexion.Cn
                };
                SqlCommand Sqlcmd = new SqlCommand
                {
                    Connection = SqlCon,
                    CommandText = "sp_Buscar_tipo_platos",
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

        #region METODO INSERTAR DETALLE PLATOS
        public string InsertarDetallePlatos(List<string> Variables)
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
                    CommandText = "sp_Insertar_detalle_plato",
                    //Indicamos que es un procedimiento almacenado
                    CommandType = CommandType.StoredProcedure
                };

                SqlParameter Id_detalle = new SqlParameter
                {
                    ParameterName = "@Id_detalle",
                    SqlDbType = SqlDbType.Int,
                    Direction = ParameterDirection.Output
                };
                SqlCmd.Parameters.Add(Id_detalle);

                SqlParameter Id_plato = new SqlParameter
                {
                    ParameterName = "@Id_plato",
                    SqlDbType = SqlDbType.Int,
                    Value = Variables[contador]
                };
                SqlCmd.Parameters.Add(Id_plato);
                contador += 1;

                SqlParameter Id_insumo = new SqlParameter
                {
                    ParameterName = "@Id_insumo",
                    SqlDbType = SqlDbType.Int,
                    Value = Variables[contador]
                };
                SqlCmd.Parameters.Add(Id_insumo);
                contador += 1;

                SqlParameter Cantidad_insumo = new SqlParameter
                {
                    ParameterName = "@Cantidad_insumo",
                    SqlDbType = SqlDbType.Int,
                    Value = Variables[contador]
                };
                SqlCmd.Parameters.Add(Cantidad_insumo);
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

        #region METODO EDITAR DETALLE PLATOS
        public string EditarDetallePlatos(List<string> Variables)
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
                    CommandText = "sp_Editar_detalle_plato",
                    //Indicamos que es un procedimiento almacenado
                    CommandType = CommandType.StoredProcedure
                };

                SqlParameter Id_detalle = new SqlParameter
                {
                    ParameterName = "@Id_detalle",
                    SqlDbType = SqlDbType.Int,
                    Value = Variables[contador]
                };
                SqlCmd.Parameters.Add(Id_detalle);
                contador += 1;

                SqlParameter Id_plato = new SqlParameter
                {
                    ParameterName = "@Id_plato",
                    SqlDbType = SqlDbType.Int,
                    Value = Variables[contador]
                };
                SqlCmd.Parameters.Add(Id_plato);
                contador += 1;

                SqlParameter Id_insumo = new SqlParameter
                {
                    ParameterName = "@Id_insumo",
                    SqlDbType = SqlDbType.Int,
                    Value = Variables[contador]
                };
                SqlCmd.Parameters.Add(Id_insumo);
                contador += 1;

                SqlParameter Cantidad_insumo = new SqlParameter
                {
                    ParameterName = "@Cantidad_insumo",
                    SqlDbType = SqlDbType.Int,
                    Value = Variables[contador]
                };
                SqlCmd.Parameters.Add(Cantidad_insumo);
                contador += 1;

                SqlParameter Tipo_update = new SqlParameter
                {
                    ParameterName = "@Tipo_update",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 50,
                    Value = Variables[contador]
                };
                SqlCmd.Parameters.Add(Tipo_update);
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

        #region METODO BUSCAR DETALLE PLATOS
        public static DataTable BuscarDetallePlatos(string tipo_busqueda, string texto_busqueda)
        {
            DataTable DtResultado = new DataTable("DetallePlatos");
            try
            {
                SqlConnection SqlCon = new SqlConnection
                {
                    ConnectionString = Conexion.Cn
                };
                SqlCommand Sqlcmd = new SqlCommand
                {
                    Connection = SqlCon,
                    CommandText = "sp_Buscar_detalle_platos",
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

        #region METODO INACTIVAR PLATOS
        public string InactivarPlato(int id_plato)
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
                    CommandText = "sp_Inactivar_plato",
                    //Indicamos que es un procedimiento almacenado
                    CommandType = CommandType.StoredProcedure
                };

                SqlParameter Id_plato = new SqlParameter
                {
                    ParameterName = "@Id_plato",
                    SqlDbType = SqlDbType.Int,
                    Value = id_plato
                };
                SqlCmd.Parameters.Add(Id_plato);

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

        #region METODO ACTUALIZAR ACOMPAÑANTE
        public string ActualizarAcompanante(int id_ingrediente, string nombre)
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
                    CommandText = "sp_Actualizar_acompanante",
                    //Indicamos que es un procedimiento almacenado
                    CommandType = CommandType.StoredProcedure
                };

                SqlParameter Id_ingrediente = new SqlParameter
                {
                    ParameterName = "@Id_ingrediente",
                    SqlDbType = SqlDbType.Int,
                    Value = id_ingrediente
                };
                SqlCmd.Parameters.Add(Id_ingrediente);

                SqlParameter Nombre_ingrediente = new SqlParameter
                {
                    ParameterName = "@Nombre_ingrediente",
                    SqlDbType = SqlDbType.VarChar,
                    Value = nombre
                };
                SqlCmd.Parameters.Add(Nombre_ingrediente);

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
