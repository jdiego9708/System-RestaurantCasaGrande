-- Comportamiento real de sp_Actualizar_detalle_pedido (esquema de produccion).
-- Todo dentro de una transaccion que se revierte: la copia queda intacta.
-- Ejecutar SOLO contra CasaGrandeBD_prod.

BEGIN TRANSACTION;

DECLARE @p int, @d int;
DECLARE @plato int = (SELECT TOP 1 Id_plato FROM Platos WHERE Estado = 'ACTIVO');
DECLARE @mesa int = (SELECT TOP 1 Id_mesa FROM Mesas WHERE Id_mesa > 0);
DECLARE @emp int  = (SELECT TOP 1 Id_empleado FROM Empleados WHERE Estado = 'ACTIVO');

INSERT INTO Pedidos (Id_mesa, Id_empleado, Id_cliente, Estado_pedido, Fecha_pedido, Hora_pedido, Tipo_pedido, Observaciones_pedido, CantidadClientes)
VALUES (@mesa, @emp, 0, 'PENDIENTE', '2026-10-24', '12:00', 'MESA', '', 1);
SET @p = SCOPE_IDENTITY();

-- 1. Alta de linea nueva (Id_detalle_pedido = 0): comportamiento del escritorio. Cantidad 3.
EXEC sp_Actualizar_detalle_pedido @Id_detalle_pedidoSalida = @d OUTPUT, @Id_detalle_pedido = 0, @Id_pedido = @p,
     @Id_tipo = @plato, @Tipo = 'PLATO', @Precio = 1000, @Cantidad = 3, @Observaciones = '', @Tipo_update = '';
SELECT '1 alta linea (esperado cantidad 3)' AS caso, Id_detalle_pedido, Cantidad FROM Detalle_pedido WHERE Id_pedido = @p;

-- 2. "Sumar 2" a la linea existente pasando @Cantidad = 2 (semantica de delta que asumia el contrato).
EXEC sp_Actualizar_detalle_pedido @Id_detalle_pedidoSalida = @d, @Id_detalle_pedido = @d, @Id_pedido = @p,
     @Id_tipo = @plato, @Tipo = 'PLATO', @Precio = 1000, @Cantidad = 2, @Observaciones = '', @Tipo_update = '';
SELECT '2 sumar 2 a una linea de 3 (delta daria 5)' AS caso, Id_detalle_pedido, Cantidad FROM Detalle_pedido WHERE Id_pedido = @p;

-- 3. Quitar con Id_detalle_pedido = 0 y 'DELETE': lo que hace FrmPedido.RemoveProduct (no fija el id).
DECLARE @d_actual int = (SELECT TOP 1 Id_detalle_pedido FROM Detalle_pedido WHERE Id_pedido = @p);
EXEC sp_Actualizar_detalle_pedido @Id_detalle_pedidoSalida = @d, @Id_detalle_pedido = 0, @Id_pedido = @p,
     @Id_tipo = @plato, @Tipo = 'PLATO', @Precio = 1000, @Cantidad = 1, @Observaciones = '', @Tipo_update = 'DELETE';
SELECT '3 quitar con id 0 y DELETE (no deberia cambiar nada)' AS caso, Id_detalle_pedido, Cantidad FROM Detalle_pedido WHERE Id_pedido = @p;

-- 4. Quitar una unidad con el id correcto: @Cantidad = cantidad actual - 1.
DECLARE @cant int = (SELECT Cantidad FROM Detalle_pedido WHERE Id_detalle_pedido = @d_actual);
DECLARE @nueva int = @cant - 1;
EXEC sp_Actualizar_detalle_pedido @Id_detalle_pedidoSalida = @d, @Id_detalle_pedido = @d_actual, @Id_pedido = @p,
     @Id_tipo = @plato, @Tipo = 'PLATO', @Precio = 1000, @Cantidad = @nueva, @Observaciones = '', @Tipo_update = 'DELETE';
SELECT '4 quitar 1 con id correcto (cantidad - 1)' AS caso, Id_detalle_pedido, Cantidad FROM Detalle_pedido WHERE Id_pedido = @p;

-- 5. @Cantidad = 0 con id correcto: borra la fila.
EXEC sp_Actualizar_detalle_pedido @Id_detalle_pedidoSalida = @d, @Id_detalle_pedido = @d_actual, @Id_pedido = @p,
     @Id_tipo = @plato, @Tipo = 'PLATO', @Precio = 1000, @Cantidad = 0, @Observaciones = '', @Tipo_update = 'DELETE';
SELECT '5 cantidad 0 con id correcto (borra)' AS caso, COUNT(*) AS filas FROM Detalle_pedido WHERE Id_pedido = @p;

ROLLBACK TRANSACTION;
GO

SELECT 'rollback ok (esperado 38579 pedidos)' AS caso, COUNT(*) AS pedidos FROM Pedidos;
GO
