using CapaNegocio;
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
    public partial class FrmDetallePedido : Form
    {
        ContextMenuDatosPedido ContextMenuDatosPedido;
        PoperContainer PoperContainer;
        public FrmDetallePedido()
        {
            InitializeComponent();

            this.ContextMenuDatosPedido = new ContextMenuDatosPedido();
            this.PoperContainer = new PoperContainer(ContextMenuDatosPedido);

            this.btnInformacionPedido.MouseDown += btnInformacionPedido_MouseDown;
            this.Load += FrmDetallePedido_Load;
            this.btnEditarPedido.Click += BtnEditarPedido_Click;
            this.ContextMenuDatosPedido.btnTerminarPedido.Click += BtnTerminarPedido_Click;
            this.ContextMenuDatosPedido.btnGuardarPedidosBorradores.Click += BtnGuardarPedidosBorradores_Click;
        }

        private void BtnGuardarPedidosBorradores_Click(object sender, EventArgs e)
        {
            FrmFacturarPedido facturarPedido = new FrmFacturarPedido();
            facturarPedido.ObtenerPedido(this.Id_pedido);
            facturarPedido.StartPosition = FormStartPosition.CenterScreen;
            facturarPedido.IsPrecuenta = true;
            facturarPedido.Show();
        }

        //FACTURAR PEDIDO
        private void BtnTerminarPedido_Click(object sender, EventArgs e)
        {
            FrmFacturarPedido facturarPedido = new FrmFacturarPedido();
            facturarPedido.ObtenerPedido(this.Id_pedido);
            facturarPedido.StartPosition = FormStartPosition.CenterScreen;
            facturarPedido.IsPrecuenta = false;
            facturarPedido.Show();
        }

        private void BuscarPedido(int id_pedido)
        {
            try
            {
                string rpta = "";
                DataTable tablaDetalle;
                TablaDatosPrincipales =
                    NPedido.BuscarPedidosYDetalle("ID PEDIDO Y DETALLE", Convert.ToString(this.Id_pedido),
                    out tablaDetalle,
                    out DataTable dtDetallePlatosPedido, out rpta);
                this.TablaDetalle = tablaDetalle;

                this.dgvPedido.DataSource = TablaDetalle;
                if (this.TablaDetalle != null)
                {
                    this.dgvPedido.Enabled = true;
                    string[] columnsHeaderText =
                    {
                        "Id pedido", "Id tipo", "Tipo", "Nombre", "Precio", "Cantidad", "Total", "Observaciones"
                    };
                    bool[] columnsVisible =
                    {
                        false, false, false, true, true, true, true, true
                    };
                    this.dgvPedido =
                        DatagridString.ChangeHeaderTextAndVisible(this.dgvPedido, columnsHeaderText, columnsVisible);
                }
                else
                {
                    this.dgvPedido.Enabled = false;
                }

                if (this.TablaDatosPrincipales != null)
                {
                    this.ContextMenuDatosPedido.txtCliente.Text = Convert.ToString(TablaDatosPrincipales.Rows[0]["Cliente"]);
                    this.ContextMenuDatosPedido.txtCliente.Tag = Convert.ToString(TablaDatosPrincipales.Rows[0]["Id_cliente"]);
                    this.lblIdPedido.Text = Convert.ToString(TablaDatosPrincipales.Rows[0]["Id_pedido"]);
                    this.lblEmpleado.Text = "Mesero encargado: " + Convert.ToString(TablaDatosPrincipales.Rows[0]["Nombre_empleado"]);

                    this.lblMesa.Text = Convert.ToString(TablaDatosPrincipales.Rows[0]["Num_mesa"]);
                }
            }
            catch (Exception ex)
            {
                this.dgvPedido.Enabled = false;
                Mensajes.MensajeErrorCompleto(this.Name, "BuscarPedido", 
                    "Hubo un error al buscar un pedido", ex.Message);
            }
        }

        private void btnInformacionPedido_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.SumarPrecios();
                this.ContextMenuDatosPedido.ObtenerPrecio();
                PoperContainer.Show(btnInformacionPedido);
            }
        }

        private void SumarPrecios()
        {
            int total_parcial = 0;
            DataTable Tabla = (DataTable)this.dgvPedido.DataSource;
            if (Tabla != null)
            {
                foreach (DataRow row in Tabla.Rows)
                {
                    total_parcial += Convert.ToInt32(row["Total"]);
                }
            }
            this.Total_parcial = total_parcial;
            this.ContextMenuDatosPedido.Total_parcial = this.Total_parcial;
        }

        private void BtnEditarPedido_Click(object sender, EventArgs e)
        {
            if (this.TablaDatosPrincipales != null)
            {
                int id_mesa = Convert.ToInt32(this.TablaDatosPrincipales.Rows[0]["Id_mesa"]);
                int num_mesa = Convert.ToInt32(this.TablaDatosPrincipales.Rows[0]["Num_mesa"]);
                int id_empleado = Convert.ToInt32(this.TablaDatosPrincipales.Rows[0]["Id_empleado"]);
                int id_pedido = Convert.ToInt32(this.TablaDatosPrincipales.Rows[0]["Id_pedido"]);
                int id_cliente = Convert.ToInt32(this.TablaDatosPrincipales.Rows[0]["Id_cliente"]);

                this.SumarPrecios();
                int total_parcial = this.Total_parcial;
                FrmRealizarPedido FrmPedido = new FrmRealizarPedido
                {
                    StartPosition = FormStartPosition.CenterScreen,
                    Id_mesa = id_mesa,
                    Numero_mesa = num_mesa,
                    Id_empleado = id_empleado,
                    Id_pedido = id_pedido,
                    IsEditar = true
                };
                FrmPedido.ShowDialog();
                if (FrmPedido.DialogResult == DialogResult.OK)
                {
                    this.BuscarPedido(this.Id_pedido);
                }
                else
                {
                    this.BuscarPedido(this.Id_pedido);
                }
            }
        }

        private void FrmDetallePedido_Load(object sender, EventArgs e)
        {
            this.ContextMenuDatosPedido.IsEditar = true;
            this.dgvPedido =
                ConfiguracionDatagridview.ConfigurationGrid(this.dgvPedido);
            this.BuscarPedido(this.Id_pedido);
            Font font = new Font("Segoe UI Semibold", 9.75F,
                FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
        }

        private DataTable tablaDetalle;
        private DataTable tablaDatosPrincipales;
        private int total_parcial;
        private int id_pedido;
        private int id_cliente;
        public DataTable TablaDetalle { get => tablaDetalle; set => tablaDetalle = value; }
        public DataTable TablaDatosPrincipales { get => tablaDatosPrincipales; set => tablaDatosPrincipales = value; }
        public int Total_parcial { get => total_parcial; set => total_parcial = value; }
        public int Id_pedido { get => id_pedido; set => id_pedido = value; }
        public int Id_cliente { get => id_cliente; set => id_cliente = value; }
    }
}
