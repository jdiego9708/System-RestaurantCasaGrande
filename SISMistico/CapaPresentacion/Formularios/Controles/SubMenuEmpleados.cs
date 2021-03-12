using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using CapaPresentacion.Formularios.FormsEmpleados;
using CapaPresentacion.Formularios.FormsPrincipales;

namespace CapaPresentacion.Formularios.Controles
{
    public partial class SubMenuEmpleados : UserControl
    {
        public SubMenuEmpleados()
        {
            InitializeComponent();
            this.btnAgregarEmpleado.Click += BtnAgregarEmpleado_Click;
            this.btnEditarEmpleado.Click += BtnEditarEmpleado_Click;
            this.btnObservarEmpleados.Click += BtnObservarEmpleados_Click;
        }

        public Panel panelForm;

        private void BtnObservarEmpleados_Click(object sender, EventArgs e)
        {
            bool existe = false;

            foreach (Form form in this.panelForm.Controls)
            {
                if (form is FrmObservarEmpleados)
                {
                    form.WindowState = FormWindowState.Normal;
                    form.Activate();
                    existe = true;
                }
            }

            if (existe == false)
            {
                FrmObservarEmpleados Frm = new FrmObservarEmpleados();
                Frm.TopLevel = false;
                Frm.FormBorderStyle = FormBorderStyle.Fixed3D;
                this.panelForm.Controls.Add(Frm);
                this.panelForm.Tag = Frm;
                Frm.Show();
            }
            this.Dispose();
        }

        private void BtnEditarEmpleado_Click(object sender, EventArgs e)
        {
            bool existe = false;

            FrmEditarEmpleado Frm = new FrmEditarEmpleado();
            FrmObservarEmpleados FrmObservarEmpleados = new FrmObservarEmpleados();
            FrmObservarEmpleados.StartPosition = FormStartPosition.CenterScreen;
            FrmObservarEmpleados.FrmEditarEmpleado = Frm;
            FrmObservarEmpleados.ShowDialog();

            if (Frm.Tag != null)
            {
                foreach (Form form in this.panelForm.Controls)
                {
                    if (form is FrmEditarEmpleado)
                    {
                        form.WindowState = FormWindowState.Normal;
                        form.Activate();
                        existe = true;
                    }
                }

                if (existe == false)
                {
                    Frm.TopLevel = false;
                    Frm.FormBorderStyle = FormBorderStyle.Fixed3D;
                    this.panelForm.Controls.Add(Frm);
                    this.panelForm.Tag = Frm;
                    Frm.Show();
                }
            }
            this.Dispose();
        }

        private void BtnAgregarEmpleado_Click(object sender, EventArgs e)
        {
            bool existe = false;

            foreach (Form form in this.panelForm.Controls)
            {
                if (form is FrmAgregarEmpleado)
                {
                    form.WindowState = FormWindowState.Normal;
                    form.Activate();
                    existe = true;
                }
            }

            if (existe == false)
            {
                FrmAgregarEmpleado Frm = new FrmAgregarEmpleado();
                Frm.TopLevel = false;
                Frm.FormBorderStyle = FormBorderStyle.Fixed3D;
                this.panelForm.Controls.Add(Frm);
                this.panelForm.Tag = Frm;
                Frm.Show();
            }
            this.Dispose();
        }
    }
}
