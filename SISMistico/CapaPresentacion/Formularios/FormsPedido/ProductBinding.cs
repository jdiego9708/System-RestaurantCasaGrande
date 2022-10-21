namespace CapaPresentacion.Formularios.FormsPedido
{
    using CapaEntidades.Models;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Text;

    public class ProductBinding
    {
        public ProductBinding()
        {

        }

        public ProductBinding(DataRow row)
        {
            try
            {
                if (row.Table.Columns.Contains("Id_detalle_producto"))
                    this.Id_detalle_producto = Convert.ToInt32(row["Id_detalle_producto"]);

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
        public int Id_detalle_producto { get; set; }

        public List<ProductDetalleBinding> ProductDetalles { get; set; }
        public string NombreImagen { get; set; }
        public bool IsEditar { get; set; }
        public bool IsAddBD { get; set; }
        private string informacion;

        public string Informacion {
            get 
            {
                StringBuilder info = new StringBuilder();
                info.Append(Tipo_producto + " - ").Append(this.Nombre).Append(Environment.NewLine);
                info.Append("Precio: ").Append(this.Precio.ToString("C").Replace(",00", "")).Append(Environment.NewLine);

                if (this.Cantidad > 0)
                    info.Append("Cantidad: ").Append(this.Cantidad).Append(Environment.NewLine);

                if (this.Tipo_producto.Equals("PLATO"))
                {
                    if (this.ProductDetalles != null)
                    {
                        foreach(ProductDetalleBinding pr in this.ProductDetalles)
                        {
                            info.Append(pr.Ingrediente.Tipo_ingrediente).Append(": ");
                            info.Append(pr.Ingrediente.Nombre_ingrediente).Append(Environment.NewLine);
                        }
                    }
                }

                informacion = info.ToString();

                return informacion; 
            }
            set => informacion = value; }

        public event EventHandler OnError;
    }

    public class ProductDetalleBinding
    {
        public ProductDetalleBinding()
        {

        }

        public ProductDetalleBinding(DataRow row)
        {
            try
            {
                this.Id_detalle_ingrediente_pedido = Convert.ToInt32(row["Id_detalle_ingrediente_pedido"]);
                this.Id_detalle_pedido = Convert.ToInt32(row["Id_detalle_pedido"]);
                this.Id_pedido = Convert.ToInt32(row["Id_pedido"]);
                this.Id_tipo = Convert.ToInt32(row["Id_tipo"]);
                this.Id_ingrediente = Convert.ToInt32(row["Id_ingrediente"]);
                this.Ingrediente = new Ingredientes(row);
                this.Observaciones = Convert.ToString(row["Observaciones"]);
            }
            catch (Exception ex)
            {
                this.OnError?.Invoke(ex, null);
            }
        }

        public int Id_detalle_ingrediente_pedido { get; set; }
        public int Id_detalle_pedido { get; set; }
        public int Id_pedido { get; set; }
        public int Id_tipo { get; set; }       
        public int Id_ingrediente { get; set; }
        public Ingredientes Ingrediente { get; set; }
        public string Observaciones { get; set; }

        public event EventHandler OnError;
    }
}
