using CapaNegocio;
using System.Data;
using System.Windows.Forms;

namespace CapaPresentacion
{
    public class LlenarListas
    {
        public static ComboBox ListaHoras(ComboBox lista)
        {
            lista.Items.Clear();
            lista.DataSource = null;

            DataTable dtHoras = new DataTable("Horas");
            dtHoras.Columns.Add("Hora");
            dtHoras.Columns.Add("Texto_hora");

            int contador24hours = 1;
            int contador12hours = 1;
            string tipo = "am";
            while (contador24hours <= 24)
            {
                DataRow row = dtHoras.NewRow();
                row["Hora"] = contador24hours + ":00";
                row["Texto_hora"] = contador12hours + ":00 " + tipo;
                dtHoras.Rows.Add(row);
                contador24hours += 1;
                contador12hours += 1;
                if (contador12hours > 12)
                {
                    contador12hours = 1;
                    tipo = "pm";
                }
            }

            lista.DataSource = dtHoras;
            lista.ValueMember = "Hora";
            lista.DisplayMember = "Texto_hora";

            return lista;
        }
        public static ComboBox LlenarListaCargo(ComboBox lista)
        {
            lista.Items.Clear();
            lista.Items.Add("ADMINISTRADOR");
            lista.Items.Add("MESERO");
            lista.Items.Add("CAJERO");
            lista.Items.Add("DOMICILIARIO");
            lista.Items.Add("COCINERO");
            lista.Items.Add("SERVICIOS VARIOS");
            return lista;
        }

        public static ComboBox LlenarListaBusquedaEmpleado(ComboBox lista)
        {
            lista.Items.Clear();
            lista.Items.Add("NOMBRE");
            return lista;
        }

        public static ComboBox LlenarListaTipoPlatos(ComboBox lista)
        {
            lista.DataSource = NPlatos.BuscarTipoPlatos("COMPLETO", "");
            lista.ValueMember = "Id_tipo_plato";
            lista.DisplayMember = "Tipo_plato";
            return lista;
        }

        public static ComboBox ListaBuscarPlatos(ComboBox lista)
        {
            lista.Items.Clear();
            lista.Items.Add("NOMBRE");
            lista.Items.Add("TIPO");
            return lista;
        }

        public static ComboBox LlenarListaTipoBebidas(ComboBox lista)
        {
            lista.DataSource = NBebidas.BuscarTipoBebidas("COMPLETO", "");
            lista.ValueMember = "Id_tipo_bebida";
            lista.DisplayMember = "Tipo_bebida";
            return lista;
        }

        public static ComboBox ListaBuscarBebidas(ComboBox lista)
        {
            lista.Items.Clear();
            lista.Items.Add("NOMBRE");
            lista.Items.Add("TIPO");
            return lista;
        }

        public static ComboBox LlenarListaTipoInsumos(ComboBox lista)
        {
            lista.DataSource = NInsumos.BuscarTipoInsumos("COMPLETO", "");
            lista.ValueMember = "Id_tipo_insumo";
            lista.DisplayMember = "Tipo_insumo";
            return lista;
        }

        public static ComboBox LlenarListaTipoMedidaInsumos(ComboBox lista)
        {
            lista.Items.Clear();
            lista.Items.Add("PORCIONES");
            return lista;
        }

        public static ComboBox LlenarListaMesas(ComboBox lista)
        {
            DataTable tabla = NMesas.BuscarMesas("COMPLETO", "");
            lista.DataSource = tabla;
            lista.DisplayMember = "Num_mesa";
            lista.ValueMember = "Id_mesa";
            return lista;
        }
    }
}
