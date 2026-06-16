using System;
using OperenciaManager.Core;
using UnityEngine;

namespace OperenciaManager.Commands
{
    public class EntityRemoveCommand : IChatCommand
    {
        public string Name => "/entityremove";

        public void Execute(ClientInfo clientInfo, string[] args)
        {
            int level = GameManager.Instance.adminTools.Users.GetUserPermissionLevel(clientInfo.CrossplatformId);
            if (level > 2)
            {
                ChatHook.SendPrivate(clientInfo, "[FF0000]Nincs jogosultságod ehhez a parancshoz.[-]");
                return;
            }

            if (args.Length < 1)
            {
                ChatHook.SendPrivate(clientInfo, "[FFFF00]Használat: /entityremove <entityId>[-]");
                return;
            }

            if (!int.TryParse(args[0], out int entityId))
            {
                ChatHook.SendPrivate(clientInfo, "[FFFF00]Érvénytelen entity ID.[-]");
                return;
            }

            try
            {
                World world = GameManager.Instance.World;
                Entity entity = world.GetEntity(entityId);

                if (entity == null)
                {
                    ChatHook.SendPrivate(clientInfo, "[FFFF00]Nem található entity ezzel az ID-val.[-]");
                    return;
                }

                string entityName = entity.EntityClass != null
                    ? entity.EntityClass.entityClassName
                    : entity.GetType().Name;

                Vector3 pos = entity.position;

                world.RemoveEntity(entityId, EnumRemoveEntityReason.Despawned);

                ChatHook.SendPrivate(clientInfo,
                    $"[00FF00]Entity eltávolítva:[-] {entityName} (ID: {entityId}) @ {pos}");

                Log.Out($"[EntityRemove] {entityName} (ID:{entityId}) eltávolítva admin által: {clientInfo.playerName} poz: {pos}");
            }
            catch (Exception ex)
            {
                Log.Error("[EntityRemove] Hiba: " + ex);
                ChatHook.SendPrivate(clientInfo, "[FF0000]Hiba történt entity törlés közben.[-]");
            }
        }
    }
}
