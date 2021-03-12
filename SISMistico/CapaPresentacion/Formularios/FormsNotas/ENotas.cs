using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaPresentacion.Formularios.FormsNotas
{
    public class ENotas
    {
        public ENotas()
        {

        }

        public ENotas(DataRow row)
        {
            try
            {
                this.Id_nota = Convert.ToInt32(row["Id_nota"]);
                this.Id_empleado = Convert.ToInt32(row["Id_empleado"]);
                this.Fecha_nota = Convert.ToDateTime(row["Fecha_nota"]);
                this.Hora_nota = Convert.ToString(row["Hora_nota"]);
                this.Titulo_nota = Convert.ToString(row["Titulo_nota"]);
                this.Descripcion_nota = Convert.ToString(row["Descripcion_nota"]);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex.Message, null);
            }
        }

        public ENotas(DataTable dt, int fila)
        {
            try
            {
                this.Id_nota = Convert.ToInt32(dt.Rows[fila]["Id_nota"]);
                this.Id_empleado = Convert.ToInt32(dt.Rows[fila]["Id_empleado"]);
                this.Fecha_nota = Convert.ToDateTime(dt.Rows[fila]["Fecha_nota"]);
                this.Hora_nota = Convert.ToString(dt.Rows[fila]["Hora_nota"]);
                this.Titulo_nota = Convert.ToString(dt.Rows[fila]["Titulo_nota"]);
                this.Descripcion_nota = Convert.ToString(dt.Rows[fila]["Descripcion_nota"]);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex.Message, null);
            }
        }

        public event EventHandler OnError;
        private int _id_nota;
        private int _id_empleado;
        private DateTime _fecha_nota;
        private string _hora_nota;
        private string _titulo_nota;
        private string _descripcion_nota;

        public int Id_nota { get => _id_nota; set => _id_nota = value; }
        public int Id_empleado { get => _id_empleado; set => _id_empleado = value; }
        public DateTime Fecha_nota { get => _fecha_nota; set => _fecha_nota = value; }
        public string Hora_nota { get => _hora_nota; set => _hora_nota = value; }
        public string Titulo_nota { get => _titulo_nota; set => _titulo_nota = value; }
        public string Descripcion_nota { get => _descripcion_nota; set => _descripcion_nota = value; }
    }
}
