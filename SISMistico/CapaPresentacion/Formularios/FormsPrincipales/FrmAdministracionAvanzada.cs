using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Configuration;
using System.Data.SqlClient;
using MetroFramework.Controls;

using CapaPresentacion.Formularios.FormsPrincipales.FormsConfiguracion;

namespace CapaPresentacion.Formularios.FormsPrincipales
{
    public partial class FrmAdministracionAvanzada : Form
    {
        public FrmAdministracionAvanzada()
        {
            InitializeComponent();
            this.Load += FrmAdministracionAvanzada_Load;
            this.btnCancelar.Click += BtnCancelar_Click;
            this.ListaCadenas.SelectedIndexChanged += ListaCadenas_SelectedIndexChanged;
            this.rdSeguridadIntegrada.CheckedChanged += RdSeguridadIntegrada_CheckedChanged;
            this.chkMostrar.CheckedChanged += ChkMostrar_CheckedChanged;
            this.btnGuardar.Click += BtnGuardar_Click;
            this.btnGuardarDatosEmail.Click += BtnGuardarDatosEmail_Click;
            this.btnCambiarRuta.Click += BtnCambiarRuta_Click;
            this.btnGuardarRuta.Click += BtnGuardarRuta_Click;
            this.btnGuardarServicio.Click += BtnGuardarServicio_Click;
        }

        private void ObtenerConfiguracionReportes()
        {
            FrmConfiguracionReportes frmConfiguracionReportes = new FrmConfiguracionReportes
            {
                FormBorderStyle = FormBorderStyle.None,
                Dock = DockStyle.Fill,
                TopLevel = false
            };
            this.metroTabPage5.Controls.Add(frmConfiguracionReportes);
            frmConfiguracionReportes.Show();
        }

        private void BtnGuardarServicio_Click(object sender, EventArgs e)
        {
            try
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                config.AppSettings.Settings["nameServiceStarter"].Value = this.txtServicioStart.Texto;
                config.Save(ConfigurationSaveMode.Modified, true);
                ConfigurationManager.RefreshSection("appSettings");
                Mensajes.MensajeOkForm("Se actualizó correctamente el servicio a iniciar");
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "BtnGuardarServicio_Click", 
                    "Hubo un error al guardar los datos de inicio de un servicio",
                    ex.Message);
            }
        }

        private void BtnGuardarRuta_Click(object sender, EventArgs e)
        {
            try
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                config.AppSettings.Settings["RUTAIMAGES"].Value = this.txtRutaImagenesNew.Text + "\\";
                config.Save(ConfigurationSaveMode.Modified, true);
                ConfigurationManager.RefreshSection("appSettings");
                Mensajes.MensajeOkForm("Se actualizó correctamente la ruta de imágenes");
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorForm(ex.Message);
            }
        }

        private void BtnCambiarRuta_Click(object sender, EventArgs e)
        {
            try
            {
                //Creo un objeto de tipo OpenFileDialog y lo instancio
                FolderBrowserDialog folder = new FolderBrowserDialog();
                //Lo abro como un Dialog
                DialogResult result = folder.ShowDialog();
                if (result == DialogResult.OK)
                {
                    this.txtRutaImagenesNew.Text = folder.SelectedPath;
                }
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "BtnCambiarRuta_Click",
                    "Hubo un error al seleccionar una carpeta", ex.Message);
            }
        }

        private void BtnGuardarDatosEmail_Click(object sender, EventArgs e)
        {
            try
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                config.AppSettings.Settings["eMailTo"].Value = this.txtDestinatario.Text;
                config.AppSettings.Settings["eMailFrom"].Value = this.txtRemitente.Text;
                config.AppSettings.Settings["eMailSMTP"].Value = this.txtSMTP.Text;
                config.AppSettings.Settings["eMailPass"].Value = this.txtContra.Text;
                config.AppSettings.Settings["portEmail"].Value = this.txtPuerto.Text;
                config.Save(ConfigurationSaveMode.Modified, true);
                ConfigurationManager.RefreshSection("appSettings");

                if (this.chkPrueba.Checked)
                {
                    Mensajes.MensajeOkForm("Se enviará un correo electrónico de prueba");
                    EnviarEmailErrores.SendEmailError("PRUEBA", "PRUEBA", "PRUEBA", "PRUEBA");
                }
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorForm(ex.Message);
            }
        }

        private void AsignarDatosEmail()
        {
            try
            {
                this.txtDestinatario.Text = ConfigurationManager.AppSettings["eMailTo"].ToString();
                this.txtRemitente.Text = ConfigurationManager.AppSettings["eMailFrom"].ToString();
                this.txtSMTP.Text = ConfigurationManager.AppSettings["eMailSMTP"].ToString();
                this.txtPuerto.Text = ConfigurationManager.AppSettings["portEmail"].ToString();
                this.txtContra.Text = ConfigurationManager.AppSettings["eMailPass"].ToString();

                this.txtService.Texto = ConfigurationManager.AppSettings["nameServiceStarter"].ToString();
                this.txtRutaImagenes.Text = ConfigurationManager.AppSettings["RUTAIMAGES"].ToString();
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorForm("Hubo un error al asignar los datos del Email " + ex.Message);
            }
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                if (rdSeguridadIntegrada.Checked == true)
                {
                    this.CambiarDatos(this.txtServidor.Text, this.txtBaseDeDatos.Text, "", "", true, this.ListaCadenas.Text);
                }
                else
                {
                    this.CambiarDatos(this.txtServidor.Text, this.txtBaseDeDatos.Text, this.txtUsuario.Text, this.txtContraseña.Text, false, this.ListaCadenas.Text);
                }

                if (this.chkConectarse.Checked == true)
                {
                    try
                    {
                        Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                        config.AppSettings.Settings["ConexionBaseDeDatos"].Value = this.ListaCadenas.Text;
                        config.Save(ConfigurationSaveMode.Modified, true);
                        ConfigurationManager.RefreshSection("appSettings");
                    }
                    catch (Exception ex)
                    {
                        Mensajes.MensajeErrorForm(ex.Message);
                    }
                }

                Mensajes.MensajeOkForm("La aplicación se cerrará para guardar los cambios, debe de ejecutarla de nuevo, la aplicación se conectará a " + this.ListaCadenas.Text);
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorForm("Hubo un error al guardar los datos de la cadena de conexión" + ex.Message);
            }
        }

        private void ChkMostrar_CheckedChanged(object sender, EventArgs e)
        {
            MetroCheckBox chk = (MetroCheckBox)sender;
            if (chk.Checked)
            {
                this.txtContraseña.UseSystemPasswordChar = false;
            }
            else
            {
                this.txtContraseña.UseSystemPasswordChar = true;
            }
        }

        private void RdSeguridadIntegrada_CheckedChanged(object sender, EventArgs e)
        {
            if (this.rdSeguridadIntegrada.Checked == true)
            {
                this.PanelCredenciales.Visible = false;
            }
            else
            {
                this.PanelCredenciales.Visible = true;
            }
        }

        private void ListaCadenas_SelectedIndexChanged(object sender, EventArgs e)
        {
            MetroComboBox lista = (MetroComboBox)sender;
            try
            {
                this.ObtenerCadenaConexionPorNombre(lista.Text);
            }
            catch (Exception ex)
            {
                this.metroToolTip1.SetToolTip(this.lblCadena, ex.Message);
            }
        }

        private void BtnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void FrmAdministracionAvanzada_Load(object sender, EventArgs e)
        {
            this.ListaCadenas.DataSource = this.ObtenerListaCadenasConexion();
            this.txtContraseña.UseSystemPasswordChar = true;
            this.AsignarDatosEmail();
            this.metroTabControl1.SelectedIndex = 0;

            this.txtService.AsignarTexto();
            this.txtServicioStart.Texto_inicial = "Servicio a iniciar";
            this.txtService.Visible_px = false;
            this.txtServicioStart.Visible_px = false;

            this.ObtenerConfiguracionReportes();
        }

        private List<string> ObtenerListaCadenasConexion()
        {
            List<string> cns = new List<string>();
            Configuration appconfig =
                ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            foreach (ConnectionStringSettings cn in appconfig.ConnectionStrings.ConnectionStrings)
            {
                cns.Add(cn.Name);
            }
            return cns;
        }

        private void ObtenerCadenaConexionPorNombre(string Nombre_cadena)
        {
            Configuration appconfig =
                ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            ConnectionStringSettings connStringSettings = appconfig.ConnectionStrings.ConnectionStrings[Nombre_cadena];

            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connStringSettings.ConnectionString);

            this.lblCadena.Text = connStringSettings.ConnectionString;
            this.txtServidor.Text = builder.DataSource;
            this.txtBaseDeDatos.Text = builder.InitialCatalog;
            this.txtUsuario.Text = builder.UserID;
            this.txtContraseña.Text = builder.Password;

            if (builder.IntegratedSecurity.Equals(true))
            {
                this.rdSeguridadIntegrada.Checked = true;
                this.rdSeguridadIntegrada.CheckedChanged += new EventHandler(this.RdSeguridadIntegrada_CheckedChanged);
            }
            else
            {
                this.rdSeguridadIntegrada.Checked = false;
                this.rdSeguridadIntegrada.CheckedChanged += new EventHandler(this.RdSeguridadIntegrada_CheckedChanged);
            }
            this.metroToolTip1.SetToolTip(this.lblCadena, this.lblCadena.Text);
        }

        private void CambiarDatos(string servidor, string baseDeDatos, string usuario, string pass, bool integrated_security, string nombre_conexion)
        {
            String cadenaNueva = "";
            if (integrated_security == true)
            {
                cadenaNueva = "Data Source =" + servidor + "; Initial Catalog =" + baseDeDatos + "; Integrated Security =" + integrated_security;
            }
            else
            {
                cadenaNueva = "Data Source =" + servidor + "; Initial Catalog =" + baseDeDatos + "; User ID=" + usuario + "; Password= " + pass;
            }
            //abrimos la configuración de nuestro proyecto
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            //hacemos la modificacion de la cadena de conexion (ServerDb es el atributo que tengo en app.config) 
            config.ConnectionStrings.ConnectionStrings[nombre_conexion].ConnectionString = cadenaNueva;
            //Cambiamos el modo de guardado
            config.Save(ConfigurationSaveMode.Modified, true);
            //modificamos el guardado 
            ConfigurationManager.RefreshSection("connectionStrings");
        }
    }
}
