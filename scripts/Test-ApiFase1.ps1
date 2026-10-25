<#
    Prueba de extremo a extremo de la fase 1 de la API movil: arranca el SISMistico.exe REAL de la
    compilacion local (contra CasaGrandeBD_prod, API en localhost), ejecuta las peticiones y lo cierra.

    Requisitos: haber compilado con  .\SISMistico\run-local.ps1 -Rebuild -NoRun  y tener la copia
    restaurada (db\Restore-ProdCopy.ps1). Las credenciales de prueba se leen de la copia local y no se imprimen.

    Uso:  .\Test-ApiFase1.ps1
#>
param(
    [string]$Database = "CasaGrandeBD_prod",
    [string]$Base = "http://localhost:8099/api/v1"
)

$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.Data
$buildDir = Join-Path $PSScriptRoot "..\SISMistico\SISMistico\bin\LocalBuild"
$exe = (Resolve-Path (Join-Path $buildDir "SISMistico.exe")).Path
$apiKey = "cambiar-esta-clave"   # valor de ejemplo: solo se admite con prefijo localhost
$results = New-Object System.Collections.Generic.List[object]

function Add-Result([string]$caso, $esperado, $obtenido) {
    $ok = ("$esperado" -eq "$obtenido")
    $results.Add([pscustomobject]@{ caso = $caso; esperado = "$esperado"; obtenido = "$obtenido"; ok = $(if ($ok) { "OK" } else { "FALLA" }) })
}

function Invoke-Api([string]$metodo, [string]$ruta, $cuerpo = $null, [hashtable]$headers = @{}, [string]$cuerpoCrudo = $null) {
    $h = @{} + $headers
    $args = @{ Uri = "$Base$ruta"; Method = $metodo; Headers = $h; UseBasicParsing = $true; TimeoutSec = 15 }
    if ($cuerpoCrudo) { $args.Body = [Text.Encoding]::UTF8.GetBytes($cuerpoCrudo); $args.ContentType = "application/json" }
    elseif ($null -ne $cuerpo) { $args.Body = [Text.Encoding]::UTF8.GetBytes(($cuerpo | ConvertTo-Json -Compress)); $args.ContentType = "application/json" }
    try {
        $r = Invoke-WebRequest @args
        return [pscustomobject]@{ status = [int]$r.StatusCode; json = $(if ($r.Content) { $r.Content | ConvertFrom-Json } else { $null }) }
    } catch {
        $resp = $_.Exception.Response
        if ($null -eq $resp) { throw }
        $text = (New-Object IO.StreamReader($resp.GetResponseStream())).ReadToEnd()
        return [pscustomobject]@{ status = [int]$resp.StatusCode; json = $(if ($text) { $text | ConvertFrom-Json } else { $null }) }
    }
}

function Invoke-Scalar([string]$sql) {
    $cn = New-Object System.Data.SqlClient.SqlConnection "Server=(localdb)\MSSQLLocalDB;Database=$Database;Integrated Security=true"
    $cn.Open(); $cmd = $cn.CreateCommand(); $cmd.CommandText = $sql
    $v = $cmd.ExecuteScalar(); $cn.Close(); return $v
}

# ---- credenciales de prueba desde la copia local (no se imprimen) ----
$cn = New-Object System.Data.SqlClient.SqlConnection "Server=(localdb)\MSSQLLocalDB;Database=$Database;Integrated Security=true"
$cn.Open(); $cmd = $cn.CreateCommand()
$cmd.CommandText = "SELECT TOP 1 Id_empleado, Password, Codigo_maestro FROM Empleados WHERE Estado='ACTIVO' AND Password IS NOT NULL AND Password <> '' AND Codigo_maestro IS NOT NULL ORDER BY Id_empleado"
$r = $cmd.ExecuteReader(); [void]$r.Read()
$idOk = [int]$r[0]; $passOk = [string]$r[1]; $pinOk = [int]$r[2]; $r.Close()
$cmd.CommandText = "SELECT TOP 1 Id_empleado, Password FROM Empleados WHERE Id_empleado > 0 AND Estado<>'ACTIVO' AND Password IS NOT NULL AND Password <> '' ORDER BY Id_empleado"
$r = $cmd.ExecuteReader(); $hayInactivo = $r.Read(); $idInact = 0; $passInact = ""
if ($hayInactivo) { $idInact = [int]$r[0]; $passInact = [string]$r[1] }; $r.Close(); $cn.Close()

$turnosAntes = "{0}|{1}|{2}" -f (Invoke-Scalar "SELECT COUNT(*) FROM Turnos"), (Invoke-Scalar "SELECT MAX(Id_turno) FROM Turnos"), (Invoke-Scalar "SELECT MAX(CASE WHEN Estado_turno='ABIERTO' THEN 1 ELSE 0 END) FROM Turnos")

# ---- arrancar el exe ----
Get-Process SISMistico -ErrorAction SilentlyContinue | ForEach-Object { throw "Ya hay un SISMistico abierto (PID $($_.Id)); cierralo antes." }
$logDir = Join-Path $buildDir "logs"
if (Test-Path $logDir) { Get-ChildItem $logDir -Filter "api-*.log" | Remove-Item -Force }
$proc = Start-Process -FilePath $exe -WorkingDirectory $buildDir -PassThru
try {
    $listo = $false
    for ($i = 0; $i -lt 30; $i++) {
        Start-Sleep -Milliseconds 500
        try { $p = Invoke-Api GET "/ping"; if ($p.status -eq 200) { $listo = $true; break } } catch { }
    }
    if (-not $listo) { throw "La API no respondio en 15 s (revisa $logDir)." }

    $k = @{ "X-Api-Key" = $apiKey }

    # 1. ping
    $p = Invoke-Api GET "/ping"
    Add-Result "ping 200" 200 $p.status
    Add-Result "ping ok=true, db=true, version 1.0" "True|True|1.0" ("{0}|{1}|{2}" -f $p.json.ok, $p.json.db, $p.json.version)
    Add-Result "ping no pide X-Api-Key" 200 (Invoke-Api GET "/ping").status

    # 2. clave de aplicacion
    Add-Result "login sin X-Api-Key -> 401" 401 (Invoke-Api POST "/auth/login" @{ id_empleado = $idOk; password = $passOk }).status
    Add-Result "login con X-Api-Key erronea -> 401" 401 (Invoke-Api POST "/auth/login" @{ id_empleado = $idOk; password = $passOk } @{ "X-Api-Key" = "otra" }).status

    # 3. validacion del cuerpo
    Add-Result "login sin cuerpo -> 400" 400 (Invoke-Api POST "/auth/login" $null $k).status
    Add-Result "login JSON invalido -> 400" 400 (Invoke-Api POST "/auth/login" $null $k "{no es json").status
    Add-Result "login solo PIN (sin id_empleado) -> 400" 400 (Invoke-Api POST "/auth/login" @{ codigo_maestra = $pinOk } $k).status
    Add-Result "login password y PIN a la vez -> 400" 400 (Invoke-Api POST "/auth/login" @{ id_empleado = $idOk; password = $passOk; codigo_maestra = $pinOk } $k).status
    Add-Result "login id_empleado como texto -> 400" 400 (Invoke-Api POST "/auth/login" @{ id_empleado = "$idOk"; password = $passOk } $k).status
    $grande = '{"id_empleado":1,"password":"' + ('x' * 300000) + '"}'
    Add-Result "cuerpo de 300 KB -> 400" 400 (Invoke-Api POST "/auth/login" $null $k $grande).status

    # 4. login correcto (password y PIN)
    $l = Invoke-Api POST "/auth/login" @{ id_empleado = $idOk; password = $passOk } $k
    Add-Result "login con password -> 200" 200 $l.status
    Add-Result "login devuelve token, expira y empleado.id" "True|True|$idOk" ("{0}|{1}|{2}" -f ([bool]$l.json.token), ([bool]$l.json.expira), $l.json.empleado.id_empleado)
    Add-Result "login NO devuelve password ni PIN" "False" ((($l.json | ConvertTo-Json -Depth 6) -match 'password|Password') -or (($l.json | ConvertTo-Json -Depth 6) -match "\b$pinOk\b"))
    $token = $l.json.token
    $l2 = Invoke-Api POST "/auth/login" @{ id_empleado = $idOk; codigo_maestra = $pinOk } $k
    Add-Result "login con id + PIN -> 200" 200 $l2.status
    $token2 = $l2.json.token

    # 5. /auth/me
    $auth = @{ "X-Api-Key" = $apiKey; "Authorization" = "Bearer $token" }
    $me = Invoke-Api GET "/auth/me" $null $auth
    Add-Result "me con token -> 200 y mismo empleado" "200|$idOk" ("{0}|{1}" -f $me.status, $me.json.empleado.id_empleado)
    Add-Result "me sin token -> 401" 401 (Invoke-Api GET "/auth/me" $null $k).status
    Add-Result "me con token falso -> 401" 401 (Invoke-Api GET "/auth/me" $null @{ "X-Api-Key" = $apiKey; "Authorization" = "Bearer 00000000-0000-0000-0000-000000000000" }).status
    Add-Result "me sin X-Api-Key -> 401" 401 (Invoke-Api GET "/auth/me" $null @{ "Authorization" = "Bearer $token" }).status

    # 6. logout invalida solo ese token
    Add-Result "logout -> 204" 204 (Invoke-Api POST "/auth/logout" $null $auth).status
    Add-Result "me tras logout -> 401" 401 (Invoke-Api GET "/auth/me" $null $auth).status
    Add-Result "el otro token sigue vivo" 200 (Invoke-Api GET "/auth/me" $null @{ "X-Api-Key" = $apiKey; "Authorization" = "Bearer $token2" }).status

    # 7. rutas
    Add-Result "ruta inexistente -> 404" 404 (Invoke-Api GET "/nada" $null $k).status
    Add-Result "GET /auth/login -> 405" 405 (Invoke-Api GET "/auth/login" $null $k).status
    Add-Result "ruta con barra final /ping/ -> 200" 200 (Invoke-Api GET "/ping/").status

    # 8. D12: empleado INACTIVO entra (paridad con el escritorio)
    if ($hayInactivo) {
        $inact = Invoke-Api POST "/auth/login" @{ id_empleado = $idInact; password = $passInact } $k
        Add-Result "D12 login de empleado INACTIVO -> 200 (paridad)" 200 $inact.status
        if ($inact.status -ne 200) { Write-Host "  detalle: $($inact.json.error.mensaje)" -ForegroundColor Yellow }
    }

    # 9. el login no toca los turnos
    $turnosDespues = "{0}|{1}|{2}" -f (Invoke-Scalar "SELECT COUNT(*) FROM Turnos"), (Invoke-Scalar "SELECT MAX(Id_turno) FROM Turnos"), (Invoke-Scalar "SELECT MAX(CASE WHEN Estado_turno='ABIERTO' THEN 1 ELSE 0 END) FROM Turnos")
    Add-Result "login no altera Turnos (cantidad|ultimo id|hay abierto)" $turnosAntes $turnosDespues

    # 10. limite de intentos: 5 fallos y el 6.o se bloquea (esto bloquea la IP 60 s, por eso va al final)
    $codigos = 1..6 | ForEach-Object { (Invoke-Api POST "/auth/login" @{ id_empleado = $idOk; password = "incorrecta-$_" } $k).status }
    Add-Result "5 fallos -> 403 y el 6.o -> 429" "403,403,403,403,403,429" ($codigos -join ",")
    Add-Result "bloqueada la IP, ni el login correcto pasa -> 429" 429 (Invoke-Api POST "/auth/login" @{ id_empleado = $idOk; password = $passOk } $k).status
}
finally {
    if ($proc -and -not $proc.HasExited) { Stop-Process -Id $proc.Id -Force }
}

# ---- el log no debe contener credenciales ni tokens ----
Start-Sleep -Milliseconds 500
$logs = Get-ChildItem $logDir -Filter "api-*.log" -ErrorAction SilentlyContinue
$texto = if ($logs) { ($logs | Get-Content -Raw -Encoding UTF8) } else { "" }
Add-Result "existe el log de la API con peticiones" "True" ([bool]($texto -match "POST /auth/login"))
Add-Result "el log no contiene la contrasena" "False" ($texto.Contains($passOk))
Add-Result "el log no contiene tokens" "False" ($texto.Contains($token) -or $texto.Contains($token2))

$results | Format-Table -AutoSize | Out-String -Width 200
$fallan = @($results | Where-Object { $_.ok -ne "OK" }).Count
"{0} OK de {1} ({2} fallan)" -f ($results.Count - $fallan), $results.Count, $fallan
if ($fallan -gt 0) { exit 1 }
