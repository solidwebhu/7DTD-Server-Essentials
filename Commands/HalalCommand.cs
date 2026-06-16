using OperenciaManager.Core;
using OperenciaManager.Systems;
using UnityEngine;

namespace OperenciaManager.Commands
{
    public class HalalCommand : IChatCommand
    {
        public string Name => "/halal";

        public void Execute(ClientInfo clientInfo, string[] args)
        {
            if (clientInfo == null)
                return;

            // Játékos entitás lekérése
            if (!GameManager.Instance.World.Players.dict.TryGetValue(clientInfo.entityId, out EntityPlayer player))
            {
                ChatHook.SendPrivate(clientInfo, "[FF0000]Nem található játékos entitás.[-]");
                return;
            }

            string userId = clientInfo.CrossplatformId.CombinedString;

            // Halálpozíció lekérése
            if (!TeleportMemory.TryGetLastPosition(userId, out Vector3 pos))
            {
                ChatHook.SendPrivate(clientInfo, "[FFFF00]Nincs elmentett halálpozíciód.[-]");
                return;
            }

            // Játékos teleportálása
            NetPackageTeleportPlayer package =
                NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(pos, null, false);
            clientInfo.SendPackage(package);

            //pet teleportálása
            PetTeleportHelper.SafeTeleportPet(player, pos);


            ChatHook.SendPrivate(clientInfo,
                $"[00FF00]Visszateleportálva az utolsó halálpozíciódra: {pos.x:F1}, {pos.y:F1}, {pos.z:F1}[-]");
        }
    }
}
