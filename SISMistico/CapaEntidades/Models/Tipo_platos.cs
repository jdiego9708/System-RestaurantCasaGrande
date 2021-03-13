namespace CapaEntidades.Models
{
    using System;
    using System.Data;

    public class Tipo_platos
    {
        public Tipo_platos()
        {

        }

        public Tipo_platos(DataRow row)
        {
            try
            {
                this.Id_tipo_plato = Convert.ToInt32(row["Id_tipo_plato"]);
                this.Tipo_plato = Convert.ToString(row["Tipo_plato"]);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex.Message, null);
            }
        }

        public int Id_tipo_plato { get; set; }

        public string Tipo_plato { get; set; }

        public event EventHandler OnError;
    }
}
