<#
Diagnostico USB/EPSON TM-T20II. Windows PowerShell 5.1.
Solo consulta el sistema y escribe informes. No imprime, instala, elimina,
reinicia servicios ni modifica registro, energia o canales de eventos.
#>
[CmdletBinding()]
param(
    [ValidateRange(1,720)][int]$HoursBack = 72,
    [string]$OutputDirectory,
    [switch]$NoPause
)
$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'
if (-not $OutputDirectory) { $OutputDirectory = [Environment]::GetFolderPath('DesktopDirectory') }
if (-not $OutputDirectory) { $OutputDirectory = $env:TEMP }
$folder = Join-Path ([IO.Path]::GetFullPath($OutputDirectory)) ('Epson-USB-' + $env:COMPUTERNAME + '-' + (Get-Date -Format 'yyyyMMdd-HHmmss') + '-' + [guid]::NewGuid().ToString('N').Substring(0,6))
[void](New-Item -ItemType Directory -Path $folder -Force)
$report = New-Object System.Text.StringBuilder
$errors = New-Object 'System.Collections.Generic.List[string]'
$since = (Get-Date).AddHours(-$HoursBack)
function Json([string]$Name, $Value) {
    ConvertTo-Json -InputObject $Value -Depth 8 | Set-Content -LiteralPath (Join-Path $folder $Name) -Encoding UTF8
}
function Section([string]$Title,[scriptblock]$Read) {
    Write-Host ('Consultando: ' + $Title) -ForegroundColor Cyan
    [void]$report.AppendLine("`r`n" + ('=' * 76) + "`r`n" + $Title)
    try {
        $data = & $Read
        if ($null -eq $data) { [void]$report.AppendLine('(Sin datos)') }
        else { [void]$report.AppendLine(($data | Out-String -Width 240).TrimEnd()) }
    } catch {
        $errors.Add($Title + ': ' + $_.Exception.Message)
        [void]$report.AppendLine('No disponible: ' + $_.Exception.Message)
    }
}
function Property([string]$Id,[string]$Key) {
    if (-not (Get-Command Get-PnpDeviceProperty -ErrorAction SilentlyContinue)) { return $null }
    try { (Get-PnpDeviceProperty -InstanceId $Id -KeyName $Key -ErrorAction Stop).Data }
    catch { return $null }
}
$principal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
$admin = $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
[void]$report.AppendLine('DIAGNOSTICO USB / EPSON TM-T20II - CASA GRANDE')
[void]$report.AppendLine('Fecha: ' + (Get-Date).ToString('yyyy-MM-dd HH:mm:ss zzz'))
[void]$report.AppendLine('Equipo: ' + $env:COMPUTERNAME + ' | Administrador: ' + $admin)
[void]$report.AppendLine('PowerShell: ' + $PSVersionTable.PSVersion + ' | Proceso 64 bits: ' + [Environment]::Is64BitProcess)
[void]$report.AppendLine('Solo lectura. Contiene identificadores de hardware y posibles numeros de serie.')
if (-not $admin) { Write-Host 'Se recomienda ejecutar como administrador para acceder a todos los registros.' -ForegroundColor Yellow }
Section 'WINDOWS, BIOS Y SERVICIOS' {
    Get-CimInstance Win32_OperatingSystem -OperationTimeoutSec 20 | Select-Object Caption,Version,BuildNumber,OSArchitecture,LastBootUpTime | Format-List
    Get-CimInstance Win32_ComputerSystem -OperationTimeoutSec 20 | Select-Object Manufacturer,Model,SystemType | Format-List
    Get-CimInstance Win32_BIOS -OperationTimeoutSec 20 | Select-Object Manufacturer,SMBIOSBIOSVersion,ReleaseDate | Format-List
    Get-Service Spooler,PlugPlay,DeviceInstall,DsmSvc -ErrorAction SilentlyContinue | Select-Object Name,Status,StartType | Format-Table
}
$devices = @()
try { $devices = @(Get-CimInstance Win32_PnPEntity -OperationTimeoutSec 30) }
catch { $errors.Add('Inventario PnP: ' + $_.Exception.Message) }
$suspects = @($devices | Where-Object {
    $_.Name -match 'EPSON|TM[- _]?T20|descriptor|desconocido|unknown USB' -or
    $_.PNPDeviceID -match 'VID_04B8|DEVICE_DESCRIPTOR_FAILURE|VID_0000&PID_0002' -or
    ($_.PNPDeviceID -match '^USB\\' -and $_.ConfigManagerErrorCode -ne 0)
})
$fields = @('Name','PNPDeviceID','PNPClass','Manufacturer','Service','Status','Present','ConfigManagerErrorCode','HardwareID','CompatibleID')
Section 'INVENTARIO USB Y DISPOSITIVOS SOSPECHOSOS' {
    $usb = @($devices | Where-Object { $_.PNPDeviceID -match '^USB\\|^USBPRINT\\' -or $_.PNPClass -eq 'USB' })
    Json 'usb-inventory.json' @($usb | Select-Object $fields)
    Json 'suspect-devices.json' @($suspects | Select-Object $fields)
    $suspects | Select-Object $fields | Format-List
    'Otros dispositivos con problemas:'
    $devices | Where-Object { $_.ConfigManagerErrorCode -ne 0 } | Select-Object Name,PNPDeviceID,ConfigManagerErrorCode | Format-Table -Wrap
}
Section 'DISPOSITIVOS ACTUALMENTE PRESENTES' {
    if (Get-Command Get-PnpDevice -ErrorAction SilentlyContinue) {
        $present = @(Get-PnpDevice -PresentOnly | Where-Object { $_.InstanceId -match '^USB\\|^USBPRINT\\|VID_04B8' -or $_.FriendlyName -match 'EPSON|TM[- _]?T20' })
        Json 'present-devices.json' @($present | Select-Object Status,Class,FriendlyName,InstanceId,Problem)
        $present | Select-Object Status,Class,FriendlyName,InstanceId,Problem | Format-Table -Wrap
    } else { 'Get-PnpDevice no disponible. Consultar el inventario CIM.' }
}
Section 'PROPIEDADES Y CADENA DE PUERTOS / CONCENTRADORES USB' {
    $keys = @('DEVPKEY_Device_ProblemCode','DEVPKEY_Device_ProblemStatus','DEVPKEY_Device_HardwareIds','DEVPKEY_Device_CompatibleIds','DEVPKEY_Device_BusReportedDeviceDesc','DEVPKEY_Device_LocationInfo','DEVPKEY_Device_LocationPaths','DEVPKEY_Device_Parent','DEVPKEY_Device_Service','DEVPKEY_Device_DriverInfPath','DEVPKEY_Device_DriverProvider','DEVPKEY_Device_DriverVersion','DEVPKEY_Device_DriverDate','DEVPKEY_Device_ContainerId','DEVPKEY_Device_IsPresent')
    $details = @(); $parents = @()
    foreach ($device in $suspects) {
        $item = [ordered]@{Name=$device.Name;InstanceId=$device.PNPDeviceID}
        foreach ($key in $keys) { $item[$key] = Property $device.PNPDeviceID $key }
        $details += [pscustomobject]$item
        $id = $device.PNPDeviceID; $visited = @{}
        for ($depth=0; $depth -lt 8 -and $id -and -not $visited.ContainsKey($id); $depth++) {
            $visited[$id]=$true
            $node = $devices | Where-Object PNPDeviceID -eq $id | Select-Object -First 1
            $parent = Property $id 'DEVPKEY_Device_Parent'
            $parents += [pscustomobject]@{Target=$device.PNPDeviceID;Depth=$depth;InstanceId=$id;Name=$node.Name;ProblemCode=$node.ConfigManagerErrorCode;Parent=$parent;Location=(Property $id 'DEVPKEY_Device_LocationInfo')}
            $id = [string]$parent
        }
    }
    Json 'device-properties.json' @($details)
    Json 'usb-parent-chain.json' @($parents)
    $details | Format-List
    $parents | Format-Table -Wrap
}
Section 'CONTROLADORES FIRMADOS USB / EPSON' {
    $drivers = @(Get-CimInstance Win32_PnPSignedDriver -OperationTimeoutSec 30 | Where-Object {
        $_.DeviceID -match '^USB\\|^USBPRINT\\|VID_04B8' -or $_.DeviceClass -eq 'USB' -or $_.Manufacturer -match 'EPSON' -or $_.DriverProviderName -match 'EPSON'
    } | Select-Object DeviceName,DeviceID,DeviceClass,Manufacturer,DriverProviderName,DriverVersion,DriverDate,InfName,IsSigned,Signer)
    Json 'signed-drivers.json' $drivers
    $drivers | Format-List
}
Section 'PAQUETES DE TERCEROS (PNPUTIL)' {
    & "$env:WINDIR\System32\pnputil.exe" /enum-drivers 2>&1 | Set-Content -LiteralPath (Join-Path $folder 'pnputil-drivers.txt') -Encoding UTF8
    'Inventario guardado en pnputil-drivers.txt. Codigo de salida: ' + $LASTEXITCODE
}
Section 'SOFTWARE EPSON INSTALADO (NO USA WIN32_PRODUCT)' {
    foreach ($path in @('HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\*','HKLM:\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\*','HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\*')) {
        Get-ItemProperty -Path $path -ErrorAction SilentlyContinue | Where-Object { $_.DisplayName -match 'EPSON|Advanced Printer Driver|TM-T20' } | Select-Object DisplayName,DisplayVersion,Publisher,InstallDate,InstallLocation | Format-List
    }
}
Section 'IMPRESORAS Y PUERTOS' {
    $printers = @(Get-CimInstance Win32_Printer -OperationTimeoutSec 20 | Select-Object Name,DriverName,PortName,PNPDeviceID,Default,WorkOffline,PrinterStatus,DetectedErrorState,ExtendedDetectedErrorState)
    Json 'printers.json' $printers
    $printers | Format-List
    if (Get-Command Get-PrinterDriver -ErrorAction SilentlyContinue) { Get-PrinterDriver | Select-Object Name,Manufacturer,PrinterEnvironment,MajorVersion,InfPath | Format-List }
    if (Get-Command Get-PrinterPort -ErrorAction SilentlyContinue) { Get-PrinterPort | Select-Object Name,Description,PortMonitor,PrinterHostAddress,PortNumber | Format-List }
}
Section 'CONTROLADORES USB, HUBS Y ENERGIA' {
    Get-CimInstance Win32_USBController -OperationTimeoutSec 20 | Select-Object Name,Manufacturer,PNPDeviceID,Status | Format-List
    Get-CimInstance Win32_USBHub -OperationTimeoutSec 20 | Select-Object Name,PNPDeviceID,Status,ConfigManagerErrorCode,NumberOfPorts | Format-List
    & "$env:WINDIR\System32\powercfg.exe" /getactivescheme
    & "$env:WINDIR\System32\powercfg.exe" /query SCHEME_CURRENT 2a737441-1930-4402-8d77-b2bebba308a3
}
Section 'EVENTOS RECIENTES USB / PNP / IMPRESION' {
    $events = New-Object 'System.Collections.Generic.List[object]'
    $availability = @()
    $logs = @('System','Microsoft-Windows-Kernel-PnP/Configuration','Microsoft-Windows-DeviceSetupManager/Admin','Microsoft-Windows-UserPnp/DeviceInstall','Microsoft-Windows-PrintService/Admin','Microsoft-Windows-PrintService/Operational','Microsoft-Windows-USB-USBHUB3/Operational','Microsoft-Windows-USB-USBXHCI/Operational')
    foreach ($log in $logs) {
        try {
            $info = Get-WinEvent -ListLog $log -ErrorAction Stop
            $availability += [pscustomobject]@{Log=$log;Enabled=$info.IsEnabled;Records=$info.RecordCount;Status='Consultado'}
            if (-not $info.IsEnabled) { continue }
            $recent = @(Get-WinEvent -FilterHashtable @{LogName=$log;StartTime=$since} -MaxEvents 1500 -ErrorAction SilentlyContinue)
            foreach ($event in $recent) {
                if ($log -eq 'System' -and $event.ProviderName -notmatch 'USB|PnP|Print|DriverFrameworks|DeviceSetup') { continue }
                $events.Add([pscustomobject]@{TimeCreated=$event.TimeCreated;Log=$log;Id=$event.Id;Level=$event.LevelDisplayName;Provider=$event.ProviderName;RecordId=$event.RecordId;Message=$event.Message;Xml=$event.ToXml()})
            }
        } catch { $availability += [pscustomobject]@{Log=$log;Enabled=$null;Records=$null;Status=$_.Exception.Message} }
    }
    Json 'event-log-availability.json' @($availability)
    Json 'events.json' @($events.ToArray() | Sort-Object TimeCreated)
    $availability | Format-Table -Wrap
    $events | Sort-Object TimeCreated -Descending | Select-Object -First 100 TimeCreated,Log,Id,Level,Provider,Message | Format-List
    'events.json conserva el XML. No se habilitaron canales desactivados.'
    'Limite: 1500 eventos recientes por canal; puede no cubrir todas las horas solicitadas si hay mucha actividad.'
}
Section 'FRAGMENTOS DE SETUPAPI.DEV.LOG' {
    $tail = @(Get-Content -LiteralPath (Join-Path $env:WINDIR 'INF\setupapi.dev.log') -Tail 30000)
    $patterns = @('EPSON','TM-T20','VID_04B8','DEVICE_DESCRIPTOR_FAILURE','VID_0000&PID_0002')
    foreach ($device in $suspects) { if ($device.PNPDeviceID) { $patterns += [string]$device.PNPDeviceID } }
    $matches = @($tail | Select-String -SimpleMatch -Pattern $patterns -Context 10,22)
    $text = New-Object System.Text.StringBuilder
    foreach ($match in $matches | Select-Object -Last 120) {
        [void]$text.AppendLine(($match.Context.PreContext -join "`r`n"))
        [void]$text.AppendLine('>>> ' + $match.Line)
        [void]$text.AppendLine(($match.Context.PostContext -join "`r`n"))
        [void]$text.AppendLine('---')
    }
    $text.ToString() | Set-Content -LiteralPath (Join-Path $folder 'setupapi-fragments.txt') -Encoding UTF8
    'Fragmentos de las ultimas 30000 lineas guardados. Coincidencias: ' + $matches.Count
}
Section 'INTERPRETACION PRELIMINAR (NO ES UN DIAGNOSTICO DEFINITIVO)' {
    $descriptor = @($suspects | Where-Object { $_.PNPDeviceID -match 'DEVICE_DESCRIPTOR_FAILURE|VID_0000&PID_0002' -or ($_.Name -match 'descriptor' -and $_.ConfigManagerErrorCode -eq 43) })
    if ($descriptor.Count -gt 0) {
        'Indicios de fallo de enumeracion USB: puede producirse antes de que Windows utilice el controlador Epson.'
        'Sin VID/PID valido no puede atribuirse automaticamente el dispositivo desconocido a esta impresora.'
        'Comparar dos informes: con la impresora conectada y despues con su cable USB desconectado, sin variar los demas dispositivos.'
        'El informe no permite distinguir por si solo cable, puerto, alimentacion, interfaz de impresora o controlador USB del equipo.'
    } else { 'No se identifico automaticamente un fallo de descriptor en esta instantanea. Revisar eventos e inventario.' }
    if (@($suspects | Where-Object ConfigManagerErrorCode -eq 28).Count -gt 0) { 'Se observo codigo 28: falta de controlador en al menos un dispositivo sospechoso.' }
    'Una cola de impresion instalada no demuestra que la impresora USB este identificada correctamente.'
    'No se aplicaron reparaciones ni pruebas de impresion.'
}
[void]$report.AppendLine("`r`nERRORES / SECCIONES NO DISPONIBLES")
foreach ($errorText in $errors) { [void]$report.AppendLine($errorText) }
$report.ToString() | Set-Content -LiteralPath (Join-Path $folder 'LEER-PRIMERO.txt') -Encoding UTF8
try {
    Compress-Archive -Path (Join-Path $folder '*') -DestinationPath ($folder + '.zip') -ErrorAction Stop
    Write-Host ("`r`nInforme listo: " + $folder + '.zip') -ForegroundColor Green
} catch { Write-Host ('No se pudo comprimir: ' + $_.Exception.Message) -ForegroundColor Yellow }
Write-Host ('Carpeta: ' + $folder)
Write-Host 'Envia el ZIP para analizarlo. No se cambio la configuracion del equipo.'
if (-not $NoPause) { [void](Read-Host 'Presiona Enter para cerrar') }
