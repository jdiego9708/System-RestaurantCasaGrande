using CapaEntidades.Models;
using CapaNegocio;
using CapaPresentacion.Formularios.Controles;
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
    public partial class DomicilioSmall : UserControl
    {
        Observacion observaciones = new Observacion();
        PoperContainer container;
        public DomicilioSmall()
        {
            InitializeComponent();
            this.observaciones.btnSave.Click += BtnSave_Click;
            this.observaciones.btnSave.Visible = true;
            this.btnNext.Click += BtnNext_Click;
            this.btnPrint.Click += BtnPrint_Click;
            this.btnAddProductos.Click += BtnAddProductos_Click;
            this.btnCancelar.Click += BtnCancelar_Click;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.observaciones.txtObservacion.Text))
            {
                string rpta = NPedido.CancelarDomicilio(this.Pedido.Id_pedido, this.observaciones.txtObservacion.Text);
                if (rpta.Equals("OK"))
                {
                    Mensajes.MensajeOkForm("Se canceló correctamente el domicilio");
                    this.OnRefresh?.Invoke(this.Pedido, e);
                }
            }
            else
            {
                Mensajes.MensajeInformacion("Debe justificar la cancelación del domicilio", "Entendido");
                container = new PoperContainer(this.observaciones);
                container.Show(this.btnCancelar);
            }
        }

        private void BtnCancelar_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.observaciones.txtObservacion.Text))
                this.observaciones.txtObservacion.Clear();

            Mensajes.MensajePregunta("¿Está seguro que desea cancelar el domicilio?", "Si", "No", out DialogResult dialog);
            if (dialog == DialogResult.Yes)
            {
                container = new PoperContainer(this.observaciones);
                container.Show(this.btnCancelar);
            }
        }

        private void BtnAddProductos_Click(object sender, EventArgs e)
        {
            //Quiere decir que va a agregar más productos
            if (this.Pedido.Estado_pedido.Equals("PENDIENTE"))
            {
                FrmRealizarPedido FrmPedido = new FrmRealizarPedido
                {
                    StartPosition = FormStartPosition.CenterScreen,
                    Id_mesa = this.Pedido.Id_mesa,
                    Numero_mesa = 0,
                    Id_empleado = this.Pedido.Id_empleado,
                    Id_pedido = this.Pedido.Id_pedido,
                    IsEditar = true,
                    IsDomicilio = true,
                };
                FrmPedido.ShowDialog();
                if (FrmPedido.DialogResult == DialogResult.OK)
                {
                    this.AsignarDatos(this.Pedido);
                }
                else
                {
                    this.AsignarDatos(this.Pedido);
                }
            }
        }

        private void BtnPrint_Click(object sender, EventArgs e)
        {
            if (this.Pedido.Estado_pedido.Equals("PENDIENTE"))
            {
                MensajeEspera.ShowWait("Imprimiendo...");
                //Imprime solo las comandas
                FrmComandas frmComandas = new FrmComandas
                {
                    Id_pedido = this.Pedido.Id_pedido,
                };
                frmComandas.ObtenerReporte();
                frmComandas.AsignarTablas();
                frmComandas.ImprimirFactura(1);
                MensajeEspera.CloseForm();
            }
            else if (this.Pedido.Estado_pedido.Equals("DISPONIBLE"))
            {
                MensajeEspera.ShowWait("Imprimiendo...");
                FrmFacturaFinal frmFacturaFinal = new FrmFacturaFinal
                {
                    Id_pedido = this.Pedido.Id_pedido,
                };
                frmFacturaFinal.AsignarReporte();
                frmFacturaFinal.AsignarTablas();
                frmFacturaFinal.ImprimirFactura(1);
                MensajeEspera.CloseForm();
            }
        }

        private void BtnNext_Click(object sender, EventArgs e)
        {
            this.OnBtnNextClick?.Invoke(this.Pedido, e);      
        }

        private void AsignarDatos(Pedido pedido)
        {
            if (pedido.Estado_pedido.Equals("PENDIENTE"))
            {
                this.panel1.BackColor = Color.FromArgb(255, 128, 128);
                this.btnAddProductos.Visible = true;
                this.btnNext.Visible = true;
                this.btnCancelar.Visible = true;
                this.btnPrint.Visible = true;
            }
            else if (pedido.Estado_pedido.Equals("CANCELADO"))
            {
                this.panel1.BackColor = Color.FromArgb(255, 128, 0);
                this.btnAddProductos.Visible = false;
                this.btnNext.Visible = false;
                this.btnCancelar.Visible = false; 
                this.btnPrint.Visible = false;
            }
            else
            {
                this.panel1.BackColor = Color.Teal;
                this.btnAddProductos.Visible = false;
                this.btnNext.Visible = false;
                this.btnCancelar.Visible = false;
                this.btnPrint.Visible = true;
            }

            StringBuilder info = new StringBuilder();
            info.Append("Pedido número: ").Append(pedido.Id_pedido).Append(Environment.NewLine);
            info.Append("Fecha: ").Append(pedido.Fecha_pedido.ToLongDateString()).Append(Environment.NewLine);
            info.Append("Hora: ").Append(pedido.Hora_pedido).Append(Environment.NewLine);

            //Obtener los datos principales
            DataTable TablaDatosPrincipales = NPedido.BuscarPedidosYDetalle("ID PEDIDO Y DETALLE",
                Convert.ToString(pedido.Id_pedido), out DataTable dtDetallePedido, out string rpta);
            if (TablaDatosPrincipales != null)
            {
                string cliente = Convert.ToString(TablaDatosPrincipales.Rows[0]["Cliente"]);
                string empleado = Convert.ToString(TablaDatosPrincipales.Rows[0]["Nombre_empleado"]);
                string observaciones = Convert.ToString(TablaDatosPrincipales.Rows[0]["Observaciones_pedido"]);
                info.Append("Cliente: ").Append(cliente).Append(Environment.NewLine);

                if (!string.IsNullOrEmpty(observaciones))
                {
                    info.Append("Observaciones: ").Append(observaciones).Append(Environment.NewLine);
                }

                info.Append("Detalle: ").Append(Environment.NewLine);

                foreach (DataRow row in dtDetallePedido.Rows)
                {
                    info.Append("- ").Append(row["Nombre"].ToString()).Append(" - Cantidad: ").Append(row["Cantidad"].ToString()).Append(" ");
                    if (!string.IsNullOrEmpty(row["Observaciones"].ToString()))
                    {
                        info.Append("- ").Append(row["Observaciones"].ToString());
                        info.Append(Environment.NewLine);
                    }
                    else
                    {
                        info.Append(Environment.NewLine);
                    }
                }

                this.txtInformacion.Text = info.ToString();
            }
        }

        public event EventHandler OnBtnNextClick;
        public event EventHandler OnRefresh;

        private Pedido _pedido;

        public Pedido Pedido
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
