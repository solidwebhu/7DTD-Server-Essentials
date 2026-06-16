using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

namespace OperenciaManager.Systems
{
    public static class TeleportStorage
    {
        private static string BasePath =>
            Path.Combine(GameIO.GetSaveGameDir(), "OperenciaManager", "Teleports");

        public static void SaveTeleport(string crossId, string name, Vector3i position)
        {
            try
            {
                Directory.CreateDirectory(BasePath);
                string filePath = Path.Combine(BasePath, $"{crossId}.json");

                Dictionary<string, Vector3i> data = File.Exists(filePath)
                    ? JsonConvert.DeserializeObject<Dictionary<string, Vector3i>>(File.ReadAllText(filePath))
                    : new Dictionary<string, Vector3i>();

                data[name] = position;
                File.WriteAllText(filePath, JsonConvert.SerializeObject(data, Formatting.Indented));
            }
            catch (Exception ex)
            {
                Log.Error($"[TeleportStorage] Mentési hiba: {ex}");
            }
        }

        public static bool TryGetTeleport(string crossId, string name, out Vector3i position)
        {
            position = default;
            try
            {
                string filePath = Path.Combine(BasePath, $"{crossId}.json");
                if (!File.Exists(filePath)) return false;

                var data = JsonConvert.DeserializeObject<Dictionary<string, Vector3i>>(File.ReadAllText(filePath));
                return data != null && data.TryGetValue(name, out position);
            }
            catch (Exception ex)
            {
                Log.Error($"[TeleportStorage] Betöltési hiba: {ex}");
                return false;
            }
        }

        public static bool DeleteTeleport(string crossId, string name)
        {
            try
            {
                string filePath = Path.Combine(BasePath, $"{crossId}.json");
                if (!File.Exists(filePath)) return false;

                var data = JsonConvert.DeserializeObject<Dictionary<string, Vector3i>>(File.ReadAllText(filePath));
                if (data == null || !data.Remove(name)) return false;

                File.WriteAllText(filePath, JsonConvert.SerializeObject(data, Formatting.Indented));
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"[TeleportStorage] Törlési hiba: {ex}");
                return false;
            }
        }

        public static List<string> ListTeleports(string crossId)
        {
            try
            {
                string filePath = Path.Combine(BasePath, $"{crossId}.json");
                if (!File.Exists(filePath)) return new List<string>();

                var data = JsonConvert.DeserializeObject<Dictionary<string, Vector3i>>(File.ReadAllText(filePath));
                return data?.Keys != null ? new List<string>(data.Keys) : new List<string>();
            }
            catch (Exception ex)
            {
                Log.Error($"[TeleportStorage] Listázási hiba: {ex}");
                return new List<string>();
            }
        }


    }
}
