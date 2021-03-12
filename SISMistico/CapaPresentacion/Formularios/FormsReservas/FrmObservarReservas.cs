using CapaNegocio;
using CapaPresentacion.Formularios.FormsClientes;
using CapaPresentacion.Formularios.FormsMesas;
using System;
using System.Configuration;
using System.Data;
using System.Windows.Forms;

namespace CapaPresentacion.Formularios.FormsReservas
{
    public partial class FrmObservarReservas : Form
    {
        public FrmObservarReservas()
        {
            InitializeComponent();
            this.Load += FrmObservarReservas_Load;
            this.rdCliente.CheckedChanged += Rd_CheckedChanged;
            this.rdFecha.CheckedChanged += Rd_CheckedChanged;
            this.rdMesa.CheckedChanged += Rd_CheckedChanged;
            this.btnNuevaReserva.Click += BtnNuevaReserva_Click;
            this.btnSeleccionar.Click += BtnSeleccionar_Click;
        }

        private void BtnSeleccionar_Click(object sender, EventArgs e)
        {
            string tipo = "";

            if (this.rdCliente.Checked)
                tipo = "CLIENTE";
            else if (this.rdFecha.Checked)
                tipo = "FECHA";
            else if (this.rdMesa.Checked)
                tipo = "MESA";

            switch (tipo)
            {
                case "MESA":
                    this.date1.Enabled = false;
                    FrmObservarMesas frmObservarMesas = new FrmObservarMesas
                    {
                        StartPosition = FormStartPosition.CenterScreen
                    };
                    frmObservarMesas.OnMesaClick += FrmObservarMesas_OnMesaClick;
                    frmObservarMesas.ShowDialog();
                    break;
                case "CLIENTE":
                    this.date1.Enabled = false;
                    FrmObservarClientes frmObservarClientes = new FrmObservarClientes
                    {
                        StartPosition = FormStartPosition.CenterScreen
                    };
                    frmObservarClientes.OnDgvDoubleClick += FrmObservarClientes_OnDgvDoubleClick;
                    frmObservarClientes.ShowDialog();
                    break;
                case "FECHA":
                    this.date1.Enabled = true;
                    this.date1.ValueChanged += Date1_ValueChanged;
                    break;
            }
        }

        private void BtnNuevaReserva_Click(object sender, EventArgs e)
        {
            FrmNuevaReserva reserva = new FrmNuevaReserva
            {
                StartPosition = FormStartPosition.CenterScreen
            };
            reserva.ShowDialog();
            if (reserva.DialogResult == DialogResult.OK)
            {
                this.Reserva_OnRefreshControls(sender, e);
            }
        }

        private void Rd_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rd = (RadioButton)sender;
            if (rd.Checked)
            {
                switch (rd.Text.ToUpper())
                {
                    case "MESA":
                        this.btnSeleccionar.Enabled = true;
                        this.date1.Enabled = false;
                        this.btnSeleccionar.Text = "Seleccionar una mesa";
                        this.btnSeleccionar.PerformClick();
                        break;
                    case "CLIENTE":
                        this.btnSeleccionar.Enabled = true;
                        this.date1.Enabled = false;
                        this.btnSeleccionar.Text = "Seleccionar un cliente";
                        this.btnSeleccionar.PerformClick();
                        break;
                    case "FECHA":
                        this.btnSeleccionar.Enabled = false;
                        this.date1.Enabled = true;
                        this.date1.ValueChanged += Date1_ValueChanged;
                        break;
                }
            }
        }

        private void FrmObservarMesas_OnMesaClick(object sender, EventArgs e)
        {
            EMesa mesa = (EMesa)sender;
            this.groupBox1.Text = "Búsqueda de reservas en la mesa " + mesa.Num_mesa;
            this.groupBox1.Tag = mesa.Id_mesa;
            this.BuscarReservas("MESA", mesa.Id_mesa.ToString());
        }

        private void Date1_ValueChanged(object sender, EventArgs e)
        {
            DateTimePicker date = (DateTimePicker)sender;
            this.BuscarReservas("FECHA", date.Value.ToString("yyyy/MM/dd"));
        }

        private void FrmObservarClientes_OnDgvDoubleClick(object sender, EventArgs e)
        {
            DataGridViewRow row = (DataGridViewRow)sender;
            this.groupBox1.Text = "Búsqueda de reservas del cliente: " + Convert.ToString(row.Cells["Nombre_cliente"].Value);
            int id_cliente = Convert.ToInt32(row.Cells["Id_cliente"].Value);
            this.groupBox1.Tag = id_cliente;
            this.BuscarReservas("CLIENTE", id_cliente.ToString());
        }

        private void FrmObservarReservas_Load(object sender, EventArgs e)
        {
            this.VerificarLicencia();

            this.rdPendiente.Checked = true;
            this.rdFecha.Checked = true;
            this.BuscarReservas("FECHA", DateTime.Now.ToString("yyyy/MM/dd"));
        }

        private void BuscarReservas(string tipo_busqueda, string texto_busqueda)
        {
            try
            {
                string estado = "";
                foreach (Control control in this.gbEstado.Controls)
                {
                    if (control is RadioButton rd)
                    {
                        if (rd.Checked)
                        {
                            estado = Convert.ToString(rd.Tag);
                            break;
                        }
                    }
                }
                this.panelReservas.clearDataSource();
                DataTable dtReservas =
                    NReservas.BuscarReservas(tipo_busqueda, texto_busqueda, estado, out string rpta);
                if (dtReservas != null)
                {
                    this.lblResultados.Text = "Se encontraron " + dtReservas.Rows.Count + " reservas";
                    foreach (DataRow row in dtReservas.Rows)
                    {
                        ReservaMesaSmall reserva = new ReservaMesaSmall
                        {
                            Reserva = new EReservas(row)
                        };
                        reserva.AsignarDatos();
                        reserva.OnRefreshControls += Reserva_OnRefreshControls;
                        this.panelReservas.AddControl(reserva);
                    }
                }
                else
                {
                    this.lblResultados.Text = "No se encontraron reservas";


                    if (!rpta.Equals("OK"))
                        throw new Exception(rpta);
                }
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "BuscarReservas",
                    "Hubo un error al buscar las reservas", ex.Message);
            }
        }

        private void Reserva_OnRefreshControls(object sender, EventArgs e)
        {
            try
            {
                if (this.rdCliente.Checked)
                {
                    if (int.TryParse(Convert.ToString(this.groupBox1.Tag), out int id_cliente))
                    {
                        this.BuscarReservas("CLIENTE", id_cliente.ToString());
                    }
                    else
                    {
                        this.BuscarReservas("FECHA", this.date1.Value.ToString("yyyy-MM-dd"));
                    }
                }
                else if (this.rdFecha.Checked)
                {
                    this.BuscarReservas("FECHA", this.date1.Value.ToString("yyyy-MM-dd"));
                }
                else if (this.rdMesa.Checked)
                {
                    if (int.TryParse(Convert.ToString(this.groupBox1.Tag), out int id_mesa))
                    {
                        this.BuscarReservas("MESA", id_mesa.ToString());
                    }
                    else
                    {
                        this.BuscarReservas("FECHA", this.date1.Value.ToString("yyyy-MM-dd"));
                    }
                }
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "Reserva_OnRefreshControls",
                    "Hubo un error al refrescar la vista", ex.Message);
            }

        }

        public event EventHandler OnReservaRefresh;

        private void VerificarLicencia()
        {
            bool moduloReservaFull = Convert.ToBoolean(ConfigurationManager.AppSettings["moduloReservaFull"]);
            if (moduloReservaFull)
            {
                this.Text = "Observar reservas (Licencia completa activada)";
            }
            else
            {
                bool moduloReservaTrial = Convert.ToBoolean(ConfigurationManager.AppSettings["moduloReservaTrial"]);
                if (moduloReservaTrial)
                {
                    //Total de días hábiles del periodo trial
                    int totalDiasTrial =
                       Convert.ToInt32(ConfigurationManager.AppSettings["totalDiasTrial"]);
                    //Conteo de Días que han transcurrido
                    int conteoDiasTrial =
                        Convert.ToInt32(ConfigurationManager.AppSettings["conteoDiasTrial"]);
                    //Fecha de activación de la licencia
                    DateTime fechaActivacion =
                       Convert.ToDateTime(ConfigurationManager.AppSettings["fechaActivacionTrial"]);
                    //Fecha de activación de la licencia más los días transcurridos
                    DateTime fechaActivacionActual = fechaActivacion.AddDays(conteoDiasTrial);

                    //Si la fecha de activación con los días transcurridos es menor que al día de hoy
                    if (fechaActivacionActual < DateTime.Now)
                    {
                        //Verificamos que sea el día anterior obligatoriamente para sumar al contador 
                        if (fechaActivacionActual == DateTime.Now.AddDays(-1))
                        {
                            //Y guardaremos en el archivo de configuración
                            conteoDiasTrial += 1;
                            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                            config.AppSettings.Settings["conteoDiasTrial"].Value = conteoDiasTrial.ToString();
                            config.Save(ConfigurationSaveMode.Modified, true);
                            ConfigurationManager.RefreshSection("appSettings");
                        }
                        else
                        {
                            conteoDiasTrial = totalDiasTrial + 1;
                        }
                    }

                    //Si el conteo de días es igual al total de días habilitados 
                    if (conteoDiasTrial == totalDiasTrial)
                    {
                        this.Text = "Observar reservas (Licencia trial activada (último día de licencia))";
                    }
                    else if (conteoDiasTrial > totalDiasTrial)
                    {
                        this.Text = "Observar reservas (Licencia trial desactivada - Actualice su licencia)";
                        foreach (Control control in this.Controls)
                        {
                            if (control is GroupBox gb)
                            {
                                foreach (Control ctrl in gb.Controls)
                                {
                                    ctrl.Enabled = false;
                                }
                            }
                            else
                            {
                                control.Enabled = false;
                            }
                        }
                    }
                    else
                    {
                        this.Text = "Observar reservas (Licencia trial activada - " +
                            (totalDiasTrial - conteoDiasTrial) + " días restantes)";
                    }
                }
                else
                {
                    this.Text = "Observar reservas (No tiene una licencia activa - Actualice su licencia)";
                    foreach (Control control in this.Controls)
                    {
                        if (control is GroupBox gb)
                        {
                            foreach (Control ctrl in gb.Controls)
                            {
                                ctrl.Enabled = false;
                            }
                        }
                        else
                        {
                            control.Enabled = false;
                        }
                    }
                }
            }
        }
    }
}
