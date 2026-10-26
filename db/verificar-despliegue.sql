-- verificar-despliegue.sql - SOLO LECTURA. Comprueba que la base del restaurante esta lista para la API movil.
-- Uso (en el PC del restaurante, con el mismo usuario de Windows que abre la app):
--   .\Invoke-Sql.ps1 -File .\verificar-despliegue.sql -Database CasaGrandeBD
-- No cambia nada; se puede ejecutar cuantas veces haga falta.

SELECT DB_NAME() AS base_de_datos;
GO

-- 1. Los tres procedimientos nuevos (db\01, 03 y 04). Los tres deben decir OK.
SELECT v.nombre AS procedimiento,
       CASE WHEN OBJECT_ID(N'dbo.' + v.nombre, N'P') IS NOT NULL THEN 'OK' ELSE 'FALTA' END AS estado
FROM (VALUES (N'sp_Movil_Insertar_pedido'), (N'sp_Movil_Agregar_items'), (N'sp_Movil_Quitar_item')) AS v(nombre);
GO

-- 2. Empleados (db\02): pines_compartidos_entre_activos y activos_con_pin_cero deben ser 0.
--    activos_sin_pin > 0 solo significa que esa persona no podra entrar por PIN al celular.
SELECT
    (SELECT COUNT(*) FROM dbo.Empleados WHERE Estado = N'ACTIVO') AS empleados_activos,
    (SELECT COUNT(*) FROM (SELECT Codigo_maestro FROM dbo.Empleados
                           WHERE Estado = N'ACTIVO' AND Codigo_maestro IS NOT NULL
                           GROUP BY Codigo_maestro HAVING COUNT(*) > 1) AS d) AS pines_compartidos_entre_activos,
    (SELECT COUNT(*) FROM dbo.Empleados WHERE Estado = N'ACTIVO' AND Codigo_maestro = 0) AS activos_con_pin_cero,
    (SELECT COUNT(*) FROM dbo.Empleados WHERE Estado = N'ACTIVO' AND Codigo_maestro IS NULL) AS activos_sin_pin;
GO

-- 3. Datos basicos para tomar pedidos.
SELECT
    (SELECT COUNT(*) FROM dbo.Mesas) AS mesas,
    (SELECT COUNT(*) FROM dbo.Pedidos) AS pedidos_historicos;
GO
