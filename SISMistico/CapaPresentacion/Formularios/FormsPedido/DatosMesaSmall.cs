using CapaEntidades.Models;
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
    public partial class DatosMesaSmall : UserControl
    {
        public DatosMesaSmall()
        {
            InitializeComponent();
            this.btnCambiarMesa.Click += BtnCambiarMesa_Click;
            this.btnCancelarPedido.Click += BtnCancelarPedido_Click;
            this.btnEditarPedido.Click += BtnEditarPedido_Click;
            this.btnFacturar.Click += BtnFacturar_Click;
            this.btnPrecuenta.Click += BtnPrecuenta_Click;
        }

        private void BtnPrecuenta_Click(object sender, EventArgs e)
        {
            this.OnBtnPrecuentaPedidoClick?.Invoke(this.Pedido, e);
        }

        public event EventHandler OnBtnPrecuentaPedidoClick;
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
            Mensajes.InputBox("Justificación de cancelación", "Continuar",
                "Cerrar", out DialogResult dialog, out string mensaje);
            if (dialog == DialogResult.Yes)
            {
                this.Pedido.Observaciones_pedido = "Justificación: " + mensaje;
                this.OnBtnCancelarPedidoClick?.Invoke(this.Pedido, e);
            }       
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

            DataTable dtDatosPedido =
               NPedido.BuscarPedidosYDetalle("ID PEDIDO Y DETALLE", pedido.Id_pedido.ToString(),
               out DataTable dtDetallePedido, out DataTable dtDetallePlatosPedido, out string rpta);

            foreach (DataRow row in dtDetallePedido.Rows)
            {
                int id_detalle = Convert.ToInt32(row["Id_detalle_pedido"]);
                int id_tipo = Convert.ToInt32(row["Id_tipo"]);
                string tipo = Convert.ToString(row["Tipo"]);
                string nombre = Convert.ToString(row["Nombre"]);

                if (tipo.Equals("PLATO"))
                {
                    info.Append("-" + nombre).Append(": ").Append(Environment.NewLine);

                    DataRow[] find = dtDetallePlatosPedido.Select(string.Format("Id_detalle_pedido = {0}", id_detalle));
                    if (find.Length > 0)
                    {
                        foreach (DataRow re in find)
                        {
                            Ingredientes ing = new Ingredientes(re);
                            info.Append("*" + ing.Nombre_ingrediente).Append(Environment.NewLine);
                        }
                    }

                    //row["Nombre"] = info.ToString();
                }
                else
                {
                    info.Append("-" + nombre).Append(Environment.NewLine);
                }
            }

            this.txtInfo.Text = info.ToString();
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
