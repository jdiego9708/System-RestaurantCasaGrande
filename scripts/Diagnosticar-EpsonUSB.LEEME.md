# Diagnostico de EPSON TM-T20II y error de descriptor USB

Script independiente para Windows 10/11 y Windows PowerShell 5.1. No necesita el proyecto ni instalar modulos. No instala o elimina controladores, no imprime, no reinicia servicios y no modifica el registro, los planes de energia ni los canales de eventos. Solo crea la carpeta del informe y su ZIP.

## Ejecutar en el equipo del restaurante

1. Copiar `Diagnosticar-EpsonUSB.ps1` a una carpeta local.
2. Dejar la Epson encendida y conectada por el mismo cable/puerto que presenta el error.
3. Abrir **Windows PowerShell como administrador** y entrar en esa carpeta.
4. Ejecutar:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File ".\Diagnosticar-EpsonUSB.ps1"
```

`ExecutionPolicy Bypass` solo se aplica al proceso que se abre; no cambia la politica persistente de Windows. El script tambien puede ejecutarse sin elevacion: las consultas restringidas quedaran anotadas.

5. Esperar el mensaje final y enviar el ZIP `Epson-USB-NOMBREPC-FECHA-....zip` que queda en el Escritorio. Si falla la compresion, conservar y enviar la carpeta indicada. `LEER-PRIMERO.txt` es el resumen; los JSON y fragmentos contienen la evidencia detallada.

Opciones:

```powershell
.\Diagnosticar-EpsonUSB.ps1 -HoursBack 168 -OutputDirectory 'C:\Soporte' -NoPause
```

Los eventos se limitan a 1500 registros recientes por canal y SetupAPI a las ultimas 30000 lineas, con un maximo de 120 fragmentos. Las limitaciones y canales inaccesibles quedan documentados; no se activan canales deshabilitados.

## Comparacion para atribuir el USB desconocido

Opcionalmente, despues del primer informe desconectar solo el cable USB de la Epson, esperar unos segundos y ejecutar otra vez el script. Comparar los dos informes ayuda a distinguir esa impresora de otros USB desconocidos. Cada ejecucion crea una carpeta nueva y no sobrescribe la anterior.

Un fallo de descriptor puede ocurrir antes de cargar el controlador Epson. La presencia de una cola de impresion o de APD instalado no prueba que Windows haya identificado fisicamente la impresora. El script no puede decidir por si solo si el origen es el cable, la alimentacion, el puerto, la impresora o el controlador USB del PC.

## Datos y estado de validacion

El informe incluye nombre del PC, identificadores de hardware, posibles numeros de serie, controladores y mensajes de eventos. No recoge documentos de la cola de impresion, claves del sistema ni contenido de recibos. No se sube a ningun servicio automaticamente.

El script se entrego sin ejecutarlo en el equipo del restaurante. No se compilo ni se realizaron pruebas de impresion.

Referencia: [Microsoft: diagnostico de un USB desconocido por fallo de enumeracion](https://learn.microsoft.com/en-us/windows-hardware/drivers/usbcon/case-study--troubleshooting-an-unknown-usb-device-by-using-etw-and-netmon).
