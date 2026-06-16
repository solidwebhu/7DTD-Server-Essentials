using GameEvent.SequenceActions;
using System.Collections.Generic;
using UnityEngine;

namespace OperenciaManager.Core
{
    public static class ChatHook
    {
        // Dinamikusan beállítható küldőnév
        public static string SenderName = "OPERENCIA";

        public static void SendPrivate(ClientInfo clientInfo, string message)
        {
            if (clientInfo == null) return;

            string formattedMessage = $"[1E90FF]{SenderName}:[-] {message}";

            var chatPackage = NetPackageManager.GetPackage<NetPackageChat>().Setup(
                EChatType.Whisper,
                -1,
                formattedMessage,
                new List<int> { clientInfo.entityId },
                EMessageSender.None,
                GeneratedTextManager.BbCodeSupportMode.Supported
            );

            clientInfo.SendPackage(chatPackage);
        }

        public static void SendGlobal(string message)
        {
            string formattedMessage = $"[1E90FF]{SenderName}:[-] {message}";

            var chatPackage = NetPackageManager.GetPackage<NetPackageChat>().Setup(
                EChatType.Global,
                -1,
                formattedMessage,
                null,
                EMessageSender.None,
                GeneratedTextManager.BbCodeSupportMode.Supported
            );

            foreach (var client in ConnectionManager.Instance.Clients.List)
            {
                client.SendPackage(chatPackage);
            }
        }

        public static void SendGlobalRaw(string message)
        {
            var chatPackage = NetPackageManager.GetPackage<NetPackageChat>().Setup(
                EChatType.Global,
                -1,
                message,
                null,
                EMessageSender.None,
                GeneratedTextManager.BbCodeSupportMode.Supported
            );

            foreach (var client in ConnectionManager.Instance.Clients.List)
            {
                client.SendPackage(chatPackage);
            }
        }


        public static void RegisterChatHandler()
        {
           // egyéb
        }

        





    }
}
