using CapaEntidades.Models;
using CapaNegocio;
using System;
using System.Configuration;
using System.Data;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CapaPresentacion.Formularios.FormsPrincipales
{
    public partial class FrmIniciarSesion : Form
    {
        public FrmIniciarSesion()
        {
            InitializeComponent();
            this.Load += FrmIniciarSesion_Load;
            this.ListaEmpleados.SelectedIndexChanged += ListaEmpleados_SelectedIndexChanged;
            this.btnCerrar.Click += BtnCerrar_Click;
            this.btnIngresar.Click += BtnIngresar_Click;
            this.FormClosing += FrmIniciarSesion_FormClosing;
            this.txtPass.onKeyPress += TxtPass_onKeyPress;
        }

        private async void TxtPass_onKeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (int)Keys.Enter)
            {
                await Login();
            }
        }

        private void FrmIniciarSesion_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }

        private async Task Login()
        {
            try
            {
                if (this.ListaEmpleados.Items.Count > 0 & !string.IsNullOrEmpty(this.txtPass.Texto))
                {
                    if (int.TryParse(Convert.ToString(this.ListaEmpleados.SelectedValue), out int id_empleado))
                    {
                        if (id_empleado == 0)
                        {
                            if (this.txtPass.Texto.Equals("administrador"))
                            {
                                DatosInicioSesion datos = DatosInicioSesion.GetInstancia();
                                datos.Id_empleado = Convert.ToInt32(0);
                                datos.Nombre_empleado = Convert.ToString("Administrador");
                                datos.Cargo_empleado = "ADMINISTRADOR";

                                FrmPrincipal frmPrincipal = new FrmPrincipal();
                                frmPrincipal.WindowState = FormWindowState.Maximized;
                                frmPrincipal.Show();

                                this.Hide();
                            }
                            else if (this.txtPass.Texto.Equals("configadmin"))
                            {
                                FrmAdministracionAvanzada frm = new FrmAdministracionAvanzada();
                                frm.StartPosition = FormStartPosition.CenterScreen;
                                frm.ShowDialog();
                            }
                        }
                        else
                        {
                            var (rpta, objects) = await NEmpleados.Login(Convert.ToString(id_empleado), this.txtPass.Texto, DateTime.Now.ToString("yyyy-MM-dd"));
                            if (rpta.Equals("OK"))
                            {
                                Empleados empleado = (Empleados)objects[0];
                                Turno turno = (Turno)objects[1];

                                DatosInicioSesion datos = DatosInicioSesion.GetInstancia();
                                datos.Id_empleado = empleado.Id_empleado;
                                datos.Nombre_empleado = empleado.Nombre_empleado;
                                datos.Cargo_empleado = empleado.Cargo_empleado;
                                datos.EmpleadoLogin = empleado;
                                datos.Turno = turno;
                                datos.EmpleadoLogin = empleado;
                                datos.EmpleadoClaveMaestra = empleado;
                                datos.ClienteDefault = new Clientes
                                {
                                    Id_cliente = 0,
                                    Nombre_cliente = "NINGUNO",
                                    Telefono_cliente = "NINGUNO",
                                    Correo_electronico = string.Empty,
                                    Direccion_cliente = "NINGUNO",
                                    Referencia_ubicacion = string.Empty,
                                    Otras_observaciones = string.Empty,
                                    Estado_cliente = "ACTIVO",
                                };

                                FrmPrincipal frmPrincipal = new FrmPrincipal
                                {
                                    WindowState = FormWindowState.Maximized
                                };
                                frmPrincipal.Show();

                                this.Hide();
                            }
                            else if (rpta.Equals(""))
                            {
                                Mensajes.MensajeInformacion("No se encontró el usuario, intentelo de nuevo", "Entendido");
                            }
                            else
                            {
                                throw new Exception(rpta);
                            }
                        }
                    }

                }
                else if (this.ListaEmpleados.Items.Count < 1)
                {
                    if (this.txtPass.Texto.Equals("configadmin"))
                    {
                        FrmAdministracionAvanzada frm = new FrmAdministracionAvanzada();
                        frm.StartPosition = FormStartPosition.CenterScreen;
                        frm.ShowDialog();
                    }
                }
                else
                {
                    Mensajes.MensajeErrorForm("La contraseña es obligatoria");
                }
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "BtnIngresar_Click",
                    "Hubo un error al ingresar", ex.Message);
            }

            //try
            //{
            //    Invoke(new MethodInvoker(async () =>
            //    {
                    
            //    }));
            //}
            //catch (Exception ex)
            //{
            //    Mensajes.MensajeInformacion("Hubo un error iniciando sesión, detalles: " + ex.Message, "Entendido");
            //}
            
        }

        private async void BtnIngresar_Click(object sender, EventArgs e)
        {
            await Login();
        }

        private void BtnCerrar_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void ListaEmpleados_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox cb = (ComboBox)sender;
            int id_empleado;
            if (int.TryParse(Convert.ToString(cb.SelectedValue), out id_empleado))
            {
                this.txtPass.txtBusqueda.Focus();
            }
        }

        private void FrmIniciarSesion_Load(object sender, EventArgs e)
        {
            this.txtPass.txtBusqueda.TextAlign = HorizontalAlignment.Center;
            this.txtPass.Texto_inicial = "Contraseña";
            this.txtPass.txtBusqueda.UseSystemPasswordChar = true;

            string rpta;
            DataTable tablaEmpleados =
                NEmpleados.BuscarEmpleados("COMPLETO", "", out rpta);
            if (tablaEmpleados != null)
            {
                this.ListaEmpleados.DataSource = tablaEmpleados;
                this.ListaEmpleados.ValueMember = "Id_empleado";
                this.ListaEmpleados.DisplayMember = "Nombre_empleado";

                this.ListaEmpleados.SelectedValue = 0;
            }
            else
            {
                Mensajes.MensajePregunta("Hubo un error conectando con el servidor, desea intentar de nuevo?",
                    "Intentar de nuevo", "Cerrar", out DialogResult dialog);
                if (dialog == DialogResult.Yes)
                {
                    string servicio = Convert.ToString(ConfigurationManager.AppSettings["nameServiceStarter"]);
                    ServiceController sc = new ServiceController(servicio);
                    try
                    {
                        if (sc != null && sc.Status == ServiceControllerStatus.Stopped)
                        {
                            sc.Start();
                        }
                        sc.WaitForStatus(ServiceControllerStatus.Running);
                        sc.Close();
                    }
                    catch (Exception ex)
                    {
                        Mensajes.MensajeErrorCompleto(this.Name, "Iniciar servicio",
                            "Error al iniciar el servicio: ", ex.Message);
                    }
                }

                Mensajes.MensajeErrorCompleto(this.Name, "FrmIniciarSesion_Load",
                    "Hubo un error al conectarse con el servidor",
                    "Hubo un error al conectarse con el servidor, por favor intentelo de nuevo o envíe un ticket " +
                    "al administrador del sistema, detalles: " + rpta);
            }
        }

        //protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        //{
        //    if (keyData == Keys.Enter || keyData == Keys.Insert)
        //    {
        //        try
        //        {
        //            Task.Run(() => this.Login());
        //        }
        //        catch (Exception ex)
        //        {
        //            Mensajes.MensajeErrorForm(ex.Message);
        //        }
        //    }
        //    else if (keyData == Keys.Escape)
        //    {
        //        this.Close();
        //    }
        //    else
        //    {
        //        return base.ProcessCmdKey(ref msg, keyData);
        //    }
        //    return true;
        //}
    }
}
