using CapaNegocio;
using CapaPresentacion.Formularios.FormsPedido.Bebidas;
using CapaPresentacion.Formularios.FormsPedido.Platos;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;

namespace CapaPresentacion.Formularios.FormsPedido
{
    public partial class FrmRealizarPedido : Form
    {
        public FrmRealizarPedido()
        {
            InitializeComponent();

            this.frmPedidoPlatos = new FrmPedidoPlatos();
            this.frmPedidoBebidas = new FrmPedidoBebidas();

            this.contextMenuDatosPedido = new ContextMenuDatosPedido();
            this.containerDatosPedido = new PoperContainer(contextMenuDatosPedido);

            this.frmPedidoPlatos.ondgvPlatosDoubleClick += FrmPedidoPlatos_ondgvPlatosDoubleClick;
            this.frmPedidoBebidas.ondgvBebidasDoubleClick += FrmPedidoBebidas_ondgvBebidasDoubleClick;
            this.Load += FrmRealizarPedido_Load;
            this.btnInformacionPedido.Click += BtnInformacionPedido_Click;
            this.btnBebidas.Click += BtnBebidas_Click;
            this.btnPlatos.Click += BtnPlatos_Click;
            this.btnQuitar.Click += BtnQuitar_Click;
            this.btnQuitarProductosEditado.Click += BtnQuitarProductosEditado_Click;
            this.contextMenuDatosPedido.btnTerminarPedido.Click += BtnTerminarPedido_Click;
            this.dgvProductos.DoubleClick += DgvProductos_DoubleClick;
            this.dgvProductosEditar.DoubleClick += DgvProductosEditar_DoubleClick;
        }

        public event EventHandler OnPedidoDomicilioSuccess;

        private FrmComandas comandas = new FrmComandas();

        private void BtnQuitarProductosEditado_Click(object sender, EventArgs e)
        {
            if (this.dgvProductosEditar.CurrentRow != null)
            {
                if (this.dgvProductosEditar.CurrentRow != null)
                {
                    int id_tipo =
                        Convert.ToInt32(dgvProductosEditar.CurrentRow.Cells["Id_tipo"].Value);
                    string tipo =
                        Convert.ToString(dgvProductosEditar.CurrentRow.Cells["Tipo"].Value);
                    if (tipo.Equals("PLATO"))
                    {
                        this.tablasPedido.RemoverPlato(id_tipo, this.Id_pedido, this.IsEditar, this.Id_empleado);
                    }
                    else
                    {
                        this.tablasPedido.RemoverBebida(id_tipo, this.Id_pedido, this.IsEditar, this.Id_empleado);
                    }
                    this.ActualizarProductos();
                }
            }
        }

        private void BtnQuitar_Click(object sender, EventArgs e)
        {
            if (this.IsEditar)
            {
                DialogResult dialog = this.Comprobacion();
                if (this.Cargo_empleado.Equals("ADMINISTRADOR") & dialog == DialogResult.OK)
                {
                    if (this.dgvProductos.CurrentRow != null)
                    {
                        int id_tipo =
                            Convert.ToInt32(dgvProductos.CurrentRow.Cells["Id_tipo"].Value);
                        string tipo =
                            Convert.ToString(dgvProductos.CurrentRow.Cells["Tipo"].Value);
                        if (tipo.Equals("PLATO"))
                        {
                            this.tablasPedido.RemoverPlato(id_tipo, this.Id_pedido, !this.IsEditar, this.Id_empleado);
                        }
                        else
                        {
                            this.tablasPedido.RemoverBebida(id_tipo, this.Id_pedido, !this.IsEditar, this.Id_empleado);
                        }

                        DatosInicioSesion datosInicioSesion = DatosInicioSesion.GetInstancia();                  
                        string rpta = NPedido.InsertarEliminacionComanda(this.Id_pedido, this.Id_empleado, datosInicioSesion.Id_empleado, id_tipo, tipo, "");
                        if (!rpta.Equals("OK"))
                            Mensajes.MensajeInformacion("No se pudo guardar la eliminación de la comanda", "Entendido");

                        this.ActualizarProductos();
                    }
                }
                else
                {
                    Mensajes.MensajeInformacion("No tiene permisos para realizar esta acción", "Entendido");
                }
            }
            else
            {
                if (this.dgvProductos.CurrentRow != null)
                {
                    int id_tipo =
                        Convert.ToInt32(dgvProductos.CurrentRow.Cells["Id_tipo"].Value);
                    string tipo =
                        Convert.ToString(dgvProductos.CurrentRow.Cells["Tipo"].Value);
                    if (tipo.Equals("PLATO"))
                    {
                        this.tablasPedido.RemoverPlato(id_tipo, this.Id_pedido, this.IsEditar, this.Id_empleado);
                    }
                    else
                    {
                        this.tablasPedido.RemoverBebida(id_tipo, this.Id_pedido, this.IsEditar, this.Id_empleado);
                    }
                    this.ActualizarProductos();
                }
            }
        }

        private void DgvProductosEditar_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                if (this.dgvProductosEditar.CurrentRow != null)
                {
                    int id_tipo = Convert.ToInt32(this.dgvProductosEditar.CurrentRow.Cells["Id_tipo"].Value);
                    string observaciones = Convert.ToString(this.dgvProductosEditar.CurrentRow.Cells["Observaciones"].Value);
                    string tipo = Convert.ToString(this.dgvProductosEditar.CurrentRow.Cells["Tipo"].Value);
                    FrmObservacionPedido frm = new FrmObservacionPedido();
                    frm.StartPosition = FormStartPosition.CenterScreen;
                    frm.Id_tipo = id_tipo;
                    frm.Tipo = tipo;
                    frm.Observacion = observaciones;
                    frm.FormClosed += Frm_FormClosed;
                    frm.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "DgvProductosEditar_DoubleClick",
                    "Hubo un error con la tabla de datos", ex.Message);
            }
        }

        private void DgvProductos_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                if (this.dgvProductos.CurrentRow != null)
                {
                    int id_tipo = Convert.ToInt32(this.dgvProductos.CurrentRow.Cells["Id_tipo"].Value);
                    string observaciones = Convert.ToString(this.dgvProductos.CurrentRow.Cells["Observaciones"].Value);
                    string tipo = Convert.ToString(this.dgvProductos.CurrentRow.Cells["Tipo"].Value);
                    FrmObservacionPedido frm = new FrmObservacionPedido();
                    frm.StartPosition = FormStartPosition.CenterScreen;
                    frm.Id_tipo = id_tipo;
                    frm.Tipo = tipo;
                    frm.Observacion = observaciones;
                    frm.FormClosed += Frm_FormClosed;
                    frm.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "DgvProductos_DoubleClick",
                    "Hubo un error con la tabla de datos", ex.Message);
            }
        }

        private void Frm_FormClosed(object sender, FormClosedEventArgs e)
        {
            FrmObservacionPedido frm = (FrmObservacionPedido)sender;
            if (frm.DialogResult == DialogResult.OK)
            {
                string rpta;
                this.tablasPedido.AddObservacion(frm.Id_tipo, frm.Observacion, frm.Tipo, out rpta);
                if (!rpta.Equals("OK"))
                {
                    Mensajes.MensajeErrorForm("No se pudo agregar la observación");
                }
                else
                {
                    this.ActualizarProductos();
                }
            }
        }

        private void BtnTerminarPedido_Click(object sender, EventArgs e)
        {
            MensajeEspera.ShowWait("Facturando y terminando");
            try
            {
                this.containerDatosPedido.Close();
                string rpta = "";
                int id_pedido;
                if (this.Comprobaciones())
                {
                    DataTable TablaDetallePedido = this.tablasPedido.dtDetallePedido();

                    if (TablaDetallePedido.Rows.Count > 0)
                    {
                        //rpta =
                        //    NPedido.InsertarPedido(this.Variables(),
                        //    TablaDetallePedido, 
                        //    out id_pedido,
                        //    out _);
                        if (rpta.Equals("OK"))
                        {
                            if (!this.IsDomicilio)
                            {
                                FrmObservarMesas FrmObservarMesas = FrmObservarMesas.GetInstancia();
                                FrmObservarMesas.ObtenerPedido(0, this.Numero_mesa, "PENDIENTE");
                            }

                            this.comandas.Id_pedido = 0;
                            this.comandas.AsignarTablas();

                            if (this.contextMenuDatosPedido.chkImprimirPedido.Checked)
                            {
                                bool plato = false;
                                bool bebida = false;
                                int imprimir = 0;
                                foreach (DataRow row in TablaDetallePedido.Rows)
                                {
                                    if (row["Tipo"].Equals("PLATO"))
                                    {
                                        plato = true;
                                    }
                                    else
                                    {
                                        bebida = true;
                                    }
                                    if (plato && bebida)
                                    {
                                        break;
                                    }
                                }

                                if (plato && bebida)
                                {
                                    imprimir = 1;
                                }
                                else
                                {
                                    imprimir = 1;
                                }

                                comandas.ImprimirFactura(imprimir);
                            }

                            if (this.IsDomicilio)
                            {
                                this.OnPedidoDomicilioSuccess?.Invoke(0, e);
                            }


                            this.Close();
                        }
                        else
                        {
                            throw new Exception(rpta);
                        }
                    }

                }
                MensajeEspera.CloseForm();
            }
            catch (Exception ex)
            {
                MensajeEspera.CloseForm();
                Mensajes.MensajeErrorCompleto(this.Name, "BtnTerminarPedido_Click",
                    "Hubo un error al terminar un pedido", ex.Message);
            }
        }

        private List<string> Variables()
        {
            string tipo_pedido = string.Empty;

            if (this.IsDomicilio)
            {
                tipo_pedido = "DOMICILIO";
            }
            else
            {
                tipo_pedido = "MESA";
            }

            return new List<string>
            {
                Convert.ToString(this.Id_mesa), Convert.ToString(this.Id_empleado),
                Convert.ToString(this.contextMenuDatosPedido.txtCliente.Tag), "0", tipo_pedido, "",
            };
        }

        private bool Comprobaciones()
        {
            bool result = true;
            if (this.dgvProductos.Rows.Count < 1)
            {
                Mensajes.MensajeErrorForm("Debe seleccionar un plato o bebida");
                result = false;
            }

            if (!IsEditar)
            {
                if (Convert.ToString(this.contextMenuDatosPedido.txtCliente.Tag).Equals("0"))
                {
                    result = true;
                    //DialogResult dialogResult;
                    //Mensajes.MensajePregunta("No ha seleccionado un cliente, ¿desea continuar?",
                    //    "Continuar", "Cancelar", out dialogResult);
                    //if (dialogResult == DialogResult.Yes)
                    //{
                    //    result = true;
                    //}
                    //else
                    //{
                    //    result = false;
                    //}
                }
            }
            return result;
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

        private void SumarPrecios()
        {
            int total_parcial = 0;
            DataTable Tabla = (DataTable)this.dgvProductos.DataSource;
            if (Tabla != null)
            {
                foreach (DataRow row in Tabla.Rows)
                {
                    total_parcial += Convert.ToInt32(row["Total"]);
                }
            }
            this.Total_parcial = total_parcial;
            this.contextMenuDatosPedido.Total_parcial = this.Total_parcial;
        }

        private void BtnInformacionPedido_Click(object sender, EventArgs e)
        {
            if (this.IsEditar)
            {
                bool result = this.dgvProductosEditar.Rows.Count > 0 ? true : false;
                if (result)
                {
                    MensajeEspera.ShowWait("Imprimiendo y finalizando");
                    DataTable TablaDetallePedido = this.tablasPedido.dtDetallePedidoEditado();
                    if (this.dgvProductosEditar.Rows.Count > 0)
                    {
                        comandas.Id_pedido = this.Id_pedido;
                        comandas.AsignarTablas(TablaDetallePedido);
                        this.tablasPedido.IsEditar = false;
                        bool plato = false;
                        bool bebida = false;
                        int imprimir = 0;
                        string rpta = "";
                        foreach (DataRow row in TablaDetallePedido.Rows)
                        {
                            if (row["Tipo"].Equals("PLATO"))
                            {
                                //rpta = NPedido.ActualizarDetallePedido(new List<string> { Convert.ToString(this.Id_pedido),
                                //        Convert.ToString(row["Id_tipo"]), "PLATO", Convert.ToString(row["Precio"]),
                                //        Convert.ToString(row["Cantidad"]), Convert.ToString(row["Observaciones"]), "0", "SUMAR"});
                                plato = true;
                            }
                            else
                            {
                                //rpta = NPedido.ActualizarDetallePedido(new List<string> { Convert.ToString(this.Id_pedido),
                                //        Convert.ToString(row["Id_tipo"]), "BEBIDA", Convert.ToString(row["Precio"]),
                                //        Convert.ToString(row["Cantidad"]), Convert.ToString(row["Observaciones"]), "0", "SUMAR"});
                                bebida = true;
                            }
                            if (plato && bebida)
                            {
                                break;
                            }
                        }
                        if (this.chkImprimirComandas.Checked)
                        {
                            if (plato && bebida)
                            {
                                imprimir = 1;
                            }
                            else
                            {
                                imprimir = 1;
                            }

                            comandas.ImprimirFactura(imprimir);
                        }
                    }

                    MensajeEspera.CloseForm();
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            else
            {
                this.contextMenuDatosPedido.IsEditar = false;
                this.SumarPrecios();
                this.containerDatosPedido.Show(btnInformacionPedido);
            }
        }

        private void ActualizarProductos()
        {
            this.dgvProductos.DataSource =
                this.tablasPedido.dtVistaPedido();
            if (this.dgvProductos.DataSource != null)
            {
                bool[] columns_visible =
                {
                    false, true, true, true, true, true
                };
                this.dgvProductos = DatagridString.ChangeColumnsVisible(this.dgvProductos, columns_visible);
            }

            if (this.IsEditar)
            {
                this.dgvProductosEditar.DataSource =
                    this.tablasPedido.dtDetallePedidoEditado();
                bool[] columns_visible =
                {
                    false, true, true, true, true, true
                };
                this.dgvProductosEditar = DatagridString.ChangeColumnsVisible(this.dgvProductosEditar, columns_visible);
            }
        }

        private void FrmPedidoPlatos_ondgvPlatosDoubleClick(object sender, EventArgs e)
        {
            this.tablasPedido.AgregarPlato(this.frmPedidoPlatos.Id_plato, this.frmPedidoPlatos.Precio_cobrar, 1, "", this.IsEditar);
            this.ActualizarProductos();
        }

        private void FrmPedidoBebidas_ondgvBebidasDoubleClick(object sender, EventArgs e)
        {
            this.tablasPedido.AgregarBebida(this.frmPedidoBebidas.Id_bebida, this.frmPedidoBebidas.Precio_cobrar, 1, "", this.IsEditar);
            this.ActualizarProductos();
        }

        private Form ComprobarExistencia(Form form)
        {
            Form frmDevolver = null;
            foreach (Form frm in this.panel2.Controls)
            {
                if (frm.Name.Equals(form.Name))
                {
                    frmDevolver = frm;
                    break;
                }

            }
            return frmDevolver;
        }

        private void BtnPlatos_Click(object sender, EventArgs e)
        {
            try
            {
                this.panel2.Controls.Clear();
                frmPedidoPlatos.Text = "PLATOS";
                frmPedidoPlatos.TopLevel = false;
                frmPedidoPlatos.FormBorderStyle = FormBorderStyle.None;
                frmPedidoPlatos.Dock = DockStyle.Fill;

                Form FormComprobado = this.ComprobarExistencia(frmPedidoPlatos);

                if (FormComprobado != null)
                {
                    frmPedidoPlatos.WindowState = FormWindowState.Normal;
                    frmPedidoPlatos.Activate();
                }
                else
                {
                    this.panel2.Controls.Add(frmPedidoPlatos);
                    frmPedidoPlatos.Show();
                }
                frmPedidoPlatos.BringToFront();
                frmPedidoPlatos.Activate();
                this.panel2.Tag = frmPedidoPlatos.Name;
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "BtnPlatos_Click",
                    "Hubo un error con el menu observar platos", ex.Message);
            }
        }

        private void BtnBebidas_Click(object sender, EventArgs e)
        {
            try
            {
                this.panel2.Controls.Clear();
                frmPedidoBebidas.Text = "BEBIDAS";
                frmPedidoBebidas.TopLevel = false;
                frmPedidoBebidas.FormBorderStyle = FormBorderStyle.None;
                frmPedidoBebidas.Dock = DockStyle.Fill;
                Form FormComprobado = this.ComprobarExistencia(frmPedidoBebidas);
                if (FormComprobado != null)
                {
                    frmPedidoBebidas.WindowState = FormWindowState.Normal;
                    frmPedidoBebidas.Activate();
                }
                else
                {
                    this.panel2.Controls.Add(frmPedidoBebidas);
                    frmPedidoBebidas.Show();
                }
                frmPedidoBebidas.BringToFront();
                frmPedidoBebidas.Activate();
                this.panel2.Tag = frmPedidoBebidas.Name;
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "BtnBebidas_Click",
                    "Hubo un error con el menu observar bebidas", ex.Message);
            }
        }

        private void FrmRealizarPedido_Load(object sender, EventArgs e)
        {
            DialogResult dialog = this.Comprobacion();

            if (dialog == DialogResult.OK)
            {
                //this.comandas.ObtenerReporte();

                if (this.IsDomicilio)
                    this.lblMistico.Text =
                    "Realizar un nuevo domicilio";
                else
                    this.lblMistico.Text =
                    "Realizar un nuevo pedido para la mesa " + this.Numero_mesa;

                if (this.IsEditar)
                {
                    try
                    {
                        string rpta;
                        DataTable dtDetalle;
                        DataTable dtPedido =
                            NPedido.BuscarPedidosYDetalle("ID PEDIDO Y DETALLE", this.Id_pedido.ToString(), out dtDetalle,
                            out DataTable dtDetallePlatosPedido, out rpta);
                        if (dtPedido != null)
                        {
                            this.lblMesero.Text = "Mesero: " + Convert.ToString(dtPedido.Rows[0]["Nombre_empleado"]);
                            //Recorrer las tablas de pedido y detalle y crear las Listas
                            this.tablasPedido = new TablasPedido(this.IsEditar, this.Id_pedido, dtDetalle);
                            DataTable dtCliente =
                                NClientes.BuscarClientes("ID CLIENTE", Convert.ToString(dtPedido.Rows[0]["Id_cliente"]));
                            if (dtCliente != null)
                            {
                                this.contextMenuDatosPedido.ObtenerCliente(DatagridString.ReturnValuesOfCells(dtCliente, 0, out rpta));
                            }
                            else
                            {
                                throw new Exception("No se encontró el cliente");
                            }
                            Label lbl1 = new Label();
                            lbl1.AutoSize = true;
                            lbl1.Location = new System.Drawing.Point(this.dgvProductos.Location.X,
                                this.panelProductosParaAgregar.Location.Y + this.panelProductosParaAgregar.Height);
                            this.Controls.Add(lbl1);
                            this.chkImprimirComandas.Visible = true;
                            this.chkImprimirComandas.Checked = true;

                            this.panelProductosParaAgregar.Visible = true;
                            this.dgvProductos.Location = new System.Drawing.Point(this.dgvProductos.Location.X,
                                this.panelProductosParaAgregar.Location.Y + this.panelProductosParaAgregar.Height + lbl1.Height);
                            this.dgvProductos =
                                    ConfiguracionDatagridview.ConfigurationGrid(this.dgvProductos);
                            this.dgvProductosEditar =
                                    ConfiguracionDatagridview.ConfigurationGrid(this.dgvProductosEditar);
                            this.btnQuitar.Location =
                                new System.Drawing.Point(this.dgvProductos.Location.X - this.btnQuitarProductosEditado.Width - 2, this.dgvProductos.Location.Y);
                            this.btnQuitarProductosEditado.Visible = true;
                        }
                        else
                        {
                            throw new Exception(rpta);
                        }
                    }
                    catch (Exception ex)
                    {
                        Mensajes.MensajeErrorCompleto("TablasPedido.cs", "TablasPedido(bool isEditar, int id_pedido)",
                            "Hubo un error al inicializar las tablas del pedido para editar", ex.Message);
                    }
                    this.ActualizarProductos();
                }
                else
                {
                    this.panelProductosParaAgregar.Visible = false;
                    this.btnQuitarProductosEditado.Visible = false;
                    this.dgvProductos =
                    ConfiguracionDatagridview.ConfigurationGrid(this.dgvProductos);
                    this.tablasPedido = new TablasPedido();
                }
            }
            else
            {
                this.Close();
            }
        }

        private void FrmComprobacion_FormClosed(object sender, FormClosedEventArgs e)
        {
            FrmComprobacion frm = (FrmComprobacion)sender;
            if (frm.DialogResult == DialogResult.OK)
            {

                this.Id_empleado = frm.Id_empleado;
                this.Nombre_empleado = frm.Nombre_empleado;
                this.Cargo_empleado = frm.Cargo_empleado;
            }
        }

        ContextMenuDatosPedido contextMenuDatosPedido;
        PoperContainer containerDatosPedido;

        private FrmPedidoPlatos frmPedidoPlatos;
        private FrmPedidoBebidas frmPedidoBebidas;
        private TablasPedido tablasPedido;

        private int _id_pedido;
        private int _id_mesa;
        private int _numero_mesa;
        private bool _isEditar;
        private bool _isDomicilio;
        private int _id_empleado;
        private string _nombre_empleado;
        private string _cargo_empleado;

        public int Id_pedido { get => _id_pedido; set => _id_pedido = value; }
        public int Id_mesa { get => _id_mesa; set => _id_mesa = value; }
        public int Numero_mesa { get => _numero_mesa; set => _numero_mesa = value; }
        public bool IsEditar { get => _isEditar; set => _isEditar = value; }
        public int Total_parcial { get; private set; }
        public int Id_empleado { get => _id_empleado; set => _id_empleado = value; }
        public string Nombre_empleado { get => _nombre_empleado; set => _nombre_empleado = value; }
        public string Cargo_empleado { get => _cargo_empleado; set => _cargo_empleado = value; }
        public bool IsDomicilio { get => _isDomicilio; set => _isDomicilio = value; }
    }
}
