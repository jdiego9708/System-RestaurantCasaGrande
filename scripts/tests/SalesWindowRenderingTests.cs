using System;
using System.Collections;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using CapaPresentacion.UI;
using CapaPresentacion.Formularios.FormsVentas;
using FontAwesome.Sharp;

internal static class SalesWindowRenderingTests
{
    [STAThread]
    private static int Main(string[] args)
    {
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        try
        {
            if (args.Length > 0 && args[0] == "guard") TestReentrantPaint();
            else TestWindow();
            Console.WriteLine("PASS: " + (args.Length > 0 ? args[0] : "window"));
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private static void TestReentrantPaint()
    {
        Type renderer = typeof(ModernButton).Assembly.GetType("CapaPresentacion.UI.IconRenderer", true);
        FieldInfo creating = renderer.GetField("creatingBitmap", BindingFlags.NonPublic | BindingFlags.Static);
        FieldInfo waiting = renderer.GetField("waiting", BindingFlags.NonPublic | BindingFlags.Static);
        FieldInfo cache = renderer.GetField("cache", BindingFlags.NonPublic | BindingFlags.Static);
        MethodInfo draw = renderer.GetMethod("Draw");
        using (var control = new Control())
        using (var bitmap = new Bitmap(48, 48))
        using (var graphics = Graphics.FromImage(bitmap))
        {
            control.CreateControl();
            int invalidations = 0;
            control.Invalidated += delegate { invalidations++; };
            object[] parameters = { control, graphics, IconChar.Search, Color.Blue, new Rectangle(0, 0, 18, 18) };
            // Simula un segundo WM_PAINT mientras la fuente esta dentro de su ValueFactory.
            creating.SetValue(null, true);
            try { draw.Invoke(null, parameters); }
            finally { creating.SetValue(null, false); }
            Check(((IDictionary)cache.GetValue(null)).Count == 0, "El Paint reentrante no debe cargar fuentes.");
            Check((int)waiting.GetValue(null).GetType().GetProperty("Count").GetValue(waiting.GetValue(null), null) == 1,
                "El control debe esperar un repintado.");
            draw.Invoke(null, parameters);
            Check(invalidations > 0, "La carga completa debe solicitar el repintado pendiente.");
            Check((int)waiting.GetValue(null).GetType().GetProperty("Count").GetValue(waiting.GetValue(null), null) == 0,
                "La lista de repintados debe quedar vacia.");
            int count = ((IDictionary)cache.GetValue(null)).Count;
            draw.Invoke(null, parameters);
            Check(((IDictionary)cache.GetValue(null)).Count == count, "El icono debe reutilizar la cache.");
        }
    }

    private static void TestWindow()
    {
        // Proceso nuevo: no se precargan los iconos antes de abrir el formulario.
        using (var form = new FrmObservarVentas())
        using (var timer = new Timer())
        {
            form.ShowInTaskbar = false;
            form.StartPosition = FormStartPosition.Manual;
            form.Location = new Point(-30000, -30000);
            int frames = 0;
            timer.Interval = 150;
            timer.Tick += delegate
            {
                using (var bitmap = new Bitmap(form.Width, form.Height)) form.DrawToBitmap(bitmap, form.ClientRectangle);
                if (++frames == 3) { timer.Stop(); form.Close(); }
            };
            form.Shown += delegate { timer.Start(); };
            Application.Run(form);
            Check(frames == 3, "La ventana debe abrirse y dibujar tres frames sin excepciones.");
        }
        using (var search = new ModernTextBox { Icon = IconChar.Search, Size = new Size(280, 42) })
        using (var card = new KpiCard { Icon = IconChar.CircleCheck })
        {
            using (var bitmap = new Bitmap(search.Width, search.Height)) search.DrawToBitmap(bitmap, search.ClientRectangle);
            using (var bitmap = new Bitmap(card.Width, card.Height)) card.DrawToBitmap(bitmap, card.ClientRectangle);
        }
    }
}
