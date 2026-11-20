<# Verifica medios previstos/reales, pagos mixtos, cache y detalle expandido en una base temporal. #>
param([string]$Instance = '(localdb)\MSSQLLocalDB', [string]$BuildDirectory)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Data
$repo = Split-Path $PSScriptRoot -Parent
$build = if ($BuildDirectory) { (Resolve-Path -LiteralPath $BuildDirectory).Path } else { Join-Path $repo 'SISMistico\SISMistico\bin\LocalBuild' }
$database = 'CasaGrandeDeliveryPaymentsAudit_' + [guid]::NewGuid().ToString('N')
$created = $false
function Execute-Sql([string]$catalog, [string]$sql) {
    $connection = New-Object System.Data.SqlClient.SqlConnection "Server=$Instance;Database=$catalog;Integrated Security=true"
    try {
        $connection.Open()
        $command = $connection.CreateCommand()
        $command.CommandText = $sql
        [void]$command.ExecuteNonQuery()
    } finally { $connection.Dispose() }
}
$schema = @'
CREATE TABLE dbo.Pedidos (Id_pedido int PRIMARY KEY, Fecha_pedido date, Tipo_pedido varchar(50), Estado_pedido varchar(50), Payment_method nvarchar(60));
CREATE TABLE dbo.Ventas (Id_venta int PRIMARY KEY, Id_pedido int, Estado varchar(50));
CREATE TABLE dbo.Detalle_venta (Id_venta int, Metodo_pago nvarchar(60), Valor_pago int);
INSERT dbo.Pedidos VALUES
(1,'20261119','DOMICILIO','PENDIENTE','Efectivo'),
(2,'20261119','DOMICILIO','PENDIENTE','Transferencia'),
(3,'20261119','DOMICILIO','PENDIENTE',NULL),
(4,'20261119','DOMICILIO','FACTURADO','Efectivo'),
(5,'20261119','DOMICILIO','FACTURADO','Tarjeta'),
(6,'20261119','DOMICILIO','CANCELADO','Efectivo'),
(7,'20261119','MESA','FACTURADO','Efectivo'),
(8,'20261118','DOMICILIO','FACTURADO','Efectivo');
INSERT dbo.Ventas VALUES (10,4,'ACTIVO'),(11,4,'ACTIVO'),(12,4,'INACTIVO'),(13,4,'ACTIVO'),(14,5,'ACTIVO');
INSERT dbo.Detalle_venta VALUES (10,'Efectivo',99999),(11,'Efectivo',15000),(11,'Efectivo',5000),(11,'Tarjeta',35000),(12,'Efectivo',100000),(14,'Nequi',10000);
'@
$source = @'
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CapaDatos;
using CapaEntidades.Models;
using CapaPresentacion.Formularios.FormsDomicilios;
class DeliveryPaymentsCheck {
 static readonly BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
 static void Check(bool ok,string message) {if(!ok)throw new Exception(message);}
 static object Field(object target,string name) {return target.GetType().GetField(name,Flags).GetValue(target);}
 static object Call(object target,string name,params object[] args) {return target.GetType().GetMethod(name,Flags).Invoke(target,args);}
 static void Sql(string text) {using(var connection=new SqlConnection(Conexion.Cn)) {connection.Open();using(var command=new SqlCommand(text,connection))command.ExecuteNonQuery();}}
 [STAThread] static int Main(string[] args) {
  AppDomain.CurrentDomain.AssemblyResolve+=(s,e)=> {string path=Path.Combine(args[0],new AssemblyName(e.Name).Name+".dll");return File.Exists(path)?Assembly.LoadFrom(path):null;};
  try {Run(args[1],args[2]);return 0;}catch(Exception ex) {Console.Error.WriteLine(ex);return 1;}
 }
 static void Run(string connection,string preview) {
  Conexion.Cn=connection;
  DateTime day=new DateTime(2026,11,19);
  var table=DVenta.GetDeliveryPayments(day);
  Check(table.Rows.Count==6,"Incluyo pedidos de otro dia, cancelados o ventas de mesa.");
  DataRow pending=table.Select("Id_pedido=1").Single();
  Check((string)pending["PaymentMethod"]=="Efectivo" && pending.IsNull("Amount") && !(bool)pending["IsFinal"],"El medio previsto se confundio con un pago real.");
  Check(table.Select("Id_pedido=3").Single().IsNull("PaymentMethod"),"Invento el medio no elegido.");
  var mixed=table.Select("Id_pedido=4");
  Check(mixed.Length==2 && mixed.All(row=>(int)row["Id_venta"]==11 && (bool)row["IsFinal"]),"Mezclo ventas antiguas, inactivas o sin pagos.");
  Check(Convert.ToDecimal(mixed.Single(row=>(string)row["PaymentMethod"]=="Efectivo")["Amount"])==20000
    && Convert.ToDecimal(mixed.Single(row=>(string)row["PaymentMethod"]=="Tarjeta")["Amount"])==35000,"No recupero los pagos mixtos reales.");
  using(var form=(FrmDeliveries)Activator.CreateInstance(typeof(FrmDeliveries),true)) {
   try {
    var cache=(Dictionary<int,DeliveryDetails>)Field(form,"detailsCache");
    foreach(int id in new[]{1,2,3,4,5})cache[id]=new DeliveryDetails {Loaded=true,CustomerName="CLIENTE DE PRUEBA",Phone="3147219270",Number=id};
    string signature=(string)Call(form,"CapturePaymentDetails",day);
    var current=(DeliveryDetails)Call(form,"DetailsOf",new Pedidos {Id_pedido=1});
    Check(current.PaymentsLoaded && !current.PaymentsFinal && current.Payments.Count==1 && current.Payments[0].Method=="Efectivo","El detalle no recibe el medio previsto.");
    var actual=(DeliveryDetails)Call(form,"DetailsOf",new Pedidos {Id_pedido=4});
    Check(actual.PaymentsLoaded && actual.PaymentsFinal && actual.Payments.Count==2,"El detalle no recibe todos los medios reales.");
    using(var root=new Panel {Size=new Size(800,800),BackColor=Color.White}) {
     int x=0;
     foreach(DeliveryStage stage in new[]{DeliveryStage.Preparing,DeliveryStage.Billed}) {
      var card=new DeliveryCard {Width=380,CompactView=true,Location=new Point(x,0)};
      var details=stage==DeliveryStage.Preparing ? current : actual;
      card.Bind(new Pedidos {Id_pedido=(int)stage+100,Estado_pedido="PENDIENTE",Hora_pedido="12:00"},details,stage,DateTime.Now);
      root.Controls.Add(card);
      int collapsed=card.Height;
      ((Button)Field(card,"expandButton")).PerformClick();
      Check(card.Height>collapsed,"No despliega el detalle de pagos.");
      foreach(bool compact in new[]{false,true}) {
       card.CompactView=compact;
       using(var bitmap=new Bitmap(card.Width,card.Height))card.DrawToBitmap(bitmap,card.ClientRectangle);
      }
      if(!(bool)Field(card,"compactExpanded"))((Button)Field(card,"expandButton")).PerformClick();
      x+=400;
     }
     using(var bitmap=new Bitmap(root.Width,root.Height)) {root.DrawToBitmap(bitmap,root.ClientRectangle);bitmap.Save(preview);}
    }
    Sql("INSERT dbo.Ventas VALUES (20,1,'ACTIVO'); INSERT dbo.Detalle_venta VALUES (20,'Nequi',24000);");
    string billed=(string)Call(form,"CapturePaymentDetails",day);
    current=(DeliveryDetails)Call(form,"DetailsOf",new Pedidos {Id_pedido=1});
    Check(billed!=signature && current.PaymentsFinal && current.Payments.Count==1 && current.Payments[0].Method=="Nequi" && current.Payments[0].Amount==24000,"La cache conserva el medio previsto despues de facturar.");
    Sql("UPDATE dbo.Detalle_venta SET Valor_pago=18000 WHERE Id_venta=11 AND Valor_pago=15000;");
    string corrected=(string)Call(form,"CapturePaymentDetails",day);
    actual=(DeliveryDetails)Call(form,"DetailsOf",new Pedidos {Id_pedido=4});
    Check(corrected!=billed && actual.Payments.Single(p=>p.Method=="Efectivo").Amount==23000,"Una correccion de pagos no se refleja en el detalle.");
    Sql("ALTER TABLE dbo.Pedidos DROP COLUMN Payment_method;");
    var legacy=DVenta.GetDeliveryPayments(day);
    Check(legacy.Select("Id_pedido=4").Length==2 && legacy.Select("Id_pedido=2").Single().IsNull("PaymentMethod"),"Falla sin la columna de medio previsto.");
   } finally {foreach(string name in new[]{"refreshTimer","clockTimer","searchDelay"})((System.Windows.Forms.Timer)Field(form,name)).Dispose();}
  }
  Console.WriteLine("OK: medios previstos, pagos mixtos reales, ultima venta activa con pagos, filtros, cache tras facturar/corregir, ambas vistas expandidas y compatibilidad de esquema.");
 }
}
'@
$temp = Join-Path $env:TEMP ('CasaGrandeDeliveryPaymentsCheck_' + [guid]::NewGuid().ToString('N'))
[void](New-Item -ItemType Directory -Path $temp)
$sourcePath = Join-Path $temp 'DeliveryPaymentsCheck.cs'
$exe = Join-Path $temp 'DeliveryPaymentsCheck.exe'
try {
    Execute-Sql 'master' "CREATE DATABASE [$database]"
    $created = $true
    Execute-Sql $database $schema
    [IO.File]::WriteAllText($sourcePath,$source)
    $references = @('CapaDatos.dll','CapaEntidades.dll','CapaPresentacion.dll') | ForEach-Object { '/r:' + (Join-Path $build $_) }
    & "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /nologo "/out:$exe" /r:System.Windows.Forms.dll /r:System.Drawing.dll /r:System.Data.dll /r:System.Data.DataSetExtensions.dll @references $sourcePath
    if ($LASTEXITCODE) { throw 'No compilo la prueba de medios de pago.' }
    Copy-Item -LiteralPath (Join-Path $build 'SISMistico.exe.config') -Destination "$exe.config"
    & $exe $build "Server=$Instance;Database=$database;Integrated Security=true" (Join-Path $temp 'DeliveryPayments.png')
    if ($LASTEXITCODE) { throw 'Fallo la prueba de medios de pago.' }
    Write-Output (Join-Path $temp 'DeliveryPayments.png')
} finally {
    if ($created -and $database -match '^CasaGrandeDeliveryPaymentsAudit_[a-f0-9]{32}$') {
        Execute-Sql 'master' "ALTER DATABASE [$database] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$database];"
    }
}
