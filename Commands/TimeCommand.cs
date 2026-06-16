using System;
using OperenciaManager.Core;

namespace OperenciaManager.Commands
{
    public class TimeCommand : IChatCommand
    {
        public string Name => "/time";

        public void Execute(ClientInfo clientInfo, string[] args)
        {
            if (clientInfo == null) return;

            string playerName = clientInfo.playerName;
            string currentTime = DateTime.Now.ToString("HH:mm:ss");

            string message = $"{playerName} megnézte az óráját. Jelenlegi idő: {currentTime}";

            ChatHook.SendGlobal(message);
        }
    }
}
