using System.Collections.Generic;

namespace OperenciaManager.Systems
{
    public static class ClaimRepairState
    {
        private static readonly Dictionary<string, bool> states = new Dictionary<string, bool>();

        public static bool IsEnabled(string crossId)
        {
            bool enabled;
            if (!states.TryGetValue(crossId, out enabled))
            {
                states[crossId] = true; // alapértelmezett: bekapcsolva
                return true;
            }
            return enabled;
        }

        public static bool Toggle(string crossId)
        {
            bool current = IsEnabled(crossId);
            bool newState = !current;
            states[crossId] = newState;
            return newState;
        }

        public static void SetDefault(string crossId)
        {
            states[crossId] = true;
        }
    }
}
