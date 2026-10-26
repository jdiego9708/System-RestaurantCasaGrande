using System.Collections.Generic;

namespace CapaPresentacion.Api
{
    public sealed class ApiResponse
    {
        public int Status { get; private set; }
        public object Cuerpo { get; private set; }
        /// <summary>Código de error del contrato (04 §1.3), solo para el log.</summary>
        public string Codigo { get; private set; }
        /// <summary>Datos para la línea del log (p. ej. "id_pedido=12"); nunca el cuerpo ni credenciales.</summary>
        public string LogExtra { get; set; }

        /// <summary>Respuesta binaria (imágenes); si no es null, se envía en vez de <see cref="Cuerpo"/>.</summary>
        public byte[] Bytes { get; private set; }
        public string TipoContenido { get; private set; }
        public Dictionary<string, string> Cabeceras { get; private set; }

        private ApiResponse()
        {
            Cabeceras = new Dictionary<string, string>();
        }

        public static ApiResponse Ok(object cuerpo)
        {
            return new ApiResponse { Status = 200, Cuerpo = cuerpo };
        }

        public static ApiResponse Archivo(byte[] bytes, string tipoContenido)
        {
            ApiResponse r = new ApiResponse { Status = 200, Bytes = bytes, TipoContenido = tipoContenido };
            r.Cabeceras["Cache-Control"] = "private, max-age=3600";
            return r;
        }

        public static ApiResponse Creado(object cuerpo)
        {
            return new ApiResponse { Status = 201, Cuerpo = cuerpo };
        }

        /// <summary>202 Accepted: la orden se encoló (p. ej. la impresión) y se ejecuta después.</summary>
        public static ApiResponse Aceptado(object cuerpo)
        {
            return new ApiResponse { Status = 202, Cuerpo = cuerpo };
        }

        public static ApiResponse SinContenido()
        {
            return new ApiResponse { Status = 204 };
        }

        public static ApiResponse Error(int status, string codigo, string mensaje, object detalle = null)
        {
            Dictionary<string, object> error = new Dictionary<string, object>
            {
                { "codigo", codigo },
                { "mensaje", mensaje },
                { "detalle", detalle }
            };
            return new ApiResponse
            {
                Status = status,
                Codigo = codigo,
                Cuerpo = new Dictionary<string, object> { { "error", error } }
            };
        }
    }
}
