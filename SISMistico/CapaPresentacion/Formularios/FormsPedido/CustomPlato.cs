namespace CapaPresentacion.Formularios.FormsPedido
{
    using CapaNegocio;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    public partial class CustomPlato : UserControl
    {
        public CustomPlato()
        {
            InitializeComponent();
            this.btnSave.Click += BtnSave_Click;
        }

        public event EventHandler OnBtnSaveClick;

        private bool Comprobaciones(out List<ProductDetalleBinding> detalles)
        {
            detalles = new List<ProductDetalleBinding>();

            //OBTENER EL ID DE LA SOPA O CREMA
            if (!int.TryParse(Convert.ToString(this.listaSopa.SelectedValue), out int id_sopa))
            {
                Mensajes.MensajeInformacion("Compruebe la sopa/crema seleccionada");
                return false;
            }
            else
            {
                //OBTENER LA ENTIDAD DE LA SOPA
                DataRow[] rows =
                    this.DtIngredientes.Select(string.Format("Id_ingrediente = {0}", id_sopa));
                if (rows.Length > 0)
                {
                    detalles.Add(new ProductDetalleBinding
                    {
                        Id_ingrediente = id_sopa,
                        Ingrediente = new CapaEntidades.Models.Ingredientes(rows[0]),
                        Observaciones = string.Empty,
                    });
                }
                else
                {
                    Mensajes.MensajeInformacion("Hubo un error obteniendo el tipo de ingrediente");
                    return false;
                }
            }

            //OBTENER EL ID DEL ARROZ
            if (!int.TryParse(Convert.ToString(this.listaArroz.SelectedValue), out int id_arroz))
            {
                Mensajes.MensajeInformacion("Compruebe el arroz seleccionado");
                return false;
            }
            else
            {
                //OBTENER LA ENTIDAD DEL ARROZ
                DataRow[] rows =
                    this.DtIngredientes.Select(string.Format("Id_ingrediente = {0}", id_arroz));
                if (rows.Length > 0)
                {
                    detalles.Add(new ProductDetalleBinding
                    {
                        Id_ingrediente = id_arroz,
                        Ingrediente = new CapaEntidades.Models.Ingredientes(rows[0]),
                        Observaciones = string.Empty,
                    });
                }
                else
                {
                    Mensajes.MensajeInformacion("Hubo un error obteniendo el tipo de ingrediente");
                    return false;
                }
            }

            //OBTENER EL ID DEL ACOMPAÑANTE
            if (!int.TryParse(Convert.ToString(this.listaAcompanante.SelectedValue), out int id_acompanante))
            {
                Mensajes.MensajeInformacion("Compruebe el acompañante seleccionado");
                return false;
            }
            else
            {
                //OBTENER LA ENTIDAD DEL ACOMPAÑANTE
                DataRow[] rows =
                    this.DtIngredientes.Select(string.Format("Id_ingrediente = {0}", id_acompanante));
                if (rows.Length > 0)
                {
                    detalles.Add(new ProductDetalleBinding
                    {
                        Id_ingrediente = id_acompanante,
                        Ingrediente = new CapaEntidades.Models.Ingredientes(rows[0]),
                        Observaciones = string.Empty,
                    });
                }
                else
                {
                    Mensajes.MensajeInformacion("Hubo un error obteniendo el tipo de ingrediente");
                    return false;
                }
            }

            //OBTENER EL ID DE LA PROTEINA
            //if (!int.TryParse(Convert.ToString(this.listaProteina.SelectedValue), out int id_proteina))
            //{
            //    Mensajes.MensajeInformacion("Compruebe la proteína seleccionada");
            //    return false;
            //}
            //else
            //{
            //    //OBTENER LA ENTIDAD DE LA PROTEINA
            //    DataRow[] rows =
            //        this.DtIngredientes.Select(string.Format("Id_ingrediente = {0}", id_proteina));
            //    if (rows.Length > 0)
            //    {
            //        detalles.Add(new ProductDetalleBinding
            //        {
            //            Id_ingrediente = id_proteina,
            //            Ingrediente = new CapaEntidades.Models.Ingredientes(rows[0]),
            //            Observaciones = string.Empty,
            //        });
            //    }
            //    else
            //    {
            //        Mensajes.MensajeInformacion("Hubo un error obteniendo el tipo de ingrediente");
            //        return false;
            //    }
            //}

            //OBTENER EL ID DE LA BEBIDA
            if (!int.TryParse(Convert.ToString(this.listaBebidas.SelectedValue), out int id_bebida))
            {
                Mensajes.MensajeInformacion("Compruebe la bebida seleccionada");
                return false;
            }
            else
            {
                //OBTENER LA ENTIDAD DE LA BEBIDA
                DataRow[] rows =
                    this.DtIngredientes.Select(string.Format("Id_ingrediente = {0}", id_bebida));
                if (rows.Length > 0)
                {
                    detalles.Add(new ProductDetalleBinding
                    {
                        Id_ingrediente = id_bebida,
                        Ingrediente = new CapaEntidades.Models.Ingredientes(rows[0]),
                        Observaciones = string.Empty,
                    });
                }
                else
                {
                    Mensajes.MensajeInformacion("Hubo un error obteniendo el tipo de ingrediente");
                    return false;
                }
            }

            return true;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (this.Comprobaciones(out List<ProductDetalleBinding> detalles))
            {
                this.OnBtnSaveClick?.Invoke(detalles, e);
            }
        }

        public async Task LoadIngredientes()
        {
            try
            {
                var (rpta, dtIngredientes) = 
                    await NIngredientes.BuscarIngredientes("COMPLETO", "");
                if (dtIngredientes != null)
                {
                    this.DtIngredientes = dtIngredientes;
                    this.listaSopa.DataSource = null;
                    this.listaBebidas.DataSource = null;
                    this.listaArroz.DataSource = null;
                    this.listaAcompanante.DataSource = null;

                    //Cargar el arroz
                    DataView view = new DataView(dtIngredientes);
                    view.RowFilter = string.Format("Tipo_ingrediente = '{0}'", "ARROZ");
                    if (view.Count > 1)
                    {
                        this.listaArroz.DataSource = view;
                        this.listaArroz.ValueMember = "Id_ingrediente";
                        this.listaArroz.DisplayMember = "Nombre_ingrediente";
                        view = null;
                    }

                    //Cargar las sopas/cremas
                    view = new DataView(dtIngredientes);
                    view.RowFilter = string.Format("Tipo_ingrediente = '{0}'", "SOPA");
                    if (view.Count > 1)
                    {
                        this.listaSopa.DataSource = view;
                        this.listaSopa.ValueMember = "Id_ingrediente";
                        this.listaSopa.DisplayMember = "Nombre_ingrediente";
                        view = null;
                    }

                    //Cargar las acompañantes
                    view = new DataView(dtIngredientes);
                    view.RowFilter = string.Format("Tipo_ingrediente = '{0}'", "ACOMPANANTE");
                    if (view.Count > 1)
                    {
                        this.listaAcompanante.DataSource = view;
                        this.listaAcompanante.ValueMember = "Id_ingrediente";
                        this.listaAcompanante.DisplayMember = "Nombre_ingrediente";
                        view = null;
                    }

                    //Cargar las proteinas
                    //view = new DataView(dtIngredientes);
                    //view.RowFilter = string.Format("Tipo_ingrediente = '{0}'", "PROTEINA");
                    //if (view.Count > 1)
                    //{
                    //    this.listaProteina.DataSource = view;
                    //    this.listaProteina.ValueMember = "Id_ingrediente";
                    //    this.listaProteina.DisplayMember = "Nombre_ingrediente";
                    //    view = null;
                    //}

                    //Cargar las bebidas
                    view = new DataView(dtIngredientes);
                    view.RowFilter = string.Format("Tipo_ingrediente = '{0}'", "BEBIDA");
                    if (view.Count > 1)
                    {
                        this.listaBebidas.DataSource = view;
                        this.listaBebidas.ValueMember = "Id_ingrediente";
                        this.listaBebidas.DisplayMember = "Nombre_ingrediente";
                        view = null;
                    }
                }
                else
                    if (!rpta.Equals("OK"))
                    throw new Exception(rpta);
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "LoadIngredientes",
                    "Hubo un error cargando los ingredientes", ex.Message);
            }
        }

        private async void AsignarDatos(ProductDetalleBinding productDetalle)
        {
            await this.LoadIngredientes();
        }

        private ProductDetalleBinding _productDetalle;
        private bool _isEditar;
        private bool _isEnabledBebida;
        private bool _isEnabledSopa;

        public ProductDetalleBinding ProductDetalle
        {
            get => _productDetalle;
            set
            {
                _productDetalle = value;
                this.AsignarDatos(value);
            }
        }

        public bool IsEditar
        {
            get => _isEditar;
            set
            {
                _isEditar = value;                
            }
        }

        public DataTable DtIngredientes { get; set; }

        public bool IsEnabledBebida
        {
            get => _isEnabledBebida;
            set
            {
                _isEnabledBebida = value;
            }
        }
        public bool IsEnabledSopa
        {
            get => _isEnabledSopa;
            set
            {
                _isEnabledSopa = value;
            }
        }
    }
}
