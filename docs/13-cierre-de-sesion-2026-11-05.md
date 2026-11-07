# Cierre de sesi?n ? 5 de noviembre de 2026

## Punto de partida para ma?ana

La copia local de producci?n de hoy ya est? restaurada, migrada y activa como `CasaGrandeBD_prod` en `(localdb)\MSSQLLocalDB`. Existen **51 relaciones exactas activas: 32 platos y 19 bebidas**. El escritorio y la app m?vil leen los v?nculos de `AlegraSettings.ConfigurationJson.Products`, la misma configuraci?n que usa la emisi?n. Las relaciones tambi?n est?n registradas en `AlegraItemLinks`.

**Todav?a no se ha verificado una emisi?n completa desde el programa hasta la aceptaci?n en Alegra.** La conexi?n JWT funciona, pero crear la venta local, tener un v?nculo y emitir una factura electr?nica son etapas diferentes. No se enviaron facturas como parte de la restauraci?n, inventario o validaciones de c?digo.

## Cambios realizados en esta conversaci?n

### Formulario del receptor

`FrmElectronicInvoiceCustomer.cs` reorganizado en siete filas: tipos persona/documento/r?gimen; documento y consulta Alegra; nombre/raz?n social y primer nombre; segundo nombre y primer apellido; DV compacto y correo; tel?fono/direcci?n; departamento/municipio/pa?s. El selector de contactos permanece entre la segunda y tercera fila. Se corrigieron contenedores que ocultaban el bot?n y campos que se agregaban de nuevo al final. El aviso de contacto seleccionado ahora aparece debajo de Documento. Los asteriscos obligatorios se actualizan seg?n persona y documento, compartiendo criterio con la validaci?n de guardado.

### Diagn?stico, autenticaci?n y configuraci?n

`run-local.ps1` compila Debug y abre el programa; eso no adjunta un depurador ni mantiene trazas autom?ticamente. Se a?adi? `scripts/Watch-Alegra.ps1`, seguimiento SQL de solo lectura de estado, intentos, identificaci?n remota y error. Ahora distingue la ausencia de la tabla de la ausencia de solicitudes.

La copia local anterior carec?a de la migraci?n 07. Se aplic? y posteriormente se observ? la solicitud local 14327062 en PENDING sin intentos. Esa solicitud y otras pruebas quedaron en la copia anterior, no en los datos reci?n restaurados. Las credenciales probadas como Basic produjeron 401/403; el token JWT nuevo funciona como **Bearer**. Las variables de Windows del usuario son `ALEGRA_EMAIL`, `ALEGRA_API_KEY`, `ALEGRA_AUTH_SCHEME`. No se version? el token ni se incluy? en la USB, informes o sitio p?blico. Reiniciar el escritorio es necesario para cargar cambios del entorno.

El endpoint company no devuelve id. `AlegraWorkflow.CompanyIdentity` usa id cuando existe y, si falta, applicationVersion m?s el n?mero de identificaci?n de la empresa. Ese criterio se utiliza tambi?n en la pantalla de configuraci?n y en el script.

Perfil confirmado mediante consulta de facturas y pagos reales existentes (solo lectura):

| Par?metro | Valor |
| --- | --- |
| Numeraci?n electr?nica | FE ID 40, usada por FE4926, FE4925 y FE4924 |
| Cuenta efectivo | ID 6, Efectivo POS - Terminal Terminal 1 |
| Cuenta transferencia | ID 1, Caja general |
| Cuenta tarjeta | ID 1, Caja general |
| Autenticaci?n | JWT Bearer |
| Emisi?n autom?tica local | Habilitada para las pruebas autorizadas |
| Impresi?n autom?tica PDF Alegra | Deshabilitada |

No se cambi? la cuenta de transferencias a Bancolombia: los pagos consultados estaban en Caja general. Los documentos POS recientes usan POSE ID 38; no se seleccion? esa numeraci?n para facturas FE.

### USB y base de datos

`Preparar-USB.ps1` genera `01-Base-de-Datos.ps1` con los SQL y `Configurar-Alegra.ps1` embebidos. La migraci?n 07 pas? a ser obligatoria. `verificar-despliegue.sql` comprueba las cuatro tablas Alegra y columnas de procesamiento. El paso 01 llama al configurador API, conserva una configuraci?n de la misma empresa y solicita elegir donde hay ambig?edad. Las instrucciones del LEEME fueron ampliadas. **Es necesario regenerar la USB** para llevar estos cambios; no se public? ni ejecut? un instalador en producci?n.

El configurador admite archivo de credencial, correo y esquema expl?citos para evitar que una variable de entorno anterior desplace la selecci?n. Existe un perfil CasaGrande para las selecciones conocidas; las coincidencias que no son ?nicas se exportan y la configuraci?n incompleta queda deshabilitada. El script de v?nculos exactos es el que se us? despu?s para habilitar las pruebas con los productos aprobados autom?ticamente.

### Restauraci?n de producci?n de hoy

Origen: `D:\Usuario_JUAN_DIEGO\Escritorio\Casa Grande\scriptCompleto.sql`, 283.218.410 bytes, modificado el 05/11 a las 22:22. Restauraci?n de 6.472 lotes en 102 segundos, primero en una base separada. Aplicadas las migraciones 01?07, incluidas la unificaci?n de empleados, activaci?n de los dos empleados administrativos/de desarrollo y Domicilios. La verificaci?n de procedimientos, relaciones, mesa 0 y Alegra pas?; cero PIN activos compartidos o iguales a cero, un empleado activo sin PIN.

Despu?s de que el usuario confirm? el cierre de SISMistico, se activ? la copia validada mediante renombrado. No se tocaron servidores remotos de producci?n.

| Elemento | Ubicaci?n / estado |
| --- | --- |
| Base local activa | CasaGrandeBD_prod, restauraci?n de hoy + migraciones |
| Base anterior conservada | CasaGrandeBD_prod_anterior_20261105 |
| Respaldo previo al reemplazo | D:\Repositorios\CasaGrande-local-backups\CasaGrandeBD_prod_20261105_222613.bak |
| Inventario e informes | D:\Repositorios\CasaGrande-inventario-20261105 |

La restauraci?n contiene 38.953 ventas, 38.907 pedidos, 640 platos y 191 bebidas. Se excluyeron del an?lisis 412 productos locales inactivos; se compararon **419 activos locales** contra **226 activos de Alegra** (359 totales remotos).

### Inventario y revisi?n humana

Resultado inicial: 51 nombres exactos ?nicos, 3 ?nicos normalizando tildes/puntuaci?n, 178 propuestas por similitud y 187 sin coincidencia clara. Los **368 no exactos** requieren revisi?n humana. La similitud textual no garantiza misma porci?n, preparaci?n ni impuesto; no se aprobaron autom?ticamente esas propuestas.

Scripts: `Export-AlegraProductInventory.ps1`, `Match-AlegraProducts.py`. Archivos locales: inventario-completo.csv, conflictos-por-resolver.csv, productos-locales.csv, productos-alegra.json, resumen.json, migraciones.log, verificacion.txt, LEEME.md. El reporte definitivo de v?nculos aplicados es `vinculos-exactos-aplicados.csv`.

Se public? un sitio est?tico independiente:

- Sitio: https://jdiego9708.github.io/CasaGrande-revision-productos/
- Repositorio: https://github.com/jdiego9708/CasaGrande-revision-productos
- Fuente local: D:\Repositorios\CasaGrande-revision-productos

Permite aprobar, buscar otros productos activos, proponer creaci?n/desactivaci?n y dejar notas. Guarda decisiones en el navegador y exporta/importa JSON; **no hay sincronizaci?n autom?tica** ni modifica Alegra/SQL. La persona revisora debe devolver el JSON. Solo contiene cat?logo activo, sin clientes, ventas ni claves. El sitio se actualiz? al final de la sesi?n para excluir los 51 exactos ya vinculados: muestra ?nicamente 368 pendientes (3 normalizados, 178 similares y 187 sin coincidencia clara). El progreso antiguo de productos excluidos no se cuenta. Al importar una revisi?n se deben respetar los v?nculos aplicados y detectar conflictos antes de modificar la configuraci?n.

GitHub Actions estaba bloqueado por un problema de facturaci?n; se public? mediante Pages desde main con .nojekyll. HTTP 200 verificado. Guardado, persistencia, exportaci?n/importaci?n y dise?o m?vil probados con Playwright/Edge.

### V?nculos e indicadores

`scripts/Link-AlegraExactProducts.ps1` consulta la empresa, cat?logo e impuestos reales. Vincula solo productos activos con nombre igual ignorando may?sculas y espacios exteriores, sin quitar acentos, y un ?nico ID remoto activo. Deduplica resultados por ID, rechaza v?nculos previos diferentes y escribe tabla y configuraci?n JSON en una transacci?n. En la segunda comprobaci?n quedaron los 51 v?nculos esperados; no se ampli? el criterio a nombres similares.

API: `ApiCatalogoHandlers.cs` expone `alegra_linked` para platos y bebidas y detalle del plato. Lee la configuraci?n una vez por listado, sin consultas por producto.

M?vil (repositorio CasaGrandeMesero): DTO, entidades, repositorio y cach? transportan el booleano; si la API antigua no lo devuelve, vale false. `ProductTile` muestra un punto verde de 6px junto al nombre, con tooltip y sem?ntica accesible, en men? de pedido nuevo y hoja para adicionar a pedidos existentes. Se gener? e instal? el APK debug mediante actualizaci?n en la tablet conectada SM-X133. Para ver datos recientes hay que abrir el escritorio actualizado y refrescar el men? con conexi?n.

Escritorio: `UI/AlegraLinkIndicator.cs` a?ade el mismo punto verde en las celdas de nombre de `FrmPedidoPlatos` y `FrmPedidoBebidas`. Usa la misma configuraci?n de v?nculos; recarga al volver visible el selector o enlazar datos y no consulta SQL durante el pintado. Escala el punto con DPI, conserva texto/selecci?n y muestra tooltip. Se agreg? al csproj y recompil? correctamente. Pendiente inspecci?n visual interactiva del selector actualizado.

**Un punto verde significa vinculado, no factura emitida ni aceptaci?n fiscal.**

## Validaciones finales

- Soluci?n .NET Debug compilada con `SISMistico\run-local.ps1 -NoRun`, incluidos indicador escritorio y API.
- Migraciones/USB probadas dos veces sobre base temporal; empaquetado y sintaxis validados.
- App Flutter: 51 pruebas aprobadas; cliente API: 71 pruebas aprobadas.
- Prueba espec?fica: campo ausente significa no vinculado; serializaci?n/cach? conserva flag; punto solo visible si est? vinculado y mide 6px.
- APK debug compilado e instalaci?n en tablet confirmada por adb (Success).
- An?lisis Flutter sin errores ni warnings; 32 avisos informativos.
- No hubo una prueba de emisi?n end-to-end desde el escritorio actualizado; no asumir aceptaci?n por ver factura local o punto verde.

SDK correcto m?vil: `D:\DevTools\flutter-3.47.8\bin`, Dart >=3.13. El Flutter antiguo del PATH falla por incompatibilidad de SDK. APK: `D:\Repositorios\CasaGrandeMesero\app\build\app\outputs\flutter-apk\app-debug.apk`.

## Pr?ximos pasos acordados

1. Abrir SISMistico actualizado y comprobar los puntos en selectores de platos/bebidas; refrescar men? m?vil y comprobarlos all?.
2. Preparar un pedido con productos vinculados. Revisar antes de emitir si incluye propina, domicilio, desechables o pagos combinados: los cargos adicionales y su medio fiscal requieren confirmaci?n; el punto verde no certifica esos par?metros.
3. Marcar factura electr?nica, confirmar receptor y terminar una sola vez. Seguir con `..\scripts\Watch-Alegra.ps1` desde SISMistico; comprobar SaleId, intentos, RemoteId y estado final. Reconciliar resultados inciertos antes de reintentar para evitar duplicados.
4. Recibir el JSON de revisi?n del sitio y validar IDs/estados/cuenta, conflictos, impuestos y porciones antes de aplicar relaciones no exactas.
5. Completar cargos de propina, domicilio, desechables e impresi?n PDF si se van a utilizar. Regenerar USB despu?s de consolidar par?metros; no se ha desplegado esta configuraci?n en el PC del restaurante.

Los repositorios de escritorio y m?vil contienen cambios locales previos del usuario y trabajo de esta sesi?n sin commit. Se conservaron; **no ejecutar reset/clean ni descartar cambios indiscriminadamente**. Solo el sitio est?tico se cre?, version? y public? en un repositorio separado. Las decisiones guardadas por el revisor no regresan autom?ticamente a estos repositorios.
