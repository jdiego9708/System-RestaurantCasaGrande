<# Paso 05 de la USB. Importa los dos TXT, verifica la empresa y configura la BD sin emitir facturas. #>
[CmdletBinding()]
param(
    [string]$Database='CasaGrandeBD', [string]$Instance='(localdb)\MSSQLLocalDB',
    [string]$Email, [string]$BearerFile, [string]$BasicFile,
    [ValidateSet('User','Machine')][string]$Scope='User',
    # Vacío = localizar la instalación automáticamente (registro, procesos, accesos directos, carpetas y unidades).
    [string]$ApplicationPath,
    [switch]$NoDeepSearch,
    [switch]$CasaGrandeProfile=$true, [switch]$VerifyOnly
)
$ErrorActionPreference='Stop'
$embeddedAlegraFiles = @{} # USB_ALEGRA_PAYLOAD

function Read-AlegraValue([string]$name) {
    foreach($target in @('Machine','User','Process')) {
        $value=[Environment]::GetEnvironmentVariable($name,$target)
        if($null -ne $value) { return $value }
    }
}
function Get-AlegraCompanyIdentity($company) {
    if($company.id) { return [string]$company.id }
    $number=[string]$company.identificationObject.number
    if(!$number) { $number=[string]$company.identification }
    if(!$number) { throw 'La API no devolvio identificacion de empresa.' }
    return [string]$company.applicationVersion+':'+$number.Trim()
}
function Assert-AlegraEnvironmentPlan($values,[string]$target,[scriptblock]$readMachine) {
    if($target -ne 'User') { return }
    foreach($name in $values.Keys) {
        $machine=& $readMachine $name
        $different=if($name -like '*_API_KEY') { $machine -cne $values[$name] } else { $machine -ine $values[$name] }
        if($null -ne $machine -and $different) {
            throw "Existe $name diferente en el entorno Machine, que tiene prioridad. Ejecute este paso elevado con el MISMO usuario de Windows y -Scope Machine, o actualice esa credencial en el instalador. No se guardo ningun valor."
        }
    }
}

# Archivos que debe tener una instalación con las herramientas de Alegra (subconjunto de Test-AlegraPackage.ps1).
$script:SisRequiredFiles = @('SISMistico.exe', 'CapaNegocio.dll', 'Alegra\Tools\SumatraPDF.exe', 'Alegra\Tools\Renderer\CasaGrandeFiscalPdf.exe')

# Devuelve datos de la carpeta si contiene SISMistico.exe (y si está completa); si no, $null.
function Get-SisInstallationInfo([string]$Folder, [string]$Source) {
    if([string]::IsNullOrWhiteSpace($Folder)) { return $null }
    try { $full=[IO.Path]::GetFullPath($Folder.Trim().Trim('"')) } catch { return $null }
    $exe=Join-Path $full 'SISMistico.exe'
    if(!(Test-Path -LiteralPath $exe -PathType Leaf)) { return $null }
    $missing=@($script:SisRequiredFiles | Where-Object { !(Test-Path -LiteralPath (Join-Path $full $_) -PathType Leaf) })
    return [pscustomobject]@{
        Path=$full; Source=$Source; Complete=($missing.Count -eq 0); Missing=$missing
        Modified=(Get-Item -LiteralPath $exe).LastWriteTime
    }
}

function Add-SisCandidate([System.Collections.ArrayList]$List, [hashtable]$Seen, [string]$Folder, [string]$Source) {
    $info=Get-SisInstallationInfo $Folder $Source
    if($null -eq $info) { return }
    $key=$info.Path.TrimEnd('\').ToLowerInvariant()
    if($Seen.ContainsKey($key)) { return }
    $Seen[$key]=$true
    [void]$List.Add($info)
}

# Busca SISMistico.exe por capas, de la más fiable a la más costosa. La búsqueda por unidades solo se hace
# si ninguna capa anterior encontró una instalación completa, y es de profundidad limitada.
function Find-SisInstallations([switch]$NoDeepSearch) {
    $list=New-Object System.Collections.ArrayList
    $seen=@{}

    # 1. Registro de desinstalación de Windows (lo escribe el instalador, con la carpeta elegida por el usuario).
    $uninstallRoots=@('HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall',
        'HKLM:\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall',
        'HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall')
    foreach($root in $uninstallRoots) {
        if(!(Test-Path $root)) { continue }
        foreach($key in @(Get-ChildItem $root -ErrorAction SilentlyContinue)) {
            $props=Get-ItemProperty $key.PSPath -ErrorAction SilentlyContinue
            if($null -eq $props) { continue }
            if(!(([string]$props.DisplayName) -like 'SISRestaurante*' -or $key.PSChildName -like '{B7C0E4F2-6A1D-4C53-9E55-0C4A5D7A2F10}*')) { continue }
            Add-SisCandidate $list $seen ([string]$props.InstallLocation) 'registro de Windows'
            $uninstaller=([string]$props.UninstallString).Trim().Trim('"')
            if($uninstaller) { Add-SisCandidate $list $seen (Split-Path $uninstaller -Parent -ErrorAction SilentlyContinue) 'registro de Windows' }
        }
    }

    # 2. Programa en ejecución.
    foreach($process in @(Get-Process -Name SISMistico -ErrorAction SilentlyContinue)) {
        $processPath=$null
        try { $processPath=$process.Path } catch { }
        if($processPath) { Add-SisCandidate $list $seen (Split-Path $processPath -Parent) 'programa en ejecución' }
    }

    # 3. Accesos directos del menú Inicio y del escritorio.
    try {
        $shell=New-Object -ComObject WScript.Shell
        $shortcutFolders=@([Environment]::GetFolderPath('CommonPrograms'), [Environment]::GetFolderPath('Programs'),
            [Environment]::GetFolderPath('CommonDesktopDirectory'), [Environment]::GetFolderPath('DesktopDirectory'))
        foreach($folder in ($shortcutFolders | Where-Object { $_ -and (Test-Path -LiteralPath $_) } | Select-Object -Unique)) {
            foreach($link in @(Get-ChildItem -LiteralPath $folder -Filter *.lnk -Recurse -ErrorAction SilentlyContinue)) {
                try {
                    $target=$shell.CreateShortcut($link.FullName).TargetPath
                    if($target -like '*SISMistico.exe') { Add-SisCandidate $list $seen (Split-Path $target -Parent) 'acceso directo' }
                } catch { }
            }
        }
    } catch { }

    # 4. Carpetas habituales: cubre nombres como SISRestaurante, SISRestaurante1 o «Casa Grande».
    $programRoots=@($env:ProgramFiles, ${env:ProgramFiles(x86)}, $env:ProgramW6432, (Join-Path $env:LOCALAPPDATA 'Programs')) |
        Where-Object { $_ -and (Test-Path -LiteralPath $_) } | Select-Object -Unique
    foreach($root in $programRoots) {
        foreach($dir in @(Get-ChildItem -LiteralPath $root -Directory -ErrorAction SilentlyContinue |
            Where-Object { $_.Name -like 'SISRestaurante*' -or $_.Name -like 'SISMistico*' -or $_.Name -like 'Casa*Grande*' })) {
            Add-SisCandidate $list $seen $dir.FullName 'carpeta de programas'
        }
    }

    # 5. Búsqueda por las unidades locales fijas, hasta 3 niveles, solo si todavía no hay una instalación completa.
    $hasComplete=@($list | Where-Object { $_.Complete }).Count -gt 0
    if(!$hasComplete -and !$NoDeepSearch) {
        Write-Host 'No hay una instalacion completa en las ubicaciones habituales; buscando en las unidades locales (puede tardar un momento)...'
        $skip=@('Windows', '$Recycle.Bin', 'System Volume Information', '$WinREAgent', 'Recovery', 'node_modules', '.git')
        foreach($disk in @(Get-CimInstance Win32_LogicalDisk -Filter 'DriveType=3' -ErrorAction SilentlyContinue)) {
            $level=@($disk.DeviceID + '\')
            for($depth=0; $depth -le 3 -and $level.Count -gt 0; $depth++) {
                $next=@()
                foreach($dir in $level) {
                    Add-SisCandidate $list $seen $dir 'busqueda en unidad'
                    if($depth -lt 3) {
                        $next += @(Get-ChildItem -LiteralPath $dir -Directory -Force -ErrorAction SilentlyContinue |
                            Where-Object { $skip -notcontains $_.Name -and !($_.Attributes -band [IO.FileAttributes]::ReparsePoint) } |
                            ForEach-Object { $_.FullName })
                    }
                }
                $level=$next
            }
        }
    }
    return $list.ToArray()
}

# Elige la instalación completa más reciente. Si no hay ninguna, explica qué se encontró y qué falta.
function Resolve-SisInstallation([switch]$NoDeepSearch) {
    Write-Host 'Buscando la instalacion de Casa Grande (SISMistico.exe)...'
    $found=@(Find-SisInstallations -NoDeepSearch:$NoDeepSearch)
    foreach($item in $found) {
        $state=if($item.Complete) { 'completa' } else { 'INCOMPLETA (falta: ' + ($item.Missing -join ', ') + ')' }
        Write-Host ('  - {0}  [{1}; {2}; {3}]' -f $item.Path, $item.Source, $item.Modified.ToString('yyyy-MM-dd HH:mm'), $state)
    }
    $complete=@($found | Where-Object { $_.Complete } | Sort-Object Modified -Descending)
    if($complete.Count -eq 0) {
        if($found.Count -gt 0) {
            throw 'Se encontro SISMistico.exe pero ninguna instalacion tiene las herramientas de Alegra: es de una version anterior. Ejecute el 02-Setup-Casa-Grande.exe de esta USB y repita este paso.'
        }
        throw 'No se encontro SISMistico.exe en este equipo. Ejecute primero el 02-Setup-Casa-Grande.exe o indique la carpeta con -ApplicationPath.'
    }
    if($complete.Count -gt 1) {
        Write-Warning ('Hay {0} instalaciones completas; se usa la mas reciente. Para otra, use -ApplicationPath.' -f $complete.Count)
    }
    Write-Host ('Instalacion seleccionada: ' + $complete[0].Path) -ForegroundColor Green
    return $complete[0].Path
}

# Permite pruebas de funciones con datos ficticios, sin importar credenciales ni consultar la red.
if($MyInvocation.InvocationName -eq '.') { return }
if($Database -notmatch '^[A-Za-z0-9_]+$') { throw 'Nombre de base invalido.' }
if($ApplicationPath) { Write-Host ('Usando la carpeta indicada: ' + $ApplicationPath) }
else { $ApplicationPath=Resolve-SisInstallation -NoDeepSearch:$NoDeepSearch }
if(!$BearerFile) { $BearerFile=Join-Path $PSScriptRoot 'Credenciales-Alegra\Token-Nuevo-Bearer.txt' }
if(!$BasicFile) { $BasicFile=Join-Path $PSScriptRoot 'Credenciales-Alegra\Token-Anterior-Basic.txt' }
foreach($file in @($BearerFile,$BasicFile)) {
    if(!(Test-Path -LiteralPath $file -PathType Leaf)) { throw "No existe el archivo de credencial: $file" }
}
$bearer=[IO.File]::ReadAllText($BearerFile).Trim()
$basic=[IO.File]::ReadAllText($BasicFile).Trim()
if(!$bearer -or !$basic -or $bearer -match '\s' -or $basic -match '\s') { throw 'Los TXT deben contener un token no vacio, sin espacios interiores.' }
if(!$Email) { $Email=Read-AlegraValue 'ALEGRA_EMAIL' }
if(!$Email -and !$VerifyOnly) { $Email=Read-Host 'Correo de la cuenta API de Alegra' }
if(!$Email -or $Email.IndexOfAny([char[]]":`r`n") -ge 0) { throw 'Indique un correo API valido con -Email.' }
$values=@{
    ALEGRA_EMAIL=$Email.Trim(); ALEGRA_API_KEY=$bearer; ALEGRA_AUTH_SCHEME='Bearer';
    ALEGRA_EMISSION_EMAIL=$Email.Trim(); ALEGRA_EMISSION_API_KEY=$basic; ALEGRA_EMISSION_AUTH_SCHEME='Basic'
}
Assert-AlegraEnvironmentPlan $values $Scope { param($n) [Environment]::GetEnvironmentVariable($n,'Machine') }
if($Scope -eq 'Machine' -and !$VerifyOnly) {
    $identity=[Security.Principal.WindowsIdentity]::GetCurrent()
    if(!(New-Object Security.Principal.WindowsPrincipal $identity).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
        throw '-Scope Machine necesita PowerShell elevado con el mismo usuario de Windows.'
    }
}
$scratch=$null
try {
    if($embeddedAlegraFiles.Count) {
        $scratch=Join-Path $env:TEMP ('CasaGrande-Alegra-setup-'+[guid]::NewGuid().ToString('N'))
        New-Item -ItemType Directory -Path $scratch | Out-Null
        foreach($entry in $embeddedAlegraFiles.GetEnumerator()) {
            if([IO.Path]::GetFileName($entry.Key) -ne $entry.Key) { throw 'Nombre de soporte invalido.' }
            [IO.File]::WriteAllBytes((Join-Path $scratch $entry.Key),[Convert]::FromBase64String($entry.Value))
        }
        $configure=Join-Path $scratch 'Configurar-Alegra.ps1'
        $check=Join-Path $scratch 'Test-AlegraPackage.ps1'
    } else {
        $configure=Join-Path (Split-Path $PSScriptRoot -Parent) 'db\Configurar-Alegra.ps1'
        $check=Join-Path $PSScriptRoot 'Test-AlegraPackage.ps1'
    }
    & $check -ApplicationPath $ApplicationPath -RunRendererSelfTest
    [Net.ServicePointManager]::SecurityProtocol=[Net.SecurityProtocolType]::Tls12
    $auth=@(('Bearer '+$bearer), ('Basic '+[Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($Email.Trim()+':'+$basic))))
    $companies=@()
    foreach($authorization in $auth) {
        try { $company=Invoke-RestMethod -Uri 'https://api.alegra.com/api/v1/company' -Headers @{Authorization=$authorization} -TimeoutSec 45 }
        catch { throw 'Alegra no permitio verificar una de las credenciales. Revise correo, tokens, conexion y permisos; no se emitieron facturas.' }
        $companies+=Get-AlegraCompanyIdentity $company
    }
    if($companies[0] -ne $companies[1]) { throw 'Los dos tokens pertenecen a empresas diferentes. No se guardaron credenciales.' }
    if($VerifyOnly) {
        foreach($name in $values.Keys) {
            $current=Read-AlegraValue $name
            $different=if($name -like '*_API_KEY') { $current -cne $values[$name] } else { $current -ine $values[$name] }
            if($different) { throw "El valor activo de $name no coincide con los archivos/esquemas previstos. Ejecute 05 sin -VerifyOnly." }
        }
    } else {
        foreach($name in $values.Keys) {
            [Environment]::SetEnvironmentVariable($name,$values[$name],$Scope)
            [Environment]::SetEnvironmentVariable($name,$values[$name],'Process')
        }
        Write-Host "Credenciales guardadas en entorno $Scope; no se mostraron sus valores."
    }
    & $configure -Instance $Instance -Database $Database -CasaGrandeProfile:$CasaGrandeProfile -VerifyOnly:$VerifyOnly
    Write-Host 'Alegra preparado. Reinicie el escritorio. Descargar PDF conserva el oficial; imprimir usa el modelo de 80 mm.'
    Write-Host 'La comprobacion fue de lectura; el permiso de emision se confirma con una venta real marcada.'
} finally {
    $bearer=$null; $basic=$null; $values=$null; $auth=$null; $authorization=$null
    # Solo archivos de soporte sin secretos, en una carpeta temporal creada por esta ejecucion.
    if($scratch -and (Test-Path -LiteralPath $scratch)) {
        $resolved=[IO.Path]::GetFullPath($scratch)
        $tempRoot=[IO.Path]::GetFullPath($env:TEMP).TrimEnd('\')+'\'
        if($resolved.StartsWith($tempRoot,[StringComparison]::OrdinalIgnoreCase) -and (Split-Path $resolved -Leaf) -like 'CasaGrande-Alegra-setup-*') {
            Remove-Item -LiteralPath $resolved -Recurse -Force
        }
    }
}
