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

using CapaNegocio;

namespace CapaPresentacion.Formularios.FormsPedido
{
    public partial class DatosUsuario : UserControl
    {
        public DatosUsuario()
        {
            InitializeComponent();
            this.Load += DatosUsuario_Load;
        }

        public event EventHandler onChangedEmail;

        private void DatosUsuario_Load(object sender, EventArgs e)
        {
            DataTable table = NClientes.BuscarClientes("ID CLIENTE", this.Id_cliente.ToString());
            if (table != null)
            {
                string rpta;
                if (this.panel1.Controls.Count > 0)
                    this.panel1.Controls.RemoveAt(0);
                FrmAgregarCliente Frm = new FrmAgregarCliente();
                Frm.ObtenerDatos(DatagridString.ReturnValuesOfCells(table, 0, out rpta));
                Frm.TopLevel = false;
                Frm.IsPedido = true;
                Frm.onChangedEmail += Frm_onChangedEmail;
                Frm.FormBorderStyle = FormBorderStyle.None;
                Frm.Dock = DockStyle.Fill;
                this.panel1.Controls.Add(Frm);
                this.panel1.Tag = Frm;
                Frm.Show();
            }
        }

        private void Frm_onChangedEmail(object sender, EventArgs e)
        {
            if (this.onChangedEmail != null)
                this.onChangedEmail(sender, e);
        }

        private int id_cliente;

        public int Id_cliente { get => id_cliente; set => id_cliente = value; }
    }
}
