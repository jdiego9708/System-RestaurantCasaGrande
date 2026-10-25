-- Datos de la copia de produccion que condicionan los endpoints de la fase 2 (catalogo, mesas, imagenes).
-- Solo lectura.

SELECT '01 mesas' AS q, Id_mesa, Num_mesa, Descripcion_mesa FROM Mesas ORDER BY Num_mesa;
GO

SELECT '02 tipo_platos' AS q, * FROM Tipo_platos ORDER BY Id_tipo_plato;
GO
SELECT '03 tipo_bebidas' AS q, * FROM Tipo_bebidas ORDER BY Id_tipo_bebida;
GO
SELECT '04 ingredientes' AS q, Id_ingrediente, Tipo_ingrediente, Nombre_ingrediente, Estado_ingrediente FROM Ingredientes ORDER BY Tipo_ingrediente, Id_ingrediente;
GO

-- Platos activos sin tipo valido (el JOIN del escritorio los perderia).
SELECT '05 platos activos sin tipo' AS q, COUNT(*) AS n
FROM Platos p WHERE p.Estado = 'ACTIVO' AND NOT EXISTS (SELECT 1 FROM Tipo_platos t WHERE t.Id_tipo_plato = p.Id_tipo_plato);
GO
SELECT '06 bebidas activas sin tipo' AS q, COUNT(*) AS n
FROM Bebidas b WHERE b.Estado = 'ACTIVO' AND NOT EXISTS (SELECT 1 FROM Tipo_bebidas t WHERE t.Id_tipo_bebida = b.Id_tipo_bebida);
GO

-- Forma de los nombres de imagen.
SELECT '07 imagen plato' AS q,
       SUM(CASE WHEN Imagen_plato IS NULL OR Imagen_plato = '' THEN 1 ELSE 0 END) AS vacias,
       SUM(CASE WHEN Imagen_plato = 'SIN IMAGEN' THEN 1 ELSE 0 END) AS sin_imagen,
       SUM(CASE WHEN Imagen_plato NOT IN ('', 'SIN IMAGEN') THEN 1 ELSE 0 END) AS con_nombre,
       COUNT(DISTINCT CASE WHEN Imagen_plato NOT IN ('', 'SIN IMAGEN') THEN Imagen_plato END) AS nombres_distintos
FROM Platos WHERE Estado = 'ACTIVO';
GO
SELECT '08 imagen bebida' AS q,
       SUM(CASE WHEN Imagen IS NULL OR Imagen = '' THEN 1 ELSE 0 END) AS vacias,
       SUM(CASE WHEN Imagen = 'SIN IMAGEN' THEN 1 ELSE 0 END) AS sin_imagen,
       SUM(CASE WHEN Imagen NOT IN ('', 'SIN IMAGEN') THEN 1 ELSE 0 END) AS con_nombre
FROM Bebidas WHERE Estado = 'ACTIVO';
GO
SELECT TOP 8 '09 ejemplos de nombres' AS q, Imagen_plato FROM Platos WHERE Estado = 'ACTIVO' AND Imagen_plato NOT IN ('', 'SIN IMAGEN') GROUP BY Imagen_plato;
GO
SELECT '10 extensiones' AS q, RIGHT(Imagen_plato, CHARINDEX('.', REVERSE(Imagen_plato))) AS ext, COUNT(*) AS n
FROM Platos WHERE Estado = 'ACTIVO' AND Imagen_plato LIKE '%.%' GROUP BY RIGHT(Imagen_plato, CHARINDEX('.', REVERSE(Imagen_plato)));
GO

-- Caracteres peligrosos en los nombres (path traversal, espacios, acentos).
SELECT '11 nombres con espacios/acentos/rutas' AS q, COUNT(*) AS n
FROM Platos WHERE Imagen_plato LIKE '%[ \/:]%' OR Imagen_plato LIKE '%[^ -~]%';
GO

-- Pedidos abiertos de hoy (la copia es del 2026-10-24): lo que el mapa de mesas debe mostrar.
SELECT '12 abiertos hoy' AS q, p.Id_pedido, m.Num_mesa, p.Estado_pedido, p.Hora_pedido, p.CantidadClientes,
       (SELECT SUM(Precio * Cantidad) FROM Detalle_pedido d WHERE d.Id_pedido = p.Id_pedido) AS total
FROM Pedidos p JOIN Mesas m ON m.Id_mesa = p.Id_mesa
WHERE p.Fecha_pedido = '2026-10-24' AND p.Estado_pedido IN ('PENDIENTE', 'SALIENDO')
ORDER BY m.Num_mesa, p.Id_pedido DESC;
GO

SELECT '13 clientes' AS q, Id_cliente, Nombre_cliente, Estado_cliente FROM Clientes ORDER BY Id_cliente;
GO
