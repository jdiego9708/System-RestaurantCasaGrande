-- Solo se ejecuta desde Test-DatabaseDeployment.ps1 en su base desechable.
IF DB_NAME() NOT LIKE 'CasaGrandeAudit[_]%' THROW 51000, 'Esta prueba requiere una base temporal de auditoria.', 1;
SET XACT_ABORT ON;
DECLARE @employee int, @client int, @normal int, @delivery int;
INSERT dbo.Empleados (Nombre_empleado, Estado) VALUES ('AUDITORIA NUMERACION', 'ACTIVO');
SET @employee = SCOPE_IDENTITY();
SELECT TOP (1) @client = Id_cliente FROM dbo.Clientes;
INSERT dbo.Pedidos (Id_mesa, Id_empleado, Id_cliente, Estado_pedido, Fecha_pedido, Hora_pedido, Tipo_pedido, Observaciones_pedido)
VALUES (0, @employee, @client, 'FACTURADO', '20261119', '12:00', 'MESA', 'AUDITORIA NUMERACION');
SET @normal = SCOPE_IDENTITY();
INSERT dbo.Pedidos (Id_mesa, Id_empleado, Id_cliente, Estado_pedido, Fecha_pedido, Hora_pedido, Tipo_pedido)
VALUES (0, @employee, @client, 'FACTURADO', '20261119', '12:00', 'DOMICILIO');
SET @delivery = SCOPE_IDENTITY();

-- Una insercion de varias ventas numera por dia y excluye los domicilios.
INSERT dbo.Ventas (Id_pedido, Fecha_venta, Hora_venta, Total_parcial, Propina, Subtotal, Desechables, Domicilio, Total_final)
VALUES (@normal, '20261119', '12:00', 100, 0, 100, 0, 0, 100),
       (@normal, '20261119', '12:01', 100, 0, 100, 0, 0, 100),
       (@normal, '20261120', '12:00', 100, 0, 100, 0, 0, 100),
       (@delivery, '20261119', '12:00', 100, 0, 100, 0, 0, 100);
IF (SELECT COUNT(*) FROM dbo.SaleReceiptNumbers) <> 3 THROW 51000, 'Numero de ventas numeradas incorrecto.', 1;
IF (SELECT MAX(ReceiptNumber) FROM dbo.SaleReceiptNumbers WHERE ReceiptDay='20261119') <> 2 THROW 51000, 'Secuencia diaria incorrecta.', 1;
IF (SELECT MAX(ReceiptNumber) FROM dbo.SaleReceiptNumbers WHERE ReceiptDay='20261120') <> 1 THROW 51000, 'No reinicia por dia.', 1;
IF EXISTS (SELECT 1 FROM dbo.SaleReceiptNumbers n JOIN dbo.Ventas v ON n.SaleId=v.Id_venta WHERE v.Id_pedido=@delivery)
    THROW 51000, 'Un domicilio consumio la secuencia de ventas normales.', 1;

-- El contador y la numeracion revierten junto con la venta si falla su transaccion.
BEGIN TRANSACTION;
INSERT dbo.Ventas (Id_pedido, Fecha_venta, Hora_venta, Total_parcial, Propina, Subtotal, Desechables, Domicilio, Total_final)
VALUES (@normal, '20261119', '12:02', 100, 0, 100, 0, 0, 100);
ROLLBACK;
IF (SELECT LastNumber FROM dbo.SaleReceiptCounters WHERE ReceiptDay='20261119') <> 2 THROW 51000, 'Rollback consumio un numero.', 1;
IF (SELECT COUNT(*) FROM dbo.SaleReceiptNumbers) <> 3 THROW 51000, 'Rollback dejo una referencia huerfana.', 1;

-- Borrar la ultima venta o anular una anterior no libera sus numeros.
DELETE dbo.Ventas WHERE Id_venta = (SELECT SaleId FROM dbo.SaleReceiptNumbers WHERE ReceiptDay='20261119' AND ReceiptNumber=2);
UPDATE dbo.Ventas SET Estado='INACTIVO' WHERE Id_pedido=@normal;
INSERT dbo.Ventas (Id_pedido, Fecha_venta, Hora_venta, Total_parcial, Propina, Subtotal, Desechables, Domicilio, Total_final)
VALUES (@normal, '20261119', '12:03', 100, 0, 100, 0, 0, 100);
IF (SELECT ReceiptNumber FROM dbo.SaleReceiptNumbers WHERE SaleId=CAST(SCOPE_IDENTITY() AS int)) <> 3
    THROW 51000, 'Reutilizo el numero de una venta eliminada.', 1;
PRINT 'OK: numeracion diaria, lote, separacion de domicilios, rollback, anulacion y eliminacion.';
