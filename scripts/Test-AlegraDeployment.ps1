[CmdletBinding()]
param([string]$KitPath)
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'Preparar-USB.ps1')
. (Join-Path $PSScriptRoot 'Configurar-Alegra-Equipo.ps1')
$repo=Split-Path $PSScriptRoot -Parent
$checks=0
function Check([bool]$condition,[string]$message) {
    if(!$condition) { throw $message }
    $script:checks++
}
$desired=@{ALEGRA_API_KEY='bearer-synthetic'; ALEGRA_AUTH_SCHEME='Bearer'}
Assert-AlegraEnvironmentPlan $desired 'User' {param($name) $null}
$checks++
try {
    Assert-AlegraEnvironmentPlan $desired 'User' {param($name) if($name -eq 'ALEGRA_API_KEY') {'different-synthetic'} else {'Bearer'}}
    throw 'No rechazo la credencial Machine que desplaza User.'
} catch {
    Check ($_.Exception.Message -like '*tiene prioridad*') 'Debe identificar la prioridad Machine sin mostrar el valor.'
    Check (!$_.Exception.Message.Contains('different-synthetic')) 'El error expone una credencial.'
}
Assert-AlegraEnvironmentPlan $desired 'Machine' {param($name) 'different-synthetic'}
$checks++
Check ((Get-AlegraCompanyIdentity ([pscustomobject]@{id='test-company'})) -eq 'test-company') 'Identidad por id incorrecta.'
Check ((Get-AlegraCompanyIdentity ([pscustomobject]@{applicationVersion='colombia';identification='123'})) -eq 'colombia:123') 'Identidad alternativa incorrecta.'
$temp=Join-Path $env:TEMP ('CasaGrande-deployment-test-'+[guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $temp | Out-Null
$scriptPath=Join-Path $temp '05.ps1'
$support=@{
    'Configurar-Alegra.ps1'=(Join-Path $repo 'db\Configurar-Alegra.ps1')
    'Test-AlegraPackage.ps1'=(Join-Path $PSScriptRoot 'Test-AlegraPackage.ps1')
}
New-AlegraDeploymentScript (Join-Path $PSScriptRoot 'Configurar-Alegra-Equipo.ps1') $support $scriptPath
$content=[IO.File]::ReadAllText($scriptPath)
$tokens=$null;$errors=$null
[void][Management.Automation.Language.Parser]::ParseFile($scriptPath,[ref]$tokens,[ref]$errors)
Check ($errors.Count -eq 0) 'El paso 05 autocontenido no tiene sintaxis valida.'
$payload=[regex]::Matches($content,"'([^']+\.ps1)' = '([A-Za-z0-9+/=]+)'")
Check ($payload.Count -eq 2) 'El paso 05 no contiene ambos auxiliares.'
foreach($entry in $payload) {
    $name=$entry.Groups[1].Value
    $bytes=[Convert]::FromBase64String($entry.Groups[2].Value)
    Check ([Convert]::ToBase64String($bytes) -eq [Convert]::ToBase64String([IO.File]::ReadAllBytes($support[$name]))) "Auxiliar embebido diferente: $name"
}
# Localización automática de la instalación: carpetas ficticias, sin tocar el sistema real.
$fakeComplete=Join-Path $temp 'SISRestaurante7'
foreach($relative in $script:SisRequiredFiles) {
    $fakeFile=Join-Path $fakeComplete $relative
    New-Item -ItemType Directory -Path (Split-Path $fakeFile -Parent) -Force | Out-Null
    Set-Content -LiteralPath $fakeFile -Value 'x'
}
$fakeOld=Join-Path $temp 'SISRestauranteViejo'
New-Item -ItemType Directory -Path $fakeOld | Out-Null
Set-Content -LiteralPath (Join-Path $fakeOld 'SISMistico.exe') -Value 'x'
$completeInfo=Get-SisInstallationInfo $fakeComplete 'prueba'
Check ($null -ne $completeInfo -and $completeInfo.Complete) 'Una instalacion con todas las herramientas debe verse completa.'
$oldInfo=Get-SisInstallationInfo $fakeOld 'prueba'
Check ($null -ne $oldInfo -and !$oldInfo.Complete -and $oldInfo.Missing.Count -eq ($script:SisRequiredFiles.Count - 1)) 'Una instalacion solo con el .exe debe verse incompleta e indicar que falta.'
Check ($null -eq (Get-SisInstallationInfo (Join-Path $temp 'no-existe') 'prueba')) 'Una carpeta sin SISMistico.exe no es una instalacion.'
Check ($null -eq (Get-SisInstallationInfo '' 'prueba')) 'Una ruta vacia no es una instalacion.'
$candidateList=New-Object System.Collections.ArrayList
$candidateSeen=@{}
Add-SisCandidate $candidateList $candidateSeen $fakeComplete 'a'
Add-SisCandidate $candidateList $candidateSeen ($fakeComplete + '\') 'b'
Add-SisCandidate $candidateList $candidateSeen $fakeOld 'c'
Check ($candidateList.Count -eq 2) 'La misma carpeta encontrada por dos capas no debe duplicarse.'
if($KitPath) {
    foreach($file in @('01-Base-de-Datos.ps1','02-Setup-Casa-Grande.exe','04-APK-Casa-Grande.apk','05-Configurar-Alegra.ps1','06-Cambiar-Codigo-Maestro.ps1','07-Configurar-Impresora-Termica.ps1',
        'Credenciales-Alegra\Token-Nuevo-Bearer.txt','Credenciales-Alegra\Token-Anterior-Basic.txt','00-LEEME.txt')) {
        Check (Test-Path -LiteralPath (Join-Path $KitPath $file) -PathType Leaf) "Falta $file en el kit."
    }
    $kitRoot=[IO.Path]::GetFullPath($KitPath).TrimEnd('\')+'\'
    foreach($line in Get-Content -LiteralPath (Join-Path $KitPath 'SHA256SUMS.txt')) {
        if($line -notmatch '^([A-Fa-f0-9]{64})  (.+)$') { throw 'Manifiesto SHA-256 invalido.' }
        $expected=$Matches[1];$relative=$Matches[2]
        $path=[IO.Path]::GetFullPath((Join-Path $KitPath $relative))
        Check ($path.StartsWith($kitRoot,[StringComparison]::OrdinalIgnoreCase)) 'Ruta de manifiesto fuera del kit.'
        Check ((Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash -eq $expected) "SHA-256 incorrecto: $relative"
    }
    foreach($file in @('01-Base-de-Datos.ps1','05-Configurar-Alegra.ps1')) {
        $tokens=$null;$errors=$null
        [void][Management.Automation.Language.Parser]::ParseFile((Join-Path $KitPath $file),[ref]$tokens,[ref]$errors)
        Check ($errors.Count -eq 0) "Sintaxis del kit incorrecta: $file"
    }
}
Write-Host "OK: $checks comprobaciones de despliegue, sin guardar credenciales ni emitir facturas."
