using CapaNegocio;
using CapaPresentacion.Formularios.FormsClientes;
using CapaPresentacion.Formularios.FormsMesas;
using System;
using System.Windows.Forms;

namespace CapaPresentacion.Formularios.FormsReservas
{
    public partial class FrmNuevaReserva : Form
    {
        public FrmNuevaReserva()
        {
            InitializeComponent();
            this.btnCliente.Click += BtnCliente_Click;
            this.Load += FrmNuevaReserva_Load;
            this.btnFinalizar.Click += BtnFinalizar_Click;
            this.btnMesa.Click += BtnMesa_Click;
        }

        private EReservas Reserva;

        public void AsignarDatos(EReservas reserva)
        {
            this.Reserva = reserva;
            this.btnFinalizar.Text = "Actualizar";
            this.btnMesa.Text = "Mesa " + reserva.Num_mesa.ToString();
            this.btnMesa.Tag = reserva.Id_mesa;
            this.btnCliente.Text = reserva.Nombre_cliente;
            this.btnCliente.Tag = reserva.Id_cliente;
            DateTime fecha = Convert.ToDateTime(reserva.Fecha);
            this.dateFecha.Value = fecha;
            this.ListaHora =
                LlenarListas.ListaHoras(this.ListaHora);
            this.ListaHora.SelectedValue = reserva.Hora;
            this.txtObservaciones.Text = reserva.Observaciones;
        }

        private void BtnMesa_Click(object sender, EventArgs e)
        {
            FrmObservarMesas frmObservarMesas = new FrmObservarMesas
            {
                StartPosition = FormStartPosition.CenterScreen
            };
            frmObservarMesas.OnMesaClick += FrmObservarMesas_OnMesaClick;
            frmObservarMesas.ShowDialog();
        }

        private void FrmObservarMesas_OnMesaClick(object sender, EventArgs e)
        {
            EMesa mesa = (EMesa)sender;
            this.btnMesa.Text = "Mesa " + mesa.Num_mesa;
            this.btnMesa.Tag = mesa.Id_mesa;
        }

        private bool Comprobaciones()
        {
            if (this.ListaHora.Text.Equals(""))
                return false;

            if (this.btnCliente.Tag == null)
                return false;

            if (this.btnMesa.Tag == null)
                return false;

            return true;
        }

        private void BtnFinalizar_Click(object sender, EventArgs e)
        {
            try
            {
                string rpta = "";
                string mensaje = "";
                if (this.Comprobaciones())
                {
                    int id_mesa = Convert.ToInt32(this.btnMesa.Tag);
                    int id_cliente = Convert.ToInt32(this.btnCliente.Tag);

                    if (this.IsEditar)
                    {
                        rpta = NReservas.EditarReserva(this.Reserva.Id_reserva, id_mesa, id_cliente,
                        this.dateFecha.Value.ToShortDateString(), this.ListaHora.SelectedValue.ToString(), 
                        this.txtObservaciones.Text, this.Reserva.Estado);
                        mensaje = "Se actualizó correctamente la reserva";
                    }
                    else
                    {
                        rpta = NReservas.InsertarReserva(out int id_reserva, id_mesa, id_cliente,
                        this.dateFecha.Value.ToShortDateString(), this.ListaHora.SelectedValue.ToString(), 
                        this.txtObservaciones.Text);
                        mensaje = "Se reservó correctamente la mesa para el día " +
                            this.dateFecha.Value.ToLongDateString() + " número de la reserva: " + id_reserva;
                    }

                    if (rpta.Equals("OK"))
                    {
                        this.DialogResult = DialogResult.OK;
                        Mensajes.MensajeInformacion(mensaje, "Entendido");
                        this.Close();
                    }
                    else
                    {
                        throw new Exception(rpta);
                    }
                }
                else
                {
                    Mensajes.MensajeInformacion("Por favor verifica los datos, selecciona un cliente y una hora", "Entendido");
                }
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "",
                    "Hubo un error al insertar una reserva", ex.Message);
            }
        }

        private void FrmNuevaReserva_Load(object sender, EventArgs e)
        {
            this.dateFecha.MinDate = DateTime.Now;
            if (!this.IsEditar)
            {
                this.ListaHora = LlenarListas.ListaHoras(this.ListaHora);
            }
        }

        private void BtnCliente_Click(object sender, EventArgs e)
        {
            FrmObservarClientes frmObservarClientes = new FrmObservarClientes
            {
                StartPosition = FormStartPosition.CenterScreen
            };
            frmObservarClientes.OnDgvDoubleClick += FrmObservarClientes_OnDgvDoubleClick;
            frmObservarClientes.ShowDialog();
        }

        private void FrmObservarClientes_OnDgvDoubleClick(object sender, EventArgs e)
        {
            DataGridViewRow row = (DataGridViewRow)sender;
            if (row != null)
            {
                this.btnCliente.Text = Convert.ToString(row.Cells["Nombre_cliente"].Value);
                this.btnCliente.Tag = Convert.ToInt32(row.Cells["Id_cliente"].Value);
            }
        }

        public void ObtenerMesa(int id_mesa, int num_mesa)
        {
            this.btnMesa.Tag = id_mesa;
            this.btnMesa.Text = "Mesa: " + num_mesa;
            this.btnMesa.Enabled = false;
        }

        private bool _isEditar;

        public bool IsEditar { get => _isEditar; set => _isEditar = value; }
    }
}
