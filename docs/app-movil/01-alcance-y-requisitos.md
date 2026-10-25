# 01 — Alcance y requisitos

## 1. Para quién y para qué

| Rol | Qué hace con la app |
|---|---|
| **Mesero** (usuario principal) | Ve el mapa de mesas, abre una mesa libre, arma el pedido con el menú, lo envía a cocina y consulta lo que lleva cada mesa |
| **Cajero / administrador** | Mismo uso, y en la fase 4 podrá quitar productos o cancelar (operaciones que piden clave maestra) |
| **Quien administra el sistema** | Empareja los teléfonos con el servidor y revisa el diagnóstico de conexión |

Contexto real (datos de producción, 2026-10-24): unos **700–850 pedidos al mes**, máximo **65 en un día**, **36 mesas**,
**19 meseros activos**, un catálogo de **415 platos y 160 bebidas activas**, y **ninguna imagen cargada**. La carga es
pequeña; el reto es la **comodidad con una mano, la rapidez y la tolerancia a un WiFi inestable**, no el volumen.

## 2. Casos de uso

| Id | Caso de uso | Etapa | Endpoints (API) |
|---|---|---|---|
| CU-01 | Emparejar el teléfono con el servidor (host, puerto, clave) y probar la conexión | MVP-1 | `GET /ping` |
| CU-02 | Iniciar sesión con empleado + PIN o contraseña | MVP-1 | `POST /auth/login` (+ B1 `GET /empleados`) |
| CU-03 | Ver el mapa de mesas del día con su estado | MVP-1 | `GET /mesas` |
| CU-04 | Explorar y buscar el menú (platos y bebidas) por tipo y texto | MVP-1 | `GET /tipos-*`, `/platos`, `/bebidas` |
| CU-05 | Armar un plato detallado: elegir sopa, arroz, acompañante y bebida (obligatorios) | MVP-1 | `GET /ingredientes` |
| CU-06 | Armar un pedido (borrador) para una mesa libre, con observaciones | MVP-1 | local |
| CU-07 | Enviar el pedido a cocina sin duplicarlo aunque falle la red | MVP-1 | `POST /pedidos` |
| CU-08 | Ver el detalle de un pedido abierto (productos, ingredientes, total) | MVP-1 | `GET /pedidos/{id}` |
| CU-09 | Ver y reintentar los pedidos pendientes de envío | MVP-1 | `POST /pedidos` |
| CU-10 | Cerrar sesión; ver estado de la conexión y diagnóstico | MVP-1 | `POST /auth/logout`, `GET /ping` |
| CU-11 | Agregar productos a un pedido ya enviado | MVP-2 | `POST /pedidos/{id}/items` (fase 4) |
| CU-12 | Quitar un producto (pide clave maestra) | MVP-2 | `DELETE /pedidos/{id}/items/{id}` (fase 4) |
| CU-13 | Pedir la precuenta (pasa a `SALIENDO`) | MVP-2 | `PATCH /pedidos/{id}/estado` (fase 4) |
| CU-14 | Cancelar un pedido con motivo | MVP-2 | `POST /pedidos/{id}/cancelar` (fase 4) |
| CU-15 | Reimprimir la comanda | MVP-2 | `POST /pedidos/{id}/imprimir` (fase 4) |
| CU-16 | Emparejar escaneando un QR | Después | requiere pantalla en el escritorio |

**Fuera de alcance** (siguen en el escritorio): facturar y cobrar, caja y turnos, inventario, reportes, domicilios
(nunca se han usado), administrar menú, clientes (el pedido de mesa usa siempre el cliente 0), cambio de mesa.

## 3. Etapas

| Etapa | Qué entrega | Depende de |
|---|---|---|
| **MVP-1** | CU-01 a CU-10: conectar, entrar, ver mesas, armar y enviar pedidos con tolerancia a fallos, ver pedidos | Solo lo ya implementado en la API (+ B1 deseable) |
| **MVP-2** | CU-11 a CU-15: modificar el pedido tras enviarlo | Fase 4 de la API |
| **Después** | QR, tema por restaurante, imágenes del menú cuando existan, ETag de catálogo | Decisiones y cambios menores del servidor |

## 4. Requisitos funcionales clave (reglas que la app debe respetar)

Vienen del [contrato v1](../04-contrato-api-v1.md); la app las aplica **solo para guiar** al usuario, el servidor sigue siendo la
autoridad.

| Regla | Detalle |
|---|---|
| Plato detallado | Exactamente **un ingrediente de cada tipo**: `SOPA`, `ARROZ`, `ACOMPANANTE`, `BEBIDA` (hay opciones "SIN …"). Sin esto el servidor responde `422` |
| Bebidas y platos simples | No admiten ingredientes |
| Una fila por producto agregado | Dos almuerzos con sopas distintas son dos filas; dos idénticos se pueden fusionar sumando cantidad |
| Límites | Máx. 200 filas; cantidad 1–999; observaciones de fila ≤ 100 y del pedido ≤ 200 caracteres |
| Una mesa, un pedido abierto | `409 MESA_OCUPADA` si ya hay uno de hoy; el mapa muestra el estado actual |
| Precio | Siempre del catálogo del servidor; el teléfono **muestra** el precio y calcula el total para el usuario, pero no lo envía |
| Idempotencia | Una clave por borrador, generada al abrirlo y reutilizada en cada reintento |
| Sesión | Token de 8 h; al vencer se pide volver a entrar **sin perder** el borrador |

## 5. Requisitos no funcionales

| Área | Requisito |
|---|---|
| **Usabilidad** | Operable con una mano; objetivos táctiles ≥ 48 dp; acciones principales abajo; un pedido típico (3 productos) en menos de 30 s |
| **Rendimiento** | Arranque en frío < 2 s hasta la pantalla útil (con caché); mapa de mesas visible < 1 s con datos locales; 60 fps al desplazar el menú |
| **Resiliencia** | Un pedido nunca se pierde ni se duplica por un corte de WiFi; el menú se ve sin conexión (de caché) |
| **Frescura** | El mapa de mesas se refresca cada ~10–15 s con la pantalla visible y la app en primer plano, y al tirar para actualizar |
| **Seguridad** | Token y clave de la API en almacenamiento seguro; solo se conecta a direcciones de red privada; nada sensible en logs |
| **Accesibilidad** | Contraste AA, escala de texto del sistema, estado de mesa por **texto/ícono además de color**, etiquetas semánticas |
| **Idioma** | Español (Colombia); precios en pesos sin decimales (`$ 15.500`); textos en archivos ARB para poder traducir |
| **Compatibilidad** | Android actual y los mínimos de Flutter 3.47; teléfono en vertical como diseño principal y tablets con rejilla adaptativa |
| **Mantenibilidad** | Lints estrictos, generación de código para modelos, cobertura de pruebas objetivo ≥ 80 % en dominio y datos |
| **Privacidad** | Sin telemetría ni nube. Los datos de empleados y pedidos nunca salen de la red del restaurante |
