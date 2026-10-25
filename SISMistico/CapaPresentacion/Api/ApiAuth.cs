using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

using CapaNegocio;

namespace CapaPresentacion.Api
{
    public sealed class ApiSesion
    {
        public string Token { get; set; }
        public EmpleadoMovil Empleado { get; set; }
        public DateTime Expira { get; set; }
    }

    /// <summary>Sesiones en memoria (se pierden al cerrar el .exe), clave compartida, límite de intentos y lista de IP.</summary>
    public static class ApiAuth
    {
        private const int MaxFallos = 5;
        private static readonly TimeSpan VentanaFallos = TimeSpan.FromSeconds(60);

        private static readonly ConcurrentDictionary<string, ApiSesion> Sesiones =
            new ConcurrentDictionary<string, ApiSesion>();
        private static readonly Dictionary<string, List<DateTime>> Fallos =
            new Dictionary<string, List<DateTime>>();

        /// <summary>Comparación en tiempo constante de X-Api-Key.</summary>
        public static bool ClaveValida(string recibida)
        {
            string esperada = ApiConfig.Clave;
            if (recibida == null || recibida.Length != esperada.Length)
                return false;
            int dif = 0;
            for (int i = 0; i < esperada.Length; i++)
                dif |= esperada[i] ^ recibida[i];
            return dif == 0;
        }

        public static ApiSesion Crear(EmpleadoMovil empleado)
        {
            Depurar();
            ApiSesion s = new ApiSesion
            {
                Token = Guid.NewGuid().ToString(),
                Empleado = empleado,
                Expira = DateTime.Now.AddHours(ApiConfig.TokenHoras)
            };
            Sesiones[s.Token] = s;
            return s;
        }

        public static ApiSesion Buscar(string token)
        {
            ApiSesion s;
            if (string.IsNullOrEmpty(token) || !Sesiones.TryGetValue(token, out s))
                return null;
            if (s.Expira <= DateTime.Now)
            {
                Sesiones.TryRemove(token, out s);
                return null;
            }
            return s;
        }

        public static void Cerrar(string token)
        {
            ApiSesion s;
            if (token != null)
                Sesiones.TryRemove(token, out s);
        }

        private static void Depurar()
        {
            foreach (KeyValuePair<string, ApiSesion> kv in Sesiones)
                if (kv.Value.Expira <= DateTime.Now)
                {
                    ApiSesion s;
                    Sesiones.TryRemove(kv.Key, out s);
                }
        }

        // ---- límite de intentos de login por IP ----

        /// <summary>True (y segundos de espera) si la IP acumuló demasiados fallos en el último minuto.</summary>
        public static bool Bloqueada(string ip, out int segundos)
        {
            segundos = 0;
            lock (Fallos)
            {
                List<DateTime> lista;
                if (!Fallos.TryGetValue(ip, out lista))
                    return false;
                lista.RemoveAll(f => f < DateTime.Now - VentanaFallos);
                if (lista.Count < MaxFallos)
                    return false;
                segundos = Math.Max(1, (int)Math.Ceiling((lista[0] + VentanaFallos - DateTime.Now).TotalSeconds));
                return true;
            }
        }

        public static void RegistrarFallo(string ip)
        {
            lock (Fallos)
            {
                List<DateTime> lista;
                if (!Fallos.TryGetValue(ip, out lista))
                    Fallos[ip] = lista = new List<DateTime>();
                lista.Add(DateTime.Now);
            }
        }

        public static void LimpiarFallos(string ip)
        {
            lock (Fallos)
                Fallos.Remove(ip);
        }

        // ---- lista de IP: solo LAN y esta máquina ----

        public static bool IpPermitida(IPAddress ip)
        {
            if (ip == null)
                return false;
            if (ip.IsIPv4MappedToIPv6)
                ip = ip.MapToIPv4();
            if (IPAddress.IsLoopback(ip))
                return true;

            byte[] b = ip.GetAddressBytes();
            if (ip.AddressFamily == AddressFamily.InterNetwork)
                return b[0] == 10
                    || (b[0] == 172 && b[1] >= 16 && b[1] <= 31)
                    || (b[0] == 192 && b[1] == 168);
            if (ip.AddressFamily == AddressFamily.InterNetworkV6)
                return ip.IsIPv6LinkLocal || (b[0] & 0xFE) == 0xFC;
            return false;
        }
    }
}
