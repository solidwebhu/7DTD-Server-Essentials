using OperenciaManager.Core;
using System.Collections.Generic;

namespace OperenciaManager.Commands
{
    public class DewCollectorToggleCommand : IChatCommand
    {
        public string Name => "/dewcollector";

        private static readonly Dictionary<string, bool> dewCollectorBlocked = new Dictionary<string, bool>();

        public void Execute(ClientInfo clientInfo, string[] args)
        {
            string userId = clientInfo.CrossplatformId.CombinedString;

            bool current;
            dewCollectorBlocked.TryGetValue(userId, out current);
            dewCollectorBlocked[userId] = !current;

            if (dewCollectorBlocked[userId])
                ChatHook.SendPrivate(clientInfo, "[FF0000]Dew collector hozzáférés letiltva![-]");
            else
                ChatHook.SendPrivate(clientInfo, "[00FF00]Dew collector hozzáférés engedélyezve![-]");
        }

        public static bool ShouldBlockAccess(int entityId)
        {
            var cm = ConnectionManager.Instance;
            if (cm == null) return false;

            var ci = cm.Clients.ForEntityId(entityId);
            if (ci == null) return false;

            string userId = ci.CrossplatformId.CombinedString;
            return dewCollectorBlocked.TryGetValue(userId, out bool blocked) && blocked;
        }
    }
}
