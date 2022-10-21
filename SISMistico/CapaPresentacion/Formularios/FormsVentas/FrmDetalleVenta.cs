using CapaNegocio;
using System;
using System.Data;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CapaPresentacion.Formularios.FormsVentas
{
    public partial class FrmDetalleVenta : Form
    {
        public FrmDetalleVenta()
        {
            InitializeComponent();

            #region Custom ToolBox
            this.pxClose.Click += PxClose_Click;
            this.pxMinimize.Click += PxMinimize_Click;
            this.pxMaximize.Click += PxMaximize_Click;
            this.panelToolBox.MouseDown += PanelToolBox_MouseDown;
            #endregion

            this.Load += FrmDetalleVenta_Load;
            this.btnPrint.Click += BtnPrint_Click;
        }

        private void BtnPrint_Click(object sender, EventArgs e)
        {
            MensajeEspera.ShowWait("Imprimiendo...");
            FrmFacturaFinal frm = new FrmFacturaFinal
            {
                Id_pedido = this.Id_pedido
            };
            frm.AsignarReporte();
            frm.AsignarTablasCuentaFinal();
            frm.ImprimirFactura(1);
            MensajeEspera.CloseForm();
        }

        private void FrmDetalleVenta_Load(object sender, EventArgs e)
        {
            this.AsignarDatos();
        }

        private void AsignarDatos()
        {
            try
            {
                if (Row_venta != null)
                {
                    this.Id_pedido = Convert.ToInt32(Row_venta.Cells["Id_pedido"].Value);
                    this.lblId_venta.Text += " " + Convert.ToString(Row_venta.Cells["Id_venta"].Value);
                    this.lblIdPedido.Text += " " + Convert.ToString(Row_venta.Cells["Id_pedido"].Value);
                    this.lblFecha.Text += " " + Convert.ToDateTime(Row_venta.Cells["Fecha_venta"].Value).ToLongDateString();
                    this.lblHora.Text += " " + Convert.ToDateTime(Row_venta.Cells["Hora_venta"].Value).ToLongTimeString();
                    this.lblMesa.Text += " " + Convert.ToString(Row_venta.Cells["Num_mesa"].Value);
                    this.lblEmpleado.Text += " " + Convert.ToString(Row_venta.Cells["Nombre_empleado"].Value);
                    this.lblCliente.Text += " " + Convert.ToString(Row_venta.Cells["Nombre_cliente"].Value);
                    this.lblTotal_parcial.Text += " " + Convert.ToInt32(Row_venta.Cells["Total_parcial"].Value).ToString("C").Replace(",00", "");
                    this.lblPropina.Text += " " + Convert.ToInt32(Row_venta.Cells["Propina"].Value).ToString("C").Replace(",00", "");
                    this.lblSubTotal.Text += " " + Convert.ToInt32(Row_venta.Cells["Subtotal"].Value).ToString("C").Replace(",00", "");
                    this.lblDescuento.Text += " " + Convert.ToString(Row_venta.Cells["Descuento"].Value);
                    this.lblTotalFinal.Text += " " + Convert.ToInt32(Row_venta.Cells["Total_final"].Value).ToString("C").Replace(",00", "");
                    this.txtObservaciones.Text = Convert.ToString(Row_venta.Cells["Observaciones"].Value);

                    DataTable dtDetallePedido;
                    DataTable dtDetalleVenta;
                    DataTable DatosPrincipales = NVentas.BuscarVentaFinal(this.Id_pedido.ToString(), out dtDetallePedido, out dtDetalleVenta);

                    this.dgvPagos =
                        ConfiguracionDatagridview.ConfigurationGrid(this.dgvPagos);

                    this.dgvDetalle_pedido =
                        ConfiguracionDatagridview.ConfigurationGrid(this.dgvDetalle_pedido);

                    this.dgvDetalle_pedido.DataSource = dtDetallePedido;
                    this.dgvPagos.DataSource = dtDetalleVenta;

                    if (dtDetallePedido != null)
                    {
                        string[] columns_header =
                        {
                            "Id pedido", "Id tipo", "Tipo", "Nombre", "Precio", "Cantidad", "Total", "Observaciones"
                        };
                        bool[] columns_visible =
                        {
                            false, false, false, true, true, true, true, true
                        };
                        this.dgvDetalle_pedido = DatagridString.ChangeHeaderTextAndVisible(this.dgvDetalle_pedido, columns_header, columns_visible);
                    }

                    if (dtDetalleVenta != null)
                    {
                        string[] columns_header =
                        {
                            "Id venta", "Método de pago", "Valor", "Vaucher", "Observaciones"
                        };
                        bool[] columns_visible =
                        {
                            false, true, true, false, true
                        };
                        this.dgvPagos = DatagridString.ChangeHeaderTextAndVisible(this.dgvPagos, columns_header, columns_visible);
                    }
                }
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "AsignarDatos()",
                    "Hubo un error al asignar los datos de la venta", ex.Message);
            }

        }

        private int _id_venta;
        private DataGridViewRow _row_venta;
        private int _id_pedido;

        public int Id_venta { get => _id_venta; set => _id_venta = value; }
        public DataGridViewRow Row_venta { get => _row_venta; set => _row_venta = value; }
        public int Id_pedido { get => _id_pedido; set => _id_pedido = value; }

        #region Custom ToolBox
        private void PanelToolBox_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();

        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hand, int wmsg, int wparam, int lparam);

        private void PxMaximize_Click(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.WindowState = FormWindowState.Normal;
            }
            else if (this.WindowState == FormWindowState.Normal)
            {
                this.WindowState = FormWindowState.Maximized;
            }
            else if (this.WindowState == FormWindowState.Maximized)
            {
                this.WindowState = FormWindowState.Normal;
            }
        }

        private void PxMinimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void PxClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        #endregion
    }
}
