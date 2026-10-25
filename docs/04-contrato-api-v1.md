# 04 — Contrato de API v1

> Contrato propuesto para la app Flutter. **Congelar este documento** una vez cerradas las decisiones de
> [06](06-decisiones-pendientes.md). Cualquier cambio posterior implica `/api/v2` o un campo opcional.
>
> **Revisión 2026-10-24** (tras contrastar con la BD de producción): el login ya no admite solo PIN; las líneas
> de pedido llevan `id_detalle_pedido`; agregar ítems inserta filas nuevas (no suma deltas); quitar ítems se
> direcciona por `id_detalle_pedido`; el estado final de un pedido es `DISPONIBLE` (no `TERMINADO`) y lo escribe
> el escritorio al facturar; la fase 1 es solo `MESA`.

---

## 1. Convenciones

| Tema | Regla |
|---|---|
| Base URL | `http://<ip-servidor>:8099/api/v1` |
| Formato | JSON UTF-8, `Content-Type: application/json; charset=utf-8` |
| Nombres | `snake_case` en JSON (igual que las columnas de la BD) |
| Fechas | `yyyy-MM-dd` · horas `HH:mm` · timestamps ISO-8601 locales (`2026-10-21T13:42:07`) |
| Dinero | entero en pesos colombianos. En BD `Detalle_pedido.Precio` es `decimal(19,2)` pero todos los valores reales son enteros; la API redondea/serializa como entero |
| IDs | enteros |
| Versionado | prefijo `/v1`; cambios incompatibles → `/v2` |
| Compresión | no requerida |
| CORS | no requerido para Flutter nativo; si se compila a web, habilitar en el router |

### 1.1 Headers

| Header | Obligatorio | Uso |
|---|---|---|
| `X-Api-Key` | sí (salvo `/ping`) | Secreto compartido de la instalación |
| `Authorization: Bearer <token>` | sí (salvo `/ping` y `/auth/login`) | Sesión del empleado |
| `X-Clave-Maestra` | según operación | PIN de 4 dígitos para operaciones sensibles |

### 1.2 Envelope de error

```json
{
  "error": {
    "codigo": "MESA_OCUPADA",
    "mensaje": "La mesa 4 ya tiene un pedido pendiente (#1233).",
    "detalle": null
  }
}
```

### 1.3 Códigos

| HTTP | `codigo` | Cuándo |
|---|---|---|
| 200 / 201 / 204 | — | Éxito |
| 400 | `VALIDACION` | JSON inválido, campo faltante, tipo incorrecto |
| 401 | `NO_AUTORIZADO` | `X-Api-Key` inválida, token ausente o vencido |
| 403 | `CLAVE_INVALIDA` | Credenciales o PIN incorrectos (mensaje genérico: no revela si el id existe) o sin permiso |
| 403 | `IP_NO_PERMITIDA` | La petición no viene de la red local (10/8, 172.16/12, 192.168/16, loopback, enlace local IPv6) |
| 405 | `METODO_NO_PERMITIDO` | La ruta existe pero no con ese método |
| 429 | `DEMASIADOS_INTENTOS` | 5 logins fallidos en 60 s desde la misma IP; el mensaje indica los segundos de espera |
| 500 | `ERROR_INTERNO` | Excepción no controlada (mensaje saneado; el detalle va al log) |
| 404 | `NO_ENCONTRADO` | Pedido, mesa, producto o imagen inexistente |
| 409 | `MESA_OCUPADA` | La mesa ya tiene un pedido `PENDIENTE`/`SALIENDO` del día |
| 409 | `CONFLICTO` | Escritura concurrente sobre el mismo pedido |
| 422 | `REGLA_NEGOCIO` | Producto inactivo, cantidad ≤ 0, estado no permitido, `tipo_pedido` distinto de `MESA` |
| 500 | `ERROR_BD` | Excepción SQL (mensaje saneado) |
| 503 | `BD_NO_DISPONIBLE` | No hay conexión a SQL Server |

---

## 2. Salud

### `GET /ping`

Sin autenticación. Sirve para que el móvil descubra si el servidor está disponible.

```json
{
  "ok": true,
  "version": "1.0",
  "servidor": "RESTAURANTE-PC",
  "hora": "2026-10-21T13:40:12",
  "db": true
}
```

---

## 3. Autenticación

### `POST /auth/login`

`id_empleado` es obligatorio y se acompaña de **una** de las dos credenciales:

```json
// opción A: contraseña (igual que el escritorio)
{ "id_empleado": 6120, "password": "••••••" }

// opción B: PIN (clave maestra) del mismo empleado
{ "id_empleado": 6120, "codigo_maestra": 9708 }
```

> **No se admite el PIN solo.** En producción hay PIN repetidos entre empleados activos y algunos no tienen
> 4 dígitos ([02 §5.16](02-base-de-datos-y-hallazgos.md)), así que no identifican a nadie por sí mismos.

```json
// 200 OK
{
  "token": "9f2c1a7e-4b0d-4c8e-9a11-6a2f0d3c8e55",
  "expira": "2026-10-21T21:40:12",
  "empleado": {
    "id_empleado": 6120,
    "nombre": "JUAN DIEGO DUQUE",
    "cargo": "ADMINISTRADOR",
    "codigo_maestra": true
  },
  "turno": { "id_turno": 60172, "fecha": "2026-10-21", "estado": "ABIERTO" }
}
```

Errores: `400 VALIDACION` (falta `id_empleado`, ambas credenciales o ninguna, `id_empleado` como texto o `<= 0`),
`401 NO_AUTORIZADO` (`X-Api-Key`), `403 CLAVE_INVALIDA`, `429 DEMASIADOS_INTENTOS`.
**D12 (decidida el 2026-10-24): paridad con el escritorio**: la API no mira `Empleados.Estado`; un empleado
`INACTIVO` con credenciales correctas entra, y queda una línea en el log. Implementado en la fase 1.
`id_empleado = 0` (comodín "NINGUNO") se rechaza.

> La API **no llama a `sp_Login`**: ese SP cierra el turno anterior y abre uno nuevo cuando cambia la fecha
> ([02 §5.18](02-base-de-datos-y-hallazgos.md)). Valida las credenciales con una consulta propia de solo lectura
> sobre `Empleados`. `turno` es opcional y de solo lectura (último turno existente); si no hay, se omite.
> El “usuario” es el **id numérico** del empleado. Si Flutter necesita login por nombre, se agrega un
> `GET /empleados?q=` previo (solo activos, sin exponer `Password` ni `Codigo_maestro`).

### `GET /auth/me`

Devuelve el empleado y turno asociados al token.

### `POST /auth/logout`

Invalida el token actual. `204 No Content`.

---

## 4. Mesas

### `GET /mesas?fecha=2026-10-21`

`fecha` es opcional (por defecto hoy).

```json
[
  { "id_mesa": 1, "num_mesa": 1, "descripcion": "", "estado": "DISPONIBLE",
    "id_pedido": 0, "hora": null, "cantidad_clientes": 0, "total": 0 },
  { "id_mesa": 2, "num_mesa": 2, "descripcion": "", "estado": "PENDIENTE",
    "id_pedido": 1234, "hora": "13:42", "cantidad_clientes": 2, "total": 49500 }
]
```

`estado` ∈ `DISPONIBLE` | `PENDIENTE` | `SALIENDO`. `fecha` mal formada → `400`. `hora` es `null` en mesas libres;
`total` es la suma de `precio × cantidad` de todas las filas del pedido.

> **Implementado y probado en la fase 2** (65 comprobaciones contra la copia de producción). Reglas heredadas del
> escritorio, que busca las mesas por **`Num_mesa`**:
> * Se devuelve **una mesa por `Num_mesa`** (1..36, sin la 0 de domicilio). En producción hay dos filas con
>   `Num_mesa = 21` (`Id_mesa` 5150 y 5200); la 5200 no tiene un solo pedido en la historia y el escritorio nunca la
>   usa, así que se expone la de menor `Id_mesa` (5150).
> * Si una mesa tuviera varios pedidos abiertos el mismo día, se muestra el más reciente (`Id_pedido` mayor).
> * Un pedido `DISPONIBLE`/`CANCELADO` cuenta como mesa libre. Los abiertos de días anteriores se ignoran (hay 77
>   huérfanos, [02 §5.17](02-base-de-datos-y-hallazgos.md)).
> * En la fase 3, la comprobación "mesa ocupada" del alta debe usar esta misma lógica (por `Num_mesa` y fecha de hoy).

### `GET /mesas/{id}/pedido`

Devuelve el pedido activo de la mesa (`200`) o `204 No Content`. **Pendiente (fase 4)**: necesita el formato de
pedido completo de §6.2.

---

## 5. Catálogo

### `GET /tipos-platos` · `GET /tipos-bebidas`

```json
[ { "id_tipo_plato": 1, "tipo_plato": "ALMUERZOS" } ]
[ { "id_tipo_bebida": 2, "tipo_bebida": "GASEOSAS" } ]
```

### `GET /platos?tipo_plato=1&q=chicharron`

Solo `Estado = 'ACTIVO'`.

```json
[
  {
    "id_plato": 101,
    "nombre": "MENU TRADICIONAL (CON CHICHARRON) SEMANA",
    "precio": 15500,
    "id_tipo_plato": 1,
    "tipo_plato": "ALMUERZOS",
    "descripcion": "",
    "detallado": true,
    "carta": true,
    "imagen_url": "http://192.168.1.20:8099/api/v1/imagenes/plato101.jpeg"
  }
]
```

`imagen_url` es `null` cuando `Imagen_plato` vale `SIN IMAGEN`, está vacío o el archivo no existe en la carpeta de
imágenes. La URL es absoluta y usa el host con el que el cliente llegó al servidor. **Hoy todos los platos y bebidas
activos valen `SIN IMAGEN`**, así que `imagen_url` es siempre `null` hasta que el restaurante cargue imágenes.

> **Qué platos mostrar (D13, resuelta):** el escritorio filtra **solo** por `Estado = 'ACTIVO'`
> (`NPlatos.BuscarPlatos(..., "ACTIVO")` en `FrmPedido.LoadPlatos`); `Plato_carta` no filtra, solo decide si la sopa
> se habilita al elegir los ingredientes. La API hace lo mismo (415 platos) y, a diferencia del escritorio, no limita a
> 50 resultados. Orden: por tipo y nombre. `tipo_plato` es el id (entero; texto → `400`); `q` busca por nombre, máx.
> 50 caracteres, y los comodines de LIKE (`%`, `_`, `[`) se tratan como texto literal.

### `GET /platos/{id}`

Igual que el anterior, más `ingredientes` (la lista global, ver abajo) si `detallado = true`. `404` si no existe o no
está activo; `400` si el id no es un entero.

### `GET /platos/{id}/ingredientes` · `GET /ingredientes?tipo=SOPA`

Los ingredientes **no son por plato**: son una lista global de 14 filas que el escritorio ofrece, igual, en cualquier
plato detallado. `tipo` ∈ `SOPA` | `ARROZ` | `ACOMPANANTE` | `BEBIDA` (sin eñe, así está en la BD). Hay opciones "SIN
SOPA", "SIN ARROZ", "SIN ACOMPAÑANTE" y "SIN BEBIDA". No se filtra por `estado` (el escritorio tampoco; hoy todos son
`ACTIVO`). `GET /platos/{id}/ingredientes` de un plato no detallado devuelve `[]`.

```json
[
  { "id_ingrediente": 1022, "tipo": "ACOMPANANTE", "nombre": "PAPA",
    "descripcion": "", "estado": "ACTIVO" }
]
```

### `GET /bebidas?tipo_bebida=2&q=`

```json
[
  { "id_bebida": 12, "nombre": "GASEOSA 1.5L", "precio": 8500,
    "id_tipo_bebida": 2, "tipo_bebida": "GASEOSAS",
    "descripcion": "", "imagen_url": "http://192.168.1.20:8099/api/v1/imagenes/bebida12.jpeg" }
]
```

> Se usa `Precio_bebida` como precio de venta (es lo que hace `FrmPedido.LoadBebidas`). Los precios de trago
> (`Precio_trago`, `Precio_trago_doble`) no se usan en el flujo actual de pedidos.

### `GET /clientes?q=` · `POST /clientes`

`GET` (implementado): máximo 50 clientes activos, por nombre o teléfono, **sin** el comodín `Id_cliente = 0`
("NINGUNO", que es el valor por defecto del pedido). Devuelve solo lo necesario, no dirección ni correo:

```json
[ { "id_cliente": 103196, "nombre": "CRISTIAN DAVID", "telefono": "3001234567" } ]
```

`POST` (**pendiente**: el pedido de mesa no lo necesita; el escritorio usa siempre el cliente 0):

```json
{ "nombre": "CRISTIAN DAVID", "telefono": "3001234567",
  "correo": "", "direccion": "", "referencia": "", "observaciones": "" }
```

### `GET /imagenes/{archivo}`

Devuelve el binario (`image/jpeg`, `image/png`, `image/gif`, `image/webp`, `image/bmp`) desde la carpeta de imágenes
(`ApiImagenesCarpeta` o, si no está, `RUTAIMAGES` de `App.config`), con `Cache-Control: private, max-age=3600`.
**Exige `X-Api-Key` y token** como el resto (en Flutter, `Image.network(..., headers: {...})`).

`404` —sin explicar el motivo— si el archivo no existe, la carpeta no existe, la extensión no es de imagen, pesa más
de 10 MB o el nombre es inseguro: separadores `/` `\`, `..`, `:`, caracteres no válidos de nombre de archivo. Además
se comprueba que la ruta resultante siga dentro de la carpeta (**path traversal**; probado con `..%2f`, `..%5c`,
`C:%5c…` y `%2e%2e`). El archivo se abre con `FileShare.ReadWrite` porque el escritorio puede tenerlo bloqueado.

---

## 6. Pedidos

### 6.1 `POST /pedidos` — crear pedido completo

**Implementado y probado en la fase 3** (78 comprobaciones de extremo a extremo contra la copia de producción,
incluida concurrencia real). **Idempotente:** repetir el mismo `idempotencia` devuelve el pedido ya creado con `200`
(en vez de `201`).

```json
{
  "idempotencia": "b0f1c2f6-6a1e-4b3e-9f0a-2c9d6e1f7a11",
  "id_mesa": 5050,
  "id_cliente": 0,
  "cantidad_clientes": 2,
  "tipo_pedido": "MESA",
  "observaciones": "",
  "imprimir_comandas": false,
  "items": [
    { "tipo": "PLATO", "id_tipo": 80812, "cantidad": 2, "observaciones": "sin cebolla",
      "ingredientes": [ { "id_ingrediente": 1012 }, { "id_ingrediente": 1026 },
                        { "id_ingrediente": 1020 }, { "id_ingrediente": 3012 } ] },
    { "tipo": "BEBIDA", "id_tipo": 500, "cantidad": 1, "observaciones": "" }
  ]
}
```

La respuesta `201` (y la `200` del reintento) es el **pedido completo** con el mismo formato de §6.2, más `impreso`:

```json
{
  "id_pedido": 14551410, "id_mesa": 5050, "num_mesa": 1, "id_cliente": 0, "cliente": "NINGUNO",
  "id_empleado": 16150, "empleado": "…", "estado": "PENDIENTE", "fecha": "2026-10-24", "hora": "19:41",
  "tipo_pedido": "MESA", "observaciones": "", "cantidad_clientes": 2, "total": 39500,
  "items": [
    { "id_detalle_pedido": 9285020, "tipo": "PLATO", "id_tipo": 80812, "nombre": "…", "precio": 15500,
      "cantidad": 2, "total": 31000, "observaciones": "sin cebolla",
      "ingredientes": [ { "id_ingrediente": 1012, "tipo": "SOPA", "nombre": "SOPA" }, … ] },
    { "id_detalle_pedido": 9285022, "tipo": "BEBIDA", "id_tipo": 500, "nombre": "…", "precio": 8500,
      "cantidad": 1, "total": 8500, "observaciones": "", "ingredientes": [] }
  ],
  "impreso": false
}
```

`impreso` = `true` solo significa que la impresión **se encoló** en el escritorio; no se sabe si la impresora la sacó
(ver "Impresión" abajo). Cada ítem trae el `id_detalle_pedido` que se necesita después para quitarlo.

Campos:

| Campo | Tipo | Obligatorio | Regla |
|---|---|---|---|
| `idempotencia` | string (GUID) | sí | 8–64 caracteres de `A-Z a-z 0-9 _ -`; mismo valor = mismo pedido |
| `id_mesa` | int | sí | `> 0`, debe existir en `Mesas` (`0` es el comodín de domicilio y no se admite) |
| `id_cliente` | int | no (0) | debe existir y estar `ACTIVO` en `Clientes` (el 0 "NINGUNO" siempre vale) → `404` |
| `cantidad_clientes` | int | no (0) | `>= 0`. El escritorio no lo captura: es `0` en el 100 % de los 2.307 pedidos de los últimos 90 días |
| `tipo_pedido` | string | sí | Solo `MESA` en la v1 (el restaurante nunca ha registrado un domicilio; D2). `DOMICILIO` → `422` |
| `observaciones` | string | no | máx. 200 |
| `imprimir_comandas` | bool | no (`ApiImprimirComandas`, por defecto `false`) | encola la impresión después de responder |
| `items` | array | sí | 1..`ApiMaxItems` (200); más → `400` |
| `items[].tipo` | string | sí | `PLATO` \| `BEBIDA` |
| `items[].id_tipo` | int | sí | debe existir (`404`) y estar `ACTIVO` (`422`) |
| `items[].cantidad` | int | sí | 1..999 |
| `items[].observaciones` | string | no | máx. 100 (se trunca, no se rechaza) |
| `items[].ingredientes[]` | array | ver regla | **Plato `detallado`: obligatorio, exactamente un ingrediente `ACTIVO` de cada tipo `SOPA`, `ARROZ`, `ACOMPANANTE` y `BEBIDA`** (hay opciones "SIN SOPA", etc.), también en los platos de carta (las 5.035 líneas de platos de carta desde julio guardan los cuatro; la sopa puede ser "SIN SOPA"). Falta uno, repetido o inactivo → `422`; id inexistente → `404`. **Plato no detallado o bebida: no admite ingredientes** (`422`). |
| `items[].precio` | — | — | Se ignora si se envía |

Cada elemento de `items` es **una fila** del pedido, en el orden recibido. Un mismo producto puede aparecer en
varias filas, cada una con sus propios ingredientes (dos almuerzos con sopas distintas), igual que el escritorio.
`cantidad` multiplica la fila (las `cantidad` > 1 existen en el 8 % de los platos detallados reales).

Reglas de servidor (en orden):

0. Forma del JSON (`400 VALIDACION`): campos obligatorios, tipos estrictos (un número no puede venir como texto),
   límites. Nada de esto toca la BD.
1. `X-Api-Key` + token válidos → `401`.
2. El pedido se registra a nombre del **empleado del token** (D12: no se mira `Estado`). `X-Clave-Maestra` **no está
   implementado**: el PIN solo se usa para iniciar sesión.
3. Idempotencia repetida → `200` con el pedido existente (mismo cuerpo → mismo pedido; cuerpo distinto → `409
   CONFLICTO`). Va **antes** de las reglas de negocio, para que un reintento no falle porque algo cambió después.
4. `tipo_pedido <> 'MESA'` → `422`. Mesa inexistente o que no es la canónica de su `Num_mesa` (p. ej. 5200) → `404`;
   con pedido `PENDIENTE`/`SALIENDO` **de hoy** en cualquier `Id_mesa` de ese `Num_mesa` → `409 MESA_OCUPADA`
   (el mensaje trae la mesa y el `#pedido`). Los abiertos de días anteriores no cuentan.
5. Cliente existente y activo → `404`.
6. Productos existen (`404`) y están activos (`422`); reglas de ingredientes (`404`/`422`, ver tabla).
7. **Precio se toma del catálogo en el SP**; cualquier precio enviado por el cliente se ignora.
8. Cada elemento de `items` es una fila (no se agrupa).
9. Todo se escribe en **una transacción** (`sp_Movil_Insertar_pedido`); si falla algo no queda nada (`500 ERROR_BD`
   o `503 BD_NO_DISPONIBLE`).
10. La comprobación de mesa, el `INSERT` y el registro de la clave son **una sección crítica**: dos peticiones
    simultáneas con la misma clave dan un `201` y un `200` con el mismo pedido; dos claves distintas para la misma
    mesa libre dan un `201` y un `409`.
11. Respuesta inmediata; el refresco del mapa de mesas del escritorio y la impresión se encolan en su hilo de UI
    después (best effort, no pueden fallar la petición).

**Impresión.** `imprimir_comandas` encola, en el hilo de UI del escritorio, el mismo `FrmComandas` que usa `FrmPedido`
(una copia de bebidas y otra de platos, con los ingredientes bajo cada plato). Por defecto está **apagada**
(`ApiImprimirComandas=false`). **No está probada con una impresora real**: si falla, `FrmComandas` muestra su aviso
modal en el escritorio, como cuando falla una impresión hecha desde allí.

### 6.2 `GET /pedidos/{id}`

> **Implementado en la fase 3** (lo necesitan el reintento idempotente y el `201`). `400` si el id no es un entero
> positivo; `404` si no existe. Los pedidos antiguos con ingredientes sin `Id_detalle_pedido` (1.048 filas) se cuelgan
> de la primera fila del mismo plato. `GET /pedidos` (listado), items, estado, cancelar e imprimir siguen en la fase 4.

```json
{
  "id_pedido": 1234, "id_mesa": 1, "num_mesa": 1, "id_cliente": 0,
  "cliente": "NINGUNO", "id_empleado": 6120, "empleado": "JUAN DIEGO DUQUE",
  "estado": "PENDIENTE", "fecha": "2026-10-21", "hora": "13:42",
  "tipo_pedido": "MESA", "observaciones": "", "cantidad_clientes": 2,
  "total": 39500,
  "items": [
    { "id_detalle_pedido": 9285020, "tipo": "PLATO", "id_tipo": 101, "nombre": "MENU TRADICIONAL…",
      "precio": 15500, "cantidad": 2, "total": 31000, "observaciones": "",
      "ingredientes": [ { "id_ingrediente": 5, "nombre": "PAPAS A LA FRANCESA" } ] },
    { "id_detalle_pedido": 9285022, "tipo": "BEBIDA", "id_tipo": 12, "nombre": "GASEOSA 1.5L",
      "precio": 8500, "cantidad": 1, "total": 8500, "observaciones": "", "ingredientes": [] }
  ]
}
```

> Un mismo producto puede venir en **varias filas** (cada una con su `id_detalle_pedido`), porque así lo crea el
> escritorio. El cliente móvil debe agruparlas para mostrarlas, pero conservar los ids para poder quitarlas.

### 6.3 `GET /pedidos?fecha=2026-10-21&estado=PENDIENTE&mesa=1`

Listado resumido (sin ítems). Fecha por defecto: hoy.

### 6.4 `POST /pedidos/{id}/items` — agregar a un pedido existente

```json
{
  "clave_maestra": 9708,
  "items": [
    { "tipo": "BEBIDA", "id_tipo": 15, "cantidad": 2, "observaciones": "" }
  ]
}
```

* Cada elemento de `items` **crea una fila nueva** en el pedido (como el "+" del escritorio). No se suma a filas
  existentes: `sp_Actualizar_detalle_pedido` no tiene semántica de delta (con `@Cantidad` menor que la actual
  **resta** 1; [02 §3.2](02-base-de-datos-y-hallazgos.md)).
* El precio se toma del catálogo, igual que en el alta.
* Solo se permite en pedidos `PENDIENTE` o `SALIENDO` (`422` en otro estado, incluido `DISPONIBLE`).
* Respuesta: `200` con el pedido completo actualizado (mismo formato de §6.2).

### 6.5 `DELETE /pedidos/{id}/items/{id_detalle_pedido}?clave_maestra=9708`

* Quita **una unidad** de esa fila; si era la última, la fila se elimina. (`?cantidad=n` quita `n`, máximo la
  cantidad de la fila.)
* Se direcciona por `id_detalle_pedido` (el que devuelve §6.2), **no** por `tipo`/`id_tipo`: puede haber varias
  filas del mismo producto.
* Exige `clave_maestra` (paridad con `FrmPedido.RemoveProduct`).
* Respuesta `200` con el pedido actualizado; `404` si la fila no existe o no pertenece a ese pedido.

### 6.6 `PATCH /pedidos/{id}/estado`

```json
{ "estado": "SALIENDO" }
```

Transiciones permitidas desde el móvil:

```
PENDIENTE → SALIENDO    (precuenta)
PENDIENTE → CANCELADO   (usar /cancelar)
SALIENDO  → CANCELADO   (usar /cancelar)
```

Cualquier otra transición → `422 REGLA_NEGOCIO`. `estado` solo acepta `SALIENDO`.

> **`DISPONIBLE` (pedido facturado) y facturar quedan fuera de la v1:** lo escribe el escritorio al cobrar
> (38.455 de 38.462 pedidos `DISPONIBLE` tienen venta). No existe estado `TERMINADO` para pedidos: la versión
> anterior de este contrato lo proponía por error. Nota: `sp_Cambiar_estado_pedido` también reescribe `Id_mesa`;
> la API debe pasar la mesa **actual** del pedido para no moverlo.

### 6.7 `POST /pedidos/{id}/cancelar`

```json
{ "observaciones": "El cliente se retiró" }
```

`observaciones` obligatorio (1–200). Deja `Estado_pedido = 'CANCELADO'` (SP `sp_Cancelar_pedido`).

### 6.8 `POST /pedidos/{id}/imprimir`

```json
{ "tipo": "TODO", "copias": 1 }
```

`tipo` ∈ `PLATOS` | `BEBIDAS` | `TODO`. Responde `202 Accepted` y ejecuta la impresión en el hilo de UI.
`409` si ya hay una impresión en curso.

---

## 7. Máquina de estados (resumen)

```
                 ┌──────────────┐
  POST /pedidos →│  PENDIENTE   │
                 └──┬────────┬──┘
      PATCH estado  │        │  POST /cancelar
                    ▼        ▼
              ┌──────────┐  ┌───────────┐
              │ SALIENDO │  │ CANCELADO │
              └────┬─────┘  └───────────┘
                   │ facturar: SOLO en el escritorio (fuera de la v1)
                   ▼
              ┌────────────┐
              │ DISPONIBLE │   (mesa libre; la API no escribe este estado)
              └────────────┘
```

---

## 8. Idempotencia

| Aspecto | Definición |
|---|---|
| Clave | `idempotencia` enviada por el cliente (GUID) |
| Ámbito | por instalación (proceso) |
| Vigencia | 24 h en memoria |
| Respuesta repetida | `200` + el pedido original **tal como está ahora** (mismo `id_pedido`) |
| Conflicto | mismo GUID con payload distinto (se compara el SHA-256 del cuerpo) → `409 CONFLICTO` |
| Pedido borrado | si el pedido de esa clave ya no existe, la clave queda libre y se crea uno nuevo |
| Persistencia | **no** hay tabla: tras reiniciar el `.exe` la clave se olvida. Un reintento posterior lo frena `MESA_OCUPADA` (`409`), no se duplica |

> Recomendación para Flutter: generar el GUID al abrir el carrito, **no** al pulsar enviar, y reutilizarlo en los
> reintentos.

---

## 9. Ejemplos con `curl`

```bash
# ping
curl -s http://192.168.1.20:8099/api/v1/ping

# login
curl -s -X POST http://192.168.1.20:8099/api/v1/auth/login \
  -H "Content-Type: application/json" -H "X-Api-Key: CAMBIAR" \
  -d '{"id_empleado":6120,"password":"******"}'

# mesas
curl -s http://192.168.1.20:8099/api/v1/mesas \
  -H "X-Api-Key: CAMBIAR" -H "Authorization: Bearer $TOKEN"

# crear pedido
curl -s -X POST http://192.168.1.20:8099/api/v1/pedidos \
  -H "Content-Type: application/json" -H "X-Api-Key: CAMBIAR" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"idempotencia":"b0f1c2f6-6a1e-4b3e-9f0a-2c9d6e1f7a11","id_mesa":1,"id_cliente":0,
       "cantidad_clientes":2,"tipo_pedido":"MESA","observaciones":"","imprimir_comandas":false,
       "items":[{"tipo":"PLATO","id_tipo":101,"cantidad":2,"observaciones":"sin cebolla"}]}'
```

---

## 10. Compatibilidad y evolución

* Campos nuevos se agregan como **opcionales**; el cliente debe ignorar los desconocidos.
* No se renombran campos dentro de `/v1`.
* Un cambio de semántica (p. ej. hacer que `POST /items` sume a una fila existente en vez de crear una nueva) obliga a `/v2`.
* El contrato se publica junto con una colección Postman en `docs/postman/` (pendiente de generar en la fase 5).
