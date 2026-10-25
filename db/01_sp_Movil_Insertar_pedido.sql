-- 01_sp_Movil_Insertar_pedido.sql
-- Alta atomica de un pedido de MESA desde la API movil. Objeto NUEVO y aditivo: no modifica nada existente.
-- Reversion: 01_sp_Movil_Insertar_pedido.revert.sql
--
-- Contrato con la capa C#:
--   * @Items es un JSON array. CADA elemento es una fila de Detalle_pedido, en el orden recibido (como el escritorio,
--     que crea una fila por cada plato que se agrega): puede haber varias filas del mismo producto, cada una con sus
--     propios ingredientes. [{ "tipo":"PLATO", "id_tipo":101, "cantidad":2, "observaciones":"",
--                              "ingredientes":[{ "id_ingrediente":5, "observaciones":"" }] }, ...]
--   * El precio SIEMPRE sale del catalogo (Platos.Precio_plato / Bebidas.Precio_bebida); no se acepta del cliente.
--   * Todo o nada: si algun item no existe, esta inactivo o no tiene precio, no se escribe nada; y si falla cualquier
--     INSERT, se deshace la cabecera, las lineas y los ingredientes.
--
-- Errores propios (THROW):
--   50001 pedido sin items          50002 items invalidos (cantidad, tipo, inexistente, inactivo, sin precio)
--   50004 @Items no es un JSON array
--
-- Lo que este SP NO valida (va en la API): mesa existente y libre, empleado, cliente, idempotencia,
-- Tipo_pedido = 'MESA', y las reglas de ingredientes (uno de cada tipo en los platos detallados).

CREATE OR ALTER PROCEDURE dbo.sp_Movil_Insertar_pedido
    @Id_mesa            int,
    @Id_empleado        int,
    @Id_cliente         int,
    @Tipo_pedido        varchar(50),
    @Observaciones      varchar(200),
    @CantidadClientes   int,
    @Items              nvarchar(max),
    @Id_pedido          int OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        IF @Items IS NULL OR ISJSON(@Items) <> 1 OR LEFT(LTRIM(@Items), 1) <> '['
            THROW 50004, 'El parametro @Items no es un JSON array valido.', 1;

        -- Items en orden, con el precio del catalogo y su estado.
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
            THROW 50001, 'El pedido no tiene items.', 1;
        IF EXISTS (SELECT 1 FROM @it WHERE tipo IS NULL OR tipo NOT IN ('PLATO', 'BEBIDA') OR id_tipo IS NULL
                                           OR cantidad IS NULL OR cantidad <= 0 OR activo = 0 OR precio IS NULL)
            THROW 50002, 'Hay items con cantidad invalida, inexistentes, inactivos o sin precio.', 1;

        BEGIN TRANSACTION;

        INSERT INTO dbo.Pedidos
            (Id_mesa, Id_empleado, Id_cliente, Estado_pedido, Fecha_pedido,
             Hora_pedido, Tipo_pedido, Observaciones_pedido, CantidadClientes)
        VALUES
            (@Id_mesa, @Id_empleado, @Id_cliente, 'PENDIENTE', CAST(GETDATE() AS date),
             CAST(GETDATE() AS time(2)), @Tipo_pedido, @Observaciones, @CantidadClientes);

        SET @Id_pedido = SCOPE_IDENTITY();

        -- Fila a fila: cada linea recibe SU Id_detalle_pedido y cuelga de el sus ingredientes.
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
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SET @Id_pedido = NULL;
        THROW;
    END CATCH
END;
GO
