-- Numeracion diaria persistente de las ventas del programa (excepto domicilios).
-- No reconstruye numeros historicos: el contador de impresion antiguo no los guardaba.
-- Una venta conserva su numero al reimprimir, anular o eliminar otras ventas.
-- Las tablas separadas conservan el INSERT heredado de Ventas y evitan reutilizar numeros eliminados.
SET XACT_ABORT ON;
GO
IF OBJECT_ID('dbo.SaleReceiptCounters', 'U') IS NULL
    CREATE TABLE dbo.SaleReceiptCounters (
        ReceiptDay date NOT NULL CONSTRAINT PK_SaleReceiptCounters PRIMARY KEY,
        LastNumber int NOT NULL
    );
IF OBJECT_ID('dbo.SaleReceiptNumbers', 'U') IS NULL
    CREATE TABLE dbo.SaleReceiptNumbers (
        SaleId int NOT NULL CONSTRAINT PK_SaleReceiptNumbers PRIMARY KEY,
        ReceiptDay date NOT NULL,
        ReceiptNumber int NOT NULL,
        CONSTRAINT UQ_SaleReceiptNumbers_Day_Number UNIQUE (ReceiptDay, ReceiptNumber)
    );
GO
CREATE OR ALTER TRIGGER dbo.tr_Ventas_AssignReceiptNumber ON dbo.Ventas
AFTER INSERT AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM inserted i LEFT JOIN dbo.Pedidos p ON p.Id_pedido = i.Id_pedido
                   WHERE ISNULL(p.Tipo_pedido, '') <> 'DOMICILIO') RETURN;

    -- Bloqueo transaccional: el numero y la venta se confirman o revierten juntos, entre todos los equipos.
    DECLARE @lockResult int;
    EXEC @lockResult = sys.sp_getapplock @Resource = 'CasaGrande.SaleReceiptNumbers',
         @LockMode = 'Exclusive', @LockOwner = 'Transaction', @LockTimeout = 15000;
    IF @lockResult < 0 THROW 51012, 'No se pudo asignar el numero de la venta. Reintente.', 1;

    DECLARE @days TABLE (ReceiptDay date PRIMARY KEY);
    INSERT @days SELECT DISTINCT CAST(i.Fecha_venta AS date)
    FROM inserted i LEFT JOIN dbo.Pedidos p ON p.Id_pedido = i.Id_pedido
    WHERE ISNULL(p.Tipo_pedido, '') <> 'DOMICILIO';
    DECLARE @day date, @last int, @count int;
    WHILE EXISTS (SELECT 1 FROM @days)
    BEGIN
        SELECT @day = MIN(ReceiptDay) FROM @days;
        IF NOT EXISTS (SELECT 1 FROM dbo.SaleReceiptCounters WHERE ReceiptDay = @day)
            INSERT dbo.SaleReceiptCounters VALUES (@day, 0);
        SELECT @last = LastNumber FROM dbo.SaleReceiptCounters WHERE ReceiptDay = @day;
        INSERT dbo.SaleReceiptNumbers (SaleId, ReceiptDay, ReceiptNumber)
        SELECT i.Id_venta, @day, @last + ROW_NUMBER() OVER (ORDER BY i.Hora_venta, i.Id_venta)
        FROM inserted i LEFT JOIN dbo.Pedidos p ON p.Id_pedido = i.Id_pedido
        WHERE CAST(i.Fecha_venta AS date) = @day AND ISNULL(p.Tipo_pedido, '') <> 'DOMICILIO';
        SET @count = @@ROWCOUNT;
        UPDATE dbo.SaleReceiptCounters SET LastNumber = @last + @count WHERE ReceiptDay = @day;
        DELETE @days WHERE ReceiptDay = @day;
    END;
END;
GO
