<#
    Compila SISMistico en Release, arma el instalador (Inno Setup) y lo deja listo para copiar al restaurante.

    Uso (en la maquina de desarrollo, no hace falta administrador):
        .\Publicar-Instalador.ps1                          # deja el instalador en installer\Output\
        .\Publicar-Instalador.ps1 -Destino E:\             # ademas lo copia (con su .sha256 y un LEEME) a la USB / carpeta de red
        .\Publicar-Instalador.ps1 -Version 1.1.0           # numero que ve el usuario en "Agregar o quitar programas"
        .\Publicar-Instalador.ps1 -SinCompilar             # reutiliza installer\build\app (solo reempaqueta)
        .\Publicar-Instalador.ps1 -InstalarInno            # instala Inno Setup con winget si falta

    Que hace:
      1. Compila la solucion en Release a SISMistico\bin\InstallerBuild (NO usa LocalBuild: ese .exe.config esta
         ajustado para desarrollo, con la copia de produccion y la API en localhost).
      2. Copia a installer\build\app solo lo necesario (sin .pdb, sin ClickOnce, sin idiomas que no se usan).
      3. Comprueba que el App.config versionado sea el del restaurante (BD local, API apagada).
      4. Compila installer\SISRestaurante.iss -> installer\Output\SISRestaurante-Setup-<version>.exe
      5. Calcula el SHA-256 y, si se pide, lo copia al destino.
#>
[CmdletBinding()]
param(
    [string]$Version = '1.0.0',
    [string]$Destino,
    [switch]$SinCompilar,
    [switch]$InstalarInno
)

$ErrorActionPreference = 'Stop'
$aqui = $PSScriptRoot
$repo = Split-Path $aqui -Parent
$sisDir = Join-Path $repo 'SISMistico'
$solucion = Join-Path $sisDir 'SISMistico.sln'
$salidaProyecto = 'bin\InstallerBuild\'
$exeDir = Join-Path $sisDir "SISMistico\$salidaProyecto"
$stage = Join-Path $aqui 'build\app'
$output = Join-Path $aqui 'Output'

function Paso($t) { Write-Host "==> $t" -ForegroundColor Cyan }
function Falla($t) { Write-Host $t -ForegroundColor Red; exit 1 }

# ---------- 1. Herramientas ----------
function Buscar-Iscc {
    $candidatos = @(
        (Join-Path ${env:ProgramFiles(x86)} 'Inno Setup 6\ISCC.exe'),
        (Join-Path $env:ProgramFiles 'Inno Setup 6\ISCC.exe'),
        (Join-Path $env:LOCALAPPDATA 'Programs\Inno Setup 6\ISCC.exe')
    )
    foreach ($c in $candidatos) { if (Test-Path $c) { return $c } }
    $cmd = Get-Command ISCC.exe -ErrorAction SilentlyContinue
    if ($cmd) { return $cmd.Source }
    return $null
}

$iscc = Buscar-Iscc
if (-not $iscc -and $InstalarInno) {
    Paso 'Instalando Inno Setup con winget'
    & winget install --id JRSoftware.InnoSetup -e --silent --accept-package-agreements --accept-source-agreements
    $iscc = Buscar-Iscc
}
if (-not $iscc) { Falla "No encontre Inno Setup 6. Instalelo (winget install JRSoftware.InnoSetup) o repita con -InstalarInno." }

# ---------- 2. Compilar ----------
if (-not $SinCompilar) {
    Paso 'Compilando SISMistico en Release'
    $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
    if (-not (Test-Path $vswhere)) { Falla 'No encontre vswhere.exe (Visual Studio / Build Tools).' }
    $msbuild = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
    if (-not $msbuild) { Falla 'No encontre MSBuild. Instale Visual Studio Build Tools con la carga ".NET desktop build tools".' }

    # Reference assemblies de .NET Framework 4.8: Developer Pack o paquete NuGet (igual que run-local.ps1).
    $estandar = Join-Path ${env:ProgramFiles(x86)} 'Reference Assemblies\Microsoft\Framework'
    $raiz = $null
    if (Test-Path (Join-Path $estandar '.NETFramework\v4.8\mscorlib.dll')) { $raiz = $estandar }
    else {
        foreach ($pkgRoot in @((Join-Path $env:USERPROFILE '.nuget\packages\microsoft.netframework.referenceassemblies.net48'),
                               (Join-Path $env:LOCALAPPDATA 'Temp\opencode\net48refs'))) {
            $pkg = Get-ChildItem $pkgRoot -Directory -ErrorAction SilentlyContinue |
                Where-Object { Test-Path (Join-Path $_.FullName 'build\.NETFramework\v4.8\mscorlib.dll') } | Select-Object -First 1
            if ($pkg) { $raiz = Join-Path $pkg.FullName 'build'; break }
        }
    }
    if (-not $raiz) { Falla 'Faltan los reference assemblies de .NET Framework 4.8 (instale el Developer Pack o el paquete NuGet; ver run-local.ps1).' }

    & $msbuild $solucion /t:Rebuild /p:Configuration=Release '/p:Platform=Any CPU' "/p:OutputPath=$salidaProyecto" `
        "/p:TargetFrameworkRootPath=$raiz\" /v:m /nologo
    if ($LASTEXITCODE -ne 0) { Falla "La compilacion fallo (codigo $LASTEXITCODE)." }

    # ---------- 3. Preparar la carpeta que empaqueta el instalador ----------
    Paso 'Preparando installer\build\app'
    if (Test-Path $stage) { Remove-Item $stage -Recurse -Force }
    New-Item -ItemType Directory -Path $stage | Out-Null
    # Fuera: simbolos, documentacion de EntityFramework, restos de ClickOnce, logs de pruebas e idiomas del
    # ReportViewer que el restaurante no usa (se conserva es\).
    $null = & robocopy $exeDir $stage /E /NFL /NDL /NJH /NJS /NP `
        /XF '*.pdb' 'EntityFramework*.xml' 'SISMistico.application' 'SISMistico.exe.manifest' `
        /XD 'app.publish' 'logs' 'de' 'fr' 'it' 'ja' 'ko' 'pt' 'ru' 'zh-CHS' 'zh-CHT'
    if ($LASTEXITCODE -ge 8) { Falla "robocopy fallo (codigo $LASTEXITCODE)." }
}

if (-not (Test-Path (Join-Path $stage 'SISMistico.exe'))) { Falla "No hay nada que empaquetar en $stage. Quite -SinCompilar." }

# ---------- 4. Verificar la configuracion que se va a instalar ----------
Paso 'Verificando SISMistico.exe.config'
[xml]$cfg = Get-Content (Join-Path $stage 'SISMistico.exe.config') -Raw
function Ajuste($k) { ($cfg.configuration.appSettings.add | Where-Object { $_.key -eq $k }).value }
if ((Ajuste 'ConexionBaseDeDatos') -ne 'conexionLocalBD') {
    Falla "ConexionBaseDeDatos es '$(Ajuste 'ConexionBaseDeDatos')': el instalador debe salir con 'conexionLocalBD' (la copia de produccion es solo para desarrollo)."
}
if ((Ajuste 'ApiHabilitada') -ne 'false' -or (Ajuste 'ApiKey') -ne 'cambiar-esta-clave') {
    Falla 'El App.config versionado debe llevar ApiHabilitada=false y la ApiKey de ejemplo: el instalador es quien la activa y la define en cada equipo.'
}
Write-Host '    OK  BD local y API apagada (el instalador la activa si se marca la opcion)' -ForegroundColor Green

# ---------- 5. Instalador ----------
Paso "Compilando el instalador $Version"
Push-Location $aqui
try {
    & $iscc "/DAppVersion=$Version" '/DSourceDir=build\app' 'SISRestaurante.iss'
    if ($LASTEXITCODE -ne 0) { Falla "ISCC fallo (codigo $LASTEXITCODE)." }
} finally { Pop-Location }

$setup = Join-Path $output "SISRestaurante-Setup-$Version.exe"
if (-not (Test-Path $setup)) { Falla "No se genero $setup." }
$hash = (Get-FileHash $setup -Algorithm SHA256).Hash
Set-Content -Path "$setup.sha256" -Value "$hash  $(Split-Path $setup -Leaf)" -Encoding ASCII
$mb = [math]::Round((Get-Item $setup).Length / 1MB, 1)

# ---------- 6. Copiar al destino ----------
$final = $setup
if ($Destino) {
    Paso "Copiando a $Destino"
    if (-not (Test-Path $Destino)) { New-Item -ItemType Directory -Path $Destino -Force | Out-Null }
    Copy-Item $setup $Destino -Force
    Copy-Item "$setup.sha256" $Destino -Force
    $leeme = @"
INSTALAR SISRESTAURANTE (Casa Grande)  -  version $Version
==========================================================
1. Copie SISRestaurante-Setup-$Version.exe al PC del restaurante.
2. Doble clic y acepte el aviso de administrador.
   (Windows puede mostrar "SmartScreen": Mas informacion > Ejecutar de todos modos; el instalador no esta firmado.)
3. Marque "Activar la API movil" si los meseros van a usar celular y anote la CLAVE que muestra el instalador.
4. Requisitos del PC: Windows 10/11, .NET Framework 4.8 y SQL Server Express LocalDB con la base CasaGrandeBD.
5. Si antes estaba instalada la version ClickOnce, desinstalela en Configuracion > Aplicaciones para no tener dos accesos.

Actualizar: ejecute el instalador nuevo encima; conserva el SISMistico.exe.config (clave, imagenes, conexion).
Instalacion silenciosa:  SISRestaurante-Setup-$Version.exe /VERYSILENT /TASKS="api" /ApiKey=SuClaveLarga /ApiPort=8099

Verificacion (SHA-256):  $hash
"@
    Set-Content -Path (Join-Path $Destino 'LEEME-INSTALACION.txt') -Value $leeme -Encoding UTF8
    $final = Join-Path $Destino (Split-Path $setup -Leaf)
}

Write-Host ''
Write-Host ('=' * 64) -ForegroundColor Green
Write-Host " INSTALADOR LISTO ($mb MB)" -ForegroundColor Green
Write-Host "   $final"
Write-Host "   SHA-256: $hash"
Write-Host ('=' * 64) -ForegroundColor Green
Write-Host ' Copielo al PC del restaurante y ejecutelo (pide administrador).'
if (-not $Destino) { Write-Host " Para dejarlo en una USB o carpeta de red: .\Publicar-Instalador.ps1 -SinCompilar -Destino E:\" }
