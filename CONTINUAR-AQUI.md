# Continuar aquí — Casa Grande / Alegra

Última comprobación: **6 de noviembre de 2026, 16:45–16:46, America/Bogota**. Este archivo es el punto de entrada para el próximo agente. El registro completo está en [cierre de sesión](docs/15-cierre-de-sesion-2026-11-06.md).

## Estado exacto al entregar

| Venta local | Factura | ID Alegra | Total COP | Emisión | Impresión/guardado |
|---|---|---|---:|---|---|
| 14327726 | FE4930 | 31954 | 116800 | ACCEPTED, con observaciones | PRINTED |
| 14327728 | FE4931 | 31955 | 43800 | ACCEPTED, con observaciones | **PRINTING: diálogo de guardado pendiente** |
| 14327730 | FE4932 | 31956 | 172200 | ACCEPTED, con observaciones | **PRINTED, circuito automático desde venta nueva validado** |

No volver a crear ni emitir estas facturas. API confirmó CUFE, total, pago completo y saldo 0 para FE4931/FE4932. Las observaciones FAZ09 y FAJ44b no son un rechazo, pero requieren revisión fiscal/catalogación/receptor.

**Pendiente inmediato de usuario:** el proceso técnico `Recover-Alegra-14327728.exe`, observado como PID 38228, sigue vivo con la ventana «Guardar factura electronica de Alegra». UI Automation confirmó que estaba en pantalla. El usuario debe guardar o cancelar ese diálogo de FE4931. No cerrarlo forzosamente ni marcar PRINTED por SQL. Cancelar deja CANCELLED sin deshacer la aceptación; permite reimpresión manual. Verificar otra vez porque los PID/estados pueden haber cambiado.

El escritorio actualizado estaba abierto como PID 38148. Cerrar ambos procesos normalmente antes de recompilar LocalBuild, porque mantienen DLL cargadas. El recuperador no es una herramienta de uso general: **no volver a ejecutarlo**; su única recuperación ya terminó la emisión. Si muere con PrintStatus PRINTING, conciliar archivo/cola antes de habilitar una reimpresión manual; no reenviar la factura para arreglar la impresión.

## Repositorios y datos

- Escritorio: `D:/Repositorios/System-RestaurantCasaGrande`.
- Móvil: `D:/Repositorios/CasaGrandeMesero`. No se desarrolló una nueva funcionalidad móvil en esta sesión; se recompiló su estado actual para el kit. Hay cambios anteriores/simultáneos del usuario que se deben conservar.
- Base local activa del ejecutable: `CasaGrandeBD_prod`, `(localdb)\MSSQLLocalDB`. Leer siempre `SISMistico/SISMistico/bin/LocalBuild/SISMistico.exe.config` para confirmar la conexión. El restaurante utiliza normalmente `CasaGrandeBD`; no confundirlos.
- Relación: `Ventas.Id_venta → AlegraInvoiceRequests.SaleId`, única por venta. Solicitud y venta transaccionales.
- PDF/datos fiscales: `%LOCALAPPDATA%/CasaGrande/Alegra/PDF`. `PdfPath` conserva el oficial; `{SaleId}-80mm.pdf` es la representación propia. No copiar datos fiscales al repositorio público.
- Snapshot previo a la recuperación: `%LOCALAPPDATA%/CasaGrande/Alegra/Recovery`, archivo de venta 14327728. Conservado como evidencia, sin cambiar la venta ni los pagos.

Hay numerosos cambios sin commit en ambos repositorios, incluidos cambios previos y del usuario. **No se hizo commit, push, reset/clean ni despliegue en producción. No descartar ni atribuir toda la lista de git status a esta sesión.**

## Decisiones finales del usuario

1. Al marcar factura electrónica y terminar: conservar comprobante local; emitir en segundo plano y, tras aceptación, generar/imprimir la representación electrónica de **80 mm**. El visor no es necesario.
2. **Descargar PDF desde el detalle:** oficial Alegra en Carta, sin modificar. **Imprimir factura de Alegra desde esa venta:** renderer propio de 80 mm. No reducir Carta para una térmica.
3. Impresora vacía: usar predeterminada Windows, igual que local. Microsoft Print to PDF/Adobe PDF abre diálogo de guardado del térmico. Se probó con diálogo real y copia byte a byte; FE4932 ya llegó a PRINTED desde el escritorio.
4. País **Colombia**, Departamento **Caldas**, Municipio/Ciudad **Manizales** por defecto; editables y reemplazados por datos del contacto elegido.
5. Clientes consultados: grid moderno nombre + Seleccionar, aviso de consulta justo encima. Selectores persona/documento/régimen con estilo moderno.
6. Historial: columnas Factura electrónica, Número y Estado electrónico.
7. Por autorización explícita, la USB privada lleva ambos TXT renombrados. Nunca mostrar su contenido ni incorporarlo al código/documentación.

## Credenciales: hallazgo que resolvió la emisión

- Archivo nuevo: `D:/Setup/apikeyAlegraCasaGrandeNuevo.txt`, **Bearer**, principal para crear/consultar catálogos.
- Archivo viejo: `D:/Setup/apikeyAlegraCasaGrandeViejo.txt`, **Basic**, emisión y consulta fiscal/PDF.
- Variables principales: `ALEGRA_EMAIL`, `ALEGRA_API_KEY`, `ALEGRA_AUTH_SCHEME`.
- Variables de emisión: `ALEGRA_EMISSION_EMAIL`, `ALEGRA_EMISSION_API_KEY`, `ALEGRA_EMISSION_AUTH_SCHEME`.
- Prioridad efectiva por variable: **Machine > User > Process**. En este equipo estaban configuradas por usuario. El nuevo paso 05 detecta conflictos Machine.
- `POST /invoices/stamp`, cuerpo `{"ids":["ID"]}`: Bearer nuevo devolvió 403; Basic viejo devolvió 200 y aceptación para el mismo ID. **No era necesario cambiar permisos después de utilizar el token Basic correcto.**
- GET con Bearer podía omitir stamp/XML y el PDF daba 500; con Basic sí se obtuvieron estado, XML y PDF oficial. No interpretar ausencia de stamp con una credencial como prueba definitiva de rechazo.
- La API company no devuelve id en esta cuenta; se usa el identificador alternativo existente (`applicationVersion` + identificación). No eliminar ese fallback.

## Hallazgo posterior: descuento / coma decimal, ya corregido

La venta 14327728 de 43800 tenía pago SQL correcto. El snapshot versión 1 guardó `"43800,00"`; cultura invariante con miles permitidos lo interpretaba como otro valor. Builder rechazó la suma **antes de crear una factura remota**, por eso no existía en el portal ni había PDF.

Corrección definitiva:

- `DVenta.FreezeElectronicRequest`: congela importes/pagos desde SQL, dentro de la transacción de venta, como **números JSON**, snapshot **versión 2**. Si pagos SQL no suman el total, revierte antes de terminar la venta. Añadida referencia framework `System.Web.Extensions` a CapaDatos.
- `AlegraInvoiceBuilder.Number`: soporta decimales locales antiguos sin tratar la coma como miles; rechaza separadores mezclados ambiguos. Pruebas de contratos pasan de 24 a **26**.
- `DAlegra`: RemoteId vacío se guarda como NULL y recuperación/reintento trata ambos equivalentes. Importante para no considerar conocido un ID vacío tras una interrupción incierta.
- Se revisaron 23 facturas del día por API para el identificador único de la solicitud, ninguna coincidía. Se respaldó el snapshot y se reparó/reanudó **esa misma solicitud**, sin nueva venta. Resultado FE4931 aceptada. FE4932 nueva confirmó snapshot versión 2 y éxito automático con PRINTED.

## USB / instalador: ya preparado y verificado

Kit local listo: **`D:/Setup/CasaGrande-USB-validada-20261106/Casa Grande Installers`**. No se escribió una USB física ni se instaló en el restaurante.

- En PC ejecutar **01, 02, 03 y 05**. El 04 es el APK de Android.
- 01: respaldo/migraciones/verificación SQL, incluida 07 Alegra. **Sin tokens ni dependencia de conexión a Alegra**.
- 02: instalador Windows **64 bits** con renderer portable y SumatraPDF. Apertura del sistema al terminar, desmarcada. Mantenerlo cerrado hasta 05.
- 05 **autocontenido**: importar TXT de `Credenciales-Alegra`, solicitar correo si falta, comprobar empresa/herramientas y configurar SQL. Por defecto Scope User; si Machine diferente tiene prioridad, requiere mismo usuario elevado y `-Scope Machine`.
- `05 ... -VerifyOnly`: comprobación de entorno/cuenta/paquete/SQL **sin cambios ni emisión**.
- Configuración nueva: exactos únicos activos, catálogo deduplicado por ID; pendientes a CSV único; Products y AlegraItemLinks se guardan en una transacción y un conflicto revierte todo. Permite emitir ventas con productos aprobados; las relaciones/cargos ausentes bloquean esa venta.
- Primera activación fija AutoPrintFromUtc; no reimprime histórico. Desactivaciones posteriores del usuario se conservan.
- TXT: `Credenciales-Alegra/Token-Nuevo-Bearer.txt` y `Token-Anterior-Basic.txt`, originales conservados en D:/Setup.
- `Build-AlegraPdfTools.ps1`: requiere **Python 3.12 de 64 bits solo en desarrollo**, dependencias fijadas en requirements, huella fuente/requisitos/script, `-Rebuild`, ZIP Sumatra con SHA-256 y renderer `--self-test`.
- `Test-AlegraPackage.ps1`: comprueba archivos/dependencias y ejecuta self-test. Lo llaman instalador, run-local y paso 05.
- La captura de INFO stderr de PyInstaller/Flutter no debe abortar Windows PowerShell si el proceso sale con 0. Está corregido; se validan los códigos de salida.
- `Preparar-USB.ps1` recompila normalmente instalador y APK, valida TXT antes, restaura PATH y verifica manifiesto (incluido LEEME). Última actualización del kit tras corrección de pagos: instalador recompilado y reutilización del APK que acababa de compilarse en la auditoría completa.

Instalador y copia del kit coinciden:

`5E795CD46E0960289D90CD3F6383B6D3422CF335FB3BFE600EFE76B9AC80FE6F`

## Validaciones y comandos útiles

- .NET Debug/Release y compilación Inno Setup correctas; APK release compilado. Flutter avisó de fuentes Cupertino/paquetes nuevos, sin fallo.
- **26** comprobaciones de contratos, **8** pruebas sintéticas renderer, **73** comprobaciones despliegue/kit.
- Base temporal: 07 dos veces, alta exacta/parcial con impresión, tabla/JSON coherentes, VerifyOnly sin cambios y rollback ante conflicto. Base eliminada; API solo lectura en esos tests.
- Paso 05 embebido del kit verificado contra copia local con VerifyOnly. Las pruebas no guardaron credenciales nuevas en Windows.
- Formulario compilado comprobado con Colombia/Caldas/Manizales.
- FE4932 valida nueva venta + aceptación + PDF + guardado automático. Salida física en una térmica aún no probada.

```powershell
cd D:\Repositorios\System-RestaurantCasaGrande
.\scripts\Watch-Alegra.ps1 -Once
.\scripts\Test-AlegraContracts.ps1
python .\scripts\Test-AlegraFiscalRenderer.py
.\scripts\Test-AlegraDeployment.ps1 -KitPath 'D:\Setup\CasaGrande-USB-validada-20261106\Casa Grande Installers'
# Cerrar app y diálogo de recuperación antes:
.\SISMistico\run-local.ps1 -NoRun
```

Ojo: Test-AlegraContracts carga **CapaNegocio/bin/Debug**, mientras run-local compila **bin/LocalBuild**. Compilar Debug antes de esa prueba si cambió el negocio; no diagnosticar una regresión usando una DLL Debug vieja. Los scripts manejan reference assemblies net48 de NuGet; MSBuild directo sin esa ruta puede fallar por Developer Pack ausente.

## Pendientes reales para el siguiente agente

1. Resolver con el usuario el diálogo abierto de FE4931 y comprobar PrintStatus. No tocar aceptación ni volver a emitir.
2. Probar salida física 80 mm en el restaurante y desplegar kit; no se ha hecho todavía.
3. Revisión contable de FAZ09/FAJ44b, diseño/leyendas y contenedor DIAN para entrega electrónica. El renderer conserva documentos reales; no certifica cumplimiento integral ni corrige la factura emitida.
4. Continuar revisión humana de productos no exactos y cargos propina/domicilio/desechables/pagos mixtos cuando se usen. Los 51 exactos iniciales fueron aprobados en la sesión anterior.
5. **Revisar captura fiscal de clientes nuevos:** se observaron `kindOfPerson` y `regime` vacíos en snapshots de los contactos existentes. Save usa SelectedValue en combos poblados con Items.AddRange. Es un hallazgo pendiente de confirmar/corregir; la emisión probada reutilizó un contacto remoto y no probó alta de receptor nuevo. No asumir que esa alta funciona por el éxito de FE4932.
6. Si se sigue cambiando código, regenerar instalador/kit y sus hashes. No editar a mano manifiestos ni copiar DLL aisladas sobre producción.

Archivos clave y decisiones completas: [cierre](docs/15-cierre-de-sesion-2026-11-06.md), [flujo PDF](docs/14-doble-impresion-electronica.md), [guía instalación](docs/08-instalacion-desde-cero.md).
