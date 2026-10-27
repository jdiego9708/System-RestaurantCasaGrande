# 00 — Estado del proyecto (2026-10-26, cierre del día)

Foto del proyecto el **26 de octubre de 2026**: qué está construido, qué está verificado, qué falta y por qué.
El detalle de cada tema vive en los documentos enlazados; aquí está el mapa.

> **Alcance actual:** que un mesero tome pedidos **de mesa** desde un teléfono Android en la red local del
> restaurante, contra una API que corre dentro del `.exe` del escritorio, con las comandas y la precuenta saliendo
> por la impresora del PC. Facturar, caja, inventario y reportes siguen siendo del escritorio.
>
> **Lo que NO está en producción:** ni la API, ni los tres SP nuevos, ni el arreglo de empleados. Todo está
> probado contra la **copia local de producción** (`CasaGrandeBD_prod`) y en esta máquina de desarrollo. Ver
> [§6](#6-base-de-datos-qué-hay-en-la-copia-y-qué-falta-en-producción) y [§10](#10-pendientes-en-orden-de-prioridad).
>
> **Para instalarlo en el restaurante:** [08 — Instalación desde cero](08-instalacion-desde-cero.md).

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
| **Impresión de adiciones** (solo lo nuevo) y **precuenta** | ✅ Probada (38/38, PDF reales) | `scripts/Test-ApiAdiciones.ps1` |
| App móvil (Android) | ✅ Escrita; fases 1–4 probadas en un S24 Ultra | [`app-movil/09`](app-movil/09-estado-de-la-implementacion.md) |
| App: buscar servidor en la red y reconexión automática | ✅ Probada contra el servidor real (sin teléfono) | `ServerScanner`, `ServerRelocator` |
| App: casilla «Imprimir precuenta», horas AM/PM, ícono nuevo | ✅ Compila y prueba la parte de servidor; **sin ver en teléfono** | [`app-movil/09`](app-movil/09-estado-de-la-implementacion.md) |
| Escritorio: indicador «N DISPOSITIVOS» en la pantalla de Mesas | ✅ Compila; **no se ha visto en pantalla** | `ApiAuth`, `FrmObservarMesas` |
| Instalador real (`Setup.exe`, Inno Setup) | 🟡 Compila; **no ejecutado en un PC real** | [`installer/`](../installer/README.md) |
| Kit USB numerado (`Preparar-USB.ps1`) | ✅ Probado con carpeta y listando USB; **no escrito en una USB real** | [`scripts/`](../scripts/README.md) |
| Datos de empleados (PIN único por persona) | ✅ En la copia | [`db/02`](../db/02_unificar_empleados_duplicados.sql) |
| Red del restaurante (urlacl, firewall, IP) | 🟡 Scripts listos; **sin ejecutar en el restaurante** (D7) | `scripts/Configurar-Red.ps1` |
| Despliegue en producción (SP + datos + API activa) | ⏳ **Pendiente** | `scripts/Desplegar-Base-de-Datos.ps1` lo guía; requiere aprobación y respaldo |
| Impresión en impresora **física** (comandas y precuenta) | ⏳ Pendiente | La ruta de PDF sí está probada |
| APK firmado de release | ⏳ Pendiente | Hoy firmado con la clave de depuración (sirve para instalar directo, no para Google Play) |

---

## 2. Dónde está el código

| Repositorio | Ramas | Estado |
|---|---|---|
| `D:\Repositorios\System-RestaurantCasaGrande` (GitHub `jdiego9708/System-RestaurantCasaGrande`) | `main` y `feature/api-movil` en `1055700`, **publicadas** | Hay trabajo posterior **sin confirmar** (ver abajo) |
| `D:\Repositorios\CasaGrandeMesero` | `main` en `c87aa20` | **Sin remoto configurado** (no se ha podido subir a ningún sitio) y con trabajo posterior sin confirmar |

Commits de esta etapa del escritorio (del más viejo al más nuevo): `a1a0331` diseño y contrato · `3304af2` esquema
y herramientas de BD · `654267e` pruebas fases 1–3 · `8ebf25b` API embebida (fases 1–3) · `22fb271` login solo por PIN
· `fa37674` unificación de empleados · `4693037` fase 4 · `c827245` impresión con PDF automático · `0380361` refresco
del mapa · `0014cf8` estado del proyecto · `3fc7a89` indicador de dispositivos, copias de comanda, instalador y red ·
`1055700` guía de instalación y verificación de la BD.
App: `b284b43`, `ea22ca2`, `01fb80d`, `5048bd4`, `721b768`, `c87aa20` (comanda siempre activa con copias).

**Trabajo posterior, aún sin confirmar** (escritorio): impresión de adiciones y de precuenta (`ApiImpresion`,
`ApiPedidosAcciones`, `FrmFacturaFinal.GuardarPdf`), `Preparar-USB.ps1`, `Desplegar-Base-de-Datos.ps1` (con reemplazo
de la base antigua), `Test-ApiAdiciones.ps1`, correcciones a las pruebas de las fases 3 y 4, y esta documentación.
(App): búsqueda de servidor y reubicación, casilla de precuenta, copias al agregar, horas AM/PM y pantalla de
registros propia, ícono.

---

## 3. Qué está hecho, fase por fase

### Fase 0 — Preparación ✅ (salvo red en el restaurante)

* Esquema de producción versionado (25 tablas, 45 SPs) y copia local restaurable (`db/Restore-ProdCopy.ps1`, ~80 s).
* Build publicada `1.0.0.108` = código del repo (2.871 métodos con IL idéntico).
* **Falta:** ejecutar la red en el PC del restaurante (D7) y conocer la versión y *collation* de su SQL Server.

### Fase 1 — Esqueleto del servidor ✅

`HttpListener` en el hilo de fondo, rutas por plantilla, `X-Api-Key`, tokens en memoria (8 h), límite de
5 intentos/min por IP, solo IP privadas, log diario sin credenciales, y `/ping` + `/auth/login|me|logout`.
El login **no** usa `sp_Login` (ese SP cierra y abre turnos) y acepta **solo PIN**: si lo comparten varios
empleados responde `409 PIN_AMBIGUO` con los candidatos. El `/ping` es público y devuelve el **nombre del PC**
(`servidor`), que la app usa para reconocer el servidor aunque cambie su IP.

### Fase 2 — Lecturas ✅

Tipos de platos y bebidas, platos (solo `ACTIVO`, como el escritorio), bebidas, ingredientes, clientes, mapa
de mesas del día (una fila por `Num_mesa`, con pedido abierto y total) e imágenes con validación de ruta.
Hallazgos vigentes: **ninguna imagen existe** (`imagen_url` siempre `null` hasta que el restaurante las cargue)
y la mesa 21 tiene un `Id_mesa` duplicado (se usa el canónico 5150).

### Fase 3 — Alta de pedido ✅

`POST /pedidos` atómico e idempotente con `sp_Movil_Insertar_pedido`: una fila por ítem, precio **del catálogo**
(el del cliente se ignora), ingredientes ligados a su línea y `MESA_OCUPADA`/`422` según corresponda. Acepta
`imprimir_comandas` y `copias_comandas` (1–5). Probado con doble toque simultáneo y dos claves distintas.

### Fase 4 — Operaciones sobre el pedido ✅

| Endpoint | Comportamiento |
|---|---|
| `GET /pedidos?fecha&estado&mesa` | resumen del día con total y número de líneas |
| `POST /pedidos/{id}/items` | fila nueva por ítem (`sp_Movil_Agregar_items`); solo `PENDIENTE`/`SALIENDO`. Con `imprimir_comandas` imprime **solo lo agregado ahora** (`copias_comandas` 1–5) |
| `DELETE /pedidos/{id}/items/{id_detalle_pedido}?clave_maestra=&cantidad=` | por id de línea; el PIN de **cualquier** empleado autoriza (como `FrmComprobacion`); al eliminar la línea borra sus ingredientes |
| `PATCH /pedidos/{id}/estado` | solo `PENDIENTE → SALIENDO`; conserva la mesa. Con `imprimir_precuenta` imprime la **precuenta** como el botón del escritorio |
| `POST /pedidos/{id}/cancelar` | motivo obligatorio (1–200) |
| `POST /pedidos/{id}/imprimir` | `PLATOS`/`BEBIDAS`/`TODO`, 1–5 copias, `202`; `409` si ya hay una en curso |

**Impresión** (real): usa la impresora predeterminada de Windows y, si esa impresora es de PDF (Microsoft Print to
PDF, Adobe PDF…), guarda el archivo sola en **`Documentos\Casa Grande\Comandas`** sin abrir el diálogo de
"Guardar como" (carpeta configurable con `ApiComandasCarpeta`). Detalles que importan:

* **Adiciones:** el servidor calcula las líneas nuevas comparando los `id_detalle_pedido` antes y después de agregar
  (sin tocar la base) y solo esas salen en la comanda, rotulada «(ADICION)»; si solo se agregaron bebidas no sale
  comanda de platos.
* **Precuenta:** mismo reporte `Precuenta.rdlc` y mismos valores por defecto que el escritorio (propina 0, descuento
  0 %, sin cupón/desechables/domicilio; total = suma de las líneas).
* **La impresión automática (crear, agregar, precuenta) ya no descarta trabajos:** si hay otra en curso, espera su
  turno hasta 90 s. La manual (`/imprimir`) sigue respondiendo `409`.

**Total de endpoints: 22.** No existen `POST /clientes` ni `GET /mesas/{id}/pedido`.

### Escritorio — cambios visibles

* **Indicador «N DISPOSITIVOS»** junto a DISPONIBLE · OCUPADA · SALIENDO en la pantalla de Mesas: IP con una petición
  autenticada en los últimos 90 s, con tooltip (empleado, IP, segundos); se oculta si la API está apagada. El botón
  «Domicilio» (oculto en la v1) se movió a su lado.

---

## 4. La app móvil (Android)

Solo Android (7.0 o superior), Flutter 3.47.8 / Dart 3.13.7 (instalado en `D:\DevTools\flutter-3.47.8`; **el
`flutter` del `PATH` es el 3.11.5 y no sirve**), Riverpod 3, go_router, Drift, Dio. Workspace con dos paquetes:
`packages/casa_grande_api` (cliente tipado y escáner de red) y `app` (feature-first: `auth`, `server_setup`, `menu`,
`cart`, `tables`, `orders`, `settings`, `core`).

| Pantalla | Qué hace |
|---|---|
| Configurar servidor | **Buscar servidor en la red** (barrido de la /24 por `/ping`) o IP/nombre + puerto + clave, con prueba real (`/ping` + clave + BD). Si la IP del PC cambia, la app lo vuelve a encontrar sola por su **nombre** y valida la clave |
| Login | Teclado de PIN; si el PIN es ambiguo, selector con aviso de fichas duplicadas |
| Mesas | Mapa del día con estados, borrador local y pedidos "por enviar" |
| Menú | Búsqueda, pestañas Platos/Bebidas, configurador de los 4 ingredientes |
| Confirmación del pedido | Comensales, nota, **comanda en cocina siempre activada** y control de **copias** (2 por defecto, 1–5) |
| Detalle del pedido | Líneas, total y acciones: **agregar productos** (imprime solo lo nuevo, con control de copias), **precuenta** (casilla «Imprimir precuenta», marcada por defecto), quitar con PIN, cancelar, imprimir |
| Bandeja (outbox) | Pedidos sin enviar, reintento con la misma clave de idempotencia |
| Ajustes | Servidor, sesión, tema, actualizar menú, registros (pantalla propia, horas AM/PM) |

* **Horas:** toda hora que muestra la app es de **12 horas con AM/PM** (`6:53 PM`) y sale de `formatTime12h`; una
  prueba guardiana falla si alguien arma una hora a mano.
* **Ícono:** cloche (cúpula de servir) crema con destellos dorados sobre degradado terracota; ícono adaptable de
  Android, monocromático para íconos temáticos (Android 13+) y clásico para Android 7. Se regenera con
  `app/tool/generate_launcher_icon.py`.
* Los precios se muestran como **`$ 5.000`** (símbolo delante) en toda la app.

Verificado en un **Samsung S24 Ultra** (Android 16) hasta la fase 4: conexión, login, mapa, alta de pedido con
ingredientes, refresco del escritorio, modo sin conexión, agregar/quitar líneas, `SALIENDO`, cancelar, imprimir a PDF.
**Lo nuevo de hoy no se ha probado en un teléfono:** búsqueda y reubicación del servidor, copias al agregar, casilla de
precuenta, horas AM/PM, pantalla de registros e ícono.

---

## 5. Configuración y operación

| Pieza | Dónde |
|---|---|
| Claves de la API | `SISMistico/SISMistico/App.config` (`ApiHabilitada`, `ApiPrefijo`, `ApiKey`, `ApiTokenHoras`, `ApiMaxItems`, `ApiImprimirComandas`, `ApiComandasCarpeta`, `ApiLogCarpeta`, `ApiImagenesCarpeta`). El instalador las activa y define la clave en cada equipo |
| Arranque local | `SISMistico\run-local.ps1` (`-Rebuild`, `-NoRun`, `-UseDevDb`, `-NoApi`, `-ApiPrefix`) |
| Log de la API | `…\logs\api-yyyyMMdd.log` (instalado: `C:\Program Files\SISRestaurante\logs\`) |
| Copia de producción | `db\Restore-ProdCopy.ps1` → `(localdb)\MSSQLLocalDB` · `CasaGrandeBD_prod` |
| Ejecutar SQL | `db\Invoke-Sql.ps1 -File <script> [-Database …]` |
| Instalador | `installer\Publicar-Instalador.ps1` → `installer\Output\SISRestaurante-Setup-<versión>.exe` |
| Kit USB | `scripts\Preparar-USB.ps1` → `Casa Grande Installers\` (numerado en orden de ejecución) |
| APK de la app | `app\build\app\outputs\flutter-apk\app-release.apk` (~63 MB) |

**Dos bases en esta máquina:** `CasaGrandeBD` es la base **antigua de desarrollo** (esquema sin
`Detalle_pedido.Id_detalle_pedido`) y la que usa la app instalada aquí; `CasaGrandeBD_prod` es la copia de producción.
`Desplegar-Base-de-Datos.ps1` detecta el esquema antiguo, lista las bases y ofrece reemplazarlo con respaldo previo.

**Prueba por USB:** `adb reverse tcp:8099 tcp:8099` y en la app servidor `localhost:8099`. Por WiFi: ver
[08](08-instalacion-desde-cero.md).

---

## 6. Base de datos: qué hay en la copia y qué falta en producción

| Objeto | Tipo | En la copia | En producción |
|---|---|---|---|
| `sp_Movil_Insertar_pedido` | SP nuevo (aditivo) | ✅ | ❌ |
| `sp_Movil_Agregar_items` | SP nuevo (aditivo) | ✅ | ❌ |
| `sp_Movil_Quitar_item` | SP nuevo (aditivo) | ✅ | ❌ |
| Arreglo de empleados duplicados + PIN `0` | Datos | ✅ | ❌ |

Estado de la copia: **38.581 pedidos** (incluye los de las pruebas por teléfono), **24 empleados activos**,
**0 PIN compartidos** entre activos, 1 activo **sin PIN** y **7 activos con PIN de menos de 4 dígitos**.
Queda a decisión del restaurante el caso `SANTIAGO` (dos fichas con historia y PIN distinto: la unificación no
las toca a propósito).

Los tres SP y el arreglo son **aditivos y reversibles** (`.revert.sql`); `db/verificar-despliegue.sql` comprueba el
resultado (solo lectura) y `scripts/Desplegar-Base-de-Datos.ps1` hace el despliegue guiado con **respaldo obligatorio**.
Orden y detalle en [`db/README.md`](../db/README.md).

---

## 7. Verificación

| Suite | Casos | Resultado | Qué cubre |
|---|---|---|---|
| `Test-ApiFase1.ps1` | 44 | ✅ 44/44 | ping, clave, login por PIN/contraseña, 409 ambiguo, PIN 0, sesión, límite de intentos, log sin secretos |
| `Test-ApiFase2.ps1` | 65 | ✅ 65/65 | catálogo, mesas, imágenes y *path traversal*; cada dato contrastado con la BD |
| `Test-ApiFase3.ps1` | 78 | ✅ 78/78 | alta, precios de catálogo, ingredientes, idempotencia, concurrencia, 37 peticiones inválidas |
| `Test-ApiFase4.ps1` | 54 | ✅ 54/54 | listado, agregar/quitar con PIN, `SALIENDO`, cancelar, imprimir (PDF comprobado y borrado) |
| `Test-ApiAdiciones.ps1` | 38 | ✅ 38/38 | adiciones (solo lo nuevo, copias, bebida sola, sin imprimir, validaciones, simultáneas), precuenta (PDF real, estado, 400 sin cambiar estado), registro y limpieza |
| `flutter test` (app) | 26 | ✅ | dinero, borradores, errores, reubicador del servidor (13), formato de hora (6) y su guardiana |
| `flutter test` (paquete api) | 71 | ✅ | DTOs, cliente (incluye adiciones y precuenta), reintentos, escáner de red (15, con un servidor HTTP real) |
| Prueba contra el servidor **real** instalado | — | ✅ | barrido de la /24 en 9 s; IP vieja corregida en 1,5 s; con clave o nombre erróneos no cambia nada |
| `flutter analyze` | — | 19 y 14 avisos informativos preexistentes | sin errores ni avisos nuevos |
| `flutter build apk --release` · compilación del instalador | — | ✅ | APK de 63 MB; `Setup.exe` de ~10 MB |

Las fases 1 y 2 no se volvieron a ejecutar con los últimos cambios (no tocan ese código). Los scripts arrancan el `.exe`
real contra la copia, limpian lo que crean y verifican que los conteos vuelven a los iniciales. **Fases 3, 4 y Adiciones
aceptan `-Puerto`** para correr aunque haya una instancia instalada, y **solo cuentan y borran los PDF que ellas mismas
crean** (una limpieza anterior borraba por número de pedido y pudo llevarse PDF de pruebas ajenas).

---

## 8. Decisiones

### Cerradas

| # | Decisión |
|---|---|
| D1 | Esquema real de producción obtenido y versionado |
| D2 (parcial) | Alcance **solo mesas** (0 domicilios en 38.579 pedidos) — **falta el visto bueno formal** |
| D3 | Login **solo por PIN**; si se comparte, `409` con selector; PIN `0` no es credencial |
| D4 | Imprimir comandas por petición (`imprimir_comandas` + `copias_comandas`, 1–5); apagado por defecto en el servidor, pero la app del mesero lo envía siempre activo (2 copias por defecto) |
| D5 | Desde el móvil: crear y agregar libremente; quitar y cancelar con PIN; facturar **no** |
| D6 | SP transaccional nuevo en lugar de reutilizar el guardado no atómico del escritorio |
| D9 | Cliente opcional en mesa (`0 / NINGUNO`) |
| D10 | HTTP en la LAN (sin TLS) en la v1 |
| D11 | Aplazada: el bug del escritorio al quitar productos no afecta a la API (usa `Id_detalle_pedido`) |
| D12 | Empleados `INACTIVO` pueden entrar (paridad con el escritorio), con log |
| D13 | Catálogo = solo `Estado = 'ACTIVO'` |
| D14 | Platos detallados exigen los 4 tipos de ingrediente, también en carta |
| D15 | **Adiciones:** al agregar productos se imprime **solo lo nuevo**, nunca el pedido completo |
| D16 | **Precuenta:** la imprime el escritorio con los valores por defecto de su botón; la casilla del móvil va marcada |
| D17 | **El móvil encuentra al servidor por su nombre** (barrido de la /24), no por una IP fija: no hace falta acceso al router |
| D18 | Horas del móvil siempre en 12 h con AM/PM |

### Abiertas

| # | Qué falta |
|---|---|
| D2 | Visto bueno formal a "solo mesa" |
| D7 | Ejecutar en el restaurante: `urlacl`, firewall, perfil de red y `ApiKey` definitiva (scripts listos) |
| D8 | Cuántos dispositivos usarán el móvil a la vez (para la prueba de carga) |
| — | PIN de menos de 4 dígitos (7 activos) y fichas `SANTIAGO` |

---

## 9. Riesgos y limitaciones conocidas

* **Los SP y el arreglo de datos no están en producción.** Hasta desplegarlos, la API solo funciona contra la copia.
* **Sin TLS** (D10): la API debe quedarse en la LAN; nunca exponer el puerto a internet.
* **Tokens en memoria**: reiniciar el `.exe` invalida todas las sesiones (los meseros deben volver a entrar).
* **Autorización por PIN, sin roles**: cualquier PIN válido permite quitar líneas; es la paridad con el escritorio.
* **PIN cortos**: el limitador (5/min/IP) es la única defensa; el restaurante debe unificarlos.
* **Impresión en el hilo de UI**: una impresora colgada bloquea la interfaz del escritorio. Los trabajos automáticos
  esperan su turno (90 s) y luego se descartan con un error en el log.
* **Impresión física sin probar:** comandas y precuenta solo se verificaron contra impresora PDF. La precuenta usa el
  mismo reporte del escritorio; falta compararla impresa en papel.
* **Descubrimiento del servidor** asume una red /24 (la habitual) y que celular y PC estén en la misma red.
* **Instalador y APK sin firma de editor**: SmartScreen y Play Protect avisan; se aceptan a mano.
* **El instalador no se ha ejecutado en un PC real**, ni se ha probado actualizar/desinstalar.
* **`sp_Cancelar_pedido` pisa las observaciones** del pedido: se pierde la nota original.
* **`sp_Cambiar_estado_pedido` reescribe `Id_mesa`**: la API le pasa siempre la mesa actual.
* La copia local contiene **datos personales y ventas reales**: no se versiona ni se comparte.
* La carpeta `Documentos\Casa Grande\Comandas` puede mezclar PDF de pruebas con reales (los nombres usan el número de
  pedido, que se repite entre la copia y la base antigua de desarrollo).

---

## 10. Pendientes, en orden de prioridad

### P0 — para que el restaurante lo pueda usar

1. **Probar el instalador en un PC real** (instalar, actualizar, desinstalar) y la USB `Casa Grande Installers`.
2. **Despliegue en producción:** `01-Base-de-Datos.ps1` (respaldo, `01`, `03`, `04`, `02`) y activar la API desde el
   instalador. *Criterio de terminado:* un teléfono en el WiFi del restaurante crea un pedido, le agrega productos,
   pide la precuenta y el escritorio lo muestra y lo imprime.
3. **Probar el APK de release en un teléfono** (lo nuevo de hoy) y decidir si se firma con *keystore* propio.
4. **Confirmar los cambios** de ambos repositorios y dar de alta un remoto para `CasaGrandeMesero`.

### P1 — calidad y cierre con el cliente

5. Imprimir con la **impresora física** (papel, márgenes, copias, precuenta) y decidir si la comanda sale sola.
6. Cerrar **D2** (solo mesa), **D8** (dispositivos) y los PIN cortos / caso `SANTIAGO`.
7. Pruebas de widgets, de Drift y de la matriz de fallos del envío (red/401/409/422/503).
8. Prueba de carga ligera (5–10 pedidos concurrentes) y revisión de tiempos.

### P2 — documentación y extras

9. Colección Postman y demo con el cliente con acta de aceptación.
10. Opcionales que hoy no están: imágenes del menú, QR de emparejamiento, TLS, domicilios (D2 opción B), reimprimir
    la precuenta de un pedido que ya está `SALIENDO`.

---

## 11. Cómo levantarlo y probarlo

```powershell
# 1. Copia local de producción (una vez; ~80 s) y objetos nuevos
cd D:\Repositorios\System-RestaurantCasaGrande\db
.\Restore-ProdCopy.ps1 -Force
.\Invoke-Sql.ps1 -File .\01_sp_Movil_Insertar_pedido.sql
.\Invoke-Sql.ps1 -File .\03_sp_Movil_Agregar_items.sql
.\Invoke-Sql.ps1 -File .\04_sp_Movil_Quitar_item.sql
.\Invoke-Sql.ps1 -File .\02_unificar_empleados_duplicados.sql

# 2. Compilar el escritorio y abrirlo con la API en localhost
cd ..\SISMistico
.\run-local.ps1 -Rebuild

# 3. Pruebas de extremo a extremo (arrancan y cierran el .exe ellas mismas; -Puerto si hay una instancia instalada)
cd ..\scripts
.\Test-ApiFase1.ps1 ; .\Test-ApiFase2.ps1 ; .\Test-ApiFase3.ps1 -Puerto 8199 ; .\Test-ApiFase4.ps1 -Puerto 8199
.\Test-ApiAdiciones.ps1      # imprime a PDF: exige Microsoft Print to PDF como predeterminada

# 4. App: pruebas y APK (con el Flutter correcto)
$env:Path = 'D:\DevTools\flutter-3.47.8\bin;' + $env:Path
cd D:\Repositorios\CasaGrandeMesero\app ; flutter test ; flutter build apk --release

# 5. Instalador y kit para la USB
cd D:\Repositorios\System-RestaurantCasaGrande\scripts
.\Preparar-USB.ps1 -Compilar      # recompila instalador y APK y los copia, numerados, a la USB que elija
```

---

## 12. Bitácora

| Fecha | Hito |
|---|---|
| 2026-10-24 | Análisis, contrato, esquema y copia de producción; fases 1–3 de la API (44/65/78 pruebas); app Flutter escrita; login solo por PIN |
| 2026-10-26 (mañana) | Primera prueba en el S24 Ultra por USB: conexión, login, pedido, sin conexión y reenvío. Tres hallazgos: botón bajo la barra del sistema, mapa sin refrescar tras la bandeja, selector de PIN con fichas duplicadas |
| 2026-10-26 (tarde) | Corrección de los tres hallazgos; arreglo de empleados en la BD; **fase 4** de la API (54/54) y sus pantallas en la app; impresión real con PDF en Documentos; `$` al inicio en todos los precios |
| 2026-10-26 (tarde-noche) | Comanda siempre activa con copias; indicador de dispositivos; **instalador real**, kit USB numerado, scripts de red y de base de datos (con reemplazo de la base antigua); guía de instalación; la app **encuentra el servidor por su nombre** y se reconecta sola; publicación de las ramas |
| 2026-10-26 (noche) | **Adiciones imprimen solo lo nuevo** y la impresión automática ya no descarta trabajos; **precuenta** impresa desde el móvil con casilla; horas en 12 h con AM/PM; pantalla de registros propia; **ícono nuevo**; corrección de las pruebas para que no borren PDF ajenos; documentación consolidada |

Los documentos se actualizan cuando cambie el estado; este es el punto de entrada para saber **cómo está hoy**.
