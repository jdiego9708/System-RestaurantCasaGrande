# Contratos API y catálogo Colombia

Consulta: 2026-11-02. Fuente de contratos: MCP `https://developer.alegra.com/mcp`, herramienta
`get-endpoint`. No se utilizó `execute-request`, token ni datos de la cuenta del restaurante.

## Fuentes y reproducción

- [Catálogo Colombia](https://developer.alegra.com/reference/colombia).
- Contratos completos, con requestBody, responses, ejemplos y componentes: carpeta `contracts/`.
- `contracts/manifest.json`: especificación, método, ruta y SHA-256 de cada contrato descargado.
- Actualización: `scripts/Get-AlegraApiContracts.ps1`. Solo invoca herramientas de documentación.

El catálogo del país define los códigos fiscales; los contratos definen la estructura JSON.
Los ejemplos publicados no son respuestas observadas de nuestra cuenta ni deben copiarse literalmente:
contienen identificadores de ejemplo y algunas erratas. Conservar el original para rastrear discrepancias.

## Llamadas de la integración

Base REST documentada: `https://api.alegra.com/api/v1`, autenticación HTTP Basic.
El servidor se identifica como producción: estos contratos no prueban que exista sandbox.

| Operación | Request | Contrato local |
|---|---|---|
| Empresa | GET /company | configuraciones-get-company.json |
| Buscar contactos | GET /contacts | contactos-get-contacts.json |
| Crear receptor | POST /contacts | contactos-post-contacts.json |
| Consultar/editar receptor | GET/PUT /contacts/{id} | contactos-get-contacts-id.json / contactos-put-contacts-id.json |
| Productos | GET /items y /items/{id} | inventario-get-items*.json |
| Impuestos | GET /taxes y /taxes/{id} | configuraciones-get-taxes*.json |
| Numeración | GET /number-templates y /number-templates/{id} | configuraciones-get-number-templates*.json |
| Bancos/cajas | GET /bank-accounts y /bank-accounts/{id} | bancos-get-bank-accounts*.json |
| Crear factura | POST /invoices | ingresos-post-invoices.json |
| Consultar factura/PDF | GET /invoices/{id}?fields=pdf,xml | ingresos-get-invoices-id.json |
| Emitir existente | POST /invoices/stamp | ingresos-post-invoices-stamp.json |
| Vista previa | POST /invoices/preview | ingresos-post-invoices-preview.json |
| Pagos | GET/POST /payments | ingresos-get-payments.json / ingresos-post-payments.json |

## Receptor colombiano con facturación electrónica

En el contrato de contactos, seleccionar la variante `Contacto para version Colombia con facturación electrónica`.
Requiere `identificationObject`, `regime`, `kindOfPerson`.

- `identificationObject`: `number`, `type`; `dv` obligatorio para NIT.
- Persona natural: `nameObject` con `firstName`, `lastName`; `secondName` opcional. No separar un nombre completo automáticamente.
- Otras personas: `name` según la condición documentada.
- Identificación nacional: dirección fiscal con `address.address`, `city`, `department`.
- Identificación extranjera: `address.country` según contrato.
- `regime` debe corresponder al receptor; no heredarlo del restaurante ni asignar un régimen por defecto.

El catálogo Colombia publica RC, TI, CC, TE, CE, NIT, PP, PEP, DIE, NUIP y FOREIGN_NIT.
El enum del contrato de creación descargado no incluye PEP, aunque el catálogo sí. Esta discrepancia debe
verificarse antes de ofrecer PEP para creación remota. El contrato también admite OTHER_ENTITY, además de
PERSON_ENTITY y LEGAL_ENTITY; el catálogo resumido solo muestra los dos últimos.

El formulario de captura inicial aún requiere adaptación a estos contratos: faltan régimen, nombres separados
y validación específica del documento. No considerar esa captura lista para enviar a Alegra.

## Factura colombiana

Usar `PostColombia`, que hereda `PostGeneral`. Campos generales requeridos: `date`, `dueDate`, `client`, `items`.
Para electrónica 2.1: `paymentForm` obligatorio; si es CASH, también `paymentMethod`.
En el catálogo Colombia CASH significa **contado**, no necesariamente que se pagó en efectivo.

No confundir el medio fiscal de la factura (códigos Colombia como CASH, CREDIT_CARD, DEBIT_CARD o
CREDIT_TRANSFER) con `payments[].paymentMethod` (enum del contrato: cash, check, transfer, deposit,
credit-card, debit-card). Los pagos mixtos necesitan ambos mapeos, definidos explícitamente.

**Medio fiscal de la cabecera (corregido el 6-nov-2026).** Antes la cabecera llevaba siempre
`FiscalPaymentMethod` (CASH) y una venta pagada por transferencia salía como efectivo (FE4935). Ahora se
deriva del pago: `cash`→`CASH`, `transfer`→`BANK_DEPOSIT` (Consignación bancaria, decisión del usuario; el
catálogo «Medios de pago» de https://developer.alegra.com/reference/colombia también tiene `CREDIT_TRANSFER`,
`CREDIT_TRANSFER_BANK` y `DEBIT_TRANSFER_BANK`, pendientes de validar con el contador), `credit-card`→`CREDIT_CARD`,
`debit-card`→`DEBIT_CARD`; cheque y consignación usan `FiscalPaymentMethod` como respaldo. Con pagos mixtos,
la cabecera (que admite un solo valor) usa el medio del pago de mayor valor; `payments[]` conserva todos.
`paymentForm` sigue en CASH (contado).

`payments[]` vincula fecha, cuenta receptora y monto. No confundir la cuenta de banco/caja con el código
del medio de pago. Validar los campos descritos como obligatorios aunque falte un array `required` en el esquema.

Los IDs son strings. `items[].price` excluye impuestos y descuentos; `discount` es porcentaje por línea.
La propina colombiana usa `additionalCharges` (máximo un elemento), con `id` UUID del cargo y `amount`.
No representar la propina como impuesto ni como un producto sin validar el tratamiento contable.

Crear y emitir separadamente permite guardar primero el ID remoto. Si se usa `stamp.generateStamp=true`
al crear, el contrato advierte que un HTTP 400 puede devolver una factura ya creada en borrador más el error
de emisión. **Un 400 no garantiza que no se creó el documento.**

## Emisión y respuesta

Request documentado para emitir una factura existente:

```json
{"ids":["ID_FACTURA"]}
```

Máximo 10 documentos. El ejemplo de éxito documentado tiene estructura:

```json
{"data":[{"id":"1","success":true,"message":"..."}]}
```

Los errores por documento usan `id`, `code`, `message`. La respuesta puede mezclar resultados exitosos
y fallidos: HTTP 200 no acredita por sí solo la emisión de todos los IDs. Revisar el elemento correspondiente
y después consultar la factura. Código 3226 documentado: documento ya emitido; conciliar, no recrear.

Según catálogo, `emission_status`: PENDING, STAMPED_AND_WAITING_RESPONSE, STAMPED_AND_ACCEPTED,
STAMPED_AND_REJECTED, STAMPED_AND_ACCEPTED_WITH_OBSERVATIONS. No confundirlo con `status` comercial
(draft/open/etc.) ni con el campo de estado local. Preservar valores desconocidos para diagnóstico.

## PDF y respuestas incompletas

GET de factura admite `fields=pdf,xml,comments,events`. El contrato señala que borradores no generan URL PDF.
El ejemplo general de respuesta consultado no documenta una respuesta completa con PDF/XML y emisión
colombiana: validar la forma concreta de esos campos con una consulta de solo lectura a una factura existente.
No inventar `pdf.url`, CUFE u otros caminos JSON que el ejemplo no confirma.

La URL se usa para descargar el documento; conservar el archivo, no asumir una URL permanente. La salida
debe comprobarse en 80 mm antes de habilitar impresión automática de ambas facturas.

## Cobertura y pendientes

Se descargaron 21 contratos y cinco índices. Las respuestas reales, permisos del token, cuentas receptoras,
numeración vigente, configuración tributaria, ID de propina y plantilla térmica siguen sin comprobarse.
La lectura de documentación no autoriza ni ejecuta emisión. Mantener estos pendientes separados de los
contratos confirmados para no presentar muestras de OpenAPI como resultados de producción.
