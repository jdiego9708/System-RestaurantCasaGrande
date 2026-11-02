using CapaNegocio;
using CapaPresentacion.Formularios.FormsPedido;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using CapaEntidades.Models;

namespace CapaPresentacion.Formularios
{
    public partial class CustomButtonMesa : UserControl
    {
        PoperContainer container;
        DatosMesaSmall datosMesa;

        public CustomButtonMesa()
        {
            InitializeComponent();
            this.btnMesa.MouseEnter += BtnMesa_MouseEnter;
            this.btnMesa.MouseLeave += BtnMesa_MouseLeave;
            this.Load += CustomButtonMesa_Load;
            this.btnMesa.MouseDown += ButtonMouseDown_Click;
        }

        #region EVENTOS

        public event EventHandler OnBtnCambiarMesaClick;
        public event EventHandler OnBtnCancelarPedidoClick;
        public event EventHandler OnBtnEditarPedidoClick;
        public event EventHandler OnBtnFacturarPedidoClick;
        public event EventHandler OnBtnPrecuentaPedidoClick;

        protected void ButtonMouseDown_Click(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:

                    if (this.Estado_mesa.Equals("DISPONIBLE"))
                    {
                        DatosInicioSesion datos = DatosInicioSesion.GetInstancia();
                        FrmPedido FrmPedido = new FrmPedido
                        {
                            StartPosition = FormStartPosition.CenterScreen,
                            Numero_mesa = Convert.ToInt32(this.Numero_mesa),
                            Tipo_servicio = "MESA",
                            //EmpleadoSelected = datos.EmpleadoClaveMaestra,
                            ClienteSelected = datos.ClienteDefault,
                            MesaSelected = new CapaEntidades.Models.Mesas
                            {
                                Id_mesa = this.Id_mesa,
                                Num_mesa = this.Numero_mesa,
                                Descripcion_mesa = string.Empty,
                            },
                            WindowState = FormWindowState.Maximized
                        };
                        FrmPedido.Show();
                    }
                    else
                    {
                        DataTable dtPedido =
                        NPedido.BuscarPedidos("ID PEDIDO", this.Id_pedido.ToString());
                        if (dtPedido != null)
                        {
                            this.datosMesa = new DatosMesaSmall
                            {
                                Pedido = new Pedidos(dtPedido.Rows[0]),
                            };
                            this.datosMesa.OnBtnCambiarMesaClick += DatosMesa_OnBtnCambiarMesaClick;
                            this.datosMesa.OnBtnCancelarPedidoClick += DatosMesa_OnBtnCancelarPedidoClick;
                            this.datosMesa.OnBtnEditarPedidoClick += DatosMesa_OnBtnEditarPedidoClick;
                            this.datosMesa.OnBtnFacturarPedidoClick += DatosMesa_OnBtnFacturarPedidoClick;
                            this.datosMesa.OnBtnPrecuentaPedidoClick += DatosMesa_OnBtnPrecuentaPedidoClick;
                            this.container = new PoperContainer(this.datosMesa);
                            this.container.Show(Cursor.Position);
                        }
                        else
                        {
                            Mensajes.MensajeInformacion("Hubo un error buscando el pedido");
                        }

                    }

                    break;

                case MouseButtons.Right:
                    // Right click
                    break;
            }
        }

        private void DatosMesa_OnBtnPrecuentaPedidoClick(object sender, EventArgs e)
        {
            this.OnBtnPrecuentaPedidoClick?.Invoke(sender, e);
        }

        private void DatosMesa_OnBtnFacturarPedidoClick(object sender, EventArgs e)
        {
            this.OnBtnFacturarPedidoClick?.Invoke(sender, e);
        }

        private void DatosMesa_OnBtnEditarPedidoClick(object sender, EventArgs e)
        {
            if (this.container != null)
                this.container.Close(); 

            this.OnBtnEditarPedidoClick?.Invoke(sender, e);
        }

        private void DatosMesa_OnBtnCancelarPedidoClick(object sender, EventArgs e)
        {
            if (this.container != null)
                this.container.Close();

            this.OnBtnCancelarPedidoClick?.Invoke(sender, e);
        }

        private void DatosMesa_OnBtnCambiarMesaClick(object sender, EventArgs e)
        {
            if (this.container != null)
                this.container.Close();

            this.OnBtnCambiarMesaClick?.Invoke(sender, e);
        }
        #endregion

        public void ObtenerEstado(string estado, int id_pedido, bool isEditar)
        {
            this.Id_pedido = id_pedido;
            this.Estado_mesa = estado;

            string rpta = NPedido.CambiarEstadoPedido(new System.Collections.Generic.List<string>()
            {
                this.Id_pedido.ToString(), this.Estado_mesa, this.Id_mesa.ToString()
            });

            if (this.Estado_mesa.Equals("DISPONIBLE"))
            {
                this.btnMesa.BackColor = Color.LightGreen;
            }
            else if (this.Estado_mesa.Equals("PENDIENTE"))
            {
                this.btnMesa.BackColor = Color.Yellow;
            }
            else if (this.Estado_mesa.Equals("SALIENDO"))
            {
                this.btnMesa.BackColor = Color.DarkOrange;
            }
        }

        /// <summary>
        /// Vuelve a armar las líneas de la tarjeta (estado, total, hora, mesero) según el estado actual de la mesa.
        /// Hay que llamarla después de ObtenerEstado cuando la mesa cambia con el mapa ya dibujado (facturar, precuenta,
        /// pedido nuevo, cambio de mesa): ObtenerEstado solo cambia el color, y sin esto el texto se queda con los datos
        /// del pedido anterior. Una mesa libre queda en "Libre", sin total ni mesero.
        /// </summary>
        public void RefreshDetails()
        {
            DataRow row = null;
            if (!string.Equals(this.Estado_mesa, "DISPONIBLE", StringComparison.OrdinalIgnoreCase) && this.Id_pedido != 0)
            {
                try
                {
                    DataRow[] found = NApiMovil.Mesas(DateTime.Today).Select("Num_mesa = " + this.Numero_mesa);
                    if (found.Length != 0 && Convert.ToInt32(found[0]["Id_pedido"]) == this.Id_pedido)
                        row = found[0];
                }
                catch (Exception) { }
            }

            this.Details = TableCardText.Detail(this.Estado_mesa, row);
            this.btnMesa.Text = this.Texto + Environment.NewLine + this.Details;
        }

        private void CustomButtonMesa_Load(object sender, EventArgs e)
        {
            this.btnMesa.Text = string.IsNullOrEmpty(this.Details)
                ? this.Texto
                : this.Texto + Environment.NewLine + this.Details;
            if (this.Estado_mesa.Equals("DISPONIBLE"))
            {
                this.btnMesa.BackColor = Color.LightGreen;
            }
            else if (this.Estado_mesa.Equals("OCUPADA"))
            {
                this.btnMesa.BackColor = Color.Yellow;
            }
            else if (this.Estado_mesa.Equals("SALIENDO"))
            {
                this.btnMesa.BackColor = Color.DarkOrange;
            }
        }

        private void BtnMesa_MouseLeave(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            btn.Font =
                new Font("Segoe UI", 12F, FontStyle.Regular,
                GraphicsUnit.Point, ((byte)(0)));
        }

        private void BtnMesa_MouseEnter(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            btn.Font =
                new Font("Segoe UI", 14F, FontStyle.Bold,
                GraphicsUnit.Point, ((byte)(0)));
        }

        private string texto;
        private string estado_mesa;
        private int id_pedido;
        private int numero_mesa;
        private int id_mesa;

        // Líneas bajo el número de mesa (estado, total, hora, mesero); las arma TableCardText.
        public string Details { get; set; }

        public string Texto { get => texto; set => texto = value; }
        public string Estado_mesa { get => estado_mesa; set => estado_mesa = value; }
        public int Id_pedido { get => id_pedido; set => id_pedido = value; }
        public int Numero_mesa { get => numero_mesa; set => numero_mesa = value; }
        public int Id_mesa { get => id_mesa; set => id_mesa = value; }
    }
}
