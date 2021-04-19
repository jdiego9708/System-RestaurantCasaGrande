using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using System.Data.SqlClient;

namespace CapaDatos
{
    public class DVenta
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

        #region METODO INSERTAR VENTA
        public string InsertarVenta(List<string> Variables, DataTable detalle_pago,
            out int id_venta)
        {
            id_venta = 0;
            int contador = 0;
            string consulta = "INSERT INTO Ventas VALUES (@Id_pedido, @Fecha_venta, @Hora_venta, @Total_parcial, " +
                "@Propina, @Subtotal, @Descuento, @Bono_cupon, @Desechables, @Domicilio, @Total_final, @Observaciones, @Estado); " +
                "SELECT CAST (SCOPE_IDENTITY() AS int)";
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
                SqlCommand SqlCmd = new SqlCommand
                {
                    Connection = SqlCon,
                    CommandText = consulta,
                    CommandType = CommandType.Text
                };

                SqlParameter Id_venta = new SqlParameter
                {
                    ParameterName = "@Id_venta",
                    SqlDbType = SqlDbType.Int,
                    Direction = ParameterDirection.Output
                };
                SqlCmd.Parameters.Add(Id_venta);

                SqlParameter Id_pedido = new SqlParameter
                {
                    ParameterName = "@Id_pedido",
                    SqlDbType = SqlDbType.Int,
                    Value = Convert.ToInt32(Variables[contador])
                };
                SqlCmd.Parameters.Add(Id_pedido);
                contador += 1;

                SqlParameter Fecha_venta = new SqlParameter
                {
                    ParameterName = "@Fecha_venta",
                    SqlDbType = SqlDbType.Date,
                    Value = DateTime.Now.ToString("yyyy-MM-dd")
                };
                SqlCmd.Parameters.Add(Fecha_venta);

                SqlParameter Hora_venta = new SqlParameter
                {
                    ParameterName = "@Hora_venta",
                    SqlDbType = SqlDbType.Time,
                    Value = DateTime.Now.ToString("HH:mm")
                };
                SqlCmd.Parameters.Add(Hora_venta);

                SqlParameter Total_parcial = new SqlParameter
                {
                    ParameterName = "@Total_parcial",
                    SqlDbType = SqlDbType.Int,
                    Value = Convert.ToInt32(Variables[contador])
                };
                SqlCmd.Parameters.Add(Total_parcial);
                contador += 1;

                SqlParameter Propina = new SqlParameter
                {
                    ParameterName = "@Propina",
                    SqlDbType = SqlDbType.Int,
                    Value = Convert.ToInt32(Variables[contador])
                };
                SqlCmd.Parameters.Add(Propina);
                contador += 1;

                SqlParameter Subtotal = new SqlParameter
                {
                    ParameterName = "@Subtotal",
                    SqlDbType = SqlDbType.Int,
                    Value = Convert.ToInt32(Variables[contador])
                };
                SqlCmd.Parameters.Add(Subtotal);
                contador += 1;

                SqlParameter Descuento = new SqlParameter
                {
                    ParameterName = "@Descuento",
                    SqlDbType = SqlDbType.Int,
                    Value = Convert.ToInt32(Variables[contador])
                };
                SqlCmd.Parameters.Add(Descuento);
                contador += 1;

                SqlParameter Bono_cupon = new SqlParameter
                {
                    ParameterName = "@Bono_cupon",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 50,
                    Value = Variables[contador].Trim()
                };
                SqlCmd.Parameters.Add(Bono_cupon);
                contador += 1;

                SqlParameter Desechables = new SqlParameter
                {
                    ParameterName = "@Desechables",
                    SqlDbType = SqlDbType.Decimal,
                    Value = Convert.ToDecimal(Variables[contador])
                };
                SqlCmd.Parameters.Add(Desechables);
                contador += 1;

                SqlParameter Domicilio = new SqlParameter
                {
                    ParameterName = "@Domicilio",
                    SqlDbType = SqlDbType.Decimal,
                    Value = Convert.ToDecimal(Variables[contador])
                };
                SqlCmd.Parameters.Add(Domicilio);
                contador += 1;

                SqlParameter Total_final = new SqlParameter
                {
                    ParameterName = "@Total_final",
                    SqlDbType = SqlDbType.Int,
                    Value = Convert.ToInt32(Variables[contador])
                };
                SqlCmd.Parameters.Add(Total_final);
                contador += 1;

                SqlParameter Observaciones = new SqlParameter
                {
                    ParameterName = "@Observaciones",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 500,
                    Value = Variables[contador].Trim()
                };
                SqlCmd.Parameters.Add(Observaciones);
                contador += 1;

                SqlParameter Estado = new SqlParameter
                {
                    ParameterName = "@Estado",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 50,
                    Value = "ACTIVO"
                };
                SqlCmd.Parameters.Add(Estado);
                contador += 1;             

                //Ejecutamos nuestro comando
                //Se puede ejecutar este metodo pero ya tenemos el mensaje que devuelve sql
                id_venta = Convert.ToInt32(SqlCmd.ExecuteScalar());
                rpta = id_venta > 0 ? "OK" : "ERROR";
            
                if (rpta.Equals("OK"))
                {
                    if (detalle_pago != null)
                    {
                        StringBuilder query = new StringBuilder();
                        foreach (DataRow row in detalle_pago.Rows)
                        {
                            string pago = Convert.ToString(row["Pago"]);
                            decimal valor_pago = Convert.ToDecimal(row["Valor_pago"]);
                            string vaucher = Convert.ToString(row["Vaucher"]);
                            string observaciones = Convert.ToString(row["Observaciones"]);

                            query.Append("INSERT INTO Detalle_venta VALUES(" + id_venta + ",'" + pago + "'," +
                                valor_pago + ",'" + vaucher + "','" + observaciones + "'); ");
                           
                        }

                        SqlCmd = new SqlCommand
                        {
                            Connection = SqlCon,
                            CommandText = query.ToString(),
                            CommandType = CommandType.Text
                        };
                        rpta = SqlCmd.ExecuteNonQuery() >= 1 ? "OK" : "ERROR";
                    }
                }
                else
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

        #region METODO INACTIVAR VENTA
        public string InactivarVenta(int id_venta)
        {
            string rpta = "";
            string consulta = "UPDATE Ventas SET Estado = 'INACTIVO' WHERE Id_venta = @Id_venta";
           
            SqlConnection SqlCon = new SqlConnection();

            SqlCon.InfoMessage += new SqlInfoMessageEventHandler(SqlCon_InfoMessage);
            SqlCon.FireInfoMessageEventOnUserErrors = true;

            try
            {
                SqlCon.ConnectionString = Conexion.Cn;
                SqlCon.Open();
                SqlCommand SqlCmd = new SqlCommand
                {
                    Connection = SqlCon,
                    CommandText = consulta,
                    CommandType = CommandType.Text
                };

                SqlParameter Id_venta = new SqlParameter
                {
                    ParameterName = "@Id_venta",
                    SqlDbType = SqlDbType.Int,
                    Value = id_venta
                };
                SqlCmd.Parameters.Add(Id_venta);
             
                rpta = SqlCmd.ExecuteNonQuery() >= 1 ? "OK" : "ERROR";

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

        #region METODO BUSCAR VENTA FACTURA FINAL
        public static DataTable BuscarVentaFinal(string texto_busqueda,
            out DataTable dtDetallePedido, out DataTable dtDetalleVenta)
        {
            dtDetallePedido = new DataTable("DetallePedido");
            dtDetalleVenta = new DataTable("DetalleVenta");
            DataTable DtResultado = new DataTable("VentaDatosPrincipales");
            string tipo_busqueda;

            SqlConnection SqlCon = new SqlConnection();
            try
            {
                #region DATOS PRINCIPALES
                /**INICIO BUSCAR PEDIDO DATOS PRINCIPALES**/
                tipo_busqueda = "DATOS VENTA";

                SqlCon.ConnectionString = Conexion.Cn;
                SqlCommand SqlCmdDatosPrincipales = new SqlCommand
                {
                    Connection = SqlCon,
                    CommandText = "sp_Factura_final",
                    CommandType = CommandType.StoredProcedure
                };

                SqlParameter Tipo_busqueda = new SqlParameter
                {
                    ParameterName = "@Tipo_busqueda",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 50,
                    Value = tipo_busqueda.Trim().ToUpper()
                };
                SqlCmdDatosPrincipales.Parameters.Add(Tipo_busqueda);

                SqlParameter Texto_busqueda = new SqlParameter
                {
                    ParameterName = "@Texto_busqueda",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 50,
                    Value = texto_busqueda.Trim().ToUpper()
                };
                SqlCmdDatosPrincipales.Parameters.Add(Texto_busqueda);

                SqlDataAdapter SqlDataPrincipales = new SqlDataAdapter(SqlCmdDatosPrincipales);
                SqlDataPrincipales.Fill(DtResultado);

                if (DtResultado.Rows.Count < 1)
                {
                    DtResultado = null;
                }

                /**FIN BUSCAR PEDIDO DATOS PRINCIPALES**/
                #endregion

                #region DETALLE PEDIDO
                /**INICIO BUSCAR PEDIDO DETALLE**/
                tipo_busqueda = "DETALLE PEDIDO";
                SqlCommand SqlCmdDetallePedido = new SqlCommand
                {
                    Connection = SqlCon,
                    CommandText = "sp_Factura_final",
                    CommandType = CommandType.StoredProcedure
                };

                SqlParameter Tipo_busqueda1 = new SqlParameter
                {
                    ParameterName = "@Tipo_busqueda",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 50,
                    Value = tipo_busqueda.Trim().ToUpper()
                };
                SqlCmdDetallePedido.Parameters.Add(Tipo_busqueda1);

                SqlParameter Texto_busqueda1 = new SqlParameter
                {
                    ParameterName = "@Texto_busqueda",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 50,
                    Value = texto_busqueda.Trim().ToUpper()
                };
                SqlCmdDetallePedido.Parameters.Add(Texto_busqueda1);

                SqlDataAdapter SqlDataDetallePedido = new SqlDataAdapter(SqlCmdDetallePedido);
                SqlDataDetallePedido.Fill(dtDetallePedido);

                if (dtDetallePedido.Rows.Count < 1)
                {
                    dtDetallePedido = null;
                }
                /**FIN BUSCAR DETALLE PEDIDO**/
                #endregion

                #region DETALLE VENTA
                /**INICIO BUSCAR DETALLE VENTA**/
                tipo_busqueda = "DETALLE VENTA";
                SqlCommand SqlCmdDetalleVenta = new SqlCommand
                {
                    Connection = SqlCon,
                    CommandText = "sp_Factura_final",
                    CommandType = CommandType.StoredProcedure
                };

                SqlParameter Tipo_busqueda2 = new SqlParameter
                {
                    ParameterName = "@Tipo_busqueda",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 50,
                    Value = tipo_busqueda.Trim().ToUpper()
                };
                SqlCmdDetalleVenta.Parameters.Add(Tipo_busqueda2);

                SqlParameter Texto_busqueda2 = new SqlParameter
                {
                    ParameterName = "@Texto_busqueda",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 50,
                    Value = texto_busqueda.Trim().ToUpper()
                };
                SqlCmdDetalleVenta.Parameters.Add(Texto_busqueda2);

                SqlDataAdapter SqlDataDetalleVenta = new SqlDataAdapter(SqlCmdDetalleVenta);
                SqlDataDetalleVenta.Fill(dtDetalleVenta);

                if (dtDetalleVenta.Rows.Count < 1)
                {
                    dtDetalleVenta = null;
                }
                /**FIN BUSCAR DETALLE VENTA**/
                #endregion
            }
            catch (SqlException)
            {
                DtResultado = null;
                dtDetalleVenta = null;
                dtDetallePedido = null;
            }
            catch (Exception)
            {
                DtResultado = null;
                dtDetalleVenta = null;
                dtDetallePedido = null;
            }

            return DtResultado;
        }

        #endregion

        #region METODO BUSCAR VENTA
        public static DataTable BuscarVenta(string tipo_busqueda, string texto_busqueda,
            string fecha1, string fecha2, string hora1, string hora2)
        {
            DataTable DtResultado = new DataTable("Venta");
            SqlConnection SqlCon = new SqlConnection();
            try
            {
                #region DATOS

                SqlCon.ConnectionString = Conexion.Cn;
                SqlCommand SqlCmd = new SqlCommand
                {
                    Connection = SqlCon,
                    CommandText = "sp_Buscar_ventas",
                    CommandType = CommandType.StoredProcedure
                };

                SqlParameter Tipo_busqueda = new SqlParameter
                {
                    ParameterName = "@Tipo_busqueda",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 50,
                    Value = tipo_busqueda.Trim().ToUpper()
                };
                SqlCmd.Parameters.Add(Tipo_busqueda);

                SqlParameter Texto_busqueda = new SqlParameter
                {
                    ParameterName = "@Texto_busqueda",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 50,
                    Value = texto_busqueda.Trim().ToUpper()
                };
                SqlCmd.Parameters.Add(Texto_busqueda);

                DateTime FechaInicio = Convert.ToDateTime(fecha1);
                SqlParameter Fecha1 = new SqlParameter
                {
                    ParameterName = "@Fecha1",
                    SqlDbType = SqlDbType.Date,
                    Value = FechaInicio.ToString("yyyy-MM-dd")
                };
                SqlCmd.Parameters.Add(Fecha1);

                DateTime FechaFin = Convert.ToDateTime(fecha2);
                SqlParameter Fecha2 = new SqlParameter
                {
                    ParameterName = "@Fecha2",
                    SqlDbType = SqlDbType.Date,
                    Value = FechaFin.ToString("yyyy-MM-dd")
                };
                SqlCmd.Parameters.Add(Fecha2);

                SqlParameter Hora1 = new SqlParameter
                {
                    ParameterName = "@Hora1",
                    SqlDbType = SqlDbType.Time,
                    Value = hora1
                };
                SqlCmd.Parameters.Add(Hora1);

                SqlParameter Hora2 = new SqlParameter
                {
                    ParameterName = "@Hora2",
                    SqlDbType = SqlDbType.Time,
                    Value = hora2
                };
                SqlCmd.Parameters.Add(Hora2);

                SqlDataAdapter SqlData= new SqlDataAdapter(SqlCmd);
                SqlData.Fill(DtResultado);

                if (DtResultado.Rows.Count < 1)
                {
                    DtResultado = null;
                }
                #endregion
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
