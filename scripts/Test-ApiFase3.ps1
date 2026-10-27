<#
    Prueba de extremo a extremo de la fase 3 de la API movil (POST /pedidos, GET /pedidos/{id}): arranca el
    SISMistico.exe REAL de la compilacion local contra CasaGrandeBD_prod, crea pedidos de verdad en la copia y
    contrasta cada resultado con consultas directas a la BD. Al terminar BORRA todo lo que creo (y restaura el
    ingrediente que inactiva) y verifica que los conteos vuelven a los iniciales.

    Requisitos: .\SISMistico\run-local.ps1 -Rebuild -NoRun, la copia restaurada (db\Restore-ProdCopy.ps1) y el SP
    creado (db\Invoke-Sql.ps1 -File db\01_sp_Movil_Insertar_pedido.sql).

    No prueba la impresion (necesita una impresora real): solo que imprimir_comandas=false/ausente no imprime.

    Uso:  .\Test-ApiFase3.ps1
#>
param(
    [string]$Database = "CasaGrandeBD_prod",
    [string]$Base = "http://localhost:8099/api/v1",
    [int]$Puerto = 0
)

$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.Data
Add-Type -AssemblyName System.Net.Http
$buildDir = Join-Path $PSScriptRoot "..\SISMistico\SISMistico\bin\LocalBuild"
$exe = (Resolve-Path (Join-Path $buildDir "SISMistico.exe")).Path
$apiKey = "cambiar-esta-clave"   # se reemplaza abajo por el ApiKey real de la compilación local
$results = New-Object System.Collections.Generic.List[object]

# La clave de la compilación local (puede ser la de ejemplo o una propia; run-local.ps1 no la cambia).
$exeConfig = Join-Path $buildDir "SISMistico.exe.config"
if (Test-Path $exeConfig) {
    [xml]$cfg = Get-Content $exeConfig
    $clave = ($cfg.configuration.appSettings.add | Where-Object { $_.key -eq 'ApiKey' }).value
    if ($clave) { $apiKey = $clave }
}
$cs = "Server=(localdb)\MSSQLLocalDB;Database=$Database;Integrated Security=true"

function Add-Result([string]$caso, $esperado, $obtenido) {
    $results.Add([pscustomobject]@{ caso = $caso; esperado = "$esperado"; obtenido = "$obtenido"; ok = $(if ("$esperado" -eq "$obtenido") { "OK" } else { "FALLA" }) })
}
function Count-Of($j) { if ($null -eq $j) { 0 } else { @($j).Count } }
function Sql-Scalar([string]$sql) {
    $cn = New-Object System.Data.SqlClient.SqlConnection $cs; $cn.Open()
    $cmd = $cn.CreateCommand(); $cmd.CommandText = $sql; $v = $cmd.ExecuteScalar(); $cn.Close(); return $v
}
function Sql-Exec([string]$sql) {
    $cn = New-Object System.Data.SqlClient.SqlConnection $cs; $cn.Open()
    $cmd = $cn.CreateCommand(); $cmd.CommandText = $sql; [void]$cmd.ExecuteNonQuery(); $cn.Close()
}

function Invoke-Api([string]$ruta, [hashtable]$headers = $null, [string]$metodo = "GET", $cuerpo = $null, [string]$crudo = $null) {
    $a = @{ Uri = "$Base$ruta"; Method = $metodo; UseBasicParsing = $true; TimeoutSec = 30 }
    if ($headers) { $a.Headers = $headers }
    if ($crudo) { $a.Body = [Text.Encoding]::UTF8.GetBytes($crudo); $a.ContentType = "application/json" }
    elseif ($null -ne $cuerpo) { $a.Body = [Text.Encoding]::UTF8.GetBytes(($cuerpo | ConvertTo-Json -Depth 10 -Compress)); $a.ContentType = "application/json" }
    try {
        $r = Invoke-WebRequest @a
        $json = $null; if ($r.Content) { $json = $r.Content | ConvertFrom-Json }
        return [pscustomobject]@{ status = [int]$r.StatusCode; json = $json; texto = $r.Content }
    } catch {
        $resp = $_.Exception.Response
        if ($null -eq $resp) { throw }
        # En PowerShell 5.1 el cuerpo del error ya viene leido en ErrorDetails; el stream queda vacio.
        $text = $_.ErrorDetails.Message
        if (-not $text) { try { $text = (New-Object IO.StreamReader($resp.GetResponseStream())).ReadToEnd() } catch { } }
        $json = $null; try { if ($text) { $json = $text | ConvertFrom-Json } } catch { }
        return [pscustomobject]@{ status = [int]$resp.StatusCode; json = $json; texto = $text }
    }
}

# Dos POST a la vez (misma o distinta clave) para probar la seccion critica de verdad.
function Post-Paralelo([object[]]$cuerpos, [hashtable]$headers) {
    $client = New-Object System.Net.Http.HttpClient
    $tareas = @()
    foreach ($c in $cuerpos) {
        $m = New-Object System.Net.Http.HttpRequestMessage ([System.Net.Http.HttpMethod]::Post), "$Base/pedidos"
        foreach ($k in $headers.Keys) { [void]$m.Headers.TryAddWithoutValidation($k, $headers[$k]) }
        $m.Content = New-Object System.Net.Http.StringContent (($c | ConvertTo-Json -Depth 10 -Compress)), ([Text.Encoding]::UTF8), "application/json"
        $tareas += $client.SendAsync($m)
    }
    [void][Threading.Tasks.Task]::WaitAll($tareas)
    $out = foreach ($t in $tareas) {
        $texto = $t.Result.Content.ReadAsStringAsync().Result
        [pscustomobject]@{ status = [int]$t.Result.StatusCode; json = ($texto | ConvertFrom-Json) }
    }
    $client.Dispose()
    return $out
}

# ---- credenciales y datos de apoyo (de la copia local) ----
$cn = New-Object System.Data.SqlClient.SqlConnection $cs; $cn.Open(); $cmd = $cn.CreateCommand()
$cmd.CommandText = "SELECT TOP 1 Id_empleado, Password FROM Empleados WHERE Estado='ACTIVO' AND Password IS NOT NULL AND Password <> '' AND Id_empleado > 0 ORDER BY Id_empleado"
$r = $cmd.ExecuteReader(); [void]$r.Read(); $idEmp = [int]$r[0]; $passEmp = [string]$r[1]; $r.Close(); $cn.Close()

$platoDet = [int](Sql-Scalar "SELECT TOP 1 Id_plato FROM Platos WHERE Estado='ACTIVO' AND Plato_detallado='ACTIVO' ORDER BY Id_plato")
$platoSimple = [int](Sql-Scalar "SELECT TOP 1 Id_plato FROM Platos WHERE Estado='ACTIVO' AND Plato_detallado<>'ACTIVO' ORDER BY Id_plato")
$bebida = [int](Sql-Scalar "SELECT TOP 1 Id_bebida FROM Bebidas WHERE Estado='ACTIVO' ORDER BY Id_bebida")
$platoInact = [int](Sql-Scalar "SELECT TOP 1 Id_plato FROM Platos WHERE Estado<>'ACTIVO' ORDER BY Id_plato")
$bebidaInact = [int](Sql-Scalar "SELECT TOP 1 Id_bebida FROM Bebidas WHERE Estado<>'ACTIVO' ORDER BY Id_bebida")
$pDet = [decimal](Sql-Scalar "SELECT Precio_plato FROM Platos WHERE Id_plato=$platoDet")
$pSimple = [decimal](Sql-Scalar "SELECT Precio_plato FROM Platos WHERE Id_plato=$platoSimple")
$pBeb = [decimal](Sql-Scalar "SELECT Precio_bebida FROM Bebidas WHERE Id_bebida=$bebida")

# Ingredientes: un juego "normal" y otro con las opciones SIN ... por tipo.
function Ing-Id([string]$tipo, [string]$patron) { [int](Sql-Scalar "SELECT TOP 1 Id_ingrediente FROM Ingredientes WHERE Tipo_ingrediente='$tipo' AND Nombre_ingrediente $patron ORDER BY Id_ingrediente") }
$set1 = @("SOPA", "ARROZ", "ACOMPANANTE", "BEBIDA") | ForEach-Object { Ing-Id $_ "NOT LIKE 'SIN%'" }
$set2 = @("SOPA", "ARROZ", "ACOMPANANTE", "BEBIDA") | ForEach-Object { Ing-Id $_ "LIKE 'SIN%'" }
function Ings($ids) { @($ids | ForEach-Object { @{ id_ingrediente = [int]$_; observaciones = "" } }) }

# Estado inicial para comprobar que no queda nada
$antes = "{0}|{1}|{2}" -f (Sql-Scalar "SELECT COUNT(*) FROM Pedidos"), (Sql-Scalar "SELECT COUNT(*) FROM Detalle_pedido"), (Sql-Scalar "SELECT COUNT(*) FROM Detalle_ingredientes_pedido")
$maxAntes = [int](Sql-Scalar "SELECT MAX(Id_pedido) FROM Pedidos")
$ingParaInactivar = Ing-Id "SOPA" "LIKE 'CREMA%'"
$estadoIngOriginal = [string](Sql-Scalar "SELECT Estado_ingrediente FROM Ingredientes WHERE Id_ingrediente=$ingParaInactivar")

# Solo molesta una instancia de LA COMPILACION LOCAL (una instalada, con otra ruta, no).
Get-Process SISMistico -ErrorAction SilentlyContinue | Where-Object { $_.Path -eq $exe } | ForEach-Object { throw "Ya hay un SISMistico de la compilacion local abierto (PID $($_.Id)); cierralo antes." }
# Con -Puerto la prueba usa otro puerto (por ejemplo si hay una instancia instalada escuchando en el 8099);
# el .exe.config de la compilacion local se cambia solo durante la prueba y se restaura al terminar.
$configOriginal = $null
if ($Puerto -gt 0) {
    $Base = "http://localhost:$Puerto/api/v1"
    $configOriginal = [IO.File]::ReadAllText($exeConfig)
    [xml]$cfgTmp = $configOriginal
    ($cfgTmp.configuration.appSettings.add | Where-Object { $_.key -eq 'ApiPrefijo' }).value = "http://localhost:$Puerto/api/v1/"
    $cfgTmp.Save($exeConfig)
}
$logDir = Join-Path $buildDir "logs"
if (Test-Path $logDir) { Get-ChildItem $logDir -Filter "api-*.log" | Remove-Item -Force }
$proc = $null
try {
    $proc = Start-Process -FilePath $exe -WorkingDirectory $buildDir -PassThru
    $listo = $false
    for ($i = 0; $i -lt 30; $i++) { Start-Sleep -Milliseconds 500; try { if ((Invoke-Api "/ping").status -eq 200) { $listo = $true; break } } catch { } }
    if (-not $listo) { throw "La API no respondio en 15 s." }

    $k = @{ "X-Api-Key" = $apiKey }
    $l = Invoke-Api "/auth/login" $k "POST" @{ id_empleado = $idEmp; password = $passEmp }
    if ($l.status -ne 200) { throw "No se pudo iniciar sesion de prueba ($($l.status))." }
    $h = @{ "X-Api-Key" = $apiKey; "Authorization" = "Bearer $($l.json.token)" }

    # mesas libres hoy (se toman de la propia API)
    $libres = @((Invoke-Api "/mesas" $h).json | Where-Object { $_.estado -eq "DISPONIBLE" } | Sort-Object num_mesa -Descending)
    if ($libres.Count -lt 6) { throw "Hacen falta 6 mesas libres y hay $($libres.Count)." }
    $mA, $mC, $mD, $mE, $mF = $libres[0].id_mesa, $libres[1].id_mesa, $libres[2].id_mesa, $libres[3].id_mesa, $libres[4].id_mesa
    $numA = $libres[0].num_mesa

    function Nuevo-Cuerpo([int]$mesa, [string]$clave, $items) {
        @{ idempotencia = $clave; id_mesa = $mesa; id_cliente = 0; cantidad_clientes = 2; tipo_pedido = "MESA"; observaciones = "prueba api"; items = $items }
    }
    function Item-Det([int]$cant, $ids, [string]$obs = "") { @{ tipo = "PLATO"; id_tipo = $platoDet; cantidad = $cant; observaciones = $obs; ingredientes = (Ings $ids) } }
    $itemSimple = @{ tipo = "PLATO"; id_tipo = $platoSimple; cantidad = 1 }
    $itemBeb = @{ tipo = "BEBIDA"; id_tipo = $bebida; cantidad = 3 }
    function Guid-Nuevo { [guid]::NewGuid().ToString() }

    # ===== autenticacion =====
    Add-Result "POST /pedidos sin token -> 401" 401 (Invoke-Api "/pedidos" $k "POST" (Nuevo-Cuerpo $mA (Guid-Nuevo) @($itemBeb))).status
    Add-Result "GET /pedidos/1 sin token -> 401" 401 (Invoke-Api "/pedidos/1" $k).status

    # ===== alta correcta =====
    $g1 = Guid-Nuevo
    $itemsOk = @((Item-Det 2 $set1 "sin cebolla"), $itemSimple, @{ tipo = "BEBIDA"; id_tipo = $bebida; cantidad = 3; observaciones = "x'; DROP TABLE Pedidos;--"; precio = 1 })
    $cuerpoOk = Nuevo-Cuerpo $mA $g1 $itemsOk
    $cuerpoOk.imprimir_comandas = $false
    $p = Invoke-Api "/pedidos" $h "POST" $cuerpoOk
    $idP = [int]$p.json.id_pedido
    $totalEsp = 2 * $pDet + $pSimple + 3 * $pBeb
    Add-Result "alta: 201" 201 $p.status
    Add-Result "alta: estado PENDIENTE, empleado del token, mesa, MESA" "PENDIENTE|$idEmp|$mA|MESA" ("{0}|{1}|{2}|{3}" -f $p.json.estado, $p.json.id_empleado, $p.json.id_mesa, $p.json.tipo_pedido)
    Add-Result "alta: fecha de hoy y 2 clientes" ("{0:yyyy-MM-dd}|2" -f (Get-Date)) ("{0}|{1}" -f $p.json.fecha, $p.json.cantidad_clientes)
    Add-Result "alta: 3 filas" 3 (Count-Of $p.json.items)
    Add-Result "alta: total = suma con precios del CATALOGO" ([long]$totalEsp) ([long]$p.json.total)
    Add-Result "alta: el precio enviado por el cliente (1) se ignora" ([long]$pBeb) ([long]$p.json.items[2].precio)
    Add-Result "alta: plato detallado con sus 4 ingredientes" (($set1 | Sort-Object) -join ",") ((@($p.json.items[0].ingredientes | ForEach-Object { $_.id_ingrediente }) | Sort-Object) -join ",")
    Add-Result "alta: texto con comilla y DROP guardado literal" "x'; DROP TABLE Pedidos;--" $p.json.items[2].observaciones
    Add-Result "alta: impreso=false (no se pidio imprimir)" "False" $p.json.impreso
    Add-Result "BD: cabecera" "PENDIENTE|MESA|2|$idEmp" (Sql-Scalar "SELECT CONCAT(Estado_pedido,'|',Tipo_pedido,'|',CantidadClientes,'|',Id_empleado) FROM Pedidos WHERE Id_pedido=$idP")
    Add-Result "BD: 3 lineas con precio de catalogo" "3|1" ("{0}|{1}" -f (Sql-Scalar "SELECT COUNT(*) FROM Detalle_pedido WHERE Id_pedido=$idP"), (Sql-Scalar "SELECT COUNT(*) FROM Detalle_pedido d JOIN Platos p ON p.Id_plato=d.Id_tipo AND d.Tipo='PLATO' WHERE d.Id_pedido=$idP AND d.Id_tipo=$platoDet AND d.Precio=p.Precio_plato AND d.Cantidad=2"))
    Add-Result "BD: 4 ingredientes, todos con la linea del plato" "4|0" ("{0}|{1}" -f (Sql-Scalar "SELECT COUNT(*) FROM Detalle_ingredientes_pedido i JOIN Detalle_pedido d ON d.Id_detalle_pedido=i.Id_detalle_pedido WHERE i.Id_pedido=$idP AND d.Id_tipo=$platoDet"), (Sql-Scalar "SELECT COUNT(*) FROM Detalle_ingredientes_pedido WHERE Id_pedido=$idP AND Id_detalle_pedido IS NULL"))

    # GET /pedidos/{id} igual a lo que devolvio el alta
    $g = Invoke-Api "/pedidos/$idP" $h
    $a = $p.json | Select-Object * -ExcludeProperty impreso | ConvertTo-Json -Depth 10 -Compress
    $b = $g.json | ConvertTo-Json -Depth 10 -Compress
    Add-Result "GET /pedidos/{id}: 200 e identico al alta" "200|True" ("{0}|{1}" -f $g.status, ($a -eq $b))
    Add-Result "GET /pedidos/{inexistente} -> 404" 404 (Invoke-Api "/pedidos/2147000000" $h).status
    Add-Result "GET /pedidos/abc -> 400" 400 (Invoke-Api "/pedidos/abc" $h).status

    # ===== idempotencia =====
    $cuentaAntes = [int](Sql-Scalar "SELECT COUNT(*) FROM Pedidos")
    $rep = Invoke-Api "/pedidos" $h "POST" $cuerpoOk
    Add-Result "reintento con la misma clave: 200 y el MISMO pedido" "200|$idP" ("{0}|{1}" -f $rep.status, $rep.json.id_pedido)
    Add-Result "reintento: no se crea otro pedido" $cuentaAntes (Sql-Scalar "SELECT COUNT(*) FROM Pedidos")
    $otro = Nuevo-Cuerpo $mA $g1 @(@{ tipo = "BEBIDA"; id_tipo = $bebida; cantidad = 5 })
    Add-Result "misma clave con cuerpo distinto -> 409" "409|CONFLICTO" (& { $x = Invoke-Api "/pedidos" $h "POST" $otro; "{0}|{1}" -f $x.status, $x.json.error.codigo })

    # ===== mesa ocupada =====
    $oc = Invoke-Api "/pedidos" $h "POST" (Nuevo-Cuerpo $mA (Guid-Nuevo) @($itemBeb))
    Add-Result "mesa con pedido abierto hoy -> 409 MESA_OCUPADA" "409|MESA_OCUPADA" ("{0}|{1}" -f $oc.status, $oc.json.error.codigo)
    Add-Result "MESA_OCUPADA nombra la mesa y el pedido" "True" (($oc.json.error.mensaje -match "mesa $numA") -and ($oc.json.error.mensaje -match "#$idP"))
    Add-Result "Id_mesa 5200 (duplicado de la 21) -> 404" 404 (Invoke-Api "/pedidos" $h "POST" (Nuevo-Cuerpo 5200 (Guid-Nuevo) @($itemBeb))).status
    $km = (Invoke-Api "/mesas" $h).json | Where-Object { $_.id_mesa -eq $mA }
    Add-Result "el mapa de mesas ve el pedido nuevo" "PENDIENTE|$idP|$([long]$totalEsp)" ("{0}|{1}|{2}" -f $km.estado, $km.id_pedido, [long]$km.total)

    # ===== concurrencia real =====
    $gc = Guid-Nuevo
    $par = Post-Paralelo @((Nuevo-Cuerpo $mC $gc @($itemBeb)), (Nuevo-Cuerpo $mC $gc @($itemBeb))) $h
    Add-Result "doble toque simultaneo, misma clave: un 201 y un 200" "200,201" ((@($par | ForEach-Object { $_.status }) | Sort-Object) -join ",")
    Add-Result "doble toque: mismo id_pedido en ambas respuestas" 1 (@($par | ForEach-Object { $_.json.id_pedido } | Sort-Object -Unique).Count)
    Add-Result "doble toque: UN solo pedido en la BD para esa mesa" 1 (Sql-Scalar "SELECT COUNT(*) FROM Pedidos WHERE Id_mesa=$mC AND Id_pedido > $maxAntes")
    $par2 = Post-Paralelo @((Nuevo-Cuerpo $mE (Guid-Nuevo) @($itemBeb)), (Nuevo-Cuerpo $mE (Guid-Nuevo) @($itemBeb))) $h
    Add-Result "misma mesa libre, dos claves distintas a la vez: un 201 y un 409" "201,409" ((@($par2 | ForEach-Object { $_.status }) | Sort-Object) -join ",")
    Add-Result "misma mesa a la vez: UN solo pedido en la BD" 1 (Sql-Scalar "SELECT COUNT(*) FROM Pedidos WHERE Id_mesa=$mE AND Id_pedido > $maxAntes")

    # ===== varias filas del mismo plato con ingredientes distintos (como el escritorio) =====
    $d = Invoke-Api "/pedidos" $h "POST" (Nuevo-Cuerpo $mD (Guid-Nuevo) @((Item-Det 1 $set1), (Item-Det 1 $set2), $itemBeb))
    $idD = [int]$d.json.id_pedido
    Add-Result "dos filas del mismo plato con ingredientes distintos: 201" "201|3" ("{0}|{1}" -f $d.status, (Count-Of $d.json.items))
    Add-Result "cada fila conserva SU juego (normal / SIN ...)" "$(($set1 | Sort-Object) -join ',')|$(($set2 | Sort-Object) -join ',')" `
        ("{0}|{1}" -f ((@($d.json.items[0].ingredientes | ForEach-Object { $_.id_ingrediente }) | Sort-Object) -join ","), ((@($d.json.items[1].ingredientes | ForEach-Object { $_.id_ingrediente }) | Sort-Object) -join ","))
    Add-Result "BD: dos filas del mismo plato, cada una con 4 ingredientes propios" "2|4,4" ("{0}|{1}" -f (Sql-Scalar "SELECT COUNT(*) FROM Detalle_pedido WHERE Id_pedido=$idD AND Id_tipo=$platoDet"), (Sql-Scalar "SELECT STRING_AGG(n,',') FROM (SELECT COUNT(*) AS n FROM Detalle_ingredientes_pedido WHERE Id_pedido=$idD GROUP BY Id_detalle_pedido) x"))
    Add-Result "sin imprimir_comandas, impreso=false" "False" $d.json.impreso

    # ===== validacion: nada de esto debe crear filas =====
    $cuentaVal = "{0}|{1}|{2}" -f (Sql-Scalar "SELECT COUNT(*) FROM Pedidos"), (Sql-Scalar "SELECT COUNT(*) FROM Detalle_pedido"), (Sql-Scalar "SELECT COUNT(*) FROM Detalle_ingredientes_pedido")
    $itemsBase = @((Item-Det 1 $set1))
    function Probar([string]$caso, [int]$esperado, $cuerpo = $null, [string]$crudo = $null, [string]$codigo = $null) {
        $x = if ($crudo) { Invoke-Api "/pedidos" $h "POST" $null $crudo } else { Invoke-Api "/pedidos" $h "POST" $cuerpo }
        $obt = "$($x.status)"; $esp = "$esperado"
        if ($codigo) { $esp = "$esperado|$codigo"; $obt = "$($x.status)|$($x.json.error.codigo)" }
        Add-Result $caso $esp $obt
    }
    function Variante($mutar) { $c = Nuevo-Cuerpo $mF (Guid-Nuevo) $itemsBase; & $mutar $c; $c }

    # 400
    Probar "400 sin idempotencia" 400 (Variante { param($c) $c.Remove("idempotencia") }) -codigo VALIDACION
    Probar "400 idempotencia corta (abc)" 400 (Variante { param($c) $c.idempotencia = "abc" })
    Probar "400 idempotencia con espacios y simbolos" 400 (Variante { param($c) $c.idempotencia = "clave con espacios `$%" })
    Probar "400 sin id_mesa" 400 (Variante { param($c) $c.Remove("id_mesa") })
    Probar "400 id_mesa = 0" 400 (Variante { param($c) $c.id_mesa = 0 })
    Probar "400 id_mesa como texto" 400 (Variante { param($c) $c.id_mesa = "$mF" })
    Probar "400 sin tipo_pedido" 400 (Variante { param($c) $c.Remove("tipo_pedido") })
    Probar "400 items ausente" 400 (Variante { param($c) $c.Remove("items") })
    Probar "400 items vacio" 400 (Variante { param($c) $c.items = @() })
    Probar "400 201 items (maximo 200)" 400 (Variante { param($c) $c.items = @(1..201 | ForEach-Object { @{ tipo = "BEBIDA"; id_tipo = $bebida; cantidad = 1 } }) })
    Probar "400 cantidad 0" 400 (Variante { param($c) $c.items = @(@{ tipo = "BEBIDA"; id_tipo = $bebida; cantidad = 0 }) })
    Probar "400 cantidad 1000" 400 (Variante { param($c) $c.items = @(@{ tipo = "BEBIDA"; id_tipo = $bebida; cantidad = 1000 }) })
    Probar "400 tipo POSTRE" 400 (Variante { param($c) $c.items = @(@{ tipo = "POSTRE"; id_tipo = $bebida; cantidad = 1 }) })
    Probar "400 id_tipo = 0" 400 (Variante { param($c) $c.items = @(@{ tipo = "BEBIDA"; id_tipo = 0; cantidad = 1 }) })
    Probar "400 observaciones de 201 caracteres" 400 (Variante { param($c) $c.observaciones = ("x" * 201) })
    Probar "400 cantidad_clientes negativa" 400 (Variante { param($c) $c.cantidad_clientes = -1 })
    Probar "400 imprimir_comandas como texto" 400 (Variante { param($c) $c.imprimir_comandas = "si" })
    Probar "400 ingredientes que no es lista" 400 (Variante { param($c) $c.items = @(@{ tipo = "PLATO"; id_tipo = $platoDet; cantidad = 1; ingredientes = "x" }) })
    Probar "400 ingrediente sin id_ingrediente" 400 (Variante { param($c) $c.items = @(@{ tipo = "PLATO"; id_tipo = $platoDet; cantidad = 1; ingredientes = @(@{ observaciones = "x" }) }) })
    Probar "400 cuerpo que no es JSON" 400 -crudo "{esto no es json"
    Probar "400 cuerpo que es un array" 400 -crudo "[1,2,3]"
    # 404
    Probar "404 mesa inexistente" 404 (Variante { param($c) $c.id_mesa = 999999 }) -codigo NO_ENCONTRADO
    Probar "404 cliente inexistente" 404 (Variante { param($c) $c.id_cliente = 2147000000 })
    Probar "404 plato inexistente" 404 (Variante { param($c) $c.items = @(@{ tipo = "PLATO"; id_tipo = 2147000000; cantidad = 1 }) })
    Probar "404 bebida inexistente" 404 (Variante { param($c) $c.items = @(@{ tipo = "BEBIDA"; id_tipo = 2147000000; cantidad = 1 }) })
    Probar "404 ingrediente inexistente" 404 (Variante { param($c) $c.items = @(@{ tipo = "PLATO"; id_tipo = $platoDet; cantidad = 1; ingredientes = @((Ings ($set1[0..2])) + @{ id_ingrediente = 2147000000 }) }) })
    # 422
    Probar "422 DOMICILIO" 422 (Variante { param($c) $c.tipo_pedido = "DOMICILIO" }) -codigo REGLA_NEGOCIO
    Probar "422 plato inactivo" 422 (Variante { param($c) $c.items = @(@{ tipo = "PLATO"; id_tipo = $platoInact; cantidad = 1 }) })
    Probar "422 bebida inactiva" 422 (Variante { param($c) $c.items = @(@{ tipo = "BEBIDA"; id_tipo = $bebidaInact; cantidad = 1 }) })
    Probar "422 plato detallado SIN ingredientes" 422 (Variante { param($c) $c.items = @(@{ tipo = "PLATO"; id_tipo = $platoDet; cantidad = 1 }) })
    Probar "422 plato detallado con lista vacia" 422 (Variante { param($c) $c.items = @(@{ tipo = "PLATO"; id_tipo = $platoDet; cantidad = 1; ingredientes = @() }) })
    Probar "422 plato detallado con 3 tipos (falta BEBIDA)" 422 (Variante { param($c) $c.items = @(@{ tipo = "PLATO"; id_tipo = $platoDet; cantidad = 1; ingredientes = (Ings ($set1[0..2])) }) })
    Probar "422 plato detallado con 2 SOPA (repite tipo)" 422 (Variante { param($c) $c.items = @(@{ tipo = "PLATO"; id_tipo = $platoDet; cantidad = 1; ingredientes = (Ings (@($set1) + @($set2[0]))) }) })
    Probar "422 ingredientes en plato NO detallado" 422 (Variante { param($c) $c.items = @(@{ tipo = "PLATO"; id_tipo = $platoSimple; cantidad = 1; ingredientes = (Ings $set1) }) })
    Probar "422 ingredientes en una bebida" 422 (Variante { param($c) $c.items = @(@{ tipo = "BEBIDA"; id_tipo = $bebida; cantidad = 1; ingredientes = (Ings $set1) }) })
    # mensajes utiles
    $sinSopa = Invoke-Api "/pedidos" $h "POST" (Variante { param($c) $c.items = @(@{ tipo = "PLATO"; id_tipo = $platoDet; cantidad = 1; ingredientes = (Ings ($set1[1..3])) }) })
    Add-Result "el 422 dice que falta la SOPA" "True" ($sinSopa.json.error.mensaje -match "SOPA")
    # ingrediente inactivo (se restaura en el finally)
    Sql-Exec "UPDATE Ingredientes SET Estado_ingrediente='INACTIVO' WHERE Id_ingrediente=$ingParaInactivar"
    $setInact = @($ingParaInactivar) + @($set1[1..3])
    Probar "422 ingrediente INACTIVO" 422 (Variante { param($c) $c.items = @(@{ tipo = "PLATO"; id_tipo = $platoDet; cantidad = 1; ingredientes = (Ings $setInact) }) })
    Sql-Exec "UPDATE Ingredientes SET Estado_ingrediente='$estadoIngOriginal' WHERE Id_ingrediente=$ingParaInactivar"

    $cuentaVal2 = "{0}|{1}|{2}" -f (Sql-Scalar "SELECT COUNT(*) FROM Pedidos"), (Sql-Scalar "SELECT COUNT(*) FROM Detalle_pedido"), (Sql-Scalar "SELECT COUNT(*) FROM Detalle_ingredientes_pedido")
    Add-Result "todas las peticiones invalidas juntas no escribieron NADA (Pedidos|Detalle|Ingredientes)" $cuentaVal $cuentaVal2

    # ===== la API sigue viva tras los efectos de UI =====
    Add-Result "tras todo, /ping responde" 200 (Invoke-Api "/ping").status
}
finally {
    if ($proc -and -not $proc.HasExited) { Stop-Process -Id $proc.Id -Force }
    if ($configOriginal) { Start-Sleep -Milliseconds 300; [IO.File]::WriteAllText($exeConfig, $configOriginal) }
    Start-Sleep -Milliseconds 300
    try { Sql-Exec "UPDATE Ingredientes SET Estado_ingrediente='$estadoIngOriginal' WHERE Id_ingrediente=$ingParaInactivar" } catch { Write-Host "No se pudo restaurar el ingrediente: $_" -ForegroundColor Red }
    try {
        Sql-Exec "DELETE FROM Detalle_ingredientes_pedido WHERE Id_pedido > $maxAntes; DELETE FROM Detalle_pedido WHERE Id_pedido > $maxAntes; DELETE FROM Pedidos WHERE Id_pedido > $maxAntes;"
    } catch { Write-Host "No se pudo limpiar la copia: $_" -ForegroundColor Red }
}

$despues = "{0}|{1}|{2}" -f (Sql-Scalar "SELECT COUNT(*) FROM Pedidos"), (Sql-Scalar "SELECT COUNT(*) FROM Detalle_pedido"), (Sql-Scalar "SELECT COUNT(*) FROM Detalle_ingredientes_pedido")
Add-Result "copia limpia: conteos iguales a los iniciales" $antes $despues
Add-Result "copia limpia: ingrediente restaurado" $estadoIngOriginal (Sql-Scalar "SELECT Estado_ingrediente FROM Ingredientes WHERE Id_ingrediente=$ingParaInactivar")

# ---- el log: sin cuerpos, sin observaciones, sin claves de idempotencia ----
$logs = Get-ChildItem $logDir -Filter "api-*.log" -ErrorAction SilentlyContinue
$texto = if ($logs) { ($logs | Get-Content -Raw -Encoding UTF8) } else { "" }
Add-Result "log: registra id_pedido de los altos" "True" ($texto -match "POST /pedidos \| 201 .*id_pedido=\d+ mesa=\d+ items=\d+")
Add-Result "log: marca el reintento idempotente" "True" ($texto -match "id_pedido=\d+ \(repetido\)")
Add-Result "log: no contiene observaciones ni SQL del cliente" "False" ($texto.Contains("sin cebolla") -or $texto.Contains("DROP TABLE"))
Add-Result "log: no contiene la clave de idempotencia" "False" ($texto.Contains($g1))

$results | Format-Table -AutoSize | Out-String -Width 230
$fallan = @($results | Where-Object { $_.ok -ne "OK" }).Count
"{0} OK de {1} ({2} fallan)" -f ($results.Count - $fallan), $results.Count, $fallan
if ($fallan -gt 0) { exit 1 }
