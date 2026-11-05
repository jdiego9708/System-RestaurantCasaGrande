using System;
using System.Net.Http.Headers;
using System.Text;

namespace CapaNegocio.Alegra
{
    /// <summary>Credenciales exclusivamente desde el entorno; nunca desde App.config o la base de datos.</summary>
    public sealed class AlegraCredentials
    {
        private readonly string email;
        private readonly string token;
        private readonly string scheme;

        private AlegraCredentials(string email, string token, string scheme)
        {
            this.email = email;
            this.token = token;
            this.scheme = scheme;
        }

        public static AlegraCredentials FromEnvironment()
        {
            return FromReader(name =>
                Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Machine)
                ?? Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.User)
                ?? Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process));
        }

        internal static AlegraCredentials FromReader(Func<string, string> read)
        {
            string email = (read("ALEGRA_EMAIL") ?? string.Empty).Trim();
            string token = (read("ALEGRA_API_KEY") ?? string.Empty).Trim();
            string scheme = (read("ALEGRA_AUTH_SCHEME") ?? "Basic").Trim();
            if (!scheme.Equals("Basic",StringComparison.OrdinalIgnoreCase) && !scheme.Equals("Bearer",StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("ALEGRA_AUTH_SCHEME debe ser Basic o Bearer.");
            if (email.Length == 0 || token.Length == 0)
                throw new InvalidOperationException("Configure ALEGRA_EMAIL y ALEGRA_API_KEY en las variables de entorno de Windows.");
            if (email.IndexOf(':') >= 0 || email.IndexOfAny(new[] { '\r', '\n' }) >= 0
                || token.IndexOfAny(new[] { '\r', '\n' }) >= 0)
                throw new InvalidOperationException("El formato de las credenciales de Alegra no es válido.");
            return new AlegraCredentials(email, token, scheme);
        }

        public AuthenticationHeaderValue CreateAuthorizationHeader()
        {
            return scheme.Equals("Bearer",StringComparison.OrdinalIgnoreCase)
                ? new AuthenticationHeaderValue("Bearer",token)
                : new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(email + ":" + token)));
        }

        public override string ToString() => "Credenciales Alegra [protegidas]";
    }
}
