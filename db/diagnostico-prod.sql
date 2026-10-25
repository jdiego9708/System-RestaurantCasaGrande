-- Diagnostico de solo lectura sobre la copia de produccion (CasaGrandeBD_prod).
-- Contrasta con datos reales los supuestos de docs/. No modifica nada.

SELECT '01 version' AS q, @@VERSION AS valor, SERVERPROPERTY('Collation') AS collation_servidor,
       DATABASEPROPERTYEX(DB_NAME(), 'Collation') AS collation_bd;
GO

SELECT '02 Estado_pedido' AS q, Estado_pedido, COUNT(*) AS n FROM Pedidos GROUP BY Estado_pedido ORDER BY n DESC;
GO

SELECT '03 Tipo_pedido' AS q, Tipo_pedido, COUNT(*) AS n FROM Pedidos GROUP BY Tipo_pedido;
GO

-- Pedidos abiertos: de hoy (copia tomada el 2026-10-24) y huerfanos de dias anteriores.
SELECT '04 abiertos' AS q,
       CASE WHEN Fecha_pedido = '2026-10-24' THEN 'hoy' ELSE 'anteriores' END AS cuando,
       Estado_pedido, COUNT(*) AS n
FROM Pedidos WHERE Estado_pedido IN ('PENDIENTE','SALIENDO')
GROUP BY CASE WHEN Fecha_pedido = '2026-10-24' THEN 'hoy' ELSE 'anteriores' END, Estado_pedido;
GO

-- Mesas con mas de un pedido abierto el mismo dia: valida la regla "una mesa, un pedido abierto".
SELECT '05 mesa con >1 abierto el mismo dia' AS q, COUNT(*) AS mesas_dia_afectadas
FROM (SELECT Id_mesa, Fecha_pedido FROM Pedidos
      WHERE Estado_pedido IN ('PENDIENTE','SALIENDO')
      GROUP BY Id_mesa, Fecha_pedido HAVING COUNT(*) > 1) x;
GO

-- Carga: pedidos por dia, y por hora el dia mas cargado.
SELECT TOP 5 '06 dias mas cargados' AS q, Fecha_pedido, COUNT(*) AS pedidos
FROM Pedidos GROUP BY Fecha_pedido ORDER BY pedidos DESC;
GO

-- Lineas de un mismo pedido con el mismo (Id_tipo, Tipo): el SP actual solo las une por Id_detalle_pedido.
SELECT '07 lineas duplicadas (pedido,tipo,id)' AS q, COUNT(*) AS casos
FROM (SELECT Id_pedido, Id_tipo, Tipo FROM Detalle_pedido
      GROUP BY Id_pedido, Id_tipo, Tipo HAVING COUNT(*) > 1) x;
GO

-- Ingredientes: cuantos traen Id_detalle_pedido y cuantos no.
SELECT '08 ingredientes' AS q,
       SUM(CASE WHEN Id_detalle_pedido IS NULL THEN 1 ELSE 0 END) AS sin_id_detalle,
       SUM(CASE WHEN Id_detalle_pedido IS NOT NULL THEN 1 ELSE 0 END) AS con_id_detalle,
       SUM(CASE WHEN Observaciones <> '' THEN 1 ELSE 0 END) AS con_observaciones
FROM Detalle_ingredientes_pedido;
GO

-- Precios: hay decimales reales? (Precio es decimal(19,2), el TVP usa int).
SELECT '09 precios con decimales' AS q,
       (SELECT COUNT(*) FROM Detalle_pedido WHERE Precio <> FLOOR(Precio)) AS detalle_pedido,
       (SELECT COUNT(*) FROM Platos WHERE Precio_plato <> FLOOR(Precio_plato)) AS platos,
       (SELECT COUNT(*) FROM Bebidas WHERE Precio_bebida <> FLOOR(Precio_bebida)) AS bebidas;
GO

SELECT '10 Platos' AS q, Estado, Plato_detallado, Plato_carta, COUNT(*) AS n
FROM Platos GROUP BY Estado, Plato_detallado, Plato_carta ORDER BY n DESC;
GO

SELECT '11 Bebidas' AS q, Estado, COUNT(*) AS n FROM Bebidas GROUP BY Estado;
GO

-- Imagenes: forma de los nombres (sin ruta, con extension).
SELECT TOP 5 '12 imagenes plato' AS q, Imagen_plato FROM Platos WHERE Imagen_plato IS NOT NULL AND Estado = 'ACTIVO';
GO
SELECT '13 imagenes con ruta o vacias' AS q,
       SUM(CASE WHEN Imagen_plato LIKE '%\%' OR Imagen_plato LIKE '%/%' THEN 1 ELSE 0 END) AS con_ruta,
       SUM(CASE WHEN Imagen_plato IS NULL OR Imagen_plato = '' THEN 1 ELSE 0 END) AS vacias,
       COUNT(*) AS total
FROM Platos;
GO

-- Empleados activos: cargo y calidad del PIN (sin mostrar claves).
SELECT '14 empleados activos' AS q, Cargo_empleado,
       COUNT(*) AS n,
       SUM(CASE WHEN Codigo_maestro IS NULL THEN 1 ELSE 0 END) AS sin_pin,
       SUM(CASE WHEN Codigo_maestro BETWEEN 1000 AND 9999 THEN 1 ELSE 0 END) AS pin_4_digitos,
       SUM(CASE WHEN Codigo_maestro IS NOT NULL AND Codigo_maestro NOT BETWEEN 1000 AND 9999 THEN 1 ELSE 0 END) AS pin_otra_longitud
FROM Empleados WHERE Estado = 'ACTIVO' GROUP BY Cargo_empleado;
GO

SELECT '15 PIN repetidos entre activos' AS q, COUNT(*) AS pines_repetidos, ISNULL(SUM(n), 0) AS empleados_afectados
FROM (SELECT Codigo_maestro, COUNT(*) AS n FROM Empleados
      WHERE Estado = 'ACTIVO' AND Codigo_maestro IS NOT NULL
      GROUP BY Codigo_maestro HAVING COUNT(*) > 1) x;
GO

-- Quien tomo los pedidos de los ultimos 30 dias: estan activos?
SELECT '16 empleados con pedidos en 30 dias' AS q, e.Estado, e.Cargo_empleado, COUNT(DISTINCT p.Id_empleado) AS empleados, COUNT(*) AS pedidos
FROM Pedidos p LEFT JOIN Empleados e ON e.Id_empleado = p.Id_empleado
WHERE p.Fecha_pedido >= '2026-09-24'
GROUP BY e.Estado, e.Cargo_empleado;
GO

SELECT '17 mesas' AS q, COUNT(*) AS total, SUM(CASE WHEN Id_mesa = 0 THEN 1 ELSE 0 END) AS mesa_domicilio FROM Mesas;
GO

-- Pedidos cuya mesa no existe o cuyo cliente no existe (no hay FKs).
SELECT '18 huerfanos' AS q,
       (SELECT COUNT(*) FROM Pedidos p WHERE NOT EXISTS (SELECT 1 FROM Mesas m WHERE m.Id_mesa = p.Id_mesa)) AS sin_mesa,
       (SELECT COUNT(*) FROM Pedidos p WHERE NOT EXISTS (SELECT 1 FROM Clientes c WHERE c.Id_cliente = p.Id_cliente)) AS sin_cliente,
       (SELECT COUNT(*) FROM Detalle_pedido d WHERE NOT EXISTS (SELECT 1 FROM Pedidos p WHERE p.Id_pedido = d.Id_pedido)) AS lineas_sin_pedido,
       (SELECT COUNT(*) FROM Pedidos p WHERE NOT EXISTS (SELECT 1 FROM Detalle_pedido d WHERE d.Id_pedido = p.Id_pedido)) AS pedidos_sin_lineas;
GO

-- Como queda el pedido al facturar: estado de los pedidos que tienen venta y de los que no.
SELECT '19 pedido vs venta' AS q, p.Estado_pedido,
       SUM(CASE WHEN v.Id_pedido IS NOT NULL THEN 1 ELSE 0 END) AS con_venta,
       SUM(CASE WHEN v.Id_pedido IS NULL THEN 1 ELSE 0 END) AS sin_venta
FROM Pedidos p
LEFT JOIN (SELECT DISTINCT Id_pedido FROM Ventas) v ON v.Id_pedido = p.Id_pedido
GROUP BY p.Estado_pedido;
GO

-- Cabeceras sin lineas: por estado y por antiguedad (evidencia del guardado no transaccional).
SELECT '20 pedidos sin lineas' AS q, p.Estado_pedido,
       CASE WHEN p.Fecha_pedido >= '2026-01-01' THEN '2026' ELSE 'antes de 2026' END AS periodo,
       COUNT(*) AS n
FROM Pedidos p
WHERE NOT EXISTS (SELECT 1 FROM Detalle_pedido d WHERE d.Id_pedido = p.Id_pedido)
GROUP BY p.Estado_pedido, CASE WHEN p.Fecha_pedido >= '2026-01-01' THEN '2026' ELSE 'antes de 2026' END;
GO

-- Empleados inactivos que siguen tomando pedidos: cuando fue el ultimo y cuantos en total.
SELECT '21 inactivos con pedidos' AS q, e.Cargo_empleado, COUNT(*) AS pedidos, MAX(p.Fecha_pedido) AS ultimo
FROM Pedidos p JOIN Empleados e ON e.Id_empleado = p.Id_empleado
WHERE e.Estado <> 'ACTIVO' AND p.Fecha_pedido >= '2026-09-24'
GROUP BY e.Cargo_empleado;
GO

SELECT '22 collation' AS q, CAST(SERVERPROPERTY('Collation') AS varchar(60)) AS servidor,
       CAST(DATABASEPROPERTYEX(DB_NAME(), 'Collation') AS varchar(60)) AS bd;
GO

-- Lineas repetidas: ejemplo del patron (mismo producto, varias filas en un pedido).
SELECT TOP 3 '23 ejemplo lineas repetidas' AS q, Id_pedido, Id_tipo, Tipo, COUNT(*) AS filas, SUM(Cantidad) AS cantidad_total
FROM Detalle_pedido GROUP BY Id_pedido, Id_tipo, Tipo HAVING COUNT(*) > 1 ORDER BY Id_pedido DESC;
GO
