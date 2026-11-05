# 11 · Módulo de Domicilios (escritorio) — guía de traspaso

Estado a **2026-11-02**. Todo lo de esta guía está **sin commit** (ver §7). Lo anterior del día ya está en `main`.

## 1. Qué hace

Un domicilio es un pedido de `Pedidos` con `Tipo_pedido = 'DOMICILIO'` e `Id_mesa = 0` (la mesa 0 es el comodín).
La dirección de entrega sale de la tabla nueva `Domicilios` (varias por cliente) y se guarda en `Pedidos.Id_direccion`
(NULL en pedidos de mesa).

Flujo: **menú «Domicilios»** → `FrmDeliveries` (KPIs + 3 paneles) → **REALIZAR PEDIDO** → `FrmDeliveryCustomer`
(buscar/crear cliente + elegir/crear dirección) → `FrmPedido` con `IsDomicilio = true` → tarjeta en EN PREPARACIÓN →
**Facturar** (`FrmFacturarPedido`, `IsDomicilio = true`) → pasa a TERMINADOS.

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
4. Borrar código muerto: `FrmDomicilios`, `FrmRealizarPedido`, el `btnDomicilio` oculto de `FrmObservarMesas`.
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
