<#
    Restaura en LocalDB una copia de la BD de produccion a partir del script completo
    (esquema + datos) generado con SSMS "Generate Scripts".

    - Crea una BD NUEVA (por defecto CasaGrandeBD_prod). No toca la CasaGrandeBD de desarrollo.
    - Ignora el CREATE DATABASE / ALTER DATABASE del script (rutas C:\Users\Casa Grande\...):
      la BD se crea en la ubicacion por defecto de LocalDB.
    - Ejecuta el resto lote a lote (separador GO), en streaming: el script pesa ~280 MB en UTF-16.

    Uso:
        .\Restore-ProdCopy.ps1                  # falla si la BD ya existe
        .\Restore-ProdCopy.ps1 -Force           # la elimina y la recrea
        .\Restore-ProdCopy.ps1 -ScriptPath X.sql -Database CasaGrandeBD_prod

    Los datos son reales (empleados, claves, ventas): la copia es solo para desarrollo local.
#>
param(
    [string]$ScriptPath = "D:\Usuario_JUAN_DIEGO\Escritorio\Casa Grande\backupCasaGrande20261024Completa.sql",
    [string]$Database = "CasaGrandeBD_prod",
    [string]$Instance = "(localdb)\MSSQLLocalDB",
    [switch]$Force
)

$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.Data

function Write-Step($msg) { Write-Host "==> $msg" -ForegroundColor Cyan }

if ($Database -notmatch '^[A-Za-z0-9_]+$') { throw "Nombre de BD no valido: $Database" }
if (-not (Test-Path -LiteralPath $ScriptPath)) { throw "No existe el script: $ScriptPath" }

Write-Step "Iniciando LocalDB"
sqllocaldb start MSSQLLocalDB | Out-Null

function New-Connection([string]$db) {
    $cs = "Server=$Instance;Database=$db;Integrated Security=true;Connect Timeout=30"
    $cn = New-Object System.Data.SqlClient.SqlConnection $cs
    $cn.Open()
    return $cn
}

function Invoke-NonQuery($cn, [string]$sql) {
    $cmd = $cn.CreateCommand()
    $cmd.CommandText = $sql
    $cmd.CommandTimeout = 0
    [void]$cmd.ExecuteNonQuery()
}

# ---------------------------------------------------------------------------
# 1. Crear la BD vacia

$master = New-Connection "master"
$exists = $master.CreateCommand()
$exists.CommandText = "SELECT COUNT(*) FROM sys.databases WHERE name = N'$Database'"
if ([int]$exists.ExecuteScalar() -gt 0) {
    if (-not $Force) { throw "La BD $Database ya existe. Usa -Force para recrearla." }
    Write-Step "Eliminando $Database existente"
    Invoke-NonQuery $master "ALTER DATABASE [$Database] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$Database];"
}
Write-Step "Creando $Database"
Invoke-NonQuery $master "CREATE DATABASE [$Database]"
$master.Close()

# ---------------------------------------------------------------------------
# 2. Ejecutar el script lote a lote

$cn = New-Connection $Database
$reader = New-Object System.IO.StreamReader($ScriptPath, [System.Text.Encoding]::Unicode, $true)
$started = $false
$batches = 0
$sw = [System.Diagnostics.Stopwatch]::StartNew()

function Invoke-Batch([string]$text) {
    $t = $text.Trim()
    if ($t.Length -eq 0) { return }
    # Todo lo anterior a este USE es el CREATE/ALTER DATABASE del servidor original.
    if (-not $script:started) {
        if ($t -match '^(?s)(/\*.*?\*/\s*)?USE \[CasaGrandeBD\]') { $script:started = $true }
        return
    }
    # Al final solo quedan USE [master] / ALTER DATABASE de cierre.
    if ($t -match '^(?s)(/\*.*?\*/\s*)?(USE \[master\]|ALTER DATABASE|CREATE DATABASE)') { return }
    Invoke-NonQuery $cn $t
    $script:batches++
    if ($script:batches % 500 -eq 0) {
        Write-Host ("    {0} lotes, {1:n0} s" -f $script:batches, $script:sw.Elapsed.TotalSeconds)
    }
}

# Los saltos de linea dentro de los textos de los datos (observaciones, descripciones) deben
# llegar a SQL Server tal cual estan en el script; por eso se lee por bloques y no por lineas
# (ReadLine + AppendLine convierte cada \n en \r\n y las cadenas dejan de caber en la columna).
$separator = [regex]'(?m)^GO[ \t]*\r?$\n?'
$buffer = New-Object char[] 8388608
$pending = ""

Write-Step "Cargando $ScriptPath"
try {
    while (($n = $reader.Read($buffer, 0, $buffer.Length)) -gt 0) {
        $pending += [string]::new($buffer, 0, $n)
        # Se procesa solo hasta el ultimo GO completo; el resto espera al siguiente bloque.
        $cut = $pending.LastIndexOf("`nGO", [StringComparison]::Ordinal)
        if ($cut -lt 0) { continue }
        $eol = $pending.IndexOf("`n", $cut + 1)
        if ($eol -lt 0) { continue }
        $chunk = $pending.Substring(0, $eol + 1)
        $pending = $pending.Substring($eol + 1)
        foreach ($b in $separator.Split($chunk)) { Invoke-Batch $b }
    }
    foreach ($b in $separator.Split($pending)) { Invoke-Batch $b }
}
finally {
    $reader.Close()
}

if (-not $started) { throw "No se encontro 'USE [CasaGrandeBD]' en el script: formato inesperado." }

# ---------------------------------------------------------------------------
# 3. Verificacion

Write-Step "Verificando ($batches lotes en $([int]$sw.Elapsed.TotalSeconds) s)"
$cmd = $cn.CreateCommand()
$cmd.CommandText = @"
SELECT t.name, SUM(p.rows) AS filas
FROM sys.tables t
JOIN sys.partitions p ON p.object_id = t.object_id AND p.index_id IN (0,1)
GROUP BY t.name ORDER BY SUM(p.rows) DESC
"@
$r = $cmd.ExecuteReader()
while ($r.Read()) { "{0,-32} {1,10:n0}" -f $r[0], $r[1] }
$r.Close()
$cn.Close()
Write-Host "Listo: $Instance -> $Database" -ForegroundColor Green
