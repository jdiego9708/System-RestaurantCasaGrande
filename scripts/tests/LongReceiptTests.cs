using System;
using System.Collections;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml;
using CapaPresentacion;
using Microsoft.Reporting.WinForms;

class LongReceiptTests
{
    static string output;
    static readonly BindingFlags Private=BindingFlags.NonPublic|BindingFlags.Instance;
    static void Check(bool condition,string error){if(!condition)throw new Exception(error);}

    static LocalReport Report(string resource,int count)
    {
        var report=new LocalReport();
        var xml=new XmlDocument();
        using(var stream=typeof(ControladorImpresion).Assembly.GetManifestResourceStream(resource))xml.Load(stream);
        string definition=Path.Combine(output,resource.Substring(resource.LastIndexOf('.',resource.Length-6)+1));
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
                foreach(string field in new[]{"Fecha_pedido","Fecha_venta"})if(table.Columns.Contains(field))row[field]="06/11/2026";
                foreach(string field in new[]{"Hora_pedido","Hora_venta"})if(table.Columns.Contains(field))row[field]="09:40 PM";
                if(table.Columns.Contains("Nombre"))row["Nombre"]="PRODUCTO-"+(i+1).ToString("D4");
                if(table.Columns.Contains("Tipo"))row["Tipo"]="PLATO";
                if(table.Columns.Contains("Observaciones"))row["Observaciones"]="OBS-"+(i+1).ToString("D4")+" sin salsa, preparacion especial para verificar el texto completo";
                table.Rows.Add(row);
            }
            report.DataSources.Add(new ReportDataSource(name,table));
        }
        foreach(XmlElement parameter in xml.SelectNodes("//r:ReportParameter",ns))report.SetParameters(new ReportParameter(parameter.GetAttribute("Name"),parameter.GetAttribute("Name").IndexOf("Observaciones",StringComparison.OrdinalIgnoreCase)>=0 ? "FIN-PRUEBA-"+count : parameter.GetAttribute("Name")=="Titulo" ? "Casa Grande PRUEBA" : "100"));
        return report;
    }

    [STAThread]static int Main(string[] args)
    {
        try
        {
            System.Threading.Thread.CurrentThread.CurrentCulture=CultureInfo.GetCultureInfo("es-CO");
            Directory.CreateDirectory(args[0]);output=args[0];
            foreach(string name in new[]{"Comandas.ComandasPedido","FacturaFinal.FacturaFinal","FacturaFinal.Precuenta"})
            {
                int shortHeight=0;
                foreach(int count in new[]{1,50})
                using(var report=Report("CapaPresentacion.ReportesFacturas."+name+".rdlc",count))
                using(var controller=new ControladorImpresion())
                {
                    typeof(ControladorImpresion).GetMethod("Prepare",Private).Invoke(controller,new object[]{report,283,12897});
                    int height=(int)typeof(ControladorImpresion).GetMethod("CurrentHeight",Private).Invoke(controller,null);
                    var streams=(IList)typeof(ControladorImpresion).GetField("m_streams",Private).GetValue(controller);
                    Check(streams.Count==1,"Un ticket de 50 productos (maximo real) debe caber en el rollo largo: "+name);
                    if(count==1)shortHeight=height;
                    else Check(height>1169 && height>shortHeight,"El ticket largo sigue limitado a 297 mm: "+name+" height="+height);
                    string prefix=Path.Combine(args[0],name.Replace('.','-')+"-"+count);
                    string device=(string)typeof(ControladorImpresion).GetMethod("DeviceInfo",Private).Invoke(controller,new object[]{315d,12897d});
                    File.WriteAllBytes(prefix+".pdf",report.Render("PDF",device));
                    Console.WriteLine(name+" rows="+count+" EMF pages="+streams.Count+" paper="+Math.Round(height*.254,2)+" mm");
                    // Simula que el driver sigue limitando cada pagina a 297 mm. No llamar PrintDocument.Print.
                    int pages=0;bool more;
                    do
                    {
                        var page=new PageSettings {PaperSize=new PaperSize("POS-80 simulado",283,1169),Margins=new Margins(0,0,0,0)};
                        using(var bitmap=new Bitmap(350,1200))
                        using(var graphics=Graphics.FromImage(bitmap))
                        {
                            graphics.Clear(Color.White);
                            var e=new PrintPageEventArgs(graphics,new Rectangle(0,0,283,1169),new Rectangle(0,0,283,1169),page);
                            typeof(ControladorImpresion).GetMethod("PrintPage",Private).Invoke(controller,new object[]{null,e});
                            more=e.HasMorePages;
                            bitmap.Save(prefix+"-fragment-"+pages+".png",ImageFormat.Png);
                        }
                        Check(++pages<30,"No termina la continuacion del ticket");
                    }while(more);
                    int printedIndex=(int)typeof(ControladorImpresion).GetField("m_currentPageIndex",Private).GetValue(controller);
                    Check(printedIndex==streams.Count,"No se consumieron todas las paginas renderizadas");
                    if(count==50)Check(pages>1,"No detecto limite de pagina del controlador simulado");
                }
            }
            Console.WriteLine("OK: reportes reales, altura dinamica y continuacion sin enviar trabajos ni consultar SQL.");return 0;
        }
        catch(Exception ex){Console.Error.WriteLine(ex);return 1;}
    }
}
