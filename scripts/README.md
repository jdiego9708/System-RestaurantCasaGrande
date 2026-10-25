# `scripts/` — pruebas y verificaciones de la API móvil

| Script | Qué hace |
|---|---|
| `Test-ApiFase1.ps1` | Arranca el `SISMistico.exe` real de la compilación local (contra `CasaGrandeBD_prod`, API en localhost), ejecuta 32 comprobaciones de `/ping` y `/auth/*` y lo cierra. Requiere `..\SISMistico\run-local.ps1 -Rebuild -NoRun` y la copia restaurada (`..\db\Restore-ProdCopy.ps1`) |
| `Test-ApiFase2.ps1` | Igual, para la fase 2 (65 comprobaciones de catálogo, mesas e imágenes). Cada respuesta se contrasta con una consulta directa a la BD. Usa una carpeta de imágenes temporal y cambia dos platos de la copia; al terminar restaura la BD y la config y lo verifica |
| `Test-ApiFase3.ps1` | Fase 3 (78 comprobaciones de `POST /pedidos`): crea pedidos **de verdad** en la copia, incluida concurrencia (dos peticiones simultáneas), y los contrasta con la BD. Al terminar borra todo lo creado y restaura el ingrediente que inactiva; verifica que los conteos vuelven a los iniciales. No prueba la impresión (necesita impresora) |
| `Compare-PublishedBuild.ps1` | Compara el IL de los ensamblados publicados por ClickOnce (`publish\Application Files\SISMistico_*`) con la compilación local, para saber si lo instalado equivale al código |

Las credenciales de prueba se leen de la copia local y no se imprimen ni se escriben en el log.
