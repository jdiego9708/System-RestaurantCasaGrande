using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CapaDatos;
using System.Data;
using CapaEntidades.Models;
using CapaEntidades.Helpers;

namespace CapaNegocio
{
    public class NTurnos
    {

        public static Task<(string rpta, DataTable dtTurnos)> BuscarTurnos(string tipo_busqueda, 
            string texto_busqueda1,
            string texto_busqueda2)
        {
            DTurnos DTurnos = new DTurnos();
            return DTurnos.BuscarTurnos(tipo_busqueda, texto_busqueda1, texto_busqueda2);
        }

        public static Task<string> InsertarTurno(Turno turno)
        {
            DTurnos DTurno = new DTurnos();
            return DTurno.InsertarTurno(turno);
        }

        public static Task<string> InsertarBase(int id_turno, decimal base_nueva)
        {
            DTurnos DTurno = new DTurnos();
            return DTurno.InsertarBase(id_turno, base_nueva);   
        }

        public static Task<string> EditarEstadoTurno(int id_turno, string estado)
        {
            DTurnos DTurno = new DTurnos();
            return DTurno.EditarEstadoTurno(id_turno, estado);
        }
    }
}
