using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CapaDatos;
using CapaNegocio;
using CapaPresentacion.Formularios.FormsProductos;
using CapaPresentacion.UI;

internal static class ProductsTests
{
    private static readonly string db = "CasaGrande_ProductsTest_" + Guid.NewGuid().ToString("N");
    private const string master = "Server=(localdb)\\MSSQLLocalDB;Database=master;Integrated Security=true";
    private static void Sql(string connection, string sql)
    { using (var cn = new SqlConnection(connection)) using (var cmd = new SqlCommand(sql, cn)) { cn.Open(); cmd.ExecuteNonQuery(); } }
    private static void Check(bool condition, string message) { if (!condition) throw new Exception(message); }
    [STAThread]
    private static int Main(string[] args)
    {
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        bool created = false;
        try
        {
            Sql(master, "CREATE DATABASE [" + db + "]"); created = true;
            Conexion.Cn = "Server=(localdb)\\MSSQLLocalDB;Database=" + db + ";Integrated Security=true";
            Sql(Conexion.Cn, @"CREATE TABLE Tipo_platos (Id_tipo_plato int PRIMARY KEY, Tipo_plato varchar(100));
                CREATE TABLE Tipo_bebidas (Id_tipo_bebida int PRIMARY KEY, Tipo_bebida varchar(100));
                INSERT INTO Tipo_platos VALUES (1,'Especiales'); INSERT INTO Tipo_bebidas VALUES (1,'Naturales');
                CREATE TABLE Platos (Id_plato int IDENTITY PRIMARY KEY, Nombre_plato varchar(200), Id_tipo_plato int,
                    Precio_plato decimal(19,2), Imagen_plato varchar(max), Descripcion_plato varchar(500), Estado varchar(50),
                    Plato_detallado varchar(50), Plato_carta varchar(50));
                CREATE TABLE Bebidas (Id_bebida int IDENTITY PRIMARY KEY, Nombre_bebida varchar(200), Descripcion_bebida varchar(200),
                    Precio_bebida decimal(19,2), Precio_trago decimal(19,2), Precio_trago_doble decimal(19,2), Precio_proveedor decimal(19,2),
                    Id_proveedor int, Imagen varchar(max) NOT NULL, Id_tipo_bebida int, Cantidad_unidades int, Cantidad_x_unidad int,
                    Cantidad_total int, Estado varchar(50));");
            NProductos.Guardar(null, false, "Plato O'Brien [100%]", 1, 25000.50m, "Inicial", true, true);
            NProductos.Guardar(null, true, "Limonada", 1, 6000m, "Natural", false, false);
            DataTable rows = NProductos.Listar();
            Check(rows.Rows.Count == 2, "Ambos tipos deben aparecer en el catálogo.");
            Check(Convert.ToInt32(rows.Rows[0]["Id"]) == Convert.ToInt32(rows.Rows[1]["Id"]), "El escenario debe cubrir IDs coincidentes.");
            Sql(Conexion.Cn, "UPDATE Bebidas SET Precio_trago=1234, Cantidad_total=17, Imagen='limonada.png'; UPDATE Platos SET Plato_detallado='ACTIVO'");
            NProductos.Guardar(1, true, "Limonada editada", 1, 7000.25m, "Editada", true, false);
            NProductos.Guardar(1, false, "Plato O'Brien [100%]", 1, 26000m, "Editado", true, false);
            NProductos.CambiarEstado(1, false, false);
            rows = NProductos.Listar();
            Check(Convert.ToString(rows.Select("Tipo='PLATO'")[0]["Estado"]) == "INACTIVO", "Debe inactivar el plato.");
            Check(Convert.ToString(rows.Select("Tipo='BEBIDA'")[0]["Estado"]) == "ACTIVO", "No debe afectar la bebida con el mismo ID.");
            NProductos.CambiarEstado(1, false, true);
            NProductos.CambiarEstado(1, true, false);
            Check(Convert.ToString(NProductos.Listar().Select("Tipo='BEBIDA'")[0]["Estado"]) == "INACTIVO", "Debe inactivar bebidas.");
            NProductos.CambiarEstado(1, true, true);
            Check(Convert.ToDecimal(NProductos.Listar().Select("Tipo='BEBIDA'")[0]["Precio"]) == 7000.25m, "Debe conservar los centavos del precio editado.");
            using (var cn = new SqlConnection(Conexion.Cn))
            using (var cmd = new SqlCommand("SELECT Precio_trago, Cantidad_total, Imagen FROM Bebidas", cn))
            {
                cn.Open(); using (var reader = cmd.ExecuteReader())
                { reader.Read(); Check(reader.GetDecimal(0) == 1234m && reader.GetInt32(1) == 17 && reader.GetString(2) == "limonada.png", "La edición debe preservar datos adicionales de las bebidas."); }
            }
            bool rejected = false;
            try { NProductos.Guardar(null, false, "Inválido", 1, -1, "", true, true); }
            catch (ArgumentException) { rejected = true; }
            Check(rejected, "Debe rechazar precios negativos.");
            Render(new FrmProductos(), Path.Combine(args[0], "productos.png"), delegate(Form form)
            {
                var grid = Field<DataGridView>(form, "grid");
                Check(grid.Rows.Count == 2, "La ventana debe mostrar ambos tipos.");
                Field<ModernTextBox>(form, "search").Text = "O'Brien [100%]";
                Check(grid.Rows.Count == 1, "La búsqueda debe tratar signos especiales literalmente.");
                Field<ModernTextBox>(form, "search").Text = "";
                Field<ModernComboBox>(form, "type").SelectedIndex = 2;
                Check(grid.Rows.Count == 1, "Debe filtrar bebidas.");
                Field<ModernComboBox>(form, "type").SelectedIndex = 0;
            });
            Render(new FrmProducto(), Path.Combine(args[0], "nuevo-producto.png"), delegate(Form form)
            {
                var combo = Field<ModernComboBox>(form, "category");
                Check(combo.Items.Count == 1, "Debe cargar categorías.");
                Check(Field<ModernButton>(form, "save").Enabled, "Guardar debe estar disponible.");
                Check(Field<ModernTextBox>(form, "description").Height >= 40, "Los campos deben conservar su altura.");
                var message = Field<Label>(form, "error");
                Check(message.Bottom <= message.Parent.ClientSize.Height, "El mensaje de validación debe ser visible sin desplazar el formulario.");
            });
            Console.WriteLine("PASS: catálogo unificado, IDs coincidentes, crear/editar, activar/inactivar, preservación de datos, validación, filtros y renderizado.");
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
        finally
        {
            SqlConnection.ClearAllPools();
            if (created) Sql(master, "ALTER DATABASE [" + db + "] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [" + db + "]");
        }
    }
    private static T Field<T>(object target, string name)
    { return (T)target.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(target); }
    private static void Render(Form form, string path, Action<Form> validate)
    {
        using (form)
        {
            form.ShowInTaskbar = false; form.StartPosition = FormStartPosition.Manual; form.Location = new Point(-20000, -20000);
            form.Show();
            DateTime timeout = DateTime.UtcNow.AddSeconds(15);
            do { Application.DoEvents(); Thread.Sleep(20); }
            while ((form is FrmProductos ? Field<bool>(form, "busy") : Field<bool>(form, "loading")) && DateTime.UtcNow < timeout);
            validate(form);
            using (var bitmap = new Bitmap(form.Width, form.Height)) { form.DrawToBitmap(bitmap, new Rectangle(Point.Empty, form.Size)); bitmap.Save(path); }
            form.Close();
        }
    }
}
