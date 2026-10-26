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

Flutter 3.47.8 / Dart 3.13.7 instalado en `D:\DevTools\flutter-3.47.8`, en paralelo al existente. **El `flutter` del
`PATH` (`D:\DevTools\flutter`) es el 3.11.5 y no sirve para este proyecto** (`pub get` falla: exige SDK ^3.13.0); hay
que usar la ruta completa o poner `D:\DevTools\flutter-3.47.8\bin` al inicio del `PATH` de la sesión. Riverpod 3, freezed 4,
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

## Comanda siempre activa y número de copias (2026-10-26)

* En **Confirmación del pedido** (`review_order_screen.dart`) el checkbox «Imprimir comanda en cocina» queda marcado y
  deshabilitado: la app siempre envía `imprimir_comandas: true`. Debajo hay un control `− n +` («Copias de la
  comanda») con el mismo estilo que Comensales: **2 por defecto, de 1 a 5** (constantes `defaultComandaCopies`,
  `minComandaCopies` y `maxComandaCopies` en `submit_order.dart`). Las copias no se guardan entre pedidos.
* Nuevo campo `copias_comandas` en `POST /pedidos` ([04 §6.1](../04-contrato-api-v1.md)): el escritorio imprime esa
  cantidad de copias de la comanda de platos y de la de bebidas. Si no llega, 1 (clientes antiguos sin cambios);
  fuera de 1–5, `400`. En la app es `CreateOrderDto.comandaCopies`, y viaja dentro del payload de la bandeja.
* Se **eliminó el ajuste «Imprimir comanda por defecto»** y con él `PrintByDefault`, `StoreKeys.printByDefault` y sus
  dos textos; ya no tenía efecto.
* **Despliegue:** primero el escritorio y después la app. Con el escritorio viejo el campo se ignora y sale 1 copia.
  Los pedidos que ya estaban en la bandeja se envían con 1 copia (se guardaron sin el campo).
* Verificado: `flutter analyze` sin avisos nuevos y `flutter test` en verde. **El cambio de C# no se compiló ni se
  probó**, y la pantalla no se ha probado en el teléfono ni con impresora física.

## Indicador de dispositivos y script de red (2026-10-26)

* **Indicador «N DISPOSITIVOS»** en `FrmObservarMesas`, a la derecha de SALIENDO. `ApiRouter` anota, en
  `ApiAuth.RegistrarActividad`, la IP de cada petición con sesión válida; un dispositivo cuenta como conectado si hizo
  alguna en los últimos **90 s** (la app consulta las mesas cada pocos segundos). Se cuenta **por IP**, no por sesión
  (las sesiones duran 8 h y un re-login las duplicaría). Con `adb reverse` el teléfono aparece como `127.0.0.1`. Un
  `Timer` de 5 s lo refresca; el tooltip lista empleado, IP y segundos desde la última petición; se oculta si la API
  no está activa. Es memoria del proceso: se pierde al cerrar el escritorio. **Compila** (`run-local.ps1 -Rebuild
  -NoRun`, que usa los *reference assemblies* de NuGet; con MSBuild a secas falla por falta del Developer Pack de
  .NET 4.8). **No se ha visto en pantalla** ni probado con un teléfono. El botón «Domicilio» (oculto en la v1) se movió
  a x=540 para no encimarse con el indicador.
* **`scripts/Configurar-Red.ps1`** prepara el PC del restaurante para la LAN real (urlacl, firewall, perfil de red, IP,
  suspensión) y al final imprime los datos para los celulares. Probado solo con `-SoloInformar`; las acciones con
  administrador están sin ejecutar. Ver [`scripts/README.md`](../../scripts/README.md).

## Instalador real que reemplaza a ClickOnce (2026-10-26)

`installer/` construye `SISRestaurante-Setup-<versión>.exe` (~10 MB) con **Inno Setup 6**; `Publicar-Instalador.ps1`
compila en Release, empaqueta, calcula el SHA-256 y lo copia a una USB o carpeta (`-Destino`). Instala en
`Archivos de programa`, da permiso de escritura a los usuarios (la app reescribe su `.exe.config`), activa y configura
la API móvil (clave, puerto, `urlacl`, firewall), conserva el `.exe.config` al actualizar y limpia red al desinstalar.
Detalle, opciones evaluadas y advertencias en [`installer/README.md`](../../installer/README.md).
**El instalador compila, pero no se ha ejecutado** (instalar, actualizar, desinstalar) en un PC real, y no está firmado.

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
