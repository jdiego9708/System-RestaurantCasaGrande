using System;
using System.Data;
using System.Linq;
using System.Text;
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

        /// <summary>Imprime la comanda de bebidas y la de platos como lo hace FrmPedido (una copia de cada una).</summary>
        public static bool ImprimirComandas(int idPedido)
        {
            return ApiUi.Ejecutar("imprimir comandas", () =>
            {
                DataSet ds = NApiMovil.PedidoCompleto(idPedido);
                DataTable platos = TablaComanda();
                DataTable bebidas = TablaComanda();

                foreach (DataRow l in ds.Tables[1].Rows)
                {
                    string tipo = Convert.ToString(l["Tipo"]);
                    string nombre = Convert.ToString(l["Nombre"]);
                    if (tipo == "PLATO")
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
                    DataTable destino = tipo == "PLATO" ? platos : bebidas;
                    decimal precio = Convert.ToDecimal(l["Precio"]);
                    int cantidad = Convert.ToInt32(l["Cantidad"]);
                    DataRow fila = destino.NewRow();
                    fila["Id_pedido"] = idPedido;
                    fila["Id_tipo"] = Convert.ToInt32(l["Id_tipo"]);
                    fila["Tipo"] = tipo;
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
                    if (bebidas.Rows.Count > 0)
                    {
                        comandas.ObtenerReporte("Casa Grande | Bebidas");
                        comandas.AsignarTablas(bebidas);
                        comandas.ImprimirFactura(1);
                    }
                    if (platos.Rows.Count > 0)
                    {
                        comandas.ObtenerReporte("Casa Grande | Platos");
                        comandas.AsignarTablas(platos);
                        comandas.ImprimirFactura(1);
                    }
                }
                ApiLog.Info("Comandas enviadas a la impresora: id_pedido=" + idPedido);
            });
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
