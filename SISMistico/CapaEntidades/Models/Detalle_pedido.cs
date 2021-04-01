namespace CapaEntidades.Models
{
    using System;
    using System.Data;

    public class Detalle_pedido
    {
        public Detalle_pedido()
        {

        }

        public Detalle_pedido(DataRow row)
        {
            try
            {
                this.Id_pedido = Convert.ToInt32(row["Id_pedido"]);
                this.Id_tipo = Convert.ToInt32(row["Id_tipo"]);
                this.Tipo = Convert.ToString(row["Tipo"]);
                this.Precio = Convert.ToDecimal(row["Precio"]);
                this.Cantidad = Convert.ToInt32(row["Cantidad"]);
                this.Observaciones = Convert.ToString(row["Observaciones"]);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex.Message, null);
            }
        }

        public int Id_pedido { get; set; }

        public int Id_tipo { get; set; }

        public string Tipo { get; set; }

        public decimal Precio { get; set; }

        public int Cantidad { get; set; }

        public string Observaciones { get; set; }

        public event EventHandler OnError;
    }
}
