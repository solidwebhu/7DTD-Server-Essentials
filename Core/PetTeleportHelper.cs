using UnityEngine;

namespace OperenciaManager.Systems
{
    public static class PetTeleportHelper
    {
        public static void SafeTeleportPet(EntityPlayer owner, Vector3 targetPos)
        {
            if (owner == null)
                return;

            EntityAlive pet = PetRegistry.GetPet(owner);
            if (pet == null)
                return;

            try
            {
                // 1) Eltávolítás a világból
                GameManager.Instance.World.RemoveEntity(pet.entityId, EnumRemoveEntityReason.Despawned);

                // 2) Új pet spawnolása
                int classId = pet.entityClass;
                Vector3 spawnPos = targetPos + new Vector3(1f, 0f, 1f);

                EntityAlive newPet = EntityFactory.CreateEntity(classId, spawnPos) as EntityAlive;
                GameManager.Instance.World.SpawnEntityInWorld(newPet);

                // 3) Registry frissítése
                PetRegistry.Remove(pet);
                PetRegistry.Register(newPet, owner);
                PetRegistry.SetState(newPet, PetState.Follow);
                PetFollowerSystem.StartFollowing(newPet, owner);

                Log.Out($"[PetTeleportHelper] Pet újraspawnolva teleport után: {newPet.entityId}");
            }
            catch (System.Exception ex)
            {
                Log.Error($"[PetTeleportHelper] Hiba a pet teleportálásakor: {ex}");
            }
        }
    }
}
