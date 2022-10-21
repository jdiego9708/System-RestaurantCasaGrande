using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using CapaPresentacion.Formularios.FormsClientes;

namespace CapaPresentacion.Formularios.FormsPedido
{
    public partial class ContextMenuDatosPedido : UserControl
    {
        public ContextMenuDatosPedido()
        {
            InitializeComponent();
            this.Load += ContextMenuDatosPedido_Load;
            this.btnSeleccionarCliente.Click += BtnSeleccionarCliente_Click;
        }

        public void ObtenerPrecio()
        {
            this.lblTotalParcial.Text = this.Total_parcial.ToString();
        }

        public void ObtenerCliente(List<string> datos)
        {
            this.txtCliente.Text = 
                "Nombre: " + datos[1] + " - Teléfono: " + datos[2];
            this.txtCliente.Tag = datos[0];
        }

        private void BtnSeleccionarCliente_Click(object sender, EventArgs e)
        {
            FrmObservarClientes FrmObservarClientes = new FrmObservarClientes();
            FrmObservarClientes.menuPedido = this;
            FrmObservarClientes.StartPosition = FormStartPosition.CenterScreen;
            FrmObservarClientes.ShowDialog();
        }

        private void ContextMenuDatosPedido_Load(object sender, EventArgs e)
        {
            this.lblFecha.Text = "Fecha: " + DateTime.Now.ToLongDateString();
            this.chkImprimirPedido.Checked = true;
            if (this.IsEditar)
            {
                this.btnSeleccionarCliente.Visible = false;
                this.btnTerminarPedido.Text = "Facturar pedido final";
                this.chkImprimirPedido.Visible = false;
                this.btnGuardarPedidosBorradores.Text = "Pre-cuenta";
                this.btnGuardarPedidosBorradores.Visible = true;
            }
            this.lblTotalParcial.Text = this.Total_parcial.ToString("C").Replace(",00", "");
            this.lblTotalParcial.Tag = this.Total_parcial;
        }

        private int total_parcial;
        private bool isEditar;

        public int Total_parcial { get => total_parcial; set => total_parcial = value; }
        public bool IsEditar { get => isEditar; set => isEditar = value; }
    }
}
