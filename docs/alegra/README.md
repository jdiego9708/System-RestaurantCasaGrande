# Integración Alegra — estado de implementación

Contratos obtenidos directamente del MCP y particularidades del país:
[02 — Contratos API Colombia](02-contratos-api-colombia.md). Los JSON originales están en `contracts/`.

Etapa final solicitada por el usuario: [03 — Cola, vínculo con Alegra y reimpresión](03-cola-emision-y-reimpresion.md).
Implementar al terminar los contratos y validaciones actuales: guardado local inmediato, emisión en segundo
plano, actualización periódica del ID/estado y reimpresión del PDF mediante el ID remoto asociado a la venta.

## Decisiones aprobadas

- Colombia; cuenta existente con emisión electrónica habilitada.
- Checkbox en Facturar Pedido, debajo de descuentos y observaciones; solo ventas marcadas se envían.
- Vincular productos existentes; identificación fiscal separada de dirección de entrega.
- Precios con impuestos incluidos. Distribuir proporcionalmente los ajustes del total de domicilios.
- Conservar impresión local y añadir PDF original de Alegra, térmica de 80 mm.
- Ante fallo remoto, conservar venta y solicitud pendiente. No volver a crear una factura si el resultado es incierto.

## Contrato y fuentes

Autenticación Basic con correo y token sobre HTTPS; no guardar el token en el repositorio ni en el paquete USB.
IDs externos como texto, compatibles con UUID.

### Variables de entorno de Windows

`ALEGRA_AUTH_SCHEME`: `Bearer` para el token recomendado actual; `Basic` para tokens anteriores
(valor predeterminado por compatibilidad). Verificación de lectura 2026-11-03: el token actual
funciona con Bearer en GET /invoices, /items y /contacts (200). GET /company devuelve 403; pendiente
habilitar ese permiso. No se guardó el token ni respuestas con datos de la cuenta en esta documentación.
En esta máquina las variables se configuraron por usuario: la sesión no dispone de permisos para escribir
el entorno Machine. El instalador elevado sí puede configurarlas a nivel de sistema.

El instalador ofrece la tarea opcional «Configurar credenciales de Alegra». Solicita correo y token
(campo oculto) y escribe ambas variables en el entorno del sistema con privilegios de administrador.
No verifica permisos ni emite documentos. En actualizaciones, dejar la tarea desmarcada conserva lo existente.
No se transporta el token como argumento de consola ni se imprime en el resumen. La tarea requiere el
asistente interactivo; la instalacion silenciosa normal no cambia estas variables.
Los permisos se revisaran al probar cada endpoint; no se bloquea la configuracion por una auditoria global.

`AlegraCredentials.FromEnvironment()` lee `ALEGRA_EMAIL` (correo del usuario API) y `ALEGRA_API_KEY`
(token de Alegra). Prioridad por variable: sistema (Machine), usuario (User), proceso (Process).
Se consulta Windows directamente para detectar cambios aunque el proceso tenga un entorno heredado anterior.
No hay valores predeterminados ni respaldo en App.config, base de datos o archivos .env.
Si faltan variables, se informa solo su nombre, nunca su contenido.

Configurar ambas en la otra máquina: la USB no transporta credenciales. La autenticación se construye
como Basic a partir de correo:token; API KEY por sí sola no reemplaza el correo.
Los permisos se verificarán por operación: leer contactos, productos, impuestos, numeraciones y bancos;
crear/editar contactos cuando corresponda, crear factura y pagos, emitir y consultar PDF/XML.
Un acceso de lectura correcto no demuestra permiso para emitir. No probar permisos de escritura creando
facturas en producción; clasificar 401 como autenticación fallida y 403 como acceso denegado.

- [Autenticación](https://developer.alegra.com/reference/autenticaci%C3%B3n)
- [Creación](https://developer.alegra.com/reference/post_invoices): precios antes de impuestos; cliente, productos, numeración y pagos.
- [Emisión](https://developer.alegra.com/reference/post_invoices-stamp): emitir la factura ya creada.
- [Consulta y PDF/XML](https://developer.alegra.com/reference/get_invoices-id): solicitar fields=pdf,xml; borradores sin PDF.
- [Parámetros Colombia](https://developer.alegra.com/reference/colombia): distinguir emisión de estado comercial.
- [Identificadores](https://developer.alegra.com/changelog/nuevo-formato-de-id).

## Persistencia

`07_alegra_integration.sql` crea las asociaciones de clientes y productos y la solicitud por venta.
La clave única de venta impide dos solicitudes para la misma venta. El token no pertenece a estas tablas.
Los datos fiscales y económicos deben congelarse en RequestJson antes de emitir.
Guardar solicitud y venta en la misma transacción; nunca mantener una transacción SQL abierta durante HTTP.

Estados locales: PENDING, CREATING, UNCERTAIN, CREATED, ISSUING, ACCEPTED, REJECTED.
UNCERTAIN requiere conciliación antes de otro POST. La reimpresión solo descarga/imprime: no crea ni emite.
La impresión tiene estado independiente; tras un reinicio no repetir automáticamente trabajos de resultado incierto.

## Impresión y cálculos

Imprimir primero el comprobante local. Cuando la electrónica esté aceptada, descargar el PDF oficial y
conservarlo por Id_venta. Confirmar que la plantilla de la cuenta sea legible en 80 mm antes de activarla.
No reemplazarlo por el reporte local ni reducir silenciosamente una hoja A4.

Distribuir el ajuste proporcionalmente entre las líneas elegibles usando decimal; resolver redondeos por
restos mayores y desempate estable por identificador de detalle. Separar cargos y propina de productos.
Desglosar impuestos según catálogo/configuración; exigir igualdad entre total local, pagos y total remoto.

## Pendiente de implementación

Implementados: credenciales exclusivamente desde variables de entorno, transporte REST, captura fiscal Colombia,
guardado transaccional de venta/solicitud, configuración con catálogos y equivalencias, trabajador periódico,
vínculo remoto, descarga del PDF y adaptador de impresión SumatraPDF. La emisión permanece desactivada por
defecto hasta configurarla expresamente. Falta validar respuestas reales de la cuenta, impuestos y numeración,
la recuperación completa de resultados inciertos y la impresión física; no se han emitido documentos de prueba.

### Consulta independiente de facturas de Alegra

En Observar ventas → **Facturas de Alegra**, consultar directamente la cuenta mediante GET /invoices.
Filtros inclusivos por fecha y paginación de 30; no requiere una venta local vinculada. El listado remoto puede
no incluir datos de emisión: se muestra «No informado en listado», sin inferir aceptación del estado comercial.
Abrir PDF consulta el documento individual con fields=pdf,xml. Los borradores no generan URL PDF.
Imprimir PDF permite seleccionar una impresora Windows y usa SumatraPDF.exe sin escalar el documento.
Esta ventana solo consulta y descarga documentos existentes: no crea, no emite y no registra una venta local.
Se debe comprobar que el PDF tenga plantilla de 80 mm; un PDF A4 no se reduce automáticamente.

Pruebas requeridas: pagos mixtos, impuestos incluidos, ajustes positivos/negativos, rechazo, timeout tras POST,
doble clic, reinicio, PDF pendiente, impresora desconectada y dos ejecuciones de migraciones.
