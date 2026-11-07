# Inventario y restauraci?n local ? 2026-11-05

Origen: `D:\Usuario_JUAN_DIEGO\Escritorio\Casa Grande\scriptCompleto.sql`, 283.218.410 bytes, modificado el 05/11/2026 a las 22:22 (hora local). Se restauraron 6.472 lotes en 102 segundos.

Se conserv? primero el respaldo local `D:\Repositorios\CasaGrande-local-backups\CasaGrandeBD_prod_20261105_222613.bak`. La base de producci?n remota no se modific?. La restauraci?n se realiz? en `(localdb)\MSSQLLocalDB`, base `CasaGrandeBD_prod_20261105`, separada de la copia anterior para validar antes de sustituirla.

Se aplicaron las migraciones 01, 02, 03, 04, 05, 06 y 07 del repositorio. Incluyen procedimientos m?viles, unificaci?n de empleados/PIN, activaci?n de los dos empleados de administraci?n/desarrollo, Domicilios e integraci?n Alegra. Verificaci?n: procedimientos, relaciones, mesa 0, tablas Alegra y columnas de procesamiento OK; cero PIN compartidos entre empleados activos y cero PIN activos iguales a 0. Queda un empleado activo sin PIN. Restauraci?n: 38.953 ventas, 38.907 pedidos, 640 platos y 191 bebidas.

## Inventario

Se comparan exclusivamente productos locales ACTIVO (NULL conserva el criterio ACTIVO usado por la aplicaci?n), contra productos de Alegra no inactivos. Se consultaron todos los lotes del cat?logo remoto con el JWT existente; ning?n archivo generado contiene el token. No se consultaron ni exportaron clientes para este inventario.

- Productos locales: 831; activos: 419; inactivos excluidos: 412.
- Productos Alegra: 359; activos: 226.
- Coincidencia exacta ?nica: 51.
- Coincidencia ?nica normalizando tildes, puntuaci?n y espacios: 3.
- Propuestas por similitud para revisar: 178.
- Sin coincidencia clara (mejor similitud inferior al 65%): 187.

La similitud es una comparaci?n de texto, no una equivalencia comercial, tributaria o de porci?n. Ejemplos de dudas: PICADA PARA 4 PERSONAS frente a PICADA 3-4 personas; COSTILLAS BBQ 520 GR frente a COSTILLAS BBQ; platos con salsas distintas pueden obtener similitud alta y no deben vincularse autom?ticamente. Las coincidencias exactas tambi?n son propuestas, no v?nculos aplicados. No se ha creado ni emitido ninguna factura durante este trabajo.

## Archivos para revisi?n

`D:\Repositorios\CasaGrande-inventario-20261105\conflictos-por-resolver.csv`: 368 filas dudosas, hasta tres candidatos por producto. Separador punto y coma, UTF-8 con BOM para Excel. Complete `IdAlegraElegido` y `Decision` (VINCULAR, CREAR_EN_ALEGRA, NO_VINCULAR o DESACTIVAR_LOCAL) y a?ada `Observaciones` si hace falta. Las decisiones de creaci?n/desactivaci?n son propuestas para una fase posterior; editar el CSV no ejecuta cambios.

`inventario-completo.csv`: incluye las 419 filas activas y las 51 propuestas exactas. `productos-locales.csv` incluye tambi?n los inactivos para auditor?a. `productos-alegra.json` contiene el cat?logo remoto y sus estados. `resumen.json`, `migraciones.log` y `verificacion.txt` documentan los resultados.

Scripts reproducibles: `scripts/Export-AlegraProductInventory.ps1` y `scripts/Match-AlegraProducts.py`. El primero permite elegir la base y carpeta de salida; el segundo genera las propuestas sin modificar ninguna base ni Alegra.

## Estado de integraci?n

La configuraci?n anterior de prueba (FE ID 40; efectivo cuenta 6; transferencia/tarjeta cuenta 1) y la venta local de prueba 14327062 permanecen en la copia anterior y en su respaldo. No se mezclan esas ventas de prueba con los datos reci?n restaurados. La base nueva deja Alegra sin habilitar hasta resolver las vinculaciones y confirmar el resto de par?metros.

Pendiente: cerrar SISMistico para sustituir `CasaGrandeBD_prod` por la base validada, conservando la anterior con otro nombre. El cierre normal solicitado no finaliz? la aplicaci?n, por lo que no se forz? ni se interrumpieron sus conexiones. Tras la sustituci?n debe reiniciarse el escritorio para cargar la base nueva y el JWT Bearer.

## Sitio de revision

Fuente independiente: https://github.com/jdiego9708/CasaGrande-revision-productos . Publicacion prevista en https://jdiego9708.github.io/CasaGrande-revision-productos/ . Incluye 419 productos locales activos y 226 productos activos de Alegra. Persistencia local en el navegador, exportacion/importacion JSON, aprobacion individual y notas. No modifica bases de datos ni Alegra; el archivo exportado debe regresar al encargado. Pruebas Playwright con Edge: guardar, persistir tras recarga, exportar, importar y ancho movil sin desbordamiento; sin errores JavaScript. GitHub Actions no pudo arrancar por bloqueo de facturacion de la cuenta; se cambio a Pages directo desde main, con .nojekyll. No se incluyeron clientes, ventas ni credenciales en el repositorio publico.


## Activaci?n de copia restaurada y v?nculos exactos

Tras el cierre confirmado por el usuario, la base restaurada se renombr? a `CasaGrandeBD_prod`; la anterior se conserva como `CasaGrandeBD_prod_anterior_20261105`, adem?s del .bak ya documentado. El escritorio recompil? con run-local.ps1 -NoRun. No se trasladaron ventas locales de prueba a la copia nueva.

Se ejecut? `scripts/Link-AlegraExactProducts.ps1` contra la copia nueva: 51 v?nculos (32 platos y 19 bebidas), exclusivamente nombres iguales ignorando may?sculas/min?sculas y espacios exteriores, con una ?nica coincidencia remota activa. No se eliminaron acentos ni se aprobaron similitudes. Se consultaron los impuestos reales de los productos. Los v?nculos se guardan en AlegraSettings.Products, que usa el emisor, y AlegraItemLinks en una misma transacci?n. El script conserva otros v?nculos existentes y rechaza conflictos; deduplica la paginaci?n remota por ID. El reporte aplicado est? en `D:\Repositorios\CasaGrande-inventario-20261105\vinculos-exactos-aplicados.csv`.

Se conserva el perfil ya elegido: empresa comprobada por API, FE ID 40, efectivo cuenta 6, transferencia/tarjeta cuenta 1, JWT Bearer desde Windows. Emisi?n habilitada para probar productos vinculados; PDF autom?tico deshabilitado. El punto verde confirma la existencia de la vinculaci?n, no certifica que una factura haya sido emitida ni que cargos adicionales est?n configurados. Los productos sin v?nculo siguen sujetos a la validaci?n de emisi?n existente. No se envi? ninguna factura durante estas comprobaciones.

La API de cat?logo devuelve `alegra_linked` usando la misma configuraci?n que utiliza el emisor (una lectura de configuraci?n por listado, sin una consulta por producto). La app Flutter propaga este dato en DTO, entidades y cach?. Ausencia del campo en API/cach? anterior significa false. ProductTile muestra un punto verde de 6px junto al nombre con tooltip y etiqueta accesible, en los listados de platos/bebidas para pedido nuevo y para adicionar a uno existente.

Validaci?n: soluci?n .NET Debug compila; 51 pruebas de la app y 71 del cliente API pasan; prueba nueva verifica indicador vinculado/no vinculado y compatibilidad/serializaci?n JSON. An?lisis est?tico sin errores ni warnings, 32 avisos informativos. APK debug generado en `D:\Repositorios\CasaGrandeMesero\app\build\app\outputs\flutter-apk\app-debug.apk`. Para ver v?nculos nuevos en m?vil se debe refrescar el men? con conexi?n al escritorio actualizado.


## Cierre definitivo de la sesi?n

La copia restaurada ya est? activa como CasaGrandeBD_prod y la anterior se conserva renombrada. Los 51 v?nculos exactos est?n aplicados (32 platos, 19 bebidas). APK instalado en tablet. Se a?adi? y recompil? el indicador verde de escritorio en ambos selectores. Estado, pruebas, configuraci?n y pr?ximos pasos: [cierre de sesi?n del d?a](../13-cierre-de-sesion-2026-11-05.md). Esta secci?n actualiza los pendientes cronol?gicos anteriores.
