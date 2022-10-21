using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CapaNegocio;
using CapaEntidades.Models;

namespace CapaPresentacion.Formularios.FormsPrincipales.Menus
{
    public partial class MenuAdministracion : UserControl
    {
        public MenuAdministracion()
        {
            InitializeComponent();
            this.Load += MenuAdministracion_Load;
            this.txtNuevaBase.KeyPress += TxtNuevaBase_KeyPress;
            this.txtNuevaBase.GotFocus += TxtNuevaBase_GotFocus;
            this.txtNuevaBase.LostFocus += TxtNuevaBase_LostFocus;

            this.btnGuardar.Click += BtnGuardar_Click;
        }
        public void LoadTurno()
        {
            var result = NTurnos.BuscarTurnos("ULTIMO TURNO", DateTime.Now.ToString("yyyy-MM-dd"), "").Result;
            string rpta = result.rpta;
            DataTable dt = result.dtTurnos;

            if (dt == null)
                return;

            Turno turno = new Turno(dt.Rows[0]);

            this.Turno = turno;
        }
        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.Turno == null)
                    return;

                if (!decimal.TryParse(this.txtNuevaBase.Text, out decimal nueva_base))
                    throw new Exception("Revise la nueva base ingresada deben ser solo números");

                string rpta = NTurnos.InsertarBase(this.Turno.Id_turno, nueva_base).Result;

                if (!rpta.Equals("OK"))
                    throw new Exception(rpta);

                this.LoadTurno();

                Mensajes.MensajeOkForm("¡Base modificada correctamente!");
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorForm($"Error guardando la nueva base | {ex.Message}");
            }
        }
        private void MenuAdministracion_Load(object sender, EventArgs e)
        {
            this.LoadTurno();
        }
        private void TxtNuevaBase_LostFocus(object sender, EventArgs e)
        {
            if (this.txtNuevaBase.Text.Equals(""))
            {
                this.txtNuevaBase.Text = "0";
            }
        }
        private void TxtNuevaBase_GotFocus(object sender, EventArgs e)
        {
            this.txtNuevaBase.SelectAll();
        }
        private void TxtNuevaBase_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsDigit(e.KeyChar))
            {
                e.Handled = false;
            }
            else if (char.IsControl(e.KeyChar))
            {
                e.Handled = false;
            }
            else if (char.IsLetter(e.KeyChar))
            {
                e.Handled = true;
            }
            else
            {
                e.Handled = true;
            }
        }
        private void AsignarDatos(Turno turno)
        {
            this.txtBase.Text = turno.Valor_inicial.ToString("C").Replace(",00", "").Replace(",00", "");
        }
        private Turno _turno;
        public Turno Turno
        {
            get => _turno;
            set
            {
                _turno = value;
                this.AsignarDatos(value);
            }
        }
    }
}
