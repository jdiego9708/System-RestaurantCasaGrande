namespace CapaEntidades.Models
{
    using System;
    using System.Data;

    public class Tipo_bebidas
    {
        public Tipo_bebidas()
        {

        }

        public Tipo_bebidas(DataRow row)
        {
            try
            {
                this.Id_tipo_bebida = Convert.ToInt32(row["Id_tipo_bebida"]);
                this.Tipo_bebida = Convert.ToString(row["Tipo_bebida"]);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex.Message, null);
            }
        }

        public int Id_tipo_bebida { get; set; }

        public string Tipo_bebida { get; set; }

        public event EventHandler OnError;
    }
}
