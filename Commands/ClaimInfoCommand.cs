using System;
using UnityEngine;
using OperenciaManager.Core;
using OperenciaManager.Systems;

namespace OperenciaManager.Commands
{
    public class ClaimInfoCommand : IChatCommand
    {
        public string Name => "/claiminfo";

        public void Execute(ClientInfo clientInfo, string[] args)
        {
            int level = GameManager.Instance.adminTools.Users.GetUserPermissionLevel(clientInfo.CrossplatformId);
            if (level > 2)
            {
                ChatHook.SendPrivate(clientInfo, "[FF0000]Nincs jogosultságod ehhez a parancshoz.");
                return;
            }

            EntityPlayer player = GameManager.Instance.World.GetEntity(clientInfo.entityId) as EntityPlayer;
            if (player == null)
            {
                ChatHook.SendPrivate(clientInfo, "[FF0000]Nem található játékos entitás.");
                return;
            }

            Vector3i position = new Vector3i(
                Mathf.FloorToInt(player.position.x),
                Mathf.FloorToInt(player.position.y),
                Mathf.FloorToInt(player.position.z)
            );

            PersistentPlayerData playerData = GameManager.Instance.persistentPlayers.GetPlayerData(clientInfo.CrossplatformId);
            if (playerData == null)
            {
                ChatHook.SendPrivate(clientInfo, "[FF0000]Nem található játékos adat.");
                return;
            }

            EnumLandClaimOwner claimStatus = GameManager.Instance.World.GetLandClaimOwner(position, playerData);
            PersistentPlayerData owner = ClaimZoneResolver.GetClaimOwner(position);
            string ownerName = owner != null && owner.PlayerName != null
                ? owner.PlayerName.SafeDisplayName
                : "ismeretlen";

            ChatHook.SendPrivate(clientInfo, "[00FFFF]Pozíció: " + position);
            ChatHook.SendPrivate(clientInfo, "[00FFFF]Claim státusz: " + claimStatus);
            ChatHook.SendPrivate(clientInfo, "[00FFFF]Tulajdonos: " + ownerName);

            // ---- Serverconfig adatok ----

            int expiryDays = GamePrefs.GetInt(EnumGamePrefs.LandClaimExpiryTime);
            int offlineDelay = GamePrefs.GetInt(EnumGamePrefs.LandClaimOfflineDelay);
            int decayMode = GamePrefs.GetInt(EnumGamePrefs.LandClaimDecayMode);

            string decayText;
            if (decayMode == 0)
                decayText = "kikapcsolva";
            else if (decayMode == 1)
                decayText = "lassú";
            else if (decayMode == 2)
                decayText = "azonnali";
            else
                decayText = decayMode.ToString();

            ChatHook.SendPrivate(clientInfo, "[00FFFF]Claim expiry (server): " + expiryDays + " nap[-]");
            ChatHook.SendPrivate(clientInfo, "[00FFFF]Offline delay: " + offlineDelay + " óra[-]");
            ChatHook.SendPrivate(clientInfo, "[00FFFF]Decay mód: " + decayText + "[-]");
        }
    }
}
