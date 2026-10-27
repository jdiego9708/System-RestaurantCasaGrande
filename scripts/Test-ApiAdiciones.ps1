<#
    Prueba de extremo a extremo de la IMPRESION DE ADICIONES: al agregar productos a un pedido que ya existe
    (POST /pedidos/{id}/items) deben imprimirse SOLO las lineas nuevas, nunca el pedido completo.

    Arranca el SISMistico.exe REAL de la compilacion local contra CasaGrandeBD_prod (en otro puerto, por defecto
    8199, para no chocar con una instancia instalada), crea pedidos de verdad, les agrega productos con y sin
    impresion y comprueba que archivos PDF aparecen en Documentos\Casa Grande\Comandas. Al terminar BORRA lo que
    creo (pedidos, lineas y PDF de la prueba) y restaura el .exe.config.

    IMPRIME DE VERDAD: exige que la impresora predeterminada sea de PDF (Microsoft Print to PDF...), para no
    gastar papel. Requisitos: .\SISMistico\run-local.ps1 -Rebuild -NoRun, la copia restaurada y los SP nuevos.

    Uso:  .\Test-ApiAdiciones.ps1
#>
param(
    [string]$Database = "CasaGrandeBD_prod",
    [int]$Puerto = 8199
)

$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.Data
$Base = "http://localhost:$Puerto/api/v1"
$buildDir = Join-Path $PSScriptRoot "..\SISMistico\SISMistico\bin\LocalBuild"
$exe = (Resolve-Path (Join-Path $buildDir "SISMistico.exe")).Path
$exeConfig = "$exe.config"
$cs = "Server=(localdb)\MSSQLLocalDB;Database=$Database;Integrated Security=true"
$results = New-Object System.Collections.Generic.List[object]

function Add-Result([string]$caso, $esperado, $obtenido) {
    $results.Add([pscustomobject]@{ caso = $caso; esperado = "$esperado"; obtenido = "$obtenido"; ok = $(if ("$esperado" -eq "$obtenido") { "OK" } else { "FALLA" }) })
}
function Sql-Scalar([string]$sql) {
    $cn = New-Object System.Data.SqlClient.SqlConnection $cs; $cn.Open()
    $cmd = $cn.CreateCommand(); $cmd.CommandText = $sql; $v = $cmd.ExecuteScalar(); $cn.Close()
    if ($v -is [DBNull]) { return $null }
    return $v
}
function Sql-Exec([string]$sql) {
    $cn = New-Object System.Data.SqlClient.SqlConnection $cs; $cn.Open()
    $cmd = $cn.CreateCommand(); $cmd.CommandText = $sql; [void]$cmd.ExecuteNonQuery(); $cn.Close()
}
function Invoke-Api([string]$ruta, [hashtable]$headers = $null, [string]$metodo = "GET", $cuerpo = $null) {
    $a = @{ Uri = "$Base$ruta"; Method = $metodo; UseBasicParsing = $true; TimeoutSec = 30 }
    if ($headers) { $a.Headers = $headers }
    if ($null -ne $cuerpo) { $a.Body = [Text.Encoding]::UTF8.GetBytes(($cuerpo | ConvertTo-Json -Depth 10 -Compress)); $a.ContentType = "application/json" }
    try {
        $r = Invoke-WebRequest @a
        $json = $null; if ($r.Content) { $json = $r.Content | ConvertFrom-Json }
        return [pscustomobject]@{ status = [int]$r.StatusCode; json = $json; texto = $r.Content }
    } catch {
        $resp = $_.Exception.Response
        if ($null -eq $resp) { throw }
        $text = $_.ErrorDetails.Message
        if (-not $text) { try { $text = (New-Object IO.StreamReader($resp.GetResponseStream())).ReadToEnd() } catch { } }
        $json = $null; try { if ($text) { $json = $text | ConvertFrom-Json } } catch { }
        return [pscustomobject]@{ status = [int]$resp.StatusCode; json = $json; texto = $text }
    }
}

# ---- impresora: debe ser de PDF ----
$documentos = [Environment]::GetFolderPath('MyDocuments')
$carpetaPdf = Join-Path $documentos "Casa Grande\Comandas"
$impresora = (Get-CimInstance Win32_Printer -ErrorAction SilentlyContinue | Where-Object { $_.Default } | Select-Object -First 1).Name
if ($impresora -and $impresora -notmatch 'PDF') {
    throw "La impresora predeterminada es '$impresora': esta prueba imprime de verdad. Cambie a Microsoft Print to PDF."
}
Write-Host ("Impresora predeterminada: {0}" -f $(if ($impresora) { $impresora } else { 'ninguna (se guardara PDF)' }))

# SOLO cuentan (y solo se borran) los PDF creados DURANTE esta prueba: la carpeta de Documentos puede tener
# comandas reales con el mismo numero de pedido (la copia de desarrollo y la base de la app instalada comparten
# numeracion), y esas no se tocan nunca.
$inicio = (Get-Date).AddSeconds(-2)

# PDF de un pedido, clasificados: comanda-<id>-<fecha>-<hora>-(platos|bebidas)[-adicion]-<copia>.pdf
function Get-Comandas([int]$idPedido) {
    @(Get-ChildItem $carpetaPdf -Filter "comanda-$idPedido-*.pdf" -ErrorAction SilentlyContinue | Where-Object { $_.LastWriteTime -ge $inicio } | ForEach-Object {
        if ($_.Name -match '^comanda-(\d+)-\d{8}-\d{6}-(platos|bebidas)(-adicion)?-(\d+)\.pdf$') {
            [pscustomobject]@{ grupo = $Matches[2]; adicion = [bool]$Matches[3]; copia = [int]$Matches[4]; nombre = $_.Name }
        }
    })
}
function Conteo([int]$idPedido, [string]$grupo, [bool]$adicion) {
    @(Get-Comandas $idPedido | Where-Object { $_.grupo -eq $grupo -and $_.adicion -eq $adicion }).Count
}
# Espera hasta que el conteo llegue a lo esperado (la impresion va en el hilo de UI despues de responder).
function Esperar-Conteo([int]$idPedido, [string]$grupo, [bool]$adicion, [int]$esperado, [int]$segundos = 30) {
    for ($i = 0; $i -lt ($segundos * 2); $i++) {
        if ((Conteo $idPedido $grupo $adicion) -ge $esperado) { break }
        Start-Sleep -Milliseconds 500
    }
    return (Conteo $idPedido $grupo $adicion)
}

# ---- datos de apoyo ----
[xml]$cfg = Get-Content $exeConfig
$apiKey = ($cfg.configuration.appSettings.add | Where-Object { $_.key -eq 'ApiKey' }).value
$cn = New-Object System.Data.SqlClient.SqlConnection $cs; $cn.Open(); $cmd = $cn.CreateCommand()
$cmd.CommandText = "SELECT TOP 1 Id_empleado, Password FROM Empleados WHERE Estado='ACTIVO' AND Password IS NOT NULL AND Password <> '' AND Id_empleado > 0 ORDER BY Id_empleado"
$r = $cmd.ExecuteReader(); [void]$r.Read(); $idEmp = [int]$r[0]; $passEmp = [string]$r[1]; $r.Close(); $cn.Close()
$platoDet = [int](Sql-Scalar "SELECT TOP 1 Id_plato FROM Platos WHERE Estado='ACTIVO' AND Plato_detallado='ACTIVO' ORDER BY Id_plato")
$bebida = [int](Sql-Scalar "SELECT TOP 1 Id_bebida FROM Bebidas WHERE Estado='ACTIVO' ORDER BY Id_bebida")
function Ing-Id([string]$tipo) { [int](Sql-Scalar "SELECT TOP 1 Id_ingrediente FROM Ingredientes WHERE Tipo_ingrediente='$tipo' AND Nombre_ingrediente NOT LIKE 'SIN%' ORDER BY Id_ingrediente") }
$set1 = @("SOPA", "ARROZ", "ACOMPANANTE", "BEBIDA") | ForEach-Object { Ing-Id $_ }
function Ings($ids) { @($ids | ForEach-Object { @{ id_ingrediente = [int]$_; observaciones = "" } }) }
function LineasDe([int]$idPedido) { [int](Sql-Scalar "SELECT COUNT(*) FROM Detalle_pedido WHERE Id_pedido=$idPedido") }

$antes = "{0}|{1}|{2}" -f (Sql-Scalar "SELECT COUNT(*) FROM Pedidos"), (Sql-Scalar "SELECT COUNT(*) FROM Detalle_pedido"), (Sql-Scalar "SELECT COUNT(*) FROM Detalle_ingredientes_pedido")
$maxAntes = [int](Sql-Scalar "SELECT ISNULL(MAX(Id_pedido), 0) FROM Pedidos")

# Solo se rechaza la instancia de la compilacion local: una instalada (otra ruta) no molesta.
Get-Process SISMistico -ErrorAction SilentlyContinue | Where-Object { $_.Path -eq $exe } |
    ForEach-Object { throw "Ya hay un SISMistico de la compilacion local abierto (PID $($_.Id)); cierralo antes." }
$logDir = Join-Path $buildDir "logs"
if (Test-Path $logDir) { Get-ChildItem $logDir -Filter "api-*.log" | Remove-Item -Force }

$configOriginal = [IO.File]::ReadAllText($exeConfig)
$proc = $null
$idsPrueba = New-Object System.Collections.Generic.List[int]

try {
    # Otro puerto para no chocar con una instancia instalada
    [xml]$nuevo = $configOriginal
    ($nuevo.configuration.appSettings.add | Where-Object { $_.key -eq 'ApiPrefijo' }).value = "http://localhost:$Puerto/api/v1/"
    $nuevo.Save($exeConfig)

    $proc = Start-Process -FilePath $exe -WorkingDirectory $buildDir -PassThru
    $listo = $false
    for ($i = 0; $i -lt 30; $i++) { Start-Sleep -Milliseconds 500; try { if ((Invoke-Api "/ping").status -eq 200) { $listo = $true; break } } catch { } }
    if (-not $listo) { throw "La API no respondio en 15 s (puerto $Puerto)." }

    $k = @{ "X-Api-Key" = $apiKey }
    $l = Invoke-Api "/auth/login" $k "POST" @{ id_empleado = $idEmp; password = $passEmp }
    if ($l.status -ne 200) { throw "No se pudo iniciar sesion de prueba ($($l.status))." }
    $token = $l.json.token
    $h = @{ "X-Api-Key" = $apiKey; "Authorization" = "Bearer $token" }

    $libres = @((Invoke-Api "/mesas" $h).json | Where-Object { $_.estado -eq "DISPONIBLE" } | Sort-Object num_mesa -Descending)
    if ($libres.Count -lt 3) { throw "Hacen falta 3 mesas libres para la prueba." }

    function Nuevo-Pedido($mesa, [bool]$imprimir) {
        $c = @{
            idempotencia = [guid]::NewGuid().ToString(); id_mesa = [int]$mesa.id_mesa; cantidad_clientes = 2; tipo_pedido = "MESA"
            observaciones = "prueba adiciones"; imprimir_comandas = $imprimir; copias_comandas = 1
            items = @(
                @{ tipo = "PLATO"; id_tipo = $platoDet; cantidad = 2; observaciones = "sin cebolla"; ingredientes = (Ings $set1) },
                @{ tipo = "BEBIDA"; id_tipo = $bebida; cantidad = 1 }
            )
        }
        $p = Invoke-Api "/pedidos" $h "POST" $c
        if ($p.status -ne 201) { throw "No se pudo crear el pedido de prueba ($($p.status))." }
        $script:idsPrueba.Add([int]$p.json.id_pedido)
        return $p
    }

    # ===== 1. pedido nuevo CON impresion: sale el pedido completo (1 copia) =====
    $p1 = Nuevo-Pedido $libres[0] $true
    $id = [int]$p1.json.id_pedido
    Add-Result "crear con impresion: impreso=true" "True" ([string]$p1.json.impreso)
    Add-Result "crear: comanda de platos (pedido completo)" 1 (Esperar-Conteo $id "platos" $false 1)
    Add-Result "crear: comanda de bebidas (pedido completo)" 1 (Esperar-Conteo $id "bebidas" $false 1)

    # ===== 2. agregar 1 plato + 1 bebida, imprimir, 2 copias: solo LO NUEVO =====
    $lineasAntes = LineasDe $id
    $ag = Invoke-Api "/pedidos/$id/items" $h "POST" @{
        imprimir_comandas = $true; copias_comandas = 2
        items = @(
            @{ tipo = "BEBIDA"; id_tipo = $bebida; cantidad = 2 },
            @{ tipo = "PLATO"; id_tipo = $platoDet; cantidad = 1; ingredientes = (Ings $set1) }
        )
    }
    Add-Result "adicion con impresion -> 200" 200 $ag.status
    Add-Result "adicion: impreso=true" "True" ([string]$ag.json.impreso)
    Add-Result "adicion: se agregaron 2 lineas" ($lineasAntes + 2) (LineasDe $id)
    Add-Result "adicion: 2 copias de la comanda de platos nueva" 2 (Esperar-Conteo $id "platos" $true 2)
    Add-Result "adicion: 2 copias de la comanda de bebidas nueva" 2 (Esperar-Conteo $id "bebidas" $true 2)
    Start-Sleep -Seconds 3
    Add-Result "adicion: NO se reimprimio el pedido completo (platos)" 1 (Conteo $id "platos" $false)
    Add-Result "adicion: NO se reimprimio el pedido completo (bebidas)" 1 (Conteo $id "bebidas" $false)

    # ===== 3. agregar SOLO una bebida: no sale comanda de platos =====
    $ag = Invoke-Api "/pedidos/$id/items" $h "POST" @{ imprimir_comandas = $true; copias_comandas = 1; items = @(@{ tipo = "BEBIDA"; id_tipo = $bebida; cantidad = 1 }) }
    Add-Result "adicion solo de bebida -> 200 e impreso" "200|True" ("{0}|{1}" -f $ag.status, $ag.json.impreso)
    Add-Result "solo bebida: una comanda de bebidas mas (total 3)" 3 (Esperar-Conteo $id "bebidas" $true 3)
    Start-Sleep -Seconds 3
    Add-Result "solo bebida: no sale comanda de platos nueva (sigue 2)" 2 (Conteo $id "platos" $true)

    # ===== 4. sin imprimir (por defecto y explicito): no sale nada =====
    $totalAntes = @(Get-Comandas $id).Count
    $ag = Invoke-Api "/pedidos/$id/items" $h "POST" @{ items = @(@{ tipo = "BEBIDA"; id_tipo = $bebida; cantidad = 1 }) }
    Add-Result "adicion sin imprimir_comandas -> 200, impreso=false" "200|False" ("{0}|{1}" -f $ag.status, $ag.json.impreso)
    $ag = Invoke-Api "/pedidos/$id/items" $h "POST" @{ imprimir_comandas = $false; copias_comandas = 2; items = @(@{ tipo = "BEBIDA"; id_tipo = $bebida; cantidad = 1 }) }
    Add-Result "adicion con imprimir_comandas=false -> impreso=false" "200|False" ("{0}|{1}" -f $ag.status, $ag.json.impreso)
    Start-Sleep -Seconds 4
    Add-Result "sin imprimir: no aparecio ningun PDF nuevo" $totalAntes (@(Get-Comandas $id).Count)

    # ===== 5. validaciones (no deben agregar nada) =====
    $lineas = LineasDe $id
    Add-Result "copias_comandas 0 -> 400" 400 (Invoke-Api "/pedidos/$id/items" $h "POST" @{ imprimir_comandas = $true; copias_comandas = 0; items = @(@{ tipo = "BEBIDA"; id_tipo = $bebida; cantidad = 1 }) }).status
    Add-Result "copias_comandas 6 -> 400" 400 (Invoke-Api "/pedidos/$id/items" $h "POST" @{ imprimir_comandas = $true; copias_comandas = 6; items = @(@{ tipo = "BEBIDA"; id_tipo = $bebida; cantidad = 1 }) }).status
    Add-Result "imprimir_comandas como texto -> 400" 400 (Invoke-Api "/pedidos/$id/items" $h "POST" @{ imprimir_comandas = "si"; items = @(@{ tipo = "BEBIDA"; id_tipo = $bebida; cantidad = 1 }) }).status
    Add-Result "las validaciones no agregaron lineas" $lineas (LineasDe $id)

    # ===== 6. dos adiciones a la vez a pedidos distintos: ninguna comanda se pierde =====
    $pB = Nuevo-Pedido $libres[1] $false
    $pC = Nuevo-Pedido $libres[2] $false
    $idB = [int]$pB.json.id_pedido; $idC = [int]$pC.json.id_pedido
    $cuerpoJson = (@{ imprimir_comandas = $true; copias_comandas = 1; items = @(@{ tipo = "BEBIDA"; id_tipo = $bebida; cantidad = 1 }) } | ConvertTo-Json -Depth 10 -Compress)
    $trabajos = foreach ($oid in @($idB, $idC)) {
        Start-Job -ScriptBlock {
            param($u, $key, $tok, $body)
            try {
                $r = Invoke-WebRequest -Uri $u -Method POST -Headers @{ "X-Api-Key" = $key; "Authorization" = "Bearer $tok" } `
                    -Body ([Text.Encoding]::UTF8.GetBytes($body)) -ContentType "application/json" -UseBasicParsing
                ($r.Content | ConvertFrom-Json).impreso
            } catch { "ERR" }
        } -ArgumentList "$Base/pedidos/$oid/items", $apiKey, $token, $cuerpoJson
    }
    $respuestas = @($trabajos | Wait-Job | Receive-Job)
    $trabajos | Remove-Job -Force
    Add-Result "adiciones simultaneas: ambas responden impreso=true" "True|True" (($respuestas | ForEach-Object { [string]$_ }) -join "|")
    Add-Result "adiciones simultaneas: sale la comanda del pedido B" 1 (Esperar-Conteo $idB "bebidas" $true 1 40)
    Add-Result "adiciones simultaneas: sale la comanda del pedido C" 1 (Esperar-Conteo $idC "bebidas" $true 1 40)

    # ===== 6b. precuenta: pasar a SALIENDO imprimiendo la precuenta (como el boton del escritorio) =====
    function Get-Precuentas([int]$idPedido) {
        @(Get-ChildItem $carpetaPdf -Filter "precuenta-$idPedido-*.pdf" -ErrorAction SilentlyContinue | Where-Object { $_.LastWriteTime -ge $inicio })
    }
    # valor invalido: 400 y el pedido NO cambia de estado
    $pre = Invoke-Api "/pedidos/$idC/estado" $h "PATCH" @{ estado = "SALIENDO"; imprimir_precuenta = "si" }
    Add-Result "precuenta: imprimir_precuenta como texto -> 400" 400 $pre.status
    Add-Result "precuenta: el 400 no cambio el estado" "PENDIENTE" ([string](Sql-Scalar "SELECT Estado_pedido FROM Pedidos WHERE Id_pedido=$idC"))

    # con impresion: SALIENDO + un PDF de precuenta
    $pre = Invoke-Api "/pedidos/$idB/estado" $h "PATCH" @{ estado = "SALIENDO"; imprimir_precuenta = $true }
    Add-Result "precuenta: SALIENDO con impresion -> 200, SALIENDO e impreso=true" "200|SALIENDO|True" ("{0}|{1}|{2}" -f $pre.status, $pre.json.estado, $pre.json.impreso)
    for ($i = 0; $i -lt 80 -and (Get-Precuentas $idB).Count -lt 1; $i++) { Start-Sleep -Milliseconds 500 }
    $pdfs = Get-Precuentas $idB
    Add-Result "precuenta: se genero 1 PDF de precuenta" 1 $pdfs.Count
    if ($pdfs.Count -ge 1) {
        $bytes = [IO.File]::ReadAllBytes($pdfs[0].FullName)
        Add-Result "precuenta: es un PDF" "%PDF" ([Text.Encoding]::ASCII.GetString($bytes, 0, 4))
        Add-Result "precuenta: el PDF no esta vacio" "True" ($bytes.Length -gt 1000)
    }
    Add-Result "precuenta: en la BD el pedido quedo SALIENDO" "SALIENDO" ([string](Sql-Scalar "SELECT Estado_pedido FROM Pedidos WHERE Id_pedido=$idB"))

    # sin impresion (por defecto): SALIENDO y ningun PDF
    $pre = Invoke-Api "/pedidos/$idC/estado" $h "PATCH" @{ estado = "SALIENDO" }
    Add-Result "precuenta: SALIENDO sin imprimir -> 200, SALIENDO e impreso=false" "200|SALIENDO|False" ("{0}|{1}|{2}" -f $pre.status, $pre.json.estado, $pre.json.impreso)
    Start-Sleep -Seconds 4
    Add-Result "precuenta: sin imprimir no se genero PDF" 0 (Get-Precuentas $idC).Count

    # ===== 7. el registro de la API cuenta las lineas imprimidas =====
    $log = Get-ChildItem $logDir -Filter "api-*.log" -ErrorAction SilentlyContinue | Get-Content -Raw
    Add-Result "registro: la adicion imprimio 2 lineas" "True" ([bool]($log -match 'adicion=2 lineas'))
    Add-Result "registro: la adicion de la bebida sola imprimio 1 linea" "True" ([bool]($log -match 'adicion=1 lineas'))
    Add-Result "registro: la precuenta se guardo con el total del pedido" "True" ([bool]($log -match 'Precuenta en PDF .*id_pedido=\d+ total=\d+'))
}
finally {
    if ($proc -and -not $proc.HasExited) { Stop-Process -Id $proc.Id -Force }
    Start-Sleep -Milliseconds 400
    [IO.File]::WriteAllText($exeConfig, $configOriginal)
    try {
        Sql-Exec "DELETE FROM Detalle_ingredientes_pedido WHERE Id_pedido > $maxAntes; DELETE FROM Detalle_pedido WHERE Id_pedido > $maxAntes; DELETE FROM Pedidos WHERE Id_pedido > $maxAntes;"
    } catch { Write-Host "No se pudo limpiar la copia: $_" -ForegroundColor Red }
    foreach ($oid in $idsPrueba) {
        Get-ChildItem $carpetaPdf -Filter "comanda-$oid-*.pdf" -ErrorAction SilentlyContinue | Where-Object { $_.LastWriteTime -ge $inicio } | Remove-Item -Force
        Get-ChildItem $carpetaPdf -Filter "precuenta-$oid-*.pdf" -ErrorAction SilentlyContinue | Where-Object { $_.LastWriteTime -ge $inicio } | Remove-Item -Force
    }
}

$despues = "{0}|{1}|{2}" -f (Sql-Scalar "SELECT COUNT(*) FROM Pedidos"), (Sql-Scalar "SELECT COUNT(*) FROM Detalle_pedido"), (Sql-Scalar "SELECT COUNT(*) FROM Detalle_ingredientes_pedido")
Add-Result "copia limpia: conteos iguales a los iniciales" $antes $despues
Add-Result "config restaurada" "True" ([bool]([IO.File]::ReadAllText($exeConfig) -eq $configOriginal))
$sobrantes = 0; foreach ($oid in $idsPrueba) { $sobrantes += @(Get-ChildItem $carpetaPdf -Filter "*-$oid-*.pdf" -ErrorAction SilentlyContinue | Where-Object { $_.LastWriteTime -ge $inicio }).Count }
Add-Result "limpieza: no quedan PDF de la prueba" 0 $sobrantes

$results | Format-Table -AutoSize | Out-String -Width 230
$fallan = @($results | Where-Object { $_.ok -ne "OK" }).Count
"{0} OK de {1} ({2} fallan)" -f ($results.Count - $fallan), $results.Count, $fallan
if ($fallan -gt 0) { exit 1 }
