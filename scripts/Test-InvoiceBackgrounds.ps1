[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$repo = Split-Path $PSScriptRoot -Parent
$build = Join-Path $repo 'SISMistico\CapaPresentacion\bin\Debug'
if (-not (Test-Path (Join-Path $build 'CapaPresentacion.dll'))) { throw 'Compile la solucion en Debug antes de probar.' }
$testDirectory = Join-Path $env:TEMP ('CasaGrande-Invoice-backgrounds-' + [guid]::NewGuid().ToString('N'))
[void](New-Item -ItemType Directory -Path $testDirectory)
Get-ChildItem $build -Filter '*.dll' | Copy-Item -Destination $testDirectory
$exe = Join-Path $testDirectory 'InvoiceBackgroundTests.exe'
& "$env:WINDIR\Microsoft.NET\Framework\v4.0.30319\csc.exe" /nologo /target:exe /platform:x86 "/out:$exe" `
    "/r:$build\CapaPresentacion.dll" /r:System.Windows.Forms.dll /r:System.Drawing.dll `
    (Join-Path $PSScriptRoot 'tests\InvoiceBackgroundTests.cs')
if ($LASTEXITCODE -ne 0) { throw 'Fallo al compilar la prueba de fondos.' }
& $exe $testDirectory
if ($LASTEXITCODE -ne 0) { throw 'Fallo la prueba de fondos.' }
Write-Host "Vista previa: $testDirectory\facturar-fondos.png"
