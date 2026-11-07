using CapaDatos;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace CapaNegocio.Alegra
{
    public static class AlegraWorkflow
    {
        private static JavaScriptSerializer Json => new JavaScriptSerializer { MaxJsonLength = 8 * 1024 * 1024 };
        public static AlegraInvoiceConfiguration Settings()
        {
            string text = DAlegra.GetSettings();
            return string.IsNullOrWhiteSpace(text) ? new AlegraInvoiceConfiguration() : Json.Deserialize<AlegraInvoiceConfiguration>(text);
        }
        public static void SaveSettings(AlegraInvoiceConfiguration settings) => DAlegra.SaveSettings(Json.Serialize(settings));
        public static DataTable LocalProducts() => DAlegra.LocalProducts();
        public static DataTable Request(int saleId) => DAlegra.GetRequest(saleId);
        public static void Retry(int saleId) => DAlegra.Retry(saleId);
        public static void Expedite(int saleId) => DAlegra.Expedite(saleId);
        public static void StorePdf(int saleId,string path) => DAlegra.StorePdf(saleId,path);
        public static void MarkPrint(int saleId,string status) => DAlegra.MarkPrint(saleId,status);
        public static bool ClaimPrint(int saleId,bool reprint) => DAlegra.ClaimPrint(saleId,reprint);
        public static DataTable PendingDocuments(bool autoPrint, DateTime? fromUtc) => DAlegra.Query(@"
IF OBJECT_ID('dbo.AlegraInvoiceRequests') IS NOT NULL
SELECT TOP(1) SaleId FROM dbo.AlegraInvoiceRequests
WHERE Status='ACCEPTED' AND NextAttemptAt<=SYSUTCDATETIME() AND
(NULLIF(PdfPath,'') IS NULL OR (@print=1 AND PrintStatus='PENDING' AND (@from IS NULL OR CreatedAt>=@from)))
ORDER BY NextAttemptAt, CreatedAt",
            new System.Data.SqlClient.SqlParameter("@print",autoPrint),
            new System.Data.SqlClient.SqlParameter("@from",System.Data.SqlDbType.DateTime2) {Value=(object)fromUtc ?? DBNull.Value});
        public static void DocumentFailed(int saleId, string error) => DAlegra.Query(@"
UPDATE dbo.AlegraInvoiceRequests SET LastError=@error, UpdatedAt=SYSUTCDATETIME(), NextAttemptAt=DATEADD(minute,1,SYSUTCDATETIME())
WHERE SaleId=@sale AND Status='ACCEPTED'",
            new System.Data.SqlClient.SqlParameter("@sale",saleId),
            new System.Data.SqlClient.SqlParameter("@error",System.Data.SqlDbType.NVarChar,2000) {Value=error.Length>2000 ? error.Substring(0,2000) : error});
        public static Dictionary<string,object> Object(string text) => Json.DeserializeObject(text) as Dictionary<string,object>
            ?? throw new InvalidOperationException("Alegra devolvió una respuesta inesperada.");
        public static string Text(Dictionary<string,object> value,string key) => value != null && value.ContainsKey(key) ? Convert.ToString(value[key]) : string.Empty;
        public static List<Dictionary<string,object>> List(string text)
        {
            object value = Json.DeserializeObject(text);
            if (value is Dictionary<string,object> wrapper && wrapper.ContainsKey("data")) value=wrapper["data"];
            return (value as object[] ?? new object[0]).Cast<Dictionary<string,object>>().ToList();
        }

        public static string CompanyIdentity(Dictionary<string, object> company)
        {
            string id = Text(company, "id");
            if (!string.IsNullOrWhiteSpace(id)) return id;
            var identification = company.ContainsKey("identificationObject") ? company["identificationObject"] as Dictionary<string, object> : null;
            string number = Text(identification, "number");
            if (string.IsNullOrWhiteSpace(number)) number = Text(company, "identification");
            if (string.IsNullOrWhiteSpace(number)) throw new InvalidOperationException("Alegra no devolvi? la identificaci?n de la empresa.");
            return Text(company, "applicationVersion") + ":" + number.Trim();
        }

        // Se invoca en un hilo de fondo. Cada paso libera la solicitud antes del siguiente ciclo.
        public static async Task<int?> TickAsync(CancellationToken cancellationToken)
        {
            var settings=Settings();
            if (!settings.Enabled) return null;
            Guid token=Guid.NewGuid();
            DataTable claimed=DAlegra.Claim(token);
            if (claimed.Rows.Count==0) return null;
            DataRow row=claimed.Rows[0];
            Guid requestId=(Guid)row["RequestId"];
            int saleId=Convert.ToInt32(row["SaleId"]);
            string remote=Convert.ToString(row["RemoteId"]);
            bool posting=false;
            try
            {
                using (var api=new AlegraClient())
                {
                    var company=Object(await api.GetCompanyAsync(cancellationToken).ConfigureAwait(false));
                    if (CompanyIdentity(company)!=settings.CompanyId || string.IsNullOrWhiteSpace(settings.CompanyId))
                        throw new InvalidOperationException("La cuenta API no corresponde a la empresa configurada.");
                    DAlegra.BindCompany(requestId,token,settings.CompanyId);
                    var snapshot=Object(Convert.ToString(row["RequestJson"]));
                    var fiscal=(Dictionary<string,object>)snapshot["fiscal"];
                    if (string.IsNullOrWhiteSpace(remote))
                    {
                        // Validar numeracion, pagos y productos antes de crear un contacto o factura.
                        settings.ClientId="validation-only";
                        snapshot["requestId"]=requestId.ToString();
                        string snapshotJson=Json.Serialize(snapshot);
                        DateTime date=DateTime.Today;
                        AlegraInvoiceBuilder.Build(snapshotJson,settings,date);
                        var contacts=List(await api.FindContactsAsync(Text(fiscal,"Identificación"),cancellationToken).ConfigureAwait(false));
                        var matches=contacts.Where(contact=>Matches(contact,fiscal)).ToList();
                        string selected=Text(fiscal,"remoteClientId");
                        if (selected.Length>0) matches=matches.Where(contact=>Text(contact,"id")==selected).ToList();
                        if (matches.Count>1) throw new InvalidOperationException("Hay varios contactos con la misma identificación. Seleccione uno en la captura fiscal.");
                        if (matches.Count==0 && selected.Length>0) throw new InvalidOperationException("El cliente seleccionado no coincide con la identificación fiscal.");
                        if (matches.Count==0)
                        {
                            posting=true;
                            var contact=await api.CreateContactAsync(Json.Serialize(BuildContact(fiscal)),cancellationToken).ConfigureAwait(false);
                            if (!contact.Success) throw new InvalidOperationException("No se pudo registrar el receptor. HTTP "+contact.StatusCode+". Revise antes de reintentar.");
                            settings.ClientId=Text(Object(contact.Json),"id");
                            posting=false;
                        }
                        else settings.ClientId=Text(matches[0],"id");
                        if (string.IsNullOrWhiteSpace(settings.ClientId)) throw new InvalidOperationException("Alegra no devolvió el ID del receptor.");
                        string payload=AlegraInvoiceBuilder.Build(snapshotJson,settings,date);
                        posting=true;
                        var response=await api.CreateInvoiceAsync(payload,cancellationToken).ConfigureAwait(false);
                        var body=Object(response.Json);
                        remote=Text(body,"id");
                        if (string.IsNullOrWhiteSpace(remote) && body.ContainsKey("invoice")) remote=Text(body["invoice"] as Dictionary<string,object>,"id");
                        if (string.IsNullOrWhiteSpace(remote))
                        {
                            DAlegra.Update(requestId,token,"UNCERTAIN",null,null,null,"Resultado de creación sin ID confirmado. HTTP "+response.StatusCode+". Conciliar antes de recrear.",payload);
                            return null;
                        }
                        DAlegra.Update(requestId,token,"CREATED",remote,null,null,null,payload);
                        return null;
                    }
                    using (var emissionApi = new AlegraClient(new System.Net.Http.HttpClientHandler { AllowAutoRedirect = false }, AlegraCredentials.FromEmissionEnvironment()))
                    {
                        var emissionCompany = Object(await emissionApi.GetCompanyAsync(cancellationToken).ConfigureAwait(false));
                        if (CompanyIdentity(emissionCompany) != settings.CompanyId)
                            throw new InvalidOperationException("La credencial de emision no corresponde a la empresa configurada.");
                        var invoice=Object(await emissionApi.GetInvoiceAsync(remote,cancellationToken).ConfigureAwait(false));
                        if (Text(invoice,"id")!=remote) throw new InvalidOperationException("El ID consultado no coincide con la factura vinculada.");
                        // Total esperado de la factura: el de la venta, o el de lo cubierto si se eligió «solo vinculados».
                        decimal expected=AlegraInvoiceBuilder.ExpectedTotal(Convert.ToString(row["RequestJson"]),settings);
                        if (!invoice.ContainsKey("total") || Math.Abs(Convert.ToDecimal(invoice["total"])-expected)>0.01m)
                            throw new InvalidOperationException("El total de Alegra no coincide con la venta. Revisar antes de emitir.");
                        string emission=Emission(invoice);
                        string number=invoice.ContainsKey("numberTemplate") ? Text(invoice["numberTemplate"] as Dictionary<string,object>,"fullNumber") : null;
                        if (emission=="STAMPED_AND_ACCEPTED" || emission=="STAMPED_AND_ACCEPTED_WITH_OBSERVATIONS" || emission=="ACCEPTED")
                        {
                            DAlegra.Update(requestId,token,"ACCEPTED",remote,number,emission,null);
                            return saleId;
                        }
                        if (emission=="STAMPED_AND_REJECTED" || emission=="REJECTED")
                        {
                            DAlegra.Update(requestId,token,"REJECTED",remote,number,emission,"La emisión fue rechazada; revise el documento en Alegra.");
                            return null;
                        }
                        if (Convert.ToString(row["Status"])=="ISSUING" || emission=="STAMPED_AND_WAITING_RESPONSE")
                        {
                            DAlegra.Update(requestId,token,"ISSUING",remote,number,emission,null);
                            return null;
                        }
                        posting=true;
                        var stamped=await emissionApi.StampInvoiceAsync(remote,cancellationToken).ConfigureAwait(false);
                        if (stamped.StatusCode == 401 || stamped.StatusCode == 403)
                        {
                            string reason = stamped.StatusCode == 403
                                ? "Alegra denego el acceso de la credencial API a POST /invoices/stamp (HTTP 403). Revisar permisos de emision con Alegra."
                                : "Alegra rechazo la autenticacion al emitir (HTTP 401). Revisar las credenciales API.";
                            DAlegra.Update(requestId, token, "NEEDS_REVIEW", remote, number, emission,
                                reason + " La factura ya existe; corregir el acceso y reintentar esta misma solicitud sin crear otra venta.");
                            return null;
                        }
                        var stampBody=Object(stamped.Json);
                        var results=stampBody.ContainsKey("data") ? stampBody["data"] as object[] : null;
                        var result=results==null ? null : results.OfType<Dictionary<string,object>>().FirstOrDefault(item=>Text(item,"id")==remote);
                        if (result==null || (!Text(result,"success").Equals("True",StringComparison.OrdinalIgnoreCase) && Text(result,"code")!="3226"))
                        {
                            DAlegra.Update(requestId,token,"NEEDS_REVIEW",remote,number,emission,"Alegra no confirmó la emisión de este ID. HTTP "+stamped.StatusCode+".");
                            return null;
                        }
                        string stampEmission = Emission(result);
                        if (stampEmission == "ACCEPTED" || stampEmission == "STAMPED_AND_ACCEPTED" || stampEmission == "STAMPED_AND_ACCEPTED_WITH_OBSERVATIONS")
                        {
                            DAlegra.Update(requestId,token,"ACCEPTED",remote,number,stampEmission,null);
                            return saleId;
                        }
                        DAlegra.Update(requestId,token,"ISSUING",remote,number,emission,null);
                    }
                }
            }
            catch (Exception ex)
            {
                bool network = ex is System.Net.Http.HttpRequestException || ex is OperationCanceledException;
                string status=posting && string.IsNullOrWhiteSpace(remote) ? "UNCERTAIN"
                    : posting && !string.IsNullOrWhiteSpace(remote) ? "ISSUING"
                    : network ? Convert.ToString(row["Status"]) : "NEEDS_REVIEW";
                string message=ex is InvalidOperationException || ex is ArgumentException ? ex.Message : "Error de conexión o respuesta. Revisar la solicitud antes de reintentar.";
                DAlegra.Update(requestId,token,status,remote,null,null,message);
            }
            return null;
        }

        public static bool Matches(Dictionary<string,object> contact,Dictionary<string,object> fiscal)
        {
            var id=contact.ContainsKey("identificationObject") ? contact["identificationObject"] as Dictionary<string,object> : null;
            return id!=null && Text(id,"number")==Text(fiscal,"Identificación") && Text(id,"type")==Text(fiscal,"identificationType");
        }
        public static string Emission(Dictionary<string,object> invoice)
        {
            foreach (string key in new[]{"emission_status","emissionStatus"}) if (!string.IsNullOrWhiteSpace(Text(invoice,key))) return Text(invoice,key);
            var stamp=invoice.ContainsKey("stamp") ? invoice["stamp"] as Dictionary<string,object> : null;
            foreach (string key in new[]{"emission_status","emissionStatus","legalStatus"}) if (!string.IsNullOrWhiteSpace(Text(stamp,key))) return Text(stamp,key);
            return string.Empty;
        }
        public static Dictionary<string,object> BuildContact(Dictionary<string,object> fiscal)
        {
            var result=new Dictionary<string,object> {
                {"identificationObject",new {number=Text(fiscal,"Identificación"),type=Text(fiscal,"identificationType"),dv=Text(fiscal,"Dígito de verificación (NIT)")}},
                {"regime",Text(fiscal,"regime")},{"kindOfPerson",Text(fiscal,"kindOfPerson")},
                {"email",Text(fiscal,"Correo electrónico")},{"phonePrimary",Text(fiscal,"Teléfono")},{"type",new[]{"client"}},
                {"address",new {address=Text(fiscal,"Dirección fiscal"),city=Text(fiscal,"Municipio"),department=Text(fiscal,"Departamento"),country=Text(fiscal,"País")}}
            };
            if (Text(fiscal,"kindOfPerson")=="PERSON_ENTITY") result.Add("nameObject",new {firstName=Text(fiscal,"Primer nombre"),secondName=Text(fiscal,"Segundo nombre"),lastName=Text(fiscal,"Apellidos")});
            else result.Add("name",Text(fiscal,"Nombre o razón social"));
            return result;
        }
    }
}
