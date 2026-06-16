using System;
using OperenciaManager.Core;
using OperenciaManager.Systems;
using OperenciaManager.Systems.Cron;

namespace OperenciaManager.Commands
{
    public class CronInditCommand : IChatCommand
    {
        public string Name => "/cronindit";

        public void Execute(ClientInfo clientInfo, string[] args)
        {
            if (args.Length == 0)
            {
                ChatHook.SendPrivate(clientInfo, "[FF0000]Használat: /cronindit perminute|quarterhourly|halfhourly|hourly|daily|weekly");
                return;
            }

            string type = args[0].ToLower();
            CronType cronType;

            switch (type)
            {
                case "perminute":
                    cronType = CronType.PerMinute;
                    break;
                case "quarterhourly":
                    cronType = CronType.QuarterHourly;
                    break;
                case "halfhourly":
                    cronType = CronType.HalfHourly;
                    break;
                case "hourly":
                    cronType = CronType.Hourly;
                    break;
                case "daily":
                    cronType = CronType.Daily;
                    break;
                case "weekly":
                    cronType = CronType.Weekly;
                    break;
                default:
                    ChatHook.SendPrivate(clientInfo, "[FF0000]Ismeretlen cron típus: " + type);
                    return;
            }

            InternalCronService.Trigger(cronType);
            ChatHook.SendPrivate(clientInfo, $"[00FF00]{type} cron lefuttatva manuálisan.");
        }
    }
}
