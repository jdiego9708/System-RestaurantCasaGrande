<#
    Prepara una USB con todo lo necesario para instalar Casa Grande en el restaurante.

    Muestra las unidades USB / extraibles conectadas, usted elige una de la lista y el script crea en ella la
    carpeta "Casa Grande Installers" con los archivos NUMERADOS en el orden en que se ejecutan:

        00-LEEME.txt                    instrucciones cortas con ese orden
        01-Base-de-Datos.ps1            PC del restaurante: respalda la base y prepara los procedimientos de la API
        02-Setup-Casa-Grande.exe        PC del restaurante: instalador del sistema (activa la API movil)
        03-Configurar-Red.ps1           PC del restaurante: IP, firewall y datos que se escriben en los celulares
        04-APK-Casa-Grande.apk          Cada celular Android: app del mesero
        Soporte\                        scripts .sql y auxiliares que usa el paso 01 (no borrar)
        SHA256SUMS.txt                  sumas para comprobar que la copia no se daño

    Uso (en la maquina de desarrollo, no requiere administrador):
        .\Preparar-USB.ps1                      # lista las USB y pregunta cual preparar
        .\Preparar-USB.ps1 -Unidad K            # salta la lista (K debe ser una unidad USB/extraible)
        .\Preparar-USB.ps1 -Compilar            # recompila instalador y APK antes de copiar
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
    [switch]$Compilar,
    [switch]$Si,
    [string]$ApkRuta,
    [string]$FlutterBin = 'D:\DevTools\flutter-3.47.8\bin'
)

$ErrorActionPreference = 'Stop'
$NombreCarpeta = 'Casa Grande Installers'

function Paso($t)  { Write-Host "`n==> $t" -ForegroundColor Cyan }
function Ok($t)    { Write-Host "    OK  $t" -ForegroundColor Green }
function Aviso($t) { Write-Host "    !!  $t" -ForegroundColor Yellow }
function Falla($t) { Write-Host "`nERROR: $t" -ForegroundColor Red; exit 1 }

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

if ($Compilar -or -not (Test-Path $setupOrigen)) {
    if (-not $Compilar) { Aviso "No existe ${setupNombre}: se compila ahora." }
    & (Join-Path $repo 'installer\Publicar-Instalador.ps1') -Version $Version
    if ($LASTEXITCODE -ne 0) { Falla 'No se pudo generar el instalador.' }
}
if (-not (Test-Path $setupOrigen)) { Falla "No existe el instalador $setupOrigen" }

if ($Compilar) {
    $flutter = Join-Path $FlutterBin 'flutter.bat'
    if (-not (Test-Path $flutter)) { Falla "No encuentro Flutter 3.47.8 en $FlutterBin (use -FlutterBin). El flutter del PATH suele ser el 3.11.5 y no sirve." }
    Push-Location (Join-Path $mesero 'app')
    try {
        $env:Path = "$FlutterBin;$env:Path"
        & $flutter build apk --release
        if ($LASTEXITCODE -ne 0) { Falla 'flutter build apk fallo.' }
    } finally { Pop-Location }
}
if (-not (Test-Path $ApkRuta)) { Falla "No existe el APK $ApkRuta. Repita con -Compilar (usa Flutter 3.47.8)." }

# Archivos de soporte del paso 01 (base de datos)
$dbDir = Join-Path $repo 'db'
$soporteDb = @('Invoke-Sql.ps1', 'Restore-ProdCopy.ps1', 'verificar-despliegue.sql', 'produccion-schema.sql',
               '01_sp_Movil_Insertar_pedido.sql', '03_sp_Movil_Agregar_items.sql', '04_sp_Movil_Quitar_item.sql',
               '02_unificar_empleados_duplicados.sql',
               '01_sp_Movil_Insertar_pedido.revert.sql', '03_sp_Movil_Agregar_items.revert.sql',
               '04_sp_Movil_Quitar_item.revert.sql', '02_unificar_empleados_duplicados.revert.sql')

# Plan de copia: origen -> nombre numerado en la USB
$plan = @(
    @{ Origen = (Join-Path $scriptsDir 'Desplegar-Base-de-Datos.ps1'); Destino = '01-Base-de-Datos.ps1' },
    @{ Origen = $setupOrigen;                                           Destino = '02-Setup-Casa-Grande.exe' },
    @{ Origen = (Join-Path $scriptsDir 'Configurar-Red.ps1');           Destino = '03-Configurar-Red.ps1' },
    @{ Origen = $ApkRuta;                                               Destino = '04-APK-Casa-Grande.apk' }
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
if ($cSetup -and $fSetup -lt $cSetup) { Aviso "El instalador ($fSetup) es MAS VIEJO que el ultimo cambio del codigo ($cSetup). Use -Compilar." }
if ($cApk -and $fApk -lt $cApk) { Aviso "El APK ($fApk) es MAS VIEJO que el ultimo cambio de la app ($cApk). Use -Compilar." }
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
Get-ChildItem $destino -File -ErrorAction SilentlyContinue | Where-Object { $_.Name -match '^\d\d-' -or $_.Name -eq 'SHA256SUMS.txt' } |
    Remove-Item -Force
$soporte = Join-Path $destino 'Soporte'
if (Test-Path $soporte) { Remove-Item $soporte -Recurse -Force }
New-Item -ItemType Directory -Path (Join-Path $soporte 'db') -Force | Out-Null

foreach ($p in $plan) {
    Copy-Item -LiteralPath $p.Origen -Destination (Join-Path $destino $p.Destino) -Force
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

EN EL PC DEL RESTAURANTE (Windows 10/11, con el mismo usuario que abre el sistema)
--------------------------------------------------------------
 01-Base-de-Datos.ps1       Clic derecho > "Ejecutar con PowerShell".
                            Hace copia de seguridad de la base, crea los procedimientos de la API movil y,
                            si usted confirma, unifica empleados duplicados. Pide confirmacion en cada paso.

 02-Setup-Casa-Grande.exe   Doble clic (pide administrador; si Windows avisa "SmartScreen":
                            Mas informacion > Ejecutar de todos modos).
                            Deje marcado "Activar la API movil" y ANOTE LA CLAVE que muestra.

 03-Configurar-Red.ps1      Clic derecho > "Ejecutar con PowerShell" (pide administrador).
                            Muestra la IP del PC, el puerto y la clave que se escriben en los celulares, y avisa si
                            la red esta como Publica o la IP es dinamica (reservela en el router).

EN CADA CELULAR ANDROID (7.0 o superior, en el mismo WiFi que el PC)
--------------------------------------------------------------
 04-APK-Casa-Grande.apk     Copielo al celular, abralo y permita "Instalar apps desconocidas".
                            Al abrir la app escriba IP, puerto 8099 y la clave; entre con el PIN.

OTROS ARCHIVOS
--------------------------------------------------------------
 Soporte\                   Scripts y .sql que usa el paso 01. No borrar.
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
