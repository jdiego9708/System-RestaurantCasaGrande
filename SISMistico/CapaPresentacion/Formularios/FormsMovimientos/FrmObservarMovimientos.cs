using CapaEntidades.Models;
using CapaNegocio;
using CapaPresentacion.Formularios.FormsEgresos;
using CapaPresentacion.Formularios.FormsIngresos;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CapaPresentacion.Formularios.FormsMovimientos
{
    public partial class FrmObservarMovimientos : Form
    {

        PoperContainer container;

        public FrmObservarMovimientos()
        {
            InitializeComponent();
            this.dateBusqueda.ValueChanged += DateBusqueda_ValueChanged;
            this.Load += FrmObservarMovimientos_Load;
            this.btnAddIngreso.Click += BtnAddIngreso_Click;
            this.btnAddEgreso.Click += BtnAddEgreso_Click;
        }

        private void BtnAddEgreso_Click(object sender, EventArgs e)
        {
            FrmNuevoEgreso frmNuevoEgreso = new FrmNuevoEgreso
            {
                TopLevel = false,
                TopMost = false,
                FormBorderStyle = FormBorderStyle.None,
            };
            frmNuevoEgreso.OnEgresoSuccess += FrmNuevoEgreso_OnEgresoSuccess;
            frmNuevoEgreso.FormClosed += FrmNuevoEgreso_FormClosed;
            this.container = new PoperContainer(frmNuevoEgreso);
            this.container.Show(this.btnAddEgreso);
            frmNuevoEgreso.Show();
        }

        private void FrmNuevoEgreso_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.container.Close();
        }

        private async void FrmNuevoEgreso_OnEgresoSuccess(object sender, EventArgs e)
        {
            await this.BuscarMovimientos("FECHA", DateTime.Now.ToString("yyyy-MM-dd"));
        }

        private void BtnAddIngreso_Click(object sender, EventArgs e)
        {
            FrmNuevoIngreso frmNuevoIngreso = new FrmNuevoIngreso
            {
                TopLevel = false,
                TopMost = false,
                FormBorderStyle = FormBorderStyle.None,
            };
            frmNuevoIngreso.OnIngresoSuccess += FrmNuevoIngreso_OnIngresoSuccess;
            frmNuevoIngreso.FormClosed += FrmNuevoIngreso_FormClosed;
            this.container = new PoperContainer(frmNuevoIngreso);
            this.container.Show(this.btnAddIngreso);
            frmNuevoIngreso.Show();
        }

        private void FrmNuevoIngreso_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.container.Close();
        }

        private async void FrmNuevoIngreso_OnIngresoSuccess(object sender, EventArgs e)
        {
            await this.BuscarMovimientos("FECHA", DateTime.Now.ToString("yyyy-MM-dd"));
        }

        private async void FrmObservarMovimientos_Load(object sender, EventArgs e)
        {
            await this.BuscarMovimientos("FECHA", DateTime.Now.ToString("yyyy-MM-dd"));
        }

        private async void DateBusqueda_ValueChanged(object sender, EventArgs e)
        {
            DateTimePicker date = (DateTimePicker)sender;
            await this.BuscarMovimientos("FECHA", date.Value.ToString("yyyy-MM-dd"));
        }

        private async Task BuscarMovimientos(string tipo_busqueda, string texto_busqueda)
        {
            try
            {
                var resultIngresos =
                    await NIngresos.BuscarIngresos(tipo_busqueda, texto_busqueda);

                var resultEgresos =
                    await NEgresos.BuscarEgresos(tipo_busqueda, texto_busqueda);

                this.txtResultados.Clear();

                StringBuilder info = new StringBuilder();

                if (resultEgresos.dtEgresos != null || resultIngresos.dtIngresos != null)
                {
                    int contador = 0;
                    if (resultEgresos.dtEgresos != null)
                    {
                        foreach (DataRow row in resultEgresos.dtEgresos.Rows)
                        {
                            Egresos egreso = new Egresos(row);
                            contador += 1;
                            info.Append(contador + "- EGRESO: ").Append(egreso.Fecha_egreso.ToLongDateString()).Append(" - ");
                            info.Append("Valor: ").Append(egreso.Valor_egreso.ToString("C").Replace(",00", "")).Append(" - ");
                            info.Append("Descripción: ").Append(egreso.Descripcion_egreso).Append(Environment.NewLine);                           
                        }
                        
                    }

                    if (resultIngresos.dtIngresos != null)
                    {
                        foreach (DataRow row in resultIngresos.dtIngresos.Rows)
                        {
                            Ingresos ingreso = new Ingresos(row);
                            contador += 1;
                            info.Append(contador + "- INGRESO: ").Append(ingreso.Fecha_ingreso.ToLongDateString()).Append(" - ");
                            info.Append("Valor: ").Append(ingreso.Valor_ingreso.ToString("C").Replace(",00", "")).Append(" - ");
                            info.Append("Descripción: ").Append(ingreso.Descripcion_ingreso).Append(Environment.NewLine);
                        }
                    }

                    this.txtResultados.Text = info.ToString();
                }
                else
                {
                    info.Append("Error cargando los movimientos");
                    this.txtResultados.Text = info.ToString();
                }
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "BuscarNominaEmpleados",
                    "HUbo un error al buscar la nómina de empleados", ex.Message);
            }
        }
    }
}
