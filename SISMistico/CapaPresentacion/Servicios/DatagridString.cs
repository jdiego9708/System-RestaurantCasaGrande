using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using System.Windows.Forms;

namespace CapaPresentacion
{
    public class DatagridString
    {
        public static CustomDataGridView ChangeHeaderTextAndVisibleCustomDataGrid(CustomDataGridView dgv,
    string[] header_text_columns, bool[] columns_visible)
        {
            CustomDataGridView CopiaGrid = dgv;
            try
            {
                if (dgv.DataSource != null)
                {
                    if (header_text_columns != null)
                    {
                        int numero_texto_columnas = header_text_columns.Length;
                        int numero_columnas = dgv.Columns.Count;
                        if (numero_columnas > 0)
                        {
                            for (int i = 0; i <= numero_texto_columnas - 1; i++)
                            {
                                dgv.Columns[i].HeaderText = header_text_columns[i];
                            }
                        }
                    }

                    if (columns_visible != null)
                    {
                        int numero_visible = columns_visible.Length;
                        int numero_columnas = dgv.Columns.Count;
                        if (numero_columnas > 0)
                        {
                            for (int i = 0; i <= numero_visible - 1; i++)
                            {
                                dgv.Columns[i].Visible = columns_visible[i];
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                dgv = CopiaGrid;
            }
            return dgv;
        }

        public static DataGridView ChangeHeaderTextAndVisible(DataGridView dgv,
            string[] header_text_columns, bool[] columns_visible)
        {
            DataGridView CopiaDataGrid = dgv;
            try
            {
                if (dgv.DataSource != null)
                {
                    if (header_text_columns != null)
                    {
                        int numero_texto_columnas = header_text_columns.Length;
                        int numero_columnas = dgv.ColumnCount;
                        if (numero_columnas > 0)
                        {
                            for (int i = 0; i <= numero_texto_columnas - 1; i++)
                            {
                                dgv.Columns[i].HeaderText = header_text_columns[i];
                            }
                        }
                    }

                    if (columns_visible != null)
                    {
                        int numero_visible = columns_visible.Length;
                        int numero_columnas = dgv.ColumnCount;
                        if (numero_columnas > 0)
                        {
                            for (int i = 0; i <= numero_visible - 1; i++)
                            {
                                dgv.Columns[i].Visible = columns_visible[i];
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                dgv = CopiaDataGrid;
            }
            return dgv;
        }
        public static DataGridView ChangeColumnsHeaderText(DataGridView dgv,
            string[] header_text_columns)
        {
            DataGridView CopiaDataGrid = dgv;
            try
            {
                if (dgv.DataSource != null)
                {
                    int numero_texto_columnas = header_text_columns.Length;
                    int numero_columnas = dgv.ColumnCount;
                    if (numero_columnas > 0)
                    {
                        for (int i = 0; i <= numero_columnas - 1; i++)
                        {
                            dgv.Columns[i].HeaderText = header_text_columns[i];
                        }
                    }
                }
            }
            catch (Exception )
            {
                dgv = CopiaDataGrid;
            }
            return dgv;
        }

        public static DataGridView ChangeColumnsVisible(DataGridView dgv,
            bool[] visible_columns)
        {
            DataGridView CopiaDataGrid = dgv;
            try
            {
                if (dgv.DataSource != null)
                {
                    int numero_visible = visible_columns.Length;
                    int numero_columnas = dgv.ColumnCount;
                    if (numero_columnas > 0)
                    {
                        for (int i = 0; i <= numero_columnas - 1; i++)
                        {
                            dgv.Columns[i].Visible = visible_columns[i];
                        }
                    }
                }
            }
            catch (Exception )
            {
                dgv = CopiaDataGrid;
            }
            return dgv;
        }

        public static List<string> ReturnValuesOfCells(object obj, int fila, out string rpta)
        {
            List<string> listaDatos = new List<string>();
            rpta = "OK";
            try
            {
                if (obj is DataGridView)
                {
                    DataGridView dgv = (DataGridView)obj;
                    int numero_columnas = dgv.Columns.Count;
                    if (numero_columnas > 0)
                    {
                        for (int i = 0; i <= numero_columnas - 1; i++)
                        {
                            string texto = Convert.ToString(dgv.Rows[fila].Cells[i].Value);
                            listaDatos.Add(texto);
                        }
                    }
                }
                else if (obj is DataTable)
                {
                    DataTable dt = (DataTable)obj;
                    int numero_columnas = dt.Columns.Count;
                    if (numero_columnas > 0)
                    {
                        for (int i = 0; i < numero_columnas; i++)
                        {
                            string texto = Convert.ToString(dt.Rows[0][i]);
                            listaDatos.Add(texto);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                listaDatos = null;
                rpta = ex.Message;
            }
            return listaDatos;
        }
    }
}
