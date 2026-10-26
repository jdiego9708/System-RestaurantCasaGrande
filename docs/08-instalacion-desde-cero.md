# 08 — Instalación desde cero en el restaurante

Guía paso a paso para dejar funcionando **el sistema de escritorio, la API móvil y la app Android** en el PC del
restaurante (Windows) y en los celulares de los meseros. Está escrita para seguirla de arriba abajo, con una casilla
`☐` por cada paso.

> **Estado de verificación (2026-10-26).** El instalador (`Setup.exe`) **compila**, pero todavía no se ha ejecutado en
> un PC real, y el APK de `release` no se ha probado en un teléfono; los scripts de base de datos y red sí se
> probaron contra la copia local de desarrollo. Haga la primera
> instalación con tiempo, **con copia de seguridad hecha** y con una persona que conozca el sistema. Si algo no
> coincide con esta guía, vea la sección [7. Problemas frecuentes](#7-problemas-frecuentes).

## Resumen

| Dónde | Qué se hace | Archivo que se ejecuta |
|---|---|---|
| Máquina de desarrollo | Preparar la carpeta que se lleva al restaurante | `installer\Publicar-Instalador.ps1` |
| PC del restaurante | Requisitos (.NET 4.8, LocalDB) | Windows Update / `SqlLocalDB.msi` |
| PC del restaurante | Respaldar y preparar la base de datos | `db\Invoke-Sql.ps1` + scripts `.sql` |
| PC del restaurante | Instalar el sistema y activar la API | **`SISRestaurante-Setup-1.0.0.exe`** |
| Router | Reservar la IP del PC | (panel del router) |
| Cada celular Android | Instalar la app y conectarla | **`CasaGrandeMesero.apk`** |

Tiempo estimado: 1 a 2 horas la primera vez (más si hay que restaurar la base de datos), y unos 10 minutos por celular.

---

## 1. Qué hay que llevar

Todo cabe en una USB (o carpeta de red). Se prepara en la máquina de desarrollo (sección 2).

```
SISRestaurante\
├─ SISRestaurante-Setup-1.0.0.exe     ← instalador del sistema de escritorio (~10 MB)
├─ SISRestaurante-Setup-1.0.0.exe.sha256
├─ LEEME-INSTALACION.txt
├─ CasaGrandeMesero.apk               ← app del mesero para Android
└─ db\                                ← scripts de base de datos (solo estos, sin respaldos)
   ├─ Invoke-Sql.ps1
   ├─ Restore-ProdCopy.ps1            (solo si hay que restaurar la base en un PC nuevo)
   ├─ verificar-despliegue.sql
   ├─ 01_sp_Movil_Insertar_pedido.sql
   ├─ 03_sp_Movil_Agregar_items.sql
   ├─ 04_sp_Movil_Quitar_item.sql
   └─ 02_unificar_empleados_duplicados.sql
```

Además, **la copia de seguridad de la base de datos del restaurante** (`CasaGrandeBD`) si el PC es nuevo. Esa copia
contiene datos personales y claves reales: llévela en un medio propio y no la suba a ningún sitio.

---

## 2. En la máquina de desarrollo: preparar la carpeta

```powershell
cd D:\Repositorios\System-RestaurantCasaGrande\installer

# 1) Compila el sistema, arma el instalador y lo copia a la USB (cambie E:\ por su unidad)
.\Publicar-Instalador.ps1 -Version 1.0.0 -Destino E:\SISRestaurante

# 2) Scripts de base de datos
New-Item -ItemType Directory -Force E:\SISRestaurante\db | Out-Null
cd ..\db
Copy-Item Invoke-Sql.ps1, Restore-ProdCopy.ps1, verificar-despliegue.sql, 01_sp_Movil_Insertar_pedido.sql, `
          03_sp_Movil_Agregar_items.sql, 04_sp_Movil_Quitar_item.sql, 02_unificar_empleados_duplicados.sql `
          E:\SISRestaurante\db

# 3) La app Android (ver la sección 6.1 si hay que generarla)
Copy-Item D:\Repositorios\CasaGrandeMesero\app\build\app\outputs\flutter-apk\app-release.apk E:\SISRestaurante\CasaGrandeMesero.apk
```

☐ La carpeta `E:\SISRestaurante` tiene el instalador, el `.apk` y `db\`.

---

## 3. En el PC del restaurante

> Use **el mismo usuario de Windows** que va a abrir el sistema todos los días. SQL Server LocalDB crea la instancia
> `MSSQLLocalDB` **por usuario**: si restaura la base con un usuario y abre el sistema con otro, la base "no existe".

Copie `E:\SISRestaurante` al PC, por ejemplo a `C:\SISRestaurante-instalacion\`.

### 3.1 Requisitos del PC

☐ **Windows 10 (1903 o posterior) u 11.** Ya traen .NET Framework 4.8; si está al día con Windows Update no hay que
instalar nada. El instalador comprueba esto y se detiene con un aviso si falta.

☐ **SQL Server Express LocalDB.** Compruébelo en PowerShell:

```powershell
sqllocaldb info
```

Debe aparecer `MSSQLLocalDB`. Si dice que `sqllocaldb` no existe, descargue **SQL Server Express** desde el sitio de
Microsoft, elija la opción de descarga de medios **LocalDB** (`SqlLocalDB.msi`), instálelo y repita el comando.
Si el sistema ya funcionaba en este PC con ClickOnce, LocalDB ya está.

☐ **Impresora de comandas como impresora predeterminada de Windows** (Configuración → Bluetooth y dispositivos →
Impresoras). Las comandas que piden los celulares salen por la predeterminada. Si es *Microsoft Print to PDF*, el
sistema guarda un PDF en `Documentos\Casa Grande\Comandas` en lugar de imprimir.

☐ Si antes estaba la versión **ClickOnce**, desinstálela (Configuración → Aplicaciones → Aplicaciones instaladas →
*SISRestaurante* → Desinstalar) para no tener dos accesos directos. Eso no borra la base de datos.

### 3.2 Base de datos

Abra **PowerShell** (no hace falta administrador) en `C:\SISRestaurante-instalacion\db`. Los scripts se ejecutan así,
sin cambiar la política de PowerShell del equipo:

```powershell
cd C:\SISRestaurante-instalacion\db
powershell -ExecutionPolicy Bypass -File .\Invoke-Sql.ps1 -File .\verificar-despliegue.sql -Database CasaGrandeBD
```

#### Caso A — el PC ya tenía el sistema funcionando (existe `CasaGrandeBD`)

El comando anterior imprime el nombre de la base y el estado de los tres procedimientos (`FALTA` es lo esperado la
primera vez). Siga en 3.2.2.

#### Caso B — PC nuevo, hay que restaurar la base

1. Con el respaldo en formato **script de SQL** (el que genera SSMS con datos, el mismo del que sale la copia de
   desarrollo), use el script incluido. Tarda unos 80 segundos:

   ```powershell
   powershell -ExecutionPolicy Bypass -File .\Restore-ProdCopy.ps1 -ScriptPath "C:\ruta\al\respaldo.sql" -Database CasaGrandeBD
   ```

   No use `-Force` si la base ya existe: ese parámetro **la borra y la recrea**.
2. Si el respaldo es un archivo `.bak`, restáurelo con SSMS (clic derecho en *Bases de datos* → *Restaurar base de
   datos*) conectándose a `(localdb)\MSSQLLocalDB`, con el nombre destino `CasaGrandeBD`. (Ruta no probada aquí.)
3. Repita `verificar-despliegue.sql` para confirmar que la base responde.

#### 3.2.1 Copia de seguridad antes de tocar nada

Cree un archivo `respaldo.sql` en esa carpeta con este contenido (ajuste la ruta; la carpeta debe existir):

```sql
BACKUP DATABASE CasaGrandeBD TO DISK = N'C:\Respaldos\CasaGrandeBD_antes_de_la_API.bak' WITH INIT, CHECKSUM;
GO
```

```powershell
New-Item -ItemType Directory -Force C:\Respaldos | Out-Null
powershell -ExecutionPolicy Bypass -File .\Invoke-Sql.ps1 -File .\respaldo.sql -Database master
```

☐ Existe `C:\Respaldos\CasaGrandeBD_antes_de_la_API.bak` y pesa más de unos pocos MB. **No siga sin este archivo.**
Guarde además una copia en otro lugar (USB o nube privada).

#### 3.2.2 Procedimientos nuevos y arreglo de empleados

Estos scripts **son aditivos** (crean tres procedimientos nuevos y no cambian los existentes); el último modifica
datos de empleados y deja respaldo en la propia base. Ejecútelos en este orden:

```powershell
powershell -ExecutionPolicy Bypass -File .\Invoke-Sql.ps1 -File .\01_sp_Movil_Insertar_pedido.sql -Database CasaGrandeBD
powershell -ExecutionPolicy Bypass -File .\Invoke-Sql.ps1 -File .\03_sp_Movil_Agregar_items.sql   -Database CasaGrandeBD
powershell -ExecutionPolicy Bypass -File .\Invoke-Sql.ps1 -File .\04_sp_Movil_Quitar_item.sql     -Database CasaGrandeBD
powershell -ExecutionPolicy Bypass -File .\Invoke-Sql.ps1 -File .\02_unificar_empleados_duplicados.sql -Database CasaGrandeBD
```

El `02` deja **una sola ficha con PIN por persona** (las duplicadas quedan sin PIN e inactivas, sin borrarse) y
convierte el PIN `0` en "sin PIN". Es necesario porque en el celular el mesero entra **solo con su PIN**. Antes de
ejecutarlo confirme con el dueño que está de acuerdo; hay un caso pendiente de decisión (`SANTIAGO`, dos fichas con
historia y PIN distinto: no se toca). Es repetible sin riesgo.

Compruebe el resultado:

```powershell
powershell -ExecutionPolicy Bypass -File .\Invoke-Sql.ps1 -File .\verificar-despliegue.sql -Database CasaGrandeBD
```

☐ Los tres procedimientos dicen **OK**.
☐ `pines_compartidos_entre_activos` = **0** y `activos_con_pin_cero` = **0**.
☐ `activos_sin_pin` puede ser mayor que 0: esas personas simplemente no podrán entrar desde el celular hasta que se les
asigne un PIN en el sistema de escritorio.

> **Deshacer:** cada script tiene su `.revert.sql` en el repositorio (`DROP PROCEDURE` de los tres SP y, para el `02`,
> restauración desde `Empleados_respaldo_unificacion`). En el peor caso, restaure el `.bak` del paso 3.2.1 (cierre el
> sistema antes): `RESTORE DATABASE CasaGrandeBD FROM DISK = N'C:\Respaldos\…bak' WITH REPLACE`.

### 3.3 Instalar el sistema de escritorio

1. Doble clic en **`SISRestaurante-Setup-1.0.0.exe`**. Si Windows muestra *SmartScreen* ("Windows protegió su PC"),
   pulse **Más información → Ejecutar de todos modos**: el instalador no está firmado digitalmente.
2. Acepte el aviso de administrador (UAC). El instalador lo necesita para reservar la URL de la API y abrir el firewall.
3. Siga el asistente (Siguiente en la carpeta de destino) y en la página de **tareas** deje marcado:
   * ☑ **Crear un acceso directo en el escritorio** (opcional).
   * ☑ **Activar la API móvil y abrir el firewall** — marque esta si los meseros van a usar celular.
4. Aparece la página **API móvil**:
   * **Clave de la API:** el instalador propone una aleatoria de 24 caracteres. Puede dejarla o escribir otra
     (mínimo 12, sin espacios). **Anótela:** es lo que se escribe en cada celular.
   * **Puerto:** `8099` (déjelo así salvo que esté ocupado).
5. Pulse **Instalar**. Al terminar, la última página repite el **puerto** y la **clave**. Deje marcado **Abrir
   SISRestaurante**.

☐ La clave de la API está anotada en un lugar seguro (no en un papel pegado a la pantalla del restaurante).

> El instalador no pregunta por la base de datos: la aplicación usa `conexionLocalBD` →
> `(localdb)\MSSQLLocalDB`, base **`CasaGrandeBD`**. Si en este PC tuviera otro nombre o servidor, cámbielo desde
> *Administración avanzada* dentro del sistema.

### 3.4 Primer arranque y comprobación local

1. Inicie sesión en el sistema con un usuario de escritorio. Compruebe que se ven las **mesas**.
2. En la barra de arriba de la pantalla de Mesas, junto a DISPONIBLE · OCUPADA · SALIENDO, debe aparecer el
   indicador **«0 DISPOSITIVOS»** (gris). Si no aparece, la API no está activa: revise
   `C:\Program Files\SISRestaurante\logs\`.
3. En el navegador del propio PC abra `http://localhost:8099/api/v1/ping`. Debe mostrar un JSON con la versión.

☐ Se ven las mesas, el indicador "0 DISPOSITIVOS" y el `ping` responde.

### 3.5 Red: IP del PC y datos para los celulares

1. Menú Inicio → **SISRestaurante → Datos para conectar celulares**. Se abre una ventana azul que muestra:

   ```
   Servidor / IP : 192.168.1.XX      ← la IP del PC
   Puerto        : 8099
   Clave de API  : ………………………………   ← la que anotó
   ```

   También avisa si la red está como **Pública** (el firewall bloquearía a los celulares) o si la IP es **dinámica**.
2. **Si la red es "Pública":** en Windows, Configuración → Red e Internet → (su WiFi o Ethernet) → Propiedades →
   **Red privada**. O, como administrador, `Herramientas\Configurar-Red.ps1 -PerfilPrivado`.
3. **Fije la IP del PC en el router** (recomendado): entre al panel del router → *DHCP / reserva de direcciones* →
   asocie la **MAC del PC** (la ventana azul la muestra) a su IP actual. Si no puede, el mismo script lo hace en el
   PC: `Herramientas\Configurar-Red.ps1 -FijarIp` (como administrador); la reserva en el router sigue siendo mejor.
4. **Que el PC no se duerma** (si se duerme, la API deja de responder): Configuración → Sistema → Energía → *Suspender*
   → "Nunca" cuando está enchufado.

☐ Anotó **IP**, **puerto** y **clave**: `IP ____________  Puerto 8099  Clave ______________________`

---

## 4. En cada celular Android

Requisitos: **Android 7.0 o superior** y estar en **el mismo WiFi que el PC** del restaurante.

### 4.1 Instalar la app (`CasaGrandeMesero.apk`)

**Opción 1 — copiar el archivo (la más simple)**

1. Pase `CasaGrandeMesero.apk` al celular: cable USB (copiar a la carpeta *Descargas*), Bluetooth, Drive, o por la
   red local.
2. En el celular abra **Mis archivos → Descargas → `CasaGrandeMesero.apk`**.
3. Android pedirá permitir **"Instalar apps desconocidas"** para la aplicación desde la que lo abrió (Mis archivos,
   Chrome…): actívelo y vuelva atrás.
4. Pulse **Instalar**. Si *Play Protect* avisa "app no verificada" o "bloqueada", elija **Instalar de todos modos**
   (el APK no está publicado en Google Play ni firmado con certificado propio).
5. Pulse **Abrir**. Se instala como **Casa Grande Mesero**.

**Opción 2 — por cable con `adb`** (desde el PC; sirve para instalar en varios celulares seguidos)

1. En el celular: Ajustes → Acerca del teléfono → pulse 7 veces *Número de compilación* → Ajustes → *Opciones de
   desarrollador* → **Depuración por USB**, y conecte el cable aceptando la huella RSA.
2. En el PC: `adb install -r CasaGrandeMesero.apk` (se obtiene `adb` con *Android SDK Platform-Tools*).

### 4.2 Conectar la app al servidor (solo la primera vez)

1. Al abrir la app aparece **Configurar servidor**. Escriba los datos de la sección 3.5:
   * **IP o nombre:** `192.168.1.XX`
   * **Puerto:** `8099`
   * **Clave:** la clave de la API
2. Pulse **Probar**. Debe confirmar que el servidor responde. Si no, vaya a [7](#7-problemas-frecuentes).
3. Guarde y continúe: aparece el **teclado de PIN**.

### 4.3 Uso diario del mesero

* Entra solo con su **PIN**. Si un PIN lo comparten dos fichas, la app muestra un selector con la persona.
* **Mesas** → elegir mesa → menú y carrito (los 4 ingredientes del plato son obligatorios) → **Revisar pedido**.
* En **Revisar pedido**: comensales, nota general y **Copias de la comanda** (2 por defecto, de 1 a 5). La comanda
  **siempre se imprime** en la impresora del PC al enviar.
* Si se pierde el WiFi, el pedido queda en la **bandeja** y se reenvía solo al volver la conexión, sin duplicarse.

☐ En el celular se ve el mapa de mesas del restaurante.

### 4.4 Android: ajustes que evitan fallos

* Desactive el cambio automático a datos móviles del WiFi ("Cambio inteligente de red" / "Cambiar a datos móviles
  automáticamente"): con un WiFi sin Internet, Android puede mandar el tráfico por datos y no llegar al PC.
* Desactive **DNS privado** y cualquier **VPN** mientras use la app.
* En ahorro de batería, deje la app sin restricciones si se queda "dormida" y pierde la sesión.

---

## 5. Prueba de punta a punta

Hágala con la primera mesa libre, antes de abrir al público.

1. En el navegador del celular abra `http://IP-DEL-PC:8099/api/v1/ping`. Debe verse un JSON (si no, el problema es de
   red/firewall, no de la app).
2. En la app, entre con un PIN, abra una mesa libre y envíe un pedido pequeño.
3. En el PC deben verse **a la vez**:
   * La mesa pasa a **OCUPADA** en la pantalla de Mesas.
   * El indicador cambia a **«1 DISPOSITIVO»** (azul); pasando el mouse por encima aparecen el nombre del mesero y la IP.
   * Salen **2 copias** de la comanda por la impresora (o se guardan los PDF en `Documentos\Casa Grande\Comandas`).
4. En el celular, abra el pedido en *Detalle* y cancélelo con motivo de prueba (pide PIN) para dejar la mesa libre.
   El pedido de prueba **queda en el historial**: avise a quien haga el cierre.

☐ Mesa ocupada · ☐ indicador de dispositivos · ☐ comanda impresa (2 copias) · ☐ pedido de prueba cancelado.

Criterio de terminado del proyecto: *un teléfono en el WiFi del restaurante crea un pedido y el escritorio lo muestra*.

---

## 6. Anexos

### 6.1 Generar el APK (máquina de desarrollo)

El proyecto necesita **Flutter 3.47.8 (Dart 3.13.7)**. El `flutter` del `PATH` de esa máquina es el 3.11.5 y **no
sirve** (`pub get` falla con "requires SDK version ^3.13.0"), así que primero se antepone el correcto:

```powershell
$env:Path = 'D:\DevTools\flutter-3.47.8\bin;' + $env:Path
cd D:\Repositorios\CasaGrandeMesero\app
flutter build apk --release
# → app\build\app\outputs\flutter-apk\app-release.apk
```

El APK de `release` pesa **62,8 MB** frente a ~174 MB del de depuración (`--debug`) y es el que debe instalarse en los
meseros. Se generó el 2026-10-26, pero **todavía no se ha instalado ni probado en un teléfono**: hasta entonces el
APK probado en el S24 Ultra es el de depuración (`app-debug.apk`); si el de `release` diera problemas, úselo como respaldo. Hoy se firma con la clave de depuración: sirve para instalar directamente en los celulares del restaurante,
pero **no** para Google Play. Un APK firmado con *keystore* propio sigue pendiente (ver
[00 §10](00-estado-del-proyecto.md)). Un APK firmado con otra clave no se instala encima del anterior: hay que
desinstalar primero.

### 6.2 Actualizar

| Qué | Cómo |
|---|---|
| Sistema de escritorio | Ejecute el instalador nuevo encima (cierra la app si está abierta y **conserva** `SISMistico.exe.config`: clave, imágenes, conexión) |
| App Android | Instale el `.apk` nuevo encima (misma firma); la configuración del servidor se conserva |
| Base de datos | Los scripts `01`, `03`, `04` se pueden repetir: si el SP existe lo reemplazan |

### 6.3 Desinstalar

* **Escritorio:** Configuración → Aplicaciones → *SISRestaurante - Casa Grande* → Desinstalar. Quita los archivos, la
  reserva de URL y la regla de firewall. **No** toca la base de datos.
* **Celular:** mantener pulsado el icono → Desinstalar.

### 6.4 Datos de configuración (referencia)

| Dato | Valor |
|---|---|
| Carpeta de instalación | `C:\Program Files\SISRestaurante\` |
| Configuración | `…\SISMistico.exe.config` (claves `ApiHabilitada`, `ApiPrefijo`, `ApiKey`) |
| Registros de la API | `…\logs\api-AAAAMMDD.log` |
| Dirección de la API | `http://IP-DEL-PC:8099/api/v1/` |
| Regla de firewall | `SISRestaurante API` (TCP 8099, redes Privada/Dominio, solo subred local) |
| Comandas en PDF | `Documentos\Casa Grande\Comandas` (si la impresora es *Print to PDF*) |
| Instalación silenciosa | `Setup.exe /VERYSILENT /TASKS="api" /ApiKey=ClaveLarga123 /ApiPort=8099` |

---

## 7. Problemas frecuentes

| Síntoma | Causa probable | Qué hacer |
|---|---|---|
| El instalador dice que falta .NET 4.8 | Windows sin actualizar | Windows Update o el instalador de .NET Framework 4.8 de Microsoft; repetir |
| El sistema abre y dice que no encuentra la base | LocalDB distinto usuario / base sin restaurar | Restaurar la base **con el mismo usuario de Windows** que abre el sistema (sección 3) |
| No aparece "N DISPOSITIVOS" | API apagada o no pudo arrancar | Mire `…\logs\`: si dice *Acceso denegado*, falta la reserva de URL → ejecute `Herramientas\Configurar-Red.ps1` como administrador; si dice *ApiKey sigue con el valor de ejemplo*, repita el instalador marcando la API |
| `ping` funciona en el PC pero no en el celular | Firewall o red "Pública" | Poner la red como **Privada** (3.5) y repetir `Configurar-Red.ps1` |
| `ping` funciona en el navegador del celular pero la app no conecta | IP/puerto/clave mal escritos, o tráfico por datos móviles | Revise los tres datos; aplique 4.4 |
| La app dice "Clave inválida" (401) | `ApiKey` distinta a la del PC | Vuelva a leerla en *Datos para conectar celulares* y reescríbala |
| "PIN incorrecto" para un mesero | No tiene PIN o es de menos de 4 dígitos | Asignarle un PIN en el sistema de escritorio (empleados) |
| Muchos intentos de PIN: "espere" | Límite de 5 intentos por minuto por IP | Esperar un minuto |
| Se pierde la IP del PC y los celulares no conectan | IP dinámica | Reservar la IP en el router (3.5) y corregir la IP en los celulares |
| Los meseros "salen" de la sesión | Se reinició el sistema de escritorio (las sesiones están en memoria) | Volver a entrar con el PIN |
| La comanda no sale | Impresora predeterminada incorrecta o apagada | Elegir la impresora correcta como predeterminada (3.1); un `409` indica que ya hay una impresión en curso |
| El indicador cuenta "1 DISPOSITIVO" con el celular por USB | Con `adb reverse` el celular llega como `127.0.0.1` | Es normal: se cuenta por IP |
| SmartScreen o Play Protect avisan | Instalador y APK sin firma de editor | Ejecutar / instalar de todos modos (3.3 y 4.1) |

---

## 8. Seguridad (lo mínimo)

* La API **no usa TLS** y está pensada solo para la **red local**: no abra el puerto 8099 en el router hacia Internet.
* El firewall solo admite la **subred local** y redes Privadas; el servidor además rechaza IP que no sean privadas.
* La clave de la API y los PIN son la única defensa: use una clave larga, no la comparta por chats públicos y cámbiela
  si se pierde un celular (edite `ApiKey` en `SISMistico.exe.config`, reinicie el sistema y actualice los celulares).
* Cualquier PIN válido permite quitar líneas o cancelar un pedido: es la misma regla del escritorio.
