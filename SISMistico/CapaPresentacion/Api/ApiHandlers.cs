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

        public static ApiResponse Login(ApiRequest req)
        {
            string cuerpo, error;
            if (!req.LeerCuerpo(out cuerpo, out error))
                return ApiResponse.Error(400, "VALIDACION", error);

            Dictionary<string, object> datos = ApiJson.LeerObjeto(cuerpo, out error);
            if (datos == null)
                return ApiResponse.Error(400, "VALIDACION", error);

            int idEmpleado;
            if (!ApiJson.TryEntero(datos, "id_empleado", out idEmpleado) || idEmpleado <= 0)
                return ApiResponse.Error(400, "VALIDACION", "id_empleado es obligatorio y debe ser un entero positivo.");

            // El PIN solo no identifica a nadie (hay PIN repetidos): siempre va con id_empleado.
            string password = ApiJson.Texto(datos, "password");
            int pin;
            bool hayPin = datos.ContainsKey("codigo_maestra");
            bool hayPassword = datos.ContainsKey("password");
            if (hayPassword == hayPin)
                return ApiResponse.Error(400, "VALIDACION", "Envía exactamente una credencial: password o codigo_maestra.");
            if (hayPassword && string.IsNullOrEmpty(password))
                return ApiResponse.Error(400, "VALIDACION", "password debe ser un texto no vacío.");
            if (hayPin && !ApiJson.TryEntero(datos, "codigo_maestra", out pin))
                return ApiResponse.Error(400, "VALIDACION", "codigo_maestra debe ser un número entero.");
            ApiJson.TryEntero(datos, "codigo_maestra", out pin);

            int espera;
            if (ApiAuth.Bloqueada(req.Ip, out espera))
                return ApiResponse.Error(429, "DEMASIADOS_INTENTOS",
                    "Demasiados intentos fallidos. Espera " + espera + " s.");

            EmpleadoMovil emp = NApiMovil.ValidarEmpleado(idEmpleado, password, pin, hayPin);
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
