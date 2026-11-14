<#
    Prueba manual: imprime un ticket real con datos de relleno en la impresora Bluetooth (PT-210).
    GASTA PAPEL. No consulta SQL ni usa la cola de impresion de Windows.

    Uso:
        .\Test-BluetoothReceipt.ps1                                   # precuenta con 3 productos
        .\Test-BluetoothReceipt.ps1 -Report Comandas.ComandasPedido -Products 5
        .\Test-BluetoothReceipt.ps1 -Report FacturaFinal.FacturaFinal -Products 50
#>
[CmdletBinding()]
param(
    [string]$Address='10:22:33:9D:1F:F2',
    [ValidateSet('Comandas.ComandasPedido','FacturaFinal.FacturaFinal','FacturaFinal.Precuenta')]
    [string]$Report='FacturaFinal.Precuenta',
    [int]$Products=3,
    # Carpeta con CapaPresentacion.dll ya compilada; por defecto, la de run-local.ps1.
    [string]$BuildPath
)
$ErrorActionPreference='Stop'
$repo=Split-Path $PSScriptRoot -Parent
if(-not $BuildPath){$BuildPath=Join-Path $repo 'SISMistico\SISMistico\bin\LocalBuild'}
$output=Join-Path $env:TEMP ('CasaGrande-bluetooth-receipt-'+[guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $output | Out-Null
Get-ChildItem -LiteralPath $BuildPath -Filter '*.dll' | Copy-Item -Destination $output
$exe=Join-Path $output 'BluetoothReceiptTests.exe'
& "$env:WINDIR\Microsoft.NET\Framework\v4.0.30319\csc.exe" /nologo /platform:x86 "/out:$exe" `
    "/r:$BuildPath\CapaPresentacion.dll" "/r:$BuildPath\Microsoft.ReportViewer.WinForms.dll" "/r:$BuildPath\Microsoft.ReportViewer.Common.dll" `
    /r:System.Windows.Forms.dll /r:System.Drawing.dll /r:System.Data.dll /r:System.Xml.dll `
    (Join-Path $PSScriptRoot 'tests\BluetoothReceiptTests.cs')
if($LASTEXITCODE -ne 0){throw 'No se compilo la prueba de impresion Bluetooth.'}
$configuration=New-Object Xml.XmlDocument
[void]$configuration.AppendChild($configuration.CreateElement('configuration'))
$settings=$configuration.CreateElement('appSettings')
$entry=$configuration.CreateElement('add')
$entry.SetAttribute('key','BluetoothPrinterAddress')
$entry.SetAttribute('value',$Address)
[void]$settings.AppendChild($entry)
[void]$configuration.DocumentElement.AppendChild($settings)
# Las redirecciones de ensamblados de la aplicacion, si la compilacion las trae.
$applicationConfig=Get-ChildItem -LiteralPath $BuildPath -Filter 'SISMistico.exe.config' -ErrorAction SilentlyContinue | Select-Object -First 1
if($applicationConfig){
    [xml]$application=Get-Content -LiteralPath $applicationConfig.FullName -Raw
    if($application.configuration.runtime){[void]$configuration.DocumentElement.AppendChild($configuration.ImportNode($application.configuration.runtime,$true))}
}
$configuration.Save($exe+'.config')
& $exe $output $Report $Products
if($LASTEXITCODE -ne 0){throw "Fallo la impresion Bluetooth. Artefactos: $output"}
