[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$repo = Split-Path $PSScriptRoot -Parent
$build = Join-Path $repo 'SISMistico\SISMistico\bin\LocalBuild'
if (-not (Test-Path (Join-Path $build 'CapaPresentacion.dll'))) { throw 'Ejecuta SISMistico\run-local.ps1 -NoRun antes de probar.' }
$testDir = Join-Path $env:TEMP ('CasaGrande-Sales-rendering-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $testDir | Out-Null
Get-ChildItem $build -Filter '*.dll' | Copy-Item -Destination $testDir
$exe = Join-Path $testDir 'SalesWindowRenderingTests.exe'
& "$env:WINDIR\Microsoft.NET\Framework\v4.0.30319\csc.exe" /nologo /target:exe /platform:x86 "/out:$exe" `
    "/r:$build\CapaPresentacion.dll" "/r:$build\FontAwesome.Sharp.dll" /r:System.Windows.Forms.dll /r:System.Drawing.dll `
    (Join-Path $PSScriptRoot 'tests\SalesWindowRenderingTests.cs')
if ($LASTEXITCODE -ne 0) { throw 'Fallo al compilar las pruebas de renderizado.' }
# Cada caso arranca en un proceso nuevo para mantener fria la carga de fuentes.
foreach ($case in @('guard', 'window')) {
    & $exe $case
    if ($LASTEXITCODE -ne 0) { throw "Fallo la prueba de renderizado: $case" }
}
