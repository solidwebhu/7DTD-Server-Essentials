using System;
using UnityEngine;
using OperenciaManager.Core;
using OperenciaManager.Systems;

namespace OperenciaManager.Commands
{
    public class ClaimRemoveCommand : IChatCommand
    {
        public string Name => "/claimremove";

        public void Execute(ClientInfo clientInfo, string[] args)
        {
            // Admin jogosultság ellenőrzése
            int level = GameManager.Instance.adminTools.Users.GetUserPermissionLevel(clientInfo.CrossplatformId);
            if (level > 2)
            {
                ChatHook.SendPrivate(clientInfo, "[FF0000]Nincs jogosultságod ehhez a parancshoz.[-]");
                return;
            }

            // Játékos pozíció lekérése
            EntityPlayer player = GameManager.Instance.World.GetEntity(clientInfo.entityId) as EntityPlayer;
            if (player == null)
            {
                ChatHook.SendPrivate(clientInfo, "[FF0000]Nem található játékos entitás.[-]");
                return;
            }

            Vector3i position = new Vector3i(
                Mathf.FloorToInt(player.position.x),
                Mathf.FloorToInt(player.position.y),
                Mathf.FloorToInt(player.position.z)
            );

            // Claim blokk tulajdonos lekérdezése
            PersistentPlayerData owner = ClaimZoneResolver.GetClaimOwner(position);
            if (owner == null)
            {
                ChatHook.SendPrivate(clientInfo, "[FFFF00]Először menj a Claim tetejére, hogy törölni tudd.[-]");
                return;
            }

            // Claim blokkok lekérése
            var blocks = owner.GetLandProtectionBlocks();
            if (blocks == null || blocks.Count == 0)
            {
                ChatHook.SendPrivate(clientInfo, "[FFFF00]Nem található törölhető Claim blokk ezen a területen.[-]");
                return;
            }

            // Törlés: csak azokat, amelyek védik ezt a pozíciót
            int claimSize = (GameStats.GetInt(EnumGameStats.LandClaimSize) - 1) / 2;
            int removed = 0;

            foreach (var blockPos in blocks)
            {
                int dx = Mathf.Abs(blockPos.x - position.x);
                int dz = Mathf.Abs(blockPos.z - position.z);
                if (dx <= claimSize && dz <= claimSize)
                {
                    GameManager.Instance.World.SetBlockRPC(blockPos, BlockValue.Air);
                    removed++;
                }
            }

            if (removed > 0)
            {
                ChatHook.SendPrivate(clientInfo, $"[00FF00]{removed} Claim blokk eltávolítva a(z) {owner.PlayerName.SafeDisplayName} területéről.[-]");
                Log.Out($"[ClaimRemove] {removed} Claim blokk eltávolítva admin által: {clientInfo.playerName}, cél: {owner.PlayerName.SafeDisplayName}");
            }
            else
            {
                ChatHook.SendPrivate(clientInfo, "[FFFF00]Nem volt eltávolítható Claim blokk ezen a pozíción.[-]");
            }
        }
    }
}
