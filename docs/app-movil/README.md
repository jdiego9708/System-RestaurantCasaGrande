# App móvil del mesero (Flutter) — modelado

> **Estado:** diseño previo al código (2026-10-24). Nada de lo que sigue está implementado.
> **Qué es:** una app para que los meseros tomen pedidos de mesa desde el celular, hablando por la red local con la API
> que ya corre dentro del escritorio ([contrato v1](../04-contrato-api-v1.md); fases 1–3 implementadas y probadas).
> **Qué no es:** no reemplaza al escritorio. Facturar, caja, inventario y reportes siguen allí.

---

## Índice

| Documento | Contenido |
|---|---|
| [01 — Alcance y requisitos](01-alcance-y-requisitos.md) | Usuarios, casos de uso, MVP por etapas, requisitos no funcionales |
| [02 — Arquitectura](02-arquitectura.md) | Capas, gestión de estado, navegación, red, offline/outbox, errores, seguridad |
| [03 — Módulos y estructura](03-modulos-y-estructura.md) | Paquetes, módulos por funcionalidad, reglas de dependencia, árbol de carpetas |
| [04 — Modelo de dominio y de datos](04-modelo-de-dominio-y-datos.md) | Entidades, correspondencia con el JSON de la API, esquema local (SQLite), almacenamiento seguro |
| [05 — Pantallas y flujos](05-pantallas-y-flujos.md) | Mapa de navegación, inventario de pantallas, estados de cada una, flujos clave |
| [06 — Librerías y herramientas](06-librerias-y-herramientas.md) | Versiones **verificadas en pub.dev el 2026-10-24** y por qué cada una |
| [07 — Calidad, seguridad y pruebas](07-calidad-seguridad-y-pruebas.md) | Lints, estrategia de pruebas, CI, seguridad, accesibilidad |
| [08 — Plan y dependencias con la API](08-plan-y-dependencias-con-la-api.md) | Fases de construcción, lo que falta en la API y riesgos |

---

## Resumen en diez líneas

1. **Una app, dos paquetes** (workspace de Dart): `casa_grande_mesero` (la app) y `casa_grande_api` (cliente HTTP + DTOs,
   Dart puro, probable sin Flutter).
2. **Organización por funcionalidad** (*feature-first*) con tres capas por módulo: `presentation → domain ← data`.
   Siete módulos: `server_setup`, `auth`, `tables`, `menu`, `cart`, `orders`, `settings`.
3. **Riverpod 3** para estado e inyección de dependencias, **go_router** para navegación con guardas de sesión.
4. **Dio** para red, con interceptores de clave de aplicación, token y errores; todo error se traduce a un tipo propio.
5. **Cache-primero para el menú** y **bandeja de salida (*outbox*)** para los pedidos: el pedido se guarda en el
   teléfono con su clave de idempotencia **antes** de enviarse; si el WiFi falla se reintenta sin duplicar.
6. **Drift (SQLite)** para borradores, bandeja de salida y caché; **flutter_secure_storage** para token y clave de la API.
7. **El servidor manda:** el teléfono valida solo para ayudar al usuario; precios, reglas y estados los decide la API.
8. **Sin Firebase ni servicios en la nube**: todo vive en la red del restaurante.
9. **Español en la interfaz** con `gen-l10n` desde el primer día; código en inglés, comentarios y commits en español.
10. **MVP con lo que la API ya tiene** (login, mesas, menú, crear y ver pedido); editar/cancelar/precuenta llegan con la
    fase 4 de la API.

---

## Decisiones por confirmar

Cada una trae una recomendación; el resto del modelado asume las recomendadas.

| # | Decisión | Recomendación | Efecto si cambia |
|---|---|---|---|
| A1 | Plataformas | **Solo Android** (decidido 2026-10-24; sin Windows, Linux ni iOS) | Las carpetas de otras plataformas no existen en el repositorio |
| A2 | Dónde vive el código | **Repositorio nuevo** `CasaGrandeMesero` en `D:\Repositorios` | Si va en el repo del escritorio, mezcla dos toolchains y dos ciclos de release |
| A3 | Cómo se distribuye | APK firmado por **instalación directa** (o *internal testing* de Play) | Play Store público exige cuenta, políticas y revisión |
| A4 | Emparejar teléfono ↔ servidor | **Manual** (IP, puerto, clave) en el MVP; **QR** después | El QR exige una pantalla nueva en el escritorio |
| A5 | Cómo se identifica el mesero | **Solo PIN** (decidido 2026-10-24); el PIN trae al empleado. Si lo comparten varios, la API responde `409 PIN_AMBIGUO` y la app muestra un selector. B1 eliminado | PIN cortos o repetidos: ver riesgo en el contrato §3 |
| A6 | Tablets | **Sí, adaptativo** (misma app, rejilla que se ajusta) | Solo teléfono simplifica pruebas visuales |
| A7 | Identidad visual | Material 3 con color semilla del restaurante; **falta logo y paleta** | Solo afecta tema e ícono |
| A8 | Identificador de la app | `com.casagrande.mesero` (cambiable hasta publicar) | Cambiarlo después obliga a reinstalar |

Dependencias del lado del servidor (lista completa en [08](08-plan-y-dependencias-con-la-api.md)): **B1** `GET /empleados`
(para A5) y la **fase 4** de la API para editar pedidos.
