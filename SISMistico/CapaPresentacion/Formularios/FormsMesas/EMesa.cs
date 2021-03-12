using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaPresentacion.Formularios.FormsMesas
{
    public class EMesa
    {
        public EMesa()
        {

        }

        public EMesa(DataRow row)
        {
            try
            {
                this.Id_mesa = Convert.ToInt32(row["Id_mesa"]);
                this.Num_mesa = Convert.ToInt32(row["Num_mesa"]);
                this.Descripcion_mesa = Convert.ToString(row["Descripcion_mesa"]);
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto("EMesa.cs", "public EMesa(DataRow row)",
                    "Hubo un error al asignar la entidad de la mesa", ex.Message);
            }
        }

        public EMesa(DataTable dt, int fila)
        {
            try
            {
                this.Id_mesa = Convert.ToInt32(dt.Rows[fila]["Id_mesa"]);
                this.Num_mesa = Convert.ToInt32(dt.Rows[fila]["Num_mesa"]);
                this.Descripcion_mesa = Convert.ToString(dt.Rows[fila]["Descripcion_mesa"]);
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto("EMesa.cs", "public EMesa(DataTable dt, int fila)",
                    "Hubo un error al asignar la entidad de la mesa", ex.Message);
            }
        }

        private int _id_mesa;
        private int _num_mesa;
        private string _descripcion_mesa;

        public int Id_mesa { get => _id_mesa; set => _id_mesa = value; }
        public int Num_mesa { get => _num_mesa; set => _num_mesa = value; }
        public string Descripcion_mesa { get => _descripcion_mesa; set => _descripcion_mesa = value; }
    }
}
