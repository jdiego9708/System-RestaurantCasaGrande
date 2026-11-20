# 11 · Módulo de Domicilios (escritorio) — guía de traspaso

Estado a **2026-11-02**. Todo lo de esta guía está **sin commit** (ver §7). Lo anterior del día ya está en `main`.

## 1. Qué hace

Un domicilio es un pedido de `Pedidos` con `Tipo_pedido = 'DOMICILIO'` e `Id_mesa = 0` (la mesa 0 es el comodín).
La dirección de entrega sale de la tabla nueva `Domicilios` (varias por cliente) y se guarda en `Pedidos.Id_direccion`
(NULL en pedidos de mesa).

Flujo: **menú «Domicilios»** → `FrmDeliveries` (KPIs + 3 paneles) → **REALIZAR PEDIDO** → `FrmDeliveryCustomer`
(buscar/crear cliente + elegir/crear dirección) → `FrmPedido` con `IsDomicilio = true` → tarjeta en EN PREPARACIÓN →
**Facturar** (`FrmFacturarPedido`, `IsDomicilio = true`) → pasa a TERMINADOS.

### 1.1 Tiempo de preparación y cuenta regresiva (2026-11-13)

Cada domicilio tiene **45 minutos** de preparación contados desde `Fecha_pedido` + `Hora_pedido`. Mientras le queda
tiempo está en EN PREPARACIÓN con un anillo de cuenta regresiva (`UI/CountdownRing`) que pasa de verde a ámbar y de
ámbar a rojo de forma continua; la franja lateral y la píldora de estado de la tarjeta siguen ese mismo color, y en el
último tramo el centro del anillo parpadea. Al agotarse, la tarjeta pasa sola a TERMINADOS.

- **No se guarda nada en la base.** La etapa se deduce de la hora del pedido (`DeliveryTiming.StageOf`), así que la
  cuenta sobrevive a cerrar la aplicación y no hay script que desplegar en el restaurante.
- **Facturación automática (2026-11-14).** Al crear un domicilio se pregunta la forma de pago (EFECTIVO, TRANSFERENCIA o
  TARJETA, `FrmPaymentMethodDialog`) y se guarda en `Pedidos.Payment_method` (script `db/09_pedidos_forma_pago.sql`). Cuando
  el tiempo se agota, `FrmDeliveries.RefreshDeliveries` llama a `DeliveryAutoBilling.TryBill`: registra la venta (un solo
  pago por el total, sin propina ni descuento ni costo de domicilio) con `NVentas.InsertarVenta` y deja el pedido
  `DISPONIBLE` («Facturado»). Corre al abrir el módulo y cada refresco, así que alcanza a los que vencieron con el programa
  cerrado. No imprime la factura. `InsertarVenta` ya rechaza facturar dos veces el mismo pedido.
- **Sin forma de pago guardada** (domicilios anteriores a la pregunta o script 09 sin aplicar) también se factura solo,
  como EFECTIVO, y la venta lo anota en sus observaciones («forma de pago no registrada, se asumió efectivo»).
  «Por facturar» solo se ve si la venta falla (p. ej. total 0 o error de base de datos); se reintenta en cada refresco.
- Facturar a mano antes de los 45 minutos también lo pasa a TERMINADOS, como antes.
- Etapas (`DeliveryStage`): `Preparing`, `AwaitingBilling`, `Billed`. EN PREPARACIÓN se ordena por tiempo restante.
- **Panel RAPPI (2026-11-14): fichas leídas de facturas PDF.** Sin integración por API, el escritorio vigila la carpeta
  `Documentos\Casa Grande\FacturasRappi` (clave opcional `RappiInvoicesFolder` en `appSettings`: ruta completa, o relativa
  a Documentos). Solo informativo: no crea pedidos ni ventas.
  - **Lectura** (`CapaNegocio/Rappi/RappiInvoiceParser`): el PDF trae texto real (sin OCR); se lee con PdfPig 0.1.11 y se
    interpreta por líneas y etiquetas (ID de orden, cliente, fecha, teléfono, dirección —une líneas y guiones de corte—,
    método de pago, productos `2x NOMBRE $55.000`, subtotal, descuento, envío, propina, tarifa, total, «paga con», cambio y
    código de entrega). Nunca lanza por un dato faltante: la orden queda `IsComplete = false` y la ficha lo avisa.
  - **Sin duplicados** (`RappiImporter`, tabla `RappiOrders`, script `db/10_rappi_ordenes.sql`): la clave es el **ID de
    orden de Rappi** (`ExternalId` único). Dos copias del mismo PDF, o dos archivos distintos de la misma orden, dan una
    sola ficha. Antes de leer, la huella SHA-256 del archivo evita releer lo ya conocido. Un PDF a medio copiar se espera;
    uno ilegible se reporta una vez y no frena a los demás. Si el PDF no trae ID se usa `HASH:<huella>`.
  - **Carpetas por día** (2026-11-14): la raíz `FacturasRappi` es la bandeja de entrada (aquí se imprime desde la web de
    Rappi con una impresora PDF que guarde sola, sin diálogo, en esa carpeta). Al importar, la factura de una orden nueva
    pasa a `FacturasRappi\AAAA-MM-DD\` (el día de la orden según el PDF); una copia o un segundo archivo de una orden ya
    guardada pasa a `FacturasRappi\Duplicados\AAAA-MM-DD\`; si el nombre ya existe se numera (`a (2).pdf`), nunca se
    pisa. Un PDF ilegible se queda en la raíz (puede estar a medio copiar) y se avisa una vez. Una factura ya leída que
    sigue sin ordenar en la raíz (importada antes de existir las carpetas) se ordena en su día sin contarla duplicada. Solo
    se lee la raíz; las subcarpetas no se vuelven a leer.
  - **Panel** (`RappiPanel` + `RappiCard`): `FileSystemWatcher` más barrido cada 30 s (el watcher puede perder eventos);
    todas las órdenes **del día en curso**, las más recientes primero, con **buscador** (orden, cliente, teléfono,
    dirección, producto, método de pago, código) y **paginación** de 10. Ficha plegada por defecto con botón Expandir. El
    indicador RAPPI de arriba cuenta las órdenes del día. La importación corre en segundo plano.
  - **Historial de ventas** (`FrmObservarVentas` / `SalesDashboard`): novena ficha de indicadores **RAPPI** con el valor
    total y la cantidad de ventas del periodo elegido (`ventas por Rappi`). Al pulsarla el listado pasa a la vista Rappi
    (como ingresos y egresos: sin detalle ni eliminación) con datos básicos: fecha, hora, orden de Rappi, cliente,
    teléfono, forma de pago y **valor del pedido** (`RappiOrder.OrderValue` = total a cobrar + valor pagado a través de
    Rappi, para que un pedido pagado con RappiCard —total a cobrar 0— cuente por lo que valió). Sale de `RappiOrders` por
    la fecha de la orden; sin la tabla (script 10 sin aplicar) la ficha queda en «—» sin molestar.
  - Menús con opciones: las líneas «ELIGE TU ARROZ: 1x Arroz de Coco -» son opciones del producto (`RappiOrderItem.Notes`),
    no productos ni parte de su nombre; se guardan en `ProductsText` como líneas `> opción`.
  - **Reimprimir la orden** (icono de impresora al final de cada fila de la vista Rappi): imprime un **ticket térmico**
    armado con lo guardado de la factura (`RappiTicketPrinter` + `RappiTicket.rdlc`, mismo pipeline y ajuste a 58/80 mm
    que los demás recibos): logo, orden, cliente, dirección, productos con sus opciones, valores, forma de pago, cambio
    y código de entrega. No se reimprime el PDF original: es tamaño carta y en un rollo quedaría ilegible. Va a la
    impresora predeterminada, igual que comandas y facturas.
  - Tablas/archivos: `DRappiOrders`, `NRappiOrders`, `RappiOrder`.
- **Detalle de los platos en la factura de domicilio (2026-11-14).** La precuenta y la factura final de domicilio imprimen, debajo
  de cada plato con ingredientes, lo mismo que las comandas de mesa (sopa, crema, arroz, acompañantes…):
  `FrmFacturaFinal.AddIngredientDetails` agrega al nombre del plato sus `Detalle_ingredientes_pedido` (`NPedido.GetIngredientLines`).
  Solo se llama en las dos ramas de domicilio; las facturas y precuentas de mesa no cambian.
- **Agregar productos a un domicilio en preparación** (botón Agregar de la tarjeta): al guardar se imprime la factura
  **completa** (lo anterior y lo nuevo, con el detalle de los platos) y **conserva el mismo número del día**: el número se asigna
  solo al crear el pedido y la edición no lo toca. Si se entra a agregar y no se agrega nada, no se imprime.
- **Numeración diaria de facturas (2026-11-14, script `db/11_numeracion_domicilios.sql`).** Solo Domicilios: cada factura
  lleva arriba a la derecha «N° k», con k de 1 a N que empieza de nuevo cada día. Son **dos secuencias separadas**:
  - *Programa:* `Pedidos.DeliveryNumber` (solo `Tipo_pedido = 'DOMICILIO'`, por `Fecha_pedido`). Se asigna al **crear**
    el domicilio (`FrmPedido`) y lo conservan la precuenta, la factura final y las reimpresiones. Quien lo necesite y no
    lo tenga (domicilio anterior) lo recibe al imprimir (`NPedido.GetOrAssignDeliveryNumber`).
  - *Rappi:* `RappiOrders.DayNumber` por el día de la orden (`NumberDay`). Se asigna al **importar** el PDF, en orden de
    llegada; el ticket de Rappi lo muestra.
  - Es un número de orden del día, **sin valor tributario**. Asignar es atómico (bloqueo + índice único por día): 12 altas
    simultáneas dan 12 números distintos y seguidos. Sin el script 11 las facturas salen sin número y no se rompe nada. PdfPig y sus dependencias (`System.Memory`, etc.) están
    en `CapaNegocio/packages.config`; `packages/` no está en git → `nuget restore`.
- La duración se puede cambiar con la clave opcional `DeliveryPreparationMinutes` de `appSettings` (sin ella, 45).
- Un `Timer` de 1 s en `FrmDeliveries` mueve todos los anillos sin consultar la base; solo recarga cuando una cuenta
  llega a cero. El refresco de 10 s sigue igual.
- La tarjeta es `FormsDomicilios/DeliveryCard` (dibujada entera; altura según el contenido). Sus datos van en
  `DeliveryDetails`, aparte, para poder dibujarla sin base de datos. **`DomicilioSmall` ya solo lo usa `FrmDomicilios`,
  que es código muerto** (§6.4).
- Pruebas: `scripts\Test-DeliveriesRendering.ps1` (reparto por tiempo, colores, tarjetas a varios anchos y la ventana);
  con `-ScreenshotFolder <carpeta>` deja capturas PNG.
- Solo se cronometran los domicilios **de hoy** (las consultas `DOMICILIOS PENDIENTES` / `OTROS` filtran por fecha).
  El panel de Rappi no se tocó.

## 2. Base de datos — `db/06_domicilios_direcciones.sql` (+ `.revert.sql`, + `pruebas-sp-domicilios.sql`)

Idempotente. Aplicado en `CasaGrandeBD` y `CasaGrandeBD_prod` (LocalDB). **Falta en el restaurante.**

- Tabla `Domicilios(Id_direccion, Id_cliente→Clientes, Alias, Direccion, Barrio, Referencia, Telefono_contacto, Es_principal, Estado)`.
  Migra la `Clientes.Direccion_cliente` existente como principal.
- `Pedidos.Id_direccion int NULL` + FK. `sp_Insertar_pedido` se reescribió con `@Id_direccion = NULL` y lista de columnas
  (antes hacía `INSERT ... VALUES` posicional: añadir la columna lo habría roto). `sp_Movil_Insertar_pedido` ya nombraba columnas.
- SPs: `sp_Domicilios_listar`, `sp_Domicilio_obtener`, `sp_Domicilio_guardar` (la primera dirección es principal; marcar una
  principal desmarca las demás), `sp_Domicilio_inactivar` (no borra; reasigna la principal), `sp_Clientes_buscar_rapido`,
  `sp_Cliente_obtener`, `sp_Cliente_editar_basico`.
- Registrado en `scripts/Desplegar-Base-de-Datos.ps1` (paso 5) y copiado a `Soporte\db` por `scripts/Preparar-USB.ps1`.

## 3. Código

| Capa | Archivos |
|---|---|
| Entidades | `CapaEntidades/Models/CustomerAddress.cs`; `Pedidos.AddressId` (`int?`) |
| Datos | `DAddresses.cs` (nuevo); `DClientes`: `SearchQuick`, `GetById`, `InsertCustomer`, `UpdateCustomer`; `DPedidos.InsertarPedido` manda `@Id_direccion` solo si hay |
| Negocio | `NAddresses.cs` (nuevo); `NClientes`: `SearchQuick`, `GetById`, `InsertCustomer`, `UpdateCustomer` |
| Presentación | `Formularios/FormsDomicilios/FrmDeliveries.cs`, `FrmDeliveryCustomer.cs`; `FormsPedido/DomicilioSmall` (tarjeta, rediseñada); `FormsPedido/FrmPedido` (`AddressId`, `AddressText`, `ShowDeliveryHeader`); `FormsPrincipales/FrmPrincipal` (botón por cargo) |
| Reportes | título «DOMICILIO» en comandas (`FrmComandas.IsDelivery`), factura final (`FrmFacturaFinal`) y precuenta (`Precuenta.rdlc`, expresión sobre `Mesa`) |

Menú por cargo (`FrmPrincipal.ApplyDeliveryLayout`): DOMICILIARIO ve Domicilios en lugar de Mesas; ADMINISTRADOR ve ambos;
los demás solo Mesas. El botón se crea en código copiando el aspecto de Mesas.

## 4. Tema de interfaz (`CapaPresentacion/UI`, ver `docs/10-estilos-ui.md`)

`Theme`, `ModernCard`, `ModernButton`, `KpiCard`, `ModernTextBox`, `ThemeExtensions`. Iconos con **FontAwesome.Sharp 6.6**
(`packages.config`; `packages/` no está en git → `nuget restore`). `FrmDeliveries` es la pantalla de referencia.
Formularios nuevos: construir con estos controles y llamar `ApplyTheme()`.

## 5. Problemas encontrados (no repetir)

- **`sp_Buscar_clientes` está roto**: devuelve las filas y termina con `COMMIT` sin transacción → `RAISERROR` → ADO.NET lanza →
  `NClientes.BuscarClientes` devuelve `null`. Por eso se crearon los SPs «rápidos». No se tocó (otras pantallas lo usan).
- **`InsertarCliente` no devuelve el Id** → `InsertCustomer`.
- **No usar `TextBox.GetLineFromCharIndex` durante la construcción**: crea el handle antes de tiempo y lanza
  `Win32Exception: Error al crear identificador de ventana` (dejó el panel de pendientes vacío). Se mide con `TextRenderer`.
- Los `SELECT *` de `sp_Buscar_pedidos` con joins ahora traen `Id_direccion`; `Pedidos(DataRow)` lo lee solo si la columna existe.
- Los reportes arman la línea «Cliente» con la `Direccion_cliente` **antigua**, no la dirección del pedido (pendiente, §6).

## 6. Pendiente / siguientes pasos

1. **Commit** (propuesta: 3 commits — esquema BD; módulo Domicilios + títulos de reportes; tema `UI`). No incluir
   `SISMistico/SISMistico/FacturaFinal_0.EMF` (artefacto binario).
2. Aplicar `06` en el restaurante (`Desplegar-Base-de-Datos.ps1`) y regenerar la USB (`Preparar-USB.ps1` compila siempre).
3. Mostrar en comanda/factura/precuenta la **dirección del pedido** (`Pedidos.Id_direccion`) en vez de `Direccion_cliente`
   (`sp_Buscar_pedidos` 'ID PEDIDO Y DETALLE' y la consulta de venta final).
4. Borrar código muerto: `FrmDomicilios`, `DomicilioSmall` (desde 2026-11-13 solo lo usa `FrmDomicilios`),
   `FrmRealizarPedido`, el `btnDomicilio` oculto de `FrmObservarMesas`.
5. Rappi: solo mensaje «ESPERANDO INTEGRACIÓN» y KPI en 0.
6. Probar en pantalla (nada se ha validado con el usuario salvo el arreglo de la tarjeta): crear cliente/dirección, domicilio
   completo, facturar con descuento (Observaciones obligatorias), impresión con etiqueta, DOMICILIARIO vs ADMINISTRADOR.
7. Otras pantallas siguen con el aspecto antiguo; aplicarles `ApplyTheme()` de una en una.

## 7. Cómo compilar y probar

**Despliegue actualizado (2026-11-02):** `Preparar-USB.ps1` genera `01-Base-de-Datos.ps1` con los SQL y
auxiliares embebidos; ya no depende de `Soporte/db`. El paso 6/7 aplica `06` (incluye la mesa comodín 0 si
no existe) y la verificación final revisa tabla, columna, FKs y los ocho SP de Domicilios. Auditoría repetible:
`scripts/Test-DatabaseDeployment.ps1` prueba dos ejecuciones en una base temporal sin tocar las bases locales.
Las consultas del historial (`Tipo`) y de `FrmSaleDetails` están en `DVenta` y se distribuyen con el instalador.

```powershell
$ms   = "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe"
$refs = "$env:LOCALAPPDATA\Temp\opencode\net48refs\Microsoft.NETFramework.ReferenceAssemblies.net48.1.0.3\build\"
& $ms SISMistico\SISMistico.sln /p:Configuration=Debug "/p:TargetFrameworkRootPath=$refs" /v:q /nologo
```

- El `.csproj` es de estilo antiguo: **cada archivo nuevo necesita su `<Compile Include=...>`**.
- Debug usa la base `CasaGrandeBD`; `db\Invoke-Sql.ps1 -File x.sql -Database CasaGrandeBD` sustituye a sqlcmd.
- Cerrar `SISMistico.exe` antes de compilar (bloquea el ejecutable).
- Para reproducir un fallo de pantalla sin interfaz: copiar `bin\Debug`, añadir un `.exe` de consola con el `.config` de
  `SISMistico.exe` y llamar por reflexión a los métodos privados (así se halló el error de §5).
- Reglas del repositorio: código en inglés, comentarios y commits en español; los archivos se editan con las herramientas de
  edición, no con heredocs; respetar los finales de línea (`git diff --stat` debe tener el tamaño del cambio real).

## 8. Otros cambios del día ya en `main`

Login del escritorio solo con empleados ACTIVO (script `05`); info de mesa en la tarjeta del mapa (igual que el móvil) y
limpieza al facturar; corrección de ventas duplicadas con descuento (`DVenta` con transacción); sondeo del móvil a 4 s;
`Preparar-USB.ps1` compila siempre. **Sin commit aún**: validación «Observaciones obligatorias con descuento» en
`FrmFacturarPedido.cs`, y en la app móvil el ajuste de teclado de `add_order_items_sheet.dart`.
