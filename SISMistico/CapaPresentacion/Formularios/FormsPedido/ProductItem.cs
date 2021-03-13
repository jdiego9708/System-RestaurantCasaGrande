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
    public partial class ProductItem : UserControl
    {
        public ProductItem()
        {
            InitializeComponent();
        }

        private void AsignarDatos(ProductBinding product)
        {

        }

        private ProductBinding productBinding;

        public ProductBinding ProductBinding
        {
            get => productBinding;
            set
            {
                productBinding = value;
            }
        }
    }
}
