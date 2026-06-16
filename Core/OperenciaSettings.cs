using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace OperenciaManager.Systems
{
    public static class OperenciaSettings
    {
        private static string FilePath
        {
            get
            {
                return Path.Combine(
                    GameIO.GetSaveGameDir(),
                    "OperenciaManager",
                    "settings.json"
                );
            }
        }

        private static Dictionary<string, bool> cached =
            new Dictionary<string, bool>();

        public static void Load()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath));

                if (!File.Exists(FilePath))
                {
                    cached = new Dictionary<string, bool>();
                    Save();
                    return;
                }

                cached = JsonConvert.DeserializeObject<Dictionary<string, bool>>(
                    File.ReadAllText(FilePath));

                if (cached == null)
                    cached = new Dictionary<string, bool>();
            }
            catch (Exception ex)
            {
                Log.Error("[OperenciaSettings] Load hiba: " + ex);
                cached = new Dictionary<string, bool>();
            }
        }

        public static void Save()
        {
            try
            {
                File.WriteAllText(
                    FilePath,
                    JsonConvert.SerializeObject(cached, Formatting.Indented));
            }
            catch (Exception ex)
            {
                Log.Error("[OperenciaSettings] Save hiba: " + ex);
            }
        }

        public static bool IsEnabled(string key)
        {
            bool v;
            if (cached.TryGetValue(key, out v))
                return v;

            return true; // default ON
        }

        public static void Set(string key, bool value)
        {
            cached[key] = value;
            Save();
        }

        public static Dictionary<string, bool> GetAll()
        {
            return new Dictionary<string, bool>(cached);
        }
    }
}
