using OperenciaManager.Core;
using UnityEngine;
using System.Linq;
using OperenciaManager.Systems;

namespace OperenciaManager.Commands
{
    public class GetHereCommand : IChatCommand
    {
        public string Name => "/gethere";

        public void Execute(ClientInfo clientInfo, string[] args)
        {
            int level = GameManager.Instance.adminTools.Users.GetUserPermissionLevel(clientInfo.CrossplatformId);
            if (level > 2)
            {
                ChatHook.SendPrivate(clientInfo, "[FF0000]Nincs jogosultságod ehhez a parancshoz.");
                return;
            }

            if (args == null || args.Length == 0 || string.IsNullOrWhiteSpace(args[0]))
            {
                ChatHook.SendPrivate(clientInfo, "[FFFF00]Használat: /gethere <név töredék vagy entity ID>");
                return;
            }

            string query = args[0].Trim().ToLower();
            EntityPlayer targetPlayer = null;

            // ID alapján
            if (int.TryParse(query, out int entityId))
            {
                GameManager.Instance.World.Players.dict.TryGetValue(entityId, out targetPlayer);
            }

            // Név alapján
            if (targetPlayer == null)
            {
                targetPlayer = GameManager.Instance.World.Players.list
                    .FirstOrDefault(p => p != null && p.EntityName != null && p.EntityName.ToLower().Contains(query));
            }

            if (targetPlayer == null)
            {
                ChatHook.SendPrivate(clientInfo, $"[FF0000]Nem található játékos a megadott azonosítóval: {query}");
                return;
            }

            // Admin entitás
            if (!GameManager.Instance.World.Players.dict.TryGetValue(clientInfo.entityId, out EntityPlayer adminPlayer) || adminPlayer == null)
            {
                ChatHook.SendPrivate(clientInfo, "[FF0000]Nem találom az admin játékos entitást.");
                return;
            }

            // Céljátékos ClientInfo
            ClientInfo targetClient = ConnectionManager.Instance.Clients.ForEntityId(targetPlayer.entityId);
            if (targetClient == null)
            {
                ChatHook.SendPrivate(clientInfo, "[FF0000]Nem található a játékos hálózati kapcsolata.");
                return;
            }

            // Mentés teleport előtt
            TeleportMemory.SavePosition(targetClient.CrossplatformId.CombinedString, targetPlayer.GetPosition());

            // Teleportálás az adminhoz
            Vector3 adminPos = adminPlayer.GetPosition();
            NetPackageTeleportPlayer package =
                NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(adminPos, null, false);
            targetClient.SendPackage(package);

            // PET TELEPORTÁLÁSA (a céljátékosé!)
            PetTeleportHelper.SafeTeleportPet(targetPlayer, adminPos);

            ChatHook.SendPrivate(clientInfo,
                $"[00FF00]{targetPlayer.EntityName} teleportálva hozzád: {adminPos}[-]");
        }
    }
}
