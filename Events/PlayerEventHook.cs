using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using OperenciaManager.Core;
using OperenciaManager.Comms;

namespace OperenciaManager.Systems
{
    public static class PlayerEventHooks
    {
        private static readonly HashSet<int> knownDead = new HashSet<int>();

        private const string DiscordWebhookUrl =
            DiscordHooks.ServerLog;

        public static void Init()
        {
            ModEvents.GameUpdate.RegisterHandler(
                new ModEvents.ModEventHandlerDelegate<ModEvents.SGameUpdateData>(Tick)
            );

            ModEvents.PlayerSpawnedInWorld.RegisterHandler(
                new ModEvents.ModEventHandlerDelegate<ModEvents.SPlayerSpawnedInWorldData>(OnPlayerSpawned)
            );

            ModEvents.PlayerDisconnected.RegisterHandler(
                new ModEvents.ModEventHandlerDelegate<ModEvents.SPlayerDisconnectedData>(OnPlayerDisconnected)
            );

            ModEvents.PlayerLogin.RegisterHandler(
                new ModEvents.ModEventInterruptibleHandlerDelegate<ModEvents.SPlayerLoginData>(OnPlayerLogin)
            );


            ModEvents.EntityKilled.RegisterHandler(
                new ModEvents.ModEventHandlerDelegate<ModEvents.SEntityKilledData>(OnEntityKilled)
            );


            Log.Out("[OperenciaManager] PlayerEventHooks inicializálva.");
        }

        private static void Tick(ref ModEvents.SGameUpdateData data)
        {
            // Halál pozíció mentése
            foreach (var entity in GameManager.Instance.World.Players.dict.Values)
            {
                if (entity == null || entity.IsAlive()) continue;

                if (!knownDead.Contains(entity.entityId))
                {
                    ClientInfo ci = ConnectionManager.Instance.Clients.ForEntityId(entity.entityId);
                    if (ci == null) continue;

                    string crossId = ci.CrossplatformId.CombinedString;
                    Vector3 pos = entity.position;

                    TeleportMemory.SavePosition(crossId, pos);
                    knownDead.Add(entity.entityId);
                }
            }

            // Tisztítás, ha újra él
            foreach (var id in new List<int>(knownDead))
            {
                if (GameManager.Instance.World.Players.dict.TryGetValue(id, out var player) && player.IsAlive())
                {
                    knownDead.Remove(id);
                }
            }
        }



        private static ModEvents.EModEventResult OnPlayerLogin(ref ModEvents.SPlayerLoginData data)
        {
            ClientInfo ci = data.ClientInfo;
            if (ci == null)
                return ModEvents.EModEventResult.Continue;

            string name = ci.playerName;
            string ip = ci.ip;

            ChatHook.SendGlobal($"{name} most próbál csatlakozni a szerverre...");

          

            return ModEvents.EModEventResult.Continue;
        }




        private static void OnPlayerSpawned(ref ModEvents.SPlayerSpawnedInWorldData data)
        {
            ClientInfo ci = data.ClientInfo;
            if (ci == null) return;
            if (data.RespawnType != RespawnType.JoinMultiplayer) return;

            // Profil mentés (FirstSeen/LastSeen, SteamID, IP)
            PlayerProfileStorage.SaveProfile(ci);

            string name = ci.playerName;

            if (!GameManager.Instance.World.Players.dict.TryGetValue(ci.entityId, out EntityPlayer player))
                return;

            int level = player.Progression?.GetLevel() ?? 0;
            int zombieKills = player.KilledZombies;
            int deaths = player.Died;

            // Összes játékidő (másodpercből)
            TimeSpan totalPlay = TimeSpan.FromSeconds(player.totalTimePlayed);
            string playtimeStr = $"{(int)totalPlay.TotalHours} óra {totalPlay.Minutes} perc";

            // Chat értesítés
            ChatHook.SendGlobal(
                $"{name} csatlakozott a szerverre. Szint: [ffff00]{level} [FFFFFF] | Zombi ölések: [ff0000]{zombieKills} [FFFFFF]"
            );

            // Aszinkron kiegészítő infók (lokáció, First/LastSeen) + Discord értesítés
            _ = Task.Run(async () =>
            {
                try
                {
                    
                  

                    // Profil betöltése
                    var profile = PlayerProfileStorage.LoadProfile(ci.CrossplatformId?.CombinedString);

                    string firstSeenStr = profile?.FirstSeen.ToString("yyyy-MM-dd HH:mm") ?? "ismeretlen";
                    string lastSeenStr = profile?.LastSeen.ToString("yyyy-MM-dd HH:mm") ?? "ismeretlen";

                    string description =
                        $"**{name}** csatlakozott a szerverre.\n" +
                        $"🎮 Szint: {level}\n" +
                        $"🧟 Zombi ölések: {zombieKills}\n" +
                        $"💀 Halálok: {deaths}\n" +
                        $"⏱️ Playtime: {playtimeStr}\n" +
                        $"📅 Utolsó belépés: {lastSeenStr}\n";
                      

                    DiscordNotifier.Send(DiscordWebhookUrl, "👤 Játékos csatlakozott", description);
                }
                catch (Exception ex)
                {
                    Log.Error($"[PlayerEventHooks] DiscordNotifier.Send kivétel (join): {ex.Message}");
                }
            });
        }

        private static void OnEntityKilled(ref ModEvents.SEntityKilledData data)
        {
            Entity killed = data.KilledEntitiy;

            if (killed is EntityPlayer player)
            {
                ClientInfo ci = ConnectionManager.Instance.Clients.ForEntityId(player.entityId);
                if (ci == null) return;

                string name = ci.playerName;
                string description = $"**{name}** meghalt.";

                ChatHook.SendGlobal($"[ff0000]{name} meghalt.");
                try
                {
                    DiscordNotifier.Send(
                        DiscordWebhookUrl,
                        "💀 Játékos halál",
                        description
                    );
                }
                catch (Exception ex)
                {
                    Log.Error($"[PlayerEventHooks] DiscordNotifier.Send kivétel (halál): {ex.Message}");
                }
            }
        }

        private static void OnPlayerDisconnected(ref ModEvents.SPlayerDisconnectedData data)
        {
            ClientInfo ci = data.ClientInfo;
            if (ci == null) return;

            string crossId = ci.CrossplatformId.CombinedString;

            if (!GameManager.Instance.World.Players.dict.TryGetValue(ci.entityId, out EntityPlayer player))
                return;

            // Pozíció mentés kilépéskor
            TeleportMemory.SavePosition(crossId, player.position);

            string name = ci.playerName;
            int level = player.Progression?.GetLevel() ?? 0;
            int zombieKills = player.KilledZombies;
            int deaths = player.Died;

            // Playtime
            TimeSpan totalPlay = TimeSpan.FromSeconds(player.totalTimePlayed);
            string playtimeStr = $"{(int)totalPlay.TotalHours} óra {totalPlay.Minutes} perc";

            ChatHook.SendGlobal($"{name} lecsatlakozott a szerverről.");

            // Aszinkron kiegészítő infók + Discord értesítés
            _ = Task.Run(async () =>
            {
                try
                {
                   
                    var profile = PlayerProfileStorage.LoadProfile(ci.CrossplatformId?.CombinedString);

                    string firstSeenStr = profile?.FirstSeen.ToString("yyyy-MM-dd HH:mm") ?? "ismeretlen";
                    string lastSeenStr = profile?.LastSeen.ToString("yyyy-MM-dd HH:mm") ?? "ismeretlen";

                    string description =
                        $"**{name}** lecsatlakozott a szerverről.\n" +
                        $"🎮 Szint: {level}\n" +
                        $"🧟 Zombi ölések: {zombieKills}\n" +
                        $"💀 Halálok: {deaths}\n" +
                        $"⏱️ Playtime: {playtimeStr}\n" +
                        $"📅 Utolsó belépés: {lastSeenStr}\n";
          

                    DiscordNotifier.Send(
                        DiscordWebhookUrl,
                        "🚪 Játékos kilépett",
                        description
                    );
                }
                catch (Exception ex)
                {
                    Log.Error($"[PlayerEventHooks] DiscordNotifier.Send kivétel (disconnect): {ex.Message}");
                }
            });
        }
    }
}
