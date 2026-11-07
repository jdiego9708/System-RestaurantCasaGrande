param([string]$Database='CasaGrandeBD_prod',[string]$ProfileDatabase='CasaGrandeBD_prod_anterior_20261105')
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Data
function Query($connection,$sql) {$cmd=$connection.CreateCommand();$cmd.CommandText=$sql;$table=New-Object Data.DataTable;[void](New-Object Data.SqlClient.SqlDataAdapter $cmd).Fill($table);return ,$table}
function Open-Db($name) {if($name -notmatch '^[A-Za-z0-9_]+$'){throw 'Nombre de base invalido'};$cn=New-Object Data.SqlClient.SqlConnection "Server=(localdb)\MSSQLLocalDB;Database=$name;Integrated Security=true";$cn.Open();return $cn}
$key=[Environment]::GetEnvironmentVariable('ALEGRA_API_KEY','User');$headers=@{Authorization=('Bearer '+$key)}
function Catalog($path) {$r=Invoke-RestMethod ('https://api.alegra.com/api/v1/'+$path) -Headers $headers -TimeoutSec 45;if($r -isnot [array] -and $null -ne $r.data){return $r.data};return $r}
$company=Catalog 'company';$companyId=[string]$company.id
if(!$companyId){$number=[string]$company.identificationObject.number;if(!$number){$number=[string]$company.identification};if(!$number){throw 'Empresa sin identificacion'};$companyId=[string]$company.applicationVersion+':'+$number.Trim()}
$items=@();for($offset=0;$offset -lt 30000;$offset+=30){$page=@(Catalog "items?start=$offset&limit=30");$items+=$page;if($page.Count -lt 30){break};if($offset -eq 29970){throw 'Catalogo incompleto'}}
$items=@($items | Group-Object id | ForEach-Object { $_.Group[0] })
$taxes=@(Catalog 'taxes');$active=@($items|Where-Object {$_.status -ne 'inactive'})
$cn=Open-Db $Database
try {
 $local=Query $cn "SELECT 'PLATO' Tipo,Id_plato Id,Nombre_plato Nombre FROM dbo.Platos WHERE ISNULL(Estado,'ACTIVO')='ACTIVO' UNION ALL SELECT 'BEBIDA',Id_bebida,Nombre_bebida FROM dbo.Bebidas WHERE ISNULL(Estado,'ACTIVO')='ACTIVO'"
 $current=Query $cn "SELECT ConfigurationJson FROM dbo.AlegraSettings WHERE CompanyKey='default'"
 if($current.Rows.Count){$settings=$current.Rows[0].ConfigurationJson|ConvertFrom-Json}
 else {$prior=Open-Db $ProfileDatabase;try{$profile=Query $prior "SELECT ConfigurationJson FROM dbo.AlegraSettings WHERE CompanyKey='default'";if(!$profile.Rows.Count){throw 'No existe configuracion previa'};$settings=$profile.Rows[0].ConfigurationJson|ConvertFrom-Json;$settings.Products=[pscustomobject]@{}}finally{$prior.Dispose()}}
 if($settings.CompanyId -ne $companyId){throw 'La configuracion no corresponde a la cuenta API'}
 $products=@{};foreach($p in $settings.Products.PSObject.Properties){$products[$p.Name]=$p.Value}
 $links=New-Object 'System.Collections.Generic.List[object]'
 foreach($row in $local.Rows){$matches=@($active|Where-Object {([string]$_.name).Trim() -ieq ([string]$row.Nombre).Trim()});if($matches.Count -ne 1){continue};$item=$matches[0];$id=[string]$item.id;$keyLocal=[string]$row.Tipo+':'+$row.Id
 if($products.ContainsKey($keyLocal) -and $products[$keyLocal].RemoteId -ne $id){throw "Vinculo previo diferente para $keyLocal"}
 $rates=@{};foreach($tax in @($item.tax)){if(!$tax){continue};$found=@($taxes|Where-Object {[string]$_.id -eq [string]$tax.id});if($found.Count -ne 1 -or $null -eq $found[0].percentage){throw "Impuesto no verificado para $id"};$rates[[string]$tax.id]=[decimal]$found[0].percentage}
 $products[$keyLocal]=@{RemoteId=$id;PercentageTaxes=$rates};$links.Add([pscustomobject]@{Tipo=$row.Tipo;IdLocal=$row.Id;NombreLocal=$row.Nombre;IdAlegra=$id;NombreAlegra=$item.name;Impuestos=$rates})
 }
 $settings.Products=$products;$settings.Enabled=$true
 $transaction=$cn.BeginTransaction()
 try {
 foreach($link in $links){$cmd=$cn.CreateCommand();$cmd.Transaction=$transaction;$cmd.CommandText="IF EXISTS(SELECT 1 FROM dbo.AlegraItemLinks WHERE CompanyKey=@company AND ItemType=@type AND ItemId=@id AND RemoteId<>@remote) THROW 50001,'Vinculo existente diferente',1; UPDATE dbo.AlegraItemLinks SET TaxJson=@tax WHERE CompanyKey=@company AND ItemType=@type AND ItemId=@id; IF @@ROWCOUNT=0 INSERT dbo.AlegraItemLinks(CompanyKey,ItemType,ItemId,RemoteId,TaxJson) VALUES(@company,@type,@id,@remote,@tax);";foreach($pair in @{company=$companyId;type=$link.Tipo;id=[int]$link.IdLocal;remote=$link.IdAlegra;tax=($link.Impuestos|ConvertTo-Json -Compress)}.GetEnumerator()){[void]$cmd.Parameters.AddWithValue('@'+$pair.Key,$pair.Value)};[void]$cmd.ExecuteNonQuery()}
 $cmd=$cn.CreateCommand();$cmd.Transaction=$transaction;$cmd.CommandText="UPDATE dbo.AlegraSettings WITH(UPDLOCK,SERIALIZABLE) SET ConfigurationJson=@json,UpdatedAt=SYSUTCDATETIME() WHERE CompanyKey='default'; IF @@ROWCOUNT=0 INSERT dbo.AlegraSettings(CompanyKey,ConfigurationJson) VALUES('default',@json)";[void]$cmd.Parameters.Add('@json',[Data.SqlDbType]::NVarChar,-1);$cmd.Parameters['@json'].Value=$settings|ConvertTo-Json -Depth 30 -Compress;[void]$cmd.ExecuteNonQuery();$transaction.Commit()
 }catch{$transaction.Rollback();throw}
 $report='D:\Repositorios\CasaGrande-inventario-20261105\vinculos-exactos-aplicados.csv';$links|Select-Object Tipo,IdLocal,NombreLocal,IdAlegra,NombreAlegra|Export-Csv $report -Delimiter ';' -NoTypeInformation -Encoding UTF8
 Write-Host ("Vinculos exactos activos aplicados: "+$links.Count+". Base: "+$Database+". Emision habilitada; se conserva la configuracion de impresion automatica.")
}finally{$cn.Dispose()}
