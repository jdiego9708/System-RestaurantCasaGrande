-- 05 - Activar a JUAN DIEGO DUQUE y ANDRES DAVID HOYOS GARCIA.
--
-- Motivo (2026-11-02): el login del escritorio pasa a mostrar SOLO empleados con Estado = 'ACTIVO'
-- (FrmIniciarSesion). Estas dos personas (desarrollo y administracion) estaban INACTIVO y habrian
-- desaparecido de la lista, asi que no podrian entrar al escritorio.
--
-- Que hace: Estado = 'ACTIVO' para esos dos nombres. Busca por NOMBRE y no por Id_empleado porque
-- el Id cambia entre la copia local y el restaurante. Idempotente: repetirlo no cambia nada.
-- No modifica SPs, tablas ni columnas, ni toca el PIN (Codigo_maestro).
--
-- Reversion: 05_activar_empleados.revert.sql
--
-- Uso (primero contra la copia local):
--   cd db
--   .\Invoke-Sql.ps1 -File .\05_activar_empleados.sql

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

BEGIN TRAN;

UPDATE Empleados
SET Estado = 'ACTIVO'
WHERE Nombre_empleado IN ('JUAN DIEGO DUQUE', 'ANDRES DAVID HOYOS GARCIA')
  AND ISNULL(Estado, '') <> 'ACTIVO';

PRINT 'Empleados activados ahora: ' + CAST(@@ROWCOUNT AS varchar(10));

COMMIT;
GO

-- Resultado: ambos deben aparecer como ACTIVO (si falta alguno, ese nombre no existe en esta base).
SELECT Id_empleado, Nombre_empleado, Cargo_empleado, Estado
FROM Empleados
WHERE Nombre_empleado IN ('JUAN DIEGO DUQUE', 'ANDRES DAVID HOYOS GARCIA')
ORDER BY Nombre_empleado;
GO
