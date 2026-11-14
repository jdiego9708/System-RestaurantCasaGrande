using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Reporting.WinForms;

namespace CapaPresentacion
{
    public class ControladorImpresion : IDisposable
    {
        private int m_currentPageIndex;
        private IList<Stream> m_streams;
        private readonly List<int> inkHeights = new List<int>();
        private int sourceOffset;
        private int targetWidth;
        private int maxPaperHeight;
        private bool continuous;
        private const int EndFeed = 12; // 3 mm al terminar, en centesimas de pulgada.
        // Por debajo de este ancho imprimible (~66 mm) el rollo es estrecho (58 mm) y el diseño se reajusta en vez de reducirse.
        private const int NarrowRollWidth = 260;
        // Ancho al que se maqueta cuando el diseño se adaptó al papel; 0 = el configurado en AnchoHoja.
        private double renderWidth;
        // Cuando el informe es una copia adaptada ya no se reconoce por su nombre: se recuerda si es ticket continuo.
        private bool? receiptOverride;

        private static double Dimension(string key, double fallback)
        {
            string value=ConfigurationManager.AppSettings[key];
            if(string.IsNullOrWhiteSpace(value))return fallback;
            if(!double.TryParse(value.Replace(',', '.'),NumberStyles.AllowDecimalPoint,CultureInfo.InvariantCulture,out double number) || number<0)
                throw new InvalidOperationException("Medida de impresion invalida: "+key);
            string unit=ConfigurationManager.AppSettings["MedidaPredeterminada"] ?? "cm";
            switch(unit.ToLowerInvariant())
            {
                case "cm": return number/2.54*100;
                case "mm": return number/25.4*100;
                case "in": return number*100;
                default: throw new InvalidOperationException("Unidad de impresion no compatible: "+unit);
            }
        }

        private static bool IsReceipt(LocalReport report)
        {
            string name=string.IsNullOrEmpty(report.ReportEmbeddedResource) ? report.ReportPath ?? "" : report.ReportEmbeddedResource;
            return name.EndsWith("ComandasPedido.rdlc",StringComparison.OrdinalIgnoreCase)
                || name.EndsWith("FacturaFinal.rdlc",StringComparison.OrdinalIgnoreCase)
                || name.EndsWith("Precuenta.rdlc",StringComparison.OrdinalIgnoreCase)
                || name.EndsWith("DeliveryInvoice.rdlc",StringComparison.OrdinalIgnoreCase)
                || name.EndsWith("DeliveryPreBill.rdlc",StringComparison.OrdinalIgnoreCase)
                || name.EndsWith("RappiTicket.rdlc",StringComparison.OrdinalIgnoreCase);
        }

        private Stream CreateStream(string name,string extension,Encoding encoding,string mimeType,bool willSeek)
        {
            // Sin nombres EMF compartidos: las comandas simultaneas no pisan archivos de otras impresiones.
            var stream=new MemoryStream();
            m_streams.Add(stream);
            return stream;
        }

        private string DeviceInfo(double width,double height)
        {
            // 203 ppp = resolucion de la POS-80: el ticket se maqueta con mas detalle que a 96 y se reduce al imprimir.
            var xml=new StringBuilder("<DeviceInfo><OutputFormat>EMF</OutputFormat><DpiX>203</DpiX><DpiY>203</DpiY>");
            xml.Append("<PageWidth>"+(width/100).ToString("0.######",CultureInfo.InvariantCulture)+"in</PageWidth>");
            xml.Append("<PageHeight>"+(height/100).ToString("0.######",CultureInfo.InvariantCulture)+"in</PageHeight>");
            foreach(var margin in new[] {new[]{"MarginTop","MargenArriba"},new[]{"MarginBottom","MargenAbajo"},new[]{"MarginLeft","MargenIzquierda"},new[]{"MarginRight","MargenDerecha"}})
                xml.Append("<"+margin[0]+">"+(Dimension(margin[1],0)/100).ToString("0.######",CultureInfo.InvariantCulture)+"in</"+margin[0]+">");
            return xml.Append("</DeviceInfo>").ToString();
        }

        // Tambien se usa por las pruebas con las medidas POS-80, sin enviar trabajos a Windows.
        private void Prepare(LocalReport report,int printableWidth,int maximumHeight)
        {
            Dispose();
            double width=renderWidth>0 ? renderWidth : Dimension("AnchoHoja",315),height=Dimension("AltoHoja",984);
            if(width<=0 || height<=0)throw new InvalidOperationException("El papel debe tener ancho y alto mayores que cero.");
            continuous=(receiptOverride ?? IsReceipt(report)) && !string.Equals(ConfigurationManager.AppSettings["TicketsAlturaAutomatica"],"false",StringComparison.OrdinalIgnoreCase);
            targetWidth=Math.Max(1,Math.Min((int)Math.Round(width),printableWidth));
            maxPaperHeight=Math.Max(1,maximumHeight);
            // Rasterizar para medir una pagina de 129 pulgadas tardaba 8-12 s antes de imprimir. Casi todos los tickets
            // caben en 40 pulgadas: se mide ahi y solo se repite con la pagina completa si el contenido llega abajo.
            double renderHeight=continuous ? Math.Min(maximumHeight,FirstPassHeight) : height;
            try
            {
                while(true)
                {
                    m_streams=new List<Stream>();
                    inkHeights.Clear();
                    Warning[] warnings;
                    report.Render("Image",DeviceInfo(width,renderHeight),CreateStream,out warnings);
                    bool reachesBottom=m_streams.Count>1;
                    foreach(var stream in m_streams)
                    {
                        stream.Position=0;
                        using(var image=new Metafile(stream))
                        {
                            int ink=continuous ? InkBottom(image) : image.Height;
                            inkHeights.Add(ink);
                            if(continuous && ink>=image.Height-image.Height/50)reachesBottom=true;
                        }
                        stream.Position=0;
                    }
                    if(!continuous || !reachesBottom || renderHeight>=maximumHeight)break;
                    foreach(var stream in m_streams)stream.Dispose();
                    renderHeight=maximumHeight;
                }
                // El indice siempre se reinicia tambien cuando se repite una copia.
                m_currentPageIndex=0;
                sourceOffset=0;
            }
            catch { Dispose(); throw; }
        }

        // Primera pasada de un ticket: 40 pulgadas (~1 m), muy por encima de los 50 productos maximos reales (~42 cm).
        private const double FirstPassHeight=4000;

        // Mide donde acaba la tinta sobre una copia a media resolucion: 4 veces menos pixeles que 1:1 y la precision
        // (+-2 px) sobra, porque despues se suman EndFeed y el redondeo del alto.
        private static int InkBottom(Metafile image)
        {
            const int step=2;
            int width=Math.Max(1,image.Width/step),height=Math.Max(1,image.Height/step);
            using(var bitmap=new Bitmap(width,height,PixelFormat.Format24bppRgb))
            {
                using(var graphics=Graphics.FromImage(bitmap))
                {
                    graphics.Clear(Color.White);
                    graphics.DrawImage(image,new Rectangle(0,0,width,height));
                }
                var data=bitmap.LockBits(new Rectangle(0,0,width,height),ImageLockMode.ReadOnly,PixelFormat.Format24bppRgb);
                try
                {
                    var bytes=new byte[Math.Abs(data.Stride)*height];
                    Marshal.Copy(data.Scan0,bytes,0,bytes.Length);
                    for(int y=height-1;y>=0;y--)
                    {
                        int row=data.Stride>=0 ? y*data.Stride : (height-1-y)*-data.Stride;
                        for(int x=0;x<width*3;x++)if(bytes[row+x]<250)return Math.Min(image.Height,(y+1)*step);
                    }
                    return 1;
                }
                finally {bitmap.UnlockBits(data);}
            }
        }

        private Metafile CurrentImage()
        {
            var stream=m_streams[m_currentPageIndex];
            stream.Position=0;
            return new Metafile(stream);
        }

        private int CurrentHeight()
        {
            using(var image=CurrentImage())
            {
                int needed=Math.Max(40,(int)Math.Ceiling((inkHeights[m_currentPageIndex]-sourceOffset)*(double)targetWidth/image.Width)+EndFeed);
                return continuous ? Math.Min(maxPaperHeight,needed) : needed;
            }
        }

        private void QueryPageSettings(object sender,QueryPageSettingsEventArgs e)
        {
            e.PageSettings.Landscape=false;
            e.PageSettings.Margins=new Margins(0,0,0,0); // Los margenes del reporte ya van dentro del EMF.
            e.PageSettings.PaperSize=new PaperSize("Casa Grande",targetWidth,CurrentHeight());
        }

        private void PrintPage(object sender,PrintPageEventArgs e)
        {
            using(var image=CurrentImage())
            {
                RectangleF available=RectangleF.Intersect(e.PageSettings.PrintableArea,e.PageBounds);
                if(available.Width<=0 || available.Height<=0)throw new InvalidOperationException("La impresora no devolvio un area imprimible valida.");
                float width=Math.Min(targetWidth,available.Width);
                float sourcePerUnit=image.Width/width;
                int remaining=inkHeights[m_currentPageIndex]-sourceOffset;
                int count=Math.Min(remaining,(int)Math.Floor(available.Height*sourcePerUnit));
                if(count<=0)throw new InvalidOperationException("La pagina no tiene altura util para imprimir.");
                var state=e.Graphics.Save();
                try
                {
                    e.Graphics.PageUnit=GraphicsUnit.Inch;
                    e.Graphics.TranslateTransform(-e.PageSettings.HardMarginX/100f,-e.PageSettings.HardMarginY/100f);
                    // El EMF mezcla texto y lineas: al dibujar un trozo suyo con escala, las lineas de las tablas se
                    // desplazan respecto al texto. Primero se rasteriza el trozo 1:1 y se escala ya la imagen.
                    using(var slice=new Bitmap(image.Width,count,PixelFormat.Format24bppRgb))
                    {
                        using(var graphics=Graphics.FromImage(slice))
                        {
                            graphics.Clear(Color.White);
                            graphics.DrawImage(image,new Rectangle(0,0,image.Width,count),new Rectangle(0,sourceOffset,image.Width,count),GraphicsUnit.Pixel);
                        }
                        e.Graphics.InterpolationMode=InterpolationMode.HighQualityBicubic;
                        e.Graphics.PixelOffsetMode=PixelOffsetMode.Half;
                        e.Graphics.DrawImage(slice,new RectangleF(available.X/100f,available.Y/100f,width/100f,count/sourcePerUnit/100f),
                            new RectangleF(0,0,slice.Width,slice.Height),GraphicsUnit.Pixel);
                    }
                }
                finally {e.Graphics.Restore(state);}
                sourceOffset+=count;
                // Si el driver limita el largo solicitado, la parte restante sale en la pagina siguiente.
                if(sourceOffset>=inkHeights[m_currentPageIndex]) {m_currentPageIndex++;sourceOffset=0;}
                e.HasMorePages=m_currentPageIndex<m_streams.Count;
            }
        }

        // Reparte el documento entre la impresora de cable (driver de Windows) y la Bluetooth, según BluetoothPrinterMode.
        public void Imprimir(LocalReport report)
        {
            switch(BluetoothReceiptPrinter.Mode)
            {
                case BluetoothPrinterMode.Only:
                    PrintBluetooth(report);
                    return;
                case BluetoothPrinterMode.Both:
                case BluetoothPrinterMode.Auto:
                    bool both=BluetoothReceiptPrinter.Mode==BluetoothPrinterMode.Both;
                    // Auto: si la predeterminada de Windows (XP-58, POS-80...) no está conectada, va directo por Bluetooth.
                    if(!both && !BluetoothReceiptPrinter.DefaultPrinterIsReady())
                    {
                        try {PrintBluetooth(report);return;}
                        catch
                        {
                            // Sin Bluetooth (o impresora apagada) todo queda como antes de existir esta función: a la cola de Windows.
                            PrintWithDriver(report);
                            return;
                        }
                    }
                    Exception wiredError=null;
                    try {PrintWithDriver(report);}
                    catch(Exception ex) {wiredError=ex;}
                    if(wiredError==null && !both)return;
                    try {PrintBluetooth(report);}
                    catch
                    {
                        // Si el documento ya salió por cable, un fallo del Bluetooth no debe provocar un reintento duplicado.
                        if(wiredError==null)return;
                        // Si falló todo, el error útil es el de la impresora de cable, no el de un Bluetooth que quizá ni existe.
                        System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(wiredError).Throw();
                    }
                    if(wiredError!=null)System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(wiredError).Throw();
                    return;
                default:
                    PrintWithDriver(report);
                    return;
            }
        }

        // Filas del EMF que se rasterizan por vez al imprimir por Bluetooth: un ticket largo no cabe en un solo bitmap.
        private const int BluetoothSliceRows=512;

        // El mismo ticket que sale por un driver de 58 mm, dibujado en una imagen de 384 puntos y enviado como trama
        // ESC/POS directamente por Bluetooth. No pasa por la cola de impresión de Windows.
        public void PrintBluetooth(LocalReport report)
        {
            int configuredWidth=(int)Math.Round(Dimension("AnchoHoja",315));
            int width=configuredWidth;
            int height=(int)Math.Round(Dimension("AltoHoja",984));
            bool roll=configuredWidth<=400;
            bool receipt=IsReceipt(report) && roll;
            LocalReport adapted=null;
            try
            {
                if(roll)
                {
                    width=Math.Min(width,BluetoothReceiptPrinter.PrintableWidth);
                    if(receipt)height=12897; // sin driver no hay largo máximo de página: el mismo tope que el rollo de la POS-80.
                    if(width<NarrowRollWidth)
                    {
                        adapted=ReceiptLayoutAdapter.Adapt(report,width);
                        if(adapted!=null)renderWidth=width;
                    }
                }
                receiptOverride=receipt;
                Prepare(adapted ?? report,width,height);
                using(var printer=BluetoothReceiptPrinter.Open())
                {
                    for(m_currentPageIndex=0;m_currentPageIndex<m_streams.Count;m_currentPageIndex++)
                    using(var image=CurrentImage())
                    {
                        // Con el diseño adaptado el EMF ya mide 384 puntos; si no se pudo adaptar, se reduce al cabezal.
                        double scale=Math.Min(1.0,(double)BluetoothReceiptPrinter.HeadDots/image.Width);
                        int outputWidth=Math.Min(BluetoothReceiptPrinter.HeadDots,(int)Math.Round(image.Width*scale));
                        int ink=inkHeights[m_currentPageIndex];
                        for(int offset=0;offset<ink;offset+=BluetoothSliceRows)
                        {
                            int count=Math.Min(BluetoothSliceRows,ink-offset);
                            using(var slice=new Bitmap(image.Width,count,PixelFormat.Format24bppRgb))
                            {
                                using(var graphics=Graphics.FromImage(slice))
                                {
                                    graphics.Clear(Color.White);
                                    graphics.DrawImage(image,new Rectangle(0,0,image.Width,count),new Rectangle(0,offset,image.Width,count),GraphicsUnit.Pixel);
                                }
                                if(outputWidth==image.Width) {printer.PrintImage(slice);continue;}
                                using(var scaled=new Bitmap(outputWidth,Math.Max(1,(int)Math.Round(count*scale)),PixelFormat.Format24bppRgb))
                                {
                                    using(var graphics=Graphics.FromImage(scaled))
                                    {
                                        graphics.Clear(Color.White);
                                        graphics.InterpolationMode=InterpolationMode.HighQualityBicubic;
                                        graphics.PixelOffsetMode=PixelOffsetMode.Half;
                                        graphics.DrawImage(slice,new Rectangle(0,0,scaled.Width,scaled.Height),new Rectangle(0,0,slice.Width,slice.Height),GraphicsUnit.Pixel);
                                    }
                                    printer.PrintImage(scaled);
                                }
                            }
                        }
                    }
                    printer.Feed();
                }
            }
            finally
            {
                Dispose();
                if(adapted!=null)adapted.Dispose();
                renderWidth=0;
                receiptOverride=null;
            }
        }

        private void PrintWithDriver(LocalReport report)
        {
            using(var document=new PrintDocument())
            {
                if(!document.PrinterSettings.IsValid)throw new InvalidOperationException("No hay una impresora predeterminada valida.");
                // El papel se mide en este momento con la impresora que se va a usar (58 o 80 mm); no va fijo.
                PrinterPaperInfo paper=PrinterPaper.Detect(document.PrinterSettings.PrinterName);
                int configuredWidth=(int)Math.Round(Dimension("AnchoHoja",315));
                int width=configuredWidth;
                int height=(int)Math.Round(Dimension("AltoHoja",984));
                bool roll=configuredWidth<=400;          // documentos de ticket (la hoja configurada es de 8 cm)
                bool receipt=IsReceipt(report) && roll;  // tickets de altura continua
                LocalReport adapted=null;
                try
                {
                    if(roll && paper.IsRoll)
                    {
                        // POS-80 anuncia 72 mm imprimibles para rollo de 80 mm y la XP-58, 48 mm para rollo de 58 mm.
                        width=Math.Min(width,paper.PrintableWidth);
                        if(receipt)
                        {
                            var rolls=document.PrinterSettings.PaperSizes.Cast<PaperSize>().Where(p=>p.Width<=400 && p.Width>=width-12).ToList();
                            height=rolls.Count>0 ? rolls.Max(p=>p.Height) : 11811; // PDF: hasta 3 metros, se recorta al contenido.
                            height=Math.Min(height,12897);
                        }
                        if(width<NarrowRollWidth)
                        {
                            // Rollo estrecho: se reajusta el diseño al ancho real en lugar de encoger el ticket terminado.
                            adapted=ReceiptLayoutAdapter.Adapt(report,width);
                            if(adapted!=null)renderWidth=width;
                        }
                    }
                    receiptOverride=receipt;
                    Prepare(adapted ?? report,width,height);
                    document.OriginAtMargins=false;
                    document.PrinterSettings.PrintRange=PrintRange.AllPages;
                    document.DocumentName=receipt ? "Casa Grande - ticket" : "Casa Grande - reporte";
                    document.DefaultPageSettings.Margins=new Margins(0,0,0,0);
                    document.DefaultPageSettings.PaperSize=new PaperSize("Casa Grande",targetWidth,CurrentHeight());
                    document.QueryPageSettings+=QueryPageSettings;
                    document.PrintPage+=PrintPage;
                    document.Print();
                }
                finally
                {
                    Dispose();
                    if(adapted!=null)adapted.Dispose();
                    renderWidth=0;
                    receiptOverride=null;
                }
            }
        }

        public void Dispose()
        {
            if(m_streams!=null)foreach(var stream in m_streams)stream.Dispose();
            m_streams=null;
            inkHeights.Clear();
            sourceOffset=0;
            m_currentPageIndex=0;
        }
    }
}
