using OperenciaManager.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Audio;

namespace OperenciaManager.Commands
{
    public class PlaySoundMixCommand : IChatCommand
    {
        public string Name => "/playsoundmix";

        // mix név → hanglista
        private static readonly Dictionary<string, string[]> Mixes =
            new Dictionary<string, string[]>
        {
            {
                "craft",
                new[]
                {
                    "wood_grab",
                    "wood_place",
                    "stone_grab",
                    "stone_place",
                    "nails_grab",
                    "nails_place"
                }
            },
            {
                "factory",
                new[]
                {
                    "steelblock_place",
                    "paper_grab",
                    "paper_place",
                    "spring_place",
                    "steel_misc_hvy_place"
                }
            },
            {
                "chem",
                new[]
                {
                    "batterybank_start",
                    "oil_grab",
                    "oil_place",
                    "polymers_grab",
                    "polymers_place",
                    "batterybank_stop"
                }
            },
            {
                "admin",
                new[]
                {
                    "generator_start",
                    "generator_run",
                    "generator_stop"
                }
            }
        };

        public void Execute(ClientInfo clientInfo, string[] args)
        {
            int level = GameManager.Instance.adminTools.Users
                .GetUserPermissionLevel(clientInfo.CrossplatformId);

            if (level > 2)
            {
                ChatHook.SendPrivate(clientInfo,
                    "[FF0000]Nincs jogosultságod ehhez a parancshoz.");
                return;
            }

            if (args == null || args.Length == 0)
            {
                ChatHook.SendPrivate(clientInfo,
                    "[FFFF00]Használat: /playsoundmix <craft|factory|chem|admin|list>");
                return;
            }

            string mixName = args[0].ToLower();

            if (mixName == "list")
            {
                foreach (var key in Mixes.Keys)
                {
                    Log.Out($"[PlaySoundMix] elérhető mix: {key}");
                }

                ChatHook.SendPrivate(clientInfo,
                    "[00FF00]Az összes mix kiírva a server logba.");
                return;
            }

            if (!Mixes.ContainsKey(mixName))
            {
                ChatHook.SendPrivate(clientInfo,
                    $"[FF0000]Ismeretlen mix: {mixName}");
                return;
            }

            if (!GameManager.Instance.World.Players.dict
                .TryGetValue(clientInfo.entityId, out EntityPlayer player))
            {
                ChatHook.SendPrivate(clientInfo,
                    "[FF0000]Nem található játékos entitás.");
                return;
            }

            GameManager.Instance.StartCoroutine(
                PlayMixCoroutine(player.position, Mixes[mixName])
            );

            ChatHook.SendPrivate(clientInfo,
                $"[00FF00]Mix elindítva: {mixName}");
        }

        private static IEnumerator PlayMixCoroutine(Vector3 position, string[] sounds)
        {
            foreach (string sound in sounds)
            {
                Manager.BroadcastPlay(position, sound, 0f);
                yield return new WaitForSeconds(0.18f);
            }
        }
    }
}
