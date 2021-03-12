using System;
using System.Configuration;
using System.Windows.Forms;

namespace CapaPresentacion.Formularios.FormsPrincipales
{
    public partial class FrmFunciones : Form
    {
        public FrmFunciones()
        {
            InitializeComponent();
            this.btnLicenciaCompleta.Click += BtnLicenciaCompleta_Click;
            this.btnPeriodoPrueba.Click += BtnPeriodoPrueba_Click;
            this.Load += FrmFunciones_Load;
        }

        private void BtnLicenciaCompleta_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            string tipo = Convert.ToString(btn.Tag);
            if (tipo.Equals("RESERVAS"))
            {
                Mensajes.InputBox("Escriba el número de licencia versión completa",
                    "Activar", "Cancelar", out DialogResult result, out string clave);
                if (result == DialogResult.Yes)
                {
                    if (clave.Equals(""))
                    {
                        Mensajes.MensajeInformacion("Licencia no válida, el campo está vacío", "Entendido");
                    }
                    else
                    {
                        string claveProductoFull =
                            ConfigurationManager.AppSettings["claveLicenciaFullModuloReserva"].ToString();
                        if (clave.Equals(claveProductoFull))
                        {
                            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                            config.AppSettings.Settings["fechaActivacionFull"].Value = DateTime.Now.ToShortDateString();
                            config.AppSettings.Settings["moduloReservaFull"].Value = "true";
                            config.Save(ConfigurationSaveMode.Modified, true);
                            ConfigurationManager.RefreshSection("appSettings");
                            Mensajes.MensajeInformacion("Se activó correctamente la versión completa, " +
                                "reinicie la aplicación para que los cambios surgan efecto", "Entendido");
                        }
                        else
                        {
                            Mensajes.MensajeInformacion("La clave introducida no concuerda " +
                                "con ninguna de nuestra base de datos, intente de nuevo", "Entendido");
                        }
                    }
                }
            }
        }

        private void FrmFunciones_Load(object sender, EventArgs e)
        {
            bool producto =
               Convert.ToBoolean(ConfigurationManager.AppSettings["moduloReservaFull"]);
            if (producto)
            {
                this.gbReservas.Text = "Reservas (Módulo adquirido)";
                this.btnPeriodoPrueba.Enabled = false;
                this.btnLicenciaCompleta.Enabled = false;
            }
            else
            {
                producto =
                    Convert.ToBoolean(ConfigurationManager.AppSettings["moduloReservaTrial"]);
                if (producto)
                    this.btnPeriodoPrueba.Enabled = false;
            }

        }

        private void BtnPeriodoPrueba_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            string tipo = Convert.ToString(btn.Tag);
            if (tipo.Equals("RESERVAS"))
            {
                Mensajes.InputBox("Escriba el número de licencia versión trial",
                    "Activar", "Cancelar", out DialogResult result, out string clave);
                if (result == DialogResult.Yes)
                {
                    if (clave.Equals(""))
                    {
                        Mensajes.MensajeInformacion("Licencia no válida, el campo está vacío", "Entendido");
                    }
                    else
                    {
                        string claveProductoTrial =
                            ConfigurationManager.AppSettings["claveLicenciaTrialModuloReserva"].ToString();
                        if (clave.Equals(claveProductoTrial))
                        {
                            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                            config.AppSettings.Settings["fechaActivacionTrial"].Value = DateTime.Now.ToShortDateString();
                            config.AppSettings.Settings["moduloReservaTrial"].Value = "true";
                            config.Save(ConfigurationSaveMode.Modified, true);
                            ConfigurationManager.RefreshSection("appSettings");
                            Mensajes.MensajeInformacion("Se activó correctamente la versión trial, " +
                                "reinicie la aplicación para que los cambios surgan efecto", "Entendido");
                        }
                        else
                        {
                            Mensajes.MensajeInformacion("La clave introducida no concuerda " +
                                "con ninguna de nuestra base de datos, intente de nuevo", "Entendido");
                        }
                    }
                }


            }
        }
    }
}
