<#
    PRUEBA REAL de instalacion limpia en ESTE equipo, usando los archivos de la USB "Casa Grande Installers".

    QUE HACE (en este orden):
      1. Copia de seguridad (.bak verificado) de TODAS las bases de LocalDB y de la configuracion del sistema instalado.
      2. Desinstala SISRestaurante, borra la instancia de LocalDB y desinstala LocalDB (todas sus versiones).
      3. Comprueba que el equipo quedo como un Windows 11 virgen (sin LocalDB ni sistema).
      4. Ejecuta la USB como en el restaurante:
           01-Base-de-Datos.ps1     con el usuario NORMAL, sin administrador: instala LocalDB solo (Windows pide permiso)
           02-Setup-Casa-Grande.exe silencioso, con la misma clave de API y el mismo codigo maestro de antes
           03-Configurar-Red.ps1, 05-Configurar-Alegra.ps1 y 07-Configurar-Impresora-Termica.ps1 -Modo Estado
      5. Abre el sistema instalado (sin privilegios), comprueba su API (ping + login por PIN) y captura la ventana.
      6. Resume cada comprobacion como OK / FALLA.

    LAS BASES Y LA CONFIGURACION ANTERIORES NO SE PIERDEN: quedan en la carpeta de respaldo para volver a restaurarlas.

    USO (PowerShell COMO ADMINISTRADOR; no se ejecuta sola por seguridad):
        .\Probar-Instalacion-Limpia.ps1 -ConfirmarDesinstalacion
        .\Probar-Instalacion-Limpia.ps1 -ConfirmarDesinstalacion -Kit 'K:\Casa Grande Installers' -SinAlegra
#>
[CmdletBinding()]
param(
    [switch]$ConfirmarDesinstalacion,
    [string]$Kit,
    [string]$CarpetaRespaldo,
    # No ejecuta el paso 05 (que lee las credenciales de Alegra y consulta su API en modo lectura).
    [switch]$SinAlegra,
    [string]$Instance = '(localdb)\MSSQLLocalDB',
    [string]$Database = 'CasaGrandeBD'
)

$ErrorActionPreference = 'Stop'
if (-not $ConfirmarDesinstalacion) { throw 'Falta -ConfirmarDesinstalacion: este script desinstala el sistema y LocalDB de ESTE equipo.' }
if (-not ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    throw 'Ejecute este script en PowerShell COMO ADMINISTRADOR.'
}

$sello = Get-Date -Format 'yyyyMMdd_HHmm'
if (-not $Kit) {
    $Kit = Get-Volume | Where-Object { $_.DriveLetter -and $_.DriveType -eq 'Removable' } |
        ForEach-Object { "$($_.DriveLetter):\Casa Grande Installers" } | Where-Object { Test-Path "$_\01-Base-de-Datos.ps1" } | Select-Object -First 1
}
if (-not $Kit -or -not (Test-Path (Join-Path $Kit '01-Base-de-Datos.ps1'))) { throw 'No encuentro la USB "Casa Grande Installers". Indique -Kit.' }
if (-not $CarpetaRespaldo) { $CarpetaRespaldo = "D:\Repositorios\CasaGrande-local-backups\pre-instalacion-limpia-$sello" }
New-Item -ItemType Directory -Path $CarpetaRespaldo -Force | Out-Null
$Registro = Join-Path $CarpetaRespaldo 'prueba-instalacion.log'
$Instalado = 'C:\Program Files\SISRestaurante'
$resultados = New-Object System.Collections.Generic.List[object]

function Log([string]$t, [string]$color = 'Gray') {
    $linea = '{0:HH:mm:ss}  {1}' -f (Get-Date), $t
    Write-Host $linea -ForegroundColor $color
    Add-Content -LiteralPath $Registro -Value $linea -Encoding UTF8
}
function Paso([string]$t) { Log ''; Log "==> $t" 'Cyan' }
function Resultado([string]$nombre, [bool]$ok, [string]$detalle = '') {
    $resultados.Add([pscustomobject]@{ Comprobacion = $nombre; Resultado = $(if ($ok) { 'OK' } else { 'FALLA' }); Detalle = $detalle })
    Log ("{0,-5} {1}  {2}" -f $(if ($ok) { 'OK' } else { 'FALLA' }), $nombre, $detalle) $(if ($ok) { 'Green' } else { 'Red' })
}
function Aborta([string]$motivo) {
    Log "ABORTA: $motivo" 'Red'
    Log "Las copias de seguridad estan en: $CarpetaRespaldo" 'Yellow'
    throw $motivo
}

function Sql([string]$base, [string]$sql, [switch]$Tabla) {
    Add-Type -AssemblyName System.Data
    $cn = New-Object System.Data.SqlClient.SqlConnection "Server=$Instance;Database=$base;Integrated Security=true;Connect Timeout=60;Pooling=false"
    try {
        $cn.Open(); $cmd = $cn.CreateCommand(); $cmd.CommandText = $sql; $cmd.CommandTimeout = 0
        if ($Tabla) { $dt = New-Object System.Data.DataTable; [void](New-Object System.Data.SqlClient.SqlDataAdapter $cmd).Fill($dt); return , $dt }
        [void]$cmd.ExecuteNonQuery()
    } finally { $cn.Dispose() }
}

# IMPORTANTE: al conectarse a LocalDB, SqlClient carga SqlUserInstance.dll DENTRO del proceso. Si ese proceso sigue vivo cuando
# se desinstala LocalDB, Windows Installer (Restart Manager) lo cierra a la fuerza: asi murio este script la primera vez, justo
# al desinstalar. Por eso, antes de desinstalar, todo el SQL va en procesos aparte que terminan solos.
function Sql-Hijo([string]$base, [string]$sql, [switch]$Filas) {
    $codigo = @'
param([string]$Instance, [string]$Base, [string]$Sql, [string]$Filas)
Add-Type -AssemblyName System.Data
$cn = New-Object System.Data.SqlClient.SqlConnection "Server=$Instance;Database=$Base;Integrated Security=true;Connect Timeout=60;Pooling=false"
$cn.Open(); $cmd = $cn.CreateCommand(); $cmd.CommandText = $Sql; $cmd.CommandTimeout = 0
if ($Filas -eq '1') { $r = $cmd.ExecuteReader(); while ($r.Read()) { [string]$r.GetValue(0) }; $r.Close() } else { [void]$cmd.ExecuteNonQuery() }
$cn.Dispose()
'@
    $tmp = Join-Path $env:TEMP ('sql_hijo_' + [guid]::NewGuid().ToString('N') + '.ps1')
    Set-Content -LiteralPath $tmp -Value $codigo -Encoding UTF8
    try {
        $salida = & powershell.exe -NoProfile -ExecutionPolicy Bypass -File $tmp -Instance $Instance -Base $base -Sql $sql -Filas $(if ($Filas) { '1' } else { '0' }) 2>&1
        if ($LASTEXITCODE -ne 0) { throw "SQL fallo: $($salida -join ' ')" }
        return $salida
    } finally { Remove-Item -LiteralPath $tmp -Force -ErrorAction SilentlyContinue }
}

# Start-Process -Wait espera tambien a los procesos hijo: msiexec deja vivo el servicio de Windows Installer y el script
# se quedaba esperando minutos. Aqui se espera solo al proceso lanzado.
function Ejecutar([string]$exe, [string]$argumentos) {
    $p = Start-Process $exe -ArgumentList $argumentos -PassThru
    $null = $p.Handle
    $p.WaitForExit()
    return $p.ExitCode
}

function Ajuste([string]$ruta, [string]$clave) {
    if (-not (Test-Path $ruta)) { return $null }
    [xml]$x = Get-Content -LiteralPath $ruta -Raw
    $n = $x.SelectSingleNode("//appSettings/add[@key='$clave']")
    if ($n) { return $n.value } else { return $null }
}
function Poner-Ajuste([string]$ruta, [string]$clave, [string]$valor) {
    $x = New-Object System.Xml.XmlDocument; $x.PreserveWhitespace = $true; $x.Load($ruta)
    $n = $x.SelectSingleNode("//appSettings/add[@key='$clave']")
    if (-not $n) { $n = $x.CreateElement('add'); $n.SetAttribute('key', $clave); [void]$x.SelectSingleNode('//appSettings').AppendChild($n) }
    $n.SetAttribute('value', $valor); $x.Save($ruta)
}

function Agregar-LocalDb-Al-Path {
    foreach ($d in @(Get-ChildItem 'C:\Program Files\Microsoft SQL Server' -Directory -ErrorAction SilentlyContinue)) {
        $bin = Join-Path $d.FullName 'Tools\Binn'
        if ((Test-Path (Join-Path $bin 'SqlLocalDB.exe')) -and ($env:Path -notlike "*$bin*")) { $env:Path += ";$bin" }
    }
}

Log "PRUEBA DE INSTALACION LIMPIA  -  equipo $env:COMPUTERNAME  -  usuario $env:USERNAME" 'White'
Log "USB: $Kit"
Log "Respaldo de lo actual: $CarpetaRespaldo"

# ------------------------------------------------------------------ 1. Respaldos
Paso '1/6  Copia de seguridad de lo que hay hoy (no se desinstala nada hasta verificarla)'
if (Get-Process SISMistico -ErrorAction SilentlyContinue) { Aborta 'SISMistico.exe esta abierto. Cierrelo y repita.' }
$bases = @()
if (Get-Command sqllocaldb -ErrorAction SilentlyContinue) {
    & sqllocaldb start MSSQLLocalDB | Out-Null
    $bases = @(Sql-Hijo 'master' "SELECT name FROM sys.databases WHERE database_id > 4 AND state_desc = 'ONLINE' ORDER BY name" -Filas | Where-Object { $_ })
}
Log ("Bases en LocalDB: {0}" -f $(if ($bases.Count) { $bases -join ', ' } else { '(ninguna)' }))
foreach ($b in $bases) {
    $bak = Join-Path $CarpetaRespaldo ("{0}.bak" -f $b)
    Sql-Hijo 'master' "BACKUP DATABASE [$b] TO DISK = N'$bak' WITH COPY_ONLY, INIT, CHECKSUM" | Out-Null
    Sql-Hijo 'master' "RESTORE VERIFYONLY FROM DISK = N'$bak' WITH CHECKSUM" | Out-Null
    Log ("  {0}.bak  {1:N1} MB  (verificado)" -f $b, ((Get-Item $bak).Length / 1MB))
}
$cfgViejo = Join-Path $Instalado 'SISMistico.exe.config'
$cfgGuardado = Join-Path $CarpetaRespaldo 'SISMistico.exe.config.anterior'
# Si la prueba se repite (el sistema ya se desinstalo), se usa la configuracion guardada en la primera ejecucion.
if (Test-Path $cfgViejo) { Copy-Item $cfgViejo $cfgGuardado -Force } else { $cfgViejo = $cfgGuardado }
$claveApi = $null; $salMaestro = $null; $hashMaestro = $null
if (Test-Path $cfgViejo) {
    $claveApi = Ajuste $cfgViejo 'ApiKey'; $salMaestro = Ajuste $cfgViejo 'MasterCodeSalt'; $hashMaestro = Ajuste $cfgViejo 'MasterCodeHash'
    if ($claveApi -and ($claveApi.Length -lt 8 -or $claveApi -match '\s' -or $claveApi -eq 'cambiar-esta-clave')) { $claveApi = $null }
    Log ("Configuracion anterior guardada. ApiKey propia: {0}; codigo maestro propio: {1}" -f [bool]$claveApi, [bool]$hashMaestro)
}
Log 'Respaldos listos y verificados.' 'Green'

# ------------------------------------------------------------------ 2. Desinstalar
Paso '2/6  Desinstalando SISRestaurante, la instancia de LocalDB y LocalDB'
$unins = Join-Path $Instalado 'unins000.exe'
if (Test-Path $unins) {
    $codigo = Ejecutar $unins '/VERYSILENT /SUPPRESSMSGBOXES /NORESTART'
    Log "Desinstalador de SISRestaurante: codigo $codigo"
}
# Lo que deja el desinstalador (configuracion y registros que escribio la propia aplicacion): solo esa carpeta.
if ((Test-Path $Instalado) -and ([IO.Path]::GetFullPath($Instalado) -eq 'C:\Program Files\SISRestaurante')) {
    Remove-Item -LiteralPath $Instalado -Recurse -Force -ErrorAction SilentlyContinue
}
if (Get-Command sqllocaldb -ErrorAction SilentlyContinue) {
    & sqllocaldb stop MSSQLLocalDB -k 2>&1 | Out-Null
    & sqllocaldb delete MSSQLLocalDB 2>&1 | Out-Null
    Log 'Instancia MSSQLLocalDB detenida y eliminada.'
}
Get-ChildItem $env:USERPROFILE -File -ErrorAction SilentlyContinue | Where-Object { $_.Name -match '^(CasaGrande|CG_).*\.(mdf|ldf|ndf)$' } |
    ForEach-Object { Remove-Item -LiteralPath $_.FullName -Force; Log "  borrado archivo de base (respaldado): $($_.Name)" }
$entradas = Get-ItemProperty 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\*' -ErrorAction SilentlyContinue |
    Where-Object { $_.DisplayName -match '^Microsoft SQL Server \d+ LocalDB' }
foreach ($e in $entradas) {
    if ($e.UninstallString -match '\{[0-9A-Fa-f-]{36}\}') {
        $guid = $Matches[0]
        $codigo = Ejecutar 'msiexec.exe' "/x $guid /qn /norestart"
        Log "Desinstalado '$($e.DisplayName)': codigo $codigo"
        if ($codigo -notin 0, 3010, 1605) { Aborta "No se pudo desinstalar $($e.DisplayName) (codigo $codigo)." }
    }
}

# ------------------------------------------------------------------ 3. Equipo virgen
Paso '3/6  Comprobando que el equipo quedo como un Windows 11 sin instalar'
$env:Path = ($env:Path -split ';' | Where-Object { $_ -and $_ -notmatch 'Microsoft SQL Server' }) -join ';'
Resultado 'LocalDB no esta instalado (sqllocaldb no existe)' (-not (Get-Command sqllocaldb -ErrorAction SilentlyContinue))
Resultado 'No queda ninguna version de LocalDB registrada' (-not (Get-ChildItem 'HKLM:\SOFTWARE\Microsoft\Microsoft SQL Server Local DB\Installed Versions' -ErrorAction SilentlyContinue))
Resultado 'SISRestaurante no esta instalado' (-not (Test-Path (Join-Path $Instalado 'SISMistico.exe')))
if ($resultados | Where-Object { $_.Resultado -eq 'FALLA' }) { Aborta 'El equipo no quedo limpio; no se sigue con la instalacion.' }

# ------------------------------------------------------------------ 4. Instalar desde la USB
Paso '4/6  Instalacion desde la USB, como en el restaurante'
$copia = Get-ChildItem (Join-Path $Kit 'Copias') -Filter '*.bak' -ErrorAction SilentlyContinue | Sort-Object LastWriteTime -Descending | Select-Object -First 1
if (-not $copia) { Aborta 'La USB no trae ninguna copia en Copias\.' }
Resultado 'La USB trae LocalDB (Requisitos\SqlLocalDB.msi)' (Test-Path (Join-Path $Kit 'Requisitos\SqlLocalDB.msi'))

# 01 con el usuario NORMAL (sin administrador): asi se prueba el permiso que Windows pide solo para instalar LocalDB.
$salida01 = Join-Path $CarpetaRespaldo 'paso-01.log'
$codigo01 = Join-Path $CarpetaRespaldo 'paso-01.codigo'
$envoltorio = Join-Path $CarpetaRespaldo 'paso-01-envoltorio.ps1'
@"
& powershell.exe -NoProfile -ExecutionPolicy Bypass -File '$((Join-Path $Kit '01-Base-de-Datos.ps1'))' -Origen Restaurar -ArchivoCopia '$($copia.FullName)' -Si -CarpetaRespaldo '$((Join-Path $CarpetaRespaldo 'respaldos-del-paso-01'))' *>&1 | Out-File -LiteralPath '$salida01' -Encoding UTF8
Set-Content -LiteralPath '$codigo01' -Value `$LASTEXITCODE
"@ | Set-Content -LiteralPath $envoltorio -Encoding UTF8
Remove-Item $codigo01 -ErrorAction SilentlyContinue
Log '01-Base-de-Datos.ps1 con el usuario normal (sin administrador). SI WINDOWS PIDE PERMISO PARA INSTALAR LOCALDB, ACEPTELO EN PANTALLA.' 'Yellow'
$sinPrivilegios = $true
# Como un usuario normal de Windows: tarea programada con el token NO elevado del mismo usuario (nivel de ejecucion limitado).
# Asi el permiso de administrador que pide el 01 para LocalDB es el aviso real de Windows (UAC). Con "runas /trustlevel" el
# token queda restringido y Windows no puede elevarlo: no representa a un usuario real.
$tarea = 'CasaGrande-prueba-01-sin-privilegios'
try {
    $accion = New-ScheduledTaskAction -Execute 'powershell.exe' -Argument "-NoProfile -ExecutionPolicy Bypass -File `"$envoltorio`""
    $principal = New-ScheduledTaskPrincipal -UserId "$env:USERDOMAIN\$env:USERNAME" -LogonType Interactive -RunLevel Limited
    $ajustes = New-ScheduledTaskSettingsSet -ExecutionTimeLimit (New-TimeSpan -Hours 1) -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries
    Register-ScheduledTask -TaskName $tarea -Action $accion -Principal $principal -Settings $ajustes -Force | Out-Null
    Start-ScheduledTask -TaskName $tarea
} catch {
    Log "No se pudo lanzar el 01 sin privilegios ($($_.Exception.Message)); se ejecuta con administrador (la ruta del permiso de LocalDB queda SIN probar)." 'Yellow'
    $sinPrivilegios = $false
    & powershell.exe -NoProfile -ExecutionPolicy Bypass -File $envoltorio
}
$limite = (Get-Date).AddMinutes(25)
while (-not (Test-Path $codigo01) -and (Get-Date) -lt $limite) { Start-Sleep -Seconds 3 }
if ($sinPrivilegios) { Unregister-ScheduledTask -TaskName $tarea -Confirm:$false -ErrorAction SilentlyContinue }
Resultado '01-Base-de-Datos.ps1 termino' (Test-Path $codigo01)
$c01 = if (Test-Path $codigo01) { [int](Get-Content $codigo01) } else { -1 }
Resultado '01 sin errores (codigo de salida 0)' ($c01 -eq 0) "codigo $c01; sin privilegios: $sinPrivilegios"
if (Test-Path $salida01) { Get-Content $salida01 | Where-Object { $_ -match 'LocalDB|Copia restaurada|Empleados activos|Mesas:|ERROR|FALTA' } | ForEach-Object { Log "   01> $($_.Trim())" } }
if ($c01 -ne 0) { Aborta '01-Base-de-Datos.ps1 fallo (ver paso-01.log).' }
Agregar-LocalDb-Al-Path
Resultado 'LocalDB quedo instalado por el 01' ([bool](Get-ChildItem 'HKLM:\SOFTWARE\Microsoft\Microsoft SQL Server Local DB\Installed Versions' -ErrorAction SilentlyContinue))

$c = (Sql $Database @'
SELECT (SELECT COUNT(*) FROM Empleados WHERE Id_empleado > 0 AND Estado = 'ACTIVO' AND Codigo_maestro > 0) AS admins,
       (SELECT COUNT(*) FROM Mesas WHERE Id_mesa > 0) AS mesas,
       (SELECT COUNT(*) FROM Platos WHERE Estado = 'ACTIVO') AS platos,
       (SELECT COUNT(*) FROM Bebidas WHERE Estado = 'ACTIVO') AS bebidas,
       (SELECT COUNT(*) FROM Pedidos) AS pedidos,
       (SELECT COUNT(*) FROM Ventas) AS ventas,
       CASE WHEN COL_LENGTH('dbo.Pedidos','DeliveryNumber') IS NOT NULL AND OBJECT_ID('dbo.RappiOrders','U') IS NOT NULL
             AND COL_LENGTH('dbo.Pedidos','Payment_method') IS NOT NULL AND OBJECT_ID('dbo.AlegraSettings','U') IS NOT NULL THEN 1 ELSE 0 END AS migraciones
'@ -Tabla).Rows[0]
Resultado "Base $Database restaurada: administrador con PIN" ($c.admins -ge 1) "$($c.admins)"
Resultado 'Base: mesas, platos y bebidas del menu' (($c.mesas -gt 0) -and ($c.platos -gt 0) -and ($c.bebidas -gt 0)) "mesas $($c.mesas), platos $($c.platos), bebidas $($c.bebidas)"
Resultado 'Base: sin pedidos ni ventas' (($c.pedidos -eq 0) -and ($c.ventas -eq 0))
Resultado 'Base: migraciones 06 a 11 aplicadas (Domicilios, Alegra, forma de pago, Rappi, numeracion)' ($c.migraciones -eq 1)
$adm = (Sql $Database "SELECT TOP 1 Id_empleado, Codigo_maestro FROM Empleados WHERE Id_empleado > 0 AND Estado = 'ACTIVO' AND Cargo_empleado = 'ADMINISTRADOR' AND Codigo_maestro > 0 ORDER BY Id_empleado" -Tabla).Rows[0]

# 02: instalador silencioso (misma clave de API que antes para no reconfigurar los celulares)
$log02 = Join-Path $CarpetaRespaldo 'paso-02-instalador.log'
$args02 = @('/VERYSILENT', '/SUPPRESSMSGBOXES', '/NORESTART', '/SP-', "/LOG=`"$log02`"", '/TASKS="desktopicon,api"')
if ($claveApi) { $args02 += "/ApiKey=$claveApi" }
Log '02-Setup-Casa-Grande.exe (silencioso)'
$codigo02 = Ejecutar (Join-Path $Kit '02-Setup-Casa-Grande.exe') ($args02 -join ' ')
Resultado '02 instalador termino sin errores' ($codigo02 -eq 0) "codigo $codigo02"
$cfg = Join-Path $Instalado 'SISMistico.exe.config'
Resultado 'Instalado: SISMistico.exe' (Test-Path (Join-Path $Instalado 'SISMistico.exe'))
Resultado 'Instalado: herramientas PDF (SumatraPDF y renderer)' ((Test-Path (Join-Path $Instalado 'Alegra\Tools\SumatraPDF.exe')) -and (Test-Path (Join-Path $Instalado 'Alegra\Tools\Renderer\CasaGrandeFiscalPdf.exe')))
Resultado 'Instalado: PdfPig y sus dependencias (lector de facturas de Rappi)' ((Test-Path (Join-Path $Instalado 'UglyToad.PdfPig.dll')) -and (Test-Path (Join-Path $Instalado 'System.Memory.dll')))
Resultado 'Configuracion: API movil habilitada' ((Ajuste $cfg 'ApiHabilitada') -eq 'true')
Resultado 'Configuracion: base CasaGrandeBD en LocalDB' ((Get-Content $cfg -Raw) -match 'conexionLocalBD"\s+connectionString="[^"]*localdb\)\\MSSQLLocalDB;Initial Catalog=CasaGrandeBD;')
if ($hashMaestro -and $salMaestro) {
    Poner-Ajuste $cfg 'MasterCodeSalt' $salMaestro; Poner-Ajuste $cfg 'MasterCodeHash' $hashMaestro
    Resultado 'Codigo maestro anterior conservado' ((Ajuste $cfg 'MasterCodeHash') -eq $hashMaestro)
}

# 03: red (reserva de URL y firewall)
Log '03-Configurar-Red.ps1'
& powershell.exe -NoProfile -ExecutionPolicy Bypass -File (Join-Path $Kit '03-Configurar-Red.ps1') *>&1 | Out-File (Join-Path $CarpetaRespaldo 'paso-03.log') -Encoding UTF8
Resultado '03 red: termino sin errores' ($LASTEXITCODE -eq 0) "codigo $LASTEXITCODE"
Resultado 'Firewall: regla de la API movil' ([bool](Get-NetFirewallRule -DisplayName 'SISRestaurante API' -ErrorAction SilentlyContinue))
Resultado 'Reserva de URL de la API (netsh http urlacl)' ((& netsh http show urlacl url=http://+:8099/api/v1/ 2>&1 | Out-String) -match '8099')

# 05: Alegra (lee las credenciales de la USB y verifica la cuenta en modo lectura)
if ($SinAlegra) { Log '05 Alegra omitido (-SinAlegra).' }
else {
    $correo = [Environment]::GetEnvironmentVariable('ALEGRA_EMAIL', 'User')
    if (-not $correo) { Resultado '05 Alegra' $false 'No hay ALEGRA_EMAIL; ejecute el 05 a mano.' }
    else {
        Log '05-Configurar-Alegra.ps1 (modo lectura contra Alegra)'
        & powershell.exe -NoProfile -ExecutionPolicy Bypass -File (Join-Path $Kit '05-Configurar-Alegra.ps1') -Email $correo *>&1 | Out-File (Join-Path $CarpetaRespaldo 'paso-05.log') -Encoding UTF8
        Resultado '05 Alegra: configuracion y verificacion sin errores' ($LASTEXITCODE -eq 0) "codigo $LASTEXITCODE"
    }
}

# 07: impresora termica (solo lectura)
Log '07-Configurar-Impresora-Termica.ps1 -Modo Estado (no cambia nada)'
& powershell.exe -NoProfile -ExecutionPolicy Bypass -File (Join-Path $Kit '07-Configurar-Impresora-Termica.ps1') -Modo Estado *>&1 | Out-File (Join-Path $CarpetaRespaldo 'paso-07.log') -Encoding UTF8
Resultado '07 impresora: estado leido sin errores' ($LASTEXITCODE -eq 0) "codigo $LASTEXITCODE"

# ------------------------------------------------------------------ 5. Abrir el sistema instalado
Paso '5/6  Abriendo el sistema instalado (sin privilegios) y probando su API'
$exe = Join-Path $Instalado 'SISMistico.exe'
& "$env:WINDIR\System32\runas.exe" /trustlevel:0x20000 "`"$exe`"" 2>&1 | Out-Null
$proc = $null
for ($i = 0; $i -lt 40 -and -not $proc; $i++) { Start-Sleep -Seconds 1; $proc = Get-Process SISMistico -ErrorAction SilentlyContinue | Select-Object -First 1 }
Resultado 'El sistema arranca' ([bool]$proc)
$ventana = $false
for ($i = 0; $i -lt 60 -and $proc; $i++) { Start-Sleep -Seconds 1; $proc.Refresh(); if ($proc.MainWindowHandle -ne [IntPtr]::Zero) { $ventana = $true; break } }
Resultado 'Se muestra la ventana (login)' $ventana
if ($ventana) {
    Start-Sleep -Seconds 4
    Add-Type -AssemblyName System.Drawing
    Add-Type -ReferencedAssemblies System.Drawing -TypeDefinition @'
using System; using System.Runtime.InteropServices; using System.Drawing; using System.Drawing.Imaging;
public static class WinShot {
  [StructLayout(LayoutKind.Sequential)] struct RECT { public int L, T, R, B; }
  [DllImport("user32.dll")] static extern bool GetWindowRect(IntPtr h, out RECT r);
  [DllImport("user32.dll")] static extern bool PrintWindow(IntPtr h, IntPtr hdc, uint flags);
  public static void Save(IntPtr h, string path) {
    RECT r; GetWindowRect(h, out r); int w = r.R - r.L, hh = r.B - r.T;
    using (var bmp = new Bitmap(w, hh)) { using (var g = Graphics.FromImage(bmp)) { IntPtr dc = g.GetHdc(); PrintWindow(h, dc, 2); g.ReleaseHdc(dc); } bmp.Save(path, ImageFormat.Png); }
  }
}
'@
    [WinShot]::Save($proc.MainWindowHandle, (Join-Path $CarpetaRespaldo 'app-instalada.png'))
    Log "Captura: $(Join-Path $CarpetaRespaldo 'app-instalada.png')"
}
$clave = Ajuste $cfg 'ApiKey'
$base = 'http://localhost:8099/api/v1'
$ping = $null
for ($i = 0; $i -lt 30 -and -not $ping; $i++) { try { $ping = Invoke-WebRequest "$base/ping" -UseBasicParsing -TimeoutSec 3 } catch { Start-Sleep -Seconds 1 } }
Resultado 'API movil: /ping responde 200' ($ping -and $ping.StatusCode -eq 200)
try {
    $cuerpo = @{ id_empleado = [int]$adm.Id_empleado; codigo_maestra = [int]$adm.Codigo_maestro } | ConvertTo-Json
    $login = Invoke-WebRequest "$base/auth/login" -Method POST -Body $cuerpo -ContentType 'application/json' -Headers @{ 'X-Api-Key' = $clave } -UseBasicParsing -TimeoutSec 20
    Resultado 'API movil: login por PIN del administrador' ($login.StatusCode -eq 200)
} catch { Resultado 'API movil: login por PIN del administrador' $false $_.Exception.Message }
Get-Process SISMistico -ErrorAction SilentlyContinue | ForEach-Object { [void]$_.CloseMainWindow() }
Start-Sleep -Seconds 4
Get-Process SISMistico -ErrorAction SilentlyContinue | Stop-Process -Force
Log 'Sistema cerrado.'

# ------------------------------------------------------------------ 6. Resumen
Paso '6/6  Resumen'
$resultados | Format-Table -AutoSize -Wrap | Out-String -Width 220 | ForEach-Object { $_.Split("`n") } | ForEach-Object { if ($_.Trim()) { Log $_.TrimEnd() } }
$fallas = @($resultados | Where-Object { $_.Resultado -eq 'FALLA' })
$resultados | Export-Csv -LiteralPath (Join-Path $CarpetaRespaldo 'resultados.csv') -NoTypeInformation -Encoding UTF8
if ($fallas.Count -eq 0) { Log ("TODO OK: {0} comprobaciones." -f $resultados.Count) 'Green' }
else { Log ("{0} de {1} comprobaciones FALLARON." -f $fallas.Count, $resultados.Count) 'Red' }
Log "Registro completo: $Registro"
Log "Respaldos de lo anterior (para volver a restaurarlo): $CarpetaRespaldo"
Set-Content -LiteralPath (Join-Path $CarpetaRespaldo 'FIN.txt') -Value ("fallas={0}" -f $fallas.Count)
