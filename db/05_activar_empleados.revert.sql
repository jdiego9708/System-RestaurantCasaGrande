-- Revierte 05_activar_empleados.sql: deja a JUAN DIEGO DUQUE y ANDRES DAVID HOYOS GARCIA como INACTIVO,
-- que era el estado con el que estaban antes (se activaron para que sigan apareciendo en el login).

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

BEGIN TRAN;

UPDATE Empleados
SET Estado = 'INACTIVO'
WHERE Nombre_empleado IN ('JUAN DIEGO DUQUE', 'ANDRES DAVID HOYOS GARCIA')
  AND ISNULL(Estado, '') <> 'INACTIVO';

PRINT 'Empleados desactivados: ' + CAST(@@ROWCOUNT AS varchar(10));

COMMIT;
GO

SELECT Id_empleado, Nombre_empleado, Cargo_empleado, Estado
FROM Empleados
WHERE Nombre_empleado IN ('JUAN DIEGO DUQUE', 'ANDRES DAVID HOYOS GARCIA')
ORDER BY Nombre_empleado;
GO
