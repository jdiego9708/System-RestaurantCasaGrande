using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CapaPresentacion.Formularios.FormsPedido.Platos
{
    public partial class FrmDetallePedidoPlato : Form
    {
        public FrmDetallePedidoPlato()
        {
            InitializeComponent();
            this.Load += FrmDetallePedidoPlato_Load;
        }

        public event EventHandler OnBtnSaveClick;

        private async void FrmDetallePedidoPlato_Load(object sender, EventArgs e)
        {
            if (this.panel1.Controls.Count > 0)
                this.panel1.Controls.Clear();

            CustomPlato customPlato = new CustomPlato
            {
                Dock = DockStyle.Fill,
                IsEnabledBebida = this.IsEnabledBebida,
            };
            await customPlato.LoadIngredientes();
            customPlato.OnBtnSaveClick += CustomPlato_OnBtnSaveClick;
            this.panel1.Controls.Add(customPlato);
        }

        private void CustomPlato_OnBtnSaveClick(object sender, EventArgs e)
        {
            //Sender = List<ProductDetalleBinding>: que son los detalles del detalle del pedido

            List<ProductDetalleBinding> detalles = (List<ProductDetalleBinding>)sender;
            //this.Product.ProductDetalles = detalles;
            //this.Product.ProductDetalles = detalles;
            this.OnBtnSaveClick?.Invoke(new object[] { this.Product, detalles }, e);
            this.Close();
        }

        public ProductBinding Product { get; set; }

        private bool _isEnabledBebida;
        private bool _isEnabledSopa;
        public bool IsEnabledBebida
        {
            get => _isEnabledBebida;
            set
            {
                _isEnabledBebida = value;
            }
        }
        public bool IsEnabledSopa
        {
            get => _isEnabledSopa;
            set
            {
                _isEnabledSopa = value;
            }
        }
    }
}
