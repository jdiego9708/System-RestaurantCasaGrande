using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace CapaPresentacion.Api
{
    /// <summary>JSON sin dependencias externas (JavaScriptSerializer). Las respuestas se arman con diccionarios para fijar el snake_case.</summary>
    public static class ApiJson
    {
        private static JavaScriptSerializer Nuevo()
        {
            return new JavaScriptSerializer { MaxJsonLength = ApiConfig.MaxCuerpoBytes * 4 };
        }

        public static string Serializar(object valor)
        {
            return Nuevo().Serialize(valor);
        }

        /// <summary>Lee el cuerpo como objeto JSON; null y mensaje si no lo es.</summary>
        public static Dictionary<string, object> LeerObjeto(string json, out string error)
        {
            error = null;
            if (string.IsNullOrWhiteSpace(json))
            {
                error = "El cuerpo de la petición está vacío.";
                return null;
            }
            try
            {
                Dictionary<string, object> d = Nuevo().DeserializeObject(json) as Dictionary<string, object>;
                if (d == null)
                    error = "El cuerpo debe ser un objeto JSON.";
                return d;
            }
            catch (Exception)
            {
                error = "El cuerpo no es un JSON válido.";
                return null;
            }
        }

        /// <summary>Entero estricto: acepta números JSON enteros, no texto.</summary>
        public static bool TryEntero(Dictionary<string, object> d, string clave, out int valor)
        {
            valor = 0;
            object o;
            if (!d.TryGetValue(clave, out o) || o == null || o is string || o is bool)
                return false;
            try
            {
                decimal m = Convert.ToDecimal(o);
                if (m != decimal.Truncate(m) || m < int.MinValue || m > int.MaxValue)
                    return false;
                valor = (int)m;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static string Texto(Dictionary<string, object> d, string clave)
        {
            object o;
            return d.TryGetValue(clave, out o) && o is string ? (string)o : null;
        }

        /// <summary>Pesos como entero (en BD son decimal(19,2) pero todos los valores reales son enteros); decimal si hubiera centavos.</summary>
        public static object Dinero(object valor)
        {
            if (valor == null || valor == DBNull.Value)
                return 0;
            decimal m = Convert.ToDecimal(valor);
            return m == decimal.Truncate(m) ? (object)(long)m : m;
        }

        /// <summary>Texto de una celda; cadena vacía si es NULL.</summary>
        public static string Celda(System.Data.DataRow fila, string columna)
        {
            object o = fila[columna];
            return o == null || o == DBNull.Value ? string.Empty : Convert.ToString(o).Trim();
        }

        public static string Iso(DateTime fecha)
        {
            return fecha.ToString("yyyy-MM-dd'T'HH:mm:ss");
        }
    }
}
