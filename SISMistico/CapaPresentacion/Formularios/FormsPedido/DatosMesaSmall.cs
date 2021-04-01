using CapaEntidades.Models;
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
    public partial class DatosMesaSmall : UserControl
    {
        public DatosMesaSmall()
        {
            InitializeComponent();
            this.btnCambiarMesa.Click += BtnCambiarMesa_Click;
            this.btnCancelarPedido.Click += BtnCancelarPedido_Click;
            this.btnEditarPedido.Click += BtnEditarPedido_Click;
            this.btnFacturar.Click += BtnFacturar_Click;
        }

        public event EventHandler OnBtnCambiarMesaClick;
        public event EventHandler OnBtnCancelarPedidoClick;
        public event EventHandler OnBtnEditarPedidoClick;
        public event EventHandler OnBtnFacturarPedidoClick;

        private void BtnFacturar_Click(object sender, EventArgs e)
        {
            this.OnBtnFacturarPedidoClick?.Invoke(this.Pedido, e);
        }

        private void BtnEditarPedido_Click(object sender, EventArgs e)
        {
            this.OnBtnEditarPedidoClick?.Invoke(this.Pedido, e);
        }

        private void BtnCancelarPedido_Click(object sender, EventArgs e)
        {
            this.OnBtnCancelarPedidoClick?.Invoke(this.Pedido, e);
        }

        private void BtnCambiarMesa_Click(object sender, EventArgs e)
        {
            this.OnBtnCambiarMesaClick?.Invoke(this.Pedido, e);           
        }

        private void AsignarDatos(Pedidos pedido)
        {
            StringBuilder info = new StringBuilder();
            info.Append("Fecha y hora: ").Append(pedido.Fecha_pedido.ToLongDateString() + " - ");
            info.Append(pedido.Hora_pedido).Append(Environment.NewLine);
            info.Append("Mesa: " + pedido.Mesa.Num_mesa).Append(Environment.NewLine);
            info.Append("Atiende: ").Append(pedido.Empleado.Nombre_empleado).Append(Environment.NewLine);
        }

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

    }
}
