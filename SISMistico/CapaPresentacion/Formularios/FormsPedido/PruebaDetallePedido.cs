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
    public partial class PruebaDetallePedido : Form
    {
        public PruebaDetallePedido()
        {
            InitializeComponent();
            this.Load += PruebaDetallePedido_Load;
        }

        DataTable tabla;

        public DataTable Tabla { get => tabla; set => tabla = value; }

        private void PruebaDetallePedido_Load(object sender, EventArgs e)
        {
            this.dataGridView1 =
                ConfiguracionDatagridview.ConfigurationGrid(this.dataGridView1);
            this.dataGridView1.DataSource = this.Tabla;
        }
    }
}
