using System;
using UnityEngine;
using OperenciaManager.Core;
using OperenciaManager.Systems;

namespace OperenciaManager.Commands
{
    public class SetTeleCommand : IChatCommand
    {
        public string Name => "/settele";

        public void Execute(ClientInfo clientInfo, string[] args)
        {
            if (args.Length < 1)
            {
                ChatHook.SendPrivate(clientInfo, "[FFFF00]Használat: /settele <név>[-]");
                return;
            }

            string label = args[0].Trim();
            if (string.IsNullOrEmpty(label))
            {
                ChatHook.SendPrivate(clientInfo, "[FFFF00]Adj meg egy érvényes nevet![-]");
                return;
            }

            EntityPlayer player = GameManager.Instance.World.GetEntity(clientInfo.entityId) as EntityPlayer;
            if (player == null)
            {
                ChatHook.SendPrivate(clientInfo, "[FF0000]Nem található játékos entitás.[-]");
                return;
            }

            Vector3i pos = new Vector3i(
                Mathf.FloorToInt(player.position.x),
                Mathf.FloorToInt(player.position.y),
                Mathf.FloorToInt(player.position.z)
            );

            string crossId = clientInfo.CrossplatformId.CombinedString;
            TeleportStorage.SaveTeleport(crossId, label, pos);

            ChatHook.SendPrivate(clientInfo, $"[00FF00]Pozíció elmentve '{label}' néven: {pos}[-]");
        }
    }
}
