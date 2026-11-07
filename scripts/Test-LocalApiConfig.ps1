$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'Local-ApiConfig.ps1')
$testDirectory = Join-Path $env:TEMP ('CasaGrande-local-api-' + [guid]::NewGuid().ToString('N'))
[void](New-Item -ItemType Directory -Path $testDirectory)
$settingsPath = Join-Path $testDirectory 'run-local.api.config'
$outputPath = Join-Path $testDirectory 'SISMistico.exe.config'
$checks = 0
function Assert-Setting($Condition, $Message) {
    if (-not $Condition) { throw $Message }
    $script:checks++
}
try {
    $fixture = '<configuration><appSettings><add key="ApiPrefijo" value="http://localhost:8099/api/v1/"/><add key="ApiKey" value="clave-ficticia-de-prueba"/><add key="OtroAjuste" value="conservar"/></appSettings></configuration>'
    [IO.File]::WriteAllText($outputPath, $fixture)
    $settings = Get-LocalApiSettings $settingsPath $outputPath
    Assert-Setting ($settings.ApiPrefijo -eq 'http://+:8099/api/v1/') 'La primera ejecucion debe migrar localhost a LAN.'
    Assert-Setting ($settings.ApiKey -eq 'clave-ficticia-de-prueba') 'Debe conservar la clave local existente.'
    Save-LocalApiSettings $settingsPath $settings

    # MSBuild reemplaza la configuracion por la plantilla; Rebuild puede borrar la salida.
    foreach ($cycle in 1..2) {
        Remove-Item -LiteralPath $outputPath
        $restored = Get-LocalApiSettings $settingsPath $outputPath
        Assert-Setting ($restored.ApiKey -eq $settings.ApiKey) 'La clave debe sobrevivir a la limpieza de bin.'
        [xml]$generated = $fixture.Replace('clave-ficticia-de-prueba', 'cambiar-esta-clave')
        Set-LocalApiConfig $generated $restored $true
        $generated.Save($outputPath)
        Assert-Setting (($generated.configuration.appSettings.add | Where-Object key -eq 'ApiPrefijo').value -eq 'http://+:8099/api/v1/') 'El prefijo debe sobrevivir a la recompilacion.'
        Assert-Setting (($generated.configuration.appSettings.add | Where-Object key -eq 'ApiKey').value -eq $settings.ApiKey) 'La plantilla no debe reemplazar la clave privada.'
        Assert-Setting (($generated.configuration.appSettings.add | Where-Object key -eq 'OtroAjuste').value -eq 'conservar') 'Debe respetar los ajustes ajenos a la API.'
    }
    Set-LocalApiConfig $generated $settings $false
    Assert-Setting (($generated.configuration.appSettings.add | Where-Object key -eq 'ApiHabilitada').value -eq 'false') '-NoApi debe desactivar la API.'
    Assert-Setting ((Get-LocalApiSettings $settingsPath $outputPath).ApiKey -eq $settings.ApiKey) '-NoApi no debe perder la clave guardada.'
    $override = Get-LocalApiSettings $settingsPath $outputPath 'http://localhost:8100/api/v1'
    Save-LocalApiSettings $settingsPath $override
    Assert-Setting ((Get-LocalApiSettings $settingsPath $outputPath).ApiPrefijo -eq 'http://localhost:8100/api/v1/') 'El prefijo explicito debe recordarse y normalizarse.'
    Write-Host "OK: $checks comprobaciones de persistencia local, sin red ni credenciales reales."
} finally {
    foreach ($path in @($settingsPath, $outputPath)) {
        if (Test-Path -LiteralPath $path) { Remove-Item -LiteralPath $path }
    }
    Remove-Item -LiteralPath $testDirectory
}
