<#
.SYNOPSIS
    Marca como PRINTED una factura electrónica que quedó en PRINTING por un proceso muerto o sin diálogo.
.DESCRIPTION
    Solo actúa si la solicitud está ACCEPTED y en PRINTING. No toca la aceptación ni reenvía nada.
    Úsese únicamente cuando el usuario confirma que la factura ya se guardó/imprimió.
#>
param(
    [Parameter(Mandatory)][int]$SaleId,
    [string]$Database = "CasaGrandeBD_prod",
    [string]$Server = "(localdb)\MSSQLLocalDB"
)

$connection = New-Object System.Data.SqlClient.SqlConnection "Data Source=$Server;Initial Catalog=$Database;Integrated Security=True;Encrypt=False;TrustServerCertificate=True"
$connection.Open()
try {
    function Show-Row {
        $command = $connection.CreateCommand()
        $command.CommandText = "SELECT SaleId, Status, InvoiceNumber, PrintStatus, LastError FROM dbo.AlegraInvoiceRequests WHERE SaleId=@sale"
        [void]$command.Parameters.AddWithValue("@sale", $SaleId)
        $table = New-Object System.Data.DataTable
        $table.Load($command.ExecuteReader())
        $table | Format-Table -AutoSize | Out-String | Write-Host
    }

    Write-Host "Antes ($Database):"
    Show-Row

    $update = $connection.CreateCommand()
    $update.CommandText = "UPDATE dbo.AlegraInvoiceRequests SET PrintStatus='PRINTED', LastError=NULL WHERE SaleId=@sale AND Status='ACCEPTED' AND PrintStatus='PRINTING'"
    [void]$update.Parameters.AddWithValue("@sale", $SaleId)
    $rows = $update.ExecuteNonQuery()
    Write-Host "Filas actualizadas: $rows"

    Write-Host "Despues:"
    Show-Row
}
finally {
    $connection.Dispose()
}
