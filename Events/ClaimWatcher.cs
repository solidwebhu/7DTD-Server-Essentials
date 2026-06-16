using OperenciaManager.Comms;
using OperenciaManager.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace OperenciaManager.Systems
{
    public class ClaimWatcher : MonoBehaviour
    {
        private Dictionary<int, Vector3i> lastKnownPositions = new Dictionary<int, Vector3i>();
        private Dictionary<int, bool> isInForeignClaim = new Dictionary<int, bool>();
        private Dictionary<int, bool> wasInOwnClaim = new Dictionary<int, bool>();

        // globális cooldown a wipe + discord spam ellen
        private DateTime lastWipe = DateTime.MinValue;

        private float checkInterval = 1.5f;
        private float nextCheckTime = 0f;

        void Update()
        {
            try
            {
                if (Time.time < nextCheckTime) return;
                nextCheckTime = Time.time + checkInterval;

                if (GameManager.Instance == null ||
                    GameManager.Instance.World == null ||
                    GameManager.Instance.persistentPlayers == null)
                    return;

                List<EntityPlayer> players = GameManager.Instance.World.GetPlayers();
                if (players == null || players.Count == 0)
                    return;

                var persistentPlayers = GameManager.Instance.persistentPlayers;
                WorldBase world = GameManager.Instance.World;

                foreach (EntityPlayer player in players)
                {
                    int entityId = player.entityId;

                    Vector3i pos = new Vector3i(
                        Mathf.FloorToInt(player.position.x),
                        Mathf.FloorToInt(player.position.y),
                        Mathf.FloorToInt(player.position.z)
                    );

                    Vector3i lp;
                    if (lastKnownPositions.TryGetValue(entityId, out lp) && lp == pos)
                        continue;

                    lastKnownPositions[entityId] = pos;

                    ClientInfo ci = ConnectionManager.Instance.Clients.ForEntityId(entityId);
                    if (ci == null) continue;

                    PersistentPlayerData playerData = persistentPlayers.GetPlayerData(ci.CrossplatformId);
                    if (playerData == null) continue;

                    EnumLandClaimOwner claimStatus =
                        world.GetLandClaimOwner(pos, playerData);

                    bool pf;
                    bool prevForeign = isInForeignClaim.TryGetValue(entityId, out pf) && pf;
                    bool nowForeign = claimStatus == EnumLandClaimOwner.Other;

                    PersistentPlayerData owner = ClaimZoneResolver.GetClaimOwner(pos);

                    // =================================================
                    // LEJÁRT CLAIM – mindig ellenőrizzük (simple core)
                    // =================================================
                    if (owner != null)
                    {
                        HandleExpiredClaim(owner, world, pos);
                    }

                    // ---------------- IDEGEN CLAIM ----------------
                    if (nowForeign && !prevForeign)
                    {
                        isInForeignClaim[entityId] = true;
                        ClaimAccessRegistry.SetForeignClaim(entityId, true);

                        string ownerName =
                            owner != null && owner.PlayerName != null
                                ? owner.PlayerName.SafeDisplayName
                                : "ismeretlen játékos";

                        string intruderName = playerData.PlayerName.SafeDisplayName;

                        string expiryText = "";
                        string discordExpiry = "";

                        try
                        {
                            if (owner != null)
                            {
                                int expiryDays = GamePrefs.GetInt(EnumGamePrefs.LandClaimExpiryTime);
                                int offlineDelayHours = GamePrefs.GetInt(EnumGamePrefs.LandClaimOfflineDelay);
                                int decayMode = GamePrefs.GetInt(EnumGamePrefs.LandClaimDecayMode);

                                DateTime estimated =
                                    owner.LastLogin.AddHours(offlineDelayHours).AddDays(expiryDays);

                                TimeSpan left = estimated - DateTime.UtcNow;

                                string decayText =
                                    decayMode == 0 ? "Lineáris (lassú)" :
                                    decayMode == 1 ? "Exponenciális (gyors)" :
                                    decayMode == 2 ? "Nincs (teljes védelem lejáratig)" :
                                    decayMode.ToString();

                                if (DateTime.UtcNow < estimated)
                                {
                                    expiryText =
                                        "\n[00FFFF]Claim lejár: " +
                                        estimated.ToString("yyyy-MM-dd HH:mm") +
                                        " (~" + Math.Ceiling(left.TotalHours) + " óra)\n" +
                                        "[00FFFF]Amortizáció (Decay): " + decayText + "[-]";

                                    discordExpiry =
                                        "\n⏳ Lejárat: `" + estimated.ToString("yyyy-MM-dd HH:mm") + "`" +
                                        "\n⌛ Hátralévő: `" + Math.Ceiling(left.TotalHours) + " óra`" +
                                        "\n🧩 Decay: `" + decayText + "`";
                                }
                            }
                        }
                        catch { }

                        ChatHook.SendPrivate(ci,
                            "[FFFFFF]" + ownerName + " [FF0000]területén tartózkodsz. Tilos a lopás!" +
                            expiryText);

                        DiscordNotifier.Send(
                            DiscordHooks.Guard,
                            "🛡️ Operencia Guard",
                            "**" + intruderName + "** idegen területen.\n" +
                            "🏠 Tulaj: **" + ownerName + "**\n" +
                            "📍 Pozíció: `" + pos + "`" +
                            discordExpiry,
                            "https://7daystodie.wiki.gg/images/LandClaimBlock.png"
                        );
                    }
                    else if (!nowForeign && prevForeign)
                    {
                        isInForeignClaim[entityId] = false;
                        ClaimAccessRegistry.SetForeignClaim(entityId, false);
                    }

                    // ---------------- SAJÁT CLAIM ----------------
                    bool nowOwn = false;

                    if (owner != null &&
                        owner.PlayerData != null &&
                        playerData.PlayerData != null &&
                        owner.PlayerData.PrimaryId == playerData.PlayerData.PrimaryId)
                    {
                        nowOwn = true;
                    }

                    bool po;
                    bool prevOwn = wasInOwnClaim.TryGetValue(entityId, out po) && po;

                    if (nowOwn && !prevOwn)
                    {
                        bool enabled =
                            ClaimRepairState.IsEnabled(ci.CrossplatformId.CombinedString);

                        if (enabled)
                            ChatHook.SendPrivate(ci, "[00FF00]Üdv itthon! Auto repair AKTÍV! (/javitas)");
                        else
                            ChatHook.SendPrivate(ci, "[FF0000]Üdv itthon! Auto repair INAKTÍV! (/javitas)");
                    }

                    wasInOwnClaim[entityId] = nowOwn;
                }
            }
            catch (Exception ex)
            {
                Log.Error("[ClaimWatcher] Hiba: " + ex);
            }
        }

        // ------------------------------------------------------
        // LEJÁRT CLAIM TELJES TÖRLÉS + DISCORD (simple core)
        // ------------------------------------------------------
        private void HandleExpiredClaim(PersistentPlayerData owner, WorldBase world, Vector3i triggerPos)
        {
            try
            {
                int expiryDays = GamePrefs.GetInt(EnumGamePrefs.LandClaimExpiryTime);
                int offlineDelayHours = GamePrefs.GetInt(EnumGamePrefs.LandClaimOfflineDelay);

                DateTime estimated =
                    owner.LastLogin.AddHours(offlineDelayHours).AddDays(expiryDays);

                if (DateTime.UtcNow < estimated)
                    return;

                // globális cooldown (30 mp)
                if ((DateTime.UtcNow - lastWipe).TotalSeconds < 30)
                    return;

                lastWipe = DateTime.UtcNow;

                List<Vector3i> blocks =
                    new List<Vector3i>(owner.GetLandProtectionBlocks());

                foreach (Vector3i lcb in blocks)
                {
                    try
                    {
                        BlockValue bv = world.GetBlock(0, lcb);

                        if (bv.isair)
                            continue;

                        bv.Block.DamageBlock(
                            world,
                            0,
                            lcb,
                            bv,
                            int.MaxValue,
                            -1,
                            null,
                            false,
                            true
                        );
                    }
                    catch (Exception e)
                    {
                        Log.Error("[ClaimWatcher] LCB törlés hiba: " + e);
                    }
                }

                string ownerName =
                    owner.PlayerName != null ? owner.PlayerName.SafeDisplayName : "ismeretlen";

                int chunkX = triggerPos.x >> 4;
                int chunkZ = triggerPos.z >> 4;

                DiscordNotifier.Send(
                    DiscordHooks.Guard,
                    "🔥 Claim automatikusan törölve",
                    "🏠 Tulaj: **" + ownerName + "**\n" +
                    "📍 Pozíció: `" + triggerPos + "`\n" +
                    "🧭 Chunk: `" + chunkX + "," + chunkZ + "`\n" +
                    "⏰ Lejárt: `" + estimated.ToString("yyyy-MM-dd HH:mm") + "`",
                    "https://7daystodie.wiki.gg/images/LandClaimBlock.png"
                );

                ChatHook.SendGlobal("Lejárt egy Claimelt terület" + triggerPos + " pozíción, így befoglalható. | (Tulaj:" + ownerName +")");
                Log.Out("[ClaimWatcher] Lejárt claim törölve: " + ownerName);
            }
            catch (Exception e)
            {
                Log.Error("[ClaimWatcher] Expiry hiba: " + e);
            }
        }
    }

    public static class ClaimAccessRegistry
    {
        private static readonly Dictionary<int, bool> foreignClaimAccess =
            new Dictionary<int, bool>();

        public static void SetForeignClaim(int entityId, bool inForeignClaim)
        {
            foreignClaimAccess[entityId] = inForeignClaim;
        }

        public static bool IsInForeignClaim(int entityId)
        {
            bool val;
            return foreignClaimAccess.TryGetValue(entityId, out val) && val;
        }
    }
}
