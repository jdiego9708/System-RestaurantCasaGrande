# Configuracion privada de la API local; sobrevive a Build, Rebuild y limpieza de bin.
function Get-LocalApiSettings {
    param([string]$SettingsPath, [string]$ExeConfigPath, [string]$PrefixOverride)

    $prefix = 'http://+:8099/api/v1/'
    $apiKey = $null
    if (Test-Path -LiteralPath $SettingsPath) {
        [xml]$saved = Get-Content -LiteralPath $SettingsPath -Raw
        $prefix = ($saved.appSettings.add | Where-Object key -eq 'ApiPrefijo').value
        $apiKey = ($saved.appSettings.add | Where-Object key -eq 'ApiKey').value
    } elseif (Test-Path -LiteralPath $ExeConfigPath) {
        [xml]$existing = Get-Content -LiteralPath $ExeConfigPath -Raw
        $apiKey = ($existing.configuration.appSettings.add | Where-Object key -eq 'ApiKey').value
        $previousPrefix = ($existing.configuration.appSettings.add | Where-Object key -eq 'ApiPrefijo').value
        # El script anterior imponia localhost: migrar ese valor a LAN en el primer uso.
        if ($previousPrefix -and $previousPrefix -notmatch '^http://(localhost|127\.0\.0\.1):') {
            $prefix = $previousPrefix
        }
    }
    if ($PrefixOverride) { $prefix = $PrefixOverride }
    if (-not $prefix -or $prefix -notmatch '^http://[^/]+:\d+/') {
        throw 'ApiPrefijo no es valido. Revise run-local.api.config o use -ApiPrefix.'
    }
    if (-not $prefix.EndsWith('/')) { $prefix += '/' }
    return @{ ApiPrefijo = $prefix; ApiKey = $apiKey }
}

function Save-LocalApiSettings {
    param([string]$SettingsPath, [hashtable]$Settings)

    $doc = New-Object System.Xml.XmlDocument
    [void]$doc.AppendChild($doc.CreateElement('appSettings'))
    foreach ($name in @('ApiPrefijo', 'ApiKey')) {
        $node = $doc.CreateElement('add')
        $node.SetAttribute('key', $name)
        $node.SetAttribute('value', [string]$Settings[$name])
        [void]$doc.DocumentElement.AppendChild($node)
    }
    $doc.Save($SettingsPath)
}

function Set-LocalApiConfig {
    param([xml]$Config, [hashtable]$Settings, [bool]$Enabled)

    $section = $Config.configuration.appSettings
    if (-not $section) {
        $section = $Config.CreateElement('appSettings')
        [void]$Config.configuration.AppendChild($section)
    }
    $values = @{
        ApiHabilitada = $Enabled.ToString().ToLowerInvariant()
        ApiPrefijo = $Settings.ApiPrefijo
        ApiKey = [string]$Settings.ApiKey
    }
    foreach ($name in $values.Keys) {
        $node = $section.add | Where-Object key -eq $name
        if (-not $node) {
            $node = $Config.CreateElement('add')
            $node.SetAttribute('key', $name)
            [void]$section.AppendChild($node)
        }
        $node.SetAttribute('value', $values[$name])
    }
}
