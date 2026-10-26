# 08 — Plan y dependencias con la API

## 1. Fases de construcción de la app

Estimaciones **aproximadas** para una persona a tiempo completo, sin contar espera de decisiones ni de pruebas en el
restaurante. Cada fase termina con algo que se puede ejecutar y probar.

| Fase | Entrega | Criterio de aceptación | Días (aprox.) |
|---|---|---|---|
| **F0 Fundaciones** | Repositorio, *workspace*, CI, lints, tema, l10n, enrutado base, Drift con migraciones, almacenamiento seguro, registro, prueba de arquitectura. **Primero: crear el proyecto y resolver conflictos de `analyzer`** ([06 §4.1](06-librerias-y-herramientas.md)) | `analyze`, `test` y `build apk` en verde en CI; la app abre una pantalla vacía en un teléfono | 3–4 |
| **F1 Cliente de la API** | `casa_grande_api`: DTOs, `CasaGrandeApi`, interceptores, errores, validación de URL; pruebas unitarias y **contrato en vivo** | Las pruebas `live` pasan contra `run-local.ps1`; cubren `/ping`, login, mesas, menú, `POST /pedidos` (idempotencia y 409) | 3–4 |
| **F2 Conexión y sesión** | `server_setup` + `auth` (con B1) | Emparejar, entrar y salir; sesión vencida redirige a Login; token en almacenamiento seguro | 3–4 |
| **F3 Mesas y menú** | `tables` (sondeo) + `menu` (caché primero, búsqueda, tipos) | Mapa fiel al escritorio; menú visible sin red tras la primera carga | 4–5 |
| **F4 Pedido** | `cart` + hoja "Armar plato" + `orders` (`SubmitOrder`, outbox, sincronizador, detalle, pendientes) | Toda la matriz de [07 §2.1](07-calidad-seguridad-y-pruebas.md) en verde; pedido real entre un teléfono y la copia de producción | 6–8 |
| **F5 Pulido y entrega** | `settings`, diagnóstico, tablets, accesibilidad, rendimiento en gama baja, ícono/arranque, APK firmado, manual corto | Recorridos de integración en un teléfono físico; APK instalable | 3–4 |
| **MVP-1** | | | **≈ 22–29** |
| **F6 Edición de pedidos** (con fase 4 de la API) | Agregar, quitar con PIN, precuenta, cancelar, reimprimir | Los casos CU-11 a CU-15 contra la API real | 5–7 |

Orden de trabajo: **vertical por funcionalidad** (cada módulo se termina con datos reales y pruebas) y no horizontal (primero
toda la red, luego toda la UI). Las fases F1 y F0 pueden solaparse.

## 2. Lo que falta en la API (trabajo del lado del escritorio)

| Id | Cambio | Para qué | Tamaño | Bloquea |
|---|---|---|---|---|
| **B1** | `GET /empleados` — empleados **activos** con `id_empleado`, `nombre`, `cargo` (nunca contraseña ni PIN); exige clave de aplicación | Selector de empleado en el Login (A5). Sin él, el mesero escribe su id numérico | 0,5 día | F2 (solo comodidad) |
| **B2** | Fase 4 de la API: `POST /pedidos/{id}/items`, `DELETE …/items/{id}`, `PATCH …/estado`, `POST …/cancelar`, `POST …/imprimir`, `GET /pedidos`, `GET /mesas/{id}/pedido` | Edición del pedido (CU-11 a CU-15) | 5–7 días (ver [05](../05-plan-de-trabajo.md)) | F6 |
| **B3** | `ETag`/`If-None-Match` en `/platos`, `/bebidas`, `/ingredientes`, `/tipos-*` | No volver a descargar el menú si no cambió | 0,5 día | nada (optimización) |
| **B4** | Pantalla en el escritorio con un QR de emparejamiento (host, puerto, clave) | Emparejar sin escribir (CU-16) | 1–2 días | nada |
| **B5** | Claves por dispositivo y revocación individual | Perder un teléfono sin cambiar la clave de todos | 2–3 días | nada (seguridad) |
| **B6** | `X-Clave-Maestra` en las operaciones sensibles | Quitar/cancelar con el PIN de otro empleado (hoy solo el del token) | parte de B2 | F6 |
| **B7** | **Desplegar el SP** (`sp_Movil_Insertar_pedido`, D6) y preparar la red del PC: IP fija, `urlacl`, firewall (D7), probar `/ping` desde otro equipo | Cualquier prueba real en el restaurante | 1 día + visita | Pruebas de campo |
| **B8** | Cargar imágenes del menú (`D:\ImagenesRestaurante\`) | Miniaturas en el menú | Operativo (el restaurante) | nada |
| **B9** | Probar la impresión con una impresora real | `imprimir_comandas` | 0,5 día en sitio | CU-15 |
| **B10** | Documento OpenAPI 3.1 del contrato | Fuente única para las pruebas de contrato y, si se desea, generación de clientes | 1 día | nada |

La app **puede empezar ya** (F0–F4) con lo que existe: login, mesas, menú, `POST /pedidos` y `GET /pedidos/{id}` están
implementados y probados. Solo B7 es imprescindible para probar en el restaurante.

## 3. Riesgos

| Riesgo | Prob. | Efecto | Mitigación |
|---|---|---|---|
| El WiFi del restaurante es inestable | Alta | Pedidos que tardan o fallan | *Outbox* + idempotencia + menú en caché (el diseño central) |
| El PC del restaurante cambia de IP (DHCP) | Media | La app deja de encontrar el servidor | Pedir IP fija o reserva DHCP (B7); pantalla de diagnóstico que lo explica; permitir cambiar el servidor en dos toques |
| El PC se duerme/apaga o el `.exe` no está abierto | Media | API caída | Banner "Servidor no disponible" con la última conexión; documentar arranque automático; los pedidos esperan en la bandeja |
| Choque de versiones de `analyzer` entre generadores | Media | No compila | Verificarlo el primer día (F0); fijar versiones; subir el generador atrasado |
| El contrato de la API cambia | Media | Roturas | Versionado `/v1`, DTOs tolerantes, pruebas `live` en CI local, OpenAPI (B10) |
| Impresión de comandas sin probar con impresora real | Media | Comandas que no salen o avisos modales en el escritorio | La app ahora envía siempre `imprimir_comandas: true` con 2 copias por defecto (`ApiImprimirComandas` solo rige si el request no lo indica); prueba en sitio (B9) |
| Dos meseros abren la misma mesa a la vez | Media | Un `409` | Diseñado: el segundo ve el conflicto con el número de pedido |
| Un teléfono viejo con poca memoria | Media | Lentitud | Probar en gama baja en F5; presupuesto de rendimiento |
| Cambio de Flutter/Dart a mitad del proyecto | Baja | Roturas de generación | Versión fijada; actualizar solo entre releases |

## 4. Próximos pasos

1. **Confirmar las decisiones A1–A8** de [README](README.md) (o aceptar las recomendadas).
2. Crear el repositorio `CasaGrandeMesero` (workspace + CI) y resolver F0, empezando por el `pub get` con las versiones de
   [06](06-librerias-y-herramientas.md).
3. En paralelo, hacer **B1** en el escritorio (media jornada) y **B7** (prerrequisito para las pruebas reales).
4. Construir `casa_grande_api` (F1) con su contrato en vivo y avanzar por módulos en el orden de la tabla.
