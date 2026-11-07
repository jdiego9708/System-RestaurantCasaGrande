<#
    Prepara una USB con todo lo necesario para instalar Casa Grande en el restaurante.

    Muestra las unidades USB / extraibles conectadas, usted elige una de la lista y el script crea en ella la
    carpeta "Casa Grande Installers" con los archivos NUMERADOS en el orden en que se ejecutan:

        00-LEEME.txt                    instrucciones cortas con ese orden
        01-Base-de-Datos.ps1            Autocontenido: respaldo, API movil, empleados y Domicilios
        02-Setup-Casa-Grande.exe        PC del restaurante: instalador del sistema (activa la API movil)
        03-Configurar-Red.ps1           PC del restaurante: IP, firewall y datos que se escriben en los celulares
        04-APK-Casa-Grande.apk          Cada celular Android: app del mesero
        05-Configurar-Alegra.ps1        PC: importa tokens y prepara cuenta e impresion
        06-Cambiar-Codigo-Maestro.ps1   PC: cambia el codigo maestro de 8 digitos cuando se requiera
        07-Configurar-Impresora-Termica.ps1   PC: deja la POS-80 en el formato de rollo largo (tickets largos sin cortar)
        Credenciales-Alegra\           Tokens renombrados para el PC destino
        Soporte\                        copia legible de scripts SQL, auxiliares y reversiones
        SHA256SUMS.txt                  sumas para comprobar que la copia no se daño

    Uso (en la maquina de desarrollo, no requiere administrador):
        .\Preparar-USB.ps1                      # lista las USB y pregunta cual preparar
        .\Preparar-USB.ps1 -Unidad K            # salta la lista (K debe ser una unidad USB/extraible)
        (siempre recompila el instalador .exe y el APK antes de copiar, sin necesidad de -Compilar)
        .\Preparar-USB.ps1 -SinCompilar         # NO recompila: copia lo que ya esta construido (puede ser una version vieja)
        .\Preparar-USB.ps1 -Version 1.1.0       # version del instalador a copiar
        .\Preparar-USB.ps1 -CarpetaDestino D:\Temp\kit   # NO USB: usa esa carpeta como raiz (pruebas, carpeta de red)
        .\Preparar-USB.ps1 -Si                  # no pide la confirmacion final

    Solo escribe dentro de "<unidad>\Casa Grande Installers". No formatea ni borra nada fuera de esa carpeta.
#>
[CmdletBinding()]
param(
    [string]$Unidad,
    [string]$CarpetaDestino,
    [string]$Version = '1.0.0',
    # Ya no hace falta: compilar es el comportamiento por defecto. Se acepta para no romper comandos viejos.
    [switch]$Compilar,
    # Copia lo que ya esté construido, sin recompilar el instalador ni el APK (pruebas del propio script).
    [switch]$SinCompilar,
    [switch]$Si,
    [string]$ApkRuta,
    [string]$FlutterBin = 'D:\DevTools\flutter-3.47.8\bin',
    [string]$AlegraBearerFile = 'D:\Setup\apikeyAlegraCasaGrandeNuevo.txt',
    [string]$AlegraBasicFile = 'D:\Setup\apikeyAlegraCasaGrandeViejo.txt'
)

$ErrorActionPreference = 'Stop'
$NombreCarpeta = 'Casa Grande Installers'

function Paso($t)  { Write-Host "`n==> $t" -ForegroundColor Cyan }
function Ok($t)    { Write-Host "    OK  $t" -ForegroundColor Green }
function Aviso($t) { Write-Host "    !!  $t" -ForegroundColor Yellow }
function Falla($t) { Write-Host "`nERROR: $t" -ForegroundColor Red; exit 1 }

function New-DatabaseDeploymentScript([string]$source, [string]$databaseDir, [string[]]$files, [string]$output) {
    $content = [IO.File]::ReadAllText($source)
    $marker = '$embeddedDatabaseFiles = @{} # USB_DATABASE_PAYLOAD'
    if (-not $content.Contains($marker)) { throw 'No se encontro el marcador para embeber los scripts de base de datos.' }
    $payload = New-Object System.Collections.Generic.List[string]
    $payload.Add('$embeddedDatabaseFiles = @{')
    foreach ($name in $files) {
        if ([IO.Path]::GetFileName($name) -ne $name) { throw "Nombre de soporte no valido: $name" }
        $base64 = [Convert]::ToBase64String([IO.File]::ReadAllBytes((Join-Path $databaseDir $name)))
        $payload.Add("    '$name' = '$base64'")
    }
    $payload.Add('} # USB_DATABASE_PAYLOAD')
    [IO.File]::WriteAllText($output, $content.Replace($marker, ($payload -join "`r`n")), (New-Object Text.UTF8Encoding $true))
}

function New-AlegraDeploymentScript([string]$source, [hashtable]$support, [string]$output) {
    $content=[IO.File]::ReadAllText($source)
    $marker='$embeddedAlegraFiles = @{} # USB_ALEGRA_PAYLOAD'
    if(!$content.Contains($marker)) { throw 'Falta el marcador del configurador Alegra.' }
    $payload=New-Object 'System.Collections.Generic.List[string]'
    $payload.Add('$embeddedAlegraFiles = @{')
    foreach($name in ($support.Keys | Sort-Object)) {
        if([IO.Path]::GetFileName($name) -ne $name) { throw 'Nombre de soporte Alegra invalido.' }
        $data=[Convert]::ToBase64String([IO.File]::ReadAllBytes($support[$name]))
        $payload.Add("    '$name' = '$data'")
    }
    $payload.Add('} # USB_ALEGRA_PAYLOAD')
    [IO.File]::WriteAllText($output,$content.Replace($marker,($payload -join "`r`n")),(New-Object Text.UTF8Encoding $true))
}

# ---------------------------------------------------------------------------
# Unidades USB / extraibles
# ---------------------------------------------------------------------------
function Obtener-Unidades {
    $letras = @{}
    # Memorias USB y tarjetas: Windows las marca como "Removable".
    Get-Volume | Where-Object { $_.DriveLetter -and $_.DriveType -eq 'Removable' } |
        ForEach-Object { $letras[[string]$_.DriveLetter] = $true }
    # Discos duros externos suelen aparecer como "Fixed": se detectan por el bus USB del disco.
    foreach ($d in (Get-Disk -ErrorAction SilentlyContinue | Where-Object { $_.BusType -eq 'USB' })) {
        Get-Partition -DiskNumber $d.Number -ErrorAction SilentlyContinue | Where-Object { $_.DriveLetter } |
            ForEach-Object { $letras[[string]$_.DriveLetter] = $true }
    }
    $sistema = $env:SystemDrive.TrimEnd(':')
    $lista = @()
    foreach ($l in ($letras.Keys | Sort-Object)) {
        if ($l -eq $sistema) { continue }     # nunca la unidad de Windows
        $v = Get-Volume -DriveLetter $l -ErrorAction SilentlyContinue
        if (-not $v -or -not $v.FileSystem) { continue }   # sin formato o sin medio
        $lista += [pscustomobject]@{
            Unidad    = "${l}:"
            Etiqueta  = if ($v.FileSystemLabel) { $v.FileSystemLabel } else { '(sin nombre)' }
            Sistema   = $v.FileSystem
            TamanoGB  = [math]::Round($v.Size / 1GB, 1)
            LibreGB   = [math]::Round($v.SizeRemaining / 1GB, 1)
            LibreBytes = [int64]$v.SizeRemaining
        }
    }
    return , $lista
}

function Elegir-Unidad($unidades) {
    Write-Host ''
    Write-Host ' Unidades USB / extraibles disponibles:' -ForegroundColor White
    Write-Host ''
    $i = 1
    foreach ($u in $unidades) {
        Write-Host ('  [{0}]  {1}  {2,-22} {3,-6} {4,7} GB  ({5} GB libres)' -f $i, $u.Unidad, $u.Etiqueta, $u.Sistema, $u.TamanoGB, $u.LibreGB)
        $i++
    }
    Write-Host ''
    $r = Read-Host 'Escriba el numero de la USB a preparar (Enter para cancelar)'
    if ([string]::IsNullOrWhiteSpace($r)) { return $null }
    $n = 0
    if (-not [int]::TryParse($r.Trim(), [ref]$n) -or $n -lt 1 -or $n -gt $unidades.Count) {
        Write-Host "    Opcion no valida: '$r'" -ForegroundColor Red
        return $null
    }
    return $unidades[$n - 1]
}

# Permite cargar las funciones sin ejecutar el script (". .\Preparar-USB.ps1") para probarlas.
if ($MyInvocation.InvocationName -eq '.') { return }

# ---------------------------------------------------------------------------
# 1. Archivos de origen
# ---------------------------------------------------------------------------
$scriptsDir = $PSScriptRoot
$repo = Split-Path $scriptsDir -Parent
$mesero = Join-Path (Split-Path $repo -Parent) 'CasaGrandeMesero'
$setupNombre = "SISRestaurante-Setup-$Version.exe"
$setupOrigen = Join-Path $repo "installer\Output\$setupNombre"
if (-not $ApkRuta) { $ApkRuta = Join-Path $mesero 'app\build\app\outputs\flutter-apk\app-release.apk' }

Paso '1/5  Comprobando los archivos de origen'
foreach($credential in @($AlegraBearerFile,$AlegraBasicFile)) {
    if(!(Test-Path -LiteralPath $credential -PathType Leaf) -or (Get-Item -LiteralPath $credential).Length -eq 0) {
        Falla "No existe el TXT de credencial o esta vacio: $credential"
    }
}


# Por defecto SIEMPRE se recompilan el instalador y el APK: la USB nunca debe llevar una version vieja.
# Solo -SinCompilar lo evita (y aun asi compila el instalador si no existe).
$compilarTodo = -not $SinCompilar
$apkPropio = $PSBoundParameters.ContainsKey('ApkRuta')   # con -ApkRuta se usa ese APK tal cual

if ($compilarTodo -or -not (Test-Path $setupOrigen)) {
    if (-not $compilarTodo) { Aviso "No existe ${setupNombre}: se compila ahora." }
    & (Join-Path $repo 'installer\Publicar-Instalador.ps1') -Version $Version
    if ($LASTEXITCODE -ne 0) { Falla 'No se pudo generar el instalador.' }
}
if (-not (Test-Path $setupOrigen)) { Falla "No existe el instalador $setupOrigen" }

if ($compilarTodo -and -not $apkPropio) {
    $flutter = Join-Path $FlutterBin 'flutter.bat'
    if (-not (Test-Path $flutter)) { Falla "No encuentro Flutter 3.47.8 en $FlutterBin (use -FlutterBin). El flutter del PATH suele ser el 3.11.5 y no sirve." }
    Push-Location (Join-Path $mesero 'app')
    try {
        $previousFlutterPath=$env:Path
        $env:Path = "$FlutterBin;$env:Path"
        $previousNativePreference=$ErrorActionPreference
        try {
            $ErrorActionPreference='Continue'
            & $flutter build apk --release 2>&1 | ForEach-Object { Write-Host $_.ToString() }
            $flutterExit=$LASTEXITCODE
        } finally { $ErrorActionPreference=$previousNativePreference }
        if ($flutterExit -ne 0) { Falla 'flutter build apk fallo.' }
    } finally { if($null -ne $previousFlutterPath) { $env:Path=$previousFlutterPath }; Pop-Location }
}
if (-not (Test-Path $ApkRuta)) { Falla "No existe el APK $ApkRuta. Repita sin -SinCompilar (compila con Flutter 3.47.8)." }

# Archivos de soporte del paso 01 (base de datos)
$dbDir = Join-Path $repo 'db'
$soporteDb = @('Invoke-Sql.ps1', 'Restore-ProdCopy.ps1', 'verificar-despliegue.sql', 'produccion-schema.sql',
               '01_sp_Movil_Insertar_pedido.sql', '03_sp_Movil_Agregar_items.sql', '04_sp_Movil_Quitar_item.sql',
               '02_unificar_empleados_duplicados.sql', '05_activar_empleados.sql',
               '06_domicilios_direcciones.sql', '07_alegra_integration.sql', '08_items_eliminados.sql', 'Configurar-Alegra.ps1',
               '01_sp_Movil_Insertar_pedido.revert.sql', '03_sp_Movil_Agregar_items.revert.sql',
               '04_sp_Movil_Quitar_item.revert.sql', '02_unificar_empleados_duplicados.revert.sql',
               '05_activar_empleados.revert.sql', '06_domicilios_direcciones.revert.sql', '08_items_eliminados.revert.sql')

# El paso 01 es autocontenido; Soporte\db se conserva para consulta y ejecucion manual.
foreach ($f in $soporteDb) { if (-not (Test-Path (Join-Path $dbDir $f))) { Falla "Falta db\$f" } }
$databaseScript = Join-Path $env:TEMP ('CasaGrande-01-' + [guid]::NewGuid().ToString('N') + '.ps1')
New-DatabaseDeploymentScript (Join-Path $scriptsDir 'Desplegar-Base-de-Datos.ps1') $dbDir $soporteDb $databaseScript
$alegraScript=Join-Path $env:TEMP ('CasaGrande-05-'+[guid]::NewGuid().ToString('N')+'.ps1')
New-AlegraDeploymentScript (Join-Path $scriptsDir 'Configurar-Alegra-Equipo.ps1') @{
    'Configurar-Alegra.ps1'=(Join-Path $dbDir 'Configurar-Alegra.ps1')
    'Test-AlegraPackage.ps1'=(Join-Path $scriptsDir 'Test-AlegraPackage.ps1')
} $alegraScript


# Plan de copia: origen -> nombre numerado en la USB
$plan = @(
    @{ Origen = $databaseScript;                                       Destino = '01-Base-de-Datos.ps1' },
    @{ Origen = $setupOrigen;                                           Destino = '02-Setup-Casa-Grande.exe' },
    @{ Origen = (Join-Path $scriptsDir 'Configurar-Red.ps1');           Destino = '03-Configurar-Red.ps1' },
    @{ Origen = $ApkRuta;                                               Destino = '04-APK-Casa-Grande.apk' },
    @{ Origen = $alegraScript; Destino = '05-Configurar-Alegra.ps1' },
    @{ Origen = (Join-Path $scriptsDir 'Cambiar-Codigo-Maestro.ps1'); Destino = '06-Cambiar-Codigo-Maestro.ps1' },
    @{ Origen = (Join-Path $scriptsDir 'Configurar-Impresora-Termica.ps1'); Destino = '07-Configurar-Impresora-Termica.ps1' },
    @{ Origen = (Join-Path $scriptsDir 'Diagnosticar-Impresora.ps1'); Destino = 'Soporte\scripts\Diagnosticar-Impresora.ps1' },
    @{ Origen = (Join-Path $scriptsDir 'Test-AlegraPackage.ps1'); Destino = 'Soporte\scripts\Test-AlegraPackage.ps1' },
    @{ Origen = $AlegraBearerFile; Destino = 'Credenciales-Alegra\Token-Nuevo-Bearer.txt' },
    @{ Origen = $AlegraBasicFile; Destino = 'Credenciales-Alegra\Token-Anterior-Basic.txt' }
)
foreach ($p in $plan) { if (-not (Test-Path $p.Origen)) { Falla "Falta el archivo de origen: $($p.Origen)" } }
foreach ($f in $soporteDb) { if (-not (Test-Path (Join-Path $dbDir $f))) { Falla "Falta db\$f" } }

# Aviso si el instalador o el APK son mas viejos que el codigo (riesgo de llevar una version vieja)
function Ultimo-Commit([string]$repoDir, [string[]]$rutas) {
    if (-not (Test-Path (Join-Path $repoDir '.git'))) { return $null }
    $t = & git -C $repoDir log -1 --format=%ct -- @rutas 2>$null
    if ($t) { return [DateTimeOffset]::FromUnixTimeSeconds([int64]$t).LocalDateTime }
    return $null
}
$cSetup = Ultimo-Commit $repo @('SISMistico', 'installer', 'scripts/Configurar-Red.ps1')
$cApk = Ultimo-Commit $mesero @('app', 'packages')
$fSetup = (Get-Item $setupOrigen).LastWriteTime
$fApk = (Get-Item $ApkRuta).LastWriteTime
if ($cSetup -and $fSetup -lt $cSetup) { Aviso "El instalador ($fSetup) es MAS VIEJO que el ultimo cambio del codigo ($cSetup). Repita sin -SinCompilar." }
if ($cApk -and $fApk -lt $cApk) { Aviso "El APK ($fApk) es MAS VIEJO que el ultimo cambio de la app ($cApk). Repita sin -SinCompilar." }
Ok ("Instalador {0}  ({1:N1} MB, {2:yyyy-MM-dd HH:mm})" -f $setupNombre, ((Get-Item $setupOrigen).Length / 1MB), $fSetup)
Ok ("APK         {0}  ({1:N1} MB, {2:yyyy-MM-dd HH:mm})" -f (Split-Path $ApkRuta -Leaf), ((Get-Item $ApkRuta).Length / 1MB), $fApk)

$totalBytes = 0
foreach ($p in $plan) { $totalBytes += (Get-Item $p.Origen).Length }
foreach ($f in $soporteDb) { $totalBytes += (Get-Item (Join-Path $dbDir $f)).Length }
$totalBytes = [int64]($totalBytes * 1.05) + 1MB

# ---------------------------------------------------------------------------
# 2. Elegir la unidad
# ---------------------------------------------------------------------------
Paso '2/5  Eligiendo la unidad'
$libreBytes = [int64]::MaxValue
if ($CarpetaDestino) {
    if (-not (Test-Path $CarpetaDestino)) { New-Item -ItemType Directory -Path $CarpetaDestino -Force | Out-Null }
    $raiz = (Resolve-Path $CarpetaDestino).Path
    $descripcion = "carpeta $raiz (no es una USB)"
} else {
    $unidades = Obtener-Unidades
    if ($unidades.Count -eq 0) { Falla 'No encontre ninguna USB o unidad extraible con formato. Conecte la USB y repita.' }
    if ($Unidad) {
        $letra = ($Unidad.Trim().TrimEnd(':', '\') + ':').ToUpper()
        $elegida = $unidades | Where-Object { $_.Unidad -eq $letra } | Select-Object -First 1
        if (-not $elegida) { Falla "$letra no es una unidad USB/extraible conectada. Disponibles: $(($unidades | ForEach-Object { $_.Unidad }) -join ', ')" }
    } else {
        $elegida = Elegir-Unidad $unidades
        if (-not $elegida) { Write-Host '    Cancelado.'; exit 0 }
    }
    $raiz = "$($elegida.Unidad)\"
    $libreBytes = $elegida.LibreBytes
    $descripcion = "unidad $($elegida.Unidad) '$($elegida.Etiqueta)' ($($elegida.TamanoGB) GB, $($elegida.Sistema))"
    if ($elegida.Sistema -eq 'FAT32') { Ok 'FAT32: sirve (ningun archivo pasa de 4 GB)' }
}
$destino = Join-Path $raiz $NombreCarpeta
if ($libreBytes -lt $totalBytes) {
    Falla ("No hay espacio: se necesitan {0:N0} MB y la unidad tiene {1:N0} MB libres." -f ($totalBytes / 1MB), ($libreBytes / 1MB))
}

Write-Host ''
Write-Host "  Destino : $destino" -ForegroundColor White
Write-Host "  Unidad  : $descripcion"
Write-Host ("  Tamano  : {0:N0} MB" -f ($totalBytes / 1MB))
if (Test-Path $destino) { Write-Host '  La carpeta YA existe: se reemplazaran sus archivos numerados y Soporte\ (no se toca nada fuera de ella).' -ForegroundColor Yellow }
if (-not $Si) {
    $r = Read-Host '  Continuar? (S/N)'
    if ($r -notmatch '^[sS]') { Write-Host '    Cancelado.'; exit 0 }
}

# ---------------------------------------------------------------------------
# 3. Copiar
# ---------------------------------------------------------------------------
Paso "3/5  Copiando a $destino"
New-Item -ItemType Directory -Path $destino -Force | Out-Null

# Quitar lo que generaron versiones anteriores del kit (solo dentro de la carpeta propia)
if ((Split-Path $destino -Leaf) -ne $NombreCarpeta) { Falla 'Ruta de destino inesperada; no se borra nada.' }
$resolvedDestination=[IO.Path]::GetFullPath($destino)
$expectedDestination=[IO.Path]::GetFullPath((Join-Path $raiz $NombreCarpeta))
if($resolvedDestination -ne $expectedDestination -or ((Get-Item -LiteralPath $destino).Attributes -band [IO.FileAttributes]::ReparsePoint)) {
    Falla 'La carpeta de la USB tiene una ruta/enlace inesperado. No se limpia.'
}

Get-ChildItem $destino -File -ErrorAction SilentlyContinue | Where-Object { $_.Name -match '^\d\d-' -or $_.Name -eq 'SHA256SUMS.txt' } |
    Remove-Item -Force
$soporte = Join-Path $destino 'Soporte'
if (Test-Path -LiteralPath $soporte) {
    if((Get-Item -LiteralPath $soporte).Attributes -band [IO.FileAttributes]::ReparsePoint) { Falla 'Soporte no puede ser un enlace.' }
    if([IO.Path]::GetFullPath($soporte) -ne (Join-Path $resolvedDestination 'Soporte')) { Falla 'Ruta Soporte inesperada.' }
    Remove-Item -LiteralPath $soporte -Recurse -Force
}
New-Item -ItemType Directory -Path (Join-Path $soporte 'db') -Force | Out-Null

foreach ($p in $plan) {
    $archivoDestino = Join-Path $destino $p.Destino
    New-Item -ItemType Directory -Path (Split-Path $archivoDestino -Parent) -Force | Out-Null
    Copy-Item -LiteralPath $p.Origen -Destination $archivoDestino -Force
    Write-Host ("    {0}" -f $p.Destino)
}
foreach ($f in $soporteDb) { Copy-Item -LiteralPath (Join-Path $dbDir $f) -Destination (Join-Path $soporte 'db') -Force }
Write-Host ("    Soporte\db\  ({0} archivos)" -f $soporteDb.Count)

# ---------------------------------------------------------------------------
# 4. LEEME y sumas de comprobacion
# ---------------------------------------------------------------------------
Paso '4/5  LEEME y comprobacion'
$commit = (& git -C $repo rev-parse --short HEAD 2>$null)
$leeme = @"
CASA GRANDE INSTALLERS  -  version $Version
Preparado el $(Get-Date -Format 'yyyy-MM-dd HH:mm')   (commit $commit)
==============================================================

EJECUTE LOS ARCHIVOS EN ORDEN (el numero del nombre es el orden).

EN EL PC DEL RESTAURANTE (Windows 10/11 de 64 bits, mismo usuario: pasos 01, 02, 03 y 05)
--------------------------------------------------------------
 01-Base-de-Datos.ps1       Clic derecho > "Ejecutar con PowerShell".
                            Hace respaldo y aplica las migraciones de API, empleados, Domicilios y Alegra (07).
                            Lleva los SQL embebidos. No requiere tokens ni conexion con Alegra.
                            La verificacion final debe indicar OK en las tablas/columnas Alegra.
                            Elija la misma base que usa el escritorio. Mantengalo cerrado hasta terminar 05.

 02-Setup-Casa-Grande.exe   Doble clic (administrador). Windows 10/11 de 64 bits.
                            Deje marcada la API movil y anote su clave para los celulares.
                            Pide el CODIGO MAESTRO de 8 digitos (dos veces): es el que se escribe en el celular, junto
                            con una observacion, para quitar un producto de un pedido. No reemplaza el PIN de 4 digitos.
                            No abra el sistema al terminar: primero configure Alegra con el paso 05.
                            La tarea opcional de credenciales tiene dos paginas si prefiere introducirlas a mano:
                            principal: correo + token nuevo + Bearer; emision: correo + token anterior + Basic.
                            El instalador incluye renderer de 80 mm y lector PDF; no necesita Python en el restaurante.

 03-Configurar-Red.ps1      Ejecutar como administrador para IP, puerto, firewall y datos de conexion movil.

 05-Configurar-Alegra.ps1   Ejecutar con el MISMO usuario de Windows del sistema (LocalDB es por usuario).
                            Localiza solo la instalacion (registro, accesos directos, Program Files y unidades):
                            no hace falta indicar la carpeta. Si hubiera varias, use -ApplicationPath "carpeta".
                            Importa Credenciales-Alegra\Token-Nuevo-Bearer.txt y Token-Anterior-Basic.txt.
                            Pide el correo API si aun no esta configurado. No muestra los tokens.
                            Verifica ambos accesos por lectura, misma empresa y herramientas PDF instaladas.
                            Conserva configuracion existente; en una base nueva usa el perfil conocido Casa Grande.
                            Solo vincula nombres exactos unicos activos; exporta pendientes, no aprueba similitudes.
                            Permite ventas con productos aprobados; los productos/cargos sin vinculo bloquean su emision.
                            Habilita doble impresion para ventas nuevas, sin reimprimir las anteriores.
                            Si hay variables Machine diferentes, use PowerShell elevado del mismo usuario y -Scope Machine.
                            Para otra base: .\05-Configurar-Alegra.ps1 -Database NombreBase
                            Comprobacion posterior sin cambios: .\05-Configurar-Alegra.ps1 -VerifyOnly
                            No crea ni emite documentos. Reinicie/abra el sistema despues de terminar este paso.

 FACTURACION ELECTRONICA    Con el sistema abierto y venta marcada: imprime la local, emite y espera aceptacion.
                            Luego genera/imprime la electronica de 80 mm usando XML y datos del PDF oficial.
                            Descargar PDF en el detalle conserva el original Carta; imprimir usa el modelo de 80 mm.
                            Impresora vacia: predeterminada Windows, como la local.
                            Con Microsoft Print to PDF abre un segundo dialogo para guardar el PDF de 80 mm.
                            COBRADA no confirma emision: revise estado electronico y errores en el historial/detalle.

 06-Cambiar-Codigo-Maestro.ps1   CUANDO SE REQUIERA cambiar el codigo maestro (no es parte de la instalacion).
                            Localiza solo la instalacion y pide el codigo nuevo (8 digitos, oculto, dos veces).
                            Guarda solo sal + hash en SISMistico.exe.config (con copia .bak). Reinicie el sistema despues.
                            Tambien esta en el menu Inicio: "Cambiar codigo maestro".

 07-Configurar-Impresora-Termica.ps1   Despues del 02, con SISRestaurante CERRADO y sin trabajos en la cola.
                            PowerShell COMO ADMINISTRADOR, con el MISMO usuario de Windows del sistema.
                            Cambia el papel predeterminado de la impresora (POS-80) de 297 mm al rollo largo de 3276 mm
                            que anuncia su controlador, para que facturas y comandas largas no salgan partidas.
                            Guarda un respaldo en el Escritorio y revierte solo si falla.
                            Ver sin cambiar: .\07-Configurar-Impresora-Termica.ps1 -Modo Estado
                            Deshacer: -Modo Restaurar -Respaldo "carpeta del respaldo". Luego abra el sistema.
                            Si algo no sale bien: Soporte\scripts\Diagnosticar-Impresora.ps1 (solo lectura).

EN CADA CELULAR ANDROID (7.0 o superior, en el mismo WiFi que el PC)
--------------------------------------------------------------
 04-APK-Casa-Grande.apk     Copielo al celular, abralo y permita "Instalar apps desconocidas".
                            Al abrir la app escriba IP, puerto 8099 y la clave; entre con el PIN.

OTROS ARCHIVOS
--------------------------------------------------------------
 Credenciales-Alegra\       Token-Nuevo-Bearer.txt y Token-Anterior-Basic.txt (contienen las claves).
 Soporte\                   Copia legible de scripts SQL y reversiones para soporte tecnico.
 SHA256SUMS.txt             Sumas de comprobacion de los archivos.

Guia completa con problemas frecuentes: docs\08-instalacion-desde-cero.md del repositorio.
"@
[IO.File]::WriteAllText((Join-Path $destino '00-LEEME.txt'), ($leeme -replace "`r?`n", "`r`n"), (New-Object Text.UTF8Encoding $true))
Write-Host '    00-LEEME.txt'

# Comprobar que lo copiado es identico al origen y guardar las sumas
$sumas = @()
$errores = 0
foreach ($p in $plan) {
    $hOrigen = (Get-FileHash $p.Origen -Algorithm SHA256).Hash
    $hCopia = (Get-FileHash (Join-Path $destino $p.Destino) -Algorithm SHA256).Hash
    if ($hOrigen -ne $hCopia) { Write-Host "    X  $($p.Destino): la copia NO coincide con el origen" -ForegroundColor Red; $errores++ }
    $sumas += "$hCopia  $($p.Destino)"
}
foreach ($f in $soporteDb) {
    $rel = "Soporte\db\$f"
    $hOrigen = (Get-FileHash (Join-Path $dbDir $f) -Algorithm SHA256).Hash
    $hCopia = (Get-FileHash (Join-Path $destino $rel) -Algorithm SHA256).Hash
    if ($hOrigen -ne $hCopia) { Write-Host "    X  ${rel}: la copia NO coincide con el origen" -ForegroundColor Red; $errores++ }
    $sumas += "$hCopia  $rel"
}
if ($errores -gt 0) { Falla "$errores archivo(s) se copiaron mal. Repita; si persiste, pruebe otro puerto USB o revise la memoria." }
$sumas += ((Get-FileHash -LiteralPath (Join-Path $destino '00-LEEME.txt') -Algorithm SHA256).Hash + '  00-LEEME.txt')
Set-Content -Path (Join-Path $destino 'SHA256SUMS.txt') -Value $sumas -Encoding ASCII
Ok "Todos los archivos coinciden con el origen (SHA-256)"

# ---------------------------------------------------------------------------
# 5. Resumen
# ---------------------------------------------------------------------------
Paso '5/5  Listo'
Write-Host ''
Write-Host ('=' * 64) -ForegroundColor Green
Write-Host " USB PREPARADA: $destino" -ForegroundColor Green
Write-Host ('=' * 64) -ForegroundColor Green
Get-ChildItem $destino | Sort-Object Name | ForEach-Object {
    if ($_.PSIsContainer) { Write-Host ("   {0}\" -f $_.Name) }
    else { Write-Host ("   {0,-28} {1,8:N1} MB" -f $_.Name, ($_.Length / 1MB)) }
}
Write-Host ''
Write-Host ' Expulse la USB con seguridad (icono de la barra de tareas) antes de sacarla.'
