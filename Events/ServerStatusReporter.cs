using OperenciaManager.Comms;
using UnityEngine;
using System;

namespace OperenciaManager.Systems
{
    public static class ServerStatusReporter
    {
        private const string DiscordWebhookUrl = DiscordHooks.ServerLog;
        private static bool hasAnnouncedStartup = false;
        private static bool hasAnnouncedShutdown = false;

        public static void Init()
        {
            ModEvents.GameUpdate.RegisterHandler(
                new ModEvents.ModEventHandlerDelegate<ModEvents.SGameUpdateData>(Tick)
            );

            Log.Out("[OperenciaManager] ServerStatusReporter inicializálva.");
        }

        public static void Tick(ref ModEvents.SGameUpdateData data)
        {
            bool started = GameManager.Instance?.GameHasStarted == true;

            if (!hasAnnouncedStartup && started)
            {
                hasAnnouncedStartup = true;
                hasAnnouncedShutdown = false;

                DiscordNotifier.Send(
                    DiscordWebhookUrl,
                    "✅ Szerver elindult",
                    $"A szerver elindult, készen áll a csatlakozásra: {GamePrefs.GetString(EnumGamePrefs.ServerName)}:{GamePrefs.GetInt(EnumGamePrefs.ServerPort)}"
                );
            }

            if (!hasAnnouncedShutdown && !started)
            {
                hasAnnouncedShutdown = true;
                hasAnnouncedStartup = false;

                DiscordNotifier.Send(
                    DiscordWebhookUrl,
                    "🛑 Szerver leállt",
                    $"A szerver leállt vagy újraindítás alatt van: {GamePrefs.GetString(EnumGamePrefs.ServerName)}"
                );
            }
        }
    }
}
