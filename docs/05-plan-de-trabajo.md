# 05 — Plan de trabajo (fase 1: endpoints)

> Estimación preliminar para **una persona** working a tiempo parcial. No incluye el desarrollo de la app Flutter
> (fase 2), salvo la colección de pruebas y el contrato.

---

## 1. Resumen

| Fase | Entregable | Días |
|---|---|---|
| 0 | Entorno validado + esquema de producción congelado | 1 |
| 1 | Esqueleto del servidor + login + `/ping` | 2 |
| 2 | Catálogo, mesas e imágenes | 2–3 |
| 3 | `POST /pedidos` transaccional + idempotencia + impresión opcional | 3–4 |
| 4 | Operaciones sobre pedidos (ítems, estado, cancelar, reimprimir) | 2–3 |
| 5 | Endurecimiento, Postman, documentación final y entrega | 2 |
| **Total** | | **10–13** |

---

## 2. Fase 0 — Preparación (bloqueante)

| # | Tarea | Salida | Estado (2026-10-24) |
|---|---|---|---|
| 0.1 | Backup *schema-only* + SPs de producción | `db/produccion-schema.sql` versionado | ✅ Hecho |
| 0.2 | Comparar firmas de SPs y columnas contra el repo | Informe en [`db/README.md`](../db/README.md) y [02 §4](02-base-de-datos-y-hallazgos.md) | ✅ Hecho: coinciden |
| 0.3 | Confirmar `@@VERSION` (¿2016+ para `OPENJSON`?) | Captura/consulta | 🟡 La config desplegada usa **LocalDB** (`(localdb)\MSSQLLocalDB`); la BD está en compatibilidad 150 (2019+) → `OPENJSON` OK. Versión exacta y *collation*: solo se ven en el equipo (`SELECT @@VERSION, SERVERPROPERTY('Collation')`) |
| 0.4 | Definir alcance: ¿solo mesa o también domicilio? | Decisión en [06](06-decisiones-pendientes.md) | ✅ Solo mesa (0 domicilios en 38.579 pedidos) |
| 0.5 | IP fija / reserva DHCP del PC servidor | IP documentada | ⏳ Pendiente (D7) |
| 0.6 | Puerto libre + `urlacl` + regla de firewall | Comandos ejecutados y verificados | ⏳ Pendiente (D7) |
| 0.7 | Rama `feature/api-movil` y respaldo de la BD antes de probar | Rama + `.bak` | ✅ Rama creada; copia local `CasaGrandeBD_prod` restaurada |
| 0.8 | Confirmar que la build instalada = código del repo (o identificar diferencias) | `scripts/Compare-PublishedBuild.ps1` | ✅ La build publicada **1.0.0.108** (la que instala ClickOnce) coincide con el código actual: 2.871 métodos de 5 ensamblados con IL idéntico, 0 diferencias. Lo instalado en el equipo sería esa u otra anterior |
| 0.9 | Copia local de producción para desarrollar y probar | `db/Restore-ProdCopy.ps1` + `CasaGrandeBD_prod` | ✅ Hecho (restaura en ~80 s) |
| 0.10 | Cerrar D11, D12 y D13 (nuevas) | Decisiones en [06](06-decisiones-pendientes.md) | D12 ✅ (A); D11 aplazada; D13 se resuelve leyendo `FrmPedido` en la fase 2 |

**Criterio de salida:** puedo abrir el proyecto, compilar y ejecutar un `SELECT` contra la BD de producción (o una
copia exacta) sin errores de esquema. **Cumplido** contra `CasaGrandeBD_prod`; falta, para pasar a la fase 3,
cerrar D6 y D12.

---

## 3. Fase 1 — Esqueleto del servidor

| # | Tarea | Archivos |
|---|---|---|
| 1.1 | Clase `ApiConfig` con lectura de `App.config` y valores por defecto | `Api/ApiConfig.cs` |
| 1.2 | `ApiServer`: arranque/parada, `HttpListener`, hilo de fondo, manejo global de excepciones | `Api/ApiServer.cs` |
| 1.3 | `ApiRouter`: tabla de rutas, query string, parámetros de ruta | `Api/ApiRouter.cs` |
| 1.4 | `ApiRequest` / `ApiResponse` / `ApiJson` | `Api/ApiRequest.cs`, `Api/ApiResponse.cs`, `Api/ApiJson.cs` |
| 1.5 | `ApiAuth`: `X-Api-Key`, token, expiración, logout | `Api/ApiAuth.cs` |
| 1.6 | `ApiLog` diario | `Api/ApiLog.cs` |
| 1.7 | `GET /ping`, `POST /auth/login`, `GET /auth/me`, `POST /auth/logout` | `Api/ApiAuth.cs` |
| 1.8 | Enganche en el arranque (`Program.cs`/`FrmPrincipal`) protegido por `ApiHabilitada` | 3–6 líneas |
| 1.9 | Prueba manual con `curl` y Postman | colección `docs/postman/` |

**Criterio de salida:** login desde el móvil devuelve token; `/ping` responde desde otro equipo de la LAN;
al reiniciar el exe los tokens quedan invalidados.

### Estado de la fase 1 (2026-10-24): implementada y probada en localhost

| # | Estado | Dónde |
|---|---|---|
| 1.1 `ApiConfig` | ✅ | `CapaPresentacion/Api/ApiConfig.cs` |
| 1.2 `ApiServer` | ✅ (nunca lanza al arrancar; si falla, el escritorio abre igual) | `ApiServer.cs` |
| 1.3 `ApiRouter` | ✅ (rutas por plantilla, 404/405, errores saneados, 503 si no hay BD) | `ApiRouter.cs` |
| 1.4 Request/Response/Json | ✅ (`JavaScriptSerializer`, sin dependencias nuevas) | `ApiRequest.cs`, `ApiResponse.cs`, `ApiJson.cs` |
| 1.5 `ApiAuth` | ✅ tokens en memoria, clave compartida en tiempo constante, límite 5 fallos/min por IP, solo IP de la LAN | `ApiAuth.cs` |
| 1.6 `ApiLog` | ✅ `logs\api-yyyyMMdd.log`, sin credenciales ni tokens, depura a 30 días | `ApiLog.cs` |
| 1.7 `/ping`, `/auth/login`, `/auth/me`, `/auth/logout` | ✅ (login sin `sp_Login`: no toca turnos) | `ApiHandlers.cs`, `CapaDatos/DApiMovil.cs`, `CapaNegocio/NApiMovil.cs` |
| 1.8 Enganche | ✅ 3 líneas en `Program.cs`; apagada por defecto (`ApiHabilitada=false`) | `Program.cs`, `App.config` |
| 1.9 Prueba | ✅ `scripts/Test-ApiFase1.ps1` arranca el `.exe` real: **32/32** | colección Postman: pendiente (fase 5) |

Pendiente de la fase 1: probar `/ping` **desde otro equipo de la LAN** (necesita `urlacl` y regla de firewall, 0.5/0.6)
y que la API arranque con el prefijo `http://+:8099/`. En local se usa `http://localhost:8099/api/v1/`, que no pide
permisos.

---

## 4. Fase 2 — Lecturas

| # | Tarea | Archivos |
|---|---|---|
| 2.1 | `ApiCatalogoService`: tipos, platos, bebidas, ingredientes, clientes | `Api/ApiCatalogoService.cs` |
| 2.2 | `GET /imagenes/{archivo}` con validación de ruta | `Api/ApiImagenesService.cs` |
| 2.3 | `ApiMesasService`: estado del día con consulta propia | `Api/ApiMesasService.cs` |
| 2.4 | DTOs y mapeos | `Api/ApiModels.cs` |
| 2.5 | Pruebas de contrato (Postman) y ajustes de nombres | colección |

**Criterio de salida:** el móvil puede pintar el menú completo (con imágenes) y el mapa de mesas sin tocar la BD.

### Estado de la fase 2 (2026-10-24): implementada y probada en localhost

| # | Estado | Dónde |
|---|---|---|
| 2.1 Catálogo: tipos, platos, bebidas, ingredientes, clientes | ✅ solo lectura, parametrizado, comodines de `LIKE` escapados | `Api/ApiCatalogoHandlers.cs`, `CapaDatos/DApiMovil.cs`, `CapaNegocio/NApiMovil.cs` |
| 2.2 `GET /imagenes/{archivo}` con validación de ruta | ✅ probado con 6 intentos de *path traversal* | `Api/ApiImagenesHandlers.cs` |
| 2.3 Mapa de mesas del día | ✅ una mesa por `Num_mesa`, estado, pedido abierto, hora y total | `Api/ApiMesasHandlers.cs` |
| 2.4 DTOs | ✅ diccionarios (snake_case fijado), sin clases extra | `Api/ApiJson.cs` |
| 2.5 Pruebas de contrato | ✅ `scripts/Test-ApiFase2.ps1`: **65/65**, cada respuesta contrastada con consultas directas a la BD. Colección Postman: pendiente (fase 5) | `scripts/` |

Pendiente de la fase 2: `POST /clientes` (el pedido de mesa no lo necesita) y `GET /mesas/{id}/pedido` (depende del
formato de pedido de la fase 4). Hallazgo para tener presente: **ninguna imagen existe** (todos los platos y bebidas
activos valen `SIN IMAGEN`), por lo que `imagen_url` es siempre `null` hasta que el restaurante cargue imágenes.

---

## 5. Fase 3 — Alta de pedido

| # | Tarea | Archivos |
|---|---|---|
| 3.1 | ✅ Script del SP transaccional (esquema real: `Id_detalle_pedido`, `Precio` decimal, ingredientes ligados a su línea). Creado y probado en la copia local: 44/44 (`db/pruebas-sp-movil-insertar-pedido.sql`); **no desplegado en producción** | `db/01_sp_Movil_Insertar_pedido.sql` (+ `.revert.sql`) |
| 3.2 | Método DAL (`SqlCommand` + parámetros, sin concatenar) | `CapaDatos/DPedidosMovil.cs` |
| 3.3 | Fachada de negocio | `CapaNegocio/NPedidoMovil.cs` |
| 3.4 | `ApiPedidosService.Crear`: validaciones, precio, idempotencia, semáforo | `Api/ApiPedidosService.cs` |
| 3.5 | `POST /pedidos` | idem |
| 3.6 | Refresco de UI (`BeginInvoke`) e impresión opcional | `Api/ApiPedidosService.cs` |
| 3.7 | Pruebas: feliz, mesa ocupada, reintento, rollback, concurrencia | ver §8 |

**Criterio de salida:** pedido creado desde el móvil aparece en el escritorio, con sus líneas e ingredientes,
y no se duplica al reintentar.

### Estado de la fase 3 (2026-10-24): implementada y probada en localhost

| # | Estado | Dónde |
|---|---|---|
| 3.1 SP transaccional | ✅ fila a fila, cada línea con su `Id_detalle_pedido`; 50/50 pruebas. **No desplegado en producción** | `db/01_sp_Movil_Insertar_pedido.sql` |
| 3.2 Método DAL parametrizado | ✅ | `CapaDatos/DApiMovil.cs` |
| 3.3 Fachada de negocio | ✅ | `CapaNegocio/NApiMovil.cs` |
| 3.4 Validaciones, precio, idempotencia, semáforo | ✅ forma (400), reglas (404/409/422), clave en memoria 24 h con hash del cuerpo, `SemaphoreSlim` | `Api/ApiPedidosHandlers.cs`, `Api/ApiIdempotencia.cs` |
| 3.5 `POST /pedidos` (+ `GET /pedidos/{id}`) | ✅ | idem |
| 3.6 Refresco de UI e impresión opcional | 🟡 refresco del mapa y cola de impresión por `BeginInvoke` (`ApiUi`); **impresión sin probar con impresora real** | `Api/ApiUi.cs`, `Api/ApiImpresion.cs` |
| 3.7 Pruebas | ✅ `scripts/Test-ApiFase3.ps1`: **78/78**, contrastadas con la BD; deja la copia como estaba | `scripts/` |

Lo que cubren las 78: alta feliz con 4 ingredientes, precio de catálogo (el del cliente se ignora), reintento (200 y
mismo pedido), misma clave con otro cuerpo (409), mesa ocupada (409, por `Num_mesa`), mesa 5200 (404), **doble toque
simultáneo** (un 201 y un 200, un solo pedido en la BD), **misma mesa libre con dos claves a la vez** (un 201 y un
409), dos almuerzos con ingredientes distintos en filas distintas, 37 peticiones inválidas (400/404/422) que no
escriben nada, y que el log no guarda observaciones ni claves.

**Pendiente de la fase 3:** probar la impresión con una impresora real y con el mapa de mesas abierto (el test solo
verifica que no imprimir no rompe nada y que el refresco no falla con el mapa cerrado); y desplegar el SP en
producción (D6), que necesita aprobación y una copia de seguridad previa.

---

## 6. Fase 4 — Operaciones sobre pedidos

| # | Tarea |
|---|---|
| 4.1 | `GET /pedidos/{id}` y `GET /pedidos` (filtros) |
| 4.2 | `POST /pedidos/{id}/items` (inserta **filas nuevas**; no hay deltas, ver [02 §3.2](02-base-de-datos-y-hallazgos.md)) |
| 4.3 | `DELETE /pedidos/{id}/items/{id_detalle_pedido}` (con clave maestra; pasa el id real al SP) |
| 4.4 | `PATCH /pedidos/{id}/estado` (solo `PENDIENTE → SALIENDO`; conserva `Id_mesa`) |
| 4.5 | `POST /pedidos/{id}/cancelar` |
| 4.6 | `POST /pedidos/{id}/imprimir` (hilo de UI, `202`) |
| 4.7 | Pruebas de estados y de permisos |

**Criterio de salida:** el ciclo completo (crear → agregar → quitar → precuenta → cancelar) funciona desde el móvil
y se refleja en el escritorio.

### Estado de la fase 4 (2026-10-26): API implementada y probada; app pendiente

| # | Estado | Dónde |
|---|---|---|
| 4.1 `GET /pedidos` (filtros) y `GET /pedidos/{id}` | ✅ fecha (por defecto hoy), `estado` y `mesa`; resumen sin ítems con total y número de líneas | `Api/ApiPedidosAcciones.cs`, `CapaDatos/DApiMovil.cs` |
| 4.2 `POST /pedidos/{id}/items` | ✅ cada ítem crea una fila nueva; precio de catálogo; solo `PENDIENTE`/`SALIENDO`; SP transaccional nuevo | `db/03_sp_Movil_Agregar_items.sql`, `Api/ApiPedidosAcciones.cs` |
| 4.3 `DELETE /pedidos/{id}/items/{id_detalle_pedido}` | ✅ por `Id_detalle_pedido`, `clave_maestra` de cualquier empleado (como `FrmComprobacion`), `?cantidad=`; al borrar la línea se borran sus ingredientes | `db/04_sp_Movil_Quitar_item.sql` |
| 4.4 `PATCH /pedidos/{id}/estado` | ✅ solo `PENDIENTE → SALIENDO`; se pasa la mesa actual porque el SP la reescribe | `ApiPedidosAcciones.CambiarEstado` |
| 4.5 `POST /pedidos/{id}/cancelar` | ✅ `observaciones` 1–200; el SP reemplaza las observaciones del pedido (queda el motivo) | `ApiPedidosAcciones.Cancelar` |
| 4.6 `POST /pedidos/{id}/imprimir` | ✅ `PLATOS`/`BEBIDAS`/`TODO`, 1–5 copias, `202`; `409` si ya hay una en curso. Imprime en la impresora predeterminada; si es de PDF (Microsoft Print to PDF), guarda el archivo solo en `Documentos\Casa Grande\Comandas` sin diálogo | `Api/ApiImpresion.cs`, `FrmComandas.GuardarPdf` |
| 4.7 Pruebas | ✅ `scripts/Test-ApiFase4.ps1`: **51/51**, con limpieza verificada | `scripts/` |

**Pendiente de la fase 4:** la app móvil todavía no tiene pantallas para editar el pedido, pasar a `SALIENDO`,
cancelar ni imprimir (hoy solo crea pedidos y los lista en la bandeja). `GET /mesas/{id}/pedido` sigue sin existir:
el móvil consulta el detalle con `GET /pedidos/{id}`, que ya devuelve lo necesario.

---

## 7. Fase 5 — Endurecimiento y entrega

| # | Tarea |
|---|---|
| 5.1 | Allowlist de IP privadas + rate limit de login |
| 5.2 | Límites de tamaño/timeout + mensajes de error saneados |
| 5.3 | Logs con rotación y sin datos sensibles |
| 5.4 | Colección Postman + `docs/postman/README.md` |
| 5.5 | Manual de operación (`docs/08-manual-operacion.md`): arranque, IP, puerto, `ApiKey`, diagnóstico |
| 5.6 | Pruebas de carga ligera (5–10 pedidos concurrentes) |
| 5.7 | Demo con el cliente y acta de aceptación |

---

## 8. Plan de pruebas

### 8.1 Funcionales (manuales/Postman)

| Caso | Esperado |
|---|---|
| Ping desde la LAN | `200` + `db: true` |
| Login correcto / incorrecto | `200` con token / `401` |
| Login empleado inactivo | Según D12 (hoy el escritorio lo permite) |
| Login solo con PIN (sin `id_empleado`) | `400` |
| Login no altera `Turnos` | Mismo número de turnos y estados antes y después |
| Catálogo con tipo inexistente | `404` o lista vacía (definir) |
| Crear pedido válido | `201` + `id_pedido`, ítems con precio de catálogo |
| Crear pedido con mesa ocupada (pedido abierto **de hoy**) | `409 MESA_OCUPADA` |
| Crear pedido en mesa con un `PENDIENTE` de hace meses | `201` (los huérfanos antiguos no bloquean) |
| Agregar ítems a un pedido con el mismo producto ya pedido | Fila nueva; la cantidad existente no cambia |
| Quitar 1 unidad de una fila de cantidad 3 | Fila queda en 2 |
| Crear pedido con producto inactivo | `422` |
| Reintentar el mismo `idempotencia` | `200` + mismo `id_pedido` (sin duplicar) |
| Quitar ítem sin clave maestra | `400/403` |
| Quitar el último ítem | línea desaparece, pedido sigue existiendo |
| Cancelar sin observaciones | `400` |
| Estado inválido (`PENDIENTE → DISPONIBLE` o `→ TERMINADO`) | `422` |
| Imprimir comanda | `202` y la comanda sale en la impresora |

### 8.2 Integridad en BD

Después de cada alta, verificar:

```sql
SELECT * FROM Pedidos WHERE Id_pedido = @id;
SELECT * FROM Detalle_pedido WHERE Id_pedido = @id;
SELECT * FROM Detalle_ingredientes_pedido WHERE Id_pedido = @id;
```

* Exactamente 1 cabecera.
* Un pedido **nuevo** desde el móvil: una fila por producto, con `Precio` igual al del catálogo. (En pedidos
  creados por el escritorio puede haber varias filas por producto; no es un error.)
* Ingredientes solo de platos detallados, cada uno con el `Id_detalle_pedido` de su línea (no nulo).
* Probar siempre contra `CasaGrandeBD_prod` (`db/Restore-ProdCopy.ps1 -Force` la deja limpia en ~80 s).

### 8.3 Fallos y concurrencia

| Prueba | Cómo | Esperado |
|---|---|---|
| Rollback | Enviar 3 ítems con el 2.º inactivo | `422`, **0 filas** insertadas |
| Corte de red | Cortar WiFi entre request y respuesta; reintentar con el mismo GUID | 1 solo pedido |
| Doble toque | Dos POST simultáneos con el mismo GUID | 1 pedido, segunda respuesta `200` |
| Concurrencia de mesas | 5 pedidos a 5 mesas distintas en paralelo | 5 pedidos, sin bloqueos |
| Misma mesa | 2 pedidos simultáneos a la misma mesa | 1 creado, otro `409` |
| BD caída | Detener SQL Server | `503` con mensaje claro; el escritorio sigue vivo |

### 8.4 Regresión del escritorio

* Crear/editar/facturar un pedido desde la UI con la API activa y con la API apagada.
* Verificar que no aparece ningún mensaje nuevo ni cambia el comportamiento.
* Verificar que el refresco automático de mesas (si se habilita) no roba el foco ni bloquea la UI.

---

## 9. Archivos: resumen de cambios

| Acción | Ruta |
|---|---|
| Crear | `CapaPresentacion/Api/*.cs` (§3 de [03](03-propuesta-api-movil.md)) |
| Crear | `CapaDatos/DPedidosMovil.cs` |
| Crear | `CapaNegocio/NPedidoMovil.cs` |
| Crear | `db/01_sp_Movil_Insertar_pedido.sql` (+ reversión). Ya existen en `db/`: esquema, restauración, diagnóstico |
| Crear | `docs/postman/*` |
| Modificar | `SISMistico/Program.cs` (o `FrmPrincipal.cs`) — arranque del servidor |
| Modificar | `SISMistico/App.config` — claves `Api*` |
| Modificar | `CapaPresentacion/CapaPresentacion.csproj` — inclusión de archivos |
| **No modificar** | `FrmPedido`, `FrmRealizarPedido`, `TablasPedido`, SPs existentes |

---

## 10. Criterios de aceptación de la fase 1

1. Un cliente HTTP en la LAN (Postman/Flutter) completa: login → mesas → catálogo → crear pedido → consultar.
2. El pedido creado desde el móvil se ve en el escritorio y en la BD con las líneas y precios correctos.
3. No se puede crear un pedido en mesa ocupada ni con productos inactivos.
4. Reintentar el envío no duplica pedidos.
5. Si la API no está habilitada o falla, el escritorio funciona exactamente igual que antes.
6. Ninguna operación deja datos parciales.
7. Los logs permiten reconstruir qué pasó con un pedido.

---

## 11. Riesgos del plan

| Riesgo | Efecto | Contingencia |
|---|---|---|
| ~~El esquema de producción no llega a tiempo~~ | Resuelto el 2026-10-24 | Esquema versionado en `db/` |
| ~~Producción con SQL &lt; 2016~~ | Descartado: compatibilidad 150 (2019+) | — |
| La build instalada difiere del repo | El SP/API se prueba contra un comportamiento distinto | Probar en el equipo del restaurante antes de la demo (0.8) |
| D12/D11 sin decidir | Bloquea login y edición de ítems | Cerrarlas antes de la fase 4 |
| El restaurante no puede reservar URL | API no arranca | `urlacl` por usuario, o servidor `TcpListener` |
| Se necesita imprimir y no hay impresora en el PC | Comanda no sale | Dejar `imprimir_comandas=false` y mantener impresión en escritorio |
| Cambios de alcance (mesas + domicilio + caja) | Retraso | Congelar alcance en fase 0 |
