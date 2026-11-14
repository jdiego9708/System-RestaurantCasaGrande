using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Script.Serialization;
using CapaNegocio;
using CapaNegocio.Alegra;
using CapaPresentacion.UI;

namespace CapaPresentacion.Formularios.FormsProductos
{
    // La caché es una fotografía de consulta; nunca crea vínculos ni productos.
    // Es única por aplicación (base de datos + empresa de Alegra): el catálogo de Productos la puebla al validar
    // y la ventana de productos de la factura la reutiliza. Vive en memoria y se persiste en disco entre sesiones.
    // Solo vence al pulsar VALIDAR PRODUCTOS CON ALEGRA (Clear); no hay caducidad por tiempo, el badge solo informa la antigüedad.
    internal sealed class AlegraProductValidationCache
    {
        public sealed class Entry
        {
            public string Kind { get; set; }
            public string Id { get; set; }
            public string Name { get; set; }
            public string Exists { get; set; }
            public decimal? Price { get; set; }
            public decimal? Tax { get; set; }
            public string Detail { get; set; }
            public bool Linked { get; set; }
            public List<AlegraSimilarProduct> Similar { get; set; }
            public AlegraSimilarProduct Exact { get; set; }
        }

        public sealed class Snapshot
        {
            public int Version { get; set; }
            public DateTime ValidatedAtUtc { get; set; }
            public List<Entry> Products { get; set; }
        }

        private static readonly object sync = new object();
        private static readonly Dictionary<string, AlegraProductValidationCache> shared = new Dictionary<string, AlegraProductValidationCache>();

        // Se dispara cuando la caché se vacía o se vuelve a poblar (nueva validación completa).
        public static event EventHandler Changed;

        private readonly string path;
        private readonly JavaScriptSerializer json = new JavaScriptSerializer { MaxJsonLength = 16 * 1024 * 1024 };
        private Snapshot memory;
        private bool loaded;

        public AlegraProductValidationCache(string scope, string directory = null)
        {
            string key;
            using (var hash = SHA256.Create())
                key = BitConverter.ToString(hash.ComputeHash(Encoding.UTF8.GetBytes(scope))).Replace("-", "");
            path = Path.Combine(directory ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "CasaGrande", "Alegra", "ProductValidation"), key + ".json");
        }

        // Instancia compartida de la aplicación para la base de datos y la empresa de Alegra actuales.
        public static AlegraProductValidationCache Shared()
        {
            string scope = NProductos.DatabaseIdentity() + "|" + AlegraWorkflow.Settings().CompanyId;
            lock (sync)
            {
                AlegraProductValidationCache cache;
                if (!shared.TryGetValue(scope, out cache)) shared[scope] = cache = new AlegraProductValidationCache(scope);
                return cache;
            }
        }

        // Fecha de la última validación completa, o null si no hay (o el archivo no se pudo leer).
        public DateTime? ValidatedAtUtc
        {
            get
            {
                try
                {
                    var snapshot = Load();
                    return snapshot == null ? (DateTime?)null : DateTime.SpecifyKind(snapshot.ValidatedAtUtc, DateTimeKind.Utc);
                }
                catch (Exception) { return null; }
            }
        }

        // Texto y color del badge de antigüedad: verde si es de hoy, ámbar hasta una semana, rojo si es más viejo.
        public static string AgeText(DateTime? validatedAtUtc)
        {
            if (!validatedAtUtc.HasValue) return "Sin validar con Alegra";
            TimeSpan age = DateTime.UtcNow - validatedAtUtc.Value;
            if (age < TimeSpan.Zero) age = TimeSpan.Zero;
            if (age.TotalMinutes < 1) return "Validado hace un momento";
            if (age.TotalMinutes < 60) return "Validado hace " + (int)age.TotalMinutes + " min";
            if (age.TotalHours < 6) return "Validado hace " + (int)age.TotalHours + " h " + age.Minutes + " min";
            if (age.TotalHours < 24) return "Validado hace " + (int)age.TotalHours + " h";
            int days = (int)age.TotalDays;
            return "Validado hace " + days + (days == 1 ? " día" : " días");
        }

        public static BadgeKind AgeKind(DateTime? validatedAtUtc)
        {
            if (!validatedAtUtc.HasValue) return BadgeKind.Neutral;
            TimeSpan age = DateTime.UtcNow - validatedAtUtc.Value;
            return age < TimeSpan.FromHours(24) ? BadgeKind.Success : age < TimeSpan.FromDays(7) ? BadgeKind.Warning : BadgeKind.Danger;
        }

        private Snapshot Load()
        {
            lock (sync)
            {
                if (loaded) return memory;
                Snapshot snapshot = null;
                if (File.Exists(path))
                {
                    snapshot = json.Deserialize<Snapshot>(File.ReadAllText(path, Encoding.UTF8));
                    if (snapshot == null || snapshot.Version != 1 || snapshot.Products == null)
                        throw new InvalidDataException("Formato de caché de productos no válido.");
                }
                memory = snapshot; loaded = true;
                return memory;
            }
        }

        public void Clear()
        {
            lock (sync)
            {
                if (File.Exists(path)) File.Delete(path);
                memory = null; loaded = true;
            }
            Changed?.Invoke(this, EventArgs.Empty);
        }

        private static string KeyOf(string kind, string id) { return kind + ":" + id; }

        private static Entry ToEntry(DataRow row)
        {
            return new Entry { Kind = Convert.ToString(row["Tipo"]),
                Id = Convert.ToString(row["Id"], CultureInfo.InvariantCulture), Name = Convert.ToString(row["Nombre"]),
                Exists = Convert.ToString(row["ExisteAlegra"]), Price = row.IsNull("PrecioAlegra") ? (decimal?)null : Convert.ToDecimal(row["PrecioAlegra"]),
                Tax = row.IsNull("ImpuestoAlegra") ? (decimal?)null : Convert.ToDecimal(row["ImpuestoAlegra"]),
                Detail = Convert.ToString(row["DetalleAlegra"]), Linked = Convert.ToBoolean(row["VinculadoAlegra"]),
                Similar = row["SimilarProducts"] as List<AlegraSimilarProduct>, Exact = row["ExactProduct"] as AlegraSimilarProduct };
        }

        private void Write(Snapshot snapshot)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            string pending = path + ".tmp";
            File.WriteAllText(pending, json.Serialize(snapshot), Encoding.UTF8);
            if (File.Exists(path)) File.Replace(pending, path, null);
            else File.Move(pending, path);
        }

        // Guarda una validación completa del catálogo y la deja disponible en memoria para toda la aplicación.
        public void Save(DataTable products, DateTime validatedAtUtc)
        {
            var snapshot = new Snapshot { Version = 1, ValidatedAtUtc = validatedAtUtc, Products = new List<Entry>() };
            foreach (DataRow row in products.Rows)
            {
                if (row.IsNull("ExisteAlegra")) continue;
                snapshot.Products.Add(ToEntry(row));
            }
            lock (sync)
            {
                Write(snapshot);
                memory = snapshot; loaded = true;
            }
            Changed?.Invoke(this, EventArgs.Empty);
        }

        // Actualiza solo las filas indicadas (p. ej. un producto recién vinculado) sin cambiar la fecha de la validación.
        // Si todavía no hay una validación guardada no hace nada: no se puede atribuir una fecha a datos sin validar.
        public void Merge(IEnumerable<DataRow> rows)
        {
            lock (sync)
            {
                var snapshot = Load();
                if (snapshot == null) return;
                var index = new Dictionary<string, int>();
                for (int i = 0; i < snapshot.Products.Count; i++) index[KeyOf(snapshot.Products[i].Kind, snapshot.Products[i].Id)] = i;
                foreach (DataRow row in rows)
                {
                    if (row.IsNull("ExisteAlegra")) continue;
                    Entry entry = ToEntry(row);
                    int position;
                    if (index.TryGetValue(KeyOf(entry.Kind, entry.Id), out position)) snapshot.Products[position] = entry;
                    else { index[KeyOf(entry.Kind, entry.Id)] = snapshot.Products.Count; snapshot.Products.Add(entry); }
                }
                Write(snapshot);
            }
        }

        public DateTime? Restore(DataTable products)
        {
            var snapshot = Load();
            if (snapshot == null) return null;
            AlegraProductValidation.PrepareColumns(products);
            var entries = new Dictionary<string, Entry>();
            foreach (var entry in snapshot.Products) entries[KeyOf(entry.Kind, entry.Id)] = entry;
            int restored = 0;
            foreach (DataRow row in products.Rows)
            {
                Entry entry;
                if (!entries.TryGetValue(KeyOf(Convert.ToString(row["Tipo"]), Convert.ToString(row["Id"], CultureInfo.InvariantCulture)), out entry)
                    || entry.Name != Convert.ToString(row["Nombre"])) continue;
                row["ExisteAlegra"] = entry.Exists;
                row["PrecioAlegra"] = (object)entry.Price ?? DBNull.Value;
                row["ImpuestoAlegra"] = (object)entry.Tax ?? DBNull.Value;
                row["DetalleAlegra"] = entry.Detail;
                row["VinculadoAlegra"] = entry.Linked;
                row["SimilarProducts"] = (object)entry.Similar ?? DBNull.Value;
                row["ExactProduct"] = (object)entry.Exact ?? DBNull.Value;
                restored++;
            }
            return restored > 0 ? (DateTime?)DateTime.SpecifyKind(snapshot.ValidatedAtUtc, DateTimeKind.Utc) : null;
        }
    }
}
