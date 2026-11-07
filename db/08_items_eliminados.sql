-- 08_items_eliminados.sql
-- sp_Movil_Quitar_item deja rastro en dbo.Delete_pedidos (quien autorizo, cuando y por que).
-- Reversion: 08_items_eliminados.revert.sql
--
-- Motivo (2026-11-06): al quitar un producto desde el movil solo se pedia la clave maestra y no quedaba
-- rastro. El escritorio ya registra sus eliminaciones en Delete_pedidos (una fila por unidad); el movil
-- pasa a usar la misma tabla, con la observacion como Motivo, y asi el reporte diario las lista juntas.
--
-- Que hace (idempotente): reemplaza sp_Movil_Quitar_item. Los parametros nuevos son opcionales (por
-- defecto NULL, las llamadas anteriores siguen funcionando) y el registro se escribe en la MISMA
-- transaccion que el borrado. Se inserta una fila por unidad quitada, igual que el escritorio.
--
-- Lo que NO valida el SP (va en la API): que la observacion sea obligatoria y que la clave exista.

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Movil_Quitar_item
    @Id_pedido                  int,
    @Id_detalle_pedido          int,
    @Cantidad                   int,
    @CantidadRestante           int OUTPUT,
    @Observacion                varchar(500) = NULL,
    @Id_usuario_clave_maestra   int = NULL,
    @Id_usuario_sesion          int = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        SET @CantidadRestante = NULL;

        DECLARE @estado varchar(50) =
            (SELECT Estado_pedido FROM dbo.Pedidos WHERE Id_pedido = @Id_pedido);
        IF @estado IS NULL
            THROW 50020, 'El pedido no existe.', 1;
        IF @estado NOT IN ('PENDIENTE', 'SALIENDO')
            THROW 50021, 'El pedido no admite cambios en su estado actual.', 1;

        DECLARE @actual int, @tipo varchar(50), @idTipo int;
        SELECT @actual = Cantidad, @tipo = Tipo, @idTipo = Id_tipo
          FROM dbo.Detalle_pedido
         WHERE Id_detalle_pedido = @Id_detalle_pedido AND Id_pedido = @Id_pedido;
        IF @actual IS NULL
            THROW 50022, 'La linea no existe o no pertenece al pedido.', 1;

        IF @Cantidad IS NULL OR @Cantidad < 1 OR @Cantidad > @actual
            THROW 50023, 'La cantidad a quitar debe estar entre 1 y la cantidad de la linea.', 1;

        BEGIN TRANSACTION;

        -- Rastro de lo eliminado: una fila por unidad, como hace el escritorio (FrmRealizarPedido).
        IF @Id_usuario_clave_maestra IS NOT NULL AND @Id_usuario_sesion IS NOT NULL
            INSERT INTO dbo.Delete_pedidos
                (Id_pedido, Id_usuario_clave_maestra, Id_usuario_sesion, Id_tipo, Tipo, Fecha, Hora, Motivo)
            SELECT TOP (@Cantidad) @Id_pedido, @Id_usuario_clave_maestra, @Id_usuario_sesion, @idTipo, @tipo,
                   CONVERT(date, GETDATE()), CONVERT(time(2), GETDATE()), @Observacion
              FROM sys.all_objects;

        IF @Cantidad = @actual
        BEGIN
            DELETE FROM dbo.Detalle_ingredientes_pedido
             WHERE Id_detalle_pedido = @Id_detalle_pedido;

            DELETE FROM dbo.Detalle_pedido
             WHERE Id_detalle_pedido = @Id_detalle_pedido;

            SET @CantidadRestante = 0;
        END
        ELSE
        BEGIN
            UPDATE dbo.Detalle_pedido
               SET Cantidad = Cantidad - @Cantidad
             WHERE Id_detalle_pedido = @Id_detalle_pedido;

            SET @CantidadRestante = @actual - @Cantidad;
        END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SET @CantidadRestante = NULL;
        THROW;
    END CATCH
END;
GO
