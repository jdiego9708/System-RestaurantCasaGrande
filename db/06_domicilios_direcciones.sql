-- 06 - Domicilios: varias direcciones por cliente y direccion de entrega en cada pedido.
--
-- Motivo (2026-11-02): el modulo de Domicilios del escritorio se retoma. Un cliente puede tener
-- una o mas direcciones (hoy Clientes.Direccion_cliente guarda solo una) y cada pedido de tipo
-- DOMICILIO debe saber a cual de ellas se entrega.
--
-- Que hace (todo idempotente: repetirlo no duplica nada):
--   1. Crea la tabla Domicilios (Id_direccion, Id_cliente -> Clientes, alias, direccion, barrio,
--      referencia, telefono de contacto, Es_principal, Estado).
--   2. Migra la Direccion_cliente existente de cada cliente como su direccion principal.
--   3. Agrega Pedidos.Id_direccion (NULL = pedido de mesa) con FK a Domicilios.
--   4. sp_Insertar_pedido pasa a aceptar @Id_direccion (opcional, por defecto NULL) y a nombrar las
--      columnas del INSERT; asi el flujo de mesas y el del movil siguen funcionando sin cambios.
--      sp_Movil_Insertar_pedido ya nombraba sus columnas y no se toca.
--   5. SPs nuevos: sp_Domicilios_listar, sp_Domicilio_obtener, sp_Domicilio_guardar,
--      sp_Domicilio_inactivar, sp_Clientes_buscar_rapido, sp_Cliente_obtener y
--      sp_Cliente_editar_basico.
--
-- Reversion: 06_domicilios_direcciones.revert.sql (borra Domicilios y Pedidos.Id_direccion; las
-- direcciones nuevas se pierden, Clientes.Direccion_cliente no se toca).
--
-- Uso (primero contra la copia local):
--   cd db
--   .\Invoke-Sql.ps1 -File .\06_domicilios_direcciones.sql

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

-- La aplicacion usa Id_mesa = 0 para domicilios; tambien debe existir en una instalacion nueva.
IF EXISTS (SELECT 1 FROM dbo.Mesas WHERE Id_mesa = 0 AND Num_mesa <> 0)
    THROW 50004, 'La mesa con Id 0 esta ocupada por una mesa real. Revise antes de continuar.', 1;
IF NOT EXISTS (SELECT 1 FROM dbo.Mesas WHERE Id_mesa = 0)
BEGIN
    IF COLUMNPROPERTY(OBJECT_ID('dbo.Mesas'), 'Id_mesa', 'IsIdentity') = 1
        SET IDENTITY_INSERT dbo.Mesas ON;
    INSERT INTO dbo.Mesas (Id_mesa, Num_mesa, Descripcion_mesa) VALUES (0, 0, 'DOMICILIOS');
    IF COLUMNPROPERTY(OBJECT_ID('dbo.Mesas'), 'Id_mesa', 'IsIdentity') = 1
        SET IDENTITY_INSERT dbo.Mesas OFF;
END
GO

-- 1. Tabla Domicilios --------------------------------------------------------------------------
IF OBJECT_ID('dbo.Domicilios', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Domicilios (
        Id_direccion     int IDENTITY(1,1) NOT NULL,
        Id_cliente       int NOT NULL,
        Alias            varchar(50)  NULL,
        Direccion        varchar(200) NOT NULL,
        Barrio           varchar(100) NULL,
        Referencia       varchar(200) NULL,
        Telefono_contacto varchar(50) NULL,
        Es_principal     bit NOT NULL CONSTRAINT DF_Domicilios_Es_principal DEFAULT (0),
        Estado           varchar(20) NOT NULL CONSTRAINT DF_Domicilios_Estado DEFAULT ('ACTIVO'),
        CONSTRAINT PK_Domicilios PRIMARY KEY CLUSTERED (Id_direccion),
        CONSTRAINT FK_Domicilios_Clientes FOREIGN KEY (Id_cliente) REFERENCES dbo.Clientes (Id_cliente)
    );
    CREATE INDEX IX_Domicilios_Id_cliente ON dbo.Domicilios (Id_cliente);
    PRINT 'Tabla Domicilios creada.';
END
GO

-- 2. Migracion de la direccion unica que ya tenian los clientes --------------------------------
INSERT INTO dbo.Domicilios (Id_cliente, Alias, Direccion, Referencia, Es_principal, Estado)
SELECT c.Id_cliente, 'PRINCIPAL', LTRIM(RTRIM(c.Direccion_cliente)),
       NULLIF(LTRIM(RTRIM(c.Referencia_ubicacion)), ''), 1, 'ACTIVO'
FROM dbo.Clientes c
WHERE LTRIM(RTRIM(ISNULL(c.Direccion_cliente, ''))) <> ''
  AND NOT EXISTS (SELECT 1 FROM dbo.Domicilios d WHERE d.Id_cliente = c.Id_cliente);
PRINT 'Direcciones migradas desde Clientes: ' + CAST(@@ROWCOUNT AS varchar(10));
GO

-- 3. Pedidos.Id_direccion ----------------------------------------------------------------------
IF COL_LENGTH('dbo.Pedidos', 'Id_direccion') IS NULL
BEGIN
    ALTER TABLE dbo.Pedidos ADD Id_direccion int NULL;
    PRINT 'Columna Pedidos.Id_direccion creada.';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Pedidos_Domicilios')
    ALTER TABLE dbo.Pedidos WITH CHECK
        ADD CONSTRAINT FK_Pedidos_Domicilios FOREIGN KEY (Id_direccion) REFERENCES dbo.Domicilios (Id_direccion);
GO

-- 4. sp_Insertar_pedido con @Id_direccion opcional ---------------------------------------------
-- Mismo cuerpo que el original; solo cambia la lista explicita de columnas y el parametro nuevo.
CREATE OR ALTER PROC dbo.sp_Insertar_pedido
@Id_pedido int output,
@Id_mesa int,
@Id_empleado int,
@Id_cliente int,
@Fecha_pedido date,
@Hora_pedido time(2),
@Tipo_pedido varchar(50),
@Observaciones_pedido varchar(200),
@CantidadClientes int,
@Id_direccion int = NULL
AS
BEGIN TRY
BEGIN TRANSACTION InsertarPedido
BEGIN
DECLARE @Estado_pedido varchar(50)= 'PENDIENTE'
INSERT INTO Pedidos
(Id_mesa, Id_empleado, Id_cliente, Estado_pedido, Fecha_pedido,
 Hora_pedido, Tipo_pedido, Observaciones_pedido, CantidadClientes, Id_direccion)
VALUES (@Id_mesa, @Id_empleado, @Id_cliente, @Estado_pedido, @Fecha_pedido,
@Hora_pedido, @Tipo_pedido, @Observaciones_pedido, @CantidadClientes, @Id_direccion)
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

-- 5. SPs de direcciones -------------------------------------------------------------------------

-- Direcciones ACTIVAS de un cliente, la principal primero.
CREATE OR ALTER PROC dbo.sp_Domicilios_listar
@Id_cliente int
AS
SET NOCOUNT ON;
SELECT Id_direccion, Id_cliente, Alias, Direccion, Barrio, Referencia,
       Telefono_contacto, Es_principal, Estado
FROM dbo.Domicilios
WHERE Id_cliente = @Id_cliente AND Estado = 'ACTIVO'
ORDER BY Es_principal DESC, Id_direccion;
GO

-- Una direccion por Id (sin filtrar Estado: un pedido viejo puede apuntar a una inactiva).
CREATE OR ALTER PROC dbo.sp_Domicilio_obtener
@Id_direccion int
AS
SET NOCOUNT ON;
SELECT Id_direccion, Id_cliente, Alias, Direccion, Barrio, Referencia,
       Telefono_contacto, Es_principal, Estado
FROM dbo.Domicilios
WHERE Id_direccion = @Id_direccion;
GO

-- Crea (@Id_direccion = 0) o edita una direccion. Si queda como principal, las demas del cliente
-- dejan de serlo. La primera direccion de un cliente siempre es la principal.
CREATE OR ALTER PROC dbo.sp_Domicilio_guardar
@Id_direccion int output,
@Id_cliente int,
@Alias varchar(50),
@Direccion varchar(200),
@Barrio varchar(100),
@Referencia varchar(200),
@Telefono_contacto varchar(50),
@Es_principal bit
AS
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRY
    IF LTRIM(RTRIM(ISNULL(@Direccion, ''))) = ''
        THROW 50001, 'La direccion es obligatoria.', 1;

    BEGIN TRAN;

    IF NOT EXISTS (SELECT 1 FROM dbo.Domicilios WHERE Id_cliente = @Id_cliente AND Estado = 'ACTIVO'
                   AND Id_direccion <> ISNULL(@Id_direccion, 0))
        SET @Es_principal = 1;

    IF @Es_principal = 1
        UPDATE dbo.Domicilios SET Es_principal = 0
        WHERE Id_cliente = @Id_cliente AND Id_direccion <> ISNULL(@Id_direccion, 0);

    IF ISNULL(@Id_direccion, 0) = 0
    BEGIN
        INSERT INTO dbo.Domicilios (Id_cliente, Alias, Direccion, Barrio, Referencia, Telefono_contacto, Es_principal)
        VALUES (@Id_cliente, @Alias, @Direccion, @Barrio, @Referencia, @Telefono_contacto, @Es_principal);
        SET @Id_direccion = SCOPE_IDENTITY();
    END
    ELSE
        UPDATE dbo.Domicilios
        SET Alias = @Alias, Direccion = @Direccion, Barrio = @Barrio, Referencia = @Referencia,
            Telefono_contacto = @Telefono_contacto, Es_principal = @Es_principal
        WHERE Id_direccion = @Id_direccion AND Id_cliente = @Id_cliente;

    COMMIT;
    SELECT 'OK' AS Respuesta, @Id_direccion AS Id_direccion;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK;
    THROW;
END CATCH
GO

-- Inactiva (no borra: los pedidos viejos siguen apuntando a ella). Si era la principal, la
-- direccion activa mas antigua del cliente pasa a serlo.
CREATE OR ALTER PROC dbo.sp_Domicilio_inactivar
@Id_direccion int
AS
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRY
    DECLARE @Id_cliente int, @Era_principal bit;
    SELECT @Id_cliente = Id_cliente, @Era_principal = Es_principal
    FROM dbo.Domicilios WHERE Id_direccion = @Id_direccion;

    IF @Id_cliente IS NULL
        THROW 50002, 'La direccion no existe.', 1;

    BEGIN TRAN;
    UPDATE dbo.Domicilios SET Estado = 'INACTIVO', Es_principal = 0 WHERE Id_direccion = @Id_direccion;

    IF @Era_principal = 1
        UPDATE dbo.Domicilios SET Es_principal = 1
        WHERE Id_direccion = (SELECT TOP 1 Id_direccion FROM dbo.Domicilios
                              WHERE Id_cliente = @Id_cliente AND Estado = 'ACTIVO'
                              ORDER BY Id_direccion);
    COMMIT;
    SELECT 'OK' AS Respuesta;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK;
    THROW;
END CATCH
GO

-- Busqueda rapida de clientes para el alta de domicilios (nombre o telefono). Sin texto devuelve los 50
-- mas recientes. Existe porque sp_Buscar_clientes termina con un COMMIT sin BEGIN TRAN: devuelve las filas y
-- despues falla, y NClientes.BuscarClientes lo convierte en null. Aqui no hay transaccion que cerrar.
CREATE OR ALTER PROC dbo.sp_Clientes_buscar_rapido
@Texto varchar(50) = ''
AS
SET NOCOUNT ON;
SELECT TOP 50 Id_cliente, Nombre_cliente, Telefono_cliente, Correo_electronico, Direccion_cliente,
       Referencia_ubicacion, Otras_observaciones, Estado_cliente
FROM dbo.Clientes
WHERE Id_cliente <> 0
  AND ISNULL(Estado_cliente, 'ACTIVO') = 'ACTIVO'
  AND (@Texto = ''
       OR Nombre_cliente LIKE '%' + @Texto + '%'
       OR Telefono_cliente LIKE '%' + @Texto + '%')
ORDER BY CASE WHEN Telefono_cliente = @Texto THEN 0 ELSE 1 END, Id_cliente DESC;
GO

CREATE OR ALTER PROC dbo.sp_Cliente_obtener
@Id_cliente int
AS
SET NOCOUNT ON;
SELECT Id_cliente, Nombre_cliente, Telefono_cliente, Correo_electronico, Direccion_cliente,
       Referencia_ubicacion, Otras_observaciones, Estado_cliente
FROM dbo.Clientes
WHERE Id_cliente = @Id_cliente;
GO

-- Edita solo los datos basicos del cliente (nombre, telefono, correo). sp_Editar_cliente reescribe todos los
-- campos, incluidos los antiguos de direccion; este deja intacto lo demas.
CREATE OR ALTER PROC dbo.sp_Cliente_editar_basico
@Id_cliente int,
@Nombre_cliente varchar(500),
@Telefono_cliente varchar(50),
@Correo_electronico varchar(500) = NULL
AS
SET NOCOUNT ON;
IF LTRIM(RTRIM(ISNULL(@Nombre_cliente, ''))) = '' OR LTRIM(RTRIM(ISNULL(@Telefono_cliente, ''))) = ''
    THROW 50003, 'El nombre y el telefono del cliente son obligatorios.', 1;
UPDATE dbo.Clientes
SET Nombre_cliente = @Nombre_cliente, Telefono_cliente = @Telefono_cliente, Correo_electronico = @Correo_electronico
WHERE Id_cliente = @Id_cliente;
SELECT 'OK' AS Respuesta, @@ROWCOUNT AS Filas;
GO

-- Resultado: cuantas direcciones hay y cuantos clientes quedaron con principal.
SELECT COUNT(*) AS Direcciones,
       SUM(CASE WHEN Es_principal = 1 THEN 1 ELSE 0 END) AS Principales
FROM dbo.Domicilios;
GO
