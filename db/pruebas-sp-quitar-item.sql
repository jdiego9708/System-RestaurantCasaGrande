-- Prueba de sp_Movil_Quitar_item con registro en Delete_pedidos. Todo dentro de una transaccion que SE REVIERTE.
SET NOCOUNT ON;
BEGIN TRANSACTION;

DECLARE @emp int = (SELECT TOP 1 Id_empleado FROM dbo.Empleados WHERE Id_empleado > 0);
DECLARE @cli int = (SELECT TOP 1 Id_cliente FROM dbo.Clientes);
DECLARE @mesa int = (SELECT TOP 1 Id_mesa FROM dbo.Mesas WHERE Id_mesa > 0);
DECLARE @plato int = (SELECT TOP 1 Id_plato FROM dbo.Platos);
DECLARE @p int;

INSERT INTO dbo.Pedidos (Id_mesa, Id_cliente, Id_empleado, Estado_pedido, Fecha_pedido, Hora_pedido, Tipo_pedido)
VALUES (@mesa, @cli, @emp, 'PENDIENTE', CONVERT(date, GETDATE()), CONVERT(time, GETDATE()), 'MESA');
SET @p = SCOPE_IDENTITY();
INSERT INTO dbo.Detalle_pedido (Id_pedido, Id_tipo, Tipo, Precio, Cantidad, Observaciones) VALUES (@p, @plato, 'PLATO', 1000, 3, '');
DECLARE @d int = SCOPE_IDENTITY();
DECLARE @r int;

EXEC dbo.sp_Movil_Quitar_item @p, @d, 2, @r OUTPUT, 'se equivoco el mesero', 0, @emp;
SELECT 'quitar 2 de 3: restante (esperado 1)' AS caso, @r AS restante,
       (SELECT COUNT(*) FROM dbo.Delete_pedidos WHERE Id_pedido = @p) AS filas_eliminados_esperado_2;

EXEC dbo.sp_Movil_Quitar_item @p, @d, 1, @r OUTPUT, 'cliente se fue', @emp, @emp;
SELECT 'quitar la ultima: restante (esperado 0)' AS caso, @r AS restante,
       (SELECT COUNT(*) FROM dbo.Delete_pedidos WHERE Id_pedido = @p) AS filas_eliminados_esperado_3;

SELECT Id_pedido, Tipo, Fecha, CONVERT(varchar(5), Hora) AS Hora, Motivo FROM dbo.Delete_pedidos WHERE Id_pedido = @p;

ROLLBACK TRANSACTION;
SELECT 'rollback hecho; filas que quedan (esperado 0)' AS caso, COUNT(*) AS n FROM dbo.Delete_pedidos WHERE Motivo IN ('se equivoco el mesero', 'cliente se fue');
