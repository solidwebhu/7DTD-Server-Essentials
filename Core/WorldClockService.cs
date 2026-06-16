using System;

namespace OperenciaManager.Core
{
    public static class WorldClockService
    {
        private static DateTime? overrideTime = null;

        public static DateTime Now => overrideTime ?? DateTime.UtcNow;

        public static void SetOverride(DateTime customTime)
        {
            overrideTime = customTime;
        }

        public static void ClearOverride()
        {
            overrideTime = null;
        }

        public static bool IsBetween(DateTime start, DateTime end)
        {
            return Now >= start && Now < end;
        }

        public static bool IsElapsed(DateTime start, TimeSpan duration)
        {
            return Now >= start + duration;
        }

        public static string Format(DateTime time)
        {
            return time.ToString("yyyy.MM.dd HH:mm:ss");
        }
    }
}
