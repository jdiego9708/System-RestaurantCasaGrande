using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CapaEntidades.Models;
using CapaPresentacion.Formularios.FormsDomicilios;
using CapaPresentacion.UI;

// Pruebas de la pagina de Domicilios: reparto por tiempo de preparacion, cuenta regresiva y dibujo de las
// tarjetas. Con una carpeta como segundo argumento guarda ademas las capturas en PNG para revisarlas a ojo.
internal static class DeliveriesRenderingTests
{
    private static string outputFolder;

    [STAThread]
    private static int Main(string[] args)
    {
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        string testCase = args.Length > 0 ? args[0] : "timing";
        outputFolder = args.Length > 1 ? args[1] : null;
        try
        {
            if (testCase == "timing") TestTiming();
            else if (testCase == "cards") TestCards();
            else if (testCase == "window") TestWindow();
            else throw new ArgumentException("Caso desconocido: " + testCase);
            Console.WriteLine("PASS: " + testCase);
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private static Pedidos Order(int id, string state, DateTime takenAt)
    {
        return new Pedidos
        {
            Id_pedido = id,
            Estado_pedido = state,
            Fecha_pedido = takenAt.Date,
            // Mismo formato que devuelve la columna time(2).
            Hora_pedido = takenAt.TimeOfDay.ToString(@"hh\:mm\:ss\.ff"),
            Tipo_pedido = "DOMICILIO",
        };
    }

    private static DeliveryDetails Details(bool longOrder)
    {
        DeliveryDetails details = new DeliveryDetails
        {
            Loaded = true,
            CustomerName = longOrder ? "María Fernanda Restrepo Echeverri" : "Carlos Gómez",
            Phone = "300 555 0142",
            Address = longOrder
                ? "Carrera 43A # 18 Sur - 135, apto 1204, Torre 2 · Ref: portería principal, frente al parque"
                : "Calle 10 # 5-21",
            Notes = longOrder ? "Sin cebolla en la bandeja. Llamar al llegar, el timbre no funciona." : null,
        };
        details.Items.Add(new DeliveryItem { Quantity = "2", Name = "Bandeja paisa" });
        details.Items.Add(new DeliveryItem { Quantity = "1", Name = "Limonada de coco", Notes = longOrder ? "Sin azúcar" : null });
        if (longOrder)
            details.Items.Add(new DeliveryItem { Quantity = "12", Name = "Empanadas de carne con ají de la casa y limón" });
        return details;
    }

    private static void TestTiming()
    {
        DateTime now = DateTime.Today.AddHours(20);
        TimeSpan total = DeliveryTiming.PreparationTime;
        Check(total == TimeSpan.FromMinutes(45), "Sin configuracion, la preparacion dura 45 minutos.");

        Pedidos recent = Order(1, "PENDIENTE", now.AddMinutes(-10));
        Check(DeliveryTiming.StageOf(recent, now) == DeliveryStage.Preparing, "A los 10 minutos sigue en preparacion.");
        Check(Math.Abs((DeliveryTiming.Remaining(recent, now) - TimeSpan.FromMinutes(35)).TotalSeconds) < 1,
            "A los 10 minutos quedan 35.");

        Pedidos onTheEdge = Order(2, "PENDIENTE", now.AddMinutes(-45));
        Check(DeliveryTiming.StageOf(onTheEdge, now) == DeliveryStage.AwaitingBilling,
            "Al cumplir los 45 minutos pasa a terminados sin facturar.");
        Check(DeliveryTiming.StageOf(Order(3, "PENDIENTE", now.AddMinutes(-44).AddSeconds(-59)), now)
            == DeliveryStage.Preparing, "Un segundo antes de los 45 minutos aun esta en preparacion.");

        Check(DeliveryTiming.StageOf(Order(4, "DISPONIBLE", now.AddMinutes(-5)), now) == DeliveryStage.Billed,
            "Un pedido facturado esta terminado aunque le quede tiempo.");
        Check(DeliveryTiming.Remaining(Order(5, "PENDIENTE", now.AddMinutes(30)), now) == total,
            "Una hora de pedido en el futuro no da mas tiempo del previsto.");

        Pedidos unreadable = Order(6, "PENDIENTE", now);
        unreadable.Hora_pedido = "no es una hora";
        Check(DeliveryTiming.StageOf(unreadable, now) == DeliveryStage.Preparing,
            "Sin hora legible el pedido se queda en preparacion.");

        Check(CountdownRing.ColorFor(1).ToArgb() == Theme.Success.ToArgb(), "Con todo el tiempo el anillo es verde.");
        Check(CountdownRing.ColorFor(0.5).ToArgb() == Theme.Warning.ToArgb(), "A la mitad el anillo es ambar.");
        Check(CountdownRing.ColorFor(0).ToArgb() == Theme.Danger.ToArgb(), "Agotado el anillo es rojo.");
        Color between = CountdownRing.ColorFor(0.75);
        Check(between.ToArgb() != Theme.Success.ToArgb() && between.ToArgb() != Theme.Warning.ToArgb(),
            "Entre medias el color se mezcla en vez de saltar.");
    }

    private static void TestCards()
    {
        DateTime now = DateTime.Today.AddHours(20);
        Pedidos order = Order(4821, "PENDIENTE", now.AddMinutes(-10));

        using (DeliveryCard card = new DeliveryCard { Width = 420 })
        using (DeliveryCard narrow = new DeliveryCard { Width = 280 })
        {
            card.Bind(order, Details(false), DeliveryStage.Preparing, now);
            int shortHeight = card.Height;
            card.Bind(order, Details(true), DeliveryStage.Preparing, now);
            Check(card.Height > shortHeight, "Mas contenido debe dar una tarjeta mas alta.");

            narrow.Bind(order, Details(true), DeliveryStage.Preparing, now);
            Check(narrow.Height > card.Height, "Mas angosta, el texto parte en mas lineas y la tarjeta crece.");
            foreach (DeliveryCard each in new[] { card, narrow })
                foreach (Control child in each.Controls)
                    Check(!child.Visible || (child.Left >= 0 && child.Right <= each.Width && child.Bottom <= each.Height),
                        "Ningun boton puede salirse de la tarjeta (ancho " + each.Width + ").");

            Check(!card.Tick(now.AddMinutes(20)), "Con tiempo por delante la tarjeta no se da por agotada.");
            Check(card.Tick(now.AddMinutes(35)), "Al cumplir los 45 minutos la tarjeta avisa que se agoto.");

            using (Bitmap bitmap = new Bitmap(card.Width, card.Height))
                card.DrawToBitmap(bitmap, new Rectangle(Point.Empty, card.Size));
        }

        // Tira con los tres aspectos y varios momentos de la cuenta, para revisarla a ojo.
        using (Panel strip = new Panel { BackColor = Theme.Surface, Padding = new Padding(12) })
        {
            int x = 12, height = 0;
            foreach (int elapsed in new[] { 2, 18, 30, 41, 44, 50, 0 })
            {
                bool billed = elapsed == 0;
                DateTime takenAt = now.AddMinutes(-elapsed).AddSeconds(-17);
                Pedidos each = Order(4800 + elapsed, billed ? "DISPONIBLE" : "PENDIENTE", takenAt);
                DeliveryCard card = new DeliveryCard { Width = 400, Location = new Point(x, 12) };
                card.Bind(each, Details(elapsed % 4 == 2), DeliveryTiming.StageOf(each, now), now);
                strip.Controls.Add(card);
                x += card.Width + 12;
                height = Math.Max(height, card.Height);
            }
            strip.Size = new Size(x, height + 24);
            Save(strip, "delivery-cards.png");
        }
    }

    private static void TestWindow()
    {
        // La pagina real, con la base local; se le cambian las tarjetas por unas de ejemplo.
        FrmDeliveries form = FrmDeliveries.GetInstance();
        using (form)
        using (Timer timer = new Timer())
        {
            form.ShowInTaskbar = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.StartPosition = FormStartPosition.Manual;
            form.Location = new Point(-30000, -30000);
            form.Size = new Size(1366, 720);
            int frames = 0;
            timer.Interval = 400;
            timer.Tick += delegate
            {
                if (frames == 0) FillWithSamples(form);
                using (Bitmap bitmap = new Bitmap(form.Width, form.Height))
                    form.DrawToBitmap(bitmap, new Rectangle(Point.Empty, form.Size));
                if (++frames == 3)
                {
                    timer.Stop();
                    Save(form, "deliveries-window.png");
                    form.Close();
                }
            };
            form.Shown += delegate { timer.Start(); };
            Application.Run(form);
            Check(frames == 3, "La ventana debe abrirse y dibujar tres frames sin excepciones.");
        }
    }

    private static void FillWithSamples(FrmDeliveries form)
    {
        // Sin los temporizadores la pagina no recarga desde la base y deja quietas las tarjetas de ejemplo.
        foreach (string name in new[] { "refreshTimer", "clockTimer" })
            ((Timer)Field(form, name)).Stop();

        DateTime now = DateTime.Now;
        FlowLayoutPanel preparing = (FlowLayoutPanel)Field(form, "listPreparing");
        FlowLayoutPanel finished = (FlowLayoutPanel)Field(form, "listFinished");
        AddSamples(preparing, now, "PENDIENTE", 41, 24, 6);
        AddSamples(finished, now, "PENDIENTE", 52);
        AddSamples(finished, now, "DISPONIBLE", 70);
        ((Badge)Field(form, "preparingCount")).Text = "3";
        ((Badge)Field(form, "finishedCount")).Text = "2";
        ((KpiCard)Field(form, "kpiPreparing")).Value = "3";
        ((KpiCard)Field(form, "kpiFinished")).Value = "2";
    }

    private static void AddSamples(FlowLayoutPanel list, DateTime now, string state, params int[] elapsedMinutes)
    {
        foreach (Control old in list.Controls.Cast<Control>().Where(c => !(c is DeliveryCard)).ToList())
            list.Controls.Remove(old);
        foreach (int elapsed in elapsedMinutes)
        {
            Pedidos order = Order(4800 + elapsed, state, now.AddMinutes(-elapsed).AddSeconds(-20));
            DeliveryCard card = new DeliveryCard
            {
                Width = list.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - 4,
            };
            card.Bind(order, Details(elapsed % 2 == 0), DeliveryTiming.StageOf(order, now), now);
            list.Controls.Add(card);
        }
    }

    private static object Field(object target, string name)
    {
        FieldInfo field = target.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
        Check(field != null, "Falta el campo " + name + " en " + target.GetType().Name + ".");
        return field.GetValue(target);
    }

    private static void Save(Control control, string fileName)
    {
        using (Bitmap bitmap = new Bitmap(control.Width, control.Height))
        {
            control.DrawToBitmap(bitmap, new Rectangle(Point.Empty, control.Size));
            if (outputFolder == null) return;
            Directory.CreateDirectory(outputFolder);
            bitmap.Save(Path.Combine(outputFolder, fileName), ImageFormat.Png);
        }
    }
}
