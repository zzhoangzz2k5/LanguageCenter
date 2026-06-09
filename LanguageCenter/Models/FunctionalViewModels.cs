using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LanguageCenter.Models
{
    public class MaterialViewModel
    {
        public int ClassId { get; set; }
        public string FileName { get; set; }
        public string DisplayName { get; set; }
        public bool IsActive { get; set; }
        public DateTime UploadedAt { get; set; }
    }

    public class ProgramTypeViewModel
    {
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public int ProgramCount { get; set; }
    }

    public static class JsonMetadataStore
    {
        private static readonly object MaterialLock = new object();
        private static readonly object ProgramTypeLock = new object();

        public static List<MaterialViewModel> LoadMaterials(string path)
        {
            lock (MaterialLock)
            {
                return Load<MaterialViewModel>(path);
            }
        }

        public static void SaveMaterials(string path, IEnumerable<MaterialViewModel> items)
        {
            lock (MaterialLock)
            {
                Save(path, items);
            }
        }

        public static List<ProgramTypeViewModel> LoadProgramTypes(string path)
        {
            lock (ProgramTypeLock)
            {
                return Load<ProgramTypeViewModel>(path);
            }
        }

        public static void SaveProgramTypes(string path, IEnumerable<ProgramTypeViewModel> items)
        {
            lock (ProgramTypeLock)
            {
                Save(path, items);
            }
        }

        private static List<T> Load<T>(string path)
        {
            if (!File.Exists(path))
                return new List<T>();

            try
            {
                string json = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<List<T>>(json) ?? new List<T>();
            }
            catch (JsonException)
            {
                return new List<T>();
            }
        }

        private static void Save<T>(string path, IEnumerable<T> items)
        {
            string directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            string json = JsonConvert.SerializeObject(items.ToList(), Formatting.Indented);
            File.WriteAllText(path, json);
        }
    }
}
