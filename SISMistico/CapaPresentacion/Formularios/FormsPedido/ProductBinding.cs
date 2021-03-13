namespace CapaPresentacion.Formularios.FormsPedido
{
    using System;
    using System.Data;

    public class ProductBinding
    {
        public ProductBinding()
        {

        }

        public ProductBinding(DataRow row)
        {
            try
            {
                this.Nombre = Convert.ToString(row["Nombre"]);
                this.Id_producto = Convert.ToInt32(row["Id_producto"]);
                this.Tipo_producto = Convert.ToString(row["Tipo_producto"]);
                this.Precio = Convert.ToDecimal(row["Precio"]);
                this.Observaciones = Convert.ToString(row["Observaciones"]);
                this.Cantidad = Convert.ToInt32(row["Cantidad"]);
            }
            catch (Exception ex)
            {
                this.OnError?.Invoke(ex, null);
            }
        }

        public int Id_producto { get; set; }
        public string Nombre { get; set; }
        public string Tipo_producto { get; set; }
        public decimal Precio { get; set; }
        public string Observaciones { get; set; }
        public int Cantidad { get; set; }
        public object Product { get; set; }

        public bool IsEditar { get; set; }
        public event EventHandler OnError;
    }
}
