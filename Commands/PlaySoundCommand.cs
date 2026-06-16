using System;
using System.Linq;
using Audio;
using UnityEngine;
using OperenciaManager.Core;

namespace OperenciaManager.Commands
{
    public class PlaySoundCommand : IChatCommand
    {
        public string Name => "/playsound";

        public void Execute(ClientInfo clientInfo, string[] args)
        {
            // Jogosultság: admin (0–2)
            int level = GameManager.Instance.adminTools.Users
                .GetUserPermissionLevel(clientInfo.CrossplatformId);

            if (level > 2)
            {
                ChatHook.SendPrivate(clientInfo,
                    "[FF0000]Nincs jogosultságod ehhez a parancshoz.[-]");
                return;
            }

            // /playsound list
            if (args.Length == 1 && args[0].Equals("list", StringComparison.OrdinalIgnoreCase))
            {
                ListSoundsToServerLog(clientInfo);
                return;
            }

            // /playsound <sound>
            // /playsound <player> <sound>
            string soundName;
            EntityPlayer targetPlayer;

            if (args.Length == 1)
            {
                soundName = args[0];
                targetPlayer = GetPlayer(clientInfo.entityId);
            }
            else if (args.Length >= 2)
            {
                targetPlayer = FindPlayer(args[0]);
                soundName = args[1];
            }
            else
            {
                ChatHook.SendPrivate(clientInfo,
                    "[FFFF00]Használat: /playsound <sound> | /playsound <játékos> <sound> | /playsound list[-]");
                return;
            }

            if (targetPlayer == null)
            {
                ChatHook.SendPrivate(clientInfo,
                    "[FF0000]Nem található a céljátékos.[-]");
                return;
            }

            if (!Manager.audioData.ContainsKey(soundName))
            {
                ChatHook.SendPrivate(clientInfo,
                    $"[FF0000]Ismeretlen hang: {soundName}[-]");
                return;
            }

            Vector3 pos = targetPlayer.GetPosition();
            Manager.BroadcastPlay(pos, soundName, 0f);

            ChatHook.SendPrivate(clientInfo,
                $"[00FF00]Hang lejátszva: {soundName} → {targetPlayer.EntityName}[-]");
        }

        // -------------------------
        // SEGÉDFÜGGVÉNYEK
        // -------------------------

        private static EntityPlayer GetPlayer(int entityId)
        {
            GameManager.Instance.World.Players.dict
                .TryGetValue(entityId, out EntityPlayer player);
            return player;
        }

        private static EntityPlayer FindPlayer(string query)
        {
            query = query.ToLower();

            // entityId
            if (int.TryParse(query, out int id))
            {
                GameManager.Instance.World.Players.dict
                    .TryGetValue(id, out EntityPlayer p);
                if (p != null) return p;
            }

            // név töredék
            return GameManager.Instance.World.Players.list
                .FirstOrDefault(p =>
                    p != null &&
                    p.EntityName != null &&
                    p.EntityName.ToLower().Contains(query));
        }

        private static void ListSoundsToServerLog(ClientInfo clientInfo)
        {
            if (Manager.audioData == null || Manager.audioData.Count == 0)
            {
                ChatHook.SendPrivate(clientInfo,
                    "[FF0000]Nem található hangadat.[-]");
                return;
            }

            ChatHook.SendPrivate(clientInfo,
                "[00FFFF]Az összes elérhető hang kilistázva a server logba.[-]");

            Log.Out($"[PlaySound] === AVAILABLE SOUNDS (count: {Manager.audioData.Count}) ===");

            foreach (string sound in Manager.audioData.Keys)
            {
                Log.Out($"[PlaySound] - {sound}");
            }

            Log.Out("[PlaySound] === END OF LIST ===");
        }
    }
}
