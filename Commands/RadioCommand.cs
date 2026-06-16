using OperenciaManager.Core;
using System.Collections;
using UnityEngine;
using Audio;

namespace OperenciaManager.Commands
{
    public class RadioCommand : IChatCommand
    {
        public string Name => "/radio";

        private static Coroutine radioCoroutine;

        // ===== BPM =====
        private const float BPM_INTRO = 110f;
        private const float BPM_BUILD = 140f;
        private const float BPM_DROP = 195f;

        private static readonly string[] IntroGroove =
        {
            "wood_place",
            "stone_place",
            "nails_place"
        };

        private static readonly string[] MainGroove =
        {
            "iron_place",
            "steelblock_place",
            "spring_place",
            "parts_place"
        };

        private static readonly string[] TraderVox =
        {
            "trader_bob_sale_accepted",
            "trader_rekt_refuse",
            "trader_jenlikeannounceopen",
            "trader_hugh_trade"
        };

        private static readonly string[] PlayerRhythm =
        {
            "stonehitglass",
            "breakleg",
            "levelupplayer",
            "quest_failed",
            "password_pass",
            "close_door_jail"
        };

        private static readonly string[] PlayerFill =
        {
            "player1painsm",
            "player2painlg",
            "player1stamina",
            "player2stamina",
            "player1sick",
            "player2vomit"
        };

        private static readonly string[] PlayerDrop =
        {
            "player1death",
            "player2death",
            "player1drowndeath",
            "player2drowndeath"
        };

        public void Execute(ClientInfo clientInfo, string[] args)
        {
            if (!GameManager.Instance.World.Players.dict
                .TryGetValue(clientInfo.entityId, out EntityPlayer player))
            {
                ChatHook.SendPrivate(clientInfo, "[FF0000]Nem található játékos.[");
                return;
            }

            if (args == null || args.Length == 0)
            {
                ChatHook.SendPrivate(clientInfo, "[FFFF00]/radio start | stop");
                return;
            }

            string cmd = args[0].ToLower();

            if (cmd == "stop")
            {
                if (radioCoroutine != null)
                {
                    GameManager.Instance.StopCoroutine(radioCoroutine);
                    radioCoroutine = null;
                }

                ChatHook.SendPrivate(clientInfo,
                    "[FF5555]Operencia FM leállítva 🔇[-]");
                return;
            }

            if (cmd == "start")
            {
                if (radioCoroutine != null)
                {
                    ChatHook.SendPrivate(clientInfo,
                        "[FFFF00]A rádió már szól 🎶[-]");
                    return;
                }

                radioCoroutine = GameManager.Instance.StartCoroutine(
                    RadioLoop(player)
                );

                ChatHook.SendPrivate(clientInfo,
                    "[00FFAA]Operencia FM elindult 🔥[-]");
            }
        }

        private static IEnumerator RadioLoop(EntityPlayer player)
        {
            System.Random rng = new System.Random();
            int beat = 0;
            float bpm = BPM_INTRO;

            while (true)
            {
                Vector3 pos = player.position;

                // ===== BPM RAMP =====
                if (beat > 24 && beat <= 80)
                    bpm = Mathf.Lerp(BPM_INTRO, BPM_BUILD, (beat - 24f) / 56f);
                else if (beat > 80)
                    bpm = BPM_DROP;

                float wait = 60f / bpm;

                // ===== BASE BEAT =====
                Manager.BroadcastPlay(pos,
                    beat < 32
                        ? IntroGroove[beat % IntroGroove.Length]
                        : MainGroove[beat % MainGroove.Length],
                    0f);

                // ===== PLAYER RHYTHM =====
                if (beat > 16 && beat % 2 == 0)
                {
                    Manager.BroadcastPlay(pos,
                        PlayerRhythm[rng.Next(PlayerRhythm.Length)], 0f);
                }

                // ===== FILL =====
                if (beat > 48 && beat % 8 == 0)
                {
                    Manager.BroadcastPlay(pos,
                        PlayerFill[rng.Next(PlayerFill.Length)], 0f);
                }

                // ===== TRADER STAB =====
                if (beat % 16 == 0)
                {
                    Manager.BroadcastPlay(pos,
                        TraderVox[rng.Next(TraderVox.Length)], 0f);
                }

                // ===== DROP ACCENT =====
                if (beat > 96 && beat % 64 == 0)
                {
                    Manager.BroadcastPlay(pos,
                        PlayerDrop[rng.Next(PlayerDrop.Length)], 0f);
                }

                beat++;
                yield return new WaitForSeconds(wait);
            }
        }
    }
}
