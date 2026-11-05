<# Descarga contratos publicos del MCP de documentacion. No usa credenciales ni ejecuta endpoints de cuenta. #>
[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$output = Join-Path (Split-Path $PSScriptRoot -Parent) 'docs\alegra\contracts'
New-Item -ItemType Directory -Path $output -Force | Out-Null
$encoding = New-Object System.Text.UTF8Encoding $false

function Read-Contract([string]$tool, [hashtable]$arguments) {
    if ($tool -notin @('list-endpoints','get-endpoint')) { throw 'Solo se permite consultar documentacion.' }
    $body = @{jsonrpc='2.0';id=1;method='tools/call';params=@{name=$tool;arguments=$arguments}} | ConvertTo-Json -Depth 10 -Compress
    $response = Invoke-WebRequest -UseBasicParsing -Uri 'https://developer.alegra.com/mcp' -Method Post `
        -Headers @{Accept='application/json, text/event-stream'} -ContentType 'application/json' -Body $body -TimeoutSec 45
    $text = [Text.Encoding]::UTF8.GetString($response.RawContentStream.ToArray())
    $dataLine = ($text -split "`n" | Where-Object { $_ -match '^data: ' } | Select-Object -Last 1)
    if ($dataLine) { $text = $dataLine.Substring(6) }
    $message = $text | ConvertFrom-Json
    if ($message.error -or $message.result.isError) { throw ($text) }
    $content = @($message.result.content | Where-Object { $_.type -eq 'text' } | ForEach-Object { $_.text }) -join "`n"
    return $content
}

$specs = @{
    ingresos='Ingresos (reference)'; contactos='Contactos (reference)'; inventario='Inventario (reference)';
    configuraciones='Configuraciones (reference)'; bancos='Bancos (reference)'
}
$manifest = @()
foreach ($key in ($specs.Keys | Sort-Object)) {
    $index = Read-Contract 'list-endpoints' @{title=$specs[$key]}
    [IO.File]::WriteAllText((Join-Path $output ($key + '-index.json')), $index, $encoding)
    $paths = $index | ConvertFrom-Json
    foreach ($path in $paths.PSObject.Properties) {
        $selected = switch ($key) {
            'ingresos' { $path.Name -in @('/invoices','/invoices/{id}','/invoices/stamp','/invoices/preview','/payments') }
            'contactos' { $path.Name -in @('/contacts','/contacts/{id}') }
            'inventario' { $path.Name -in @('/items','/items/{id}') }
            'configuraciones' { $path.Name -match '^/(taxes|number-templates|additional-charges|company|payment-terms)(/\{id\})?$' }
            'bancos' { $path.Name -in @('/bank-accounts','/bank-accounts/{id}','/banks','/banks/{id}') }
        }
        if (-not $selected) { continue }
        foreach ($method in $path.Value.PSObject.Properties.Name) {
            if ($method -notin @('get','post','put')) { continue }
            if ($method -eq 'put' -and $path.Name -notin @('/contacts/{id}','/invoices/{id}')) { continue }
            if ($method -eq 'post' -and $key -notin @('ingresos','contactos')) { continue }
            $file = $key + '-' + $method + '-' + (($path.Name.Trim('/') -replace '[{}]', '') -replace '/', '-') + '.json'
            $contract = Read-Contract 'get-endpoint' @{title=$specs[$key];path=$path.Name;method=$method.ToUpperInvariant()}
            [IO.File]::WriteAllText((Join-Path $output $file), $contract, $encoding)
            $manifest += [pscustomobject]@{spec=$specs[$key];path=$path.Name;method=$method.ToUpperInvariant();file=$file;sha256=(Get-FileHash (Join-Path $output $file) -Algorithm SHA256).Hash}
            Write-Host "$($method.ToUpperInvariant()) $($path.Name) -> $file"
        }
    }
}
$result = @{source='https://developer.alegra.com/mcp'; retrievedAt=[DateTime]::UtcNow.ToString('o');contracts=$manifest}
[IO.File]::WriteAllText((Join-Path $output 'manifest.json'), ($result | ConvertTo-Json -Depth 8), $encoding)
Write-Host "Contratos descargados: $($manifest.Count)"
