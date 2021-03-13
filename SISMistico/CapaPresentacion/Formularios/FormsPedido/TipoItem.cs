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
    public partial class TipoItem : UserControl
    {
        public TipoItem()
        {
            InitializeComponent();
            this.btnTipo.Click += BtnTipo_Click;
        }

        private void BtnTipo_Click(object sender, EventArgs e)
        {
            this.OnBtnTipoClick?.Invoke(this, e);
        }

        public event EventHandler OnBtnTipoClick;

        private object _tipoObject;
        private string _tipo;
        private string _nombreTipo;

        public string Tipo
        {
            get => _tipo;
            set
            {
                _tipo = value;
            }
        }

        public string NombreTipo
        {
            get => _nombreTipo;
            set
            {
                _nombreTipo = value;
                this.btnTipo.Text = value;
            }
        }

        public object TipoObject { get => _tipoObject; set => _tipoObject = value; }
    }
}
