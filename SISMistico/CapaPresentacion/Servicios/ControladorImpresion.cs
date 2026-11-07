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
                || name.EndsWith("Precuenta.rdlc",StringComparison.OrdinalIgnoreCase);
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
            double width=Dimension("AnchoHoja",315),height=Dimension("AltoHoja",984);
            if(width<=0 || height<=0)throw new InvalidOperationException("El papel debe tener ancho y alto mayores que cero.");
            continuous=IsReceipt(report) && !string.Equals(ConfigurationManager.AppSettings["TicketsAlturaAutomatica"],"false",StringComparison.OrdinalIgnoreCase);
            targetWidth=Math.Max(1,Math.Min((int)Math.Round(width),printableWidth));
            maxPaperHeight=Math.Max(1,maximumHeight);
            if(continuous)height=maximumHeight;
            m_streams=new List<Stream>();
            try
            {
                Warning[] warnings;
                report.Render("Image",DeviceInfo(width,height),CreateStream,out warnings);
                foreach(var stream in m_streams)
                {
                    stream.Position=0;
                    using(var image=new Metafile(stream))inkHeights.Add(continuous ? InkBottom(image) : image.Height);
                    stream.Position=0;
                }
                // El indice siempre se reinicia tambien cuando se repite una copia.
                m_currentPageIndex=0;
                sourceOffset=0;
            }
            catch { Dispose(); throw; }
        }

        private static int InkBottom(Metafile image)
        {
            using(var bitmap=new Bitmap(image.Width,image.Height,PixelFormat.Format24bppRgb))
            {
                using(var graphics=Graphics.FromImage(bitmap))
                {
                    graphics.Clear(Color.White);
                    graphics.DrawImage(image,new Rectangle(0,0,bitmap.Width,bitmap.Height));
                }
                var data=bitmap.LockBits(new Rectangle(0,0,bitmap.Width,bitmap.Height),ImageLockMode.ReadOnly,PixelFormat.Format24bppRgb);
                try
                {
                    var bytes=new byte[Math.Abs(data.Stride)*bitmap.Height];
                    Marshal.Copy(data.Scan0,bytes,0,bytes.Length);
                    for(int y=bitmap.Height-1;y>=0;y--)
                    {
                        int row=data.Stride>=0 ? y*data.Stride : (bitmap.Height-1-y)*-data.Stride;
                        for(int x=0;x<bitmap.Width*3;x++)if(bytes[row+x]<250)return y+1;
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

        public void Imprimir(LocalReport report)
        {
            using(var document=new PrintDocument())
            {
                if(!document.PrinterSettings.IsValid)throw new InvalidOperationException("No hay una impresora predeterminada valida.");
                int configuredWidth=(int)Math.Round(Dimension("AnchoHoja",315));
                int width=configuredWidth;
                int height=(int)Math.Round(Dimension("AltoHoja",984));
                bool receipt=IsReceipt(report) && configuredWidth<=400;
                if(receipt)
                {
                    // POS-80 anuncia 72 mm imprimibles para rollo de 80 mm y formatos hasta 3276 mm.
                    if(document.DefaultPageSettings.PaperSize.Width<=400)
                        width=Math.Min(width,document.DefaultPageSettings.PaperSize.Width);
                    var rolls=document.PrinterSettings.PaperSizes.Cast<PaperSize>().Where(p=>p.Width<=400 && p.Width>=width-12).ToList();
                    height=rolls.Count>0 ? rolls.Max(p=>p.Height) : 11811; // PDF: hasta 3 metros, se recorta al contenido.
                    height=Math.Min(height,12897);
                }
                Prepare(report,width,height);
                document.OriginAtMargins=false;
                document.PrinterSettings.PrintRange=PrintRange.AllPages;
                document.DocumentName=receipt ? "Casa Grande - ticket" : "Casa Grande - reporte";
                document.DefaultPageSettings.Margins=new Margins(0,0,0,0);
                document.DefaultPageSettings.PaperSize=new PaperSize("Casa Grande",targetWidth,CurrentHeight());
                document.QueryPageSettings+=QueryPageSettings;
                document.PrintPage+=PrintPage;
                try {document.Print();}finally {Dispose();}
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
