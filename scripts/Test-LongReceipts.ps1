[CmdletBinding()]
param()
$ErrorActionPreference='Stop'
$repo=Split-Path $PSScriptRoot -Parent
$build=Join-Path $repo 'SISMistico\SISMistico\bin\Debug'
$output=Join-Path $env:TEMP ('CasaGrande-long-receipts-'+[guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $output | Out-Null
Get-ChildItem -LiteralPath $build -Filter '*.dll' | Copy-Item -Destination $output
$exe=Join-Path $output 'LongReceiptTests.exe'
& "$env:WINDIR\Microsoft.NET\Framework\v4.0.30319\csc.exe" /nologo /platform:x86 "/out:$exe" `
    "/r:$build\CapaPresentacion.dll" "/r:$build\Microsoft.ReportViewer.WinForms.dll" "/r:$build\Microsoft.ReportViewer.Common.dll" `
    /r:System.Windows.Forms.dll /r:System.Drawing.dll /r:System.Data.dll /r:System.Xml.dll `
    (Join-Path $PSScriptRoot 'tests\LongReceiptTests.cs')
if($LASTEXITCODE -ne 0){throw 'No se compilo la prueba de tickets largos.'}
[xml]$application=Get-Content -LiteralPath (Join-Path $build 'SISMistico.exe.config') -Raw
$configuration=New-Object Xml.XmlDocument
[void]$configuration.AppendChild($configuration.CreateElement('configuration'))
[void]$configuration.DocumentElement.AppendChild($configuration.ImportNode($application.configuration.runtime,$true))
$configuration.Save($exe+'.config')
& $exe $output
if($LASTEXITCODE -ne 0){throw "Fallo prueba tickets largos. Artefactos: $output"}
Write-Host "Artefactos de prueba: $output"
