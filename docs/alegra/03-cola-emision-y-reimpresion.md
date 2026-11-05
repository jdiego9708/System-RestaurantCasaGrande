# Etapa final: emisión en segundo plano y reimpresión por venta

Requisito del usuario, 2026-11-02. Implementar después de concluir contratos, captura fiscal,
configuración y construcción/validación de las solicitudes.

## Comportamiento acordado

- Guardar la venta local inmediatamente. Si solicita electrónica, guardar su solicitud en la misma
  transacción, sin esperar a Internet ni mantener la transacción abierta durante llamadas HTTP.
- Un trabajador en segundo plano toma las solicitudes persistidas, crea el documento remoto y guarda
  la relación `Ventas.Id_venta` → `AlegraInvoiceRequests.SaleId` → `RemoteId` de Alegra.
- Conservar por separado número visible de factura, ID remoto, estado de emisión y estado de impresión.
  Un ID de pago no debe reemplazar al ID de factura necesario para consultar su PDF.
- Actualizar estado mediante consultas periódicas a Alegra. La aplicación de escritorio puede reiniciar:
  la cola y el vínculo sobreviven. Un timer solo dispara trabajo asíncrono; no bloquea el hilo WinForms.
- El webhook es opcional futuro y exige un endpoint accesible, verificación y deduplicación de eventos.
  No abrir puertos públicos ni depender de un webhook para el funcionamiento inicial.
- Desde el detalle de venta: ver estado electrónico, abrir PDF y reimprimir factura de Alegra.
  Consultar por `RemoteId`, obtener el PDF, conservar copia local y enviarlo a la térmica de 80 mm.
- Mantener la factura que ya imprime el programa. Al aceptar la electrónica, imprimir además la de Alegra;
  no volver a imprimir automáticamente el comprobante local.

## Recuperación

Un timeout tras crear puede significar que Alegra sí creó el documento. Guardar UNCERTAIN y conciliar;
no repetir POST de creación sin comprobarlo. Para documentos con RemoteId conocido, consultar/reanudar
ese mismo ID. Las solicitudes requieren exclusión entre trabajadores y recuperación de trabajos interrumpidos.

No deducir aceptación por HTTP 200 ni por existencia de PDF. Revisar el estado electrónico real.
Reintentar descarga o impresión nunca debe crear otra venta, otro pago u otra factura.
Una impresión de resultado incierto tras reinicio requiere reimpresión explícita para evitar duplicados.

## Aceptación

1. El cobro local termina aunque Alegra esté desconectado; la solicitud queda pendiente.
2. Tras recuperar conexión o reiniciar, se conserva una sola relación con la factura remota.
3. Historial y detalle reflejan ID, número y estado sin duplicar documentos.
4. Reimprimir usa el mismo RemoteId y el PDF oficial; no llama a creación ni emisión.
5. Probar timeout tras POST, dos trabajadores, cierre durante emisión, 429, rechazo, PDF pendiente
   e impresora desconectada, con respuestas simuladas antes de la prueba controlada real.
