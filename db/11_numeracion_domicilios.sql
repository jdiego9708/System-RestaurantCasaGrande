-- 11 - Domicilios: numeracion diaria de las facturas (una para el programa y otra para Rappi).
--
-- Motivo (2026-11-14): cada factura del modulo de Domicilios lleva un numero de 1 a N que empieza de nuevo cada dia, y
-- Rappi tiene su propia numeracion, separada. El numero se ve arriba a la derecha de la factura. Es un numero de
-- orden del dia y no tiene valor tributario.
--   * Programa: Pedidos.DeliveryNumber, solo para pedidos de tipo DOMICILIO, por Fecha_pedido.
--   * Rappi:    RappiOrders.DayNumber, por el dia de la orden (NumberDay).
-- Un indice unico por dia impide repetir un numero aunque dos equipos numeren a la vez.
--
-- Que hace (idempotente: repetirlo no cambia nada):
--   1. Agrega Pedidos.DeliveryNumber, RappiOrders.NumberDay y RappiOrders.DayNumber (todas NULL).
--   2. Numera lo que ya existe, por dia y en orden de hora (las filas que ya tienen numero no se tocan).
--   3. Crea los indices unicos por dia.
--
-- Reversion: 11_numeracion_domicilios.revert.sql
--
-- Uso (primero contra la copia local):
--   cd db
--   .\Invoke-Sql.ps1 -File .\11_numeracion_domicilios.sql

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF COL_LENGTH('dbo.Pedidos', 'DeliveryNumber') IS NULL
    ALTER TABLE dbo.Pedidos ADD DeliveryNumber int NULL;
GO

IF OBJECT_ID('dbo.RappiOrders', 'U') IS NOT NULL
BEGIN
    IF COL_LENGTH('dbo.RappiOrders', 'NumberDay') IS NULL
        ALTER TABLE dbo.RappiOrders ADD NumberDay date NULL;
    IF COL_LENGTH('dbo.RappiOrders', 'DayNumber') IS NULL
        ALTER TABLE dbo.RappiOrders ADD DayNumber int NULL;
END
GO

-- Domicilios existentes: 1..N por dia, por hora del pedido.
WITH ordered AS
(
    SELECT Id_pedido,
           ROW_NUMBER() OVER (PARTITION BY Fecha_pedido ORDER BY Hora_pedido, Id_pedido) AS rn
    FROM dbo.Pedidos
    WHERE Tipo_pedido = 'DOMICILIO' AND DeliveryNumber IS NULL
)
UPDATE p
SET DeliveryNumber = ISNULL((SELECT MAX(x.DeliveryNumber) FROM dbo.Pedidos x
                             WHERE x.Tipo_pedido = 'DOMICILIO' AND x.Fecha_pedido = p.Fecha_pedido
                               AND x.DeliveryNumber IS NOT NULL), 0) + o.rn
FROM dbo.Pedidos p
INNER JOIN ordered o ON o.Id_pedido = p.Id_pedido;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_Pedidos_DeliveryNumber' AND object_id = OBJECT_ID('dbo.Pedidos'))
    CREATE UNIQUE INDEX UX_Pedidos_DeliveryNumber ON dbo.Pedidos (Fecha_pedido, DeliveryNumber)
    WHERE DeliveryNumber IS NOT NULL AND Tipo_pedido = 'DOMICILIO';
GO

IF OBJECT_ID('dbo.RappiOrders', 'U') IS NOT NULL
BEGIN
    -- Ordenes de Rappi existentes: el dia de la orden (o el de importacion si la factura no traia fecha).
    EXEC('UPDATE dbo.RappiOrders SET NumberDay = CAST(COALESCE(OrderDateTime, ImportedAt) AS date) WHERE NumberDay IS NULL;');
    EXEC('
    WITH ordered AS
    (
        SELECT Id,
               ROW_NUMBER() OVER (PARTITION BY NumberDay ORDER BY COALESCE(OrderDateTime, ImportedAt), Id) AS rn
        FROM dbo.RappiOrders
        WHERE DayNumber IS NULL
    )
    UPDATE r
    SET DayNumber = ISNULL((SELECT MAX(x.DayNumber) FROM dbo.RappiOrders x
                            WHERE x.NumberDay = r.NumberDay AND x.DayNumber IS NOT NULL), 0) + o.rn
    FROM dbo.RappiOrders r
    INNER JOIN ordered o ON o.Id = r.Id;');
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_RappiOrders_Day_Number' AND object_id = OBJECT_ID('dbo.RappiOrders'))
        EXEC('CREATE UNIQUE INDEX UX_RappiOrders_Day_Number ON dbo.RappiOrders (NumberDay, DayNumber) WHERE DayNumber IS NOT NULL;');
END
GO

PRINT '11: numeracion diaria de domicilios y de Rappi lista.';
GO
