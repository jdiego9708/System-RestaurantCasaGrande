-- Reversion de 02_unificar_empleados_duplicados.sql:
-- devuelve a Empleados el PIN y el Estado que tenia antes de la unificacion y borra el respaldo.
--
--   cd db
--   .\Invoke-Sql.ps1 -File .\02_unificar_empleados_duplicados.revert.sql
--
-- No revierte nada mas: el script original no borro filas ni movio historia.

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID('dbo.Empleados_respaldo_unificacion') IS NULL
BEGIN
    PRINT 'No existe dbo.Empleados_respaldo_unificacion: no hay nada que revertir.';
    RETURN;
END
GO

BEGIN TRY
    BEGIN TRANSACTION;

    UPDATE e
       SET e.Codigo_maestro = b.Codigo_maestro,
           e.Estado = b.Estado
      FROM dbo.Empleados e
      INNER JOIN dbo.Empleados_respaldo_unificacion b ON b.Id_empleado = e.Id_empleado;

    PRINT 'Fichas restauradas: ' + CAST(@@ROWCOUNT AS varchar(10));

    DROP TABLE dbo.Empleados_respaldo_unificacion;
    PRINT 'Respaldo eliminado: dbo.Empleados_respaldo_unificacion';

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
