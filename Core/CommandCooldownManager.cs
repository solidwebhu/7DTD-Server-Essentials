using System;
using System.Collections.Generic;

namespace OperenciaManager.Systems
{
    public static class CommandCooldownManager
    {
        private static readonly Dictionary<string, Dictionary<string, DateTime>> cooldowns = new Dictionary<string, Dictionary<string, DateTime>>();

        public static bool IsOnCooldown(string commandName, string userId, int cooldownSeconds, out int remaining)
        {
            remaining = 0;

            if (!cooldowns.ContainsKey(commandName))
                cooldowns[commandName] = new Dictionary<string, DateTime>();

            var userCooldowns = cooldowns[commandName];

            if (userCooldowns.TryGetValue(userId, out DateTime lastUsed))
            {
                TimeSpan elapsed = DateTime.UtcNow - lastUsed;
                if (elapsed.TotalSeconds < cooldownSeconds)
                {
                    remaining = cooldownSeconds - (int)elapsed.TotalSeconds;
                    return true;
                }
            }

            userCooldowns[userId] = DateTime.UtcNow;
            return false;
        }
    }
}
