using System;
using System.Collections.Generic;
using UnityEngine;
using OperenciaManager.Core;

public class AntiCheatItemScanner : MonoBehaviour
{
    private readonly float checkInterval = 10f;
    private float timer = 0f;

    private static readonly HashSet<string> bannedItems = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "meleeToolPaintToolAdmin",
        "meleeToolSalvageWrenchAdmin",
        "meleeToolBlockReplaceTool",
        "gunToolDiggerAdmin",
        "gunHandgunPistolAdmin",
        "meleeToolHammerOfGodAdmin",
        "pimpMiningHelmetAdmin",
        "toughGuyShirtAdmin",
        "ringOfFireAdmin",
        "staminaBootsAdmin"
    };

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer < checkInterval) return;
        timer = 0f;

        World world = GameManager.Instance.World;
        foreach (var kvp in world.Players.dict)
        {
            EntityPlayer player = kvp.Value;
            if (player == null || player.IsDead()) continue;

            ClientInfo clientInfo = ConnectionManager.Instance.Clients.ForEntityId(player.entityId);
            if (clientInfo == null) continue;

            int level = GameManager.Instance.adminTools.Users.GetUserPermissionLevel(clientInfo.CrossplatformId);
            if (level <= 3) continue;

            ItemValue held = player.inventory.holdingItemItemValue;
            if (held.IsEmpty()) continue;

            string itemName = held.ItemClass.Name;
            if (!bannedItems.Contains(itemName)) continue;

            ChatHook.SendGlobal($"[FF0000]{player.EntityName} bannolva lett az Anti-Cheat által! | Oka: Tiltott eszközhasználat.");

            string reason = "Tiltott eszközhasználat";
            DateTime banUntil = DateTime.Now.AddYears(100);

            GameManager.Instance.adminTools.Blacklist.AddBan(clientInfo.playerName, clientInfo.PlatformId, banUntil, reason);

            // Kick 
            GameUtils.KickPlayerData kickData = new GameUtils.KickPlayerData(GameUtils.EKickReason.Banned, clientInfo.entityId, banUntil, reason);
            GameUtils.KickPlayerForClientInfo(clientInfo, kickData);
        }
    }
}
