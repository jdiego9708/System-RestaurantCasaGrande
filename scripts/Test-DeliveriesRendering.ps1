[CmdletBinding()]
param(
    # Carpeta donde dejar las capturas PNG de las tarjetas y de la ventana; sin ella solo se comprueba.
    [string]$ScreenshotFolder
)
$ErrorActionPreference = 'Stop'
$repo = Split-Path $PSScriptRoot -Parent
$build = Join-Path $repo 'SISMistico\SISMistico\bin\LocalBuild'
if (-not (Test-Path (Join-Path $build 'CapaPresentacion.dll'))) { throw 'Ejecuta SISMistico\run-local.ps1 -NoRun antes de probar.' }
$testDir = Join-Path $env:TEMP ('CasaGrande-Deliveries-rendering-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $testDir | Out-Null
Get-ChildItem $build -Filter '*.dll' | Copy-Item -Destination $testDir
$exe = Join-Path $testDir 'DeliveriesRenderingTests.exe'
& "$env:WINDIR\Microsoft.NET\Framework\v4.0.30319\csc.exe" /nologo /target:exe /platform:x86 "/out:$exe" `
    "/r:$build\CapaPresentacion.dll" "/r:$build\CapaEntidades.dll" "/r:$build\FontAwesome.Sharp.dll" `
    /r:System.Windows.Forms.dll /r:System.Drawing.dll /r:System.Data.dll `
    (Join-Path $PSScriptRoot 'tests\DeliveriesRenderingTests.cs')
if ($LASTEXITCODE -ne 0) { throw 'Fallo al compilar las pruebas de Domicilios.' }
# El caso de la ventana consulta la base local: usa la misma cadena de conexion que la compilacion local.
Copy-Item (Join-Path $build 'SISMistico.exe.config') "$exe.config"
foreach ($case in @('timing', 'cards', 'window')) {
    if ($ScreenshotFolder) { & $exe $case $ScreenshotFolder } else { & $exe $case }
    if ($LASTEXITCODE -ne 0) { throw "Fallo la prueba de Domicilios: $case" }
}
