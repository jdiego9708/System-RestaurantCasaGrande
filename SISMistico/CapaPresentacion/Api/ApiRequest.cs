using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;

namespace CapaPresentacion.Api
{
    public sealed class ApiRequest
    {
        public string Metodo { get; private set; }
        public string Ruta { get; private set; }
        public string Ip { get; private set; }
        public IPAddress DireccionIp { get; private set; }
        public NameValueCollection Query { get; private set; }
        public Dictionary<string, string> Parametros { get; private set; }
        public ApiSesion Sesion { get; set; }

        /// <summary>http://host:puerto/api/v1 tal como lo alcanzó el cliente (para armar URLs absolutas).</summary>
        public string UrlBase
        {
            get { return _req.Url.GetLeftPart(UriPartial.Authority) + ApiConfig.RutaBase; }
        }

        private readonly HttpListenerRequest _req;

        private ApiRequest(HttpListenerRequest req, string ruta)
        {
            _req = req;
            Metodo = req.HttpMethod.ToUpperInvariant();
            Ruta = ruta;
            Query = req.QueryString;
            Parametros = new Dictionary<string, string>();
            DireccionIp = req.RemoteEndPoint == null ? IPAddress.None : req.RemoteEndPoint.Address;
            Ip = DireccionIp.ToString();
        }

        /// <summary>Ruta sin el prefijo (/api/v1) y sin barra final; "/" si queda vacía.</summary>
        public static ApiRequest Crear(HttpListenerContext ctx)
        {
            string ruta = ctx.Request.Url.AbsolutePath;
            string baseRuta = ApiConfig.RutaBase;
            if (baseRuta.Length > 0 && ruta.StartsWith(baseRuta, StringComparison.OrdinalIgnoreCase))
                ruta = ruta.Substring(baseRuta.Length);
            ruta = ruta.TrimEnd('/');
            return new ApiRequest(ctx.Request, ruta.Length == 0 ? "/" : ruta);
        }

        public string Cabecera(string nombre)
        {
            return _req.Headers[nombre];
        }

        public string Token
        {
            get
            {
                string a = Cabecera("Authorization");
                const string prefijo = "Bearer ";
                return a != null && a.StartsWith(prefijo, StringComparison.OrdinalIgnoreCase)
                    ? a.Substring(prefijo.Length).Trim() : null;
            }
        }

        /// <summary>Lee el cuerpo con tope de tamaño. Devuelve false y deja el motivo en <paramref name="error"/>.</summary>
        public bool LeerCuerpo(out string cuerpo, out string error)
        {
            cuerpo = null;
            error = null;
            if (_req.ContentLength64 > ApiConfig.MaxCuerpoBytes)
            {
                error = "El cuerpo supera el tamaño máximo permitido.";
                return false;
            }
            if (!_req.HasEntityBody)
            {
                cuerpo = string.Empty;
                return true;
            }

            using (MemoryStream ms = new MemoryStream())
            {
                byte[] buffer = new byte[8192];
                int n;
                while ((n = _req.InputStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, n);
                    if (ms.Length > ApiConfig.MaxCuerpoBytes)
                    {
                        error = "El cuerpo supera el tamaño máximo permitido.";
                        return false;
                    }
                }
                Encoding enc = _req.ContentEncoding ?? Encoding.UTF8;
                cuerpo = enc.GetString(ms.ToArray());
                return true;
            }
        }
    }
}
