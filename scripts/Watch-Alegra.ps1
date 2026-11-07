param([switch]$Once, [int]$IntervalSeconds = 2)
$ErrorActionPreference = 'Stop'
$configPath = Join-Path $PSScriptRoot '..\SISMistico\SISMistico\bin\LocalBuild\SISMistico.exe.config'
[xml]$config = Get-Content $configPath -Raw
$name = ($config.configuration.appSettings.add | Where-Object key -eq 'ConexionBaseDeDatos').value
$entry = $config.configuration.connectionStrings.add | Where-Object name -eq $name
if (-not $entry) { throw 'No se encontro la conexion activa en el ejecutable local.' }
Add-Type -AssemblyName System.Data
$previous = ''
Write-Host 'Seguimiento de Alegra (solo lectura). Ctrl+C para salir.' -ForegroundColor Cyan
do {
    $connection = New-Object System.Data.SqlClient.SqlConnection $entry.connectionString
    try {
        $connection.Open()
        $command = $connection.CreateCommand()
        $command.CommandText = "IF OBJECT_ID('dbo.AlegraInvoiceRequests') IS NULL THROW 50001, 'Falta aplicar db/07_alegra_integration.sql en la base activa.', 1; SELECT TOP (10) SaleId, Status, RemoteId, InvoiceNumber, EmissionStatus, Attempts, PrintStatus, LastError, UpdatedAt FROM dbo.AlegraInvoiceRequests ORDER BY CreatedAt DESC"
        $adapter = New-Object System.Data.SqlClient.SqlDataAdapter $command
        $table = New-Object System.Data.DataTable
        [void]$adapter.Fill($table)
        $current = if ($table.Rows.Count) { $table | Format-List | Out-String -Width 180 } else { 'Todavia no hay solicitudes de factura electronica.' }
        if ($current -ne $previous) {
            Write-Host (Get-Date -Format 'yyyy-MM-dd HH:mm:ss') -ForegroundColor Cyan
            Write-Host $current
            $previous = $current
        }
    } finally { $connection.Dispose() }
    if (-not $Once) { Start-Sleep -Seconds $IntervalSeconds }
} while (-not $Once)
