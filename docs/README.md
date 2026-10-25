# Documentación — Sistema Restaurante Casa Grande

Carpeta de documentación técnica del proyecto `System-RestaurantCasaGrande`.

> **Estado:** borrador de trabajo, **revisado el 2026-10-24** con la base de datos de producción.
> **Alcance de esta documentación:** Fase 1 — exponer endpoints locales (LAN) desde la aplicación
> WinForms actual para que un futuro cliente Flutter pueda registrar pedidos **de mesa**.
> **Nada de lo aquí descrito modifica el sistema en producción.** Es análisis + propuesta.

---

## Índice

| Documento | Contenido |
|---|---|
| [01 — Flujo actual de pedidos](01-flujo-actual-de-pedidos.md) | Cómo se guarda un pedido hoy: formularios activos y muertos, secuencia exacta, modelo de datos, reglas de negocio, estados reales |
| [02 — Base de datos y hallazgos](02-base-de-datos-y-hallazgos.md) | Esquema **de producción**, SPs, comparación con el código, riesgos y matriz de impacto |
| [03 — Propuesta de arquitectura](03-propuesta-api-movil.md) | API embebida en el `.exe`, componentes nuevos, seguridad, hilos, despliegue y red |
| [04 — Contrato de API v1](04-contrato-api-v1.md) | Endpoints, JSON, validaciones, códigos de error, máquina de estados |
| [05 — Plan de trabajo](05-plan-de-trabajo.md) | Fases, archivos a crear/modificar, estado de la fase 0, pruebas y criterios de aceptación |
| [06 — Decisiones pendientes](06-decisiones-pendientes.md) | Preguntas abiertas (D1 resuelta; D11–D13 nuevas) |
| [07 — Anexos técnicos](07-anexos-tecnicos.md) | SP propuesto (borrador), esqueleto de código, comandos de diagnóstico, glosario, referencias |
| [**App móvil (Flutter)**](app-movil/README.md) | Modelado previo al código de la app del mesero: alcance, arquitectura, módulos, modelo de datos, pantallas, librerías verificadas, pruebas y plan |
| [`../db/`](../db/README.md) | Esquema de producción versionado, restauración de la copia local y scripts de diagnóstico |

---

## Resumen ejecutivo

1. **El alta de pedidos está concentrada en `FrmPedido.cs`**, que se apoya en `NPedido`/`DPedidos`.
   El formulario `FrmRealizarPedido` es legado: su guardado está comentado y no persiste nada.
2. **El guardado actual no es transaccional**: cabecera (`sp_Insertar_pedido`), luego un viaje a BD por cada
   línea (`sp_Actualizar_detalle_pedido`) y finalmente los ingredientes (SQL concatenado). En producción hay 209
   pedidos sin ninguna línea.
3. **El esquema de producción coincide con el código.** El desajuste del 2026-10-21 era de la LocalDB de
   desarrollo, que no es una referencia válida. Desde el 2026-10-24 el esquema real está en `db/` y hay una copia
   local de producción (`CasaGrandeBD_prod`) para desarrollar y probar.
4. **Hallazgos que cambian el diseño** ([02 §5](02-base-de-datos-y-hallazgos.md)):
   * el estado final de un pedido facturado es **`DISPONIBLE`**, no `TERMINADO`;
   * `sp_Actualizar_detalle_pedido` **no suma deltas** (con una línea de 3 y `@Cantidad = 2` queda en 2) y trabaja
     por `Id_detalle_pedido`; el 31 % de los pedidos tiene el mismo producto en varias filas;
   * `sp_Login` **cierra y abre turnos**: la API no debe llamarlo;
   * hay PIN repetidos (el login no puede ser solo con PIN) y empleados `INACTIVO` que siguen tomando pedidos;
   * en 38.579 pedidos **nunca hubo un domicilio**: la fase 1 es solo mesa.
5. **Propuesta:** servidor HTTP embebido en el propio `.exe` (`HttpListener`, puerto 8099) con una capa nueva
   `CapaPresentacion/Api/`, reutilizando `CapaNegocio`/`CapaDatos`, y **un SP nuevo y aditivo** para que el alta
   desde móvil sea atómica. Los formularios y SPs existentes no se tocan.
6. **Estimación:** 10–13 días hábiles para la fase 1 completa (endpoints de pedidos + catálogo + seguridad básica).
   **Avance (2026-10-24):** fase 0 cerrada salvo red del PC del restaurante (IP fija, `urlacl`, firewall); fases 1, 2
   y 3 de código hechas y probadas en localhost (login, catálogo, mesas, imágenes y **alta de pedidos atómica e
   idempotente**). El SP está probado en la copia local pero **no desplegado en producción**. Siguiente: fase 4
   (agregar/quitar ítems, estado, cancelar, imprimir). Ver [05](05-plan-de-trabajo.md).

---

## Cómo leer esta documentación

* **[01](01-flujo-actual-de-pedidos.md)** responde “¿cómo funciona hoy?”. Útil para el equipo que mantiene el
  escritorio y para entender qué debe replicar la API.
* **[02](02-base-de-datos-y-hallazgos.md)** responde “¿qué está mal o en riesgo?”. Es la lista de verificación
  previa a escribir código.
* **[03](03-propuesta-api-movil.md)** y **[04](04-contrato-api-v1.md)** son la base para implementar y para que el
  equipo Flutter trabaje en paralelo contra un contrato congelado (aún no congelado: faltan D11–D13).
* **[05](05-plan-de-trabajo.md)** y **[06](06-decisiones-pendientes.md)** son la parte de gestión: qué se hace,
  en qué orden, y qué falta decidir.

---

## Base analizada

* Repositorio: `D:\Repositorios\System-RestaurantCasaGrande`
* Solución: `SISMistico/SISMistico.sln` — .NET Framework 4.8, WinForms, 4 capas + proyecto de arranque.
* Último commit analizado: `3ee08f3` (2026-01-23). La build publicada 1.0.0.108 tiene el mismo IL que este código.
* Base de datos: **producción**, según los respaldos de esquema y completo del 2026-10-24 (SSMS, equipo del
  restaurante). Copia local: LocalDB `(localdb)\MSSQLLocalDB` → `CasaGrandeBD_prod`. La `CasaGrandeBD` de
  desarrollo en LocalDB **no es** el esquema de producción y no debe usarse.
* Las referencias `Archivo.cs:línea` corresponden al estado del repositorio en la fecha indicada.

---

## Convenciones de esta carpeta

* 🔴 bloqueante · 🟠 riesgo alto · 🟡 riesgo medio · ✅ resuelto
* “**Verificado**” = comprobado ejecutando contra la copia de producción o leyendo el código en el commit indicado.
* “**Supuesto**” = inferencia pendiente de confirmar (normalmente, en el equipo del restaurante).
* Los documentos se actualizan a medida que se cierren las decisiones de [06](06-decisiones-pendientes.md).

---

## Registro de cambios

| Fecha | Cambio |
|---|---|
| 2026-10-24 (final) | **App Flutter escrita** en el repositorio nuevo `CasaGrandeMesero` (solo Android): paquete `casa_grande_api` (51 pruebas) y app con login por PIN, mesas, menú, pedido, bandeja con reintento y ajustes; `flutter analyze` sin errores y APK de depuración compilado. **Login solo por PIN** en la API (`409 PIN_AMBIGUO`, `db/diagnostico-pin.sql`, 43 pruebas en `Test-ApiFase1.ps1`). Detalle en [app-movil/09](app-movil/09-estado-de-la-implementacion.md) |
| 2026-10-21 | Primera versión (análisis del código y de la LocalDB de desarrollo) |
| 2026-10-24 (madrugada) | **Fase 3 implementada**: `POST /pedidos` atómico e idempotente (+ `GET /pedidos/{id}`), con el SP reescrito fila a fila; D14 decidida (los 4 tipos de ingrediente); 78/78 pruebas de extremo a extremo con concurrencia real; la impresión de comandas queda implementada pero sin probar con impresora |
| 2026-10-24 (noche) | **Fase 2 implementada**: catálogo (tipos, platos, bebidas, ingredientes, clientes), mapa de mesas e imágenes; 65/65 pruebas contrastadas con la BD; D13 resuelta; hallazgos: ninguna imagen existe, mesa 21 duplicada, `CantidadClientes` siempre 0, ingredientes globales |
| 2026-10-24 (tarde) | **Fase 1 de código implementada** (`/ping`, `/auth/*`; 32/32 pruebas contra el `.exe` real); SP `sp_Movil_Insertar_pedido` creado y probado en la copia local (44/44, no desplegado); D12 decidida (paridad con el escritorio); build publicada = código del repo; `run-local.ps1` usa la copia de producción |
| 2026-10-24 | Revisión con la BD de producción: D1 resuelta; estado `DISPONIBLE`; `sp_Actualizar_detalle_pedido` sin deltas y por `Id_detalle_pedido`; login sin `sp_Login` y sin PIN solo; bug de mesas inexistente en producción; domicilios fuera de alcance; nuevas D11–D13; carpeta `db/` |
