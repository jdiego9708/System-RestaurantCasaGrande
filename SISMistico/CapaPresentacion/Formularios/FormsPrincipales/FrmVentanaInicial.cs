using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using CapaPresentacion.Formularios.FormsReservas;
using CapaNegocio;
using CapaPresentacion.Properties;

namespace CapaPresentacion.Formularios.FormsPrincipales
{
    public partial class FrmVentanaInicial : Form
    {
        public FrmVentanaInicial()
        {
            InitializeComponent();
            this.Load += FrmVentanaInicial_Load;
        }
        private void FrmVentanaInicial_Load(object sender, EventArgs e)
        {

        }

        public void ObtenerReservas()
        {
            try
            {
                DataTable dtReservas =
                    NReservas.BuscarReservas("FECHA", DateTime.Now.ToString("yyyy-MM-dd"), "PENDIENTE", out string rpta);
                this.panelAlertas.clearDataSource();
                if (dtReservas != null)
                {
                    this.panelAlertas.BackgroundImage = null;
                    List<UserControl> controls = new List<UserControl>();
                    foreach (DataRow row in dtReservas.Rows)
                    {
                        ReservaMesaSmall reserva = new ReservaMesaSmall
                        {
                            Reserva = new EReservas(row),
                            OnlyView = true
                        };
                        reserva.OnBtnOcultarNotification += Reserva_OnBtnOcultarNotification;
                        reserva.AsignarDatos();
                        controls.Add(reserva);
                    }
                    this.panelAlertas.AddArrayControl(controls);
                }
                else
                {
                    this.panelAlertas.BackgroundImage = Resources.No_hay_reserva;
                    this.panelAlertas.BackgroundImageLayout = ImageLayout.Stretch;

                    if (!rpta.Equals("OK"))
                        throw new Exception();
                }
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "ObtenerReservas",
                    "Hubo un error al obtener las reservas diarias", ex.Message);
            }
        }

        private void Reserva_OnBtnOcultarNotification(object sender, EventArgs e)
        {
            ReservaMesaSmall reservaMesaSmall = (ReservaMesaSmall)sender;
            this.panelAlertas.RemoveControl(reservaMesaSmall);
        }
    }
}
