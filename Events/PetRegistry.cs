using System.Collections.Generic;
using UnityEngine;

namespace OperenciaManager.Systems
{
    public enum PetState
    {
        Passive,
        Follow,
        Sit,
        Guard
    }

    public static class PetRegistry
    {
        private static readonly Dictionary<int, EntityAlive> petsByOwner = new Dictionary<int, EntityAlive>();
        private static readonly Dictionary<int, int> ownerByPet = new Dictionary<int, int>();
        private static readonly Dictionary<int, PetState> statesByPet = new Dictionary<int, PetState>();

        public static void Register(EntityAlive pet, EntityPlayer owner)
        {
            petsByOwner[owner.entityId] = pet;
            ownerByPet[pet.entityId] = owner.entityId;
            statesByPet[pet.entityId] = PetState.Follow;
        }

        public static void Remove(EntityAlive pet)
        {
            // -------------------------------
            // 1) Ha a pet null → registry takarítás
            // -------------------------------
            if (pet == null)
            {
                // petsByOwner tisztítása
                List<int> ownersToRemove = new List<int>();
                foreach (var kvp in petsByOwner)
                {
                    if (kvp.Value == null)
                        ownersToRemove.Add(kvp.Key);
                }
                foreach (int ownerKey in ownersToRemove)
                    petsByOwner.Remove(ownerKey);

                // ownerByPet + statesByPet tisztítása
                List<int> deadPets = new List<int>();
                foreach (var kvp in ownerByPet)
                {
                    int petId = kvp.Key;
                    EntityAlive e = GameManager.Instance.World.GetEntity(petId) as EntityAlive;
                    if (e == null)
                        deadPets.Add(petId);
                }
                foreach (int petId in deadPets)
                {
                    ownerByPet.Remove(petId);
                    statesByPet.Remove(petId);
                }

                return;
            }

            // -------------------------------
            // 2) Ha a pet létezik → normál törlés
            // -------------------------------
            statesByPet.Remove(pet.entityId);

            int ownerId;
            if (ownerByPet.TryGetValue(pet.entityId, out ownerId))
            {
                petsByOwner.Remove(ownerId);
                ownerByPet.Remove(pet.entityId);
            }
        }

        public static bool IsPet(EntityAlive entity)
        {
            if (entity == null)
                return false;

            return ownerByPet.ContainsKey(entity.entityId);
        }

        public static EntityPlayer GetOwner(EntityAlive pet)
        {
            if (pet == null)
                return null;

            int ownerId;
            if (!ownerByPet.TryGetValue(pet.entityId, out ownerId))
                return null;

            EntityPlayer owner;
            if (GameManager.Instance.World.Players.dict.TryGetValue(ownerId, out owner))
                return owner;

            return null;
        }

        public static bool HasPet(EntityPlayer owner)
        {
            if (owner == null)
                return false;

            return petsByOwner.ContainsKey(owner.entityId);
        }

        public static EntityAlive GetPet(EntityPlayer owner)
        {
            if (owner == null)
                return null;

            EntityAlive pet;
            petsByOwner.TryGetValue(owner.entityId, out pet);
            return pet;
        }

        public static void SetState(EntityAlive pet, PetState state)
        {
            if (pet == null)
                return;

            if (statesByPet.ContainsKey(pet.entityId))
                statesByPet[pet.entityId] = state;
        }

        public static void SetState(int ownerId, PetState state)
        {
            EntityAlive pet;
            if (petsByOwner.TryGetValue(ownerId, out pet))
                SetState(pet, state);
        }

        public static PetState GetState(EntityAlive pet)
        {
            if (pet == null)
                return PetState.Passive;

            PetState state;
            if (statesByPet.TryGetValue(pet.entityId, out state))
                return state;

            return PetState.Passive;
        }
    }
}
