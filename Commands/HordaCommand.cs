using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OperenciaManager.Core;
using OperenciaManager.Systems;

namespace OperenciaManager.Commands
{
    public class HordaCommand : IChatCommand
    {
        public string Name => "/horda";

        private readonly List<string> hordeEvents = new List<string>
        {
            "vote_horde_s_crawler",
            "vote_horde_s_party_girl",
            "vote_horde_s_business",
            "vote_horde_s_random",
            "vote_horde_s_nurse",
            "vote_horde_s_janitor",
            "vote_horde_s_inmate"
        };

        public void Execute(ClientInfo clientInfo, string[] args)
        {
            string userId = clientInfo.CrossplatformId.CombinedString;
            EntityPlayer player = GameManager.Instance.World.GetEntity(clientInfo.entityId) as EntityPlayer;
            if (player == null)
            {
                ChatHook.SendPrivate(clientInfo, "[FF0000]Nem található játékos entitás.[-]");
                return;
            }
            // ⏳ Cooldown ellenőrzés (600 másodperc)
            if (CommandCooldownManager.IsOnCooldown("/horda", userId, 600, out int remaining))
            {
                ChatHook.SendPrivate(clientInfo, $"[FF0000]A horda még nem hívható újra. Várj {remaining} másodpercet!");
                return;
            }

            GameManager.Instance.StartCoroutine(TriggerHordeWaves(player, clientInfo));
        }

        private IEnumerator TriggerHordeWaves(EntityPlayer player, ClientInfo ci)
        {
            ChatHook.SendPrivate(ci, "[FF9900]A horda itt van! 5 horda hullám vár rád!");
            Log.Out($"[Horda] Rituális horda hullám indul {player.EntityName} játékosra.");

            int waveCount = 5;
            for (int i = 1; i <= waveCount; i++)
            {
                string eventName = hordeEvents[UnityEngine.Random.Range(0, hordeEvents.Count)];
                ChatHook.SendPrivate(ci, $"[FF9900]Horda hullám {i}/5");
                Log.Out($"[Horda] Hullám {i}: Twitch event aktiválva → {eventName}");

                bool success = GameEventManager.Current.HandleAction(eventName, player, player, false);

                if (!success)
                {
                    ChatHook.SendPrivate(ci, $"[FF0000]Hullám {i} nem indult el ({eventName}). A rituálé megszakadt.");
                    Log.Warning($"[Horda] Hullám {i} sikertelen: {eventName}");
                    yield break;
                }

                // Várunk, amíg az event lemegy — ez lehet fix idő vagy dinamikus
                yield return new WaitForSeconds(30f); // 30 másodperc hullámonként
            }

            ChatHook.SendPrivate(ci, "[00FFCC]Gratulálunk! Túlélted az 5 horda hullámot! [-]");
            Log.Out($"[Horda] Játékos {player.EntityName} túlélte az 5 Twitch horda hullámot.");
        }
    }
}