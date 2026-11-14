using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

// Sonda mínima: abre RFCOMM directo contra la PT210 por Winsock (AF_BTH), sin puerto COM ni driver.
namespace Pt210Probe
{
    // SOCKADDR_BTH empaquetado a 1 byte: familia (2) + dirección (8) + GUID del servicio (16) + canal (4).
    public sealed class BluetoothEndPoint : EndPoint
    {
        public const int BluetoothFamily = 32;
        private const int SockaddrSize = 30;
        private readonly ulong address;
        private readonly Guid service;
        private readonly int channel;

        public BluetoothEndPoint(ulong address, Guid service, int channel)
        {
            this.address = address;
            this.service = service;
            this.channel = channel;
        }

        public override AddressFamily AddressFamily { get { return (AddressFamily)BluetoothFamily; } }

        public override SocketAddress Serialize()
        {
            var socketAddress = new SocketAddress(AddressFamily, SockaddrSize);
            byte[] addressBytes = BitConverter.GetBytes(address);
            for (int i = 0; i < 8; i++) socketAddress[2 + i] = addressBytes[i];
            byte[] serviceBytes = service.ToByteArray();
            for (int i = 0; i < 16; i++) socketAddress[10 + i] = serviceBytes[i];
            byte[] channelBytes = BitConverter.GetBytes(channel);
            for (int i = 0; i < 4; i++) socketAddress[26 + i] = channelBytes[i];
            return socketAddress;
        }

        public override EndPoint Create(SocketAddress socketAddress) { return this; }
    }

    public static class Program
    {
        private const int RfcommProtocol = 3;
        private static readonly Guid SerialPortService = new Guid("00001101-0000-1000-8000-00805f9b34fb");

        public static int Main(string[] args)
        {
            string mode = args.Length > 0 ? args[0] : "connect";
            ulong address = Convert.ToUInt64((args.Length > 1 ? args[1] : "1022339D1FF2").Replace(":", ""), 16);
            // Canal 0 = que Windows lo resuelva por SDP con el UUID de SPP.
            int channel = args.Length > 2 ? int.Parse(args[2]) : 1;
            Guid service = channel == 0 ? SerialPortService : Guid.Empty;

            var watch = Stopwatch.StartNew();
            using (var socket = new Socket((AddressFamily)BluetoothEndPoint.BluetoothFamily, SocketType.Stream, (ProtocolType)RfcommProtocol))
            {
                try
                {
                    socket.Connect(new BluetoothEndPoint(address, service, channel));
                }
                catch (SocketException ex)
                {
                    Console.WriteLine("CONNECT FAILED after " + watch.ElapsedMilliseconds + " ms: " + ex.ErrorCode + " " + ex.SocketErrorCode + " - " + ex.Message);
                    return 1;
                }
                Console.WriteLine("CONNECTED in " + watch.ElapsedMilliseconds + " ms (channel " + channel + ")");

                if (mode == "hex")
                {
                    // Envía bytes arbitrarios (hex en args[3]) y vuelca lo que responda durante args[4] ms.
                    string hex = args[3].Replace(" ", "");
                    var raw = new byte[hex.Length / 2];
                    for (int i = 0; i < raw.Length; i++) raw[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
                    int waitMs = args.Length > 4 ? int.Parse(args[4]) : 3000;
                    int sent = socket.Send(raw);
                    Console.WriteLine("SENT " + sent + " of " + raw.Length + " bytes");
                    socket.ReceiveTimeout = waitMs;
                    var reply = new byte[256];
                    try
                    {
                        while (true)
                        {
                            int read = socket.Receive(reply);
                            if (read == 0) { Console.WriteLine("REMOTE CLOSED"); break; }
                            Console.WriteLine("RECV " + BitConverter.ToString(reply, 0, read));
                        }
                    }
                    catch (SocketException ex)
                    {
                        Console.WriteLine("RECV end (" + ex.SocketErrorCode + ")");
                    }
                }

                if (mode == "status" || mode == "print")
                {
                    // DLE EOT n: consulta de estado en tiempo real de ESC/POS; si responde, el dialecto queda confirmado.
                    for (byte n = 1; n <= 4; n++)
                    {
                        socket.Send(new byte[] { 0x10, 0x04, n });
                        socket.ReceiveTimeout = 1500;
                        var buffer = new byte[16];
                        try
                        {
                            int read = socket.Receive(buffer);
                            Console.WriteLine("DLE EOT " + n + " -> " + BitConverter.ToString(buffer, 0, read));
                        }
                        catch (SocketException ex)
                        {
                            Console.WriteLine("DLE EOT " + n + " -> no reply (" + ex.SocketErrorCode + ")");
                        }
                    }
                }

                if (mode == "print")
                {
                    var payload = new System.IO.MemoryStream();
                    Action<byte[]> write = bytes => payload.Write(bytes, 0, bytes.Length);
                    write(new byte[] { 0x1B, 0x40 });              // ESC @: reinicia
                    write(new byte[] { 0x1B, 0x61, 0x01 });        // ESC a 1: centrado
                    write(new byte[] { 0x1B, 0x45, 0x01 });        // ESC E 1: negrita
                    write(Encoding.ASCII.GetBytes("CASA GRANDE\n"));
                    write(new byte[] { 0x1B, 0x45, 0x00 });
                    write(Encoding.ASCII.GetBytes("Prueba directa .NET -> PT210\n"));
                    write(Encoding.ASCII.GetBytes(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\n"));
                    write(new byte[] { 0x1B, 0x61, 0x00 });
                    write(Encoding.ASCII.GetBytes("12345678901234567890123456789012\n"));
                    write(Encoding.ASCII.GetBytes("\n\n\n"));
                    socket.Send(payload.ToArray());
                    Console.WriteLine("SENT " + payload.Length + " bytes");
                    // Margen para que el módulo vacíe su búfer antes de cerrar el enlace.
                    System.Threading.Thread.Sleep(1500);
                }

                socket.Shutdown(SocketShutdown.Both);
            }
            Console.WriteLine("CLOSED");
            return 0;
        }
    }
}
