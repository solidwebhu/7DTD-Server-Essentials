using System.Collections.Generic;
using UnityEngine;

namespace OperenciaManager.Systems
{
    public static class FeatureManager
    {
        private static readonly Dictionary<string, Behaviour> features =
            new Dictionary<string, Behaviour>();

        public static void Register(string name, Behaviour behaviour)
        {
            features[name] = behaviour;
            Apply(name);
        }

        public static void Reload()
        {
            OperenciaSettings.Load();

            foreach (var kv in features)
            {
                Apply(kv.Key);
            }
        }

        private static void Apply(string name)
        {
            Behaviour b;
            if (!features.TryGetValue(name, out b))
                return;

            bool enabled = OperenciaSettings.IsEnabled(name);
            b.enabled = enabled;
        }
    }
}
