using System;
using System.Data;

namespace CapaEntidades.Models
{
    public class Platos
    {
        public Platos()
        {

        }

        public Platos(DataRow row)
        {
            try
            {
                this.Id_plato = Convert.ToInt32(row["Id_plato"]);
                this.Nombre_plato = Convert.ToString(row["Nombre_plato"]);
                this.Id_tipo_plato = Convert.ToInt32(row["Id_tipo_plato"]);
                this.Precio_plato = Convert.ToDecimal(row["Precio_plato"]);
                this.Imagen_plato = Convert.ToString(row["Imagen_plato"]);
                this.Descripcion_plato = Convert.ToString(row["Descripcion_plato"]);
                this.Estado = Convert.ToString(row["Estado"]);
                this.Plato_detallado = Convert.ToString(row["Plato_detallado"]);
                this.Plato_carta = Convert.ToString(row["Plato_carta"]);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex.Message, null);
            }
        }

        public int Id_plato { get; set; }

        public string Nombre_plato { get; set; }

        public int Id_tipo_plato { get; set; }

        public decimal Precio_plato { get; set; }

        public string Imagen_plato { get; set; }

        public string Descripcion_plato { get; set; }

        public string Estado { get; set; }

        public string Plato_detallado { get; set; }

        public string Plato_carta { get; set; }

        public event EventHandler OnError;
    }
}
