using System;

using System.Data;

namespace CapaPresentacion.Formularios.FormsReservas
{
    public class EReservas
    {
        public EReservas()
        {

        }

        public EReservas(DataRow row)
        {
            try
            {
                this.Id_reserva = Convert.ToInt32(row["Id_reserva"]);
                this.Id_mesa = Convert.ToInt32(row["Id_mesa"]);
                this.Num_mesa = Convert.ToInt32(row["Num_mesa"]);
                this.Id_cliente = Convert.ToInt32(row["Id_cliente"]);
                this.Nombre_cliente = Convert.ToString(row["Nombre_cliente"]);
                this.Fecha = Convert.ToString(row["Fecha"]);
                this.Hora = Convert.ToString(row["Hora"]);
                this.Observaciones = Convert.ToString(row["Observaciones"]);
                this.Estado = Convert.ToString(row["Estado"]);
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto("EReservas", "public EReservas(DataRow row)",
                    "Hubo un error al generar la entidad de reservas", ex.Message);
            }
        }

        public EReservas(DataTable dt, int fila)
        {
            try
            {
                this.Id_reserva = Convert.ToInt32(dt.Rows[fila]["Id_reserva"]);
                this.Id_mesa = Convert.ToInt32(dt.Rows[fila]["Id_mesa"]);
                this.Num_mesa = Convert.ToInt32(dt.Rows[fila]["Num_mesa"]);
                this.Id_cliente = Convert.ToInt32(dt.Rows[fila]["Id_cliente"]);
                this.Nombre_cliente = Convert.ToString(dt.Rows[fila]["Nombre_cliente"]);
                this.Fecha = Convert.ToString(dt.Rows[fila]["Fecha"]);
                this.Hora = Convert.ToString(dt.Rows[fila]["Hora"]);
                this.Observaciones = Convert.ToString(dt.Rows[fila]["Observaciones"]);
                this.Estado = Convert.ToString(dt.Rows[fila]["Estado"]);
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto("EReservas", "public EReservas(DataTable dt, int fila)",
                    "Hubo un error al generar la entidad de reservas", ex.Message);
            }
        }

        private int _id_reserva;
        private int _id_mesa;
        private int _num_mesa;
        private int _id_cliente;
        private string _nombre_cliente;
        private string _fecha;
        private string _hora;
        private string _observaciones;
        private string _estado;

        public int Id_reserva { get => _id_reserva; set => _id_reserva = value; }
        public int Id_mesa { get => _id_mesa; set => _id_mesa = value; }
        public int Num_mesa { get => _num_mesa; set => _num_mesa = value; }
        public int Id_cliente { get => _id_cliente; set => _id_cliente = value; }
        public string Nombre_cliente { get => _nombre_cliente; set => _nombre_cliente = value; }
        public string Fecha { get => _fecha; set => _fecha = value; }
        public string Hora { get => _hora; set => _hora = value; }
        public string Observaciones { get => _observaciones; set => _observaciones = value; }
        public string Estado { get => _estado; set => _estado = value; }
    }
}
