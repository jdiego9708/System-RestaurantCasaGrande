using System;
using System.Data;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using CapaPresentacion.Formularios.FormsVentas;

internal static class SalesSortingTests
{
    private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

    private static object Field(object owner, string name) { return owner.GetType().GetField(name, Private).GetValue(owner); }
    private static void Set(object owner, string name, object value) { owner.GetType().GetField(name, Private).SetValue(owner, value); }
    private static object Call(object owner, string name, params object[] args)
    {
        try { return owner.GetType().GetMethod(name, Private).Invoke(owner, args); }
        catch (TargetInvocationException error) { throw error.InnerException; }
    }
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private static void Header(DataGridView grid, string field, bool filter = false)
    {
        DataGridViewColumn column = grid.Columns[field];
        int x = filter ? column.Width - 15 : 15;
        Call(grid, "OnColumnHeaderMouseClick", new DataGridViewCellMouseEventArgs(column.Index, -1, x, 10,
            new MouseEventArgs(MouseButtons.Left, 1, x, 10, 0)));
    }

    private static DataTable Records(string date, bool rappi)
    {
        DataTable table = new DataTable();
        table.Columns.Add(date, typeof(DateTime));
        table.Columns.Add(rappi ? "OrderTime" : "Hora_venta", rappi ? typeof(DateTime) : typeof(string));
        table.Columns.Add(rappi ? "CustomerName" : "Nombre_cliente", typeof(string));
        table.Columns.Add(rappi ? "Total" : "Total_final", typeof(int));
        if (rappi)
        {
            table.Columns.Add("CashAmount", typeof(int));
            table.Columns.Add("OtherPaymentAmount", typeof(int));
            table.Columns.Add("ExternalId", typeof(string));
            table.Columns.Add("Phone", typeof(string));
            table.Columns.Add("PaymentMethod", typeof(string));
        }
        for (int i = 0; i < 205; i++)
        {
            DataRow row = table.NewRow();
            row[0] = new DateTime(2026, 1, 1).AddDays(i);
            row[1] = rappi ? (object)new DateTime(2026, 1, 1).AddHours(i % 24) : (i % 24).ToString("00") + ":00";
            row[2] = i % 2 == 0 ? "Ada" : "Zoe";
            row[3] = i * 1000;
            if (rappi) { row[4] = i * 1000; row[5] = 0; }
            table.Rows.Add(row);
        }
        return table;
    }

    private static void TestView(string mode, string date)
    {
        using (FrmObservarVentas form = new FrmObservarVentas())
        using (Form host = new Form
        {
            ClientSize = new Size(1100, 500), ShowInTaskbar = false,
            StartPosition = FormStartPosition.Manual, Location = new Point(-30000, -30000),
        })
        {
            bool rappi = mode == "rappi";
            if (rappi) Set(form, "showingRappi", true);
            if (mode == "deliveries") Set(form, "showingDeliveries", true);
            if (mode == "income") Set(form, "showingIncome", true);
            if (mode == "expenses") Set(form, "showingExpenses", true);
            DataTable table = Records(date, rappi);
            if (mode == "income" || mode == "expenses")
            {
                string suffix = mode == "income" ? "ingreso" : "egreso";
                table.Columns[2].ColumnName = "Descripcion_" + suffix;
                table.Columns[3].ColumnName = "Valor_" + suffix;
            }
            string text = table.Columns[2].ColumnName;
            string amount = table.Columns[3].ColumnName;
            DataGridView grid = (DataGridView)Field(form, "dgvVentas");
            grid.BindingContext = new BindingContext();
            grid.Dock = DockStyle.Fill;
            host.Controls.Add(grid);
            host.Show();
            Call(form, "BindResults", table);
            ((Button)Field(form, "btnBuscar")).Enabled = true;

            // Reproducir la excepcion original y alternar el orden en columnas regeneradas.
            Header(grid, date);
            Check(Convert.ToDateTime(table.DefaultView[0][date]) == new DateTime(2026, 1, 1).AddDays(204), mode + ": fecha descendente");
            Check(grid.Columns[date].HeaderCell.SortGlyphDirection == SortOrder.Descending, "Flecha descendente");
            Call(form, "ShowPage", 1, false);
            Check(grid.Columns[date].HeaderCell.SortGlyphDirection == SortOrder.Descending, "Conservar flecha al paginar");
            DataTable page = (DataTable)grid.DataSource;
            Check(page.Rows.Count == 100 && Convert.ToDateTime(page.Rows[0][date]) == new DateTime(2026, 1, 1).AddDays(104), "Orden global antes de paginar");
            Header(grid, date);
            Check(Convert.ToDateTime(table.DefaultView[0][date]) == new DateTime(2026, 1, 1), "Alternar a fecha ascendente");
            Check((int)Field(form, "currentPage") == 0, "Ordenar vuelve a la primera pagina");
            Header(grid, amount);
            Check(Convert.ToInt32(table.DefaultView[0][amount]) == 204000, "Importes descendentes numericos");
            Header(grid, amount);
            Check(Convert.ToInt32(table.DefaultView[0][amount]) == 0, "Importes ascendentes numericos");
            Header(grid, text);
            Check(Convert.ToString(table.DefaultView[0][text]) == "Zoe", "Texto descendente desde el titulo");
            Header(grid, text);
            Check(Convert.ToString(table.DefaultView[0][text]) == "Ada", "Texto ascendente desde el titulo");

            // El icono de filtro mantiene su menu, sin ordenar accidentalmente.
            string previousSort = table.DefaultView.Sort;
            Header(grid, text, true);
            object filters = Field(form, "textFilters");
            ToolStripDropDown popup = (ToolStripDropDown)Field(filters, "popup");
            Check(popup != null && popup.Visible && table.DefaultView.Sort == previousSort, "El icono solo abre el filtro");
            Control panel = ((ToolStripControlHost)popup.Items[0]).Control;
            LinkLabel descending = FindLink(panel, "Z - A");
            Call(descending, "OnLinkClicked", new LinkLabelLinkClickedEventArgs(descending.Links[0]));
            Check(Convert.ToString(table.DefaultView[0][text]) == "Zoe", "Ordenar desde el menu");
            Call(form, "ShowPage", 1, false);
            Check(grid.Columns[text].HeaderCell.SortGlyphDirection == SortOrder.Descending, "Conservar orden del menu al paginar");
            Header(grid, text);
            Check(Convert.ToString(table.DefaultView[0][text]) == "Ada", "Alternar en titulo tras ordenar desde menu");

            table.DefaultView.RowFilter = "[" + text + "] = 'Ada'";
            Call(form, "ApplyResultFilters");
            Header(grid, amount);
            Check(table.DefaultView.Count == 103 && Convert.ToInt32(table.DefaultView[0][amount]) == 204000, "Ordenar conserva el filtro");
            Check(grid.Columns[amount].HeaderCell.SortGlyphDirection == SortOrder.Descending, "Flecha tras filtrar");
            Header(grid, "ResultRowNumber");
            Check(table.DefaultView.Sort.Contains(amount), "La numeracion no modifica el orden");
            table.DefaultView.RowFilter = "1 = 0";
            Call(form, "ApplyResultFilters");
            Header(grid, date);
            Check(((DataTable)grid.DataSource).Rows.Count == 0, "Ordenar una consulta vacia");
            Call(form, "BindResults", table.Clone());
            Check(grid.Columns[date].HeaderCell.SortGlyphDirection == SortOrder.None, "Una consulta nueva limpia la flecha");
            host.Close();
        }
        Console.WriteLine("PASS: " + mode);
    }

    private static LinkLabel FindLink(Control parent, string text)
    {
        foreach (Control child in parent.Controls)
        {
            if (child is LinkLabel && child.Text == text) return (LinkLabel)child;
            LinkLabel found = FindLink(child, text);
            if (found != null) return found;
        }
        return null;
    }

    [STAThread]
    private static int Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        try
        {
            TestView("sales", "Fecha_venta");
            TestView("deliveries", "Fecha_venta");
            TestView("rappi", "OrderDate");
            TestView("income", "Fecha_ingreso");
            TestView("expenses", "Fecha_egreso");
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}
