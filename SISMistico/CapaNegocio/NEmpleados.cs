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
    public class NEmpleados
    {
        #region INSERTAR EMPLEADO

        public static string InsertarEmpleado(string[] variables)
        {
            DEmpleados DEmpleados = new DEmpleados();
            DEmpleados.Variables = variables;
            return DEmpleados.InsertarEmpleado(DEmpleados);
        }

        #endregion

        #region EDITAR EMPLEADO

        public static string EditarEmpleado(string[] variables)
        {
            DEmpleados DEmpleados = new DEmpleados();
            DEmpleados.Variables = variables;
            return DEmpleados.EditarEmpleado(DEmpleados);
        }

        #endregion

        #region BUSCAR EMPLEADOS

        public static DataTable BuscarEmpleados(string tipo_busqueda, string texto_busqueda, out string rpta)
        {
            return DEmpleados.BuscarEmpleados(tipo_busqueda, texto_busqueda, out rpta);
        }

        #endregion

        #region LOGIN


        public static async Task<(string rpta, List<object> objects)> Login(string usuario,
            string pass, string fecha)
        {
            DEmpleados DEmpleados = new DEmpleados();
            return await DEmpleados.Login(usuario, pass, fecha);
        }

        public static async Task<(string rpta, Empleados empleado, DataTable dtEmpleado)> ClaveMaestra(int codigo)
        {
            DEmpleados DEmpleados = new DEmpleados();
            return await DEmpleados.ClaveMaestra(codigo);
        }

        public static DataTable Login(string tipo_busqueda,
            string texto_busqueda1, string texto_busqueda2, out string rpta)
        {
            rpta = "ok";
            return new DataTable();
            //return DEmpleados.Login(tipo_busqueda, texto_busqueda1, texto_busqueda2, out rpta);
        }

        #endregion

        #region ACTUALIZAR CLAVE MAESTRA

        public static string ActualizarClaveMaestra(int id_empleado, int clave_maestra)
        {
            DEmpleados DEmpleados = new DEmpleados();
            return DEmpleados.ActualizarClaveMaestra(id_empleado, clave_maestra);
        }

        #endregion

        #region ACTUALIZAR CLAVE USUARIO

        public static string ActualizarClaveUsuario(int id_empleado, string pass)
        {
            DEmpleados DEmpleados = new DEmpleados();
            return DEmpleados.ActualizarPassword(id_empleado, pass);
        }

        #endregion

        #region ACTUALIZAR INACTIVAR EMPLEADO

        public static string InactivarEmpleado(int id_empleado)
        {
            DEmpleados DEmpleados = new DEmpleados();
            return DEmpleados.InactivarEmpleado(id_empleado);
        }

        #endregion

        #region ACTUALIZAR ACTIVAR EMPLEADO

        public static string ActivarEmpleado(int id_empleado)
        {
            DEmpleados DEmpleados = new DEmpleados();
            return DEmpleados.ActivarEmpleado(id_empleado);
        }

        #endregion
    }
}
