-- 02 - Unificar fichas duplicadas de Empleados y dejar un PIN por persona.
--
-- Problema (2026-10-26, hallazgo en la prueba con el telefono real):
--   * Hay personas registradas dos o tres veces con el mismo Nombre_empleado y el mismo PIN.
--     Al entrar solo con PIN la API las ve como un PIN ambiguo (409) y el mesero ve dos fichas
--     identicas en el selector: no puede saber cual elegir.
--   * 9 fichas tenian Codigo_maestro = 0, que es "sin PIN" escrito como numero. Con el login
--     por PIN, un 0 identifica a varias personas a la vez.
--
-- Que hace, en una transaccion:
--   1. Respalda Empleados completa en dbo.Empleados_respaldo_unificacion (solo la primera vez).
--   2. Deja una sola ficha con PIN por nombre: la que tiene historia (pedidos, ingresos, egresos,
--      nomina, notas) o, si ninguna tiene, la de Id mas bajo. La historia de las fichas duplicadas
--      se reasigna a la superviviente; las fichas duplicadas quedan sin PIN e INACTIVO (no se
--      borran filas, para no perder la trazabilidad).
--   3. Convierte Codigo_maestro = 0 en NULL ("sin PIN" de verdad).
--   4. Informa lo que quedo: nombres con dos o mas fichas, PIN repetidos entre ACTIVO y PIN de
--      menos de 4 digitos. Si un nombre tiene dos fichas CON historia y datos distintos (cargo o
--      PIN), la unificacion NO lo toca: puede tratarse de dos personas homonimas.
--
-- No modifica SPs, tablas ni columnas: solo datos de Empleados y de las tablas que la referencian.
-- Reversion: 02_unificar_empleados_duplicados.revert.sql
--
-- Uso (siempre contra la copia local, nunca contra el servidor del restaurante):
--   cd db
--   .\Invoke-Sql.ps1 -File .\02_unificar_empleados_duplicados.sql

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

-- ---------------------------------------------------------------------------
-- 1. Respaldo
-- ---------------------------------------------------------------------------
IF OBJECT_ID('dbo.Empleados_respaldo_unificacion') IS NULL
BEGIN
    SELECT * INTO dbo.Empleados_respaldo_unificacion FROM dbo.Empleados;
    PRINT 'Respaldo creado: dbo.Empleados_respaldo_unificacion';
END
ELSE
    PRINT 'El respaldo dbo.Empleados_respaldo_unificacion ya existe; no se vuelve a crear.';
GO

-- ---------------------------------------------------------------------------
-- 2. Informe previo (sin PIN: solo cuantos y de que largo)
-- ---------------------------------------------------------------------------
SELECT 'ANTES 01 nombres con mas de una ficha' AS paso,
       COUNT(*) AS nombres,
       SUM(fichas) AS fichas
FROM (SELECT Nombre_empleado, COUNT(*) AS fichas
      FROM dbo.Empleados WHERE Id_empleado > 0
      GROUP BY Nombre_empleado HAVING COUNT(*) > 1) x;

SELECT 'ANTES 02 PIN compartido entre ACTIVO' AS paso, COUNT(*) AS pines
FROM (SELECT Codigo_maestro FROM dbo.Empleados
      WHERE Id_empleado > 0 AND Estado = 'ACTIVO' AND Codigo_maestro IS NOT NULL
      GROUP BY Codigo_maestro HAVING COUNT(*) > 1) x;

SELECT 'ANTES 03 PIN 0 o NULL' AS paso,
       SUM(CASE WHEN Codigo_maestro = 0 THEN 1 ELSE 0 END) AS pin_cero,
       SUM(CASE WHEN Codigo_maestro IS NULL THEN 1 ELSE 0 END) AS pin_nulo,
       SUM(CASE WHEN Estado = 'ACTIVO' THEN 1 ELSE 0 END) AS de_ellos_activos
FROM dbo.Empleados WHERE Id_empleado > 0;
GO

-- ---------------------------------------------------------------------------
-- 3. Unificacion
-- ---------------------------------------------------------------------------
BEGIN TRY
    BEGIN TRANSACTION;

    -- 3.1 A quien se unifica con quien.
    --     Superviviente: la ficha con historia (pedidos, ingresos, egresos, nomina, notas), luego
    --     la ACTIVA, luego la de mas movimientos y, en empate, la de Id mas bajo.
    --     Se unifica el grupo si es un duplicado exacto (mismo cargo y mismo PIN) o si solo una de
    --     sus fichas tiene historia. Si hay dos fichas con historia y datos distintos, NO se toca:
    --     puede tratarse de dos personas homonimas.
    CREATE TABLE #fusion (
        Id_empleado      INT NOT NULL PRIMARY KEY,
        Id_superviviente INT NOT NULL
    );

    ;WITH historia AS (
        SELECT e.Id_empleado,
               e.Nombre_empleado,
               e.Cargo_empleado,
               e.Estado,
               e.Codigo_maestro,
               (SELECT COUNT(*) FROM dbo.Pedidos         x WHERE x.Id_empleado = e.Id_empleado)
             + (SELECT COUNT(*) FROM dbo.Ingresos        x WHERE x.Id_empleado = e.Id_empleado)
             + (SELECT COUNT(*) FROM dbo.Egresos         x WHERE x.Id_empleado = e.Id_empleado)
             + (SELECT COUNT(*) FROM dbo.Nomina_empleado x WHERE x.Id_empleado = e.Id_empleado)
             + (SELECT COUNT(*) FROM dbo.Notas_empleado  x WHERE x.Id_empleado = e.Id_empleado) AS movimientos
        FROM dbo.Empleados e
        WHERE e.Id_empleado > 0
    ),
    grupo AS (
        SELECT Nombre_empleado,
               COUNT(*) AS fichas,
               SUM(CASE WHEN movimientos > 0 THEN 1 ELSE 0 END) AS fichas_con_movimientos,
               COUNT(DISTINCT Cargo_empleado) AS cargos,
               COUNT(DISTINCT Codigo_maestro) AS pines
        FROM historia
        GROUP BY Nombre_empleado
    ),
    ranking AS (
        SELECT h.Id_empleado,
               h.Nombre_empleado,
               g.fichas,
               g.fichas_con_movimientos,
               g.cargos,
               g.pines,
               FIRST_VALUE(h.Id_empleado) OVER (
                   PARTITION BY h.Nombre_empleado
                   ORDER BY CASE WHEN h.movimientos > 0 THEN 0 ELSE 1 END,
                            CASE WHEN h.Estado = 'ACTIVO' THEN 0 ELSE 1 END,
                            h.movimientos DESC,
                            h.Id_empleado
                   ROWS BETWEEN UNBOUNDED PRECEDING AND UNBOUNDED FOLLOWING) AS superviviente
        FROM historia h
        INNER JOIN grupo g ON g.Nombre_empleado = h.Nombre_empleado
        WHERE g.fichas > 1
    )
    INSERT INTO #fusion (Id_empleado, Id_superviviente)
    SELECT Id_empleado, superviviente
    FROM ranking
    WHERE Id_empleado <> superviviente
      AND ((cargos = 1 AND pines = 1)      -- duplicado exacto: mismo cargo y mismo PIN
           OR fichas_con_movimientos <= 1);

    PRINT 'Fichas a unificar: ' + CAST(@@ROWCOUNT AS varchar(10));

    -- 3.2 Mover la historia de las fichas duplicadas a la superviviente (no se pierde nada).
    UPDATE p SET p.Id_empleado = f.Id_superviviente
      FROM dbo.Pedidos p INNER JOIN #fusion f ON f.Id_empleado = p.Id_empleado;
    PRINT 'Pedidos reasignados: ' + CAST(@@ROWCOUNT AS varchar(10));

    UPDATE x SET x.Id_empleado = f.Id_superviviente
      FROM dbo.Ingresos x INNER JOIN #fusion f ON f.Id_empleado = x.Id_empleado;
    PRINT 'Ingresos reasignados: ' + CAST(@@ROWCOUNT AS varchar(10));

    UPDATE x SET x.Id_empleado = f.Id_superviviente
      FROM dbo.Egresos x INNER JOIN #fusion f ON f.Id_empleado = x.Id_empleado;
    PRINT 'Egresos reasignados: ' + CAST(@@ROWCOUNT AS varchar(10));

    UPDATE x SET x.Id_empleado = f.Id_superviviente
      FROM dbo.Nomina_empleado x INNER JOIN #fusion f ON f.Id_empleado = x.Id_empleado;
    PRINT 'Nominas reasignadas: ' + CAST(@@ROWCOUNT AS varchar(10));

    UPDATE x SET x.Id_empleado = f.Id_superviviente
      FROM dbo.Notas_empleado x INNER JOIN #fusion f ON f.Id_empleado = x.Id_empleado;
    PRINT 'Notas reasignadas: ' + CAST(@@ROWCOUNT AS varchar(10));

    -- 3.3 La ficha duplicada queda sin PIN e inactiva, pero no se borra (auditoria).
    UPDATE e
       SET e.Codigo_maestro = NULL,
           e.Estado = 'INACTIVO'
      FROM dbo.Empleados e
      INNER JOIN #fusion f ON f.Id_empleado = e.Id_empleado;
    PRINT 'Fichas duplicadas desactivadas y sin PIN: ' + CAST(@@ROWCOUNT AS varchar(10));

    -- 3.4 PIN 0 es "sin PIN" escrito como numero: se normaliza.
    UPDATE dbo.Empleados
       SET Codigo_maestro = NULL
     WHERE Id_empleado > 0
       AND Codigo_maestro = 0;
    PRINT 'PIN 0 convertido en NULL (sin PIN): ' + CAST(@@ROWCOUNT AS varchar(10));

    DROP TABLE #fusion;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

-- ---------------------------------------------------------------------------
-- 4. Informe posterior
-- ---------------------------------------------------------------------------
-- 4.1 Nombres que siguen con varias fichas. Deben ser todos INACTIVO y sin PIN; si alguno
--     tiene dos fichas con historia, la unificacion NO lo toco y hay que decidirlo a mano.
SELECT 'DESPUES 01 nombres con varias fichas' AS paso,
       e.Nombre_empleado,
       COUNT(*) AS fichas,
       SUM(CASE WHEN e.Estado = 'ACTIVO' THEN 1 ELSE 0 END) AS activas,
       SUM(CASE WHEN e.Codigo_maestro IS NOT NULL THEN 1 ELSE 0 END) AS con_pin,
       SUM(CASE WHEN p.pedidos > 0 THEN 1 ELSE 0 END) AS con_pedidos
FROM dbo.Empleados e
OUTER APPLY (SELECT COUNT(*) AS pedidos FROM dbo.Pedidos x WHERE x.Id_empleado = e.Id_empleado) p
WHERE e.Id_empleado > 0
GROUP BY e.Nombre_empleado
HAVING COUNT(*) > 1
ORDER BY e.Nombre_empleado;
GO

-- 4.2 PIN repetidos entre ACTIVO: debe quedar vacio. Si aparece algo, el login devolvera 409.
SELECT 'DESPUES 02 PIN compartido entre ACTIVO' AS paso, Codigo_maestro, COUNT(*) AS activos
FROM dbo.Empleados
WHERE Id_empleado > 0 AND Estado = 'ACTIVO' AND Codigo_maestro IS NOT NULL
GROUP BY Codigo_maestro HAVING COUNT(*) > 1;
GO

-- 4.3 Activos con PIN corto, sin PIN o de mas de 4 digitos. No se corrigen aqui: el PIN lo
--     decide el restaurante. Sin mostrar el PIN, solo su largo.
SELECT 'DESPUES 03 PIN debiles entre ACTIVO' AS paso,
       e.Id_empleado, e.Nombre_empleado, e.Cargo_empleado,
       CASE WHEN e.Codigo_maestro IS NULL THEN 'sin PIN'
            ELSE CAST(LEN(CAST(e.Codigo_maestro AS varchar(20))) AS varchar(10)) + ' digitos'
       END AS largo_del_pin
FROM dbo.Empleados e
WHERE e.Id_empleado > 0 AND e.Estado = 'ACTIVO'
  AND (e.Codigo_maestro IS NULL OR LEN(CAST(e.Codigo_maestro AS varchar(20))) <> 4)
ORDER BY e.Id_empleado;
GO

SELECT 'DESPUES 04 resumen' AS paso,
       (SELECT COUNT(*) FROM dbo.Empleados WHERE Id_empleado > 0 AND Estado = 'ACTIVO') AS activos,
       (SELECT COUNT(*) FROM dbo.Empleados WHERE Id_empleado > 0 AND Codigo_maestro = 0) AS con_pin_cero,
       (SELECT COUNT(*) FROM dbo.Empleados WHERE Id_empleado > 0 AND Estado = 'ACTIVO'
                                              AND Codigo_maestro IS NULL) AS activos_sin_pin;
GO
