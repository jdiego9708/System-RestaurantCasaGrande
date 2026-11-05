# Representación gráfica para revisión contable

Se obtuvo el XML original de una factura existente usando `GET /invoices/{id}?fields=xml`.
La consulta sin campos también funciona. Solicitar `fields=pdf`, `fields=pdf,xml` o la descarga
1to1 produjo HTTP 500 en las pruebas; no se debe depender del PDF para consultar el estado.

`scripts/Render-AlegraFiscalRepresentation.py` genera un PDF de 80 mm a partir del XML UBL
y el detalle REST. No crea ni modifica ventas remotas, ni imprime físicamente. Verifica
coincidencia exacta del CUFE, total y contenido QR. Conserva el XML original y genera
un informe `.revision.json` con su SHA-256 y los pendientes. Los archivos fiscales reales
permanecen fuera del repositorio en `%LOCALAPPDATA%\CasaGrande\Alegra\PDF`.

El diseño incluye emisor y adquirente, identificaciones, direcciones disponibles,
responsabilidades fiscales, generación con zona horaria, productos, cantidades, unidades,
precios, descuentos de línea, bases, tributos y tarifas, totales, forma y medio de pago,
notas, autorización de numeración, vigencia, rango, identificación del software y CUFE/QR.

## Pendientes antes de entregar al cliente

- Obtener y revisar el contenedor con ApplicationResponse DIAN. El estado aceptado con
  observaciones reportado por Alegra no sustituye verificar ese archivo. No se verificó
  criptográficamente la firma XML en este script.
- Confirmar razón social del fabricante y nombre del software. El XML descargado contiene
  NIT del proveedor e ID del software; no inventar los datos restantes.
- Confirmar condición de agente retenedor de IVA y demás leyendas aplicables con el contador.
- Revisar integridad, legibilidad del diseño y QR, y requisitos vigentes de entrega.

Por esos pendientes el PDF se marca para revisión del contador y el informe indica
`listo_para_cliente: false`. No afirmar cumplimiento integral ni activarlo como reemplazo
automático del PDF de Alegra en producción todavía.

Referencia normativa: [Resolución DIAN 227 de 2025, compilación vigente](https://normograma.dian.gov.co/dian/compilacion/docs/resolucion_dian_0227_2025.htm),
artículos 1.5.1.2.2.1 (requisitos de factura electrónica), 1.5.1.2.2.3 (calidad de
retenedor IVA), reglas de entrega y anexo técnico aplicable. Una representación propia
debe conservar los datos de la factura real y satisfacer los requisitos aplicables;
no basta con añadir CUFE y QR a un recibo local.

## Segunda revisión del diseño y las fuentes

El PDF incorpora separadores de sección con fondo gris, bordes de celdas para cantidad,
unidad, precio y valor, y filas de totales con énfasis del importe final. El diseño usa
contraste en escala de grises y conserva el QR original del XML sin superponer líneas.
Se comprobó una sola página de 80 mm, contenido extraíble y ausencia de texto fuera de página.

Se contrastó el NIT 900559088 del XML con el [listado oficial DIAN de proveedores](https://micrositios.dian.gov.co/sistema-de-facturacion-electronica/proveedores-tecnologicos/):
corresponde a SOLUCIONES ALEGRA S.A.S. Esto permite identificar al proveedor tecnológico;
no confirma automáticamente la identidad del fabricante ni el nombre registrado del software.

Se incluyen literalmente las observaciones reportadas por Alegra, sin modificar el documento
fiscal: FAZ09 (identificación del bien o servicio) y FAJ44b (identificación/nombre frente al RUT).
El contador debe revisar su alcance y cualquier corrección mediante el mecanismo fiscal que
corresponda; cambiar el PDF no corrige una factura ya emitida.

La consulta pública del QR DIAN respondió HTTP 403 desde este entorno. No se obtuvo
ApplicationResponse. La API contable documenta `pdf`, `xml`, `comments` y `events` como
campos adicionales de consulta; no documenta allí un enlace de ApplicationResponse.
La [API e-provider](https://e-provider-docs.alegra.com/changelog/cambios-en-la-respuesta-al-crear-documentos)
documenta ZIP y respuesta DIAN, pero es otro servicio: no asumir que este token o ID de la
API contable funciona allí. Obtener el contenedor desde Alegra web o confirmar con soporte
el acceso de lectura correspondiente. No se crearon documentos ni se cambiaron permisos.

## Consulta desde Windows

En Facturas de Alegra, doble clic consulta el detalle por ID sin pedir PDF y abre pestañas
de productos/tributos, pagos, cliente, emisión y todos los campos de la respuesta. Permite
revisar borradores. Abrir PDF solicita primero el documento oficial con `fields=pdf`.
Ante HTTP 500 o enlace ausente, descarga el XML y genera la representación compacta para
revisión. El estado de la ventana distingue ambas procedencias. La impresión manual de
esa alternativa requiere confirmar el documento de revisión; la impresión automática
queda bloqueada para esa representación.

El renderer se copia al directorio de aplicación `Alegra`. Esta alternativa requiere
Python 3 en PATH y reportlab en la máquina donde se ejecuta; el instalador aún no instala
esas dependencias. El PDF oficial no necesita Python. La prueba local desde las DLL
compiladas consultó una factura existente, construyó el formulario y generó un PDF válido
sin crear documentos, modificar datos remotos ni enviar trabajos a una impresora.

El detalle utiliza tarjetas adaptables y badges: un producto por ficha con cantidades,
precio, descuento, total e impuestos; pagos con fecha, medio e importe; cliente con
contactos y dirección agrupados; emisión con estado, CUFE y observaciones completas en
campos copiables. «Todos los datos» conserva la respuesta completa, agrupada por objetos,
y se construye al seleccionar la pestaña para evitar demorar la apertura inicial.
El doble clic muestra un indicador centrado con barra animada hasta abrir el detalle;
se elimina también al fallar o cancelar. Se comprobó este ciclo con consulta real y se
inspeccionaron las cinco pestañas y el ajuste al tamaño mínimo de ventana.
