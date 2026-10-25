using System;
using System.Collections.Generic;

namespace CapaPresentacion.Api
{
    /// <summary>
    /// Clave de idempotencia -> pedido ya creado, en memoria y 24 h. Se pierde al cerrar el .exe; si el cliente
    /// reintenta después de un reinicio, la mesa ocupada (409) impide el duplicado.
    /// </summary>
    internal static class ApiIdempotencia
    {
        private sealed class Entrada
        {
            public string Hash;
            public int IdPedido;
            public DateTime Expira;
        }

        private static readonly TimeSpan Vigencia = TimeSpan.FromHours(24);
        private static readonly Dictionary<string, Entrada> Entradas = new Dictionary<string, Entrada>();

        public static bool Buscar(string clave, out string hash, out int idPedido)
        {
            hash = null;
            idPedido = 0;
            lock (Entradas)
            {
                Entrada e;
                if (!Entradas.TryGetValue(clave, out e))
                    return false;
                if (e.Expira <= DateTime.Now)
                {
                    Entradas.Remove(clave);
                    return false;
                }
                hash = e.Hash;
                idPedido = e.IdPedido;
                return true;
            }
        }

        public static void Guardar(string clave, string hash, int idPedido)
        {
            lock (Entradas)
            {
                List<string> vencidas = new List<string>();
                foreach (KeyValuePair<string, Entrada> kv in Entradas)
                    if (kv.Value.Expira <= DateTime.Now)
                        vencidas.Add(kv.Key);
                foreach (string k in vencidas)
                    Entradas.Remove(k);

                Entradas[clave] = new Entrada { Hash = hash, IdPedido = idPedido, Expira = DateTime.Now.Add(Vigencia) };
            }
        }

        public static void Quitar(string clave)
        {
            lock (Entradas)
                Entradas.Remove(clave);
        }
    }
}
