using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using System.Data.SqlClient;

namespace CapaDatos
{
    public class DNotas
    {
        public static string InsertarNota(List<string> variables, out int id_nota)
        {
            string consulta = "INSERT INTO Notas_empleado VALUES ('" + variables[0] + "','" + variables[1] + "','" +
                variables[2] + "','" + variables[3] + "','" + variables[4] + "' ) " +
                "SELECT CAST(SCOPE_IDENTITY() AS int) ";
            string rpta = Conexion.EjecutarConsultaCadena(consulta, true);
            id_nota = Conexion.Id_generado;
            return rpta;
        }

        public static string EditarNota(int id_nota, List<string> variables)
        {
            string consulta = "UPDATE Notas_empleado SET " +
                "Id_empleado = '" + variables[0] + "', " +
                "Fecha_nota = '" + variables[1] + "', " +
                "Hora_nota = '" + variables[2] + "', " +
                "Titulo_nota = '" + variables[3] + "', " +
                "Descripcion_nota = '" + variables[4] + "' " +
                "WHERE Id_nota = '" + id_nota + "' ";
            return Conexion.EjecutarConsultaCadena(consulta, false);
        }

        public static DataTable BuscarNotas(string tipo_busqueda, string texto_busqueda,
            out string rpta)
        {
            StringBuilder consulta = new StringBuilder();
            consulta.Append("SELECT * FROM Notas_empleado no INNER JOIN Empleados em ON no.Id_empleado = em.Id_empleado ");

            if (tipo_busqueda.Equals("ID EMPLEADO"))
            {
                consulta.Append("WHERE no.Id_empleado = '" + texto_busqueda + "' ");
            }

            consulta.Append("ORDER BY no.Id_nota DESC");
            return Conexion.EjecutarConsultaDt(Convert.ToString(consulta), out rpta);
        }
    }
    
}
