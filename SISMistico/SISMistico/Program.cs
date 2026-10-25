using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using CapaPresentacion.Formularios.FormsPrincipales;

namespace SISMistico
{
    static class Program
    {
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            // API móvil: apagada por defecto (ApiHabilitada); si falla al arrancar, el sistema abre igual.
            CapaPresentacion.Api.ApiServer.Iniciar();
            Application.Run(new FrmIniciarSesion());
            CapaPresentacion.Api.ApiServer.Detener();
        }
    }
}
