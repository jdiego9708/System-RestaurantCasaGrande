using System;
using System.Data;
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
                // Application.OpenForms solo se toca desde el hilo de UI; no se usa GetInstancia() porque crearía el formulario.
                foreach (Form f in Application.OpenForms)
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
                foreach (Form f in Application.OpenForms)
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
            if (ApiConfig.ImpresionSimulada)
            {
                // Solo para pruebas: no toca la impresora ni abre formularios.
                ApiLog.Info("Impresion simulada (ApiImpresionSimulada=true): id_pedido=" + idPedido +
                            " tipo=" + tipo + " copias=" + copias);
                return;
            }

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

            using (FrmComandas comandas = new FrmComandas())
            {
                comandas.Id_pedido = idPedido;
                for (int copia = 0; copia < copias; copia++)
                {
                    if (bebidas.Rows.Count > 0 && tipo != "PLATOS")
                    {
                        comandas.ObtenerReporte("Casa Grande | Bebidas");
                        comandas.AsignarTablas(bebidas);
                        comandas.ImprimirFactura(1);
                    }
                    if (platos.Rows.Count > 0 && tipo != "BEBIDAS")
                    {
                        comandas.ObtenerReporte("Casa Grande | Platos");
                        comandas.AsignarTablas(platos);
                        comandas.ImprimirFactura(1);
                    }
                }
            }
            ApiLog.Info("Comandas enviadas a la impresora: id_pedido=" + idPedido + " tipo=" + tipo + " copias=" + copias);
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
