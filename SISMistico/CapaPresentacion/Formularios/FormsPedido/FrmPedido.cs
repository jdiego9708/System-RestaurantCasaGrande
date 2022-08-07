namespace CapaPresentacion.Formularios.FormsPedido
{
    using CapaEntidades.Models;
    using CapaNegocio;
    using CapaPresentacion.Formularios.FormsPedido.Platos;
    using CapaPresentacion.Properties;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    public partial class FrmPedido : Form
    {
        public FrmPedido()
        {
            InitializeComponent();
            this.txtBusqueda.onKeyPress += TxtBusqueda_OnTextoKeyPress;
            this.btnPlatos.Click += BtnPlatos_Click;
            this.btnBebidas.Click += BtnBebidas_Click;
            this.btnSave.Click += BtnSave_Click;
            this.Load += FrmPedido_Load;
        }

        private DialogResult Comprobacion()
        {
            FrmComprobacion frm = new FrmComprobacion
            {
                StartPosition = FormStartPosition.CenterScreen
            };
            frm.FormClosed += FrmComprobacion_FormClosed;
            frm.ShowDialog();
            return frm.DialogResult;
        }

        private void FrmComprobacion_FormClosed(object sender, FormClosedEventArgs e)
        {
            FrmComprobacion frm = (FrmComprobacion)sender;
            if (frm.DialogResult == DialogResult.OK)
            {
                DatosInicioSesion datos = DatosInicioSesion.GetInstancia();
                datos.EmpleadoClaveMaestra = frm.EmpleadoSelected;
                this.EmpleadoSelected = frm.EmpleadoSelected;
            }
        }

        private bool Comprobaciones(out Pedidos pedido,
            out List<Detalle_pedido> listDetallePedido,
            out DataTable dtDetallePedido,
            out DataTable dtPlatos,
            out DataTable dtBebidas)
        {
            dtPlatos = new DataTable("DetallePedidoPlatos");
            dtBebidas = new DataTable("DetallePedidoBebidas");
            //Asignar las variables del pedido, es decir los datos principales
            pedido = new Pedidos();
            listDetallePedido = new List<Detalle_pedido>();
            dtDetallePedido = new DataTable();

            if (this.EmpleadoSelected == null)
            {
                MensajeEspera.CloseForm();
                Mensajes.MensajeInformacion("No hay un empleado seleccionado");
                return false;
            }

            if (this.ClienteSelected == null)
            {
                MensajeEspera.CloseForm();
                Mensajes.MensajeInformacion("No hay un cliente seleccionado");
                return false;
            }

            if (this.MesaSelected == null)
            {
                MensajeEspera.CloseForm();
                Mensajes.MensajeInformacion("No hay una mesa seleccionada");
                return false;
            }

            string tipo_pedido;
            if (this.IsDomicilio)
                tipo_pedido = "DOMICILIO";
            else
                tipo_pedido = "MESA";


            pedido = new Pedidos
            {
                Id_pedido = 0,
                Id_mesa = this.MesaSelected.Id_mesa,
                Id_empleado = this.EmpleadoSelected.Id_empleado,
                Id_cliente = this.ClienteSelected.Id_cliente,
                Fecha_pedido = DateTime.Now,
                Hora_pedido = DateTime.Now.ToString("HH:mm"),
                Tipo_pedido = tipo_pedido,
                Observaciones_pedido = string.Empty,
                CantidadClientes = (int)this.numericClientes.Value
            };

            //Comprobar si hay productos seleccionados
            if (this.ProductsAddSelected == null && !this.IsEditar)
            {
                MensajeEspera.CloseForm();
                Mensajes.MensajeInformacion("No hay productos seleccionados");
                return false;
            }

            dtDetallePedido.Columns.Add("Id_pedido", typeof(int));
            dtDetallePedido.Columns.Add("Id_tipo", typeof(int));
            dtDetallePedido.Columns.Add("Tipo", typeof(string));
            dtDetallePedido.Columns.Add("Nombre", typeof(string));
            dtDetallePedido.Columns.Add("Precio", typeof(decimal));
            dtDetallePedido.Columns.Add("Cantidad", typeof(int));
            dtDetallePedido.Columns.Add("Total", typeof(string));
            dtDetallePedido.Columns.Add("Observaciones", typeof(string));

            dtPlatos = dtDetallePedido.Clone();
            dtBebidas = dtDetallePedido.Clone();

            if (this.ProductsAddSelected != null)
            {
                foreach (ProductBinding pr in this.ProductsAddSelected)
                {
                    DataRow newRow = dtDetallePedido.NewRow();

                    string nombre = pr.Nombre;

                    if (this.IsEditar)
                        newRow["Id_pedido"] = this.Pedido.Id_pedido;
                    else
                        newRow["Id_pedido"] = 0;

                    newRow["Id_tipo"] = pr.Id_producto;
                    newRow["Tipo"] = pr.Tipo_producto;
                    newRow["Nombre"] = pr.Nombre;
                    newRow["Precio"] = pr.Precio;
                    newRow["Cantidad"] = pr.Cantidad;
                    newRow["Total"] = pr.Cantidad * pr.Precio;
                    newRow["Observaciones"] = pr.Observaciones;

                    Detalle_pedido detalle = new Detalle_pedido();

                    if (this.IsEditar)
                        detalle.Id_pedido = this.Pedido.Id_pedido;
                    else
                        detalle.Id_pedido = 0;

                    detalle.Id_tipo = pr.Id_producto;
                    detalle.Tipo = pr.Tipo_producto;
                    detalle.Precio = pr.Precio;
                    detalle.Cantidad = pr.Cantidad;
                    detalle.Observaciones = pr.Observaciones;

                    //Agregamos la lista de detalles si es un plato
                    if (pr.Tipo_producto.Equals("PLATO"))
                    {
                        CapaEntidades.Models.Platos plato =
                            (CapaEntidades.Models.Platos)pr.Product;

                        if (plato.Plato_detallado.Equals("ACTIVO"))
                        {
                            if (pr.ProductDetalles != null)
                            {
                                StringBuilder info = new StringBuilder();
                                info.Append("-" + plato.Nombre_plato).Append(": ").Append(Environment.NewLine);

                                foreach (ProductDetalleBinding de in pr.ProductDetalles)
                                {
                                    Detalle_ingredientes_pedido detail = new Detalle_ingredientes_pedido
                                    {
                                        Id_ingrediente = de.Id_ingrediente,
                                        Ingrediente = de.Ingrediente,
                                        Id_pedido = de.Id_pedido,
                                        Id_tipo = pr.Id_producto,
                                    };

                                    info.Append(de.Ingrediente.Nombre_ingrediente).Append(Environment.NewLine);

                                    nombre = info.ToString();
                                    newRow["Nombre"] = info.ToString();

                                    if (detalle.ListDetalleIngredientes == null)
                                        detalle.ListDetalleIngredientes = new List<Detalle_ingredientes_pedido>();

                                    detalle.ListDetalleIngredientes.Add(detail);
                                }
                            }
                        }

                        listDetallePedido.Add(detalle);

                        DataRow newRowPlato = dtPlatos.NewRow();
                        newRowPlato["Id_tipo"] = pr.Id_producto;
                        newRowPlato["Tipo"] = pr.Tipo_producto;
                        newRowPlato["Nombre"] = nombre;
                        newRowPlato["Precio"] = pr.Precio;
                        newRowPlato["Cantidad"] = pr.Cantidad;
                        newRowPlato["Total"] = pr.Cantidad * pr.Precio;
                        newRowPlato["Observaciones"] = pr.Observaciones;
                        dtPlatos.Rows.Add(newRowPlato);
                    }
                    else
                    {
                        CapaEntidades.Models.Bebidas bebida =
                            (CapaEntidades.Models.Bebidas)pr.Product;

                        listDetallePedido.Add(detalle);

                        DataRow newRowBebida = dtBebidas.NewRow();
                        newRowBebida["Id_tipo"] = pr.Id_producto;
                        newRowBebida["Tipo"] = pr.Tipo_producto;
                        newRowBebida["Nombre"] = nombre;
                        newRowBebida["Precio"] = pr.Precio;
                        newRowBebida["Cantidad"] = pr.Cantidad;
                        newRowBebida["Total"] = pr.Cantidad * pr.Precio;
                        newRowBebida["Observaciones"] = pr.Observaciones;
                        dtBebidas.Rows.Add(newRowBebida);
                    }

                    dtDetallePedido.Rows.Add(newRow);
                }
            }
            else
                dtDetallePedido = null;

            return true;
        }

        private async void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                MensajeEspera.ShowWait("Cargando...");
                if (this.Comprobaciones(out Pedidos pedido,
                                        out List<Detalle_pedido> listDetalle,
                                        out DataTable dtDetallePedido,
                                        out DataTable dtPlatos,
                                        out DataTable dtBebidas))
                {
                    DatosInicioSesion datos = DatosInicioSesion.GetInstancia();

                    string rpta = "OK";
                    int id_pedido = 0;

                    if (!this.IsEditar)
                    {
                        rpta = NPedido.InsertarPedido(pedido);
                        id_pedido = pedido.Id_pedido;
                    }
                    else
                    {
                        id_pedido = this.Pedido.Id_pedido;
                        //if (dtDetallePedido != null)
                        //{
                        //    foreach (DataRow row in dtDetallePedido.Rows)
                        //    {
                        //        Detalle_pedido de = new Detalle_pedido(row);
                        //        rpta = NPedido.ActualizarDetallePedido(de, datos.EmpleadoClaveMaestra.Id_empleado, "");
                        //        if (!rpta.Equals("OK"))
                        //        {
                        //            throw new Exception(rpta);
                        //        }
                        //    }
                        //}
                    }

                    if (rpta.Equals("OK"))
                    {
                        foreach (Detalle_pedido detalle in listDetalle)
                        {
                            detalle.Id_pedido = id_pedido;                         

                            rpta = NPedido.ActualizarDetallePedido(detalle, datos.EmpleadoClaveMaestra.Id_empleado, "");
                            if (!rpta.Equals("OK"))
                            {
                                throw new Exception(rpta);
                            }

                            if (detalle.ListDetalleIngredientes != null)
                            {
                                if (detalle.ListDetalleIngredientes.Count > 0)
                                {
                                    foreach (Detalle_ingredientes_pedido detalleIngrediente in detalle.ListDetalleIngredientes)
                                    {
                                        detalleIngrediente.Id_detalle_pedido = detalle.Id_detalle_pedido;
                                        detalleIngrediente.Id_pedido = detalle.Id_pedido;
                                        detalleIngrediente.Observaciones = string.Empty;
                                    }

                                    rpta = await NPedido.InsertarDetalleIngredientesPedido(detalle.ListDetalleIngredientes);
                                }
                            }
                        }

                        if (!rpta.Equals("OK"))
                        {
                            Mensajes.MensajeInformacion("No se ingresaron detalles de platos, operación interrumpida");
                        }

                        if (!this.IsDomicilio)
                        {
                            FrmObservarMesas FrmObservarMesas = FrmObservarMesas.GetInstancia();
                            FrmObservarMesas.ObtenerPedido(id_pedido, this.Numero_mesa, "PENDIENTE");
                        }

                        this.frmComandas.Id_pedido = id_pedido;

                        //if (this.IsEditar)
                        //{
                        //    if (dtDetallePedido != null)
                        //    {
                        //        this.frmComandas.Id_pedido = id_pedido;
                        //        this.frmComandas.AsignarTablas(dtDetallePedido);
                        //    }
                        //}
                        //else
                        //{
                        //    this.frmComandas.Id_pedido = id_pedido;                         
                        //    this.frmComandas.AsignarTablas();
                        //}

                        if (this.chkPrintComandas.Checked)
                        {
                            if (dtBebidas.Rows.Count > 0)
                            {
                                this.frmComandas.ObtenerReporte("Casa Grande | Bebidas");
                                this.frmComandas.AsignarTablas(dtBebidas);
                                this.frmComandas.ImprimirFactura((int)this.numericComandas.Value);
                            }

                            if (dtPlatos.Rows.Count > 0)
                            {
                                this.frmComandas.ObtenerReporte("Casa Grande | Platos");
                                this.frmComandas.AsignarTablas(dtPlatos);
                                this.frmComandas.ImprimirFactura((int)this.numericComandas.Value);
                            }
                        }
                        MensajeEspera.CloseForm();
                        this.Close();
                    }
                    else
                        throw new Exception(rpta);
                }
                MensajeEspera.CloseForm();
            }
            catch (Exception ex)
            {
                MensajeEspera.CloseForm();
                Mensajes.MensajeErrorCompleto(this.Name, "BtnSave_Click",
                    "Hubo un error guardando el pedido",
                    ex.Message);
            }
        }

        private void TxtBusqueda_OnTextoKeyPress(object sender, KeyPressEventArgs e)
        {
            CustomTextBox txt = (CustomTextBox)sender;
            if (e.KeyChar == (int)Keys.Enter)
            {
                if (this.optionSelected.Equals("PLATOS"))
                {
                    this.LoadPlatos("NOMBRE", txt.Texto);
                }
                else
                {
                    this.LoadBebidas("NOMBRE", txt.Texto);
                }
            }
        }

        private void FrmPedido_Load(object sender, EventArgs e)
        {
            DialogResult dialog = this.Comprobacion();
            if (dialog == DialogResult.OK)
            {
                MensajeEspera.ShowWait("Cargando...");
                this.optionSelected = "PLATOS";
                this.LoadTipoPlatos("COMPLETO", "");

                if (this.ListTipoPlatos != null)
                    this.LoadPlatos("ID TIPO PLATO", this.ListTipoPlatos[0].Id_tipo_plato.ToString());

                if (this.frmComandas == null)
                    this.frmComandas = new FrmComandas();

                this.frmComandas.ObtenerReporte("Casa grande comanda");

                if (this.IsEditar)
                {
                    this.LoadProductsSelected(this.ProductsSelected);
                }

                MensajeEspera.CloseForm();
            }
            else
                this.Close();
        }

        private void BtnBebidas_Click(object sender, EventArgs e)
        {
            this.optionSelected = "BEBIDAS";
            this.LoadTipoBebidas("COMPLETO", "");
        }

        private void BtnPlatos_Click(object sender, EventArgs e)
        {
            this.optionSelected = "PLATOS";
            this.LoadTipoPlatos("COMPLETO", "");
        }

        private void AddProduct(ProductBinding product,
            List<ProductDetalleBinding> detalles, bool isEditar)
        {
            //Si la lista de productos seleccionados está vacía la inicializamos
            if (this.ProductsSelected == null)
                this.ProductsSelected = new List<ProductBinding>();

            bool isNew = false;
            ProductBinding productNew = null;

            //Comprobar existencia del producto en la lista de ProductsSelected
            List<ProductBinding> productExiste =
                this.ProductsSelected.Where(x => x.Id_producto == product.Id_producto).ToList();
            if (productExiste.Count > 0)
            {
                /**Si existe el producto en la lista de productos haremos:
                 * 1- Verificar si es un plato
                 * 2- Obtener el plato desde el object de la entidad de producto
                 * 3- Si es un plato detallado, verificar si hay cambios en su detalle
                 * 4- Si hay cambios, agregar un item nuevo
                 * 5- Si no hay cambios agregar +1 al producto en la lista existente**/

                if (productExiste[0].Tipo_producto.Equals("PLATO"))
                {
                    CapaEntidades.Models.Platos plato =
                        (CapaEntidades.Models.Platos)productExiste[0].Product;
                    if (plato.Plato_detallado.Equals("ACTIVO"))
                    {
                        if (detalles == null)
                        {
                            Mensajes.MensajeInformacion("No se envío ningún detalle");
                            return;
                        }

                        if (productExiste[0].ProductDetalles == null)
                            productExiste[0].ProductDetalles = detalles;


                        foreach (ProductDetalleBinding pr1 in detalles)
                        {
                            List<ProductDetalleBinding> find =
                                productExiste[0].ProductDetalles.Where(x => x.Id_ingrediente == pr1.Id_ingrediente).ToList();
                            //QUIERE DECIR QUE NO HAY COINCIDENCIAS
                            if (find.Count == 0)
                            {
                                isNew = true;
                                break;
                            }
                        }

                        if (isNew)
                        {
                            //Si es new agregamos uno nuevo
                            productNew = new ProductBinding
                            {
                                Id_producto = product.Id_producto,
                                Nombre = product.Nombre,
                                Tipo_producto = product.Tipo_producto,
                                Precio = product.Precio,
                                Observaciones = string.Empty,
                                NombreImagen = product.NombreImagen,
                                ProductDetalles = detalles,
                                IsAddBD = true,
                                IsEditar = isEditar,
                                Cantidad = 1,
                                Product = plato,
                            };
                            this.ProductsSelected.Add(productNew);
                        }
                        else
                            productExiste[0].Cantidad += 1;
                    }
                    else
                        productExiste[0].Cantidad += 1;
                }
                else
                    productExiste[0].Cantidad += 1;
            }
            else
            {
                //Si no existe agregamos un producto nuevo a la lista de vista

                product.IsAddBD = true;
                product.IsEditar = isEditar;
                product.Cantidad = 1;

                if (product.ProductDetalles == null)
                    product.ProductDetalles = detalles;

                this.ProductsSelected.Add(product);
            }

            //Comprobar existencia del producto en ProductsAddSelected
            List<ProductBinding> productExisteAdd = new List<ProductBinding>();

            if (this.ProductsAddSelected == null)
                this.ProductsAddSelected = new List<ProductBinding>();

            productExisteAdd =
                this.ProductsAddSelected.Where(x => x.Id_producto == product.Id_producto).ToList();
            if (productExisteAdd.Count > 0)
            {
                /**Si existe el producto en la lista de productos haremos:
                 * 1- Agregar +1 al producto en la lista existente**/

                if (isNew && productNew != null)
                    this.ProductsAddSelected.Add(productNew);

                //productExisteAdd[0].Cantidad += 1;
                //this.ProductsAddSelected.Add(productExiste[0]);
            }
            else
            {
                //Si no existe agregamos un producto nuevo
                product.IsAddBD = true;
                product.IsEditar = isEditar;
                product.Cantidad = 1;
                if (product.ProductDetalles == null)
                    product.ProductDetalles = detalles;
                this.ProductsAddSelected.Add(product);
            }

            //Cargar los productos desde la lista de seleccionados general
            this.LoadProductsSelected(this.ProductsSelected);
        }

        private void RemoveProduct(ProductBinding product)
        {
            DatosInicioSesion datos = DatosInicioSesion.GetInstancia();
            /**Si el producto IsEditar y IsAddBD es false
             * Significa que el producto venía desde la BD**/
            if (product.IsEditar && product.IsAddBD == false)
            {
                DialogResult dialog = this.Comprobacion();
                if (dialog == DialogResult.OK)
                {
                    Detalle_pedido detalle = new Detalle_pedido
                    {
                        Id_pedido = this.Pedido.Id_pedido,
                        Id_tipo = product.Id_producto,
                        Tipo = product.Tipo_producto,
                        Precio = product.Precio,
                        Cantidad = product.Cantidad - 1,
                        Observaciones = product.Observaciones == null ? string.Empty : product.Observaciones,
                    };

                    //Eliminar de la base de datos
                    string rpta = NPedido.ActualizarDetallePedido(detalle, datos.EmpleadoClaveMaestra.Id_empleado, "DELETE");
                    if (!rpta.Equals("OK"))
                    {
                        Mensajes.MensajeInformacion("Hubo un error actualizando el detalle del pedido en la bd");
                    }
                }
                else
                    return;
            }

            //Comprobar existencia del producto en la lista de ProductsSelected
            List<ProductBinding> productExiste =
                this.ProductsSelected.Where(x => x.Id_producto == product.Id_producto).ToList();
            if (productExiste.Count > 0)
            {
                /**Si existe el producto en la lista de productos haremos:
                 * 1- Comprobar cuánta cantidad tiene el producto en la lista existente
                 * si la cantidad es 1 se quitará de la lista, si la cantidad es > 0 
                 * se restará -1.**/

                if (productExiste[0].Cantidad == 1)
                {
                    this.ProductsSelected.Remove(productExiste[0]);
                }
                else
                    productExiste[0].Cantidad -= 1;
            }
            else
            {
                //Si no existe, es porque hay algún error
                Mensajes.MensajeInformacion("Hubo un error eliminando el producto, " +
                    "no se encuentra en al lista de productos seleccionados");
            }

            //Comprobar existencia del producto en ProductsAddSelected
            if (this.ProductsAddSelected != null &&
                this.ProductsAddSelected.Count > 0)
            {
                productExiste =
                    this.ProductsAddSelected.Where(x => x.Id_producto == product.Id_producto).ToList();
                if (productExiste.Count > 0)
                {
                    /**Si existe el producto en la lista de productos haremos:
                     * 1- Comprobar cuánta cantidad tiene el producto en la lista existente
                     * si la cantidad es 1 se quitará de la lista, si la cantidad es > 0 
                     * se restará -1.**/
                    if (productExiste[0].Cantidad == 1)
                    {
                        this.ProductsAddSelected.Remove(productExiste[0]);
                    }
                    //else
                    //    productExiste[0].Cantidad -= 1;
                }
            }

            this.LoadProductsSelected(this.ProductsSelected);
        }

        private void LoadProductsSelected(List<ProductBinding> products)
        {
            try
            {
                this.panelPedido.clearDataSource();
                StringBuilder info = new StringBuilder();
                info.Append("Fecha y hora: ").Append(DateTime.Now.ToLongDateString());
                info.Append(" " + DateTime.Now.ToLongTimeString()).Append(Environment.NewLine);
                if (products != null)
                {
                    decimal subtotal = 0;
                    this.panelPedido.BackgroundImage = null;
                    List<UserControl> controls = new List<UserControl>();
                    foreach (ProductBinding pr in products)
                    {
                        subtotal += pr.Precio * pr.Cantidad;
                        ProductoItem productoItem = new ProductoItem
                        {
                            Product = pr,
                        };
                        productoItem.OnBtnAddClick += ProductoItem_OnBtnAddClick;
                        productoItem.OnBtnRemoveClick += ProductoItem_OnBtnRemoveClick;
                        controls.Add(productoItem);
                    }
                    info.Append("Subtotal: ").Append(subtotal.ToString("C"));
                    this.panelPedido.AddArrayControl(controls);

                    this.threadLoadImages = new Thread(new ThreadStart(() => LoadImagesProductsSelected()))
                    {
                        IsBackground = true
                    };
                    this.threadLoadImages.SetApartmentState(ApartmentState.STA);
                    this.threadLoadImages.Start();
                }
                else
                {
                    info.Append("Subtotal: ").Append(0.ToString("C"));
                    this.panelPedido.BackgroundImage = Resources.SIN_IMAGENES;
                    this.panelPedido.BackgroundImageLayout = ImageLayout.Center;
                }
                this.txtInfoPedido.Text = info.ToString();
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "LoadProductsSelected(List<ProductBinding> products)",
                    "Hubo un error al cargar los productos seleccionados", ex.Message);
            }
        }

        public void LoadPlatos(string tipo_busqueda, string texto_busqueda)
        {
            try
            {
                DataTable dtPlatos = NPlatos.BuscarPlatos(tipo_busqueda, texto_busqueda, "ACTIVO", out string rpta);
                this.panelResultados.clearDataSource();
                if (dtPlatos != null)
                {
                    this.panelResultados.BackgroundImage = null;
                    List<UserControl> controls = new List<UserControl>();
                    foreach (DataRow row in dtPlatos.Rows)
                    {
                        CapaEntidades.Models.Platos plato = new CapaEntidades.Models.Platos(row);

                        ProductBinding productBinding = new ProductBinding
                        {
                            Nombre = plato.Nombre_plato,
                            Id_producto = plato.Id_plato,
                            Tipo_producto = "PLATO",
                            Precio = plato.Precio_plato,
                            Observaciones = string.Empty,
                            Cantidad = 0,
                            Product = plato,
                            NombreImagen = plato.Imagen_plato,
                        };

                        ProductoItem productoItem = new ProductoItem
                        {
                            Product = productBinding,
                        };

                        productoItem.OnBtnAddClick += ProductoItem_OnBtnAddClick;

                        controls.Add(productoItem);
                    }
                    this.panelResultados.AddArrayControl(controls);

                    this.threadLoadImages = new Thread(new ThreadStart(() => LoadImages()))
                    {
                        IsBackground = true
                    };
                    this.threadLoadImages.SetApartmentState(ApartmentState.STA);
                    this.threadLoadImages.Start();
                }
                else
                {
                    this.panelResultados.BackgroundImage = Resources.SIN_IMAGENES;
                    this.panelResultados.BackgroundImageLayout = ImageLayout.Center;

                    if (!rpta.Equals("OK"))
                        throw new Exception(rpta);
                }
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "LoadPlatos(string tipo_busqueda, string texto_busqueda)",
                    "Hubo un error cargando los platos", ex.Message);
            }
        }

        public void LoadBebidas(string tipo_busqueda, string texto_busqueda)
        {
            try
            {
                MensajeEspera.ShowWait("Cargando...");
                DataTable dtBebidas = NBebidas.BuscarBebida(tipo_busqueda, texto_busqueda, "ACTIVO", out string rpta);
                this.panelResultados.clearDataSource();
                if (dtBebidas != null)
                {
                    this.panelResultados.BackgroundImage = null;
                    List<UserControl> controls = new List<UserControl>();
                    foreach (DataRow row in dtBebidas.Rows)
                    {
                        CapaEntidades.Models.Bebidas bebida = new CapaEntidades.Models.Bebidas(row);

                        ProductBinding productBinding = new ProductBinding
                        {
                            Nombre = bebida.Nombre_bebida,
                            Id_producto = bebida.Id_bebida,
                            Tipo_producto = "BEBIDA",
                            Precio = bebida.Precio_bebida,
                            Observaciones = string.Empty,
                            Cantidad = 0,
                            Product = bebida,
                            NombreImagen = bebida.Imagen
                        };

                        ProductoItem productoItem = new ProductoItem
                        {
                            Product = productBinding,
                        };

                        productoItem.OnBtnAddClick += ProductoItem_OnBtnAddClick;

                        controls.Add(productoItem);
                    }
                    this.panelResultados.AddArrayControl(controls);

                    this.threadLoadImages = new Thread(new ThreadStart(() => LoadImages()))
                    {
                        IsBackground = true
                    };
                    this.threadLoadImages.SetApartmentState(ApartmentState.STA);
                    this.threadLoadImages.Start();
                }
                else
                {
                    MensajeEspera.CloseForm();

                    this.panelResultados.BackgroundImage = Resources.SIN_IMAGENES;
                    this.panelResultados.BackgroundImageLayout = ImageLayout.Center;

                    if (!rpta.Equals("OK"))
                        throw new Exception(rpta);
                }

                MensajeEspera.CloseForm();
            }
            catch (Exception ex)
            {
                MensajeEspera.CloseForm();

                Mensajes.MensajeErrorCompleto(this.Name, "LoadBebidas(string tipo_busqueda, string texto_busqueda)",
                    "Hubo un error cargando las bebidas", ex.Message);
            }
        }

        private void LoadImages()
        {
            foreach (UserControl control in this.panelResultados.controlsUser)
            {
                if (control is ProductoItem product)
                {
                    Image img;
                    string nombreimagen = product.Product.NombreImagen;
                    if (!string.IsNullOrEmpty(nombreimagen))
                    {
                        img = Imagenes.ObtenerImagen("RUTAIMAGES", nombreimagen, out string ruta_destino);

                        if (img == null)
                            img = Resources.SIN_IMAGENES;

                        product.ImageProduct = img;
                    }
                    else
                    {
                        img = Resources.SIN_IMAGENES;
                        product.ImageProduct = img;
                    }
                }
            }

            if (this.threadLoadImages.IsAlive)
                this.threadLoadImages.Interrupt();
        }

        private void LoadImagesProductsSelected()
        {
            if (this.ProductsSelected != null)
            {
                foreach (UserControl control in this.panelPedido.controlsUser)
                {
                    if (control is ProductoItem product)
                    {
                        Image img;
                        string nombreimagen = product.Product.NombreImagen;
                        if (!string.IsNullOrEmpty(nombreimagen))
                        {
                            img = Imagenes.ObtenerImagen("RUTAIMAGES", nombreimagen, out string ruta_destino);

                            if (img == null)
                                img = Resources.SIN_IMAGENES;

                            product.ImageProduct = img;
                        }
                        else
                        {
                            img = Resources.SIN_IMAGENES;
                            product.ImageProduct = img;
                        }
                    }
                }

                if (this.threadLoadImages.IsAlive)
                    this.threadLoadImages.Interrupt();
            }
        }

        private void ProductoItem_OnBtnRemoveClick(object sender, EventArgs e)
        {
            ProductBinding product = (ProductBinding)sender;
            this.RemoveProduct(product);
        }

        private void ProductoItem_OnBtnAddClick(object sender, EventArgs e)
        {
            //Obtenemos y guardamos el producto con sus detalles
            ProductBinding product = (ProductBinding)sender;
            if (product.Tipo_producto.Equals("PLATO"))
            {
                CapaEntidades.Models.Platos plato =
                    (CapaEntidades.Models.Platos)product.Product;
                if (plato.Plato_detallado.Equals("ACTIVO"))
                {
                    /**Como abrimos los detalles, se debe de generar el evento guardar 
                      * para agregarlo correctamente a la lista**/
                    bool isCarta = plato.Plato_carta.Equals("ACTIVO") ? true : false;

                    FrmDetallePedidoPlato frmDetallePedidoPlato = new FrmDetallePedidoPlato
                    {
                        StartPosition = FormStartPosition.CenterScreen,
                        MaximizeBox = false,
                        MinimizeBox = false,
                        Product = product,
                        IsEnabledBebida = true,
                        IsEnabledSopa = !isCarta,
                    };
                    frmDetallePedidoPlato.OnBtnSaveClick += FrmDetallePedidoPlato_OnBtnSaveClick;
                    frmDetallePedidoPlato.ShowDialog();
                    return;
                }
            }

            //Si es diferente a plato, se guardará sin más.
            this.AddProduct(product, null, this.IsEditar);
        }

        private void FrmDetallePedidoPlato_OnBtnSaveClick(object sender, EventArgs e)
        {
            object[] objs = (object[])sender;
            //Obtenemos y guardamos el producto con sus detalles
            ProductBinding product = (ProductBinding)objs[0];
            List<ProductDetalleBinding> detalles = (List<ProductDetalleBinding>)objs[1];
            this.AddProduct(product, detalles, this.IsEditar);
        }

        public void LoadTipoPlatos(string tipo_busqueda, string texto_busqueda)
        {
            try
            {
                string rpta = "OK";
                DataTable dtTipos = NPlatos.BuscarTipoPlatos(tipo_busqueda, texto_busqueda);
                this.panelTipo.clearDataSource();
                this.ListTipoPlatos = null;
                if (dtTipos != null)
                {
                    this.ListTipoPlatos = new List<Tipo_platos>();
                    this.panelTipo.BackgroundImage = null;

                    List<UserControl> controls = new List<UserControl>();
                    foreach (DataRow row in dtTipos.Rows)
                    {
                        Tipo_platos tipo = new Tipo_platos(row);
                        this.ListTipoPlatos.Add(tipo);

                        TipoItem tipoItem = new TipoItem
                        {
                            Tipo = "PLATO",
                            TipoObject = tipo,
                            NombreTipo = tipo.Tipo_plato.ToUpper(),
                        };
                        tipoItem.OnBtnTipoClick += TipoItem_OnBtnTipoClick;
                        controls.Add(tipoItem);
                    }
                    this.panelTipo.AddArrayControl(controls);
                }
                else
                {
                    this.panelTipo.BackgroundImage = Resources.SIN_IMAGENES;
                    this.panelTipo.BackgroundImageLayout = ImageLayout.Center;

                    if (!rpta.Equals("OK"))
                        throw new Exception(rpta);
                }
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "LoadTipoPlatos(string tipo_busqueda, string texto_busqueda)",
                    "Hubo un error cargando los tipos de platos", ex.Message);
            }
        }

        public void LoadTipoBebidas(string tipo_busqueda, string texto_busqueda)
        {
            try
            {
                string rpta = "OK";
                DataTable dtTipos = NBebidas.BuscarTipoBebidas(tipo_busqueda, texto_busqueda);
                this.panelTipo.clearDataSource();
                this.ListTipoBebidas = null;
                if (dtTipos != null)
                {
                    this.ListTipoBebidas = new List<Tipo_bebidas>();
                    this.panelTipo.BackgroundImage = null;
                    List<UserControl> controls = new List<UserControl>();
                    foreach (DataRow row in dtTipos.Rows)
                    {
                        Tipo_bebidas tipo = new Tipo_bebidas(row);
                        this.ListTipoBebidas.Add(tipo);

                        TipoItem tipoItem = new TipoItem
                        {
                            Tipo = "BEBIDA",
                            TipoObject = tipo,
                            NombreTipo = tipo.Tipo_bebida.ToUpper(),
                        };
                        tipoItem.OnBtnTipoClick += TipoItem_OnBtnTipoClick;
                        controls.Add(tipoItem);
                    }
                    this.panelTipo.AddArrayControl(controls);
                }
                else
                {
                    this.panelTipo.BackgroundImage = Resources.SIN_IMAGENES;
                    this.panelTipo.BackgroundImageLayout = ImageLayout.Center;

                    if (!rpta.Equals("OK"))
                        throw new Exception(rpta);
                }
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "LoadTipoBebidas(string tipo_busqueda, string texto_busqueda)",
                    "Hubo un error cargando los tipos de bebidas", ex.Message);
            }
        }

        private void TipoItem_OnBtnTipoClick(object sender, EventArgs e)
        {
            TipoItem tipoItem = (TipoItem)sender;
            if (tipoItem.Tipo.Equals("BEBIDA"))
            {
                Tipo_bebidas tipo = (Tipo_bebidas)tipoItem.TipoObject;
                this.LoadBebidas("ID TIPO BEBIDA", tipo.Id_tipo_bebida.ToString());
            }
            else
            {
                Tipo_platos tipo = (Tipo_platos)tipoItem.TipoObject;
                this.LoadPlatos("ID TIPO PLATO", tipo.Id_tipo_plato.ToString());
            }
        }

        private void AsignarDatos(Pedidos pedido)
        {
            this.ClienteSelected = pedido.Cliente;
            //this.EmpleadoSelected = pedido.Empleado;
            this.MesaSelected = pedido.Mesa;

            this.lblMesero.Text = "Mesero/Empleado " + pedido.Empleado.Nombre_empleado;
            this.lblTitulo.Text = "Adicionar/Remover productos del pedido número " + pedido.Id_pedido;
            this.numericClientes.Value = pedido.CantidadClientes;
            this.IsEditar = true;

            //Obtener el detalle del pedido
            DataTable dtDatosPrincipales =
                NPedido.BuscarPedidosYDetalle("ID PEDIDO Y DETALLE", pedido.Id_pedido.ToString(),
                out DataTable dtDetalles,
                out DataTable dtDetallePlatosPedido, out string rpta);

            if (dtDetalles != null)
            {
                List<ProductBinding> products = new List<ProductBinding>();
                products = (from DataRow dr in dtDetalles.Rows
                            select new ProductBinding
                            {
                                Id_detalle_producto = Convert.ToInt32(dr["Id_detalle_pedido"]),
                                Nombre = Convert.ToString(dr["Nombre"]),
                                Precio = Convert.ToDecimal(dr["Precio"]),
                                Cantidad = Convert.ToInt32(dr["Cantidad"]),
                                Id_producto = Convert.ToInt32(dr["Id_tipo"]),
                                Tipo_producto = Convert.ToString(dr["Tipo"]),
                                Product = this.GetProduct(Convert.ToString(dr["Tipo"]),
                                Convert.ToInt32(dr["Id_tipo"]),
                                dtDetallePlatosPedido,
                                out string nombre_imagen,
                                out List<ProductDetalleBinding> detalles),
                                NombreImagen = nombre_imagen,
                                ProductDetalles = detalles,
                                IsEditar = true,
                                IsAddBD = false,
                            }).ToList();

                this.ProductsSelected = new List<ProductBinding>();
                this.ProductsSelected.AddRange(products);
            }
        }

        private Ingredientes GetIngrediente(int id_ingrediente)
        {
            DataTable dtIngrediente =
                NIngredientes.BuscarIngredientes("ID INGREDIENTE", id_ingrediente.ToString(), out string rpta);
            if (dtIngrediente != null)
            {
                return new Ingredientes(dtIngrediente.Rows[0]);
            }
            else
                return null;
        }

        private object GetProduct(string tipo_producto,
            int id_producto,
            DataTable dtDetallesPlatosPedido,
            out string nombre_imagen,
            out List<ProductDetalleBinding> detalles)
        {
            detalles = new List<ProductDetalleBinding>();
            nombre_imagen = string.Empty;
            if (tipo_producto.Equals("PLATO"))
            {
                DataTable dtPlato =
                    NPlatos.BuscarPlatos("ID PLATO", id_producto.ToString(), "ACTIVO", out string rpta);
                if (dtPlato != null)
                {
                    CapaEntidades.Models.Platos plato =
                        new CapaEntidades.Models.Platos(dtPlato.Rows[0]);
                    nombre_imagen = plato.Imagen_plato;
                    if (plato.Plato_detallado.Equals("ACTIVO"))
                    {
                        DataRow[] find =
                            dtDetallesPlatosPedido.Select(string.Format("Id_tipo = {0}", plato.Id_plato));
                        if (find.Length > 0)
                        {
                            detalles = new List<ProductDetalleBinding>();
                            detalles = (from DataRow dr in find
                                        select new ProductDetalleBinding
                                        {
                                            Id_detalle_ingrediente_pedido =
                                            Convert.ToInt32(dr["Id_detalle_ingrediente_pedido"]),
                                            Id_pedido =
                                            Convert.ToInt32(dr["Id_pedido"]),
                                            Id_tipo =
                                            Convert.ToInt32(dr["Id_tipo"]),
                                            Id_ingrediente =
                                            Convert.ToInt32(dr["Id_ingrediente"]),
                                            Ingrediente =
                                            this.GetIngrediente(Convert.ToInt32(dr["Id_ingrediente"])),
                                            Observaciones =
                                            Convert.ToString(dr["Observaciones"]),
                                        }).ToList();
                        }
                    }
                    return plato;
                }
                else
                    return null;
            }
            else if (tipo_producto.Equals("BEBIDA"))
            {
                DataTable dtBebida =
                    NBebidas.BuscarBebida("ID BEBIDA", id_producto.ToString(), "ACTIVO", out string rpta);
                if (dtBebida != null)
                {
                    CapaEntidades.Models.Bebidas bebida =
                        new CapaEntidades.Models.Bebidas(dtBebida.Rows[0]);
                    nombre_imagen = bebida.Imagen;
                    return bebida;
                }
                else
                    return null;
            }
            else
                return null;
        }

        #region PROPIEDADES Y VARIABLES
        private string _informacion;
        private bool _isEditar;
        private string _tipo_servicio;
        private string optionSelected = string.Empty;
        private Pedidos _pedido;
        public Pedidos Pedido
        {
            get => _pedido;
            set
            {
                _pedido = value;
                this.AsignarDatos(value);
            }
        }

        Thread threadLoadImages;
        //PoperContainer container;

        public Clientes ClienteSelected { get; set; }
        public Empleados EmpleadoSelected { get; set; }
        public Mesas MesaSelected { get; set; }

        public FrmComandas frmComandas;

        public List<Tipo_platos> ListTipoPlatos { get; set; }
        public List<Tipo_bebidas> ListTipoBebidas { get; set; }
        public List<ProductBinding> ProductsSelected { get; set; }
        public List<ProductBinding> ProductsAddSelected { get; set; }
        public string Informacion
        {
            get => _informacion;
            set
            {
                _informacion = value;
                this.txtInfoPedido.Text = value;
            }
        }
        public bool IsEditar
        {
            get => _isEditar;
            set
            {
                _isEditar = value;
                this.numericClientes.Visible = false;
                this.gbNumClientes.Visible = false;
            }
        }
        public bool IsDomicilio { get; set; }
        public int Numero_mesa { get; set; }
        public string Tipo_servicio
        {
            get => _tipo_servicio;
            set
            {
                _tipo_servicio = value;
                this.lblTitulo.Text = "Nuevo pedido para la mesa " + Numero_mesa;
            }
        }

        #endregion
    }
}
