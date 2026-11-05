using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Web.Script.Serialization;

namespace CapaNegocio.Alegra
{
    /// <summary>Transporte REST. No reintenta operaciones que puedan crear documentos.</summary>
    public sealed class AlegraClient : IDisposable
    {
        private readonly HttpClient client;

        public AlegraClient() : this(new HttpClientHandler { AllowAutoRedirect = false }, AlegraCredentials.FromEnvironment()) { }

        public AlegraClient(HttpMessageHandler handler, AlegraCredentials credentials)
        {
            if (credentials == null) throw new ArgumentNullException(nameof(credentials));
            client = new HttpClient(handler, true)
            {
                BaseAddress = new Uri("https://api.alegra.com/api/v1/"),
                Timeout = TimeSpan.FromSeconds(30)
            };
            client.DefaultRequestHeaders.Authorization = credentials.CreateAuthorizationHeader();
            client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
        }

        public Task<string> GetContactsAsync(int start, CancellationToken cancellationToken) =>
            GetAsync("contacts?start=" + CheckOffset(start) + "&limit=30", cancellationToken);
        public Task<string> GetItemsAsync(int start, CancellationToken cancellationToken) =>
            GetAsync("items?start=" + CheckOffset(start) + "&limit=30", cancellationToken);
        public Task<string> GetInvoicesAsync(DateTime from,DateTime to,int start,CancellationToken token)
        {
            if(to.Date<from.Date)throw new ArgumentException("La fecha final debe ser posterior o igual a la inicial.");
            return GetAsync("invoices?date_afterOrNow="+from.ToString("yyyy-MM-dd",System.Globalization.CultureInfo.InvariantCulture)
                +"&date_beforeOrNow="+to.ToString("yyyy-MM-dd",System.Globalization.CultureInfo.InvariantCulture)
                +"&start="+CheckOffset(start)+"&limit=30&order_field=date&order_direction=DESC",token);
        }
        public Task<string> GetCompanyAsync(CancellationToken cancellationToken) => GetAsync("company", cancellationToken);
        public Task<string> GetTaxesAsync(CancellationToken cancellationToken) => GetAsync("taxes", cancellationToken);
        public Task<string> GetNumberTemplatesAsync(CancellationToken cancellationToken) => GetAsync("number-templates", cancellationToken);
        public Task<string> GetBankAccountsAsync(CancellationToken cancellationToken) => GetAsync("bank-accounts", cancellationToken);
        public Task<string> FindContactsAsync(string identification, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(identification)) throw new ArgumentException("Ingrese la identificación del receptor.");
            return GetAsync("contacts?identification=" + Uri.EscapeDataString(identification.Trim()) + "&limit=30", cancellationToken);
        }

        public Task<AlegraResponse> CreateInvoiceAsync(string json, CancellationToken cancellationToken) =>
            PostAsync("invoices", json, cancellationToken);
        public Task<AlegraResponse> CreateContactAsync(string json, CancellationToken cancellationToken) =>
            PostAsync("contacts", json, cancellationToken);
        public Task<AlegraResponse> StampInvoiceAsync(string id, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Falta el ID de la factura.");
            return PostAsync("invoices/stamp", new JavaScriptSerializer().Serialize(new { ids = new[] { id } }), cancellationToken);
        }

        private async Task<AlegraResponse> PostAsync(string path, string json, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(json)) throw new ArgumentException("Falta el cuerpo de la solicitud.");
            new JavaScriptSerializer().DeserializeObject(json);
            using (var content = new StringContent(json, Encoding.UTF8, "application/json"))
            using (HttpResponseMessage response = await client.PostAsync(path, content, cancellationToken).ConfigureAwait(false))
            {
                // Un 400 puede contener una factura ya creada. El coordinador debe conciliar el cuerpo.
                return new AlegraResponse((int)response.StatusCode, await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            }
        }
        public Task<string> GetInvoiceAsync(string id, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Falta el identificador de la factura.");
            return GetInvoiceDetailsAsync(id, cancellationToken);
        }

        public Task<string> GetInvoicePdfAsync(string id,CancellationToken cancellationToken)
        {
            if(string.IsNullOrWhiteSpace(id))throw new ArgumentException("Falta el ID de factura.");
            return GetAsync("invoices/"+Uri.EscapeDataString(id)+"?fields=pdf",cancellationToken);
        }
        public Task<string> GetInvoiceDetailsAsync(string id,CancellationToken cancellationToken)
        {
            if(string.IsNullOrWhiteSpace(id))throw new ArgumentException("Falta el ID de factura.");
            return GetAsync("invoices/"+Uri.EscapeDataString(id),cancellationToken);
        }
        public Task<string> GetInvoiceXmlAsync(string id,CancellationToken cancellationToken)
        {
            if(string.IsNullOrWhiteSpace(id))throw new ArgumentException("Falta el ID de factura.");
            return GetAsync("invoices/"+Uri.EscapeDataString(id)+"?fields=xml",cancellationToken);
        }
        public Task<string> GetInvoiceDownloadAsync(string id,CancellationToken cancellationToken)
        {
            if(string.IsNullOrWhiteSpace(id))throw new ArgumentException("Falta el ID de factura.");
            return GetAsync("invoices?id="+Uri.EscapeDataString(id)+"&download=true&downloadType=1to1",cancellationToken);
        }

        private static int CheckOffset(int start)
        {
            if (start < 0) throw new ArgumentOutOfRangeException(nameof(start));
            return start;
        }

        private async Task<string> GetAsync(string path, CancellationToken cancellationToken)
        {
            using (HttpResponseMessage response = await client.GetAsync(path, cancellationToken).ConfigureAwait(false))
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                    throw new InvalidOperationException("Alegra rechazó la autenticación (401). Revise correo y token.");
                if (response.StatusCode == HttpStatusCode.Forbidden)
                    throw new InvalidOperationException("Alegra denegó el acceso (403). Revise los permisos para esta operación.");
                if ((int)response.StatusCode == 429)
                    throw new InvalidOperationException("Alegra alcanzó el límite de solicitudes (429). Espere antes de reintentar.");
                if (!response.IsSuccessStatusCode)
                {
                    string detail=string.Empty;
                    try
                    {
                        var error=new JavaScriptSerializer().DeserializeObject(await response.Content.ReadAsStringAsync().ConfigureAwait(false)) as System.Collections.Generic.Dictionary<string,object>;
                        if(error!=null && error.ContainsKey("message"))detail=Convert.ToString(error["message"]);
                        if(client.DefaultRequestHeaders.Authorization!=null)
                            detail=detail.Replace(client.DefaultRequestHeaders.Authorization.Parameter,"[OMITIDO]");
                        detail=detail.Replace("\r"," ").Replace("\n"," ");
                        if(detail.Length>300)detail=detail.Substring(0,300);
                    }
                    catch {detail=string.Empty;}
                    throw new InvalidOperationException("No se pudo consultar Alegra. HTTP " + (int)response.StatusCode + ". "+detail);
                }
                return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
        }

        public void Dispose() => client.Dispose();
    }

    public sealed class AlegraResponse
    {
        public int StatusCode { get; private set; }
        public string Json { get; private set; }
        public bool Success => StatusCode >= 200 && StatusCode < 300;
        public AlegraResponse(int statusCode, string json) { StatusCode = statusCode; Json = json; }
        public override string ToString() => "Respuesta Alegra HTTP " + StatusCode;
    }
}
