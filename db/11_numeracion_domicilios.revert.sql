-- 11_numeracion_domicilios.revert.sql
-- Quita la numeracion diaria de domicilios y de Rappi (indices y columnas). Se pierden los numeros ya asignados; las
-- facturas ya impresas conservan el que se imprimio en papel.
SET NOCOUNT ON;
GO

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_Pedidos_DeliveryNumber' AND object_id = OBJECT_ID('dbo.Pedidos'))
    DROP INDEX UX_Pedidos_DeliveryNumber ON dbo.Pedidos;
GO
IF COL_LENGTH('dbo.Pedidos', 'DeliveryNumber') IS NOT NULL
    ALTER TABLE dbo.Pedidos DROP COLUMN DeliveryNumber;
GO

IF OBJECT_ID('dbo.RappiOrders', 'U') IS NOT NULL
BEGIN
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_RappiOrders_Day_Number' AND object_id = OBJECT_ID('dbo.RappiOrders'))
        DROP INDEX UX_RappiOrders_Day_Number ON dbo.RappiOrders;
    IF COL_LENGTH('dbo.RappiOrders', 'DayNumber') IS NOT NULL
        ALTER TABLE dbo.RappiOrders DROP COLUMN DayNumber;
    IF COL_LENGTH('dbo.RappiOrders', 'NumberDay') IS NOT NULL
        ALTER TABLE dbo.RappiOrders DROP COLUMN NumberDay;
END
GO
