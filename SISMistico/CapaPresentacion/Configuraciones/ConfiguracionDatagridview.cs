using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Forms;

namespace CapaPresentacion
{
    public class ConfiguracionDatagridview
    {
        public static DataGridView ConfigurationGrid(DataGridView Grid)
        {
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle4 = new DataGridViewCellStyle();

            Grid.AllowUserToAddRows = false;
            Grid.AllowUserToDeleteRows = false;
            Grid.AllowUserToResizeColumns = true;
            Grid.AllowUserToResizeRows = false;

            Grid.DefaultCellStyle.NullValue = "No hay dato";

            Grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing;
            Grid.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.EnableResizing;

            Grid.RowTemplate.Height = 30;

            Grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            Grid.MultiSelect = true;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.PowderBlue;
            Grid.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            Grid.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) | AnchorStyles.Left) | AnchorStyles.Right)));
            Grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            Grid.BackgroundColor = System.Drawing.Color.White;
            Grid.BorderStyle = BorderStyle.Fixed3D;
            Grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            Grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.Bisque;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 10F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.True;
            Grid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = 
                new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, 
                System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.SkyBlue;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle3.WrapMode = DataGridViewTriState.False;
            Grid.DefaultCellStyle = dataGridViewCellStyle3;
            //Grid.Location = new System.Drawing.Point(12, 132);
            Grid.Name = "Datagrid";
            Grid.ReadOnly = true;
            Grid.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            Grid.RowHeadersVisible = false;
            dataGridViewCellStyle4.BackColor = System.Drawing.Color.GhostWhite;
            Grid.RowsDefaultCellStyle = dataGridViewCellStyle4;
            //Grid.Size = new System.Drawing.Size(857, 334);
            Grid.TabIndex = 7;

            return Grid;
        }

        public static DataGridView ConfigurationGrid(DataGridView Grid, bool anchor)
        {
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle4 = new DataGridViewCellStyle();

            Grid.AllowUserToAddRows = false;
            Grid.AllowUserToDeleteRows = false;
            Grid.AllowUserToResizeColumns = true;
            Grid.AllowUserToResizeRows = false;

            Grid.DefaultCellStyle.NullValue = "No hay dato";

            Grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing;
            Grid.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.EnableResizing;

            Grid.RowTemplate.Height = 30;

            Grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            Grid.MultiSelect = true;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.PowderBlue;
            Grid.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            if (!anchor)
            {
                Grid.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) | AnchorStyles.Left) | AnchorStyles.Right)));
            }
            Grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            Grid.BackgroundColor = System.Drawing.Color.White;
            Grid.BorderStyle = BorderStyle.Fixed3D;
            Grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            Grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.Bisque;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 10F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.True;
            Grid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font =
                new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular,
                System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.SkyBlue;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle3.WrapMode = DataGridViewTriState.False;
            Grid.DefaultCellStyle = dataGridViewCellStyle3;
            //Grid.Location = new System.Drawing.Point(12, 132);
            Grid.Name = "Datagrid";
            Grid.ReadOnly = true;
            Grid.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            Grid.RowHeadersVisible = false;
            dataGridViewCellStyle4.BackColor = System.Drawing.Color.GhostWhite;
            Grid.RowsDefaultCellStyle = dataGridViewCellStyle4;
            //Grid.Size = new System.Drawing.Size(857, 334);
            Grid.TabIndex = 7;

            return Grid;
        }
    }
}
