using System;
using OperenciaManager.Core;
using OperenciaManager.Systems.Events.Core;
using OperenciaManager.Systems.Events.Challenges;

namespace OperenciaManager.Commands
{
    public class EventCommand : IChatCommand
    {
        public string Name
        {
            get { return "/event"; }
        }

        public void Execute(ClientInfo clientInfo, string[] args)
        {
            if (clientInfo == null)
                return;

            if (args == null || args.Length == 0)
            {
                SendUsage(clientInfo);
                return;
            }

            string sub = args[0].ToLowerInvariant();

            switch (sub)
            {
                case "join":
                    EventManager.Instance.Join(clientInfo.entityId);
                    ChatHook.SendPrivate(clientInfo, "[00FF00]Csatlakoztál az eventhez.");
                    break;

                case "leave":
                    EventManager.Instance.Leave(clientInfo.entityId);
                    ChatHook.SendPrivate(clientInfo, "[FF0000]Kiléptél az eventből.");
                    break;

                case "status":
                    SendStatus(clientInfo);
                    break;

                case "list":
                    SendList(clientInfo);
                    break;

                case "start":
                    StartManualEvent(clientInfo, args);
                    break;

                case "stop":
                    StopEvent(clientInfo);
                    break;

                default:
                    SendUsage(clientInfo);
                    break;
            }
        }

        // =========================
        // Info
        // =========================

        private void SendUsage(ClientInfo ci)
        {
            ChatHook.SendPrivate(ci,
                "[FFFFFF]/event join | leave | status | list\n" +
                "[AAAAAA]Admin: /event start <id>, /event stop");
        }

        private void SendStatus(ClientInfo ci)
        {
            if (!EventManager.Instance.IsActive)
            {
                ChatHook.SendPrivate(ci, "[FFFF00]Jelenleg nincs aktív event.");
                return;
            }

            ChatHook.SendPrivate(ci,
                "[00FFAA]Van aktív event.\n" +
                "[AAAAAA]Parancsok: /event join, /event leave");
        }

        private void SendList(ClientInfo ci)
        {
            ChatHook.SendPrivate(ci, "[00FFAA]Elérhető eventek:");

            foreach (IEventChallenge c in ChallengeRegistry.All)
            {
                ChatHook.SendPrivate(ci,
                    "[AAAAAA]- " + c.Id + " → " + c.Name);
            }

            ChatHook.SendPrivate(ci,
                "[AAAAAA]Admin indítás: /event start <id>");
        }

        // =========================
        // Admin
        // =========================

        private void StartManualEvent(ClientInfo ci, string[] args)
        {
            int level = GameManager.Instance.adminTools.Users
                .GetUserPermissionLevel(ci.CrossplatformId);

            if (level > 2)
            {
                ChatHook.SendPrivate(ci, "[FF0000]Nincs jogosultságod ehhez.");
                return;
            }

            if (args.Length < 2)
            {
                ChatHook.SendPrivate(ci,
                    "[FFFF00]Használat: /event start <id>");
                return;
            }

            string id = args[1].ToLowerInvariant();

            IEventChallenge selected = null;

            foreach (IEventChallenge c in ChallengeRegistry.All)
            {
                if (c.Id.Equals(id, StringComparison.OrdinalIgnoreCase))
                {
                    selected = c;
                    break;
                }
            }

            if (selected == null)
            {
                ChatHook.SendPrivate(ci,
                    "[FF0000]Nincs ilyen event ID.\n" +
                    "[AAAAAA]Használd: /event list");
                return;
            }

            if (!EventManager.Instance.StartEvent(selected))
            {
                ChatHook.SendPrivate(ci, "[FFFF00]Már fut egy event.");
                return;
            }

            ChatHook.SendPrivate(ci,
                "[00FF00]Event manuálisan elindítva: " + selected.Name);
        }

        private void StopEvent(ClientInfo ci)
        {
            int level = GameManager.Instance.adminTools.Users
                .GetUserPermissionLevel(ci.CrossplatformId);

            if (level > 2)
            {
                ChatHook.SendPrivate(ci, "[FF0000]Nincs jogosultságod ehhez.");
                return;
            }

            if (!EventManager.Instance.IsActive)
            {
                ChatHook.SendPrivate(ci, "[FFFF00]Nincs futó event.");
                return;
            }

            EventManager.Instance.EndEvent();
            ChatHook.SendPrivate(ci, "[FF0000]Event leállítva.");
        }
    }
}
