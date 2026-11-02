# 09 — Cierre de sesión (2026-10-26) y plan para retomar

Resumen para continuar mañana sin perder contexto: **dónde quedó todo, qué se hizo, qué se encontró por el camino,
qué falta y qué hay que decidir**. El mapa general del proyecto sigue siendo [00 — Estado del proyecto](00-estado-del-proyecto.md);
este documento es la bitácora detallada de la sesión y la lista de arranque.

---

## 0. Retomar en 5 minutos

**Se trabaja desde `main`** en ambos repositorios (acuerdo del 2026-10-26: `main` es la rama de trabajo).

| Repositorio | Rama | HEAD | Estado |
|---|---|---|---|
| `D:\Repositorios\System-RestaurantCasaGrande` | `main` (= `feature/api-movil`, sin diferencia) | `7011c4b`, publicado en GitHub | **Sin confirmar:** clave de la API personalizable (`installer/SISRestaurante.iss`, `scripts/Cambiar-Clave-API.ps1`, `scripts/Configurar-Red.ps1`) y documentación (`docs/00`, `08`, `09`, `README`, `installer/README.md`, `scripts/README.md`) |
| `D:\Repositorios\CasaGrandeMesero` | `main` | `0612f4e` | Limpio y **publicado** en `https://github.com/jdiego9708/CasaGrandeMesero` (privado, creado después del cierre con la sesión de GitHub de `jdiego9708`) |

**Primera acción de mañana:** confirmar y subir lo pendiente (commit + push de `System-RestaurantCasaGrande`: clave
personalizable, hora de la precuenta en 12 h y documentación). El remoto de `CasaGrandeMesero` **ya está dado de alta**.

**Herramientas (rutas de esta máquina):**

| Qué | Dónde |
|---|---|
| Flutter correcto (3.47.8 / Dart 3.13.7) | `D:\DevTools\flutter-3.47.8\bin` — el `flutter` del `PATH` es el 3.11.5 y **no sirve** |
| Inno Setup 6 (instalador) | `%LOCALAPPDATA%\Programs\Inno Setup 6\ISCC.exe` |
| MSBuild | VS 2022 Build Tools; las referencias de .NET 4.8 salen del paquete NuGet (`run-local.ps1` y `Publicar-Instalador.ps1` ya lo resuelven) |
| Python con Pillow y numpy | para `CasaGrandeMesero\app\tool\generate_launcher_icon.py` |
| Compilar escritorio | `SISMistico\run-local.ps1 -Rebuild -NoRun` |
| Instalador | `installer\Publicar-Instalador.ps1` → `installer\Output\SISRestaurante-Setup-1.0.0.exe` |
| Kit USB | `scripts\Preparar-USB.ps1 [-Compilar] [-Unidad K] [-Si]` |

**Artefactos al cierre** (no se versionan; se regeneran): `app-release.apk` 62,6 MB (19:26) y
`SISRestaurante-Setup-1.0.0.exe` 10,1 MB (19:40, con la clave personalizable). La USB (`K:`, "CHICOUSB") **no estaba
conectada** al cierre y su kit `Casa Grande Installers` tiene una versión **anterior** de todo: hay que refrescarla.

---

## 1. Qué se hizo hoy

### 1.1 Comanda siempre activa y copias
* Confirmación del pedido: la casilla «Imprimir comanda en cocina» queda marcada y bloqueada; control **Copias de la
  comanda** (2 por defecto, 1–5). Servidor: nuevo `copias_comandas` en `POST /pedidos`. Se eliminó el ajuste
  «Imprimir comanda por defecto».

### 1.2 Indicador de dispositivos (escritorio)
* «N DISPOSITIVOS» junto a DISPONIBLE · OCUPADA · SALIENDO: IP con una petición autenticada en los últimos 90 s, tooltip
  con empleado/IP/segundos, se oculta si la API está apagada. «Domicilio» (oculto) se movió a su lado.

### 1.3 Instalador real y kit USB (reemplazo de ClickOnce)
* **Inno Setup** → `Setup.exe` (~10 MB): instala en `Archivos de programa`, da escritura a los usuarios (el sistema
  reescribe su `.exe.config`), activa y configura la API (clave, puerto, `urlacl` por SID, firewall solo LAN),
  conserva el `.config` al actualizar y limpia la red al desinstalar. `Publicar-Instalador.ps1` compila en Release,
  verifica el `App.config` (BD local, API apagada) y copia a un destino.
* **`Preparar-USB.ps1`:** lista las USB, crea `Casa Grande Installers` con `00-LEEME.txt`, `01-Base-de-Datos.ps1`,
  `02-Setup-Casa-Grande.exe`, `03-Configurar-Red.ps1`, `04-APK-Casa-Grande.apk`, `Soporte\db\` y `SHA256SUMS.txt`
  (verifica cada copia por SHA-256 y avisa si el instalador/APK son más viejos que el código).
* **`Desplegar-Base-de-Datos.ps1`** (el `01-`): respaldo **obligatorio**, procedimientos `01/03/04`, arreglo de
  empleados `02` con confirmación aparte, verificación; si la base tiene el **esquema antiguo** se detiene, **lista las
  bases** y ofrece **reemplazarla** (copia de otra base con esquema de producción, respaldo `.sql` o vacía), siempre con
  copia previa y confirmando con la palabra `REEMPLAZAR`.
* **`Configurar-Red.ps1`:** `urlacl`, firewall, perfil de red, aviso de IP dinámica; pide administrador solo; lee la
  clave instalada. **`db/verificar-despliegue.sql`:** comprobación de solo lectura.
* **Guía** [08 — Instalación desde cero](08-instalacion-desde-cero.md): PC del restaurante + cada celular.

### 1.4 La app encuentra el servidor sola
* Botón «Buscar servidor en la red» (barrido de la /24 por `/ping`, 253 hosts, 48 a la vez) y **reubicación
  automática** si la IP del PC cambia: reconoce al PC por su **nombre** y valida la clave antes de cambiar nada, con
  espera creciente (45 s → 5 min). Sin cambios en el escritorio. Probado contra el servidor real: barrido en 9 s, IP
  vieja corregida en 1,5 s, con clave o nombre erróneos no cambia nada.

### 1.5 Impresión: adiciones y precuenta
* **Bug:** al agregar productos a un pedido ya enviado **no se imprimía nada**. Ahora se imprime **solo lo nuevo**
  (rotulado «ADICION»), con copias; el servidor detecta las líneas nuevas por diferencia de `id_detalle_pedido`.
* **La impresión automática ya no descarta trabajos** (antes, con otra impresión en curso, la comanda se perdía con
  `impreso: false`): espera su turno hasta 90 s.
* **Precuenta:** `PATCH /pedidos/{id}/estado` acepta `imprimir_precuenta`; el escritorio imprime la precuenta como su
  botón (reporte `Precuenta.rdlc`, valores por defecto: propina 0, descuento 0 %, total = suma de las líneas) y la guarda
  a PDF si la impresora es de PDF. En la app, la casilla **«Imprimir precuenta»** (marcada por defecto).
* `Test-ApiAdiciones.ps1`: **39/39** con PDF reales.

### 1.6 Horas en 12 h, ícono y registros
* Toda hora de la app en **12 h con AM/PM** (`formatTime12h`) con una prueba guardiana; pantalla de **registros propia**
  (la librería solo muestra 24 h).
* **Ícono** nuevo: cloche crema con destellos dorados sobre degradado terracota; adaptable + monocromático (Android 13+)
  + clásico (Android 7); generador reproducible y verificado dentro de la zona segura.

### 1.7 Clave de la API personalizable (última del día)
* La clave **la define el restaurante en el instalador** (no incrustada al compilar): aleatoria **fácil de teclear**
  (12 caracteres en minúscula sin `0/o/1/l/i`, botón «Generar otra») o **propia** de ≥ 8 caracteres sin espacios.
  `Cambiar-Clave-API.ps1` (menú Inicio) la cambia **después** sin reinstalar (con `.bak`). Una `/ApiKey` inválida en
  instalación silenciosa se rechaza. **Compila; el asistente no se ha ejecutado.**

### 1.8 Publicación
* `main` y `feature/api-movil` publicadas en `7011c4b`; `CasaGrandeMesero` confirmada localmente (`0612f4e`).

---

## 2. Hallazgos

### 2.1 De producto y datos
| # | Hallazgo | Impacto | Estado |
|---|---|---|---|
| 1 | Agregar productos no imprimía (solo la creación) | Cocina sin las adiciones | ✅ Arreglado (1.5) |
| 2 | La impresión automática descartaba la comanda si había otra en curso | Comandas perdidas con varios meseros | ✅ Arreglado: espera su turno |
| 3 | La base de producción **no tiene** los 3 SP móviles ni el arreglo de empleados | La API no puede crear pedidos allí | ⏳ Desplegar (`01-Base-de-Datos.ps1`) |
| 4 | La base de desarrollo antigua (`CasaGrandeBD`) no tenía `Detalle_pedido.Id_detalle_pedido`: el SP `01` fallaba con `Invalid column name` | Confusión al desplegar en la base equivocada | ✅ El script lo detecta antes de tocar nada y ofrece reemplazarla (se hizo a las 18:20; hoy `CasaGrandeBD` tiene esquema de producción y los 3 SP) |
| 5 | LocalDB crea la instancia **por usuario de Windows** | Restaurar con un usuario y abrir con otro = «la base no existe» | ✅ Documentado en 08 |
| 6 | El sistema **reescribe su propio `.exe.config`** (imágenes, conexión, correo, licencias) y escribe `logs\` junto al `.exe` | En `Archivos de programa` un usuario normal no podría guardar | ✅ El instalador da permiso de escritura a los usuarios |
| 7 | `netsh … user="Everyone"` falla en Windows en español | `urlacl` no se creaba | ✅ Se usa el SDDL de «Todos» |
| 8 | El `.exe.config` de `LocalBuild` está ajustado para desarrollo (copia de producción, API en localhost) | Un instalador hecho desde ahí saldría mal | ✅ `Publicar-Instalador.ps1` compila a `InstallerBuild` y exige BD local y API apagada |
| 9 | La numeración de pedidos se repite entre `CasaGrandeBD` y `CasaGrandeBD_prod` (parten del mismo punto) | Los PDF `comanda-<id>-…` pueden mezclar pruebas con reales | ✅ Las pruebas ya solo tocan los PDF que crean |
| 10 | Siete empleados activos con PIN de menos de 4 dígitos, uno sin PIN, y el caso `SANTIAGO` (dos fichas con historia) | Seguridad del login por PIN | ⏳ Decisión del restaurante |
| 11 | El escritorio imprimía la hora de la precuenta con la cultura de Windows (`ToShortTimeString`) | Podía salir en 24 h aunque el móvil use 12 h | ✅ Decidido y hecho: 12 h con AM/PM (D20). Las horas de comandas y factura siguen con el formato de Windows |

### 2.2 De entorno y herramientas
| # | Hallazgo | Estado |
|---|---|---|
| 12 | MSBuild a secas no encuentra las referencias de .NET 4.8 (falta el Developer Pack) | ✅ Se usa el paquete NuGet, como `run-local.ps1` |
| 13 | El `flutter` del `PATH` es el 3.11.5 y el proyecto exige 3.13 | ✅ Documentado; usar `D:\DevTools\flutter-3.47.8` |
| 14 | Inno Setup (Pascal Script) no admite `const` locales ni `GetTickCount`; el GUID aleatorio sale de `Scriptlet.TypeLib` | ✅ Resuelto en el `.iss` |
| 15 | PowerShell 5.1 necesita BOM UTF-8 para leer acentos; los `.cs` usan CRLF | ✅ Se respetan en las ediciones |
| 16 | `talker_flutter` solo muestra horas de 24 h | ✅ Pantalla de registros propia |
| 17 | `dart format` sobre carpetas enteras reformatea archivos ajenos; `build_runner` reflowea comentarios de `data_changed.g.dart` | Se formatea solo lo propio y se revierte ese archivo |
| 18 | El APK de depuración pesa 174 MB; el de release, 63 MB (firmado con la clave de depuración: sirve para instalar directo, no para Google Play; Play Protect avisa) | ⏳ Keystore propio (P1) |
| 19 | Las pruebas de las fases 3 y 4 se negaban a correr con **cualquier** escritorio abierto (la app instalada lo está siempre) | ✅ `-Puerto` y detección solo de la instancia de desarrollo |
| 20 | El descubrimiento asume una red /24 y mismo WiFi | Limitación aceptada (el usuario confirmó que siempre estarán en la misma red) |

### 2.3 Incidentes (errores míos durante la sesión) y lecciones
| # | Qué pasó | Qué se hizo / lección |
|---|---|---|
| 21 | Un comando con rutas relativas **vació por un momento** tres scripts (`Configurar-Red.ps1`, `SISRestaurante.iss`, `Publicar-Instalador.ps1`) | Se reescribieron completos y se recompiló. Lección: rutas absolutas y comprobar el tamaño antes de reescribir un archivo |
| 22 | La limpieza de `Test-ApiAdiciones.ps1` borraba PDF **por número de pedido** y se llevó PDF de un pedido real de otra base (los datos no se tocaron; el usuario indicó que no hacía falta recuperarlos: son pruebas) | Las tres pruebas con impresión **solo cuentan y borran los PDF creados durante su ejecución** |
| 23 | `Test-ApiFase4.ps1` borraba solo uno de los dos PDF que generaba | Corregido en el mismo cambio |
| 24 | `dart format` aplicado a carpetas y scripts largos pegados en línea fallaban por comillas del intérprete | Los parches largos se guardan en archivo; se formatea solo lo propio |

---

## 3. Decisiones

**Tomadas hoy** (detalle en [00 §8](00-estado-del-proyecto.md#8-decisiones)): D15 adiciones imprimen solo lo nuevo · D16 precuenta
con los valores por defecto del escritorio y casilla marcada · D17 el móvil encuentra al servidor por su nombre ·
D18 horas en 12 h con AM/PM · D19 la clave de la API la define el restaurante en el instalador (aleatoria fácil de
teclear o personalizada ≥ 8 caracteres) y se puede cambiar después · D20 la hora de la precuenta que imprime el
escritorio va en 12 h con AM/PM. Además: **se trabaja desde `main`**.

**Por confirmar** → ver [§5](#5-preguntas-para-confirmar).

---

## 4. Pendientes (en orden)

### P0 — antes de poner esto en el restaurante
1. **Confirmar y subir** lo pendiente de `System-RestaurantCasaGrande`. (El remoto de `CasaGrandeMesero` ya existe.)
2. **Instalar el `Setup.exe` nuevo en esta PC** (la app instalada es la versión **anterior**: no imprime adiciones ni
   precuenta, y no tiene la clave personalizable) y el **APK nuevo en el teléfono**.
3. **Probar en el teléfono todo lo de hoy** (checklist en [§6](#6-checklist-de-pruebas-manuales-para-mañana)).
4. **Probar el instalador en un PC real**: asistente (página de clave, «Generar otra», validación), instalación
   silenciosa, actualizar encima, desinstalar (¿quita `urlacl` y firewall?), `Cambiar-Clave-API.ps1` por el camino de
   «pedir administrador» (el UAC no se pudo ejecutar en la sesión).
5. **Refrescar la USB** (`Preparar-USB.ps1 -Compilar -Unidad K -Si`) cuando esté conectada.
6. **Despliegue en producción:** `01-Base-de-Datos.ps1` (respaldo → `01/03/04` → `02`) y activar la API desde el
   instalador. *Criterio de terminado:* un teléfono en el WiFi del restaurante crea un pedido, le agrega productos,
   pide la precuenta y el escritorio lo muestra y lo imprime.

### P1 — calidad
7. **Impresora física**: comandas, adiciones y precuenta en papel (márgenes, copias) y **comparar la precuenta con la
   del escritorio**. Todo lo verificado fue contra *Microsoft Print to PDF*.
8. APK con **keystore propio** (firma de release).
9. Cerrar **D2** (solo mesas), **D8** (cuántos celulares), PIN cortos y `SANTIAGO`.
10. Pruebas de widgets, de Drift y de la matriz de fallos del envío (red/401/409/422/503); prueba de carga ligera
    (5–10 pedidos a la vez).
11. Re-ejecutar las **fases 1 y 2** (no se repitieron tras los cambios de hoy; no tocan ese código).

### P2 — mejoras posibles
12. **Emparejamiento por QR**: el escritorio muestra un QR con servidor y clave y el celular lo escanea (evita teclear la
    clave). Ya figura como opcional en el diseño.
13. Reimprimir la **precuenta** de un pedido que ya está `SALIENDO` (hoy solo se imprime al pasar a `SALIENDO`):
    podría ser `tipo: PRECUENTA` en `POST /pedidos/{id}/imprimir`.
14. Limpiar `C:\Respaldos` de pruebas. (`feature/api-movil` **se queda tal cual**, decisión del usuario.)
15. Firmar el instalador (certificado de firma de código) para evitar el aviso de SmartScreen.

---

## 5. Preguntas resueltas y pendientes

**Resueltas** (respuestas del usuario tras el cierre):

| Pregunta | Respuesta | Qué se hizo |
|---|---|---|
| ¿Casilla «Imprimir precuenta» marcada o desmarcada? | **Marcada** | Se queda como está (D16) |
| ¿Copias al agregar productos: 2 o 1? | **2** | Se queda como está (D15) |
| ¿Hora de la precuenta del escritorio con formato de Windows o 12 h? | **12 h con AM/PM** | Hecho: `FrmFacturaFinal.HoraPrecuenta` (`6:53 PM`); probado, 39/39 (D20) |
| ¿Clave mínima de 8 caracteres? | **Aceptable** | Se queda como está (D19) |
| URL del remoto de `CasaGrandeMesero` | «Créalo ahora» | Hecho: repositorio **privado** `jdiego9708/CasaGrandeMesero`, `main` subida (7 commits) |
| ¿Borrar `feature/api-movil`? | **No**, dejarla tal cual | Se mantiene (igual a `main`) |

**Pendiente de responder:**

1. ¿Se mantiene la meta de «un solo PC con LocalDB por usuario», o el restaurante usará otra instancia/servidor SQL?

---

## 6. Checklist de pruebas manuales para mañana

> **Antes:** instalar el `Setup.exe` nuevo (marcar «Activar la API móvil») y el APK nuevo. Esta PC usa `CasaGrandeBD`
> (ya con esquema de producción y SP). Impresora predeterminada: *Microsoft Print to PDF* (los PDF van a
> `Documentos\Casa Grande\Comandas`).

**Instalador (PC)**
- ☐ Página «API móvil»: viene una clave aleatoria de 12 caracteres; «Generar otra» la cambia; escribir una de menos de 8
  caracteres o con espacios la rechaza; escribir `casagrande26` la acepta.
- ☐ Al terminar, la página final repite puerto y clave; «Datos para conectar celulares» muestra IP, puerto y clave.
- ☐ Actualizar encima conserva la clave y el `.exe.config`.
- ☐ «Cambiar clave de la API» (menú Inicio): cambia la clave, deja `.bak`, pide reiniciar la app.
- ☐ Desinstalar: desaparece la regla de firewall `SISRestaurante API` y la reserva de URL (`netsh http show urlacl`).

**Teléfono**
- ☐ Configurar servidor → «Buscar servidor en la red» encuentra el PC; clave; Probar; Guardar.
- ☐ Cambiar la IP del PC (o simularlo) y comprobar que la app se reconecta sola.
- ☐ Crear un pedido: control de copias (2) y comanda siempre marcada → salen platos y bebidas ×2.
- ☐ **Agregar productos** a ese pedido: aviso «solo lo nuevo», control de copias → salen **solo** las líneas nuevas
  («ADICION»); agregar solo una bebida → sin comanda de platos.
- ☐ **Precuenta** con la casilla marcada → mesa a «Saliendo» y sale `precuenta-<id>-…pdf`; con la casilla desmarcada →
  solo cambia el estado. Revisar que el PDF se parece a la precuenta del escritorio.
- ☐ La hora en el detalle del pedido y en Ajustes → Registros se ve como `6:53 PM`.
- ☐ El ícono nuevo (en el lanzador, con ícono temático de Android 13+ si aplica).
- ☐ En el escritorio, el indicador «N DISPOSITIVOS» cuenta el teléfono y el tooltip lista empleado/IP.

**Impresora física (cuando haya)**
- ☐ Comanda de platos/bebidas, adición y precuenta en papel.

---

## 7. Riesgos abiertos

* **Nada de lo de hoy en la app se ha visto en un teléfono**: búsqueda y reubicación, copias al agregar, casilla de
  precuenta, horas, registros, ícono.
* **El instalador y el kit USB no se han ejecutado en un PC real**; el asistente y el UAC de `Cambiar-Clave-API.ps1`
  solo se compilaron/verificaron por partes.
* **Producción sin desplegar** (SP y datos); **impresora física sin probar**; **precuenta sin comparar en papel**.
* **Sin TLS** en la LAN; autorización por PIN sin roles; clave de la API mínima de 8 caracteres.
* **Sesiones en memoria**: reiniciar el `.exe` (o cambiar la clave) obliga a los meseros a volver a entrar.

---

## 8. Estado de esta máquina (referencia)

| Cosa | Estado |
|---|---|
| App instalada | `C:\Program Files\SISRestaurante\` (versión **anterior** a los cambios de hoy), API habilitada, usa `CasaGrandeBD` |
| `CasaGrandeBD` | 38.582 pedidos, `MAX(Id_pedido)=14551460`, esquema de producción + 3 SP (reemplazada a las 18:20) |
| `CasaGrandeBD_prod` | 38.581 pedidos, `MAX=14551458`; la usan `run-local.ps1` y las pruebas |
| `C:\Respaldos` | 3 respaldos de pruebas (`…antes_de_la_API_…1804/1820.bak`, `…ESQUEMA_ANTIGUO_…1820.bak`); se pueden borrar |
| `Documentos\Casa Grande\Comandas` | 2 PDF antiguos de pruebas |
| USB `K:` "CHICOUSB" (29,6 GB, FAT32) | desconectada al cierre; su kit es de una versión anterior |
| Secretos | la clave de la API del PC está en su `.exe.config` instalado; **no** se documenta aquí |

---

## 9. Dónde está cada cosa

[00 Estado](00-estado-del-proyecto.md) · [04 Contrato de la API](04-contrato-api-v1.md) ·
[08 Instalación desde cero](08-instalacion-desde-cero.md) · [`app-movil/09`](app-movil/09-estado-de-la-implementacion.md) ·
[`installer/README.md`](../installer/README.md) · [`scripts/README.md`](../scripts/README.md) · [`db/README.md`](../db/README.md)
