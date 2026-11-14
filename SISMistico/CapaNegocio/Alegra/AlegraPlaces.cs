using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CapaNegocio.Alegra
{
    // Catálogo de departamentos y municipios de Colombia tal como lo publica Alegra
    // (https://developer.alegra.com/reference/colombia, sección «Departamentos y municipios»).
    // Alegra recibe address.department y address.city como texto y los resuelve contra este catálogo; por eso
    // se envían exactamente con esta grafía («Bogotá D.C.» como departamento, «Bogotá, D.C.» como municipio).
    // No hay endpoint de consulta: la lista se embebe en el ensamblado (ColombiaPlaces.txt, «departamento<TAB>municipio»).
    public static class AlegraPlaces
    {
        public const string Country = "Colombia";
        private static readonly Lazy<Dictionary<string, List<string>>> Catalog = new Lazy<Dictionary<string, List<string>>>(Load);

        private static Dictionary<string, List<string>> Load()
        {
            var assembly = typeof(AlegraPlaces).Assembly;
            string resource = assembly.GetManifestResourceNames().First(name => name.EndsWith("ColombiaPlaces.txt", StringComparison.OrdinalIgnoreCase));
            var catalog = new Dictionary<string, List<string>>();
            using (var reader = new StreamReader(assembly.GetManifestResourceStream(resource), System.Text.Encoding.UTF8))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] parts = line.Split('\t');
                    if (parts.Length != 2) continue;
                    List<string> municipalities;
                    if (!catalog.TryGetValue(parts[0], out municipalities)) catalog[parts[0]] = municipalities = new List<string>();
                    municipalities.Add(parts[1]);
                }
            }
            return catalog;
        }

        public static IReadOnlyList<string> Departments => Catalog.Value.Keys.ToList();

        public static IReadOnlyList<string> Municipalities(string department)
        {
            List<string> municipalities;
            return department != null && Catalog.Value.TryGetValue(department, out municipalities) ? municipalities : new List<string>();
        }

        // Devuelve el nombre del catálogo que corresponde al texto (sin distinguir mayúsculas ni tildes) o null.
        public static string CanonicalDepartment(string text)
        {
            return Catalog.Value.Keys.FirstOrDefault(name => Same(name, text));
        }

        public static string CanonicalMunicipality(string department, string text)
        {
            return Municipalities(department).FirstOrDefault(name => Same(name, text));
        }

        private static bool Same(string catalogName, string text)
        {
            return !string.IsNullOrWhiteSpace(text)
                && string.Compare(catalogName, text.Trim(), CultureInfo.InvariantCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) == 0;
        }
    }
}
