using CapaNegocio;
using CapaPresentacion.Formularios.FormsEmpleados;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CapaPresentacion.Formularios.FormsNotas
{
    public partial class FrmNuevaNota : Form
    {
        public FrmNuevaNota()
        {
            InitializeComponent();
            this.btnGuardar.Click += BtnGuardar_Click;
        }

        private bool Comprobaciones(out List<string> variables)
        {
            variables = new List<string>();

            if (this.txtTitulo.Text.Equals(""))
            {
                Mensajes.MensajeInformacion("El titulo es necesario", "Entendido");
                return false;
            }

            if (this.txtDescripcion.Text.Equals(""))
            {
                Mensajes.MensajeInformacion("Debe poner una pequeña descripción", "Entendido");
                return false;
            }

            variables.Add(this.EEmpleado.Id_empleado.ToString());
            variables.Add(DateTime.Now.ToString("yyyy-MM-dd"));
            variables.Add(DateTime.Now.ToString("HH:mm"));
            variables.Add(this.txtTitulo.Text.ToUpper());
            variables.Add(this.txtDescripcion.Text);

            return true;
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.Comprobaciones(out List<string> variables))
                {
                    string rpta = NNotas.InsertarNota(variables, out int id_nota);
                    if (rpta.Equals("OK"))
                    {
                        Mensajes.MensajeOkForm("Se insertó la nota correctamente " + id_nota);
                        OnRefresh?.Invoke(sender, e);
                        this.Close();                  
                    }
                    else
                    {
                        throw new Exception(rpta);
                    }
                }
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "BtnGuardar_Click",
                    "Hubo un error al guardar una nota", ex.Message);
            }
        }

        public event EventHandler OnRefresh;
        EEmpleado EEmpleado;

        public void AsignarDatos(EEmpleado eEmpleado)
        {
            this.EEmpleado = eEmpleado;
            this.txtEmpleado.Text = eEmpleado.Nombre_empleado;
            this.txtEmpleado.Tag = eEmpleado;
        }
    }
}
