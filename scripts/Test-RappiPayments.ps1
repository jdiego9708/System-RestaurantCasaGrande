<#
    Comprueba lectura, persistencia y grid de pagos mixtos de Rappi en una base temporal.
    Requiere la compilacion local: .\SISMistico\run-local.ps1 -NoRun
    No importa PDFs reales ni modifica las ordenes del usuario.
#>
param([string]$Instance = '(localdb)\MSSQLLocalDB', [string]$BuildDirectory)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Data
$repo = Split-Path $PSScriptRoot -Parent
$build = if ($BuildDirectory) { (Resolve-Path -LiteralPath $BuildDirectory).Path } else { Join-Path $repo 'SISMistico\SISMistico\bin\LocalBuild' }
$database = 'CasaGrandeRappiAudit_' + [guid]::NewGuid().ToString('N')
$created = $false
function Execute-Sql([string]$catalog, [string]$sql) {
    $builder = New-Object System.Data.SqlClient.SqlConnectionStringBuilder
    $builder['Data Source'] = $Instance
    $builder['Initial Catalog'] = $catalog
    $builder['Integrated Security'] = $true
    $connection = New-Object System.Data.SqlClient.SqlConnection $builder.ConnectionString
    try {
        $connection.Open()
        $command = $connection.CreateCommand()
        $command.CommandText = $sql
        [void]$command.ExecuteNonQuery()
    } finally { $connection.Dispose() }
}

$source = @'
using System;
using System.Data;
using System.Reflection;
using System.Windows.Forms;
using CapaDatos;
using CapaEntidades.Models;
using CapaNegocio.Rappi;
using CapaPresentacion.Formularios.FormsVentas;

class RappiPaymentsCheck
{
    static void Check(bool condition, string message) { if (!condition) throw new Exception(message); }
    static RappiOrder Parse(int id, string method, string payable, string paid, bool splitLines = false)
    {
        var lines = new System.Collections.Generic.List<string> {
            "CLIENTE DE PRUEBA", "ID " + id, "Fecha orden: 2026-11-19 12:00",
            "Direcci\u00f3n: DIRECCION DE PRUEBA",
            splitLines ? "FORMA DE PAGO" : "M\u00e9todo de pago: " + method
        };
        if (splitLines) lines.Add(method);
        lines.AddRange(new[] { "DETALLE ORDEN", "1x PRODUCTO $30.000", "Subtotal $30.000" });
        if (paid != null) lines.Add("Valor pagado a trav\u00e9s de Rappi " + paid);
        if (payable != null) {
            lines.Add(splitLines ? "TOTAL A COBRAR" : "Total a cobrar al cliente " + payable);
            if (splitLines) lines.Add(payable);
        }
        lines.Add("El cliente va a pagar con $50.000");
        return RappiInvoiceParser.ParseLines(lines);
    }

    [STAThread] static int Main(string[] args)
    {
        try {
            Conexion.Cn = args[0];
            var mixed = Parse(90001, "Efectivo", "$10.000", "-$20.000", true);
            Check(mixed.PaymentMethod == "Efectivo" && mixed.Total == 10000 && mixed.PaidThroughRappi == -20000,
                "No leyo forma de pago y total a cobrar en lineas separadas.");
            Check(mixed.CashPayWith == 50000 && mixed.CashAmount == 10000 && mixed.OtherPaymentAmount == 20000,
                "Confundio el efectivo a cobrar con el billete del cliente o con el prepago de Rappi.");
            Check(mixed.IsComplete && mixed.OrderValue == 30000, "Total de pedido mixto incorrecto.");
            var card = Parse(90002, "RappiCard", "$0", "-$30.000");
            var cash = Parse(90003, " EFECTIVO ", "$30.000", null);
            var terminal = Parse(90004, "Dat\u00e1fono", "$10.000", "$20.000");
            var transfer = Parse(90005, "Transferencia", "$10.000", "-$20.000");
            var missingMethod = Parse(90006, "", "$10.000", "-$20.000");
            var missingTotal = Parse(90007, "Efectivo", null, "-$20.000");
            Check(card.CashAmount == 0 && card.OtherPaymentAmount == 30000, "RappiCard contado como efectivo.");
            Check(cash.CashAmount == 30000 && cash.OtherPaymentAmount == 0, "Venta solo en efectivo incorrecta.");
            Check(terminal.CashAmount == 0 && terminal.OtherPaymentAmount == 30000, "Datafono contado como efectivo.");
            Check(transfer.CashAmount == 0 && transfer.OtherPaymentAmount == 30000, "Transferencia contada como efectivo.");
            Check(!missingMethod.IsComplete && missingMethod.CashAmount == null && missingMethod.OtherPaymentAmount == null,
                "Invento la forma de pago ausente o no marco la orden para revision.");
            Check(missingTotal.CashAmount == null && missingTotal.OtherPaymentAmount == null, "Invento un total ausente.");
            foreach (var order in new[] { mixed, card, cash, terminal, transfer, missingMethod, missingTotal })
                Check(DRappiOrders.InsertIfNew(order), "No guardo una orden de prueba.");
            Check(!DRappiOrders.InsertIfNew(mixed), "Duplico una orden importada.");

            var stored = DRappiOrders.GetByExternalId("90001");
            Check(stored.Total == 10000 && stored.PaidThroughRappi == -20000 && stored.CashAmount == 10000
                && stored.OtherPaymentAmount == 20000, "Se perdio el desglose al recuperar la orden guardada.");
            var table = DRappiOrders.ListRange(new DateTime(2026,11,19), new DateTime(2026,11,19));
            var row = table.Select("ExternalId = '90001'")[0];
            Check((int)row["CashAmount"] == 10000 && (int)row["OtherPaymentAmount"] == 20000 && (int)row["Total"] == 30000,
                "La consulta del historial no recupero el desglose persistido.");
            foreach (DataRow sale in table.Rows)
                if (sale["CashAmount"] != DBNull.Value && sale["OtherPaymentAmount"] != DBNull.Value)
                    Check((int)sale["CashAmount"] + (int)sale["OtherPaymentAmount"] == (int)sale["Total"], "Conto dos veces un pago.");

            var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            using (var form = new FrmObservarVentas()) {
                form.BindingContext = new BindingContext();
                var grid = (DataGridView)form.GetType().GetField("dgvVentas", flags).GetValue(form);
                grid.BindingContext = form.BindingContext;
                var handle = form.Handle; handle = grid.Handle;
                form.GetType().GetField("showingRappi", flags).SetValue(form, true);
                form.GetType().GetMethod("BindResults", flags).Invoke(form, new object[] { table });
                Check(grid.Columns["CashAmount"].Visible && grid.Columns["CashAmount"].HeaderText == "Efectivo"
                    && grid.Columns["OtherPaymentAmount"].Visible && grid.Columns["OtherPaymentAmount"].HeaderText == "Otros medios",
                    "Faltan las columnas de pagos en el grid.");
                table.DefaultView.Sort = "CashAmount DESC";
                form.GetType().GetMethod("ShowPage", flags).Invoke(form, new object[] { 0, false });
                Check(Convert.ToInt32(grid.Rows[0].Cells["CashAmount"].Value) == 30000, "No ordena por efectivo real.");
                table.DefaultView.RowFilter = "ExternalId = '90001'";
                form.GetType().GetMethod("ShowPage", flags).Invoke(form, new object[] { 0, true });
                Check(grid.Rows.Count == 1 && Convert.ToInt32(grid.Rows[0].Cells["CashAmount"].Value) == 10000
                    && Convert.ToInt32(grid.Rows[0].Cells["OtherPaymentAmount"].Value) == 20000,
                    "Los importes cambiaron al filtrar el grid.");
                Check(Convert.ToString(grid.Rows[0].Cells["ResultRowNumber"].FormattedValue).StartsWith("R - "), "Perdio el numero asignado de Rappi.");
            }
            for(int i=8;i<=11;i++) Check(DRappiOrders.InsertIfNew(Parse(90000+i,"Efectivo","$10.000",null)),"No preparo los consecutivos R10/R11.");
            var day=new DateTime(2026,11,19);
            int matches;
            foreach(var query in new[]{"R1","r 1"," R - 001 ","R#1"}) {
                var found=DRappiOrders.SearchDay(day,query,0,10,out matches);
                Check(matches==1 && found.Count==1 && found[0].ExternalId=="90001","R1 no filtra por consecutivo exacto: "+query);
            }
            var eleven=DRappiOrders.SearchDay(day,"R11",0,10,out matches);
            Check(matches==1 && eleven.Count==1 && eleven[0].DayNumber==11,"R11 no recupera el numero asignado.");
            var emptyPage=DRappiOrders.SearchDay(day,"R1",1,1,out matches);
            Check(matches==1 && emptyPage.Count==0,"El conteo y la paginacion usan distintos filtros.");
            var general=DRappiOrders.SearchDay(day,"PRUEBA",0,4,out matches);
            Check(matches==11 && general.Count==4,"Cambio el buscador general de Rappi.");
            using(var connection=new System.Data.SqlClient.SqlConnection(Conexion.Cn)) {
                connection.Open();
                using(var command=connection.CreateCommand()) {
                    command.CommandText="DROP INDEX UX_RappiOrders_Day_Number ON dbo.RappiOrders; ALTER TABLE dbo.RappiOrders DROP COLUMN DayNumber;";
                    command.ExecuteNonQuery();
                }
            }
            Check(DRappiOrders.SearchDay(day,"R1",0,10,out matches).Count==0 && matches==0,"Falla el buscador en una base sin numeracion.");
            Console.WriteLine("OK: lectura de etiquetas, efectivo, pago mixto, RappiCard, datafono, transferencia, datos ausentes, persistencia, duplicados, grid, orden y filtros.");
            Console.WriteLine("OK: R1/R11, variantes, coincidencia exacta, conteo/paginacion y busqueda general; compatibilidad sin numeracion.");
            return 0;
        } catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
    }
}
'@
$sourcePath = Join-Path $env:TEMP ('RappiPaymentsCheck-' + [guid]::NewGuid().ToString('N') + '.cs')
$testExe = Join-Path $build 'RappiPaymentsCheck.exe'
try {
    if (!(Test-Path (Join-Path $build 'SISMistico.exe.config'))) { throw 'Primero compile con SISMistico\run-local.ps1 -NoRun.' }
    Execute-Sql 'master' "CREATE DATABASE [$database]"
    $created = $true
    # El script 11 numera tambien domicilios; basta su esquema minimo para esta prueba de Rappi.
    Execute-Sql $database 'CREATE TABLE dbo.Pedidos (Id_pedido int PRIMARY KEY, Fecha_pedido date, Hora_pedido time, Tipo_pedido varchar(50));'
    foreach ($file in @('10_rappi_ordenes.sql','11_numeracion_domicilios.sql')) {
        $sql = [IO.File]::ReadAllText((Join-Path $repo "db\$file"))
        foreach ($batch in [regex]::Split($sql, '(?m)^GO[ \t]*\r?$\n?')) {
            if ($batch.Trim()) { Execute-Sql $database $batch }
        }
    }
    [IO.File]::WriteAllText($sourcePath, $source, [Text.Encoding]::UTF8)
    $references = @('CapaEntidades.dll','CapaDatos.dll','CapaNegocio.dll','CapaPresentacion.dll') |
        ForEach-Object { '/r:' + (Join-Path $build $_) }
    & "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /nologo "/out:$testExe" /r:System.Windows.Forms.dll /r:System.Data.dll /r:System.Drawing.dll @references $sourcePath
    if ($LASTEXITCODE) { throw 'No compilo la prueba de pagos Rappi.' }
    Copy-Item -LiteralPath (Join-Path $build 'SISMistico.exe.config') -Destination "$testExe.config" -Force
    & $testExe "Server=$Instance;Database=$database;Integrated Security=true"
    if ($LASTEXITCODE) { throw 'Fallo la prueba de pagos Rappi.' }
} finally {
    foreach ($artifact in @($testExe, "$testExe.config", $sourcePath)) {
        if (Test-Path -LiteralPath $artifact) { Remove-Item -LiteralPath $artifact -Force }
    }
    if ($created -and $database -match '^CasaGrandeRappiAudit_[a-f0-9]{32}$') {
        Execute-Sql 'master' "ALTER DATABASE [$database] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$database];"
    }
}
