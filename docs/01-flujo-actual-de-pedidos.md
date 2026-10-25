# 01 — Flujo actual de pedidos

> Análisis del código del repositorio (commit `3ee08f3`; el código fuente tiene cambios hasta enero de 2026 y la
> última build publicada es la 1.0.0.108, con IL idéntico a este código), **contrastado el 2026-10-24 con una copia
> de la BD de producción** (ver [02](02-base-de-datos-y-hallazgos.md) y [`db/`](../db/README.md)).
> Referencias en formato `Archivo.cs:línea`. Las rutas de formularios son relativas a
> `SISMistico/CapaPresentacion/Formularios/`; las de capas, a `SISMistico/`.

---

## 1. Punto de entrada

La pantalla de mesas del sistema es `FrmObservarMesas` (variante de `FormsPedido`, no la de `FormsMesas`):

```
FrmPrincipal                         [FormsPrincipales/FrmPrincipal.cs:248]
  └─ btnMesasPedidos → FrmObservarMesas.GetInstancia()      (singleton)
        └─ panel con un CustomButtonMesa por mesa             [FormsPedido/FrmObservarMesas.cs:199]
              │
              ├─ clic izquierdo, mesa DISPONIBLE
              │     → new FrmPedido { MesaSelected, ClienteSelected = default(0) }
              │       [Controles/CustomButtonMesa.cs:42]
              │       → FrmPedido_Load → FrmComprobacion (PIN de 4 dígitos)
              │         [FormsPedido/FrmPedido.cs:379]
              │
              ├─ clic izquierdo, mesa PENDIENTE / SALIENDO
              │     → DatosMesaSmall (popover) con:
              │        Editar pedido · Facturar · Precuenta · Cancelar · Cambiar de mesa
              │        [Controles/CustomButtonMesa.cs:62-75]
              │
              └─ botón Domicilio
                    → FrmDomicilios / FrmRealizarPedido   (ver §9)
```

**Formulario a adaptar para móvil: `FrmPedido`.**

---

## 2. Formularios de `FormsPedido`: activos vs. código muerto

| Archivo | Estado | Evidencia |
|---|---|---|
| **`FrmPedido.cs`** | **ACTIVO — núcleo del alta y edición** | Abierto desde `CustomButtonMesa.cs:42` y `FrmObservarMesas.cs:275` |
| `FrmObservarMesas.cs` | ACTIVO | `FrmPrincipal.cs:248` |
| `ProductoItem.cs`, `TipoItem.cs`, `ProductBinding.cs` | ACTIVO | Controles y modelos de la vista de pedido |
| `FormsPlatos/FrmDetallePedidoPlato.cs`, `CustomPlato.cs` | ACTIVO | Selección de ingredientes de platos detallados (`FrmPedido.cs:879`) |
| `FrmComprobacion.cs` | ACTIVO | Clave maestra al abrir y al quitar productos (`FrmPedido.cs:32`, `:550`) |
| `DatosMesaSmall.cs`, `FrmFacturarPedido.cs`, `FrmCambiarMesa.cs` | ACTIVO | Acciones sobre mesa ocupada |
| `FrmComandas` (`ReportesFacturas/`) | ACTIVO | Impresión de comanda (`FrmPedido.cs:332-341`) |
| **`FrmRealizarPedido.cs`** | **MUERTO — no persiste** | Guardado comentado en `FrmRealizarPedido.cs:201-206`; después evalúa `rpta` que quedó `""` y lanza excepción |
| `TablasPedido.cs` | Solo lo usa el formulario muerto | Única instanciación en `FrmRealizarPedido.cs:567`; sus escrituras a BD están comentadas (`TablasPedido.cs:355`, `:408`) |
| `FrmDetallePedido.cs` | LEGADO | Abre `FrmRealizarPedido` (`FrmDetallePedido.cs:138`) |
| `FrmDomicilios.cs` | Activo en UI, **roto al crear** | `BtnAddDomicilio_Click` abre `FrmRealizarPedido` (`FrmDomicilios.cs:133`) |
| `PruebaDetallePedido.cs` | Basura de pruebas | Sin referencias |
| `ContextMenuDatosPedido.cs`, `DescuentosOpcionesPedido.cs`, `MetodoPagoSmall.cs`, `EmailFactura.cs`, `FrmDeletePedido.cs` | Periféricos | Impresión, descuentos, correo, auditoría de eliminados; no participan del alta |

> ⚠️ **Consecuencia:** en el código del repositorio, el alta de **domicilios no persiste** (quedó a medias en el
> commit `7b9552f` del 2022-08-06, "Actualizacion varia").
> **Confirmado con datos de producción (2026-10-24):** de los 38.579 pedidos, el 100 % es `Tipo_pedido = 'MESA'`;
> nunca se ha registrado un domicilio. El alcance móvil de la fase 1 es solo mesa (decisión D2).

---

## 3. Secuencia exacta de guardado (pedido de mesa)

Archivo: `CapaPresentacion/Formularios/FormsPedido/FrmPedido.cs`

```
BtnSave_Click (:235)
│
├─ Comprobaciones(out pedido, out listDetalle, out dtDetalle, out dtPlatos, out dtBebidas)   (:52)
│    ├─ exige EmpleadoSelected (clave maestra), ClienteSelected y MesaSelected
│    ├─ arma Pedidos {
│    │      Id_mesa, Id_empleado, Id_cliente,
│    │      Fecha_pedido = Now, Hora_pedido = "HH:mm",
│    │      Tipo_pedido = "MESA" | "DOMICILIO",
│    │      Observaciones_pedido = "",
│    │      CantidadClientes }
│    └─ por cada ProductBinding de ProductsAddSelected arma:
│           Detalle_pedido { Id_pedido, Id_tipo, Tipo, Precio, Cantidad, Observaciones }
│           + List<Detalle_ingredientes_pedido> si el plato es "detallado"
│           + filas para la comanda (dtPlatos / dtBebidas)
│
├─ NPedido.InsertarPedido(pedido)                                        (:253)
│     → DPedidos.InsertarPedido (:59) → sp_Insertar_pedido
│       · inserta Pedidos con Estado_pedido = 'PENDIENTE'
│       · devuelve Id_pedido (output) y un SELECT 'OK' AS Respuesta, Id_pedido
│
├─ foreach (Detalle_pedido detalle in listDetalle)                       (:275)
│     detalle.Id_pedido = id_pedido     (detalle.Id_detalle_pedido = 0: línea nueva)
│     NPedido.ActualizarDetallePedido(detalle, idEmpleadoClaveMaestra, "")   (:279)
│       → DPedidos.ActualizarDetallePedido (:471) → sp_Actualizar_detalle_pedido
│         (9 parámetros; la línea se identifica por @Id_detalle_pedido, NO por Id_tipo)
│         · @Id_detalle_pedido = 0 y Tipo_update <> 'DELETE': INSERT y devuelve el id nuevo (output)
│         · @Id_detalle_pedido = 0 y Tipo_update = 'DELETE':  no hace nada
│         · @Id_detalle_pedido > 0 y @Cantidad = 0:           DELETE de la fila
│         · @Id_detalle_pedido > 0 y cantidad_actual > @Cantidad: Cantidad -= 1   (¡ignora el valor!)
│         · @Id_detalle_pedido > 0 y cantidad_actual <= @Cantidad: Cantidad += @Cantidad
│         (comportamiento verificado en `db/pruebas-sp-actualizar-detalle.sql`)
│
├─ si hay ingredientes: NPedido.InsertarDetalleIngredientesPedido(lista)  (:296)
│     → DPedidos.InsertarDetalleIngredientesPedido (:918)
│       · construye un único SQL concatenando INSERT ... VALUES (...)
│
├─ FrmObservarMesas.ObtenerPedido(id_pedido, num_mesa, "PENDIENTE")       (:308)
│     (solo cambia el estado visual del botón de la mesa en el escritorio)
│
├─ si chkPrintComandas (marcado por defecto)                             (:328-341)
│     · FrmComandas.ObtenerReporte("Casa Grande | Bebidas") + AsignarTablas + ImprimirFactura
│     · FrmComandas.ObtenerReporte("Casa Grande | Platos")   + AsignarTablas + ImprimirFactura
│
└─ this.Close()                                                          (:345)
```

### Observaciones de la secuencia

* **No hay transacción global.** Cabecera, detalles e ingredientes son operaciones independientes sobre la misma
  conexión lógica (conexiones distintas, de hecho). Un fallo intermedio deja datos parciales.
* El alta usa `sp_Actualizar_detalle_pedido` (no `sp_Insertar_detalle_pedido`). Es una decisión de diseño del
  sistema actual: el SP “decide” insertar/actualizar/borrar.
* `DPedidos.InsertarDetallePedido` (SP `sp_Insertar_detalle_pedido`) **no se usa** en el flujo actual.
* El parámetro `id_empleado` de `DPedidos.ActualizarDetallePedido` **se recibe pero no se envía al SP**. En
  producción es correcto: el SP no tiene `@Id_usuario` (ver [02 §5.6](02-base-de-datos-y-hallazgos.md)).
* **Cada clic en "+" del escritorio crea una fila nueva** (`Id_detalle_pedido = 0`), por eso el 31 % de los pedidos
  (11.953) tiene varias filas del mismo producto (ver [02 §5.13](02-base-de-datos-y-hallazgos.md)).
* **`FrmPedido.RemoveProduct` (`:553-564`) no fija `Id_detalle_pedido`** al quitar una unidad ya guardada, y el SP
  con id 0 + `DELETE` no hace nada: el escritorio podría estar quitando la línea solo de pantalla. Verificado a
  nivel de SP; **falta confirmarlo en la aplicación** (ver [06 D11](06-decisiones-pendientes.md)).

---

## 4. Modelo de datos involucrado

Tablas reales en la **BD de producción** (copia del 2026-10-24; detalle en [02](02-base-de-datos-y-hallazgos.md)):

| Tabla | Columnas relevantes | Notas |
|---|---|---|
| `Pedidos` | `Id_pedido` (identity 1010,2), `Id_mesa`, `Id_empleado`, `Id_cliente`, `Estado_pedido`, `Fecha_pedido` (date), `Hora_pedido` (time(2)), `Tipo_pedido`, `Observaciones_pedido` (varchar 200), `CantidadClientes` | Hoy `Tipo_pedido` es siempre `MESA`. Sin FKs |
| `Detalle_pedido` | **`Id_detalle_pedido`** (identity, PK), `Id_pedido`, `Id_tipo`, `Tipo` (`PLATO`\|`BEBIDA`), `Precio` (**decimal(19,2)**, hoy siempre entero), `Cantidad`, `Observaciones` (varchar 100) | La línea se identifica por `Id_detalle_pedido`; **puede haber varias filas del mismo `(Id_pedido, Id_tipo)`** |
| `Detalle_ingredientes_pedido` | `Id_detalle_ingrediente_pedido` (identity), `Id_pedido`, `Id_tipo`, `Id_ingrediente`, **`Id_detalle_pedido`** (nullable), `Observaciones` | El escritorio siempre guarda `Observaciones = ''`. 1.048 filas antiguas sin `Id_detalle_pedido` |
| `Mesas` | `Id_mesa`, `Num_mesa`, `Descripcion_mesa` | **La mesa no tiene estado propio**. `Id_mesa = 0` es el comodín de domicilio (38 filas en total) |
| `Clientes` | `Id_cliente`, `Nombre_cliente`, `Telefono_cliente`, … | Existe el comodín `Id_cliente = 0` ("NINGUNO"); solo 7 filas |

### Consecuencias del modelo

1. **Varias filas por producto.** El escritorio crea una fila por cada alta (`Id_detalle_pedido = 0`), de modo que
   un mismo producto puede aparecer en varias líneas del pedido. El total de un producto es la suma de sus filas.
   La API debe leer así los pedidos, y al editar debe direccionar la fila por `Id_detalle_pedido`.
2. **`Precio` pertenece a la línea** (no se referencia al catálogo). Si cambia el precio del plato,
   los pedidos viejos conservan el precio cobrado (correcto), pero obliga a validar el precio del lado servidor.
   Es `decimal(19,2)` aunque hoy todos los valores son enteros.
3. **El estado de la mesa es derivado**, no almacenado: se calcula cruzando `Mesas` con `Pedidos` del día y su
   `Estado_pedido`.
4. **Totales no persistidos**: se calculan en memoria (`Precio * Cantidad`) y se recalculan en cada consulta.

---

## 5. Reglas de negocio implícitas

| Regla | Dónde vive | Relevancia para el móvil |
|---|---|---|
| **Clave maestra obligatoria** (PIN de 4 dígitos) para abrir el pedido y para quitar líneas ya guardadas | `FrmPedido.cs:379`, `:550`; `DEmpleados.ClaveMaestra` | El endpoint debe exigir el PIN y registrar ese empleado como autor |
| **Mesa debe estar DISPONIBLE** para crear el pedido (sin pedido `PENDIENTE`/`SALIENDO` **de hoy**) | UI (`CustomButtonMesa`), no la BD | El servidor debe validarlo (hoy no hay constraint). Ojo: hay 77 pedidos abiertos de días anteriores y 8 casos históricos de una mesa con más de un pedido abierto el mismo día; filtrar por fecha de hoy, como la UI |
| **Empleado inactivo puede operar** | `sp_Login` solo valida `Id_empleado` + `Password`, no `Estado` | Un mesero `INACTIVO` tomó pedidos el 2026-10-24 (93 en 30 días). Ver [06 D12](06-decisiones-pendientes.md) |
| **Cliente opcional**, por defecto `0 / NINGUNO` | `FrmIniciarSesion.cs` → `DatosInicioSesion.ClienteDefault` | El móvil puede no enviar cliente |
| **No se valida turno abierto** al crear pedido | — | El turno solo se crea/cierra en el login |
| **Precio lo envía el cliente** | `FrmPedido.Comprobaciones` | La API debe recalcular desde catálogo |
| **Productos activos** | `NPlatos.BuscarPlatos(..., "ACTIVO")` / `NBebidas` | Validar `Estado='ACTIVO'` al crear |
| **Impresión de comandas activada por defecto** | `chkPrintComandas.Checked = true` (`FrmPedido.Designer.cs:278`) | Decidir si el móvil imprime o no |
| **Platos detallados** requieren ingredientes (`Plato_detallado='ACTIVO'`) | `FrmPedido.cs:873-891`, `CustomPlato` | Enviar `ingredientes[]` por línea |
| **Cliente/empleado se registran** para auditoría | `Pedidos.Id_empleado` / `Id_cliente` | El token debe resolver el empleado |

---

## 6. Estados de pedido y quién los cambia

```
                    ┌──────────────┐
   alta (móvil/PC) →│  PENDIENTE   │
                    └──────┬───────┘
                           │ precuenta
                           ▼
                    ┌──────────────┐
                    │   SALIENDO   │
                    └──────┬───────┘
                           │ facturar (escritorio)
                           ▼
                    ┌──────────────┐
                    │  DISPONIBLE  │   ← estado final real de un pedido facturado
                    └──────────────┘

   PENDIENTE / SALIENDO ──cancelar──► CANCELADO
```

* `PENDIENTE`: lo pone `sp_Insertar_pedido`.
* `SALIENDO`: `CustomButtonMesa.cs:132` (`NPedido.CambiarEstadoPedido`).
* `DISPONIBLE`: es el estado **final** de un pedido ya facturado ("mesa libre"). Lo escribe
  `CustomButtonMesa.ObtenerEstado("DISPONIBLE", …)` (`:127-137`, también `FrmObservarMesas.cs:78`) y, para
  domicilios, `FrmDomicilios.cs:117`. Verificado en producción: 38.462 de 38.579 pedidos están en `DISPONIBLE` y
  38.455 de ellos tienen una `Venta`.
* **`TERMINADO` no se usa para pedidos** (0 filas). Solo aparece en `Ingresos`, `Egresos` y `Nomina_empleado`
  (`Estado_*`), y en `sp_Buscar_clientes` (que cuenta pedidos `TERMINADO` y por tanto siempre da 0).
* `CANCELADO`: `sp_Cancelar_pedido` (31 pedidos; observaciones obligatorias en la práctica).
* `sp_Cambiar_estado_pedido` reescribe también `Id_mesa` con el valor recibido.

La UI de mesas (`FrmObservarMesas.CargarMesas`, `:153`) combina dos consultas del SP `sp_Buscar_pedidos`
(`MESAS CON PEDIDO` del día y `MESAS COMPLETAS`) para pintar cada botón.

> ✅ En producción la rama `MESAS CON PEDIDO` ya filtra bien (`Fecha_pedido = hoy AND Estado_pedido IN
> ('SALIENDO','PENDIENTE')`). El bug de precedencia `AND/OR` descrito en versiones anteriores de este documento
> solo existía en la BD de desarrollo. Ver [02 §5.5](02-base-de-datos-y-hallazgos.md).

---

## 7. Edición de un pedido y quitar ítems

* **Editar**: `FrmObservarMesas.cs:275` abre `FrmPedido` con `IsEditar = true` y el `Pedido` cargado. No se
  permite cambiar productos ya guardados con “+” (la lista `ProductsAddSelected` solo contiene los nuevos).
* **Quitar una unidad**: `FrmPedido.RemoveProduct` (`:543`) exige clave maestra si la línea vino de BD y llama
  `ActualizarDetallePedido(..., tipo_update: "DELETE")`.
* **Observaciones por línea**: `FrmObservacionPedido` (usado desde el formulario legado) y
  `TablasPedido.AddObservacion`; en `FrmPedido` las observaciones se capturan al agregar el producto.

---

## 8. Impresión de comandas

* `FrmComandas` (`ReportesFacturas/Comandas/`) usa **ReportViewer** (`.rdlc`) y abre diálogo de impresión.
* Se invoca desde `FrmPedido.cs:332-341` (platos y bebidas por separado) con el número de comandas del control
  `numericComandas`.
* **Es código de UI**: no es thread-safe y debe ejecutarse en el hilo principal del formulario. Cualquier endpoint
  que imprima tiene que hacer *marshalling* a ese hilo.

---

## 9. Domicilios (estado real del código)

```
FrmObservarMesas.btnDomicilio → FrmDomicilios
    └─ BtnAddDomicilio_Click → new FrmRealizarPedido { IsDomicilio = true }   [FrmDomicilios.cs:133]

FrmRealizarPedido.BtnTerminarPedido_Click                                    [:187]
    ├─ //rpta = NPedido.InsertarPedido(...)   ← COMENTADO                     [:201-206]
    ├─ if (rpta.Equals("OK"))   ← rpta nunca se asigna ("")  → false
    └─ throw new Exception(rpta)
```

En el repositorio, **el alta de domicilios no puede persistir**. `TablasPedido` (que sí tiene la lógica de
colección de productos) solo se usa desde ese formulario y sus llamadas de escritura también están comentadas.

**Implicación:** el restaurante nunca ha registrado un domicilio (0 de 38.579 pedidos), así que la fase 1 es solo
mesa. Si algún día el móvil debe tomar domicilios, la API tendrá que implementar ese flujo desde cero
(`Id_mesa = 0`, `Tipo_pedido = 'DOMICILIO'` y cliente obligatorio), porque el escritorio no lo hace.

---

## 10. Métodos de capa usados por el alta (superficie a reutilizar)

| Operación | Negocio | Datos | SP |
|---|---|---|---|
| Crear cabecera | `NPedido.InsertarPedido` | `DPedidos.InsertarPedido` | `sp_Insertar_pedido` |
| Insertar/actualizar/borrar línea (por `Id_detalle_pedido`) | `NPedido.ActualizarDetallePedido` | `DPedidos.ActualizarDetallePedido` | `sp_Actualizar_detalle_pedido` |
| Insertar línea (no usado por la UI) | `NPedido.InsertarDetallePedido` | `DPedidos.InsertarDetallePedido` | `sp_Insertar_detalle_pedido` |
| Insertar ingredientes | `NPedido.InsertarDetalleIngredientesPedido` | `DPedidos.InsertarDetalleIngredientesPedido` | *(SQL concatenado)* |
| Consultar pedido + detalle | `NPedido.BuscarPedidosYDetalle` | `DPedidos.BuscarPedidosYDetalle` | `sp_Buscar_pedidos` (`ID PEDIDO Y DETALLE`) |
| Listar / buscar | `NPedido.BuscarPedidos` | `DPedidos.BuscarPedidos` | `sp_Buscar_pedidos` |
| Cambiar estado | `NPedido.CambiarEstadoPedido` | `DPedidos.CambiarEstadoPedido` | `sp_Cambiar_estado_pedido` |
| Cancelar | `NPedido.CancelarPedido` | `DPedidos.CancelarPedido` | `sp_Cancelar_pedido` |
| Auditoría de eliminados | `NPedido.InsertarEliminacionComanda` | `DPedidos.InsertarEliminaciónComanda` | *(SQL concatenado)* |
| Login | `NEmpleados.Login` | `DEmpleados.Login` | `sp_Login` |
| Clave maestra | `NEmpleados.ClaveMaestra` | `DEmpleados.ClaveMaestra` | *(SELECT directo)* |
| Catálogo platos/bebidas | `NPlatos`, `NBebidas` | `DPlatos`, `DBebidas` | `sp_Buscar_platos`, `sp_Buscar_bebidas` |

Estos métodos son la base de la capa `Api` propuesta en [03](03-propuesta-api-movil.md).
