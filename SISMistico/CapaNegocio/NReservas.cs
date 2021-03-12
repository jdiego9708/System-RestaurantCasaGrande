using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CapaDatos;
using System.Data;

namespace CapaNegocio
{
    public class NReservas
    {
        public static string InsertarReserva(out int id_reserva,
            int id_mesa, int id_cliente, string fecha, string hora, string observaciones)
        {
            DReservas dReservas = new DReservas();
            return dReservas.InsertarReserva(out id_reserva, id_mesa, id_cliente, fecha, hora, observaciones);
        }

        public static string EditarReserva(int id_reserva,
            int id_mesa, int id_cliente, string fecha, string hora, string observaciones, string estado)
        {
            DReservas dReservas = new DReservas();
            return dReservas.EditarReserva(id_reserva, id_mesa, id_cliente, fecha, hora, observaciones, estado);
        }

        public static DataTable BuscarReservas(string tipo_busqueda, string texto_busqueda, string estado, out string rpta)
        {
            DReservas dReservas = new DReservas();
            return dReservas.BuscarReservas(tipo_busqueda, texto_busqueda, estado, out rpta);
        }
    }
}
