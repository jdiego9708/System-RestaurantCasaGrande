# 02 — Base de datos y hallazgos

> **Actualizado el 2026-10-24** con la BD de **producción**: respaldo de esquema y respaldo completo (datos)
> generados con SSMS en el equipo del restaurante (`C:\Users\Casa Grande\CasaGrandeBD.mdf`). La versión del
> 2026-10-21 se había hecho sobre la LocalDB de desarrollo, cuyo esquema **no** era el real; lo que sigue
> reemplaza esos hallazgos. Scripts y resultados reproducibles en [`db/`](../db/README.md).

---

## 1. Cómo se inspeccionó

1. Los respaldos (`backupCasaGrande20261024Esquema.sql` y `…Completa.sql`, scripts T-SQL en UTF-16) se
   convirtieron a UTF-8 (`db/produccion-schema.sql`) y se restauró una **copia** en LocalDB como
   `CasaGrandeBD_prod` con `db/Restore-ProdCopy.ps1`. Las 25 tablas cargadas coinciden fila por fila con los
   `INSERT` del script (38.579 pedidos, 157.217 líneas, 324.508 ingredientes…).
2. Las consultas de contraste están en `db/diagnostico-prod.sql`; el comportamiento de
   `sp_Actualizar_detalle_pedido` se probó en `db/pruebas-sp-actualizar-detalle.sql` (dentro de una transacción
   que se revierte; la copia queda intacta).
3. La `CasaGrandeBD` de desarrollo en LocalDB **no se toca** y queda descartada como fuente de verdad.

**Qué sí y qué no se sabe de producción**

| Dato | Estado |
|---|---|
| Esquema (tablas, columnas, SPs, tipos) | ✅ Conocido: es el del script de esquema |
| Nivel de compatibilidad | ✅ 150 (SQL Server 2019 o superior) → `OPENJSON` disponible |
| Motor en producción | ✅ **LocalDB** (`(localdb)\MSSQLLocalDB`, BD `CasaGrandeBD`): así lo dice la `exe.config` de la build publicada 108, y el script de esquema muestra el archivo `C:\Users\Casa Grande\CasaGrandeBD.mdf` |
| Versión exacta de LocalDB y *collation* | ❓ No constan en el script. En la copia local es `SQL_Latin1_General_CP1_CI_AS`; si producción difiere, las comparaciones de texto (p. ej. contraseñas) pueden variar. Se ve en el equipo con `SELECT @@VERSION, SERVERPROPERTY('Collation')` |
| Que el `.exe` instalado equivalga al código del repo | ✅ La build publicada **1.0.0.108** (la que instala ClickOnce desde `publish/`) tiene el **mismo IL** que el código actual: 2.871 métodos en 5 ensamblados, 0 diferencias (`scripts/Compare-PublishedBuild.ps1`). Falta solo saber qué versión tiene instalada el restaurante |

---

## 2. Tablas del flujo (producción)

### 2.1 `Pedidos`

| Columna | Tipo | Nulo | Notas |
|---|---|---|---|
| `Id_pedido` | int | no | identity (1010, 2), PK |
| `Id_mesa` | int | no | `0` = comodín de domicilio |
| `Id_empleado` | int | no | quién tomó el pedido (clave maestra) |
| `Id_cliente` | int | no | `0` = NINGUNO |
| `Estado_pedido` | varchar(50) | no | `PENDIENTE` → `SALIENDO` → `DISPONIBLE` (facturado); `CANCELADO`. Ver [01 §6](01-flujo-actual-de-pedidos.md) |
| `Fecha_pedido` | date | no | |
| `Hora_pedido` | time(2) | no | |
| `Tipo_pedido` | varchar(50) | sí | `MESA` (el 100 % de las filas) |
| `Observaciones_pedido` | varchar(200) | sí | |
| `CantidadClientes` | int | sí | |

### 2.2 `Detalle_pedido`

| Columna | Tipo | Nulo | Notas |
|---|---|---|---|
| `Id_detalle_pedido` | int | no | identity (1,1), PK — **identifica la línea** |
| `Id_pedido` | int | no | |
| `Id_tipo` | int | no | id de plato **o** de bebida según `Tipo` |
| `Tipo` | varchar(50) | no | `PLATO` \| `BEBIDA` |
| `Precio` | decimal(19,2) | no | pesos; hoy ninguna fila tiene decimales |
| `Cantidad` | int | no | |
| `Observaciones` | varchar(100) | sí | |

### 2.3 `Detalle_ingredientes_pedido`

| Columna | Tipo | Nulo | Notas |
|---|---|---|---|
| `Id_detalle_ingrediente_pedido` | int | no | identity (1,2), PK |
| `Id_pedido` | int | no | |
| `Id_tipo` | int | no | id del plato |
| `Id_ingrediente` | int | no | |
| `Id_detalle_pedido` | int | **sí** | línea a la que pertenece; nulo en 1.048 filas antiguas |
| `Observaciones` | varchar(200) | sí | siempre vacío en los datos reales |

### 2.4 `Mesas`

`Id_mesa` (identity 5050, 5), `Num_mesa`, `Descripcion_mesa`. **Sin columna de estado.** 38 filas, una de ellas
`Id_mesa = 0` ("DOMICILIO").

### 2.5 `Clientes`

`Id_cliente`, `Nombre_cliente`, `Telefono_cliente`, `Correo_electronico`, `Direccion_cliente`,
`Referencia_ubicacion`, `Otras_observaciones`, `Estado_cliente`. 7 filas; `Id_cliente = 0 → 'NINGUNO'`.

### 2.6 Integridad referencial

**No hay claves foráneas** entre `Pedidos`, `Detalle_pedido`, `Detalle_ingredientes_pedido`, `Mesas` ni `Clientes`,
ni índices únicos más allá de las PK. Hoy no hay huérfanos (0 pedidos sin mesa o sin cliente, 0 líneas sin pedido),
pero **nada en la BD impide** duplicar un pedido: la idempotencia y la validación de mesa viven solo en la API.

---

## 3. Procedimientos almacenados relevantes (producción)

### 3.1 `sp_Insertar_pedido` — 9 parámetros

`@Id_pedido (output)`, `@Id_mesa`, `@Id_empleado`, `@Id_cliente`, `@Fecha_pedido`, `@Hora_pedido time(2)`,
`@Tipo_pedido`, `@Observaciones_pedido`, `@CantidadClientes`.

Inserta en `Pedidos` con `Estado_pedido = 'PENDIENTE'`, asigna `SCOPE_IDENTITY()` y devuelve
`SELECT 'OK' AS Respuesta, @Id_pedido AS Id_pedido`. Es lo que envía `DPedidos.InsertarPedido`.

### 3.2 `sp_Actualizar_detalle_pedido` — 9 parámetros

`@Id_detalle_pedidoSalida (output)`, `@Id_detalle_pedido`, `@Id_pedido`, `@Id_tipo`, `@Tipo`, `@Precio`,
`@Cantidad`, `@Observaciones`, `@Tipo_update`.

**La línea se identifica por `@Id_detalle_pedido`, no por `(Id_pedido, Id_tipo)`.** Comportamiento verificado en
la copia (`db/pruebas-sp-actualizar-detalle.sql`):

| Llamada | Efecto |
|---|---|
| `@Id_detalle_pedido = 0`, `@Tipo_update <> 'DELETE'` | `INSERT` de una línea nueva; devuelve su id en `@Id_detalle_pedidoSalida` |
| `@Id_detalle_pedido = 0`, `@Tipo_update = 'DELETE'` | **No hace nada** (y responde OK) |
| `@Id_detalle_pedido > 0`, `@Cantidad = 0` | Borra la fila |
| `@Id_detalle_pedido > 0`, `cantidad_actual > @Cantidad` | `Cantidad -= 1` (el valor recibido solo sirve para decidir la rama) |
| `@Id_detalle_pedido > 0`, `cantidad_actual <= @Cantidad` | `Cantidad += @Cantidad` |

Consecuencia: **no sirve como "sumar un delta"**. Con una línea de 3 y `@Cantidad = 2` el resultado es **2**, no 5.

### 3.3 Otros SPs usados por el flujo

| SP | Uso |
|---|---|
| `sp_Insertar_detalle_pedido` | Inserta una línea y devuelve `@Id_detalle_pedido`. Existe; el escritorio no lo usa |
| `sp_Buscar_pedidos` | ramas `ID PEDIDO`, `ID PEDIDO Y DETALLE`, `MESAS CON PEDIDO`, `MESAS COMPLETAS`, `DOMICILIOS *`, `FECHA`, `RANGO FECHAS` |
| `sp_Cambiar_estado_pedido` | `UPDATE Pedidos SET Estado_pedido=@Estado, Id_mesa=@Id_mesa` (reescribe también la mesa) |
| `sp_Cancelar_pedido` | `Estado_pedido='CANCELADO'` + `Observaciones_pedido` |
| `sp_Login` | valida `Id_empleado`+`Password` **y tiene efectos sobre `Turnos`** (ver §5.18) |
| `sp_Buscar_platos`, `sp_Buscar_bebidas`, `sp_Buscar_tipo_platos`, `sp_Buscar_tipo_bebidas` | catálogo |
| `sp_Buscar_ingredientes`, `sp_Buscar_detalle_platos` | ingredientes de platos detallados |
| `sp_Buscar_clientes`, `sp_Insertar_cliente` | clientes |

Los tipos de tabla `tp_Detalle_pedido_modificado` y `tp_Detalle_pago` existen pero **ningún SP del flujo de pedidos
los usa** (restos de un rediseño que no llegó a aplicarse).

---

## 4. ✅ Desajuste código ↔ BD: resuelto

El desajuste encontrado el 2026-10-21 era de la **LocalDB de desarrollo**, no de producción.

| SP / tabla | Lo que envía el código | Producción | Coincide |
|---|---|---|---|
| `sp_Insertar_pedido` | 9 parámetros | 9 parámetros, mismos nombres | ✅ |
| `sp_Actualizar_detalle_pedido` | 9 (`@Id_detalle_pedidoSalida`, `@Id_detalle_pedido`, …, `@Tipo_update`) | 9, idénticos | ✅ |
| `sp_Cambiar_estado_pedido` | `@Id_pedido`, `@Estado`, `@Id_mesa` | idénticos | ✅ |
| `sp_Cancelar_pedido` | `@Id_pedido`, `@Observaciones` | idénticos | ✅ |
| `sp_Insertar_detalle_pedido` | 7 parámetros | 7 parámetros | ✅ |
| `Detalle_pedido.Id_detalle_pedido` | lo usa el código | existe (identity) | ✅ |
| `Detalle_ingredientes_pedido.Id_detalle_pedido` | lo inserta el código | existe (nullable) | ✅ |

La LocalDB de desarrollo (`CasaGrandeBD`) tenía `@Id_usuario`, un TVP en `sp_Insertar_pedido` y no tenía
`Id_detalle_pedido`: un rediseño posterior que nunca llegó a producción ni al repositorio. **No usar esa BD**;
usar `CasaGrandeBD_prod`.

**Tarea 0.8 del plan, cerrada:** la build publicada 108 equivale al código del repo (mismo IL, ver §1). Las firmas
de los SPs coinciden con lo que esa build envía.

---

## 5. Hallazgos y riesgos

### 5.1 🟠 Alta no transaccional

Cabecera + N líneas + ingredientes = `2 + N` operaciones independientes. Si falla una quedan datos parciales.
**Evidencia en producción:** 209 pedidos sin ninguna línea (159 `DISPONIBLE`, 38 `PENDIENTE`, 12 `CANCELADO`).
Casi todos son anteriores a 2026 (solo 5 este año), y parte pueden deberse a líneas quitadas después, pero el
patrón es compatible con guardados interrumpidos. En LAN/WiFi, con reintentos del móvil, el riesgo se multiplica.

**Segunda evidencia (ingredientes):** 1.404 líneas de platos detallados no tienen ningún ingrediente (173 en 2022,
140 en 2023, 79 en 2024, 605 en 2025, 407 en 2026; desde julio de 2026 son 40 de 5.077). El escritorio guarda la línea
y *luego* los ingredientes en otro paso; si ese segundo paso falla, queda una línea de plato sin sopa, arroz,
acompañante ni bebida.

**Mitigación implementada:** SP único transaccional para el alta desde móvil (`db/01_sp_Movil_Insertar_pedido.sql`,
[03 §4](03-propuesta-api-movil.md)); probado con un fallo a mitad de la transacción y con 78 pruebas de extremo a extremo.

### 5.2 🟠 El precio lo define el cliente

`Detalle_pedido.Precio` llega desde la UI. La API debe **recalcular** desde `Platos.Precio_plato` /
`Bebidas.Precio_bebida` y no confiar en el valor recibido.

### 5.3 🟠 Reintentos → pedidos duplicados

`POST /pedidos` puede llegar dos veces (timeout, doble toque, reconexión) y la BD no tiene ninguna restricción que
lo impida (§2.6). **Mitigación:** `idempotencia` (GUID del cliente) + caché de resultados en el servidor.

### 5.4 🟡 SQL concatenado en ingredientes

`DPedidos.InsertarDetalleIngredientesPedido` (`:918-926`) construye:

```csharp
consulta.Append(string.Format(
    "INSERT INTO Detalle_ingredientes_pedido VALUES ({0}, {1}, {2}, {3}, '{4}'); ",
    detalle.Id_pedido, detalle.Id_tipo, detalle.Id_ingrediente, detalle.Id_detalle_pedido, detalle.Observaciones));
```

El escritorio siempre envía `Observaciones = ""` (en producción ninguna fila tiene observaciones), pero el móvil
sí podría enviar texto → **inyección SQL**. **Mitigación:** no usar ese método en la ruta móvil (SP parametrizado).

### 5.5 ✅ `sp_Buscar_pedidos 'MESAS CON PEDIDO'`: no aplica a producción

El bug de precedencia `AND/OR` que se describió el 2026-10-21 existía solo en la BD de desarrollo. En producción
la rama es correcta:

```sql
WHERE pedidos.Id_mesa = mesas.Id_mesa and
pedidos.Fecha_pedido = CONVERT(date, @Texto_busqueda1) AND
pedidos.Estado_pedido IN ('SALIENDO', 'PENDIENTE') and
pedidos.Tipo_pedido != 'DOMICILIO' and
pedidos.Tipo_pedido != 'TERMINADO'
```

La consulta es `SELECT TOP 50 *` ordenada por `Id_pedido DESC` (con unas 34 mesas activas sobra). La API puede
**reutilizar este SP** para el mapa de mesas en lugar de escribir una consulta propia, aunque una consulta propia
sigue siendo válida si se necesitan más columnas (total de la cuenta, hora).

### 5.6 ✅ Parámetro `@Id_usuario`: no aplica a producción

`DPedidos.ActualizarDetallePedido` recibe `id_empleado` y no lo envía; en producción el SP no tiene
`@Id_usuario`, así que es correcto. **No hay auditoría de quién modificó una línea**: queda como mejora futura.

### 5.7 🟡 Estado en memoria y hilos de UI

* `DatosInicioSesion` y `FrmObservarMesas` son **singletons del proceso**.
* La API no debe usarlos para autenticar ni para leer estado.
* Para refrescar la UI hay que usar `Control.BeginInvoke` (nunca tocar controles desde el hilo del listener).
* Si el proceso se cierra, se pierden tokens, caché de idempotencia y estado visual.

### 5.8 🟡 Impresión de comandas desde el servidor

`FrmComandas` usa ReportViewer y abre diálogo de impresión → **solo hilo de UI**, no thread-safe, puede tardar.
**Mitigación:** imprimir en `BeginInvoke`, con try/catch, y no bloquear la respuesta HTTP.

### 5.9 🟡 `HttpListener` requiere reserva de URL

Como usuario sin privilegios falla con "Acceso denegado" si no hay reserva.
**Mitigación:** `netsh http add urlacl url=http://+:8099/ user="Everyone"` (una vez, como admin). Alternativa:
mini-servidor sobre `TcpListener`.

### 5.10 🟡 Imágenes: ruta fija y fallback frágil

* `App.config` → `RUTAIMAGES = D:\ImagenesRestaurante\`; la carpeta **no existe** en el equipo de desarrollo y
  falta confirmar que exista en el PC del restaurante.
* **Hoy no hay ninguna imagen:** los 415 platos y las 160 bebidas activos tienen `SIN IMAGEN` (solo 7 platos
  *inactivos* guardan un nombre de archivo). El catálogo devuelve `imagen_url = null` y el endpoint de imágenes
  existe (probado con una carpeta temporal) pero no tiene nada que servir hasta que se carguen imágenes.
* Cuando haya imágenes, `Platos.Imagen_plato` guarda **solo el nombre de archivo** (sin ruta).
* `Imagenes.ObtenerImagen` (`Servicios/Imagenes.cs:17-35`) devuelve un placeholder si el archivo no existe, pero
  el `catch` intenta abrir `ruta + "AlmuerzoEspecial.jpg"`: si esa ruta tampoco existe, lanza excepción no controlada.
* Para el móvil: `GET /imagenes/{archivo}` leyendo de `RUTAIMAGES` con protección de *path traversal*.

### 5.11 🟡 No hay scripts SQL versionados

Hasta ahora no había `.sql` en el repositorio. Desde el 2026-10-24 existe `db/` con el esquema de producción
(`db/produccion-schema.sql`) y los scripts de diagnóstico. Todo cambio de BD nuevo debe ir ahí (aditivo).

### 5.12 🟡 Contraseñas en texto plano

`Empleados.Password` y `Codigo_maestro` se comparan en claro; `App.config` contiene cadenas de conexión con
credenciales y credenciales SMTP. No es parte del alcance, pero: no loguear credenciales en la API, restringir
el acceso a la LAN y al equipo, considerar (fase posterior) hash de contraseñas.

### 5.13 🟠 Varias filas por producto (11.953 pedidos)

El escritorio inserta una fila nueva en cada alta (`Id_detalle_pedido = 0`). Resultado: **11.953 de 38.579
pedidos (31 %)** tienen un mismo producto repartido en varias filas (p. ej. un plato pedido 3 veces = 3 filas de
cantidad 1). Implicaciones:

* `GET /pedidos/{id}` debe devolver cada fila con su `id_detalle_pedido` (el total del producto es la suma).
* No se puede asumir "una línea por `id_tipo`" al leer ni al probar integridad.
* Para agregar ítems a un pedido existente, la API debe **insertar filas nuevas** (como el escritorio) y no
  intentar "sumar" a una fila: el SP no soporta deltas (§3.2).
* Para quitar, hay que direccionar la fila por `Id_detalle_pedido`.

### 5.14 🟠 El escritorio podría no estar quitando líneas ya guardadas

`FrmPedido.RemoveProduct` (`:553-564`) construye el `Detalle_pedido` sin `Id_detalle_pedido` (queda en 0) y llama
al SP con `"DELETE"`. Según §3.2, con id 0 + `DELETE` el SP **no hace nada** y devuelve OK. Verificado a nivel
de SP; **no se ha probado la acción en la aplicación**. Si se confirma, es un error del escritorio que
preexiste y que la API **no** debe replicar (ver decisión D11).

### 5.15 🟠 Empleados inactivos siguen operando

`sp_Login` solo valida `Id_empleado` + `Password`; no mira `Empleados.Estado`. En los últimos 30 días un mesero
marcado `INACTIVO` tomó **93 pedidos (el último, el 2026-10-24)** y un cajero inactivo tomó 1. Una regla
"empleado inactivo → 403" **bloquearía a alguien que hoy trabaja con normalidad** en el escritorio (decisión D12).

### 5.16 🟡 Calidad de la clave maestra (PIN)

Entre los 26 empleados activos: 2 PIN están repetidos (afectan a 4 empleados) y 8 no tienen 4 dígitos (7 meseros
y el cajero). Un login **solo con PIN** no identifica al empleado de forma única → se descarta la opción B de D3.
La API debe pedir `id_empleado` + (contraseña o PIN).

### 5.17 🟡 Pedidos abiertos huérfanos

Hay **82 pedidos `PENDIENTE` y 4 `SALIENDO`**; 77 son de días anteriores (desde 2023) y nunca se cerraron.
Además hay 8 casos (mesa, día) con más de un pedido abierto. La validación de "mesa ocupada" debe filtrar por
**la fecha de hoy**, igual que la UI; si no, esas mesas quedarían bloqueadas.

### 5.18 🟠 `sp_Login` tiene efectos sobre los turnos

Además de validar credenciales, `sp_Login` **cierra el turno anterior** (ejecuta `sp_Cerrar_turno`, que calcula
totales) **y abre uno nuevo** cuando la fecha de login es posterior a la del último turno. Un login desde el móvil
a las 00:30 podría cerrar prematuramente el turno del día anterior. **La API no debe llamar a `sp_Login`:** valida
credenciales con una consulta propia de solo lectura sobre `Empleados` y, si necesita el turno, lo lee sin
modificarlo.

---

### 5.19 🟡 Mesa duplicada: dos filas con `Num_mesa = 21`

`Mesas` tiene `Id_mesa` 5150 y 5200 con `Num_mesa = 21`. El escritorio construye el mapa por **`Num_mesa`**
(`FrmObservarMesas.CargarMesas`: `Select("Num_mesa = '21'")` y toma la primera fila, o sea la 5150; además calcula
`Numero_mesas = filas - 2`). La 5200 nunca ha tenido un pedido. Si la API la expusiera, un pedido en la 5200
aparecería en el escritorio como la mesa 21 (por `Num_mesa`) pero con otro `Id_mesa`. **Decisión:** la API expone una
mesa por `Num_mesa` (la de menor `Id_mesa`) y la comprobación "mesa ocupada" debe razonar por `Num_mesa`
([04 §4](04-contrato-api-v1.md)). Conviene que el restaurante borre o renumere la 5200.

### 5.20 🟡 `CantidadClientes` siempre es 0

En los 2.307 pedidos de los últimos 90 días `CantidadClientes = 0`: el escritorio no lo captura. El contrato lo hace
opcional (`>= 0`) en vez de obligatorio `>= 1`.

### 5.21 🟡 Los ingredientes son globales, no por plato

La tabla `Ingredientes` (14 filas) se ofrece igual en cualquier plato con `Plato_detallado = 'ACTIVO'`, agrupada por
`Tipo_ingrediente` (`SOPA`, `ARROZ`, `ACOMPANANTE` —sin eñe—, `BEBIDA`) y con opciones "SIN …". El escritorio hace
elegir uno de cada tipo. `Detalle_platos` es de **insumos** (inventario), no de ingredientes elegibles.

### 5.22 🟡 Los platos detallados guardan los 4 tipos también en la carta

Desde 2026-07-01, 5.035 líneas de platos de carta (`Plato_carta = 'ACTIVO'`) guardan exactamente una sopa, un arroz,
un acompañante y una bebida (la sopa fue `SOPA` 4.966 veces, `CREMA` 58, `SIN SOPA` 11), aunque el escritorio
deshabilita el selector de sopa en la carta. De ahí la decisión D14: la API exige los cuatro tipos sin excepción.
Además, ~8 % de las líneas de platos detallados tienen `Cantidad > 1` (un solo juego de ingredientes para varias
unidades).

---

## 6. Matriz de riesgos

| # | Riesgo | Prob. | Impacto | Mitigación | Estado |
|---|---|---|---|---|---|
| 1 | Esquema de producción ≠ repo | — | — | Respaldo de esquema obtenido y comparado | ✅ **Cerrado** (§4) |
| 2 | Alta parcial de pedidos | Media | 🔴 Alto | SP transaccional (aditivo) | **Implementado y probado** (no desplegado) |
| 3 | Pedidos duplicados por reintento | Alta | 🟠 Alto | Clave de idempotencia | **Implementado y probado** (incl. doble toque simultáneo) |
| 4 | Precio manipulado desde el móvil | Media | 🟠 Alto | Recalcular en servidor/SP | **Implementado y probado** |
| 5 | Inyección SQL en ingredientes | Baja | 🔴 Alto | SP parametrizado / sanitizar | **Implementado y probado** |
| 6 | Mesa ocupada por doble alta | Media | 🟡 Medio | Validación por `Num_mesa` y fecha de hoy + `SemaphoreSlim` | **Implementado y probado** (dos claves a la vez: un 201 y un 409) |
| 7 | Impresión bloquea la respuesta | Media | 🟡 Medio | Responder y luego `BeginInvoke` | Implementado; **impresión sin probar con impresora** |
| 8 | Listener sin permisos (`urlacl`) | Alta | 🟡 Medio | Documentar/automatizar el `netsh` | Propuesto |
| 9 | Bug de mesas `SALIENDO` de otra fecha | — | — | No existe en producción | ✅ **No aplica** (§5.5) |
| 10 | Caída del `.exe` = caída de la API | Baja | 🟠 Alto | Operación ya depende del exe; monitoreo/log | Aceptado |
| 11 | Sin TLS en LAN | Media | 🟡 Medio | Token + `X-Api-Key` + allowlist IP | Aceptado |
| 12 | Imágenes no disponibles | Cierta (no hay ninguna) | 🟡 Bajo | Endpoint de imágenes + `imagen_url = null` | **Implementado** (§5.10) |
| 13 | "Sumar delta" decrementa la línea | Alta si se usa el SP así | 🔴 Alto | No usar el SP para sumar; insertar filas nuevas | **Nuevo** (§5.13) |
| 14 | Login de la API altera los turnos | Alta si se llama a `sp_Login` | 🟠 Alto | Validación propia, sin tocar `Turnos` | **Nuevo** (§5.18) |
| 15 | Empleados inactivos bloqueados por error | Media | 🟠 Alto | Decidir D12 antes de implementar el login | **Nuevo** (§5.15) |
| 16 | Quitar línea no persiste en el escritorio | Por confirmar | 🟠 Alto | Probar en la app (D11) | **Nuevo** (§5.14) |

---

## 7. Verificación en producción: qué se hizo y qué falta

**Hecho (2026-10-24, sobre la copia):** firmas de SPs, columnas y tipos de las tablas del flujo, definición de los
SPs, distribución de estados, carga diaria, calidad de PIN, pedidos huérfanos y comportamiento de
`sp_Actualizar_detalle_pedido`. Resultados completos reproducibles con:

```powershell
.\db\Restore-ProdCopy.ps1 -Force
.\db\Invoke-Sql.ps1 -File .\db\diagnostico-prod.sql
.\db\Invoke-Sql.ps1 -File .\db\pruebas-sp-actualizar-detalle.sql
```

**Falta (necesita el servidor o la aplicación reales):**

```sql
SELECT @@VERSION, SERVERPROPERTY('Collation');      -- versión exacta y collation de producción
```

* Confirmar en la aplicación instalada el comportamiento de "quitar producto" (§5.14; aplazado, D11).
* Confirmar que `D:\ImagenesRestaurante\` existe en el PC del restaurante y qué contiene.
* Saber qué versión de ClickOnce tiene instalada el restaurante (Panel de control → Programas); la última publicada es la 1.0.0.108.
