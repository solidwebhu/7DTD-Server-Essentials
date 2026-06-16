using System;
using System.Collections.Generic;
using HarmonyLib;
using System.Reflection;
using UnityEngine;
using DiscordWebhook;
using OperenciaManager.Systems;

namespace QuestsLogger.Harmony
{
    public class H_NetPackageQuestEvent
    {
        [HarmonyPatch(typeof(NetPackageQuestEvent))]
        public class H_NetPackageQuestEvent_Setup
        {
            [HarmonyTargetMethods]
            static IEnumerable<MethodBase> FindMethods()
            {
                return AccessTools.GetDeclaredMethods(typeof(NetPackageQuestEvent)).FindAll(m => m.Name == "Setup");
            }

            [HarmonyPostfix]
            static void Setup(NetPackageQuestEvent __instance)
            {
                var player = GameManager.Instance.World.Players.dict[__instance.entityID];
                if (!player || player.prefab is null || player.prefab.prefab is null) return;
                var prefab = GameManager.Instance.World.GetPOIAtPosition(__instance.prefabPos);
                if (prefab is null) return;

                var message = $"[D30000]Operencia: [FFFFFF]{player.EntityName} questet indított: {prefab.prefab.LocalizedName} | {player.position} | Tier: {prefab.prefab.DifficultyTier}";
                Log.Warning(message.Substring(23, message.Length - 23));

                if (__instance.eventType == NetPackageQuestEvent.QuestEventTypes.RallyMarkerActivated)
                {
                    int difficultyTier = prefab.prefab.DifficultyTier;
                    GameManager.Instance.ChatMessageServer(null, EChatType.Global, -1, message, null, EMessageSender.None);
                    SendWebhook(player.EntityName, prefab.prefab.LocalizedName, player.position, difficultyTier, prefab.prefab.PrefabName);
                }
            }

            static void SendWebhook(string playerName, string poiName, Vector3 position, int difficultyTier, string prefabName)
            {
                var webhook = new DiscordWebhook.DiscordWebhook(DiscordHooks.Guard);

                // Kép URL összeállítása a prefabName alapján
                string imageUrl = $"https://7dtd.operencia.net/images/POI/{prefabName}.jpg";

                var embed = new DiscordEmbed
                {
                    Title = $"{playerName} questet indított",
                    Description = $"Helyszín: {poiName}\nPozíció: {position}\nPOI Tier: {difficultyTier}",
                    Image = new EmbedMedia
                    {
                        Url = imageUrl
                    }
                };

                var message = new DiscordMessage
                {
                    Embeds = new List<DiscordEmbed> { embed }
                };

                webhook.Send(message);
            }
        }
    }
}