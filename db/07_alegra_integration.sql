-- Base aditiva e idempotente de la integracion. No envia datos a Alegra ni almacena tokens.
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF OBJECT_ID('dbo.AlegraCustomerLinks','U') IS NULL
CREATE TABLE dbo.AlegraCustomerLinks (
    CompanyKey varchar(100) NOT NULL,
    CustomerId int NOT NULL REFERENCES dbo.Clientes(Id_cliente),
    RemoteId varchar(100) NOT NULL,
    FiscalJson nvarchar(max) NULL,
    CONSTRAINT PK_AlegraCustomerLinks PRIMARY KEY(CompanyKey, CustomerId)
);
IF OBJECT_ID('dbo.AlegraItemLinks','U') IS NULL
CREATE TABLE dbo.AlegraItemLinks (
    CompanyKey varchar(100) NOT NULL,
    ItemType varchar(50) NOT NULL,
    ItemId int NOT NULL,
    RemoteId varchar(100) NOT NULL,
    TaxJson nvarchar(max) NULL,
    CONSTRAINT PK_AlegraItemLinks PRIMARY KEY(CompanyKey,ItemType,ItemId)
);
IF OBJECT_ID('dbo.AlegraInvoiceRequests','U') IS NULL
CREATE TABLE dbo.AlegraInvoiceRequests (
    RequestId uniqueidentifier NOT NULL CONSTRAINT DF_AlegraRequestId DEFAULT NEWID(),
    SaleId int NOT NULL REFERENCES dbo.Ventas(Id_venta),
    CompanyKey varchar(100) NOT NULL,
    Status varchar(30) NOT NULL CONSTRAINT DF_AlegraStatus DEFAULT 'PENDING',
    RemoteId varchar(100) NULL,
    InvoiceNumber varchar(100) NULL,
    EmissionStatus varchar(100) NULL,
    RequestJson nvarchar(max) NOT NULL,
    LastError nvarchar(2000) NULL,
    Attempts int NOT NULL CONSTRAINT DF_AlegraAttempts DEFAULT 0,
    PdfPath nvarchar(1000) NULL,
    PrintStatus varchar(30) NOT NULL CONSTRAINT DF_AlegraPrintStatus DEFAULT 'PENDING',
    CreatedAt datetime2 NOT NULL CONSTRAINT DF_AlegraCreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedAt datetime2 NOT NULL CONSTRAINT DF_AlegraUpdatedAt DEFAULT SYSUTCDATETIME(),
    Revision rowversion NOT NULL,
    CONSTRAINT PK_AlegraInvoiceRequests PRIMARY KEY(RequestId),
    CONSTRAINT UQ_AlegraInvoiceRequests_Sale UNIQUE(SaleId)
);
IF OBJECT_ID('dbo.AlegraSettings','U') IS NULL
CREATE TABLE dbo.AlegraSettings (
    CompanyKey varchar(100) NOT NULL CONSTRAINT PK_AlegraSettings PRIMARY KEY,
    ConfigurationJson nvarchar(max) NOT NULL,
    UpdatedAt datetime2 NOT NULL CONSTRAINT DF_AlegraSettingsUpdated DEFAULT SYSUTCDATETIME()
);
IF COL_LENGTH('dbo.AlegraInvoiceRequests','RemoteRequestJson') IS NULL
    ALTER TABLE dbo.AlegraInvoiceRequests ADD RemoteRequestJson nvarchar(max) NULL;
IF COL_LENGTH('dbo.AlegraInvoiceRequests','WorkToken') IS NULL
    ALTER TABLE dbo.AlegraInvoiceRequests ADD WorkToken uniqueidentifier NULL;
IF COL_LENGTH('dbo.AlegraInvoiceRequests','NextAttemptAt') IS NULL
    ALTER TABLE dbo.AlegraInvoiceRequests ADD NextAttemptAt datetime2 NOT NULL CONSTRAINT DF_AlegraNextAttempt DEFAULT SYSUTCDATETIME();
COMMIT;
