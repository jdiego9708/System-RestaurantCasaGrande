using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using CapaNegocio.Alegra;

class AlegraContractTests
{
    static int passed;
    static void Assert(bool value, string name) { if (!value) throw new Exception(name); passed++; }
    static void Reject(Action action, string name) { try { action(); } catch (InvalidOperationException) { passed++; return; } throw new Exception(name); }

    sealed class FakeHandler : HttpMessageHandler
    {
        public HttpStatusCode Code = HttpStatusCode.OK;
        public string LastBody;
        public int Calls;
        public string LastUri;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            Calls++;
            LastUri=request.RequestUri.AbsoluteUri;
            Assert(request.RequestUri.Scheme == "https", "HTTPS");
            Assert(request.Headers.Authorization.Scheme == "Basic", "Basic auth");
            LastBody = request.Content == null ? null : await request.Content.ReadAsStringAsync();
            return new HttpResponseMessage(Code) { Content = new StringContent("{\"invoice\":{\"id\":\"remote-id\"}}") };
        }
    }

    static void Main()
    {
        var allocations = AlegraAllocation.AllocateGross(new decimal[] { 1, 1, 1 }, 100);
        Assert(allocations.Sum() == 100 && allocations[0] == 34, "Restos mayores y desempate estable");
        var credentials = (AlegraCredentials)typeof(AlegraCredentials).GetMethod("FromReader", BindingFlags.NonPublic | BindingFlags.Static)
            .Invoke(null, new object[] { new Func<string, string>(name => name == "ALEGRA_EMAIL" ? "test@example.invalid" : name == "ALEGRA_API_KEY" ? "fake-token" : null) });
        var bearer = (AlegraCredentials)typeof(AlegraCredentials).GetMethod("FromReader", BindingFlags.NonPublic | BindingFlags.Static)
            .Invoke(null, new object[] { new Func<string, string>(name => name == "ALEGRA_EMAIL" ? "test@example.invalid" : name == "ALEGRA_API_KEY" ? "fake-token" : "Bearer") });
        Assert(bearer.CreateAuthorizationHeader().Scheme == "Bearer", "Token recomendado usa Bearer");
        Assert(!credentials.ToString().Contains("fake-token"), "Sin token en ToString");
        var fake = new FakeHandler();
        using (var api = new AlegraClient(fake, credentials))
        {
            api.GetCompanyAsync(CancellationToken.None).GetAwaiter().GetResult();
            fake.Code = HttpStatusCode.BadRequest;
            var response = api.CreateInvoiceAsync("{}", CancellationToken.None).GetAwaiter().GetResult();
            Assert(!response.Success && response.Json.Contains("remote-id"), "400 conserva cuerpo para conciliar");
            Assert(fake.Calls == 2, "Sin reintento POST");
            fake.Code = HttpStatusCode.Forbidden;
            Reject(() => api.GetCompanyAsync(CancellationToken.None).GetAwaiter().GetResult(), "403 denegado");
            fake.Code = HttpStatusCode.OK;
            api.StampInvoiceAsync("uuid-test", CancellationToken.None).GetAwaiter().GetResult();
            Assert(fake.LastBody == "{\"ids\":[\"uuid-test\"]}", "Contrato emision");
            api.GetInvoicesAsync(new DateTime(2026,11,1),new DateTime(2026,11,3),30,CancellationToken.None).GetAwaiter().GetResult();
            Assert(fake.LastUri.Contains("date_afterOrNow=2026-11-01")&&fake.LastUri.Contains("date_beforeOrNow=2026-11-03"),"Rango inclusivo por fecha");
            Assert(fake.LastUri.Contains("start=30&limit=30"),"Paginacion remota");
            try {api.GetInvoicesAsync(DateTime.Today,DateTime.Today.AddDays(-1),0,CancellationToken.None);throw new Exception("Acepto rango invalido");}
            catch(ArgumentException){passed++;}
        }
        var serializer = new JavaScriptSerializer();
        var products = new[] {
            new { Tipo="PLATO", Id_tipo=1, Nombre="Plato", Precio=10800, Cantidad=1 },
            new { Tipo="BEBIDA", Id_tipo=2, Nombre="Bebida", Precio=5400, Cantidad=1 }
        };
        string snapshot = serializer.Serialize(new {
            orderId=42, saleValues=new[] {"42","16200","0","16200","0","","0","0","15000",""},
            products=products, payments=new[] {new {Pago="EFECTIVO",Valor_pago=5000},new {Pago="TRANSFERENCIA",Valor_pago=10000}}
        });
        var config = new AlegraInvoiceConfiguration { ClientId="customer-uuid",NumberTemplateId="template-uuid",FiscalPaymentMethod="CASH" };
        config.Products.Add("PLATO:1",new AlegraItemMapping { RemoteId="item-1",PercentageTaxes=new Dictionary<string,decimal>{{"tax-inc",8}} });
        config.Products.Add("BEBIDA:2",new AlegraItemMapping { RemoteId="item-2",PercentageTaxes=new Dictionary<string,decimal>{{"tax-inc",8}} });
        config.PaymentAccounts.Add("EFECTIVO","cash-account"); config.PaymentMethods.Add("EFECTIVO","cash");
        config.PaymentAccounts.Add("TRANSFERENCIA","bank-account"); config.PaymentMethods.Add("TRANSFERENCIA","transfer");
        string json = AlegraInvoiceBuilder.Build(snapshot,config,new DateTime(2026,11,2));
        Assert(json.Contains("customer-uuid") && json.Contains("template-uuid"),"IDs texto");
        Assert(!json.Contains("generateStamp"),"No emitir al crear");
        var parsed = serializer.DeserializeObject(json) as Dictionary<string,object>;
        Assert((string)parsed["paymentMethod"]=="BANK_DEPOSIT","Pago mixto: la cabecera usa el medio del pago mayor (transferencia)");
        string cashOnly=AlegraInvoiceBuilder.Build(snapshot.Replace("\"Pago\":\"TRANSFERENCIA\"","\"Pago\":\"EFECTIVO\""),config,new DateTime(2026,11,2));
        Assert((string)((Dictionary<string,object>)serializer.DeserializeObject(cashOnly))["paymentMethod"]=="CASH","Solo efectivo: la cabecera es CASH");
        string transferOnly=AlegraInvoiceBuilder.Build(snapshot.Replace("\"Pago\":\"EFECTIVO\"","\"Pago\":\"TRANSFERENCIA\""),config,new DateTime(2026,11,2));
        Assert((string)((Dictionary<string,object>)serializer.DeserializeObject(transferOnly))["paymentMethod"]=="BANK_DEPOSIT","Solo transferencia: la cabecera no es CASH");
        var items = (object[])parsed["items"];
        decimal gross = 0;
        foreach(Dictionary<string,object> item in items) gross += Convert.ToDecimal(item["price"]) * Convert.ToDecimal(item["quantity"]) * 1.08m;
        Assert(Math.Abs(gross-15000) < 0.01m,"Impuestos incluidos y total editado");
        // Precio personalizado: solo cambia la linea editada; el total de la factura pasa a 12000 + 5000 y los pagos lo suman.
        string customSnapshot=snapshot.Replace("\"Nombre\":\"Plato\",\"Precio\":10800,\"Cantidad\":1","\"Nombre\":\"Plato\",\"Precio\":10800,\"Cantidad\":1,\"ElectronicPrice\":12000");
        Assert(customSnapshot!=snapshot,"La prueba de precio personalizado modifico el snapshot");
        Assert(AlegraInvoiceBuilder.ExpectedTotal(customSnapshot,config)==17000,"El total esperado usa el precio personalizado");
        var custom=serializer.DeserializeObject(AlegraInvoiceBuilder.Build(customSnapshot,config,new DateTime(2026,11,11))) as Dictionary<string,object>;
        var customItems=((object[])custom["items"]).Cast<Dictionary<string,object>>().ToArray();
        Assert(Math.Abs(Convert.ToDecimal(customItems[0]["price"])*1.08m-12000)<0.01m,"El item lleva el precio personalizado sin impuestos");
        Assert(Math.Abs(Convert.ToDecimal(customItems[1]["price"])*1.08m-5000)<0.01m,"Las demas lineas no cambian");
        Assert(((object[])custom["payments"]).Cast<Dictionary<string,object>>().Sum(p=>Convert.ToDecimal(p["amount"]))==17000,"Los pagos suman el total personalizado");
        Reject(()=>AlegraInvoiceBuilder.Build(customSnapshot.Replace("\"ElectronicPrice\":12000","\"ElectronicPrice\":0"),config,DateTime.Today),"Rechazar precio personalizado en cero");
        string localizedSnapshot=serializer.Serialize(new {
            orderId=43, saleValues=new[] {"43","16200","0,00","16200","0","","0,00","0,00","15000",""},
            products=products, payments=new[] {new {Pago="EFECTIVO",Valor_pago="15000,00"}}
        });
        string localizedJson=AlegraInvoiceBuilder.Build(localizedSnapshot,config,new DateTime(2026,11,6));
        var localized=serializer.DeserializeObject(localizedJson) as Dictionary<string,object>;
        var localizedPayment=(Dictionary<string,object>)((object[])localized["payments"])[0];
        Assert(Convert.ToDecimal(localizedPayment["amount"])==15000,"Coma decimal es-CO conserva el importe real del pago");
        Reject(()=>AlegraInvoiceBuilder.Build(localizedSnapshot.Replace("15000,00","1.5000,00"),config,DateTime.Today),"No aceptar separadores ambiguos como miles");
        config.Products.Remove("PLATO:1");
        Reject(() => AlegraInvoiceBuilder.Build(snapshot,config,DateTime.Today),"Rechazar producto sin vinculo");
        // Factura parcial: solo BEBIDA:2 (5400 de 16200 -> 5000 de los 15000 de la venta) y pagos repartidos en proporcion.
        Assert(AlegraInvoiceBuilder.UnlinkedProducts(snapshot,config).SequenceEqual(new[]{"Plato"}),"Detectar el producto sin vinculo");
        Assert(AlegraInvoiceBuilder.AnyLinked(snapshot,config),"Hay al menos un producto vinculado");
        string partialSnapshot=snapshot.Replace("\"orderId\":42","\"onlyLinked\":true,\"orderId\":42");
        decimal partialTotal=AlegraInvoiceBuilder.ExpectedTotal(partialSnapshot,config);
        Assert(partialTotal==5000,"El total esperado es solo el de los productos vinculados");
        Assert(AlegraInvoiceBuilder.ExcludedProducts(partialSnapshot,config).SequenceEqual(new[]{"Plato"}),"Lista de productos omitidos");
        var partialInvoice=serializer.DeserializeObject(AlegraInvoiceBuilder.Build(partialSnapshot,config,new DateTime(2026,11,6))) as Dictionary<string,object>;
        Assert(((object[])partialInvoice["items"]).Length==1,"La factura parcial solo lleva el producto vinculado");
        decimal partialPaid=((object[])partialInvoice["payments"]).Cast<Dictionary<string,object>>().Sum(p=>Convert.ToDecimal(p["amount"]));
        Assert(partialPaid==5000,"Los pagos de la factura parcial suman el total de la factura");
        config.Products.Remove("BEBIDA:2");
        Reject(() => AlegraInvoiceBuilder.Build(partialSnapshot,config,DateTime.Today),"Nada vinculado: no hay nada que facturar");
        Assert(!AlegraInvoiceBuilder.AnyLinked(snapshot,config),"Ningun producto vinculado");
        // Catalogo de departamentos y municipios de Alegra (grafia exacta) y cambios de contacto.
        Assert(AlegraPlaces.Departments.Count==33,"33 departamentos en el catalogo");
        Assert(AlegraPlaces.Municipalities("Caldas").Contains("Manizales"),"Manizales pertenece a Caldas");
        Assert(AlegraPlaces.Municipalities("Bogotá D.C.").SequenceEqual(new[]{"Bogotá, D.C."}),"Bogota: departamento y municipio con grafia distinta");
        Assert(AlegraPlaces.CanonicalDepartment("valle del cauca")=="Valle del Cauca","Departamento sin distinguir mayusculas");
        Assert(AlegraPlaces.CanonicalMunicipality("Antioquia","MEDELLIN")=="Medellín","Municipio sin distinguir tildes");
        Assert(AlegraPlaces.CanonicalMunicipality("Caldas","Cali")==null,"Un municipio de otro departamento no es valido");
        var contact=new Dictionary<string,object>{
            {"id","9"},{"name","LAURA HOYOS"},{"email","a@x.co"},{"phonePrimary","300"},{"kindOfPerson","PERSON_ENTITY"},{"regime","SIMPLIFIED_REGIME"},
            {"identificationObject",new Dictionary<string,object>{{"number","1053"},{"type","CC"},{"dv",""}}},
            {"nameObject",new Dictionary<string,object>{{"firstName","LAURA"},{"secondName",""},{"lastName","HOYOS"}}},
            {"address",new Dictionary<string,object>{{"address","Cll 1"},{"department","Caldas"},{"city","Manizales"},{"country","Colombia"}}}};
        var form=new Dictionary<string,string>{
            {"identificationType","CC"},{"kindOfPerson","PERSON_ENTITY"},{"regime","SIMPLIFIED_REGIME"},{"Dígito de verificación (NIT)",""},
            {"Nombre o razón social","LAURA HOYOS"},{"Primer nombre","LAURA"},{"Segundo nombre",""},{"Apellidos","HOYOS"},
            {"Correo electrónico","a@x.co"},{"Teléfono","300"},{"Dirección fiscal","Cll 1"},{"Departamento","Caldas"},{"Municipio","Manizales"},{"País","Colombia"}};
        Assert(AlegraWorkflow.ContactChanges(contact,form).Count==0,"Sin cambios no se envia nada");
        form["Correo electrónico"]="nuevo@x.co"; form["Municipio"]="Villamaría";
        var changes=AlegraWorkflow.ContactChanges(contact,form);
        Assert(changes.Count==2 && changes.ContainsKey("email") && changes.ContainsKey("address"),"Solo viajan los campos modificados (correo y direccion completa)");
        Assert(!changes.ContainsKey("name") && !changes.ContainsKey("nameObject") && !changes.ContainsKey("phonePrimary"),"Los campos intactos no se envian");
        Console.WriteLine("OK: " + passed + " comprobaciones sin red, credenciales reales ni escrituras en BD.");
    }
}
