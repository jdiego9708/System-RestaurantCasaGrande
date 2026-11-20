<#
    Verifica el desplazamiento tactil de Domicilios sin abrir la ventana, consultar SQL ni imprimir.
    Comprueba la configuracion nativa, limites, inercia, aislamiento de listas, clics y recambio de fichas.
    La prueba fisica con el digitalizador se realiza en el equipo tactil.
#>
param([string]$BuildDirectory)
$ErrorActionPreference = 'Stop'
$repo = Split-Path $PSScriptRoot -Parent
if (!$BuildDirectory) { $BuildDirectory = Join-Path $repo 'SISMistico\SISMistico\bin\LocalBuild' }
$build = (Resolve-Path -LiteralPath $BuildDirectory).Path
$temp = Join-Path $env:TEMP ('CasaGrandeTouchCheck_' + [guid]::NewGuid().ToString('N'))
[void](New-Item -ItemType Directory -Path $temp)
$source = @'
using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using CapaEntidades.Models;
using CapaEntidades.Helpers;
using CapaPresentacion.Formularios.FormsDomicilios;

class DeliveryTouchCheck
{
    static readonly BindingFlags Private = BindingFlags.NonPublic | BindingFlags.Instance;
    static void Check(bool ok, string message) { if (!ok) throw new Exception(message); }
    static object Field(object target, string name) { return target.GetType().GetField(name, Private).GetValue(target); }
    static object Call(object target, string name, params object[] args) { return target.GetType().GetMethod(name, Private | BindingFlags.Public).Invoke(target, args); }
    static void Pan(Control list, int y, uint flags) { Call(list, "ApplyPan", y, flags); }
    static bool CanActivate(Control list, bool touch) { return (bool)Call(list, "CanActivate", touch); }

    [StructLayout(LayoutKind.Sequential)] struct GestureConfig { public uint Id, Want, Block; }
    [DllImport("user32.dll", SetLastError=true)] static extern bool GetGestureConfig(IntPtr hwnd, uint reserved, uint flags, ref uint count, ref GestureConfig config, uint size);
    [DllImport("user32.dll")] static extern IntPtr SetMessageExtraInfo(IntPtr info);
    [DllImport("user32.dll")] static extern IntPtr SendMessage(IntPtr hwnd, int message, IntPtr wParam, IntPtr lParam);
    static void NativeConfig(Control control)
    {
        uint count = 1;
        var config = new GestureConfig { Id = 4 };
        Check(GetGestureConfig(control.Handle, 0, 0, ref count, ref config, (uint)Marshal.SizeOf(typeof(GestureConfig))), "Windows no registro la configuracion de gestos.");
        Check((config.Want & 0x12) == 0x12 && (config.Block & 4) == 4, "Falta pan vertical/inercia o no bloquea el arrastre horizontal.");
    }
    static Control List(Type type)
    {
        var list = (FlowLayoutPanel)Activator.CreateInstance(type);
        list.Size = new Size(350,160);
        list.AutoScroll = true;
        list.FlowDirection = FlowDirection.TopDown;
        list.WrapContents = false;
        list.Controls.Add(new Panel { Size=new Size(280,1000), Margin=Padding.Empty });
        list.CreateControl();
        list.PerformLayout();
        return list;
    }
    static void CheckCard(Control card)
    {
        var buttons = card.Controls.OfType<Button>().Where(b => b.Visible).ToArray();
        foreach(var button in buttons) {
            Check(button.Height >= 44 && button.Width >= 44, "Boton pequeno para tactil: " + button.Text);
            Check(card.ClientRectangle.Contains(button.Bounds), "Boton fuera de la ficha: " + button.Text);
        }
        for(int i=0;i<buttons.Length;i++) for(int j=i+1;j<buttons.Length;j++)
            Check(!buttons[i].Bounds.IntersectsWith(buttons[j].Bounds), "Botones encimados: " + buttons[i].Text + "/" + buttons[j].Text);
    }
    static void CheckNeutral(Button button)
    {
        using(var image=new Bitmap(button.Width,button.Height)) {
            button.DrawToBitmap(image,button.ClientRectangle);
            int ink=0;
            for(int x=0;x<image.Width;x++) for(int y=0;y<image.Height;y++) {
                Color pixel=image.GetPixel(x,y);
                Check(!(pixel.B-pixel.R>80 && pixel.B-pixel.G>50),"El boton minimalista tiene azul.");
                if(x>12 && x<image.Width-12 && y>12 && y<image.Height-12 && pixel.R<170) ink++;
            }
            Check(ink>3,"El icono Lucide no se dibujo.");
        }
    }
    [STAThread] static int Main(string[] args)
    {
        AppDomain.CurrentDomain.AssemblyResolve += (sender,e) => {
            string path=Path.Combine(args[0],new AssemblyName(e.Name).Name+".dll");
            return File.Exists(path) ? Assembly.LoadFrom(path) : null;
        };
        try { Run(args[1]); return 0; }
        catch(Exception ex) { Console.Error.WriteLine(ex); return 1; }
    }
    static void Run(string temp)
    {
        var assembly = typeof(RappiCard).Assembly;
        var icons=assembly.GetType("CapaPresentacion.UI.LucideIcons",true);
        foreach(var icon in new[]{"list","layout-grid","chevron-up","chevron-down","printer","ellipsis"})
            Check((bool)icons.GetMethod("Contains",BindingFlags.Static|BindingFlags.Public).Invoke(null,new object[]{icon}),"Icono Lucide ausente: "+icon);
        using(var license=new StreamReader(assembly.GetManifestResourceStream("CapaPresentacion.Resources.Lucide.LICENSE"))) {
            string text=license.ReadToEnd();
            Check(text.Contains("ISC License") && text.Contains("Cole Bemis"),"Falta licencia/atribucion de Lucide y Feather.");
        }
        Type listType = assembly.GetType("CapaPresentacion.Formularios.FormsDomicilios.DeliveryTouchList", true);
        Type buttonType = assembly.GetType("CapaPresentacion.Formularios.FormsDomicilios.DeliveryTouchButton", true);
        using(var list = (FlowLayoutPanel)List(listType))
        using(var other = (FlowLayoutPanel)List(listType)) {
            NativeConfig(list);
            NativeConfig(list.Controls[0]);
            Pan(list,500,1); Pan(list,400,0);
            Check(-list.AutoScrollPosition.Y==100, "El deslizamiento no sigue el dedo.");
            Pan(list,300,2); Pan(list,250,6);
            Check(-list.AutoScrollPosition.Y==250, "Se interrumpio la fase de inercia.");
            Check(other.AutoScrollPosition.Y==0, "Un gesto desplazo otra columna.");
            Check(!CanActivate(list,true) && CanActivate(list,false), "La proteccion tactil afecta al raton o permite una accion al deslizar.");

            var card = new Panel { Size=new Size(280,100) };
            var action = (Button)Activator.CreateInstance(buttonType);
            card.Controls.Add(action);
            list.Controls.Add(card);
            NativeConfig(action);
            int clicks = 0; action.Click += (sender,e) => clicks++;
            IntPtr saved = SetMessageExtraInfo(new IntPtr(unchecked((long)0xFF515780)));
            try { Call(action,"OnClick",EventArgs.Empty); }
            finally { SetMessageExtraInfo(saved); }
            Check(clicks==0, "Un dedo que termino de desplazar activo una accion.");
            saved = SetMessageExtraInfo(IntPtr.Zero);
            try { Call(action,"OnClick",EventArgs.Empty); }
            finally { SetMessageExtraInfo(saved); }
            Check(clicks==1, "Se bloqueo un clic normal del raton.");
            Thread.Sleep(320);
            saved = SetMessageExtraInfo(new IntPtr(unchecked((long)0xFF515780)));
            try { Call(action,"OnClick",EventArgs.Empty); }
            finally { SetMessageExtraInfo(saved); }
            Check(clicks==2, "Un toque posterior al desplazamiento no funciono.");

            Pan(list,500,1); Pan(list,30000,0);
            Check(list.AutoScrollPosition.Y==0, "El gesto sobrepaso el inicio.");
            Pan(list,-30000,4);
            int maximum=Math.Max(0,list.VerticalScroll.Maximum-list.VerticalScroll.LargeChange+1);
            Check(-list.AutoScrollPosition.Y==maximum, "El gesto sobrepaso el final.");
            Call(action,"RecreateHandle"); NativeConfig(action);
            Pan(list,500,1);
            SendMessage(action.Handle,0x001F,IntPtr.Zero,IntPtr.Zero);
            Check(!((bool)Field(list,"panning")),"Una interrupcion de Windows dejo el gesto activo.");
            int count=((IDictionary)Field(list,"windows")).Count;
            list.Controls.Remove(card); card.Dispose();
            Check(((IDictionary)Field(list,"windows")).Count==count-2,"El refresco de fichas dejo manejadores registrados.");
            Pan(list,500,1); list.Controls.Clear();
            Check(!((bool)Field(list,"panning")),"Al retirar una ficha quedo un gesto activo.");
        }
        foreach(int width in new[] { 280,320,380,500 }) {
            using(var card = new DeliveryCard()) {
                card.Width=width;
                foreach(DeliveryStage stage in Enum.GetValues(typeof(DeliveryStage))) {
                    card.Bind(new Pedidos { Id_pedido=99000+width, Hora_pedido="12:00", Estado_pedido="PENDIENTE" },
                        new DeliveryDetails { CustomerName="PRUEBA TACTIL", Phone=" 3147219270 ", Loaded=true },stage,DateTime.Now);
                    Check((string)card.GetType().GetProperty("ContactText",Private).GetValue(card,null)=="Contacto: 3147219270",
                        "No muestra el contacto en preparacion/terminados.");
                    CheckCard(card);
                    int cardHeight = card.Height;
                    card.CompactView = true;
                    Check(card.Height==72 && !((Control)Field(card,"ring")).Visible,"La lista de domicilios no se compacto a dos lineas.");
                    CheckCard(card);
                    CheckNeutral((Button)Field(card,"expandButton"));
                    CheckNeutral((Button)Field(card,"moreButton"));
                    Check(((Control)Field(card,"printButton")).Visible==(stage==DeliveryStage.Billed)
                        && ((Control)Field(card,"moreButton")).Visible==(stage!=DeliveryStage.Billed),
                        "Los terminados facturados no tienen impresion directa.");
                    Call(card,"BuildCompactActions");
                    var menu=(ContextMenuStrip)Field(card,"compactMenu");
                    Check(menu.Items.Count==(stage==DeliveryStage.Preparing ? 5 : stage==DeliveryStage.AwaitingBilling ? 4 : 1),"El menu compacto no respeta la etapa.");
                    foreach(ToolStripItem item in menu.Items) Check(item.Height>=44,"Menu de acciones pequeno para tactil.");
                    if(stage==DeliveryStage.Preparing) {
                        object billed=null; EventHandler handler=(sender,e)=>billed=sender;
                        card.BillRequested += handler;
                        menu.Items[0].PerformClick();
                        Check(billed==card.Order,"El menu compacto no ejecuta la accion del pedido.");
                        card.BillRequested -= handler;
                    }
                    ((Button)Field(card,"expandButton")).PerformClick();
                    Check(card.Height>72,"No abre el detalle desde la lista.");
                    ((Button)Field(card,"expandButton")).PerformClick();
                    Check(card.Height==72,"No vuelve a las dos lineas.");
                    card.CompactView=false;
                    Check(card.Height==cardHeight,"No restaura la ficha original.");
                    CheckCard(card);
                }
            }
            using(var card = new RappiCard()) {
                card.Width=width;
                card.Bind(new RappiOrder { ExternalId="TEST-TOUCH", DayNumber=1, CustomerName=" PRUEBA TACTIL ", Phone=" 300 123 4567 ", Total=10000, PaymentMethod="Efectivo" });
                var contact=card.GetType().GetProperty("CustomerContact",Private);
                Check((string)contact.GetValue(card,null)=="PRUEBA TACTIL - 300 123 4567","No une el cliente y el telefono con guion.");
                CheckCard(card);
                CheckNeutral((Button)Field(card,"expandButton"));
                CheckNeutral((Button)Field(card,"printButton"));
                int height=card.Height;
                card.CompactView=true;
                int compactHeight=card.Height;
                Check(compactHeight>=96 && compactHeight<=116 && !((Control)Field(card,"ring")).Visible,"Rappi no muestra el resumen de pagos en lista.");
                CheckCard(card);
                card.Order.Phone=" ";
                Check((string)contact.GetValue(card,null)=="PRUEBA TACTIL","Telefono ausente deja un guion sobrante.");
                card.Order.Phone="3001234567"; card.Order.CustomerName=null;
                Check((string)contact.GetValue(card,null)=="3001234567","Un telefono sin nombre no se muestra correctamente.");
                ((Button)Field(card,"expandButton")).PerformClick();
                Check(card.Height>compactHeight,"No muestra el detalle de Rappi en lista.");
                ((Button)Field(card,"expandButton")).PerformClick();
                Check(card.Height==compactHeight,"No compacta de nuevo la orden Rappi.");
                card.Order.CustomerName=" PRUEBA TACTIL "; card.Order.Phone=" 300 123 4567 ";
                card.CompactView=false;
                Check(card.Height==height,"No restaura la ficha Rappi.");
                CheckCard(card);
            }
        }
        using(var card = new RappiCard { Width=400 }) {
            card.Bind(new RappiOrder { ExternalId="TEST-JORGE", DayNumber=3, CustomerName="Jorge", Phone="3147219270",
                Total=17850, PaidThroughRappi=-10000, PaymentMethod="Efectivo", OrderDateTime=DateTime.Now.AddHours(-2) });
            Check((string)card.GetType().GetProperty("TotalText",Private).GetValue(card,null)=="Total: $27.850","Total incorrecto del pedido mixto.");
            Check((string)card.GetType().GetProperty("CashText",Private).GetValue(card,null)=="Efectivo: $17.850","El efectivo incluye lo ya pagado en Rappi.");
            foreach(bool compact in new[]{false,true}) {
                card.CompactView=compact;
                Check((string)card.GetType().GetProperty("CountdownText",Private).GetValue(card,null)==string.Empty
                    && !((Control)Field(card,"ring")).Visible && !((System.Windows.Forms.Timer)Field(card,"clock")).Enabled,
                    "El contador agotado no desaparecio.");
                using(var image=new Bitmap(400,50)) using(var graphics=Graphics.FromImage(image)) {
                    graphics.Clear(Color.White);
                    Call(card,"ComposePaymentSummary",graphics,12,0,376,string.Empty);
                    int red=0;
                    for(int x=12;x<388;x++) for(int y=0;y<44;y++) {
                        Color pixel=image.GetPixel(x,y);
                        if(pixel.R>150 && pixel.R-pixel.G>70 && pixel.R-pixel.B>70) red++;
                    }
                    Check(red>30,"No resalta el efectivo en rojo.");
                }
            }
            card.Order.Total=0; card.Order.PaidThroughRappi=-27850; card.Order.PaymentMethod="RappiCard";
            foreach(bool compact in new[]{false,true}) {
                card.CompactView=compact;
                Check((string)card.GetType().GetProperty("CashText",Private).GetValue(card,null)==string.Empty,"Muestra Efectivo: $0.");
                Check((string)card.GetType().GetProperty("TotalText",Private).GetValue(card,null)=="Total: $27.850"
                    && card.Order.PaymentMethod=="RappiCard","Ocultar efectivo altero el total o el metodo de pago.");
                using(var image=new Bitmap(400,50)) using(var graphics=Graphics.FromImage(image)) {
                    graphics.Clear(Color.White);
                    Call(card,"ComposePaymentSummary",graphics,12,0,376,string.Empty);
                    int red=0;
                    for(int x=12;x<388;x++) for(int y=0;y<44;y++) {
                        Color pixel=image.GetPixel(x,y);
                        if(pixel.R>150 && pixel.R-pixel.G>70 && pixel.R-pixel.B>70) red++;
                    }
                    Check(red==0,"El resumen conserva texto rojo de un efectivo de cero.");
                }
            }
            card.Order.OrderDateTime=DateTime.Now.AddMinutes(-5);
            Call(card,"ClockTick");
            Check((string)card.GetType().GetProperty("CountdownText",Private).GetValue(card,null)!=string.Empty,"Oculta un contador que todavia tiene tiempo.");
        }
        using(var panel = new RappiPanel()) {
            Check(Field(panel,"list").GetType()==listType,"El panel Rappi no usa la lista tactil.");
            var list=(Control)Field(panel,"list");
            var card=new RappiCard { CompactView=true };
            card.Bind(new RappiOrder { ExternalId="TEST-VIEW", Total=10000 });
            list.Controls.Add(card);
            var bar=Field(panel,"searchBar");
            Check((bool)bar.GetType().GetProperty("CompactView").GetValue(bar,null),"Rappi no inicia en lista.");
            var selector=(Button)Field(bar,"viewButton");
            var copies=bar.GetType().GetProperty("Copies");
            int previousCopies=(int)copies.GetValue(bar,null);
            selector.PerformClick();
            CheckNeutral(selector);
            Check(!card.CompactView && (int)copies.GetValue(bar,null)==previousCopies,"El selector no aplica fichas o cambia las copias.");
            selector.PerformClick();
            Check(card.CompactView,"El selector no restaura lista.");
        }
        var factory=typeof(FrmDeliveries).GetMethod("NewList",BindingFlags.Static|BindingFlags.NonPublic);
        using(var list=(Control)factory.Invoke(null,null)) Check(list.GetType()==listType,"Preparacion/Terminados no usan listas tactiles.");
        int parsed;
        foreach(var query in new[]{"N1","n 1"," N - 001 ","N\u00b01","N\u00ba 1","N#1","N\u20131"})
            Check(OrderNumberSearch.TryParse(query,'N',out parsed) && parsed==1,"No reconoce el consecutivo: "+query);
        foreach(var query in new[]{"R1","r - 1","R #1"})
            Check(OrderNumberSearch.TryParse(query,'R',out parsed) && parsed==1,"No reconoce Rappi: "+query);
        foreach(var query in new[]{"N0","N-","N999999999999999999999","cliente N1","N1 cliente","1","R1"})
            Check(!OrderNumberSearch.TryParse(query,'N',out parsed),"Interpreto texto normal como consecutivo: "+query);
        using(var form=(FrmDeliveries)Activator.CreateInstance(typeof(FrmDeliveries),true)) {
            foreach(var name in new[]{"preparingSearchBar","finishedSearchBar"}) {
                var bar=Field(form,name);
                Check((bool)bar.GetType().GetProperty("CompactView").GetValue(bar,null),"Un panel de domicilios no inicia en lista.");
            }
            var cache=(System.Collections.Generic.Dictionary<int,DeliveryDetails>)Field(form,"detailsCache");
            var orders=new System.Collections.Generic.List<Pedidos> {
                new Pedidos {Id_pedido=8001}, new Pedidos {Id_pedido=8002}, new Pedidos {Id_pedido=8003}
            };
            cache[8001]=new DeliveryDetails {Number=1,Loaded=true,CustomerName="Julian",Phone="3147219270"};
            cache[8002]=new DeliveryDetails {Number=10,Loaded=true,CustomerName="Cliente N1",Phone="3000000000"};
            cache[8003]=new DeliveryDetails {Number=11,Loaded=true,CustomerName="Otro cliente"};
            foreach(var query in new[]{"N1","n - 1","N\u00b0 1"}) {
                var found=(System.Collections.Generic.List<Pedidos>)Call(form,"Filter",orders,query);
                Check(found.Count==1 && found[0].Id_pedido==8001,"N1 incluye N10/N11 o coincidencias por nombre.");
            }
            foreach(var query in new[]{"Julian","3147219270","8001"}) {
                var found=(System.Collections.Generic.List<Pedidos>)Call(form,"Filter",orders,query);
                Check(found.Count==1 && found[0].Id_pedido==8001,"Cambio la busqueda normal: "+query);
            }
            foreach(var name in new[]{"refreshTimer","clockTimer","searchDelay"}) ((System.Windows.Forms.Timer)Field(form,name)).Dispose();
        }
        Console.WriteLine("OK: configuracion nativa en listas y controles hijos, desplazamiento, inercia, limites, columnas independientes, bloqueo de acciones al deslizar, toques/raton, recreacion y retirada de fichas, botones de 44px sin solaparse (280 a 500px).");
        Console.WriteLine("OK: variantes de N/R, busqueda exacta de consecutivos y busqueda habitual de domicilios.");
    }
}
'@
$sourcePath = Join-Path $temp 'DeliveryTouchCheck.cs'
$testExe = Join-Path $temp 'DeliveryTouchCheck.exe'
[IO.File]::WriteAllText($sourcePath, $source)
$references = @('CapaEntidades.dll','CapaPresentacion.dll') | ForEach-Object { '/r:' + (Join-Path $build $_) }
& "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /nologo "/out:$testExe" /r:System.Windows.Forms.dll /r:System.Data.dll /r:System.Drawing.dll @references $sourcePath
if ($LASTEXITCODE) { throw 'No compilo la prueba tactil.' }
Copy-Item -LiteralPath (Join-Path $build 'SISMistico.exe.config') -Destination "$testExe.config"
& $testExe $build $temp
if ($LASTEXITCODE) { throw 'Fallo la prueba tactil.' }
