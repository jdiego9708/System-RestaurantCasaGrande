# 07 — Calidad, seguridad y pruebas

## 1. Análisis estático y formato

| Medida | Configuración |
|---|---|
| Lints | `very_good_analysis` (conjunto estricto) + `riverpod_lint` |
| Modo estricto del analizador | `strict-casts`, `strict-inference`, `strict-raw-types` |
| Formato | `dart format --set-exit-if-changed` en CI |
| Excluir | `**/*.g.dart`, `**/*.freezed.dart`, `**/*.drift.dart` del análisis |
| Regla de arquitectura | Test que recorre los `import` de `lib/features/*` y falla si un módulo importa internos de otro, si `domain` importa Flutter/Dio/Drift, o si `presentation` importa `data` ([03 §2.1](03-modulos-y-estructura.md)) |
| Cero advertencias | `flutter analyze --fatal-infos --fatal-warnings` |
| Dependencias | `dart pub outdated` semanal en CI (aviso) y revisión de seguridad de paquetes antes de cada release |

## 2. Estrategia de pruebas

Pirámide, de lo más barato y abundante a lo más caro y escaso.

| Nivel | Qué prueba | Herramientas | Objetivo |
|---|---|---|---|
| **Unidad — dominio** | `Draft` (agregar, fusionar, validar, total), `Pesos`, estado de mesas, regla de los 4 ingredientes, `BuildOrderPayload`, huella de pedido | `flutter_test` | ≥ 90 % |
| **Unidad — mapeadores** | DTO ⇄ entidad, enums desconocidos, campos ausentes | | ≥ 90 % |
| **Datos** | Repositorios contra un *fake* del cliente y **Drift en memoria**; outbox (transiciones), caché (frescura, versión de esquema) | `mocktail`, `NativeDatabase.memory()` | ≥ 80 % |
| **Migraciones** | Cada versión de esquema de Drift migra sin perder datos | pruebas de migración de `drift_dev` | 100 % de las migraciones |
| **Cliente HTTP** | Interceptores, mapeo de errores, reintentos y tiempos, con un adaptador falso de Dio | `flutter_test` (Dart) | ≥ 85 % |
| **Estado (providers)** | Notifiers: login, mesas (sondeo), carrito, envío | `ProviderContainer.test()` + `overrides` | Casos felices y de error |
| **Widget** | Cada pantalla en sus estados: cargando, vacío, error, sin conexión, datos; accesibilidad semántica | `flutter_test` | Todas las pantallas |
| **Golden** | Tarjeta de mesa, tarjeta de producto, línea de carrito, hoja "Armar plato" en claro/oscuro y texto grande | `alchemist` | Componentes visuales clave |
| **Arquitectura** | Reglas de import | test propio | Siempre en CI |
| **Contrato en vivo** | `casa_grande_api` contra la **API real** (etiqueta `live`, fuera del CI por defecto) | `flutter_test` + variables de entorno | Espejo de `scripts/Test-ApiFase1/2/3.ps1` |
| **Integración (dispositivo)** | 3–4 recorridos: emparejar+entrar, tomar un pedido, pedido sin red que se envía al volver, sesión vencida | `integration_test` (y `patrol` si hay permisos nativos) | Antes de cada release |

### 2.1 Matriz de fallos del *outbox* (pruebas obligatorias)

Cada fila es una prueba de datos + una de estado. Reproduce los comportamientos que la API ya tiene probados.

| Escenario del servidor | Resultado esperado en el teléfono |
|---|---|
| `201` | Se borra la entrada y el borrador; resultado "enviado" |
| `200` (reintento con la misma clave) | Igual que `201`, `wasReplay = true`, sin duplicar |
| Timeout **después** de que el servidor creó el pedido, luego `200` | Una sola entrada, un solo pedido; éxito al reintentar |
| Sin red | Entrada `pending`, `attempts + 1`; se reintenta al volver la conexión |
| `401` | `pending`; ir a Login; tras entrar, reintento automático |
| `409 MESA_OCUPADA` y el pedido **es suyo** (huella igual) | Éxito (limpieza), no conflicto |
| `409 MESA_OCUPADA` y **no** es suyo | `conflict`, con el número de pedido |
| `409 CONFLICTO` | Error de programación: se registra y queda `failed` |
| `422` (producto inactivo / falta ingrediente) | `failed` con el mensaje; "Editar" crea borrador nuevo con clave nueva |
| `503 BD_NO_DISPONIBLE` / `5xx` | `pending`, reintento con espera creciente |
| App cerrada con una entrada `sending` | Al abrir, pasa a `pending` y se reintenta (no queda atascada) |
| Dos disparos simultáneos del sincronizador | Un solo trabajador: no se manda dos veces |

## 3. Integración continua (GitHub Actions)

Pasos en cada *pull request* y en `main`:

1. Fijar la versión de Flutter (la de `06`) y restaurar la caché de `pub`.
2. `dart pub get` (resuelve el *workspace*).
3. `dart run build_runner build --delete-conflicting-outputs`.
4. `dart format --set-exit-if-changed .` · `flutter analyze --fatal-infos --fatal-warnings`.
5. `flutter test --coverage` en ambos paquetes (con umbral mínimo de cobertura).
6. `flutter build apk --debug` y guardar el artefacto.
7. Semanal: `dart pub outdated` y compilación con la Flutter en `beta` para enterarse a tiempo de roturas.

Los *releases* (APK firmado) se hacen con una etiqueta `vX.Y.Z`; la **clave de firma nunca va en el repositorio** (secreto de CI
o máquina de quien publica).

## 4. Seguridad

| Amenaza | Medida |
|---|---|
| Teléfono perdido o robado | Token y clave de la API en Keystore/Keychain; el token dura 8 h; PIN, no contraseña guardada; **rotar la `ApiKey` en el escritorio revoca todos los teléfonos** (hoy no hay revocación por dispositivo: ver B5) |
| Copia de seguridad de Android extrae la BD local | `android:allowBackup="false"` |
| Escucha en la red WiFi (HTTP sin cifrar) | Riesgo aceptado (D10) mientras la WiFi del personal sea privada y separada de la de clientes; mitigación futura: TLS con una CA local. La app solo se conecta a IP privadas |
| Servidor falso en la misma red | El usuario escribe/escanea la dirección; la app muestra el **nombre del servidor** (`/ping`) y la versión para confirmación |
| Fuerza bruta de PIN | El servidor limita 5 fallos/min por IP; la app muestra la cuenta regresiva y no automatiza reintentos |
| Datos sensibles en logs | El interceptor registra método, ruta, estado y duración; **nunca** cuerpos, cabeceras, tokens ni claves. Revisión en cada PR |
| Capturas de pantalla / vista previa | PIN oculto, sin autocompletado ni sugerencias; el Login no se muestra en la vista previa de tareas (marca de seguridad de ventana) |
| Dependencias vulnerables | Revisión de `pub outdated`/avisos antes de cada release; actualizaciones pequeñas y frecuentes |
| Inyección vía respuestas del servidor | DTOs estrictos; textos del servidor se muestran como texto plano (nunca como HTML/enlaces) |
| Ofuscación | `--obfuscate --split-debug-info` en el APK de producción; no sustituye a nada de lo anterior (no hay secretos en el binario: la clave se escribe en pantalla) |

## 5. Accesibilidad

* Objetivos táctiles ≥ 48 dp; separación suficiente entre botones de cantidad (– / +) para evitar toques erróneos.
* Contraste AA en claro y oscuro (verificado con tema Material 3 y revisión manual de los colores de estado).
* El estado de la mesa y el de conexión se comunican con **texto + ícono**, no solo con color.
* `Semantics` en tarjetas de mesa ("Mesa 5, pendiente, 134 mil 800 pesos") y en botones con solo ícono.
* Escala de texto del sistema hasta 200 %: se prueban *goldens* con `textScaleFactor` alto.
* Foco y orden de lectura lógicos; mensajes de error asociados al campo.

## 6. Rendimiento

* Listas con `ListView.builder`/`SliverGrid`; tarjetas `const`; `ref.watch(provider.select(...))` para reconstruir lo mínimo.
* El arranque pinta con la caché y refresca en segundo plano; nada de bloquear la primera pantalla por la red.
* Trabajo pesado (parseo de ~600 productos, escritura de caché) fuera del hilo de UI cuando supere unos milisegundos.
* Medición en **modo *profile*** en un teléfono de gama baja antes de cada release (no en emulador).
* Presupuesto: arranque < 2 s con caché; 60 fps al desplazar el menú; APK < 30 MB.

## 7. Definición de "terminado" (por historia)

1. Código con lints sin advertencias y formato correcto.
2. Pruebas del nivel que corresponda (dominio/datos/estado/widget) en verde y cobertura sin bajar.
3. Todos los textos en ARB; sin literales en widgets.
4. Estados de carga/vacío/error/sin conexión resueltos.
5. Probada contra la API real (`live`) si toca el contrato.
6. Documentación actualizada (esta carpeta) si cambia una decisión o el modelo.
7. Mensaje de commit en español, con el porqué.
