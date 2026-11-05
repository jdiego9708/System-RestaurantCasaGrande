using CapaNegocio.Alegra;
using CapaPresentacion.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace CapaPresentacion.Formularios.FormsVentas
{
    public sealed class FrmAlegraInvoiceDetails : Form
    {
        private static readonly CultureInfo Culture=CultureInfo.GetCultureInfo("es-CO");
        private readonly ToolTip tips=new ToolTip {AutoPopDelay=20000};
        private static readonly Dictionary<string,string> Labels=new Dictionary<string,string> {
            {"id","ID en Alegra"},{"name","Nombre"},{"description","Descripción"},{"price","Precio unitario base"},
            {"quantity","Cantidad"},{"discount","Descuento (%)"},{"reference","Referencia"},{"productKey","Código de producto"},
            {"unit","Unidad de medida"},{"tax","Impuestos"},{"total","Total"},{"amount","Valor"},{"percentage","Tarifa (%)"},
            {"date","Fecha"},{"datetime","Fecha y hora"},{"dueDate","Vencimiento"},{"paymentMethod","Medio de pago"},
            {"paymentForm","Forma de pago"},{"payments","Pagos"},{"status","Estado"},{"observations","Observaciones"},
            {"anotation","Nota"},{"createdAt","Fecha de creación"},{"updatedAt","Última actualización"},{"prefix","Prefijo"},
            {"number","Número"},{"fullNumber","Número de factura"},{"client","Cliente"},{"items","Productos"},
            {"identification","Identificación"},{"identificationType","Tipo de documento"},{"identificationObject","Documento de identidad"},
            {"phonePrimary","Teléfono principal"},{"phoneSecondary","Teléfono adicional"},{"mobile","Celular"},{"email","Correo electrónico"},
            {"address","Dirección"},{"department","Departamento"},{"city","Ciudad"},{"country","País"},{"regime","Régimen tributario"},
            {"kindOfPerson","Tipo de persona"},{"branchOffice","Sucursal"},{"stamp","Emisión electrónica"},{"legalStatus","Estado ante DIAN"},
            {"cufe","CUFE"},{"barCodeContent","Contenido del QR"},{"warnings","Observaciones de validación"},
            {"subtotal","Subtotal"},{"totalPaid","Pagado"},{"balance","Saldo pendiente"},{"numberTemplate","Numeración"},
            {"termsConditions","Términos y condiciones"},{"warehouse","Bodega"},{"seller","Vendedor"},{"costCenter","Centro de costos"},
            {"printingTemplate","Plantilla de impresión"},{"originApp","Aplicación de origen"},{"operationType","Tipo de operación"},
            {"type","Tipo"},{"rate","Tasa"},{"deductible","Deducible"},{"decimalPrecision","Decimales"},{"term","Plazo"},
            {"priceList","Lista de precios"},{"station","Punto de venta"},{"fax","Fax"},
            {"categoryFavorable","Categoría de saldo a favor"},{"categoryToBePaid","Categoría por pagar"},
            {"categorySalesReturns","Categoría de devoluciones de venta"},{"categoryPurchaseReturns","Categoría de devoluciones de compra"}
        };
        private static readonly Dictionary<string,string> Values=new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase) {
            {"closed","Cerrada"},{"open","Abierta"},{"draft","Borrador"},{"void","Anulada"},
            {"active","Activo"},{"inactive","Inactivo"},{"ACCEPTED","Aceptada"},{"STAMPED_AND_ACCEPTED","Aceptada por DIAN"},
            {"STAMPED_AND_ACCEPTED_WITH_OBSERVATIONS","Aceptada con observaciones"},{"REJECTED","Rechazada"},
            {"STAMPED_AND_REJECTED","Rechazada por DIAN"},{"CASH","Contado"},{"CREDIT","Crédito"},
            {"PERSON_ENTITY","Persona natural"},{"LEGAL_ENTITY","Persona jurídica"},{"NATIONAL","Nacional"},{"STANDARD","Estándar"},
            {"debit-transfer","Transferencia débito"},{"credit-transfer","Transferencia crédito"},{"credit-card","Tarjeta de crédito"},
            {"debit-card","Tarjeta débito"}
        };

        public FrmAlegraInvoiceDetails(Dictionary<string,object> invoice)
        {
            Text="Detalle de factura de Alegra · "+AlegraWorkflow.Text(invoice,"id");
            ClientSize=new Size(1100,760);MinimumSize=new Size(820,580);StartPosition=FormStartPosition.CenterParent;
            Font=Theme.Body;BackColor=Theme.Background;Padding=new Padding(18);DoubleBuffered=true;
            var heading=new Label {Dock=DockStyle.Top,Height=50,Text="Factura "+Nested(invoice,"numberTemplate","fullNumber")+" · "+Nested(invoice,"client","name")};
            heading.StyleLabel(LabelKind.Title);
            var summary=new FlowLayoutPanel {Dock=DockStyle.Top,Height=80,WrapContents=true,AutoScroll=true};
            foreach(var key in new[]{"date","status","total","totalPaid","balance"})
                if(invoice.ContainsKey(key))AddBadge(summary,Label(key),Display(key,invoice[key]),key=="total"?BadgeKind.Success:BadgeKind.Neutral);
            var tabs=new TabControl {Dock=DockStyle.Fill};
            AddTab(tabs,"Productos y tributos",Curate(Get(invoice,"items"),new[]{"name","quantity","unit","price","discount","total","reference","productKey","description","tax"}),"Producto");
            AddTab(tabs,"Pagos",Curate(Get(invoice,"payments"),new[]{"number","date","amount","paymentMethod","status","observations","anotation"}),"Pago");
            AddTab(tabs,"Cliente",Curate(Get(invoice,"client"),new[]{"name","identification","identificationObject","identificationType","email","phonePrimary","phoneSecondary","mobile","address","regime","kindOfPerson","branchOffice"}),"Datos del cliente");
            var emission=new Dictionary<string,object>();
            foreach(var key in new[]{"stamp","observations","anotation","termsConditions"})if(invoice.ContainsKey(key)&&invoice[key]!=null&&(!(invoice[key] is string note)||!string.IsNullOrWhiteSpace(note)))emission[key]=invoice[key];
            AddTab(tabs,"Emisión y observaciones",emission,"Información de emisión");
            var allPage=new TabPage("Todos los datos") {BackColor=Theme.Background};tabs.TabPages.Add(allPage);
            bool loaded=false;
            tabs.SelectedIndexChanged+=(s,e)=>{if(tabs.SelectedTab==allPage&&!loaded){loaded=true;Populate(allPage,invoice,"Factura completa");}};
            Controls.Add(tabs);Controls.Add(summary);Controls.Add(heading);
            KeyPreview=true;KeyDown+=(s,e)=>{if(e.KeyCode==Keys.Escape)Close();};
        }
        protected override void Dispose(bool disposing){if(disposing)tips.Dispose();base.Dispose(disposing);}
        private static object Get(Dictionary<string,object> data,string key)=>data.ContainsKey(key)?data[key]:null;
        private static object Curate(object value,string[] fields)
        {
            if(value is Dictionary<string,object> map)
            {
                var selected=new Dictionary<string,object>();
                foreach(var key in fields)
                    if(map.ContainsKey(key) && map[key]!=null && (!(map[key] is string text)||!string.IsNullOrWhiteSpace(text)))
                        selected[key]=key=="tax"?Curate(map[key],new[]{"name","percentage","amount","type"}):map[key];
                return selected;
            }
            if(value is IEnumerable sequence && !(value is string))return sequence.Cast<object>().Select(item=>Curate(item,fields)).ToArray();
            return value;
        }
        private static string Nested(Dictionary<string,object> data,string key,string child)=>AlegraWorkflow.Text(Get(data,key) as Dictionary<string,object>,child);
        private static string Label(string key)=>Labels.ContainsKey(key)?Labels[key]:Regex.Replace(key,"([a-z])([A-Z])","$1 $2");
        private static string Display(string key,object value)
        {
            if(value==null)return "No informado";
            if(value is bool flag)return flag?"Sí":"No";
            string text=Convert.ToString(value,Culture);
            if(new[]{"price","total","subtotal","totalPaid","balance","amount"}.Contains(key) && decimal.TryParse(text,NumberStyles.Number,Culture,out decimal amount))return amount.ToString("C2",Culture)+" COP";
            if(key=="paymentMethod"&&text.Equals("cash",StringComparison.OrdinalIgnoreCase))return "Efectivo";
            if(key=="unit"&&text=="unit")return "Unidad";
            if(key=="identificationType"||key=="type")
            {
                if(text=="NIT"||text=="31")return "NIT";
                if(text=="CC"||text=="13")return "Cédula de ciudadanía";
            }
            if(Values.TryGetValue(text,out string friendly))return friendly;
            return string.IsNullOrWhiteSpace(text)?"No informado":text;
        }
        private void AddTab(TabControl tabs,string caption,object value,string title)
        {var page=new TabPage(caption) {BackColor=Theme.Background};tabs.TabPages.Add(page);Populate(page,value,title);}
        private void Populate(TabPage page,object value,string title)
        {
            var host=new FlowLayoutPanel {Dock=DockStyle.Fill,AutoScroll=true,FlowDirection=FlowDirection.TopDown,WrapContents=false,Padding=new Padding(8)};
            page.Controls.Add(host);host.SuspendLayout();AddGroup(host,title,value);
            host.SizeChanged+=(s,e)=>{foreach(Control card in host.Controls)card.Width=Math.Max(260,host.ClientSize.Width-32);};
            foreach(Control card in host.Controls)card.Width=Math.Max(260,host.ClientSize.Width-32);
            host.ResumeLayout(true);
        }
        private void AddGroup(FlowLayoutPanel host,string title,object value)
        {
            if(value is IEnumerable sequence && !(value is string) && !(value is Dictionary<string,object>))
            {
                int index=0;foreach(var entry in sequence)AddGroup(host,title+" "+(++index),entry);
                if(index==0)AddGroup(host,title,"Sin registros");return;
            }
            var map=value as Dictionary<string,object>;
            string name=map==null?"":AlegraWorkflow.Text(map,"name");
            var card=new ModernCard {Width=950,Height=110,Margin=new Padding(2,2,2,10),AccentColor=Theme.Primary};
            var body=new FlowLayoutPanel {Dock=DockStyle.Top,AutoSize=true,FlowDirection=FlowDirection.TopDown,WrapContents=false,BackColor=Theme.Surface};
            card.Controls.Add(body);
            body.Controls.Add(new Label {Text=title+(string.IsNullOrWhiteSpace(name)?"":" · "+name),Font=Theme.BodyBold,AutoSize=true,MaximumSize=new Size(850,0),Margin=new Padding(0,0,0,10)});
            var badges=new FlowLayoutPanel {AutoSize=true,WrapContents=true,BackColor=Theme.Surface,Width=900,Margin=new Padding(0)};body.Controls.Add(badges);
            card.SizeChanged+=(s,e)=>{body.Width=Math.Max(220,card.Width-card.Padding.Horizontal);badges.MaximumSize=new Size(body.Width,0);badges.Width=body.Width;foreach(Control c in body.Controls)if(c!=badges){if(c is TextBox){int height=c.Text.Length>180?90:52;c.MaximumSize=new Size(body.Width,height);c.MinimumSize=new Size(100,height);c.Size=new Size(body.Width,height);}else c.MaximumSize=new Size(body.Width,0);}};
            body.SizeChanged+=(s,e)=>card.Height=body.Height+card.Padding.Vertical;
            host.Controls.Add(card);
            if(map==null)
            {
                string text=Display("",value);
                if(text.Length>90)body.Controls.Add(new TextBox {Text=text,ReadOnly=true,Multiline=true,ScrollBars=ScrollBars.Vertical,Width=850,Height=90,MinimumSize=new Size(100,90),BackColor=Theme.Surface,Font=Theme.Body});
                else AddBadge(badges,"",text,BadgeKind.Neutral);
                return;
            }
            foreach(var pair in map)
            {
                if(pair.Key=="name"&&!string.IsNullOrWhiteSpace(name))continue;
                if(pair.Value is Dictionary<string,object> || pair.Value is IEnumerable && !(pair.Value is string))continue;
                string text=Display(pair.Key,pair.Value);
                if(text.Length>90 || new[]{"cufe","barCodeContent","observations","anotation","termsConditions","description"}.Contains(pair.Key))
                {
                    body.Controls.Add(new Label {Text=Label(pair.Key),Font=Theme.BodyBold,AutoSize=true,Margin=new Padding(0,6,0,2)});
                    body.Controls.Add(new TextBox {Text=text,ReadOnly=true,Multiline=true,ScrollBars=ScrollBars.Vertical,BorderStyle=BorderStyle.FixedSingle,Width=850,Height=text.Length>180?90:52,MinimumSize=new Size(100,text.Length>180?90:52),BackColor=Theme.Surface,Font=Theme.Body});
                }
                else AddBadge(badges,Label(pair.Key),text,pair.Key=="total"||pair.Key=="amount"?BadgeKind.Success:pair.Key=="legalStatus"?BadgeKind.Warning:BadgeKind.Neutral);
            }
            foreach(var pair in map)
            {
                if(pair.Key=="tax"&&title.StartsWith("Producto ")&&pair.Value is IEnumerable taxes)
                {
                    foreach(var tax in taxes.OfType<Dictionary<string,object>>())
                        AddBadge(badges,AlegraWorkflow.Text(tax,"name"),AlegraWorkflow.Text(tax,"percentage")+"% · "+Display("amount",Get(tax,"amount")),BadgeKind.Info);
                }
                else if(pair.Value is Dictionary<string,object> || pair.Value is IEnumerable && !(pair.Value is string))AddGroup(host,title+" · "+Label(pair.Key),pair.Value);
            }
        }
        private void AddBadge(FlowLayoutPanel host,string label,string value,BadgeKind kind)
        {
            string text=(string.IsNullOrEmpty(label)?"":label+": ")+value;
            var badge=new Badge {Text=text,Kind=kind,Font=Theme.Small,Height=30,Width=Math.Min(620,Math.Max(85,TextRenderer.MeasureText(text,Theme.Small).Width+28)),Margin=new Padding(0,0,7,7)};
            tips.SetToolTip(badge,text);host.Controls.Add(badge);
        }
    }
}
