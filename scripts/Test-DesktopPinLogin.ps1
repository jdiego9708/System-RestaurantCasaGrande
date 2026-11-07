[CmdletBinding()]
param([switch]$CheckLocalDb)
$ErrorActionPreference = 'Stop'
$repo = Split-Path $PSScriptRoot -Parent
$build = Join-Path $repo 'SISMistico\SISMistico\bin\LocalBuild'
if (-not (Test-Path (Join-Path $build 'CapaPresentacion.dll'))) { throw 'Ejecuta SISMistico\run-local.ps1 -NoRun antes de probar.' }
$testDir = Join-Path $env:TEMP ('CasaGrande-Pin-login-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $testDir | Out-Null
Get-ChildItem $build -Filter '*.dll' | Copy-Item -Destination $testDir
$exe = Join-Path $testDir 'DesktopPinLoginTests.exe'
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$csc = & $vswhere -latest -products * -find 'MSBuild\**\Bin\Roslyn\csc.exe' | Select-Object -First 1
if (-not $csc) { throw 'Se necesita el compilador C# de Visual Studio Build Tools.' }
& $csc /nologo /target:exe /platform:x86 "/out:$exe" /r:System.Windows.Forms.dll /r:System.Drawing.dll /r:System.Data.dll `
    "/r:$build\CapaPresentacion.dll" "/r:$build\CapaNegocio.dll" "/r:$build\CapaDatos.dll" "/r:$build\CapaEntidades.dll" `
    (Join-Path $PSScriptRoot 'tests\DesktopPinLoginTests.cs')
if ($LASTEXITCODE -ne 0) { throw 'Fallo al compilar las pruebas de inicio de sesion.' }
if ($CheckLocalDb) { Copy-Item (Join-Path $build 'SISMistico.exe.config') "$exe.config" }
Push-Location $testDir
try {
    if ($CheckLocalDb) { & $exe database } else { & $exe }
    if ($LASTEXITCODE -ne 0) { throw 'Fallaron las pruebas de inicio de sesion.' }
    Write-Host "Previsualizaciones: $testDir"
} finally { Pop-Location }
