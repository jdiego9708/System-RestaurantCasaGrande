using CapaEntidades.Models;
using CapaNegocio;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace CapaPresentacion.Formularios.FormsPedido
{
    public partial class FrmObservarMesas : Form
    {
        #region PATRON SINGLETON
        private static FrmObservarMesas _Instancia;
        public static FrmObservarMesas GetInstancia()
        {
            if (_Instancia == null || _Instancia.IsDisposed)
            {
                _Instancia = new FrmObservarMesas();
            }
            return _Instancia;
        }
        #endregion

        private FrmObservarMesas()
        {
            InitializeComponent();
            this.Load += FrmNuevoPedido_Load;
            this.btnCargar.Click += BtnCargar_Click;
            this.btnDomicilio.Click += BtnDomicilio_Click;
        }


        private void BtnDomicilio_Click(object sender, EventArgs e)
        {
            if (this.PedidosDomicilios.Count > 0)
            {
                FrmDomicilios frmDomicilios = new FrmDomicilios
                {
                    WindowState = FormWindowState.Maximized,
                };
                frmDomicilios.Show();
            }
            else
            {
                FrmRealizarPedido FrmPedido = new FrmRealizarPedido
                {
                    StartPosition = FormStartPosition.CenterScreen,
                    Id_mesa = 0,
                    Numero_mesa = 0,
                    WindowState = FormWindowState.Maximized,
                    IsDomicilio = true,
                };
                FrmPedido.OnPedidoDomicilioSuccess += FrmPedido_OnPedidoDomicilioSuccess;
                FrmPedido.ShowDialog();
            }
        }

        private void FrmPedido_OnPedidoDomicilioSuccess(object sender, EventArgs e)
        {
            int id_pedido = (int)sender;

            if (this.PedidosDomicilios == null)
                this.PedidosDomicilios = new List<Pedidos>();

            this.CargarDomicilios(DateTime.Now.ToString("yyyy-MM-dd"));
        }

        public void LiberarMesa(int id_mesa)
        {
            foreach (Control control in this.panelMesas.Controls)
            {
                if (control is CustomButtonMesa btn)
                {
                    if (btn.Id_mesa == id_mesa)
                    {
                        btn.ObtenerEstado("DISPONIBLE", btn.Id_pedido, false);
                        break;
                    }
                }
            }
        }

        public void MesaSaliendo(int id_mesa, int id_pedido)
        {
            foreach (Control control in this.panelMesas.Controls)
            {
                if (control is CustomButtonMesa btn)
                {
                    if (btn.Id_mesa == id_mesa)
                    {
                        btn.ObtenerEstado("SALIENDO", id_pedido, true);
                        break;
                    }
                }
            }
        }

        public void ObtenerPedido(int id_pedido, int numero_mesa, string estado)
        {
            foreach (Control control in panelMesas.Controls)
            {
                if (control is CustomButtonMesa btn)
                {
                    if (Convert.ToInt32(btn.Tag) == numero_mesa)
                    {
                        btn.ObtenerEstado(estado, id_pedido, true);
                        break;
                    }
                }
            }
        }

        private void BtnCargar_Click(object sender, EventArgs e)
        {
            this.CargarMesas();
        }

        private void FrmNuevoPedido_Load(object sender, EventArgs e)
        {
            this.CargarMesas();
            this.CargarDomicilios(DateTime.Now.ToString("yyyy-MM-dd"));
        }

        public void CargarDomicilios(string fecha)
        {
            this.PedidosDomicilios = new List<Pedidos>();
            DataTable dtPedidos = NPedido.BuscarPedidos("DOMICILIOS COMPLETO", fecha);
            if (dtPedidos != null)
            {
                int contadorPendientes = 0;
                int contadorOtros = 0;
                foreach (DataRow row in dtPedidos.Rows)
                {
                    Pedidos pedido = new Pedidos(row);
                    if (pedido.Estado_pedido.Equals("PENDIENTE"))
                    {
                        this.PedidosDomicilios.Add(pedido);
                        contadorPendientes += 1;
                    }
                    else
                    {
                        this.PedidosDomicilios.Add(pedido);
                        contadorOtros += 1;
                    }
                }

                this.btnDomicilio.Text = "Domicilios P:(" + contadorPendientes + ") T:(" + this.PedidosDomicilios.Count + ")";
            }
        }

        private void CargarMesas()
        {
            MensajeEspera.ShowWait("Cargando mesas");
            try
            {
                this.panelMesas.Controls.Clear();
                string estado = "DISPONIBLE";
                int id_pedido = 0;
                int id_mesa = 0;
                DataTable TablaMesasConPedido =
                    NPedido.BuscarPedidos("MESAS CON PEDIDO", DateTime.Now.ToString("yyyy-MM-dd"));
                DataTable TablaMesas =
                    NPedido.BuscarPedidos("MESAS COMPLETAS", "");
                this.Numero_mesas = TablaMesas.Rows.Count - 2;
                int positionX = 0;
                int positionY = 0;
                int contador = 1;
                List<Control> controles = new List<Control>();
                while (contador <= this.Numero_mesas)
                {
                    if (TablaMesas != null)
                    {
                        DataRow[] result =
                            TablaMesas.Select("Num_mesa = '" + Convert.ToString(contador) + "'");
                        if (result.Length != 0)
                        {
                            id_mesa = Convert.ToInt32(result[0]["Id_mesa"]);
                        }
                    }

                    if (TablaMesasConPedido != null)
                    {
                        DataRow[] result =
                            TablaMesasConPedido.Select("Num_mesa = '" + Convert.ToString(contador) + "'");
                        if (result.Length != 0)
                        {
                            id_pedido = Convert.ToInt32(result[0]["Id_pedido"]);
                            estado = Convert.ToString(result[0]["Estado_pedido"]);
                        }
                        else
                        {
                            id_pedido = Convert.ToInt32(0);
                            estado = Convert.ToString("DISPONIBLE");
                        }
                    }

                    CustomButtonMesa ButtonMesa = new CustomButtonMesa
                    {
                        Estado_mesa = estado,
                        Texto = "Mesa " + (contador),
                        Tag = contador,
                        Numero_mesa = contador,
                        Id_mesa = id_mesa,
                        Id_pedido = id_pedido
                    };
                    ButtonMesa.ObtenerEstado(estado, id_pedido, false);
                    ButtonMesa.OnBtnCambiarMesaClick += ButtonMesa_OnBtnCambiarMesaClick;
                    ButtonMesa.OnBtnCancelarPedidoClick += ButtonMesa_OnBtnCancelarPedidoClick;
                    ButtonMesa.OnBtnEditarPedidoClick += ButtonMesa_OnBtnEditarPedidoClick;
                    ButtonMesa.OnBtnFacturarPedidoClick += ButtonMesa_OnBtnFacturarPedidoClick;
                    ButtonMesa.OnBtnPrecuentaPedidoClick += ButtonMesa_OnBtnPrecuentaPedidoClick; ;
                    if (positionX > this.panelMesas.Width ||
                        this.panelMesas.Width < positionX + ButtonMesa.Width)
                    {
                        positionY += ButtonMesa.Height + 2;
                        positionX = 0;
                    }
                    ButtonMesa.Location = new Point(positionX, positionY);
                    controles.Add(ButtonMesa);

                    positionX = ButtonMesa.Location.X + ButtonMesa.Width + 2;

                    contador += 1;
                    if (contador == this.Numero_mesas)
                    {
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                this.panelMesas.Controls.AddRange(controles.ToArray());
                this.panelMesas.Refresh();
                MensajeEspera.CloseForm();
            }
            catch (Exception ex)
            {
                MensajeEspera.CloseForm();
                Mensajes.MensajeErrorCompleto(this.Name, "CargarMesas",
                    "Hubo un error al cargar las mesas", ex.Message);
            }
        }

        private void ButtonMesa_OnBtnPrecuentaPedidoClick(object sender, EventArgs e)
        {
            Pedidos pedido = (Pedidos)sender;
            FrmFacturarPedido facturarPedido = new FrmFacturarPedido
            {
                StartPosition = FormStartPosition.CenterScreen,
                IsPrecuenta = true
            };
            facturarPedido.ObtenerPedido(pedido.Id_pedido);
            facturarPedido.Show();
        }

        private void ButtonMesa_OnBtnFacturarPedidoClick(object sender, EventArgs e)
        {
            Pedidos pedido = (Pedidos)sender;
            FrmFacturarPedido facturarPedido = new FrmFacturarPedido
            {
                StartPosition = FormStartPosition.CenterScreen,
                IsPrecuenta = false
            };
            facturarPedido.ObtenerPedido(pedido.Id_pedido);
            facturarPedido.Show();
        }

        private void ButtonMesa_OnBtnEditarPedidoClick(object sender, EventArgs e)
        {
            Pedidos pedido = (Pedidos)sender;
            DatosInicioSesion datos = DatosInicioSesion.GetInstancia();
            FrmPedido FrmPedido = new FrmPedido
            {
                StartPosition = FormStartPosition.CenterScreen,
                Numero_mesa = pedido.Mesa.Num_mesa,
                Tipo_servicio = "MESA",
                EmpleadoSelected = datos.EmpleadoClaveMaestra,
                ClienteSelected = pedido.Cliente,
                MesaSelected = pedido.Mesa,
                WindowState = FormWindowState.Maximized,
                Pedido = pedido,
                IsEditar = true,
            };
            FrmPedido.ShowDialog();
        }

        private void ButtonMesa_OnBtnCancelarPedidoClick(object sender, EventArgs e)
        {
            Pedidos pedido = (Pedidos)sender;
            string rpta = NPedido.CancelarPedido(pedido.Id_pedido, pedido.Observaciones_pedido);
            if (rpta.Equals("OK"))
            {
                this.CargarMesas();
            }
            else
            {
                Mensajes.MensajeInformacion("Hubo un error al cancelar el pedido");
            }
        }

        private void ButtonMesa_OnBtnCambiarMesaClick(object sender, EventArgs e)
        {
            Pedidos pedido = (Pedidos)sender;

            if (pedido != null)
            {
                FrmCambiarMesa frmCambiarMesa = new FrmCambiarMesa
                {
                    StartPosition = FormStartPosition.CenterScreen,
                    Id_pedido_origen = pedido.Id_pedido,
                    Numero_mesa_origen = pedido.Mesa.Num_mesa,
                    Id_mesa_origen = pedido.Id_mesa,
                    Estado_mesa_origen = pedido.Estado_pedido
                };
                frmCambiarMesa.FormClosed += FrmCambiarMesa_FormClosed;
                frmCambiarMesa.ShowDialog();
            }
        }

        private void FrmCambiarMesa_FormClosed(object sender, FormClosedEventArgs e)
        {
            FrmCambiarMesa frm = (FrmCambiarMesa)sender;
            if (frm.DialogResult == DialogResult.OK)
            {
                bool result = true;
                if (frm.Numero_mesa_destino <= this.Numero_mesas)
                {
                    foreach (Control control in this.panelMesas.Controls)
                    {
                        if (control is CustomButtonMesa btn)
                        {
                            if (btn.Numero_mesa == frm.Numero_mesa_destino)
                            {
                                if (btn.Estado_mesa.Equals("DISPONIBLE"))
                                {
                                    btn.ObtenerEstado(frm.Estado_mesa_origen, frm.Id_pedido_origen, true);
                                }
                                else
                                {
                                    Mensajes.MensajeInformacion("La mesa de destino debe estar disponible", "Entendido");
                                    result = false;
                                }
                                break;
                            }
                        }
                    }
                    if (result)
                    {
                        this.LiberarMesa(frm.Id_mesa_origen);
                    }
                }
                else
                {
                    Mensajes.MensajeErrorForm("La mesa no existe o no está habilitada");
                }
            }
        }

        int _numero_mesas;

        private List<Pedidos> _pedidosDomicilios;
        public int Numero_mesas { get => _numero_mesas; set => _numero_mesas = value; }
        public List<Pedidos> PedidosDomicilios
        {
            get => _pedidosDomicilios;
            set
            {
                _pedidosDomicilios = value;
                if (value.Count > 0)
                    this.btnDomicilio.Text = "Domicilios (" + value.Count + ")";
                else
                    this.btnDomicilio.Text = "Domicilio";
            }
        }
    }
}
