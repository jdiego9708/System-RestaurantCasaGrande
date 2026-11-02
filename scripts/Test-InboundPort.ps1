<#
    Prueba de entrada TCP independiente de SISRestaurante y de http.sys.

    Abre un servidor minimo en un puerto (8098 por defecto), con una regla de firewall temporal, y contesta
    a cualquier navegador con un texto. Sirve para saber si un celular puede abrir CUALQUIER conexion hacia
    este PC por un puerto que no sea de Windows (135/139/445).

    EJECUTAR EN EL PC DEL RESTAURANTE:
        powershell -NoProfile -ExecutionPolicy Bypass -File .\Test-InboundPort.ps1
        powershell -NoProfile -ExecutionPolicy Bypass -File .\Test-InboundPort.ps1 -Port 8080 -Seconds 180

    Al terminar (o con Ctrl+C) borra la regla temporal. No toca SISRestaurante ni su configuracion.
#>
[CmdletBinding()]
param(
    [int]$Port = 8098,
    [int]$Seconds = 120
)

$ErrorActionPreference = 'Stop'
$TempRuleName = "Prueba de entrada TCP $Port (temporal)"

$isElevated = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()
              ).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isElevated) {
    Write-Host 'Se necesitan permisos de administrador: acepte el aviso de Windows (UAC)...' -ForegroundColor Yellow
    $relaunch = @('-NoProfile', '-ExecutionPolicy', 'Bypass', '-NoExit', '-File', "`"$PSCommandPath`"", '-Port', $Port, '-Seconds', $Seconds)
    Start-Process -FilePath 'powershell.exe' -Verb RunAs -ArgumentList $relaunch
    exit 0
}

$ips = @(Get-NetIPAddress -AddressFamily IPv4 | Where-Object { $_.IPAddress -notlike '127.*' -and $_.IPAddress -notlike '169.254.*' } |
    ForEach-Object { $_.IPAddress })

$listener = $null
try {
    New-NetFirewallRule -DisplayName $TempRuleName -Direction Inbound -Action Allow -Protocol TCP -LocalPort $Port -Profile Any | Out-Null
    $listener = [Net.Sockets.TcpListener]::new([Net.IPAddress]::Any, $Port)
    $listener.Start()

    Write-Host ''
    Write-Host "Servidor de prueba escuchando en el puerto $Port durante $Seconds segundos." -ForegroundColor Green
    foreach ($ip in $ips) { Write-Host "  En el navegador del celular abra:  http://${ip}:$Port/" -ForegroundColor Cyan }
    Write-Host '  (Ctrl+C para terminar antes)'
    Write-Host ''

    $hits = 0
    $deadline = (Get-Date).AddSeconds($Seconds)
    while ((Get-Date) -lt $deadline) {
        if (-not $listener.Pending()) { Start-Sleep -Milliseconds 200; continue }
        $client = $listener.AcceptTcpClient()
        try {
            $remote = $client.Client.RemoteEndPoint.ToString()
            $hits++
            Write-Host ("{0:HH:mm:ss}  CONEXION desde {1}" -f (Get-Date), $remote) -ForegroundColor Green
            $stream = $client.GetStream()
            $stream.ReadTimeout = 2000
            # Se lee la peticion (si llega) solo para no cortar al navegador antes de responder.
            $buffer = New-Object byte[] 4096
            try { [void]$stream.Read($buffer, 0, $buffer.Length) } catch { }
            $body = "Conexion OK con $env:COMPUTERNAME en el puerto $Port. Su IP: $remote"
            $bytes = [Text.Encoding]::UTF8.GetBytes($body)
            $head = "HTTP/1.1 200 OK`r`nContent-Type: text/plain; charset=utf-8`r`nContent-Length: $($bytes.Length)`r`nConnection: close`r`n`r`n"
            $headBytes = [Text.Encoding]::ASCII.GetBytes($head)
            $stream.Write($headBytes, 0, $headBytes.Length)
            $stream.Write($bytes, 0, $bytes.Length)
            $stream.Flush()
        } finally { $client.Close() }
    }

    Write-Host ''
    if ($hits -gt 0) {
        Write-Host "RESULTADO: llegaron $hits conexiones. La red entre el celular y este PC funciona por el puerto $Port." -ForegroundColor Green
    } else {
        Write-Host "RESULTADO: no llego ninguna conexion al puerto $Port." -ForegroundColor Yellow
    }
} finally {
    if ($listener) { $listener.Stop() }
    Get-NetFirewallRule -DisplayName $TempRuleName -ErrorAction SilentlyContinue | Remove-NetFirewallRule
    Write-Host 'Regla temporal borrada.'
}
