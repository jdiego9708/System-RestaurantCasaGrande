<#
    Ejecuta un .sql (lotes separados por GO) contra LocalDB e imprime cada resultado como tabla.
    Sustituye a sqlcmd, que no esta instalado en esta maquina.

    Uso:
        .\Invoke-Sql.ps1 -File .\diagnostico-prod.sql
        .\Invoke-Sql.ps1 -File .\diagnostico-prod.sql -Database CasaGrandeBD_prod
#>
param(
    [Parameter(Mandatory)][string]$File,
    [string]$Database = "CasaGrandeBD_prod",
    [string]$Instance = "(localdb)\MSSQLLocalDB"
)

$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.Data

sqllocaldb start MSSQLLocalDB | Out-Null
$cn = New-Object System.Data.SqlClient.SqlConnection "Server=$Instance;Database=$Database;Integrated Security=true"
$cn.Open()

$text = [System.IO.File]::ReadAllText((Resolve-Path -LiteralPath $File))
$batches = [regex]::Split($text, '(?m)^GO[ \t]*\r?$\n?')

foreach ($b in $batches) {
    if ($b.Trim().Length -eq 0) { continue }
    $cmd = $cn.CreateCommand()
    $cmd.CommandText = $b
    $cmd.CommandTimeout = 0
    $da = New-Object System.Data.SqlClient.SqlDataAdapter $cmd
    $ds = New-Object System.Data.DataSet
    [void]$da.Fill($ds)
    foreach ($t in $ds.Tables) {
        $t | Format-Table -AutoSize | Out-String -Width 220
    }
}
$cn.Close()
