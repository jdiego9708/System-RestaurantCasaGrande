-- verificar-despliegue.sql - SOLO LECTURA. Comprueba API movil, Domicilios e historial de ventas.
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

-- Domicilios: dependencias del pedido, las direcciones y la nueva ficha de venta.
SELECT v.nombre AS procedimiento_domicilios,
       CASE WHEN OBJECT_ID(N'dbo.' + v.nombre, N'P') IS NOT NULL THEN 'OK' ELSE 'FALTA' END AS estado
FROM (VALUES (N'sp_Insertar_pedido'), (N'sp_Domicilios_listar'), (N'sp_Domicilio_obtener'),
             (N'sp_Domicilio_guardar'), (N'sp_Domicilio_inactivar'), (N'sp_Clientes_buscar_rapido'),
             (N'sp_Cliente_obtener'), (N'sp_Cliente_editar_basico')) AS v(nombre);

SELECT 'Domicilios' AS requisito,
       CASE WHEN OBJECT_ID('dbo.Domicilios', 'U') IS NOT NULL THEN 'OK' ELSE 'FALTA' END AS estado
UNION ALL SELECT 'Pedidos.Id_direccion', CASE WHEN COL_LENGTH('dbo.Pedidos','Id_direccion') IS NOT NULL THEN 'OK' ELSE 'FALTA' END
UNION ALL SELECT 'sp_Insertar_pedido.@Id_direccion', CASE WHEN EXISTS
    (SELECT 1 FROM sys.parameters WHERE object_id=OBJECT_ID('dbo.sp_Insertar_pedido') AND name='@Id_direccion') THEN 'OK' ELSE 'FALTA' END
UNION ALL SELECT 'FK_Pedidos_Domicilios', CASE WHEN EXISTS
    (SELECT 1 FROM sys.foreign_keys WHERE parent_object_id=OBJECT_ID('dbo.Pedidos') AND name='FK_Pedidos_Domicilios' AND is_disabled=0 AND is_not_trusted=0) THEN 'OK' ELSE 'FALTA' END
UNION ALL SELECT 'FK_Domicilios_Clientes', CASE WHEN EXISTS
    (SELECT 1 FROM sys.foreign_keys WHERE parent_object_id=OBJECT_ID('dbo.Domicilios') AND name='FK_Domicilios_Clientes' AND is_disabled=0 AND is_not_trusted=0) THEN 'OK' ELSE 'FALTA' END
UNION ALL SELECT 'Mesa comodin 0', CASE WHEN EXISTS
    (SELECT 1 FROM dbo.Mesas WHERE Id_mesa=0 AND Num_mesa=0) THEN 'OK' ELSE 'FALTA' END;
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

-- Requisitos de facturacion electronica (07). Solo lectura.
SELECT requisito, CASE WHEN OBJECT_ID('dbo.' + requisito, 'U') IS NOT NULL THEN 'OK' ELSE 'FALTA' END AS estado
FROM (VALUES ('AlegraCustomerLinks'), ('AlegraItemLinks'), ('AlegraInvoiceRequests'), ('AlegraSettings')) AS tablas(requisito);
SELECT requisito, CASE WHEN COL_LENGTH('dbo.AlegraInvoiceRequests', requisito) IS NOT NULL THEN 'OK' ELSE 'FALTA' END AS estado
FROM (VALUES ('RemoteRequestJson'), ('WorkToken'), ('NextAttemptAt')) AS columnas(requisito);
