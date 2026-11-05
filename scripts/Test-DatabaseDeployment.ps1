<#
    Verifica el paso 01 autocontenido y aplica las migraciones dos veces sobre una base TEMPORAL vacia.
    No modifica CasaGrandeBD ni CasaGrandeBD_prod. Elimina unicamente la base temporal que crea.
    Uso: .\scripts\Test-DatabaseDeployment.ps1
#>
[CmdletBinding()]
param([string]$Instance = '(localdb)\MSSQLLocalDB')
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Data
$repo = Split-Path $PSScriptRoot -Parent
$dbDir = Join-Path $repo 'db'
. (Join-Path $PSScriptRoot 'Preparar-USB.ps1')

function Execute-Sql([string]$database, [string]$sql, [switch]$Results) {
    $builder = New-Object System.Data.SqlClient.SqlConnectionStringBuilder
    $builder['Data Source'] = $Instance
    $builder['Initial Catalog'] = $database
    $builder['Integrated Security'] = $true
    $connection = New-Object System.Data.SqlClient.SqlConnection $builder.ConnectionString
    try {
        $connection.Open()
        $command = $connection.CreateCommand()
        $command.CommandText = $sql
        $command.CommandTimeout = 120
        if ($Results) {
            $data = New-Object System.Data.DataSet
            [void](New-Object System.Data.SqlClient.SqlDataAdapter $command).Fill($data)
            return ,$data
        }
        [void]$command.ExecuteNonQuery()
    } finally { $connection.Dispose() }
}

function Execute-File([string]$database, [string]$path, [switch]$Baseline) {
    $sql = [IO.File]::ReadAllText($path)
    if ($Baseline) { $sql = [regex]::Replace($sql, '(?m)^USE \[[^\]]+\]\r?\n', '') }
    foreach ($batch in [regex]::Split($sql, '(?m)^GO[ \t]*\r?$\n?')) {
        if ($batch.Trim()) { Execute-Sql $database $batch }
    }
}

# Usa la lista real de Preparar-USB, no una segunda lista que pueda quedarse desactualizada.
$tokens = $null; $parseErrors = $null
$usbAst = [Management.Automation.Language.Parser]::ParseFile((Join-Path $PSScriptRoot 'Preparar-USB.ps1'), [ref]$tokens, [ref]$parseErrors)
if ($parseErrors.Count) { throw ($parseErrors | Out-String) }
$assignment = $usbAst.Find({ param($node) $node -is [Management.Automation.Language.AssignmentStatementAst] -and $node.Left.Extent.Text -eq '$soporteDb' }, $true)
$files = @($assignment.Right.FindAll({ param($node) $node -is [Management.Automation.Language.StringConstantExpressionAst] }, $true) | ForEach-Object { $_.Value })
$output = Join-Path $env:TEMP ('CasaGrande-audit-01-' + [guid]::NewGuid().ToString('N') + '.ps1')
New-DatabaseDeploymentScript (Join-Path $PSScriptRoot 'Desplegar-Base-de-Datos.ps1') $dbDir $files $output
$generatedAst = [Management.Automation.Language.Parser]::ParseFile($output, [ref]$tokens, [ref]$parseErrors)
if ($parseErrors.Count) { throw ($parseErrors | Out-String) }
$payloadAssignment = $generatedAst.Find({ param($node) $node -is [Management.Automation.Language.AssignmentStatementAst] -and $node.Left.Extent.Text -eq '$embeddedDatabaseFiles' }, $true)
$hashAst = $payloadAssignment.Right.Find({ param($node) $node -is [Management.Automation.Language.HashtableAst] }, $true)
$payload = @{}
foreach ($pair in $hashAst.KeyValuePairs) {
    $payload[$pair.Item1.Value] = $pair.Item2.Find({ param($node) $node -is [Management.Automation.Language.StringConstantExpressionAst] }, $true).Value
}
foreach ($file in $files) {
    $expected = [Convert]::ToBase64String([IO.File]::ReadAllBytes((Join-Path $dbDir $file)))
    if ($payload[$file] -cne $expected) { throw "Archivo embebido incorrecto: $file" }
}
Write-Host "OK: paso 01 autocontenido; $($files.Count) archivos identicos al origen; sintaxis valida."

$database = 'CasaGrandeAudit_' + [guid]::NewGuid().ToString('N')
$created = $false
try {
    Execute-Sql 'master' "CREATE DATABASE [$database]"
    $created = $true
    Execute-File $database (Join-Path $dbDir 'produccion-schema.sql') -Baseline
    Execute-Sql $database "INSERT dbo.Clientes (Nombre_cliente, Direccion_cliente, Referencia_ubicacion, Estado_cliente) VALUES ('AUDITORIA MIGRACION', 'CRA PRUEBA 123', 'REFERENCIA PRUEBA', 'ACTIVO');"
    $migrations = @('01_sp_Movil_Insertar_pedido.sql','02_unificar_empleados_duplicados.sql',
        '03_sp_Movil_Agregar_items.sql','04_sp_Movil_Quitar_item.sql','05_activar_empleados.sql','06_domicilios_direcciones.sql','07_alegra_integration.sql')
    $firstDefinitions = $null
    for ($pass = 1; $pass -le 2; $pass++) {
        foreach ($file in $migrations) {
            if ($files -notcontains $file) { throw "Migracion ausente de la USB: $file" }
            Execute-File $database (Join-Path $dbDir $file)
        }
        $checks = Execute-Sql $database "SELECT COUNT(*) AS Directions FROM dbo.Domicilios; SELECT COUNT(*) AS ReservedTable FROM dbo.Mesas WHERE Id_mesa=0 AND Num_mesa=0; SELECT name, definition FROM sys.sql_modules m INNER JOIN sys.procedures p ON p.object_id=m.object_id ORDER BY name;" -Results
        if ($checks.Tables[0].Rows[0].Directions -ne 1) { throw 'La migracion duplico o perdio la direccion del cliente.' }
        if ($checks.Tables[1].Rows[0].ReservedTable -ne 1) { throw 'Falta la mesa 0 de Domicilios.' }
        $definitions = ($checks.Tables[2].Rows | ForEach-Object { $_.name + $_.definition }) -join "`n"
        if ($pass -eq 1) { $firstDefinitions = $definitions }
        elseif ($definitions -cne $firstDefinitions) { throw 'La segunda ejecucion cambio las definiciones de los procedimientos.' }
        $verification = Execute-Sql $database ([IO.File]::ReadAllText((Join-Path $dbDir 'verificar-despliegue.sql')) -replace '(?m)^GO[ \t]*\r?$', '') -Results
        foreach ($table in $verification.Tables) {
            if ($table.Columns.Contains('estado')) {
                foreach ($row in $table.Rows) { if ($row.estado -ne 'OK') { throw ('Verificacion fallida: ' + ($row.ItemArray -join ' ')) } }
            }
        }
        Write-Host "OK: ejecucion $pass; esquema, procedimientos, direccion migrada y mesa 0 correctos."
    }
} finally {
    # Solo se permite borrar el nombre exacto generado por esta prueba, nunca una base del usuario.
    if ($created -and $database -match '^CasaGrandeAudit_[a-f0-9]{32}$') {
        Execute-Sql 'master' "ALTER DATABASE [$database] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$database];"
    }
}
Write-Host "OK: idempotencia verificada en base temporal; paso 01 de prueba: $output"
