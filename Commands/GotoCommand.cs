using OperenciaManager.Core;
using OperenciaManager.Systems;
using UnityEngine;
using System.Linq;

namespace OperenciaManager.Commands
{
    public class GotoCommand : IChatCommand
    {
        public string Name => "/goto";

        public void Execute(ClientInfo clientInfo, string[] args)
        {
            // Admin jogosultság lekérése
            int level = GameManager.Instance.adminTools.Users.GetUserPermissionLevel(clientInfo.CrossplatformId);

            // Argumentum ellenőrzés
            if (args == null || args.Length == 0 || string.IsNullOrWhiteSpace(args[0]))
            {
                ChatHook.SendPrivate(clientInfo, "[FFFF00]Használat: /goto <név töredék vagy entity ID>[-]");
                return;
            }

            string query = args[0].Trim().ToLower();
            EntityPlayer targetPlayer = null;

            // ID alapján
            if (int.TryParse(query, out int entityId))
            {
                GameManager.Instance.World.Players.dict.TryGetValue(entityId, out targetPlayer);
            }

            // Név töredék alapján
            if (targetPlayer == null)
            {
                targetPlayer = GameManager.Instance.World.Players.list
                    .FirstOrDefault(p =>
                        p != null &&
                        p.EntityName != null &&
                        p.EntityName.ToLower().Contains(query));
            }

            if (targetPlayer == null)
            {
                ChatHook.SendPrivate(clientInfo, $"[FF0000]Nem található játékos: {query}[-]");
                return;
            }

            // Saját entitás lekérése
            if (!GameManager.Instance.World.Players.dict.TryGetValue(clientInfo.entityId, out EntityPlayer callerPlayer) || callerPlayer == null)
            {
                ChatHook.SendPrivate(clientInfo, "[FF0000]Nem találom a saját játékos entitásodat.[-]");
                return;
            }

            // 🔒 Nem admin → csak friendre teleportálhat
            if (level > 2)
            {
                bool callerIsFriend = callerPlayer.IsFriendsWith(targetPlayer);
                bool targetIsFriend = targetPlayer.IsFriendsWith(callerPlayer);

                if (!callerIsFriend || !targetIsFriend)
                {
                    ChatHook.SendPrivate(clientInfo,
                        "[FF0000]Csak olyan játékosra teleportálhatsz, aki friendben van veled (kétirányú friend).[-]");
                    return;
                }
            }

            // Mentés teleport előtt
            TeleportMemory.SavePosition(clientInfo.CrossplatformId.CombinedString, callerPlayer.GetPosition());

            // Teleport
            Vector3 targetPos = targetPlayer.GetPosition();
            NetPackageTeleportPlayer package =
                NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(targetPos, null, false);
            clientInfo.SendPackage(package);

            // PET teleport
            PetTeleportHelper.SafeTeleportPet(callerPlayer, targetPos);

            ChatHook.SendPrivate(clientInfo,
                $"[00FF00]Teleportálva {targetPlayer.EntityName} pozíciójára: {targetPos}[-]");
        }
    }
}
