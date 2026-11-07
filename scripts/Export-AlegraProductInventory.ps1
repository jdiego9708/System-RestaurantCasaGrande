param([string]$Database='CasaGrandeBD_prod',[string]$OutputDirectory='D:\Repositorios\CasaGrande-inventario-20261105')
$ErrorActionPreference='Stop'
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
Add-Type -AssemblyName System.Data
$cn=New-Object Data.SqlClient.SqlConnection "Server=(localdb)\MSSQLLocalDB;Database=$Database;Integrated Security=true"
try {
 $cn.Open();$cmd=$cn.CreateCommand()
 $cmd.CommandText="SELECT 'PLATO' Tipo, Id_plato Id, Nombre_plato Nombre, Estado FROM dbo.Platos UNION ALL SELECT 'BEBIDA', Id_bebida, Nombre_bebida, Estado FROM dbo.Bebidas"
 $table=New-Object Data.DataTable;[void](New-Object Data.SqlClient.SqlDataAdapter $cmd).Fill($table)
 $table | Select-Object Tipo,Id,Nombre,Estado | Export-Csv (Join-Path $OutputDirectory 'productos-locales.csv') -Encoding UTF8 -NoTypeInformation
} finally { $cn.Dispose() }
$key=[Environment]::GetEnvironmentVariable('ALEGRA_API_KEY','User')
$headers=@{Authorization=('Bearer '+$key)}
$items=@()
for($offset=0;$offset -lt 30000;$offset+=30) {
 $page=Invoke-RestMethod ("https://api.alegra.com/api/v1/items?start=$offset&limit=30") -Headers $headers -TimeoutSec 60
 if($page -isnot [array] -and $null -ne $page.data) {$page=$page.data}
 $page=@($page);$items+=$page
 if($page.Count -lt 30) {break}
 if($offset -eq 29970) {throw 'No se obtuvo el catalogo completo.'}
}
$items | Select-Object id,name,status,reference | ConvertTo-Json -Depth 6 | Set-Content (Join-Path $OutputDirectory 'productos-alegra.json') -Encoding UTF8
Write-Host ('Productos locales: '+$table.Rows.Count+'; Alegra: '+$items.Count)
