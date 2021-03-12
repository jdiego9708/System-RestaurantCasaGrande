using System;
using System.ComponentModel;
using System.Data;
using System.Windows.Forms;

namespace CapaPresentacion
{
    public class CustomDataGridView : DataGridView
    {
        public CustomDataGridView()
        {
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle4 = new DataGridViewCellStyle();

            this.AllowUserToAddRows = false;
            this.AllowUserToDeleteRows = false;
            this.AllowUserToResizeColumns = false;
            this.AllowUserToResizeRows = false;

            this.DefaultCellStyle.NullValue = "No hay dato";

            this.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;

            this.RowTemplate.Height = 30;

            this.SelectionMode = DataGridViewSelectionMode.CellSelect;
            this.MultiSelect = true;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.Beige;
            this.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) | AnchorStyles.Left) | AnchorStyles.Right)));
            this.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            this.BackgroundColor = System.Drawing.Color.White;
            this.BorderStyle = BorderStyle.Fixed3D;
            this.CellBorderStyle = DataGridViewCellBorderStyle.None;
            this.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.Bisque;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 10F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.True;
            this.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.ScrollBar;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = DataGridViewTriState.False;
            this.DefaultCellStyle = dataGridViewCellStyle3;
            //this.Location = new System.Drawing.Point(12, 132);
            this.Name = "Datagrid";
            this.ReadOnly = true;
            this.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            this.RowHeadersVisible = false;
            dataGridViewCellStyle4.BackColor = System.Drawing.Color.Bisque;
            this.RowsDefaultCellStyle = dataGridViewCellStyle4;
            //this.Size = new System.Drawing.Size(857, 334);
            this.TabIndex = 7;
            this.BorderStyle = BorderStyle.Fixed3D;
        }

        public int PageSize
        {
            get
            {
                return _pageSize;
            }
            set
            {
                _pageSize = value;
            }
        }

        public int _pageSize = 10;

        BindingSource bs = new BindingSource();
        BindingList<DataTable> tables = new BindingList<DataTable>();
        public void SetPagedDataSource(DataTable dataTable, BindingNavigator bnav)
        {
            DataTable dt = null;
            this.clearDataSource();
            int counter = 1;
            foreach (DataRow dr in dataTable.Rows)
            {
                if (counter == 1)
                {
                    dt = dataTable.Clone();
                    tables.Add(dt);
                }
                dt.Rows.Add(dr.ItemArray);
                if (PageSize < ++counter)
                {
                    counter = 1;
                }
            }
            bnav.BindingSource = bs;
            bs.DataSource = tables;
            bs.PositionChanged += bs_PositionChanged;
            bs_PositionChanged(bs, EventArgs.Empty);
        }
        void bs_PositionChanged(object sender, EventArgs e)
        {
            this.DataSource = tables[bs.Position];
        }

        public void clearDataSource()
        {
            this.tables = new BindingList<DataTable>();
            this.DataSource = null;
            this.bs = new BindingSource();
        }
    }
}
