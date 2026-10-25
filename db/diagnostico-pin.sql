-- Unicidad del PIN (Codigo_maestro) para decidir el login solo por PIN. Solo lectura. Sin mostrar PIN ni claves.

-- Cuantos empleados reales (Id > 0) tienen PIN, y como son de largos.
SELECT '01 empleados con PIN' AS q, Estado,
       COUNT(*) AS empleados,
       SUM(CASE WHEN Codigo_maestro IS NULL THEN 1 ELSE 0 END) AS sin_pin,
       SUM(CASE WHEN Codigo_maestro IS NOT NULL AND Codigo_maestro < 1000 THEN 1 ELSE 0 END) AS pin_menor_1000,
       SUM(CASE WHEN Codigo_maestro BETWEEN 1000 AND 9999 THEN 1 ELSE 0 END) AS pin_4_digitos,
       SUM(CASE WHEN Codigo_maestro > 9999 THEN 1 ELSE 0 END) AS pin_mas_4_digitos
FROM Empleados WHERE Id_empleado > 0 GROUP BY Estado;
GO

-- PIN compartidos entre TODOS los empleados (cualquier estado): cuantos PIN y cuantas personas afectan.
SELECT '02 PIN repetidos (todos los estados)' AS q, COUNT(*) AS pines_repetidos, ISNULL(SUM(n), 0) AS empleados_afectados
FROM (SELECT Codigo_maestro, COUNT(*) AS n FROM Empleados
      WHERE Id_empleado > 0 AND Codigo_maestro IS NOT NULL
      GROUP BY Codigo_maestro HAVING COUNT(*) > 1) x;
GO

-- Detalle de los grupos repetidos: SIN el PIN, solo quienes comparten y su estado/cargo.
SELECT '03 grupos repetidos' AS q, DENSE_RANK() OVER (ORDER BY e.Codigo_maestro) AS grupo,
       e.Id_empleado, e.Nombre_empleado, e.Cargo_empleado, e.Estado
FROM Empleados e
WHERE e.Id_empleado > 0 AND e.Codigo_maestro IN (
    SELECT Codigo_maestro FROM Empleados WHERE Id_empleado > 0 AND Codigo_maestro IS NOT NULL
    GROUP BY Codigo_maestro HAVING COUNT(*) > 1)
ORDER BY grupo, e.Id_empleado;
GO

-- Solo entre ACTIVOS.
SELECT '04 PIN repetidos (solo activos)' AS q, COUNT(*) AS pines_repetidos, ISNULL(SUM(n), 0) AS empleados_afectados
FROM (SELECT Codigo_maestro, COUNT(*) AS n FROM Empleados
      WHERE Id_empleado > 0 AND Estado = 'ACTIVO' AND Codigo_maestro IS NOT NULL
      GROUP BY Codigo_maestro HAVING COUNT(*) > 1) x;
GO

-- Quienes tomaron pedidos en los ultimos 30 dias y si su PIN es unico entre todos.
WITH op AS (
    SELECT e.Id_empleado, e.Estado, e.Cargo_empleado, e.Codigo_maestro,
           (SELECT COUNT(*) FROM Empleados x WHERE x.Id_empleado > 0 AND x.Codigo_maestro = e.Codigo_maestro) AS comparten
    FROM Empleados e
    WHERE e.Id_empleado IN (SELECT DISTINCT Id_empleado FROM Pedidos WHERE Fecha_pedido >= '2026-09-24')
)
SELECT '05 quienes operan (30 dias) y unicidad de su PIN' AS q, Estado, Cargo_empleado,
       COUNT(*) AS empleados,
       SUM(CASE WHEN comparten = 1 THEN 1 ELSE 0 END) AS con_pin_unico,
       SUM(CASE WHEN Codigo_maestro IS NULL THEN 1 ELSE 0 END) AS sin_pin
FROM op GROUP BY Estado, Cargo_empleado;
GO

-- El comodin 0 (NINGUNO): tiene PIN? (no debe poder entrar nunca)
SELECT '06 empleado 0' AS q, Id_empleado, Nombre_empleado, Estado, CASE WHEN Codigo_maestro IS NULL THEN 'sin PIN' ELSE 'con PIN' END AS pin
FROM Empleados WHERE Id_empleado = 0;
GO
