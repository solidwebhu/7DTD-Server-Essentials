using System;
using OperenciaManager.Commands;
using OperenciaManager.Core;
using OperenciaManager.Systems;
using UnityEngine;
using static LightingAround;

public class HomeCommand : IChatCommand
{
    public string Name => "/home";

    public void Execute(ClientInfo clientInfo, string[] args)
    {
        if (clientInfo == null)
            return;

        // Játékos entitás lekérése
        if (!GameManager.Instance.World.Players.dict.TryGetValue(clientInfo.entityId, out EntityPlayer player) || player == null)
        {
            ChatHook.SendPrivate(clientInfo, "Nem találom a játékos entitást.");
            return;
        }

        // Mentés teleport előtt
        TeleportMemory.SavePosition(clientInfo.CrossplatformId.CombinedString, player.GetPosition());

        // Bedroll pozíció lekérése
        PersistentPlayerData data = GameManager.Instance.GetPersistentPlayerList().GetPlayerData(clientInfo.CrossplatformId);
        if (data == null || !data.HasBedrollPos)
        {
            ChatHook.SendPrivate(clientInfo, "Nincs lerakott ágyad (bedroll).");
            return;
        }

        Vector3 bedrollPos = data.BedrollPos;
        Vector3 teleportPos = new Vector3(bedrollPos.x, bedrollPos.y + 1f, bedrollPos.z);

        // Játékos teleportálása
        NetPackageTeleportPlayer package =
            NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(teleportPos, null, false);
        clientInfo.SendPackage(package);

        // PET TELEPORTÁLÁSA
        PetTeleportHelper.SafeTeleportPet(player, teleportPos);


        ChatHook.SendPrivate(clientInfo, $"Teleportálva az ágyadhoz: {teleportPos}");
    }
}
