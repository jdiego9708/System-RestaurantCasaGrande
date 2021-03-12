using CapaNegocio;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CapaPresentacion.Formularios.FormsMesas
{
    public partial class FrmObservarMesas : Form
    {
        public FrmObservarMesas()
        {
            InitializeComponent();
            this.Load += FrmObservarMesas_Load;
        }

        public event EventHandler OnMesaClick;

        private void FrmObservarMesas_Load(object sender, EventArgs e)
        {
            this.BuscarMesas("COMPLETO", "");
        }

        private void BuscarMesas(string tipo_busqueda, string texto_busqueda)
        {
            try
            {
                DataTable dtMesas =
                    NMesas.BuscarMesas(tipo_busqueda, texto_busqueda);
                if (dtMesas != null)
                {
                    foreach (DataRow row in dtMesas.Rows)
                    {
                        MesaSmall mesa = new MesaSmall
                        {
                            Mesa = new EMesa(row)
                        };
                        mesa.AsignarDatos();
                        mesa.OnMesaClick += Mesa_OnMesaClick;
                        this.panelMesas.AddControl(mesa);
                    }
                }
                else
                {
                    this.lblResultados.Text = "No se encontraron mesas";
                }
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "BuscarMesas",
                    "Hubo un error al buscar mesas", ex.Message);
            }
        }

        private void Mesa_OnMesaClick(object sender, EventArgs e)
        {
            this.OnMesaClick?.Invoke(sender, e);
            this.Close();
        }
    }
}
