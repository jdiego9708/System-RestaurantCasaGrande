<#
.SYNOPSIS
    Diagnóstico de solo lectura: medios de pago configurados para Alegra y los enviados en cada solicitud.
.DESCRIPTION
    No muestra tokens. Lee la configuración guardada (mapeo de métodos locales -> medio/cuenta de Alegra,
    medio fiscal) y, por solicitud, el paymentForm/paymentMethod de cabecera y los pagos del RemoteRequestJson.
#>
param(
    [string]$Database = "CasaGrandeBD_prod",
    [string]$Server = "(localdb)\MSSQLLocalDB"
)

Add-Type -AssemblyName System.Web.Extensions
$json = New-Object System.Web.Script.Serialization.JavaScriptSerializer
$json.MaxJsonLength = 16MB

$connection = New-Object System.Data.SqlClient.SqlConnection "Data Source=$Server;Initial Catalog=$Database;Integrated Security=True;Encrypt=False;TrustServerCertificate=True"
$connection.Open()
try {
    function Read-Table([string]$sql) {
        $command = $connection.CreateCommand()
        $command.CommandText = $sql
        $table = New-Object System.Data.DataTable
        $table.Load($command.ExecuteReader())
        # La coma evita que PowerShell desenrolle el DataTable en filas al devolverlo.
        return ,$table
    }

    Write-Host "== Configuracion guardada =="
    $settingsCommand = $connection.CreateCommand()
    $settingsCommand.CommandText = "SELECT TOP(1) name FROM sys.columns WHERE object_id=OBJECT_ID('dbo.AlegraSettings')"
    $columns = Read-Table "SELECT c.name FROM sys.columns c WHERE c.object_id=OBJECT_ID('dbo.AlegraSettings')"
    Write-Host ("Columnas de AlegraSettings: " + (($columns | ForEach-Object { $_.name }) -join ", "))
    $settings = Read-Table "SELECT * FROM dbo.AlegraSettings"
    Write-Host "Filas de configuracion: $($settings.Rows.Count)"
    foreach ($row in $settings.Rows) {
        foreach ($column in $settings.Columns) {
            $value = [string]$row[$column.ColumnName]
            if ($value.TrimStart().StartsWith("{")) {
                $config = $json.DeserializeObject($value)
                Write-Host "FiscalPaymentMethod : $($config['FiscalPaymentMethod'])"
                Write-Host "PaymentMethods      : $(($config['PaymentMethods'].GetEnumerator() | ForEach-Object { $_.Key + '=' + $_.Value }) -join '; ')"
                Write-Host "PaymentAccounts     : $(($config['PaymentAccounts'].GetEnumerator() | ForEach-Object { $_.Key + '=' + $_.Value }) -join '; ')"
                $linkedKeys = @($config['Products'].Keys)
                Write-Host "Productos vinculados: $($linkedKeys.Count)  ejemplos de clave: $((($linkedKeys | Select-Object -First 5)) -join ', ')"
                Write-Host "Enabled=$($config['Enabled'])  CompanyId=$($config['CompanyId'])"
            }
        }
    }

    Write-Host ""
    Write-Host "== Por solicitud: pago local guardado (RequestJson) vs. lo enviado a Alegra (RemoteRequestJson) =="
    $requests = Read-Table "SELECT SaleId, InvoiceNumber, RequestJson, RemoteRequestJson FROM dbo.AlegraInvoiceRequests ORDER BY CreatedAt"
    foreach ($row in $requests.Rows) {
        $snapshot = $json.DeserializeObject([string]$row["RequestJson"])
        $localPays = @($snapshot["payments"] | ForEach-Object { "$($_['Pago'])=$($_['Valor_pago'])" }) -join " | "
        Write-Host "$($row['InvoiceNumber']) (venta $($row['SaleId'])) pago local: [$localPays]"
        if ($row["RemoteRequestJson"] -eq [DBNull]::Value) { continue }
        $payload = $json.DeserializeObject([string]$row["RemoteRequestJson"])
        $pays = @($payload["payments"] | ForEach-Object { "$($_['paymentMethod']) cuenta=$($_['account']['id']) monto=$($_['amount'])" }) -join " | "
        Write-Host "    enviado: paymentForm=$($payload['paymentForm']) paymentMethod(cabecera)=$($payload['paymentMethod']) pagos=[$pays]"
    }
}
finally {
    $connection.Dispose()
}
