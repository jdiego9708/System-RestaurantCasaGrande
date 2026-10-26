using System;
using System.Data;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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

        /// <summary>Imprime las comandas de bebidas y de platos como lo hace FrmPedido (una copia de cada una).</summary>
        public static bool ImprimirComandas(int idPedido)
        {
            bool ocupado;
            return ImprimirComandas(idPedido, "TODO", 1, out ocupado);
        }

        /// <summary>
        /// Encola la impresión de PLATOS, BEBIDAS o TODO. Devuelve false si ya había una en curso
        /// (<paramref name="ocupado"/> = true) o si no hay hilo de UI al que encolarla.
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
                try { Imprimir(idPedido, tipo, copias); }
                finally { Volatile.Write(ref _imprimiendo, 0); }
            });
            if (!encolada)
                Volatile.Write(ref _imprimiendo, 0);
            return encolada;
        }

        private static void Imprimir(int idPedido, string tipo, int copias)
        {
            DataSet ds = NApiMovil.PedidoCompleto(idPedido);
            DataTable platos = TablaComanda();
            DataTable bebidas = TablaComanda();

            foreach (DataRow l in ds.Tables[1].Rows)
            {
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
                        comandas.ObtenerReporte("Casa Grande | Bebidas");
                        comandas.AsignarTablas(bebidas);
                        if (aPdf)
                            ApiLog.Info("Comanda guardada en " + comandas.GuardarPdf(carpeta, NombrePdf(idPedido, "bebidas", copia)));
                        else
                            comandas.ImprimirFactura(1);
                    }
                    if (platos.Rows.Count > 0 && tipo != "BEBIDAS")
                    {
                        comandas.ObtenerReporte("Casa Grande | Platos");
                        comandas.AsignarTablas(platos);
                        if (aPdf)
                            ApiLog.Info("Comanda guardada en " + comandas.GuardarPdf(carpeta, NombrePdf(idPedido, "platos", copia)));
                        else
                            comandas.ImprimirFactura(1);
                    }
                }
            }

            ApiLog.Info(aPdf
                ? "Comandas en PDF (impresora '" + impresora + "'): id_pedido=" + idPedido + " tipo=" + tipo + " copias=" + copias + " carpeta=" + carpeta
                : "Comandas enviadas a la impresora '" + impresora + "': id_pedido=" + idPedido + " tipo=" + tipo + " copias=" + copias);
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
