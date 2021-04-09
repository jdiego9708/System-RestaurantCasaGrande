using CapaEntidades.Models;
using CapaNegocio;
using System;
using System.Data;
using System.Windows.Forms;

namespace CapaPresentacion.Formularios
{
    public partial class FrmComprobacion : Form
    {
        public FrmComprobacion()
        {
            InitializeComponent();
            this.txtCodigo.KeyPress += TxtCodigo_KeyPress;
            this.txtCodigo.TextChanged += TxtCodigo_TextChanged;
            this.Load += FrmComprobacion_Load;
        }

        private string _cargo_empleado;
        private int _id_empleado;
        private string _nombre_empleado;
        public Empleados EmpleadoSelected { get; set; }

        public string Cargo_empleado { get => _cargo_empleado; set => _cargo_empleado = value; }
        public int Id_empleado { get => _id_empleado; set => _id_empleado = value; }
        public string Nombre_empleado { get => _nombre_empleado; set => _nombre_empleado = value; }

        private void FrmComprobacion_Load(object sender, EventArgs e)
        {
            this.txtCodigo.Focus();
        }

        private async void TxtCodigo_TextChanged(object sender, EventArgs e)
        {
            TextBox txt = (TextBox)sender;
            try
            {
                if (txt.TextLength == 4)
                {
                    if (!int.TryParse(txt.Text, out int codigo))
                    {
                        Mensajes.MensajeInformacion("¡Solo números!", "Entendido");
                        return;
                    }

                    var (rpta, empleado, dtEmpleado) = await NEmpleados.ClaveMaestra(codigo);
                    if (dtEmpleado != null)
                    {                       
                        this.Id_empleado = empleado.Id_empleado;
                        this.Nombre_empleado = empleado.Nombre_empleado;
                        this.Cargo_empleado = empleado.Cargo_empleado;
                        this.EmpleadoSelected = empleado;
                        this.DialogResult = DialogResult.OK;
                        this.Tag = dtEmpleado;
                        this.Close();
                    }
                    else
                    {
                        if (rpta.Equals("OK"))
                        {
                            Mensajes.MensajeInformacion("El código no corresponde a ninguno de nuestros empleados", "Entendido");
                            this.DialogResult = DialogResult.Abort;
                            this.Close();
                        }
                        else
                            throw new Exception(rpta);
                    }
                }
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorForm(ex.Message);
            }
        }

        private async void TxtCodigo_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBox txt = (TextBox)sender;
            if (Char.IsDigit(e.KeyChar))
            {
                e.Handled = false;
            }
            else if (Char.IsControl(e.KeyChar))
            {
                e.Handled = false;
                if ((int)e.KeyChar == (int)Keys.Enter)
                {
                    if (!int.TryParse(txt.Text, out int codigo))
                    {
                        Mensajes.MensajeInformacion("¡Solo números!", "Entendido");
                        return;
                    }

                    var (rpta, empleado, dtEmpleado) = await NEmpleados.ClaveMaestra(codigo);
                    if (rpta.Equals("OK"))
                    {
                        this.Id_empleado = empleado.Id_empleado;
                        this.Nombre_empleado = empleado.Nombre_empleado;
                        this.Cargo_empleado = empleado.Cargo_empleado;
                        this.DialogResult = DialogResult.OK;
                        this.Tag = dtEmpleado;
                        this.Close();
                    }
                    else if (rpta.Equals(""))
                    {
                        Mensajes.MensajeInformacion("El código no corresponde a ninguno de nuestros empleados", "Entendido");
                    }
                    else
                    {
                        throw new Exception(rpta);
                    }
                }
                else if ((int)e.KeyChar == (int)Keys.Escape)
                {
                    this.DialogResult = DialogResult.Cancel;
                    this.Close();
                }
            }
            else if (Char.IsLetter(e.KeyChar))
            {
                e.Handled = true;
            }
            else
            {
                e.Handled = true;
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
            else
            {
                return base.ProcessCmdKey(ref msg, keyData);
            }
            return true;
        }

    }
}
