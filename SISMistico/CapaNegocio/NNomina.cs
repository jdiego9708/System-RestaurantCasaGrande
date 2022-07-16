using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CapaDatos;
using System.Data;
using CapaEntidades.Models;

namespace CapaNegocio
{
    public class NNomina
    {
        #region INSERTAR NOMINA EMPLEADO

        public static async Task<(string rpta, int id_nomina)> InsertarNomina(EmpleadoNominaBinding empleadoNomina)
        {
            DNomina DNomina = new DNomina();
            return await DNomina.InsertarNomina(empleadoNomina);
        }

        #endregion

        #region EDITAR NOMINA

        public static async Task<string> EditarNomina(int id_nomina, EmpleadoNominaBinding empleadoNomina)
        {
            DNomina DNomina = new DNomina();
            return await DNomina.EditarNomina(id_nomina, empleadoNomina);
        }
        #endregion

        #region BUSCAR EMPLEADOS

        public static DataTable BuscarNomina(string tipo_busqueda, string texto_busqueda, out string rpta)
        {
            DNomina DNomina = new DNomina();
            return DNomina.BuscarNomina(tipo_busqueda, texto_busqueda, out rpta);
        }

        public static DataTable BuscarNomina(string tipo_busqueda, string texto_busqueda1, 
            string texto_busqueda2, out string rpta)
        {
            DNomina DNomina = new DNomina();
            return DNomina.BuscarNomina(tipo_busqueda, texto_busqueda1, texto_busqueda2, out rpta);
        }

        #endregion

        #region BUSCAR ESTADISTICA DIARIA

        public static async Task<(string rpta, DataTable dtEstadistica, DataTable dtDetalle, DataTable dtPagos)> EstadisticasDiarias(int id_turno, string fecha)
        {
            DNomina DNomina = new DNomina();
            return await DNomina.BuscarEstadistica(id_turno, fecha);
        }

        public static async Task<(string rpta, DataTable dtEstadistica, DataTable dtDetalle, DataTable dtPagos)> EstadisticasDiarias(string fecha1, string fecha2)
        {
            DNomina DNomina = new DNomina();
            return await DNomina.BuscarEstadistica(fecha1, fecha2);
        }

        #endregion
    }
}
