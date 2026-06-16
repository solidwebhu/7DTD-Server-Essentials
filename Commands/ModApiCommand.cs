using System.Text;
using OperenciaManager.Core;
using OperenciaManager.Systems;

namespace OperenciaManager.Commands
{
    public class ModApiCommand : IChatCommand
    {
        public string Name => "/modapi";

        public void Execute(ClientInfo clientInfo, string[] args)
        {
            int level = GameManager.Instance.adminTools.Users
                .GetUserPermissionLevel(clientInfo.CrossplatformId);

            // csak admin 0
            if (level != 0)
            {
                ChatHook.SendPrivate(clientInfo,
                    "[FF0000]Ehhez a parancshoz admin szint 0 szükséges.[-]");
                return;
            }

            if (args == null || args.Length == 0)
            {
                SendHelp(clientInfo);
                return;
            }

            string sub = args[0].ToLower();

            if (sub == "reload")
            {
                FeatureManager.Reload();
                ChatHook.SendPrivate(clientInfo,
                    "[00FF00]ModAPI settings újratöltve.[-]");
                return;
            }

            if (sub == "status")
            {
                var all = OperenciaSettings.GetAll();

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("[FFFF00]Operencia feature státusz:[-]");

                foreach (var kv in all)
                {
                    sb.AppendLine(kv.Key + ": " + (kv.Value ? "ON" : "OFF"));
                }

                ChatHook.SendPrivate(clientInfo, sb.ToString());
                return;
            }

            if (sub == "set")
            {
                if (args.Length < 3)
                {
                    ChatHook.SendPrivate(clientInfo,
                        "[FF0000]Használat: /modapi set <FeatureName> <0|1>[-]");
                    return;
                }

                string feature = args[1];
                string val = args[2];

                bool enabled;

                if (val == "1")
                    enabled = true;
                else if (val == "0")
                    enabled = false;
                else
                {
                    ChatHook.SendPrivate(clientInfo,
                        "[FF0000]Érték csak 0 vagy 1 lehet.[-]");
                    return;
                }

                OperenciaSettings.Set(feature, enabled);
                FeatureManager.Reload();

                ChatHook.SendPrivate(clientInfo,
                    "[00FF00]" + feature + " → " + (enabled ? "ON" : "OFF") + "[-]");

                return;
            }

            SendHelp(clientInfo);
        }

        private void SendHelp(ClientInfo ci)
        {
            ChatHook.SendPrivate(ci,
                "[FFFF00]/modapi reload\n" +
                "[FFFF00]/modapi status\n" +
                "[FFFF00]/modapi set <FeatureName> <0|1>");
        }
    }
}
