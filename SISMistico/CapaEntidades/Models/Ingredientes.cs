namespace CapaEntidades.Models
{
    using System;
    using System.Data;

    public class Ingredientes
    {
        public Ingredientes()
        {

        }

        public Ingredientes(DataRow row)
        {
            try
            {
                this.Id_ingrediente = Convert.ToInt32(row["Id_ingrediente"]);
                this.Tipo_ingrediente = Convert.ToString(row["Tipo_ingrediente"]);
                this.Nombre_ingrediente = Convert.ToString(row["Nombre_ingrediente"]);
                this.Descripcion_ingrediente = Convert.ToString(row["Descripcion_ingrediente"]);
                this.Estado_ingrediente = Convert.ToString(row["Estado_ingrediente"]);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex.Message, null);
            }
        }

        public int Id_ingrediente { get; set; }

        public string Tipo_ingrediente { get; set; }

        public string Nombre_ingrediente { get; set; }

        public string Descripcion_ingrediente { get; set; }

        public string Estado_ingrediente { get; set; }

        public event EventHandler OnError;
    }
}
