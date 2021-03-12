using CapaNegocio;
using CapaPresentacion.Properties;
using System;
using System.Text;
using System.Windows.Forms;

namespace CapaPresentacion.Formularios.FormsReservas
{
    public partial class ReservaMesaSmall : UserControl
    {
        PoperContainer container;
        public ReservaMesaSmall()
        {
            InitializeComponent();
            this.btnEditar.Click += BtnEditar_Click;
            this.btnEliminar.Click += BtnEliminar_Click;
        }
        public event EventHandler OnBtnOcultarNotification;

        public event EventHandler OnRefreshControls;

        private void BtnEliminar_Click(object sender, EventArgs e)
        {
            try
            {
                string rpta = NReservas.EditarReserva(this.Reserva.Id_reserva, this.Reserva.Id_mesa, this.Reserva.Id_cliente,
                        this.Reserva.Fecha, this.Reserva.Hora, this.Reserva.Observaciones, "CANCELADA");
                if (rpta.Equals("OK"))
                {
                    Mensajes.MensajeOkForm("Se canceló correctamente la reserva");
                    this.OnRefreshControls?.Invoke(this, e);
                }
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "BtnEliminar_Click",
                    "Hubo un error al eliminar una reserva", ex.Message);
            }
        }

        private void BtnEditar_Click(object sender, EventArgs e)
        {
            if (OnlyView)
            {
                MenuNotification menuNotification = new MenuNotification();
                menuNotification.btnOcultarNotificacion.Click += BtnOcultarNotificacion_Click;
                container = new PoperContainer(menuNotification);
                container.Show(btnEditar);
                return;
            }

            FrmNuevaReserva reserva = new FrmNuevaReserva
            {
                IsEditar = true,
                StartPosition = FormStartPosition.CenterScreen
            };
            reserva.AsignarDatos(this.Reserva);
            reserva.FormClosed += Reserva_FormClosed;
            reserva.ShowDialog();
        }

        private void BtnOcultarNotificacion_Click(object sender, EventArgs e)
        {
            if (this.container != null)
                this.container.Close();

            OnBtnOcultarNotification?.Invoke(this, e);
        }

        private void Reserva_FormClosed(object sender, FormClosedEventArgs e)
        {
            Form frm = (Form)sender;
            if (frm.DialogResult == DialogResult.OK)
            {
                this.OnRefreshControls?.Invoke(sender, e);
            }
        }

        public void AsignarDatos()
        {
            if (this.Reserva != null)
            {
                this.txtMesa.Text = Reserva.Num_mesa.ToString();
                StringBuilder info = new StringBuilder();
                info.Append("El cliente ");
                info.Append(Reserva.Nombre_cliente + " ");
                info.Append("tiene reservada la mesa ");
                info.Append(Reserva.Num_mesa + " ");
                info.Append("para el día ");
                DateTime fechaReserva = Convert.ToDateTime(Reserva.Fecha);
                info.Append(fechaReserva.ToLongDateString() + " ");
                info.Append("desde las ");
                DateTime horaReserva = Convert.ToDateTime(Reserva.Hora);
                info.Append(horaReserva.ToLongTimeString() + " ");
                this.txtInformacion.Text = Convert.ToString(info);
            }

            if (this.OnlyView)
            {
                this.btnEditar.Image = Resources.notification;
                this.btnEliminar.Visible = false;
                this.toolTip1.SetToolTip(this.btnEditar, "Requiere su atención");
            }
        }

        private EReservas _reserva;
        private bool _onlyView;

        public EReservas Reserva { get => _reserva; set => _reserva = value; }
        public bool OnlyView { get => _onlyView; set => _onlyView = value; }
    }
}
