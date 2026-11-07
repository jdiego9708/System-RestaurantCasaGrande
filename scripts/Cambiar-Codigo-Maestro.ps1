<#
    Cambia el CODIGO MAESTRO de 8 digitos del sistema ya instalado, sin reinstalar.

    El codigo maestro se pide en el celular, en un unico dialogo (junto con una observacion), para quitar un
    producto de un pedido. Es distinto del PIN de 4 digitos de cada empleado, que no cambia.

    Se guarda en SISMistico.exe.config como sal + hash SHA-256 (MasterCodeSalt y MasterCodeHash): el codigo
    en claro no queda escrito en ningun archivo.

    Uso (en el PC del restaurante; en el menu Inicio esta como "Cambiar codigo maestro"):
        .\Cambiar-Codigo-Maestro.ps1                        # localiza la instalacion y pide el codigo (oculto, dos veces)
        .\Cambiar-Codigo-Maestro.ps1 -Codigo 12345678       # sin preguntar
        .\Cambiar-Codigo-Maestro.ps1 -RutaConfig 'C:\Program Files\SISRestaurante1\SISMistico.exe.config'

    Que hace: guarda una copia (.bak) del SISMistico.exe.config y cambia SOLO MasterCodeSalt y MasterCodeHash.
    Pide permiso de administrador (Windows lo confirma) unicamente si el archivo esta en una carpeta protegida.

    DESPUES: cierre y vuelva a abrir SISRestaurante (lee el codigo al arrancar).
#>
[CmdletBinding()]
param(
    [string]$Codigo,
    [string]$RutaConfig
)

$ErrorActionPreference = 'Stop'

function Ok($t)    { Write-Host "    OK  $t" -ForegroundColor Green }
function Pausa([int]$codigo) {
    Write-Host ''
    Read-Host 'Pulse Enter para cerrar' | Out-Null
    exit $codigo
}

function Test-MasterCode([string]$c) { return [bool]($c -match '^\d{8}$') }

function ConvertTo-PlainText([Security.SecureString]$secure) {
    $ptr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($secure)
    try { return [Runtime.InteropServices.Marshal]::PtrToStringBSTR($ptr) }
    finally { [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($ptr) }
}

# Misma formula que la app: SHA-256 en hexadecimal minuscula de (sal + codigo) en UTF-8.
function Get-MasterCodeHash([string]$salt, [string]$code) {
    $sha = [Security.Cryptography.SHA256]::Create()
    try {
        $bytes = $sha.ComputeHash([Text.Encoding]::UTF8.GetBytes($salt + $code))
        return (($bytes | ForEach-Object { $_.ToString('x2') }) -join '')
    } finally { $sha.Dispose() }
}

function New-Salt {
    $bytes = New-Object byte[] 16
    $rng = [Security.Cryptography.RNGCryptoServiceProvider]::new()
    try { $rng.GetBytes($bytes) } finally { $rng.Dispose() }
    return (($bytes | ForEach-Object { $_.ToString('x2') }) -join '')
}

# Localiza SISMistico.exe.config: registro de desinstalacion y, si no, carpetas SISRestaurante* de Program Files.
function Find-ConfigPath {
    $found = @()
    foreach ($root in @('HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall',
                        'HKLM:\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall',
                        'HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall')) {
        if (-not (Test-Path $root)) { continue }
        foreach ($key in @(Get-ChildItem $root -ErrorAction SilentlyContinue)) {
            $p = Get-ItemProperty $key.PSPath -ErrorAction SilentlyContinue
            if ($null -eq $p) { continue }
            if (-not (([string]$p.DisplayName) -like 'SISRestaurante*' -or $key.PSChildName -like '{B7C0E4F2-6A1D-4C53-9E55-0C4A5D7A2F10}*')) { continue }
            $dir = ([string]$p.InstallLocation).Trim().Trim('"')
            if ($dir) { $found += (Join-Path $dir 'SISMistico.exe.config') }
        }
    }
    foreach ($pf in @($env:ProgramFiles, ${env:ProgramFiles(x86)})) {
        if (-not $pf) { continue }
        foreach ($d in @(Get-ChildItem -LiteralPath $pf -Directory -Filter 'SISRestaurante*' -ErrorAction SilentlyContinue)) {
            $found += (Join-Path $d.FullName 'SISMistico.exe.config')
        }
    }
    $existing = @($found | Where-Object { Test-Path -LiteralPath $_ } | Select-Object -Unique |
        Sort-Object { (Get-Item -LiteralPath $_).LastWriteTime } -Descending)
    if ($existing.Count -gt 1) { Write-Host "    Hay varias instalaciones; se usa la mas reciente: $($existing[0])" -ForegroundColor Yellow }
    if ($existing.Count -gt 0) { return $existing[0] }
    return $null
}

if (-not $RutaConfig) { $RutaConfig = Find-ConfigPath }
if (-not $RutaConfig -or -not (Test-Path -LiteralPath $RutaConfig)) {
    Write-Host 'No encuentro SISMistico.exe.config. Use -RutaConfig con la ruta de ese archivo.' -ForegroundColor Red
    Pausa 1
}

# ---- ¿Se puede escribir sin permisos especiales? Si no, se relanza con administrador ----
$canWrite = $true
try {
    $f = [IO.File]::Open($RutaConfig, 'Open', 'ReadWrite', 'ReadWrite'); $f.Dispose()
    $probe = "$RutaConfig.prueba"
    [IO.File]::WriteAllText($probe, 'x'); Remove-Item $probe -Force
} catch [UnauthorizedAccessException] { $canWrite = $false }
catch [System.Security.SecurityException] { $canWrite = $false }

if (-not $canWrite) {
    Write-Host 'Se necesitan permisos de administrador: acepte el aviso de Windows (UAC)...' -ForegroundColor Yellow
    # El codigo no viaja por la linea de comandos: se vuelve a preguntar en la ventana elevada.
    $relaunch = @('-NoProfile', '-ExecutionPolicy', 'Bypass', '-NoExit', '-File', "`"$PSCommandPath`"", '-RutaConfig', "`"$RutaConfig`"")
    try { Start-Process -FilePath 'powershell.exe' -Verb RunAs -ArgumentList $relaunch }
    catch { Write-Host 'No se obtuvo permiso de administrador.' -ForegroundColor Red; Pausa 1 }
    exit 0
}

Write-Host ''
Write-Host ' Cambiar el codigo maestro (8 digitos)' -ForegroundColor Cyan
Write-Host " Archivo: $RutaConfig"

if (-not $Codigo) {
    Write-Host ''
    Write-Host ' Es el codigo que se pide en el celular para quitar un producto de un pedido. No se muestra al escribirlo.'
    $first = ConvertTo-PlainText (Read-Host ' Codigo maestro nuevo (8 digitos)' -AsSecureString)
    $second = ConvertTo-PlainText (Read-Host ' Repita el codigo' -AsSecureString)
    if ($first -ne $second) {
        Write-Host ''
        Write-Host ' Los dos codigos no coinciden. No se cambio nada.' -ForegroundColor Red
        Pausa 1
    }
    $Codigo = $first
}
if (-not (Test-MasterCode $Codigo)) {
    Write-Host ''
    Write-Host ' El codigo maestro debe tener exactamente 8 digitos (solo numeros). No se cambio nada.' -ForegroundColor Red
    Pausa 1
}

[xml]$xml = Get-Content -LiteralPath $RutaConfig -Raw
$app = $xml.configuration.appSettings
if (-not $app) { Write-Host 'El archivo no tiene la seccion appSettings.' -ForegroundColor Red; Pausa 1 }

$salt = New-Salt
$values = [ordered]@{ MasterCodeSalt = $salt; MasterCodeHash = (Get-MasterCodeHash $salt $Codigo) }

Copy-Item -LiteralPath $RutaConfig -Destination "$RutaConfig.bak" -Force
foreach ($name in $values.Keys) {
    $node = $app.add | Where-Object { $_.key -eq $name }
    if (-not $node) {
        $node = $xml.CreateElement('add'); $node.SetAttribute('key', $name); $node.SetAttribute('value', $values[$name])
        [void]$app.AppendChild($node)
    } else { $node.value = $values[$name] }
}
$xml.Save($RutaConfig)

Write-Host ''
Ok "Codigo maestro cambiado. Copia del archivo anterior: $RutaConfig.bak"
Write-Host ''
Write-Host ' Falta:' -ForegroundColor Cyan
if (Get-Process SISMistico -ErrorAction SilentlyContinue) {
    Write-Host '   SISRestaurante esta abierto: cierrelo y vuelvalo a abrir para que use el codigo nuevo.' -ForegroundColor Yellow
} else {
    Write-Host '   Abra SISRestaurante (leera el codigo nuevo al arrancar).'
}
Pausa 0
