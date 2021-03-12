using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using CapaPresentacion.Servicios.FormsCorreoTicket;
using CapaPresentacion.Servicios;

namespace CapaPresentacion.Formularios.FormsPrincipales
{
    public partial class OpcionesUsuario : UserControl
    {
        public OpcionesUsuario()
        {
            InitializeComponent();
            this.btnReportes.Click += BtnReportes_Click;
        }

        private void BtnReportes_Click(object sender, EventArgs e)
        {
            FrmEmailTicket formEmail = new FrmEmailTicket
            {
                StartPosition = FormStartPosition.CenterScreen
            };
            formEmail.Show();
        }
    }
}
