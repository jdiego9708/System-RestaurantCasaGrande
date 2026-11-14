using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Xml;
using CapaPresentacion;
using Microsoft.Reporting.WinForms;

// Imprime un ticket real (comanda, factura o precuenta) con datos de relleno en la impresora Bluetooth configurada.
// GASTA PAPEL: es una prueba manual, no forma parte de las pruebas automáticas.
class BluetoothReceiptTests
{
    static LocalReport Report(string resource,string folder,int count)
    {
        var report=new LocalReport();
        var xml=new XmlDocument();
        using(var stream=typeof(ControladorImpresion).Assembly.GetManifestResourceStream(resource))xml.Load(stream);
        string definition=Path.Combine(folder,resource.Substring(resource.LastIndexOf('.',resource.Length-6)+1));
        xml.Save(definition);report.ReportPath=definition;
        var ns=new XmlNamespaceManager(xml.NameTable);ns.AddNamespace("r",xml.DocumentElement.NamespaceURI);
        foreach(XmlElement dataset in xml.SelectNodes("//r:DataSet",ns))
        {
            string name=dataset.GetAttribute("Name");var table=new DataTable(name);
            foreach(XmlElement field in dataset.SelectNodes("r:Fields/r:Field",ns))table.Columns.Add(field.GetAttribute("Name"),typeof(string));
            int rows=name=="DetallePedido" ? count : 1;
            for(int i=0;i<rows;i++)
            {
                var row=table.NewRow();foreach(DataColumn column in table.Columns)row[column]="1";
                foreach(string field in new[]{"Fecha_pedido","Fecha_venta"})if(table.Columns.Contains(field))row[field]="11/11/2026";
                foreach(string field in new[]{"Hora_pedido","Hora_venta"})if(table.Columns.Contains(field))row[field]="09:40 PM";
                if(table.Columns.Contains("Nombre"))row["Nombre"]="PRODUCTO DE PRUEBA "+(i+1).ToString("D2");
                if(table.Columns.Contains("Tipo"))row["Tipo"]="PLATO";
                if(table.Columns.Contains("Observaciones"))row["Observaciones"]="sin salsa";
                table.Rows.Add(row);
            }
            report.DataSources.Add(new ReportDataSource(name,table));
        }
        foreach(XmlElement parameter in xml.SelectNodes("//r:ReportParameter",ns))
        {
            string name=parameter.GetAttribute("Name");
            report.SetParameters(new ReportParameter(name,name.IndexOf("Observaciones",StringComparison.OrdinalIgnoreCase)>=0 ? "PRUEBA BLUETOOTH" : name=="Titulo" ? "Casa Grande PRUEBA" : "100"));
        }
        return report;
    }

    // args: carpeta de trabajo, informe (p. ej. FacturaFinal.Precuenta), número de productos.
    [STAThread]static int Main(string[] args)
    {
        try
        {
            System.Threading.Thread.CurrentThread.CurrentCulture=CultureInfo.GetCultureInfo("es-CO");
            Directory.CreateDirectory(args[0]);
            var timer=System.Diagnostics.Stopwatch.StartNew();
            using(var report=Report("CapaPresentacion.ReportesFacturas."+args[1]+".rdlc",args[0],int.Parse(args[2])))
            using(var controller=new ControladorImpresion())
                controller.PrintBluetooth(report);
            Console.WriteLine("OK: "+args[1]+" con "+args[2]+" productos enviado en "+timer.ElapsedMilliseconds+" ms.");
            return 0;
        }
        catch(Exception ex){Console.Error.WriteLine(ex);return 1;}
    }
}
