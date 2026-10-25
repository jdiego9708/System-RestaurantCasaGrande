# 06 — Decisiones pendientes

> Preguntas que condicionan el diseño de la API. Cada una tiene opciones, recomendación e impacto.
> Marcar la opción elegida y la fecha. Las bloqueantes (🔴) impiden avanzar a la fase 3.

---

## Resumen

| ID | Decisión | Bloqueante | Estado / recomendación |
|---|---|---|---|
| D1 | Esquema de BD de producción | ✅ | **Resuelta 2026-10-24**: esquema obtenido, versionado en `db/` y coincide con el código |
| D2 | Alcance: mesas y/o domicilio | 🔴 Sí | **Propuesta cerrada por datos** (0 domicilios en 38.579 pedidos): solo mesas. Falta tu visto bueno |
| D3 | Login del móvil | 🟠 No | `id_empleado` + (contraseña **o** PIN). **Solo PIN descartado** (PIN repetidos) |
| D4 | Impresión de comandas desde el móvil | 🟠 No | Sí, opcional por request (`imprimir_comandas`) |
| D5 | Permisos del móvil | 🟠 No | Crear + agregar; quitar/cancelar solo con PIN |
| D6 | Persistencia transaccional (SP nuevo) | 🔴 Sí | Aprobar `sp_Movil_Insertar_pedido` (aditivo) |
| D7 | Red y permisos del PC | 🟠 No | IP fija + `urlacl` + regla de firewall |
| D8 | Concurrencia esperada | 🟡 No | Máximo histórico 65 pedidos/día y ~34 mesas activas: diseñar para 5–10 dispositivos sobra |
| D9 | Cliente en el pedido | 🟡 No | Opcional; `0 / NINGUNO` por defecto |
| D10 | TLS en la LAN | 🟡 No | No en fase 1 (HTTP + token + allowlist) |
| D11 | Quitar ítems: ¿el escritorio realmente los quita? | 🟠 No | **Aplazada (2026-10-24):** no hace falta ahora; la API usará `Id_detalle_pedido` igualmente |
| D12 | ¿Los empleados `INACTIVO` pueden usar el móvil? | ✅ | **Decidida 2026-10-24: opción A**, paridad con el escritorio. Implementada y probada en el login |
| D14 | Ingredientes de los platos detallados | ✅ | **Decidida 2026-10-24: se exigen los 4 tipos** (`SOPA`, `ARROZ`, `ACOMPANANTE`, `BEBIDA`), sin excepción para los platos de carta. Implementada y probada |
| D13 | Qué platos muestra el catálogo | ✅ | **Resuelta 2026-10-24**: solo `Estado='ACTIVO'`, igual que el escritorio (`Plato_carta` no filtra). Implementado |

---

## D1 — Esquema de base de datos de producción ✅ RESUELTA (2026-10-24)

**Resultado:** se obtuvieron el script de esquema y el respaldo completo del equipo del restaurante. El esquema se
versionó en `db/produccion-schema.sql`, se restauró una copia local (`CasaGrandeBD_prod`) y se comprobó que las
firmas de los SPs coinciden con el código. El desajuste de la versión del 2026-10-21 era de la LocalDB de
desarrollo, no de producción ([02 §4](02-base-de-datos-y-hallazgos.md)).

**Lo que queda abierto de esta decisión:** versión exacta y *collation* del servidor, y confirmar que el `.exe`
instalado es la build del repo (tareas 0.3 y 0.8 del plan). No bloquean.

> Cuidado con los datos: la copia incluye empleados, claves y ventas reales. Solo para desarrollo local; no se
> versiona (ver [`db/README.md`](../db/README.md)).

---

## D2 — Alcance funcional de la fase 1 🔴

**Contexto:** el alta de domicilios en el escritorio está rota en el código del repo
([01 §9](01-flujo-actual-de-pedidos.md)). **Dato de producción (2026-10-24):** el 100 % de los 38.579 pedidos
es `MESA`; el restaurante nunca ha registrado un domicilio. Esto inclina la decisión a la opción A.

**Opciones**

| Opción | Alcance | Esfuerzo |
|---|---|---|
| A. Solo pedidos de mesa | `tipo_pedido = MESA` | Base |
| B. Mesa + domicilio | agrega cliente obligatorio, dirección y reglas propias | +2–3 días |
| C. Mesa + domicilio + facturación desde el móvil | incluye `Ventas`, medios de pago | +1 semana o más |

**Recomendación:** A en fase 1; B como fase 1.5 una vez validado el flujo base.
**Impacto:** define endpoints, validaciones y pruebas.

---

## D3 — Cómo se autentica el mesero en el móvil 🟠

**Contexto:** `sp_Login` usa `Id_empleado` como “usuario” y `Password`; el PIN de 4 dígitos
(`Codigo_maestro`) es lo que el escritorio pide al abrir el pedido.

**Opciones**

| Opción | Experiencia | Seguridad |
|---|---|---|
| A. `id_empleado` + contraseña | Incómoda en móvil (contraseñas largas) | Igual que el escritorio |
| B. Solo PIN de 4 dígitos | Muy rápida | Baja (4 dígitos, sin bloqueo) |
| C. A o B (el cliente elige) | Flexible | La que se implemente |

**Recomendación:** C, con **bloqueo tras 5 intentos** y registro en log. El PIN autoriza la sesión; el token
identifica al empleado en cada pedido.
**Impacto:** define `POST /auth/login` y si `clave_maestra` viaja en cada request.

> **Actualización 2026-10-24:** la opción B (solo PIN) queda **descartada por los datos**: entre los 26 empleados
> activos hay 2 PIN repetidos (4 empleados) y 8 PIN que no tienen 4 dígitos. El PIN solo no identifica a nadie.
> Las opciones A y C se mantienen **siempre con `id_empleado`** (contraseña o PIN de ese empleado). Además, el
> login de la API no usa `sp_Login` porque ese SP cierra y abre turnos ([02 §5.18](02-base-de-datos-y-hallazgos.md)).

---

## D4 — ¿El pedido del móvil imprime comanda? 🟠

**Contexto:** el escritorio imprime por defecto (`chkPrintComandas` marcado). La impresión usa ReportViewer
(hilo de UI) y puede tardar.

**Opciones**

| Opción | Efecto |
|---|---|
| A. Sí, por defecto | Cocina recibe la comanda sin intervención |
| B. Solo si el request lo pide | Control total desde el móvil |
| C. No en fase 1 | Menos riesgo; el escritorio imprime |

**Recomendación:** B con valor por defecto configurable (`ApiImprimirComandas`).
**Impacto:** complejidad de hilos, manejo de errores de impresora, tiempos de respuesta.

---

## D5 — Permisos del móvil 🟠

| Acción | ¿Permitida desde el móvil? | Requiere PIN |
|---|---|---|
| Crear pedido | Sí (recomendado) | Solo al iniciar sesión |
| Agregar ítems | Sí | No |
| Quitar ítems | Recomendado | Sí |
| Cambiar estado (SALIENDO) | Recomendado | No |
| Cancelar pedido | Recomendado | Sí |
| Facturar / cobrar | **No en fase 1** | — |
| Cambiar de mesa | Fase 2 | — |

**Recomendación:** la tabla anterior. Refleja los permisos reales del escritorio.
**Impacto:** endpoints disponibles y pruebas de autorización.

---

## D6 — ¿Se autoriza el SP nuevo transaccional? 🔴

**Contexto:** el guardado actual no es atómico ([02 §5.1](02-base-de-datos-y-hallazgos.md)).

**Opciones**

| Opción | Atomicidad | Cambios en BD |
|---|---|---|
| A. Reutilizar `sp_Insertar_pedido` + `sp_Actualizar_detalle_pedido` | ❌ | Ninguno |
| B. SP nuevo `sp_Movil_Insertar_pedido` | ✅ | 1 objeto nuevo (aditivo) |

**Recomendación:** B. No modifica SPs existentes ni el comportamiento del escritorio.
**Impacto:** si se rechaza, hay que aceptar el riesgo de pedidos parciales o implementar la transacción en C#
(`SqlTransaction`) reutilizando los comandos actuales — también válido, pero con más código y menos control del
precio.

> Alternativa C (sin tocar BD): abrir `SqlTransaction` en un método nuevo de `CapaDatos` y ejecutar
> `sp_Insertar_pedido` + `sp_Actualizar_detalle_pedido` dentro de la misma transacción. Es la opción de
> contingencia si no se permite crear el SP.

---

## D7 — Red y permisos del PC servidor 🟠

Necesito confirmar:

1. ¿Se puede asignar **IP fija** o reserva DHCP al PC? (IP actual: ______________)
2. ¿Hay acceso de **administrador** para ejecutar `netsh http add urlacl` y la regla de firewall?
3. ¿Windows 10 u 11? ¿El equipo se apaga/duerme en algún momento?
4. ¿El restaurante tiene switch/AP propio o red compartida con clientes? (define si se usa allowlist)

**Recomendación:** IP fija, `urlacl` para “Everyone”, firewall solo perfil privado, y no exponer el puerto a internet.

---

## D8 — Concurrencia esperada 🟡

¿Cuántos dispositivos usarán la API al mismo tiempo? (meseros + caja + posibles tablets)

**Recomendación de diseño:** soportar 5–10 peticiones concurrentes sin bloqueos; escrituras de alta serializadas.
**Impacto:** pruebas de carga y política de reintentos del cliente.

> **Dato de producción:** el día más cargado fue de 65 pedidos, el volumen es de unos 700–850 pedidos al mes en 2026 y hay
> unas 34 mesas activas. La carga es mínima; sigue faltando saber cuántos dispositivos (meseros) usarán el móvil a la vez.

---

## D9 — Cliente en el pedido 🟡

Hoy el pedido de mesa puede ir con `Id_cliente = 0` (“NINGUNO”). En domicilio, el cliente es imprescindible.

**Recomendación:** en mesa, opcional; en domicilio, obligatorio (con alta rápida desde el móvil).

---

## D10 — TLS 🟡

**Recomendación:** no en fase 1. HTTP en LAN + `X-Api-Key` + token + allowlist de IP.
Si más adelante se requiere, evaluar un proxy inverso (Caddy/IIS) con certificado y confianza en el dispositivo.

---

## D11 — ¿El escritorio realmente quita los productos ya guardados? 🟠

**Contexto:** `FrmPedido.RemoveProduct` (`:553-564`) llama a `sp_Actualizar_detalle_pedido` sin `Id_detalle_pedido`
(queda en 0) y con `"DELETE"`; el SP, con id 0 y `DELETE`, no hace nada
([02 §3.2 y §5.14](02-base-de-datos-y-hallazgos.md)). Verificado a nivel de SP, **no en la aplicación**.

**Qué hay que probar (5 minutos, en el equipo del restaurante o con la copia local):** abrir un pedido `PENDIENTE`
con varias unidades, quitar una con la clave maestra, cerrar y reabrir el pedido. ¿Sigue ahí?

| Resultado | Consecuencia |
|---|---|
| Se quita | Hay otro camino que no vimos; revisar `ProductBinding` y `NPedido` |
| **No** se quita | Error preexistente del escritorio. La API no lo replica (usa el id real). Avisar al restaurante |

**Impacto:** no cambia el diseño de la API (siempre usa `Id_detalle_pedido`), pero sí lo que se le cuenta al cliente.

---

## D12 — ¿Los empleados `INACTIVO` pueden usar el móvil? ✅ DECIDIDA: A (2026-10-24)

**Decisión:** "seguir como en el escritorio": la API no mira `Empleados.Estado` al hacer login. Queda en el log una
línea `Login de empleado no ACTIVO … D12` cada vez que ocurre, para poder pasar a B (exigir `ACTIVO`) cuando el
restaurante corrija las fichas. Probado en `scripts/Test-ApiFase1.ps1` (un empleado inactivo entra con 200).
El id `0` (la fila comodín "NINGUNO") sí se rechaza, como hace `sp_Login`.

<details><summary>Planteamiento original</summary>

**Contexto:** `sp_Login` no valida `Empleados.Estado`. Un mesero `INACTIVO` tomó **93 pedidos en los últimos 30 días,
el último el 2026-10-24** ([02 §5.15](02-base-de-datos-y-hallazgos.md)). Puede ser una persona que sigue trabajando
sin que se actualizara su ficha, o una cuenta compartida.

| Opción | Efecto |
|---|---|
| A. Paridad con el escritorio (no mirar `Estado`) | Nadie queda bloqueado; se hereda el hueco de seguridad |
| B. Exigir `ACTIVO` | Más correcto; hoy dejaría fuera a quien trabaja |
| C. A al principio, con el log marcando los inactivos, y pasar a B cuando se corrijan las fichas | Transición sin sorpresas |

**Recomendación:** C. **Decidir con el restaurante** quién es ese mesero antes de la fase 1.
**Impacto:** una línea en `ApiAuth` y la prueba "login empleado inactivo" del plan.

</details>

---

## D13 — ¿Qué platos muestra el catálogo? ✅ RESUELTA (2026-10-24)

**Resultado:** se leyó el código. `FrmPedido.LoadPlatos` llama a `NPlatos.BuscarPlatos(..., "ACTIVO")` y
`sp_Buscar_platos` filtra solo por `Estado = 'ACTIVO'` (415 de 626 platos). `Plato_carta` (148 activos) no filtra:
únicamente decide si se habilita la sopa al elegir ingredientes (`IsEnabledSopa = !isCarta`). `Plato_detallado`
(153 activos) decide si se piden ingredientes. La API replica el filtro; la única diferencia es que no recorta a 50
resultados como hace el escritorio en las búsquedas por tipo y nombre.

---

## D14 — Ingredientes obligatorios en los platos detallados ✅ DECIDIDA (2026-10-24)

**Decisión:** `POST /pedidos` exige, en cada fila de un plato detallado (`Plato_detallado = 'ACTIVO'`), **exactamente un
ingrediente activo de cada tipo**: `SOPA`, `ARROZ`, `ACOMPANANTE` y `BEBIDA`. Los platos no detallados y las bebidas no
admiten ingredientes.

**Por qué sin excepción para la carta:** se había propuesto eximir la sopa en los platos de carta (el escritorio
deshabilita ese selector). Los datos lo desmienten: desde el 2026-07-01 las **5.035 líneas de platos de carta** guardan
sopa + arroz + acompañante + bebida; la sopa elegida fue `SOPA` (4.966), `CREMA` (58) o `SIN SOPA` (11). Es decir, también
ahí se guarda una sopa, y "SIN SOPA" es la forma de no querer. El móvil debe ofrecer las opciones "SIN …".

**Efecto en datos históricos:** hay platos detallados guardados **sin** ingredientes (40 líneas desde julio, 1.404 en
total desde 2022). Son restos del guardado no transaccional del escritorio ([02 §5.1](02-base-de-datos-y-hallazgos.md)),
no una regla de negocio; la API no los permite.

---

## D4 (actualización) — impresión de comandas desde el móvil

Implementada la opción B (`imprimir_comandas` por petición; por defecto `ApiImprimirComandas=false`). Encola la
impresión en el hilo de UI y responde antes. **No probada con impresora real.** Ver [04 §6.1](04-contrato-api-v1.md).

---

## Plantilla de respuesta

```
D1: ✅ resuelta
D2: [A/B/C]  →  _____________________________
D3: [A/B/C]  →  _____________________________
D4: [A/B/C]  →  _____________________________
D5: [sí/no por acción]
D6: [A/B/C]  →  _____________________________
D7: IP=______  admin=[sí/no]  Windows=______  red=______
D8: dispositivos=______
D9: [opcional/obligatorio]
D10: [HTTP/TLS]
D11: aplazada (no necesaria ahora)
D12: ✅ A (paridad con el escritorio), 2026-10-24
D13: ✅ solo Estado=ACTIVO (como el escritorio)
D14: ✅ los 4 tipos de ingrediente, sin excepción para la carta
```

Una vez respondidas, actualizar este documento con la decisión y la fecha, y ajustar
[04](04-contrato-api-v1.md) si cambia el contrato.
