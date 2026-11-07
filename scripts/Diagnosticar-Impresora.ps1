<#
Diagnostico de impresora predeterminada. Solo lectura: no imprime ni cambia preferencias.
Ejecutar en el PC del restaurante con el mismo usuario de Windows que usa el sistema.
Salida: TXT en el Escritorio. Las secciones no disponibles se registran sin detener el resto.
Uso: .\Diagnosticar-Impresora.ps1
     .\Diagnosticar-Impresora.ps1 -PrinterName 'Nombre exacto' -NoPause
#>
[CmdletBinding()]
param([string]$PrinterName, [string]$OutputPath, [switch]$NoPause)

$ErrorActionPreference='Stop'
$report=New-Object System.Text.StringBuilder
function Section([string]$title,[scriptblock]$read) {
    [void]$report.AppendLine('')
    [void]$report.AppendLine(('=' * 72))
    [void]$report.AppendLine($title)
    [void]$report.AppendLine(('=' * 72))
    Write-Host "Consultando: $title"
    try {
        $result=& $read
        if($null -eq $result) { [void]$report.AppendLine('(Sin datos disponibles)') }
        else { [void]$report.AppendLine(($result | Out-String -Width 260).TrimEnd()) }
    } catch {
        [void]$report.AppendLine('No disponible: '+$_.Exception.Message)
    }
}
function Millimeters($value) { [math]::Round([double]$value * 0.254,2) }

$now=Get-Date
if(!$OutputPath) {
    $folder=[Environment]::GetFolderPath('DesktopDirectory')
    if(!$folder -or !(Test-Path -LiteralPath $folder)) { $folder=$PSScriptRoot }
    $OutputPath=Join-Path $folder ('Diagnostico-Impresora-'+$env:COMPUTERNAME+'-'+$now.ToString('yyyyMMdd-HHmmss')+'.txt')
}
$OutputPath=[IO.Path]::GetFullPath($OutputPath)
[void]$report.AppendLine('DIAGNOSTICO DE IMPRESORA - CASA GRANDE')
[void]$report.AppendLine('Fecha local: '+$now.ToString('yyyy-MM-dd HH:mm:ss zzz'))
[void]$report.AppendLine('Equipo: '+$env:COMPUTERNAME+' | Usuario: '+[Environment]::UserName)
[void]$report.AppendLine('PowerShell: '+$PSVersionTable.PSVersion+' | Proceso 64 bits: '+[Environment]::Is64BitProcess)
[void]$report.AppendLine('Solo lectura. No se enviaron pruebas de impresion ni se cambiaron preferencias.')
[void]$report.AppendLine('Los tamanos se convierten de centesimas de pulgada a milimetros (factor 0.254).')
[void]$report.AppendLine('Estos son ajustes de Windows/controlador; cada aplicacion puede sobrescribir papel y margenes.')

$printers=@()
Section 'WINDOWS Y SERVICIO DE IMPRESION' {
    Get-CimInstance Win32_OperatingSystem -OperationTimeoutSec 15 |
        Select-Object Caption,Version,BuildNumber,OSArchitecture | Format-List
    Get-Service Spooler | Select-Object Name,Status,StartType | Format-List
}
try { $printers=@(Get-CimInstance Win32_Printer -OperationTimeoutSec 15) }
catch { [void]$report.AppendLine('No se pudo consultar Win32_Printer: '+$_.Exception.Message) }
Section 'IMPRESORAS INSTALADAS' {
    $printers | Select-Object Name,Default,DriverName,PortName,Network,Shared,WorkOffline,PrinterStatus |
        Format-Table -AutoSize -Wrap
}
if(!$PrinterName) {
    $default=$printers | Where-Object Default | Select-Object -First 1
    if($default) { $PrinterName=$default.Name }
}

if($PrinterName) {
    [void]$report.AppendLine('Impresora seleccionada para el diagnostico: '+$PrinterName)
    $selected=$printers | Where-Object Name -eq $PrinterName | Select-Object -First 1
    Section 'DATOS COMPLETOS WIN32_PRINTER' { $selected | Format-List * }
    Section 'COLA Y CONFIGURACION GENERAL' {
        Get-Printer -ErrorAction Stop | Where-Object Name -eq $PrinterName | Format-List *
    }
    Section 'PREFERENCIAS DE IMPRESION - PRINTCONFIGURATION' {
        Get-PrintConfiguration -PrinterName $PrinterName -ErrorAction Stop | Format-List *
    }
    Section 'CONFIGURACION WIN32_PRINTERCONFIGURATION' {
        Get-CimInstance Win32_PrinterConfiguration -OperationTimeoutSec 15 |
            Where-Object Name -eq $PrinterName | Format-List *
    }
    Section 'PUERTO Y CONEXION (USB / TCP-IP / OTRO)' {
        if(!$selected) { throw 'No se encontro la cola en Win32_Printer.' }
        Get-PrinterPort -ErrorAction Stop | Where-Object Name -eq $selected.PortName | Format-List *
    }
    Section 'CONTROLADOR INSTALADO' {
        if(!$selected) { throw 'No se pudo determinar el controlador.' }
        Get-PrinterDriver -ErrorAction Stop | Where-Object Name -eq $selected.DriverName | Format-List *
    }
    Section 'VERSIONES DE ARCHIVOS DEL CONTROLADOR' {
        if(!$selected) { throw 'No se pudo determinar el controlador.' }
        $driver=Get-PrinterDriver -ErrorAction Stop | Where-Object Name -eq $selected.DriverName | Select-Object -First 1
        foreach($path in @($driver.DriverPath,$driver.ConfigFile,$driver.DataFile) | Select-Object -Unique) {
            if($path -and (Test-Path -LiteralPath $path -PathType Leaf)) {
                (Get-Item -LiteralPath $path).VersionInfo |
                    Select-Object FileName,FileVersion,ProductVersion,CompanyName,ProductName | Format-List
            }
        }
    }
    Section 'PAPEL PREDETERMINADO, AREA IMPRIMIBLE, MARGENES Y RESOLUCION (.NET)' {
        Add-Type -AssemblyName System.Drawing
        $settings=New-Object System.Drawing.Printing.PrinterSettings
        $settings.PrinterName=$PrinterName
        if(!$settings.IsValid) { throw 'El controlador no devuelve una PrinterSettings valida.' }
        $page=$settings.DefaultPageSettings
        [pscustomobject]@{
            Impresora=$settings.PrinterName; Predeterminada=$settings.IsDefaultPrinter
            Papel=$page.PaperSize.PaperName; CodigoPapel=$page.PaperSize.RawKind
            Ancho_mm=(Millimeters $page.PaperSize.Width); Alto_mm=(Millimeters $page.PaperSize.Height)
            Horizontal=$page.Landscape; Color=$page.Color
            ResolucionTipo=$page.PrinterResolution.Kind
            ResolucionX=$page.PrinterResolution.X; ResolucionY=$page.PrinterResolution.Y
            MargenIzquierdo_mm=(Millimeters $page.Margins.Left)
            MargenDerecho_mm=(Millimeters $page.Margins.Right)
            MargenSuperior_mm=(Millimeters $page.Margins.Top)
            MargenInferior_mm=(Millimeters $page.Margins.Bottom)
            MargenFisicoX_mm=(Millimeters $page.HardMarginX)
            MargenFisicoY_mm=(Millimeters $page.HardMarginY)
            AreaImprimibleX_mm=(Millimeters $page.PrintableArea.X)
            AreaImprimibleY_mm=(Millimeters $page.PrintableArea.Y)
            AreaImprimibleAncho_mm=(Millimeters $page.PrintableArea.Width)
            AreaImprimibleAlto_mm=(Millimeters $page.PrintableArea.Height)
            SoportaColor=$settings.SupportsColor; SoportaDuplex=$settings.CanDuplex
            Duplex=$settings.Duplex; Copias=$settings.Copies; CopiasMaximas=$settings.MaximumCopies
        } | Format-List
    }
    Section 'TAMANOS DE PAPEL DISPONIBLES (.NET)' {
        Add-Type -AssemblyName System.Drawing
        $settings=New-Object System.Drawing.Printing.PrinterSettings
        $settings.PrinterName=$PrinterName
        $settings.PaperSizes | Select-Object PaperName,RawKind,
            @{N='Ancho_mm';E={Millimeters $_.Width}},@{N='Alto_mm';E={Millimeters $_.Height}} |
            Format-Table -AutoSize -Wrap
    }
    Section 'RESOLUCIONES DISPONIBLES (.NET)' {
        Add-Type -AssemblyName System.Drawing
        $settings=New-Object System.Drawing.Printing.PrinterSettings
        $settings.PrinterName=$PrinterName
        $settings.PrinterResolutions | Select-Object Kind,X,Y | Format-Table -AutoSize
    }
    Section 'PROPIEDADES ADICIONALES DEL CONTROLADOR' {
        Get-PrinterProperty -PrinterName $PrinterName -ErrorAction Stop | Format-List *
    }
} else {
    [void]$report.AppendLine('No se encontro impresora predeterminada para este usuario. Puede repetir con -PrinterName.')
}
Section 'DISPOSITIVOS DE IMPRESION RECONOCIDOS POR WINDOWS' {
    Get-CimInstance Win32_PnPEntity -OperationTimeoutSec 15 |
        Where-Object { $_.PNPClass -in @('Printer','PrintQueue') } |
        Select-Object Name,Manufacturer,PNPClass,PNPDeviceID,Status,ConfigManagerErrorCode | Format-List
}
[void]$report.AppendLine('')
[void]$report.AppendLine('NOTAS: Un controlador generico puede no identificar el modelo fisico exacto.')
[void]$report.AppendLine('Corte automatico, avance y densidad pueden estar solo en opciones privadas del fabricante.')
[void]$report.AppendLine('Las propiedades que requieran elevacion quedan registradas como no disponibles; no se elevaron permisos automaticamente.')

$parent=Split-Path $OutputPath -Parent
if(!(Test-Path -LiteralPath $parent)) { New-Item -ItemType Directory -Path $parent -Force | Out-Null }
[IO.File]::WriteAllText($OutputPath,$report.ToString(),(New-Object Text.UTF8Encoding $true))
Write-Host ''
Write-Host 'DIAGNOSTICO TERMINADO' -ForegroundColor Green
Write-Host "Archivo para compartir: $OutputPath" -ForegroundColor Cyan
if(!$NoPause) { Read-Host 'Pulse Enter para cerrar' | Out-Null }
