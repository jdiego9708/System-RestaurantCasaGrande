param([string]$Instance='(localdb)\MSSQLLocalDB', [Parameter(Mandatory=$true)][string]$Database, [string]$CredentialFile, [string]$Email, [ValidateSet("Basic","Bearer")][string]$AuthScheme, [switch]$CasaGrandeProfile, [switch]$VerifyOnly)
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Data
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
function Env-Value($name) {
    foreach($scope in @('Machine','User','Process')) {
        $value=[Environment]::GetEnvironmentVariable($name,$scope)
        if($value) { return $value }
    }
}
$email=Env-Value 'ALEGRA_EMAIL'; $key=Env-Value 'ALEGRA_API_KEY'; $scheme=Env-Value 'ALEGRA_AUTH_SCHEME'
if($CredentialFile) { $key=(Get-Content -LiteralPath $CredentialFile -Raw).Trim() }
if($PSBoundParameters.ContainsKey('Email')) { $email=$PSBoundParameters['Email'] }
if($AuthScheme) { $scheme=$AuthScheme }
if(!$email -or !$key) { throw 'Faltan ALEGRA_EMAIL y ALEGRA_API_KEY en Windows. Configure las credenciales API de la cuenta antes de repetir este paso.' }
if(!$scheme) { $scheme='Basic' }
if($scheme -notin @('Basic','Bearer')) { throw 'ALEGRA_AUTH_SCHEME debe ser Basic o Bearer.' }
$authorization=if($scheme -eq 'Bearer') { 'Bearer '+$key } else { 'Basic '+[Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($email+':'+$key)) }
function Get-Catalog($path) {
    try { $result=Invoke-RestMethod -Uri ('https://api.alegra.com/api/v1/'+$path) -Headers @{Authorization=$authorization} -TimeoutSec 45 }
    catch {
        $status=if($_.Exception.Response) { 'HTTP '+[int]$_.Exception.Response.StatusCode } else { $_.Exception.GetType().Name }
        throw "No se pudo consultar el catalogo Alegra: $path ($status). Revise conexion, credenciales y permisos."
    }
    if($result -isnot [array] -and $null -ne $result.data) { return $result.data }; return $result
}
function Select-Option($values, $caption) {
    $values=@($values)
    if(!$values.Count) { throw "No hay opciones disponibles: $caption" }
    if($CasaGrandeProfile) {
        $wanted=if($caption -like '*numeracion*') {'40'} elseif($caption -like '*EFECTIVO*') {'6'} elseif($caption -like '*TRANSFERENCIA*' -or $caption -like '*TARJETA*') {'1'} elseif($caption -like '*Medio fiscal*') {'CASH'} elseif($caption -like '*Producto de Alegra para*') {''} else {throw "Seleccion no definida: $caption"}
        $selected=@($values | Where-Object { [string]$_.id -eq $wanted })
        if($selected.Count -ne 1) { throw "La seleccion configurada no existe o es ambigua: $caption" }
        return $selected[0]
    }
    if($values.Count -eq 1) { return $values[0] }
    Write-Host $caption -ForegroundColor Cyan
    for($i=0;$i -lt $values.Count;$i++) { Write-Host ("{0}. {1} {2} [ID {3}]" -f ($i+1),$values[$i].name,$values[$i].prefix,$values[$i].id) }
    $selection=0
    if(![int]::TryParse((Read-Host 'Seleccione el numero'),[ref]$selection) -or $selection -lt 1 -or $selection -gt $values.Count) { throw 'Seleccion invalida; no se guardo configuracion.' }
    return $values[$selection-1]
}
$connection=New-Object System.Data.SqlClient.SqlConnection "Server=$Instance;Database=$Database;Integrated Security=true"
try {
    $connection.Open()
    $command=$connection.CreateCommand()
    $command.CommandText="SELECT ConfigurationJson FROM dbo.AlegraSettings WHERE CompanyKey='default'"
    $existing=$command.ExecuteScalar()
    $company=Get-Catalog 'company'
    $companyIdentity=[string]$company.id
    if(!$companyIdentity) {
        $numberIdentity=[string]$company.identificationObject.number
        if(!$numberIdentity) { $numberIdentity=[string]$company.identification }
        if(!$numberIdentity) { throw 'La cuenta no devolvio identificacion de empresa.' }
        $companyIdentity=[string]$company.applicationVersion+':'+$numberIdentity.Trim()
    }
    $emissionKey=Env-Value 'ALEGRA_EMISSION_API_KEY'
    if(!$emissionKey -and $scheme -eq 'Bearer') {
        throw 'Falta ALEGRA_EMISSION_API_KEY. Configure en el instalador el token anterior con Basic para emision y repita 01-Base-de-Datos.ps1. La configuracion SQL existente no se modifica.'
    }
    if($emissionKey) {
        $emissionEmail=Env-Value 'ALEGRA_EMISSION_EMAIL'
        if(!$emissionEmail) { $emissionEmail=$email }
        $emissionScheme=Env-Value 'ALEGRA_EMISSION_AUTH_SCHEME'
        if(!$emissionScheme) { $emissionScheme='Basic' }
        if($emissionScheme -notin @('Basic','Bearer')) { throw 'ALEGRA_EMISSION_AUTH_SCHEME debe ser Basic o Bearer.' }
        $emissionAuthorization=if($emissionScheme -eq 'Bearer') { 'Bearer '+$emissionKey } else { 'Basic '+[Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($emissionEmail+':'+$emissionKey)) }
        try { $emissionCompany=Invoke-RestMethod -Uri 'https://api.alegra.com/api/v1/company' -Headers @{Authorization=$emissionAuthorization} -TimeoutSec 45 }
        catch { throw 'No se pudo verificar la empresa con la credencial de emision. Revise ALEGRA_EMISSION_*; no se enviaron facturas.' }
        $emissionIdentity=[string]$emissionCompany.id
        if(!$emissionIdentity) {
            $emissionNumber=[string]$emissionCompany.identificationObject.number
            if(!$emissionNumber) { $emissionNumber=[string]$emissionCompany.identification }
            if(!$emissionNumber) { throw 'La credencial de emision no devolvio identificacion de empresa.' }
            $emissionIdentity=[string]$emissionCompany.applicationVersion+':'+$emissionNumber.Trim()
        }
        if($emissionIdentity -ne $companyIdentity) { throw 'Las credenciales principal y de emision pertenecen a empresas diferentes.' }
        Write-Host 'Credenciales principal y de emision verificadas por lectura: misma empresa. El permiso de emitir se confirma al emitir una factura real.'
    }
    if($VerifyOnly -and (!$existing -or $existing -is [DBNull])) { throw 'No existe configuracion Alegra en esta base. Ejecute el paso 05 sin -VerifyOnly.' }
    if($existing -and $existing -isnot [DBNull]) {
        $saved=$existing | ConvertFrom-Json
        if([string]$saved.CompanyId -ne $companyIdentity) { throw 'La configuracion existente pertenece a otra empresa. No se sobrescribe.' }
        if($VerifyOnly) {
            if($saved.Enabled -and (!$saved.NumberTemplateId -or !$saved.Products.PSObject.Properties.Count)) {
                throw 'La integracion esta habilitada sin numeracion o sin productos vinculados.'
            }
            Write-Host ('Verificacion SQL sin cambios. Emision habilitada: '+$saved.Enabled+'; impresion automatica: '+$saved.AutoPrint)
            return
        }
        # Primera activacion del nuevo flujo: no imprimir retroactivamente ventas antiguas.
        # Si el usuario lo desactiva despues, conserva esa eleccion y la fecha existente.
        if(!$saved.AutoPrintFromUtc) {
            $saved | Add-Member -NotePropertyName AutoPrint -NotePropertyValue $true -Force
            $saved | Add-Member -NotePropertyName AutoPrintFromUtc -NotePropertyValue ([DateTime]::UtcNow.ToString('o')) -Force
            $command.CommandText="UPDATE dbo.AlegraSettings SET ConfigurationJson=@json,UpdatedAt=SYSUTCDATETIME() WHERE CompanyKey='default'"
            [void]$command.Parameters.Add('@json',[System.Data.SqlDbType]::NVarChar,-1)
            $command.Parameters['@json'].Value=($saved | ConvertTo-Json -Depth 30 -Compress)
            [void]$command.ExecuteNonQuery()
            Write-Host 'Doble impresion automatica habilitada para ventas nuevas; se conserva el historial sin reimprimir.'
        }
        Write-Host 'Configuracion existente conservada y cuenta API comprobada.'
        return
    }
    $unmatched=New-Object 'System.Collections.Generic.List[object]'
    $number=Select-Option @(Get-Catalog 'number-templates' | Where-Object { $_.documentType -eq 'invoice' -and $_.isElectronic -eq $true -and $_.status -eq 'active' }) 'Seleccione la numeracion ELECTRONICA autorizada que usa en Alegra'
    $banks=@(Get-Catalog 'bank-accounts')
    $taxes=@(Get-Catalog 'taxes')
    $items=@()
    for($offset=0;$offset -lt 30000;$offset+=30) {
        $page=@(Get-Catalog "items?start=$offset&limit=30")
        $items+=$page
        if($page.Count -lt 30) { break }
        if($offset -eq 29970) { throw 'Catalogo demasiado grande para verificarlo completamente.' }
    }
    $items=@($items | Where-Object { $_.status -ne 'inactive' } | Group-Object id | ForEach-Object { $_.Group[0] })
    function Mapping($item) {
        $rates=@{}
        foreach($tax in @($item.tax)) {
            if(!$tax) { continue }
            $match=@($taxes | Where-Object { [string]$_.id -eq [string]$tax.id })
            if($match.Count -ne 1 -or $null -eq $match[0].percentage) { throw ('Impuesto no identificado para '+$item.name) }
            $rates[[string]$tax.id]=[decimal]$match[0].percentage
        }
        return @{RemoteId=[string]$item.id;PercentageTaxes=$rates}
    }
    $settings=@{CompanyId=$companyIdentity;Enabled=$true;AutoPrint=$true;AutoPrintFromUtc=[DateTime]::UtcNow.ToString("o");NumberTemplateId=[string]$number.id;FiscalPaymentMethod='';Products=@{};PaymentAccounts=@{};PaymentMethods=@{EFECTIVO='cash';TRANSFERENCIA='transfer';TARJETA='credit-card'}}
    foreach($method in @('EFECTIVO','TRANSFERENCIA','TARJETA')) {
        $bank=Select-Option $banks "Cuenta receptora para $method"
        $settings.PaymentAccounts[$method]=[string]$bank.id
    }
    $command.CommandText="SELECT 'PLATO' AS Tipo,Id_plato AS Id,Nombre_plato AS Nombre FROM dbo.Platos WHERE ISNULL(Estado,'ACTIVO')='ACTIVO' UNION ALL SELECT 'BEBIDA',Id_bebida,Nombre_bebida FROM dbo.Bebidas WHERE ISNULL(Estado,'ACTIVO')='ACTIVO'"
    $table=New-Object System.Data.DataTable
    [void](New-Object System.Data.SqlClient.SqlDataAdapter $command).Fill($table)
    foreach($row in $table.Rows) {
        $matches=@($items | Where-Object { ([string]$_.name).Trim() -ieq ([string]$row.Nombre).Trim() })
        if($CasaGrandeProfile -and $matches.Count -ne 1) {
            $unmatched.Add([pscustomobject]@{Tipo=$row.Tipo;Id=$row.Id;Nombre=$row.Nombre;Coincidencias=$matches.Count})
            continue
        }
        $item=if($matches.Count -eq 1) { $matches[0] } else { Select-Option $items ("Vincule producto local: "+$row.Nombre) }
        $settings.Products[([string]$row.Tipo+':'+$row.Id)]=Mapping $item
    }
    foreach($charge in @(@('DeliveryItem','domicilio'),@('PackagingItem','desechables'))) {
        $options=@([pscustomobject]@{id='';name='No se cobra este concepto'})+$items
        $item=Select-Option $options ('Producto de Alegra para '+$charge[1])
        if($item.id) { $settings[$charge[0]]=Mapping $item }
    }
    Write-Host 'Si cobra propina, indique el ID del cargo configurado en Alegra.'
    $settings.TipChargeId=if($CasaGrandeProfile) { '' } else { Read-Host 'ID cargo propina (Enter si no aplica)' }
    # El medio fiscal de la cabecera ya se deduce del pago (efectivo=CASH, transferencia=BANK_DEPOSIT, tarjeta=CREDIT_CARD).
    # Este valor solo es el respaldo para medios sin equivalente (cheque, consignacion): no se pregunta.
    $settings.FiscalPaymentMethod='CASH'
    if($CasaGrandeProfile -and $unmatched.Count) {
        $settings.Enabled=($settings.Products.Count -gt 0)
        $report=Join-Path $env:TEMP ('CasaGrande-Alegra-productos-sin-vincular-'+[guid]::NewGuid().ToString('N')+'.csv')
        $unmatched | Export-Csv -LiteralPath $report -NoTypeInformation -Encoding UTF8
        Write-Host ("Productos por vincular: "+$unmatched.Count+". Informe: "+$report) -ForegroundColor Yellow
    }
    $transaction=$connection.BeginTransaction()
    try {
        $command.Transaction=$transaction
        $command.CommandText="INSERT dbo.AlegraSettings(CompanyKey,ConfigurationJson) VALUES ('default',@json)"
        [void]$command.Parameters.Add('@json',[System.Data.SqlDbType]::NVarChar,-1)
        $command.Parameters['@json'].Value=$settings | ConvertTo-Json -Depth 20 -Compress
        [void]$command.ExecuteNonQuery()
        foreach($entry in $settings.Products.GetEnumerator()) {
            $parts=$entry.Key.Split(':')
            $link=$connection.CreateCommand()
            $link.Transaction=$transaction
            $link.CommandText="IF EXISTS(SELECT 1 FROM dbo.AlegraItemLinks WHERE CompanyKey=@company AND ItemType=@type AND ItemId=@id AND RemoteId<>@remote) THROW 50001,'Existe un vinculo previo diferente. No se guardo la configuracion.',1; UPDATE dbo.AlegraItemLinks SET TaxJson=@tax WHERE CompanyKey=@company AND ItemType=@type AND ItemId=@id; IF @@ROWCOUNT=0 INSERT dbo.AlegraItemLinks(CompanyKey,ItemType,ItemId,RemoteId,TaxJson) VALUES(@company,@type,@id,@remote,@tax);"
            [void]$link.Parameters.AddWithValue('@company',$companyIdentity)
            [void]$link.Parameters.AddWithValue('@type',$parts[0])
            [void]$link.Parameters.AddWithValue('@id',[int]$parts[1])
            [void]$link.Parameters.AddWithValue('@remote',$entry.Value.RemoteId)
            [void]$link.Parameters.AddWithValue('@tax',($entry.Value.PercentageTaxes | ConvertTo-Json -Compress))
            [void]$link.ExecuteNonQuery()
        }
        $transaction.Commit()
    } catch { $transaction.Rollback(); throw }
    finally { $transaction.Dispose() }
    Write-Host ('Configuracion guardada. Emision automatica: '+$settings.Enabled+'; PDF automatico: '+$settings.AutoPrint)
    Write-Host 'Los productos sin vincular no se envian a Alegra: al terminar una venta el sistema avisa y permite enviar solo los vinculados o no enviar. Las coincidencias dudosas no se aprobaron.' -ForegroundColor Green
} finally { $connection.Dispose(); $authorization=$null; $key=$null }
