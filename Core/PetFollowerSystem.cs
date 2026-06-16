using UnityEngine;

namespace OperenciaManager.Systems
{
    public static class PetFollowerSystem
    {
        public static void StartFollowing(EntityAlive pet, EntityPlayer owner)
        {
            Log.Out("[PetFollowerSystem] Pet követés indult: " + pet.entityId + " → owner " + owner.entityId);
        }

        public static void StopFollowing(EntityAlive pet)
        {
            Log.Out("[PetFollowerSystem] Pet követés leállt: " + pet.entityId);
        }

        public static void Update(EntityAlive pet)
        {
            if (!PetRegistry.IsPet(pet)) return;

            PetState state = PetRegistry.GetState(pet);
            EntityPlayer owner = PetRegistry.GetOwner(pet);
            if (owner == null) return;

            switch (state)
            {
                case PetState.Follow:
                case PetState.Passive:
                    FollowOwner(pet, owner);
                    break;

                case PetState.Sit:
                    SitDown(pet);
                    break;

                case PetState.Guard:
                    FollowOwner(pet, owner);
                    break;
            }

       
        }


        private static void FollowOwner(EntityAlive pet, EntityPlayer owner)
        {
            Vector3 ownerPos = owner.GetPosition();

            // Stabil oldalsó offset (nem a transform.right minden frame-ben!)
            Vector3 sideOffset = new Vector3(1.8f, 0f, 1.2f);
            Vector3 targetPos = ownerPos + owner.transform.TransformDirection(sideOffset);

            float distance = Vector3.Distance(pet.GetPosition(), targetPos);

            // Ha túl messze → teleport
            if (distance > 25f)
            {
                pet.SetPosition(targetPos);
                


                return;
            }

            // Deadzone: ha 2 méteren belül → ne mozogjon
            if (distance < 2f)
            {
                if (pet.moveHelper != null)
                    pet.moveHelper.Stop();
                return;
            }

            // Csak akkor adj új mozgási parancsot, ha tényleg kell
            if (pet.moveHelper != null)
            {
                pet.moveHelper.SetMoveTo(targetPos, false);
            }
        }


        private static void SitDown(EntityAlive pet)
        {
            if (pet.moveHelper != null) pet.moveHelper.Stop();
            if (pet.navigator != null) pet.navigator.clearPath();

            var animal = pet as EntityEnemyAnimal;
            if (animal != null && animal.animator != null)
            {
                int sitHash = Animator.StringToHash("SleeperIdleSit");
                animal.animator.SetTrigger(sitHash);
                Log.Out("[PetFollowerSystem] Pet ülés animáció triggerelve: " + pet.entityId);
            }
        }

        public static void TeleportToOwner(int ownerId)
        {
            EntityPlayer owner;
            if (!GameManager.Instance.World.Players.dict.TryGetValue(ownerId, out owner) || owner == null) return;

            foreach (Entity entity in GameManager.Instance.World.Entities.list)
            {
                EntityAlive pet = entity as EntityAlive;
                if (pet == null) continue;
                if (!PetRegistry.IsPet(pet)) continue;

                EntityPlayer petOwner = PetRegistry.GetOwner(pet);
                if (petOwner != null && petOwner.entityId == ownerId)
                {
                    pet.SetPosition(owner.GetPosition() + owner.transform.right * 1.5f);
                    Log.Out("[PetFollowerSystem] Pet teleportálva a gazdához: " + pet.entityId);
                }
            }
        }
    }
}
