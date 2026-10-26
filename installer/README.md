# `installer/` — instalador real (`Setup.exe`) de SISRestaurante

Reemplaza a ClickOnce. Se construye con **Inno Setup 6** (gratis; ya está en la máquina de desarrollo).
El resultado es un único `SISRestaurante-Setup-<versión>.exe` (~10 MB) que se copia al PC del restaurante y se
ejecuta con doble clic.

## Por qué Inno Setup

| Opción | Resultado | Decisión |
|---|---|---|
| **Inno Setup** | `Setup.exe` con asistente en español, desinstalador, accesos directos y código propio (Pascal) | **Elegida** |
| WiX Toolset | `.msi` (útil para políticas de grupo) | Más complejo y no aporta aquí |
| MSIX | Paquete moderno | Exige certificado de firma; encaja mal con `netsh`, LocalDB y una app que reescribe su `.config` |
| ClickOnce | `.application`, no un `.exe` | Lo que se reemplaza |
| Advanced Installer / InstallShield | Comerciales | Innecesarios |

## Publicar (en la máquina de desarrollo)

```powershell
cd installer
.\Publicar-Instalador.ps1                    # deja installer\Output\SISRestaurante-Setup-1.0.0.exe
.\Publicar-Instalador.ps1 -Destino E:\       # además lo copia a la USB / carpeta de red, con .sha256 y LEEME
.\Publicar-Instalador.ps1 -Version 1.1.0     # el número que se ve en "Aplicaciones instaladas"
.\Publicar-Instalador.ps1 -SinCompilar       # solo reempaqueta lo que ya está en installer\build\app
```

El script compila la solución en **Release** a `SISMistico\bin\InstallerBuild` (nunca usa `LocalBuild`, cuyo
`.exe.config` está ajustado para desarrollo), deja en `installer\build\app` solo lo necesario, **se niega a continuar**
si el `App.config` versionado no trae `conexionLocalBD` y la API apagada con la `ApiKey` de ejemplo, y compila
`SISRestaurante.iss`. Usa los mismos *reference assemblies* de .NET 4.8 que `run-local.ps1`, así que no hace falta el
Developer Pack. `installer\build\` y `installer\Output\` no se versionan.

## Qué hace el instalador

* Pide administrador (la reserva de URL y el firewall lo necesitan) e instala en `Archivos de programa\SISRestaurante`.
* Verifica **.NET Framework 4.8** (si falta, se detiene) y avisa si no encuentra **LocalDB** (continúa).
* Da permiso de escritura a los usuarios en la carpeta de la app: el sistema reescribe su propio
  `SISMistico.exe.config` (ruta de imágenes, conexión, correo, licencias) y escribe `logs\`. Con ClickOnce esto vivía
  en el perfil del usuario.
* Tarea **«Activar la API móvil y abrir el firewall»**: pregunta la clave (propone una aleatoria de 24 caracteres) y el
  puerto (8099), escribe `ApiHabilitada`, `ApiPrefijo` y `ApiKey` en el `.exe.config`, reserva la URL por SID
  (válido en Windows en español) y abre el firewall solo para redes Privadas/Dominio y la subred local.
  Al terminar muestra puerto y clave; la IP sale en **Inicio > SISRestaurante > Datos para conectar celulares**.
* **Actualizar** = ejecutar el instalador nuevo encima: cierra la app si está abierta y **conserva** el
  `.exe.config` del restaurante (clave, imágenes, conexión). Si se vuelve a marcar la API se propone la clave actual.
* **Desinstalar** quita los archivos, la reserva de URL y la regla de firewall.
* Instalación silenciosa: `Setup.exe /VERYSILENT /TASKS="api" /ApiKey=ClaveLarga123 /ApiPort=8099`.

## Pendientes y advertencias

* **No está firmado**: Windows SmartScreen mostrará «Editor desconocido» (Más información > Ejecutar de todos modos).
  Firmarlo exige un certificado de firma de código.
* El instalador **no crea ni restaura la base de datos** (`CasaGrandeBD` en LocalDB) ni mueve `RUTAIMAGES`
  (`D:\ImagenesRestaurante\`).
* Si la versión ClickOnce sigue instalada, conviene desinstalarla para no tener dos accesos directos.
* Como un cambio de `SISMistico.exe.config` ya instalado no se pisa, una clave nueva de `App.config` solo llega
  a equipos que ya tenían instalación si se edita a mano; el código usa valores por defecto para las claves `Api*`.
* El `AppId` de `SISRestaurante.iss` no debe cambiar jamás, o las actualizaciones instalarían una copia aparte.
* La compilación del instalador está verificada; **su ejecución (instalar, actualizar, desinstalar) no se ha probado**
  en un PC real.
