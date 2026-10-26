# 00 — Estado del proyecto (2026-10-26)

Foto del proyecto al **26 de octubre de 2026**: qué está construido, qué está verificado, qué falta y por qué.
El detalle de cada tema vive en los documentos enlazados; aquí está el mapa.

> **Alcance actual:** que un mesero tome pedidos **de mesa** desde un teléfono Android en la red local del
> restaurante, contra una API que corre dentro del `.exe` del escritorio. Facturar, caja, inventario y reportes
> siguen siendo del escritorio.
>
> **Lo que NO está en producción:** ni la API, ni los tres SP nuevos, ni el arreglo de empleados. Todo está
> probado contra la **copia local de producción** (`CasaGrandeBD_prod`). Ver [§6](#6-base-de-datos-qué-hay-en-la-copia-y-qué-falta-en-producción).

---

## 1. Resumen en una mirada

| Bloque | Estado | Evidencia |
|---|---|---|
| Análisis y contrato de la API | ✅ Cerrado | [01](01-flujo-actual-de-pedidos.md)–[07](07-anexos-tecnicos.md), [04 contrato v1](04-contrato-api-v1.md) |
| Esquema de producción versionado + copia local | ✅ | [`db/`](../db/README.md), `CasaGrandeBD_prod` |
| Fase 1 — `/ping` y `/auth/*` | ✅ Probada (44/44) | `scripts/Test-ApiFase1.ps1` |
| Fase 2 — catálogo, mesas, imágenes | ✅ Probada (65/65) | `scripts/Test-ApiFase2.ps1` |
| Fase 3 — alta de pedido atómica | ✅ Probada (78/78) | `scripts/Test-ApiFase3.ps1` |
| Fase 4 — listar/agregar/quitar/estado/cancelar/imprimir | ✅ Probada (54/54) | `scripts/Test-ApiFase4.ps1` |
| App móvil (Android) | ✅ Escrita y probada en un S24 Ultra | [`app-movil/09`](app-movil/09-estado-de-la-implementacion.md) |
| Datos de empleados (PIN único por persona) | ✅ En la copia | [`db/02`](../db/02_unificar_empleados_duplicados.sql) |
| Red del restaurante (IP fija, `urlacl`, firewall) | ⏳ **Pendiente (D7)** | Solo probado por USB con `adb reverse` |
| Despliegue en producción (SP + datos + API activa) | ⏳ **Pendiente** | Requiere aprobación y copia de seguridad |
| Impresión en impresora **física** | ⏳ Pendiente | La ruta de PDF sí está probada |
| APK firmado de release | ⏳ Pendiente | Hoy solo APK de depuración |

---

## 2. Dónde está el código

| Repositorio | Rama | Último commit | Contenido |
|---|---|---|---|
| `D:\Repositorios\System-RestaurantCasaGrande` | `feature/api-movil` | `0380361` | Escritorio + API embebida + `db/` + `scripts/` + documentación |
| `D:\Repositorios\CasaGrandeMesero` | — | `01fb80d` | App Android (workspace de Dart: `app` + `packages/casa_grande_api`) |

`main` del repositorio del escritorio sigue en `3ee08f3` (sin la API): no se ha publicado nada.

Commits de esta etapa (del más viejo al más nuevo):

1. `a1a0331` — diseño y contrato de la API.
2. `3304af2` — esquema de producción y herramientas de BD.
3. `654267e` — pruebas de fases 1–3 y comparación con la build publicada.
4. `8ebf25b` — API embebida en el escritorio (fases 1–3).
5. `22fb271` — login solo por PIN + documentación de la app.
6. `fa37674` — unificación de fichas duplicadas y aviso en el selector de PIN.
7. `4693037` — fase 4 de la API.
8. `c827245` — impresión real con PDF automático en Documentos.
9. `0380361` — refresco del mapa sin "colección modificada".

App: `b284b43` (primera versión), `ea22ca2` (botón tapado por la barra + refresco tras la bandeja),
`01fb80d` (pantallas de fase 4 + símbolo `$` al inicio).

---

## 3. Qué está hecho, fase por fase

### Fase 0 — Preparación ✅ (salvo red)

* Esquema de producción versionado (25 tablas, 45 SPs) y copia local restaurable (`db/Restore-ProdCopy.ps1`, ~80 s).
* Build publicada `1.0.0.108` = código del repo (2.871 métodos con IL idéntico).
* **Falta:** IP fija, `urlacl` y regla de firewall (D7); versión y *collation* exactas del SQL Server del restaurante.

### Fase 1 — Esqueleto del servidor ✅

`HttpListener` en el hilo de fondo, rutas por plantilla, `X-Api-Key`, tokens en memoria (8 h), límite de
5 intentos/min por IP, solo IP privadas, log diario sin credenciales, y `/ping` + `/auth/login|me|logout`.
El login **no** usa `sp_Login` (ese SP cierra y abre turnos) y acepta **solo PIN**: si lo comparten varios
empleados responde `409 PIN_AMBIGUO` con los candidatos.

### Fase 2 — Lecturas ✅

Tipos de platos y bebidas, platos (solo `ACTIVO`, como el escritorio), bebidas, ingredientes, clientes, mapa
de mesas del día (una fila por `Num_mesa`, con pedido abierto y total) e imágenes con validación de ruta.
Hallazgos vigentes: **ninguna imagen existe** (`imagen_url` siempre `null` hasta que el restaurante las cargue)
y la mesa 21 tiene un `Id_mesa` duplicado (se usa el canónico 5150).

### Fase 3 — Alta de pedido ✅

`POST /pedidos` atómico e idempotente con `sp_Movil_Insertar_pedido`: una fila por ítem, precio **del catálogo**
(el del cliente se ignora), ingredientes ligados a su línea y `MESA_OCUPADA`/`422` según corresponda.
Probado con doble toque simultáneo y dos claves distintas sobre la misma mesa.

### Fase 4 — Operaciones sobre el pedido ✅

| Endpoint | Comportamiento |
|---|---|
| `GET /pedidos?fecha&estado&mesa` | resumen del día con total y número de líneas |
| `POST /pedidos/{id}/items` | fila nueva por ítem (`sp_Movil_Agregar_items`); solo `PENDIENTE`/`SALIENDO` |
| `DELETE /pedidos/{id}/items/{id_detalle_pedido}?clave_maestra=&cantidad=` | por id de línea; el PIN de **cualquier** empleado autoriza (como `FrmComprobacion`); al eliminar la línea borra sus ingredientes |
| `PATCH /pedidos/{id}/estado` | solo `PENDIENTE → SALIENDO`; conserva la mesa |
| `POST /pedidos/{id}/cancelar` | motivo obligatorio (1–200) |
| `POST /pedidos/{id}/imprimir` | `PLATOS`/`BEBIDAS`/`TODO`, 1–5 copias, `202`; `409` si ya hay una en curso |

La impresión es **real**: usa la impresora predeterminada de Windows y, si esa impresora es de PDF
(Microsoft Print to PDF, Adobe PDF…), guarda el archivo sola en **`Documentos\Casa Grande\Comandas`** sin abrir
el diálogo de "Guardar como" (configurable con `ApiComandasCarpeta`).

**Total de endpoints: 22.** No existen `POST /clientes` (el pedido de mesa no lo necesita) ni
`GET /mesas/{id}/pedido` (el móvil usa `GET /pedidos/{id}`).

---

## 4. La app móvil (Android)

Solo Android, Flutter 3.47.8 / Dart 3.13.7 (instalado en `D:\DevTools\flutter-3.47.8`), Riverpod 3, go_router,
Drift, Dio. Workspace con dos paquetes: `packages/casa_grande_api` (cliente tipado) y `app` (feature-first:
`auth`, `server_setup`, `menu`, `cart`, `tables`, `orders`, `settings`, `core`).

| Pantalla | Qué hace |
|---|---|
| Configurar servidor | IP/nombre + puerto + clave, con prueba real (`/ping` + clave + BD) |
| Login | Teclado de PIN; si el PIN es ambiguo, selector con aviso de fichas duplicadas |
| Mesas | Mapa del día con estados, borrador local y pedidos "por enviar" |
| Menú | Búsqueda, pestañas Platos/Bebidas, configurador de los 4 ingredientes |
| Pedido (carrito) | Cantidades, comensales, notas, imprimir comanda |
| Detalle del pedido | Líneas, total y acciones: agregar productos, precuenta, quitar con PIN, cancelar, imprimir |
| Bandeja (outbox) | Pedidos sin enviar, reintento con la misma clave de idempotencia |
| Ajustes | Servidor, sesión, imprimir por defecto, tema, actualizar menú, registros |

Verificado en un **Samsung S24 Ultra** (Android 16): conexión, login, mapa, alta de pedido con ingredientes,
refresco del escritorio, modo sin conexión y reenvío sin duplicar, agregar/quitar líneas, `SALIENDO`, cancelar,
imprimir a PDF. Los precios se muestran como **`$ 5.000`** (símbolo delante) en toda la app.

---

## 5. Configuración y operación en local

| Pieza | Dónde |
|---|---|
| Claves de la API | `SISMistico/SISMistico/App.config` (`ApiHabilitada`, `ApiPrefijo`, `ApiKey`, `ApiTokenHoras`, `ApiMaxItems`, `ApiImprimirComandas`, `ApiComandasCarpeta`, `ApiLogCarpeta`, `ApiImagenesCarpeta`) |
| Arranque local | `SISMistico\run-local.ps1` (`-Rebuild`, `-NoRun`, `-UseDevDb`, `-NoApi`, `-ApiPrefix`) |
| Log de la API | `SISMistico\SISMistico\bin\LocalBuild\logs\api-yyyyMMdd.log` |
| Copia de producción | `db\Restore-ProdCopy.ps1` → `(localdb)\MSSQLLocalDB` · `CasaGrandeBD_prod` |
| Ejecutar SQL | `db\Invoke-Sql.ps1 -File <script>` |
| APK de la app | `app\build\app\outputs\flutter-apk\app-debug.apk` |

**Prueba por USB (la que se usó):** `adb reverse tcp:8099 tcp:8099` y en la app servidor `localhost:8099`.
No pide administrador ni toca la red. Para usarlo sin cable hace falta D7.

---

## 6. Base de datos: qué hay en la copia y qué falta en producción

| Objeto | Tipo | En la copia | En producción |
|---|---|---|---|
| `sp_Movil_Insertar_pedido` | SP nuevo (aditivo) | ✅ | ❌ |
| `sp_Movil_Agregar_items` | SP nuevo (aditivo) | ✅ | ❌ |
| `sp_Movil_Quitar_item` | SP nuevo (aditivo) | ✅ | ❌ |
| Arreglo de empleados duplicados + PIN `0` | Datos | ✅ | ❌ |

Estado de la copia tras el arreglo: **38.579 pedidos**, `MAX(Id_pedido) = 14551390`, **24 empleados activos**,
**0 PIN compartidos** entre activos, 1 activo **sin PIN** y **7 activos con PIN de menos de 4 dígitos**.
Queda a decisión del restaurante el caso `SANTIAGO` (dos fichas con historia y PIN distinto: la unificación no
las toca a propósito).

Los tres SP y el arreglo son **aditivos y reversibles**; ver [`db/README.md`](../db/README.md) para el orden de
ejecución y los `.revert.sql`.

---

## 7. Verificación (todo ejecutado el 2026-10-26)

| Suite | Casos | Resultado | Qué cubre |
|---|---|---|---|
| `Test-ApiFase1.ps1` | 44 | ✅ 44/44 | ping, clave, login por PIN/contraseña, 409 ambiguo, PIN 0, sesión, límite de intentos, log sin secretos |
| `Test-ApiFase2.ps1` | 65 | ✅ 65/65 | catálogo, mesas, imágenes y *path traversal*; cada dato contrastado con la BD |
| `Test-ApiFase3.ps1` | 78 | ✅ 78/78 | alta, precios de catálogo, ingredientes, idempotencia, concurrencia, 37 peticiones inválidas |
| `Test-ApiFase4.ps1` | 54 | ✅ 54/54 | listado, agregar/quitar con PIN, `SALIENDO`, cancelar, imprimir (PDF comprobado y borrado) |
| `flutter test` (app) | 6 | ✅ | dinero (formato `$` delante), borradores, errores |
| `flutter test` (paquete api) | 52 | ✅ | DTOs, cliente, reintentos, candidatos duplicados |
| `flutter analyze` | — | 19 avisos informativos | sin errores |
| `flutter build apk --debug` | — | ✅ | APK de depuración |

Los scripts arrancan el `.exe` real contra la copia, limpian lo que crean y verifican que los conteos vuelven a
los iniciales. Los cuatro leen el `ApiKey` del `.exe.config` de la compilación local.

---

## 8. Decisiones

### Cerradas

| # | Decisión |
|---|---|
| D1 | Esquema real de producción obtenido y versionado |
| D2 (parcial) | Alcance **solo mesas** (0 domicilios en 38.579 pedidos) — **falta el visto bueno formal** |
| D3 | Login **solo por PIN**; si se comparte, `409` con selector; PIN `0` no es credencial |
| D4 | Imprimir comandas por petición (`imprimir_comandas`), apagado por defecto |
| D5 | Desde el móvil: crear y agregar libremente; quitar y cancelar con PIN; facturar **no** |
| D6 | SP transaccional nuevo en lugar de reutilizar el guardado no atómico del escritorio (aprobado para la copia) |
| D9 | Cliente opcional en mesa (`0 / NINGUNO`) |
| D10 | HTTP en la LAN (sin TLS) en la v1 |
| D11 | Aplazada: el bug del escritorio al quitar productos no afecta a la API (usa `Id_detalle_pedido`) |
| D12 | Empleados `INACTIVO` pueden entrar (paridad con el escritorio), con log |
| D13 | Catálogo = solo `Estado = 'ACTIVO'` |
| D14 | Platos detallados exigen los 4 tipos de ingrediente, también en carta |

### Abiertas

| # | Qué falta |
|---|---|
| D2 | Visto bueno formal a "solo mesa" |
| D7 | IP fija o reserva DHCP, permisos de administrador, Windows, tipo de red; `urlacl` + firewall + `ApiKey` definitiva |
| D8 | Cuántos dispositivos usarán el móvil a la vez (para la prueba de carga) |
| — | PIN de menos de 4 dígitos (7 activos) y fichas `SANTIAGO` |

---

## 9. Riesgos y limitaciones conocidas

* **Los SP y el arreglo de datos no están en producción.** Mientras no se desplieguen, la API puede crear pedidos
  con el SP de la copia solo en local.
* **Sin TLS** (D10): la API debe quedarse en la LAN; nunca exponer el puerto a internet.
* **Tokens en memoria**: reiniciar el `.exe` invalida todas las sesiones (los meseros deben volver a entrar).
* **Autorización por PIN, sin roles**: cualquier PIN válido permite quitar líneas; es la paridad con el escritorio.
* **PIN cortos**: el limitador (5/min/IP) es la única defensa; el restaurante debe unificarlos.
* **Impresión en el hilo de UI**: una impresora colgada bloquea la interfaz del escritorio; el `409` evita
  encolar dos a la vez, pero no un driver que se quede esperando.
* **`sp_Cancelar_pedido` pisa las observaciones** del pedido: se pierde la nota original.
* **`sp_Cambiar_estado_pedido` reescribe `Id_mesa`**: la API le pasa siempre la mesa actual.
* La copia local contiene **datos personales y ventas reales**: no se versiona ni se comparte.

---

## 10. Pendientes, en orden de prioridad

### P0 — para que el restaurante lo pueda usar

1. **Red del PC (D7):** IP fija, `netsh http add urlacl` para el prefijo `http://+:8099/api/v1/`, regla de
   firewall (perfil privado) y `ApiKey` propia en el `App.config` que se publica.
2. **Despliegue en producción:** copia de seguridad, ejecutar `01`, `03`, `04` y `02` (arreglo de datos), y activar
   `ApiHabilitada=true`. *Criterio de terminado:* un teléfono en el WiFi del restaurante crea un pedido y el
   escritorio lo muestra.
3. **APK de release firmado** (keystore + `signingConfig`) e instalación directa en los teléfonos.

### P1 — calidad y cierre con el cliente

4. Imprimir con la **impresora física** (papel, márgenes, copias) y decidir si la comanda sale sola al crear.
5. Cerrar **D2** (solo mesa), **D8** (dispositivos) y los PIN cortos / caso `SANTIAGO`.
6. Pruebas de widgets, de Drift y de la matriz de fallos del envío (red/401/409/422/503).
7. Prueba de carga ligera (5–10 pedidos concurrentes) y revisión de tiempos.

### P2 — documentación y extras

8. Colección Postman y **manual de operación** (`docs/08-manual-operacion.md`): arranque, IP, puerto, clave,
   diagnóstico y qué hacer si la API no arranca.
9. Demo con el cliente y acta de aceptación.
10. Opcionales que hoy no están: imágenes del menú, QR de emparejamiento, TLS, domicilios (D2 opción B).

---

## 11. Cómo levantarlo y probarlo

```powershell
# 1. Copia local de producción (una vez; ~80 s)
cd D:\Repositorios\System-RestaurantCasaGrande\db
.\Restore-ProdCopy.ps1 -Force

# 2. Objetos nuevos y arreglo de datos (en la copia)
.\Invoke-Sql.ps1 -File .\01_sp_Movil_Insertar_pedido.sql
.\Invoke-Sql.ps1 -File .\03_sp_Movil_Agregar_items.sql
.\Invoke-Sql.ps1 -File .\04_sp_Movil_Quitar_item.sql
.\Invoke-Sql.ps1 -File .\02_unificar_empleados_duplicados.sql

# 3. Compilar el escritorio y abrirlo con la API en localhost
cd ..\SISMistico
.\run-local.ps1 -Rebuild

# 4. Pruebas de extremo a extremo (arrancan y cierran el .exe ellas mismas)
cd ..\scripts
.\Test-ApiFase1.ps1 ; .\Test-ApiFase2.ps1 ; .\Test-ApiFase3.ps1 ; .\Test-ApiFase4.ps1

# 5. App: instalar en el teléfono por USB
cd D:\Repositorios\CasaGrandeMesero\app
D:\DevTools\flutter-3.47.8\bin\flutter.bat run -d <id-del-dispositivo>
#    y en la app: servidor localhost, puerto 8099, la misma ApiKey del .exe.config
adb reverse tcp:8099 tcp:8099
```

---

## 12. Bitácora de esta etapa

| Fecha | Hito |
|---|---|
| 2026-10-24 | Análisis, contrato, esquema y copia de producción; fases 1–3 de la API (44/65/78 pruebas); app Flutter escrita; login solo por PIN |
| 2026-10-26 (mañana) | Primera prueba en el S24 Ultra por USB: conexión, login, pedido, sin conexión y reenvío. Tres hallazgos: botón bajo la barra del sistema, mapa sin refrescar tras la bandeja, selector de PIN con fichas duplicadas |
| 2026-10-26 (tarde) | Corrección de los tres hallazgos; arreglo de empleados en la BD; **fase 4** de la API (54/54) y sus pantallas en la app; impresión real con PDF en Documentos; `$` al inicio en todos los precios |

Los documentos se actualizan cuando cambie el estado; este es el punto de entrada para saber **cómo está hoy**.
