using CapaNegocio;
using CapaPresentacion.Formularios.FormsPedido;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace CapaPresentacion.Formularios
{
    public partial class CustomButtonMesa : UserControl
    {
        public CustomButtonMesa()
        {
            InitializeComponent();
            this.btnMesa.MouseEnter += BtnMesa_MouseEnter;
            this.btnMesa.MouseLeave += BtnMesa_MouseLeave;
            this.Load += CustomButtonMesa_Load;
            this.btnMesa.Click += new EventHandler(Button_Click);
            this.btnMesa.MouseClick += new MouseEventHandler(MouseButton_Click);
            this.btnMesa.MouseDown += ButtonMouseDown_Click;
        }

        #region EVENTOS

        public event EventHandler cambioMesaClick;

        protected void CambioMesa_Click(object sender, EventArgs e)
        {
            if (this.cambioMesaClick != null)
                this.cambioMesaClick(this, e);
        }


        protected void ButtonMouseDown_Click(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    Button btnSender = (Button)sender;
                    this.MenuOpciones.Show(Cursor.Position);
                    btnSender.Select();
                    break;

                case MouseButtons.Right:
                    // Right click
                    break;
            }
        }

        public event MouseEventHandler mouseButtonClick;

        protected void MouseButton_Click(object sender, MouseEventArgs e)
        {
            //bubble the event up to the parent
            if (this.mouseButtonClick != null)
                this.mouseButtonClick(this, e);
        }

        public event EventHandler buttonClick;

        protected void Button_Click(object sender, EventArgs e)
        {
            //bubble the event up to the parent
            if (this.buttonClick != null)
                this.buttonClick(this, e);
        }
        #endregion

        public Panel panelDetalle;

        public ContextMenuStrip MenuOpciones = new ContextMenuStrip();

        private void MenuContextuales()
        {
            Font font = new Font("Segoe UI", 9.75F,
                FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));

            #region MENU OPCIONES MESA
            ToolStripMenuItem NuevoPedido =
                new ToolStripMenuItem("Iniciar un nuevo pedido");
            ToolStripMenuItem ObservarPedidos =
                new ToolStripMenuItem("Observar pedidos");
            ToolStripMenuItem CambioMesa =
                new ToolStripMenuItem("Cambiar de mesa");

            NuevoPedido.BackColor = Color.Silver;
            NuevoPedido.ForeColor = Color.Black;
            NuevoPedido.Font = font;
            NuevoPedido.Name = "NuevoPedido";
            NuevoPedido.Click += NuevoPedido_Click;

            ObservarPedidos.BackColor = Color.Silver;
            ObservarPedidos.ForeColor = Color.Black;
            ObservarPedidos.Name = "ObservarPedidos";
            ObservarPedidos.Font = font;
            ObservarPedidos.Click += ObservarPedidos_Click;

            CambioMesa.BackColor = Color.Silver;
            CambioMesa.ForeColor = Color.Black;
            CambioMesa.Name = "CambioMesa";
            CambioMesa.Font = font;
            CambioMesa.Click += CambioMesa_Click;

            this.MenuOpciones.Items.Add(NuevoPedido);
            this.MenuOpciones.Items.Add(ObservarPedidos);
            this.MenuOpciones.Items.Add(CambioMesa);

            #endregion

        }

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

        private void ObservarPedidos_Click(object sender, EventArgs e)
        {
            if (this.Id_pedido != 0)
            {
                FrmObservarMesas frm = FrmObservarMesas.GetInstancia();
                frm.AbrirDetallePedido(this.Id_pedido);
            }
        }

        private void NuevoPedido_Click(object sender, EventArgs e)
        {
            if (this.Estado_mesa.Equals("DISPONIBLE"))
            {
                FrmRealizarPedido FrmPedido = new FrmRealizarPedido
                {
                    StartPosition = FormStartPosition.CenterScreen,
                    Id_mesa = Convert.ToInt32(this.Id_mesa),
                    Numero_mesa = Convert.ToInt32(this.Numero_mesa),
                    WindowState = FormWindowState.Maximized
                };
                FrmPedido.ShowDialog();
            }
        }

        private void CustomButtonMesa_Load(object sender, EventArgs e)
        {
            this.btnMesa.Text = this.Texto;
            this.MenuContextuales();
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

        public string Texto { get => texto; set => texto = value; }
        public string Estado_mesa { get => estado_mesa; set => estado_mesa = value; }
        public int Id_pedido { get => id_pedido; set => id_pedido = value; }
        public int Numero_mesa { get => numero_mesa; set => numero_mesa = value; }
        public int Id_mesa { get => id_mesa; set => id_mesa = value; }
    }
}
