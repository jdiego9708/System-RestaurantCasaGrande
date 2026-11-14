-- 10 - Rappi: pedidos leidos de las facturas PDF de la carpeta FacturasRappi.
--
-- Motivo (2026-11-14): sin integracion por API, el escritorio vigila una carpeta con las facturas PDF de Rappi,
-- lee sus datos y los muestra en fichas informativas del panel RAPPI de Domicilios. Cada orden se guarda una
-- sola vez: ExternalId (el ID de orden de Rappi) es unico, asi que copiar dos veces la misma factura no crea
-- una segunda ficha y todos los equipos ven lo mismo.
--
-- Que hace (idempotente: repetirlo no cambia nada):
--   Crea la tabla dbo.RappiOrders y su indice por fecha. No toca Pedidos ni Ventas: son fichas informativas,
--   no pasan por el flujo de preparacion/facturacion.
--
-- Reversion: 10_rappi_ordenes.revert.sql
--
-- Uso (primero contra la copia local):
--   cd db
--   .\Invoke-Sql.ps1 -File .\10_rappi_ordenes.sql

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID('dbo.RappiOrders', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.RappiOrders
    (
        Id               int IDENTITY(1,1) NOT NULL CONSTRAINT PK_RappiOrders PRIMARY KEY,
        -- ID de orden de Rappi; si el PDF no se pudo leer del todo, "HASH:" + huella del archivo.
        ExternalId       varchar(80)    NOT NULL,
        CustomerName     nvarchar(150)  NULL,
        Phone            varchar(40)    NULL,
        Address          nvarchar(600)  NULL,
        PaymentMethod    nvarchar(60)   NULL,
        Platform         nvarchar(40)   NULL,
        -- Fecha y hora de la orden segun la factura.
        OrderDateTime    datetime2(0)   NULL,
        -- Una linea por producto: "2x NOMBRE $55.000".
        ProductsText     nvarchar(max)  NULL,
        ProductCount     int            NOT NULL CONSTRAINT DF_RappiOrders_ProductCount DEFAULT (0),
        Subtotal         int            NULL,
        Discount         int            NULL,
        ShippingCost     int            NULL,
        Tip              int            NULL,
        ServiceFee       int            NULL,
        PaidThroughRappi int            NULL,
        Total            int            NULL,
        CashPayWith      int            NULL,
        ChangeToBring    int            NULL,
        DeliveryCode     varchar(30)    NULL,
        -- False si faltaron datos clave (ID o total): la ficha lo avisa.
        IsComplete       bit            NOT NULL CONSTRAINT DF_RappiOrders_IsComplete DEFAULT (1),
        SourceFile       nvarchar(260)  NULL,
        FileHash         char(64)       NULL,
        ImportedAt       datetime2(0)   NOT NULL CONSTRAINT DF_RappiOrders_ImportedAt DEFAULT (SYSDATETIME()),
        CONSTRAINT UQ_RappiOrders_ExternalId UNIQUE (ExternalId)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_RappiOrders_OrderDateTime' AND object_id = OBJECT_ID('dbo.RappiOrders'))
    CREATE INDEX IX_RappiOrders_OrderDateTime ON dbo.RappiOrders (OrderDateTime DESC, ImportedAt DESC);
GO

PRINT '10: dbo.RappiOrders lista.';
GO
