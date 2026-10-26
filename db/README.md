# `db/` — Base de datos de producción: esquema, copia local y diagnóstico

Fuente de verdad del esquema de **producción** (`CasaGrandeBD` del PC del restaurante) y herramientas para
trabajar contra una copia local. Contexto y hallazgos en [`docs/02`](../docs/02-base-de-datos-y-hallazgos.md).

> ⚠️ La `CasaGrandeBD` que ya existía en LocalDB (la de `run-local.ps1`) **no es** el esquema de producción.
> Para desarrollar la API usar la copia `CasaGrandeBD_prod` que crea `Restore-ProdCopy.ps1`.

## Contenido

| Archivo | Qué es |
|---|---|
| `produccion-schema.sql` | Esquema de producción (25 tablas, 45 SPs, 2 tipos de tabla). **Sin datos.** Generado por SSMS el 2026-10-24 |
| `Export-ProdSchema.ps1` | Convierte el respaldo de esquema (UTF-16) a este UTF-8. Se niega a procesar scripts con datos |
| `Restore-ProdCopy.ps1` | Crea `CasaGrandeBD_prod` en LocalDB desde el respaldo **completo** (con datos reales) |
| `Invoke-Sql.ps1` | Ejecuta un `.sql` (lotes con `GO`) contra LocalDB e imprime los resultados; sustituye a `sqlcmd` |
| `diagnostico-prod.sql` | Consultas de solo lectura que contrastan con datos reales los supuestos de `docs/` |
| `pruebas-sp-actualizar-detalle.sql` | Prueba, con rollback, el comportamiento real de `sp_Actualizar_detalle_pedido` |
| `diagnostico-fase2.sql`, `diagnostico-fase2b.sql` | Consultas de solo lectura sobre catálogo, mesas e imágenes (mesa 21 duplicada, `SIN IMAGEN`, `CantidadClientes`) que condicionaron la fase 2 |
| `01_sp_Movil_Insertar_pedido.sql` | **SP nuevo y aditivo** para el alta atómica desde la API. Creado y probado en la copia local; **no desplegado en producción** |
| `01_sp_Movil_Insertar_pedido.revert.sql` | Reversión del anterior (`DROP PROCEDURE IF EXISTS`) |
| `pruebas-sp-movil-insertar-pedido.sql` | 50 comprobaciones del SP nuevo (alta válida, precios de catálogo, ingredientes, varias filas del mismo plato con ingredientes distintos, casos de error, rollback real). Deja la copia intacta |
| `diagnostico-fase3.sql` | Cómo guarda el escritorio los ingredientes de los platos detallados (patrón por línea, sopa en carta, líneas sin ingredientes, cantidades) |
| `02_unificar_empleados_duplicados.sql` | **Arreglo de datos**: deja una sola ficha con PIN por persona (reasigna la historia a la superviviente, desactiva las duplicadas sin borrarlas) y convierte el PIN `0` en `NULL`. Idempotente; respalda antes en `Empleados_respaldo_unificacion`. Ejecutado en la copia el 2026-10-26; **falta en producción** |
| `02_unificar_empleados_duplicados.revert.sql` | Devuelve `Codigo_maestro` y `Estado` desde ese respaldo y lo elimina |
| `03_sp_Movil_Agregar_items.sql` | **Fase 4.** Agrega líneas a un pedido existente en una transacción (precio de catálogo, ingredientes por línea, solo si está `PENDIENTE`/`SALIENDO`). Aditivo; **no desplegado en producción** |
| `04_sp_Movil_Quitar_item.sql` | **Fase 4.** Quita unidades de una línea por `Id_detalle_pedido`; si desaparece, borra sus ingredientes en la misma transacción. Aditivo; **no desplegado en producción** |
| `verificar-despliegue.sql` | **Solo lectura.** Comprueba que estén los tres SP nuevos, que no haya PIN compartidos entre activos ni PIN en cero, y cuenta mesas y pedidos. Se usa al desplegar en el restaurante ([`docs/08`](../docs/08-instalacion-desde-cero.md)) |
| `03_*.revert.sql`, `04_*.revert.sql` | `DROP PROCEDURE` de los dos anteriores |

Los respaldos originales **no** están en el repositorio (el completo tiene 280 MB de datos reales: empleados,
claves, ventas). Viven en `D:\Usuario_JUAN_DIEGO\Escritorio\Casa Grande\`.

## Cómo reproducirlo

```powershell
cd D:\Repositorios\System-RestaurantCasaGrande\db

.\Export-ProdSchema.ps1                      # produccion-schema.sql (solo si hay un respaldo nuevo)
.\Restore-ProdCopy.ps1 -Force                # ~80 s; recrea CasaGrandeBD_prod
.\Invoke-Sql.ps1 -File .\diagnostico-prod.sql
.\Invoke-Sql.ps1 -File .\pruebas-sp-actualizar-detalle.sql

.\Invoke-Sql.ps1 -File .\01_sp_Movil_Insertar_pedido.sql          # crea el SP en la copia
.\Invoke-Sql.ps1 -File .\pruebas-sp-movil-insertar-pedido.sql     # debe terminar en "50 OK de 50"

.\Invoke-Sql.ps1 -File .\02_unificar_empleados_duplicados.sql     # unifica fichas y limpia PIN 0
.\Invoke-Sql.ps1 -File .\02_unificar_empleados_duplicados.revert.sql   # solo si hay que deshacerlo

.\Invoke-Sql.ps1 -File .\03_sp_Movil_Agregar_items.sql            # fase 4: agregar líneas
.\Invoke-Sql.ps1 -File .\04_sp_Movil_Quitar_item.sql              # fase 4: quitar unidades
```

La API y la app de escritorio contra esta copia: `SISMistico\run-local.ps1` (ver [`scripts/`](../scripts) para las
pruebas de extremo a extremo y la comparación con la build publicada).

Cadena de conexión de la copia: `Server=(localdb)\MSSQLLocalDB;Database=CasaGrandeBD_prod;Integrated Security=true`.

### Notas

* `Restore-ProdCopy.ps1` descarta el `CREATE DATABASE`/`ALTER DATABASE` del script (rutas
  `C:\Users\Casa Grande\…`) y carga el resto por bloques **sin tocar los saltos de línea de los datos**. Leer línea
  a línea y volver a unir cambia `\n` por `\r\n` dentro de los textos y provoca
  *"String or binary data would be truncated"* (pasó en el primer intento).
* La copia contiene **datos personales y credenciales reales**. Es solo para desarrollo local: no exportarla ni
  subirla a ningún sitio.
* Las pruebas que escriben (`pruebas-*.sql`) van envueltas en `BEGIN TRANSACTION … ROLLBACK`. Aun así, ejecutarlas
  solo contra `CasaGrandeBD_prod`, nunca contra el servidor del restaurante.
* `02_unificar_empleados_duplicados.sql` **sí escribe** (es un arreglo de datos, no una prueba). Es idempotente:
  volver a ejecutarlo informa "PIN compartido entre ACTIVO: 0" y no cambia nada. Deja respaldo en
  `dbo.Empleados_respaldo_unificacion` y su `.revert.sql` restaura `Codigo_maestro` y `Estado`.

## Comparación esquema ↔ código (tarea 0.2)

Resumen; el detalle está en [`docs/02 §4`](../docs/02-base-de-datos-y-hallazgos.md). Las firmas de los 5 SPs
que invoca `DPedidos.cs` (`sp_Insertar_pedido`, `sp_Actualizar_detalle_pedido`, `sp_Insertar_detalle_pedido`,
`sp_Cambiar_estado_pedido`, `sp_Cancelar_pedido`) y las columnas `Id_detalle_pedido` coinciden **exactamente**
con lo que envía el código. Lo que no se pudo comprobar sin acceso al servidor: la versión exacta de SQL Server
y su *collation* (el script solo indica nivel de compatibilidad 150).

## Scripts nuevos de la API

Los objetos de BD que cree la API (p. ej. `sp_Movil_Insertar_pedido`) irán aquí como `01_…sql`, `02_…sql`,
siempre **aditivos** (`CREATE`, nunca `ALTER` de un objeto existente) y con su script de reversión.
