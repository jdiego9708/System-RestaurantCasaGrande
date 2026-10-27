using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using CapaNegocio;

namespace CapaPresentacion.Api
{
    /// <summary>
    /// Efectos sobre el escritorio tras crear un pedido: refrescar el mapa de mesas e imprimir las comandas.
    /// Todo se encola en el hilo de UI después de responder el HTTP; nada de esto puede fallar la petición.
    /// </summary>
    internal static class ApiImpresion
    {
        /// <summary>1 mientras hay una impresión encolada o en curso (el contrato responde 409).</summary>
        private static int _imprimiendo;

        // No se usa FrmObservarMesas.GetInstancia(): si el mapa no está abierto, crearía el formulario.
        /// <summary>Si el mapa de mesas está abierto, pinta la mesa como PENDIENTE (igual que al guardar en FrmPedido).</summary>
        public static void RefrescarMapaDeMesas(int idPedido, int numMesa)
        {
            ApiUi.Ejecutar("refrescar mesas", () =>
            {
                // Se copia la lista: CargarMesas puede abrir el aviso de espera y modificar OpenForms.
                foreach (Form f in Application.OpenForms.Cast<Form>().ToArray())
                {
                    CapaPresentacion.Formularios.FormsPedido.FrmObservarMesas mapa =
                        f as CapaPresentacion.Formularios.FormsPedido.FrmObservarMesas;
                    if (mapa != null && !mapa.IsDisposed)
                        mapa.ObtenerPedido(idPedido, numMesa, "PENDIENTE");
                }
            });
        }

        /// <summary>
        /// Vuelve a leer el estado de todas las mesas si el mapa está abierto. Se usa después de cambiar un
        /// pedido (agregar, quitar, SALIENDO, cancelar): ObtenerPedido escribe el estado y no sirve aquí.
        /// </summary>
        public static void RefrescarMapa()
        {
            ApiUi.Ejecutar("recargar mesas", () =>
            {
                foreach (Form f in Application.OpenForms.Cast<Form>().ToArray())
                {
                    CapaPresentacion.Formularios.FormsPedido.FrmObservarMesas mapa =
                        f as CapaPresentacion.Formularios.FormsPedido.FrmObservarMesas;
                    if (mapa != null && !mapa.IsDisposed)
                        mapa.RecargarMesas();
                }
            });
        }

        /// <summary>
        /// Encola la impresión de PLATOS, BEBIDAS o TODO. Devuelve false si ya había una en curso
        /// (<paramref name="ocupado"/> = true) o si no hay hilo de UI al que encolarla. Es la que usa
        /// POST /pedidos/{id}/imprimir, que responde 409 en vez de esperar.
        /// </summary>
        public static bool ImprimirComandas(int idPedido, string tipo, int copias, out bool ocupado)
        {
            ocupado = false;
            if (Interlocked.CompareExchange(ref _imprimiendo, 1, 0) != 0)
            {
                ocupado = true;
                return false;
            }

            bool encolada = ApiUi.Ejecutar("imprimir comandas", () =>
            {
                try { Imprimir(idPedido, tipo, copias, null); }
                finally { Volatile.Write(ref _imprimiendo, 0); }
            });
            if (!encolada)
                Volatile.Write(ref _imprimiendo, 0);
            return encolada;
        }

        /// <summary>
        /// Impresión automática al crear un pedido o al agregarle productos. A diferencia de la manual, si ya hay
        /// una impresión en curso ESPERA su turno (hasta 90 s) en vez de descartar la comanda: con varios meseros
        /// enviando a la vez, una comanda perdida es un plato que no sale. Devuelve true si quedó aceptada.
        /// </summary>
        /// <param name="soloDetalles">
        /// Si no es null, solo se imprimen las líneas con esos Id_detalle_pedido (una adición); null = todo el pedido.
        /// </param>
        public static bool EncolarComandas(int idPedido, string tipo, int copias, ICollection<int> soloDetalles)
        {
            return EncolarTrabajo("imprimir comandas", idPedido,
                () => Imprimir(idPedido, tipo, copias, soloDetalles));
        }

        /// <summary>
        /// Imprime la PRECUENTA del pedido tal como la saca el escritorio (reporte Precuenta.rdlc con los valores por
        /// defecto: sin propina, descuento ni cargos extra). Usa la misma cola que las comandas: espera su turno.
        /// </summary>
        public static bool EncolarPrecuenta(int idPedido)
        {
            return EncolarTrabajo("imprimir precuenta", idPedido, () => ImprimirPrecuenta(idPedido));
        }

        /// <summary>Espera el turno de impresión (hasta 90 s) y ejecuta el trabajo en el hilo de UI.</summary>
        private static bool EncolarTrabajo(string nombre, int idPedido, Action trabajo)
        {
            if (!ApiUi.Disponible)
                return false;

            Task.Run(() =>
            {
                DateTime limite = DateTime.UtcNow.AddSeconds(90);
                while (Interlocked.CompareExchange(ref _imprimiendo, 1, 0) != 0)
                {
                    if (DateTime.UtcNow > limite)
                    {
                        ApiLog.Error("Trabajo descartado (" + nombre + "): la impresora siguió ocupada 90 s (id_pedido=" + idPedido + ")", null);
                        return;
                    }
                    Thread.Sleep(250);
                }

                bool encolada = ApiUi.Ejecutar(nombre, () =>
                {
                    try { trabajo(); }
                    finally { Volatile.Write(ref _imprimiendo, 0); }
                });
                if (!encolada)
                    Volatile.Write(ref _imprimiendo, 0);
            });
            return true;
        }

        private static void ImprimirPrecuenta(int idPedido)
        {
            string rpta;
            DataTable detalle;
            DataTable detallePlatos;
            DataTable principales = NPedido.BuscarPedidosYDetalle("ID PEDIDO Y DETALLE",
                Convert.ToString(idPedido), out detalle, out detallePlatos, out rpta);
            if (principales == null || detalle == null)
            {
                ApiLog.Error("Precuenta: no se pudo leer el pedido " + idPedido + " (" + rpta + ")", null);
                return;
            }

            // Igual que FrmFacturarPedido.SumarPrecios: el total parcial es la suma de la columna Total.
            int totalParcial = 0;
            foreach (DataRow fila in detalle.Rows)
                totalParcial += Convert.ToInt32(fila["Total"]);

            // Los valores que deja el escritorio al abrir la precuenta sin tocar nada: propina sugerida 0, descuento 0 %,
            // sin cupón, desechables ni domicilio, observaciones vacías (FrmFacturarPedido.Variables).
            string total = Convert.ToString(totalParcial);
            List<string> variables = new List<string>
            {
                Convert.ToString(idPedido), total, "0", total, "0", string.Empty, "0", "0", total, string.Empty
            };

            string impresora = ImpresoraPredeterminada();
            bool aPdf = EsImpresoraPdf(impresora);
            string carpeta = CarpetaComandas();

            using (FrmFacturaFinal precuenta = new FrmFacturaFinal())
            {
                precuenta.Is_precuenta = true;
                precuenta.Id_pedido = idPedido;
                precuenta.AsignarReporte();
                precuenta.AsignarTablasPrecuenta(variables);
                if (aPdf)
                    ApiLog.Info("Precuenta guardada en " + precuenta.GuardarPdf(carpeta,
                        string.Format("precuenta-{0}-{1:yyyyMMdd-HHmmss}", idPedido, DateTime.Now)));
                else
                    precuenta.ImprimirFactura(1);
            }

            ApiLog.Info(aPdf
                ? "Precuenta en PDF (impresora '" + impresora + "'): id_pedido=" + idPedido + " total=" + total + " carpeta=" + carpeta
                : "Precuenta enviada a la impresora '" + impresora + "': id_pedido=" + idPedido + " total=" + total);
        }

        private static void Imprimir(int idPedido, string tipo, int copias, ICollection<int> soloDetalles)
        {
            bool adicion = soloDetalles != null;
            string sufijo = adicion ? " (ADICION)" : string.Empty;
            string marcaPdf = adicion ? "-adicion" : string.Empty;

            DataSet ds = NApiMovil.PedidoCompleto(idPedido);
            DataTable platos = TablaComanda();
            DataTable bebidas = TablaComanda();

            foreach (DataRow l in ds.Tables[1].Rows)
            {
                // En una adición solo se imprime lo que acaba de agregarse, no todo el pedido.
                if (adicion && !soloDetalles.Contains(Convert.ToInt32(l["Id_detalle_pedido"])))
                    continue;

                string tipoLinea = Convert.ToString(l["Tipo"]);
                string nombre = Convert.ToString(l["Nombre"]);
                if (tipoLinea == "PLATO")
                {
                    // "-Plato:" y debajo cada ingrediente, como arma FrmPedido el nombre del renglón.
                    StringBuilder info = new StringBuilder("-" + nombre);
                    DataRow[] ings = ds.Tables[2].Select("Id_detalle_pedido = " + Convert.ToInt32(l["Id_detalle_pedido"]));
                    if (ings.Length > 0)
                    {
                        info.Append(": ").Append(Environment.NewLine);
                        foreach (DataRow i in ings)
                            info.Append(Convert.ToString(i["Nombre_ingrediente"])).Append(Environment.NewLine);
                    }
                    nombre = info.ToString();
                }
                DataTable destino = tipoLinea == "PLATO" ? platos : bebidas;
                decimal precio = Convert.ToDecimal(l["Precio"]);
                int cantidad = Convert.ToInt32(l["Cantidad"]);
                DataRow fila = destino.NewRow();
                fila["Id_pedido"] = idPedido;
                fila["Id_tipo"] = Convert.ToInt32(l["Id_tipo"]);
                fila["Tipo"] = tipoLinea;
                fila["Nombre"] = nombre;
                fila["Precio"] = precio;
                fila["Cantidad"] = cantidad;
                fila["Total"] = precio * cantidad;
                fila["Observaciones"] = Convert.ToString(l["Observaciones"]);
                destino.Rows.Add(fila);
            }

            // Igual que el escritorio: se imprime en la impresora predeterminada. La diferencia es que, si esa
            // impresora es de PDF (Microsoft Print to PDF, Adobe PDF...), el archivo se guarda solo en Documentos
            // en vez de abrir el diálogo de "Guardar como", que dejaría la petición colgada.
            string impresora = ImpresoraPredeterminada();
            bool aPdf = EsImpresoraPdf(impresora);
            string carpeta = CarpetaComandas();

            using (FrmComandas comandas = new FrmComandas())
            {
                comandas.Id_pedido = idPedido;
                for (int copia = 1; copia <= copias; copia++)
                {
                    if (bebidas.Rows.Count > 0 && tipo != "PLATOS")
                    {
                        comandas.ObtenerReporte("Casa Grande | Bebidas" + sufijo);
                        comandas.AsignarTablas(bebidas);
                        if (aPdf)
                            ApiLog.Info("Comanda guardada en " + comandas.GuardarPdf(carpeta, NombrePdf(idPedido, "bebidas" + marcaPdf, copia)));
                        else
                            comandas.ImprimirFactura(1);
                    }
                    if (platos.Rows.Count > 0 && tipo != "BEBIDAS")
                    {
                        comandas.ObtenerReporte("Casa Grande | Platos" + sufijo);
                        comandas.AsignarTablas(platos);
                        if (aPdf)
                            ApiLog.Info("Comanda guardada en " + comandas.GuardarPdf(carpeta, NombrePdf(idPedido, "platos" + marcaPdf, copia)));
                        else
                            comandas.ImprimirFactura(1);
                    }
                }
            }

            ApiLog.Info(aPdf
                ? "Comandas en PDF (impresora '" + impresora + "'): id_pedido=" + idPedido + " tipo=" + tipo + " copias=" + copias + " carpeta=" + carpeta + (adicion ? " adicion=" + soloDetalles.Count + " lineas" : string.Empty)
                : "Comandas enviadas a la impresora '" + impresora + "': id_pedido=" + idPedido + " tipo=" + tipo + " copias=" + copias + (adicion ? " adicion=" + soloDetalles.Count + " lineas" : string.Empty));
        }

        /// <summary>Impresora predeterminada de Windows; cadena vacía si no hay ninguna.</summary>
        private static string ImpresoraPredeterminada()
        {
            try
            {
                foreach (string nombre in PrinterSettings.InstalledPrinters)
                {
                    PrinterSettings s = new PrinterSettings { PrinterName = nombre };
                    if (s.IsDefaultPrinter)
                        return nombre;
                }
            }
            catch (Exception ex)
            {
                ApiLog.Error("No se pudo leer la impresora predeterminada", ex);
            }
            return string.Empty;
        }

        /// <summary>
        /// True si la impresora genera un archivo (Microsoft Print to PDF, Adobe PDF...). Sin impresora también
        /// se devuelve true: es preferible dejar el PDF guardado a reventar con un mensaje en la cara.
        /// </summary>
        private static bool EsImpresoraPdf(string impresora)
        {
            if (string.IsNullOrWhiteSpace(impresora))
                return true;
            return impresora.IndexOf("PDF", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>Documentos\Casa Grande\Comandas, o lo que diga ApiComandasCarpeta (relativo a Documentos).</summary>
        private static string CarpetaComandas()
        {
            string configurada = ApiConfig.ComandasCarpeta;
            if (!string.IsNullOrWhiteSpace(configurada))
                return Path.IsPathRooted(configurada)
                    ? configurada
                    : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), configurada);

            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "Casa Grande",
                "Comandas");
        }

        private static string NombrePdf(int idPedido, string grupo, int copia)
        {
            return string.Format("comanda-{0}-{1:yyyyMMdd-HHmmss}-{2}-{3}",
                idPedido, DateTime.Now, grupo, copia);
        }

        private static DataTable TablaComanda()
        {
            DataTable t = new DataTable();
            t.Columns.Add("Id_pedido", typeof(int));
            t.Columns.Add("Id_tipo", typeof(int));
            t.Columns.Add("Tipo", typeof(string));
            t.Columns.Add("Nombre", typeof(string));
            t.Columns.Add("Precio", typeof(decimal));
            t.Columns.Add("Cantidad", typeof(int));
            t.Columns.Add("Total", typeof(string));
            t.Columns.Add("Observaciones", typeof(string));
            return t;
        }
    }

}
