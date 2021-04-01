using CapaNegocio;
using CapaPresentacion.Formularios.FormsPedido.Bebidas;
using CapaPresentacion.Formularios.FormsPedido.Platos;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace CapaPresentacion.Formularios.FormsPedido
{
    public class TablasPedido
    {
        public TablasPedido(bool isEditar, int id_pedido, DataTable dtDetalle)
        {
            try
            {
                this.IsEditar = isEditar;
                this.Id_pedido = id_pedido;

                this.BebidasPedido = new List<EBebidas>();
                this.PlatosPedido = new List<EPlatos>();

                this.BebidasPedidoEditar = new List<EBebidas>();
                this.PlatosPedidoEditar = new List<EPlatos>();

                if (dtDetalle != null)
                {
                    foreach (DataRow row in dtDetalle.Rows)
                    {
                        if (Convert.ToString(row["Tipo"]).Equals("PLATO"))
                        {
                            this.AgregarPlato(Convert.ToInt32(row["Id_tipo"]), Convert.ToInt32(row["Precio"]),
                                Convert.ToInt32(row["Cantidad"]), Convert.ToString(row["Observaciones"]), false);
                        }
                        else
                        {
                            this.AgregarBebida(Convert.ToInt32(row["Id_tipo"]), Convert.ToInt32(row["Precio"]),
                                Convert.ToInt32(row["Cantidad"]), Convert.ToString(row["Observaciones"]), false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto("TablasPedido.cs", "TablasPedido(bool isEditar, int id_pedido)",
                    "Hubo un error al inicializar las tablas del pedido", ex.Message);
            }
        }

        public TablasPedido()
        {
            this.BebidasPedido = new List<EBebidas>();
            this.PlatosPedido = new List<EPlatos>();
        }

        //Variables para que guardan los platos y bebidas que se hacen en una edición de pedido
        private List<EBebidas> bebidasPedidoEditar;
        private List<EPlatos> platosPedidoEditar;

        public List<EBebidas> BebidasPedidoEditar { get => bebidasPedidoEditar; set => bebidasPedidoEditar = value; }
        public List<EPlatos> PlatosPedidoEditar { get => platosPedidoEditar; set => platosPedidoEditar = value; }
        //Variables lista de tipo EBebidas y EPlatos que contienen los datos de
        //Los productos que agregaremos al pedido

        private List<EBebidas> bebidasPedido;
        private List<EPlatos> platosPedido;
        private bool _isEditar;
        private int _id_pedido;

        public List<EBebidas> BebidasPedido { get => bebidasPedido; set => bebidasPedido = value; }
        public List<EPlatos> PlatosPedido { get => platosPedido; set => platosPedido = value; }
        public bool IsEditar { get => _isEditar; set => _isEditar = value; }
        public int Id_pedido { get => _id_pedido; set => _id_pedido = value; }

        public IEnumerable<EPlatos> ComprobacionPlato(int id_plato)
        {
            IEnumerable<EPlatos> Query = from platos in this.PlatosPedido
                                         where platos.Id_plato == id_plato
                                         select platos;
            return Query;
        }

        public IEnumerable<EBebidas> ComprobacionBebida(int id_bebida)
        {
            IEnumerable<EBebidas> Query = from bebidas in this.BebidasPedido
                                          where bebidas.Id_bebida == id_bebida
                                          select bebidas;

            return Query;
        }

        public IEnumerable<EPlatos> ComprobacionPlatoEditar(int id_plato)
        {
            IEnumerable<EPlatos> Query = from platos in this.PlatosPedidoEditar
                                         where platos.Id_plato == id_plato
                                         select platos;
            return Query;
        }

        public IEnumerable<EBebidas> ComprobacionBebidaEditar(int id_bebida)
        {
            IEnumerable<EBebidas> Query = from bebidas in this.BebidasPedidoEditar
                                          where bebidas.Id_bebida == id_bebida
                                          select bebidas;

            return Query;
        }

        public void AgregarPlato(int id_plato, int precio_cobrar,
            int cantidad_pedido, string observaciones, bool editar)
        {
            //1- Comprobación si existe en la lista de EPlatos
            //platoComprobado para guardar los platos que existen
            IEnumerable<EPlatos> platoComprobado = null;
            //platoComprobadoEditar para guardar los platos que existen en edición
            IEnumerable<EPlatos> platoComprobadoEditar = null;
            bool existeEditar = false;
            bool existe = false;
            //Si es editar buscaremos platos en edición
            if (editar)
            {
                platoComprobadoEditar = this.ComprobacionPlatoEditar(id_plato);
                //Recorremos platoComprobadoEditar a ver si existen platos en edición
                foreach (EPlatos pl in platoComprobadoEditar)
                {
                    existeEditar = true;
                    pl.Cantidad_pedido += 1;
                    pl.Precio_cobrar = precio_cobrar;
                    pl.Observaciones_pedido = observaciones;
                    break;
                }
                //Si no existen platos en edición vamos a agregar uno nuevo
                if (!existeEditar)
                {
                    DataTable dtPlato =
                            NPlatos.BuscarPlatos("ID PLATO", id_plato.ToString(), "ACTIVO", out string rpta);
                    if (dtPlato != null)
                    {
                        foreach (DataRow row in dtPlato.Rows)
                        {
                            EPlatos ePlatos = new EPlatos();
                            ePlatos.Id_plato = Convert.ToInt32(row["Id_plato"]);
                            ePlatos.Nombre_plato = Convert.ToString(row["Nombre_plato"]);
                            ePlatos.Precio_plato = Convert.ToInt32(row["Precio_plato"]);
                            ePlatos.Imagen_plato = Convert.ToString(row["Imagen_plato"]);
                            ePlatos.Descripcion_plato = Convert.ToString(row["Descripcion_plato"]);
                            ePlatos.Id_tipo_plato = Convert.ToInt32(row["Id_tipo_plato"]);
                            ePlatos.Tipo_plato = Convert.ToString(row["Tipo_plato"]);

                            ePlatos.Cantidad_pedido = cantidad_pedido;
                            ePlatos.Observaciones_pedido = observaciones;
                            ePlatos.Precio_cobrar = precio_cobrar;
                            this.PlatosPedidoEditar.Add(ePlatos);
                        }
                    }
                }
            }

            platoComprobado = this.ComprobacionPlato(id_plato);
            //Recorremos platoComprobado a ver si existen platos anteriormente
            foreach (EPlatos pl in platoComprobado)
            {
                existe = true;
                if (!editar)
                {
                    pl.Cantidad_pedido += 1;
                    pl.Precio_cobrar = precio_cobrar;
                    pl.Observaciones_pedido = observaciones;
                    break;
                }
            }
            //Si no existen platos en edición vamos a agregar uno nuevo
            if (!existe)
            {
                DataTable dtPlato =
                        NPlatos.BuscarPlatos("ID PLATO", id_plato.ToString(), "ACTIVO", out string rpta);
                if (dtPlato != null)
                {
                    foreach (DataRow row in dtPlato.Rows)
                    {
                        EPlatos ePlatos = new EPlatos();
                        ePlatos.Id_plato = Convert.ToInt32(row["Id_plato"]);
                        ePlatos.Nombre_plato = Convert.ToString(row["Nombre_plato"]);
                        ePlatos.Precio_plato = Convert.ToInt32(row["Precio_plato"]);
                        ePlatos.Imagen_plato = Convert.ToString(row["Imagen_plato"]);
                        ePlatos.Descripcion_plato = Convert.ToString(row["Descripcion_plato"]);
                        ePlatos.Id_tipo_plato = Convert.ToInt32(row["Id_tipo_plato"]);
                        ePlatos.Tipo_plato = Convert.ToString(row["Tipo_plato"]);

                        ePlatos.Cantidad_pedido = cantidad_pedido;
                        ePlatos.Observaciones_pedido = observaciones;
                        ePlatos.Precio_cobrar = precio_cobrar;
                        if (!editar)
                            this.PlatosPedido.Add(ePlatos);
                    }
                }
            }
        }

        public void AgregarBebida(int id_bebida, int precio_cobrar, 
            int cantidad_pedido, string observaciones, bool editar)
        {
            //1- Comprobación si existe en la lista de EPlatos
            IEnumerable<EBebidas> bebidaComprobado;
            IEnumerable<EBebidas> bebidaComprobadoEditar;

            //Si la lista es diferente de null, es decir que si encontró coincidencias
            //y actualizamos la cantidad de dicho plato
            bool existe = false;
            bool existeEditar = false;
            if (editar)
            {
                bebidaComprobadoEditar = ComprobacionBebidaEditar(id_bebida);

                foreach (EBebidas be in bebidaComprobadoEditar)
                {
                    existeEditar = true;
                    be.Cantidad_pedido += 1;
                    be.Precio_cobrar = precio_cobrar;
                    be.Observaciones_pedido = observaciones;
                    break;
                }

                if (!existeEditar)
                {
                    DataTable dtBebida =
                                NBebidas.BuscarBebida("ID BEBIDA", id_bebida.ToString(), "ACTIVO", out string rpta);
                    if (dtBebida != null)
                    {
                        foreach (DataRow row in dtBebida.Rows)
                        {
                            EBebidas eBebidas = new EBebidas();
                            eBebidas.Id_bebida = Convert.ToInt32(row["Id_bebida"]);
                            eBebidas.Nombre_bebida = Convert.ToString(row["Nombre_bebida"]);
                            eBebidas.Precio_bebida = Convert.ToInt32(row["Precio_bebida"]);
                            eBebidas.Precio_trago = Convert.ToInt32(row["Precio_trago"]);
                            eBebidas.Precio_trago_doble = Convert.ToInt32(row["Precio_trago_doble"]);
                            eBebidas.Precio_proveedor = Convert.ToInt32(row["Precio_proveedor"]);
                            eBebidas.Id_proveedor = Convert.ToInt32(row["Id_proveedor"]);
                            eBebidas.Imagen = Convert.ToString(row["Imagen"]);
                            eBebidas.Id_tipo_bebida = Convert.ToInt32(row["Id_tipo_bebida"]);
                            eBebidas.Cantidad_unidades = Convert.ToInt32(row["Cantidad_unidades"]);
                            eBebidas.Cantidad_x_unidad = Convert.ToInt32(row["Cantidad_x_unidad"]);
                            eBebidas.Cantidad_total = Convert.ToInt32(row["Cantidad_total"]);
                            eBebidas.Tipo_bebida = Convert.ToString(row["Tipo_bebida"]);

                            eBebidas.Cantidad_pedido = cantidad_pedido;

                            eBebidas.Observaciones_pedido = observaciones;
                            eBebidas.Precio_cobrar = precio_cobrar;

                            this.BebidasPedidoEditar.Add(eBebidas);

                        }
                    }
                }
            }

            bebidaComprobado = ComprobacionBebida(id_bebida);

            foreach (EBebidas be in bebidaComprobado)
            {
                existe = true;
                if (!editar)
                {
                    be.Cantidad_pedido += 1;
                    be.Precio_cobrar = precio_cobrar;
                    be.Observaciones_pedido = observaciones;
                }

                break;
            }
            if (!existe)
            {
                DataTable dtBebida =
                            NBebidas.BuscarBebida("ID BEBIDA", id_bebida.ToString(), "ACTIVO", out string rpta);
                if (dtBebida != null)
                {
                    foreach (DataRow row in dtBebida.Rows)
                    {
                        EBebidas eBebidas = new EBebidas();
                        eBebidas.Id_bebida = Convert.ToInt32(row["Id_bebida"]);
                        eBebidas.Nombre_bebida = Convert.ToString(row["Nombre_bebida"]);
                        eBebidas.Precio_bebida = Convert.ToInt32(row["Precio_bebida"]);
                        eBebidas.Precio_trago = Convert.ToInt32(row["Precio_trago"]);
                        eBebidas.Precio_trago_doble = Convert.ToInt32(row["Precio_trago_doble"]);
                        eBebidas.Precio_proveedor = Convert.ToInt32(row["Precio_proveedor"]);
                        eBebidas.Id_proveedor = Convert.ToInt32(row["Id_proveedor"]);
                        eBebidas.Imagen = Convert.ToString(row["Imagen"]);
                        eBebidas.Id_tipo_bebida = Convert.ToInt32(row["Id_tipo_bebida"]);
                        eBebidas.Cantidad_unidades = Convert.ToInt32(row["Cantidad_unidades"]);
                        eBebidas.Cantidad_x_unidad = Convert.ToInt32(row["Cantidad_x_unidad"]);
                        eBebidas.Cantidad_total = Convert.ToInt32(row["Cantidad_total"]);
                        eBebidas.Tipo_bebida = Convert.ToString(row["Tipo_bebida"]);

                        eBebidas.Cantidad_pedido = cantidad_pedido;

                        eBebidas.Observaciones_pedido = observaciones;
                        eBebidas.Precio_cobrar = precio_cobrar;
                        if (!editar)
                            this.BebidasPedido.Add(eBebidas);

                    }
                }
            }

        }

        public void RemoverPlato(int id_plato, int id_pedido, bool iseditar, int id_usuario)
        {
            //1- Comprobación si existe en la lista de EPlatos
            IEnumerable<EPlatos> platoComprobado;
            if (iseditar)
            {
                platoComprobado = this.ComprobacionPlatoEditar(id_plato);
            }
            else
            {
                platoComprobado = this.ComprobacionPlato(id_plato);
            }

            //Si la lista es diferente de null, es decir que si encontró coincidencias
            //y actualizamos la cantidad de dicho plato
            if (platoComprobado != null)
            {
                string tipo_update = "DELETE";
                string rpta = "";
                EPlatos ePlatos = new EPlatos();
                foreach (EPlatos pl in platoComprobado)
                {
                    ePlatos = pl;
                    if (pl.Cantidad_pedido >= 1)
                    {
                        //Si es mayor que uno, descontamos uno y enviamos a la base de datos
                        pl.Cantidad_pedido -= 1;

                        //Si es menor que 1 es decir 0 o algún valor negativo lo eliminaremos
                        if (pl.Cantidad_pedido < 1)
                        {
                            if (iseditar)
                            {
                                this.PlatosPedidoEditar.Remove(pl);
                            }
                            else
                            {

                                this.PlatosPedido.Remove(pl);
                            }
                            break;
                        }
                    }
                }
                if (this.IsEditar & iseditar == false)
                {
                    //rpta = NPedido.ActualizarDetallePedido(new List<string> { Convert.ToString(this.Id_pedido),
                    //                    Convert.ToString(ePlatos.Id_plato), "PLATO", Convert.ToString(ePlatos.Precio_cobrar),
                    //                    Convert.ToString(ePlatos.Cantidad_pedido), ePlatos.Observaciones_pedido, id_usuario.ToString(), tipo_update});
                }
            }
        }

        public void RemoverBebida(int id_bebida, int id_pedido, bool iseditar, int id_usuario)
        {
            //1- Comprobación si existe en la lista de EPlatos
            IEnumerable<EBebidas> bebidaComprobado;
            if (iseditar)
            {
                bebidaComprobado = this.ComprobacionBebidaEditar(id_bebida);
            }
            else
            {
                bebidaComprobado = this.ComprobacionBebida(id_bebida);
            }

            //Si la lista es diferente de null, es decir que si encontró coincidencias
            //y actualizamos la cantidad de dicho plato
            if (bebidaComprobado != null)
            {
                string tipo_update = "DELETE";
                string rpta = "";
                EBebidas eBebidas = new EBebidas();
                foreach (EBebidas pl in bebidaComprobado)
                {
                    eBebidas = pl;
                    if (pl.Cantidad_pedido >= 1)
                    {
                        //Si es mayor que uno, descontamos uno y enviamos a la base de datos
                        pl.Cantidad_pedido -= 1;

                        //Si es menor que 1 es decir 0 o algún valor negativo lo eliminaremos
                        if (pl.Cantidad_pedido < 1)
                        {
                            if (iseditar)
                            {
                                this.BebidasPedidoEditar.Remove(pl);
                            }
                            else
                            {
                                this.BebidasPedido.Remove(pl);
                            }
                            break;
                        }
                    }
                }

                if (this.IsEditar & iseditar == false)
                {
                    //rpta = NPedido.ActualizarDetallePedido(new List<string> { Convert.ToString(this.Id_pedido),
                    //                Convert.ToString(eBebidas.Id_bebida), "BEBIDA", Convert.ToString(eBebidas.Precio_cobrar),
                    //                Convert.ToString(eBebidas.Cantidad_pedido), eBebidas.Observaciones_pedido, id_usuario.ToString(), tipo_update});
                }
            }
        }

        public void AddObservacion(int id_tipo, string observaciones, string tipo, out string rpta)
        {
            rpta = "OK";
            try
            {
                if (tipo.Equals("PLATO"))
                {
                    //1- Comprobación si existe en la lista de EPlatos
                    IEnumerable<EPlatos> platoComprobado = this.ComprobacionPlato(id_tipo);
                    foreach (EPlatos pl in platoComprobado)
                    {
                        pl.Observaciones_pedido = observaciones;                        
                    }

                    if (this.IsEditar)
                    {
                        //1- Comprobación si existe en la lista de EPlatos
                        IEnumerable<EPlatos> platoComprobadoEditar = this.ComprobacionPlatoEditar(id_tipo);
                        foreach (EPlatos plEdit in platoComprobadoEditar)
                        {
                            plEdit.Observaciones_pedido = observaciones;
                        }
                    }
                }
                else
                {
                    //1- Comprobación si existe en la lista de EPlatos
                    IEnumerable<EBebidas> bebidaComprobado = this.ComprobacionBebida(id_tipo);
                    foreach (EBebidas be in bebidaComprobado)
                    {
                        be.Observaciones_pedido = observaciones;
                    }

                    if (this.IsEditar)
                    {
                        //1- Comprobación si existe en la lista de EPlatos
                        IEnumerable<EBebidas> bebidaComprobadoEditar = this.ComprobacionBebidaEditar(id_tipo);
                        foreach (EBebidas beEdit in bebidaComprobadoEditar)
                        {
                            beEdit.Observaciones_pedido = observaciones;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                rpta = ex.Message;
            }
        }

        public DataTable dtDetallePedido()
        {
            DataTable dtDetalle = new DataTable("DetallePedido");
            dtDetalle.Columns.Add("Id_tipo", typeof(string));
            dtDetalle.Columns.Add("Tipo", typeof(string));
            dtDetalle.Columns.Add("Precio", typeof(string));
            dtDetalle.Columns.Add("Cantidad", typeof(string));
            dtDetalle.Columns.Add("Observaciones", typeof(string));

            foreach (EPlatos pl in this.PlatosPedido)
            {
                DataRow row = dtDetalle.NewRow();
                row["Id_tipo"] = pl.Id_plato;
                row["Tipo"] = "PLATO";
                row["Precio"] = pl.Precio_cobrar;
                row["Cantidad"] = pl.Cantidad_pedido;
                row["Observaciones"] = pl.Observaciones_pedido;
                dtDetalle.Rows.Add(row);
            }

            foreach (EBebidas be in this.BebidasPedido)
            {
                DataRow row = dtDetalle.NewRow();
                row["Id_tipo"] = be.Id_bebida;
                row["Tipo"] = "BEBIDA";
                row["Precio"] = be.Precio_cobrar;
                row["Cantidad"] = be.Cantidad_pedido;
                row["Observaciones"] = be.Observaciones_pedido;
                dtDetalle.Rows.Add(row);
            }

            return dtDetalle;
        }

        public DataTable dtVistaPedido()
        {
            DataTable dtVista = new DataTable("VistaPedido");
            dtVista.Columns.Add("Id_tipo", typeof(string));
            dtVista.Columns.Add("Tipo", typeof(string));
            dtVista.Columns.Add("Nombre", typeof(string));
            dtVista.Columns.Add("Precio", typeof(string));
            dtVista.Columns.Add("Cantidad", typeof(string));
            dtVista.Columns.Add("Total", typeof(string));
            dtVista.Columns.Add("Observaciones", typeof(string));

            foreach (EPlatos pl in this.PlatosPedido)
            {
                DataRow row = dtVista.NewRow();
                row["Id_tipo"] = pl.Id_plato;
                row["Tipo"] = "PLATO";
                row["Nombre"] = pl.Nombre_plato;
                row["Precio"] = pl.Precio_cobrar;
                row["Cantidad"] = pl.Cantidad_pedido;
                row["Total"] = pl.Precio_cobrar * pl.Cantidad_pedido;
                row["Observaciones"] = pl.Observaciones_pedido;
                dtVista.Rows.Add(row);
            }

            foreach (EBebidas be in this.BebidasPedido)
            {
                DataRow row = dtVista.NewRow();
                row["Id_tipo"] = be.Id_bebida;
                row["Tipo"] = "BEBIDA";
                row["Nombre"] = be.Nombre_bebida;
                row["Precio"] = be.Precio_cobrar;
                row["Cantidad"] = be.Cantidad_pedido;
                row["Total"] = be.Precio_cobrar * be.Cantidad_pedido;
                row["Observaciones"] = be.Observaciones_pedido;
                dtVista.Rows.Add(row);
            }

            return dtVista;
        }

        public DataTable dtDetallePedidoEditado()
        {
            DataTable dtVista = new DataTable("VistaPedido");
            dtVista.Columns.Add("Id_tipo", typeof(string));
            dtVista.Columns.Add("Tipo", typeof(string));
            dtVista.Columns.Add("Nombre", typeof(string));
            dtVista.Columns.Add("Precio", typeof(string));
            dtVista.Columns.Add("Cantidad", typeof(string));
            dtVista.Columns.Add("Total", typeof(string));
            dtVista.Columns.Add("Observaciones", typeof(string));

            foreach (EPlatos pl in this.PlatosPedidoEditar)
            {
                DataRow row = dtVista.NewRow();
                row["Id_tipo"] = pl.Id_plato;
                row["Tipo"] = "PLATO";
                row["Nombre"] = pl.Nombre_plato;
                row["Precio"] = pl.Precio_cobrar;
                row["Cantidad"] = pl.Cantidad_pedido;
                row["Total"] = pl.Precio_cobrar * pl.Cantidad_pedido;
                row["Observaciones"] = pl.Observaciones_pedido;
                dtVista.Rows.Add(row);
            }

            foreach (EBebidas be in this.BebidasPedidoEditar)
            {
                DataRow row = dtVista.NewRow();
                row["Id_tipo"] = be.Id_bebida;
                row["Tipo"] = "BEBIDA";
                row["Nombre"] = be.Nombre_bebida;
                row["Precio"] = be.Precio_cobrar;
                row["Cantidad"] = be.Cantidad_pedido;
                row["Total"] = be.Precio_cobrar * be.Cantidad_pedido;
                row["Observaciones"] = be.Observaciones_pedido;
                dtVista.Rows.Add(row);
            }

            return dtVista;
        }
    }
}
