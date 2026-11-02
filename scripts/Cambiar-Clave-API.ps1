<#
    Cambia la clave de la API movil (ApiKey) del sistema YA instalado, sin reinstalar.

    La clave es la que cada celular escribe una vez en "Configurar servidor". Puede ser:
      * una clave PERSONALIZADA, corta y facil de recordar (minimo 8 caracteres, sin espacios), o
      * una ALEATORIA facil de teclear (12 caracteres en minuscula, sin letras ni numeros ambiguos: 0/o, 1/l/i).

    Uso (en el PC del restaurante; en el menu Inicio esta como "Cambiar clave de la API"):
        .\Cambiar-Clave-API.ps1                       # pregunta la clave nueva (Enter = aleatoria)
        .\Cambiar-Clave-API.ps1 -Clave MiClave2026    # personalizada, sin preguntar
        .\Cambiar-Clave-API.ps1 -Aleatoria            # aleatoria, sin preguntar
        .\Cambiar-Clave-API.ps1 -RutaConfig 'D:\otra\SISMistico.exe.config'

    Que hace: guarda una copia (.bak) del SISMistico.exe.config, cambia SOLO la clave ApiKey y muestra la nueva.
    Pide permiso de administrador (Windows lo confirma) unicamente si el archivo esta en una carpeta protegida.

    DESPUES: cierre y vuelva a abrir SISRestaurante (lee la clave al arrancar; las sesiones de los celulares se
    cierran) y escriba la clave nueva en cada celular (Configurar servidor > Clave de la API).
#>
[CmdletBinding()]
param(
    [string]$Clave,
    [switch]$Aleatoria,
    [string]$RutaConfig = (Join-Path $env:ProgramFiles 'SISRestaurante\SISMistico.exe.config')
)

$ErrorActionPreference = 'Stop'
$ClavePlaceholder = 'cambiar-esta-clave'
# Sin 0/o, 1/l/i: se confunden al leerlas y al teclearlas en el celular.
$Alfabeto = 'abcdefghjkmnpqrstuvwxyz23456789'

function Ok($t)    { Write-Host "    OK  $t" -ForegroundColor Green }
function Aviso($t) { Write-Host "    !!  $t" -ForegroundColor Yellow }

function Nueva-Clave([int]$largo = 12) {
    $bytes = New-Object byte[] $largo
    $rng = [Security.Cryptography.RNGCryptoServiceProvider]::new()
    try { $rng.GetBytes($bytes) } finally { $rng.Dispose() }
    -join ($bytes | ForEach-Object { $Alfabeto[$_ % $Alfabeto.Length] })
}

function Clave-Valida([string]$c) {
    return ($c -and $c.Length -ge 8 -and $c -notmatch '\s' -and $c -ne $ClavePlaceholder)
}

function Pausa([int]$codigo) {
    Write-Host ''
    Read-Host 'Pulse Enter para cerrar' | Out-Null
    exit $codigo
}

if (-not (Test-Path -LiteralPath $RutaConfig)) {
    Write-Host "No encuentro $RutaConfig. Use -RutaConfig con la ruta de SISMistico.exe.config." -ForegroundColor Red
    Pausa 1
}

# ---- ¿Se puede escribir sin permisos especiales? Si no, se relanza con administrador ----
$sePuedeEscribir = $true
try {
    $f = [IO.File]::Open($RutaConfig, 'Open', 'ReadWrite', 'ReadWrite'); $f.Dispose()
    # El .bak se crea junto al archivo: la carpeta tambien debe permitirlo
    $probe = "$RutaConfig.prueba"
    [IO.File]::WriteAllText($probe, 'x'); Remove-Item $probe -Force
} catch [UnauthorizedAccessException] { $sePuedeEscribir = $false }
catch [System.Security.SecurityException] { $sePuedeEscribir = $false }

if (-not $sePuedeEscribir) {
    Write-Host 'Se necesitan permisos de administrador: acepte el aviso de Windows (UAC)...' -ForegroundColor Yellow
    $relanzar = @('-NoProfile', '-ExecutionPolicy', 'Bypass', '-NoExit', '-File', "`"$PSCommandPath`"")
    foreach ($p in $PSBoundParameters.GetEnumerator()) {
        if ($p.Value -is [System.Management.Automation.SwitchParameter]) { if ($p.Value) { $relanzar += "-$($p.Key)" } }
        else { $relanzar += "-$($p.Key)"; $relanzar += "`"$($p.Value)`"" }
    }
    # Si se pregunta la clave, se pregunta ya en la ventana elevada (aqui no se puede pasar de forma segura).
    try { Start-Process -FilePath 'powershell.exe' -Verb RunAs -ArgumentList $relanzar }
    catch { Write-Host 'No se obtuvo permiso de administrador.' -ForegroundColor Red; Pausa 1 }
    exit 0
}

# ---- Clave nueva ----
Write-Host ''
Write-Host ' Cambiar la clave de la API movil' -ForegroundColor Cyan
Write-Host " Archivo: $RutaConfig"
[xml]$xml = Get-Content -LiteralPath $RutaConfig -Raw
$app = $xml.configuration.appSettings
if (-not $app) { Write-Host 'El archivo no tiene la seccion appSettings.' -ForegroundColor Red; Pausa 1 }
$nodo = $app.add | Where-Object { $_.key -eq 'ApiKey' }
$habilitada = ($app.add | Where-Object { $_.key -eq 'ApiHabilitada' }).value
if ($habilitada -ne 'true') {
    Aviso 'ApiHabilitada no es "true": la API movil esta apagada. Este cambio no la enciende (use el instalador o Configurar-Red.ps1).'
}

if ($Aleatoria) { $Clave = Nueva-Clave }
elseif (-not $Clave) {
    Write-Host ''
    Write-Host ' Escriba la clave nueva (minimo 8 caracteres, sin espacios) o pulse Enter para una aleatoria.'
    Write-Host ' Una clave corta y facil de recordar ahorra trabajo si hay varios celulares.'
    $Clave = (Read-Host ' Clave nueva').Trim()
    if (-not $Clave) { $Clave = Nueva-Clave; Write-Host " Aleatoria: $Clave" }
}
if (-not (Clave-Valida $Clave)) {
    Write-Host ''
    Write-Host ' La clave debe tener al menos 8 caracteres, sin espacios, y no puede ser la de ejemplo. No se cambio nada.' -ForegroundColor Red
    Pausa 1
}

# ---- Aplicar ----
Copy-Item -LiteralPath $RutaConfig -Destination "$RutaConfig.bak" -Force
if (-not $nodo) {
    $nodo = $xml.CreateElement('add'); $nodo.SetAttribute('key', 'ApiKey'); $nodo.SetAttribute('value', $Clave)
    [void]$app.AppendChild($nodo)
} else { $nodo.value = $Clave }
$xml.Save($RutaConfig)

Write-Host ''
Ok "Clave cambiada. Copia del archivo anterior: $RutaConfig.bak"
Write-Host ''
Write-Host ('=' * 60) -ForegroundColor Green
Write-Host "  CLAVE DE LA API (la que se escribe en cada celular): $Clave" -ForegroundColor Green
Write-Host ('=' * 60) -ForegroundColor Green
Write-Host ''
Write-Host ' Falta:' -ForegroundColor Cyan
if (Get-Process SISMistico -ErrorAction SilentlyContinue) {
    Write-Host '   1. SISRestaurante esta abierto: cierrelo y vuelvalo a abrir para que use la clave nueva.' -ForegroundColor Yellow
} else {
    Write-Host '   1. Abra SISRestaurante (leera la clave nueva al arrancar).'
}
Write-Host '   2. En cada celular: Configurar servidor > Clave de la API > escribir la clave nueva.'
Write-Host '      (Hasta entonces los celulares veran "La clave de la API no es correcta".)'
Pausa 0
