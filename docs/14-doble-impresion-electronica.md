# Doble impresion: comprobante local y factura electronica

Estado del 6 de noviembre de 2026. Este flujo sustituye la restriccion historica de usar el renderer solo para revision manual, por solicitud expresa del usuario de imprimir ambas representaciones.

- El comprobante local conserva su impresion actual al terminar la venta.
- La solicitud electronica se procesa sin depender del visor. Al confirmar aceptacion en Alegra, se descarga su PDF oficial con Basic de emision.
- Descargar PDF desde el detalle de venta conserva el documento original de Alegra. En FE4930 es Carta, plantilla Clasico, ID 9.
- Imprimir factura de Alegra, tanto automatico como manual desde la venta, genera la representacion propia de 80 mm desde XML UBL original. El PDF oficial se usa para contrastar CUFE/numero y extraer fecha de validacion y datos del software/proveedor.
- Se comprueban ID remoto, empresa, total de la venta, aceptacion, numero, CUFE y QR. No se imprime un documento rechazado o sin los datos requeridos por el renderer. No se recorta ni reduce el PDF Carta para una termica.
- Impresora vacia en configuracion: misma predeterminada Windows que usa la impresion local. Microsoft Print to PDF y Adobe PDF abren un dialogo Windows para guardar una copia intacta del PDF de 80 mm, en un hilo STA. No necesitan reprocesar el archivo con el controlador virtual.
- El instalador para Windows de 64 bits incluye renderer ejecutable y SumatraPDF portable para impresoras fisicas. El restaurante no necesita instalar Python; solo el equipo que compila usa scripts/requirements-alegra-pdf.txt.

## Persistencia y recuperacion

`AlegraInvoiceRequests.PdfPath` apunta al PDF **oficial**. La representacion se guarda aparte, con nombre `{SaleId}-80mm.pdf`, junto con XML, detalle y `.revision.json` de verificaciones. Los documentos reales quedan en `%LOCALAPPDATA%/CasaGrande/Alegra/PDF`, fuera del repositorio y de la USB.

`PrintStatus` mantiene el intento de impresion: PENDING, PRINTING, PRINTED, UNKNOWN y CANCELLED. PRINTED significa que termino el guardado virtual o el envio al lector de impresion; no confirma salida fisica del papel. UNKNOWN y CANCELLED requieren reimpresion manual, evitando duplicados automaticos. La impresion no crea ni vuelve a emitir facturas.

Los errores previos al envio conservan la solicitud aceptada y se registran en LastError; se reintenta pasado un minuto sin bloquear indefinidamente otras solicitudes. Una fila aceptada puede descargar su PDF aunque la impresion automatica este apagada. La primera activacion establece `AutoPrintFromUtc` en el JSON de configuracion: no reimprime ventas anteriores. No hay migracion SQL nueva; se usan los campos de la 07.

## Configuracion y despliegue

`db/Configurar-Alegra.ps1` conserva cuenta, productos y pagos. Para configuraciones anteriores sin AutoPrintFromUtc habilita una vez la doble impresion desde ese momento. Si posteriormente el usuario la desactiva en el formulario, conserva esa decision. `Link-AlegraExactProducts.ps1` deja de desactivar la impresion al vincular productos.

`Build-AlegraPdfTools.ps1` empaqueta el renderer y verifica SHA-256 del archivo portable descargado de SumatraPDF. `run-local.ps1` y `Publicar-Instalador.ps1` preparan las herramientas antes de compilar. Preparar-USB lleva esas herramientas dentro del instalador y documenta ambos botones en LEEME. Se deben regenerar el instalador/USB despues de cambios al renderer.

## Validaciones

Pruebas sinteticas: rechazo de numero/CUFE/total/QR discrepantes, estado no aceptado, PDF oficial de otra factura y metadatos oficiales incompletos; salida de una pagina de 80 mm sin texto fuera del area.

Integracion local con FE4930: descarga oficial Carta y generacion de 80 mm mediante las DLL compiladas y el ejecutable portable, sin nueva venta, nueva emision ni salida a una impresora. Se conservan estado de aceptacion y PrintStatus. Se comprueba exclusion de ventas anteriores a la activacion. Una prueba con UI Automation, limitada al proceso de prueba, abrio el dialogo Windows real, guardo una copia byte a byte identica del PDF de 80 mm y cancelo correctamente un segundo intento. No envio trabajos al controlador de una impresora. Falta comprobar la salida fisica con la impresora del restaurante y una venta nueva desde el escritorio actualizado.

## Alcance fiscal

La representacion conserva datos de documentos reales, incluidas observaciones de validacion. No corrige la factura emitida ni certifica cumplimiento tributario integral. El informe mantiene pendientes de revision contable del diseno y leyendas aplicables. Se debe conservar el XML y el contenedor DIAN para la entrega electronica; la impresion no sustituye esa entrega.

Fuentes: https://developer.alegra.com/reference/get_invoices-id y https://normograma.dian.gov.co/dian/compilacion/docs/resolucion_dian_0227_2025.htm (requisitos y representaciones graficas).


## Actualizacion de auditoria y pagos

El paso 01 ahora solo prepara/verifica SQL, sin depender de credenciales. El paso 05 autocontenido importa los TXT y configura Alegra despues del instalador. Ver cierre del 6 de noviembre para uso, pruebas y kit validado.

Se corrigieron pagos locales con coma decimal: el snapshot version 2 se congela desde los valores SQL dentro de la transaccion de venta. FE4931 se recupero sin duplicar la venta y FE4932 se proceso automaticamente desde una venta nueva, con PDF generado y PrintStatus PRINTED. La termica fisica sigue pendiente de prueba.
