using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CapaPresentacion.Formularios.FormsPedido;

// Paginacion de la lista de platos y bebidas de FrmPedido: 20 tarjetas por pagina, sin base de datos.
internal static class OrderCatalogPagingTests
{
    private const BindingFlags Private = BindingFlags.NonPublic | BindingFlags.Instance;

    [STAThread]
    private static int Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        try
        {
            TestPaging();
            Console.WriteLine("PASS: paging");
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private static List<ProductBinding> Products(int count)
    {
        return Enumerable.Range(1, count).Select(i => new ProductBinding
        {
            Id_producto = i,
            Nombre = "Producto " + i,
            Tipo_producto = "BEBIDA",
            Precio = 1000 * i,
            Observaciones = string.Empty,
        }).ToList();
    }

    private static void TestPaging()
    {
        // El formulario no se muestra: su Load pide la clave del mesero y consulta la base.
        using (FrmPedido form = new FrmPedido())
        {
            Panel list = (Panel)typeof(FrmPedido).GetField("panelResultados", Private).GetValue(form);
            Label label = (Label)typeof(FrmPedido).GetField("pagerLabel", Private).GetValue(form);
            Button previous = (Button)typeof(FrmPedido).GetField("previousPageButton", Private).GetValue(form);
            Button next = (Button)typeof(FrmPedido).GetField("nextPageButton", Private).GetValue(form);
            MethodInfo showCatalog = typeof(FrmPedido).GetMethod("ShowCatalog", Private);
            MethodInfo showPage = typeof(FrmPedido).GetMethod("ShowCatalogPage", Private);

            Stopwatch watch = Stopwatch.StartNew();
            showCatalog.Invoke(form, new object[] { Products(95) });
            Console.WriteLine("Primera pagina de 95 productos: " + watch.ElapsedMilliseconds + " ms");
            Check(list.Controls.Count == 20, "La primera pagina muestra 20 productos, no " + list.Controls.Count + ".");
            Check(label.Text.StartsWith("Página 1 de 5") && label.Text.Contains("95 productos"),
                "El rotulo debe decir pagina 1 de 5 y 95 productos: " + label.Text);
            Check(!previous.Enabled && next.Enabled, "En la primera pagina solo se puede avanzar.");

            Control firstCard = list.Controls[0];
            showPage.Invoke(form, new object[] { 4 });
            Check(list.Controls.Count == 15, "La ultima pagina muestra los 15 que sobran.");
            Check(previous.Enabled && !next.Enabled, "En la ultima pagina solo se puede retroceder.");
            Check(firstCard.IsDisposed, "Las tarjetas de la pagina anterior se liberan.");
            Check(((ProductoItem)list.Controls[0]).Product.Id_producto == 81, "La pagina 5 empieza en el producto 81.");

            showPage.Invoke(form, new object[] { 99 });
            Check(label.Text.StartsWith("Página 5 de 5"), "Una pagina fuera de rango se queda en la ultima.");

            showCatalog.Invoke(form, new object[] { Products(20) });
            Check(list.Controls.Count == 20 && !previous.Enabled && !next.Enabled,
                "Con 20 productos hay una sola pagina.");

            showCatalog.Invoke(form, new object[] { new List<ProductBinding>() });
            Check(list.Controls.Count == 0 && label.Text == "Sin productos", "Sin resultados la lista queda vacia.");
        }
    }
}
