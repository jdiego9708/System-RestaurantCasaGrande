<#
    Prepara la base del restaurante para la API movil y Domicilios, con respaldo y confirmaciones.
    En la USB se llama "01-Base-de-Datos.ps1" (Preparar-USB.ps1 lo copia con ese nombre).

    EJECUTAR EN EL PC DEL RESTAURANTE, con el MISMO usuario de Windows que abre el sistema (LocalDB es por usuario):
    clic derecho > "Ejecutar con PowerShell". No necesita administrador.

        .\Desplegar-Base-de-Datos.ps1                       # flujo completo, con confirmaciones
        .\Desplegar-Base-de-Datos.ps1 -SoloVerificar        # solo lee: dice que falta, no cambia nada
        .\Desplegar-Base-de-Datos.ps1 -Database CasaGrandeBD_prod -CarpetaRespaldo D:\Respaldos

    Flujo:
      1. Comprueba LocalDB y que exista la base. Si NO existe (PC nuevo) siempre ofrece dos caminos:
           [1] RESTAURAR desde una copia (.bak o script .sql; busca las copias en la USB, C:\Respaldos, Escritorio y
               Documentos, o se escribe la ruta). Despues revisa que la copia traiga lo necesario (administrador con PIN,
               mesas, menu) y ofrece completarlo.
           [2] CREAR una base NUEVA y vacia (esquema + procedimientos + migraciones) y PEDIR el administrador (nombre y
               PIN de 4 digitos) y el numero de mesas.
      2. Muestra el estado actual (verificar-despliegue.sql).
      3. HACE UNA COPIA DE SEGURIDAD (.bak) de la base. Sin copia no continua.
      4. Con su confirmacion, crea/actualiza los 3 procedimientos nuevos (01, 03, 04). Son aditivos y repetibles.
      5. Con su confirmacion aparte, unifica empleados duplicados y PIN 0 (02). Deja respaldo dentro de la base.
      6. Aplica Domicilios (06): tabla, direccion del pedido y ocho procedimientos.
      7. Vuelve a verificar y resume (Domicilios incluido).
#>
[CmdletBinding()]
param(
    [string]$Database = 'CasaGrandeBD',
    [string]$Instance = '(localdb)\MSSQLLocalDB',
    [string]$CarpetaRespaldo = 'C:\Respaldos',
    # Copia (.bak o script .sql) para restaurar la base si no existe (opcional; si falta se pregunta).
    [Alias('ScriptRespaldo')]
    [string]$ArchivoCopia,
    [switch]$SoloVerificar,
    # Para automatizar (pruebas, instalaciones repetidas): si no se dan, el script pregunta.
    [ValidateSet('', 'Restaurar', 'Nueva')][string]$Origen = '',
    [string]$AdminNombre,
    [string]$AdminPin,
    [int]$CantidadMesas = -1,
    # Responde SI a todas las confirmaciones (no a las que piden escribir una palabra).
    [switch]$Si
)

$ErrorActionPreference = 'Stop'
# Preparar-USB reemplaza esta asignacion por los archivos de BD embebidos (Base64, sin datos de clientes).
$embeddedDatabaseFiles = @{} # USB_DATABASE_PAYLOAD
if ($Database -notmatch '^[A-Za-z0-9_]+$') { throw "Nombre de base no valido: $Database" }

function Paso($t)  { Write-Host "`n==> $t" -ForegroundColor Cyan }
function Ok($t)    { Write-Host "    OK  $t" -ForegroundColor Green }
function Aviso($t) { Write-Host "    !!  $t" -ForegroundColor Yellow }
function Termina($codigo) {
    Write-Host ''
    if (-not $Si) { Read-Host 'Pulse Enter para cerrar' | Out-Null }
    exit $codigo
}
function Pregunta([string]$texto) {
    if ($Si) { Write-Host "$texto (S/N): S (-Si)"; return $true }
    $r = Read-Host "$texto (S/N)"
    return ($r -match '^[sS]')
}

# ---------- Donde estan los scripts de la base ----------
$embeddedDb = $null
if ($embeddedDatabaseFiles.Count -gt 0) {
    $embeddedDb = Join-Path $env:TEMP ('CasaGrande-db-' + [guid]::NewGuid().ToString('N'))
    New-Item -ItemType Directory -Path $embeddedDb -Force | Out-Null
    foreach ($fileName in $embeddedDatabaseFiles.Keys) {
        if ([IO.Path]::GetFileName($fileName) -ne $fileName) { throw "Nombre de archivo embebido no valido: $fileName" }
        [IO.File]::WriteAllBytes((Join-Path $embeddedDb $fileName), [Convert]::FromBase64String($embeddedDatabaseFiles[$fileName]))
    }
}
$db = @($embeddedDb, (Join-Path $PSScriptRoot 'Soporte\db'), (Join-Path $PSScriptRoot '..\db')) |
    Where-Object { $_ } |
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

# ---------------------------------------------------------------------------
# Instalacion nueva: restaurar desde una copia o crear una base vacia, y revisar que la base tenga lo necesario
# ---------------------------------------------------------------------------
# Ejecuta SQL en la base de trabajo, con parametros. Sin -Tabla no devuelve nada; con -Tabla devuelve un DataTable.
function Sql-Base([string]$sql, [hashtable]$parametros = @{}, [switch]$Tabla) {
    Add-Type -AssemblyName System.Data
    $cn = New-Object System.Data.SqlClient.SqlConnection "Server=$Instance;Database=$Database;Integrated Security=true;Connect Timeout=30;Pooling=false"
    try {
        $cn.Open()
        $cmd = $cn.CreateCommand(); $cmd.CommandText = $sql; $cmd.CommandTimeout = 0
        foreach ($k in $parametros.Keys) { [void]$cmd.Parameters.AddWithValue($k, $parametros[$k]) }
        if ($Tabla) {
            $dt = New-Object System.Data.DataTable
            [void](New-Object System.Data.SqlClient.SqlDataAdapter $cmd).Fill($dt)
            return , $dt
        }
        [void]$cmd.ExecuteNonQuery()
    } finally { $cn.Dispose() }
}

# Copias (.bak o script .sql) que parezcan de la base del restaurante, de la mas reciente a la mas antigua.
function Buscar-Copias {
    $carpetas = @(
        (Join-Path $PSScriptRoot 'Copias'), (Join-Path $PSScriptRoot '..\Copias'), $CarpetaRespaldo,
        [Environment]::GetFolderPath('Desktop'), [Environment]::GetFolderPath('MyDocuments')
    ) | Where-Object { $_ -and (Test-Path $_) } | ForEach-Object { (Resolve-Path $_).Path } | Select-Object -Unique
    $lista = @()
    foreach ($c in $carpetas) {
        foreach ($f in (Get-ChildItem -LiteralPath $c -File -ErrorAction SilentlyContinue |
                Where-Object { $_.Extension -in '.bak', '.sql' -and $_.Length -gt 1MB })) {
            $lista += $f
        }
    }
    return @($lista | Sort-Object LastWriteTime -Descending)
}

function Pedir-Copia {
    $copias = Buscar-Copias
    Write-Host ''
    Write-Host '    Copias encontradas (USB\Copias, C:\Respaldos, Escritorio, Documentos):' -ForegroundColor White
    if ($copias.Count -eq 0) { Write-Host '      (ninguna)' -ForegroundColor Yellow }
    $i = 1
    foreach ($f in $copias) {
        Write-Host ('      [{0}] {1,-52} {2,8:N0} MB   {3:yyyy-MM-dd HH:mm}' -f $i, $f.Name, ($f.Length / 1MB), $f.LastWriteTime)
        Write-Host ('          {0}' -f $f.DirectoryName) -ForegroundColor DarkGray
        $i++
    }
    Write-Host '      [R] Escribir la ruta de otra copia   [0] Volver'
    $e = (Read-Host '    Numero de la copia').Trim().ToUpper()
    $n = 0
    if ([int]::TryParse($e, [ref]$n) -and $n -ge 1 -and $n -le $copias.Count) { return $copias[$n - 1].FullName }
    if ($e -eq 'R') { return (Read-Host '    Ruta de la copia (.bak o .sql)').Trim('" ') }
    return $null
}

# Restaura un respaldo .bak como $Database, dejando los archivos en la carpeta de datos de LocalDB.
function Restaurar-Bak([string]$ruta) {
    $ficheros = Sql-Master "RESTORE FILELISTONLY FROM DISK = $(Lit $ruta)" -Tabla
    $dir = [string](Sql-Master "SELECT CAST(SERVERPROPERTY('InstanceDefaultDataPath') AS nvarchar(260))" -Tabla).Rows[0][0]
    if ([string]::IsNullOrWhiteSpace($dir)) { $dir = $env:USERPROFILE }
    $moves = @(); $primero = $true
    foreach ($f in $ficheros.Rows) {
        if ($f.Type -eq 'L') { $ext = '.ldf' } elseif ($primero) { $ext = '.mdf'; $primero = $false } else { $ext = '.ndf' }
        $moves += "MOVE $(Lit $f.LogicalName) TO $(Lit (Join-Path $dir ('{0}_{1}{2}' -f $Database, $f.LogicalName, $ext)))"
    }
    Sql-Master "RESTORE DATABASE $(Q $Database) FROM DISK = $(Lit $ruta) WITH $($moves -join ', ')"
}

function Restaurar-Copia([string]$ruta) {
    if (-not $ruta -or -not (Test-Path -LiteralPath $ruta)) { throw "No existe la copia: $ruta" }
    Paso "Restaurando la copia $(Split-Path $ruta -Leaf) (puede tardar unos minutos)"
    if ([IO.Path]::GetExtension($ruta).ToLower() -eq '.bak') {
        Restaurar-Bak $ruta
    } else {
        & powershell.exe -NoProfile -ExecutionPolicy Bypass -File (Join-Path $db 'Restore-ProdCopy.ps1') `
            -ScriptPath $ruta -Database $Database -Instance $Instance
        if ($LASTEXITCODE -ne 0) { throw "Restore-ProdCopy.ps1 fallo (codigo $LASTEXITCODE)" }
    }
    if (-not (Existe-Base)) { throw "La restauracion termino pero $Database no aparece." }
    Ok "Copia restaurada como '$Database'"
}

# PIN de 4 digitos (el mesero entra con el): no vale 0000 ni uno ya usado por otra persona.
function Pedir-Pin {
    while ($true) {
        $pin = $AdminPin
        if (-not $pin) {
            $s1 = Read-Host '    PIN del administrador (4 digitos, no se muestra)' -AsSecureString
            $p1 = [Runtime.InteropServices.Marshal]::PtrToStringBSTR([Runtime.InteropServices.Marshal]::SecureStringToBSTR($s1))
            $s2 = Read-Host '    Repita el PIN' -AsSecureString
            $p2 = [Runtime.InteropServices.Marshal]::PtrToStringBSTR([Runtime.InteropServices.Marshal]::SecureStringToBSTR($s2))
            if ($p1 -cne $p2) { Write-Host '    Los dos PIN no coinciden. Intente de nuevo.' -ForegroundColor Yellow; continue }
            $pin = $p1
        }
        if ($pin -notmatch '^[1-9]\d{3}$') {
            if ($AdminPin) { throw 'AdminPin debe tener 4 digitos y no empezar en 0.' }
            Write-Host '    El PIN debe tener 4 digitos y no empezar en 0.' -ForegroundColor Yellow; continue
        }
        $usado = [int](Sql-Base 'SELECT COUNT(*) FROM Empleados WHERE Codigo_maestro = @p' @{ '@p' = [int]$pin } -Tabla).Rows[0][0]
        if ($usado -gt 0) {
            if ($AdminPin) { throw 'Ese PIN ya lo usa otra persona.' }
            Write-Host '    Ese PIN ya lo usa otra persona. Elija otro.' -ForegroundColor Yellow; continue
        }
        return $pin
    }
}

function Crear-Administrador {
    $nombre = $AdminNombre
    if (-not $nombre) {
        $nombre = (Read-Host '    Nombre del administrador (Enter = ADMINISTRADOR)').Trim()
        if (-not $nombre) { $nombre = 'ADMINISTRADOR' }
    }
    $nombre = $nombre.ToUpper()
    $pin = Pedir-Pin
    # La clave interna la usa el sistema para iniciar sesion tras validar el PIN: se genera al azar y no se muestra.
    $clave = -join ((48..57) + (65..90) | Get-Random -Count 12 | ForEach-Object { [char]$_ })
    Sql-Base @'
INSERT INTO Empleados (Nombre_empleado, Telefono_empleado, Identificacion_empleado, Correo_electronico,
                       Cargo_empleado, Password, Codigo_maestro, Estado)
VALUES (@nombre, '', '', '', 'ADMINISTRADOR', @clave, @pin, 'ACTIVO')
'@ @{ '@nombre' = $nombre; '@clave' = $clave; '@pin' = [int]$pin }
    Ok "Administrador '$nombre' creado (entra con su PIN de 4 digitos)"
}

function Crear-Mesas {
    $n = $CantidadMesas
    if ($n -lt 0) {
        $t = (Read-Host '    Cuantas mesas tiene el restaurante? (0 a 200, Enter = 10)').Trim()
        $n = if ($t) { 0 } else { 10 }
        if ($t -and -not [int]::TryParse($t, [ref]$n)) { $n = -1 }
    }
    while ($n -lt 0 -or $n -gt 200) {
        if ($CantidadMesas -ge 0) { throw 'CantidadMesas debe estar entre 0 y 200.' }
        $t = (Read-Host '    Numero no valido. Cuantas mesas (0 a 200)?').Trim()
        if (-not [int]::TryParse($t, [ref]$n)) { $n = -1 }
    }
    if ($n -eq 0) { Aviso 'Sin mesas: se pueden crear despues desde el sistema.'; return }
    Sql-Base @'
DECLARE @i int = 1;
WHILE @i <= @n
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Mesas WHERE Num_mesa = @i AND Id_mesa > 0)
        INSERT INTO Mesas (Num_mesa, Descripcion_mesa) VALUES (@i, '');
    SET @i += 1;
END
'@ @{ '@n' = $n }
    Ok "$n mesas creadas (numeradas de 1 a $n)"
}

# Crea una base vacia con el esquema de produccion y la deja lista para empezar: tipos "OTRO", administrador y mesas.
function Crear-Base-Nueva {
    $esquema = Join-Path $db 'produccion-schema.sql'
    if (-not (Test-Path $esquema)) { throw "Falta $esquema" }
    Paso "Creando la base NUEVA '$Database' (esquema de produccion, sin datos)"
    # El script de esquema empieza con USE [CasaGrandeBD]: se quita para que NO escriba en otra base.
    $texto = [IO.File]::ReadAllText($esquema, [Text.Encoding]::UTF8)
    $quitarUse = New-Object System.Text.RegularExpressions.Regex '(?m)^USE \[[^\]]+\]\r?\n'
    $texto = $quitarUse.Replace($texto, '', 1)
    $tmpSql = Join-Path $env:TEMP "esquema_$([guid]::NewGuid().ToString('N')).sql"
    [IO.File]::WriteAllText($tmpSql, $texto, (New-Object Text.UTF8Encoding $true))
    try {
        Sql-Master "CREATE DATABASE $(Q $Database)"
        Correr-Sql $tmpSql $Database
    } finally { Remove-Item $tmpSql -ErrorAction SilentlyContinue }
    if (-not (Existe-Base) -or -not (Tiene-EsquemaProduccion)) { throw "La base $Database no quedo con el esquema de produccion." }
    Ok 'Esquema creado'

    # Los platos y bebidas sin tipo usan el tipo 0 "OTRO".
    Sql-Base @'
SET IDENTITY_INSERT Tipo_platos ON;
IF NOT EXISTS (SELECT 1 FROM Tipo_platos WHERE Id_tipo_plato = 0) INSERT INTO Tipo_platos (Id_tipo_plato, Tipo_plato) VALUES (0, 'OTRO');
SET IDENTITY_INSERT Tipo_platos OFF;
SET IDENTITY_INSERT Tipo_bebidas ON;
IF NOT EXISTS (SELECT 1 FROM Tipo_bebidas WHERE Id_tipo_bebida = 0) INSERT INTO Tipo_bebidas (Id_tipo_bebida, Tipo_bebida) VALUES (0, 'OTRO');
SET IDENTITY_INSERT Tipo_bebidas OFF;
'@
    Write-Host ''
    Write-Host '    Datos minimos para empezar a trabajar:' -ForegroundColor White
    Crear-Administrador
    Crear-Mesas
    Aviso 'La base queda SIN platos, bebidas ni clientes: se cargan desde el sistema (Platos / Bebidas).'
}

# El PC es nuevo: SIEMPRE se ofrecen las dos opciones (copia o base nueva). Termina el script si se cancela.
function Resolver-BaseInexistente {
    $modo = $Origen
    $ruta = $ArchivoCopia
    if (-not $modo) {
        Write-Host ''
        Write-Host '    INSTALACION NUEVA: en este equipo (y para este usuario de Windows) no hay base de datos del restaurante.' -ForegroundColor White
        Write-Host '    Que desea hacer?' -ForegroundColor White
        Write-Host '      [1] RESTAURAR desde una COPIA de la base del restaurante (.bak o script .sql) -- trae sus empleados,'
        Write-Host '          mesas, platos y bebidas. Es lo recomendado si ya hay una copia.'
        Write-Host '      [2] Crear una base NUEVA y VACIA: se piden el administrador (nombre y PIN) y el numero de mesas;'
        Write-Host '          el menu (platos y bebidas) se carga despues desde el sistema.'
        Write-Host '      [0] Salir'
        switch ((Read-Host '    Opcion').Trim()) {
            '1' { $modo = 'Restaurar' }
            '2' { $modo = 'Nueva' }
            default { Write-Host '    No se hizo ningun cambio.'; Termina 1 }
        }
    }
    if ($modo -eq 'Restaurar') {
        if (-not $ruta) { $ruta = Pedir-Copia }
        if (-not $ruta) { Write-Host '    Sin copia no se restaura nada.'; Termina 1 }
        Restaurar-Copia $ruta
    } else {
        Crear-Base-Nueva
    }
}

# Resume lo que tiene la base y avisa lo que le falta para poder trabajar. Con -Reparar ofrece completarlo.
function Revisar-Contenido([switch]$Reparar) {
    $c = Sql-Base @'
SELECT
 (SELECT COUNT(*) FROM Empleados WHERE Id_empleado > 0 AND Estado = 'ACTIVO') AS empleados,
 (SELECT COUNT(*) FROM Empleados WHERE Id_empleado > 0 AND Estado = 'ACTIVO' AND Codigo_maestro > 0) AS con_pin,
 (SELECT COUNT(*) FROM Empleados WHERE Id_empleado > 0 AND Estado = 'ACTIVO' AND Cargo_empleado = 'ADMINISTRADOR' AND Codigo_maestro > 0) AS admins,
 (SELECT COUNT(*) FROM Mesas WHERE Id_mesa > 0) AS mesas,
 (SELECT COUNT(*) FROM Platos WHERE Estado = 'ACTIVO') AS platos,
 (SELECT COUNT(*) FROM Bebidas WHERE Estado = 'ACTIVO') AS bebidas,
 (SELECT COUNT(*) FROM Clientes) AS clientes,
 (SELECT COUNT(*) FROM Ventas) AS ventas
'@ @{} -Tabla
    $r = $c.Rows[0]
    Write-Host ''
    Write-Host '    Contenido de la base:' -ForegroundColor White
    Write-Host ('      Empleados activos: {0} ({1} con PIN, {2} administradores con PIN)' -f $r.empleados, $r.con_pin, $r.admins)
    Write-Host ('      Mesas: {0}   Platos activos: {1}   Bebidas activas: {2}   Clientes: {3}   Ventas: {4}' -f $r.mesas, $r.platos, $r.bebidas, $r.clientes, $r.ventas)
    if ($r.admins -eq 0) {
        Aviso 'No hay ningun ADMINISTRADOR activo con PIN: nadie podria administrar el sistema.'
        if ($Reparar -and (Pregunta '    Crear un administrador ahora?')) { Crear-Administrador }
    }
    if ($r.mesas -eq 0) {
        Aviso 'No hay mesas: no se podran tomar pedidos de mesa.'
        if ($Reparar -and (Pregunta '    Crear las mesas ahora?')) { Crear-Mesas }
    }
    if ($r.platos -eq 0 -and $r.bebidas -eq 0) { Aviso 'El menu esta vacio (sin platos ni bebidas activos): se carga desde el sistema.' }
    elseif ($r.platos -eq 0) { Aviso 'No hay platos activos.' }
    elseif ($r.bebidas -eq 0) { Aviso 'No hay bebidas activas.' }
}

# ---------------------------------------------------------------------------
# LocalDB: windows 11 no lo trae. Si falta, se instala solo desde Requisitos\SqlLocalDB.msi de la USB.
# ---------------------------------------------------------------------------
function Agregar-LocalDb-Al-Path {
    foreach ($d in @(Get-ChildItem 'C:\Program Files\Microsoft SQL Server' -Directory -ErrorAction SilentlyContinue | Sort-Object Name -Descending)) {
        $bin = Join-Path $d.FullName 'Tools\Binn'
        if ((Test-Path (Join-Path $bin 'SqlLocalDB.exe')) -and ($env:Path -notlike "*$bin*")) { $env:Path += ";$bin" }
    }
}

function Instalar-LocalDb {
    $msi = @((Join-Path $PSScriptRoot 'Requisitos\SqlLocalDB.msi'), (Join-Path $PSScriptRoot '..\Requisitos\SqlLocalDB.msi')) |
        Where-Object { Test-Path $_ } | Select-Object -First 1
    if (-not $msi) { return }
    $msi = (Resolve-Path $msi).Path
    # Solo se instala un MSI firmado por Microsoft.
    $firma = Get-AuthenticodeSignature -LiteralPath $msi
    if ($firma.Status -ne 'Valid' -or $firma.SignerCertificate.Subject -notmatch 'O=Microsoft Corporation') {
        throw "El instalador de LocalDB no tiene una firma valida de Microsoft ($($firma.Status)). No se instala."
    }
    Write-Host '    No hay LocalDB en este equipo (Windows 11 no lo trae): se instala solo desde Requisitos\SqlLocalDB.msi.' -ForegroundColor White
    $admin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
    if (-not $admin) { Write-Host '    Windows pedira permiso de administrador SOLO para instalar LocalDB; el resto sigue con su usuario.' -ForegroundColor Yellow }
    $log = Join-Path $env:TEMP 'CasaGrande-LocalDB-install.log'
    $argumentos = "/i `"$msi`" /qn /norestart IACCEPTSQLLOCALDBLICENSETERMS=YES /l*v `"$log`""
    # No se usa -Wait: esperaria tambien al servicio de Windows Installer (hijo de msiexec) y no terminaria en minutos.
    try {
        if ($admin) { $p = Start-Process msiexec.exe -ArgumentList $argumentos -PassThru }
        else { $p = Start-Process msiexec.exe -ArgumentList $argumentos -Verb RunAs -PassThru }
        $null = $p.Handle
        $p.WaitForExit()
    } catch {
        throw "No se pudo instalar LocalDB (permiso de administrador denegado?): $($_.Exception.Message)"
    }
    # El resultado se comprueba por lo que quedo instalado (el codigo de salida de un proceso elevado no siempre se puede leer).
    Agregar-LocalDb-Al-Path
    if (-not (Get-Command sqllocaldb -ErrorAction SilentlyContinue)) {
        $motivo = ''
        if ((Test-Path $log) -and (Select-String -Path $log -Pattern 'administrator group' -Quiet)) {
            $motivo = ' Windows no concedio permisos de administrador: acepte el aviso con una cuenta de administrador.'
        }
        throw "La instalacion de LocalDB no termino bien.$motivo Registro: $log"
    }
    Ok 'LocalDB instalado'
}

try {
    Write-Host "Base de datos: $Database   Instancia: $Instance   Usuario de Windows: $env:USERNAME" -ForegroundColor White

    # ---------- 1. LocalDB y existencia de la base ----------
    Paso '1/7  Comprobando SQL Server LocalDB'
    # LocalDB puede estar instalado y solo faltar en el PATH de esta ventana (se acaba de instalar, p. ej.).
    if (-not (Get-Command sqllocaldb -ErrorAction SilentlyContinue)) { Agregar-LocalDb-Al-Path }
    if (-not (Get-Command sqllocaldb -ErrorAction SilentlyContinue)) { Instalar-LocalDb }
    if (-not (Get-Command sqllocaldb -ErrorAction SilentlyContinue)) {
        Write-Host '    No se encontro SQL Server Express LocalDB (Windows 11 no lo trae) y la USB no trae Requisitos\SqlLocalDB.msi.' -ForegroundColor Red
        Write-Host '    Instale "SqlLocalDB.msi" (SQL Server Express, medios LocalDB) y repita.' -ForegroundColor Red
        Termina 1
    }
    & sqllocaldb start MSSQLLocalDB | Out-Null
    Ok 'LocalDB iniciado'

    if (-not (Existe-Base)) {
        Aviso "La base $Database NO existe para el usuario $env:USERNAME."
        Aviso 'Si el sistema ya funcionaba aqui, entre con el MISMO usuario de Windows que lo usa (LocalDB es por usuario).'
        if ($SoloVerificar) { Aviso 'Con -SoloVerificar no se restaura ni se crea nada.'; Termina 1 }
        Resolver-BaseInexistente
        if (-not (Existe-Base)) { throw "La base $Database no aparece despues del paso anterior." }
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
    Paso '2/7  Estado actual (solo lectura)'
    Correr-Sql (Join-Path $db 'verificar-despliegue.sql') $Database
    Revisar-Contenido
    if ($SoloVerificar) { Ok 'Verificacion terminada (no se cambio nada).'; Termina 0 }

    # ---------- 3. Copia de seguridad ----------
    Paso '3/7  Copia de seguridad'
    if (-not (Test-Path $CarpetaRespaldo)) { New-Item -ItemType Directory -Path $CarpetaRespaldo -Force | Out-Null }
    $bak = Join-Path $CarpetaRespaldo ("{0}_antes_de_la_API_{1:yyyyMMdd_HHmm}.bak" -f $Database, (Get-Date))
    $sqlBak = Join-Path $env:TEMP "respaldo_$([guid]::NewGuid().ToString('N')).sql"
    [IO.File]::WriteAllText($sqlBak, "BACKUP DATABASE [$Database] TO DISK = N'$bak' WITH INIT, CHECKSUM;`r`nGO`r`n")
    try { Correr-Sql $sqlBak 'master' } finally { Remove-Item $sqlBak -ErrorAction SilentlyContinue }
    if (-not (Test-Path $bak) -or (Get-Item $bak).Length -lt 1MB) { throw "No se creo una copia valida en $bak. NO se continua." }
    Ok ("Copia creada: {0}  ({1:N1} MB)" -f $bak, ((Get-Item $bak).Length / 1MB))
    Aviso 'Guarde ademas una copia de ese archivo en otro lugar (USB o nube privada).'

    # ---------- 4. Procedimientos nuevos ----------
    Paso '4/7  Procedimientos de la API movil (01, 03, 04)'
    Write-Host '    Crean 3 procedimientos NUEVOS; no cambian los existentes ni los datos. Se pueden repetir.'
    if (Pregunta '    Crear/actualizar los 3 procedimientos ahora?') {
        foreach ($n in '01_sp_Movil_Insertar_pedido.sql', '03_sp_Movil_Agregar_items.sql', '04_sp_Movil_Quitar_item.sql') {
            Write-Host "    -> $n"
            Correr-Sql (Join-Path $db $n) $Database
        }
        Ok 'Procedimientos listos'
    } else { Aviso 'Omitido. La API movil no podra crear ni modificar pedidos hasta que se ejecuten.' }

    # ---------- 5. Empleados ----------
    Paso '5/7  Empleados duplicados y PIN 0 (02)'
    Write-Host '    CAMBIA DATOS: deja una sola ficha con PIN por persona; las duplicadas quedan sin PIN e INACTIVAS (no se borran)'
    Write-Host '    y el PIN 0 pasa a "sin PIN". Es necesario porque el mesero entra solo con su PIN. Deja un respaldo en la'
    Write-Host '    tabla Empleados_respaldo_unificacion y se puede repetir sin riesgo. Confirmelo antes con el dueno.'
    if (Pregunta '    Ejecutar el arreglo de empleados ahora?') {
        Correr-Sql (Join-Path $db '02_unificar_empleados_duplicados.sql') $Database
        Ok 'Arreglo de empleados aplicado'
    } else { Aviso 'Omitido. Con PIN compartidos el celular mostrara un selector de persona.' }

    # El login del escritorio solo muestra empleados ACTIVO: estas dos personas deben estarlo para poder entrar.
    Write-Host ''
    Write-Host '    Login del escritorio: solo se listan empleados ACTIVO. Este paso deja ACTIVO a JUAN DIEGO DUQUE y a'
    Write-Host '    ANDRES DAVID HOYOS GARCIA (05). Solo cambia su Estado, no su PIN; se puede repetir y se revierte con el .revert.'
    if (Pregunta '    Activar a esos dos empleados ahora?') {
        Correr-Sql (Join-Path $db '05_activar_empleados.sql') $Database
        Ok 'Empleados activados'
    } else { Aviso 'Omitido. Si siguen INACTIVO no aparecen en el login del escritorio.' }

    # Modulo de Domicilios: varias direcciones por cliente y direccion de entrega en cada pedido.
    Paso '6/7  Domicilios, clientes y direcciones (06)'
    Write-Host ''
    Write-Host '    Domicilios (06): crea la tabla Domicilios (varias direcciones por cliente), pasa la direccion que ya tiene'
    Write-Host '    cada cliente como su principal y agrega Pedidos.Id_direccion. Reemplaza sp_Insertar_pedido por una version'
    Write-Host '    con el mismo comportamiento (la direccion es opcional). Se puede repetir; se revierte con el .revert.'
    if (Pregunta '    Aplicar el esquema de Domicilios ahora?') {
        Correr-Sql (Join-Path $db '06_domicilios_direcciones.sql') $Database
        Ok 'Esquema de Domicilios aplicado'
    } else { Aviso 'Omitido. El modulo de Domicilios del escritorio no funcionara hasta aplicarlo.' }

    Write-Host ''
    Write-Host '    Alegra (07): requisito de esta version. Prepara las solicitudes de factura electronica y su seguimiento.'
    Write-Host '    Se aplica siempre; es repetible, no envia documentos ni guarda credenciales de Alegra.'
    Correr-Sql (Join-Path $db '07_alegra_integration.sql') $Database
    Ok 'Tablas de integracion Alegra preparadas'
    Write-Host '    Configure credenciales, cuenta e impresion con 05-Configurar-Alegra.ps1 despues de instalar el programa.'
    Write-Host '    Este paso prepara SQL y no necesita tokens ni conexion con Alegra. Mantenga el escritorio cerrado hasta terminar 05.'

    Write-Host ''
    Write-Host '    Productos eliminados (08): actualiza sp_Movil_Quitar_item para que el movil deje en Delete_pedidos quien'
    Write-Host '    autorizo, cuando y el motivo. Se aplica siempre; es repetible y no borra datos.'
    Correr-Sql (Join-Path $db '08_items_eliminados.sql') $Database
    Ok 'Registro de productos eliminados preparado'

    Write-Host ''
    Write-Host '    Forma de pago de los domicilios (09): agrega Pedidos.Payment_method para que el domicilio se facture solo'
    Write-Host '    al cumplirse el tiempo. Se aplica siempre; es repetible y no borra datos.'
    Correr-Sql (Join-Path $db '09_pedidos_forma_pago.sql') $Database
    Ok 'Forma de pago de domicilios preparada'

    Write-Host ''
    Write-Host '    Pedidos de Rappi (10): crea la tabla RappiOrders donde se guardan las facturas PDF de Rappi que lee el'
    Write-Host '    escritorio (una ficha por orden, sin duplicados). Se aplica siempre; es repetible y no borra datos.'
    Correr-Sql (Join-Path $db '10_rappi_ordenes.sql') $Database
    Ok 'Tabla de pedidos de Rappi preparada'

    Write-Host ''
    Write-Host '    Numeracion diaria de domicilios (11): agrega el numero de factura del dia (1..N) a los domicilios y a las'
    Write-Host '    ordenes de Rappi, cada uno con su propia secuencia. Numera lo que ya existe. Se aplica siempre; es'
    Write-Host '    repetible y no borra datos.'
    Correr-Sql (Join-Path $db '11_numeracion_domicilios.sql') $Database
    Ok 'Numeracion diaria de domicilios preparada'

    Write-Host ''
    Write-Host '    Numeracion de ventas (12): guarda el consecutivo diario al registrar cada venta del programa.'
    Write-Host '    Conserva el numero en las reimpresiones. No reconstruye numeros de facturas antiguas.'
    Correr-Sql (Join-Path $db '12_numeracion_ventas.sql') $Database
    Ok 'Numeracion persistente de ventas preparada'

    # ---------- 6. Verificacion final ----------
    Paso '7/7  Verificacion final (solo lectura)'
    Correr-Sql (Join-Path $db 'verificar-despliegue.sql') $Database
    Write-Host ''
    Write-Host '    Deben decir OK los tres procedimientos; pines_compartidos_entre_activos y activos_con_pin_cero deben ser 0.' -ForegroundColor White
    Write-Host '    Domicilios debe decir OK en tabla, columna, relaciones, mesa 0 y los ocho procedimientos.' -ForegroundColor White
    Write-Host '    Alegra debe decir OK en las cuatro tablas y en las columnas de procesamiento de solicitudes.' -ForegroundColor White
    Write-Host "    Copia de seguridad: $bak" -ForegroundColor White
    # Al final (ya con copia hecha y migraciones aplicadas): si la base no trae administrador o mesas, se ofrece completarlo.
    Paso 'Revision final del contenido'
    Revisar-Contenido -Reparar
    Termina 0
}
catch {
    Write-Host "`nERROR: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host 'Si ya se hizo la copia de seguridad, los pasos anteriores pueden estar aplicados. Revise el error antes de repetir; el respaldo permite restaurar.' -ForegroundColor Yellow
    Termina 1
}
