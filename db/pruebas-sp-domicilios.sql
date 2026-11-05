-- Pruebas de 06_domicilios_direcciones.sql. Todo dentro de una transaccion que se deshace al final.
-- Uso: .\Invoke-Sql.ps1 -File .\pruebas-sp-domicilios.sql -Database CasaGrandeBD_prod
SET NOCOUNT ON;
BEGIN TRAN;

DECLARE @cli int = (SELECT TOP 1 Id_cliente FROM Clientes ORDER BY Id_cliente DESC);
DECLARE @emp int = (SELECT TOP 1 Id_empleado FROM Empleados WHERE Estado = 'ACTIVO');
DECLARE @d1 int = 0, @d2 int = 0, @p1 int, @p2 int;

-- Dos direcciones: la primera queda principal; la segunda pide serlo y le quita la marca a la primera.
EXEC sp_Domicilio_guardar @d1 OUTPUT, @cli, 'CASA', 'CRA 1 # 2-3', 'CENTRO', 'PORTON NEGRO', '3000000000', 0;
EXEC sp_Domicilio_guardar @d2 OUTPUT, @cli, 'OFICINA', 'CLL 4 # 5-6', NULL, NULL, NULL, 1;
SELECT 'listar (esperado: OFICINA principal primero, CASA no principal)' AS Prueba;
EXEC sp_Domicilios_listar @cli;

-- Pedido de domicilio con direccion, y pedido de mesa con la firma antigua (sin @Id_direccion).
EXEC sp_Insertar_pedido @p1 OUTPUT, 0, @emp, @cli, '2026-11-02', '12:00', 'DOMICILIO', '', 1, @d2;
EXEC sp_Insertar_pedido @p2 OUTPUT, 5050, @emp, @cli, '2026-11-02', '12:00', 'MESA', '', 1;
SELECT 'pedidos (esperado: domicilio con Id_direccion, mesa con NULL)' AS Prueba;
SELECT Id_pedido, Tipo_pedido, Id_direccion FROM Pedidos WHERE Id_pedido IN (@p1, @p2);

-- Inactivar la principal: la otra pasa a serlo.
EXEC sp_Domicilio_inactivar @d2;
SELECT 'tras inactivar OFICINA (esperado: CASA principal)' AS Prueba;
EXEC sp_Domicilios_listar @cli;
EXEC sp_Domicilio_obtener @d2;

ROLLBACK;
SELECT 'transaccion deshecha' AS Fin;
