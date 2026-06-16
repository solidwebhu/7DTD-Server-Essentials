using System;
using OperenciaManager.Core;
using OperenciaManager.Systems;

namespace OperenciaManager.Commands
{
    public class DelTeleCommand : IChatCommand
    {
        public string Name => "/deltele";

        public void Execute(ClientInfo clientInfo, string[] args)
        {
            if (args.Length < 1)
            {
                ChatHook.SendPrivate(clientInfo, "[FFFF00]Használat: /deltele <név>[-]");
                return;
            }

            string label = args[0].Trim();
            string crossId = clientInfo.CrossplatformId.CombinedString;

            bool success = TeleportStorage.DeleteTeleport(crossId, label);
            if (success)
            {
                ChatHook.SendPrivate(clientInfo, $"[00FF00]Mentett teleport törölve: '{label}'[-]");
            }
            else
            {
                ChatHook.SendPrivate(clientInfo, $"[FF0000]Nincs ilyen mentett teleport: '{label}'[-]");
            }
        }
    }
}
