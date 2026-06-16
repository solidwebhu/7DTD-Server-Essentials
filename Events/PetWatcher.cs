using System.Collections.Generic;
using UnityEngine;

namespace OperenciaManager.Systems
{
    public class PetWatcher : MonoBehaviour
    {
        private float checkInterval = 0.2f; 
        private float nextCheckTime = 0f;

        void Update()
        {
            if (Time.time < nextCheckTime) return;
            nextCheckTime = Time.time + checkInterval;

            if (GameManager.Instance == null || GameManager.Instance.World == null) return;

            List<Entity> entities = GameManager.Instance.World.Entities.list;
            if (entities == null || entities.Count == 0) return;

            foreach (Entity entity in entities)
            {
                EntityAlive pet = entity as EntityAlive;
                if (pet == null) continue;

                if (!PetRegistry.IsPet(pet)) continue;

                PetFollowerSystem.Update(pet);
            }
        }
    }
}
