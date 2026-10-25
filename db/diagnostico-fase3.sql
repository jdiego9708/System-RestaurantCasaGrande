-- Como guarda el escritorio los ingredientes de los platos detallados. Solo lectura.
-- Define la regla "un ingrediente de cada tipo" de la fase 3.

-- Ingredientes guardados por linea de plato detallado: cuantos y de que tipos, separando carta / no carta.
WITH lin AS (
    SELECT d.Id_detalle_pedido, pl.Plato_carta,
           COUNT(i.Id_detalle_ingrediente_pedido) AS n,
           COUNT(DISTINCT ing.Tipo_ingrediente) AS tipos_distintos,
           SUM(CASE WHEN ing.Tipo_ingrediente = 'SOPA' THEN 1 ELSE 0 END) AS sopas,
           SUM(CASE WHEN ing.Tipo_ingrediente = 'ARROZ' THEN 1 ELSE 0 END) AS arroces,
           SUM(CASE WHEN ing.Tipo_ingrediente = 'ACOMPANANTE' THEN 1 ELSE 0 END) AS acomp,
           SUM(CASE WHEN ing.Tipo_ingrediente = 'BEBIDA' THEN 1 ELSE 0 END) AS bebidas
    FROM Detalle_pedido d
    JOIN Pedidos p ON p.Id_pedido = d.Id_pedido AND p.Fecha_pedido >= '2026-07-01'
    JOIN Platos pl ON pl.Id_plato = d.Id_tipo AND d.Tipo = 'PLATO' AND pl.Plato_detallado = 'ACTIVO'
    LEFT JOIN Detalle_ingredientes_pedido i ON i.Id_detalle_pedido = d.Id_detalle_pedido
    LEFT JOIN Ingredientes ing ON ing.Id_ingrediente = i.Id_ingrediente
    GROUP BY d.Id_detalle_pedido, pl.Plato_carta
)
SELECT '01 patron por linea (desde 2026-07-01)' AS q, Plato_carta AS carta,
       CONCAT('sopa=', sopas, ' arroz=', arroces, ' acomp=', acomp, ' bebida=', bebidas) AS patron,
       COUNT(*) AS lineas
FROM lin GROUP BY Plato_carta, sopas, arroces, acomp, bebidas ORDER BY Plato_carta, lineas DESC;
GO

-- Para los platos de carta, que sopa guardan.
SELECT '02 sopa en platos de carta' AS q, ing.Nombre_ingrediente, COUNT(*) AS veces
FROM Detalle_pedido d
JOIN Pedidos p ON p.Id_pedido = d.Id_pedido AND p.Fecha_pedido >= '2026-07-01'
JOIN Platos pl ON pl.Id_plato = d.Id_tipo AND d.Tipo = 'PLATO' AND pl.Plato_detallado = 'ACTIVO' AND pl.Plato_carta = 'ACTIVO'
JOIN Detalle_ingredientes_pedido i ON i.Id_detalle_pedido = d.Id_detalle_pedido
JOIN Ingredientes ing ON ing.Id_ingrediente = i.Id_ingrediente AND ing.Tipo_ingrediente = 'SOPA'
GROUP BY ing.Nombre_ingrediente ORDER BY veces DESC;
GO

-- Lineas de platos detallados SIN ningun ingrediente (por antiguedad).
SELECT '03 lineas detalladas sin ingredientes' AS q, YEAR(p.Fecha_pedido) AS anio, COUNT(*) AS lineas
FROM Detalle_pedido d
JOIN Pedidos p ON p.Id_pedido = d.Id_pedido
JOIN Platos pl ON pl.Id_plato = d.Id_tipo AND d.Tipo = 'PLATO' AND pl.Plato_detallado = 'ACTIVO'
WHERE NOT EXISTS (SELECT 1 FROM Detalle_ingredientes_pedido i WHERE i.Id_detalle_pedido = d.Id_detalle_pedido)
GROUP BY YEAR(p.Fecha_pedido) ORDER BY anio;
GO

-- Cantidad por linea de plato detallado: cada fila es una unidad?
SELECT '04 cantidad por linea detallada' AS q, d.Cantidad, COUNT(*) AS lineas
FROM Detalle_pedido d
JOIN Pedidos p ON p.Id_pedido = d.Id_pedido AND p.Fecha_pedido >= '2026-07-01'
JOIN Platos pl ON pl.Id_plato = d.Id_tipo AND d.Tipo = 'PLATO' AND pl.Plato_detallado = 'ACTIVO'
GROUP BY d.Cantidad ORDER BY lineas DESC;
GO
