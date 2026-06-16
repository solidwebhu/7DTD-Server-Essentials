using System.Collections.Generic;
using UnityEngine;

namespace OperenciaManager.Systems
{
    public class SanctuaryEntityWatcher : MonoBehaviour
    {
        private readonly HashSet<string> sanctuaryPOIs = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase)
        {
            "operencia_center",
            "playercity"
        };

        private readonly float checkInterval = 2f;
        private float nextCheckTime = 0f;

        void Update()
        {
            if (Time.time < nextCheckTime)
                return;

            nextCheckTime = Time.time + checkInterval;

            if (GameManager.Instance?.World == null)
                return;

            var entities = GameManager.Instance.World.Entities.list;
            if (entities == null || entities.Count == 0)
                return;

            List<Entity> toRemove = new List<Entity>();

            foreach (Entity entity in entities)
            {
                if (!(entity is EntityAlive alive))
                    continue;

                // Játékosokat SOHA nem törlünk
                if (alive is EntityPlayer)
                    continue;

                // Peteket SOHA nem törlünk
                if (PetRegistry.IsPet(alive))
                    continue;

                // Trader / NPC SOHA nem törlünk
                if (alive is EntityNPC)
                    continue;

                // Járműveket SOHA nem törlünk
                if (alive is EntityVehicle)
                    continue;

                // Drónt SOHA nem törlünk
                if (alive is EntityDrone)
                    continue;

                // Pozíció alapján POI meghatározása
                Vector3 pos = alive.GetPosition();
                PrefabInstance poi = GameManager.Instance.World.GetPOIAtPosition(pos);
                if (poi?.prefab?.PrefabName == null)
                    continue;

                string prefabName = poi.prefab.PrefabName;
                if (!sanctuaryPOIs.Contains(prefabName))
                    continue;

                // Ha Sanctuary POI-ban van → törlésre jelöljük
                toRemove.Add(alive);
            }

            // Entitások eltávolítása
            foreach (Entity entity in toRemove)
            {
                GameManager.Instance.World.RemoveEntity(entity.entityId, EnumRemoveEntityReason.Killed);

                Vector3 pos = entity.GetPosition();
                string prefabName = GameManager.Instance.World.GetPOIAtPosition(pos)?.prefab?.PrefabName ?? "ismeretlen";

                Log.Out($"[Sanctuary] Entitás eltávolítva a szentélyből: {entity.GetType().Name} @ {prefabName} @ {pos}");
            }
        }
    }
}
