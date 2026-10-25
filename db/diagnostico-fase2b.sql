-- Mesa duplicada (Num_mesa 21) y cantidad de clientes en los pedidos reales. Solo lectura.

SELECT '01 pedidos por Id_mesa de las mesas con Num_mesa repetido' AS q, m.Id_mesa, m.Num_mesa,
       COUNT(p.Id_pedido) AS pedidos, MAX(p.Fecha_pedido) AS ultimo
FROM Mesas m LEFT JOIN Pedidos p ON p.Id_mesa = m.Id_mesa
WHERE m.Num_mesa IN (SELECT Num_mesa FROM Mesas GROUP BY Num_mesa HAVING COUNT(*) > 1)
GROUP BY m.Id_mesa, m.Num_mesa ORDER BY m.Id_mesa;
GO

SELECT '02 CantidadClientes (ultimos 90 dias)' AS q,
       SUM(CASE WHEN CantidadClientes IS NULL THEN 1 ELSE 0 END) AS nulos,
       SUM(CASE WHEN CantidadClientes = 0 THEN 1 ELSE 0 END) AS ceros,
       SUM(CASE WHEN CantidadClientes > 0 THEN 1 ELSE 0 END) AS positivos,
       COUNT(*) AS total
FROM Pedidos WHERE Fecha_pedido >= '2026-07-26';
GO

SELECT '03 pedidos abiertos hoy por Num_mesa (duplicados)' AS q, m.Num_mesa, COUNT(*) AS abiertos
FROM Pedidos p JOIN Mesas m ON m.Id_mesa = p.Id_mesa
WHERE p.Fecha_pedido = '2026-10-24' AND p.Estado_pedido IN ('PENDIENTE', 'SALIENDO')
GROUP BY m.Num_mesa HAVING COUNT(*) > 1;
GO
