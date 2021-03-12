using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using System.Configuration;
using System.IO;
using System.Net.Mail;
using System.Net.Mime;
using System.Windows.Forms;
using CapaNegocio;

namespace CapaPresentacion.Formularios.FormsPedido
{
    public class EmailFactura
    {
        private static string concatTemplateEmailWithHeaderBody(string header, string body)
        {
            try
            {
                body = header + body;

                //HTMLTemplate = HTMLTemplate.Replace(@"#_HEADER_MAIL", header);
                //HTMLTemplate = HTMLTemplate.Replace(@"#_BODY_MAIL", body);
            }
            catch (Exception)
            {
                return body;
            }
            return body;
        }

        public static string SendEmailFactura(int id_pedido, string correo)
        {
            string rpta = "OK";
            try
            {
                string HTMLTemplateMail = "";
                DataTable dtDetallePedido;
                DataTable dtDetalleVenta;
                DataTable dtDatosPrincipales = NVentas.BuscarVentaFinal(Convert.ToString(id_pedido),
                out dtDetallePedido, out dtDetalleVenta);

                //Solo si dtDatosPrincipales es diferente de null crearemos y enviaremos el correo
                if (dtDatosPrincipales != null)
                {
                    int id_venta = Convert.ToInt32(dtDatosPrincipales.Rows[0]["Id_venta"]);
                    int num_mesa = Convert.ToInt32(dtDatosPrincipales.Rows[0]["Num_mesa"]);
                    string nombre_empleado = Convert.ToString(dtDatosPrincipales.Rows[0]["Nombre_empleado"]);
                    string nombre_cliente = Convert.ToString(dtDatosPrincipales.Rows[0]["Nombre_cliente"]);
                    string fecha_venta = Convert.ToString(dtDatosPrincipales.Rows[0]["Fecha_venta"]);
                    DateTime fecha_venta_convertida = Convert.ToDateTime(fecha_venta);
                    string hora_venta = Convert.ToString(dtDatosPrincipales.Rows[0]["Hora_venta"]);
                    DateTime hora_venta_convertida = Convert.ToDateTime(hora_venta);
                    int total_parcial = Convert.ToInt32(dtDatosPrincipales.Rows[0]["Total_parcial"]);
                    int propina = Convert.ToInt32(dtDatosPrincipales.Rows[0]["Propina"]);
                    int subtotal = Convert.ToInt32(dtDatosPrincipales.Rows[0]["Subtotal"]);
                    int descuento = Convert.ToInt32(dtDatosPrincipales.Rows[0]["Descuento"]);
                    string bono_cupon = Convert.ToString(dtDatosPrincipales.Rows[0]["Bono_cupon"]);
                    int total_final = Convert.ToInt32(dtDatosPrincipales.Rows[0]["Total_final"]);
                    string observaciones = Convert.ToString(dtDatosPrincipales.Rows[0]["Observaciones"]);

                    StringBuilder headerEmail = new StringBuilder();
                    StringBuilder contentEmail = new StringBuilder();

                    headerEmail.Append("<h2>");
                    headerEmail.Append("FACTURA TIO PEPE - FECHA: " + DateTime.Now.ToLongDateString() + " HORA: " + DateTime.Now.ToLongTimeString());
                    headerEmail.Append("</h2>");

                    contentEmail.Append("<table style='font-family: arial; font-size: 12px; border: 1px solid silver;' cellpadding='7' cellspacing='7'>");
                    contentEmail.Append("<tbody>");

                    contentEmail.Append("<tr>");
                    contentEmail.Append("   <td  style='border: 1px solid silver;text-align:center;'>Pedido número</td>");
                    contentEmail.Append("   <td  style='border: 1px solid silver;text-align:center;'>" + id_pedido + "</td>");
                    contentEmail.Append("</tr>");
                    contentEmail.Append("<tr>");
                    contentEmail.Append("   <td  style='border: 1px solid silver;text-align:center;'>Venta número</td>");
                    contentEmail.Append("   <td  style='border: 1px solid silver;text-align:center;'>" + id_venta + "</td>");
                    contentEmail.Append("</tr>");
                    contentEmail.Append("<tr>");
                    contentEmail.Append("   <td  style='border: 1px solid silver;text-align:center;'>Fecha y hora</td>");
                    contentEmail.Append("   <td  style='border: 1px solid silver;text-align:center;'>" + fecha_venta_convertida.ToShortDateString() + " - " + hora_venta_convertida.ToShortTimeString() + "</td>");
                    contentEmail.Append("</tr>");
                    contentEmail.Append("<tr>");
                    contentEmail.Append("   <td  style='border: 1px solid silver;text-align:center;'>Mesa</td>");
                    contentEmail.Append("   <td  style='border: 1px solid silver;text-align:center;'>" + num_mesa + "</td>");
                    contentEmail.Append("</tr>");
                    contentEmail.Append("<tr>");
                    contentEmail.Append("   <td  style='border: 1px solid silver;text-align:center;'>Empleado</td>");
                    contentEmail.Append("   <td  style='border: 1px solid silver;text-align:center;'>" + nombre_empleado + "</td>");
                    contentEmail.Append("</tr>");
                    contentEmail.Append("<tr>");
                    contentEmail.Append("   <td  style='border: 1px solid silver;text-align:center;'>Cliente</td>");
                    contentEmail.Append("   <td  style='border: 1px solid silver;text-align:center;'>" + nombre_cliente + "</td>");
                    contentEmail.Append("</tr>");
                    contentEmail.Append("<tr>");
                    contentEmail.Append("   <td  style='border: 1px solid silver;text-align:center;'>Total parcial</td>");
                    contentEmail.Append("   <td  style='border: 1px solid silver;text-align:center;'>" + total_parcial + "</td>");
                    contentEmail.Append("</tr>");
                    contentEmail.Append("<tr>");
                    contentEmail.Append("   <td  style='border: 1px solid silver;text-align:center;'>Propina</td>");
                    contentEmail.Append("   <td  style='border: 1px solid silver;text-align:center;'>" + propina + "</td>");
                    contentEmail.Append("</tr>");
                    contentEmail.Append("<tr>");
                    contentEmail.Append("   <td  style='border: 1px solid silver;text-align:center;'>Subtotal</td>");
                    contentEmail.Append("   <td  style='border: 1px solid silver;text-align:center;'>" + subtotal + "</td>");
                    contentEmail.Append("</tr>");
                    contentEmail.Append("<tr>");
                    contentEmail.Append("   <td  style='border: 1px solid silver;text-align:center;'>Descuento</td>");
                    contentEmail.Append("   <td  style='border: 1px solid silver;text-align:center;'>" + descuento + "</td>");
                    contentEmail.Append("</tr>");
                    contentEmail.Append("<tr>");
                    contentEmail.Append("   <td  style='border: 1px solid silver;text-align:center;'>TOTAL</td>");
                    contentEmail.Append("   <td  style='border: 1px solid silver;text-align:center;'>" + total_final + "</td>");
                    contentEmail.Append("</tr>");

                    contentEmail.Append("</tbody>");
                    contentEmail.Append("</table>");

                    contentEmail.Append("<table style='font-family: arial; font-size: 12px; border: 1px solid silver;' cellpadding='7' cellspacing='7'>");
                    contentEmail.Append("<tbody>");
                    contentEmail.Append("<tr>");
                    contentEmail.Append("   <td  style='border: 1px solid silver;text-align:center;'>Observaciones</td>");
                    contentEmail.Append("   <td  style='border: 1px solid silver;text-align:center;'>" + observaciones + "</td>");
                    contentEmail.Append("</tr>");
                    contentEmail.Append("</tbody>");
                    contentEmail.Append("</table>");

                    contentEmail.Append("<br />");
                    contentEmail.Append("<br />");

                    contentEmail.Append("<h4>");
                    contentEmail.Append("Detalle de pago");
                    contentEmail.Append("</h4>");

                    contentEmail.Append("<br />");
                    contentEmail.Append("<br />");

                    if (dtDetalleVenta != null)
                    {
                        contentEmail.Append("<table style='font-family: arial; font-size: 12px; border: 1px solid silver;' cellpadding='7' cellspacing='7'>");
                        contentEmail.Append("<tbody>");
                        contentEmail.Append("<tr>");
                        contentEmail.Append("   <td  style='border: 1px solid silver;text-align:center;'>Método de pago</td>");
                        contentEmail.Append("   <td  style='border: 1px solid silver;text-align:center;'>Valor</td>");
                        contentEmail.Append("   <td  style='border: 1px solid silver;text-align:center;'>Observaciones</td>");
                        contentEmail.Append("</tr>");

                        foreach (DataRow row in dtDetalleVenta.Rows)
                        {
                            contentEmail.Append("<tr>");
                            contentEmail.Append("   <td  style='border: 1px solid silver;text-align:center;'>" + row["Metodo_pago"] + "</td>");
                            contentEmail.Append("   <td  style='border: 1px solid silver;text-align:center;'>" + row["Valor_pago"] + "</td>");
                            contentEmail.Append("   <td  style='border: 1px solid silver;text-align:center;'>" + row["Observaciones"] + "</td>");
                            contentEmail.Append("</tr>");
                        }
                        contentEmail.Append("</tbody>");
                        contentEmail.Append("</table>");
                    }

                    contentEmail.Append("<br />");
                    contentEmail.Append("<br />");

                    contentEmail.Append("<h4>");
                    contentEmail.Append("Detalle del pedido");
                    contentEmail.Append("</h4>");

                    contentEmail.Append("<br />");
                    contentEmail.Append("<br />");

                    if (dtDetallePedido != null)
                    {
                        contentEmail.Append("<table style='font-family: arial; font-size: 12px; border: 1px solid silver;' cellpadding='7' cellspacing='7'>");
                        contentEmail.Append("<tbody>");
                        contentEmail.Append("<tr>");
                        contentEmail.Append("   <td  style='border: 1px solid silver;text-align:center;'>Nombre</td>");
                        contentEmail.Append("   <td  style='border: 1px solid silver;text-align:center;'>Precio</td>");
                        contentEmail.Append("   <td  style='border: 1px solid silver;text-align:center;'>Cantidad</td>");
                        contentEmail.Append("   <td  style='border: 1px solid silver;text-align:center;'>Total</td>");
                        contentEmail.Append("</tr>");

                        foreach (DataRow row in dtDetallePedido.Rows)
                        {
                            contentEmail.Append("<tr>");
                            contentEmail.Append("   <td  style='border: 1px solid silver;text-align:center;'>" + row["Nombre"] + "</td>");
                            contentEmail.Append("   <td  style='border: 1px solid silver;text-align:center;'>" + row["Precio"] + "</td>");
                            contentEmail.Append("   <td  style='border: 1px solid silver;text-align:center;'>" + row["Cantidad"] + "</td>");
                            contentEmail.Append("   <td  style='border: 1px solid silver;text-align:center;'>" + row["Total"] + "</td>");
                            contentEmail.Append("</tr>");
                        }
                        contentEmail.Append("</tbody>");
                        contentEmail.Append("</table>");
                    }
                    contentEmail.Append("<br />");
                    contentEmail.Append("<br />");

                    contentEmail.Append("<p>");
                    contentEmail.Append("Por disposición de la superintendencia de industria y comercio se informa que en este establecimiento ");
                    contentEmail.Append("la propina sugerida al consumidor corresponde al 10% sobre el total de la cuenta antes de impuestos ");
                    contentEmail.Append("la cual podrá ser aceptado, rechazado o modificado por usted de acuerdo con su valoración del servicio prestado");
                    contentEmail.Append("Autorización resolución DIAN 18762002374004 del 2017//02//27");
                    contentEmail.Append("</p>");

                    contentEmail.Append("<p>Juan Diego Duque - jdiego9708@gmail.com - Copyright 2021</p>");

                    HTMLTemplateMail = concatTemplateEmailWithHeaderBody(headerEmail.ToString(), contentEmail.ToString());

                    MailMessage mail = new MailMessage(ConfigurationManager.AppSettings["eMailFrom"], correo);
                    mail.From = new MailAddress(ConfigurationManager.AppSettings["eMailFrom"], "Restaurante Tio Pepe", System.Text.Encoding.UTF8);
                    mail.IsBodyHtml = true;
                    string fecha = DateTime.Now.ToString("G");
                    mail.Subject = "Factura TIO PEPE" + " - " + fecha;
                    mail.Body = HTMLTemplateMail;
                    //Línea para enviar una copia del correo
                    //mail.Bcc.Add(ConfigurationManager.AppSettings["eMail"].ToString());

                    SmtpClient client = new SmtpClient(ConfigurationManager.AppSettings["eMailSMTP"].ToString());
                    client.Credentials = new System.Net.NetworkCredential(ConfigurationManager.AppSettings["eMailFrom"], ConfigurationManager.AppSettings["eMailPass"]);
                    client.Port = int.Parse(ConfigurationManager.AppSettings["portEmail"].ToString());
                    client.EnableSsl = true;
                    client.Send(mail);
                }
            }
            catch (Exception ex)
            {
                rpta = ex.Message;
            }

            return rpta;
        }
    }
}
