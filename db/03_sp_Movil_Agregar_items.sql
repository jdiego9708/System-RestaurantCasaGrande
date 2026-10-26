-- 03_sp_Movil_Agregar_items.sql
-- Agrega lineas a un pedido existente desde la API movil. Objeto NUEVO y aditivo.
-- Reversion: 03_sp_Movil_Agregar_items.revert.sql
--
-- Por que un SP y no sp_Actualizar_detalle_pedido: ese SP tiene semantica de delta (si @Cantidad es menor
-- que la actual RESTA 1, si es mayor SUMA @Cantidad) y reescribe precio y observaciones de la linea. Para
-- "agregar una fila nueva" (como el "+" del escritorio) no sirve: aqui cada item crea su fila con su
-- Id_detalle_pedido, igual que sp_Movil_Insertar_pedido.
--
-- Contrato con la capa C#:
--   * @Items es un JSON array con la misma forma que el alta:
--     [{ "tipo":"PLATO", "id_tipo":101, "cantidad":2, "observaciones":"",
--        "ingredientes":[{ "id_ingrediente":5, "observaciones":"" }] }, ...]
--   * El precio SIEMPRE sale del catalogo; no se acepta del cliente.
--   * Todo o nada: si un item no existe, esta inactivo o no tiene precio, no se escribe nada.
--
-- Errores propios (THROW):
--   50004 @Items no es un JSON array
--   50010 el pedido no existe
--   50011 el pedido no admite cambios (no esta PENDIENTE ni SALIENDO)
--   50012 items invalidos (tipo, cantidad, inexistente, inactivo, sin precio)
--
-- Lo que este SP NO valida (va en la API): permisos, reglas de ingredientes (uno de cada tipo en los platos
-- detallados) y que los productos existan antes de llamar.

CREATE OR ALTER PROCEDURE dbo.sp_Movil_Agregar_items
    @Id_pedido  int,
    @Items      nvarchar(max),
    @Filas      int OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        SET @Filas = 0;

        IF @Items IS NULL OR ISJSON(@Items) <> 1 OR LEFT(LTRIM(@Items), 1) <> '['
            THROW 50004, 'El parametro @Items no es un JSON array valido.', 1;

        DECLARE @estado varchar(50) =
            (SELECT Estado_pedido FROM dbo.Pedidos WHERE Id_pedido = @Id_pedido);

        IF @estado IS NULL
            THROW 50010, 'El pedido no existe.', 1;
        IF @estado NOT IN ('PENDIENTE', 'SALIENDO')
            THROW 50011, 'El pedido no admite cambios en su estado actual.', 1;

        DECLARE @it TABLE (
            n int IDENTITY(1,1), tipo varchar(10), id_tipo int, cantidad int, observaciones varchar(100),
            precio decimal(19,2), activo bit, ingredientes nvarchar(max));

        INSERT INTO @it (tipo, id_tipo, cantidad, observaciones, precio, activo, ingredientes)
        SELECT j.tipo, j.id_tipo, j.cantidad, LEFT(ISNULL(j.observaciones, ''), 100),
               CASE j.tipo WHEN 'PLATO' THEN p.Precio_plato ELSE b.Precio_bebida END,
               CASE WHEN (j.tipo = 'PLATO'  AND p.Estado = 'ACTIVO')
                      OR (j.tipo = 'BEBIDA' AND b.Estado = 'ACTIVO') THEN 1 ELSE 0 END,
               j.ingredientes
        FROM OPENJSON(@Items) AS e
        CROSS APPLY OPENJSON(e.value)
             WITH (
                 tipo          varchar(10)   '$.tipo',
                 id_tipo       int           '$.id_tipo',
                 cantidad      int           '$.cantidad',
                 observaciones varchar(100)  '$.observaciones',
                 ingredientes  nvarchar(max) '$.ingredientes' AS JSON
             ) AS j
        LEFT JOIN dbo.Platos  p ON j.tipo = 'PLATO'  AND p.Id_plato  = j.id_tipo
        LEFT JOIN dbo.Bebidas b ON j.tipo = 'BEBIDA' AND b.Id_bebida = j.id_tipo
        ORDER BY CAST(e.[key] AS int);

        IF NOT EXISTS (SELECT 1 FROM @it)
            THROW 50012, 'No hay items para agregar.', 1;
        IF EXISTS (SELECT 1 FROM @it WHERE tipo IS NULL OR tipo NOT IN ('PLATO', 'BEBIDA') OR id_tipo IS NULL
                                           OR cantidad IS NULL OR cantidad <= 0 OR activo = 0 OR precio IS NULL)
            THROW 50012, 'Hay items con cantidad invalida, inexistentes, inactivos o sin precio.', 1;

        BEGIN TRANSACTION;

        DECLARE @n int = 1, @max int = (SELECT MAX(n) FROM @it), @d int;
        WHILE @n <= @max
        BEGIN
            INSERT INTO dbo.Detalle_pedido (Id_pedido, Id_tipo, Tipo, Precio, Cantidad, Observaciones)
            SELECT @Id_pedido, id_tipo, tipo, precio, cantidad, observaciones FROM @it WHERE n = @n;
            SET @d = SCOPE_IDENTITY();

            INSERT INTO dbo.Detalle_ingredientes_pedido
                (Id_pedido, Id_tipo, Id_ingrediente, Id_detalle_pedido, Observaciones)
            SELECT @Id_pedido, it.id_tipo, i.id_ingrediente, @d, LEFT(ISNULL(i.observaciones, ''), 200)
            FROM @it it
            CROSS APPLY OPENJSON(it.ingredientes)
                 WITH (
                     id_ingrediente int          '$.id_ingrediente',
                     observaciones  varchar(200) '$.observaciones'
                 ) AS i
            WHERE it.n = @n AND it.tipo = 'PLATO' AND it.ingredientes IS NOT NULL;

            SET @n += 1;
        END

        COMMIT TRANSACTION;

        SET @Filas = @max;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SET @Filas = 0;
        THROW;
    END CATCH
END;
GO
