using System;
using UnityEngine;
using OperenciaManager.Systems.Events.Core;

namespace OperenciaManager.Systems.Events.Challenges
{
    public class DistanceRunnerChallenge : IEventChallenge
    {
        private const float GoalMeters = 3000f;

        private EventContext context;

        public string Id { get { return "distancerunner"; } }
        public string Name { get { return "Távfutás"; } }
        public string IntroText { get { return "Tegyél meg 3 km-t 15 perc alatt gyalog!"; } }

        public TimeSpan Duration
        {
            get { return TimeSpan.FromMinutes(15); }
        }

        public void OnStart(EventContext ctx)
        {
            context = ctx;
        }

        public void OnEnd(EventContext ctx)
        {
        }

        public void EventPlayerJoined(int entityId)
        {
            PlayerEventState state = context.GetPlayer(entityId);

            EntityPlayer player;
            if (GameManager.Instance.World.Players.dict.TryGetValue(entityId, out player))
            {
                state.LastPosition = player.position;
            }
        }

        public void EventPlayerLeft(int entityId)
        {
        }

        public void OnGameUpdate()
        {
            foreach (EntityPlayer player in GameManager.Instance.World.Players.dict.Values)
            {
                if (player == null)
                    continue;

                int id = player.entityId;

                PlayerEventState state = context.GetPlayer(id);

                if (player.AttachedToEntity != null)
                    continue;

                Vector3 pos = player.position;

                if (state.LastPosition == Vector3.zero)
                {
                    state.LastPosition = pos;
                    continue;
                }

                float delta = Vector3.Distance(pos, state.LastPosition);

                // teleport / lag filter
                if (delta < 5f)
                {
                    state.Distance += delta;
                }

                state.LastPosition = pos;
            }
        }

        public void OnEntityKilled(EntityPlayer player, EntityZombie zombie)
        {
        }


        public bool IsCompleted(int entityId)
        {
            return context.GetPlayer(entityId).Distance >= GoalMeters;
        }

        public string GetProgressText(int entityId)
        {
            PlayerEventState state = context.GetPlayer(entityId);
            return Mathf.RoundToInt(state.Distance) + " / " + Mathf.RoundToInt(GoalMeters) + " m";
        }
    }
}
