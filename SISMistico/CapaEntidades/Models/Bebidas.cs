using System;
using System.Data;

namespace CapaEntidades.Models
{
    public class Bebidas
    {
        public Bebidas()
        {

        }

        public Bebidas(DataRow row)
        {
            try
            {
                this.Id_bebida = Convert.ToInt32(row["Id_bebida"]);
                this.Nombre_bebida = Convert.ToString(row["Nombre_bebida"]);
                this.Descripcion_bebida = Convert.ToString(row["Descripcion_bebida"]);
                this.Precio_bebida = Convert.ToDecimal(row["Precio_bebida"]);
                this.Precio_trago = Convert.ToDecimal(row["Precio_trago"]);
                this.Precio_trago_doble = Convert.ToDecimal(row["Precio_trago_doble"]);
                this.Precio_proveedor = Convert.ToDecimal(row["Precio_proveedor"]);
                this.Id_proveedor = Convert.ToInt32(row["Id_proveedor"]);
                this.Imagen = Convert.ToString(row["Imagen"]);
                this.Id_tipo_bebida = Convert.ToInt32(row["Id_tipo_bebida"]);
                this.Cantidad_unidades = Convert.ToInt32(row["Cantidad_unidades"]);
                this.Cantidad_x_unidad = Convert.ToDecimal(row["Cantidad_x_unidad"]);
                this.Cantidad_total = Convert.ToDecimal(row["Cantidad_total"]);
                this.Estado = Convert.ToString(row["Estado"]);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex.Message, null);
            }
        }

        public int Id_bebida { get; set; }

        public string Nombre_bebida { get; set; }

        public string Descripcion_bebida { get; set; }

        public decimal Precio_bebida { get; set; }

        public decimal Precio_trago { get; set; }

        public decimal Precio_trago_doble { get; set; }

        public decimal Precio_proveedor { get; set; }

        public int Id_proveedor { get; set; }

        public string Imagen { get; set; }

        public int Id_tipo_bebida { get; set; }

        public int Cantidad_unidades { get; set; }

        public decimal Cantidad_x_unidad { get; set; }

        public decimal Cantidad_total { get; set; }

        public string Estado { get; set; }

        public event EventHandler OnError;
    }
}
