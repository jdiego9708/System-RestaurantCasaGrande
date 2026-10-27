<#
    Prueba de extremo a extremo de la fase 4 de la API movil (listado y operaciones sobre un pedido):
    arranca el SISMistico.exe REAL de la compilacion local contra CasaGrandeBD_prod, crea un pedido de verdad
    y lo opera (agregar, quitar, SALIENDO, cancelar, imprimir). Al terminar BORRA todo lo que creo y verifica
    que los conteos vuelven a los iniciales.

    La impresion es REAL: se comporta como el escritorio e imprime en la impresora predeterminada. Si esa
    impresora es de PDF (Microsoft Print to PDF, Adobe PDF...), la comanda se guarda sola en
    Documentos\Casa Grande\Comandas sin abrir el dialogo de "Guardar como"; la prueba comprueba el archivo y lo
    borra. En un equipo con impresora fisica, la prueba IMPRIME de verdad (una copia de platos y otra de bebidas).

    Requisitos: .\SISMistico\run-local.ps1 -Rebuild -NoRun, la copia restaurada (db\Restore-ProdCopy.ps1) y los
    SP nuevos (db\03_sp_Movil_Agregar_items.sql y db\04_sp_Movil_Quitar_item.sql).

    Uso:  .\Test-ApiFase4.ps1
#>
param(
    [string]$Database = "CasaGrandeBD_prod",
    [string]$Base = "http://localhost:8099/api/v1",
    [int]$Puerto = 0
)

$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.Data
$buildDir = Join-Path $PSScriptRoot "..\SISMistico\SISMistico\bin\LocalBuild"
$exe = (Resolve-Path (Join-Path $buildDir "SISMistico.exe")).Path
$exeConfig = "$exe.config"
$cs = "Server=(localdb)\MSSQLLocalDB;Database=$Database;Integrated Security=true"
$results = New-Object System.Collections.Generic.List[object]

# La clave de la compilación local.
[xml]$cfg = Get-Content $exeConfig
$apiKey = ($cfg.configuration.appSettings.add | Where-Object { $_.key -eq 'ApiKey' }).value
if (-not $apiKey) { throw "No se encontro ApiKey en $exeConfig" }

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

# ---- datos de apoyo ----
$cn = New-Object System.Data.SqlClient.SqlConnection $cs; $cn.Open(); $cmd = $cn.CreateCommand()
$cmd.CommandText = "SELECT TOP 1 Id_empleado, Password FROM Empleados WHERE Estado='ACTIVO' AND Password IS NOT NULL AND Password <> '' AND Id_empleado > 0 ORDER BY Id_empleado"
$r = $cmd.ExecuteReader(); [void]$r.Read(); $idEmp = [int]$r[0]; $passEmp = [string]$r[1]; $r.Close(); $cn.Close()

$platoDet = [int](Sql-Scalar "SELECT TOP 1 Id_plato FROM Platos WHERE Estado='ACTIVO' AND Plato_detallado='ACTIVO' ORDER BY Id_plato")
$bebida = [int](Sql-Scalar "SELECT TOP 1 Id_bebida FROM Bebidas WHERE Estado='ACTIVO' ORDER BY Id_bebida")
$platoInexistente = [int](Sql-Scalar "SELECT ISNULL(MAX(Id_plato), 0) + 500 FROM Platos")
$pDet = [decimal](Sql-Scalar "SELECT Precio_plato FROM Platos WHERE Id_plato=$platoDet")
$pBeb = [decimal](Sql-Scalar "SELECT Precio_bebida FROM Bebidas WHERE Id_bebida=$bebida")
$pinOk = [int](Sql-Scalar "SELECT TOP 1 Codigo_maestro FROM Empleados WHERE Id_empleado>0 AND Codigo_maestro>0 AND Estado='ACTIVO' ORDER BY Id_empleado")
$pinMalo = [int](Sql-Scalar "SELECT ISNULL(MAX(Codigo_maestro), 0) + 4021 FROM Empleados")

function Ing-Id([string]$tipo, [string]$patron) { [int](Sql-Scalar "SELECT TOP 1 Id_ingrediente FROM Ingredientes WHERE Tipo_ingrediente='$tipo' AND Nombre_ingrediente $patron ORDER BY Id_ingrediente") }
$set1 = @("SOPA", "ARROZ", "ACOMPANANTE", "BEBIDA") | ForEach-Object { Ing-Id $_ "NOT LIKE 'SIN%'" }
function Ings($ids) { @($ids | ForEach-Object { @{ id_ingrediente = [int]$_; observaciones = "" } }) }

$antes = "{0}|{1}|{2}" -f (Sql-Scalar "SELECT COUNT(*) FROM Pedidos"), (Sql-Scalar "SELECT COUNT(*) FROM Detalle_pedido"), (Sql-Scalar "SELECT COUNT(*) FROM Detalle_ingredientes_pedido")
$maxAntes = [int](Sql-Scalar "SELECT ISNULL(MAX(Id_pedido), 0) FROM Pedidos")

# Impresión real: se detecta si la impresora predeterminada genera PDF para comprobar el archivo.
$documentos = [Environment]::GetFolderPath('MyDocuments')
$carpetaPdf = Join-Path $documentos "Casa Grande\Comandas"
$impresora = (Get-CimInstance Win32_Printer -ErrorAction SilentlyContinue | Where-Object { $_.Default } | Select-Object -First 1).Name
$esPdf = (-not $impresora) -or ($impresora -match 'PDF')
# Solo cuentan y se borran los PDF creados durante esta prueba (puede haber comandas reales con el mismo numero).
$inicio = (Get-Date).AddSeconds(-2)
Write-Host ("Impresora predeterminada: {0} ({1})" -f $(if ($impresora) { $impresora } else { 'ninguna' }), $(if ($esPdf) { 'se guardara el PDF en Documentos' } else { 'se imprimira de verdad' }))

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

    # mesa libre de hoy
    $libre = @((Invoke-Api "/mesas" $h).json | Where-Object { $_.estado -eq "DISPONIBLE" } | Sort-Object num_mesa -Descending)[0]
    if (-not $libre) { throw "No hay mesas libres para la prueba." }
    $mesa = [int]$libre.id_mesa
    $numMesa = [int]$libre.num_mesa

    # ===== 1. crear el pedido de trabajo =====
    $cuerpo = @{
        idempotencia = [guid]::NewGuid().ToString(); id_mesa = $mesa; id_cliente = 0; cantidad_clientes = 2
        tipo_pedido = "MESA"; observaciones = "prueba fase 4"
        items = @(
            @{ tipo = "PLATO"; id_tipo = $platoDet; cantidad = 2; observaciones = "sin cebolla"; ingredientes = (Ings $set1) },
            @{ tipo = "BEBIDA"; id_tipo = $bebida; cantidad = 1 }
        )
    }
    $p = Invoke-Api "/pedidos" $h "POST" $cuerpo
    Add-Result "crear pedido de trabajo -> 201" 201 $p.status
    $id = [int]$p.json.id_pedido

    # ===== 2. GET /pedidos =====
    $lista = Invoke-Api "/pedidos" $h
    Add-Result "listado -> 200" 200 $lista.status
    Add-Result "listado trae el pedido creado" "True" ([bool](@($lista.json.pedidos | Where-Object { $_.id_pedido -eq $id }).Count))
    $uno = @($lista.json.pedidos | Where-Object { $_.id_pedido -eq $id })[0]
    Add-Result "listado: total del pedido" "True" ([bool]($uno.total -eq ($pDet * 2 + $pBeb)))
    Add-Result "listado: lineas del pedido" 2 $uno.lineas
    Add-Result "listado por mesa" "True" ([bool](@((Invoke-Api "/pedidos?mesa=$numMesa" $h).json.pedidos | Where-Object { $_.id_pedido -eq $id }).Count))
    Add-Result "listado por estado PENDIENTE" "True" ([bool](@((Invoke-Api "/pedidos?estado=PENDIENTE" $h).json.pedidos | Where-Object { $_.id_pedido -eq $id }).Count))
    Add-Result "listado por estado CANCELADO no lo trae" "False" ([bool](@((Invoke-Api "/pedidos?estado=CANCELADO" $h).json.pedidos | Where-Object { $_.id_pedido -eq $id }).Count))
    Add-Result "listado con fecha invalida -> 400" 400 (Invoke-Api "/pedidos?fecha=26-10-2026" $h).status
    Add-Result "listado con estado invalido -> 400" 400 (Invoke-Api "/pedidos?estado=TERMINADO" $h).status
    Add-Result "listado con mesa 0 -> 400" 400 (Invoke-Api "/pedidos?mesa=0" $h).status

    # ===== 3. agregar items =====
    $ag = Invoke-Api "/pedidos/$id/items" $h "POST" @{ items = @(@{ tipo = "BEBIDA"; id_tipo = $bebida; cantidad = 2 }) }
    Add-Result "agregar bebida -> 200" 200 $ag.status
    Add-Result "agregar bebida: queda una linea nueva" 3 (@($ag.json.items).Count)
    $nueva = @($ag.json.items | Where-Object { $_.tipo -eq "BEBIDA" -and $_.cantidad -eq 2 })[0]
    Add-Result "agregar bebida: precio del catalogo" "True" ([bool]($nueva.precio -eq $pBeb))
    Add-Result "agregar sin items -> 400" 400 (Invoke-Api "/pedidos/$id/items" $h "POST" @{ items = @() }).status
    Add-Result "agregar cantidad 0 -> 400" 400 (Invoke-Api "/pedidos/$id/items" $h "POST" @{ items = @(@{ tipo = "BEBIDA"; id_tipo = $bebida; cantidad = 0 }) }).status
    Add-Result "agregar tipo invalido -> 400" 400 (Invoke-Api "/pedidos/$id/items" $h "POST" @{ items = @(@{ tipo = "POSTRE"; id_tipo = $bebida; cantidad = 1 }) }).status
    Add-Result "agregar plato inexistente -> 404" 404 (Invoke-Api "/pedidos/$id/items" $h "POST" @{ items = @(@{ tipo = "PLATO"; id_tipo = $platoInexistente; cantidad = 1 }) }).status
    Add-Result "agregar detallado sin ingredientes -> 422" 422 (Invoke-Api "/pedidos/$id/items" $h "POST" @{ items = @(@{ tipo = "PLATO"; id_tipo = $platoDet; cantidad = 1 }) }).status
    Add-Result "agregar bebida con ingredientes -> 422" 422 (Invoke-Api "/pedidos/$id/items" $h "POST" @{ items = @(@{ tipo = "BEBIDA"; id_tipo = $bebida; cantidad = 1; ingredientes = (Ings $set1) }) }).status
    Add-Result "agregar a pedido inexistente -> 404" 404 (Invoke-Api "/pedidos/999999999/items" $h "POST" @{ items = @(@{ tipo = "BEBIDA"; id_tipo = $bebida; cantidad = 1 }) }).status

    # ===== 4. quitar items =====
    $lineaPlato = [int](@($ag.json.items | Where-Object { $_.tipo -eq "PLATO" })[0].id_detalle_pedido)
    $lineaBebida = [int](@($ag.json.items | Where-Object { $_.tipo -eq "BEBIDA" })[0].id_detalle_pedido)
    Add-Result "quitar sin clave_maestra -> 400" 400 (Invoke-Api "/pedidos/$id/items/$lineaBebida" $h "DELETE").status
    Add-Result "quitar con clave mala -> 403" 403 (Invoke-Api "/pedidos/$id/items/$lineaBebida`?clave_maestra=$pinMalo" $h "DELETE").status
    Add-Result "quitar linea de otro pedido -> 404" 404 (Invoke-Api "/pedidos/999999999/items/$lineaBebida`?clave_maestra=$pinOk" $h "DELETE").status
    Add-Result "quitar mas unidades de las que hay -> 422" 422 (Invoke-Api "/pedidos/$id/items/$lineaPlato`?clave_maestra=$pinOk&cantidad=9" $h "DELETE").status

    $q1 = Invoke-Api "/pedidos/$id/items/$lineaPlato`?clave_maestra=$pinOk" $h "DELETE"
    Add-Result "quitar 1 de 2 -> 200" 200 $q1.status
    Add-Result "quitar 1 de 2: la linea queda en 1" 1 ([int](Sql-Scalar "SELECT Cantidad FROM Detalle_pedido WHERE Id_detalle_pedido=$lineaPlato"))
    Add-Result "quitar 1 de 2: los ingredientes siguen" "True" ([int](Sql-Scalar "SELECT COUNT(*) FROM Detalle_ingredientes_pedido WHERE Id_detalle_pedido=$lineaPlato") -gt 0)

    $q2 = Invoke-Api "/pedidos/$id/items/$lineaPlato`?clave_maestra=$pinOk" $h "DELETE"
    Add-Result "quitar la ultima unidad -> 200" 200 $q2.status
    Add-Result "quitar la ultima unidad: la linea desaparece" 0 ([int](Sql-Scalar "SELECT COUNT(*) FROM Detalle_pedido WHERE Id_detalle_pedido=$lineaPlato"))
    Add-Result "quitar la ultima unidad: sus ingredientes tambien" 0 ([int](Sql-Scalar "SELECT COUNT(*) FROM Detalle_ingredientes_pedido WHERE Id_detalle_pedido=$lineaPlato"))
    Add-Result "quitar una linea que ya no existe -> 404" 404 (Invoke-Api "/pedidos/$id/items/$lineaPlato`?clave_maestra=$pinOk" $h "DELETE").status

    # ===== 5. PATCH /estado =====
    Add-Result "estado distinto de SALIENDO -> 422" 422 (Invoke-Api "/pedidos/$id/estado" $h "PATCH" @{ estado = "TERMINADO" }).status
    $e = Invoke-Api "/pedidos/$id/estado" $h "PATCH" @{ estado = "SALIENDO" }
    Add-Result "PENDIENTE -> SALIENDO -> 200 y SALIENDO" "200|SALIENDO" ("{0}|{1}" -f $e.status, $e.json.estado)
    Add-Result "repetir SALIENDO -> 422" 422 (Invoke-Api "/pedidos/$id/estado" $h "PATCH" @{ estado = "SALIENDO" }).status
    Add-Result "el pedido no cambio de mesa" $mesa ([int](Sql-Scalar "SELECT Id_mesa FROM Pedidos WHERE Id_pedido=$id"))
    Add-Result "en SALIENDO se pueden agregar items" 200 (Invoke-Api "/pedidos/$id/items" $h "POST" @{ items = @(@{ tipo = "BEBIDA"; id_tipo = $bebida; cantidad = 1 }) }).status

    # ===== 6. cancelar =====
    Add-Result "cancelar sin observaciones -> 400" 400 (Invoke-Api "/pedidos/$id/cancelar" $h "POST" @{ }).status
    Add-Result "cancelar con 201 caracteres -> 400" 400 (Invoke-Api "/pedidos/$id/cancelar" $h "POST" @{ observaciones = ('x' * 201) }).status
    $c = Invoke-Api "/pedidos/$id/cancelar" $h "POST" @{ observaciones = "El cliente se retiro" }
    Add-Result "cancelar -> 200 y CANCELADO" "200|CANCELADO" ("{0}|{1}" -f $c.status, $c.json.estado)
    Add-Result "cancelar guarda el motivo" "El cliente se retiro" ([string](Sql-Scalar "SELECT Observaciones_pedido FROM Pedidos WHERE Id_pedido=$id"))
    Add-Result "cancelar dos veces -> 422" 422 (Invoke-Api "/pedidos/$id/cancelar" $h "POST" @{ observaciones = "otra" }).status
    Add-Result "tras cancelar no se agregan items -> 422" 422 (Invoke-Api "/pedidos/$id/items" $h "POST" @{ items = @(@{ tipo = "BEBIDA"; id_tipo = $bebida; cantidad = 1 }) }).status
    Add-Result "tras cancelar no se quitan items -> 422" 422 (Invoke-Api "/pedidos/$id/items/$lineaBebida`?clave_maestra=$pinOk" $h "DELETE").status
    Add-Result "tras cancelar no se cambia el estado -> 422" 422 (Invoke-Api "/pedidos/$id/estado" $h "PATCH" @{ estado = "SALIENDO" }).status

    # ===== 7. imprimir =====
    Add-Result "imprimir tipo invalido -> 400" 400 (Invoke-Api "/pedidos/$id/imprimir" $h "POST" @{ tipo = "POSTRES" }).status
    Add-Result "imprimir copias 0 -> 400" 400 (Invoke-Api "/pedidos/$id/imprimir" $h "POST" @{ tipo = "TODO"; copias = 0 }).status
    Add-Result "imprimir pedido inexistente -> 404" 404 (Invoke-Api "/pedidos/999999999/imprimir" $h "POST" @{ }).status
    $imp = Invoke-Api "/pedidos/$id/imprimir" $h "POST" @{ tipo = "TODO"; copias = 1 }
    Add-Result "imprimir (real) -> 202" 202 $imp.status
    if ($esPdf) {
        # La impresión va en el hilo de UI después del 202: se espera a que aparezca el archivo.
        $archivo = $null
        for ($i = 0; $i -lt 20 -and -not $archivo; $i++) {
            Start-Sleep -Milliseconds 500
            $archivo = Get-ChildItem $carpetaPdf -Filter "comanda-$id-*.pdf" -ErrorAction SilentlyContinue | Where-Object { $_.LastWriteTime -ge $inicio } | Select-Object -First 1
        }
        Add-Result "imprimir: guarda el PDF en Documentos sin dialogo" "True" ([bool]$archivo)
        if ($archivo) {
            $bytes = [IO.File]::ReadAllBytes($archivo.FullName)
            $cabecera = [Text.Encoding]::ASCII.GetString($bytes, 0, 4)
            Add-Result "el archivo guardado es un PDF" "%PDF" $cabecera
            Add-Result "el PDF no esta vacio" "True" ($bytes.Length -gt 1000)
            Start-Sleep -Milliseconds 1500   # el otro grupo (platos/bebidas) termina justo despues
            Get-ChildItem $carpetaPdf -Filter "comanda-$id-*.pdf" -ErrorAction SilentlyContinue | Where-Object { $_.LastWriteTime -ge $inicio } | Remove-Item -Force
        }
    } else {
        Add-Result "impresora fisica: no se genera PDF" "False" (Test-Path $carpetaPdf)
    }
}
finally {
    if ($proc -and -not $proc.HasExited) { Stop-Process -Id $proc.Id -Force }
    if ($configOriginal) { Start-Sleep -Milliseconds 300; [IO.File]::WriteAllText($exeConfig, $configOriginal) }
    Start-Sleep -Milliseconds 300
    try {
        Sql-Exec "DELETE FROM Detalle_ingredientes_pedido WHERE Id_pedido > $maxAntes; DELETE FROM Detalle_pedido WHERE Id_pedido > $maxAntes; DELETE FROM Pedidos WHERE Id_pedido > $maxAntes;"
    } catch { Write-Host "No se pudo limpiar la copia: $_" -ForegroundColor Red }
}

$despues = "{0}|{1}|{2}" -f (Sql-Scalar "SELECT COUNT(*) FROM Pedidos"), (Sql-Scalar "SELECT COUNT(*) FROM Detalle_pedido"), (Sql-Scalar "SELECT COUNT(*) FROM Detalle_ingredientes_pedido")
Add-Result "copia limpia: conteos iguales a los iniciales" $antes $despues
Add-Result "copia limpia: no quedo ningun pedido nuevo" $maxAntes ([int](Sql-Scalar "SELECT ISNULL(MAX(Id_pedido), 0) FROM Pedidos"))

$results | Format-Table -AutoSize | Out-String -Width 230
$fallan = @($results | Where-Object { $_.ok -ne "OK" }).Count
"{0} OK de {1} ({2} fallan)" -f ($results.Count - $fallan), $results.Count, $fallan
if ($fallan -gt 0) { exit 1 }
