using System;
using UnityEngine;
using OperenciaManager.Core;
using OperenciaManager.Systems;
using static LightingAround;

namespace OperenciaManager.Commands
{
    public class ReturnCommand : IChatCommand
    {
        public string Name => "/return";

        public void Execute(ClientInfo clientInfo, string[] args)
        {
            // Admin jogosultság ellenőrzése
            int level = GameManager.Instance.adminTools.Users.GetUserPermissionLevel(clientInfo.CrossplatformId);
            if (level > 2)
            {
                ChatHook.SendPrivate(clientInfo, "[FF0000]Nincs jogosultságod ehhez a parancshoz.[-]");
                return;
            }

            // Játékos entitás lekérése
            if (!GameManager.Instance.World.Players.dict.TryGetValue(clientInfo.entityId, out EntityPlayer player))
            {
                ChatHook.SendPrivate(clientInfo, "[FF0000]Nem található játékos entitás.[-]");
                return;
            }

            string userId = clientInfo.CrossplatformId.CombinedString;

            // Előző pozíció lekérése
            if (!TeleportMemory.TryGetLastPosition(userId, out Vector3 lastPos))
            {
                ChatHook.SendPrivate(clientInfo, "[FFFF00]Nincs elmentett előző pozíciód. Használj előtte egy teleport parancsot.[-]");
                return;
            }

            // Játékos teleportálása
            NetPackageTeleportPlayer package =
                NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(lastPos, null, false);
            clientInfo.SendPackage(package);

            // PET TELEPORTÁLÁSA
            PetTeleportHelper.SafeTeleportPet(player, lastPos);


            ChatHook.SendPrivate(clientInfo, $"[00FF00]Visszateleportálva az előző pozíciódra: {lastPos}[-]");

            // Pozíció törlése
            TeleportMemory.ClearPosition(userId);
        }
    }
}
