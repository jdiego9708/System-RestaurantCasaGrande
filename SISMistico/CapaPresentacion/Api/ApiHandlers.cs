using System;
using System.Collections.Generic;

using CapaNegocio;

namespace CapaPresentacion.Api
{
    /// <summary>Endpoints de la fase 1: /ping y /auth/*.</summary>
    internal static class ApiHandlers
    {
        public static ApiResponse Ping(ApiRequest req)
        {
            return ApiResponse.Ok(new Dictionary<string, object>
            {
                { "ok", true },
                { "version", ApiConfig.Version },
                { "servidor", Environment.MachineName },
                { "hora", ApiJson.Iso(DateTime.Now) },
                { "db", NApiMovil.ProbarConexion() }
            });
        }

        /// <summary>
        /// Tres modos: solo PIN (la app móvil), id_empleado + PIN, o id_empleado + password.
        /// El PIN solo puede ser ambiguo (lo comparten varios empleados): 409 PIN_AMBIGUO con los candidatos.
        /// </summary>
        public static ApiResponse Login(ApiRequest req)
        {
            string cuerpo, error;
            if (!req.LeerCuerpo(out cuerpo, out error))
                return ApiResponse.Error(400, "VALIDACION", error);

            Dictionary<string, object> datos = ApiJson.LeerObjeto(cuerpo, out error);
            if (datos == null)
                return ApiResponse.Error(400, "VALIDACION", error);

            bool hayId = datos.ContainsKey("id_empleado") && datos["id_empleado"] != null;
            bool hayPassword = datos.ContainsKey("password") && datos["password"] != null;
            bool hayPin = datos.ContainsKey("codigo_maestra") && datos["codigo_maestra"] != null;

            if (hayPassword == hayPin)
                return ApiResponse.Error(400, "VALIDACION", "Envía exactamente una credencial: password o codigo_maestra.");
            if (hayPassword && !hayId)
                return ApiResponse.Error(400, "VALIDACION", "password requiere id_empleado; solo el PIN puede ir solo.");

            int idEmpleado = 0;
            if (hayId && (!ApiJson.TryEntero(datos, "id_empleado", out idEmpleado) || idEmpleado <= 0))
                return ApiResponse.Error(400, "VALIDACION", "id_empleado debe ser un entero positivo.");

            string password = null;
            if (hayPassword)
            {
                password = ApiJson.Texto(datos, "password");
                if (string.IsNullOrEmpty(password))
                    return ApiResponse.Error(400, "VALIDACION", "password debe ser un texto no vacío.");
            }

            int pin = 0;
            if (hayPin && (!ApiJson.TryEntero(datos, "codigo_maestra", out pin) || pin <= 0))
                return ApiResponse.Error(400, "VALIDACION", "codigo_maestra debe ser un entero mayor que 0.");

            int espera;
            if (ApiAuth.Bloqueada(req.Ip, out espera))
                return ApiResponse.Error(429, "DEMASIADOS_INTENTOS",
                    "Demasiados intentos fallidos. Espera " + espera + " s.");

            EmpleadoMovil emp;
            if (hayId)
            {
                emp = NApiMovil.ValidarEmpleado(idEmpleado, password, pin, hayPin);
            }
            else
            {
                ResultadoPin r = NApiMovil.ResolverPorPin(pin);
                if (r.Ambiguo)
                {
                    // Un PIN válido pero compartido también cuenta como intento: frena el barrido de PIN.
                    ApiAuth.RegistrarFallo(req.Ip);
                    return ApiResponse.Error(409, "PIN_AMBIGUO",
                        "Ese PIN lo comparten varios empleados; elige quién eres.", Candidatos(r.Candidatos));
                }
                emp = r.Unico;
            }

            if (emp == null)
            {
                ApiAuth.RegistrarFallo(req.Ip);
                return ApiResponse.Error(403, "CLAVE_INVALIDA", "Credenciales inválidas.");
            }

            ApiAuth.LimpiarFallos(req.Ip);
            // D12: paridad con el escritorio (sp_Login no mira Estado). Se deja constancia en el log.
            if (!emp.EstaActivo)
                ApiLog.Info("Login de empleado no ACTIVO (permitido por paridad con el escritorio, D12): id=" + emp.IdEmpleado);

            ApiSesion sesion = ApiAuth.Crear(emp);
            Dictionary<string, object> respuesta = new Dictionary<string, object>
            {
                { "token", sesion.Token },
                { "expira", ApiJson.Iso(sesion.Expira) },
                { "empleado", Empleado(emp) }
            };
            AgregarTurno(respuesta);
            req.Sesion = sesion;
            return ApiResponse.Ok(respuesta);
        }

        public static ApiResponse Me(ApiRequest req)
        {
            Dictionary<string, object> respuesta = new Dictionary<string, object>
            {
                { "empleado", Empleado(req.Sesion.Empleado) },
                { "expira", ApiJson.Iso(req.Sesion.Expira) }
            };
            AgregarTurno(respuesta);
            return ApiResponse.Ok(respuesta);
        }

        public static ApiResponse Logout(ApiRequest req)
        {
            ApiAuth.Cerrar(req.Sesion.Token);
            return ApiResponse.SinContenido();
        }

        private static Dictionary<string, object> Empleado(EmpleadoMovil e)
        {
            return new Dictionary<string, object>
            {
                { "id_empleado", e.IdEmpleado },
                { "nombre", e.Nombre },
                { "cargo", e.Cargo },
                { "codigo_maestra", e.TienePin }
            };
        }

        /// <summary>
        /// Candidatos de un PIN compartido. `duplicado` marca las fichas que repiten nombre con otra del
        /// mismo grupo: casi siempre es una persona registrada dos veces y hay que unificarla en la BD
        /// (db/02_unificar_empleados_duplicados.sql). El cliente lo avisa en pantalla.
        /// </summary>
        private static Dictionary<string, object> Candidatos(List<EmpleadoMovil> lista)
        {
            List<object> items = new List<object>();
            bool hayDuplicados = false;
            foreach (EmpleadoMovil e in lista)
            {
                int mismoNombre = 0;
                foreach (EmpleadoMovil otro in lista)
                    if (string.Equals(otro.Nombre, e.Nombre, StringComparison.OrdinalIgnoreCase))
                        mismoNombre++;
                bool duplicado = mismoNombre > 1;
                hayDuplicados |= duplicado;
                items.Add(new Dictionary<string, object>
                {
                    { "id_empleado", e.IdEmpleado },
                    { "nombre", e.Nombre },
                    { "cargo", e.Cargo },
                    { "activo", e.EstaActivo },
                    { "duplicado", duplicado }
                });
            }
            return new Dictionary<string, object>
            {
                { "candidatos", items },
                { "duplicados", hayDuplicados }
            };
        }

        /// <summary>Último turno, solo lectura; si no hay o falla la lectura, se omite.</summary>
        private static void AgregarTurno(Dictionary<string, object> respuesta)
        {
            try
            {
                TurnoMovil t = NApiMovil.UltimoTurno();
                if (t != null)
                    respuesta["turno"] = new Dictionary<string, object>
                    {
                        { "id_turno", t.IdTurno },
                        { "fecha", t.Fecha.ToString("yyyy-MM-dd") },
                        { "estado", t.Estado }
                    };
            }
            catch (Exception ex)
            {
                ApiLog.Error("No se pudo leer el turno", ex);
            }
        }
    }
}
