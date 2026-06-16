using System.Linq;
using OperenciaManager.Core;
using OperenciaManager.Systems;

namespace OperenciaManager.Systems
{
    public static class PetDeathHandler
    {
        public static void Init()
        {
            // Helyes delegate: ref SEntityKilledData
            ModEvents.EntityKilled.RegisterHandler(OnEntityKilled);
        }

        private static void OnEntityKilled(ref ModEvents.SEntityKilledData data)
        {
            // A megölt entitás lehet csak EntityAlive
            var pet = data.KilledEntitiy as EntityAlive;
            if (pet == null) return;
            if (!PetRegistry.IsPet(pet)) return;

            // Törlés a registryből
            PetRegistry.Remove(pet);

            // Gazda felderítése és értesítése (ha online)
            var owner = PetRegistry.GetOwner(pet);
            var cInfo = GetClientInfoForOwner(owner);
            if (cInfo != null)
            {
                ChatHook.SendPrivate(cInfo, "[FF0000]A háziállatod meghalt...[-]");
            }

            Log.Out($"[PetDeathHandler] Pet {pet.entityId} meghalt, törölve a gazda kapcsolatából.");
        }

        // Fallback: végigmegyünk az online klienseken és entityId alapján keresünk
        private static ClientInfo GetClientInfoForOwner(EntityPlayer owner)
        {
            if (owner == null) return null;

            // Clients.List lehet IEnumerable<ClientInfo> vagy lista — mindkettőre jó
            foreach (var cInfo in ConnectionManager.Instance.Clients.List)
            {
                if (cInfo != null && cInfo.entityId == owner.entityId)
                    return cInfo;
            }

            return null;
        }
    }
}
