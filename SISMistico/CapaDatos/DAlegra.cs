using System;
using System.Data;
using System.Data.SqlClient;

namespace CapaDatos
{
    public static class DAlegra
    {
        public static DataTable Query(string sql, params SqlParameter[] parameters)
        {
            using (var connection = new SqlConnection(Conexion.Cn))
            using (var command = new SqlCommand(sql, connection))
            using (var adapter = new SqlDataAdapter(command))
            {
                command.Parameters.AddRange(parameters);
                var table = new DataTable();
                adapter.Fill(table);
                return table;
            }
        }

        public static string GetSettings()
        {
            var table = Query("IF OBJECT_ID('dbo.AlegraSettings') IS NOT NULL SELECT ConfigurationJson FROM dbo.AlegraSettings WHERE CompanyKey='default'");
            return table.Rows.Count == 0 ? null : Convert.ToString(table.Rows[0][0]);
        }

        public static void SaveSettings(string json)
        {
            Query(@"SET XACT_ABORT ON; BEGIN TRAN;
UPDATE dbo.AlegraSettings WITH (UPDLOCK,SERIALIZABLE) SET ConfigurationJson=@json,UpdatedAt=SYSUTCDATETIME() WHERE CompanyKey='default';
IF @@ROWCOUNT=0 INSERT dbo.AlegraSettings(CompanyKey,ConfigurationJson) VALUES ('default',@json);
COMMIT;", new SqlParameter("@json", SqlDbType.NVarChar,-1) { Value=json });
        }

        public static DataTable LocalProducts() => Query(@"
SELECT 'PLATO' AS Tipo,Id_plato AS Id,Nombre_plato AS Nombre FROM dbo.Platos WHERE ISNULL(Estado,'ACTIVO')='ACTIVO'
UNION ALL SELECT 'BEBIDA',Id_bebida,Nombre_bebida FROM dbo.Bebidas WHERE ISNULL(Estado,'ACTIVO')='ACTIVO' ORDER BY Tipo,Nombre");

        public static DataTable GetRequest(int saleId) => Query(@"
IF OBJECT_ID('dbo.AlegraInvoiceRequests') IS NOT NULL
SELECT * FROM dbo.AlegraInvoiceRequests WHERE SaleId=@sale",new SqlParameter("@sale",saleId));

        public static DataTable Claim(Guid token)
        {
            return Query(@"
SET XACT_ABORT ON; BEGIN TRAN;
UPDATE dbo.AlegraInvoiceRequests SET Status=CASE WHEN NULLIF(LTRIM(RTRIM(RemoteId)),'') IS NULL THEN 'UNCERTAIN' ELSE 'CREATED' END,
WorkToken=NULL,LastError=N'Trabajo interrumpido; consultar antes de recrear.',NextAttemptAt=DATEADD(minute,1,SYSUTCDATETIME())
WHERE WorkToken IS NOT NULL AND UpdatedAt<DATEADD(minute,-5,SYSUTCDATETIME());
;WITH nextRequest AS (
SELECT TOP(1) * FROM dbo.AlegraInvoiceRequests WITH (UPDLOCK,READPAST,ROWLOCK)
WHERE WorkToken IS NULL AND Status IN ('PENDING','CREATED','ISSUING') AND NextAttemptAt<=SYSUTCDATETIME()
ORDER BY CreatedAt)
UPDATE nextRequest SET WorkToken=@token,UpdatedAt=SYSUTCDATETIME(),Attempts=Attempts+1
OUTPUT inserted.*;
COMMIT;",new SqlParameter("@token",token));
        }

        public static void BindCompany(Guid requestId,Guid token,string companyId)
        {
            var rows = Query(@"UPDATE dbo.AlegraInvoiceRequests SET CompanyKey=@company OUTPUT inserted.RequestId
WHERE RequestId=@id AND WorkToken=@token AND (CompanyKey='default' OR CompanyKey=@company)",
                new SqlParameter("@company",companyId),new SqlParameter("@id",requestId),new SqlParameter("@token",token));
            if (rows.Rows.Count != 1) throw new InvalidOperationException("Esta solicitud pertenece a otra empresa de Alegra.");
        }

        public static void Update(Guid requestId, Guid token, string status, string remoteId, string number, string emission, string error,
            string remoteJson = null)
        {
            var result = Query(@"
UPDATE dbo.AlegraInvoiceRequests SET Status=@status,RemoteId=COALESCE(@remote,RemoteId),InvoiceNumber=COALESCE(@number,InvoiceNumber),
EmissionStatus=COALESCE(@emission,EmissionStatus),LastError=@error,RemoteRequestJson=COALESCE(@json,RemoteRequestJson),
UpdatedAt=SYSUTCDATETIME(),NextAttemptAt=CASE WHEN @status='ACCEPTED' THEN SYSUTCDATETIME() ELSE DATEADD(second,15,SYSUTCDATETIME()) END,WorkToken=NULL
OUTPUT inserted.RequestId WHERE RequestId=@id AND WorkToken=@token;",
                new SqlParameter("@status",status),new SqlParameter("@remote",string.IsNullOrWhiteSpace(remoteId) ? (object)DBNull.Value : remoteId),
                new SqlParameter("@number",(object)number ?? DBNull.Value),new SqlParameter("@emission",(object)emission ?? DBNull.Value),
                new SqlParameter("@error",(object)error ?? DBNull.Value),new SqlParameter("@json",SqlDbType.NVarChar,-1) { Value=(object)remoteJson ?? DBNull.Value },
                new SqlParameter("@id",requestId),new SqlParameter("@token",token));
            if (result.Rows.Count != 1) throw new InvalidOperationException("La solicitud ya está siendo procesada por otra instancia.");
        }

        public static void StorePdf(int saleId,string path) => Query("UPDATE dbo.AlegraInvoiceRequests SET PdfPath=@path,LastError=NULL WHERE SaleId=@sale",
            new SqlParameter("@sale",saleId),new SqlParameter("@path",path));
        public static void MarkPrint(int saleId,string status) => Query("UPDATE dbo.AlegraInvoiceRequests SET PrintStatus=@status,LastError=CASE WHEN @status='PRINTED' THEN NULL ELSE LastError END WHERE SaleId=@sale",
            new SqlParameter("@sale",saleId),new SqlParameter("@status",status));
        public static bool ClaimPrint(int saleId,bool reprint) => Query(@"
UPDATE dbo.AlegraInvoiceRequests SET PrintStatus='PRINTING' OUTPUT inserted.SaleId
WHERE SaleId=@sale AND Status='ACCEPTED' AND (PrintStatus='PENDING' OR (@reprint=1 AND PrintStatus IN ('PRINTED','UNKNOWN','CANCELLED')))",
            new SqlParameter("@sale",saleId),new SqlParameter("@reprint",reprint)).Rows.Count==1;
        // Adelanta el siguiente paso de una solicitud que un usuario está esperando en pantalla.
        // No toca las que fallaron al descargar/imprimir (ACCEPTED con error): esas esperan su reintento.
        public static void Expedite(int saleId) => Query(@"
UPDATE dbo.AlegraInvoiceRequests SET NextAttemptAt=SYSUTCDATETIME()
WHERE SaleId=@sale AND WorkToken IS NULL AND NextAttemptAt>SYSUTCDATETIME()
AND (Status IN ('PENDING','CREATED','ISSUING') OR (Status='ACCEPTED' AND LastError IS NULL))",
            new SqlParameter("@sale",saleId));
        public static void Retry(int saleId) => Query(@"
UPDATE dbo.AlegraInvoiceRequests SET Status=CASE WHEN NULLIF(LTRIM(RTRIM(RemoteId)),'') IS NULL THEN 'PENDING' ELSE 'CREATED' END,
NextAttemptAt=SYSUTCDATETIME(),LastError=NULL WHERE SaleId=@sale AND WorkToken IS NULL AND Status IN ('NEEDS_REVIEW','REJECTED')",
            new SqlParameter("@sale",saleId));
    }
}
