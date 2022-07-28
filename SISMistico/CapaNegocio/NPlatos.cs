using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using CapaDatos;

namespace CapaNegocio
{
    public class NPlatos
    {
        #region INSERTAR PLATOS
        public static string InsertarPlatos(string[] variables, out int id_plato)
        {
            DPlatos DPlatos = new DPlatos();
            DPlatos.Variables = variables;
            return DPlatos.InsertarPlatos(DPlatos, out id_plato);
        }

        #endregion

        #region EDITAR PLATOS
        public static string EditarPlatos(string[] variables)
        {
            DPlatos DPlatos = new DPlatos();
            DPlatos.Variables = variables;
            return DPlatos.EditarPlatos(DPlatos);
        }

        #endregion

        #region BUSCAR PLATOS
        public static DataTable BuscarPlatos(string tipo_busqueda, string texto_busqueda, string estado_plato,
            out string rpta)
        {
            return DPlatos.BuscarPlatos(tipo_busqueda, texto_busqueda, estado_plato, out rpta);
        }

        #endregion

        #region BUSCAR TIPO PLATOS
        public static DataTable BuscarTipoPlatos(string tipo_busqueda, string texto_busqueda)
        {
            return DPlatos.BuscarTipoPlatos(tipo_busqueda, texto_busqueda);
        }

        #endregion

        #region INSERTAR DETALLE PLATOS
        public static string InsertarDetallePlatos(List<string> variables)
        {
            DPlatos DPlatos = new DPlatos();
            return DPlatos.InsertarDetallePlatos(variables);
        }

        #endregion

        #region EDITAR DETALLE PLATOS
        public static string EditarDetallePlatos(List<string> variables)
        {
            DPlatos DPlatos = new DPlatos();
            return DPlatos.EditarDetallePlatos(variables);
        }

        #endregion

        #region BUSCAR DETALLE PLATOS
        public static DataTable BuscarDetallePlatos(string tipo_busqueda, string texto_busqueda)
        {
            return DPlatos.BuscarDetallePlatos(tipo_busqueda, texto_busqueda);
        }

        #endregion

        #region INACTIVAR PLATO

        public static string InactivarPlato(int id_plato)
        {
            DPlatos DPlatos = new DPlatos();
            return DPlatos.InactivarPlato(id_plato);
        }

        #endregion

        #region ACTUALIZAR ACOMPALANTE
        public static string ActualizarAcompanante(int id_ingrediente, string nombre)
        {
            DPlatos DPlatos = new DPlatos();
            return DPlatos.ActualizarAcompanante(id_ingrediente, nombre);
        }

        #endregion
    }
}
