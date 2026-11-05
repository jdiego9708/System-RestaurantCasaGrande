[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$repo = Split-Path $PSScriptRoot -Parent
$build = Join-Path $repo 'SISMistico\CapaNegocio\bin\Debug'
if (-not (Test-Path (Join-Path $build 'CapaNegocio.dll'))) { throw 'Compile CapaNegocio en Debug antes de probar.' }
$testDir = Join-Path $env:TEMP ('CasaGrande-Alegra-tests-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $testDir | Out-Null
Get-ChildItem $build -Filter '*.dll' | Copy-Item -Destination $testDir
$exe = Join-Path $testDir 'AlegraContractTests.exe'
& "$env:WINDIR\Microsoft.NET\Framework\v4.0.30319\csc.exe" /nologo /target:exe "/out:$exe" `
    "/r:$build\CapaNegocio.dll" /r:System.Net.Http.dll /r:System.Web.Extensions.dll `
    (Join-Path $PSScriptRoot 'tests\AlegraContractTests.cs')
if ($LASTEXITCODE -ne 0) { throw 'Fallo al compilar las pruebas Alegra.' }
& $exe
if ($LASTEXITCODE -ne 0) { throw 'Fallaron las pruebas de contratos Alegra.' }
