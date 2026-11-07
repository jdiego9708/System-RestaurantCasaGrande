# Cierre de sesión — 6 de noviembre de 2026

Registro de la sesión de integración Alegra, interfaz de facturación y revisión del despliegue. La decisión final es **descargar el PDF oficial y usar el renderizador propio para imprimir a 80 mm**. Los cambios están en el área de trabajo; no se hizo commit, push ni instalación en el restaurante.

## Resultado funcional

### Datos para factura electrónica

- Se corrigió `KeyNotFoundException` al marcar la casilla: las claves de dígito de verificación y correo tenían tildes reemplazadas por `?`. Se corrigieron también las comparaciones de obligatoriedad, etiquetas, actualización al cambiar persona/documento y conservación del texto de las etiquetas.
- La consulta de clientes dejó de usar un ComboBox. Ahora muestra nombre y botón Seleccionar por fila. La carga fiscal sigue requiriendo una selección explícita.
- El grid usa el tema moderno: cabecera clara, filas amplias, selección suave, esquinas redondeadas y botones dibujados con estados de interacción y cliente seleccionado. Se conserva la navegación del grid.
- El aviso «Consulta correcta: … Confirme los datos fiscales antes de continuar» está inmediatamente encima de los resultados. El cliente seleccionado se indica junto a la lista.
- Los selectores de persona, documento y régimen usan un ComboBox personalizado: opciones dibujadas, bordes redondeados, flecha propia, foco/hover y lista con ancho suficiente. Conservan el comportamiento nativo de selección y teclado.
- Valores iniciales: **País Colombia, Departamento Caldas, Municipio/Ciudad Manizales**. Siguen siendo editables. Al elegir un contacto existente se cargan sus datos remotos.

Archivo principal: `SISMistico/CapaPresentacion/Formularios/FormsVentas/FrmElectronicInvoiceCustomer.cs`.

### Historial de ventas y persistencia

Se agregaron tres columnas a Observar ventas: Factura electrónica (Sí/No), Número y Estado electrónico. El estado diferencia solicitud pendiente, creada pendiente de emisión, en emisión, emitida, rechazada y requiere revisión. Solo una solicitud aceptada se muestra como Emitida.

`DVenta.BuscarVenta` incorpora la información de `AlegraInvoiceRequests` por lotes de hasta 500 ventas; no consulta una vez por fila ni modifica el procedimiento SQL del historial. Se conserva el orden de las columnas existentes y se integran las nuevas en el estilo/filtros del grid.

La relación persistida es `Ventas.Id_venta → AlegraInvoiceRequests.SaleId`. La venta y solicitud se guardan en la misma transacción. La tabla electrónica conserva RemoteId, InvoiceNumber, Status, EmissionStatus, RequestJson, LastError, PdfPath y PrintStatus. La existencia de una solicitud no acredita aceptación. La unicidad de SaleId evita dos solicitudes locales por la misma venta.

### Diagnóstico y recuperación de FE4930

- Venta local **14327726**, pedido **14552048**, factura remota **31954 / FE4930**, total **$116.800**.
- Se confirmó que Windows ya tenía el token nuevo correcto y esquema Bearer. Permitía consultas y creación, pero `POST /invoices/stamp` respondía **403 Forbidden**.
- El usuario indicó usar el archivo viejo para Basic. Con ese token, el mismo endpoint y el mismo ID respondieron **200**, `success=true`, `STAMPED_AND_ACCEPTED_WITH_OBSERVATIONS`.
- Una consulta posterior confirmó estado fiscal y CUFE. Se concilió la solicitud local a ACCEPTED conservando ID y número. **No se creó otra venta ni otra factura para resolver el problema.**
- Las observaciones devueltas fueron FAZ09 (identificación del bien/servicio) y FAJ44b (documento/nombre frente al RUT). Se conservan en la representación; cambiar el PDF no corrige el documento emitido.
- COBRADA/closed significa pago; POR EMITIR es independiente. El flujo no depende del visor.

Las credenciales quedan separadas por operación: `ALEGRA_EMAIL/API_KEY/AUTH_SCHEME` para el token nuevo Bearer y `ALEGRA_EMISSION_EMAIL/API_KEY/AUTH_SCHEME` para Basic anterior. `FromEmissionEnvironment()` permite compatibilidad con una sola credencial si no existe la específica. Antes de emitir o renderizar se comprueba la empresa. El rechazo 401/403 se identifica expresamente sin borrar el RemoteId ni recrear la venta.

Los tokens **no se escriben en esta documentación, SQL ni código**. Por instrucción del usuario, el kit privado sí copia los dos TXT originales con nombres descriptivos.

### PDF oficial y representación térmica

Se descargó el PDF oficial de FE4930 con Basic: plantilla Clásico, tamaño **Carta (215,9 × 279,4 mm)**. La decisión final fue conservar dos rutas:

| Acción | Archivo utilizado |
|---|---|
| Descargar PDF desde el detalle de venta | PDF oficial de Alegra, sin modificar |
| Imprimir factura de Alegra desde esa venta | Representación propia de 80 mm |
| Impresión automática tras terminar | Comprobante local y, tras aceptación, representación electrónica de 80 mm |

El renderer usa XML UBL original y detalle REST. Comprueba número, CUFE, total, QR y aceptación. El PDF oficial debe corresponder al mismo CUFE/número y aporta fecha de validación y datos de software/proveedor. Se conserva el XML original y un informe de verificaciones, sin presentar el PDF propio como una descarga oficial.

`PdfPath` sigue apuntando al **oficial**. El térmico es `{SaleId}-80mm.pdf`, con XML, JSON e informe separados. Todo queda en `%LOCALAPPDATA%/CasaGrande/Alegra/PDF`, fuera del repositorio y del instalador. La representación conserva las observaciones reales y las responsabilidades fiscales disponibles. No se certifica cumplimiento tributario integral ni se sustituye el contenedor de entrega electrónica; queda la revisión contable del diseño/leyendas.

La impresora vacía en configuración usa la predeterminada de Windows, como la impresión local. Para Microsoft Print to PDF/Adobe PDF se abre un diálogo Windows de guardado en un hilo STA y se copia intacto el térmico, conservando los 80 mm. Para impresoras físicas se usa SumatraPDF sin reducir el documento.

La generación/impresión se ejecuta en segundo plano. `AutoPrintFromUtc` evita reimprimir ventas anteriores a la activación. El reintento manual conserva el ID de factura. UNKNOWN/CANCELLED no se reimprimen automáticamente. Los fallos de descarga/render/preparación se registran y aplazan para no monopolizar la cola. PRINTED acredita guardado o finalización del envío al lector, no salida física comprobada.

Detalle operativo: [doble impresión electrónica](14-doble-impresion-electronica.md).

## Auditoría de scripts y kit USB

### Archivos de la USB

En el PC se ejecutan **01, 02, 03 y 05**; el **04** es para Android.

| Archivo | Responsabilidad |
|---|---|
| `01-Base-de-Datos.ps1` | Respaldo y migraciones, incluida 07 Alegra; no necesita tokens ni red Alegra |
| `02-Setup-Casa-Grande.exe` | Aplicación Windows de 64 bits, herramientas PDF y configuración opcional de las dos credenciales |
| `03-Configurar-Red.ps1` | Red y API móvil |
| `04-APK-Casa-Grande.apk` | App Android recompilada |
| `05-Configurar-Alegra.ps1` | Importar TXT, comprobar cuenta/herramientas y configurar SQL sin emitir |
| `Credenciales-Alegra/Token-Nuevo-Bearer.txt` | Copia del TXT nuevo de D:/Setup |
| `Credenciales-Alegra/Token-Anterior-Basic.txt` | Copia del TXT viejo de D:/Setup |
| `Soporte`, `00-LEEME.txt`, `SHA256SUMS.txt` | Auxiliares, instrucciones y comprobación de integridad |

### Correcciones de la auditoría

1. **Configuración separada de la migración.** El paso 01 ya no se detiene por credenciales ausentes antes de su verificación final. El paso 05 es autocontenido: embebe configurador SQL y validador de herramientas, sin secretos embebidos.
2. **Importación de credenciales.** El paso 05 lee ambos TXT sin mostrarlos, solicita correo si falta y comprueba por GET que correspondan a la misma empresa. Guarda por usuario; detecta variables Machine diferentes que desplazarían las de usuario. En ese caso requiere ejecutar elevado con el mismo usuario y `-Scope Machine`, sin guardar parcialmente el plan por ese conflicto.
3. **Verificación sin cambios.** `05 ... -VerifyOnly` comprueba archivos, entorno activo, herramientas, cuenta y configuración SQL sin guardar credenciales ni modificar SQL. No usa creación ni emisión como prueba.
4. **Catálogo nuevo.** El configurador descarta productos remotos inactivos y deduplica por ID. El perfil Casa Grande vincula solo exactos únicos; exporta pendientes. Permite activar con productos aprobados, manteniendo el bloqueo por venta si algún producto/cargo carece de relación. Los IDs conocidos se comprueban en los catálogos reales antes de elegirlos.
5. **Impresión.** Se corrigieron mensajes antiguos que afirmaban que estaba deshabilitada. La vinculación exacta conserva la elección de impresión. La primera configuración establece fecha de activación; una desactivación posterior del usuario se conserva.
6. **Herramientas completas.** El build empaqueta el renderer con Python/reportlab/PyMuPDF/Pillow y SumatraPDF portable. El PC destino no necesita instalar Python. `Test-AlegraPackage.ps1` comprueba archivos requeridos y ejecuta `--self-test`, que genera/lee un PDF sintético de 80 mm con QR.
7. **Recompilación.** Se usa una huella de fuente, requisitos y script para invalidar el renderer en caché; `-Rebuild` permite reconstruirlo. Se verifica SHA-256 del ZIP de SumatraPDF. El instalador y ejecución local verifican las herramientas antes de continuar. Se corrigió el manejo de INFO por stderr de PyInstaller bajo Windows PowerShell al capturar logs.
8. **Kit.** Verifica los TXT antes de compilar, conserva PATH tras Flutter, comprueba los hashes de copias e incluye LEEME en el manifiesto. Se validan rutas y enlaces antes de limpiar las carpetas administradas. Los tokens quedan solo en la carpeta de credenciales prevista.
9. **Arranque.** Abrir el programa al finalizar el instalador queda desmarcado por defecto. Debe mantenerse cerrado hasta completar el 05 para no procesar pendientes con configuración incompleta.

No se necesita una migración SQL adicional a **07**. El número 05 del configurador USB es independiente del SQL 05 de empleados.

## Validaciones de la sesión

- Compilaciones .NET Debug y Release, y compilación real del instalador Inno Setup.
- Apertura/render del formulario y comprobación del aviso inmediatamente anterior al grid, con datos sintéticos y sin red.
- Historial: FE4930 emitida, ventas sin solicitud, tres columnas visibles y 502 filas para ejercitar lotes; SQL solo lectura.
- 26 comprobaciones de contratos sin red ni credenciales reales.
- 8 pruebas sintéticas del renderer: salida de 80 mm y rechazo de número/CUFE/total/QR discrepantes, estado no aceptado, otro PDF oficial y metadatos insuficientes.
- Integración FE4930 desde DLL y renderer portable: oficial Carta y térmico de una página, sin texto fuera del área. Descarga/render no cambian aceptación ni PrintStatus ni vuelven a emitir.
- Prueba del diálogo Windows real con UI Automation limitada al proceso de prueba: guardó una copia byte a byte idéntica y canceló correctamente otro intento. No se envió un trabajo a una impresora física.
- Paso 05 en modo VerifyOnly contra la copia local: credenciales activas correctas, misma empresa, herramientas operativas y configuración SQL sin cambios.
- Pruebas de despliegue: precedencia Machine/User con valores ficticios, identidad de empresa, sintaxis y contenido de auxiliares embebidos; comprobación del kit y manifiesto al finalizar la auditoría.

Auditoria final: kit completo generado en `D:/Setup/CasaGrande-USB-validada-20261106/Casa Grande Installers`. La ejecucion normal recompilo instalador y APK release; despues de la correccion de importes se recompilo el instalador y se actualizo el kit conservando ese APK recien construido. El manifiesto y 73 comprobaciones de despliegue pasaron. El paso 05 embebido se ejecuto en VerifyOnly contra la copia local sin cambios. Se probo en una base temporal la migracion 07 dos veces, alta de configuracion con un exacto y un producto sin vinculo, sincronizacion JSON/AlegraItemLinks, VerifyOnly sin cambios y rollback transaccional ante conflicto. La base temporal se elimino. Las consultas a Alegra fueron de lectura; no crearon facturas durante la auditoria.

La configuracion nueva guarda Products y AlegraItemLinks en la misma transaccion. El CSV de pendientes usa nombre unico para no sobrescribir revisiones previas. Flutter release termino correctamente; informo un aviso de fuentes Cupertino y paquetes con versiones mas nuevas, sin fallo de compilacion.

## Incidencia posterior: descuento y coma decimal (FE4931)

El usuario creo la venta 14327728, pedido 14552050, total 43800 y descuento de 25 %. Quedo en NEEDS_REVIEW, sin RemoteId, con error de suma de pagos. El pago SQL era el correcto (43800), pero el snapshot version 1 guardo el texto `43800,00`. Convertirlo con cultura invariante y separadores de miles permitidos cambiaba su valor, por lo que la validacion bloqueo la creacion antes de contactar el endpoint de facturas. La primera prueba habia guardado un entero sin coma y no encontro ese error.

Correcciones:

- `DVenta.FreezeElectronicRequest` reconstruye los importes y pagos de la solicitud desde las filas SQL recien guardadas, dentro de la misma transaccion. La version 2 contiene numeros JSON y conserva el redondeo real de Detalle_venta. Si la suma SQL difiere del total, se revierte antes de terminar la venta.
- El builder admite decimales locales antiguos sin interpretar la coma como miles y rechaza formatos ambiguos con separadores mezclados. Se agregaron dos pruebas a los contratos.
- Se normalizan RemoteId vacios como NULL y los caminos de recuperacion/reintento consideran ambos casos; evita tratar como conocido un ID remoto vacio.

Se consultaron por API las 23 facturas del dia antes de reanudar: ninguna contenia el identificador unico de la solicitud. Se respaldo el snapshot anterior en `%LOCALAPPDATA%/CasaGrande/Alegra/Recovery` y se reparo exclusivamente la solicitud no enviada, usando pagos/venta SQL sin cambiar esos registros. El trabajador actualizado creo y emitio **FE4931 / RemoteId 31955**, aceptada con observaciones, total y pago 43800, saldo 0 y CUFE confirmado. No se creo otra venta local ni se duplico una factura remota. Los PDF oficial y termico se generaron; el dialogo de guardado seguia abierto en el proceso de recuperacion al ultimo seguimiento.

## Prueba nueva del usuario: FE4932

La venta 14327730 se proceso desde el escritorio actualizado y guardo snapshot version 2. API y SQL confirmaron **FE4932 / RemoteId 31956**, total y pagado 172200, saldo 0, CUFE y STAMPED_AND_ACCEPTED_WITH_OBSERVATIONS. Se descargaron el oficial y el modelo de 80 mm. El ultimo seguimiento local mostro PrintStatus PRINTED, por lo que termino el guardado/envio de esta nueva factura. Esta prueba confirma el circuito automatico desde una nueva venta hasta el PDF; la salida fisica en una termica sigue sin comprobarse.

## Pendientes y continuidad

No se instaló en el PC del restaurante ni se copió a una USB física. FE4932 valida la venta nueva automatica y el PDF. Falta la salida en la termica real. La emisión de FE4930 se recuperó por API y se concilió; esa prueba no demuestra por sí sola toda la experiencia automática de una venta nueva.

Continúa la revisión humana de los productos no exactos y la configuración de propina, domicilio, desechables y pagos combinados cuando se utilicen. Se deben revisar las observaciones fiscales y el diseño con el contador. No reenviar una venta para resolver un problema de PDF o impresión.

Hay cambios anteriores y cambios simultáneos del usuario en ambos repositorios. Se conservaron. No usar reset/clean ni atribuir a esta sesión todas las modificaciones que muestra git status.


## Punto de entrada para otro agente

Leer primero [CONTINUAR-AQUI](../CONTINUAR-AQUI.md): contiene tabla final de las tres facturas, procesos observados, dialogo de FE4931 pendiente, credenciales por operacion, rutas del kit, hash del instalador y comandos para retomar sin duplicar emisiones. El ultimo estado comprobado a las 16:45 del 6 de noviembre es FE4930 PRINTED, FE4931 PRINTING y FE4932 PRINTED, todas ACCEPTED con observaciones.

Hallazgo adicional pendiente, sin cambios de codigo en este cierre: los snapshots de contactos existentes mostraban kindOfPerson y regime vacios; Save usa SelectedValue en combos cargados con Items.AddRange. Debe confirmarse/corregirse y probarse el alta de un receptor nuevo. El exito con contactos remotos existentes no prueba esa ruta.

El recuperador de FE4931 seguia esperando el dialogo Windows en PID 38228; no ejecutarlo de nuevo ni forzar estado PRINTED por SQL. Los PID pueden cambiar. No hubo nuevas emisiones ni pruebas durante la redaccion del cierre.
