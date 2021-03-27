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

namespace CapaPresentacion.Formularios.FormsPedido
{
    public partial class CustomPlato : UserControl
    {
        public CustomPlato()
        {
            InitializeComponent();
        }

        private async Task LoadIngredientes()
        {
            try
            {
                var (rpta, dtIngredientes) = 
                    await NIngredientes.BuscarIngredientes("COMPLETO", "");
                if (dtIngredientes != null)
                {

                }
                else
                    if (!rpta.Equals("OK"))
                    throw new Exception(rpta);
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "LoadIngredientes",
                    "Hubo un error cargando los ingredientes", ex.Message);
            }
        }
    }
}
