namespace CapaEntidades.Models
{
    using System;
    using System.Data;

    public class Detalle_ingredientes_pedido
    {
        public Detalle_ingredientes_pedido()
        {

        }

        public Detalle_ingredientes_pedido(DataRow row)
        {
            try
            {
                this.Id_detalle_ingrediente_pedido = Convert.ToInt32(row["Id_detalle_ingrediente_pedido"]);
                this.Id_ingrediente = Convert.ToInt32(row["Id_ingrediente"]);
                this.Id_pedido = Convert.ToInt32(row["Id_pedido"]);
                this.Id_tipo = Convert.ToInt32(row["Id_tipo"]);
                this.Id_detalle_pedido = Convert.ToInt32(row["Id_detalle_pedido"]);
                this.Observaciones = Convert.ToString(row["Observaciones"]);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex.Message, null);
            }
        }

        public int Id_detalle_ingrediente_pedido { get; set; }

        public int Id_pedido { get; set; }

        public int Id_tipo { get; set; }

        public int Id_ingrediente { get; set; }

        public int Id_detalle_pedido { get; set; }

        public Ingredientes Ingrediente { get; set; }

        public string Observaciones { get; set; }

        public event EventHandler OnError;
    }
}
