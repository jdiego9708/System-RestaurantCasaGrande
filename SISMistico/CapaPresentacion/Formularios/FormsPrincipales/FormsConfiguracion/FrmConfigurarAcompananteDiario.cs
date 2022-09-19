using CapaNegocio;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CapaPresentacion.Formularios.FormsPrincipales.FormsConfiguracion
{
    public partial class FrmConfigurarAcompananteDiario : Form
    {
        public FrmConfigurarAcompananteDiario()
        {
            InitializeComponent();
            this.btnSave.Click += BtnSave_Click;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            string rpta = NPlatos.ActualizarAcompanante(3030, this.txtAcompanante1.Text.ToUpper());
            if (rpta.Equals("OK"))
            {
                if (!string.IsNullOrEmpty(this.txtAcompanante2.Text))
                {
                    rpta = NPlatos.ActualizarAcompanante(3032, this.txtAcompanante2.Text.ToUpper());
                    if (rpta.Equals("OK"))
                    {
                        Mensajes.MensajeInformacion("Se actualizó correctamente los acompañantes del día");
                        this.Close();
                    }
                    else
                    {
                        Mensajes.MensajeInformacion("Se actualizó correctamente el acompañante 1 pero no el 2");
                        this.Close();
                    }
                }
            }
            else
            {
                Mensajes.MensajeInformacion("No se pudo actualizar el acompañante 1");
                this.Close();
            }
        }
    }
}
