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

* `flutter analyze`: sin errores (19 avisos de estilo, todos informativos).
* `flutter test`: 6 pruebas de dominio en la app y 52 en el paquete api.
* `flutter build apk --debug`: compila.
* `scripts/Test-ApiFase1.ps1`: **44/44**.

## Fase 4 en la app (2026-10-26)

* `order_detail_screen.dart` pasa a `ConsumerStatefulWidget`: muestra el pedido, permite **agregar productos**,
  **precuenta** (`SALIENDO`), **quitar una unidad** (con PIN) y **cancelar** (con motivo), y **imprimir** desde la
  barra superior. La barra de acciones desaparece cuando el pedido ya no es editable.
* `add_order_items_sheet.dart`: carrito local con búsqueda y pestañas Platos/Bebidas; reutiliza el configurador de
  ingredientes y envía `POST /pedidos/{id}/items`.
* Tras cada acción se invalida el detalle y se avisa con `dataChanged`, de modo que el mapa de mesas se recarga.
* El símbolo de pesos pasó al inicio en toda la app (`$ 5.000`) cambiando solo `Pesos.format()`.
* Cuidado con el tema: `filledButtonTheme`/`outlinedButtonTheme` usan `minimumSize: Size.fromHeight(...)`, que
  equivale a ancho infinito; cualquier botón dentro de un `Row` debe llevar `minimumSize` propio o rompe el layout.

## Prueba en teléfono real (S24 Ultra, 2026-10-26)

Conexión por USB con `adb reverse tcp:8099 tcp:8099` y en la app servidor `localhost:8099` (no hace falta
`urlacl` ni regla de firewall; ver D7 para la prueba por WiFi). Contra `CasaGrandeBD_prod` y el escritorio real.

| Qué | Resultado |
|---|---|
| Sondeo (`/ping` + clave + BD) | 134 ms, servidor y versión correctos |
| Login con PIN único | 200, entra como CELESTE/MESERO |
| Login con PIN compartido | 409 `PIN_AMBIGUO` y selector con los candidatos |
| Mapa de mesas | estado real del día |
| Pedido con plato detallado y 4 ingredientes | 201; en la BD la línea con SOPA/BLANCO/FRIJOL/JUGO DEL DIA y precio de catálogo |
| Refresco del escritorio | la mesa pasó a OCUPADA sin tocar nada |
| Envío sin conexión | pedido a la bandeja, mesa en "Por enviar" |
| Reenvío al volver la conexión | 201, sin duplicados |
| Detalle del pedido y "Agregar productos" | 200; línea nueva y total actualizado |
| Precuenta (`SALIENDO`) | chip "Saliendo" y `PATCH` 200 |
| Quitar una unidad con PIN | línea eliminada, total recalculado |
| Cancelar con motivo | chip "Cancelado", barra de acciones oculta |
| Imprimir | `202` y PDF guardado en `Documentos\Casa Grande\Comandas` |

### Hallazgos corregidos

1. **El botón de la hoja de ingredientes quedaba debajo de la barra de navegación.** `showModalBottomSheet` con
   `useSafeArea: true` agrega `SafeArea(bottom: false)`, así que el inset inferior no se respetaba. Se envolvió
   la acción en `SafeArea(top: false)` en `dish_configurator_sheet.dart`.
2. **El mapa de mesas no se refrescaba tras el envío automático de la bandeja.** El pedido se creaba pero la
   tarjeta seguía "Libre" hasta refrescar a mano. Se añadió `core/events/data_changed.dart`: el sincronizador
   avisa cuando envía algo y `TablesNotifier` recarga al escucharlo.
3. **Selector de PIN ambiguo sin información útil.** Las fichas duplicadas de la misma persona se veían
   idénticas. La API ahora devuelve `duplicado` por candidato y `duplicados` en `detalle`, y la app muestra un
   aviso, la etiqueta "Duplicada" y el número de ficha. El origen se corrigió en la base de datos con
   [`db/02_unificar_empleados_duplicados.sql`](../../db/02_unificar_empleados_duplicados.sql): una ficha con PIN
   por persona, historia reasignada a la superviviente y PIN `0` convertido en `NULL`.

## Pendiente

* Probar por WiFi de la LAN (`urlacl`, regla de firewall, IP fija y `ApiKey` definitiva: D7).
* Fase 4 de la API y de la app: agregar/quitar ítems, `PENDIENTE → SALIENDO`, cancelar e imprimir.
* Pruebas de widgets, del envío con fallos (matriz red/401/409/422/503) y de Drift.
* Archivo de firma y APK de release; impresión de comandas con impresora **física** (la ruta de PDF ya está
  probada: si la impresora predeterminada es Microsoft Print to PDF, el archivo se guarda en Documentos); desplegar
  el SP y el arreglo de datos en producción.
