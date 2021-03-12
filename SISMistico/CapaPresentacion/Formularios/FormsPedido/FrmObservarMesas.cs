﻿using CapaEntidades.Models;
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
            this.panelMesas.SizeChanged += PanelMesas_SizeChanged;
            this.txtNumeroMesas.KeyPress += TxtNumeroMesas_KeyPress;
            this.txtNumeroMesas.LostFocus += TxtNumeroMesas_LostFocus;
            this.txtNumeroMesas.GotFocus += TxtNumeroMesas_GotFocus;
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
                this.PedidosDomicilios = new List<Pedido>();

            this.CargarDomicilios(DateTime.Now.ToString("yyyy-MM-dd"));
        }

        private void TxtNumeroMesas_LostFocus(object sender, EventArgs e)
        {
            TextBox txt = (TextBox)sender;
            if (txt.Text.Equals(""))
            {
                txt.Text = "0";
            }
        }

        private void TxtNumeroMesas_GotFocus(object sender, EventArgs e)
        {
            TextBox txt = (TextBox)sender;
            txt.SelectAll();
        }

        private void TxtNumeroMesas_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (Char.IsDigit(e.KeyChar))
            {
                e.Handled = false;
            }
            else if (Char.IsControl(e.KeyChar))
            {
                e.Handled = false;
            }
            else if (Char.IsLetter(e.KeyChar))
            {
                e.Handled = true;
            }
            else
            {
                e.Handled = true;
            }
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
            this.panelDetallePedido.Controls.Clear();
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
            this.panelDetallePedido.Controls.Clear();
        }

        private void PanelMesas_SizeChanged(object sender, EventArgs e)
        {
            this.panelDetallePedido.Location =
                new Point(this.panelMesas.Location.X + this.panelMesas.Width + 5,
                this.panelMesas.Location.Y);
        }

        public void AbrirDetallePedido(int id_pedido)
        {
            if (this.panelDetallePedido.Controls.Count > 0)
            {
                this.panelDetallePedido.Controls.Clear();
            }

            FrmDetallePedido frmDetallePedido = new FrmDetallePedido();
            frmDetallePedido.WindowState = FormWindowState.Maximized;
            frmDetallePedido.FormBorderStyle = FormBorderStyle.None;
            frmDetallePedido.Dock = DockStyle.Fill;
            frmDetallePedido.TopLevel = false;
            frmDetallePedido.Id_pedido = id_pedido;
            this.panelDetallePedido.Controls.Add(frmDetallePedido);
            frmDetallePedido.Show();
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

            if (!this.txtNumeroMesas.Text.Equals(""))
            {
                int numero = Convert.ToInt32(this.txtNumeroMesas.Text);
                this.CargarMesas(numero);
            }
            this.txtNumeroMesas.Clear();

        }

        private void FrmNuevoPedido_Load(object sender, EventArgs e)
        {
            CustomButtonMesa ButtonMesa = new CustomButtonMesa();
            this.panelMesas.Width = ButtonMesa.Width * 3 + 30;
            this.Numero_mesas = 12;
            this.txtNumeroMesas.Text = Numero_mesas.ToString();
            this.CargarMesas(Numero_mesas);
            this.CargarDomicilios(DateTime.Now.ToString("yyyy-MM-dd"));
        }

        public void CargarDomicilios(string fecha)
        {
            this.PedidosDomicilios = new List<Pedido>();
            DataTable dtPedidos = NPedido.BuscarPedidos("DOMICILIOS COMPLETO", fecha);
            if (dtPedidos != null)
            {
                int contadorPendientes = 0;
                int contadorOtros = 0;
                foreach (DataRow row in dtPedidos.Rows)
                {
                    Pedido pedido = new Pedido(row);
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

        private void CargarMesas(int numero_mesas)
        {
            MensajeEspera.ShowWait("Cargando mesas");
            try
            {
                this.panelMesas.Controls.Clear();
                string estado = "DISPONIBLE";
                int id_pedido = 0;
                int id_mesa = 0;
                DataTable TablaMesasConPedido =
                    NPedido.BuscarPedidos("MESAS CON PEDIDO", "");
                DataTable TablaMesas =
                    NPedido.BuscarPedidos("MESAS COMPLETAS", "");
                int positionX = 0;
                int positionY = 0;
                int contador = 1;
                List<Control> controles = new List<Control>();
                while (contador <= numero_mesas)
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
                    ButtonMesa.cambioMesaClick += ButtonMesa_cambioMesaClick;
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
                    if (contador == numero_mesas)
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

        private void ButtonMesa_cambioMesaClick(object sender, EventArgs e)
        {
            CustomButtonMesa btn = (CustomButtonMesa)sender;

            if (btn.Id_pedido != 0)
            {
                FrmCambiarMesa frmCambiarMesa = new FrmCambiarMesa
                {
                    StartPosition = FormStartPosition.CenterScreen,
                    Id_pedido_origen = btn.Id_pedido,
                    Numero_mesa_origen = btn.Numero_mesa,
                    Id_mesa_origen = btn.Id_mesa,
                    Estado_mesa_origen = btn.Estado_mesa
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
                this.panelDetallePedido.Controls.Clear();
                bool result = true;
                if (frm.Numero_mesa_destino <= Convert.ToInt32(this.txtNumeroMesas.Text))
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

        int numero_mesas;

        private List<Pedido> _pedidosDomicilios;
        public int Numero_mesas { get => numero_mesas; set => numero_mesas = value; }
        public List<Pedido> PedidosDomicilios
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
