using CapaEntidades.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaPresentacion.Formularios
{
    public class DatosInicioSesion
    {
        #region PATRON SINGLETON
        private static DatosInicioSesion _Instancia;
        public static DatosInicioSesion GetInstancia()
        {
            if (_Instancia == null)
            {
                _Instancia = new DatosInicioSesion();
            }
            return _Instancia;
        }
        #endregion

        private int id_empleado;
        private string nombre_empleado;
        private string cargo_empleado;
        private Turno _turno;
        private Empleados _empleadoLogin;
        private Empleados _empleadoClaveMaestra;
        private Clientes _clienteDefault;

        public int NumeroComandas { get; set; }
        public int Id_empleado { get => id_empleado; set => id_empleado = value; }
        public string Nombre_empleado { get => nombre_empleado; set => nombre_empleado = value; }
        public string Cargo_empleado { get => cargo_empleado; set => cargo_empleado = value; }
        public Turno Turno { get => _turno; set => _turno = value; }
        public Empleados EmpleadoLogin 
        { get => _empleadoLogin; set => _empleadoLogin = value; }
        public Empleados EmpleadoClaveMaestra 
        { get => _empleadoClaveMaestra; set => _empleadoClaveMaestra = value; }
        public Clientes ClienteDefault { get => _clienteDefault; set => _clienteDefault = value; }

        public bool Adiministrador()
        {
            if (Cargo_empleado.Equals("ADMINISTRADOR"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}
