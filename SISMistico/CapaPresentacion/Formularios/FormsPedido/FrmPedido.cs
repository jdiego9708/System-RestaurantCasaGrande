using CapaEntidades.Models;
using CapaNegocio;
using CapaPresentacion.Properties;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CapaPresentacion.Formularios.FormsPedido
{
    public partial class FrmPedido : Form
    {
        public FrmPedido()
        {
            InitializeComponent();
        }

        public void LoadPlatos(string tipo_busqueda, string texto_busqueda)
        {
            try
            {
                DataTable dtPlatos = NPlatos.BuscarPlatos(tipo_busqueda, texto_busqueda, "ACTIVO", out string rpta);
                this.panelResultados.clearDataSource();
                if (dtPlatos != null)
                {
                    //this.panelResultados.BackgroundImage = null;
                    //List<UserControl> controls = new List<UserControl>();
                    //foreach (DataRow row in dtPlatos.Rows)
                    //{
                    //    Platos plato = new Platos(row);

                    //    StringBuilder info = new StringBuilder();
                    //    info.Append("Nombre: " + plato.Nombre_plato + "").Append(Environment.NewLine);
                    //    info.Append("Precio: " + plato.Precio_plato.ToString("C") + " ");

                    //    ProductoItem productoItem = new ProductoItem
                    //    {
                    //        TipoProducto = plato,
                    //        Tipo = "PLATO",
                    //        Id_tipo = plato.Id_plato,
                    //        Informacion = info.ToString(),
                    //        RutaImage = plato.Imagen_plato,
                    //    };
                    //    productoItem.OnBtnNext += ProductoItem_OnBtnNext;
                    //    controls.Add(productoItem);
                    //}
                    //this.panelResultados.AddArrayControl(controls);

                    //this.threadLoadImages = new Thread(new ThreadStart(() => LoadImages()))
                    //{
                    //    IsBackground = true
                    //};
                    //this.threadLoadImages.SetApartmentState(ApartmentState.STA);
                    //this.threadLoadImages.Start();
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
                    //this.panelResultados.BackgroundImage = null;
                    //List<UserControl> controls = new List<UserControl>();
                    //foreach (DataRow row in dtBebidas.Rows)
                    //{
                    //    Bebidas
                    //    StringBuilder info = new StringBuilder();
                    //    info.Append("Nombre: " + bebida.Nombre_bebida + "").Append(Environment.NewLine);
                    //    info.Append("Precio: " + bebida.Precio_bebida.ToString("C") + " ");
                    //    if (bebida.Precio_trago != 0)
                    //        info.Append("- Precio trago: " + bebida.Precio_trago.ToString("C"));

                    //    ProductoItem productoItem = new ProductoItem
                    //    {
                    //        TipoProducto = bebida,
                    //        Tipo = "BEBIDA",
                    //        Id_tipo = bebida.Id_bebida,
                    //        Informacion = info.ToString(),
                    //        RutaImage = bebida.Imagen,
                    //    };
                    //    productoItem.OnBtnNext += ProductoItem_OnBtnNext;
                    //    controls.Add(productoItem);
                    //}
                    //this.panelResultados.AddArrayControl(controls);

                    //this.threadLoadImages = new Thread(new ThreadStart(() => LoadImages()))
                    //{
                    //    IsBackground = true
                    //};
                    //this.threadLoadImages.SetApartmentState(ApartmentState.STA);
                    //this.threadLoadImages.Start();
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

        #region PROPIEDADES Y VARIABLES
        private string _informacion;
        private bool _isEditar;
        private string _tipo_servicio;

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

        private void AsignarDatos(Pedidos pedido)
        {
            
        }

        public Clientes ClienteSelected { get; set; }
        public Empleados EmpleadoSelected { get; set; }
        public Mesas MesaSelected { get; set; }

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
        public bool IsEditar { get => _isEditar; set => _isEditar = value; }
        public bool IsDomicilio { get; set; }
        public string Tipo_servicio
        {
            get => _tipo_servicio;
            set => _tipo_servicio = value;
        }


        #endregion
    }
}
