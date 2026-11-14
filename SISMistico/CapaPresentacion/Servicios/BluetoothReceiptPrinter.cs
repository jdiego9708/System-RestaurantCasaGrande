using System;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;

namespace CapaPresentacion
{
    // Cómo convive la impresora Bluetooth con la de cable (clave BluetoothPrinterMode de App.config).
    public enum BluetoothPrinterMode
    {
        Off,   // no se usa
        Auto,  // impresora predeterminada de Windows si está conectada; si no, o si falla, la Bluetooth
        Both,  // cada documento sale por las dos
        Only,  // todo sale por Bluetooth
    }

    // Dirección de Winsock para Bluetooth (SOCKADDR_BTH, empaquetada a 1 byte): familia (2) + dirección (8) +
    // GUID del servicio (16) + canal RFCOMM (4). .NET Framework no trae esta familia, pero acepta un EndPoint propio.
    internal sealed class BluetoothEndPoint : EndPoint
    {
        public const int BluetoothFamily = 32;
        private const int SockaddrSize = 30;
        private readonly ulong address;
        private readonly int channel;

        public BluetoothEndPoint(ulong address, int channel)
        {
            this.address = address;
            this.channel = channel;
        }

        public override AddressFamily AddressFamily { get { return (AddressFamily)BluetoothFamily; } }

        public override SocketAddress Serialize()
        {
            // El GUID del servicio queda en ceros: se conecta por número de canal, sin consulta SDP.
            var socketAddress = new SocketAddress(AddressFamily, SockaddrSize);
            byte[] addressBytes = BitConverter.GetBytes(address);
            for (int i = 0; i < 8; i++) socketAddress[2 + i] = addressBytes[i];
            byte[] channelBytes = BitConverter.GetBytes(channel);
            for (int i = 0; i < 4; i++) socketAddress[26 + i] = channelBytes[i];
            return socketAddress;
        }

        public override EndPoint Create(SocketAddress socketAddress) { return this; }
    }

    // Impresión directa en una térmica ESC/POS por Bluetooth clásico (RFCOMM), sin driver de Windows, sin puerto COM
    // y sin emparejar. Probada con la PT-210 (PT210_1FF2): 58 mm, cabezal de 384 puntos a 203 ppp, canal 1; acepta
    // la conexión sin PIN. Diagnóstico completo en docs/17-diagnostico-bluetooth-pt210-2026-11-11.md.
    public sealed class BluetoothReceiptPrinter : IDisposable
    {
        // Puntos del cabezal: el ancho exacto de la imagen que se envía.
        public const int HeadDots = 384;
        // El mismo ancho en centésimas de pulgada (384 puntos a 203 ppp = 48 mm), como lo anuncia un driver de 58 mm.
        public const int PrintableWidth = 189;

        private const int RfcommProtocol = 3;
        private const int DefaultChannel = 1;
        // Filas por orden GS v 0: tramas pequeñas (6 KB) para no desbordar el búfer del módulo.
        private const int BandRows = 128;
        // Por debajo de esta luminosidad el punto se quema. Algo alto para que el texto fino no salga gris.
        private const int InkThreshold = 170;
        private const int ConnectAttempts = 3;
        // La impresora atiende un solo enlace: los trabajos del escritorio y de la API móvil van de uno en uno.
        private static readonly object Gate = new object();

        private Socket socket;
        private bool lockTaken;

        private BluetoothReceiptPrinter() { }

        public static string ConfiguredAddress
        {
            get { return (ConfigurationManager.AppSettings["BluetoothPrinterAddress"] ?? "").Trim(); }
        }

        public static BluetoothPrinterMode Mode
        {
            get
            {
                if (ConfiguredAddress.Length == 0) return BluetoothPrinterMode.Off;
                string value = (ConfigurationManager.AppSettings["BluetoothPrinterMode"] ?? "").Trim();
                BluetoothPrinterMode mode;
                return Enum.TryParse(value, true, out mode) ? mode : BluetoothPrinterMode.Auto;
            }
        }

        // ¿La impresora predeterminada de Windows (XP-58, POS-80, etc.) está lista para imprimir? Una USB desconectada
        // no hace fallar la impresión: el trabajo se queda en cola. Por eso se consulta el estado antes de elegir.
        public static bool DefaultPrinterIsReady()
        {
            string name;
            try
            {
                var settings = new System.Drawing.Printing.PrinterSettings();
                if (!settings.IsValid) return false;
                name = settings.PrinterName;
            }
            catch (Exception) { return false; }

            // Impresoras virtuales (PDF, XPS, OneNote): siempre "listas", pero no son térmicas.
            if (name.IndexOf("PDF", StringComparison.OrdinalIgnoreCase) >= 0
                || name.IndexOf("XPS", StringComparison.OrdinalIgnoreCase) >= 0
                || name.IndexOf("OneNote", StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            try
            {
                string escaped = name.Replace("\\", "\\\\").Replace("'", "\\'");
                using (var searcher = new System.Management.ManagementObjectSearcher(
                    "SELECT WorkOffline, PrinterStatus, DetectedErrorState FROM Win32_Printer WHERE Name='" + escaped + "'"))
                {
                    foreach (System.Management.ManagementObject printer in searcher.Get())
                    {
                        if (printer["WorkOffline"] is bool && (bool)printer["WorkOffline"]) return false;
                        // PrinterStatus 7 = sin conexión; DetectedErrorState 8 = sin conexión.
                        if (printer["PrinterStatus"] != null && Convert.ToInt32(printer["PrinterStatus"]) == 7) return false;
                        if (printer["DetectedErrorState"] != null && Convert.ToInt32(printer["DetectedErrorState"]) == 8) return false;
                        return true;
                    }
                }
            }
            catch (Exception) { return true; } // si WMI no responde, se confía en la predeterminada como antes
            return false;
        }

        private static ulong ParseAddress(string text)
        {
            string hex = text.Replace(":", "").Replace("-", "");
            ulong address;
            if (hex.Length != 12 || !ulong.TryParse(hex, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out address))
                throw new InvalidOperationException("La dirección de la impresora Bluetooth no es válida: " + text);
            return address;
        }

        // Abre el enlace con la impresora configurada. Hay que liberar el objeto para soltarla.
        public static BluetoothReceiptPrinter Open()
        {
            string configured = ConfiguredAddress;
            if (configured.Length == 0) throw new InvalidOperationException("No hay una impresora Bluetooth configurada.");
            int channel;
            if (!int.TryParse(ConfigurationManager.AppSettings["BluetoothPrinterChannel"], out channel) || channel < 1 || channel > 30)
                channel = DefaultChannel;
            return Open(ParseAddress(configured), channel);
        }

        public static BluetoothReceiptPrinter Open(ulong address, int channel)
        {
            var printer = new BluetoothReceiptPrinter();
            Monitor.Enter(Gate, ref printer.lockTaken);
            try
            {
                SocketException last = null;
                for (int attempt = 0; attempt < ConnectAttempts; attempt++)
                {
                    // Tras un cierre reciente el módulo tarda un instante en volver a aceptar enlaces.
                    if (attempt > 0) Thread.Sleep(700);
                    Socket candidate;
                    try
                    {
                        candidate = new Socket((AddressFamily)BluetoothEndPoint.BluetoothFamily, SocketType.Stream, (ProtocolType)RfcommProtocol);
                    }
                    catch (SocketException ex)
                    {
                        // Sin adaptador Bluetooth Windows no ofrece la familia de direcciones: reintentar no sirve.
                        throw new InvalidOperationException("Este equipo no tiene Bluetooth disponible o está apagado. (" + ex.SocketErrorCode + ")", ex);
                    }
                    try
                    {
                        candidate.Connect(new BluetoothEndPoint(address, channel));
                        candidate.SendTimeout = 20000;
                        printer.socket = candidate;
                        printer.Send(0x1B, 0x40); // ESC @: estado inicial conocido
                        return printer;
                    }
                    catch (SocketException ex)
                    {
                        candidate.Dispose();
                        last = ex;
                    }
                }
                throw new InvalidOperationException(
                    "No se pudo conectar con la impresora Bluetooth. Compruebe que esté encendida, cerca y que el celular no la esté usando. ("
                    + (last == null ? "sin detalle" : last.SocketErrorCode.ToString()) + ")", last);
            }
            catch
            {
                printer.Dispose();
                throw;
            }
        }

        private void Send(params byte[] bytes)
        {
            int sent = 0;
            while (sent < bytes.Length) sent += socket.Send(bytes, sent, bytes.Length - sent, SocketFlags.None);
        }

        // Imprime una imagen de hasta 384 puntos de ancho, pegada a la izquierda. Se puede llamar varias veces
        // seguidas: los trozos salen uno a continuación del otro, sin separación.
        public void PrintImage(Bitmap image)
        {
            if (image.Width > HeadDots) throw new ArgumentException("La imagen supera los " + HeadDots + " puntos del cabezal.");
            int width = image.Width, height = image.Height, rowBytes = (width + 7) / 8;
            BitmapData data = image.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            byte[] pixels;
            int stride = data.Stride;
            try
            {
                pixels = new byte[Math.Abs(stride) * height];
                Marshal.Copy(stride >= 0 ? data.Scan0 : IntPtr.Add(data.Scan0, stride * (height - 1)), pixels, 0, pixels.Length);
            }
            finally { image.UnlockBits(data); }

            for (int top = 0; top < height; top += BandRows)
            {
                int rows = Math.Min(BandRows, height - top);
                var band = new byte[8 + rowBytes * rows];
                // GS v 0: trama de bits, un bit por punto, el más significativo a la izquierda.
                band[0] = 0x1D; band[1] = 0x76; band[2] = 0x30; band[3] = 0x00;
                band[4] = (byte)(rowBytes & 0xFF); band[5] = (byte)(rowBytes >> 8);
                band[6] = (byte)(rows & 0xFF); band[7] = (byte)(rows >> 8);
                for (int y = 0; y < rows; y++)
                {
                    int sourceRow = stride >= 0 ? (top + y) * stride : (height - 1 - top - y) * -stride;
                    int target = 8 + y * rowBytes;
                    for (int x = 0; x < width; x++)
                    {
                        int offset = sourceRow + x * 3;
                        int luminance = (pixels[offset + 2] * 299 + pixels[offset + 1] * 587 + pixels[offset] * 114) / 1000;
                        if (luminance < InkThreshold) band[target + (x >> 3)] |= (byte)(0x80 >> (x & 7));
                    }
                }
                Send(band);
            }
        }

        // Avanza el papel para que la última línea pase la barra de corte.
        public void Feed(int lines = 3)
        {
            Send(0x1B, 0x64, (byte)Math.Max(0, Math.Min(255, lines)));
        }

        public void Dispose()
        {
            if (socket != null)
            {
                try
                {
                    // Cerrar de golpe puede descartar lo que el módulo aún no pasó al cabezal.
                    socket.Shutdown(SocketShutdown.Send);
                    Thread.Sleep(400);
                }
                catch (SocketException) { }
                catch (ObjectDisposedException) { }
                socket.Dispose();
                socket = null;
            }
            if (lockTaken)
            {
                lockTaken = false;
                Monitor.Exit(Gate);
            }
        }
    }
}
