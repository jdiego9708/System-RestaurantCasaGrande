using CapaNegocio;
using CapaPresentacion.Formularios.FormsEmpleados;
using CapaPresentacion.Properties;
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
    public partial class FrmObservarNotas : Form
    {
        PoperContainer container;
        public FrmObservarNotas()
        {
            InitializeComponent();
            this.btnNuevaNota.Click += BtnNuevaNota_Click;
            this.Load += FrmObservarNotas_Load;
        }

        private void FrmObservarNotas_Load(object sender, EventArgs e)
        {
            this.BuscarNotas("ID EMPLEADO", this.EEmpleado.Id_empleado.ToString());
        }

        private void BtnNuevaNota_Click(object sender, EventArgs e)
        {
            FrmNuevaNota frmNuevaNota = new FrmNuevaNota
            {
                Dock = DockStyle.Fill,
                FormBorderStyle = FormBorderStyle.None,
                TopLevel = false
            };
            frmNuevaNota.AsignarDatos(this.EEmpleado);
            frmNuevaNota.OnRefresh += FrmNuevaNota_OnRefresh;
            this.container = new PoperContainer(frmNuevaNota);
            this.container.Show(this.btnNuevaNota);
        }

        private void FrmNuevaNota_OnRefresh(object sender, EventArgs e)
        {
            if (this.container != null)
                this.container.Close();

            this.BuscarNotas("ID EMPLEADO", this.EEmpleado.Id_empleado.ToString());
        }

        private void BuscarNotas(string tipo_busqueda, string texto_busqueda)
        {
            try
            {
                this.panelNotas.clearDataSource();
                DataTable dtNotas = NNotas.BuscarNotas(tipo_busqueda, texto_busqueda, out string rpta);
                if (dtNotas != null)
                {                   
                    this.panelNotas.Enabled = true;
                    this.panelNotas.BackgroundImage = null;
                    this.lblResultados.Click += LblResultados_Click;
                    List<UserControl> controls = new List<UserControl>();
                    if (this.IsBusquedaCompleta)
                    {
                        this.lblResultados.Text = "Se encontraron " + dtNotas.Rows.Count + " notas. (Ver todas)";

                        foreach (DataRow row in dtNotas.Rows)
                        {
                            ENotas eNota = new ENotas(row);
                            NotaSmall notaSmall = new NotaSmall();
                            notaSmall.AsignarDatos(eNota);
                            controls.Add(notaSmall);
                        }
                    }
                    else
                    {
                        this.lblResultados.Text = "Última nota. (Ver todas)";
                        ENotas eNota = new ENotas(dtNotas, 0);
                        NotaSmall notaSmall = new NotaSmall();
                        notaSmall.AsignarDatos(eNota);
                        controls.Add(notaSmall);
                    }

                    this.panelNotas.AddArrayControl(controls);
                }
                else
                {
                    this.panelNotas.BackgroundImage = Resources.No_hay_notas;
                    this.panelNotas.Enabled = false;
                    this.lblResultados.Text = "No se encontraron notas recientes";
                    this.lblResultados.Click -= LblResultados_Click;

                    if (!rpta.Equals("OK"))
                        throw new Exception(rpta);
                }
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "BuscarNotas",
                    "Hubo un error al buscar las notas", ex.Message);
            }
        }

        private void LblResultados_Click(object sender, EventArgs e)
        {
            if (this.IsBusquedaCompleta)
            {
                this.IsBusquedaCompleta = false;
                this.BuscarNotas("ID EMPLEADO", this.EEmpleado.Id_empleado.ToString());
                if (this.panelNotas.Enabled)
                    this.lblResultados.Text = "Última nota. (Ver todas)";
            }
            else
            {
                this.IsBusquedaCompleta = true;
                this.BuscarNotas("ID EMPLEADO", this.EEmpleado.Id_empleado.ToString());
                if (this.panelNotas.Enabled)
                    this.lblResultados.Text = "Se encontraron " + this.panelNotas.Controls.Count + " notas. (Ver última)";
            }
        }

        EEmpleado EEmpleado;

        public void AsignarDatos(EEmpleado eEmpleado)
        {
            this.EEmpleado = eEmpleado;
            this.txtEmpleado.Text = eEmpleado.Nombre_empleado;
        }

        private bool IsBusquedaCompleta = false;
    }
}
