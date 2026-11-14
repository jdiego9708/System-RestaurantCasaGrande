# Diagnostico Bluetooth de PT210_1FF2

Fecha: 2026-11-11. Estado: investigacion pausada a solicitud del usuario para continuar posteriormente.

## Objetivo y alcance

Identificar como RawBT conecta e imprime en la impresora PT-210 sin solicitar un PIN visible, y determinar una via de integracion directa con Casa Grande en Windows. El usuario considero escribir un controlador en Rust; todavia no se ha implementado ninguna integracion ni controlador.

Se realizaron consultas de diagnostico desde PowerShell, llamadas nativas de Windows mediante Python/ctypes y lecturas ADB del telefono autorizado. No se modifico el proyecto para incorporar impresion Bluetooth. No se instalaron controladores, no se cambiaron claves y no se enviaron trabajos de impresion desde el PC. El usuario envio manualmente un Test desde RawBT y confirmo que imprimio.

## Hallazgos confirmados

| Dato | Evidencia / resultado |
|---|---|
| Nombre Bluetooth | `PT210_1FF2` |
| Etiqueta fisica segun el usuario | `wirelessprinter - PT-210`, sin marca visible |
| Direccion Bluetooth | `10:22:33:9D:1F:F2` |
| Clase de dispositivo | `0x040680` (263808), coincidente en Windows y Android |
| Windows | La exploracion Bluetooth nativa detecto el dispositivo; estaba sin autenticar/emparejar y sin conexion en el momento de consultar |
| Telefono | Samsung `SM-S928B`, Android 16, API 36 |
| Aplicacion | RawBT, paquete `ru.a402d.rawbtprinter`, version 7.1.2, versionCode 188 |
| Perfil anunciado en Android | SPP: Serial Port Profile, Bluetooth clasico |
| UUID utilizado por RawBT | `00001101-0000-1000-8000-00805f9b34fb` |
| Canal serie utilizado | RFCOMM **1**, confirmado por `mPort=1` y `channel=1` en los registros de RawBT |
| Estado en Android | La PT210 figura entre los dispositivos vinculados; durante el diagnostico se observo `ACL BR/EDR:Y`, `LE:N` |
| Seguridad observada | Android respondio con una clave de vinculacion ya guardada. Conexion con `keySize=16` y `algorithm=1` reportados por el sistema |
| Chip Bluetooth anunciado | Identificador de compania 1294 decimal, `0x050E`: Yichip Microelectronics (Hangzhou) Co.,Ltd. |
| Ancho configurado en RawBT | **384 puntos**, leido en la pantalla de configuracion de la aplicacion mediante la jerarquia de controles, sin captura de pantalla |
| Impresion real | El usuario confirmo que el Test enviado desde RawBT imprimio |

El identificador Yichip corresponde al fabricante anunciado por el controlador/chip Bluetooth. **No demuestra que Yichip sea la marca comercial o el fabricante final de la impresora.**

La pantalla mostraba opciones de 203 y 304 dpi, pero no se documento cual estaba seleccionada. Los 384 puntos son una configuracion observada en RawBT, no una medicion independiente del cabezal.

## Explicacion del comportamiento sin PIN visible

Al principio se plantearon BLE y RFCOMM sin autenticacion como posibilidades. La evidencia posterior permite precisar el caso observado:

1. El celular ya tiene una vinculacion guardada con esta impresora.
2. RawBT abre un socket Bluetooth clasico contra el UUID SPP.
3. Android reutiliza una clave de vinculacion y la conexion se establece cifrada.
4. Por eso, conectar desde RawBT no requiere volver a introducir un PIN.

**No se comprobo una conexion sin autenticacion ni un mecanismo que omita el PIN.** Que RawBT no presente una pregunta de PIN no significa que Windows pueda vincularse sin credenciales. Tampoco permite recuperar el PIN original a partir de la vinculacion existente.

No se extrajeron, mostraron ni importaron las claves de vinculacion. No se elimino la vinculacion funcional del celular.

## Evidencia de conexion

Las horas siguientes son las que aparecieron en los registros de Android, sin conversion de zona horaria:

```text
20:45:13.179 BluetoothSocket: connect() ... 1F:F2 / 00001101-0000-1000-8000-00805f9b34fb ... ru.a402d.rawbtprinter
20:45:14.949 BluetoothSocket: CONNECTED ... type::1
21:00:27.692 BluetoothSocket: connect() ... UUID SPP ... ru.a402d.rawbtprinter
21:00:28.236 BluetoothSocket: CONNECTED
21:00:52.640 BluetoothSocket: connect() ... UUID SPP ... ru.a402d.rawbtprinter
21:00:53.580 BluetoothSocket: CONNECTED
RawBT: connect(), socket connected. mPort=1
RawBT: close() ... channel=1 ... mSocketState=CONNECTED
```

En un momento intermedio el telefono estaba en Ajustes de Bluetooth de Android. Seleccionar la impresora alli no generaba un nuevo socket de RawBT. Posteriormente se confirmo que RawBT estaba en `EditPrinterActivity` y si produjo las conexiones registradas.

## Diagnostico realizado en Windows

- `Get-PnpDevice`, `Win32_PnPEntity`, `Get-Printer` y lectura de dispositivos Bluetooth registrados.
- La PT210 no figuraba como impresora instalada ni como dispositivo PnP emparejado. La impresora fisica instalada que aparecio fue una Xprinter XP-58 por USB; es otro dispositivo.
- Una consulta WinRT de dispositivos sin emparejar no termino en 20 segundos. No es evidencia de que la PT210 no exista.
- La suscripcion de PowerShell 5.1 a eventos de `DeviceWatcher` fallo porque PowerShell no admite directamente esos eventos WinRT.
- La exploracion nativa `BluetoothFindFirstDevice` / `BluetoothFindNextDevice`, invocada mediante ctypes, si detecto `PT210_1FF2` y su direccion completa.
- `BluetoothGetDeviceInfo` devolvio resultado correcto.
- `BluetoothEnumerateInstalledServices` devolvio cero servicios instalados en Windows. Esto **no equivale a ausencia de servicios remotos anunciados**; Android confirmo el perfil SPP.
- Se consultaron los registros IEEE MA-L, MA-M y MA-S. No hubo una asignacion coincidente para la direccion consultada. No se dedujo una marca a partir del prefijo `10:22:33`.
- No se completo un emparejamiento ni una conexion RFCOMM desde Windows.

## Captura Android y limitaciones

### Estado de HCI confirmado por el usuario al pausar

El usuario confirma que **Registro HCI Bluetooth esta en HABILITADO**. Las opciones que ofrece su Samsung son:

- Inhabilitado.
- Habilitado.
- Filtrado habilitado.

**No volver a asumir que esta deshabilitado por no encontrar el archivo.** En este equipo, las consultas de propiedades devolvieron valores vacios o accesos restringidos; no constituyen una lectura fiable del selector de Samsung.

### Lo que se pudo recuperar

- Eventos de conexion desde `dumpsys bluetooth_manager` y `logcat` de RawBT.
- Resumen `BTSNOOP_LOG_SUMMARY`, decodificado con base64 y zlib conforme al formato btsnooz de Android, version 2.
- Ese resumen contenia comandos HCI (`0x20`) y eventos HCI (`0x10`), pero **ningun paquete ACL de datos** (`0x11` / `0x21`) en las capturas analizadas.
- El resumen permitio observar la reutilizacion de una clave de vinculacion, pero no los bytes de impresion.
- Se obtuvo un informe mediante `adb bugreport` despues del Test. Se reviso el inventario del ZIP buscando registros Bluetooth; no incluia un archivo `btsnoop_hci.log` completo.
- Los registros de Samsung hacen referencia a `/data/log/bt/`. La lectura directa por ADB de ese directorio devolvio `Permission denied`.
- Los caminos publicos habituales `/sdcard/btsnoop_hci.log` y el directorio externo de Bluetooth consultado no contenian el archivo esperado.
- `run-as ru.a402d.rawbtprinter` no esta disponible porque la aplicacion no es depurable. No se intento forzar su acceso ni obtener root.
- La jerarquia de interfaz se leyo con `uiautomator dump`; no se tomaron ni extrajeron capturas de pantalla.

### Lo que NO se ha determinado

- PIN original o PIN valido para emparejar desde Windows. `0000` fue la sugerencia inicial para la familia PT210; **no se verifico en esta unidad**. Tampoco se verifico `1234`.
- Marca comercial de la impresora.
- Secuencia exacta de inicializacion o bytes del Test.
- Perfil de comandos seleccionado en RawBT. ESC/POS es una posibilidad a comprobar, no una conclusion extraida de la captura.
- Resolucion real, pagina de codigos, comandos graficos, estado de bateria, corte de papel u otras funciones.
- Compatibilidad de una futura conexion desde Windows ni diferencias entre sus requisitos y los de Android.

## Archivos de diagnostico locales

Directorio fuera del repositorio:

```text
%TEMP%\CasaGrande-PT210-Diagnostics\
```

Archivos relevantes generados durante la sesion:

- `connection-events.txt`: eventos filtrados de conexion.
- `rawbt-socket-events.txt` y `live-printer-events.txt`: conexiones registradas de RawBT/PT210.
- `latest-btsnooz.bin`: ultima captura resumida observada.
- `after-test-btsnooz.bin`: resumen guardado despues del Test, sin ACL de impresion.
- `rawbt-after-test.txt`: registros filtrados de RawBT.
- `hci-configuration.txt`: referencias a configuracion/registro HCI del informe.
- `monitor-heartbeat.txt`: ultima actividad del observador.
- `watch-printer.py`: observador temporal.
- `pt210-after-test.zip`: informe Android obtenido, aproximadamente 48,5 MB; contiene diagnostico general del telefono, no solo Bluetooth.

Los archivos temporales y el informe no se incorporaron al repositorio. No publicar el ZIP ni las capturas crudas: pueden contener informacion de otros dispositivos y del telefono. Este documento conserva los datos tecnicos pertinentes, sin claves de vinculacion ni informacion de otras aplicaciones.

El observador temporal se **detuvo** al preparar esta documentacion. No hay una vigilancia permanente prometida; hay que reiniciarla si se necesita otra captura.

## Plan para retomar

1. Mantener intacta la vinculacion actual del telefono y la impresora.
2. Partir de lo ya confirmado: Bluetooth clasico, UUID SPP, canal RFCOMM 1, ancho configurado 384 puntos y HCI en Habilitado.
3. Investigar la exportacion del HCI completo especifica de Samsung/Android 16. No repetir un Test hasta tener una via real para obtener los paquetes ACL; el resumen y el bugreport obtenidos no los proporcionaron.
4. Si la captura completa resulta accesible, desconectar otros dispositivos durante la captura y analizar solo el enlace de la PT210. Identificar inicializacion, datos de prueba y protocolo de comandos.
5. Resolver el emparejamiento de Windows por una via compatible con esta unidad; conocer el canal no sustituye la autenticacion.
6. Evaluar un transporte de impresion en espacio de usuario desde el sistema actual C#/.NET Framework 4.8. Rust podria ser un componente auxiliar, pero **no hay evidencia que justifique escribir un controlador completo de Windows** en esta etapa.
7. Definir con el usuario si busca impresion directa desde Windows o tambien desde la app Android antes de implementar cambios.

No se debe afirmar que la integracion ya funciona: solo se verifico la conexion/impresion de RawBT en el celular.

## Fuentes tecnicas consultadas

- [Bluetooth SIG: Assigned Numbers](https://www.bluetooth.com/specifications/assigned-numbers/): identificador de compania `0x050E`, Yichip.
- [Microsoft: busqueda nativa de dispositivos Bluetooth](https://learn.microsoft.com/en-us/windows/win32/api/bluetoothapis/ns-bluetoothapis-bluetooth_device_search_params).
- [Microsoft: descubrimiento de servicios Bluetooth](https://learn.microsoft.com/en-us/windows/win32/bluetooth/bluetooth-and-wsalookupservicebegin-for-service-discovery).
- [Microsoft: niveles de proteccion RFCOMM](https://learn.microsoft.com/en-us/uwp/api/windows.networking.sockets.socketprotectionlevel).
- [Android: APIs de BluetoothDevice](https://developer.android.com/reference/android/bluetooth/BluetoothDevice): existe RFCOMM sin autenticacion, pero no se demostro su uso en esta conexion.
- [Android: informes de diagnostico](https://developer.android.com/studio/debug/bug-report).
- [AOSP: formato y decodificacion btsnooz](https://android.googlesource.com/platform/packages/modules/Bluetooth/+/refs/heads/main/system/tools/scripts/btsnooz.py).
- [Haulmer: instrucciones de la familia PT210](https://help.haulmer.com/hc/impresora-bluetooth/indicaciones-impresora-bluetooth-mtp-ii-pt200-pt210?lang=es): referencia inicial de PIN `0000`, sin validacion para esta unidad.

## Continuacion: registro de pila en Depurar

El usuario activo el registro de pila Bluetooth y selecciono Depurar. ADB confirmo `log.tag.bluetooth=debug` y `persist.log.tag.bluetooth=debug`. Esto aumenta la verbosidad de los mensajes de la pila; no demuestra que se este exportando un BTSnoop completo con datos ACL. No aparecio un nuevo archivo HCI en `/sdcard/log` tras este cambio.

La siguiente via de exportacion a investigar es SysDump de Samsung (`*#9900#`), documentada por Samsung Knox: Run dumpstate/logcat y Copy to sdcard(include CP Ramdump), que exporta un ZIP a `/sdcard/log`. Todavia no se ejecuto esta exportacion ni se confirmo que incluya el HCI completo en esta unidad. No se cambio Auto Blocker, el nivel general de depuracion de SysDump ni la configuracion de vinculacion.

Referencia: https://docs.samsungknox.com/admin/knox-platform-for-enterprise/troubleshoot/get-device-logs/

## Resultado: conexión directa desde .NET, sin RawBT, sin PIN y sin emparejar

Misma fecha, más tarde. **Este resultado deja sin objeto la captura HCI y la búsqueda del PIN**: no hace falta ninguna de las dos.

Una sonda en C# (`scripts/pt210-probe/Pt210Probe.cs`, .NET Framework, sin dependencias) abre un socket Winsock `AF_BTH` (familia 32, protocolo RFCOMM 3) contra `10:22:33:9D:1F:F2`, canal 1. No usa puerto COM, driver de Windows ni biblioteca externa: la clase `BluetoothEndPoint` serializa a mano el `SOCKADDR_BTH` de 30 bytes.

| Prueba | Resultado |
|---|---|
| Conexión RFCOMM canal 1 desde el PC, con la PT210 **sin emparejar** en Windows | Conecta en 226–461 ms. Windows no pidió PIN ni mostró aviso de vinculación |
| `DLE EOT 1..4` (estado en tiempo real de ESC/POS) | La impresora responde `0x12` a las cuatro consultas: habla ESC/POS |
| Envío de 111 bytes: `ESC @`, centrado, negrita, tres líneas de texto ASCII y avance | **Imprimió**, confirmado por el usuario |
| Texto plano con `CR LF` | Imprimió `HOLA CASA GRANDE` |
| Trama `GS v 0` de 384 × 24 puntos, toda en negro, más `ESC d 5` | Imprimió la franja negra a todo el ancho: acepta gráficos de 384 puntos |
| Texto con centrado y negrita; texto cerrando el socket a los 0,2 s; texto tras `DLE EOT 1` | Enviados sin error; el usuario indicó que todo salió desde el principio, sin detallar cada papel |

Correcciones a lo anotado arriba:

- La impresora **sí acepta RFCOMM sin autenticación**. Que Android usara una clave guardada era decisión del teléfono, no una exigencia de la impresora.
- El dialecto de comandos es ESC/POS, confirmado por la respuesta a `DLE EOT`.
- La dirección y el canal se pasan directamente; no hizo falta descubrimiento SDP.

Compilar y ejecutar la sonda:

```text
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /out:Pt210Probe.exe Pt210Probe.cs
Pt210Probe.exe connect     (solo conecta)
Pt210Probe.exe status      (conecta y consulta DLE EOT)
Pt210Probe.exe print       (además imprime un recibo corto)
```

### Integración en el sistema (implementada, sin probar desde la aplicación abierta)

- `BluetoothReceiptPrinter.cs` (conexión RFCOMM y envío de imagen `GS v 0`) y `ControladorImpresion.PrintBluetooth` (mismo diseño de 58 mm de la XP-58, a 384 puntos).
- Claves de `App.config`: `BluetoothPrinterAddress` (vacía = desactivado), `BluetoothPrinterMode` (`Auto` por defecto, `Both`, `Only`) y `BluetoothPrinterChannel`.
- Modo `Auto`: si la impresora **predeterminada de Windows** (XP-58, POS-80 u otra de cable) está lista, imprime por ella; si no está conectada (`WorkOffline` o estado sin conexión en WMI) o falla, imprime por la PT-210. Una impresora de cable desconectada no hace fallar a Windows —el trabajo se queda en cola—, por eso se consulta su estado antes.
- Prueba manual con papel: `scripts/Test-BluetoothReceipt.ps1`.
- No cubierto: la factura electrónica de Alegra (PDF por SumatraPDF) sigue yendo solo por la impresora de Windows.

### Pendiente para integrar

1. Hecho: recibo de prueba confirmado por el usuario.
2. Los gráficos de trama funcionan. Falta el paso real: los recibos actuales se dibujan con GDI (`PrintDocument`) contra un driver de Windows, así que para esta vía hay que dibujarlos en un bitmap de 384 px y enviarlo con `GS v 0`, o reescribirlos como texto ESC/POS. No se ha probado todavía una imagen alta (un recibo completo) ni el control de flujo en envíos grandes.
3. Decidir cómo se elige esta impresora en el sistema (clave de `App.config` con la dirección Bluetooth) y qué documentos van por ella.
4. La impresora solo atiende un enlace a la vez: si RawBT está conectado desde el celular, el PC no podrá conectar, y al revés.
