using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;

namespace CapaDatos
{
    public class DErrores
    {
        #region PATRON SINGLETON
        private static DErrores _Instancia;
        public static DErrores GetInstancia()
        {
            if (_Instancia == null)
            {
                _Instancia = new DErrores();
            }
            return _Instancia;
        }
        #endregion

        private DataTable TablaErrores = null;

        public DErrores()
        {
            TablaErrores = new DataTable("Errores " + DateTime.Now);
            TablaErrores.Columns.Add("Informacion_error", typeof(string));
            TablaErrores.Columns.Add("Clase_error", typeof(string));
            TablaErrores.Columns.Add("Metodo_error", typeof(string));
            TablaErrores.Columns.Add("Detalle_error", typeof(string));
            TablaErrores.Columns.Add("Fecha_hora_error", typeof(string));
        }

        public void NewError(string error_info, string error_class, string error_method,
            string error_details, string date)
        {
            DataRow row = this.TablaErrores.NewRow();
            row["Informacion_error"] = error_info;
            row["Clase_error"] = error_class;
            row["Metodo_error"] = error_method;
            row["Detalle_error"] = error_details;
            row["Fecha_hora_error"] = date;
            this.TablaErrores.Rows.Add(row);
        }
    }
}
