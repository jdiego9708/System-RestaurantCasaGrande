namespace CapaEntidades.Models
{
    using CapaEntidades.Helpers;
    using System;
    using System.Data;

    public class Turno
    {
        public Turno()
        {

        }

        public Turno(DataRow row)
        {
            try
            {
                this.Id_turno = ConvertValueHelper.ConvertirNumero(row["Id_turno"]);
                this.Fecha_turno = ConvertValueHelper.ConvertirFecha(row["Fecha_turno"]);
                //this.Hora_turno = ConvertValueHelper.ConvertirHora(row["Hora_turno"]);
                this.Valor_inicial = ConvertValueHelper.ConvertirDecimal(row["Valor_inicial"]);
                this.Total_ingresos = ConvertValueHelper.ConvertirDecimal(row["Total_ingresos"]);
                this.Total_egresos = ConvertValueHelper.ConvertirDecimal(row["Total_egresos"]);
                this.Total_ventas = ConvertValueHelper.ConvertirDecimal(row["Total_ventas"]);
                this.Total_nomina = ConvertValueHelper.ConvertirDecimal(row["Total_nomina"]);
                this.Total_turno = ConvertValueHelper.ConvertirDecimal(row["Total_turno"]);
                this.Estado_turno = ConvertValueHelper.ConvertirCadena(row["Estado_turno"]);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex.Message, null);
            }
        }

        public int Id_turno { get; set; }
        public DateTime Fecha_turno { get; set; }
        public TimeSpan Hora_turno { get; set; }
        public decimal Valor_inicial { get; set; }
        public decimal Total_ingresos { get; set; }
        public decimal Total_egresos { get; set; }
        public decimal Total_ventas { get; set; }
        public decimal Total_nomina { get; set; }
        public decimal Total_turno { get; set; }
        public string Estado_turno { get; set; }

        public event EventHandler OnError;
    }
}
