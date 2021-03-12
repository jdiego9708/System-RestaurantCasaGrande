using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Data;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Windows.Forms;
using Microsoft.Reporting.WinForms;
using System.Configuration;

namespace CapaPresentacion
{
    public class ControladorImpresion
    {
        #region Atributos

        private int m_currentPageIndex;
        private IList<Stream> m_streams;

        #endregion

        #region Métodos privados

        private Stream CreateStream(string name, string fileNameExtension, Encoding encoding, string mimeType, bool willSeek)
        {
            //Routine to provide to the report renderer, in order to save an image for each page of the report.
            Stream stream = new FileStream(@"..\..\" + name + "." + fileNameExtension, FileMode.Create);
            m_streams.Add(stream);
            return stream;
        }

        private void Export(LocalReport report)
        {
            try
            {
                string medidaDefault = Convert.ToString(ConfigurationManager.AppSettings["MedidaPredeterminada"]);
                string ancho = Convert.ToString(ConfigurationManager.AppSettings["AnchoHoja"]) + medidaDefault;
                string alto = Convert.ToString(ConfigurationManager.AppSettings["AltoHoja"]) + medidaDefault;
                string margenTop = Convert.ToString(ConfigurationManager.AppSettings["MargenArriba"]) + medidaDefault;
                string margenBottom = Convert.ToString(ConfigurationManager.AppSettings["MargenAbajo"]) + medidaDefault;
                string margenLeft= Convert.ToString(ConfigurationManager.AppSettings["MargenIzquierda"]) + medidaDefault;
                string margenRight = Convert.ToString(ConfigurationManager.AppSettings["MargenDerecha"]) + medidaDefault;

                StringBuilder builder = new StringBuilder();
                builder.Append("<DeviceInfo> ");
                builder.Append("<OutputFormat>EMF</OutputFormat> ");
                builder.Append("<PageWidth>" + ancho + "</PageWidth> ");
                builder.Append("<PageHeight>" + alto + "</PageHeight> ");
                builder.Append("<MarginTop>" + margenTop + "</MarginTop> ");
                builder.Append("<MarginLeft>" + margenLeft + "</MarginLeft> ");
                builder.Append("<MarginRight>" + margenRight + "</MarginRight> ");
                builder.Append("<MarginBottom>" + margenBottom + "</MarginBottom> ");
                builder.Append("</DeviceInfo>");

                //Export the given report as an EMF(Enhanced Metafile) file.
                string deviceInfo = Convert.ToString(builder);
                Warning[] warnings;
                m_streams = new List<Stream>();
                report.Render("Image", deviceInfo, CreateStream, out warnings);
                foreach (Stream stream in m_streams)
                { stream.Position = 0; }
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorForm("Hubo un error al imprimir: Detalle: " + ex.Message);
            }
        }

        private void PrintPage(object sender, PrintPageEventArgs ev)
        {
            //Handler for PrintPageEvents
            Metafile pageImage = new Metafile(m_streams[m_currentPageIndex]);
            ev.Graphics.DrawImage(pageImage, ev.PageBounds);
            m_currentPageIndex++;
            ev.HasMorePages = (m_currentPageIndex < m_streams.Count);
        }

        private void Print()
        {
            //
            PrintDocument printDoc;

            string printerName = ImpresoraPredeterminada();
            if (m_streams == null || m_streams.Count == 0)
            { return; }

            printDoc = new PrintDocument();
            printDoc.PrinterSettings.PrinterName = printerName;
            if (!printDoc.PrinterSettings.IsValid)
            {
                string msg = String.Format("No se encontró la impresora \"{0}\".", printerName);
                MessageBox.Show(msg, "Error de impresión");
                return;
            }
            printDoc.PrintPage += new PrintPageEventHandler(PrintPage);
            printDoc.Print();
        }

        private string ImpresoraPredeterminada()
        {
            for (Int32 i = 0; i < PrinterSettings.InstalledPrinters.Count; i++)
            {
                PrinterSettings a = new PrinterSettings();
                a.PrinterName = PrinterSettings.InstalledPrinters[i].ToString();
                if (a.IsDefaultPrinter)
                { return PrinterSettings.InstalledPrinters[i].ToString(); }
            }
            return "";
        }

        #endregion

        #region Métodos públicos

        public void Imprimir(LocalReport argReporte)
        {
            Export(argReporte);
            m_currentPageIndex = 0;
            Print();
        }

        #endregion

        #region Soporte para implementación de interfaces

        #region IDisposable

        public void Dispose()
        {
            if (m_streams != null)
            {
                foreach (Stream stream in m_streams)
                { stream.Close(); }
                m_streams = null;
            }
        }
        #endregion

        #endregion
    }
}
