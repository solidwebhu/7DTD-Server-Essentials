using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

namespace OperenciaManager.Systems
{
    public static class VehicleMemoryStorage
    {
        private static readonly string BasePath =
            Path.Combine(GameIO.GetSaveGameDir(), "OperenciaManager", "VehicleMemory");

        private static readonly Dictionary<int, EntityVehicle> lastVehicle = new Dictionary<int, EntityVehicle>();

        public static void Init()
        {
            GameObject watcher = new GameObject("VehicleMemoryWatcher");
            watcher.AddComponent<VehicleMemoryBehaviour>();
            UnityEngine.Object.DontDestroyOnLoad(watcher);
            Log.Out("[VehicleMemoryStorage] VehicleMemoryWatcher aktiválva.");
        }

        private class VehicleMemoryBehaviour : MonoBehaviour
        {
            void Update()
            {
                if (GameManager.Instance == null || GameManager.Instance.World == null || GameManager.Instance.World.Players == null)
                    return;

                foreach (var kvp in GameManager.Instance.World.Players.dict)
                {
                    EntityPlayer player = kvp.Value;
                    if (player == null) continue;

                    int entityId = player.entityId;
                    EntityVehicle currentVehicle = player.AttachedToEntity as EntityVehicle;

                    if (lastVehicle.TryGetValue(entityId, out EntityVehicle previousVehicle))
                    {
                        if (previousVehicle != null && currentVehicle == null)
                        {
                            if (EntityClass.list == null || !EntityClass.list.ContainsKey(previousVehicle.entityClass))
                                continue;

                            string className = EntityClass.list[previousVehicle.entityClass].entityClassName;
                            Vector3 pos = previousVehicle.GetPosition();
                            Vector3i posInt = new Vector3i((int)pos.x, (int)pos.y, (int)pos.z);

                            if (ConnectionManager.Instance != null && ConnectionManager.Instance.Clients != null)
                            {
                                ClientInfo ci = ConnectionManager.Instance.Clients.ForEntityId(entityId);
                                if (ci != null && ci.CrossplatformId != null)
                                {
                                    SaveLastPosition(ci.CrossplatformId.CombinedString, className, posInt);
                                    Log.Out($"[VehicleMemoryStorage] Mentve: {className} @ {posInt} for {ci.CrossplatformId.CombinedString}");
                                }
                            }
                        }
                    }

                    lastVehicle[entityId] = currentVehicle;
                }
            }
        }

        public static void SaveLastPosition(string crossId, string vehicleClass, Vector3i position)
        {
            try
            {
                Directory.CreateDirectory(BasePath);
                string filePath = Path.Combine(BasePath, $"{crossId}.json");

                Dictionary<string, Vector3i> data = File.Exists(filePath)
                    ? JsonConvert.DeserializeObject<Dictionary<string, Vector3i>>(File.ReadAllText(filePath))
                    : new Dictionary<string, Vector3i>();

                data[vehicleClass] = position;
                File.WriteAllText(filePath, JsonConvert.SerializeObject(data, Formatting.Indented));
            }
            catch (Exception ex)
            {
                Log.Error($"[VehicleMemoryStorage] Mentési hiba: {ex}");
            }
        }

        public static bool TryGetLastPosition(string crossId, string vehicleClass, out Vector3i position)
        {
            position = default;
            try
            {
                string filePath = Path.Combine(BasePath, $"{crossId}.json");
                if (!File.Exists(filePath)) return false;

                var data = JsonConvert.DeserializeObject<Dictionary<string, Vector3i>>(File.ReadAllText(filePath));
                return data != null && data.TryGetValue(vehicleClass, out position);
            }
            catch (Exception ex)
            {
                Log.Error($"[VehicleMemoryStorage] Betöltési hiba: {ex}");
                return false;
            }
        }

        public static bool DeleteLastPosition(string crossId, string vehicleClass)
        {
            try
            {
                string filePath = Path.Combine(BasePath, $"{crossId}.json");
                if (!File.Exists(filePath)) return false;

                var data = JsonConvert.DeserializeObject<Dictionary<string, Vector3i>>(File.ReadAllText(filePath));
                if (data == null || !data.Remove(vehicleClass)) return false;

                File.WriteAllText(filePath, JsonConvert.SerializeObject(data, Formatting.Indented));
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"[VehicleMemoryStorage] Törlési hiba: {ex}");
                return false;
            }
        }

        public static List<string> ListStoredVehicles(string crossId)
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
                Log.Error($"[VehicleMemoryStorage] Listázási hiba: {ex}");
                return new List<string>();
            }
        }
    }
}
