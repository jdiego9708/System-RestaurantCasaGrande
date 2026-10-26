-- 04_sp_Movil_Quitar_item.sql
-- Quita unidades de UNA linea de un pedido (por Id_detalle_pedido) desde la API movil. Objeto NUEVO y aditivo.
-- Reversion: 04_sp_Movil_Quitar_item.revert.sql
--
-- Por que un SP y no sp_Actualizar_detalle_pedido: ese SP recibe el precio y las observaciones de la linea y
-- las reescribe, y con @Cantidad = 0 borra la fila pero deja huerfanos los ingredientes. Aqui se conserva el
-- precio guardado y, si la linea desaparece, se borran tambien sus ingredientes en la misma transaccion.
--
-- Contrato con la capa C#:
--   * @Cantidad es cuantas unidades quitar (>= 1). Si llega a la cantidad de la linea, la fila se elimina.
--   * @CantidadRestante devuelve lo que queda en la linea (0 si se elimino), para poder responder sin releer.
--
-- Errores propios (THROW):
--   50020 el pedido no existe
--   50021 el pedido no admite cambios (no esta PENDIENTE ni SALIENDO)
--   50022 la linea no existe o no pertenece al pedido
--   50023 cantidad invalida (menor que 1 o mayor que la cantidad de la linea)
--
-- Lo que este SP NO valida (va en la API): la clave maestra de autorizacion y el formato de los parametros.

CREATE OR ALTER PROCEDURE dbo.sp_Movil_Quitar_item
    @Id_pedido              int,
    @Id_detalle_pedido      int,
    @Cantidad               int,
    @CantidadRestante       int OUTPUT
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

        DECLARE @actual int =
            (SELECT Cantidad FROM dbo.Detalle_pedido
             WHERE Id_detalle_pedido = @Id_detalle_pedido AND Id_pedido = @Id_pedido);
        IF @actual IS NULL
            THROW 50022, 'La linea no existe o no pertenece al pedido.', 1;

        IF @Cantidad IS NULL OR @Cantidad < 1 OR @Cantidad > @actual
            THROW 50023, 'La cantidad a quitar debe estar entre 1 y la cantidad de la linea.', 1;

        BEGIN TRANSACTION;

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
