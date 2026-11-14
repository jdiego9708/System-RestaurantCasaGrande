-- 10_rappi_ordenes.revert.sql
-- Borra dbo.RappiOrders. Se pierden las fichas leidas; los PDF de la carpeta no se tocan y se vuelven a
-- importar solos si se aplica de nuevo el script 10.
SET NOCOUNT ON;
GO

IF OBJECT_ID('dbo.RappiOrders', 'U') IS NOT NULL
    DROP TABLE dbo.RappiOrders;
GO
