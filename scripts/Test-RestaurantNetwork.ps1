<#
    Diagnostico de red de la API movil, para el PC REAL del restaurante.

    Sirve cuando en casa funciona y en el restaurante no: separa si el fallo esta en el PC (API apagada,
    reserva de URL, firewall, perfil de red, antivirus) o en la red (aislamiento de clientes del router,
    red de invitados, repetidor con otra subred, celular con datos moviles).

    EJECUTAR EN EL PC DEL RESTAURANTE, con SISRestaurante ABIERTO:
        .\Test-RestaurantNetwork.ps1                          # diagnostico completo + prueba en vivo con el celular
        .\Test-RestaurantNetwork.ps1 -PhoneIp 192.168.1.57    # ademas compara la subred y hace ping al celular
        .\Test-RestaurantNetwork.ps1 -OpenWide                # durante la prueba en vivo abre el puerto a CUALQUIER
                                                              #   origen y perfil (regla temporal, se borra al final)
        .\Test-RestaurantNetwork.ps1 -Fix                     # aplica los arreglos del lado del PC (ver al final)
        .\Test-RestaurantNetwork.ps1 -SkipLive                # sin la prueba en vivo

    No cambia nada salvo: registro del firewall durante la prueba (se restaura), la regla temporal de -OpenWide
    (se borra) y lo que haga -Fix. Deja un informe en el Escritorio para enviarlo.
#>
[CmdletBinding()]
param(
    # IP del celular (Ajustes > WiFi > red conectada > Direccion IP).
    [string]$PhoneIp,
    # Segundos que dura la prueba en vivo esperando la peticion del celular.
    [int]$WatchSeconds = 90,
    [switch]$OpenWide,
    [switch]$Fix,
    [switch]$SkipLive,
    [string]$ConfigPath = (Join-Path $env:ProgramFiles 'SISRestaurante\SISMistico.exe.config')
)

$ErrorActionPreference = 'Stop'
$FirewallRuleName = 'SISRestaurante API'
$TempRuleName = 'SISRestaurante API (diagnostico temporal)'
$PlaceholderKey = 'cambiar-esta-clave'

function Step($t) { Write-Host ''; Write-Host "==> $t" -ForegroundColor Cyan }
function Ok($t)   { Write-Host "    OK  $t" -ForegroundColor Green }
function Warn($t) { Write-Host "    !!  $t" -ForegroundColor Yellow; $script:Problems += $t }
function Info($t) { Write-Host "        $t" }

# ---------- Administrador ----------
$isElevated = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()
              ).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isElevated) {
    # Se relanza con UAC: el firewall y su registro solo se pueden leer como administrador.
    Write-Host 'Se necesitan permisos de administrador: acepte el aviso de Windows (UAC)...' -ForegroundColor Yellow
    $relaunch = @('-NoProfile', '-ExecutionPolicy', 'Bypass', '-NoExit', '-File', "`"$PSCommandPath`"")
    foreach ($p in $PSBoundParameters.GetEnumerator()) {
        if ($p.Value -is [System.Management.Automation.SwitchParameter]) { if ($p.Value) { $relaunch += "-$($p.Key)" } }
        else { $relaunch += "-$($p.Key)"; $relaunch += "`"$($p.Value)`"" }
    }
    try { Start-Process -FilePath 'powershell.exe' -Verb RunAs -ArgumentList $relaunch }
    catch { throw 'No se obtuvo permiso de administrador. Abra PowerShell como administrador y repita.' }
    exit 0
}

$Problems = @()
$reportPath = Join-Path ([Environment]::GetFolderPath('Desktop')) ("diagnostico-red-{0:yyyyMMdd-HHmm}.txt" -f (Get-Date))
try { Start-Transcript -Path $reportPath -Force | Out-Null } catch { $reportPath = $null }

Write-Host ('=' * 70) -ForegroundColor Green
Write-Host (' Diagnostico de red de la API movil  -  {0}  -  {1:yyyy-MM-dd HH:mm}' -f $env:COMPUTERNAME, (Get-Date)) -ForegroundColor Green
Write-Host ('=' * 70) -ForegroundColor Green

# ---------- 1. Configuracion instalada ----------
Step 'Configuracion del sistema instalado'
$prefix = 'http://+:8099/api/v1/'
$apiKey = $null
$logFolder = $null
if (Test-Path -LiteralPath $ConfigPath) {
    [xml]$cfgXml = Get-Content -LiteralPath $ConfigPath -Raw
    function Get-Setting($k) { ($cfgXml.configuration.appSettings.add | Where-Object { $_.key -eq $k }).value }
    Ok "Config: $ConfigPath"
    if ((Get-Setting 'ApiHabilitada') -eq 'true') { Ok 'ApiHabilitada = true' }
    else { Warn 'ApiHabilitada no es true: la API esta APAGADA. Reinstale marcando "Activar la API movil".' }
    $p = Get-Setting 'ApiPrefijo'; if ($p) { $prefix = $p }
    if (-not $prefix.EndsWith('/')) { $prefix += '/' }
    Ok "ApiPrefijo = $prefix"
    $apiKey = Get-Setting 'ApiKey'
    if (-not $apiKey -or $apiKey -eq $PlaceholderKey) { Warn 'ApiKey sigue con el valor de ejemplo: la API NO arranca en red con esa clave.' }
    else { Ok ("ApiKey definida ({0}... , {1} caracteres)" -f $apiKey.Substring(0, [Math]::Min(3, $apiKey.Length)), $apiKey.Length) }
    $lf = Get-Setting 'ApiLogCarpeta'; if (-not $lf) { $lf = 'logs' }
    if ([IO.Path]::IsPathRooted($lf)) { $logFolder = $lf } else { $logFolder = Join-Path (Split-Path $ConfigPath) $lf }
} else {
    Warn "No existe $ConfigPath (el sistema no esta instalado ahi). Indique -ConfigPath."
}
if ($prefix -notmatch '^http://(?<host>[^:/]+):(?<port>\d+)(?<path>/.*)$') { throw "ApiPrefijo con forma inesperada: $prefix" }
$Port = [int]$Matches['port']
$BasePath = $Matches['path'].TrimEnd('/')
if ($Matches['host'] -match '^(localhost|127\.0\.0\.1)$') {
    Warn "El prefijo escucha solo en $($Matches['host']): los celulares NUNCA podran entrar. Debe ser http://+:$Port$BasePath/"
}

# ---------- 2. Proceso, puerto y reserva de URL ----------
Step 'SISRestaurante y el puerto de la API'
$proc = Get-Process -Name 'SISMistico' -ErrorAction SilentlyContinue
if ($proc) { Ok "SISMistico abierto (PID $($proc.Id -join ', '))" }
else { Warn 'SISMistico NO esta abierto: la API solo existe mientras el programa esta abierto.' }

$listen = Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue
if ($listen) {
    foreach ($l in $listen) {
        $owner = (Get-Process -Id $l.OwningProcess -ErrorAction SilentlyContinue).ProcessName
        Ok "Escuchando en $($l.LocalAddress):$Port (proceso $($l.OwningProcess) $owner)"
    }
    # El HttpListener escucha por http.sys, que aparece como proceso 4 (System). Otro dueno = puerto robado.
    if ($listen | Where-Object { $_.OwningProcess -ne 4 }) { Warn "Otro programa ocupa el puerto $Port." }
} else {
    Warn "Nadie escucha en el puerto $Port."
}

$svcState = (& netsh http show servicestate view=requestq) -join "`n"
if ($svcState -match [regex]::Escape(":$Port$BasePath")) { Ok 'http.sys tiene registrada la URL de la API (el programa la abrio)' }
else { Warn 'http.sys NO tiene la URL de la API registrada: el programa no logro arrancar la API (ver log mas abajo).' }

$urlacl = (& netsh http show urlacl url=$prefix) -join "`n"
if ($urlacl -match [regex]::Escape($prefix)) { Ok "Reserva de URL presente para $prefix" }
else { Warn "Falta la reserva de URL $prefix (error 5 Acceso denegado al arrancar). Arreglo: -Fix." }

# ---------- 3. Log de la API ----------
Step 'Ultimas lineas del log de la API'
$apiLog = $null
foreach ($folder in @($logFolder, (Join-Path $env:TEMP 'SISMistico-api-logs'), 'C:\Windows\Temp\SISMistico-api-logs')) {
    if ($folder -and (Test-Path $folder)) {
        $f = Get-ChildItem $folder -Filter 'api-*.log' -ErrorAction SilentlyContinue | Sort-Object LastWriteTime -Descending | Select-Object -First 1
        if ($f -and (-not $apiLog -or $f.LastWriteTime -gt $apiLog.LastWriteTime)) { $apiLog = $f }
    }
}
if ($apiLog) {
    Ok "$($apiLog.FullName)  (modificado $($apiLog.LastWriteTime))"
    Get-Content -LiteralPath $apiLog.FullName -Tail 15 | ForEach-Object { Info $_ }
    $tail = (Get-Content -LiteralPath $apiLog.FullName -Tail 200) -join "`n"
    if ($tail -match 'API no iniciada') { Warn 'El log dice "API no iniciada": ver el motivo en las lineas de arriba.' }
    # Las peticiones del propio PC (por su IP de red) no prueban nada de la red.
    $ownIps = @(Get-NetIPAddress -ErrorAction SilentlyContinue | ForEach-Object { $_.IPAddress })
    $remoteReq = (Get-Content -LiteralPath $apiLog.FullName) | Where-Object {
        $_ -match '\| REQ .*ip=(?<ip>\S+)' -and $Matches['ip'] -notin $ownIps -and $Matches['ip'] -notmatch '^(127\.0\.0\.1|::1)$' }
    if ($remoteReq) { Ok "Hoy SI llegaron $(@($remoteReq).Count) peticiones desde otros equipos (la red funciona al menos a ratos)" }
    else { Info 'Hoy no ha llegado ninguna peticion desde otro equipo.' }
} else {
    Warn 'No encontre el log de la API (api-*.log).'
}

# ---------- 4. Adaptadores y red ----------
Step 'Adaptadores de red de este PC'
$adapters = @(Get-NetIPConfiguration | Where-Object { $_.IPv4Address -and $_.NetAdapter.Status -eq 'Up' })
$mainCfg = $null
foreach ($a in $adapters) {
    $ip = $a.IPv4Address | Select-Object -First 1
    $netProfile = Get-NetConnectionProfile -InterfaceIndex $a.InterfaceIndex -ErrorAction SilentlyContinue
    $origin = (Get-NetIPAddress -InterfaceIndex $a.InterfaceIndex -AddressFamily IPv4 | Where-Object { $_.IPAddress -eq $ip.IPAddress }).PrefixOrigin
    $gw = if ($a.IPv4DefaultGateway) { $a.IPv4DefaultGateway.NextHop } else { '(sin puerta de enlace)' }
    Info ("{0,-28} {1}/{2}  gw {3}  perfil {4}  origen {5}  [{6}]" -f $a.InterfaceAlias, $ip.IPAddress, $ip.PrefixLength, $gw,
        $(if ($netProfile) { $netProfile.NetworkCategory } else { '?' }), $origin, $a.NetAdapter.InterfaceDescription)
    if ($a.IPv4DefaultGateway -and -not $mainCfg -and $a.NetAdapter.InterfaceDescription -notmatch 'Virtual|VPN|TAP|Hyper-V|WSL') { $mainCfg = $a }
    if ($a.IPv4DefaultGateway -and $netProfile -and $netProfile.NetworkCategory -eq 'Public') {
        Warn "La red '$($netProfile.Name)' ($($a.InterfaceAlias)) esta como PUBLICA: la regla de la API no aplica ahi. Arreglo: -Fix."
    }
    if ($ip.IPAddress -like '169.254.*') { Warn "$($a.InterfaceAlias) tiene IP 169.254.x: no recibio IP del router." }
}
if (@($adapters | Where-Object { $_.IPv4DefaultGateway }).Count -gt 1) {
    Warn 'Hay varios adaptadores con puerta de enlace (p. ej. cable + WiFi, o VPN): el celular debe estar en la red de la IP que se le escribio.'
}
if (-not $mainCfg) { Warn 'Ningun adaptador fisico tiene puerta de enlace: el PC no esta conectado al router.' }
$PcIp = $null; $PrefixLength = 24
if ($mainCfg) {
    $PcIp = ($mainCfg.IPv4Address | Select-Object -First 1).IPAddress
    $PrefixLength = ($mainCfg.IPv4Address | Select-Object -First 1).PrefixLength
    Ok "IP que deben usar los celulares: $PcIp  (nombre del PC: $env:COMPUTERNAME)"
    $gwIp = $mainCfg.IPv4DefaultGateway.NextHop
    if (Test-Connection -ComputerName $gwIp -Count 2 -Quiet) { Ok "El router ($gwIp) responde" } else { Info "El router ($gwIp) no responde al ping (algunos lo bloquean; no es concluyente)" }
    $neighbors = @(Get-NetNeighbor -InterfaceIndex $mainCfg.InterfaceIndex -AddressFamily IPv4 -ErrorAction SilentlyContinue |
        Where-Object { $_.State -in 'Reachable', 'Stale', 'Delay', 'Probe' -and $_.IPAddress -notmatch '\.255$' -and $_.IPAddress -ne $gwIp })
    Info "Equipos vistos recientemente en la red (tabla ARP, sin el router): $($neighbors.Count)"
    if ($neighbors.Count -eq 0) { Info 'Si hay celulares conectados y aqui no aparece ninguno, apunta a aislamiento de clientes en el router.' }
}

# Calcula si dos IP estan en la misma subred con la mascara del PC.
function Test-SameSubnet([string]$a, [string]$b, [int]$len) {
    $ba = ([Net.IPAddress]::Parse($a)).GetAddressBytes(); $bb = ([Net.IPAddress]::Parse($b)).GetAddressBytes()
    [Array]::Reverse($ba); [Array]::Reverse($bb)
    $mask = if ($len -eq 0) { 0 } else { [uint32]([uint64]4294967295 -shl (32 - $len) -band 4294967295) }
    return (([BitConverter]::ToUInt32($ba, 0) -band $mask) -eq ([BitConverter]::ToUInt32($bb, 0) -band $mask))
}

if ($PhoneIp -and $PcIp) {
    Step "Celular $PhoneIp"
    if (Test-SameSubnet $PcIp $PhoneIp $PrefixLength) { Ok "Misma subred que el PC ($PcIp/$PrefixLength)" }
    else {
        Warn "El celular ($PhoneIp) esta en OTRA subred que el PC ($PcIp/$PrefixLength): otro router, repetidor o red de invitados."
        Warn 'La regla de firewall solo admite la subred local, y la app solo busca el servidor en su /24. Conecte ambos al MISMO router/red.'
    }
    if (Test-Connection -ComputerName $PhoneIp -Count 3 -Quiet) { Ok 'El celular responde al ping desde el PC: hay comunicacion entre ambos' }
    else { Info 'El celular no responde al ping (Android a veces no responde; si tampoco aparece en ARP, sospechar aislamiento de clientes)' }
    $n = Get-NetNeighbor -IPAddress $PhoneIp -ErrorAction SilentlyContinue
    if ($n -and $n.State -ne 'Unreachable') { Ok "El PC ve la MAC del celular ($($n.LinkLayerAddress)): estan en la misma red fisica" }
    else { Warn 'El PC no ve la MAC del celular: no estan en el mismo segmento o el router los aisla (AP/client isolation).' }
}

# ---------- 5. Firewall de Windows ----------
Step 'Firewall de Windows'
$fwProfiles = Get-NetFirewallProfile -PolicyStore ActiveStore
foreach ($fp in $fwProfiles) {
    Info ("Perfil {0,-8} activo={1}  entrada por defecto={2}  reglas locales permitidas={3}" -f $fp.Name, $fp.Enabled, $fp.DefaultInboundAction, $fp.AllowLocalFirewallRules)
    if ($fp.AllowLocalFirewallRules -eq 'False') { Warn "En el perfil $($fp.Name) una directiva de dominio IGNORA las reglas locales: la regla de la API no sirve ahi." }
}
$rule = Get-NetFirewallRule -DisplayName $FirewallRuleName -ErrorAction SilentlyContinue
if ($rule) {
    $pf = $rule | Get-NetFirewallPortFilter; $af = $rule | Get-NetFirewallAddressFilter
    Ok ("Regla '{0}': habilitada={1} accion={2} perfiles={3} puerto={4} origen={5}" -f $FirewallRuleName, $rule.Enabled, $rule.Action, $rule.Profile, $pf.LocalPort, ($af.RemoteAddress -join ','))
    if ($rule.Enabled -ne 'True') { Warn 'La regla de la API esta deshabilitada.' }
    if ("$($pf.LocalPort)" -ne "$Port") { Warn "La regla abre el puerto $($pf.LocalPort) pero la API usa el $Port." }
} else {
    Warn "No existe la regla de firewall '$FirewallRuleName'. Arreglo: -Fix."
}
# Las reglas de bloqueo ganan siempre a las de permitir.
$blocking = Get-NetFirewallRule -Direction Inbound -Action Block -Enabled True -ErrorAction SilentlyContinue | Where-Object {
    $ports = ($_ | Get-NetFirewallPortFilter).LocalPort
    $ports -contains "$Port" -or ($ports -contains 'Any' -and $_.DisplayName -match 'SIS|Mistico|Restaurante')
}
foreach ($b in $blocking) { Warn "Regla de BLOQUEO que afecta al puerto: '$($b.DisplayName)' (perfiles $($b.Profile))" }

# ---------- 6. Antivirus / firewall de terceros ----------
Step 'Antivirus y firewall de terceros'
$thirdParty = $false
foreach ($cls in 'FirewallProduct', 'AntiVirusProduct') {
    $items = Get-CimInstance -Namespace 'root/SecurityCenter2' -ClassName $cls -ErrorAction SilentlyContinue
    foreach ($i in $items) {
        Info "$cls : $($i.displayName)"
        if ($i.displayName -notmatch 'Windows Defender|Microsoft Defender') {
            $thirdParty = $true
            if ($cls -eq 'FirewallProduct') { Warn "Firewall de terceros '$($i.displayName)': manda sobre el de Windows y puede bloquear el puerto $Port aunque Windows lo permita." }
        }
    }
}
if (-not $thirdParty) { Ok 'Solo Microsoft Defender' }

# ---------- 7. Arreglos del lado del PC ----------
# Van antes de la prueba en vivo para que esta ya mida el resultado del arreglo.
if ($Fix) {
    Step 'Aplicando arreglos del lado del PC (-Fix)'
    if ($mainCfg) {
        $mp = Get-NetConnectionProfile -InterfaceIndex $mainCfg.InterfaceIndex
        if ($mp.NetworkCategory -eq 'Public') { Set-NetConnectionProfile -InterfaceIndex $mainCfg.InterfaceIndex -NetworkCategory Private; Ok "Red '$($mp.Name)' pasada a Privada" }
    }
    & netsh http delete urlacl url=$prefix 2>&1 | Out-Null
    $o = & netsh http add urlacl url=$prefix 'sddl=D:(A;;GX;;;WD)' 2>&1
    if ($LASTEXITCODE -eq 0) { Ok "Reserva de URL $prefix creada" } else { Warn "netsh urlacl fallo: $o" }
    $remote = @('LocalSubnet')
    if ($PhoneIp -and $PcIp -and -not (Test-SameSubnet $PcIp $PhoneIp $PrefixLength)) {
        # El celular esta en otra subred privada: se admite su /24 en concreto, no internet.
        $remote += (($PhoneIp -split '\.')[0..2] -join '.') + '.0/24'
    }
    Get-NetFirewallRule -DisplayName $FirewallRuleName -ErrorAction SilentlyContinue | Remove-NetFirewallRule
    New-NetFirewallRule -DisplayName $FirewallRuleName -Direction Inbound -Action Allow -Protocol TCP -LocalPort $Port `
        -Profile Private, Domain -RemoteAddress $remote | Out-Null
    Ok "Regla '$FirewallRuleName' recreada: puerto $Port, perfiles Privado/Dominio, origen $($remote -join ', ')"
    Info 'Si la reserva de URL cambio, cierre y vuelva a abrir SISRestaurante.'
}

# ---------- 8. Prueba local ----------
Step 'Prueba de la API desde este mismo PC'
foreach ($h in @('localhost', $PcIp)) {
    if (-not $h) { continue }
    $u = "http://${h}:$Port$BasePath/ping"
    try {
        $r = Invoke-RestMethod -Uri $u -TimeoutSec 5
        Ok "$u -> servidor=$($r.servidor) db=$($r.db) version=$($r.version)"
    } catch { Warn "$u no responde: $($_.Exception.Message)" }
}
Info 'Ojo: probar la IP propia desde el PC no pasa por el firewall; la prueba de verdad es desde el celular (paso 8).'

# ---------- 8. Prueba en vivo con el celular ----------
$liveResult = 'no ejecutada'
if (-not $SkipLive -and $PcIp) {
    Step "Prueba en vivo: abra en el NAVEGADOR del celular  http://${PcIp}:$Port$BasePath/ping"
    Info 'El celular debe estar en el WiFi del restaurante con los DATOS MOVILES APAGADOS.'
    Info "Tiene $WatchSeconds segundos. Recargue la pagina 2 o 3 veces. (Ctrl+C para terminar antes)"

    $saved = Get-NetFirewallProfile | Select-Object Name, LogAllowed, LogBlocked, LogFileName
    $fwLogs = @{}
    try {
        Set-NetFirewallProfile -All -LogAllowed True -LogBlocked True
        foreach ($s in $saved) {
            $path = [Environment]::ExpandEnvironmentVariables($s.LogFileName)
            $len = 0; if (Test-Path $path) { $len = (Get-Item $path).Length }
            $fwLogs[$path] = $len
        }
        if ($OpenWide) {
            New-NetFirewallRule -DisplayName $TempRuleName -Direction Inbound -Action Allow -Protocol TCP -LocalPort $Port -Profile Any -RemoteAddress Any | Out-Null
            Info 'Regla temporal abierta: puerto permitido desde cualquier origen y perfil.'
        }
        $apiLogStart = 0; if ($apiLog) { $apiLogStart = @(Get-Content -LiteralPath $apiLog.FullName).Count }

        $seenAllow = @(); $seenDrop = @(); $seenReq = @()
        $deadline = (Get-Date).AddSeconds($WatchSeconds)
        while ((Get-Date) -lt $deadline) {
            Start-Sleep -Seconds 3
            foreach ($path in @($fwLogs.Keys)) {
                if (-not (Test-Path $path)) { continue }
                # El servicio del firewall tiene el fichero abierto: se lee compartido.
                $fs = [IO.File]::Open($path, 'Open', 'Read', 'ReadWrite')
                try {
                    if ($fs.Length -lt $fwLogs[$path]) { $fwLogs[$path] = 0 }
                    $fs.Seek($fwLogs[$path], 'Begin') | Out-Null
                    $text = (New-Object IO.StreamReader($fs)).ReadToEnd()
                    $fwLogs[$path] = $fs.Length
                } finally { $fs.Dispose() }
                foreach ($line in ($text -split "`r?`n")) {
                    $c = $line -split ' '
                    if ($c.Count -lt 8 -or $c[7] -ne "$Port" -or $c[3] -ne 'TCP') { continue }
                    if ($c[4] -match '^(127\.|::1)' -or $c[4] -eq $PcIp) { continue }
                    if ($c[2] -eq 'DROP') { $seenDrop += $c[4]; Write-Host "    BLOQUEADO por el firewall de Windows: $($c[4]) -> puerto $Port" -ForegroundColor Red }
                    elseif ($c[2] -eq 'ALLOW') { $seenAllow += $c[4]; Write-Host "    Llego y el firewall lo dejo pasar: $($c[4])" -ForegroundColor Green }
                }
            }
            if ($apiLog) {
                $all = @(Get-Content -LiteralPath $apiLog.FullName)
                if ($all.Count -gt $apiLogStart) {
                    foreach ($l in $all[$apiLogStart..($all.Count - 1)]) {
                        if ($l -match '\| REQ ' -and $l -notmatch 'ip=(127\.0\.0\.1|::1)' -and $l -notmatch "ip=$([regex]::Escape($PcIp))\b") { $seenReq += $l; Write-Host "    La API recibio: $l" -ForegroundColor Green }
                    }
                    $apiLogStart = $all.Count
                }
            }
            if ($seenReq.Count -ge 2) { break }
        }

        Write-Host ''
        if ($seenReq.Count -gt 0) {
            $liveResult = 'OK: la peticion del celular llego a la API'
            Ok 'La peticion del celular LLEGO a la API. La red funciona.'
            Info 'Si la app sigue sin conectar: revise en la app la IP, el puerto y la CLAVE (y que no tenga espacios).'
            if ($OpenWide) { Warn 'Funciono con la regla temporal abierta: el problema era el perfil de red o la subred. Ejecute con -Fix (y -PhoneIp si el celular esta en otra subred).' }
        } elseif ($seenDrop.Count -gt 0) {
            $liveResult = "BLOQUEADO por el firewall de Windows (origen $(($seenDrop | Select-Object -Unique) -join ', '))"
            Warn 'El celular SI llega al PC pero el firewall de Windows lo bloquea. Arreglo: -Fix (perfil Privado + regla). Si persiste, repita con -OpenWide.'
        } elseif ($seenAllow.Count -gt 0) {
            $liveResult = 'llega al PC y pasa el firewall, pero la API no lo registra'
            Warn 'El paquete pasa el firewall pero la API no lo atiende: la API no arranco o la URL no coincide (ver pasos 2 y 3).'
        } else {
            $liveResult = 'NADA llego al PC'
            Warn 'No llego NADA del celular al PC en el puerto de la API.'
            if ($thirdParty) { Warn 'Primer sospechoso: el firewall/antivirus de terceros (bloquea antes que Windows y no deja rastro aqui). Desactivelo un momento y repita.' }
            Info 'Si no es eso, el problema es de la RED, no del PC:'
            Info '  1. Datos moviles del celular encendidos o celular en otra WiFi (p. ej. "Invitados"/"Guest").'
            Info '  2. AISLAMIENTO DE CLIENTES en el router o punto de acceso ("AP Isolation", "Client Isolation",'
            Info '     "Aislamiento de AP", "Wireless Isolation", "Permitir que los clientes se vean"): hay que DESACTIVARLO.'
            Info '     Es lo mas comun en routers de operador y en redes WiFi para clientes del restaurante.'
            Info '  3. El PC por cable a un router y el celular a un repetidor/otro router con su propia red (otra subred).'
            Info '  Prueba rapida: comparta internet desde otro celular (zona WiFi), conecte PC y celular a ella y repita.'
            Info '  Si asi funciona, el PC esta bien y hay que ajustar el router del restaurante.'
        }
    } finally {
        foreach ($s in $saved) { Set-NetFirewallProfile -Name $s.Name -LogAllowed $s.LogAllowed -LogBlocked $s.LogBlocked }
        if ($OpenWide) { Get-NetFirewallRule -DisplayName $TempRuleName -ErrorAction SilentlyContinue | Remove-NetFirewallRule }
    }
}

# ---------- Resumen ----------
Write-Host ''
Write-Host ('=' * 70) -ForegroundColor Green
Write-Host ' RESUMEN' -ForegroundColor Green
Write-Host ('=' * 70) -ForegroundColor Green
Info "PC: $env:COMPUTERNAME  IP: $PcIp  Puerto: $Port  URL de prueba: http://${PcIp}:$Port$BasePath/ping"
Info "Prueba en vivo: $liveResult"
if ($Problems.Count -eq 0) { Ok 'No se detectaron problemas del lado del PC.' }
else { Write-Host '  Problemas encontrados:' -ForegroundColor Yellow; $Problems | Select-Object -Unique | ForEach-Object { Write-Host "   - $_" -ForegroundColor Yellow } }
if ($reportPath) { try { Stop-Transcript | Out-Null } catch { }; Write-Host ''; Write-Host "Informe guardado en: $reportPath  (envielo para revisarlo; la clave no aparece completa)" -ForegroundColor Cyan }
