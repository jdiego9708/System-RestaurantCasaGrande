<#
    Compara el IL de los ensamblados PUBLICADOS por ClickOnce (publish\Application Files\SISMistico_x_x_x_N)
    con una compilacion local (por defecto SISMistico\SISMistico\bin\LocalBuild). Sirve para saber si
    lo que esta instalado en el restaurante equivale al codigo del repositorio.

    Hace hash del cuerpo IL de cada metodo (carga solo-reflexion, no ejecuta nada). Cada lado se procesa
    en un proceso aparte porque .NET no deja cargar el mismo ensamblado desde dos rutas a la vez.
    Diferencias pequenas pueden venir de otra version del compilador; lo relevante es si cambian
    metodos de negocio (DPedidos, FrmPedido, ...).

    Uso:
        .\Compare-PublishedBuild.ps1
        .\Compare-PublishedBuild.ps1 -Published "...\SISMistico_1_0_0_108" -Local "...\LocalBuild"
#>
param(
    [string]$Published,
    [string]$Local = (Join-Path $PSScriptRoot "..\SISMistico\SISMistico\bin\LocalBuild"),
    [string[]]$Assemblies = @("CapaDatos", "CapaNegocio", "CapaEntidades", "CapaPresentacion", "SISMistico"),
    # Uso interno: modo "volcar hashes de una carpeta" (lo invoca el propio script).
    [string]$DumpDir,
    [string]$OutFile
)

$ErrorActionPreference = "Stop"

# ---------------------------------------------------------------------------
# Modo interno: volcar "ensamblado|tipo::metodo<TAB>hash" de una carpeta
# ---------------------------------------------------------------------------
if ($DumpDir) {
    # Con -File la lista llega como un solo texto "a,b,c".
    $Assemblies = $Assemblies -split ','
    $script:dir = $DumpDir
    [AppDomain]::CurrentDomain.add_ReflectionOnlyAssemblyResolve({
        param($sender, $e)
        $short = ($e.Name -split ',')[0]
        $p = Join-Path $script:dir ($short + ".dll")
        if (-not (Test-Path $p)) { $p = Join-Path $script:dir ($short + ".exe") }
        if (Test-Path $p) { return [Reflection.Assembly]::ReflectionOnlyLoadFrom($p) }
        return [Reflection.Assembly]::ReflectionOnlyLoad($e.Name)
    })

    $flags = [Reflection.BindingFlags]'Instance,Static,Public,NonPublic,DeclaredOnly'
    $sha = [Security.Cryptography.SHA1]::Create()
    $lines = New-Object System.Collections.Generic.List[string]
    foreach ($name in $Assemblies) {
        $path = Join-Path $DumpDir "$name.dll"
        if (-not (Test-Path $path)) { $path = Join-Path $DumpDir "$name.exe" }
        if (-not (Test-Path $path)) { continue }
        $asm = [Reflection.Assembly]::ReflectionOnlyLoadFrom($path)
        try { $types = $asm.GetTypes() }
        catch [Reflection.ReflectionTypeLoadException] { $types = $_.Exception.Types | Where-Object { $_ } }
        foreach ($t in $types) {
            $members = @()
            try { $members += $t.GetMethods($flags); $members += $t.GetConstructors($flags) } catch { continue }
            foreach ($m in $members) {
                try {
                    $body = $m.GetMethodBody()
                    if ($null -eq $body) { continue }
                    $h = [BitConverter]::ToString($sha.ComputeHash($body.GetILAsByteArray()))
                    $lines.Add(("{0}|{1}::{2}`t{3}" -f $name, $t.FullName, $m.ToString(), $h))
                } catch { }
            }
        }
    }
    [IO.File]::WriteAllLines($OutFile, $lines)
    exit 0
}

# ---------------------------------------------------------------------------
# Modo normal
# ---------------------------------------------------------------------------
if (-not $Published) {
    $apps = Join-Path $PSScriptRoot "..\SISMistico\SISMistico\publish\Application Files"
    $Published = Get-ChildItem $apps -Directory |
        Sort-Object { [int]($_.Name -replace '.*_', '') } | Select-Object -Last 1 -ExpandProperty FullName
}
$Local = (Resolve-Path $Local).Path
Write-Host "Publicada: $Published"
Write-Host "Local:     $Local"

# Copia temporal de la publicada sin la extension .deploy (ClickOnce la agrega a cada archivo).
$tmp = Join-Path $env:TEMP ("pubcmp_" + [guid]::NewGuid().ToString("N"))
New-Item -ItemType Directory $tmp | Out-Null
Get-ChildItem $Published -File | ForEach-Object {
    Copy-Item $_.FullName (Join-Path $tmp ($_.Name -replace '\.deploy$', ''))
}

function Get-Dump([string]$dir, [string]$out) {
    $argList = @("-NoProfile", "-File", $PSCommandPath, "-DumpDir", $dir, "-OutFile", $out,
                 "-Assemblies", ($Assemblies -join ","))
    $p = Start-Process powershell.exe -ArgumentList $argList -Wait -PassThru -NoNewWindow
    if ($p.ExitCode -ne 0) { throw "Fallo el volcado de $dir (codigo $($p.ExitCode))" }
    $map = @{}
    foreach ($l in [IO.File]::ReadAllLines($out)) { $kv = $l -split "`t"; $map[$kv[0]] = $kv[1] }
    return $map
}

try {
    $a = Get-Dump $tmp (Join-Path $tmp "_pub.txt")
    $b = Get-Dump $Local (Join-Path $tmp "_loc.txt")
}
finally { }

$byAsm = @{}
foreach ($k in ($a.Keys + $b.Keys | Sort-Object -Unique)) {
    $asm = $k.Split('|')[0]
    if (-not $byAsm.ContainsKey($asm)) { $byAsm[$asm] = @{ same = 0; diff = @(); pub = @(); loc = @() } }
    $r = $byAsm[$asm]
    if ($a.ContainsKey($k) -and $b.ContainsKey($k)) {
        if ($a[$k] -eq $b[$k]) { $r.same++ } else { $r.diff += $k }
    } elseif ($a.ContainsKey($k)) { $r.pub += $k } else { $r.loc += $k }
}

$detail = @()
foreach ($name in $Assemblies) {
    if (-not $byAsm.ContainsKey($name)) { continue }
    $r = $byAsm[$name]
    "{0,-18} iguales={1,5}  distintos={2,4}  solo-publicada={3,3}  solo-local={4,3}" -f $name, $r.same, $r.diff.Count, $r.pub.Count, $r.loc.Count
    $detail += $r.diff | ForEach-Object { "  distinto:       $_" }
    $detail += $r.pub  | ForEach-Object { "  solo publicada: $_" }
    $detail += $r.loc  | ForEach-Object { "  solo local:     $_" }
}
if ($detail.Count -gt 0) {
    Write-Host "`nDiferencias (primeras 60):"
    $detail | Select-Object -First 60
}
Remove-Item $tmp -Recurse -Force
