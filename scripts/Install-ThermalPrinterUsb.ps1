<#
Instala y configura una impresora termica USB (por ejemplo NP L22L, de 58 mm) en Windows 10/11.
Windows PowerShell 5.1. Instalar/configurar necesita elevacion; el diagnostico no.

Pasos: 1) detecta el dispositivo USB, 2) instala el driver (si se da -DriverFolder), 3) crea la cola de impresion
en el puerto USB que Windows asigna, 4) con papel de 80 mm deja el rollo largo con Configurar-Impresora-Termica.ps1,
5) opcionalmente la deja predeterminada e imprime una pagina de prueba.
Para repetirlo en otra maquina: -ExportDriver en la maquina que ya funciona y luego -DriverFolder en la nueva.

Ejemplos:
  .\Install-ThermalPrinterUsb.ps1 -DiagnoseOnly
  .\Install-ThermalPrinterUsb.ps1 -DriverFolder 'C:\Drivers\NP-L22L' -SetDefault -TestPage
  .\Install-ThermalPrinterUsb.ps1 -PrinterName 'NP L22L' -DriverName 'POS-80' -SetDefault
  .\Install-ThermalPrinterUsb.ps1 -PrinterName 'NP L22L' -ExportDriver 'C:\Kit\NP-L22L'

No cambia otras impresoras, no vacia colas y no reinicia servicios. Es seguro volver a ejecutarlo.
#>
[CmdletBinding()]
param(
    [string]$PrinterName = 'NP L22L',
    # Carpeta con el driver del fabricante (.inf) o con un kit creado por -ExportDriver.
    [string]$DriverFolder,
    # Nombre exacto del controlador. Si se omite: el del kit, el unico nuevo o el unico modelo del .inf.
    [string]$DriverName,
    # Puerto USB (USB001, ...). Si se omite se elige el puerto USB libre.
    [string]$PortName,
    [switch]$DiagnoseOnly,
    [switch]$SetDefault,
    [switch]$TestPage,
    # Ancho del papel (informativo): el NP L22L es de 58 mm. La aplicacion mide el papel de la impresora al imprimir;
    # Configurar-Impresora-Termica.ps1 elige solo el rollo largo que anuncia el controlador (58 u 80 mm).
    [ValidateSet(58, 80)][int]$PaperWidthMm = 58,
    [switch]$SkipRollFormat,
    # Exporta controlador y datos de la cola instalada a esta carpeta, para instalarla en otra maquina.
    [string]$ExportDriver
)
$ErrorActionPreference = 'Stop'

function Write-Step([string]$text) { Write-Host ''; Write-Host ('== ' + $text) -ForegroundColor Cyan }
function Test-Administrator {
    $principal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
    return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

# Dispositivos USB presentes que podrian ser la impresora: clase Printer/USBPRINT, con problema o con nombre de impresora.
function Get-PrinterCandidates {
    $pattern = '\bNP\b|L22|\bPOS\b|xprinter|thermal|termic|receipt|\bticket|printer|impresora|impresi'
    Get-PnpDevice -PresentOnly -ErrorAction SilentlyContinue | Where-Object {
        $_.Class -ne 'PrintQueue' -and (
            $_.Class -eq 'Printer' -or $_.InstanceId -like 'USBPRINT\*' -or
            ($_.InstanceId -like 'USB\VID_*' -and ($_.Class -eq 'Unknown' -or $_.Status -ne 'OK' -or $_.FriendlyName -match $pattern)))
    }
}

# Nombres de modelo que declara un .inf de impresora: lineas "Modelo" = Seccion,HardwareId.
function Get-InfModelNames([string]$path) {
    $text = Get-Content -LiteralPath $path -Raw
    $strings = @{}
    if ($text -match '(?ims)^\[Strings\]\s*(.*?)(?=^\[|\z)') {
        foreach ($line in ($Matches[1] -split "`r?`n")) {
            if ($line -match '^\s*(\w+)\s*=\s*"?(.*?)"?\s*(;.*)?$') { $strings[$Matches[1]] = $Matches[2] }
        }
    }
    $names = New-Object System.Collections.Generic.List[string]
    foreach ($match in [regex]::Matches($text, '(?m)^\s*(?:%(\w+)%|"([^"]+)")\s*=\s*[^,;\r\n]+,\s*\S+')) {
        $name = if ($match.Groups[1].Success) { $strings[$match.Groups[1].Value] } else { $match.Groups[2].Value }
        if ($name -and -not $names.Contains($name)) { $names.Add($name) }
    }
    return $names
}

Write-Host "Equipo: $env:COMPUTERNAME | $((Get-CimInstance Win32_OperatingSystem).Caption)"

# ---------------------------------------------------------------- Exportar
if ($ExportDriver) {
    if (-not (Test-Administrator)) { throw 'Abra Windows PowerShell como administrador para exportar el controlador.' }
    Write-Step "Exportando '$PrinterName' a $ExportDriver"
    $printer = Get-Printer -Name $PrinterName -ErrorAction Stop
    $driver = Get-PrinterDriver -Name $printer.DriverName -ErrorAction Stop
    New-Item -ItemType Directory -Path $ExportDriver -Force | Out-Null
    $infName = if ($driver.InfPath) { Split-Path $driver.InfPath -Leaf } else { $null }
    if ($infName -and $infName -match '^oem\d+\.inf$') {
        & pnputil.exe /export-driver $infName $ExportDriver | Out-Host
    } elseif ($driver.InfPath) {
        # Controlador de la bandeja de Windows: se copia la carpeta del .inf.
        Copy-Item -Path (Join-Path (Split-Path $driver.InfPath -Parent) '*') -Destination $ExportDriver -Recurse -Force
    } else {
        Write-Warning 'El controlador no declara .inf (controlador integrado de Windows): no hay paquete que exportar.'
    }
    $config = [ordered]@{ PrinterName = $printer.Name; DriverName = $printer.DriverName; Port = $printer.PortName; ExportedFrom = $env:COMPUTERNAME; Date = (Get-Date -Format o) }
    ($config | ConvertTo-Json) | Set-Content -LiteralPath (Join-Path $ExportDriver 'printer.json') -Encoding UTF8
    Write-Host "Listo. En la otra maquina: .\Install-ThermalPrinterUsb.ps1 -DriverFolder '$ExportDriver' -SetDefault" -ForegroundColor Green
    return
}

# ---------------------------------------------------------------- Diagnostico
Write-Step 'Estado actual'
Write-Host 'Servicio de impresion:' ((Get-Service Spooler).Status)
$candidates = @(Get-PrinterCandidates)
if ($candidates.Count -gt 0) {
    Write-Host 'Dispositivos USB candidatos:'
    $candidates | Select-Object Status, Class, FriendlyName, InstanceId | Format-Table -AutoSize | Out-Host
} else {
    Write-Warning 'Windows NO detecta ninguna impresora USB. Revise: cable USB de DATOS (no solo carga), impresora encendida, otro puerto USB directo del PC (sin hub), y pruebe otro cable.'
}
$usbPorts = @(Get-PrinterPort | Where-Object { $_.Name -match '^USB\d+$' })
Write-Host ('Puertos USB de impresora: ' + $(if ($usbPorts) { ($usbPorts.Name -join ', ') } else { '(ninguno)' }))
Get-Printer | Select-Object Name, DriverName, PortName, PrinterStatus | Format-Table -AutoSize | Out-Host

if ($DiagnoseOnly) { Write-Host 'Modo diagnostico: no se cambio nada.'; return }
if (-not (Test-Administrator)) { throw 'Abra Windows PowerShell como administrador para instalar o configurar.' }

# ---------------------------------------------------------------- Driver
$existing = Get-Printer -Name $PrinterName -ErrorAction SilentlyContinue
if ($DriverFolder) {
    Write-Step "Instalando controlador desde $DriverFolder"
    $DriverFolder = (Resolve-Path -LiteralPath $DriverFolder).Path
    $kit = Join-Path $DriverFolder 'printer.json'
    if (Test-Path $kit) {
        $saved = Get-Content -LiteralPath $kit -Raw | ConvertFrom-Json
        if (-not $DriverName) { $DriverName = $saved.DriverName }
        Write-Host "Kit exportado desde $($saved.ExportedFrom): controlador '$($saved.DriverName)'."
    }
    $before = @((Get-PrinterDriver).Name)
    $infs = @(Get-ChildItem -LiteralPath $DriverFolder -Filter *.inf -Recurse)
    if ($infs.Count -eq 0) { throw 'La carpeta no contiene archivos .inf. Si el fabricante entrega un instalador (.exe), ejecutelo primero y vuelva a correr este script sin -DriverFolder.' }
    foreach ($inf in $infs) { & pnputil.exe /add-driver $inf.FullName /install | Out-Host }
    if (-not $DriverName) {
        $newDrivers = @((Get-PrinterDriver).Name | Where-Object { $_ -notin $before })
        if ($newDrivers.Count -eq 1) { $DriverName = $newDrivers[0] }
        else {
            $models = @($infs | ForEach-Object { Get-InfModelNames $_.FullName } | Select-Object -Unique)
            if ($models.Count -eq 1) { $DriverName = $models[0] }
            elseif ($models.Count -gt 1) { throw ('Varios modelos en el .inf: ' + ($models -join ' | ') + '. Repita con -DriverName "<uno de ellos>".') }
        }
    }
}
if (-not $DriverName -and $existing) { $DriverName = $existing.DriverName }
if (-not $DriverName) {
    throw 'No se sabe que controlador usar. Indique -DriverFolder (carpeta del driver del fabricante) o -DriverName (por ejemplo el controlador ya instalado de otra termica de 80 mm).'
}
if (-not (Get-PrinterDriver -Name $DriverName -ErrorAction SilentlyContinue)) {
    Write-Step "Registrando controlador '$DriverName'"
    $infArgs = @{}
    if ($DriverFolder) {
        $inf = Get-ChildItem -LiteralPath $DriverFolder -Filter *.inf -Recurse | Where-Object { (Get-InfModelNames $_.FullName) -contains $DriverName } | Select-Object -First 1
        if ($inf) { $infArgs.InfPath = $inf.FullName }
    }
    Add-PrinterDriver -Name $DriverName @infArgs
}
Write-Host "Controlador: $DriverName" -ForegroundColor Green

# ---------------------------------------------------------------- Puerto y cola
Write-Step 'Cola de impresion'
if (-not $PortName) {
    $usedPorts = @(Get-Printer | ForEach-Object { $_.PortName })
    $free = @(Get-PrinterPort | Where-Object { $_.Name -match '^USB\d+$' -and $_.Name -notin $usedPorts })
    if ($free.Count -eq 1) { $PortName = $free[0].Name }
    elseif ($free.Count -gt 1) { throw ('Hay varios puertos USB libres (' + ($free.Name -join ', ') + '). Desconecte las demas USB o indique -PortName.') }
    elseif ($existing) { $PortName = $existing.PortName }
    else { throw 'No hay un puerto USB de impresora libre: Windows aun no detecto la impresora. Revise cable/encendido y ejecute con -DiagnoseOnly.' }
}
if ($existing) {
    if ($existing.DriverName -ne $DriverName -or $existing.PortName -ne $PortName) {
        Set-Printer -Name $PrinterName -DriverName $DriverName -PortName $PortName
        Write-Host "Cola '$PrinterName' actualizada: $DriverName en $PortName."
    } else { Write-Host "La cola '$PrinterName' ya existe con ese controlador y puerto." }
} else {
    Add-Printer -Name $PrinterName -DriverName $DriverName -PortName $PortName
    Write-Host "Cola '$PrinterName' creada: $DriverName en $PortName." -ForegroundColor Green
}

# ---------------------------------------------------------------- Rollo largo de 80 mm
if (-not $SkipRollFormat) {
    $rollScript = Join-Path $PSScriptRoot 'Configurar-Impresora-Termica.ps1'
    if (-not (Test-Path $rollScript)) { $rollScript = Join-Path $PSScriptRoot '07-Configurar-Impresora-Termica.ps1' }
    if (Test-Path $rollScript) {
        Write-Step "Formato de rollo largo de $PaperWidthMm mm"
        if (Get-Process SISMistico -ErrorAction SilentlyContinue) { Write-Warning 'SISMistico esta abierto: cierrelo y ejecute despues Configurar-Impresora-Termica.ps1 -Modo Configurar.' }
        else { try { & $rollScript -Modo Configurar -NombreImpresora $PrinterName -NoPause } catch { Write-Warning ('No se pudo fijar el rollo: ' + $_.Exception.Message) } }
    } else { Write-Warning 'No se encontro Configurar-Impresora-Termica.ps1 junto a este script; el formato de rollo queda sin tocar.' }
}

# ---------------------------------------------------------------- Predeterminada y prueba
if ($SetDefault) {
    Write-Step 'Impresora predeterminada'
    # En Windows 11 «Permitir que Windows administre mi impresora predeterminada» la cambia sola; se desactiva para este usuario.
    $key = 'HKCU:\Software\Microsoft\Windows NT\CurrentVersion\Windows'
    Set-ItemProperty -Path $key -Name LegacyDefaultPrinterMode -Value 1 -Type DWord
    $queue = Get-CimInstance Win32_Printer -Filter ("Name='" + $PrinterName.Replace("'", "''") + "'")
    [void](Invoke-CimMethod -InputObject $queue -MethodName SetDefaultPrinter)
    Write-Host "'$PrinterName' es ahora la predeterminada de este usuario."
}
if ($TestPage) {
    Write-Step 'Pagina de prueba'
    & rundll32.exe printui.dll,PrintUIEntry /k /n"$PrinterName"
    Write-Host 'Pagina de prueba enviada: compruebe que sale completa, sin cortes y con acentos.'
}

Write-Step 'Resumen'
Get-Printer -Name $PrinterName | Select-Object Name, DriverName, PortName, PrinterStatus | Format-List | Out-Host
Write-Host 'En Configurar Alegra, deje el campo impresora vacio (usa la predeterminada de Windows) o escriba exactamente:' $PrinterName
