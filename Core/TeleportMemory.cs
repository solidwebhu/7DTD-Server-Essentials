using System.Collections.Generic;
using UnityEngine;

namespace OperenciaManager.Core
{
    public static class TeleportMemory
    {
        private static Dictionary<string, Vector3> lastPositions = new Dictionary<string, Vector3>();

        public static void SavePosition(string crossplatformId, Vector3 position)
        {
            lastPositions[crossplatformId] = position;
        }

        public static bool TryGetLastPosition(string crossplatformId, out Vector3 position)
        {
            return lastPositions.TryGetValue(crossplatformId, out position);
        }

        public static void ClearPosition(string crossplatformId)
        {
            lastPositions.Remove(crossplatformId);
        }
    }
}
