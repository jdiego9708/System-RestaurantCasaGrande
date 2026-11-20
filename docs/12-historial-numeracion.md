# Historial y numeración de ventas

Las fichas del historial seleccionan listas separadas:

| Ficha | Ventas incluidas | Número mostrado |
|---|---|---|
| Ventas / Total vendido | Ventas del programa, excepto domicilios | `N - #`, desde `SaleReceiptNumbers.ReceiptNumber` |
| Domicilios | Solo pedidos de tipo `DOMICILIO` que tienen venta | `N - #`, desde `Pedidos.DeliveryNumber` |
| Rappi | Órdenes importadas de Rappi | `R - #`, desde `RappiOrders.DayNumber` |

Cada secuencia es diaria e independiente. Ordenar, filtrar o cambiar de página conserva el número asignado. Los ingresos y egresos mantienen su numeración de filas.

## Persistencia de las ventas del programa

Aplicar `db/12_numeracion_ventas.sql` antes de usar esta versión. `Desplegar-Base-de-Datos.ps1` lo aplica y `Preparar-USB.ps1` lo incluye en el despliegue autocontenido.

El trigger `tr_Ventas_AssignReceiptNumber` guarda el número al insertar una venta, dentro de la misma transacción que la venta y sus pagos. Usa un bloqueo transaccional entre equipos y admite inserciones de varias filas y fechas. `SaleReceiptCounters` conserva el último número del día y `SaleReceiptNumbers` relaciona cada venta con su número. Estas tablas se separan de `Ventas` para conservar los INSERT heredados sin lista de columnas. Se conservan las referencias de ventas eliminadas para no reutilizar números.

La factura y sus reimpresiones consultan el número guardado; imprimir ya no aumenta el contador. Los domicilios y Rappi mantienen sus secuencias existentes del script 11.

El contador anterior vivía en la configuración del usuario de Windows y aumentaba con cada impresión. No guardaba qué número correspondía a cada venta. Por eso no se numeran retrospectivamente las ventas antiguas: el historial muestra `—` y su reimpresión omite ese número. La nueva secuencia empieza en 1 cada día a partir de la migración; puede coincidir con números impresos por la versión anterior el día de actualización.

La migración no cambia la numeración de factura electrónica de Alegra. No se debe ejecutar el script de reversión sin respaldo: elimina los números guardados.

## Desglose de pagos de Rappi

El grid muestra **Efectivo**, **Otros medios** y **Valor del pedido**. Los datos originales ya se guardan en `RappiOrders`: `Total` es el «TOTAL A COBRAR» del PDF, `PaymentMethod` indica cómo cobrar ese saldo y `PaidThroughRappi` contiene lo ya pagado en la plataforma. Esta separación no requiere otra migración ni volver a importar las órdenes guardadas con esos datos.

- **Efectivo:** el saldo `Total` cuando la forma de pago es Efectivo/Cash.
- **Otros medios:** el valor absoluto de `PaidThroughRappi`, más el saldo `Total` si se cobra por datáfono, transferencia u otro medio distinto de efectivo.
- **Valor del pedido:** saldo a cobrar más lo pagado a través de Rappi. No se suman de nuevo las columnas del desglose al KPI.

Por ejemplo, un pedido de $30.000 con $20.000 pagados en Rappi y $10.000 pendientes en efectivo muestra $10.000 en Efectivo y $20.000 en Otros medios. `CashPayWith` («el cliente paga con $50.000») no es el importe de la venta y no se usa en el desglose. Si falta el total o la forma de cobro de un saldo pendiente, el desglose muestra `—` para evitar asignarlo a un medio sin evidencia.

La lectura admite las etiquetas «Método de pago» y «Forma de pago», con el medio en la misma línea o en la siguiente. Los importes de los totales también pueden estar en la línea siguiente.

## Verificación

`scripts/Test-DatabaseDeployment.ps1` usa una base temporal y comprueba que la migración se pueda repetir, la separación de domicilios, la numeración de lotes, el reinicio diario, el rollback de venta y contador, la conservación de números tras anulaciones/eliminaciones y dos conexiones facturando simultáneamente. No utiliza ventas reales.

`scripts/Test-RappiPayments.ps1` comprueba lectura de etiquetas, pagos mixtos, efectivo, RappiCard, datáfono, transferencia, datos ausentes, persistencia y recuperación en SQL, duplicados y columnas/orden/filtros del grid. Usa una base temporal y datos ficticios.
