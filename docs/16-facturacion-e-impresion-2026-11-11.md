# Facturación electrónica e impresión: cambios del 11 de noviembre de 2026

Rama de trabajo: `day/2026-11-11` (sin commit al cierre de este documento). Continúa lo descrito en
[la sesión del 6 de noviembre](15-cierre-de-sesion-2026-11-06.md) y en
[doble impresión](14-doble-impresion-electronica.md). Qué se probó y qué no está al final.

## Resumen

| Área | Cambio | Dónde |
|---|---|---|
| Impresión 80 mm | Diseño nuevo de la factura, QR para térmica, fechas en 12 h con segundos | `scripts/Render-AlegraFiscalRepresentation.py` |
| Impresión remota | Imprimir cualquier factura de la cuenta Alegra | `AlegraPdfService.PrintRemoteAsync` |
| Ventana de facturas | Ventana independiente «Facturas Alegra» con grid, estado, filtros, caché, PDF e imprimir | `FrmAlegraInvoices.cs` |
| Facturar pedido | Precio editable por producto solo para Alegra; columnas del detalle | `FrmFacturarPedido.PriceEdit.cs` y builder |
| Datos del receptor | Catálogo de departamentos/municipios y actualización del cliente en Alegra | `FrmElectronicInvoiceCustomer.cs`, `AlegraPlaces.cs` |
| Otros | Fondo del formulario Principal; «# Facturas» por defecto 1 | `FrmPrincipal.cs`, designer |

## 1. Diseño de la factura de 80 mm

El renderer (`Render-AlegraFiscalRepresentation.py`, reportlab) genera el PDF de 80 mm desde el XML UBL y
el PDF oficial. Las validaciones no cambiaron (número, CUFE, total, QR, aceptación, datos del PDF oficial).

Orden y contenido, todo centrado salvo la tabla de detalle y los totales:

1. Logo en blanco y negro (1 bit, 33 mm, embebido en el renderer como base64; origen
   `Fotos/Logo Casa Grande Restaurante en blanco y negro.png`).
2. Separador fino.
3. Emisor: nombre, NIT o CC con dígito de verificación, dirección y ciudad, correo,
   «Régimen: Impuesto Nacional al Consumo - INC» (texto fijo pedido por el usuario).
4. Separador grueso.
5. Adquirente: nombre e identificación.
6. Separador, «FACTURA ELECTRÓNICA DE VENTA», número, «Emisión DIAN», forma de pago, método de pago, vencimiento.
7. Separador, DETALLE: tabla `#`, Producto, Cantidad, Precio, Imp (tarifa por línea). Precio es el unitario sin
   impuestos.
8. Subtotal, total del impuesto por tarifa (por ejemplo «Total impuesto nacional al consumo (8%)») y
   TOTAL CON IMPUESTOS. Si hay anticipos o redondeo, se añade TOTAL A PAGAR.
9. Notas del XML (términos y condiciones), separador.
10. QR, CUFE en texto, fecha de generación y fecha de validación.
11. Autorización de numeración.
12. Software y trazabilidad, y observaciones de validación de Alegra al final.

Decisiones que conviene revisar: se conservan las notas legales del XML, las observaciones de validación y las
leyendas de gran contribuyente/autorretenedor cuando aplican; se quitó el teléfono del emisor.

**Fechas.** Todas las fechas con hora salen en 12 h con segundos (`2026-11-06 05:45:58 PM`, función
`timestamp()`), sin el desfase `-05:00`. Vencimiento y vigencia no tienen hora y no cambian.

**QR para térmica.** Antes era vectorial de 53 mm escalado, con módulos de fracción de punto que el
controlador suaviza. Ahora es una imagen de 1 bit con corrección M, zona de silencio de 4 módulos y módulo de
8 puntos de 203 dpi (1 mm), máximo 560 puntos de ancho. Se decodificó con OpenCV a 203 dpi y con desenfoque;
a media resolución no lee. **La lectura en papel real sigue pendiente.**

**Pruebas.** `scripts/Test-AlegraFiscalRenderer.py` (9 pruebas sintéticas, incluye orden de secciones, imágenes
y módulos del QR en puntos enteros) y `--self-test` del ejecutable.

Tras cualquier cambio del renderer hay que regenerar el ejecutable (`scripts/Build-AlegraPdfTools.ps1`) y
después el instalador o la USB.

## 2. Ventana «Facturas Alegra» (Historial de ventas)

`FrmAlegraInvoices` es una ventana independiente: ni modal ni con propietario, con botón en la barra de tareas.
Si ya está abierta se reutiliza (`FrmObservarVentas` guarda la referencia).

- **Consulta:** solo lectura y descarga; nunca crea ni emite. Abre con el mes en curso. Controles de fecha
  `ModernDateField`, formato `06 nov. 2026`. Alegra ordena por fecha descendente y dentro de un día se ordena por
  ID descendente (entre páginas el orden de un mismo día puede no ser exacto).
- **Columnas:** Factura, Fecha, Cliente, Identificación (`CC 1053852532`, `NIT 900123456-1`), Subtotal,
  Impuestos, Total con impuestos, Estado, PDF, Imprimir.
- **Estado (pin de color):** verde emitida y aceptada (con o sin observaciones); amarillo pendiente o enviada
  esperando respuesta (o sin estado en el listado, o borrador); rojo rechazada, anulada u otro estado. El nombre
  sale en el tooltip. El listado de Alegra puede omitir el estado electrónico.
- **PDF:** abre el PDF oficial. **Imprimir:** `AlegraPdfService.PrintRemoteAsync` descarga el oficial (obligatorio,
  para la fecha de validación y el software), genera la representación de 80 mm y la envía a la impresora
  configurada o a la predeterminada. No usa ni cambia el estado de impresión de las ventas locales. Los
  borradores tienen ambos iconos desactivados.
- **Paginación:** 30 por página con « ‹ ›; la API no informa el total, «siguiente» se activa si la página vino llena.
- **Caché de páginas:** las páginas ya consultadas se muestran sin llamar a Alegra. Buscar (y abrir la ventana)
  descarta el caché y consulta de nuevo.
- **Filtros de cabecera** (reutilizan `SalesColumnFilters`): Factura, Cliente, Identificación y Estado (por nombre
  de estado). Se aplican a la página visible y se reinician al cambiar de página.
- **Pendiente de producto:** reenviar a emitir desde un pin amarillo no está implementado (reemitir crea
  documentos en Alegra y requiere decisión aparte).

## 3. Facturar pedido: precio editable solo para Alegra

**Cómo lo recibe la API.** Cada línea de `POST /invoices` lleva su propio `items[].price`, sin impuestos, que
sustituye al precio del catálogo sin modificarlo (`docs/alegra/contracts/ingresos-post-invoices.json`).

**Comportamiento (decisión del usuario).** El lápiz es la primera columna del grid Detalle del pedido y solo
existe con «Solicitar factura electrónica con Alegra» marcada; al desmarcarla se descartan los precios editados.
Al pulsarlo queda editable solo la celda Precio (solo dígitos); Enter guarda. Escribir el precio original quita la
edición. La fila editada se pinta en rojo pastel y el lápiz se vuelve rojo; el tooltip de Precio muestra el
original. No se pide código maestro ni observación (decisión explícita del usuario).

**Alcance.** El precio editado **no** cambia el pedido, el total cobrado, los pagos, el recibo local ni el precio
del producto en Alegra. Solo viaja a la factura electrónica.

**Implementación.**
- El snapshot del pedido añade `ElectronicPrice` a las líneas editadas (`BuildElectronicRequest`).
- `AlegraInvoiceBuilder` usa `ElectronicPrice × Cantidad` como importe de esa línea (precio unitario con
  impuestos, como el de la columna), sin pasar por el reparto del total, y rechaza valores menores o iguales a cero.
- El total de la factura puede diferir del de la venta; los pagos de Alegra se reparten en proporción al total de la
  factura, igual que en una factura parcial. `ExpectedTotal` incluye los precios editados, así que las
  validaciones posteriores (descarga del PDF, conciliación) siguen cuadrando.
- Código de la interfaz en `FrmFacturarPedido.PriceEdit.cs`: la edición no escribe en el `DataTable` del pedido
  (`CellParsing` devuelve el valor original y guarda el precio en un diccionario por `Id_detalle_pedido`).

**Otros cambios del grid.** «Cantidad» pasa a «Cant»; se oculta Observaciones; el nombre se muestra completo, con
las líneas que necesite. El campo «# Facturas» arranca en 1 (mínimo 1, máximo 5).

## 4. Datos para factura electrónica (receptor)

**Catálogo de departamentos y municipios.** Alegra recibe `address.department` y `address.city` como texto, pero
publica la tabla válida en https://developer.alegra.com/reference/colombia (33 departamentos, 1.123 municipios). No
hay endpoint de catálogo. La grafía es exacta, con sus rarezas: «Bogotá D.C.» como departamento y
«Bogotá, D.C.» como municipio, «Ciudad Bolivar», «N. de Santander». Coincide con los contactos reales de la cuenta.

- La tabla se embebió en `CapaNegocio/Alegra/ColombiaPlaces.txt` y se lee con `AlegraPlaces`
  (departamentos, municipios por departamento, emparejado sin distinguir mayúsculas ni tildes).
- El formulario usa listas: el municipio depende del departamento y ambos son obligatorios salvo con documento
  extranjero. Si el contacto de Alegra trae un valor que no está en el catálogo, la ventana avisa y exige elegirlo.
- País: Alegra no publica lista. Queda fijo en «Colombia» con documento nacional y solo es editable con PP, DIE o
  FOREIGN_NIT.

**Guardar cambios en Alegra.** Casilla «Guardar datos modificados en Alegra», encima de Confirmar datos, activa
solo con un cliente de Alegra seleccionado (editar el Documento la desactiva). Al confirmar,
`AlegraWorkflow.ContactChanges` compara el formulario con el contacto seleccionado y `AlegraClient.UpdateContactAsync`
envía `PUT /contacts/{id}` **solo con los campos modificados**: `kindOfPerson`, `regime`, `identificationObject`
(solo el DV), `name` o `nameObject` completo, `email`, `phonePrimary` y `address` completa. Mientras se actualiza,
una capa con barra animada cubre la ventana y no se puede cerrar; al terminar bien se cierra. Si falla, queda
abierta con el mensaje y se puede desmarcar la casilla para continuar sin tocar Alegra.

**Riesgo abierto.** La documentación no confirma que el PUT de contactos sea parcial (otros PUT de Alegra lo dicen
expresamente). Antes de usarlo en producción, probar con un contacto de prueba cambiando un solo campo y
comprobar que el resto no se altera. Si no lo es, habrá que enviar el contacto completo.

## 5. Otros

- Fondo del formulario Principal: `Fondo elegante de Casa Grande Restaurante.png`, embebido
  (`Resources/MainBackground.png`), con ajuste «cubrir» (llena el panel sin deformar y recorta los bordes).

## 6. Impresión según el papel de la impresora conectada (58 u 80 mm)

El ancho de papel no va fijo: se mide **cada vez que se imprime** con la impresora que se va a usar.

- `Servicios/PrinterPaper.cs` (`PrinterPaper.Detect`) lee el ancho imprimible que anuncia el driver: 48 mm → rollo de
  58 mm, 72 mm → rollo de 80 mm, más ancho → hoja. Si el driver deja una hoja grande como predeterminada, usa el ancho de
  sus formatos de rollo. Forzado opcional con la clave `AnchoPapelMm` de `App.config` (`58`, `80` o `auto`).
- **RDLC** (comandas, factura, precuenta, ingreso, egreso, reporte diario): `ControladorImpresion.Imprimir` detecta el
  papel; con rollo estrecho `Servicios/ReceiptLayoutAdapter.cs` reajusta el diseño en vez de encoger el ticket
  terminado: escala anchos y posiciones horizontales, baja las fuentes un 15 %, muestra los encabezados de la tabla de
  detalle como símbolos pequeños (`Nombre`, `$` precio, `#` cantidad, `Σ` total de la línea; constantes en el adaptador)
  y reparte las columnas para que los importes no se partan. Con rollo de 80 mm el comportamiento es el de antes.
  Los documentos de ticket que no eran «recibo» (ingreso, egreso) también se limitan ahora al ancho real del papel.
- **Factura de Alegra:** el renderer acepta `--paper-mm` (72 = rollo de 80, 48 = rollo de 58). `AlegraPdfService` detecta
  la impresora de destino, genera el PDF a su ancho y lo nombra `{venta}-58mm.pdf` o `{venta}-80mm.pdf`. En 58 mm la
  página mide 48 mm con margen de 1 mm, el logo 22 mm y el QR usa módulos de 6 puntos (0,75 mm); decodifica con el
  decodificador Aruco de OpenCV, también con desenfoque.
- **Importes sin decimales:** Precio, Total y Valor del pago de la factura y la precuenta salen como `$9900`, no
  `$9900.00` (`Format(Val(...),"0")`, no depende de la cultura). Aplica a 58 y 80 mm.
- **Precuenta:** la etiqueta «Mesa: MESA: 4» se repetía porque la base ya entrega `Mesa` como `MESA: 4`; ahora se quita
  el prefijo y sale «Mesa: 4» (en 58 y 80 mm).
- `Configurar-Impresora-Termica.ps1` acepta rollos de 45 a 85 mm (la XP-58 anuncia «58(48)mm x 3276mm»). Para una
  impresora nueva: `scripts/Install-ThermalPrinterUsb.ps1` (diagnóstico, controlador, cola y exportación para otra PC).
- Impresora de este equipo: **Xprinter XP-58** (NP L22L, USB001), controlador oficial de Xprinter.

**Decisión:** no se migran los RDLC al generador de PDF de Alegra (latencia, riesgo en comandas, la salida física de
ese camino aún no está validada y habría que reescribir 7 informes). Se reconsidera si se abandona .NET Framework
(ReportViewer es el bloqueo real); en ese caso, preferir C# en el mismo proceso.

**Pendiente:** imprimir en la XP-58 una comanda, una factura, una precuenta, un ingreso, un egreso y la factura de Alegra;
regenerar el ejecutable del renderer (`Build-AlegraPdfTools.ps1`) y el instalador; ejecutar `Test-LongReceipts.ps1`
(regresión de 80 mm, no se ha corrido tras estos cambios).

## Pruebas y verificación

- `scripts/Test-AlegraFiscalRenderer.py`: 9 pruebas; `Render-AlegraFiscalRepresentation.py --self-test`.
- `scripts/tests/AlegraContractTests.cs`: 52 comprobaciones sin red ni escrituras (precio personalizado, catálogo de
  lugares, cambios de contacto). Se ejecutan con `scripts/Test-AlegraContracts.ps1`.
- La solución compila con MSBuild y los reference assemblies de NuGet (como `run-local.ps1`).
- Capturas con datos sintéticos de la ventana de facturas y del fondo; el renderer se probó con una factura real
  (FE4938) en una página de 80 mm.

**No se probó:** salida en la térmica real (diseño y lectura del QR), las ventanas de Facturar pedido y de datos
del receptor ejecutándose con la aplicación completa, ni ningún PUT real contra Alegra.

## Siguientes pasos

1. Regenerar el ejecutable del renderer, el instalador y la USB; imprimir una factura en la térmica del restaurante.
2. Probar el PUT parcial de contactos con un contacto de prueba.
3. Probar de punta a punta una venta con precio editado: total de la factura, pagos de Alegra y PDF.
4. Decidir si el pin amarillo debe permitir reenviar a emitir.
5. Revisión contable del diseño de 80 mm y de las leyendas (pendiente desde el 6 de noviembre).
