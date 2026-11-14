[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$repo = Split-Path $PSScriptRoot -Parent
$build = Join-Path $repo 'SISMistico\SISMistico\bin\LocalBuild'
$testDir = Join-Path $env:TEMP ('CasaGrande-Products-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $testDir | Out-Null
Get-ChildItem $build -Filter '*.dll' | Copy-Item -Destination $testDir
$exe = Join-Path $testDir 'ProductsTests.exe'
Copy-Item (Join-Path $build 'SISMistico.exe.config') "$exe.config"
& "$env:WINDIR\Microsoft.NET\Framework\v4.0.30319\csc.exe" /nologo /target:exe /platform:x86 "/out:$exe" `
    "/r:$build\CapaPresentacion.dll" "/r:$build\CapaNegocio.dll" "/r:$build\CapaDatos.dll" `
    /r:System.Windows.Forms.dll /r:System.Drawing.dll /r:System.Data.dll `
    (Join-Path $PSScriptRoot 'tests\ProductsTests.cs')
if ($LASTEXITCODE -ne 0) { throw 'No se pudieron compilar las pruebas de productos.' }
& $exe $testDir
if ($LASTEXITCODE -ne 0) { throw 'Fallaron las pruebas de productos.' }
Write-Host "Capturas de las ventanas: $testDir"
