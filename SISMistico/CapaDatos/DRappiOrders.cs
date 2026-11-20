using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CapaEntidades.Models;
using CapaEntidades.Helpers;

namespace CapaDatos
{
    /// <summary>Acceso a la tabla RappiOrders (ordenes de Rappi leidas de las facturas PDF).</summary>
    public static class DRappiOrders
    {
        /// <summary>
        /// Guarda la orden si su ExternalId no existe todavia. Devuelve true si se inserto y false si ya estaba
        /// (la misma factura copiada dos veces, o dos archivos de la misma orden). Al insertar le da el siguiente numero
        /// del dia de la orden (script 11); si la base aun no tiene la numeracion, guarda la orden sin numero.
        /// </summary>
        public static bool InsertIfNew(RappiOrder order)
        {
            for (int attempt = 0; ; attempt++)
            {
                try
                {
                    return Insert(order, true);
                }
                catch (SqlException ex) when (ex.Number == 207)
                {
                    // Columna inexistente: el script 11 no esta aplicado. La orden se guarda igual, sin numero.
                    return Insert(order, false);
                }
                catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
                {
                    // O la orden ya existe (otro equipo la inserto entre la comprobacion y el insert) o dos equipos
                    // tomaron el mismo numero a la vez: en ese caso se reintenta con el siguiente.
                    if (Exists(order.ExternalId)) return false;
                    if (attempt >= 3) throw;
                }
            }
        }

        private static bool Exists(string externalId)
        {
            using (SqlConnection con = new SqlConnection(Conexion.Cn))
            using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM dbo.RappiOrders WHERE ExternalId = @ExternalId", con))
            {
                cmd.Parameters.Add("@ExternalId", SqlDbType.VarChar, 80).Value = externalId;
                con.Open();
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        private const string InsertColumns = @"ExternalId, CustomerName, Phone, Address, PaymentMethod, Platform, OrderDateTime,
        ProductsText, ProductCount, Subtotal, Discount, ShippingCost, Tip, ServiceFee, PaidThroughRappi, Total,
        CashPayWith, ChangeToBring, DeliveryCode, IsComplete, SourceFile, FileHash";

        private const string InsertValues = @"@ExternalId, @CustomerName, @Phone, @Address, @PaymentMethod, @Platform, @OrderDateTime,
        @ProductsText, @ProductCount, @Subtotal, @Discount, @ShippingCost, @Tip, @ServiceFee, @PaidThroughRappi, @Total,
        @CashPayWith, @ChangeToBring, @DeliveryCode, @IsComplete, @SourceFile, @FileHash";

        private static bool Insert(RappiOrder order, bool numbered)
        {
            // Numerada: la comprobacion y el siguiente numero del dia se toman bajo el mismo bloqueo, asi dos equipos no
            // reparten el mismo numero (el indice unico por dia es la red de seguridad).
            string query = numbered
                ? @"SET XACT_ABORT ON;
BEGIN TRAN;
IF NOT EXISTS (SELECT 1 FROM dbo.RappiOrders WITH (UPDLOCK, HOLDLOCK) WHERE ExternalId = @ExternalId)
BEGIN
    DECLARE @Day date = CAST(COALESCE(@OrderDateTime, SYSDATETIME()) AS date);
    INSERT INTO dbo.RappiOrders (" + InsertColumns + @", NumberDay, DayNumber)
    VALUES (" + InsertValues + @", @Day,
        ISNULL((SELECT MAX(DayNumber) FROM dbo.RappiOrders WITH (UPDLOCK, HOLDLOCK) WHERE NumberDay = @Day), 0) + 1);
    COMMIT;
    SELECT 1;
END
ELSE
BEGIN
    COMMIT;
    SELECT 0;
END"
                : @"IF NOT EXISTS (SELECT 1 FROM dbo.RappiOrders WHERE ExternalId = @ExternalId)
BEGIN
    INSERT INTO dbo.RappiOrders (" + InsertColumns + @") VALUES (" + InsertValues + @");
    SELECT 1;
END
ELSE SELECT 0;";

            using (SqlConnection con = new SqlConnection(Conexion.Cn))
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.Add("@ExternalId", SqlDbType.VarChar, 80).Value = order.ExternalId;
                cmd.Parameters.Add("@CustomerName", SqlDbType.NVarChar, 150).Value = Db(order.CustomerName, 150);
                cmd.Parameters.Add("@Phone", SqlDbType.VarChar, 40).Value = Db(order.Phone, 40);
                cmd.Parameters.Add("@Address", SqlDbType.NVarChar, 600).Value = Db(order.Address, 600);
                cmd.Parameters.Add("@PaymentMethod", SqlDbType.NVarChar, 60).Value = Db(order.PaymentMethod, 60);
                cmd.Parameters.Add("@Platform", SqlDbType.NVarChar, 40).Value = Db(order.Platform, 40);
                cmd.Parameters.Add("@OrderDateTime", SqlDbType.DateTime2).Value = order.OrderDateTime.HasValue ? (object)order.OrderDateTime.Value : DBNull.Value;
                cmd.Parameters.Add("@ProductsText", SqlDbType.NVarChar, -1).Value = order.Items.Count == 0 ? (object)DBNull.Value : order.ItemsText;
                cmd.Parameters.Add("@ProductCount", SqlDbType.Int).Value = order.Items.Count;
                cmd.Parameters.Add("@Subtotal", SqlDbType.Int).Value = Db(order.Subtotal);
                cmd.Parameters.Add("@Discount", SqlDbType.Int).Value = Db(order.Discount);
                cmd.Parameters.Add("@ShippingCost", SqlDbType.Int).Value = Db(order.ShippingCost);
                cmd.Parameters.Add("@Tip", SqlDbType.Int).Value = Db(order.Tip);
                cmd.Parameters.Add("@ServiceFee", SqlDbType.Int).Value = Db(order.ServiceFee);
                cmd.Parameters.Add("@PaidThroughRappi", SqlDbType.Int).Value = Db(order.PaidThroughRappi);
                cmd.Parameters.Add("@Total", SqlDbType.Int).Value = Db(order.Total);
                cmd.Parameters.Add("@CashPayWith", SqlDbType.Int).Value = Db(order.CashPayWith);
                cmd.Parameters.Add("@ChangeToBring", SqlDbType.Int).Value = Db(order.ChangeToBring);
                cmd.Parameters.Add("@DeliveryCode", SqlDbType.VarChar, 30).Value = Db(order.DeliveryCode, 30);
                cmd.Parameters.Add("@IsComplete", SqlDbType.Bit).Value = order.IsComplete;
                cmd.Parameters.Add("@SourceFile", SqlDbType.NVarChar, 260).Value = Db(order.SourceFile, 260);
                cmd.Parameters.Add("@FileHash", SqlDbType.Char, 64).Value = Db(order.FileHash, 64);
                con.Open();
                return Convert.ToInt32(cmd.ExecuteScalar()) == 1;
            }
        }

        /// <summary>
        /// Numero del dia de la orden; si todavia no lo tiene (orden anterior a la numeracion) se le asigna el siguiente.
        /// Null si la base no tiene la numeracion (script 11) o la orden no existe.
        /// </summary>
        public static int? EnsureDayNumber(string externalId)
        {
            const string query = @"
DECLARE @n int = (SELECT DayNumber FROM dbo.RappiOrders WHERE ExternalId = @ExternalId);
IF @n IS NULL AND EXISTS (SELECT 1 FROM dbo.RappiOrders WHERE ExternalId = @ExternalId)
BEGIN
    SET XACT_ABORT ON;
    BEGIN TRAN;
    UPDATE r SET NumberDay = ISNULL(r.NumberDay, CAST(COALESCE(r.OrderDateTime, r.ImportedAt) AS date)),
                 DayNumber = ISNULL((SELECT MAX(x.DayNumber) FROM dbo.RappiOrders x WITH (UPDLOCK, HOLDLOCK)
                                     WHERE x.NumberDay = ISNULL(r.NumberDay, CAST(COALESCE(r.OrderDateTime, r.ImportedAt) AS date))), 0) + 1
    FROM dbo.RappiOrders r WHERE r.ExternalId = @ExternalId AND r.DayNumber IS NULL;
    COMMIT;
    SET @n = (SELECT DayNumber FROM dbo.RappiOrders WHERE ExternalId = @ExternalId);
END
SELECT @n;";
            for (int attempt = 0; ; attempt++)
            {
                try
                {
                    using (SqlConnection con = new SqlConnection(Conexion.Cn))
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.Add("@ExternalId", SqlDbType.VarChar, 80).Value = externalId;
                        con.Open();
                        object value = cmd.ExecuteScalar();
                        return value == null || value == DBNull.Value ? (int?)null : Convert.ToInt32(value);
                    }
                }
                catch (SqlException ex) when (ex.Number == 207)
                {
                    return null;
                }
                catch (SqlException ex) when ((ex.Number == 2627 || ex.Number == 2601) && attempt < 3)
                {
                    // Otro equipo tomo ese numero justo ahora: se reintenta con el siguiente.
                }
            }
        }

        /// <summary>Un archivo ya importado: con que nombre llego y de que dia es su orden.</summary>
        public sealed class KnownFile
        {
            public string SourceFile { get; set; }

            public DateTime Day { get; set; }
        }

        /// <summary>Huellas de los archivos ya importados, para no releer PDFs que ya se conocen.</summary>
        public static Dictionary<string, KnownFile> KnownFiles()
        {
            Dictionary<string, KnownFile> files = new Dictionary<string, KnownFile>(StringComparer.OrdinalIgnoreCase);
            using (SqlConnection con = new SqlConnection(Conexion.Cn))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT FileHash, SourceFile, COALESCE(OrderDateTime, ImportedAt) FROM dbo.RappiOrders WHERE FileHash IS NOT NULL", con))
            {
                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                    while (reader.Read())
                        files[reader.GetString(0).Trim()] = new KnownFile
                        {
                            SourceFile = reader.IsDBNull(1) ? null : reader.GetString(1),
                            Day = reader.GetDateTime(2).Date,
                        };
            }
            return files;
        }

        /// <summary>
        /// Ordenes de un dia (por la fecha de la orden; la de importacion si la factura no traia fecha), las mas recientes
        /// primero, con busqueda y paginacion. <paramref name="total"/> es el numero de ordenes que cumplen el filtro.
        /// </summary>
        public static List<RappiOrder> SearchDay(DateTime day, string text, int page, int pageSize, out int total)
        {
            int? number = OrderNumberSearch.TryParse(text, 'R', out int parsed) ? (int?)parsed : null;
            string like = number.HasValue || string.IsNullOrWhiteSpace(text) ? null : "%" + Escape(text.Trim()) + "%";
            string filter = @"
WHERE CAST(COALESCE(OrderDateTime, ImportedAt) AS date) = @Day
  AND (@Like IS NULL OR ExternalId LIKE @Like ESCAPE '\' OR CustomerName LIKE @Like ESCAPE '\'
       OR Phone LIKE @Like ESCAPE '\' OR Address LIKE @Like ESCAPE '\' OR ProductsText LIKE @Like ESCAPE '\'
       OR PaymentMethod LIKE @Like ESCAPE '\' OR DeliveryCode LIKE @Like ESCAPE '\')";
            if (number.HasValue) filter += " AND DayNumber = @DayNumber";
            List<RappiOrder> result = new List<RappiOrder>();
            using (SqlConnection con = new SqlConnection(Conexion.Cn))
            {
                con.Open();
                using (SqlCommand count = new SqlCommand("SELECT COUNT(*) FROM dbo.RappiOrders" + filter, con))
                {
                    AddFilter(count, day, like, number);
                    try { total = Convert.ToInt32(count.ExecuteScalar()); }
                    catch (SqlException ex) when (ex.Number == 207 && number.HasValue)
                    {
                        // Base anterior al script 11: aun no hay consecutivos que buscar.
                        total = 0;
                        return result;
                    }
                }
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT * FROM dbo.RappiOrders" + filter +
                    " ORDER BY COALESCE(OrderDateTime, ImportedAt) DESC, Id DESC OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY", con))
                {
                    AddFilter(cmd, day, like, number);
                    cmd.Parameters.Add("@Skip", SqlDbType.Int).Value = Math.Max(0, page) * pageSize;
                    cmd.Parameters.Add("@Take", SqlDbType.Int).Value = pageSize;
                    DataTable table = new DataTable();
                    new SqlDataAdapter(cmd).Fill(table);
                    foreach (DataRow row in table.Rows)
                        result.Add(RappiOrder.FromRow(row));
                }
            }
            return result;
        }

        /// <summary>La orden con ese ID de Rappi, o null si no existe.</summary>
        public static RappiOrder GetByExternalId(string externalId)
        {
            using (SqlConnection con = new SqlConnection(Conexion.Cn))
            using (SqlCommand cmd = new SqlCommand("SELECT * FROM dbo.RappiOrders WHERE ExternalId = @ExternalId", con))
            {
                cmd.Parameters.Add("@ExternalId", SqlDbType.VarChar, 80).Value = externalId;
                DataTable table = new DataTable();
                new SqlDataAdapter(cmd).Fill(table);
                return table.Rows.Count == 0 ? null : RappiOrder.FromRow(table.Rows[0]);
            }
        }

        /// <summary>
        /// Ordenes de Rappi de un rango de dias (por la fecha de la orden; la de importacion si la factura no traia fecha),
        /// con los datos basicos para listarlas: fecha, hora, orden, cliente, telefono, forma de pago y total a cobrar.
        /// </summary>
        public static DataTable ListRange(DateTime from, DateTime to)
        {
            const string query = @"
SELECT CAST(COALESCE(OrderDateTime, ImportedAt) AS date) AS OrderDate,
       COALESCE(OrderDateTime, ImportedAt) AS OrderTime,
       ExternalId, CustomerName, Phone, PaymentMethod, Total AS TotalToCollect, PaidThroughRappi,
       Total + ABS(ISNULL(PaidThroughRappi, 0)) AS Total,
       DayNumber
FROM dbo.RappiOrders
WHERE CAST(COALESCE(OrderDateTime, ImportedAt) AS date) BETWEEN @From AND @To
ORDER BY COALESCE(OrderDateTime, ImportedAt) DESC, Id DESC";
            using (SqlConnection con = new SqlConnection(Conexion.Cn))
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.Add("@From", SqlDbType.Date).Value = from.Date;
                cmd.Parameters.Add("@To", SqlDbType.Date).Value = to.Date;
                DataTable table = new DataTable("RappiOrders");
                try
                {
                    new SqlDataAdapter(cmd).Fill(table);
                }
                catch (SqlException ex) when (ex.Number == 207)
                {
                    // La numeracion es opcional en bases anteriores al script 11.
                    cmd.CommandText = query.Replace("       DayNumber", "       CAST(NULL AS int) AS DayNumber");
                    new SqlDataAdapter(cmd).Fill(table);
                }
                // Los importes originales ya estan persistidos: separar el saldo segun su forma de pago,
                // sin confundir TOTAL A COBRAR con el valor completo del pedido ni con "paga con".
                table.Columns.Add("CashAmount", typeof(int));
                table.Columns.Add("OtherPaymentAmount", typeof(int));
                foreach (DataRow row in table.Rows)
                {
                    RappiOrder payment = new RappiOrder
                    {
                        PaymentMethod = Convert.ToString(row["PaymentMethod"]),
                        Total = row["TotalToCollect"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["TotalToCollect"]),
                        PaidThroughRappi = row["PaidThroughRappi"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["PaidThroughRappi"]),
                    };
                    row["CashAmount"] = payment.CashAmount.HasValue ? (object)payment.CashAmount.Value : DBNull.Value;
                    row["OtherPaymentAmount"] = payment.OtherPaymentAmount.HasValue ? (object)payment.OtherPaymentAmount.Value : DBNull.Value;
                }
                return table;
            }
        }

        /// <summary>Cuantas ordenes hay en el dia, sin filtro (para el indicador superior).</summary>
        public static int CountDay(DateTime day)
        {
            using (SqlConnection con = new SqlConnection(Conexion.Cn))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT COUNT(*) FROM dbo.RappiOrders WHERE CAST(COALESCE(OrderDateTime, ImportedAt) AS date) = @Day", con))
            {
                cmd.Parameters.Add("@Day", SqlDbType.Date).Value = day.Date;
                con.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        private static void AddFilter(SqlCommand cmd, DateTime day, string like, int? number)
        {
            cmd.Parameters.Add("@Day", SqlDbType.Date).Value = day.Date;
            cmd.Parameters.Add("@Like", SqlDbType.NVarChar, 300).Value = like == null ? (object)DBNull.Value : like;
            if (number.HasValue) cmd.Parameters.Add("@DayNumber", SqlDbType.Int).Value = number.Value;
        }

        // Los comodines de LIKE escritos por el usuario se toman literalmente.
        private static string Escape(string text) =>
            text.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_").Replace("[", "\\[");

        private static object Db(string value, int max)
        {
            if (string.IsNullOrWhiteSpace(value))
                return DBNull.Value;
            value = value.Trim();
            return value.Length > max ? value.Substring(0, max) : value;
        }

        private static object Db(int? value) => value.HasValue ? (object)value.Value : DBNull.Value;
    }
}
