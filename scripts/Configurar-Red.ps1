<#
    Prepara el PC del restaurante para que los celulares se conecten a la API por la red local (D7).

    EJECUTAR EN EL PC DEL RESTAURANTE, en PowerShell COMO ADMINISTRADOR:
        .\Configurar-Red.ps1                      # reserva de URL + regla de firewall + resumen para los celulares
        .\Configurar-Red.ps1 -SoloInformar        # no cambia nada (no requiere administrador): solo muestra el resumen
        .\Configurar-Red.ps1 -RutaConfig 'C:\...\SISMistico.exe.config'   # ademas activa la API y fija la ApiKey ahi
        .\Configurar-Red.ps1 -PerfilPrivado       # si la WiFi esta como "Publica", la pasa a "Privada"
        .\Configurar-Red.ps1 -FijarIp             # convierte la IP actual (DHCP) en IP fija en este PC
        .\Configurar-Red.ps1 -NoSuspender         # el PC no se duerme mientras esta enchufado

    Que hace (todo es idempotente: se puede ejecutar varias veces):
      1. Reserva la URL del HttpListener para cualquier usuario (por SID, vale en Windows en espanol o ingles),
         de modo que SISMistico.exe NO necesita ejecutarse como administrador.
      2. Crea la regla de firewall de entrada para el puerto de la API, solo perfiles Privado/Dominio y solo
         desde la subred local (LocalSubnet). No expone nada a internet.
      3. Revisa el perfil de red, si la IP es dinamica y si el PC se suspende, y avisa.
      4. Al final imprime los datos que hay que escribir en cada celular.

    No arranca ni toca WSL, ni cambia la base de datos.
#>
[CmdletBinding()]
param(
    # Debe coincidir con la clave ApiPrefijo de App.config.
    [string]$Prefijo = 'http://+:8099/api/v1/',
    # Clave de la API (X-Api-Key). Si no se indica, se lee de -RutaConfig; si tampoco, se debe definir a mano.
    [string]$ApiKey,
    # SISMistico.exe.config (o App.config) donde activar la API y escribir la clave. Se hace copia .bak.
    [string]$RutaConfig,
    [switch]$SoloInformar,
    [switch]$PerfilPrivado,
    [switch]$FijarIp,
    [switch]$NoSuspender
)

$ErrorActionPreference = 'Stop'
$ClavePlaceholder = 'cambiar-esta-clave'
$ReglaFirewall = 'SISRestaurante API'

function Paso($t)  { Write-Host "==> $t" -ForegroundColor Cyan }
function Ok($t)    { Write-Host "    OK  $t" -ForegroundColor Green }
function Aviso($t) { Write-Host "    !!  $t" -ForegroundColor Yellow }

# ---------- 0. Datos del prefijo ----------
# Si no se indico -Prefijo pero hay config, se respeta el ApiPrefijo que ya tiene (puerto elegido en el instalador).
if ($RutaConfig -and -not $PSBoundParameters.ContainsKey('Prefijo') -and (Test-Path $RutaConfig)) {
    [xml]$cfgPrevio = Get-Content -LiteralPath $RutaConfig -Raw
    $prefijoConfig = ($cfgPrevio.configuration.appSettings.add | Where-Object { $_.key -eq 'ApiPrefijo' }).value
    if ($prefijoConfig) { $Prefijo = $prefijoConfig }
}
if (-not $Prefijo.EndsWith('/')) { $Prefijo += '/' }
if ($Prefijo -notmatch '^http://[^:/]+:(?<puerto>\d+)(?<ruta>/.*)$') {
    throw "ApiPrefijo no tiene la forma http://+:PUERTO/ruta/ : $Prefijo"
}
$Puerto = [int]$Matches['puerto']
$RutaBase = $Matches['ruta'].TrimEnd('/')

$esAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()
           ).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $SoloInformar -and -not $esAdmin) {
    throw 'Hay que ejecutarlo como administrador (clic derecho en PowerShell > Ejecutar como administrador), o usar -SoloInformar.'
}

# ---------- 1. Red: adaptador y IP que usaran los celulares ----------
Paso 'Buscando la IP de este PC en la red local'
$cfgs = Get-NetIPConfiguration | Where-Object {
    $_.IPv4DefaultGateway -and $_.IPv4Address -and $_.NetAdapter.Status -eq 'Up' -and
    $_.InterfaceAlias -notmatch 'vEthernet|WSL|VirtualBox|VMware|Hyper-V|Loopback|VPN|Tailscale|ZeroTier|Bluetooth' -and
    $_.NetAdapter.InterfaceDescription -notmatch 'Virtual|VPN|TAP|Tailscale|ZeroTier'
}
if (-not $cfgs) { throw 'No encontre un adaptador activo con puerta de enlace. Conecta el PC al router (cable o WiFi) y repite.' }
$cfg = $cfgs | Sort-Object { $_.NetAdapter.Name -match 'Wi-?Fi|Inal' } | Select-Object -First 1   # prefiere cable
$ip4 = $cfg.IPv4Address | Select-Object -First 1
$IpPc = $ip4.IPAddress
$ifIndex = $cfg.InterfaceIndex
$ipInfo = Get-NetIPAddress -InterfaceIndex $ifIndex -AddressFamily IPv4 | Where-Object { $_.IPAddress -eq $IpPc } | Select-Object -First 1
$esDhcp = $ipInfo.PrefixOrigin -eq 'Dhcp'
Ok "Adaptador '$($cfg.InterfaceAlias)'  IP $IpPc/$($ip4.PrefixLength)  puerta de enlace $($cfg.IPv4DefaultGateway.NextHop)"
if ($cfgs.Count -gt 1) {
    Aviso ('Hay varios adaptadores activos: ' + (($cfgs | ForEach-Object { "$($_.InterfaceAlias)=$($_.IPv4Address.IPAddress)" }) -join ', ') +
           '. Use la IP de la misma red a la que se conectan los celulares.')
}

# ---------- 2. Reserva de URL ----------
if (-not $SoloInformar) {
    Paso "Reservando la URL $Prefijo para cualquier usuario"
    # sddl D:(A;;GX;;;WD) = "Todos" (World) con permiso de escuchar; no depende del idioma de Windows.
    & netsh http delete urlacl url=$Prefijo 2>&1 | Out-Null
    $salida = & netsh http add urlacl url=$Prefijo 'sddl=D:(A;;GX;;;WD)' 2>&1
    if ($LASTEXITCODE -ne 0) { throw "netsh urlacl fallo: $salida" }
    Ok 'Reserva creada'
}

# ---------- 3. Firewall ----------
if (-not $SoloInformar) {
    Paso "Regla de firewall para el puerto $Puerto (solo red local)"
    Get-NetFirewallRule -DisplayName $ReglaFirewall -ErrorAction SilentlyContinue | Remove-NetFirewallRule
    New-NetFirewallRule -DisplayName $ReglaFirewall -Direction Inbound -Action Allow -Protocol TCP `
        -LocalPort $Puerto -Profile Private, Domain -RemoteAddress LocalSubnet | Out-Null
    Ok "Entrada TCP $Puerto permitida en perfiles Privado/Dominio, solo desde la subred local"
}

# ---------- 4. Perfil de red ----------
$perfil = Get-NetConnectionProfile -InterfaceIndex $ifIndex
if ($perfil.NetworkCategory -eq 'Public') {
    if ($PerfilPrivado -and -not $SoloInformar) {
        Set-NetConnectionProfile -InterfaceIndex $ifIndex -NetworkCategory Private
        Ok "La red '$($perfil.Name)' paso de Publica a Privada"
    } else {
        Aviso "La red '$($perfil.Name)' esta como PUBLICA: el firewall bloqueara a los celulares. Repita con -PerfilPrivado."
    }
} else {
    Ok "Perfil de red '$($perfil.Name)': $($perfil.NetworkCategory)"
}

# ---------- 5. IP fija ----------
if ($esDhcp) {
    if ($FijarIp -and -not $SoloInformar) {
        Paso 'Convirtiendo la IP actual en fija'
        $gw = $cfg.IPv4DefaultGateway.NextHop
        $dns = @($cfg.DNSServer | Where-Object { $_.AddressFamily -eq 2 } | ForEach-Object { $_.ServerAddresses } | Select-Object -Unique)
        Set-NetIPInterface -InterfaceIndex $ifIndex -Dhcp Disabled
        Get-NetIPAddress -InterfaceIndex $ifIndex -AddressFamily IPv4 -ErrorAction SilentlyContinue | Remove-NetIPAddress -Confirm:$false
        Remove-NetRoute -InterfaceIndex $ifIndex -DestinationPrefix '0.0.0.0/0' -Confirm:$false -ErrorAction SilentlyContinue
        New-NetIPAddress -InterfaceIndex $ifIndex -IPAddress $IpPc -PrefixLength $ip4.PrefixLength -DefaultGateway $gw | Out-Null
        if ($dns) { Set-DnsClientServerAddress -InterfaceIndex $ifIndex -ServerAddresses $dns }
        Ok "IP fija $IpPc. Si el router entrega esa misma IP a otro equipo habra conflicto: lo ideal es ademas reservarla en el router."
    } else {
        Aviso "La IP $IpPc es DINAMICA (DHCP): puede cambiar y los celulares dejarian de encontrar el PC."
        Aviso 'Lo mejor es una reserva DHCP en el router (con la MAC de este PC); si no, repita con -FijarIp.'
        Aviso "MAC de este adaptador: $($cfg.NetAdapter.MacAddress)"
    }
} else {
    Ok "La IP $IpPc ya es fija"
}

# ---------- 6. Suspension ----------
if ($NoSuspender -and -not $SoloInformar) {
    & powercfg /change standby-timeout-ac 0
    & powercfg /change hibernate-timeout-ac 0
    Ok 'Sin suspension ni hibernacion mientras este enchufado'
} else {
    Aviso 'Si el PC se duerme, la API deja de responder. Para evitarlo: -NoSuspender (o ajustarlo en Energia).'
}

# ---------- 7. App.config / ApiKey ----------
$claveFinal = $ApiKey
if ($RutaConfig) {
    Paso "Configurando $RutaConfig"
    if (-not (Test-Path $RutaConfig)) { throw "No existe $RutaConfig" }
    [xml]$xml = Get-Content -LiteralPath $RutaConfig -Raw
    $app = $xml.configuration.appSettings
    function Obtener($k) { ($app.add | Where-Object { $_.key -eq $k }).value }
    if (-not $claveFinal) { $claveFinal = Obtener 'ApiKey' }
    if ($SoloInformar -and $claveFinal -eq $ClavePlaceholder) {
        Aviso 'La ApiKey del config sigue con el valor de ejemplo (la API movil no se ha activado).'
        $claveFinal = $null
    } elseif (-not $claveFinal -or $claveFinal -eq $ClavePlaceholder) {
        $claveFinal = -join ((48..57) + (65..90) + (97..122) | Get-Random -Count 24 | ForEach-Object { [char]$_ })
        Aviso 'La ApiKey seguia con el valor de ejemplo: generé una nueva.'
    }
    if (-not $SoloInformar) {
        Copy-Item -LiteralPath $RutaConfig -Destination "$RutaConfig.bak" -Force
        foreach ($par in @(@('ApiHabilitada', 'true'), @('ApiPrefijo', $Prefijo), @('ApiKey', $claveFinal))) {
            $nodo = $app.add | Where-Object { $_.key -eq $par[0] }
            if (-not $nodo) {
                $nodo = $xml.CreateElement('add'); $nodo.SetAttribute('key', $par[0]); $nodo.SetAttribute('value', $par[1])
                [void]$app.AppendChild($nodo)
            } else { $nodo.value = $par[1] }
        }
        $xml.Save($RutaConfig)
        Ok "ApiHabilitada=true, ApiPrefijo y ApiKey escritos (copia en $RutaConfig.bak). Reinicie SISMistico."
    }
} elseif (-not $claveFinal) {
    Aviso 'No se indico -ApiKey ni -RutaConfig: use la ApiKey que ya tenga el App.config del restaurante.'
}

# ---------- 8. Comprobacion local ----------
Paso 'Comprobando que la API responde en este PC'
$urlPing = "http://${IpPc}:$Puerto$RutaBase/ping"
try {
    $r = Invoke-RestMethod -Uri $urlPing -TimeoutSec 4
    Ok "Responde: $urlPing"
} catch {
    Aviso "Aun no responde en $urlPing. Es normal si SISMistico no esta abierto, si ApiHabilitada=false o si no se reinicio tras cambiar el config."
}

# ---------- 9. Datos para los celulares ----------
$linea = '=' * 64
Write-Host ''
Write-Host $linea -ForegroundColor Green
Write-Host ' DATOS PARA ESCRIBIR EN CADA CELULAR (pantalla "Configurar servidor")' -ForegroundColor Green
Write-Host $linea -ForegroundColor Green
Write-Host "  Servidor / IP : $IpPc"
Write-Host "  Puerto        : $Puerto"
if ($claveFinal -and $claveFinal -ne $ClavePlaceholder) {
    Write-Host "  Clave de API  : $claveFinal"
} else {
    Write-Host '  Clave de API  : (use la ApiKey que quede en el App.config del restaurante; vuelva a ejecutar con -RutaConfig o -ApiKey para verla aqui)'
}
Write-Host '  Usuario       : cada mesero entra solo con su PIN'
Write-Host ''
Write-Host ' Antes de usarlo en el celular:' -ForegroundColor Cyan
Write-Host '   1. El celular debe estar en la MISMA red WiFi que este PC (no datos moviles).'
Write-Host "   2. Prueba en el navegador del celular: http://${IpPc}:$Puerto$RutaBase/ping"
Write-Host '      Debe mostrar un JSON con la version; si no abre, revise firewall, perfil de red y que SISMistico este abierto.'
Write-Host '   3. Apague "DNS privado"/VPN en el celular si la prueba falla solo en el celular.'
Write-Host '   4. En esta pantalla de Mesas del escritorio aparecera "N DISPOSITIVOS" cuando se conecten.'
if ($esDhcp -and -not $FijarIp) {
    Write-Host "   *  Recuerde reservar $IpPc en el router, o la IP podria cambiar." -ForegroundColor Yellow
}
Write-Host $linea -ForegroundColor Green
