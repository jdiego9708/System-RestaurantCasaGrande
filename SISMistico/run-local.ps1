<#
    Ejecuta SISMistico (Restaurante Casa Grande) en local, SIN instalar (sin ClickOnce).

    - Compila a SISMistico\SISMistico\bin\LocalBuild\ (carpeta ignorada por git)
    - Usa la copia local de PRODUCCION: (localdb)\MSSQLLocalDB -> CasaGrandeBD_prod (db\Restore-ProdCopy.ps1).
      Con -UseDevDb usa la CasaGrandeBD de desarrollo. El cambio se hace en el .exe.config de la compilacion
      local; el App.config versionado (el que se publica al restaurante) no se toca.
    - Activa la API movil solo en esta compilacion, escuchando en http://localhost:8099/api/v1/ (-NoApi la apaga)
    - No toca la carpeta publish\ ni crea accesos directos ni entradas de "Agregar o quitar programas"

    Uso:
        .\run-local.ps1              # compila si hace falta y abre la aplicacion
        .\run-local.ps1 -Rebuild     # fuerza recompilacion y abre
        .\run-local.ps1 -NoRun       # solo compila / valida el entorno

    Requisitos: .NET Framework 4.8 (runtime), SQL Server LocalDB, MSBuild (VS 2022 o Build Tools).
#>
param(
    [switch]$Rebuild,
    [switch]$NoRun,
    [string]$Configuration = "Debug",
    # Por defecto usa la copia local de PRODUCCION (CasaGrandeBD_prod, ver db\Restore-ProdCopy.ps1).
    # -UseDevDb vuelve a la CasaGrandeBD de desarrollo (esquema distinto al real).
    [switch]$UseDevDb,
    # API movil: se activa solo en la compilacion local, escuchando en localhost (no pide urlacl ni admin).
    [switch]$NoApi,
    [string]$ApiPrefix = "http://localhost:8099/api/v1/"
)

$ErrorActionPreference = "Stop"
$root = $PSScriptRoot
$solution = Join-Path $root "SISMistico.sln"
$outputDir = Join-Path $root ("SISMistico\bin\LocalBuild")
$exe = Join-Path $outputDir "SISMistico.exe"

function Write-Step($msg) { Write-Host "==> $msg" -ForegroundColor Cyan }

# Cadena de conexion a usar: el nombre se lee de SISMistico\App.config y la BD sale de su Initial Catalog.
# El App.config versionado sigue apuntando a conexionLocalBD (es la que se publica al restaurante);
# el cambio se hace solo en el .exe.config de la compilacion local (ver el paso 2b).
$connName = if ($UseDevDb) { "conexionLocalBD" } else { "conexionLocalBDProd" }
[xml]$appConfig = Get-Content (Join-Path $root "SISMistico\App.config") -Raw
$connNode = $appConfig.configuration.connectionStrings.add | Where-Object { $_.name -eq $connName }
if (-not $connNode) { Write-Host "App.config no tiene la cadena '$connName'." -ForegroundColor Red; exit 1 }
$dbName = ([regex]::Match($connNode.connectionString, 'Initial Catalog=([^;]+)')).Groups[1].Value

# ---------------------------------------------------------------------------
# 1. LocalDB
# ---------------------------------------------------------------------------
Write-Step "Verificando SQL Server LocalDB"
$localdb = Get-Command sqllocaldb -ErrorAction SilentlyContinue
if (-not $localdb) {
    Write-Host "No se encontro 'sqllocaldb'. Instala SQL Server Express LocalDB." -ForegroundColor Red
    exit 1
}

$info = (& sqllocaldb info 2>&1) -split "`r?`n" | Where-Object { $_.Trim() -ne "" }
if ($info -notcontains "MSSQLLocalDB") {
    Write-Host "La instancia MSSQLLocalDB no existe. Creala con: sqllocaldb create MSSQLLocalDB" -ForegroundColor Red
    exit 1
}
& sqllocaldb start MSSQLLocalDB | Out-Null

Add-Type -AssemblyName "System.Data"
$dbOk = $false
try {
    $cn = New-Object System.Data.SqlClient.SqlConnection "Server=(localdb)\MSSQLLocalDB;Database=master;Integrated Security=true;Connection Timeout=20;"
    $cn.Open()
    $cmd = $cn.CreateCommand()
    $cmd.CommandText = "SELECT COUNT(*) FROM sys.databases WHERE name = '$dbName'"
    $dbOk = ([int]$cmd.ExecuteScalar() -gt 0)
    $cn.Close()
} catch {
    Write-Host "No fue posible conectar con LocalDB: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

if ($dbOk) {
    Write-Host "Base de datos $dbName disponible (cadena $connName)." -ForegroundColor Green
} else {
    Write-Host "ATENCION: no existe la base de datos $dbName en (localdb)\MSSQLLocalDB." -ForegroundColor Yellow
    Write-Host "Para la copia de produccion: .\db\Restore-ProdCopy.ps1 (en la raiz del repositorio)." -ForegroundColor Yellow
}

# ---------------------------------------------------------------------------
# 2. Compilar si hace falta
# ---------------------------------------------------------------------------
if ($Rebuild -or -not (Test-Path $exe)) {
    Write-Step "Compilando solucion ($Configuration)"

    $vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
    if (-not (Test-Path $vswhere)) {
        Write-Host "No se encontro vswhere.exe (Visual Studio / Build Tools)." -ForegroundColor Red
        exit 1
    }
    $msbuild = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1
    if (-not $msbuild) {
        Write-Host "No se encontro MSBuild. Instala Visual Studio Build Tools con la carga '.NET desktop build tools'." -ForegroundColor Red
        exit 1
    }

    # .NET Framework 4.8 reference assemblies:
    #  - Developer Pack instalado -> carpeta estandar
    #  - si no, paquete NuGet microsoft.netframework.referenceassemblies.net48
    $targetFrameworkRoot = $null
    $standard = Join-Path ${env:ProgramFiles(x86)} "Reference Assemblies\Microsoft\Framework"
    if (Test-Path (Join-Path $standard ".NETFramework\v4.8\mscorlib.dll")) {
        $targetFrameworkRoot = $standard
    } else {
        $pkgRoot = Join-Path $env:USERPROFILE ".nuget\packages\microsoft.netframework.referenceassemblies.net48"
        if (-not (Test-Path $pkgRoot)) {
            $pkgRoot = Join-Path $env:LOCALAPPDATA "Temp\opencode\net48refs"
        }
        $pkg = Get-ChildItem $pkgRoot -Directory -ErrorAction SilentlyContinue |
            Where-Object { Test-Path (Join-Path $_.FullName "build\.NETFramework\v4.8\mscorlib.dll") } |
            Select-Object -First 1
        if ($pkg) {
            $targetFrameworkRoot = Join-Path $pkg.FullName "build"
            Write-Host "Usando reference assemblies de NuGet: $targetFrameworkRoot"
        }
    }
    if (-not $targetFrameworkRoot) {
        Write-Host "Faltan los reference assemblies de .NET Framework 4.8." -ForegroundColor Red
        Write-Host "Instala el '.NET Framework 4.8 Developer Pack' o ejecuta:" -ForegroundColor Yellow
        Write-Host "  nuget install Microsoft.NETFramework.ReferenceAssemblies.net48 -OutputDirectory `$env:USERPROFILE\.nuget\packages" -ForegroundColor Yellow
        exit 1
    }

    $msbuildArgs = @($solution, "/t:Build", "/p:Configuration=$Configuration", "/p:Platform=Any CPU",
              "/p:OutputPath=bin\LocalBuild\", "/p:TargetFrameworkRootPath=$targetFrameworkRoot\",
              "/v:m", "/nologo")
    & $msbuild @msbuildArgs
    if ($LASTEXITCODE -ne 0) {
        Write-Host "La compilacion fallo (codigo $LASTEXITCODE)." -ForegroundColor Red
        exit $LASTEXITCODE
    }
    Write-Host "Compilacion OK." -ForegroundColor Green
}

# ---------------------------------------------------------------------------
# 2b. Ajustar el .exe.config de la compilacion local (el App.config versionado no se toca)
# ---------------------------------------------------------------------------
$exeConfig = "$exe.config"
if (Test-Path $exeConfig) {
    [xml]$cfg = Get-Content $exeConfig -Raw
    function Set-AppSetting([xml]$doc, [string]$key, [string]$value) {
        $node = $doc.configuration.appSettings.add | Where-Object { $_.key -eq $key }
        if (-not $node) {
            $node = $doc.CreateElement("add")
            [void]$node.SetAttribute("key", $key)
            [void]$doc.configuration.appSettings.AppendChild($node)
        }
        $node.SetAttribute("value", $value)
    }
    Set-AppSetting $cfg "ConexionBaseDeDatos" $connName
    if (-not $NoApi) {
        Set-AppSetting $cfg "ApiHabilitada" "true"
        Set-AppSetting $cfg "ApiPrefijo" $ApiPrefix
    }
    $cfg.Save($exeConfig)
    $apiInfo = if ($NoApi) { "" } else { "; API en $ApiPrefix" }
    Write-Host "Config local: ConexionBaseDeDatos=$connName$apiInfo"
}

if ($NoRun) {
    Write-Host "Listo. Ejecutable: $exe"
    exit 0
}

# ---------------------------------------------------------------------------
# 3. Ejecutar
# ---------------------------------------------------------------------------
if (Get-Process SISMistico -ErrorAction SilentlyContinue) {
    Write-Host "Ya hay una instancia de SISMistico abierta. Cierrala antes de abrir otra." -ForegroundColor Yellow
    exit 0
}

Write-Step "Abriendo SISMistico"
Start-Process -FilePath $exe -WorkingDirectory $outputDir
