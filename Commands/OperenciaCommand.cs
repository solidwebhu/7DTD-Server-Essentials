using System;
using UnityEngine;
using OperenciaManager.Core;
using OperenciaManager.Systems;
using static LightingAround;

namespace OperenciaManager.Commands
{
    public class OperenciaCommand : IChatCommand
    {
        public string Name => "/operencia";

        public void Execute(ClientInfo clientInfo, string[] args)
        {
            if (clientInfo == null) return;

            if (!GameManager.Instance.World.Players.dict.TryGetValue(clientInfo.entityId, out EntityPlayer player))
            {
                ChatHook.SendPrivate(clientInfo, "[FF0000]Nem található játékos entitás.[-]");
                return;
            }

            // Mentés teleport előtt
            TeleportMemory.SavePosition(clientInfo.CrossplatformId.CombinedString, player.GetPosition());

            // Célpozíció: Operencia szent helye
            Vector3 teleportPos = new Vector3(-4284f, 37f, 2065f);

            // Játékos teleportálása
            NetPackageTeleportPlayer package = NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(teleportPos, null, false);
            clientInfo.SendPackage(package);

            // PET TELEPORTÁLÁSA
            PetTeleportHelper.SafeTeleportPet(player, teleportPos);


            ChatHook.SendPrivate(clientInfo, "[00FF00]Megérkeztél a kezdőhelyre!");
            Log.Out($"[OperenciaCommand] {clientInfo.playerName} elteleportálva Operenciába: {teleportPos}");
        }
    }
}
