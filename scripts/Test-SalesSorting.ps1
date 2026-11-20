[CmdletBinding()]
param([string]$BuildDirectory)
$ErrorActionPreference = 'Stop'
$repo = Split-Path $PSScriptRoot -Parent
if (-not $BuildDirectory) { $BuildDirectory = Join-Path $repo 'SISMistico\SISMistico\bin\LocalBuild' }
if (-not (Test-Path (Join-Path $BuildDirectory 'CapaPresentacion.dll'))) { throw 'Compila la aplicacion antes de probar.' }
$testDir = Join-Path $env:TEMP ('CasaGrande-SalesSorting-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $testDir | Out-Null
Get-ChildItem $BuildDirectory -Filter '*.dll' | Copy-Item -Destination $testDir
$exe = Join-Path $testDir 'SalesSortingTests.exe'
& "$env:WINDIR\Microsoft.NET\Framework\v4.0.30319\csc.exe" /nologo /target:exe /platform:x86 "/out:$exe" `
    "/r:$testDir\CapaPresentacion.dll" /r:System.Windows.Forms.dll /r:System.Drawing.dll /r:System.Data.dll `
    (Join-Path $PSScriptRoot 'tests\SalesSortingTests.cs')
if ($LASTEXITCODE -ne 0) { throw 'Fallo al compilar las pruebas de ordenacion.' }
# La prueba usa datos sinteticos y no carga la ventana ni consulta la base de datos.
& $exe
if ($LASTEXITCODE -ne 0) { throw 'Fallo la prueba de ordenacion del historial.' }
