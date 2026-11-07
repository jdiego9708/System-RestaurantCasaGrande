# 05 — Reenvío desde el detalle de venta y conciliación de resultados inciertos

Pensado el 6 de noviembre de 2026, tras comprobar que un fallo de red o de Alegra deja la venta local
guardada y la factura electrónica a medias. Se hace en dos etapas.

## Etapa 1 — hecha

Pantalla: Observar ventas → doble clic en una venta → **Detalle de venta**
(`FrmSaleDetails.cs`). Antes mostraba una línea con el estado crudo (`ACCEPTED`, `NEEDS_REVIEW`…)
y no tenía forma de reintentar. Ahora:

- El estado sale en español, con color y una explicación de qué hacer.
- Un botón de reintento cuyo texto y efecto dependen del estado:

| Estado | Mensaje | Botón | Qué hace |
|---|---|---|---|
| `PENDING`, `CREATED`, `ISSUING`, `CREATING` | Se reintenta automáticamente | **Reintentar ahora** | `AlegraWorkflow.Expedite`: adelanta el siguiente intento del worker (no espera los 15 s) |
| `NEEDS_REVIEW` | Requiere revisión | **Reintentar envío** | Pide confirmación y llama a `AlegraWorkflow.Retry`: vuelve a `PENDING`/`CREATED` **con el mismo `RemoteId`**, así que no crea otra factura |
| `REJECTED` | Rechazada por la DIAN | **Volver a consultar** | Igual que `Retry`. Solo relee el estado en Alegra; **no corrige** una factura rechazada por datos |
| `UNCERTAIN` | Resultado incierto | *(ninguno)* | Ver etapa 2 |
| `ACCEPTED` + impresión pendiente/fallida | Emitida, impresión sin confirmar | *(ya existía «Imprimir Alegra»)* | Reimprime con el renderer de 80 mm |
| `ACCEPTED` + `PRINTED` | Emitida y entregada | — | — |

`Expedite` (`DAlegra.Expedite`) solo adelanta solicitudes sin trabajo en curso y no toca las
`ACCEPTED` con error de descarga/impresión: esas esperan su reintento normal. La misma función la usa la
ventana de espera de Facturar pedido.

### Límites conocidos de la etapa 1

- **Rechazo por datos del receptor** (identificación, régimen…): reintentar volverá a dar rechazo. Hace falta
  poder editar los datos fiscales de la solicitud. No está hecho.
- `Retry` no avisa si no cambió ninguna fila (por ejemplo, si el worker justo tenía la solicitud tomada). El
  detalle se refresca solo cada 15 s y mostrará el estado real.
- Una venta guardada **sin** marcar factura electrónica no tiene solicitud; facturarla después es otra
  funcionalidad, no está incluida.

## Productos sin vincular: aviso previo y factura parcial (6-nov-2026)

Antes de guardar la venta, con «Facturar con Alegra» marcado, `FrmFacturarPedido` revisa que cada producto
tenga vínculo con Alegra. Si faltan, `FrmUnlinkedProductsDialog` lista los que faltan y ofrece:

| Botón | Efecto |
|---|---|
| **Enviar solo los vinculados** | Se guarda la venta con `onlyLinked: true` en el snapshot. A Alegra va una factura **solo con los productos vinculados** |
| **No enviar a Alegra** | Se desmarca la casilla y la venta se guarda sin factura electrónica |
| **Volver** | No se guarda nada; se puede vincular y repetir |

Si ningún producto está vinculado solo quedan «No enviar» y «Volver».

**Qué incluye la factura parcial.** Total = productos vinculados (con su parte del descuento, como siempre) +
propina + desechables + domicilio. Los cargos van completos aunque se omitan productos. Los pagos de la
factura se reparten en proporción a lo realmente pagado, de modo que sumen ese total (el medio fiscal de la
cabecera sigue siendo el del pago mayor). La **factura local y los pagos de la venta no cambian**.

**Qué no cambia el sistema:** el total de la venta queda mayor que el de la factura electrónica. La diferencia
queda solo en el comprobante local. Es una decisión fiscal del negocio: confirmar con el contador que una
factura electrónica parcial es válida para esa venta.

Implementación: `AlegraInvoiceBuilder.UnlinkedProducts / AnyLinked / ExpectedTotal / ExcludedProducts`.
El worker compara el total de Alegra con `ExpectedTotal` (no con el total de la venta). El detalle de venta
marca «PARCIAL». Límite conocido: el total esperado se recalcula con la configuración vigente; si se vincula
un producto omitido después de crear la factura parcial, el chequeo de total fallará y la solicitud quedará
en «Requiere revisión» (no se emite nada equivocado).

## Etapa 2 — pendiente: conciliar `UNCERTAIN`

### El problema

`UNCERTAIN` significa que se envió `POST /invoices` y no se confirmó el ID de la factura: se cortó la
conexión, hubo timeout, o Alegra respondió sin ID. Puede que la factura **exista** en Alegra o **no**.
Reenviar a ciegas puede duplicar una factura fiscal; no reenviar puede dejar una venta sin factura.
Por eso la etapa 1 no ofrece botón en este estado (regla ya vigente: «no volver a crear una factura si el
resultado es incierto»).

### Cómo se concilia hoy (a mano)

Así se hizo con FE4931: consultar las facturas del día por API y buscar una cuyo campo `observations` contenga
`CasaGrande solicitud <RequestId>`. `AlegraInvoiceBuilder` escribe ese texto al crear la factura
(`observations`), y el `RequestId` está en `AlegraInvoiceRequests`.

### Propuesta

Botón **Conciliar con Alegra** en el detalle, solo para `UNCERTAIN`:

1. Consultar `GET /invoices` filtrando por fecha de la venta (con margen de un día por la zona horaria),
   paginando, usando la credencial de emisión (Basic) que sí devuelve estado y datos completos.
2. Buscar facturas cuyo `observations` contenga el texto exacto con el `RequestId`.
3. Según el resultado:
   - **Una coincidencia**: guardar su `RemoteId` y número, pasar la solicitud a `CREATED` y dejar que el worker
     siga con la emisión. Comprobar antes que el total coincide con la venta.
   - **Ninguna** (consulta completa, sin errores de red): mostrar «Alegra no tiene esta factura», y con
     confirmación del usuario pasar a `PENDING` para reenviar.
   - **Más de una**: no tocar nada; mostrar los números para revisión humana.
   - **Consulta incompleta o con error**: no concluir nada; mantener `UNCERTAIN`.
4. Registrar en `LastError`/bitácora qué se decidió y por qué.

### Decisiones a tomar antes de implementarla

- Ventana de fechas y límite de páginas de la consulta (cuántas facturas hay por día en el restaurante).
- Si el paso «ninguna coincidencia → reenviar» debe requerir clave de administrador, por el riesgo fiscal.
- Dónde vive la lógica: método nuevo en `AlegraWorkflow` (`ReconcileAsync`), sin lógica en el formulario.
- Pruebas: simular timeout tras `POST` con y sin creación real, en el entorno de pruebas de contratos
  (`scripts/Test-AlegraContracts.ps1`), nunca emitiendo en producción.

### Estados que no cambian

`ACCEPTED` nunca se vuelve a emitir. El renderer y la reimpresión solo descargan e imprimen: no crean ni emiten.
