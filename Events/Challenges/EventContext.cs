using System;
using System.Collections.Generic;

namespace OperenciaManager.Systems.Events.Core
{
    public class EventContext
    {
        private readonly Dictionary<int, PlayerEventState> players =
            new Dictionary<int, PlayerEventState>();

        public DateTime StartTime;

        public PlayerEventState GetPlayer(int id)
        {
            PlayerEventState s;
            if (!players.TryGetValue(id, out s))
            {
                s = new PlayerEventState();
                players[id] = s;
            }

            return s;
        }
    }
}
