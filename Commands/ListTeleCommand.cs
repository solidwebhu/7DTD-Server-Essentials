using System;
using OperenciaManager.Core;
using OperenciaManager.Systems;

namespace OperenciaManager.Commands
{
    public class ListTeleCommand : IChatCommand
    {
        public string Name => "/listtele";

        public void Execute(ClientInfo clientInfo, string[] args)
        {
            string crossId = clientInfo.CrossplatformId.CombinedString;
            var labels = TeleportStorage.ListTeleports(crossId);

            if (labels.Count == 0)
            {
                ChatHook.SendPrivate(clientInfo, "[FFFF00]Nincs mentett teleportod.[-]");
                return;
            }

            ChatHook.SendPrivate(clientInfo, "[00FFFF]Mentett teleportjaid:");
            foreach (var label in labels)
            {
                ChatHook.SendPrivate(clientInfo, $"[00FF00]• {label}[-]");
            }
        }
    }
}
