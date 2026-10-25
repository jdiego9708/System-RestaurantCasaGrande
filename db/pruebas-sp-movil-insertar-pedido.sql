-- Pruebas de sp_Movil_Insertar_pedido contra CasaGrandeBD_prod (copia local; nunca contra el servidor real).
-- Los casos que escriben van dentro de BEGIN TRANSACTION ... ROLLBACK; los casos de error se ejecutan sin
-- transaccion externa (un ROLLBACK dentro del SP deshace tambien la externa) y comprueban que no queda nada.
-- Requisito: haber ejecutado antes 01_sp_Movil_Insertar_pedido.sql.

CREATE TABLE #res (n int IDENTITY, caso varchar(200), esperado varchar(120), obtenido varchar(120), ok AS (CASE WHEN esperado = obtenido THEN 'OK' ELSE 'FALLA' END));
GO

-- ---------------------------------------------------------------------------------------------------
-- Casos 1-4: alta valida, con ingredientes (dentro de una transaccion que se revierte)
-- ---------------------------------------------------------------------------------------------------
DECLARE @res TABLE (n int IDENTITY, caso varchar(200), esperado varchar(120), obtenido varchar(120));
DECLARE @plato int  = (SELECT TOP 1 Id_plato FROM Platos WHERE Estado = 'ACTIVO' AND Plato_detallado = 'ACTIVO' ORDER BY Id_plato);
DECLARE @bebida int = (SELECT TOP 1 Id_bebida FROM Bebidas WHERE Estado = 'ACTIVO' ORDER BY Id_bebida);
DECLARE @ing1 int   = (SELECT MIN(Id_ingrediente) FROM Ingredientes);
DECLARE @ing2 int   = (SELECT MAX(Id_ingrediente) FROM Ingredientes);
DECLARE @mesa int   = (SELECT TOP 1 Id_mesa FROM Mesas WHERE Id_mesa > 0 ORDER BY Id_mesa);
DECLARE @emp int    = (SELECT TOP 1 Id_empleado FROM Empleados WHERE Estado = 'ACTIVO' ORDER BY Id_empleado);
DECLARE @obsLarga varchar(200) = REPLICATE('x', 150);
DECLARE @items nvarchar(max) = CONCAT(
    '[{"tipo":"PLATO","id_tipo":', @plato, ',"cantidad":2,"observaciones":"', @obsLarga, '",',
    '"ingredientes":[{"id_ingrediente":', @ing1, ',"observaciones":""},{"id_ingrediente":', @ing2, ',"observaciones":"''; DROP TABLE Pedidos;--"}]},',
    '{"tipo":"BEBIDA","id_tipo":', @bebida, ',"cantidad":1,"observaciones":""}]');
DECLARE @p int, @antes int = (SELECT COUNT(*) FROM Pedidos);

BEGIN TRANSACTION;
EXEC dbo.sp_Movil_Insertar_pedido @Id_mesa = @mesa, @Id_empleado = @emp, @Id_cliente = 0, @Tipo_pedido = 'MESA',
     @Observaciones = 'prueba', @CantidadClientes = 3, @Items = @items, @Id_pedido = @p OUTPUT;

INSERT @res (caso, esperado, obtenido) VALUES
 ('1 devuelve Id_pedido', '1', CAST(CASE WHEN @p IS NOT NULL AND @p > 0 THEN 1 ELSE 0 END AS varchar(10))),
 ('1 cabecera PENDIENTE de hoy, MESA, 3 clientes', 'PENDIENTE|1|MESA|3',
    (SELECT CONCAT(Estado_pedido, '|', CASE WHEN Fecha_pedido = CAST(GETDATE() AS date) THEN 1 ELSE 0 END, '|', Tipo_pedido, '|', CantidadClientes) FROM Pedidos WHERE Id_pedido = @p)),
 ('1 dos lineas (1 plato + 1 bebida)', '2', CAST((SELECT COUNT(*) FROM Detalle_pedido WHERE Id_pedido = @p) AS varchar(10))),
 ('2 precio del plato = catalogo', '1',
    CAST((SELECT COUNT(*) FROM Detalle_pedido d JOIN Platos pl ON pl.Id_plato = d.Id_tipo WHERE d.Id_pedido = @p AND d.Tipo = 'PLATO' AND d.Precio = pl.Precio_plato AND d.Cantidad = 2) AS varchar(10))),
 ('2 precio de la bebida = catalogo', '1',
    CAST((SELECT COUNT(*) FROM Detalle_pedido d JOIN Bebidas b ON b.Id_bebida = d.Id_tipo WHERE d.Id_pedido = @p AND d.Tipo = 'BEBIDA' AND d.Precio = b.Precio_bebida AND d.Cantidad = 1) AS varchar(10))),
 ('3 ingredientes: 2, ligados a la linea del plato', '2',
    CAST((SELECT COUNT(*) FROM Detalle_ingredientes_pedido i JOIN Detalle_pedido d ON d.Id_detalle_pedido = i.Id_detalle_pedido
          WHERE i.Id_pedido = @p AND d.Tipo = 'PLATO' AND i.Id_tipo = d.Id_tipo) AS varchar(10))),
 ('3 ningun ingrediente sin Id_detalle_pedido', '0',
    CAST((SELECT COUNT(*) FROM Detalle_ingredientes_pedido WHERE Id_pedido = @p AND Id_detalle_pedido IS NULL) AS varchar(10))),
 ('4 observaciones truncadas a 100', '100',
    CAST((SELECT LEN(Observaciones) FROM Detalle_pedido WHERE Id_pedido = @p AND Tipo = 'PLATO') AS varchar(10))),
 ('4 texto con comilla/DROP guardado literal', '1',
    CAST((SELECT COUNT(*) FROM Detalle_ingredientes_pedido WHERE Id_pedido = @p AND Observaciones = '''; DROP TABLE Pedidos;--') AS varchar(10))),
 ('4 tabla Pedidos sigue existiendo', '1', CAST(CASE WHEN OBJECT_ID('dbo.Pedidos') IS NOT NULL THEN 1 ELSE 0 END AS varchar(10)));
ROLLBACK TRANSACTION;

INSERT @res (caso, esperado, obtenido) VALUES
 ('rollback de los casos 1-4: Pedidos sin cambios', CAST(@antes AS varchar(20)), CAST((SELECT COUNT(*) FROM Pedidos) AS varchar(20)));
INSERT #res (caso, esperado, obtenido) SELECT caso, esperado, obtenido FROM @res ORDER BY n;
GO

-- ---------------------------------------------------------------------------------------------------
-- Casos 13-14: varias filas del mismo producto (como el escritorio): dos almuerzos con ingredientes distintos,
-- y el orden de las lineas se conserva. Dentro de una transaccion que se revierte.
-- ---------------------------------------------------------------------------------------------------
DECLARE @res TABLE (n int IDENTITY, caso varchar(200), esperado varchar(120), obtenido varchar(120));
DECLARE @plato int  = (SELECT TOP 1 Id_plato FROM Platos WHERE Estado = 'ACTIVO' AND Plato_detallado = 'ACTIVO' ORDER BY Id_plato);
DECLARE @bebida int = (SELECT TOP 1 Id_bebida FROM Bebidas WHERE Estado = 'ACTIVO' ORDER BY Id_bebida);
DECLARE @ingA int   = (SELECT MIN(Id_ingrediente) FROM Ingredientes);
DECLARE @ingB int   = (SELECT MAX(Id_ingrediente) FROM Ingredientes);
DECLARE @ingC int   = (SELECT TOP 1 Id_ingrediente FROM Ingredientes WHERE Id_ingrediente NOT IN (@ingA, @ingB) ORDER BY Id_ingrediente);
DECLARE @mesa int   = (SELECT TOP 1 Id_mesa FROM Mesas WHERE Id_mesa > 0 ORDER BY Id_mesa);
DECLARE @emp int    = (SELECT TOP 1 Id_empleado FROM Empleados WHERE Estado = 'ACTIVO' ORDER BY Id_empleado);
DECLARE @items nvarchar(max) = CONCAT(
    '[{"tipo":"PLATO","id_tipo":', @plato, ',"cantidad":1,"ingredientes":[{"id_ingrediente":', @ingA, '}]},',
    '{"tipo":"BEBIDA","id_tipo":', @bebida, ',"cantidad":3},',
    '{"tipo":"PLATO","id_tipo":', @plato, ',"cantidad":1,"ingredientes":[{"id_ingrediente":', @ingB, '},{"id_ingrediente":', @ingC, '}]},',
    '{"tipo":"BEBIDA","id_tipo":', @bebida, ',"cantidad":1}]');
DECLARE @p int, @antes int = (SELECT COUNT(*) FROM Pedidos);

BEGIN TRANSACTION;
EXEC dbo.sp_Movil_Insertar_pedido @Id_mesa = @mesa, @Id_empleado = @emp, @Id_cliente = 0, @Tipo_pedido = 'MESA',
     @Observaciones = '', @CantidadClientes = 0, @Items = @items, @Id_pedido = @p OUTPUT;

DECLARE @orden varchar(120) = (SELECT STRING_AGG(CONCAT(Tipo, ':', Cantidad), ',') WITHIN GROUP (ORDER BY Id_detalle_pedido) FROM Detalle_pedido WHERE Id_pedido = @p);
DECLARE @l1 int = (SELECT MIN(Id_detalle_pedido) FROM Detalle_pedido WHERE Id_pedido = @p);
DECLARE @l3 int = (SELECT Id_detalle_pedido FROM (SELECT Id_detalle_pedido, ROW_NUMBER() OVER (ORDER BY Id_detalle_pedido) AS r FROM Detalle_pedido WHERE Id_pedido = @p) x WHERE r = 3);
INSERT @res (caso, esperado, obtenido) VALUES
 ('13 cuatro filas, dos del mismo plato y dos de la misma bebida', '4', CAST((SELECT COUNT(*) FROM Detalle_pedido WHERE Id_pedido = @p) AS varchar(10))),
 ('13 el orden recibido se conserva', 'PLATO:1,BEBIDA:3,PLATO:1,BEBIDA:1', @orden),
 ('14 la primera fila del plato tiene SOLO su ingrediente A', CAST(@ingA AS varchar(20)),
    (SELECT STRING_AGG(CAST(Id_ingrediente AS varchar(20)), ',') WITHIN GROUP (ORDER BY Id_ingrediente) FROM Detalle_ingredientes_pedido WHERE Id_detalle_pedido = @l1)),
 ('14 la segunda fila del plato tiene SOLO B y C', (SELECT STRING_AGG(v, ',') WITHIN GROUP (ORDER BY v) FROM (VALUES (CAST(@ingB AS varchar(20))), (CAST(@ingC AS varchar(20)))) t(v)),
    (SELECT STRING_AGG(CAST(Id_ingrediente AS varchar(20)), ',') WITHIN GROUP (ORDER BY Id_ingrediente) FROM Detalle_ingredientes_pedido WHERE Id_detalle_pedido = @l3)),
 ('14 3 ingredientes en total, todos con linea', '3|0',
    CONCAT((SELECT COUNT(*) FROM Detalle_ingredientes_pedido WHERE Id_pedido = @p), '|', (SELECT COUNT(*) FROM Detalle_ingredientes_pedido WHERE Id_pedido = @p AND Id_detalle_pedido IS NULL)));
ROLLBACK TRANSACTION;

INSERT @res (caso, esperado, obtenido) VALUES
 ('rollback de los casos 13-14: Pedidos sin cambios', CAST(@antes AS varchar(20)), CAST((SELECT COUNT(*) FROM Pedidos) AS varchar(20)));
INSERT #res (caso, esperado, obtenido) SELECT caso, esperado, obtenido FROM @res ORDER BY n;
GO

-- ---------------------------------------------------------------------------------------------------
-- Casos de error: sin transaccion externa. Cada uno comprueba el numero de error y que no queda nada.
-- ---------------------------------------------------------------------------------------------------
DECLARE @plato int  = (SELECT TOP 1 Id_plato FROM Platos WHERE Estado = 'ACTIVO' AND Plato_detallado = 'ACTIVO' ORDER BY Id_plato);
DECLARE @platoInactivo int = (SELECT TOP 1 Id_plato FROM Platos WHERE Estado <> 'ACTIVO' ORDER BY Id_plato);
DECLARE @bebida int = (SELECT TOP 1 Id_bebida FROM Bebidas WHERE Estado = 'ACTIVO' ORDER BY Id_bebida);
DECLARE @mesa int   = (SELECT TOP 1 Id_mesa FROM Mesas WHERE Id_mesa > 0 ORDER BY Id_mesa);
DECLARE @emp int    = (SELECT TOP 1 Id_empleado FROM Empleados WHERE Estado = 'ACTIVO' ORDER BY Id_empleado);

DECLARE @casos TABLE (n int IDENTITY, caso varchar(200), esperado int, items nvarchar(max));
INSERT @casos (caso, esperado, items) VALUES
 ('5 lista vacia []',                          50001, N'[]'),
 ('6 JSON invalido',                           50004, N'esto no es json'),
 ('6 @Items NULL',                             50004, NULL),
 ('7 producto inexistente',                    50002, N'[{"tipo":"PLATO","id_tipo":2147000000,"cantidad":1}]'),
 ('8 plato inactivo',                          50002, CONCAT('[{"tipo":"PLATO","id_tipo":', @platoInactivo, ',"cantidad":1}]')),
 ('9 cantidad 0',                              50002, CONCAT('[{"tipo":"BEBIDA","id_tipo":', @bebida, ',"cantidad":0}]')),
 ('9 cantidad negativa',                       50002, CONCAT('[{"tipo":"BEBIDA","id_tipo":', @bebida, ',"cantidad":-2}]')),
 ('10 tipo invalido POSTRE',                   50002, CONCAT('[{"tipo":"POSTRE","id_tipo":', @bebida, ',"cantidad":1}]')),
 ('11 @Items es un objeto, no un array',        50004, N'{"tipo":"BEBIDA","id_tipo":1,"cantidad":1}'),
 -- Falla DENTRO de la transaccion (Id_ingrediente NULL viola NOT NULL) tras insertar cabecera y lineas:
 ('12 error a mitad de transaccion (rollback real)', 515,
    CONCAT('[{"tipo":"PLATO","id_tipo":', @plato, ',"cantidad":1,"ingredientes":[{"observaciones":"sin id"}]},{"tipo":"BEBIDA","id_tipo":', @bebida, ',"cantidad":1}]'));

DECLARE @i int = 1, @max int = (SELECT MAX(n) FROM @casos), @caso varchar(200), @esp int, @it nvarchar(max), @err int, @p int;
WHILE @i <= @max
BEGIN
    SELECT @caso = caso, @esp = esperado, @it = items FROM @casos WHERE n = @i;
    DECLARE @a int = (SELECT COUNT(*) FROM Pedidos), @b int = (SELECT COUNT(*) FROM Detalle_pedido), @c int = (SELECT COUNT(*) FROM Detalle_ingredientes_pedido);
    SET @err = 0; SET @p = NULL;
    BEGIN TRY
        EXEC dbo.sp_Movil_Insertar_pedido @Id_mesa = @mesa, @Id_empleado = @emp, @Id_cliente = 0, @Tipo_pedido = 'MESA',
             @Observaciones = '', @CantidadClientes = 1, @Items = @it, @Id_pedido = @p OUTPUT;
    END TRY
    BEGIN CATCH
        SET @err = ERROR_NUMBER();
    END CATCH
    INSERT #res (caso, esperado, obtenido) VALUES (@caso + ' -> error', CAST(@esp AS varchar(10)), CAST(@err AS varchar(10)));
    INSERT #res (caso, esperado, obtenido) VALUES (@caso + ' -> sin residuo (Pedidos/Detalle/Ingredientes)', '0|0|0',
        CONCAT((SELECT COUNT(*) FROM Pedidos) - @a, '|', (SELECT COUNT(*) FROM Detalle_pedido) - @b, '|', (SELECT COUNT(*) FROM Detalle_ingredientes_pedido) - @c));
    INSERT #res (caso, esperado, obtenido) VALUES (@caso + ' -> @Id_pedido NULL', 'NULL', ISNULL(CAST(@p AS varchar(20)), 'NULL'));
    SET @i += 1;
END
GO

INSERT #res (caso, esperado, obtenido) VALUES ('FINAL Pedidos intacto (38579 en la copia original)', '38579', CAST((SELECT COUNT(*) FROM Pedidos) AS varchar(20)));
INSERT #res (caso, esperado, obtenido) VALUES ('FINAL Detalle_pedido intacto', '157217', CAST((SELECT COUNT(*) FROM Detalle_pedido) AS varchar(20)));
INSERT #res (caso, esperado, obtenido) VALUES ('FINAL Detalle_ingredientes_pedido intacto', '324508', CAST((SELECT COUNT(*) FROM Detalle_ingredientes_pedido) AS varchar(20)));
GO

SELECT n, caso, esperado, obtenido, ok FROM #res ORDER BY n;
SELECT CONCAT(SUM(CASE WHEN ok = 'OK' THEN 1 ELSE 0 END), ' OK de ', COUNT(*), ' (', SUM(CASE WHEN ok = 'FALLA' THEN 1 ELSE 0 END), ' fallan)') AS resumen FROM #res;
GO
