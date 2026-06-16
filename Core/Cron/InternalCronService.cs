using System;
using System.Collections.Generic;
using OperenciaManager.Systems.Cron;

namespace OperenciaManager.Systems
{
    public static class InternalCronService
    {
        private static DateTime lastPerMinute = DateTime.MinValue;
        private static DateTime lastQuarterHourly = DateTime.MinValue;
        private static DateTime lastHalfHourly = DateTime.MinValue;
        private static DateTime lastHourly = DateTime.MinValue;
        private static DateTime lastDaily = DateTime.MinValue;
        private static DateTime lastWeekly = DateTime.MinValue;

        private static readonly Dictionary<CronType, List<Action>> handlers = new Dictionary<CronType, List<Action>>();

        public static void Init()
        {
            ModEvents.GameUpdate.RegisterHandler(
                new ModEvents.ModEventHandlerDelegate<ModEvents.SGameUpdateData>(Tick)
            );
            Log.Out("[InternalCronService] Időmotor aktiválva: " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        public static void Register(CronType type, Action handler)
        {
            if (!handlers.ContainsKey(type))
                handlers[type] = new List<Action>();

            handlers[type].Add(handler);
        }

        public static void Trigger(CronType type)
        {
            if (handlers.ContainsKey(type))
            {
                foreach (Action handler in handlers[type])
                {
                    try { handler(); } catch (Exception ex) { Log.Error("[InternalCronService] Hiba: " + ex.Message); }
                }
            }
        }

        private static void Tick(ref ModEvents.SGameUpdateData data)
        {
            DateTime now = DateTime.UtcNow;

            if ((now - lastPerMinute).TotalMinutes >= 1)
            {
                lastPerMinute = now;
                Log.Out("[InternalCronService] PerMinute cron lefutott: " + now.ToString("yyyy-MM-dd HH:mm"));
                Trigger(CronType.PerMinute);
            }

            if ((now - lastQuarterHourly).TotalMinutes >= 15)
            {
                lastQuarterHourly = now;
                Log.Out("[InternalCronService] QuarterHourly cron lefutott: " + now.ToString("yyyy-MM-dd HH:mm"));
                Trigger(CronType.QuarterHourly);
            }

            if ((now - lastHalfHourly).TotalMinutes >= 30)
            {
                lastHalfHourly = now;
                Log.Out("[InternalCronService] HalfHourly cron lefutott: " + now.ToString("yyyy-MM-dd HH:mm"));
                Trigger(CronType.HalfHourly);
            }

            if ((now - lastHourly).TotalMinutes >= 60)
            {
                lastHourly = now;
                Log.Out("[InternalCronService] Hourly cron lefutott: " + now.ToString("yyyy-MM-dd HH:mm"));
                Trigger(CronType.Hourly);
            }

            if ((now - lastDaily).TotalHours >= 24)
            {
                lastDaily = now;
                Log.Out("[InternalCronService] Daily cron lefutott: " + now.ToString("yyyy-MM-dd HH:mm"));
                Trigger(CronType.Daily);
            }

            if ((now - lastWeekly).TotalDays >= 7)
            {
                lastWeekly = now;
                Log.Out("[InternalCronService] Weekly cron lefutott: " + now.ToString("yyyy-MM-dd HH:mm"));
                Trigger(CronType.Weekly);
            }
        }
    }
}
