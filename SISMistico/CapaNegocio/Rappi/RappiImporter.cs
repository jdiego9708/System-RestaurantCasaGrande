using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using CapaDatos;
using CapaEntidades.Models;

namespace CapaNegocio.Rappi
{
    /// <summary>Resultado de una pasada de importacion.</summary>
    public sealed class RappiImportResult
    {
        /// <summary>Ordenes nuevas guardadas en esta pasada.</summary>
        public int Imported { get; set; }

        /// <summary>
        /// PDF cuya orden ya estaba guardada (copias del mismo archivo u otro archivo de la misma orden): se apartaron
        /// en la carpeta Duplicados.
        /// </summary>
        public int Duplicates { get; set; }

        /// <summary>PDF que no se pudieron leer: "archivo: motivo". Se dejan donde estan.</summary>
        public List<string> Failed { get; } = new List<string>();
    }

    /// <summary>
    /// Lee las facturas PDF de Rappi que llegan a la carpeta, guarda cada orden una sola vez y deja la carpeta ordenada:
    /// <list type="bullet">
    /// <item>la factura de una orden nueva pasa a <c>FacturasRappi\AAAA-MM-DD\</c> (el dia de la orden);</item>
    /// <item>una copia o un segundo archivo de una orden ya guardada pasa a <c>FacturasRappi\Duplicados\AAAA-MM-DD\</c>;</item>
    /// <item>un PDF que no se puede leer se queda donde esta y se avisa una vez (puede estar a medio copiar).</item>
    /// </list>
    /// El duplicado se decide por el ID de orden de Rappi (indice unico en la base) y, antes de leer, por la huella del
    /// archivo. Es seguro llamarlo muchas veces y desde varios equipos a la vez.
    /// </summary>
    public static class RappiImporter
    {
        public const string DuplicatesFolderName = "Duplicados";

        private static readonly object Gate = new object();

        // Archivos de la carpeta raiz ya vistos y dejados ahi (ilegibles): evita volver a leerlos mientras no cambien.
        private static readonly Dictionary<string, string> Seen = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // Un archivo recien escrito puede estar aun copiandose: se espera este tiempo desde su ultima escritura.
        private static readonly TimeSpan SettleTime = TimeSpan.FromSeconds(1);

        public static RappiImportResult ImportFolder(string folder)
        {
            RappiImportResult result = new RappiImportResult();
            if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder))
                return result;

            lock (Gate)
            {
                Dictionary<string, DRappiOrders.KnownFile> known = DRappiOrders.KnownFiles();
                // Solo la carpeta raiz es la bandeja de entrada: lo que ya se ordeno en subcarpetas no se vuelve a leer.
                foreach (string path in Directory.GetFiles(folder, "*.pdf").OrderBy(f => File.GetCreationTimeUtc(f)))
                {
                    try
                    {
                        FileInfo info = new FileInfo(path);
                        string signature = info.Length + "|" + info.LastWriteTimeUtc.Ticks;
                        string previous;
                        if (Seen.TryGetValue(path, out previous) && previous == signature)
                            continue;
                        if (info.Length == 0 || DateTime.UtcNow - info.LastWriteTimeUtc < SettleTime)
                            continue; // se esta copiando; se reintenta en la siguiente pasada

                        byte[] bytes = ReadShared(path);
                        if (bytes == null)
                            continue; // otro proceso lo tiene abierto escribiendo
                        string hash = Hash(bytes);
                        string name = Path.GetFileName(path);

                        DRappiOrders.KnownFile known1;
                        if (known.TryGetValue(hash, out known1))
                        {
                            // El mismo archivo ya se leyo. Si es el original (mismo nombre y todavia sin ordenar, p. ej.
                            // importado antes de existir las carpetas por dia) solo se ordena; si no, es una copia.
                            bool original = string.Equals(known1.SourceFile, name, StringComparison.OrdinalIgnoreCase)
                                && !File.Exists(Path.Combine(DayFolder(folder, known1.Day), name));
                            if (original)
                            {
                                MoveInto(path, DayFolder(folder, known1.Day));
                            }
                            else
                            {
                                MoveInto(path, DuplicatesDayFolder(folder, known1.Day));
                                result.Duplicates++;
                            }
                            Seen.Remove(path);
                            continue;
                        }

                        RappiOrder order;
                        try
                        {
                            order = RappiInvoiceParser.Parse(bytes, name, hash);
                        }
                        catch (Exception ex)
                        {
                            // No es un PDF legible (o esta incompleto): se deja donde esta y no se vuelve a intentar
                            // hasta que el archivo cambie.
                            Seen[path] = signature;
                            result.Failed.Add(name + ": " + ex.Message);
                            continue;
                        }

                        DateTime day = order.EffectiveDay();
                        bool inserted = DRappiOrders.InsertIfNew(order);
                        known[hash] = new DRappiOrders.KnownFile { SourceFile = name, Day = day };
                        if (inserted)
                        {
                            MoveInto(path, DayFolder(folder, day));
                            result.Imported++;
                        }
                        else
                        {
                            // Otro archivo de la misma orden (u otro equipo se adelanto).
                            MoveInto(path, DuplicatesDayFolder(folder, day));
                            result.Duplicates++;
                        }
                        Seen.Remove(path);
                    }
                    catch (IOException)
                    {
                        // El archivo desaparecio o sigue en uso: se reintenta despues.
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // Sin permiso para moverlo ahora: la orden ya esta guardada, se reintenta despues.
                    }
                }
            }
            return result;
        }

        private static DateTime EffectiveDay(this RappiOrder order) =>
            (order.OrderDateTime ?? DateTime.Now).Date;

        /// <summary>FacturasRappi\2026-11-14</summary>
        public static string DayFolder(string root, DateTime day) =>
            Path.Combine(root, day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));

        /// <summary>FacturasRappi\Duplicados\2026-11-14</summary>
        public static string DuplicatesDayFolder(string root, DateTime day) =>
            Path.Combine(root, DuplicatesFolderName, day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));

        // Mueve el archivo a la carpeta (creandola); si ya hay uno con ese nombre agrega " (2)", " (3)"...
        private static void MoveInto(string file, string targetFolder)
        {
            Directory.CreateDirectory(targetFolder);
            string name = Path.GetFileNameWithoutExtension(file);
            string extension = Path.GetExtension(file);
            string target = Path.Combine(targetFolder, name + extension);
            for (int n = 2; File.Exists(target); n++)
                target = Path.Combine(targetFolder, name + " (" + n + ")" + extension);
            File.Move(file, target);
        }

        // Lee el archivo permitiendo que otro lo tenga abierto para leer; null si esta bloqueado para escritura.
        private static byte[] ReadShared(string path)
        {
            try
            {
                using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (MemoryStream memory = new MemoryStream())
                {
                    stream.CopyTo(memory);
                    return memory.ToArray();
                }
            }
            catch (IOException)
            {
                return null;
            }
        }

        private static string Hash(byte[] bytes)
        {
            using (SHA256 sha = SHA256.Create())
                return string.Concat(sha.ComputeHash(bytes).Select(b => b.ToString("x2")));
        }
    }
}
