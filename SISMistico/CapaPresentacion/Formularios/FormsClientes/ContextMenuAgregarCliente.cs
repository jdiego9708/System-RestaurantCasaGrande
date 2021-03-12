using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CapaPresentacion.Formularios.FormsClientes
{
    public partial class ContextMenuAgregarCliente : UserControl
    {
        public ContextMenuAgregarCliente()
        {
            InitializeComponent();
            this.Load += ContextMenuAgregarCliente_Load;
        }

        public FrmObservarClientes FrmObservarClientes;

        private void ContextMenuAgregarCliente_Load(object sender, EventArgs e)
        {
            if (this.panel1.Controls.Count > 0)
                this.panel1.Controls.RemoveAt(0);
            FrmAgregarCliente Frm = new FrmAgregarCliente();
            Frm.FrmObservarClientes = this.FrmObservarClientes;
            Frm.TopLevel = false;
            Frm.FormBorderStyle = FormBorderStyle.None;
            Frm.Dock = DockStyle.Fill;
            this.panel1.Controls.Add(Frm);
            this.panel1.Tag = Frm;
            Frm.Show();
        }
    }
}
