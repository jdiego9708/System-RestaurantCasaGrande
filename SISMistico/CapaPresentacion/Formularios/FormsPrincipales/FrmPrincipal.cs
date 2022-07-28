using CapaPresentacion.Formularios.FormsBebidas;
using CapaPresentacion.Formularios.FormsClientes;
using CapaPresentacion.Formularios.FormsDetalles;
using CapaPresentacion.Formularios.FormsEmpleados;
using CapaPresentacion.Formularios.FormsInsumos;
using CapaPresentacion.Formularios.FormsPedido;
using CapaPresentacion.Formularios.FormsPlatos;
using CapaPresentacion.Formularios.FormsReservas;
using CapaPresentacion.Formularios.FormsVentas;
using CapaPresentacion.Formularios.FormsPrincipales.Menus;
using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CapaNegocio;
using CapaPresentacion.Formularios.FormsNotas;
using CapaPresentacion.Formularios.FormsMovimientos;
using CapaPresentacion.Formularios.FormsEstadisticas;
using CapaEntidades.Models;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using CapaPresentacion.Formularios.FormsPrincipales.FormsConfiguracion;

namespace CapaPresentacion.Formularios.FormsPrincipales
{
    public partial class FrmPrincipal : Form
    {
        PoperContainer container;
        PoperContainer containerOpcionesUsuario;
        OpcionesUsuario opcionesUsuario;

        public FrmPrincipal()
        {
            InitializeComponent();

            this.opcionesUsuario = new OpcionesUsuario();
            this.containerOpcionesUsuario = new PoperContainer(opcionesUsuario);
            this.Load += FrmPrincipal_Load;
            this.lblMistico.DoubleClick += LblMistico_DoubleClick;
            this.btnEmpleados.Click += BtnEmpleados_Click;
            this.btnAdministracionAvanzada.Click += BtnAdministracionAvanzada_Click;
            this.btnPlatos.Click += BtnPlatos_Click;
            this.btnBebidas.Click += BtnBebidas_Click;
            this.btnClientes.Click += BtnClientes_Click;
            this.btnMesasPedidos.Click += BtnMesasPedidos_Click;
            this.btnInsumos.Click += BtnInsumos_Click;
            this.btnUsuario.Click += BtnUsuario_Click;
            this.FormClosing += FrmPrincipal_FormClosing;
            this.opcionesUsuario.btnCerrarSesion.Click += BtnCerrarSesion_Click;
            this.opcionesUsuario.btnFunciones.Click += BtnFunciones_Click;
            this.btnReservas.Click += BtnReservas_Click;
            this.btnAdministracion.Click += BtnAdministracion_Click;
        }

        private void BtnAdministracion_Click(object sender, EventArgs e)
        {
            MenuAdministracion menuAdmin = new MenuAdministracion();
            menuAdmin.btnEstadisticasDiarias.Click += BtnEstadisticasDiarias_Click;
            menuAdmin.btnObservarMovimientos.Click += BtnObservarMovimientos_Click;
            menuAdmin.btnHistorialVentas.Click += BtnHistorialVentas_Click;
            container = new PoperContainer(menuAdmin);
            container.Show(this.btnAdministracion);
        }

        private void BtnHistorialVentas_Click(object sender, EventArgs e)
        {
            FrmObservarVentas frmObservarVentas = new FrmObservarVentas
            {
                WindowState = FormWindowState.Maximized,
            };
            frmObservarVentas.Show();
        }

        private void BtnObservarMovimientos_Click(object sender, EventArgs e)
        {
            try
            {
                FrmObservarMovimientos frm = new FrmObservarMovimientos();
                frm.TopLevel = false;
                frm.WindowState = FormWindowState.Maximized;
                frm.MaximizeBox = false;
                Form FormComprobado = this.ComprobarExistencia(frm);
                if (FormComprobado != null)
                {
                    frm.Activate();
                }
                else
                {
                    frm.FormBorderStyle = FormBorderStyle.Fixed3D;
                    this.panel1.Controls.Add(frm);
                    this.panel1.Tag = frm;
                    frm.Show();
                }
                frm.BringToFront();
                frm.Activate();
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "BtnObservarMovimientos_Click",
                    "Hubo un error con el menu observar movimientos", ex.Message);
            }
        }

        private async void BtnEstadisticasDiarias_Click(object sender, EventArgs e)
        {
            FrmReporteDiario FrmReporteDiario = new FrmReporteDiario
            {
                WindowState = FormWindowState.Maximized
            };
            await FrmReporteDiario.LoadEstadistica(DateTime.Now, DateTime.Now);
            FrmReporteDiario.Show();
        }

        private void BtnFunciones_Click(object sender, EventArgs e)
        {
            try
            {
                FrmFunciones frm = new FrmFunciones
                {
                    TopLevel = false
                };
                Form FormComprobado = this.ComprobarExistencia(frm);
                if (FormComprobado != null)
                {
                    frm.WindowState = FormWindowState.Normal;
                    frm.Activate();
                }
                else
                {
                    frm.FormBorderStyle = FormBorderStyle.Fixed3D;
                    this.panel1.Controls.Add(frm);
                    this.panel1.Tag = frm;
                    frm.Show();
                }
                frm.BringToFront();
                frm.Activate();
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "BtnFunciones_Click",
                    "Hubo un error con el formulario funciones", ex.Message);
            }
        }

        private void BtnReservas_Click(object sender, EventArgs e)
        {
            try
            {
                FrmObservarReservas frm = new FrmObservarReservas
                {
                    TopLevel = false,
                    WindowState = FormWindowState.Normal
                };
                frm.StartPosition = FormStartPosition.CenterParent;
                frm.OnReservaRefresh += Frm_OnReservaRefresh;
                Form FormComprobado = this.ComprobarExistencia(frm);
                if (FormComprobado != null)
                {
                    frm.WindowState = FormWindowState.Normal;
                    frm.Activate();
                }
                else
                {
                    frm.FormBorderStyle = FormBorderStyle.Fixed3D;
                    this.panel1.Controls.Add(frm);
                    this.panel1.Tag = frm;
                    frm.Show();
                }
                frm.BringToFront();
                frm.Activate();
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "BtnReservas_Click",
                    "Hubo un error con el menu observar las reservas", ex.Message);
            }
        }

        private void Frm_OnReservaRefresh(object sender, EventArgs e)
        {
            if (this.VentanaInicial != null)
            {
                VentanaInicial.ObtenerReservas();
            }
            else
            {
                VentanaInicial = new FrmVentanaInicial
                {
                    Location = new Point(0, 0),
                    Size = new Size(this.panel1.Width / 2,
                    this.panel1.Height / 2),
                    FormBorderStyle = FormBorderStyle.None,
                    TopLevel = false,
                    Opacity = 0.5
                };
                VentanaInicial.ObtenerReservas();
                this.panel1.Controls.Add(VentanaInicial);
                VentanaInicial.Show();
            }
        }

        private void BtnCerrarSesion_Click(object sender, EventArgs e)
        {
            Form formExiste =
                this.panel1.Controls.OfType<Form>().Where(pre => pre.Name == "FrmIniciarSesion").SingleOrDefault<Form>();
            if (formExiste != null)
            {
                formExiste.WindowState = FormWindowState.Normal;
                formExiste.Activate();
                formExiste.BringToFront();
                this.Hide();
            }
            else
            {
                FrmIniciarSesion frm = new FrmIniciarSesion
                {
                    StartPosition = FormStartPosition.CenterScreen
                };
                this.Hide();
                frm.ShowDialog();
            }
        }

        private void BtnUsuario_Click(object sender, EventArgs e)
        {
            this.containerOpcionesUsuario.Show(this.btnUsuario);
        }

        private void FrmPrincipal_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }

        private void BtnInsumos_Click(object sender, EventArgs e)
        {
            Button btnSender = (Button)sender;
            Point ptLowerLeft = new Point(btnSender.Width, btnSender.Height);
            ptLowerLeft = btnSender.PointToScreen(ptLowerLeft);
            this.MenuInsumos.Show(ptLowerLeft);
        }

        private void BtnMesasPedidos_Click(object sender, EventArgs e)
        {
            try
            {
                FrmObservarMesas frm = FrmObservarMesas.GetInstancia();
                frm.TopLevel = false;
                frm.WindowState = FormWindowState.Maximized;
                frm.MaximizeBox = false;
                Form FormComprobado = this.ComprobarExistencia(frm);
                if (FormComprobado != null)
                {
                    frm.Activate();
                }
                else
                {
                    frm.FormBorderStyle = FormBorderStyle.Fixed3D;
                    this.panel1.Controls.Add(frm);
                    this.panel1.Tag = frm;
                    frm.Show();
                }
                frm.BringToFront();
                frm.Activate();
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "ObservarMesas_Click",
                    "Hubo un error con el menu observar mesas y pedidos", ex.Message);
            }
        }

        private void BtnClientes_Click(object sender, EventArgs e)
        {
            Button btnSender = (Button)sender;
            Point ptLowerLeft = new Point(btnSender.Width, btnSender.Height);
            ptLowerLeft = btnSender.PointToScreen(ptLowerLeft);
            this.MenuClientes.Show(ptLowerLeft);
        }

        private void BtnBebidas_Click(object sender, EventArgs e)
        {
            Button btnSender = (Button)sender;
            Point ptLowerLeft = new Point(btnSender.Width, btnSender.Height);
            ptLowerLeft = btnSender.PointToScreen(ptLowerLeft);
            this.MenuBebidas.Show(ptLowerLeft);
        }

        private void BtnPlatos_Click(object sender, EventArgs e)
        {
            MenuPlatos menuPlatos = new MenuPlatos();
            menuPlatos.btnAgregarPlato.Click += BtnAgregarPlato_Click;
            menuPlatos.btnEditarAcompanante.Click += BtnEditarAcompanante_Click;
            menuPlatos.btnEditarPlato.Click += BtnEditarPlato_Click;
            menuPlatos.btnObservarPlatos.Click += BtnObservarPlatos_Click;
            menuPlatos.btnInactivarPlatos.Click += BtnInactivarPlatos_Click;
            container = new PoperContainer(menuPlatos);
            container.Show(this.btnPlatos);
        }

        private void BtnInactivarPlatos_Click(object sender, EventArgs e)
        {
            try
            {
                FrmObservarPlatos frm = new FrmObservarPlatos
                {
                    TopLevel = false,
                    InactivarPlatos = true
                };
                Form FormComprobado = this.ComprobarExistencia(frm);
                if (FormComprobado != null)
                {
                    frm.WindowState = FormWindowState.Normal;
                    frm.Activate();
                }
                else
                {
                    frm.FormBorderStyle = FormBorderStyle.Fixed3D;
                    this.panel1.Controls.Add(frm);
                    this.panel1.Tag = frm;
                    frm.Show();
                }
                frm.BringToFront();
                frm.Activate();
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "InactivarPlato_Click",
                    "Hubo un error con el menu observar platos para inactivar", ex.Message);
            }
        }

        private void BtnObservarPlatos_Click(object sender, EventArgs e)
        {
            try
            {
                FrmObservarPlatos frm = new FrmObservarPlatos
                {
                    TopLevel = false
                };
                Form FormComprobado = this.ComprobarExistencia(frm);
                if (FormComprobado != null)
                {
                    frm.WindowState = FormWindowState.Normal;
                    frm.Activate();
                }
                else
                {
                    frm.FormBorderStyle = FormBorderStyle.Fixed3D;
                    this.panel1.Controls.Add(frm);
                    this.panel1.Tag = frm;
                    frm.Show();
                }
                frm.BringToFront();
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "ObservarPlatos_Click",
                    "Hubo un error con el menu observar plato", ex.Message);
            }
        }

        private void BtnEditarPlato_Click(object sender, EventArgs e)
        {
            try
            {
                FrmObservarPlatos frmObservar = new FrmObservarPlatos
                {
                    StartPosition = FormStartPosition.CenterScreen,
                    IsEditar = true,
                };
                frmObservar.OnDgvDoubleClick += FrmObservar_OnDgvDoubleClick;
                frmObservar.ShowDialog();
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "EditarEmpleado_Click",
                    "Hubo un error con el menu Editar empleado", ex.Message);
            }
        }

        private void BtnEditarAcompanante_Click(object sender, EventArgs e)
        {
            FrmConfigurarAcompananteDiario frm = new FrmConfigurarAcompananteDiario()
            {
                StartPosition = FormStartPosition.CenterScreen,
                MaximizeBox = false,
                MinimizeBox = false,
            };
            frm.ShowDialog();
        }

        private void BtnAgregarPlato_Click(object sender, EventArgs e)
        {
            try
            {
                FrmAgregarPlato frm = new FrmAgregarPlato
                {
                    TopLevel = false
                };
                Form FormComprobado = this.ComprobarExistencia(frm);
                if (FormComprobado != null)
                {
                    frm.WindowState = FormWindowState.Normal;
                    frm.Activate();
                }
                else
                {
                    frm.FormBorderStyle = FormBorderStyle.Fixed3D;
                    this.panel1.Controls.Add(frm);
                    this.panel1.Tag = frm;
                    frm.Show();
                }
                frm.BringToFront();
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "AgregarPlato_Click",
                    "Hubo un error con el menu agregar plato", ex.Message);
            }
        }

        private void BtnEmpleados_Click(object sender, EventArgs e)
        {
            MenuEmpleados menuEmpleados = new MenuEmpleados();
            menuEmpleados.btnAgregarEmpleado.Click += BtnAgregarEmpleado_Click;
            menuEmpleados.btnEditarEmpleado.Click += BtnEditarEmpleado_Click;
            menuEmpleados.btnObservarEmpleados.Click += BtnObservarEmpleados_Click;
            menuEmpleados.btnActivarEmpleado.Click += BtnActivarEmpleado_Click;
            menuEmpleados.btnInactivarEmpleados.Click += BtnInactivarEmpleados_Click;
            menuEmpleados.btnCambiarClaveMaestra.Click += BtnCambiarClaveMaestra_Click;
            menuEmpleados.btnCambiarClaveUsuario.Click += BtnCambiarClaveUsuario_Click;
            menuEmpleados.btnComandasEliminadas.Click += BtnComandasEliminadas_Click;
            menuEmpleados.btnNotas.Click += BtnNotas_Click;
            menuEmpleados.btnNomina.Click += BtnNomina_Click;
            container = new PoperContainer(menuEmpleados);
            container.Show(this.btnEmpleados);
        }

        private void BtnNomina_Click(object sender, EventArgs e)
        {
            try
            {
                FrmNominaEmpleados frm = new FrmNominaEmpleados
                {
                    TopLevel = false
                };
                Form FormComprobado = this.ComprobarExistencia(frm);
                if (FormComprobado != null)
                {
                    frm.WindowState = FormWindowState.Maximized;
                    frm.Activate();
                }
                else
                {
                    frm.WindowState = FormWindowState.Maximized;
                    frm.FormBorderStyle = FormBorderStyle.Fixed3D;
                    this.panel1.Controls.Add(frm);
                    this.panel1.Tag = frm;
                    frm.Show();
                }
                frm.BringToFront();
                frm.Activate();
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "BtnNomina_Click",
                    "Hubo un error con el menu observar la nómina de empleados", ex.Message);
            }
        }

        private void BtnNotas_Click(object sender, EventArgs e)
        {
            FrmObservarEmpleados frmObservarEmpleados = new FrmObservarEmpleados
            {
                StartPosition = FormStartPosition.CenterScreen
            };
            frmObservarEmpleados.OnDataRow += FrmObservarEmpleados_OnDataRow;
            frmObservarEmpleados.ShowDialog();
        }

        private void FrmObservarEmpleados_OnDataRow(object sender, EventArgs e)
        {
            DataGridViewRow rowDataGrid = (DataGridViewRow)sender;
            DataRow row = ((DataRowView)rowDataGrid.DataBoundItem).Row;
            EEmpleado eEmpleado = new EEmpleado(row);
            try
            {
                FrmObservarNotas frm = new FrmObservarNotas
                {
                    TopLevel = false
                };
                frm.AsignarDatos(eEmpleado);
                Form FormComprobado = this.ComprobarExistencia(frm);
                if (FormComprobado != null)
                {
                    frm.WindowState = FormWindowState.Normal;
                    frm.Activate();
                }
                else
                {
                    frm.FormBorderStyle = FormBorderStyle.Fixed3D;
                    this.panel1.Controls.Add(frm);
                    this.panel1.Tag = frm;
                    frm.Show();
                }
                frm.BringToFront();
                frm.Activate();
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "FrmObservarEmpleados_OnDataRow",
                    "Hubo un error con el menu observar wmpleados para notas", ex.Message);
            }
        }

        private void BtnComandasEliminadas_Click(object sender, EventArgs e)
        {
            try
            {
                FrmDeletePedido frm = new FrmDeletePedido
                {
                    TopLevel = false
                };
                Form FormComprobado = this.ComprobarExistencia(frm);
                if (FormComprobado != null)
                {
                    frm.WindowState = FormWindowState.Normal;
                    frm.Activate();
                }
                else
                {
                    frm.FormBorderStyle = FormBorderStyle.Fixed3D;
                    this.panel1.Controls.Add(frm);
                    this.panel1.Tag = frm;
                    frm.Show();
                }
                frm.BringToFront();
                frm.Activate();
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "ComandasEliminadas_Click",
                    "Hubo un error con el menu observar comandas eliminadas", ex.Message);
            }
        }

        private void BtnCambiarClaveUsuario_Click(object sender, EventArgs e)
        {
            FrmObservarEmpleados frmObservarEmpleadosCambiarClaveUsuario = new FrmObservarEmpleados
            {
                StartPosition = FormStartPosition.CenterScreen,
                MaximizeBox = false,
                MinimizeBox = false
            };
            frmObservarEmpleadosCambiarClaveUsuario.OnDataRow += FrmObservarEmpleadosCambiarClaveUsuario_OnDataRow;
            frmObservarEmpleadosCambiarClaveUsuario.ShowDialog();
        }

        private void FrmObservarEmpleadosCambiarClaveUsuario_OnDataRow(object sender, EventArgs e)
        {
            DataGridViewRow dataGridRow = (DataGridViewRow)sender;
            DataRow row = ((DataRowView)dataGridRow.DataBoundItem).Row;

            FrmModificarClaveMaestra frm = new FrmModificarClaveMaestra
            {
                StartPosition = FormStartPosition.CenterScreen
            };
            frm.AsignarDatos(new EEmpleado(row), false);
            frm.ShowDialog();
        }

        private void BtnCambiarClaveMaestra_Click(object sender, EventArgs e)
        {
            FrmObservarEmpleados frmObservarEmpleadosCambiarClaveMaestra = new FrmObservarEmpleados
            {
                StartPosition = FormStartPosition.CenterScreen,
                MaximizeBox = false,
                MinimizeBox = false
            };
            frmObservarEmpleadosCambiarClaveMaestra.OnDataRow += FrmObservarEmpleadosCambiarClaveMaestra_OnDataRow;
            frmObservarEmpleadosCambiarClaveMaestra.ShowDialog();
        }

        private void FrmObservarEmpleadosCambiarClaveMaestra_OnDataRow(object sender, EventArgs e)
        {
            DataGridViewRow dataGridRow = (DataGridViewRow)sender;
            DataRow row = ((DataRowView)dataGridRow.DataBoundItem).Row;

            FrmModificarClaveMaestra frm = new FrmModificarClaveMaestra
            {
                StartPosition = FormStartPosition.CenterScreen
            };
            frm.AsignarDatos(new EEmpleado(row), true);
            frm.ShowDialog();
        }

        private void BtnInactivarEmpleados_Click(object sender, EventArgs e)
        {
            FrmObservarEmpleados frmObservarEmpleadosInactivar = new FrmObservarEmpleados
            {
                StartPosition = FormStartPosition.CenterScreen,
                MaximizeBox = false,
                MinimizeBox = false
            };
            frmObservarEmpleadosInactivar.OnDataRow += FrmObservarEmpleadosInactivar_OnDataRow;
            frmObservarEmpleadosInactivar.ShowDialog();
        }

        private void FrmObservarEmpleadosInactivar_OnDataRow(object sender, EventArgs e)
        {
            DataGridViewRow dataGridViewRow = (DataGridViewRow)sender;
            DataRow row = ((DataRowView)dataGridViewRow.DataBoundItem).Row;

            if (row != null)
            {
                EEmpleado eEmpleado = new EEmpleado(row);

                if (eEmpleado.Estado_empleado.Equals("INACTIVO"))
                {
                    Mensajes.MensajeInformacion("El usuario ya está inactivo", "Entendido");
                }
                else
                {
                    Mensajes.MensajePregunta("¿Está seguro que desea inactivar el empleado " + eEmpleado.Nombre_empleado + "?",
                        "Inactivar", "Cancelar", out DialogResult result);
                    if (result == DialogResult.Yes)
                    {
                        string rpta = NEmpleados.InactivarEmpleado(eEmpleado.Id_empleado);
                        if (rpta.Equals("OK"))
                        {
                            Mensajes.MensajeOkForm("Se inactivó correctamente el empleado");
                        }
                        else
                        {
                            Mensajes.MensajeErrorCompleto(this.Name, "FrmObservarEmpleadosInactivar_OnDataRow",
                                "Hubo un error al inactivar el empleado", rpta);
                        }
                    }
                }
            }
        }

        private void BtnActivarEmpleado_Click(object sender, EventArgs e)
        {
            FrmObservarEmpleados frmObservarEmpleadosActivar = new FrmObservarEmpleados
            {
                StartPosition = FormStartPosition.CenterScreen,
                MaximizeBox = false,
                MinimizeBox = false
            };
            frmObservarEmpleadosActivar.OnDataRow += FrmObservarEmpleadosActivar_OnDataRow;
            frmObservarEmpleadosActivar.ShowDialog();
        }

        private void FrmObservarEmpleadosActivar_OnDataRow(object sender, EventArgs e)
        {
            DataGridViewRow dataGridViewRow = (DataGridViewRow)sender;
            DataRow row = ((DataRowView)dataGridViewRow.DataBoundItem).Row;

            if (row != null)
            {
                EEmpleado eEmpleado = new EEmpleado(row);

                if (eEmpleado.Estado_empleado.Equals("ACTIVO"))
                {
                    Mensajes.MensajeInformacion("El usuario ya está activo", "Entendido");
                }
                else
                {
                    Mensajes.MensajePregunta("¿Está seguro que desea activar el empleado " + eEmpleado.Nombre_empleado + "?",
                        "Activar", "Cancelar", out DialogResult result);
                    if (result == DialogResult.Yes)
                    {
                        string rpta = NEmpleados.ActivarEmpleado(eEmpleado.Id_empleado);
                        if (rpta.Equals("OK"))
                        {
                            Mensajes.MensajeOkForm("Se activó correctamente el empleado");
                        }
                        else
                        {
                            Mensajes.MensajeErrorCompleto(this.Name, "FrmObservarEmpleadosActivar_OnDataRow",
                                "Hubo un error al activar el empleado", rpta);
                        }
                    }
                }
            }
        }

        private void BtnObservarEmpleados_Click(object sender, EventArgs e)
        {
            try
            {
                FrmObservarEmpleados frm = new FrmObservarEmpleados
                {
                    TopLevel = false
                };
                Form FormComprobado = this.ComprobarExistencia(frm);
                if (FormComprobado != null)
                {
                    frm.WindowState = FormWindowState.Normal;
                    frm.Activate();
                }
                else
                {
                    frm.FormBorderStyle = FormBorderStyle.Fixed3D;
                    this.panel1.Controls.Add(frm);
                    this.panel1.Tag = frm;
                    frm.Show();
                }
                frm.BringToFront();
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "ObservarEmpleados_Click",
                    "Hubo un error con el menu Observar empleados", ex.Message);
            }
        }

        private void BtnEditarEmpleado_Click(object sender, EventArgs e)
        {
            try
            {
                FrmAgregarEmpleado frm = new FrmAgregarEmpleado
                {
                    TopLevel = false
                };
                FrmObservarEmpleados frmObservar = new FrmObservarEmpleados
                {
                    StartPosition = FormStartPosition.CenterScreen,
                    FrmAgregarEmpleado = frm
                };
                frmObservar.ShowDialog();

                if (frm.Tag != null)
                {
                    Form FormComprobado = this.ComprobarExistencia(frm);
                    if (FormComprobado != null)
                    {
                        frm.WindowState = FormWindowState.Normal;
                        frm.Activate();
                    }
                    else
                    {
                        frm.FormBorderStyle = FormBorderStyle.Fixed3D;
                        this.panel1.Controls.Add(frm);
                        this.panel1.Tag = frm;
                        frm.Show();
                    }
                }
                frm.BringToFront();
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "EditarEmpleado_Click",
                    "Hubo un error con el menu Editar empleado", ex.Message);
            }
        }

        private void BtnAgregarEmpleado_Click(object sender, EventArgs e)
        {
            try
            {
                FrmAgregarEmpleado frm = new FrmAgregarEmpleado
                {
                    TopLevel = false
                };
                Form FormComprobado = this.ComprobarExistencia(frm);
                if (FormComprobado != null)
                {
                    frm.WindowState = FormWindowState.Normal;
                    frm.Activate();
                }
                else
                {
                    frm.FormBorderStyle = FormBorderStyle.Fixed3D;
                    this.panel1.Controls.Add(frm);
                    this.panel1.Tag = frm;
                    frm.Show();
                }
                frm.BringToFront();
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "AgregarEmpleado_Click",
                    "Hubo un error con el menu agregar empleados", ex.Message);
            }
        }

        private void BtnAdministracionAvanzada_Click(object sender, EventArgs e)
        {
            FrmAdministracionAvanzada FrmAdministracionAvanzada = new FrmAdministracionAvanzada
            {
                StartPosition = FormStartPosition.CenterScreen
            };
            FrmAdministracionAvanzada.ShowDialog();
        }

        private void LblMistico_DoubleClick(object sender, EventArgs e)
        {
            if (this.btnAdministracionAvanzada.Visible)
            {
                this.btnAdministracionAvanzada.Visible = false;
            }
            else
            {
                this.btnAdministracionAvanzada.Visible = true;
            }
        }
        FrmVentanaInicial VentanaInicial;
        private void FrmPrincipal_Load(object sender, EventArgs e)
        {
            DatosInicioSesion datos = DatosInicioSesion.GetInstancia();
            this.btnUsuario.Text = datos.Nombre_empleado;
            if (datos.Cargo_empleado.Equals("ADMINISTRADOR"))
            {
                this.btnEmpleados.Enabled = true;
                this.btnClientes.Enabled = true;
                this.btnPlatos.Enabled = true;
                this.btnBebidas.Enabled = true;
                this.btnMesasPedidos.Enabled = true;
                this.btnInsumos.Enabled = true;
                this.btnAdministracion.Enabled = true;
            }
            else if (datos.Cargo_empleado.Equals("MESERO"))
            {
                this.btnEmpleados.Enabled = false;
                this.btnClientes.Enabled = true;
                this.btnPlatos.Enabled = false;
                this.btnBebidas.Enabled = false;
                this.btnMesasPedidos.Enabled = true;
                this.btnInsumos.Enabled = false;
                this.btnAdministracion.Enabled = false;
            }
            else if (datos.Cargo_empleado.Equals("CAJERO"))
            {
                this.btnEmpleados.Enabled = false;
                this.btnClientes.Enabled = true;
                this.btnPlatos.Enabled = false;
                this.btnBebidas.Enabled = false;
                this.btnMesasPedidos.Enabled = true;
                this.btnInsumos.Enabled = false;
                this.btnAdministracion.Enabled = false;
            }
            else if (datos.Cargo_empleado.Equals("COCINERO"))
            {
                this.btnEmpleados.Enabled = false;
                this.btnClientes.Enabled = true;
                this.btnPlatos.Enabled = false;
                this.btnBebidas.Enabled = false;
                this.btnMesasPedidos.Enabled = true;
                this.btnInsumos.Enabled = false;
                this.btnAdministracion.Enabled = false;
            }
            else
            {
                this.btnEmpleados.Enabled = false;
                this.btnClientes.Enabled = false;
                this.btnPlatos.Enabled = false;
                this.btnBebidas.Enabled = false;
                this.btnMesasPedidos.Enabled = false;
                this.btnInsumos.Enabled = false;
                this.btnAdministracion.Enabled = false;
            }

            //Id de Juan Diego Administrador
            if (datos.Id_empleado == 6060)
                this.btnAdministracionAvanzada.Visible = true;

            this.MenuContextuales();

            //VentanaInicial = new FrmVentanaInicial
            //{
            //    Location = new Point(0, 0),
            //    Size = new Size(this.panel1.Width / 2,
            //    this.panel1.Height / 2),
            //    FormBorderStyle = FormBorderStyle.None,
            //    TopLevel = false,
            //    Opacity = .75
            //};
            //VentanaInicial.ObtenerReservas();
            //this.panel1.Controls.Add(VentanaInicial);
            //VentanaInicial.Show();
        }

        private void MenuContextuales()
        {
            Font font = new Font("Segoe UI Semibold", 9.75F,
                FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));

            #region MENU CLIENTES
            ToolStripMenuItem AgregarCliente =
                new ToolStripMenuItem("Nuevo cliente");
            ToolStripMenuItem Editarcliente =
                new ToolStripMenuItem("Actualizar datos de un cliente");
            ToolStripMenuItem ObservarClientes =
                new ToolStripMenuItem("Observar clientes");

            AgregarCliente.BackColor = Color.FromArgb(31, 143, 242);
            AgregarCliente.ForeColor = Color.White;
            AgregarCliente.Font = font;
            AgregarCliente.Name = "AgregarCliente";
            AgregarCliente.Click += AgregarCliente_Click;

            Editarcliente.BackColor = Color.FromArgb(31, 143, 242);
            Editarcliente.ForeColor = Color.White;
            Editarcliente.Name = "Editarcliente";
            Editarcliente.Font = font;
            Editarcliente.Click += Editarcliente_Click;

            ObservarClientes.BackColor = Color.FromArgb(31, 143, 242);
            ObservarClientes.ForeColor = Color.White;
            ObservarClientes.Name = "ObservarClientes";
            ObservarClientes.Font = font;
            ObservarClientes.Click += ObservarClientes_Click;

            this.MenuClientes.Items.Add(AgregarCliente);
            this.MenuClientes.Items.Add(Editarcliente);
            this.MenuClientes.Items.Add(ObservarClientes);

            #endregion

            #region MENU BEBIDAS
            ToolStripMenuItem AgregarBebida =
                new ToolStripMenuItem("Agregar una bebida");
            ToolStripMenuItem EditarBebida =
                new ToolStripMenuItem("Actualizar una bebida");
            ToolStripMenuItem ObservarBebidas =
                new ToolStripMenuItem("Observar bebidas");
            ToolStripMenuItem InactivarBebidas =
                new ToolStripMenuItem("Inactivar bebidas");

            AgregarBebida.BackColor = Color.FromArgb(31, 143, 242);
            AgregarBebida.ForeColor = Color.White;
            AgregarBebida.Font = font;
            AgregarBebida.Name = "AgregarBebida";
            AgregarBebida.Click += AgregarBebida_Click;

            EditarBebida.BackColor = Color.FromArgb(31, 143, 242);
            EditarBebida.ForeColor = Color.White;
            EditarBebida.Name = "EditarBebida";
            EditarBebida.Font = font;
            EditarBebida.Click += EditarBebida_Click;

            ObservarBebidas.BackColor = Color.FromArgb(31, 143, 242);
            ObservarBebidas.ForeColor = Color.White;
            ObservarBebidas.Name = "ObservarBebidas";
            ObservarBebidas.Font = font;
            ObservarBebidas.Click += ObservarBebidas_Click;

            InactivarBebidas.BackColor = Color.FromArgb(31, 143, 242);
            InactivarBebidas.ForeColor = Color.White;
            InactivarBebidas.Name = "InactivarBebidas";
            InactivarBebidas.Font = font;
            InactivarBebidas.Click += InactivarBebidas_Click;

            this.MenuBebidas.Items.Add(AgregarBebida);
            this.MenuBebidas.Items.Add(EditarBebida);
            this.MenuBebidas.Items.Add(ObservarBebidas);
            this.MenuBebidas.Items.Add(InactivarBebidas);

            #endregion

            #region MENU MESAS/PEDIDOS
            ToolStripMenuItem ObservarMesas =
                new ToolStripMenuItem("Observar mesas")
                {
                    BackColor = Color.FromArgb(31, 143, 242),
                    ForeColor = Color.White,
                    Font = font,
                    Name = "ObservarMesas"
                };

            this.MenuMesasPedidos.Items.Add(ObservarMesas);

            #endregion

            #region MENU INSUMOS
            ToolStripMenuItem AgregarInsumo =
                new ToolStripMenuItem("Agregar insumos");
            ToolStripMenuItem EditarInsumo =
                new ToolStripMenuItem("Actualizar insumos");
            ToolStripMenuItem ObservarInsumos =
                new ToolStripMenuItem("Observar insumos");

            AgregarInsumo.BackColor = Color.FromArgb(31, 143, 242);
            AgregarInsumo.ForeColor = Color.White;
            AgregarInsumo.Font = font;
            AgregarInsumo.Name = "AgregarBebida";
            AgregarInsumo.Click += AgregarInsumo_Click;

            EditarInsumo.BackColor = Color.FromArgb(31, 143, 242);
            EditarInsumo.ForeColor = Color.White;
            EditarInsumo.Name = "EditarBebida";
            EditarInsumo.Font = font;
            EditarInsumo.Click += EditarInsumo_Click;

            ObservarInsumos.BackColor = Color.FromArgb(31, 143, 242);
            ObservarInsumos.ForeColor = Color.White;
            ObservarInsumos.Name = "ObservarBebidas";
            ObservarInsumos.Font = font;
            ObservarInsumos.Click += ObservarInsumos_Click;

            this.MenuInsumos.Items.Add(AgregarInsumo);
            this.MenuInsumos.Items.Add(EditarInsumo);
            this.MenuInsumos.Items.Add(ObservarInsumos);

            #endregion
        }

        private void InactivarBebidas_Click(object sender, EventArgs e)
        {
            try
            {
                FrmObservarBebidas frm = new FrmObservarBebidas
                {
                    TopLevel = false,
                    InactivarBebidas = true
                };
                Form FormComprobado = this.ComprobarExistencia(frm);
                if (FormComprobado != null)
                {
                    frm.WindowState = FormWindowState.Normal;
                    frm.Activate();
                }
                else
                {
                    frm.FormBorderStyle = FormBorderStyle.Fixed3D;
                    this.panel1.Controls.Add(frm);
                    this.panel1.Tag = frm;
                    frm.Show();
                }
                frm.BringToFront();
                frm.Activate();
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "InactivarBebidas_Click",
                    "Hubo un error con el menu observar bebidas para inactivar", ex.Message);
            }
        }

        private void DetallePlato_Click(object sender, EventArgs e)
        {
            try
            {
                FrmAgregarDetallePlato frm = new FrmAgregarDetallePlato
                {
                    TopLevel = false
                };
                Form FormComprobado = this.ComprobarExistencia(frm);
                if (FormComprobado != null)
                {
                    frm.WindowState = FormWindowState.Normal;
                    frm.Activate();
                }
                else
                {
                    frm.FormBorderStyle = FormBorderStyle.Fixed3D;
                    this.panel1.Controls.Add(frm);
                    this.panel1.Tag = frm;
                    frm.Show();
                }
                frm.BringToFront();
                frm.Activate();
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "DetallePlato_Click",
                    "Hubo un error con el menu agregar detalle platos", ex.Message);
            }
        }

        private void ObservarInsumos_Click(object sender, EventArgs e)
        {
            try
            {
                FrmObservarInsumos frm = new FrmObservarInsumos
                {
                    TopLevel = false
                };
                Form FormComprobado = this.ComprobarExistencia(frm);
                if (FormComprobado != null)
                {
                    frm.WindowState = FormWindowState.Normal;
                    frm.Activate();
                }
                else
                {
                    frm.FormBorderStyle = FormBorderStyle.Fixed3D;
                    this.panel1.Controls.Add(frm);
                    this.panel1.Tag = frm;
                    frm.Show();
                }
                frm.BringToFront();
                frm.Activate();
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "ObservarInsumos_Click",
                    "Hubo un error con el menu observar insumos", ex.Message);
            }
        }

        private void EditarInsumo_Click(object sender, EventArgs e)
        {
            try
            {
                FrmAgregarInsumos frm = new FrmAgregarInsumos
                {
                    TopLevel = false
                };
                FrmObservarInsumos frmObservar = new FrmObservarInsumos
                {
                    StartPosition = FormStartPosition.CenterScreen,
                    agregarInsumos = frm
                };
                frmObservar.ShowDialog();

                if (frm.Tag != null)
                {
                    Form FormComprobado = this.ComprobarExistencia(frm);
                    if (FormComprobado != null)
                    {
                        frm.WindowState = FormWindowState.Normal;
                        frm.Activate();
                    }
                    else
                    {
                        frm.FormBorderStyle = FormBorderStyle.Fixed3D;
                        this.panel1.Controls.Add(frm);
                        this.panel1.Tag = frm;
                        frm.Show();
                    }
                }
                frm.BringToFront();
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "EditarInsumo_Click",
                    "Hubo un error con el menu Editar insumos", ex.Message);
            }
        }

        private void AgregarInsumo_Click(object sender, EventArgs e)
        {
            try
            {
                FrmAgregarInsumos frm = new FrmAgregarInsumos
                {
                    TopLevel = false
                };
                Form FormComprobado = this.ComprobarExistencia(frm);
                if (FormComprobado != null)
                {
                    frm.WindowState = FormWindowState.Normal;
                    frm.Activate();
                }
                else
                {
                    frm.FormBorderStyle = FormBorderStyle.Fixed3D;
                    this.panel1.Controls.Add(frm);
                    this.panel1.Tag = frm;
                    frm.Show();
                }
                frm.BringToFront();
                frm.Activate();
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "AgregarInsumo_Click",
                    "Hubo un error con el menu agregar insumos", ex.Message);
            }
        }

        private void ObservarBebidas_Click(object sender, EventArgs e)
        {
            try
            {
                FrmObservarBebidas frm = new FrmObservarBebidas
                {
                    TopLevel = false
                };
                Form FormComprobado = this.ComprobarExistencia(frm);
                if (FormComprobado != null)
                {
                    frm.WindowState = FormWindowState.Normal;
                    frm.Activate();
                }
                else
                {
                    frm.FormBorderStyle = FormBorderStyle.Fixed3D;
                    this.panel1.Controls.Add(frm);
                    this.panel1.Tag = frm;
                    frm.Show();
                }
                frm.BringToFront();
                frm.Activate();
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "ObservarBebidas_Click",
                    "Hubo un error con el menu observar bebidas", ex.Message);
            }
        }

        private void EditarBebida_Click(object sender, EventArgs e)
        {
            try
            {
                FrmAgregarBebida frm = new FrmAgregarBebida
                {
                    TopLevel = false
                };
                FrmObservarBebidas frmObservar = new FrmObservarBebidas
                {
                    StartPosition = FormStartPosition.CenterScreen,
                    FrmAgregarBebidas = frm
                };
                frmObservar.ShowDialog();

                if (frm.Tag != null)
                {
                    Form FormComprobado = this.ComprobarExistencia(frm);
                    if (FormComprobado != null)
                    {
                        frm.WindowState = FormWindowState.Normal;
                        frm.Activate();
                    }
                    else
                    {
                        frm.FormBorderStyle = FormBorderStyle.Fixed3D;
                        this.panel1.Controls.Add(frm);
                        this.panel1.Tag = frm;
                        frm.Show();
                    }
                }
                frm.BringToFront();
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "Editarcliente_Click",
                    "Hubo un error con el menu Editar cliente", ex.Message);
            }
        }

        private void AgregarBebida_Click(object sender, EventArgs e)
        {
            try
            {
                FrmAgregarBebida frm = new FrmAgregarBebida
                {
                    TopLevel = false
                };
                Form FormComprobado = this.ComprobarExistencia(frm);
                if (FormComprobado != null)
                {
                    frm.WindowState = FormWindowState.Normal;
                    frm.Activate();
                }
                else
                {
                    frm.FormBorderStyle = FormBorderStyle.Fixed3D;
                    this.panel1.Controls.Add(frm);
                    this.panel1.Tag = frm;
                    frm.Show();
                }
                frm.BringToFront();
                frm.Activate();
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "AgregarBebida_Click",
                    "Hubo un error con el menu agregar bebida", ex.Message);
            }
        }

        private void ObservarClientes_Click(object sender, EventArgs e)
        {
            try
            {
                FrmObservarClientes frm = new FrmObservarClientes
                {
                    TopLevel = false
                };
                Form FormComprobado = this.ComprobarExistencia(frm);
                if (FormComprobado != null)
                {
                    frm.WindowState = FormWindowState.Normal;
                    frm.Activate();
                }
                else
                {
                    frm.FormBorderStyle = FormBorderStyle.Fixed3D;
                    this.panel1.Controls.Add(frm);
                    this.panel1.Tag = frm;
                    frm.Show();
                }
                frm.BringToFront();
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "ObservarClientes_Click",
                    "Hubo un error con el menu observar cliente", ex.Message);
            }
        }

        private void Editarcliente_Click(object sender, EventArgs e)
        {
            try
            {
                FrmAgregarCliente frm = new FrmAgregarCliente
                {
                    TopLevel = false
                };
                FrmObservarClientes frmObservar = new FrmObservarClientes
                {
                    StartPosition = FormStartPosition.CenterScreen,
                    FrmAgregarCliente = frm
                };
                frmObservar.ShowDialog();

                if (frm.Tag != null)
                {
                    Form FormComprobado = this.ComprobarExistencia(frm);
                    if (FormComprobado != null)
                    {
                        frm.WindowState = FormWindowState.Normal;
                        frm.Activate();
                    }
                    else
                    {
                        frm.FormBorderStyle = FormBorderStyle.Fixed3D;
                        this.panel1.Controls.Add(frm);
                        this.panel1.Tag = frm;
                        frm.Show();
                    }
                }
                frm.BringToFront();
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "Editarcliente_Click",
                    "Hubo un error con el menu Editar cliente", ex.Message);
            }
        }

        private void AgregarCliente_Click(object sender, EventArgs e)
        {
            try
            {
                FrmAgregarCliente frm = new FrmAgregarCliente
                {
                    TopLevel = false
                };
                Form FormComprobado = this.ComprobarExistencia(frm);
                if (FormComprobado != null)
                {
                    frm.WindowState = FormWindowState.Normal;
                    frm.Activate();
                }
                else
                {
                    frm.FormBorderStyle = FormBorderStyle.Fixed3D;
                    this.panel1.Controls.Add(frm);
                    this.panel1.Tag = frm;
                    frm.Show();
                }
                frm.BringToFront();
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "AgregarCliente_Click",
                    "Hubo un error con el menu agregar cliente", ex.Message);
            }
        }

        private void FrmObservar_OnDgvDoubleClick(object sender, EventArgs e)
        {
            Platos plato = (Platos)sender;

            FrmAgregarPlato frm = new FrmAgregarPlato()
            {
                StartPosition = FormStartPosition.CenterParent,
                Plato = plato,
                TopMost = false,
                TopLevel = false,
            };

            Form FormComprobado = this.ComprobarExistencia(frm);
            if (FormComprobado != null)
            {
                frm.WindowState = FormWindowState.Normal;
                frm.Activate();
                frm.Show();
            }
            else
            {
                frm.FormBorderStyle = FormBorderStyle.Fixed3D;
                this.panel1.Controls.Add(frm);
                this.panel1.Tag = frm;
                frm.Show();
            }
            frm.BringToFront();
        }
        private Form ComprobarExistencia(Form form)
        {
            if (this.container != null)
                this.container.Close();

            Form frmDevolver = null;
            foreach (Form frm in this.panel1.Controls)
            {
                if (frm.Name.Equals(form.Name))
                {
                    frmDevolver = frm;
                    break;
                }

            }
            return frmDevolver;
        }

        #region MENUS PRINCIPALES

        ContextMenuStrip MenuEmpleados = new ContextMenuStrip();
        ContextMenuStrip MenuClientes = new ContextMenuStrip();
        ContextMenuStrip MenuPlatos = new ContextMenuStrip();
        ContextMenuStrip MenuBebidas = new ContextMenuStrip();
        ContextMenuStrip MenuExtras = new ContextMenuStrip();
        ContextMenuStrip MenuMesasPedidos = new ContextMenuStrip();
        ContextMenuStrip MenuVentas = new ContextMenuStrip();
        ContextMenuStrip MenuInsumos = new ContextMenuStrip();
        ContextMenuStrip OpcionesUsuario = new ContextMenuStrip();
        #endregion
    }
}
