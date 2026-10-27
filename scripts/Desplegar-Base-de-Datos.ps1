<#
    Prepara la base de datos del restaurante para la API movil, paso a paso y pidiendo confirmacion antes de escribir.
    En la USB se llama "01-Base-de-Datos.ps1" (Preparar-USB.ps1 lo copia con ese nombre).

    EJECUTAR EN EL PC DEL RESTAURANTE, con el MISMO usuario de Windows que abre el sistema (LocalDB es por usuario):
    clic derecho > "Ejecutar con PowerShell". No necesita administrador.

        .\Desplegar-Base-de-Datos.ps1                       # flujo completo, con confirmaciones
        .\Desplegar-Base-de-Datos.ps1 -SoloVerificar        # solo lee: dice que falta, no cambia nada
        .\Desplegar-Base-de-Datos.ps1 -Database CasaGrandeBD_prod -CarpetaRespaldo D:\Respaldos

    Flujo:
      1. Comprueba LocalDB y que exista la base (si no existe, ofrece restaurarla desde un respaldo .sql).
      2. Muestra el estado actual (verificar-despliegue.sql).
      3. HACE UNA COPIA DE SEGURIDAD (.bak) de la base. Sin copia no continua.
      4. Con su confirmacion, crea/actualiza los 3 procedimientos nuevos (01, 03, 04). Son aditivos y repetibles.
      5. Con su confirmacion aparte, unifica empleados duplicados y PIN 0 (02). Deja respaldo dentro de la base.
      6. Vuelve a verificar y resume.
#>
[CmdletBinding()]
param(
    [string]$Database = 'CasaGrandeBD',
    [string]$Instance = '(localdb)\MSSQLLocalDB',
    [string]$CarpetaRespaldo = 'C:\Respaldos',
    # Respaldo en formato script .sql para restaurar la base si no existe (opcional; si falta se pregunta).
    [string]$ScriptRespaldo,
    [switch]$SoloVerificar
)

$ErrorActionPreference = 'Stop'
if ($Database -notmatch '^[A-Za-z0-9_]+$') { throw "Nombre de base no valido: $Database" }

function Paso($t)  { Write-Host "`n==> $t" -ForegroundColor Cyan }
function Ok($t)    { Write-Host "    OK  $t" -ForegroundColor Green }
function Aviso($t) { Write-Host "    !!  $t" -ForegroundColor Yellow }
function Termina($codigo) {
    Write-Host ''
    Read-Host 'Pulse Enter para cerrar' | Out-Null
    exit $codigo
}
function Pregunta([string]$texto) {
    $r = Read-Host "$texto (S/N)"
    return ($r -match '^[sS]')
}

# ---------- Donde estan los scripts de la base ----------
$db = @((Join-Path $PSScriptRoot 'Soporte\db'), (Join-Path $PSScriptRoot '..\db')) |
    Where-Object { Test-Path (Join-Path $_ 'Invoke-Sql.ps1') } | Select-Object -First 1
if (-not $db) { Write-Host 'No encuentro la carpeta con Invoke-Sql.ps1 (Soporte\db o ..\db).' -ForegroundColor Red; Termina 1 }
$db = (Resolve-Path $db).Path
$invoke = Join-Path $db 'Invoke-Sql.ps1'

function Correr-Sql([string]$archivo, [string]$base) {
    & powershell.exe -NoProfile -ExecutionPolicy Bypass -File $invoke -File $archivo -Database $base -Instance $Instance
    if ($LASTEXITCODE -ne 0) { throw "Fallo $(Split-Path $archivo -Leaf) (codigo $LASTEXITCODE). Nada mas se ejecuto." }
}

function Existe-Base {
    Add-Type -AssemblyName System.Data
    $cn = New-Object System.Data.SqlClient.SqlConnection "Server=$Instance;Database=master;Integrated Security=true;Connect Timeout=30"
    try {
        $cn.Open()
        $cmd = $cn.CreateCommand()
        $cmd.CommandText = "SELECT COUNT(*) FROM sys.databases WHERE name = N'$Database'"
        return ([int]$cmd.ExecuteScalar() -gt 0)
    } finally { $cn.Dispose() }
}

# La base de PRODUCCION tiene Detalle_pedido.Id_detalle_pedido; la base antigua de desarrollo no. Los procedimientos
# de la API dependen de esa columna, asi que sobre el esquema antiguo fallarian ("Invalid column name").
function Tiene-EsquemaProduccion {
    Add-Type -AssemblyName System.Data
    $cn = New-Object System.Data.SqlClient.SqlConnection "Server=$Instance;Database=$Database;Integrated Security=true;Connect Timeout=30"
    try {
        $cn.Open()
        $cmd = $cn.CreateCommand()
        $cmd.CommandText = "SELECT COUNT(*) FROM sys.columns c JOIN sys.tables t ON t.object_id = c.object_id " +
                           "WHERE t.name = N'Detalle_pedido' AND c.name = N'Id_detalle_pedido'"
        return ([int]$cmd.ExecuteScalar() -gt 0)
    } finally { $cn.Dispose() }
}

# ---------------------------------------------------------------------------
# Esquema antiguo: listar las bases y, si el usuario lo pide, reemplazar la antigua (siempre con copia previa)
# ---------------------------------------------------------------------------
function Q([string]$nombre) { '[' + $nombre.Replace(']', ']]') + ']' }
function Lit([string]$texto) { "N'" + $texto.Replace("'", "''") + "'" }

# Ejecuta SQL en master. Sin -Tabla no devuelve nada; con -Tabla devuelve un DataTable.
function Sql-Master([string]$sql, [switch]$Tabla) {
    Add-Type -AssemblyName System.Data
    $cn = New-Object System.Data.SqlClient.SqlConnection "Server=$Instance;Database=master;Integrated Security=true;Connect Timeout=30;Pooling=false"
    try {
        $cn.Open()
        $cmd = $cn.CreateCommand(); $cmd.CommandText = $sql; $cmd.CommandTimeout = 0
        if ($Tabla) {
            $dt = New-Object System.Data.DataTable
            [void](New-Object System.Data.SqlClient.SqlDataAdapter $cmd).Fill($dt)
            return , $dt
        }
        [void]$cmd.ExecuteNonQuery()
    } finally { $cn.Dispose() }
}

# Bases de usuario de la instancia, con tamano y si tienen el esquema de produccion.
function Listar-Bases {
    $dt = Sql-Master ("SELECT d.name, d.state_desc, CAST((SELECT SUM(CAST(f.size AS bigint)) * 8 / 1024 FROM sys.master_files f " +
                      "WHERE f.database_id = d.database_id) AS int) AS mb FROM sys.databases d WHERE d.database_id > 4 ORDER BY d.name") -Tabla
    $lista = @()
    foreach ($r in $dt.Rows) {
        $produccion = $false
        if ($r.state_desc -eq 'ONLINE') {
            $n = Q $r.name
            $c = Sql-Master ("SELECT COUNT(*) FROM $n.sys.columns c JOIN $n.sys.tables t ON t.object_id = c.object_id " +
                             "WHERE t.name = N'Detalle_pedido' AND c.name = N'Id_detalle_pedido'") -Tabla
            $produccion = ([int]$c.Rows[0][0] -gt 0)
        }
        $lista += [pscustomobject]@{ Nombre = $r.name; MB = $r.mb; Estado = $r.state_desc; Produccion = $produccion }
    }
    return , $lista
}

function Mostrar-Bases($bases) {
    Write-Host ''
    Write-Host "    Bases de datos en $Instance :" -ForegroundColor White
    $i = 1
    foreach ($b in $bases) {
        $esq = if ($b.Estado -ne 'ONLINE') { $b.Estado } elseif ($b.Produccion) { 'esquema de PRODUCCION' } else { 'esquema ANTIGUO' }
        $marca = if ($b.Nombre -eq $Database) { '   <-- la seleccionada' } else { '' }
        $color = if ($b.Produccion) { 'Green' } else { 'Yellow' }
        Write-Host ('      [{0}] {1,-28} {2,7:N0} MB   {3}{4}' -f $i, $b.Nombre, $b.MB, $esq, $marca) -ForegroundColor $color
        $i++
    }
}

function Respaldar-Base([string]$base, [string]$archivo, [switch]$CopyOnly) {
    $carpeta = Split-Path $archivo -Parent
    if (-not (Test-Path $carpeta)) { New-Item -ItemType Directory -Path $carpeta -Force | Out-Null }
    $extra = if ($CopyOnly) { ', COPY_ONLY' } else { '' }
    Sql-Master "BACKUP DATABASE $(Q $base) TO DISK = $(Lit $archivo) WITH INIT, CHECKSUM$extra"
    if (-not (Test-Path $archivo) -or (Get-Item $archivo).Length -lt 100KB) { throw "No se creo una copia valida de '$base' en $archivo. NO se continua." }
}

# Reemplaza la base $Database (antigua) por una con el esquema de produccion. Devuelve $true si quedo bien.
function Reemplazar-Base($bases) {
    if (Get-Process SISMistico -ErrorAction SilentlyContinue) {
        Write-Host '    Cierre SISRestaurante (SISMistico.exe) antes de reemplazar la base y repita.' -ForegroundColor Red
        return $false
    }
    $otras = @($bases | Where-Object { $_.Produccion -and $_.Nombre -ne $Database })
    Write-Host ''
    Write-Host "    De donde sale la base NUEVA que reemplazara a '$Database'?" -ForegroundColor White
    if ($otras.Count -gt 0) {
        Write-Host '    Bases existentes con el esquema de produccion (se copian con sus datos):' -ForegroundColor White
        $k = 1
        foreach ($o in $otras) { Write-Host ('      [{0}] {1,-28} {2,7:N0} MB' -f $k, $o.Nombre, $o.MB) -ForegroundColor Green; $k++ }
    } else {
        Write-Host '    (No hay otra base con el esquema de produccion en esta instancia.)' -ForegroundColor Yellow
    }
    Write-Host '    Otras fuentes:' -ForegroundColor White
    Write-Host '      [F] Restaurar un respaldo de produccion en formato script .sql (con datos)'
    Write-Host '      [V] Crear una base VACIA con el esquema de produccion (sin mesas, platos ni empleados)'
    Write-Host '      [0] Cancelar'
    $eleccion = (Read-Host '    Opcion').Trim().ToUpper()
    $origen = $null; $scriptSql = $null; $op = $null
    $n = 0
    if ([int]::TryParse($eleccion, [ref]$n) -and $n -ge 1 -and $n -le $otras.Count) {
        $op = 'copia'; $origen = $otras[$n - 1].Nombre
    }
    elseif ($eleccion -eq 'F') {
        $op = 'sql'
        $scriptSql = (Read-Host '    Ruta del respaldo .sql').Trim('" ')
        if (-not $scriptSql -or -not (Test-Path $scriptSql)) { Write-Host '    Ese archivo no existe. No se cambio nada.' -ForegroundColor Red; return $false }
    }
    elseif ($eleccion -eq 'V') {
        $op = 'vacia'
        $scriptSql = Join-Path $db 'produccion-schema.sql'
        if (-not (Test-Path $scriptSql)) { Write-Host "    Falta $scriptSql" -ForegroundColor Red; return $false }
    }
    else { Write-Host '    Cancelado: no se cambio nada.'; return $false }

    $resumen = switch ($op) { 'copia' { "copia de '$origen'" } 'sql' { "respaldo $scriptSql" } 'vacia' { 'base VACIA con el esquema de produccion' } }
    Write-Host ''
    Write-Host "    Se va a REEMPLAZAR '$Database' (esquema antiguo) por: $resumen" -ForegroundColor Yellow
    Write-Host '    Antes se hara una COPIA DE SEGURIDAD de la base antigua; con ella se puede volver atras.' -ForegroundColor Yellow
    if ($op -eq 'vacia') { Write-Host '    OJO: la base nueva queda sin datos; el sistema no tendra mesas, platos ni empleados hasta cargarlos.' -ForegroundColor Red }
    if ((Read-Host '    Escriba REEMPLAZAR para confirmar (cualquier otra cosa cancela)').Trim() -cne 'REEMPLAZAR') {
        Write-Host '    Cancelado: no se cambio nada.'; return $false
    }

    # 1) Copia de seguridad de la base ANTIGUA (obligatoria)
    $sello = Get-Date -Format 'yyyyMMdd_HHmm'
    $bakAntigua = Join-Path $CarpetaRespaldo ("{0}_ESQUEMA_ANTIGUO_{1}.bak" -f $Database, $sello)
    Write-Host ''
    Write-Host "    Copia de la base antigua -> $bakAntigua"
    Respaldar-Base $Database $bakAntigua
    Ok ("Copia de la base antigua creada ({0:N1} MB)" -f ((Get-Item $bakAntigua).Length / 1MB))

    # 2) Reemplazo
    $tmp = $null
    if ($op -eq 'copia') {
        $tmp = Join-Path $CarpetaRespaldo ("copia_de_{0}_para_reemplazo_{1}.bak" -f $origen, $sello)
        Write-Host "    Copiando '$origen' (COPY_ONLY: no altera esa base)..."
        Respaldar-Base $origen $tmp -CopyOnly
        $ficheros = Sql-Master "RESTORE FILELISTONLY FROM DISK = $(Lit $tmp)" -Tabla
        $viejo = Sql-Master "SELECT physical_name FROM sys.master_files WHERE database_id = DB_ID($(Lit $Database)) AND type = 0 ORDER BY file_id" -Tabla
        $dirDatos = Split-Path ([string]$viejo.Rows[0].physical_name) -Parent
        $moves = @(); $primero = $true
        foreach ($f in $ficheros.Rows) {
            if ($f.Type -eq 'L') { $ext = '.ldf' } elseif ($primero) { $ext = '.mdf'; $primero = $false } else { $ext = '.ndf' }
            $moves += "MOVE $(Lit $f.LogicalName) TO $(Lit (Join-Path $dirDatos ('{0}_{1}{2}' -f $Database, $f.LogicalName, $ext)))"
        }
        Write-Host "    Reemplazando '$Database'..."
        Sql-Master "ALTER DATABASE $(Q $Database) SET SINGLE_USER WITH ROLLBACK IMMEDIATE"
        try { Sql-Master "RESTORE DATABASE $(Q $Database) FROM DISK = $(Lit $tmp) WITH REPLACE, $($moves -join ', ')" }
        finally { try { Sql-Master "ALTER DATABASE $(Q $Database) SET MULTI_USER" } catch { } }
        Remove-Item $tmp -ErrorAction SilentlyContinue
    }
    elseif ($op -eq 'sql') {
        & powershell.exe -NoProfile -ExecutionPolicy Bypass -File (Join-Path $db 'Restore-ProdCopy.ps1') `
            -ScriptPath $scriptSql -Database $Database -Instance $Instance -Force
        if ($LASTEXITCODE -ne 0) { throw "Restore-ProdCopy.ps1 fallo (codigo $LASTEXITCODE). La copia de la base antigua esta en $bakAntigua" }
    }
    else {
        # El script de esquema empieza con USE [CasaGrandeBD]: se quita para que NO escriba en otra base.
        $texto = [IO.File]::ReadAllText($scriptSql, [Text.Encoding]::UTF8)
        $quitarUse = New-Object System.Text.RegularExpressions.Regex '(?m)^USE \[[^\]]+\]\r?\n'
        $texto = $quitarUse.Replace($texto, '', 1)   # solo la primera coincidencia
        $tmpSql = Join-Path $env:TEMP "esquema_$([guid]::NewGuid().ToString('N')).sql"
        [IO.File]::WriteAllText($tmpSql, $texto, (New-Object Text.UTF8Encoding $true))
        try {
            Sql-Master "ALTER DATABASE $(Q $Database) SET SINGLE_USER WITH ROLLBACK IMMEDIATE"
            Sql-Master "DROP DATABASE $(Q $Database)"
            Sql-Master "CREATE DATABASE $(Q $Database)"
            Correr-Sql $tmpSql $Database
        } finally { Remove-Item $tmpSql -ErrorAction SilentlyContinue }
    }

    if (-not (Existe-Base)) { throw "Tras el reemplazo la base $Database no aparece. La copia antigua esta en $bakAntigua" }
    if (-not (Tiene-EsquemaProduccion)) { throw "Tras el reemplazo la base $Database sigue sin el esquema de produccion. La copia antigua esta en $bakAntigua" }
    Ok "Base '$Database' reemplazada: ahora tiene el esquema de produccion"
    Write-Host "    La base ANTIGUA quedo respaldada en: $bakAntigua" -ForegroundColor White
    return $true
}

# Tras el aviso de esquema antiguo: lista las bases y ofrece reemplazar / usar otra / salir. $true = se puede seguir.
function Resolver-EsquemaAntiguo {
    $bases = Listar-Bases
    Mostrar-Bases $bases
    if ($SoloVerificar) {
        Write-Host ''
        Write-Host '    (-SoloVerificar: no se ofrece ningun cambio.)'
        return $false
    }
    Write-Host ''
    Write-Host '    Que desea hacer?' -ForegroundColor White
    Write-Host "      [R] REEMPLAZAR '$Database' por una base nueva con el esquema de produccion (antes se respalda la antigua)"
    Write-Host '      [U] Usar OTRA base de la lista para este despliegue (no cambia ninguna base;'
    Write-Host "          el sistema de escritorio seguira usando '$Database' a menos que cambie su conexion)"
    Write-Host '      [S] Salir sin cambiar nada'
    switch ((Read-Host '    Opcion').Trim().ToUpper()) {
        'R' { return (Reemplazar-Base $bases) }
        'U' {
            $n = 0
            if (-not [int]::TryParse((Read-Host '    Numero de la base a usar').Trim(), [ref]$n) -or $n -lt 1 -or $n -gt $bases.Count) {
                Write-Host '    Opcion no valida.' -ForegroundColor Red; return $false
            }
            $elegida = $bases[$n - 1]
            if (-not $elegida.Produccion) { Write-Host "    '$($elegida.Nombre)' tampoco tiene el esquema de produccion." -ForegroundColor Red; return $false }
            $script:Database = $elegida.Nombre
            Aviso "Se continua con '$($script:Database)'. Recuerde: el sistema de escritorio usa la base de su App.config (conexionLocalBD)."
            return $true
        }
        default { Write-Host '    No se cambio nada.'; return $false }
    }
}

try {
    Write-Host "Base de datos: $Database   Instancia: $Instance   Usuario de Windows: $env:USERNAME" -ForegroundColor White

    # ---------- 1. LocalDB y existencia de la base ----------
    Paso '1/6  Comprobando SQL Server LocalDB'
    if (-not (Get-Command sqllocaldb -ErrorAction SilentlyContinue)) {
        Write-Host '    No se encontro LocalDB. Instale "SqlLocalDB.msi" (SQL Server Express, medios LocalDB) y repita.' -ForegroundColor Red
        Termina 1
    }
    & sqllocaldb start MSSQLLocalDB | Out-Null
    Ok 'LocalDB iniciado'

    if (-not (Existe-Base)) {
        Aviso "La base $Database NO existe para el usuario $env:USERNAME."
        Aviso 'Si el sistema ya funcionaba aqui, entre con el MISMO usuario de Windows que lo usa (LocalDB es por usuario).'
        if (-not $ScriptRespaldo) { $ScriptRespaldo = (Read-Host 'Ruta del respaldo en formato script .sql para restaurarla (Enter = salir)').Trim('" ') }
        if (-not $ScriptRespaldo) { Write-Host '    Sin base no se puede continuar.' -ForegroundColor Red; Termina 1 }
        if (-not (Test-Path $ScriptRespaldo)) { throw "No existe $ScriptRespaldo" }
        if ($SoloVerificar) { Aviso 'Con -SoloVerificar no se restaura nada.'; Termina 1 }
        Paso 'Restaurando la base desde el respaldo (puede tardar unos minutos)'
        & powershell.exe -NoProfile -ExecutionPolicy Bypass -File (Join-Path $db 'Restore-ProdCopy.ps1') `
            -ScriptPath $ScriptRespaldo -Database $Database -Instance $Instance
        if ($LASTEXITCODE -ne 0) { throw "Restore-ProdCopy.ps1 fallo (codigo $LASTEXITCODE)" }
        if (-not (Existe-Base)) { throw "La restauracion termino pero $Database no aparece." }
    }
    Ok "La base $Database existe"

    if (-not (Tiene-EsquemaProduccion)) {
        Write-Host ''
        Write-Host "    ALTO: la base '$Database' tiene el esquema ANTIGUO (la tabla Detalle_pedido no tiene Id_detalle_pedido)." -ForegroundColor Red
        Write-Host '    No es la base de produccion del restaurante y los procedimientos de la API no pueden crearse en ella.' -ForegroundColor Red
        Write-Host '    Hasta aqui no se hizo ningun cambio ni copia.' -ForegroundColor Red
        Write-Host ''
        Write-Host '    Revise: en el PC del restaurante use el MISMO usuario de Windows que usa el sistema (LocalDB es por usuario);'
        Write-Host '    en la maquina de desarrollo la copia de produccion se llama CasaGrandeBD_prod.'
        if (-not (Resolver-EsquemaAntiguo)) { Termina 1 }
        Ok "Se continua con la base '$Database'"
    }

    # ---------- 2. Estado actual ----------
    Paso '2/6  Estado actual (solo lectura)'
    Correr-Sql (Join-Path $db 'verificar-despliegue.sql') $Database
    if ($SoloVerificar) { Ok 'Verificacion terminada (no se cambio nada).'; Termina 0 }

    # ---------- 3. Copia de seguridad ----------
    Paso '3/6  Copia de seguridad'
    if (-not (Test-Path $CarpetaRespaldo)) { New-Item -ItemType Directory -Path $CarpetaRespaldo -Force | Out-Null }
    $bak = Join-Path $CarpetaRespaldo ("{0}_antes_de_la_API_{1:yyyyMMdd_HHmm}.bak" -f $Database, (Get-Date))
    $sqlBak = Join-Path $env:TEMP "respaldo_$([guid]::NewGuid().ToString('N')).sql"
    [IO.File]::WriteAllText($sqlBak, "BACKUP DATABASE [$Database] TO DISK = N'$bak' WITH INIT, CHECKSUM;`r`nGO`r`n")
    try { Correr-Sql $sqlBak 'master' } finally { Remove-Item $sqlBak -ErrorAction SilentlyContinue }
    if (-not (Test-Path $bak) -or (Get-Item $bak).Length -lt 1MB) { throw "No se creo una copia valida en $bak. NO se continua." }
    Ok ("Copia creada: {0}  ({1:N1} MB)" -f $bak, ((Get-Item $bak).Length / 1MB))
    Aviso 'Guarde ademas una copia de ese archivo en otro lugar (USB o nube privada).'

    # ---------- 4. Procedimientos nuevos ----------
    Paso '4/6  Procedimientos de la API movil (01, 03, 04)'
    Write-Host '    Crean 3 procedimientos NUEVOS; no cambian los existentes ni los datos. Se pueden repetir.'
    if (Pregunta '    Crear/actualizar los 3 procedimientos ahora?') {
        foreach ($n in '01_sp_Movil_Insertar_pedido.sql', '03_sp_Movil_Agregar_items.sql', '04_sp_Movil_Quitar_item.sql') {
            Write-Host "    -> $n"
            Correr-Sql (Join-Path $db $n) $Database
        }
        Ok 'Procedimientos listos'
    } else { Aviso 'Omitido. La API movil no podra crear ni modificar pedidos hasta que se ejecuten.' }

    # ---------- 5. Empleados ----------
    Paso '5/6  Empleados duplicados y PIN 0 (02)'
    Write-Host '    CAMBIA DATOS: deja una sola ficha con PIN por persona; las duplicadas quedan sin PIN e INACTIVAS (no se borran)'
    Write-Host '    y el PIN 0 pasa a "sin PIN". Es necesario porque el mesero entra solo con su PIN. Deja un respaldo en la'
    Write-Host '    tabla Empleados_respaldo_unificacion y se puede repetir sin riesgo. Confirmelo antes con el dueno.'
    if (Pregunta '    Ejecutar el arreglo de empleados ahora?') {
        Correr-Sql (Join-Path $db '02_unificar_empleados_duplicados.sql') $Database
        Ok 'Arreglo de empleados aplicado'
    } else { Aviso 'Omitido. Con PIN compartidos el celular mostrara un selector de persona.' }

    # ---------- 6. Verificacion final ----------
    Paso '6/6  Verificacion final (solo lectura)'
    Correr-Sql (Join-Path $db 'verificar-despliegue.sql') $Database
    Write-Host ''
    Write-Host '    Deben decir OK los tres procedimientos; pines_compartidos_entre_activos y activos_con_pin_cero deben ser 0.' -ForegroundColor White
    Write-Host "    Copia de seguridad: $bak" -ForegroundColor White
    Termina 0
}
catch {
    Write-Host "`nERROR: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host 'Si ya se hizo la copia de seguridad, la base sigue como estaba antes del paso que fallo.' -ForegroundColor Yellow
    Termina 1
}
