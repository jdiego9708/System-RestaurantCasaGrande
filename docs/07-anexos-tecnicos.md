# 07 — Anexos técnicos

---

## A. SP transaccional propuesto

> **Aditivo**: crea un objeto nuevo, no modifica nada existente.
> **Revisado el 2026-10-24 contra el esquema de producción** ([02 §2](02-base-de-datos-y-hallazgos.md)). Cambios
> respecto al borrador anterior: `Detalle_pedido` tiene `Id_detalle_pedido`; `Detalle_ingredientes_pedido` debe
> guardarlo para colgar cada ingrediente de su línea; el SP falla si algún producto no existe o está inactivo
> (antes lo descartaba en silencio); `OPENJSON` está disponible (compatibilidad 150).
>
> ✅ **Probado el 2026-10-24 en la copia local** (`CasaGrandeBD_prod`): 50/50 casos, incluido el rollback real
> cuando falla un `INSERT` tras crear cabecera y líneas, y el caso de varias filas del mismo plato con ingredientes
> distintos. Versión vigente: `db/01_sp_Movil_Insertar_pedido.sql` (reversión: `…revert.sql`; pruebas:
> `db/pruebas-sp-movil-insertar-pedido.sql`). La API lo usa desde la fase 3 (`POST /pedidos`, 78/78 pruebas de
> extremo a extremo). **No está desplegado en producción.**

**El código vive solo en [`db/01_sp_Movil_Insertar_pedido.sql`](../db/01_sp_Movil_Insertar_pedido.sql)** (para no
tener dos copias que se desincronicen). Resumen de su diseño:

* Parsea `@Items` (JSON array) **en orden** a una variable de tabla, con el precio del catálogo y el estado del producto.
* Valida antes de abrir la transacción: `50001` sin ítems, `50002` ítems inválidos (cantidad, tipo, inexistente,
  inactivo, sin precio), `50004` JSON que no es un array.
* Dentro de **una** transacción: inserta la cabecera `PENDIENTE` con fecha/hora del servidor y, **fila a fila**, cada
  línea de `Detalle_pedido` con su `Id_detalle_pedido` (`SCOPE_IDENTITY()`), y de esa línea cuelga sus ingredientes.
* Cada elemento de `@Items` es una fila: puede haber varias del mismo producto con ingredientes distintos, como en el
  escritorio. No agrupa.
* Cualquier fallo hace `ROLLBACK`, deja `@Id_pedido = NULL` y relanza el error (probado con un `INSERT` que falla
  tras crear cabecera y líneas).

### A.1 Validaciones que el SP **no** hace (van en C#)

* Existencia de la mesa y que no tenga un pedido `PENDIENTE`/`SALIENDO` **con fecha de hoy**.
* Empleado válido y autorización (y, según D12, que esté activo).
* Idempotencia.
* Que `Tipo_pedido = 'MESA'` (la v1 no admite domicilios).
* El cliente (`Id_cliente`) y las reglas de ingredientes: solo los platos con `Plato_detallado = 'ACTIVO'` los llevan,
  con **un ingrediente activo de cada tipo** (`SOPA`, `ARROZ`, `ACOMPANANTE`, `BEBIDA`). El SP inserta lo que recibe.
  (Los ingredientes no son por plato —ver [02 §5.21](02-base-de-datos-y-hallazgos.md)—, así que no hay relación plato↔ingrediente que validar.)
* No agrupa ítems repetidos: cada elemento es una fila (el error 50003 de la versión anterior ya no existe).

### A.2 Alternativa sin el SP nuevo (opción C de D6)

Abrir `SqlTransaction` en C# y ejecutar los SPs actuales dentro de ella: `sp_Insertar_pedido`, luego
`sp_Actualizar_detalle_pedido` con `@Id_detalle_pedido = 0` por cada línea (devuelve el id nuevo) y los ingredientes
con ese id, **parametrizados** (sin reutilizar `InsertarDetalleIngredientesPedido`, que concatena SQL). Es lo que
hace el escritorio, pero atómico. Ya no hace falta contemplar SQL Server &lt; 2016: producción tiene
compatibilidad 150.

---

## B. Esqueleto de implementación (referencia)

### B.1 `ApiServer.cs`

```csharp
namespace CapaPresentacion.Api
{
    public static class ApiServer
    {
        private static HttpListener _listener;
        private static CancellationTokenSource _cts;
        private static Task _loop;

        public static bool Activo { get; private set; }

        public static void Iniciar()
        {
            if (!ApiConfig.Habilitada || Activo) return;

            _listener = new HttpListener();
            _listener.Prefixes.Add(ApiConfig.Prefijo);   // http://+:8099/api/v1/
            _listener.Start();
            _cts = new CancellationTokenSource();

            _loop = Task.Run(LoopAsync);
            Activo = true;
            ApiLog.Info($"API escuchando en {ApiConfig.Prefijo}");
        }

        public static void Detener()
        {
            if (!Activo) return;
            _cts?.Cancel();
            try { _listener?.Stop(); } catch { /* ignore */ }
            try { _listener?.Close(); } catch { /* ignore */ }
            Activo = false;
            ApiLog.Info("API detenida");
        }

        private static async Task LoopAsync()
        {
            while (_listener.IsListening && !_cts.IsCancellationRequested)
            {
                HttpListenerContext ctx;
                try { ctx = await _listener.GetContextAsync(); }
                catch (HttpListenerException) { break; }
                catch (ObjectDisposedException) { break; }

                _ = Task.Run(() => ApiRouter.Despachar(ctx));
            }
        }
    }
}
```

### B.2 `ApiRouter.cs` (idea)

```csharp
private static readonly (string Metodo, string Plantilla, Func<ApiRequest, ApiResponse> Handler)[] Rutas =
{
    ("GET",  "/ping",              ApiHandlers.Ping),
    ("POST", "/auth/login",        ApiHandlers.Login),
    ("GET",  "/mesas",             ApiHandlers.Mesas),
    ("GET",  "/platos",            ApiHandlers.Platos),
    ("POST", "/pedidos",           ApiHandlers.CrearPedido),
    ("GET",  "/pedidos/{id}",      ApiHandlers.PedidoPorId),
    // ...
};
```

* Normalizar `Path` quitando el prefijo (`/api/v1`).
* Buscar coincidencia exacta o con parámetros `{id}`.
* Envolver el handler en try/catch → `500 ERROR_BD` saneado.
* Validar `X-Api-Key` y token antes de despachar (excepto `/ping` y `/auth/login`).

### B.3 `ApiPedidosService.Crear` (pseudocódigo)

```csharp
public ApiResponse Crear(ApiRequest req)
{
    var dto = ApiJson.Deserializar<CrearPedidoRequest>(req.Body);
    if (!dto.EsValido(out var error)) return ApiResponse.BadRequest(error);

    var sesion = ApiAuth.SesionActual(req);                 // 401 si no hay
    var autorizado = ApiAuth.ResolverAutorizacion(dto.ClaveMaestra) ?? sesion.Empleado; // 403

    if (Idempotencia.Existe(dto.Idempotencia, out var previo))
        return ApiResponse.Ok(previo);                      // 200, mismo pedido

    if (dto.TipoPedido == "MESA")
    {
        if (!ApiMesas.Existe(dto.IdMesa)) return ApiResponse.NotFound("MESA_NO_EXISTE");
        if (ApiMesas.TienePedidoActivo(dto.IdMesa, DateTime.Today))
            return ApiResponse.Conflict("MESA_OCUPADA");
    }
    // ... domicilio, productos activos, agrupación por id_tipo

    _sem.Wait();                                            // SemaphoreSlim(1,1)
    try
    {
        var rpta = NPedidoMovil.InsertarPedido(
            dto, autorizado.Id_empleado, out int idPedido);

        if (rpta != "OK") return ApiResponse.ServerError("ERROR_BD", rpta);

        var pedido = ApiPedidosService.Obtener(idPedido);
        Idempotencia.Guardar(dto.Idempotencia, pedido);

        if (dto.ImprimirComandas) ImprimirEnUi(idPedido);
        RefrescarMesasEnUi(idPedido, dto.IdMesa);

        return ApiResponse.Created(pedido);
    }
    finally { _sem.Release(); }
}
```

---

## C. Comandos de diagnóstico

### C.0 Herramientas del repositorio (preferidas)

Desde 2026-10-24 el diagnóstico está scriptado en [`db/`](../db/README.md); usar esos scripts en vez de comandos
sueltos:

```powershell
.\db\Restore-ProdCopy.ps1 -Force                              # recrea CasaGrandeBD_prod (~80 s)
.\db\Invoke-Sql.ps1 -File .\db\diagnostico-prod.sql           # consultas de contraste
.\db\Invoke-Sql.ps1 -File .\db\pruebas-sp-actualizar-detalle.sql
```

### C.1 Inspección de BD (ad hoc)

PowerShell + ADO.NET (sin necesidad de `sqlcmd`). Apuntar a la **copia de producción**, no a la `CasaGrandeBD` de
desarrollo (cuyo esquema es distinto):

```powershell
Add-Type -AssemblyName "System.Data"
$cs = "Server=(localdb)\MSSQLLocalDB;Database=CasaGrandeBD_prod;Integrated Security=true;"
$c  = New-Object System.Data.SqlClient.SqlConnection $cs
$c.Open()

# tablas y filas
$cmd = $c.CreateCommand()
$cmd.CommandText = "SELECT t.name, SUM(p.rows) FROM sys.tables t
                    JOIN sys.partitions p ON p.object_id = t.object_id AND p.index_id IN (0,1)
                    GROUP BY t.name ORDER BY t.name"
$r = $cmd.ExecuteReader(); while ($r.Read()) { "{0} -> {1}" -f $r[0], $r[1] }; $r.Close()

# columnas
$cmd.CommandText = "SELECT t.name, c.name, ty.name, c.max_length, c.is_nullable
                    FROM sys.columns c
                    JOIN sys.tables t ON t.object_id = c.object_id
                    JOIN sys.types  ty ON ty.user_type_id = c.user_type_id
                    WHERE t.name IN ('Pedidos','Detalle_pedido','Detalle_ingredientes_pedido','Mesas','Clientes')
                    ORDER BY t.name, c.column_id"
$r = $cmd.ExecuteReader(); while ($r.Read()) { "{0}.{1} {2}" -f $r[0],$r[1],$r[2] }; $r.Close()

# firma de un SP
$cmd.CommandText = "SELECT o.name, p.name, TYPE_NAME(p.user_type_id), p.is_output, p.has_default_value
                    FROM sys.parameters p JOIN sys.objects o ON o.object_id = p.object_id
                    WHERE o.name = 'sp_Insertar_pedido' ORDER BY p.parameter_id"
$r = $cmd.ExecuteReader(); while ($r.Read()) { "{0} {1} out={2}" -f $r[0],$r[1],$r[3] }; $r.Close()

$c.Close()
```

### C.2 Prueba del desajuste de la LocalDB de desarrollo (histórica)

> Solo reproduce el fallo de la `CasaGrandeBD` **de desarrollo** (esquema que no es el de producción; ver
> [02 §4](02-base-de-datos-y-hallazgos.md)). Contra producción esa llamada debería aceptarse (las firmas coinciden), pero no se ha ejecutado: insertaría una fila.

```powershell
$cmd = $c.CreateCommand()
$cmd.CommandText = "sp_Insertar_pedido"
$cmd.CommandType = [System.Data.CommandType]::StoredProcedure
$p = $cmd.Parameters.Add("@Id_pedido", [System.Data.SqlDbType]::Int)
$p.Direction = [System.Data.ParameterDirection]::Output
$cmd.Parameters.AddWithValue("@Id_mesa", 1) | Out-Null
$cmd.Parameters.AddWithValue("@Id_empleado", 6120) | Out-Null
$cmd.Parameters.AddWithValue("@Id_cliente", 0) | Out-Null
$cmd.Parameters.AddWithValue("@Fecha_pedido", [DateTime]::Today) | Out-Null
$cmd.Parameters.AddWithValue("@Hora_pedido", [TimeSpan]::FromHours(13)) | Out-Null
$cmd.Parameters.AddWithValue("@Tipo_pedido", "MESA") | Out-Null
$cmd.Parameters.AddWithValue("@Observaciones_pedido", "") | Out-Null
$cmd.Parameters.AddWithValue("@CantidadClientes", 2) | Out-Null
try { $cmd.ExecuteNonQuery() } catch { $_.Exception.Message }
# -> "Procedure or function 'sp_Insertar_pedido' expects parameter '@Id_usuario', which was not supplied."
```

### C.3 Verificaciones de red

```powershell
netstat -ano | findstr :8099
netsh http show urlacl | findstr 8099
Get-NetFirewallRule -DisplayName "*SISRestaurante*" | Format-List
Test-NetConnection -ComputerName 192.168.1.20 -Port 8099
```

### C.4 Compilación local (entorno de desarrollo)

```powershell
cd D:\Repositorios\System-RestaurantCasaGrande\SISMistico
.\run-local.ps1 -Rebuild -NoRun    # valida LocalDB y compila a bin\LocalBuild
.\run-local.ps1                    # compila si hace falta y abre la app
```

> `run-local.ps1` usa los *reference assemblies* de .NET 4.8 vía NuGet porque este equipo no tiene el
> Developer Pack instalado.

---

## D. Glosario

| Término | Significado |
|---|---|
| **Comanda** | Ticket que va a cocina/barra con los ítems del pedido |
| **Precuenta** | Impresión previa del consumo sin cerrar la venta (`SALIENDO`) |
| **Clave maestra** | PIN de 4 dígitos del empleado (`Empleados.Codigo_maestro`); autoriza abrir/quitar ítems |
| **Turno** | Jornada de caja; se crea/cierra en el login (`Turnos`) |
| **SP** | Stored procedure |
| **TVP** | Table-Valued Parameter (parámetro de tabla) |
| **Idempotencia** | Propiedad de que repetir la misma petición no duplique el efecto |
| **LocalDB** | Instancia ligera de SQL Server para desarrollo |
| **`urlacl`** | Reserva de URL de HTTP.sys que permite a un usuario escuchar un prefijo |
| **Allowlist** | Lista blanca de IPs autorizadas |
| **DTO** | Objeto de transferencia de datos (contrato JSON) |

---

## E. Índice de referencias de código

> Rutas relativas a `SISMistico/`. Los formularios y controles están bajo
> `CapaPresentacion/Formularios/` (se abrevian como `FormsPedido/…`, `Controles/…`).

| Tema | Referencia |
|---|---|
| Apertura del pedido desde la mesa | `Controles/CustomButtonMesa.cs:42` |
| Menú de mesa ocupada | `Controles/CustomButtonMesa.cs:62-75` |
| Cambio de estado “SALIENDO” | `Controles/CustomButtonMesa.cs:132` |
| Pantalla de mesas | `FormsPedido/FrmObservarMesas.cs:153`, `:199`, `:275` |
| Clave maestra al abrir | `FormsPedido/FrmPedido.cs:379` (diálogo `FrmComprobacion`) |
| Validaciones del pedido | `FormsPedido/FrmPedido.cs:52` |
| Guardado | `FormsPedido/FrmPedido.cs:235-345` |
| Inserción de cabecera | `FormsPedido/FrmPedido.cs:253` → `CapaDatos/DPedidos.cs:59` |
| Detalle (insert/update/delete) | `FormsPedido/FrmPedido.cs:279` → `CapaDatos/DPedidos.cs:471` |
| Ingredientes | `FormsPedido/FrmPedido.cs:296` → `CapaDatos/DPedidos.cs:918` |
| Impresión de comandas | `FormsPedido/FrmPedido.cs:328-341` |
| Quitar producto (con PIN) | `FormsPedido/FrmPedido.cs:543` |
| Formulario legado (sin guardado) | `FormsPedido/FrmRealizarPedido.cs:187-273`, `:201-206` |
| Tablas del formulario legado | `FormsPedido/TablasPedido.cs:355`, `:408` |
| Domicilios | `FormsPedido/FrmDomicilios.cs:117`, `:133` |
| Login | `CapaDatos/DEmpleados.cs:308` |
| Clave maestra (consulta) | `CapaDatos/DEmpleados.cs:443` |
| Imágenes | `CapaPresentacion/Servicios/Imagenes.cs:17-35` |
| Modelos | `CapaEntidades/Models/{Pedidos,Detalle_pedido,Detalle_ingredientes_pedido,Mesas,Platos,Bebidas,Ingredientes}.cs` |
| Script de ejecución local | `SISMistico/run-local.ps1` |
