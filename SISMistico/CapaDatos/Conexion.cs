using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace CapaDatos
{
    public class Conexion
    {
        //Obtener la cadena de conexión
        public static string ObtenerCadenaDeConexion(string Nombre_cadena_de_conexion, string tipo_dato)
        {
            string cadena = "";
            // se obtienen las conexiones
            ConnectionStringSettingsCollection connections = ConfigurationManager.ConnectionStrings;

            //si existe por lo menos una conexión continuamos
            if (connections.Count != 0)
            {
                //Recorremos las conexiones existentes
                foreach (ConnectionStringSettings connection in connections)
                {
                    //asignamos el nombre
                    string name = connection.Name;
                    //obtenemos el proveedor, solo por demostración, no lo utilizaremos ni nada.
                    string provider = connection.ProviderName;
                    //obtenemos la cadena
                    string connectionString = connection.ConnectionString;

                    //comparamos el nombre al de nuestro atributo de la clase para verificar si es la cadena
                    //de conexión que modificaremos
                    if (name.Equals(Nombre_cadena_de_conexion) && tipo_dato.Equals("COMPLETA"))
                    {
                        cadena = connectionString;
                        break;
                    }
                    else if (name.Equals(Nombre_cadena_de_conexion) && tipo_dato.Equals("NOMBRE SERVIDOR"))
                    {
                        cadena = name;
                        break;
                    }
                    else if (name.Equals(Nombre_cadena_de_conexion) && tipo_dato.Equals("COMPLETA SIN PASS"))
                    {
                    }
                }
            }
            else
            {
                Console.WriteLine("No existe la conexión");
            }
            return cadena;
        }
        public static string Cn = ObtenerCadenaDeConexion(ConfigurationManager.AppSettings["ConexionBaseDeDatos"].ToString(), "COMPLETA");

        public static string EjecutarConsultaCadena(string consulta, bool returnIdentity)
        {
            string rpta = "OK";
            SqlConnection cnn = new SqlConnection
            {
                ConnectionString = Cn
            };
            try
            {
                cnn.Open();
                SqlCommand cmd = new SqlCommand(consulta, cnn);

                if (returnIdentity)
                {
                    int id = Convert.ToInt32(cmd.ExecuteScalar());
                    Id_generado = id;
                    if (id > 0)
                        rpta = "OK";
                    else
                        throw new Exception("La identificación única (ID) no se obtuvo correctamente");
                }
                else
                {
                    rpta = cmd.ExecuteNonQuery() >= 1 ? "OK" : "ERROR";
                }
            }
            catch (SqlException ex)
            {
                rpta = ex.Message;
            }
            catch (Exception ex)
            {
                rpta = ex.Message;
            }
            finally
            {
                if (cnn.State == ConnectionState.Open)
                    cnn.Close();
            }

            return rpta;
        }

        public static DataTable EjecutarConsultaDt(string consulta, out string rpta)
        {
            rpta = "OK";
            DataTable tabla = new DataTable();
            try
            {
                SqlConnection cnn = new SqlConnection
                {
                    ConnectionString = Cn
                };

                cnn.Open();
                SqlCommand m = new SqlCommand(consulta, cnn);
                SqlDataAdapter da = new SqlDataAdapter(m);

                da.Fill(tabla);

                if (tabla.Rows.Count < 1)
                    tabla = null;

                return tabla;
            }
            catch (SqlException ex)
            {
                rpta = ex.Message;
                MessageBox.Show("Hubo un error al ejecutar la consulta 'SqlException', detalles: " +
                    ex.Message + " Consulta: " + consulta);
                return null;
            }
            catch (Exception ex)
            {
                rpta = ex.Message;
                MessageBox.Show("Hubo un error al ejecutar la consulta 'Exception', detalles: " + ex.Message + " Consulta: " + consulta);
                return null;
            }
        }

        public static int Id_generado;
    }
}
