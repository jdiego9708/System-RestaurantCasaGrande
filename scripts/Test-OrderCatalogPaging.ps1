[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$repo = Split-Path $PSScriptRoot -Parent
$build = Join-Path $repo 'SISMistico\SISMistico\bin\LocalBuild'
if (-not (Test-Path (Join-Path $build 'CapaPresentacion.dll'))) { throw 'Ejecuta SISMistico\run-local.ps1 -NoRun antes de probar.' }
$testDir = Join-Path $env:TEMP ('CasaGrande-Order-catalog-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $testDir | Out-Null
Get-ChildItem $build -Filter '*.dll' | Copy-Item -Destination $testDir
$exe = Join-Path $testDir 'OrderCatalogPagingTests.exe'
& "$env:WINDIR\Microsoft.NET\Framework\v4.0.30319\csc.exe" /nologo /target:exe /platform:x86 "/out:$exe" `
    "/r:$build\CapaPresentacion.dll" /r:System.Windows.Forms.dll /r:System.Drawing.dll /r:System.Core.dll `
    (Join-Path $PSScriptRoot 'tests\OrderCatalogPagingTests.cs')
if ($LASTEXITCODE -ne 0) { throw 'Fallo al compilar las pruebas de paginacion.' }
Copy-Item (Join-Path $build 'SISMistico.exe.config') "$exe.config"
& $exe
if ($LASTEXITCODE -ne 0) { throw 'Fallo la prueba de paginacion del pedido.' }
