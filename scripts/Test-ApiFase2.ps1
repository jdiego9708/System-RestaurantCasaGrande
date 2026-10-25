<#
    Prueba de extremo a extremo de la fase 2 de la API movil (catalogo, mesas, imagenes): arranca el
    SISMistico.exe REAL de la compilacion local contra CasaGrandeBD_prod y compara cada respuesta con
    consultas directas a la BD (fuente independiente). Deja la copia y la config como estaban.

    Requisitos: .\SISMistico\run-local.ps1 -Rebuild -NoRun  y la copia restaurada (db\Restore-ProdCopy.ps1).
    Los datos esperados de mesas asumen el dia de la copia (2026-10-24); por eso se pide ?fecha= explicita.

    Uso:  .\Test-ApiFase2.ps1
#>
param(
    [string]$Database = "CasaGrandeBD_prod",
    [string]$Base = "http://localhost:8099/api/v1",
    [string]$FechaCopia = "2026-10-24"
)

$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.Data
$buildDir = Join-Path $PSScriptRoot "..\SISMistico\SISMistico\bin\LocalBuild"
$exe = (Resolve-Path (Join-Path $buildDir "SISMistico.exe")).Path
$exeConfig = "$exe.config"
$apiKey = "cambiar-esta-clave"
$results = New-Object System.Collections.Generic.List[object]
$cs = "Server=(localdb)\MSSQLLocalDB;Database=$Database;Integrated Security=true"

function Add-Result([string]$caso, $esperado, $obtenido) {
    $results.Add([pscustomobject]@{ caso = $caso; esperado = "$esperado"; obtenido = "$obtenido"; ok = $(if ("$esperado" -eq "$obtenido") { "OK" } else { "FALLA" }) })
}
function Count-Of($j) { if ($null -eq $j) { 0 } else { @($j).Count } }

function Invoke-Sql-Scalar([string]$sql) {
    $cn = New-Object System.Data.SqlClient.SqlConnection $cs; $cn.Open()
    $cmd = $cn.CreateCommand(); $cmd.CommandText = $sql; $v = $cmd.ExecuteScalar(); $cn.Close(); return $v
}
function Invoke-Sql-Exec([string]$sql) {
    $cn = New-Object System.Data.SqlClient.SqlConnection $cs; $cn.Open()
    $cmd = $cn.CreateCommand(); $cmd.CommandText = $sql; [void]$cmd.ExecuteNonQuery(); $cn.Close()
}

function Invoke-Api([string]$ruta, [hashtable]$headers = $null, [string]$metodo = "GET", $cuerpo = $null) {
    $a = @{ Uri = "$Base$ruta"; Method = $metodo; UseBasicParsing = $true; TimeoutSec = 20 }
    if ($headers) { $a.Headers = $headers }
    if ($null -ne $cuerpo) { $a.Body = [Text.Encoding]::UTF8.GetBytes(($cuerpo | ConvertTo-Json -Compress)); $a.ContentType = "application/json" }
    try {
        $r = Invoke-WebRequest @a
        $bytes = $r.RawContentStream.ToArray()
        $json = $null
        if ($r.Headers["Content-Type"] -like "application/json*" -and $r.Content) { $json = $r.Content | ConvertFrom-Json }
        return [pscustomobject]@{ status = [int]$r.StatusCode; json = $json; bytes = $bytes; tipo = "$($r.Headers['Content-Type'])" }
    } catch {
        $resp = $_.Exception.Response
        if ($null -eq $resp) { throw }
        $text = (New-Object IO.StreamReader($resp.GetResponseStream())).ReadToEnd()
        $json = $null; try { if ($text) { $json = $text | ConvertFrom-Json } } catch { }
        return [pscustomobject]@{ status = [int]$resp.StatusCode; json = $json; bytes = $null; tipo = "" }
    }
}

# ---- credenciales de prueba (de la copia local; no se imprimen) ----
$cn = New-Object System.Data.SqlClient.SqlConnection $cs; $cn.Open(); $cmd = $cn.CreateCommand()
$cmd.CommandText = "SELECT TOP 1 Id_empleado, Password FROM Empleados WHERE Estado='ACTIVO' AND Password IS NOT NULL AND Password <> '' AND Id_empleado > 0 ORDER BY Id_empleado"
$r = $cmd.ExecuteReader(); [void]$r.Read(); $idEmp = [int]$r[0]; $passEmp = [string]$r[1]; $r.Close(); $cn.Close()

# ---- datos de apoyo: platos para las pruebas de detalle e imagen ----
$platoDet = [int](Invoke-Sql-Scalar "SELECT TOP 1 Id_plato FROM Platos WHERE Estado='ACTIVO' AND Plato_detallado='ACTIVO' ORDER BY Id_plato")
$platoSimple = [int](Invoke-Sql-Scalar "SELECT TOP 1 Id_plato FROM Platos WHERE Estado='ACTIVO' AND Plato_detallado<>'ACTIVO' ORDER BY Id_plato")
$platoInact = [int](Invoke-Sql-Scalar "SELECT TOP 1 Id_plato FROM Platos WHERE Estado<>'ACTIVO' ORDER BY Id_plato")
$platoImg = [int](Invoke-Sql-Scalar "SELECT TOP 1 Id_plato FROM Platos WHERE Estado='ACTIVO' ORDER BY Id_plato DESC")
$platoImgRota = [int](Invoke-Sql-Scalar "SELECT TOP 1 Id_plato FROM Platos WHERE Estado='ACTIVO' AND Id_plato <> $platoImg ORDER BY Id_plato DESC")
$imgOriginal = [string](Invoke-Sql-Scalar "SELECT Imagen_plato FROM Platos WHERE Id_plato = $platoImg")
$imgRotaOriginal = [string](Invoke-Sql-Scalar "SELECT Imagen_plato FROM Platos WHERE Id_plato = $platoImgRota")
$imagenesAntes = "{0}|{1}|{2}" -f (Invoke-Sql-Scalar "SELECT COUNT(*) FROM Platos WHERE Imagen_plato <> 'SIN IMAGEN'"), $imgOriginal, $imgRotaOriginal
$palabra = [string](Invoke-Sql-Scalar "SELECT TOP 1 LEFT(Nombre_plato, 4) FROM Platos WHERE Estado='ACTIVO' AND LEN(Nombre_plato) >= 4 AND Nombre_plato NOT LIKE '%[^A-Za-z]%' ORDER BY Id_plato")

# ---- carpeta temporal de imagenes + config temporal ----
$imgDir = Join-Path $env:TEMP ("api_img_" + [guid]::NewGuid().ToString("N"))
New-Item -ItemType Directory $imgDir | Out-Null
$jpg = [byte[]](0xFF, 0xD8, 0xFF, 0xE0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0xFF, 0xD9)
[IO.File]::WriteAllBytes((Join-Path $imgDir "ok.jpg"), $jpg)
[IO.File]::WriteAllText((Join-Path $imgDir "secret.txt"), "no debe salir")
$configBak = "$exeConfig.testbak"
Copy-Item $exeConfig $configBak -Force
[xml]$cfg = Get-Content $exeConfig -Raw
$node = $cfg.CreateElement("add"); [void]$node.SetAttribute("key", "ApiImagenesCarpeta"); [void]$node.SetAttribute("value", $imgDir)
[void]$cfg.configuration.appSettings.AppendChild($node); $cfg.Save($exeConfig)

Get-Process SISMistico -ErrorAction SilentlyContinue | ForEach-Object { throw "Ya hay un SISMistico abierto (PID $($_.Id)); cierralo antes." }
$proc = $null
try {
    Invoke-Sql-Exec "UPDATE Platos SET Imagen_plato = 'ok.jpg' WHERE Id_plato = $platoImg"
    Invoke-Sql-Exec "UPDATE Platos SET Imagen_plato = 'noexiste.jpg' WHERE Id_plato = $platoImgRota"

    $proc = Start-Process -FilePath $exe -WorkingDirectory $buildDir -PassThru
    $listo = $false
    for ($i = 0; $i -lt 30; $i++) { Start-Sleep -Milliseconds 500; try { if ((Invoke-Api "/ping").status -eq 200) { $listo = $true; break } } catch { } }
    if (-not $listo) { throw "La API no respondio en 15 s." }

    $k = @{ "X-Api-Key" = $apiKey }
    $l = Invoke-Api "/auth/login" $k "POST" @{ id_empleado = $idEmp; password = $passEmp }
    if ($l.status -ne 200) { throw "No se pudo iniciar sesion de prueba ($($l.status))." }
    $h = @{ "X-Api-Key" = $apiKey; "Authorization" = "Bearer $($l.json.token)" }

    # ===== autenticacion =====
    foreach ($ruta in "/mesas", "/platos", "/bebidas", "/tipos-platos", "/ingredientes", "/clientes", "/imagenes/ok.jpg") {
        Add-Result "sin token: GET $ruta -> 401" 401 (Invoke-Api $ruta $k).status
    }
    Add-Result "sin X-Api-Key: GET /mesas -> 401" 401 (Invoke-Api "/mesas" @{ "Authorization" = "Bearer $($l.json.token)" }).status

    # ===== tipos =====
    $t = Invoke-Api "/tipos-platos" $h
    Add-Result "tipos-platos: cantidad = BD" (Invoke-Sql-Scalar "SELECT COUNT(*) FROM Tipo_platos") (Count-Of $t.json)
    Add-Result "tipos-platos incluye ALMUERZOS=8080" 1 (Count-Of ($t.json | Where-Object { $_.id_tipo_plato -eq 8080 -and $_.tipo_plato -eq "ALMUERZOS" }))
    Add-Result "tipos-bebidas: cantidad = BD" (Invoke-Sql-Scalar "SELECT COUNT(*) FROM Tipo_bebidas") (Count-Of (Invoke-Api "/tipos-bebidas" $h).json)

    # ===== platos =====
    $p = Invoke-Api "/platos" $h
    Add-Result "platos: 200" 200 $p.status
    Add-Result "platos: solo ACTIVO (cantidad = BD)" (Invoke-Sql-Scalar "SELECT COUNT(*) FROM Platos p JOIN Tipo_platos t ON t.Id_tipo_plato=p.Id_tipo_plato WHERE p.Estado='ACTIVO'") (Count-Of $p.json)
    Add-Result "platos: suma de precios = BD (enteros)" ([long](Invoke-Sql-Scalar "SELECT SUM(Precio_plato) FROM Platos WHERE Estado='ACTIVO'")) ([long](($p.json | Measure-Object -Property precio -Sum).Sum))
    Add-Result "platos: ningun inactivo" 0 (Count-Of ($p.json | Where-Object { $_.id_plato -eq $platoInact }))
    Add-Result "platos: campos del contrato" "True" ((@("id_plato","nombre","precio","id_tipo_plato","tipo_plato","descripcion","detallado","carta","imagen_url") | Where-Object { $p.json[0].PSObject.Properties.Name -notcontains $_ }).Count -eq 0)
    Add-Result "platos?tipo_plato=8080 = BD" (Invoke-Sql-Scalar "SELECT COUNT(*) FROM Platos WHERE Estado='ACTIVO' AND Id_tipo_plato=8080") (Count-Of (Invoke-Api "/platos?tipo_plato=8080" $h).json)
    Add-Result "platos?tipo_plato=abc -> 400" 400 (Invoke-Api "/platos?tipo_plato=abc" $h).status
    Add-Result "platos?q=$palabra = BD" (Invoke-Sql-Scalar "SELECT COUNT(*) FROM Platos WHERE Estado='ACTIVO' AND Nombre_plato LIKE '%$palabra%'") (Count-Of (Invoke-Api "/platos?q=$palabra" $h).json)
    Add-Result "platos?q=% (comodin escapado) = BD literal" (Invoke-Sql-Scalar "SELECT COUNT(*) FROM Platos WHERE Estado='ACTIVO' AND Nombre_plato LIKE '%[%]%'") (Count-Of (Invoke-Api "/platos?q=%25" $h).json)
    Add-Result "platos?q=_ (comodin escapado) = BD literal" (Invoke-Sql-Scalar "SELECT COUNT(*) FROM Platos WHERE Estado='ACTIVO' AND Nombre_plato LIKE '%[_]%'") (Count-Of (Invoke-Api "/platos?q=_" $h).json)
    $inj = Invoke-Api ("/platos?q=" + [uri]::EscapeDataString("' OR 1=1 --")) $h
    Add-Result "platos?q=inyeccion SQL -> 200 y 0 resultados" "200|0" ("{0}|{1}" -f $inj.status, (Count-Of $inj.json))
    Add-Result "platos?q de 51 caracteres -> 400" 400 (Invoke-Api ("/platos?q=" + ("a" * 51)) $h).status

    # ===== detalle de plato e ingredientes =====
    $d = Invoke-Api "/platos/$platoDet" $h
    Add-Result "platos/{detallado}: detallado=true con 14 ingredientes" "200|True|14" ("{0}|{1}|{2}" -f $d.status, $d.json.detallado, (Count-Of $d.json.ingredientes))
    Add-Result "platos/{detallado}/ingredientes = 14" 14 (Count-Of (Invoke-Api "/platos/$platoDet/ingredientes" $h).json)
    $s = Invoke-Api "/platos/$platoSimple" $h
    Add-Result "platos/{simple}: detallado=false y sin ingredientes" "200|False|False" ("{0}|{1}|{2}" -f $s.status, $s.json.detallado, ($s.json.PSObject.Properties.Name -contains "ingredientes"))
    Add-Result "platos/{simple}/ingredientes = []" 0 (Count-Of (Invoke-Api "/platos/$platoSimple/ingredientes" $h).json)
    Add-Result "platos/{inactivo} -> 404" 404 (Invoke-Api "/platos/$platoInact" $h).status
    Add-Result "platos/{inexistente} -> 404" 404 (Invoke-Api "/platos/2147000000" $h).status
    Add-Result "platos/abc -> 400" 400 (Invoke-Api "/platos/abc" $h).status

    # ===== bebidas =====
    $b = Invoke-Api "/bebidas" $h
    Add-Result "bebidas: solo ACTIVO (cantidad = BD)" (Invoke-Sql-Scalar "SELECT COUNT(*) FROM Bebidas b JOIN Tipo_bebidas t ON t.Id_tipo_bebida=b.Id_tipo_bebida WHERE b.Estado='ACTIVO'") (Count-Of $b.json)
    Add-Result "bebidas: suma de precios = BD" ([long](Invoke-Sql-Scalar "SELECT SUM(Precio_bebida) FROM Bebidas WHERE Estado='ACTIVO'")) ([long](($b.json | Measure-Object -Property precio -Sum).Sum))
    Add-Result "bebidas?tipo_bebida=2022 = BD" (Invoke-Sql-Scalar "SELECT COUNT(*) FROM Bebidas WHERE Estado='ACTIVO' AND Id_tipo_bebida=2022") (Count-Of (Invoke-Api "/bebidas?tipo_bebida=2022" $h).json)
    Add-Result "bebidas?q=%25 (comodin escapado) = BD literal" (Invoke-Sql-Scalar "SELECT COUNT(*) FROM Bebidas WHERE Estado='ACTIVO' AND Nombre_bebida LIKE '%[%]%'") (Count-Of (Invoke-Api "/bebidas?q=%25" $h).json)

    # ===== ingredientes y clientes =====
    Add-Result "ingredientes = BD" (Invoke-Sql-Scalar "SELECT COUNT(*) FROM Ingredientes") (Count-Of (Invoke-Api "/ingredientes" $h).json)
    Add-Result "ingredientes?tipo=SOPA = BD" (Invoke-Sql-Scalar "SELECT COUNT(*) FROM Ingredientes WHERE Tipo_ingrediente='SOPA'") (Count-Of (Invoke-Api "/ingredientes?tipo=SOPA" $h).json)
    Add-Result "ingredientes?tipo=ACOMPANANTE = BD" (Invoke-Sql-Scalar "SELECT COUNT(*) FROM Ingredientes WHERE Tipo_ingrediente='ACOMPANANTE'") (Count-Of (Invoke-Api "/ingredientes?tipo=ACOMPANANTE" $h).json)
    $c = Invoke-Api "/clientes" $h
    Add-Result "clientes: sin el comodin 0 (cantidad = BD)" (Invoke-Sql-Scalar "SELECT COUNT(*) FROM Clientes WHERE Id_cliente>0 AND Estado_cliente='ACTIVO'") (Count-Of $c.json)
    Add-Result "clientes: no incluye id 0" 0 (Count-Of ($c.json | Where-Object { $_.id_cliente -eq 0 }))
    Add-Result "clientes: solo id, nombre y telefono" "id_cliente,nombre,telefono" ((@($c.json)[0].PSObject.Properties.Name) -join ",")
    Add-Result "clientes?q=CRISTIAN = BD" (Invoke-Sql-Scalar "SELECT COUNT(*) FROM Clientes WHERE Id_cliente>0 AND Nombre_cliente LIKE '%CRISTIAN%'") (Count-Of (Invoke-Api "/clientes?q=CRISTIAN" $h).json)

    # ===== mesas =====
    $m = Invoke-Api "/mesas?fecha=$FechaCopia" $h
    $nums = @($m.json | ForEach-Object { $_.num_mesa })
    Add-Result "mesas: 36 (una por Num_mesa, sin la 0)" 36 (Count-Of $m.json)
    Add-Result "mesas: num_mesa 1..36 sin repetir" "True" (($nums | Sort-Object -Unique).Count -eq 36 -and $nums[0] -eq 1 -and $nums[-1] -eq 36)
    Add-Result "mesas: Mesa 21 usa Id_mesa 5150 (no el duplicado 5200)" "5150" (@($m.json | Where-Object { $_.num_mesa -eq 21 })[0].id_mesa)
    Add-Result "mesas: ni la 0 ni la 5200" 0 (Count-Of ($m.json | Where-Object { $_.id_mesa -eq 0 -or $_.id_mesa -eq 5200 }))
    $abiertas = Invoke-Sql-Scalar "SELECT COUNT(DISTINCT m.Num_mesa) FROM Pedidos p JOIN Mesas m ON m.Id_mesa=p.Id_mesa WHERE p.Fecha_pedido='$FechaCopia' AND p.Estado_pedido IN ('PENDIENTE','SALIENDO')"
    Add-Result "mesas: ocupadas = BD ($abiertas)" $abiertas (Count-Of ($m.json | Where-Object { $_.estado -ne "DISPONIBLE" }))
    $m1 = @($m.json | Where-Object { $_.num_mesa -eq 1 })[0]
    $esp1 = Invoke-Sql-Scalar "SELECT TOP 1 CONCAT(p.Id_pedido,'|',p.Estado_pedido,'|',LEFT(CONVERT(varchar(8),p.Hora_pedido),5),'|',CAST((SELECT SUM(Precio*Cantidad) FROM Detalle_pedido d WHERE d.Id_pedido=p.Id_pedido) AS bigint)) FROM Pedidos p JOIN Mesas m ON m.Id_mesa=p.Id_mesa WHERE m.Num_mesa=1 AND p.Fecha_pedido='$FechaCopia' AND p.Estado_pedido IN ('PENDIENTE','SALIENDO') ORDER BY p.Id_pedido DESC"
    Add-Result "mesa 1: pedido|estado|hora|total = BD" $esp1 ("{0}|{1}|{2}|{3}" -f $m1.id_pedido, $m1.estado, $m1.hora, $m1.total)
    $libre = @($m.json | Where-Object { $_.estado -eq "DISPONIBLE" })[0]
    Add-Result "mesa libre: id_pedido 0, total 0, hora null" "0|0|" ("{0}|{1}|{2}" -f $libre.id_pedido, $libre.total, $libre.hora)
    Add-Result "mesas sin fecha -> 200 y 36" "200|36" ("{0}|{1}" -f (Invoke-Api "/mesas" $h).status, (Count-Of (Invoke-Api "/mesas" $h).json))
    Add-Result "mesas?fecha=2026-13-45 -> 400" 400 (Invoke-Api "/mesas?fecha=2026-13-45" $h).status
    Add-Result "mesas?fecha=hoy -> 400" 400 (Invoke-Api "/mesas?fecha=hoy" $h).status
    Add-Result "mesas?fecha=2025-01-01 (sin abiertos de ese dia) -> todas DISPONIBLE" 36 (Count-Of (@((Invoke-Api "/mesas?fecha=2025-01-01" $h).json) | Where-Object { $_.estado -eq "DISPONIBLE" -and $_.id_pedido -eq 0 }))

    # ===== imagenes =====
    $img = Invoke-Api "/imagenes/ok.jpg" $h
    Add-Result "imagenes/ok.jpg: 200 image/jpeg y bytes identicos" "200|image/jpeg|True" ("{0}|{1}|{2}" -f $img.status, $img.tipo, (($img.bytes -join ",") -eq ($jpg -join ",")))
    Add-Result "imagenes/noexiste.jpg -> 404" 404 (Invoke-Api "/imagenes/noexiste.jpg" $h).status
    Add-Result "imagenes/secret.txt (extension no permitida) -> 404" 404 (Invoke-Api "/imagenes/secret.txt" $h).status
    Add-Result "imagenes/..%2f..%2fSISMistico.exe.config -> 404" 404 (Invoke-Api "/imagenes/..%2f..%2fSISMistico.exe.config" $h).status
    Add-Result "imagenes/..%5c..%5cx.jpg -> 404" 404 (Invoke-Api "/imagenes/..%5c..%5cx.jpg" $h).status
    Add-Result "imagenes/C:%5cwindows%5cwin.ini -> 404" 404 (Invoke-Api "/imagenes/C:%5Cwindows%5Cwin.ini" $h).status
    Add-Result "imagenes/%2e%2e.jpg -> 404" 404 (Invoke-Api "/imagenes/%2e%2e.jpg" $h).status
    $conImg = Invoke-Api "/platos/$platoImg" $h
    Add-Result "plato con imagen existente: imagen_url absoluta" "$Base/imagenes/ok.jpg" $conImg.json.imagen_url
    Add-Result "la imagen_url se descarga (200)" 200 (Invoke-Api ($conImg.json.imagen_url.Substring($Base.Length)) $h).status
    Add-Result "plato con imagen inexistente: imagen_url null" "" ("" + (Invoke-Api "/platos/$platoImgRota" $h).json.imagen_url)
    Add-Result "plato 'SIN IMAGEN': imagen_url null" "" ("" + (Invoke-Api "/platos/$platoSimple" $h).json.imagen_url)
}
finally {
    if ($proc -and -not $proc.HasExited) { Stop-Process -Id $proc.Id -Force }
    Start-Sleep -Milliseconds 300
    try { Invoke-Sql-Exec ("UPDATE Platos SET Imagen_plato = '{0}' WHERE Id_plato = {1}" -f $imgOriginal.Replace("'", "''"), $platoImg) } catch { Write-Host "No se pudo restaurar la imagen del plato ${platoImg}: $_" -ForegroundColor Red }
    try { Invoke-Sql-Exec ("UPDATE Platos SET Imagen_plato = '{0}' WHERE Id_plato = {1}" -f $imgRotaOriginal.Replace("'", "''"), $platoImgRota) } catch { Write-Host "No se pudo restaurar la imagen del plato ${platoImgRota}: $_" -ForegroundColor Red }
    if (Test-Path $configBak) { Move-Item $configBak $exeConfig -Force }
    Remove-Item $imgDir -Recurse -Force -ErrorAction SilentlyContinue
}

Add-Result "BD restaurada (platos con imagen | valor plato 1 | valor plato 2)" $imagenesAntes ("{0}|{1}|{2}" -f (Invoke-Sql-Scalar "SELECT COUNT(*) FROM Platos WHERE Imagen_plato <> 'SIN IMAGEN'"), (Invoke-Sql-Scalar "SELECT Imagen_plato FROM Platos WHERE Id_plato = $platoImg"), (Invoke-Sql-Scalar "SELECT Imagen_plato FROM Platos WHERE Id_plato = $platoImgRota"))
Add-Result "config restaurada (sin ApiImagenesCarpeta)" "False" ((Get-Content $exeConfig -Raw) -match "ApiImagenesCarpeta")

$results | Format-Table -AutoSize | Out-String -Width 220
$fallan = @($results | Where-Object { $_.ok -ne "OK" }).Count
"{0} OK de {1} ({2} fallan)" -f ($results.Count - $fallan), $results.Count, $fallan
if ($fallan -gt 0) { exit 1 }
