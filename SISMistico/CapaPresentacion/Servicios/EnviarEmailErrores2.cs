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

namespace CapaPresentacion
{
    public class EnviarEmailErrores2
    {
        public static string concatTemplateEmailWithHeaderBody(string header, string body)
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

        public static string SendEmailError(string detalle_error, string metodo, 
            string formulario, string informacion, DataTable dtDetalles)
        {
            string rpta = "OK";
            try
            {
                //Este codigo pone la plantilla en el string HTMLTemplate

                string HTMLTemplateMail = ""; //utilitiesService.templateEmail();

                //if (HTMLTemplateMail == null)
                //{
                //    throw new Exception("No se envió el correo, no se encontró la plantilla");
                //}

                StringBuilder headerEmail = new StringBuilder();
                StringBuilder contentEmail = new StringBuilder();

                headerEmail.Append("<h2>");
                headerEmail.Append("TICKET SOFTWARE MÍSTICO - FECHA: " + DateTime.Now.ToLongDateString() + " HORA: " + DateTime.Now.ToLongTimeString());
                headerEmail.Append("</h2>");

                contentEmail.Append("<h3>");
                contentEmail.Append("Información resumida: ");
                contentEmail.Append("</h3>");

                contentEmail.Append("<p>");
                contentEmail.Append(informacion);
                contentEmail.Append("</p>");

                contentEmail.Append("<h4>");
                contentEmail.Append("Formulario del error:");
                contentEmail.Append("</h4>");

                contentEmail.Append("<p>");
                contentEmail.Append(formulario);
                contentEmail.Append("</p>");

                contentEmail.Append("<h4>");
                contentEmail.Append("Método o evento donde se detectó el error:");
                contentEmail.Append("</h4>");

                headerEmail.Append("<p>");
                headerEmail.Append(metodo);
                headerEmail.Append("</p>");

                contentEmail.Append("<h3>");
                contentEmail.Append("Detalles completos del error: ");
                contentEmail.Append("</h3>");

                contentEmail.Append("<p>");
                contentEmail.Append(detalle_error);
                contentEmail.Append("</p>");

                for (int i = 0; i <= dtDetalles.Rows.Count - 1; i++)
                {
                    string rutaOrigen = Convert.ToString(dtDetalles.Rows[i]["Ruta_origen"]);
                    contentEmail.Append("<h3>Paso " + i +"</h3>");
                    contentEmail.Append("<p>Descripción: " + Convert.ToString(dtDetalles.Rows[i]["Observaciones"]) + "</p>");
                    contentEmail.Append("<img src='"+ rutaOrigen + "'width='300' height='300'/>");
                    contentEmail.Append("<br />");
                    contentEmail.Append("<br />");
                }

                HTMLTemplateMail = concatTemplateEmailWithHeaderBody(headerEmail.ToString(), contentEmail.ToString());

                MailMessage mail = new MailMessage(ConfigurationManager.AppSettings["eMailFrom"], ConfigurationManager.AppSettings["eMailTo"]);
                mail.From = new MailAddress(ConfigurationManager.AppSettings["eMailFrom"], "SIELAB", System.Text.Encoding.UTF8);
                mail.IsBodyHtml = true;
                string fecha = DateTime.Now.ToString("G");
                mail.Subject = "TICKET - Software MÍSTICO" + " - " + fecha;
                mail.Body = HTMLTemplateMail;
                //Línea para enviar una copia del correo
                //mail.Bcc.Add(ConfigurationManager.AppSettings["eMail"].ToString());

                SmtpClient client = new SmtpClient(ConfigurationManager.AppSettings["eMailSMTP"].ToString());
                client.Credentials = new System.Net.NetworkCredential(ConfigurationManager.AppSettings["eMailFrom"], ConfigurationManager.AppSettings["eMailPass"]);
                client.Port = int.Parse(ConfigurationManager.AppSettings["portEmail"].ToString());
                client.EnableSsl = true;
                client.Send(mail);
            }
            catch (Exception ex)
            {
                rpta = ex.Message;
            }

            return rpta;
        }
    }
}
