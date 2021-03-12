using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Runtime.InteropServices;

namespace CapaPresentacion.Formularios.FormsPedido
{
    public partial class FrmObservacionPedido : Form
    {
        public FrmObservacionPedido()
        {
            InitializeComponent();
            this.Load += FrmObservacionPedido_Load;
            this.btnListo.Click += BtnListo_Click;
            #region Custom ToolBox
            this.pxClose.Click += PxClose_Click;
            this.pxMinimize.Click += PxMinimize_Click;
            this.pxMaximize.Click += PxMaximize_Click;
            this.panelToolBox.MouseDown += PanelToolBox_MouseDown;
            #endregion

        }

        private void AsignarObservacion()
        {
            this.Observacion = this.txtObservacion.Text.Trim().ToUpper();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void BtnListo_Click(object sender, EventArgs e)
        {
            this.AsignarObservacion();
        }

        private int _id_tipo;
        private string _observacion;
        private string _tipo;

        public string Observacion { get => _observacion; set => _observacion = value; }
        public int Id_tipo { get => _id_tipo; set => _id_tipo = value; }
        public string Tipo { get => _tipo; set => _tipo = value; }

        private void FrmObservacionPedido_Load(object sender, EventArgs e)
        {
            if (this.Observacion != null)
            {
                this.txtObservacion.Text = this.Observacion;
            }

            this.pxMaximize.Visible = false;
            this.pxMinimize.Visible = false;
        }

        #region Custom ToolBox
        private void PanelToolBox_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();

        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hand, int wmsg, int wparam, int lparam);

        private void PxMaximize_Click(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.WindowState = FormWindowState.Normal;
            }
            else if (this.WindowState == FormWindowState.Normal)
            {
                this.WindowState = FormWindowState.Maximized;
            }
            else if (this.WindowState == FormWindowState.Maximized)
            {
                this.WindowState = FormWindowState.Normal;
            }
        }

        private void PxMinimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void PxClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        #endregion
    }
}
