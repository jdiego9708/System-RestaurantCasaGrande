using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CapaPresentacion
{
    public partial class FrmWait : Form
    {
        #region PATRON SINGLETON
        //private static FrmWait _Instancia;
        //public static FrmWait GetInstancia()
        //{
        //    if (_Instancia == null || _Instancia.IsDisposed)
        //    {
        //        _Instancia = new FrmWait();
        //    }
        //    return _Instancia;
        //}
        #endregion

        public FrmWait()
        {
            InitializeComponent();
            this.Load += FrmWait_Load;
            this.btnCerrar.Click += BtnCerrar_Click;
        }

        private void BtnCerrar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void FrmWait_Load(object sender, EventArgs e)
        {
            this.txtMensaje.Text = this.Mensaje;
        }

        public string Mensaje { get; set; }
    }
}
