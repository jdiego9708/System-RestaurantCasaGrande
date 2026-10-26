# 03 — Propuesta de arquitectura: API local para la app móvil

> Objetivo de la fase 1: que un cliente Flutter pueda **consultar catálogo/mesas y crear pedidos** contra la
> aplicación WinForms actual, en la red local (LAN), persistiendo en el mismo SQL Server que usa el escritorio.
> Sin reescribir el sistema y sin romper lo que hoy funciona.

---

## 1. Principios de diseño

1. **No tocar la lógica existente.** `FrmPedido`, `DPedidos`, `NPedido` y los SPs actuales quedan como están.
   La API se agrega como una capa nueva.
2. **Un solo escritor de la BD.** El `.exe` del restaurante sigue siendo el único proceso que escribe; la API
   corre dentro de él y usa la misma cadena de conexión (`Conexion.Cn`).
3. **Aditivo, no sustractivo.** El único cambio en BD propuesto es un SP **nuevo**; no se modifican SPs existentes.
4. **Todo lo nuevo es tolerante a fallos.** Si la API se cae o se desactiva, el escritorio debe seguir operando igual.
5. **Contrato congelado y versionado** (`/api/v1`) para que Flutter avance en paralelo.
6. **LAN únicamente.** Sin exposición a internet. Seguridad por token + clave compartida + allowlist de IP.

---

## 2. Decisión: API embebida en el `.exe`

```
┌──────────────────────── PC del restaurante (Windows, 192.168.x.x) ─────────────────────────┐
│                                                                                             │
│  SISMistico.exe  (.NET Framework 4.8, WinForms)                                             │
│                                                                                             │
│   ┌───────────────────────────┐        ┌──────────────────────────────────────────────┐    │
│   │ UI actual (sin cambios)    │        │ CapaPresentacion/Api   (NUEVA)                │    │
│   │                            │        │                                               │    │
│   │  FrmPrincipal              │◄──────►│  ApiServer  (HttpListener :8099)              │    │
│   │  FrmObservarMesas          │ Invoke │    ├─ Router + Json + Log                     │    │
│   │  FrmPedido                 │        │    ├─ AuthService    (tokens + clave maestra) │    │
│   │  FrmFacturarPedido         │        │    ├─ PedidosService (validación + alta)      │    │
│   │  FrmComandas               │        │    ├─ CatalogoService                        │    │
│   └───────────────────────────┘        │    ├─ MesasService                          │    │
│                                        │    └─ ImagenesService                        │    │
│   ┌───────────────────────────┐        │                                               │    │
│   │ CapaNegocio / CapaDatos    │◄───────┤  (reutiliza NPedido, NPlatos, NEmpleados…)    │    │
│   └───────────────────────────┘        └──────────────────────────────────────────────┘    │
│                 │                                          ▲                                │
│                 ▼                                          │                                │
│        SQL Server / LocalDB                                │                                │
│                                                            │  HTTP/JSON (LAN)               │
└────────────────────────────────────────────────────────────┼────────────────────────────────┘
                                                             │
                                              ┌──────────────┴───────────────┐
                                              │ App Flutter (fase 2)          │
                                              └──────────────────────────────┘
```

### 2.1 Por qué embebida

| Ventaja | Detalle |
|---|---|
| Reutiliza la configuración | La cadena de conexión ya la elige el usuario en `App.config` (`ConexionBaseDeDatos`) |
| Un solo escritor | Evita bloqueos y condiciones de carrera entre dos procesos sobre la misma BD local |
| Coherencia de sesión | La API puede refrescar la pantalla del escritorio y disparar la impresión de comandas |
| Despliegue mínimo | Es el mismo `.exe`; no hay servicio, instalador ni tarea programada nuevos |
| Aislamiento | Si el listener falla, se registra el error y el escritorio continúa |

### 2.2 Costos y alternativas

| Costo | Mitigación |
|---|---|
| Concurrencia de hilos dentro de un proceso WinForms | `SemaphoreSlim` para escrituras y `BeginInvoke` para UI |
| El proceso debe estar abierto | Ya lo está durante el horario del restaurante; documentar arranque automático |
| Un crash tumba la API | Log + reinicio programado/operación (ya es un riesgo actual del escritorio) |

**Alternativa descartada (por ahora):** un servicio/console app separado que referencie `CapaNegocio`/`CapaDatos`.
Ventaja: arranca solo con Windows y no depende de la UI. Desventaja: duplica configuración, compite por la BD
local y **no puede** refrescar la UI ni imprimir la comanda. Si más adelante se quiere desacoplar, esta capa `Api`
está diseñada para poder moverse a un proceso propio con cambios mínimos (solo el arranque y el refresco de UI).

---

## 3. Componentes nuevos

Todo dentro de `SISMistico/CapaPresentacion/Api/` (namespace `CapaPresentacion.Api`), sin tocar archivos existentes
salvo los 3 puntos indicados en §3.2.

| Archivo | Responsabilidad |
|---|---|
| `ApiServer.cs` | Ciclo de vida del `HttpListener`, hilo de fondo, `CancellationTokenSource`, arranque/parada |
| `ApiRouter.cs` | Tabla de rutas `(método, plantilla)` → handler; extracción de parámetros de ruta/query |
| `ApiRequest.cs` / `ApiResponse.cs` | Contexto de request (body, headers, token) y respuesta (status, JSON) |
| `ApiAuth.cs` | Login, tokens en memoria con expiración, validación de `X-Api-Key` y `Authorization` |
| `ApiPedidosService.cs` | Validaciones del pedido, recálculo de precio, idempotencia, alta y operaciones sobre líneas |
| `ApiCatalogoService.cs` | Platos, bebidas, tipos, ingredientes, clientes |
| `ApiMesasService.cs` | Estado de mesas del día (reutiliza `sp_Buscar_pedidos 'MESAS CON PEDIDO'`, correcto en producción, o una consulta propia si hacen falta total/hora) |
| `ApiImagenesService.cs` | Sirve archivos de `RUTAIMAGES` con protección de *path traversal* |
| `ApiModels.cs` | DTOs de request/response (POCOs simples) |
| `ApiJson.cs` | Serialización/deserialización y helpers |
| `ApiLog.cs` | Log diario en `logs\api-YYYYMMDD.log` |
| `ApiConfig.cs` | Lectura de claves de `App.config` con valores por defecto |

### 3.1 Capa de datos/nuevos métodos

| Archivo | Responsabilidad |
|---|---|
| `CapaDatos/DPedidosMovil.cs` | Acceso al SP transaccional de alta (`sp_Movil_Insertar_pedido`) |
| `CapaNegocio/NPedidoMovil.cs` | Fachada del anterior (mismo estilo que `NPedido`) |
| `db/01_sp_Movil_Insertar_pedido.sql` | Script **aditivo** del SP (ver [07 §A](07-anexos-tecnicos.md#a-sp-transaccional-propuesto)) |

### 3.2 Modificaciones mínimas a archivos existentes

| Archivo | Cambio | Líneas aprox. |
|---|---|---|
| `SISMistico/Program.cs` o `FrmPrincipal.cs` | Arrancar/detener `ApiServer` (según configuración) | 3–6 |
| `SISMistico/App.config` | Nuevas claves `Api*` (§7) | 5–8 |
| `CapaPresentacion/CapaPresentacion.csproj` | Incluir los archivos nuevos y, si se usa, el paquete JSON | generado por IDE |

> No se modifica `FrmPedido` ni ningún SP existente.

---

## 4. Estrategia de persistencia

### 4.1 Comparación

| Criterio | Opción A — reutilizar lo actual | Opción B — SP nuevo transaccional (**recomendada**) |
|---|---|---|
| Cambios | Ninguno | 1 SP nuevo + 1 clase DAL + 1 clase N |
| Atomicidad | ❌ `2 + N` viajes sin transacción | ✅ una transacción |
| Duplicados por reintento | No controlados | Controlados (idempotencia) |
| Precio | Lo manda el cliente | Se toma del catálogo en el SP |
| Inyección SQL en observaciones | Riesgo (SQL concatenado) | Eliminado (parametrizado) |
| Impacto en el escritorio | Nulo | Nulo |
| Requisito | — | SQL Server 2016+ (`OPENJSON`): ✅ producción tiene nivel de compatibilidad 150 |
| Esfuerzo | Bajo | Medio |

**Decisión propuesta:** **B** para `POST /pedidos`; **A** solo para lecturas y para los cambios de estado y la
cancelación (`sp_Cambiar_estado_pedido`, `sp_Cancelar_pedido`), que existen, son simples y funcionan. **Agregar y
quitar ítems NO puede apoyarse tal cual en `sp_Actualizar_detalle_pedido`** (ver §4.3 y
[02 §3.2](02-base-de-datos-y-hallazgos.md)): se resuelve con filas nuevas y con `Id_detalle_pedido` explícito.

### 4.2 Flujo del alta con la opción B

```
POST /pedidos
  │
  ├─ 1. Autenticación: X-Api-Key + Bearer token          → 401
  ├─ 2. Clave maestra (PIN) → empleado válido            → 403   (¿exigir `Estado='ACTIVO'`? ver D12)
  ├─ 3. Idempotencia: ¿ya existe esta clave?             → 200 (devuelve el pedido previo)
  ├─ 4. Validaciones de negocio                          → 400/404/409/422
  │      · mesa existe y no tiene pedido PENDIENTE/SALIENDO del día
  │      · items no vacío, cantidades > 0
  │      · cada producto existe y está ACTIVO
  │      · reglas de domicilio (si aplica)
  ├─ 5. SemaphoreSlim (serializa escrituras de alta)
  ├─ 6. sp_Movil_Insertar_pedido (una transacción)
  │      · INSERT Pedidos (PENDIENTE, fecha/hora del servidor)
  │      · INSERT Detalle_pedido con Precio del catálogo
  │      · INSERT Detalle_ingredientes_pedido
  │      · devuelve @Id_pedido
  ├─ 7. Registrar idempotencia → id_pedido
  ├─ 8. Responder 201
  └─ 9. Post-respuesta (best effort): refrescar mesas en UI + imprimir comanda si se pidió
```

### 4.3 Semántica de líneas (importante)

Corregido el 2026-10-24 tras probar `sp_Actualizar_detalle_pedido` en producción (la versión anterior asumía que
sumaba un delta; **no lo hace**: ver [02 §3.2](02-base-de-datos-y-hallazgos.md)).

* La línea se identifica por **`Id_detalle_pedido`**. Un mismo producto puede ocupar varias filas del pedido
  (el 31 % de los pedidos reales lo hace); `GET /pedidos/{id}` devuelve cada fila con su `id_detalle_pedido`.
* En un **pedido nuevo**, el SP transaccional crea **una fila por cada elemento de `items`**, en el orden recibido
  (no agrupa: dos almuerzos con ingredientes distintos son dos filas, como en el escritorio). Corregido en la fase 3.
* En un pedido **existente**, `POST /pedidos/{id}/items` **inserta filas nuevas** (equivale a
  `sp_Actualizar_detalle_pedido` con `@Id_detalle_pedido = 0`, o a `sp_Insertar_detalle_pedido`), igual que hace
  el escritorio al pulsar "+". No se intenta sumar a una fila existente: con `@Cantidad` menor que la cantidad actual
  el SP **resta 1** en vez de sumar.
* `DELETE /pedidos/{id}/items/{id_detalle_pedido}` quita **una unidad** de esa fila: llama al SP con el
  `@Id_detalle_pedido` real y `@Cantidad = cantidad_actual - 1` (si queda en 0, el SP borra la fila).
  El escritorio hoy no fija el id al quitar y el SP no hace nada con id 0 ([02 §5.14](02-base-de-datos-y-hallazgos.md)).

---

## 5. Autenticación y seguridad

| Elemento | Propuesta |
|---|---|
| Secreto de aplicación | `X-Api-Key` fija, definida en `App.config` (`ApiKey`). Si falta o no coincide → 401 |
| Login | `POST /auth/login` con `id_empleado` + (`password` **o** `codigo_maestra`). **Nunca solo el PIN** (hay PIN repetidos, [02 §5.16](02-base-de-datos-y-hallazgos.md)). La API valida con una consulta propia sobre `Empleados`; **no llama a `sp_Login`**, que cierra/abre turnos ([02 §5.18](02-base-de-datos-y-hallazgos.md)) |
| Token | GUID aleatorio en `ConcurrentDictionary<token, (empleado, expira)>`; expiración 8 h; se pierde al reiniciar el exe |
| Autorización por operación | Crear pedido y quitar líneas exigen `clave_maestra` (paridad con el escritorio) |
| Allowlist | Aceptar solo IP privadas (`10.x`, `172.16-31.x`, `192.168.x`) y opcionalmente una lista blanca explícita |
| Límites | Body ≤ 256 KB, timeout de lectura 10 s, máximo 200 ítems por pedido |
| Rate limit | Login: 5 intentos/minuto por IP |
| Logs | Sin contraseñas, sin tokens completos; sí `id_pedido`, endpoint, duración, IP y código de respuesta |
| Transporte | HTTP en LAN (sin TLS). Si se requiere TLS, `HttpListener` puede usar un certificado, pero Flutter tendría que confiar en él → fuera de alcance |
| Secretos en el repo | La `ApiKey` **no** se versiona con valor real: usar plantilla `ApiKey.example` o valor por defecto cambiado en el equipo |

> Nota: el sistema actual guarda contraseñas en texto plano. La API no empeora eso, pero tampoco debe exponer
> `Empleados.Password` en ninguna respuesta.

---

## 6. Hilos, UI e impresión

Reglas para la implementación:

1. El listener corre en un hilo de fondo (`Task.Run` + `GetContextAsync`); cada request se atiende en el
   thread-pool.
2. **Nunca** acceder a controles WinForms desde el listener. Para refrescar:
   ```csharp
   var frm = FrmObservarMesas.GetInstancia();
   if (frm.IsHandleCreated)
       frm.BeginInvoke((Action)(() => frm.ObtenerPedido(idPedido, numMesa, "PENDIENTE")));
   ```
3. La impresión de comandas (`FrmComandas` + ReportViewer) se ejecuta **después** de responder el HTTP y dentro
   de `BeginInvoke`, con try/catch propio.
4. Las conexiones a SQL se abren y cierran por operación (como ya hace `CapaDatos`); no compartir `SqlConnection`.
5. El alta de pedidos se serializa con `SemaphoreSlim(1,1)` para evitar condiciones de carrera con el escritorio.
6. `DatosInicioSesion` **no** se usa para autenticar peticiones de la API (es estado de la UI).

---

## 7. Configuración (`App.config`)

| Clave | Ejemplo | Descripción |
|---|---|---|
| `ApiHabilitada` | `true` | Arranca el listener al iniciar la app |
| `ApiPrefijo` | `http://+:8099/api/v1/` | Prefijo del `HttpListener` (requiere `urlacl`) |
| `ApiKey` | `cambiar-esta-clave` | Secreto compartido (`X-Api-Key`) |
| `ApiTokenHoras` | `8` | Vigencia del token |
| `ApiImprimirComandas` | `true` | Valor por defecto cuando el request no lo especifica |
| `ApiPermitirDomicilio` | `true` | Habilita `tipo_pedido = DOMICILIO` en el alta |
| `ApiLogCarpeta` | `logs` | Carpeta de logs (relativa al exe o absoluta) |
| `ApiMaxItems` | `200` | Límite de ítems por pedido |

Las claves nuevas son **opcionales**: si faltan, `ApiConfig` aplica valores por defecto y `ApiHabilitada=false`
para no exponer nada por accidente.

**Estado (2026-10-24):** `App.config` ya trae `ApiHabilitada=false`, `ApiPrefijo`, `ApiKey` (valor de ejemplo),
`ApiTokenHoras`, `ApiLogCarpeta`, `ApiMaxItems` (200) y `ApiImprimirComandas` (`false`). `ApiPermitirDomicilio` ya no
aplica (la v1 es solo mesa). `ApiImagenesCarpeta` es opcional (si falta se usa `RUTAIMAGES`). Si `ApiKey` conserva el valor de ejemplo, la API **solo arranca con un prefijo localhost**; con un prefijo de
red (`http://+:…`) se niega a iniciar y lo deja en el log.

`App.config` también incluye `conexionLocalBDProd` (la copia local de producción). **`ConexionBaseDeDatos` sigue
apuntando a `conexionLocalBD`**: ese archivo es el que se publica al restaurante por ClickOnce (la build desplegada
usa `(localdb)\MSSQLLocalDB` → `CasaGrandeBD`), así que cambiarlo ahí haría que la app instalada busque una BD que no
existe. `SISMistico/run-local.ps1` hace el cambio solo en el `.exe.config` de la compilación local.

---

## 8. Despliegue y red

### 8.1 Requisitos en el PC del restaurante

* Windows con .NET Framework 4.8 (ya lo tiene).
* SQL Server / LocalDB en ejecución (ya lo tiene).
* IP **fija** o reserva DHCP (ej. `192.168.1.20`).
* Puerto libre (sugerido **8099**; verificar con `netstat -ano | findstr :8099`).

### 8.2 Comandos de instalación (una vez, como administrador)

> **Atajo:** `scripts/Configurar-Red.ps1` hace lo de abajo y más. Usa el `sddl` de «Todos» en lugar de
> `user="Everyone"`, porque ese nombre no existe en Windows en español (allí es «Todos»).

```bat
:: Reserva de URL para que el listener funcione sin ejecutar como admin
netsh http add urlacl url=http://+:8099/ user="Everyone"

:: Regla de firewall para la LAN
netsh advfirewall firewall add rule name="SISRestaurante API (8099)" dir=in action=allow ^
       protocol=TCP localport=8099 profile=private

:: Verificación
netsh http show urlacl | findstr 8099
netstat -ano | findstr :8099
```

Si no se puede obtener permiso de administrador, alternativas:

1. Ejecutar la app “como administrador” (no recomendado por seguridad).
2. Reservar la URL solo para el usuario que corre la app:
   `netsh http add urlacl url=http://+:8099/ user="NOMBRE_EQUIPO\usuario"`.
3. Usar un servidor mínimo sobre `TcpListener` (sin reserva) — más código y más superficie de error.

### 8.3 Verificación desde el móvil o desde el PC

```powershell
# desde otro equipo de la LAN
curl http://192.168.1.20:8099/api/v1/ping

# ver la regla de firewall
Get-NetFirewallRule -DisplayName "*SISRestaurante*" | Format-List Name,Enabled,Direction,Action
```

### 8.4 Operación

* El listener arranca con la aplicación y se detiene al cerrarla (o con el botón de la UI, opcional).
* Si el restaurante reinicia el PC, la API deja de estar disponible hasta que se abra el sistema.
* Documentar en el manual de operación: IP, puerto, `ApiKey`, y qué hacer si el puerto está ocupado.

---

## 9. Observabilidad

* **Log diario** `logs\api-YYYYMMDD.log` con líneas:
  `2026-10-21 13:42:07 | POST /pedidos | 201 | 84 ms | ip=192.168.1.45 | id_pedido=1234`
* **Errores**: stack trace saneado + código de error; nunca credenciales ni el body completo.
* **`GET /ping`** devuelve estado del listener y de la BD (para monitoreo desde el móvil o un script).
* **Contadores en memoria** (opcional): requests, errores, último pedido creado; visibles en una pantalla de
  diagnóstico del escritorio.

---

## 10. Compatibilidad técnica

| Tema | Decisión |
|---|---|
| Framework | .NET Framework 4.8 (ya es el target del proyecto) |
| `HttpListener` | Disponible en `System.dll`; no requiere paquetes nuevos |
| JSON | **Newtonsoft.Json 13.x** (NuGet) — o `JavaScriptSerializer` (`System.Web.Extensions` ya referenciado) si se quiere cero dependencias |
| SQL | ADO.NET existente (`System.Data.SqlClient`); no se usa EF |
| `OPENJSON` | Requiere SQL Server 2016+; producción tiene nivel de compatibilidad 150 (2019+), así que está disponible. Falta solo anotar la versión exacta ([02 §7](02-base-de-datos-y-hallazgos.md)) |
| Impresión | ReportViewer 11 (`Microsoft.Reporting.WinForms.v11`, ya referenciado en el `.csproj`); solo hilo de UI |
| Flutter | Cliente HTTP nativo; no requiere CORS (no es web). Si más adelante se compila para web, agregar cabeceras CORS en el router |
| Instalación | Sin cambios en el instalador ClickOnce; la API viaja dentro del `.exe` |
