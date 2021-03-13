using CapaEntidades.Models;
using CapaNegocio;
using CapaPresentacion.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CapaPresentacion.Formularios.FormsPedido
{
    public partial class FrmDomicilios : Form
    {
        public FrmDomicilios()
        {
            InitializeComponent();
            this.btnAddDomicilio.Click += BtnAddDomicilio_Click;
            this.Load += FrmDomicilios_Load;
            this.rdEnCurso.CheckedChanged += Rd_CheckedChanged;
            this.rdTerminados.CheckedChanged += Rd_CheckedChanged;
        }

        private void Rd_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rd = (RadioButton)sender;
            if (rd.Checked)
            {
                if (Convert.ToString(rd.Tag).Equals("PENDIENTE"))
                {
                    this.BuscarPedidos("DOMICILIOS PENDIENTES", DateTime.Now.ToString("yyyy-MM-dd"));
                }
                else
                {
                    this.BuscarPedidos("DOMICILIOS OTROS", DateTime.Now.ToString("yyyy-MM-dd"));
                }
            }
        }

        private void FrmDomicilios_Load(object sender, EventArgs e)
        {
            this.rdEnCurso.Checked = true;
            this.BuscarPedidos("DOMICILIOS PENDIENTES", DateTime.Now.ToString("yyyy-MM-dd"));
        }

        private void BuscarPedidos(string tipo_busqueda, string texto_busqueda)
        {
            try
            {
                DataTable dtPedidos = NPedido.BuscarPedidos(tipo_busqueda, texto_busqueda);

                this.panelDomicilios.clearDataSource();

                if (dtPedidos != null)
                {
                    this.gbResultados.Text = "Se encontraron " + dtPedidos.Rows.Count + " resultados.";
                    this.panelDomicilios.BackgroundImage = null;

                    List<UserControl> controls = new List<UserControl>();
                    foreach (DataRow row in dtPedidos.Rows)
                    {
                        Pedidos pedido = new Pedidos(row);
                        DomicilioSmall domicilio = new DomicilioSmall
                        {
                            Pedido = pedido,
                        };
                        domicilio.OnBtnNextClick += Domicilio_OnBtnNextClick;
                        domicilio.OnRefresh += Domicilio_OnRefresh;
                        controls.Add(domicilio);
                    }
                    this.panelDomicilios.AddArrayControl(controls);
                }
                else
                {
                    this.panelDomicilios.BackgroundImageLayout = ImageLayout.Center;
                    this.panelDomicilios.BackgroundImage = Resources.No_hay_domicilios;
                    this.gbResultados.Text = "No se encontraron resultados.";
                }                  
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "BuscarPedidos",
                    "HUbo un error al buscar los pedidos de domicilios", ex.Message);
            }
        }

        private void Domicilio_OnRefresh(object sender, EventArgs e)
        {
            this.rdEnCurso.Checked = true;
            this.BuscarPedidos("DOMICILIOS PENDIENTES", DateTime.Now.ToString("yyyy-MM-dd"));
        }

        private void Domicilio_OnBtnNextClick(object sender, EventArgs e)
        {
            Pedidos pedido = (Pedidos)sender;
            if (pedido.Estado_pedido.Equals("PENDIENTE"))
            {
                FrmFacturarPedido facturarPedido = new FrmFacturarPedido
                {
                    StartPosition = FormStartPosition.CenterScreen,
                    IsPrecuenta = false,
                    IsDomicilio = true,
                };
                facturarPedido.ObtenerPedido(pedido.Id_pedido);
                facturarPedido.OnFacturarPedidoSuccess += FacturarPedido_OnFacturarPedidoSuccess;
                facturarPedido.Show();
            }
        }

        private void FacturarPedido_OnFacturarPedidoSuccess(object sender, EventArgs e)
        {
            int id_pedido = (int)sender;

            string rpta = NPedido.CambiarEstadoPedido(new List<string>
            {
                id_pedido.ToString(),
                "DISPONIBLE",
                "0",
            });

            if (!rpta.Equals("OK"))
                Mensajes.MensajeInformacion("Hubo un error al actualizar el estado del pedido", "Entendido");

            this.rdEnCurso.Checked = true;
            this.BuscarPedidos("DOMICILIOS PENDIENTES", DateTime.Now.ToString("yyyy-MM-dd"));
        }

        private void BtnAddDomicilio_Click(object sender, EventArgs e)
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

        private void FrmPedido_OnPedidoDomicilioSuccess(object sender, EventArgs e)
        {
            this.rdEnCurso.Checked = true;
            this.BuscarPedidos("DOMICILIOS PENDIENTES", DateTime.Now.ToString("yyyy-MM-dd"));

            FrmObservarMesas frmObservarMesas = FrmObservarMesas.GetInstancia();
            if (frmObservarMesas != null)
            {
                int id_pedido = (int)sender;

                if (frmObservarMesas.PedidosDomicilios == null)
                    frmObservarMesas.PedidosDomicilios = new List<Pedidos>();

                frmObservarMesas.CargarDomicilios(DateTime.Now.ToString("yyyy-MM-dd"));
            }
        }
    }
}
