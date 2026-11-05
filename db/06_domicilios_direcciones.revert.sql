-- 06_domicilios_direcciones.revert.sql
-- Deshace 06: restaura sp_Insertar_pedido original, quita Pedidos.Id_direccion y borra Domicilios
-- con sus SPs. Se pierden las direcciones creadas desde el escritorio y la direccion de entrega
-- de los pedidos ya hechos; Clientes.Direccion_cliente no se toca.
SET NOCOUNT ON;
GO

-- sp_Insertar_pedido tal como estaba en produccion (sin @Id_direccion).
CREATE OR ALTER PROC dbo.sp_Insertar_pedido
@Id_pedido int output,
@Id_mesa int,
@Id_empleado int,
@Id_cliente int,
@Fecha_pedido date,
@Hora_pedido time(2),
@Tipo_pedido varchar(50),
@Observaciones_pedido varchar(200),
@CantidadClientes int
AS
BEGIN TRY
BEGIN TRANSACTION InsertarPedido
BEGIN
DECLARE @Estado_pedido varchar(50)= 'PENDIENTE'
INSERT INTO Pedidos
(Id_mesa, Id_empleado, Id_cliente, Estado_pedido, Fecha_pedido,
 Hora_pedido, Tipo_pedido, Observaciones_pedido, CantidadClientes)
VALUES (@Id_mesa, @Id_empleado, @Id_cliente, @Estado_pedido ,@Fecha_pedido,
@Hora_pedido, @Tipo_pedido, @Observaciones_pedido, @CantidadClientes)
SET @Id_pedido = SCOPE_IDENTITY();

SELECT 'OK' as Respuesta, @Id_pedido as Id_pedido

END
COMMIT
RETURN 1
END TRY
BEGIN CATCH
IF @@TRANCOUNT > 0
DECLARE @Mensaje_error NVARCHAR(4000) = ERROR_MESSAGE();
DECLARE @Mensaje_severidad INT = ERROR_SEVERITY();
DECLARE @Estado_error INT = ERROR_STATE();
DECLARE @Numero_error INT = ERROR_NUMBER();
DECLARE @Error_procedure varchar(500) = ERROR_PROCEDURE();
DECLARE @Error_line INT = Error_line();
RAISERROR (@Mensaje_error,
           @Mensaje_severidad,
           @Estado_error,
		   @Numero_error,
		   @Error_procedure,
		   @Error_line
           );
ROLLBACK
RETURN -1
END CATCH
GO

IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Pedidos_Domicilios')
    ALTER TABLE dbo.Pedidos DROP CONSTRAINT FK_Pedidos_Domicilios;
GO
IF COL_LENGTH('dbo.Pedidos', 'Id_direccion') IS NOT NULL
    ALTER TABLE dbo.Pedidos DROP COLUMN Id_direccion;
GO

DROP PROCEDURE IF EXISTS dbo.sp_Cliente_editar_basico;
DROP PROCEDURE IF EXISTS dbo.sp_Cliente_obtener;
DROP PROCEDURE IF EXISTS dbo.sp_Clientes_buscar_rapido;
DROP PROCEDURE IF EXISTS dbo.sp_Domicilio_inactivar;
DROP PROCEDURE IF EXISTS dbo.sp_Domicilio_guardar;
DROP PROCEDURE IF EXISTS dbo.sp_Domicilio_obtener;
DROP PROCEDURE IF EXISTS dbo.sp_Domicilios_listar;
GO
DROP TABLE IF EXISTS dbo.Domicilios;
GO
