# 09 — Estado de la implementación (2026-10-24)

Lo que se construyó frente a lo modelado en 01–08, y las diferencias que importan.

## Decisiones nuevas

* **Solo Android.** No hay carpetas `ios/`, `windows/` ni `linux/`. El tráfico HTTP sin TLS está permitido en el
  manifiesto (la app valida que la IP sea privada) y `allowBackup=false`.
* **Login solo por PIN.** El PIN identifica al mesero. Si dos empleados lo comparten, la API responde
  `409 PIN_AMBIGUO` con los candidatos y la app muestra un selector (`loginAs`). El endpoint de empleados (B1)
  se elimina. Contrato en [04 §3](../04-contrato-api-v1.md).
* **Riesgo aceptado:** PIN de menos de 4 dígitos (8 de 26 activos). Acción del restaurante: unificar registros
  duplicados y usar PIN únicos.

## Repositorio `D:\Repositorios\CasaGrandeMesero`

Workspace de Dart con dos paquetes:

* `packages/casa_grande_api`: cliente tipado (Dio, reintentos, idempotencia, errores de negocio). 51 pruebas con
  `flutter_test`.
* `app`: la app, feature-first (`auth`, `server_setup`, `menu`, `cart`, `orders`, `tables`, `settings`, `core`).

## Diferencias respecto al modelado

| Modelado | Realidad |
|---|---|
| Módulo `menu` con el pedido | `menu` es el catálogo; el menú del pedido vive en `cart`; revisión y envío en `orders` |
| Configurar servidor en módulo propio | `server_setup` guarda la config; el token y el cliente HTTP viven en `core/network` |
| `Product` sellado | Eliminado: `Dish` y `Beverage` separados |
| Clase para mapear errores | Función `toFailure` en `core/error` |
| Resultado del envío en pantalla propia | `SnackBar` + redirección (mesas o bandeja) |
| Reglas de import estrictas | Relajadas: dominio sin Flutter ni Dio, el resto flexible |

## Versiones y conflictos resueltos

Flutter 3.47.8 / Dart 3.13.7 instalado en `D:\DevTools` en paralelo al existente. Riverpod 3, freezed 4,
go_router 18, Drift, Dio 5. `build_runner` fijado a `<2.17.0` por conflicto con `riverpod_generator`; el paquete
api prueba con `flutter_test` porque `test` choca con freezed ≥ 4.0.2.

## Verificado

* `flutter analyze`: sin errores ni advertencias (17 avisos de estilo).
* `flutter test`: 5 pruebas de dominio en la app y 51 en el paquete api.
* `flutter build apk --debug`: compila.

## Pendiente

* Probar en un teléfono real contra la API (login por PIN, envío, modo sin conexión, bandeja).
* Pruebas de widgets, del envío con fallos (matriz red/401/409/422/503) y de Drift.
* Archivo de firma y APK de release; impresión de comandas con impresora real; desplegar el SP en producción.
