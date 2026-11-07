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
        // Consulta la venta exacta y sus pagos, sin mezclar otras ventas del mismo pedido.
        public static DataSet GetSaleDetails(int saleId)
        {
            const string query = @"
DECLARE @OrderId int = (SELECT Id_pedido FROM Ventas WHERE Id_venta = @SaleId);
SELECT v.*, p.Tipo_pedido, p.Estado_pedido, p.Fecha_pedido, p.Hora_pedido,
       p.Observaciones_pedido, p.CantidadClientes, p.Id_cliente, p.Id_empleado, p.Id_mesa,
       m.Num_mesa, c.Nombre_cliente, c.Telefono_cliente, c.Correo_electronico,
       e.Nombre_empleado, p.Id_direccion,
       CASE WHEN a.Id_direccion IS NOT NULL THEN a.Direccion ELSE c.Direccion_cliente END AS Direccion_entrega,
       CASE WHEN a.Id_direccion IS NOT NULL THEN a.Referencia ELSE c.Referencia_ubicacion END AS Referencia_entrega,
       a.Alias AS Alias_direccion, a.Barrio,
       COALESCE(NULLIF(a.Telefono_contacto, ''), c.Telefono_cliente) AS Telefono_entrega
FROM Ventas v
INNER JOIN Pedidos p ON p.Id_pedido = v.Id_pedido
LEFT JOIN Clientes c ON c.Id_cliente = p.Id_cliente
LEFT JOIN Empleados e ON e.Id_empleado = p.Id_empleado
LEFT JOIN Mesas m ON m.Id_mesa = p.Id_mesa
LEFT JOIN Domicilios a ON a.Id_direccion = p.Id_direccion
WHERE v.Id_venta = @SaleId;
SELECT d.Id_detalle_pedido, d.Tipo,
       COALESCE(pl.Nombre_plato, b.Nombre_bebida, 'Producto sin nombre') AS Nombre,
       d.Precio, d.Cantidad, d.Precio * d.Cantidad AS Total, d.Observaciones
FROM Detalle_pedido d
LEFT JOIN Platos pl ON d.Tipo = 'PLATO' AND pl.Id_plato = d.Id_tipo
LEFT JOIN Bebidas b ON d.Tipo = 'BEBIDA' AND b.Id_bebida = d.Id_tipo
WHERE d.Id_pedido = @OrderId ORDER BY d.Id_detalle_pedido;
SELECT Metodo_pago, Valor_pago, Vaucher, Observaciones
FROM Detalle_venta WHERE Id_venta = @SaleId;
SELECT di.Id_detalle_pedido, di.Id_tipo, pl.Nombre_plato AS Plato, i.Nombre_ingrediente, di.Observaciones
FROM Detalle_ingredientes_pedido di
LEFT JOIN Ingredientes i ON i.Id_ingrediente = di.Id_ingrediente
LEFT JOIN Platos pl ON pl.Id_plato = di.Id_tipo
WHERE di.Id_pedido = @OrderId ORDER BY di.Id_detalle_pedido, di.Id_detalle_ingrediente_pedido;";
            using (SqlConnection connection = new SqlConnection(Conexion.Cn))
            using (SqlCommand command = new SqlCommand(query, connection))
            using (SqlDataAdapter adapter = new SqlDataAdapter(command))
            {
                command.Parameters.Add("@SaleId", SqlDbType.Int).Value = saleId;
                DataSet result = new DataSet();
                adapter.Fill(result);
                string[] names = { "Sale", "Products", "Payments", "Ingredients" };
                for (int i = 0; i < result.Tables.Count; i++) result.Tables[i].TableName = names[i];
                return result;
            }
        }

        // Congela importes JSON numericos desde las mismas filas SQL que se acaban de guardar.
        // Evita depender de coma/punto de la interfaz y mantiene el redondeo real de los pagos.
        private static string FreezeElectronicRequest(string json, int saleId, SqlConnection connection, SqlTransaction transaction)
        {
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer { MaxJsonLength=8*1024*1024 };
            var snapshot = serializer.DeserializeObject(json) as Dictionary<string,object>;
            if(snapshot==null)throw new InvalidOperationException("Solicitud electronica invalida.");
            using(var command=new SqlCommand(@"SELECT Id_pedido,Total_parcial,Propina,Subtotal,Descuento,Bono_cupon,Desechables,Domicilio,Total_final,Observaciones FROM dbo.Ventas WHERE Id_venta=@sale;
SELECT Metodo_pago,Valor_pago,Vaucher,Observaciones FROM dbo.Detalle_venta WHERE Id_venta=@sale;",connection,transaction))
            {
                command.Parameters.Add("@sale",SqlDbType.Int).Value=saleId;
                var data=new DataSet();
                using(var adapter=new SqlDataAdapter(command))adapter.Fill(data);
                if(data.Tables[0].Rows.Count!=1 || data.Tables[1].Rows.Count==0)
                    throw new InvalidOperationException("Falta la venta o sus pagos para la solicitud electronica.");
                var sale=data.Tables[0].Rows[0];
                if(Convert.ToInt32(snapshot["orderId"])!=Convert.ToInt32(sale["Id_pedido"]))
                    throw new InvalidOperationException("La solicitud electronica no corresponde al pedido vendido.");
                snapshot["saleValues"]=sale.ItemArray.Select(value=>value==DBNull.Value ? null : value).ToArray();
                var payments=new List<Dictionary<string,object>>();
                decimal paid=0;
                foreach(DataRow row in data.Tables[1].Rows)
                {
                    paid+=Convert.ToDecimal(row["Valor_pago"]);
                    payments.Add(new Dictionary<string,object> {
                        {"Pago",Convert.ToString(row["Metodo_pago"])}, {"Valor_pago",Convert.ToInt32(row["Valor_pago"])},
                        {"Vaucher",Convert.ToString(row["Vaucher"])}, {"Observaciones",row.IsNull("Observaciones") ? null : row["Observaciones"]}
                    });
                }
                if(paid!=Convert.ToDecimal(sale["Total_final"]))
                    throw new InvalidOperationException("La suma de pagos guardados no coincide con el total final. Revise los pagos antes de terminar.");
                snapshot["payments"]=payments;
                snapshot["version"]=2;
                return serializer.Serialize(snapshot);
            }
        }

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
            out int id_venta, string electronicRequestJson = null)
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
            // La venta y sus pagos van en una sola transacción: antes, si fallaba el insert de los pagos, la
            // venta ya quedaba guardada sin pagos y cada reintento creaba otra (el informe la duplicaba).
            SqlTransaction transaction = null;
            //Capturador de errores
            try
            {
                SqlCon.ConnectionString = Conexion.Cn;
                SqlCon.Open();
                transaction = SqlCon.BeginTransaction();

                // Un pedido que ya tiene una venta ACTIVA con sus pagos no se factura otra vez (doble clic o
                // reintento). Las ventas huérfanas, sin pagos, no cuentan: no bloquean la nueva factura.
                using (SqlCommand check = new SqlCommand(
                    "SELECT COUNT(*) FROM Ventas v WHERE v.Id_pedido = @Id_pedido AND v.Estado = 'ACTIVO' " +
                    "AND EXISTS (SELECT 1 FROM Detalle_venta d WHERE d.Id_venta = v.Id_venta)", SqlCon, transaction))
                {
                    check.Parameters.Add("@Id_pedido", SqlDbType.Int).Value = Convert.ToInt32(Variables[0]);
                    if (Convert.ToInt32(check.ExecuteScalar()) > 0)
                    {
                        transaction.Rollback();
                        return "El pedido " + Variables[0] + " ya fue facturado.";
                    }
                }

                SqlCommand SqlCmd = new SqlCommand
                {
                    Connection = SqlCon,
                    Transaction = transaction,
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
                        int inserted = 0;
                        foreach (DataRow row in detalle_pago.Rows)
                        {
                            // Con parámetros: antes el valor se concatenaba con la cultura del equipo (es-CO da
                            // "123200,00" con coma) y un descuento que dejaba decimales rompía el INSERT.
                            // Valor_pago es int en la tabla, así que se redondea igual que Total_final.
                            using (SqlCommand pay = new SqlCommand(
                                "INSERT INTO Detalle_venta VALUES (@Id_venta, @Metodo_pago, @Valor_pago, @Vaucher, @Observaciones)",
                                SqlCon, transaction))
                            {
                                pay.Parameters.Add("@Id_venta", SqlDbType.Int).Value = id_venta;
                                pay.Parameters.Add("@Metodo_pago", SqlDbType.VarChar, 50).Value = Convert.ToString(row["Pago"]);
                                pay.Parameters.Add("@Valor_pago", SqlDbType.Int).Value =
                                    Convert.ToInt32(Math.Round(Convert.ToDecimal(row["Valor_pago"])));
                                pay.Parameters.Add("@Vaucher", SqlDbType.VarChar, 50).Value = Convert.ToString(row["Vaucher"]);
                                pay.Parameters.Add("@Observaciones", SqlDbType.VarChar, 150).Value = Convert.ToString(row["Observaciones"]);
                                inserted += pay.ExecuteNonQuery();
                            }
                        }
                        rpta = inserted >= 1 ? "OK" : "ERROR";
                    }

                    if (rpta.Equals("OK"))
                    {
                        if (electronicRequestJson != null)
                        {
                            electronicRequestJson = FreezeElectronicRequest(electronicRequestJson, id_venta, SqlCon, transaction);
                            using (SqlCommand request = new SqlCommand(
                                "INSERT dbo.AlegraInvoiceRequests (SaleId,CompanyKey,RequestJson) VALUES (@sale,'default',@json)", SqlCon, transaction))
                            {
                                request.Parameters.Add("@sale", SqlDbType.Int).Value = id_venta;
                                request.Parameters.Add("@json", SqlDbType.NVarChar, -1).Value = electronicRequestJson;
                                request.ExecuteNonQuery();
                            }
                        }
                        transaction.Commit();
                    }
                    else
                    {
                        transaction.Rollback();
                        id_venta = 0;
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
                id_venta = 0;
                try { transaction?.Rollback(); } catch (Exception) { }
            }
            catch (Exception ex)
            {
                rpta = ex.Message;
                id_venta = 0;
                try { transaction?.Rollback(); } catch (Exception) { }
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

        // Consultas por lotes limitadas a las ventas visibles; evita una consulta por fila.
        private static void AddElectronicInvoiceColumns(DataTable sales, SqlConnection connection)
        {
            sales.Columns.Add("Factura_electronica", typeof(string));
            sales.Columns.Add("Numero_factura_electronica", typeof(string));
            sales.Columns.Add("Estado_factura_electronica", typeof(string));
            if (!sales.Columns.Contains("Id_venta")) return;
            var rowsBySale = new Dictionary<int, List<DataRow>>();
            foreach (DataRow row in sales.Rows)
            {
                row["Factura_electronica"] = "No";
                row["Numero_factura_electronica"] = "";
                row["Estado_factura_electronica"] = "";
                int saleId = Convert.ToInt32(row["Id_venta"]);
                if (!rowsBySale.ContainsKey(saleId)) rowsBySale[saleId] = new List<DataRow>();
                rowsBySale[saleId].Add(row);
            }
            var ids = rowsBySale.Keys.ToList();
            for (int start = 0; start < ids.Count; start += 500)
            {
                using (var command = connection.CreateCommand())
                {
                    var parameters = new List<string>();
                    foreach (int id in ids.Skip(start).Take(500))
                    {
                        string name = "@sale" + parameters.Count;
                        parameters.Add(name);
                        command.Parameters.Add(name, SqlDbType.Int).Value = id;
                    }
                    command.CommandText = "SELECT SaleId, InvoiceNumber, Status FROM dbo.AlegraInvoiceRequests WHERE SaleId IN ("
                        + string.Join(",", parameters) + ")";
                    var requests = new DataTable();
                    using (var adapter = new SqlDataAdapter(command)) adapter.Fill(requests);
                    foreach (DataRow request in requests.Rows)
                    {
                        string status;
                        switch (Convert.ToString(request["Status"]))
                        {
                            case "ACCEPTED": status = "Emitida"; break;
                            case "REJECTED": status = "Rechazada"; break;
                            case "ISSUING": status = "En emisi\u00f3n"; break;
                            case "CREATING": status = "En creaci\u00f3n"; break;
                            case "CREATED": status = "Pendiente de emisi\u00f3n"; break;
                            case "PENDING": status = "Pendiente"; break;
                            default: status = "Requiere revisi\u00f3n"; break;
                        }
                        foreach (DataRow row in rowsBySale[Convert.ToInt32(request["SaleId"])])
                        {
                            row["Factura_electronica"] = "S\u00ed";
                            row["Numero_factura_electronica"] = Convert.ToString(request["InvoiceNumber"]);
                            row["Estado_factura_electronica"] = status;
                        }
                    }
                }
            }
        }

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

                // Se agrega al final para conservar las columnas que consumen las pantallas existentes.
                // Una sola consulta obtiene el tipo real de todos los pedidos de la lista.
                if (DtResultado.Rows.Count > 0 && DtResultado.Columns.Contains("Id_pedido")
                    && DtResultado.Columns.Contains("Num_mesa") && !DtResultado.Columns.Contains("Tipo_pedido"))
                {
                    using (SqlCommand typesCommand = SqlCon.CreateCommand())
                    {
                        List<string> parameters = new List<string>();
                        HashSet<int> orderIds = new HashSet<int>();
                        foreach (DataRow row in DtResultado.Rows)
                            orderIds.Add(Convert.ToInt32(row["Id_pedido"]));
                        foreach (int orderId in orderIds)
                        {
                            string parameter = "@order" + parameters.Count;
                            parameters.Add(parameter);
                            typesCommand.Parameters.Add(parameter, SqlDbType.Int).Value = orderId;
                        }
                        typesCommand.CommandText = "SELECT Id_pedido, Tipo_pedido FROM Pedidos WHERE Id_pedido IN ("
                            + string.Join(",", parameters) + ")";
                        DataTable orderTypes = new DataTable();
                        using (SqlDataAdapter adapter = new SqlDataAdapter(typesCommand))
                            adapter.Fill(orderTypes);
                        Dictionary<int, string> types = new Dictionary<int, string>();
                        foreach (DataRow row in orderTypes.Rows)
                            types[Convert.ToInt32(row["Id_pedido"])] = Convert.ToString(row["Tipo_pedido"]);
                        DtResultado.Columns.Add("Tipo_pedido", typeof(string));
                        foreach (DataRow row in DtResultado.Rows)
                        {
                            if (types.TryGetValue(Convert.ToInt32(row["Id_pedido"]), out string orderType))
                                row["Tipo_pedido"] = orderType;
                        }
                    }
                }

                AddElectronicInvoiceColumns(DtResultado, SqlCon);

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
