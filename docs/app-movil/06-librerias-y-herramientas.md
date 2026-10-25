# 06 — Librerías y herramientas

> **Versiones consultadas en pub.dev y en el índice de lanzamientos de Flutter el 2026-10-24.** Se fijan con `^` en el
> `pubspec.yaml` al crear el proyecto y se revisan antes de cada release. La columna "requiere" es el SDK mínimo que declara
> cada paquete.

## 1. Plataforma

| Herramienta | Versión | Nota |
|---|---|---|
| **Flutter (estable)** | **3.47.8** (2026-10-23) | Incluye Dart 3.13.7 |
| **Dart** | **3.13.7** | Los miembros del *workspace* fijan `sdk: ^3.13.0` |
| Ciclo de Flutter | 3.50 prevista para noviembre de 2026 | Subir de versión mayor solo entre releases de la app, con CI en verde |

Fijar la versión de Flutter para todo el equipo y el CI (FVM o el archivo de versión de la acción de CI): "funciona en mi
máquina" con otra versión de Flutter es la causa más común de fallos de generación de código.

## 2. Dependencias de la app (`casa_grande_mesero`)

| Necesidad | Paquete | Versión | Requiere | Por qué |
|---|---|---|---|---|
| Estado + DI | `flutter_riverpod` | 3.4.3 | Dart ^3.12 | Un solo mecanismo para estado, DI y asincronía (`AsyncValue`); pruebas con `overrides` |
| | `riverpod_annotation` | 4.0.7 | ^3.12 | Anotaciones del generador |
| Navegación | `go_router` | 18.1.1 | Flutter ≥ 3.44 | Declarativa, guardas con `redirect`, `StatefulShellRoute` |
| Modelos inmutables | `freezed_annotation` | 3.1.0 | ≥ 3.0 | Con `freezed` genera `==`, `copyWith`, uniones |
| | `json_annotation` | (la que pide `freezed`, ^4.8) | | JSON de DTOs |
| Base de datos local | `drift` | 2.35.3 | ≥ 3.10 | SQLite tipado, consultas reactivas, migraciones y pruebas de migración |
| | `drift_flutter` | 0.3.1 | ≥ 3.10 | Abre la BD en el dispositivo (trae el SQLite nativo) |
| Almacenamiento seguro | `flutter_secure_storage` | 11.2.0 | ≥ 3.8 | Keystore (Android) / Keychain (iOS) |
| Preferencias | `shared_preferences` | 2.5.6 | ^3.11, Flutter ≥ 3.41 | Valores no secretos |
| Conectividad | `connectivity_plus` | 7.3.2 | | Detecta cambios de red; la verdad la da `GET /ping` |
| Identificadores | `uuid` | 4.6.0 | ≥ 3.0 | v4 para la clave de idempotencia y ids locales |
| i18n / formato | `intl` + `flutter_localizations` (SDK) | 0.20.3 | ^3.9 | ARB con `gen-l10n`, formato de pesos `es_CO` |
| Versión de la app | `package_info_plus` | 10.2.2 | | Mostrar versión/compilación en Ajustes |
| Registro local | `talker_flutter` | 5.2.0 | ≥ 3.6 | Registro en memoria + pantalla de registros para soporte; sin nube |
| Estados de carga | `skeletonizer` | 3.0.0 | ≥ 3.7 | Esqueletos sin duplicar widgets |
| QR (después) | `mobile_scanner` | 7.4.2 | Flutter ≥ 3.29 | Solo si se aprueba el emparejamiento por QR (A4); añade permiso de cámara |

## 3. Dependencias del cliente HTTP (`casa_grande_api`, Dart puro)

| Paquete | Versión | Nota |
|---|---|---|
| `dio` | 5.11.1 | Interceptores, tiempos, cancelación; adaptador sustituible en pruebas |
| `freezed_annotation` / `json_annotation` | como arriba | DTOs |
| `meta`, `collection` | las del SDK/`pub` | |

## 4. Herramientas de desarrollo (`dev_dependencies`)

| Para | Paquete | Versión | Nota |
|---|---|---|---|
| Generación de código | `build_runner` | 2.17.1 | `dart run build_runner build` |
| | `freezed` | **4.0.3** | Ver 4.1 |
| | `json_serializable` | 6.15.2 | |
| | `riverpod_generator` | 4.0.9 | |
| | `drift_dev` | 2.35.2 | Esquema, migraciones, pruebas de migración |
| Lints | `very_good_analysis` | 11.0.0 | Exige Dart ^3.13; es el conjunto estricto de referencia |
| | `riverpod_lint` | 3.1.9 | Funciona sobre `analyzer_plugin` (ya no sobre `custom_lint`) |
| Pruebas | `flutter_test`, `integration_test` | SDK | |
| | `mocktail` | 1.0.5 | Dobles sin generación de código |
| | `alchemist` | 0.14.1 | *Golden tests* estables entre máquinas |
| | `patrol` | 4.10.0 | Opcional: pruebas E2E con permisos nativos (cámara del QR) |
| Identidad | `flutter_native_splash` | 2.4.8 | Pantalla de arranque |
| | `flutter_launcher_icons` | 0.14.4 | Último release en jun-2025; si deja de funcionar con la versión de Flutter, se genera el ícono adaptativo a mano |

### 4.1 Notas de compatibilidad (leer antes de crear el proyecto)

* **`freezed` 4 (ago-2026) rompe con 3.x en una cosa:** ya **no** acepta `final` en los parámetros de los constructores (Dart
  3.13 dejó de permitir esa sintaxis). Además añade soporte para *primary constructors* de Dart 3.13. Todo código nuevo se
  escribe ya en la sintaxis de la versión 4; no hay migración que hacer porque no hay código previo.
* **Conflictos de `analyzer`:** `freezed` 4.0.3 exige `analyzer >=14 <15`; `riverpod_generator` 4.0.9 admite `>=13 <15`. Otros
  generadores (`json_serializable`, `drift_dev`) también restringen `analyzer`. En un *workspace* hay **una sola
  resolución**, así que un choque aparece enseguida en `dart pub get`. Procedimiento: crear el proyecto con las versiones de
  arriba, correr `dart pub get` y `dart pub deps`; si algo choca, subir el generador que quede atrás en vez de bajar `freezed`.
  Esto **no se pudo verificar sin crear el proyecto** y es lo primero que se comprueba en la fase de fundaciones.
* **`sqlite3_flutter_libs` está retirado** (última versión `0.6.0+eol`: "usar `sqlite3` 3.x"). **No se añade.** Se espera que
  `drift_flutter` traiga ya el `sqlite3` 3.x; se comprueba con `dart pub deps` al crear el proyecto (no se verificó la lista
  de dependencias de `drift_flutter`).
* **Riverpod 3:** los proveedores `StateProvider`, `StateNotifierProvider` y `ChangeNotifierProvider` pasaron a `legacy`; no
  se usan. `AsyncValue.value` devuelve `null` en error (ya no existe `valueOrNull`). Los errores de `ref.watch` llegan
  envueltos en `ProviderException`: los mapeadores de fallos deben desenvolverlo.
* **`go_router` 18** requiere Flutter ≥ 3.44: coherente con 3.47.
* `shared_preferences` 2.5.x ofrece la API asíncrona nueva (`SharedPreferencesAsync`/`WithCache`); se usa la nueva.

## 5. Decisiones: qué NO se adopta y por qué

| Alternativa | Decisión | Motivo |
|---|---|---|
| Bloc/Cubit | No | Riverpod ya cubre estado + DI con menos ceremonia para este tamaño; no se mezclan dos paradigmas |
| `provider`, `get_it`, GetX | No | Riverpod sustituye a los dos primeros; GetX acopla todo a estado global |
| Retrofit / generación de clientes | No (por ahora) | Son ~15 endpoints; un cliente tipado a mano con Dio y DTOs es más fácil de depurar. Se reevalúa si el contrato crece (ver B10: escribir el OpenAPI) |
| `Either`/`fpdart` | No | `sealed class AppFailure` + `switch` exhaustivo + `AsyncValue` ya expresan éxito/fallo sin otra dependencia |
| `equatable` | No | `freezed` ya da igualdad por valor |
| `hive`, `sqflite` crudo | No | Falta de tipos/migraciones/consultas reactivas frente a Drift |
| Firebase (Crashlytics, Analytics, FCM) | No | Privacidad y arquitectura: todo se queda en la LAN del restaurante |
| `flutter_hooks` | No | Una abstracción más sin necesidad clara |
| `go_router_builder` / `auto_route` | No (por ahora) | Con ~10 rutas, las constantes de nombre bastan; sin otro generador en el *build* |

## 6. Estructura de `pubspec` (esbozo, sin versiones definitivas)

```yaml
# pubspec.yaml (raíz del repositorio)
name: casa_grande_mesero_workspace
environment:
  sdk: ^3.13.0
workspace:
  - packages/casa_grande_api
  - app
```

Cada miembro lleva `resolution: workspace`. Los números exactos se escriben al crear el proyecto, contra la tabla de arriba.
