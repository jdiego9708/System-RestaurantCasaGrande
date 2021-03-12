using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using System.Configuration;

namespace CapaPresentacion
{
    public static class ArchivosAdjuntos
    {
        /*
         * id: Identificación devuelta por 
         * appKey: Llave de archivo de configuración
         * archivo: Nombre del archivo a guardar, este nombre es el original del archivo 
         * que tiene la extensión incluida
         * rutaOrigen: La ruta de la cual se copiará el archivo
         * rutaDestino: La ruta de destino donde se pegará el archivo copiado.
        */
        public static string GuardarArchivo(int id, string appKey, string archivo,
            string rutaOrigen)
        {
            string rpta = "OK";
            string rutaDestino = ObtenerRutaDestino(appKey);
            try
            {
                string nombre_archivo = string.Concat(archivo);
                bool insert = true;
                if (rutaDestino.Equals(rutaOrigen))
                {
                    insert = false;
                }

                if (insert)
                {
                    DirectoryInfo DirectoryInfo = new DirectoryInfo(rutaDestino);
                    string destino = Path.Combine(DirectoryInfo.ToString(), nombre_archivo);
                    File.Copy(rutaOrigen, destino, true);
                }
            }
            catch (Exception ex)
            {
                rpta = ex.Message;
            }
            return rpta;
        }

        public static string ObtenerRutaDestino(string appKey)
        {
            string ruta = "";
            var appSettings = ConfigurationManager.AppSettings;
            try
            {
                ruta = Convert.ToString(appSettings[appKey]);
            }
            catch (ConfigurationErrorsException ex)
            {
                ruta = ex.Message;
            }

            return ruta;
        }
    }
}
