using System;
using UnityEngine;
using OperenciaManager.Core;
using OperenciaManager.Systems;

namespace OperenciaManager.Commands
{
    public class TeleCommand : IChatCommand
    {
        public string Name => "/tele";

        public void Execute(ClientInfo clientInfo, string[] args)
        {
            if (args.Length < 1)
            {
                ChatHook.SendPrivate(clientInfo, "[FFFF00]Használat: /tele <név>[-]");
                return;
            }

            string label = args[0].Trim();
            string crossId = clientInfo.CrossplatformId.CombinedString;

            if (!TeleportStorage.TryGetTeleport(crossId, label, out Vector3i pos))
            {
                ChatHook.SendPrivate(clientInfo, $"[FF0000]Nincs ilyen mentett pozíciód: '{label}'[-]");
                return;
            }

            // Játékos entitás lekérése
            if (!GameManager.Instance.World.Players.dict.TryGetValue(clientInfo.entityId, out EntityPlayer player))
            {
                ChatHook.SendPrivate(clientInfo, "[FF0000]Nem található játékos entitás.[-]");
                return;
            }

            // Játékos teleportálása
            NetPackageTeleportPlayer package =
                NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(pos, null, false);
            clientInfo.SendPackage(package);

            // PET TELEPORTÁLÁSA (biztonságosan)
            PetTeleportHelper.SafeTeleportPet(player, pos);

            ChatHook.SendPrivate(clientInfo,
                $"[00FF00]Teleportálva ide: '{label}' → {pos}[-]");
        }
    }
}
