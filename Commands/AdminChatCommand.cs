using System;
using UnityEngine;
using OperenciaManager.Core;

namespace OperenciaManager.Commands
{
    public class AdminChatCommand : IChatCommand
    {
        public string Name => "/a";

        public void Execute(ClientInfo clientInfo, string[] args)
        {
            if (clientInfo == null || args.Length == 0)
            {
                ChatHook.SendPrivate(clientInfo, "[FF0000]Használat: /a <üzenet>");
                return;
            }

            int level = GameManager.Instance.adminTools.Users.GetUserPermissionLevel(clientInfo.CrossplatformId);
            if (level > 2)
            {
                ChatHook.SendPrivate(clientInfo, "[FF0000]Ehhez a parancshoz legalább 2-es admin jogosultság szükséges.");
                return;
            }

            string message = string.Join(" ", args);
            string senderName = clientInfo.playerName;

            foreach (var ci in ConnectionManager.Instance.Clients.List)
            {
                int targetLevel = GameManager.Instance.adminTools.Users.GetUserPermissionLevel(ci.CrossplatformId);
                if (targetLevel <= 2)
                {
                    ChatHook.SendPrivate(ci, $"[00CCCC][AdminChat] {senderName}[FFFFFF]: {message}");
                }
            }

            Log.Out($"[AdminChat] {senderName}: {message}");
        }
    }
}
