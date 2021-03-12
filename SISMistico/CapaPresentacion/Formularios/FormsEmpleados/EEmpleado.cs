using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaPresentacion.Formularios.FormsEmpleados
{
    public class EEmpleado
    {
        public EEmpleado()
        {

        }

        public EEmpleado(DataRow row)
        {
            try
            {
                this.Id_empleado = Convert.ToInt32(row["Id_empleado"]);
                this.Nombre_empleado = Convert.ToString(row["Nombre_empleado"]);
                this.Telefono_empleado = Convert.ToString(row["Telefono_empleado"]);
                this.Correo_electronico = Convert.ToString(row["Correo_electronico"]);
                this.Cargo_empleado = Convert.ToString(row["Cargo_empleado"]);
                this.Password = Convert.ToString(row["Password"]);
                this.Codigo_maestro = Convert.ToInt32(row["Codigo_maestro"]);
                this.Estado_empleado = Convert.ToString(row["Estado"]);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex.Message, null);
            }
        }

        public EEmpleado(DataTable dt, int fila)
        {
            try
            {
                this.Id_empleado = Convert.ToInt32(dt.Rows[fila]["Id_empleado"]);
                this.Nombre_empleado = Convert.ToString(dt.Rows[fila]["Nombre_empleado"]);
                this.Telefono_empleado = Convert.ToString(dt.Rows[fila]["Telefono_empleado"]);
                this.Correo_electronico = Convert.ToString(dt.Rows[fila]["Correo_electronico"]);
                this.Cargo_empleado = Convert.ToString(dt.Rows[fila]["Cargo_empleado"]);
                this.Password = Convert.ToString(dt.Rows[fila]["Password"]);
                this.Codigo_maestro = Convert.ToInt32(dt.Rows[fila]["Codigo_maestro"]);
                this.Estado_empleado = Convert.ToString(dt.Rows[fila]["Estado"]);

            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex.Message, null);
            }
        }

        public event EventHandler OnError;

        private int _id_empleado;
        private string _nombre_empleado;
        private string _telefono_empleado;
        private string _correo_electronico;
        private string _cargo_empleado;
        private string _password;
        private int _codigo_maestro;
        private string _estado_empleado;

        public int Id_empleado { get => _id_empleado; set => _id_empleado = value; }
        public string Nombre_empleado { get => _nombre_empleado; set => _nombre_empleado = value; }
        public string Correo_electronico { get => _correo_electronico; set => _correo_electronico = value; }
        public string Telefono_empleado { get => _telefono_empleado; set => _telefono_empleado = value; }
        public string Cargo_empleado { get => _cargo_empleado; set => _cargo_empleado = value; }
        public string Password { get => _password; set => _password = value; }
        public int Codigo_maestro { get => _codigo_maestro; set => _codigo_maestro = value; }
        public string Estado_empleado { get => _estado_empleado; set => _estado_empleado = value; }
    }
}
