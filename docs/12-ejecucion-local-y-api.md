# Ejecucion local y conexion de los meseros

`SISMistico/run-local.ps1` conserva la configuracion de la API entre compilaciones
en `SISMistico/run-local.api.config`. Este archivo es privado, queda fuera de `bin`
y esta excluido de Git. No se distribuye con el instalador ni con la USB.

En el primer uso importa la clave del ejecutable local existente y migra el
prefijo localhost del script anterior a `http://+:8099/api/v1/`. Si no existe una
clave propia, pide definirla en el archivo privado antes de habilitar la API.
La clave no se imprime en consola.

Para compilar y abrir, cerrar primero el escritorio y ejecutar:

```powershell
.\SISMistico\run-local.ps1
```

`-Rebuild` recompila todo; `-NoRun` solo compila. Ambos conservan prefijo y clave,
incluso si se borra `bin`. `-NoApi` desactiva la API en esa ejecucion sin perder
los valores guardados. La siguiente ejecucion normal la vuelve a activar.

Para cambiar la clave, editar **ApiKey en run-local.api.config**, cerrar el
programa y ejecutar el script. Actualizar tambien la clave en todos los moviles.
No editar el `.exe.config` generado: la siguiente compilacion lo regenera.

Para cambiar el prefijo y recordarlo:

```powershell
.\SISMistico\run-local.ps1 -ApiPrefix 'http://+:8099/api/v1/'
```

Tambien se puede editar ApiPrefijo en el archivo privado. Para pruebas limitadas
al PC, usar `http://localhost:8099/api/v1/`. El script no cambia automaticamente
el firewall ni las reservas de URL. Para acceso desde la red, usar la preparacion
de red existente (`scripts/Configurar-Red.ps1`), en el puerto seleccionado.

Verificacion aislada: `scripts/Test-LocalApiConfig.ps1` simula dos recompilaciones,
limpieza de salida, desactivacion y cambio explicito del prefijo.
