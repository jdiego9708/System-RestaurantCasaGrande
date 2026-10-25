<#
    Convierte el respaldo de ESQUEMA de produccion (script T-SQL de SSMS, UTF-16) a UTF-8 con LF,
    para versionarlo en git y poder leerlo con cualquier herramienta.

    Uso:
        .\Export-ProdSchema.ps1
        .\Export-ProdSchema.ps1 -Source "X.sql" -Target .\produccion-schema.sql

    El script de esquema NO contiene datos (solo CREATE TABLE / TYPE / PROC): es seguro versionarlo.
    NO uses este script con el respaldo completo: ese contiene datos reales.
#>
param(
    [string]$Source = "D:\Usuario_JUAN_DIEGO\Escritorio\Casa Grande\backupCasaGrande20261024Esquema.sql",
    [string]$Target = (Join-Path $PSScriptRoot "produccion-schema.sql")
)

$ErrorActionPreference = "Stop"

if ((Split-Path $Source -Leaf) -match 'Completa') { throw "Ese es el respaldo con datos; no se exporta." }

$text = [System.IO.File]::ReadAllText($Source, [System.Text.Encoding]::Unicode)
if ($text -match 'INSERT \[dbo\]') { throw "El script contiene INSERT (datos); abortado." }

$text = $text -replace "`r`n", "`n"
$utf8NoBom = New-Object System.Text.UTF8Encoding($false)
[System.IO.File]::WriteAllText($Target, $text, $utf8NoBom)

"{0}: {1:n0} lineas, {2:n0} bytes" -f $Target, ($text -split "`n").Count, (Get-Item $Target).Length
