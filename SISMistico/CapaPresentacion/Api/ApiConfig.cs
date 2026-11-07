using System;
using System.Configuration;
using System.IO;

namespace CapaPresentacion.Api
{
    /// <summary>Lectura de las claves Api* de App.config; todas opcionales.</summary>
    public static class ApiConfig
    {
        public const string Version = "1.0";
        public const string ClavePlaceholder = "cambiar-esta-clave";
        public const int MaxCuerpoBytes = 256 * 1024;

        public static bool Habilitada
        {
            get { return string.Equals(Leer("ApiHabilitada", "false"), "true", StringComparison.OrdinalIgnoreCase); }
        }

        public static string Prefijo
        {
            get
            {
                string p = Leer("ApiPrefijo", "http://+:8099/api/v1/");
                return p.EndsWith("/") ? p : p + "/";
            }
        }

        public static string Clave
        {
            get { return Leer("ApiKey", ClavePlaceholder); }
        }

        /// <summary>True si el instalador (o Cambiar-Codigo-Maestro.ps1) ya dejó el código maestro configurado.</summary>
        public static bool CodigoMaestroConfigurado
        {
            get { return Leer("MasterCodeSalt", string.Empty).Length > 0 && Leer("MasterCodeHash", string.Empty).Length > 0; }
        }

        /// <summary>
        /// Comprueba el código maestro de 8 dígitos (solo para quitar productos de un pedido). En la configuración
        /// solo hay sal + SHA-256 hexadecimal de (sal + código) en UTF-8; el código en claro no se guarda.
        /// </summary>
        public static bool CodigoMaestroValido(string codigo)
        {
            if (!CodigoMaestroConfigurado || string.IsNullOrEmpty(codigo) || codigo.Length != 8) return false;
            foreach (char c in codigo)
                if (c < '0' || c > '9') return false;

            string esperado = Leer("MasterCodeHash", string.Empty).ToLowerInvariant();
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(Leer("MasterCodeSalt", string.Empty) + codigo);
            string obtenido;
            using (System.Security.Cryptography.SHA256 sha = System.Security.Cryptography.SHA256.Create())
                obtenido = BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", string.Empty).ToLowerInvariant();

            // Comparación de tiempo constante: no filtra cuántos caracteres coinciden.
            int diferencia = esperado.Length ^ obtenido.Length;
            for (int i = 0; i < esperado.Length && i < obtenido.Length; i++)
                diferencia |= esperado[i] ^ obtenido[i];
            return diferencia == 0;
        }

        public static int TokenHoras
        {
            get
            {
                int h;
                return int.TryParse(Leer("ApiTokenHoras", "8"), out h) && h > 0 ? h : 8;
            }
        }

        public static int MaxItems
        {
            get
            {
                int n;
                return int.TryParse(Leer("ApiMaxItems", "200"), out n) && n > 0 ? n : 200;
            }
        }

        /// <summary>Valor por defecto de imprimir_comandas cuando la petición no lo indica (D4: apagado).</summary>
        public static bool ImprimirComandas
        {
            get { return string.Equals(Leer("ApiImprimirComandas", "false"), "true", StringComparison.OrdinalIgnoreCase); }
        }

        /// <summary>
        /// Carpeta donde la API guarda las comandas en PDF cuando la impresora predeterminada es de PDF.
        /// Relativa a Documentos del usuario si no es una ruta absoluta; por defecto "Casa Grande\Comandas".
        /// </summary>
        public static string ComandasCarpeta
        {
            get { return Leer("ApiComandasCarpeta", string.Empty); }
        }

        /// <summary>Carpeta de imágenes: ApiImagenesCarpeta o, si no está, la RUTAIMAGES que ya usa el escritorio.</summary>
        public static string ImagenesCarpeta
        {
            get { return Leer("ApiImagenesCarpeta", Leer("RUTAIMAGES", string.Empty)); }
        }

        public static string LogCarpeta
        {
            get { return Leer("ApiLogCarpeta", "logs"); }
        }

        /// <summary>Parte de ruta del prefijo (p. ej. "/api/v1"), para quitarla de cada petición.</summary>
        public static string RutaBase
        {
            get
            {
                string p = Prefijo.Replace("://+", "://localhost").Replace("://*", "://localhost");
                return new Uri(p).AbsolutePath.TrimEnd('/');
            }
        }

        /// <summary>True si el prefijo solo escucha en esta máquina (no hace falta urlacl ni clave real).</summary>
        public static bool PrefijoLocal
        {
            get
            {
                string p = Prefijo.ToLowerInvariant();
                return p.Contains("://localhost") || p.Contains("://127.0.0.1") || p.Contains("://[::1]");
            }
        }

        public static string CarpetaLogAbsoluta()
        {
            string c = LogCarpeta;
            return Path.IsPathRooted(c) ? c : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, c);
        }

        private static string Leer(string clave, string porDefecto)
        {
            string v = ConfigurationManager.AppSettings[clave];
            return string.IsNullOrWhiteSpace(v) ? porDefecto : v.Trim();
        }
    }
}
