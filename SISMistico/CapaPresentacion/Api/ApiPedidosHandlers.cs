using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

using CapaNegocio;

namespace CapaPresentacion.Api
{
    /// <summary>POST /pedidos (alta atómica e idempotente) y GET /pedidos/{id}.</summary>
    internal static class ApiPedidosHandlers
    {
        private const int MaxCantidad = 999;
        private static readonly Regex ClaveValida = new Regex("^[A-Za-z0-9_-]{8,64}$", RegexOptions.Compiled);

        // Un ingrediente de cada tipo en los platos detallados (el escritorio guarda siempre estos cuatro,
        // también en los platos de carta: la sopa puede ser "SIN SOPA", pero se elige).
        private static readonly string[] TiposIngrediente = { "SOPA", "ARROZ", "ACOMPANANTE", "BEBIDA" };

        // Serializa el alta: la comprobación de mesa libre + el INSERT + guardar la clave son una sola sección crítica.
        private static readonly SemaphoreSlim Candado = new SemaphoreSlim(1, 1);

        internal sealed class Ing
        {
            public int Id;
            public string Observaciones;
        }

        internal sealed class Item
        {
            public string Tipo;
            public int IdTipo;
            public int Cantidad;
            public string Observaciones;
            public bool TraeIngredientes;
            public List<Ing> Ingredientes = new List<Ing>();
        }

        internal sealed class Solicitud
        {
            public string Clave;
            public int IdMesa;
            public int IdCliente;
            public int CantidadClientes;
            public string TipoPedido;
            public string Observaciones;
            public bool? Imprimir;
            public int CopiasComandas = 1;
            public List<Item> Items = new List<Item>();
        }

        // =========================================================================================
        // POST /pedidos
        // =========================================================================================

        public static ApiResponse Crear(ApiRequest req)
        {
            string cuerpo, error;
            if (!req.LeerCuerpo(out cuerpo, out error))
                return ApiResponse.Error(400, "VALIDACION", error);

            Dictionary<string, object> datos = ApiJson.LeerObjeto(cuerpo, out error);
            if (datos == null)
                return ApiResponse.Error(400, "VALIDACION", error);

            Solicitud s = Leer(datos, out error);
            if (s == null)
                return ApiResponse.Error(400, "VALIDACION", error);

            string hash = Sha256(cuerpo);

            Candado.Wait();
            try
            {
                // 1. Idempotencia: la misma clave devuelve el pedido ya creado.
                string hashPrevio;
                int idPrevio;
                if (ApiIdempotencia.Buscar(s.Clave, out hashPrevio, out idPrevio))
                {
                    Dictionary<string, object> previo = ObtenerPedido(idPrevio);
                    if (previo != null)
                    {
                        if (hashPrevio != hash)
                            return ApiResponse.Error(409, "CONFLICTO",
                                "Esa clave de idempotencia ya se usó con un pedido distinto.");
                        previo["impreso"] = false;
                        ApiResponse repetida = ApiResponse.Ok(previo);
                        repetida.LogExtra = "id_pedido=" + idPrevio + " (repetido)";
                        return repetida;
                    }
                    ApiIdempotencia.Quitar(s.Clave);   // el pedido ya no existe: la clave queda libre
                }

                // 2. Reglas de negocio (404 / 409 / 422), en el orden del contrato.
                ApiResponse falla = Validar(s);
                int numMesa;
                if (falla != null) return falla;

                DataTable mesa = NApiMovil.MesaPorId(s.IdMesa);
                numMesa = Convert.ToInt32(mesa.Rows[0]["Num_mesa"]);

                // 3. Alta atómica.
                int idPedido;
                try
                {
                    idPedido = NApiMovil.InsertarPedidoMovil(s.IdMesa, req.Sesion.Empleado.IdEmpleado, s.IdCliente,
                        "MESA", s.Observaciones, s.CantidadClientes, ArmarItemsSp(s));
                }
                catch (SqlException ex) when (ex.Number >= 50001 && ex.Number <= 50004)
                {
                    // Las reglas del SP ya se validaron arriba; si saltan es una condición de carrera (producto inactivado).
                    ApiLog.Error("El SP rechazó el pedido", ex);
                    return ApiResponse.Error(422, "REGLA_NEGOCIO", "El pedido ya no es válido: " + ex.Message);
                }

                ApiIdempotencia.Guardar(s.Clave, hash, idPedido);

                // 4. Respuesta; el refresco de pantalla y la impresión se encolan en el hilo de UI y no la retrasan.
                Dictionary<string, object> pedido = ObtenerPedido(idPedido);
                bool imprimir = s.Imprimir ?? ApiConfig.ImprimirComandas;
                bool encolada = false;
                ApiImpresion.RefrescarMapaDeMesas(idPedido, numMesa);
                if (imprimir)
                {
                    bool ocupado;
                    encolada = ApiImpresion.ImprimirComandas(idPedido, "TODO", s.CopiasComandas, out ocupado);
                }
                pedido["impreso"] = encolada;

                ApiResponse ok = ApiResponse.Creado(pedido);
                ok.LogExtra = "id_pedido=" + idPedido + " mesa=" + numMesa + " items=" + s.Items.Count;
                return ok;
            }
            finally
            {
                Candado.Release();
            }
        }

        // ---- lectura y validación de la forma del JSON (400) ----

        private static Solicitud Leer(Dictionary<string, object> d, out string error)
        {
            error = null;
            Solicitud s = new Solicitud();

            s.Clave = ApiJson.Texto(d, "idempotencia");
            if (s.Clave == null || !ClaveValida.IsMatch(s.Clave))
            { error = "idempotencia es obligatoria: 8 a 64 caracteres (letras, números, guion y guion bajo)."; return null; }

            if (!ApiJson.TryEntero(d, "id_mesa", out s.IdMesa) || s.IdMesa <= 0)
            { error = "id_mesa es obligatorio y debe ser un entero positivo."; return null; }

            s.IdCliente = 0;
            if (Presente(d, "id_cliente") && (!ApiJson.TryEntero(d, "id_cliente", out s.IdCliente) || s.IdCliente < 0))
            { error = "id_cliente debe ser un entero mayor o igual a 0."; return null; }

            s.CantidadClientes = 0;
            if (Presente(d, "cantidad_clientes") &&
                (!ApiJson.TryEntero(d, "cantidad_clientes", out s.CantidadClientes) || s.CantidadClientes < 0 || s.CantidadClientes > 1000))
            { error = "cantidad_clientes debe ser un entero entre 0 y 1000."; return null; }

            s.TipoPedido = ApiJson.Texto(d, "tipo_pedido");
            if (string.IsNullOrEmpty(s.TipoPedido))
            { error = "tipo_pedido es obligatorio."; return null; }

            s.Observaciones = string.Empty;
            if (Presente(d, "observaciones"))
            {
                s.Observaciones = ApiJson.Texto(d, "observaciones");
                if (s.Observaciones == null || s.Observaciones.Length > 200)
                { error = "observaciones debe ser un texto de máximo 200 caracteres."; return null; }
            }

            if (Presente(d, "imprimir_comandas"))
            {
                if (!(d["imprimir_comandas"] is bool))
                { error = "imprimir_comandas debe ser true o false."; return null; }
                s.Imprimir = (bool)d["imprimir_comandas"];
            }

            if (Presente(d, "copias_comandas") &&
                (!ApiJson.TryEntero(d, "copias_comandas", out s.CopiasComandas) || s.CopiasComandas < 1 || s.CopiasComandas > 5))
            { error = "copias_comandas debe ser un entero entre 1 y 5."; return null; }

            IList<object> items = Lista(d.ContainsKey("items") ? d["items"] : null);
            if (items == null || items.Count == 0)
            { error = "items es obligatorio y no puede estar vacío."; return null; }
            if (items.Count > ApiConfig.MaxItems)
            { error = "items admite como máximo " + ApiConfig.MaxItems + " elementos."; return null; }

            List<Item> listaItems = LeerItems(items, out error);
            if (listaItems == null)
                return null;
            s.Items = listaItems;
            return s;
        }

        /// <summary>
        /// Lee la lista de items (400). La comparten el alta de pedido y el agregar a un pedido existente.
        /// Devuelve null y deja el motivo en <paramref name="error"/> si la forma no es válida.
        /// </summary>
        internal static List<Item> LeerItems(IList<object> items, out string error)
        {
            error = null;
            List<Item> lista = new List<Item>();

            for (int i = 0; i < items.Count; i++)
            {
                Dictionary<string, object> it = items[i] as Dictionary<string, object>;
                string donde = "items[" + i + "]";
                if (it == null)
                { error = donde + " debe ser un objeto."; return null; }

                Item item = new Item { Tipo = ApiJson.Texto(it, "tipo") };
                if (item.Tipo != "PLATO" && item.Tipo != "BEBIDA")
                { error = donde + ".tipo debe ser PLATO o BEBIDA."; return null; }
                if (!ApiJson.TryEntero(it, "id_tipo", out item.IdTipo) || item.IdTipo <= 0)
                { error = donde + ".id_tipo debe ser un entero positivo."; return null; }
                if (!ApiJson.TryEntero(it, "cantidad", out item.Cantidad) || item.Cantidad < 1 || item.Cantidad > MaxCantidad)
                { error = donde + ".cantidad debe ser un entero entre 1 y " + MaxCantidad + "."; return null; }

                item.Observaciones = string.Empty;
                if (Presente(it, "observaciones"))
                {
                    string o = ApiJson.Texto(it, "observaciones");
                    if (o == null)
                    { error = donde + ".observaciones debe ser un texto."; return null; }
                    item.Observaciones = o.Length > 100 ? o.Substring(0, 100) : o;   // contrato: se trunca
                }

                if (Presente(it, "ingredientes"))
                {
                    IList<object> ings = Lista(it["ingredientes"]);
                    if (ings == null)
                    { error = donde + ".ingredientes debe ser una lista."; return null; }
                    item.TraeIngredientes = true;
                    for (int j = 0; j < ings.Count; j++)
                    {
                        Dictionary<string, object> g = ings[j] as Dictionary<string, object>;
                        Ing ing = new Ing { Observaciones = string.Empty };
                        if (g == null || !ApiJson.TryEntero(g, "id_ingrediente", out ing.Id) || ing.Id <= 0)
                        { error = donde + ".ingredientes[" + j + "].id_ingrediente debe ser un entero positivo."; return null; }
                        if (Presente(g, "observaciones"))
                        {
                            string o = ApiJson.Texto(g, "observaciones");
                            if (o == null)
                            { error = donde + ".ingredientes[" + j + "].observaciones debe ser un texto."; return null; }
                            ing.Observaciones = o.Length > 200 ? o.Substring(0, 200) : o;
                        }
                        item.Ingredientes.Add(ing);
                    }
                }
                lista.Add(item);
            }
            return lista;
        }

        // ---- reglas de negocio (404 / 409 / 422) ----

        private static ApiResponse Validar(Solicitud s)
        {
            if (s.TipoPedido != "MESA")
                return ApiResponse.Error(422, "REGLA_NEGOCIO", "La API solo admite pedidos de MESA.");

            DataTable mesa = NApiMovil.MesaPorId(s.IdMesa);
            if (mesa.Rows.Count == 0 || Convert.ToInt32(mesa.Rows[0]["Canonica"]) == 0)
                return ApiResponse.Error(404, "NO_ENCONTRADO", "La mesa " + s.IdMesa + " no existe.");

            int numMesa = Convert.ToInt32(mesa.Rows[0]["Num_mesa"]);
            DataTable abierto = NApiMovil.PedidoAbiertoDeMesa(numMesa, DateTime.Today);
            if (abierto.Rows.Count > 0)
                return ApiResponse.Error(409, "MESA_OCUPADA", string.Format("La mesa {0} ya tiene un pedido {1} (#{2}).",
                    numMesa, Convert.ToString(abierto.Rows[0]["Estado_pedido"]).ToLowerInvariant(), abierto.Rows[0]["Id_pedido"]));

            if (!NApiMovil.ClienteActivo(s.IdCliente))
                return ApiResponse.Error(404, "NO_ENCONTRADO", "El cliente " + s.IdCliente + " no existe o no está activo.");

            return ValidarProductos(s.Items);
        }

        /// <summary>
        /// Productos e ingredientes de una lista de items (404 si no existe, 422 si está inactivo o no cumple
        /// las reglas de ingredientes). La comparten el alta y el agregar a un pedido existente.
        /// </summary>
        internal static ApiResponse ValidarProductos(List<Item> itItems)
        {
            // Productos: existen (404) y están activos (422).
            Dictionary<string, bool> detallado = new Dictionary<string, bool>();
            foreach (bool esPlato in new[] { true, false })
            {
                string tipo = esPlato ? "PLATO" : "BEBIDA";
                List<int> ids = itItems.Where(i => i.Tipo == tipo).Select(i => i.IdTipo).Distinct().ToList();
                if (ids.Count == 0) continue;
                Dictionary<int, DataRow> hallados = NApiMovil.Productos(esPlato, "[" + string.Join(",", ids) + "]")
                    .Rows.Cast<DataRow>().ToDictionary(r => Convert.ToInt32(r["Id"]));
                foreach (int id in ids)
                {
                    DataRow r;
                    if (!hallados.TryGetValue(id, out r))
                        return ApiResponse.Error(404, "NO_ENCONTRADO", (esPlato ? "El plato " : "La bebida ") + id + " no existe.");
                    if (!string.Equals(Convert.ToString(r["Estado"]), "ACTIVO", StringComparison.OrdinalIgnoreCase))
                        return ApiResponse.Error(422, "REGLA_NEGOCIO", (esPlato ? "El plato " : "La bebida ") + id + " está inactivo.");
                    detallado[tipo + id] = string.Equals(Convert.ToString(r["Detallado"]), "ACTIVO", StringComparison.OrdinalIgnoreCase);
                }
            }

            // Ingredientes: existen y están activos; los platos detallados llevan uno de cada tipo; los demás, ninguno.
            List<int> idsIng = itItems.SelectMany(i => i.Ingredientes).Select(g => g.Id).Distinct().ToList();
            Dictionary<int, DataRow> ingredientes = idsIng.Count == 0
                ? new Dictionary<int, DataRow>()
                : NApiMovil.IngredientesPorIds("[" + string.Join(",", idsIng) + "]").Rows.Cast<DataRow>()
                      .ToDictionary(r => Convert.ToInt32(r["Id_ingrediente"]));

            for (int n = 0; n < itItems.Count; n++)
            {
                Item it = itItems[n];
                string donde = "items[" + n + "]";
                bool pideIngredientes = it.Tipo == "PLATO" && detallado[it.Tipo + it.IdTipo];

                if (!pideIngredientes)
                {
                    if (it.Ingredientes.Count > 0)
                        return ApiResponse.Error(422, "REGLA_NEGOCIO", donde + ": " + (it.Tipo == "PLATO" ? "el plato " : "la bebida ") +
                            it.IdTipo + " no admite ingredientes.");
                    continue;
                }

                Dictionary<string, int> porTipo = TiposIngrediente.ToDictionary(t => t, t => 0);
                foreach (Ing g in it.Ingredientes)
                {
                    DataRow r;
                    if (!ingredientes.TryGetValue(g.Id, out r))
                        return ApiResponse.Error(404, "NO_ENCONTRADO", donde + ": el ingrediente " + g.Id + " no existe.");
                    if (!string.Equals(Convert.ToString(r["Estado_ingrediente"]), "ACTIVO", StringComparison.OrdinalIgnoreCase))
                        return ApiResponse.Error(422, "REGLA_NEGOCIO", donde + ": el ingrediente " + g.Id + " está inactivo.");
                    string tipoIng = Convert.ToString(r["Tipo_ingrediente"]);
                    if (!porTipo.ContainsKey(tipoIng))
                        return ApiResponse.Error(422, "REGLA_NEGOCIO", donde + ": el ingrediente " + g.Id + " es de un tipo no admitido (" + tipoIng + ").");
                    porTipo[tipoIng]++;
                }
                foreach (string t in TiposIngrediente)
                {
                    if (porTipo[t] == 0)
                        return ApiResponse.Error(422, "REGLA_NEGOCIO", donde + ": el plato " + it.IdTipo +
                            " necesita un ingrediente de tipo " + t + ".");
                    if (porTipo[t] > 1)
                        return ApiResponse.Error(422, "REGLA_NEGOCIO", donde + ": el plato " + it.IdTipo +
                            " admite un solo ingrediente de tipo " + t + ".");
                }
            }
            return null;
        }

        /// <summary>JSON para el SP: una entrada por fila, en el orden recibido. El precio no viaja: lo toma el SP del catálogo.</summary>
        internal static string ArmarItemsSp(Solicitud s)
        {
            return ArmarItemsSp(s.Items);
        }

        /// <summary>Igual que el anterior, para el agregar a un pedido existente.</summary>
        internal static string ArmarItemsSp(List<Item> items)
        {
            List<object> lista = new List<object>();
            foreach (Item it in items)
            {
                Dictionary<string, object> d = new Dictionary<string, object>
                {
                    { "tipo", it.Tipo }, { "id_tipo", it.IdTipo }, { "cantidad", it.Cantidad }, { "observaciones", it.Observaciones }
                };
                if (it.Ingredientes.Count > 0)
                    d["ingredientes"] = it.Ingredientes
                        .Select(g => (object)new Dictionary<string, object> { { "id_ingrediente", g.Id }, { "observaciones", g.Observaciones } })
                        .ToList();
                lista.Add(d);
            }
            return ApiJson.Serializar(lista);
        }

        // =========================================================================================
        // GET /pedidos/{id}
        // =========================================================================================

        public static ApiResponse Obtener(ApiRequest req)
        {
            int id;
            if (!int.TryParse(req.Parametros["id"], out id) || id <= 0)
                return ApiResponse.Error(400, "VALIDACION", "El id del pedido debe ser un entero positivo.");
            Dictionary<string, object> pedido = ObtenerPedido(id);
            return pedido == null
                ? ApiResponse.Error(404, "NO_ENCONTRADO", "No existe el pedido " + id + ".")
                : ApiResponse.Ok(pedido);
        }

        /// <summary>El pedido con el formato del contrato (04 §6.2). Un mismo producto puede venir en varias filas.</summary>
        internal static Dictionary<string, object> ObtenerPedido(int idPedido)
        {
            DataSet ds = NApiMovil.PedidoCompleto(idPedido);
            if (ds.Tables[0].Rows.Count == 0)
                return null;

            DataRow h = ds.Tables[0].Rows[0];
            DataTable lineas = ds.Tables[1];
            DataTable ings = ds.Tables[2];

            // Ingredientes de pedidos antiguos sin Id_detalle_pedido: se cuelgan de la primera fila del mismo plato.
            Dictionary<int, List<object>> porLinea = new Dictionary<int, List<object>>();
            foreach (DataRow l in lineas.Rows)
                porLinea[Convert.ToInt32(l["Id_detalle_pedido"])] = new List<object>();
            foreach (DataRow g in ings.Rows)
            {
                int linea;
                if (g["Id_detalle_pedido"] != DBNull.Value)
                    linea = Convert.ToInt32(g["Id_detalle_pedido"]);
                else
                {
                    int idTipo = Convert.ToInt32(g["Id_tipo"]);
                    DataRow cand = lineas.Rows.Cast<DataRow>().FirstOrDefault(l =>
                        Convert.ToString(l["Tipo"]) == "PLATO" && Convert.ToInt32(l["Id_tipo"]) == idTipo);
                    if (cand == null) continue;
                    linea = Convert.ToInt32(cand["Id_detalle_pedido"]);
                }
                List<object> destino;
                if (porLinea.TryGetValue(linea, out destino))
                    destino.Add(new Dictionary<string, object>
                    {
                        { "id_ingrediente", Convert.ToInt32(g["Id_ingrediente"]) },
                        { "tipo", ApiJson.Celda(g, "Tipo_ingrediente") },
                        { "nombre", ApiJson.Celda(g, "Nombre_ingrediente") }
                    });
            }

            decimal total = 0;
            List<object> items = new List<object>();
            foreach (DataRow l in lineas.Rows)
            {
                decimal precio = Convert.ToDecimal(l["Precio"]);
                int cantidad = Convert.ToInt32(l["Cantidad"]);
                total += precio * cantidad;
                items.Add(new Dictionary<string, object>
                {
                    { "id_detalle_pedido", Convert.ToInt32(l["Id_detalle_pedido"]) },
                    { "tipo", ApiJson.Celda(l, "Tipo") },
                    { "id_tipo", Convert.ToInt32(l["Id_tipo"]) },
                    { "nombre", ApiJson.Celda(l, "Nombre") },
                    { "precio", ApiJson.Dinero(precio) },
                    { "cantidad", cantidad },
                    { "total", ApiJson.Dinero(precio * cantidad) },
                    { "observaciones", ApiJson.Celda(l, "Observaciones") },
                    { "ingredientes", porLinea[Convert.ToInt32(l["Id_detalle_pedido"])] }
                });
            }

            object hora = h["Hora_pedido"];
            return new Dictionary<string, object>
            {
                { "id_pedido", Convert.ToInt32(h["Id_pedido"]) },
                { "id_mesa", Convert.ToInt32(h["Id_mesa"]) },
                { "num_mesa", h["Num_mesa"] == DBNull.Value ? 0 : Convert.ToInt32(h["Num_mesa"]) },
                { "id_cliente", Convert.ToInt32(h["Id_cliente"]) },
                { "cliente", ApiJson.Celda(h, "Nombre_cliente") },
                { "id_empleado", Convert.ToInt32(h["Id_empleado"]) },
                { "empleado", ApiJson.Celda(h, "Nombre_empleado") },
                { "estado", ApiJson.Celda(h, "Estado_pedido") },
                { "fecha", Convert.ToDateTime(h["Fecha_pedido"]).ToString("yyyy-MM-dd") },
                { "hora", hora is TimeSpan ? ((TimeSpan)hora).ToString(@"hh\:mm") : null },
                { "tipo_pedido", ApiJson.Celda(h, "Tipo_pedido") },
                { "observaciones", ApiJson.Celda(h, "Observaciones_pedido") },
                { "cantidad_clientes", h["CantidadClientes"] == DBNull.Value ? 0 : Convert.ToInt32(h["CantidadClientes"]) },
                { "total", ApiJson.Dinero(total) },
                { "items", items }
            };
        }

        // ---- utilidades ----

        private static bool Presente(Dictionary<string, object> d, string clave)
        {
            return d.ContainsKey(clave) && d[clave] != null;
        }

        /// <summary>JavaScriptSerializer entrega los arrays como object[] o ArrayList.</summary>
        private static IList<object> Lista(object o)
        {
            object[] arr = o as object[];
            if (arr != null) return arr;
            ArrayList al = o as ArrayList;
            return al == null ? null : al.Cast<object>().ToList();
        }

        private static string Sha256(string texto)
        {
            using (SHA256 sha = SHA256.Create())
                return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(texto)));
        }
    }
}
