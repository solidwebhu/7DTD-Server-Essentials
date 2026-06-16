using System;
using OperenciaManager.Systems.Events.Core;

namespace OperenciaManager.Systems.Events.Challenges
{
    public class DeadeyeChallenge : IEventChallenge
    {
        private const int Goal = 25;

        private EventContext context;

        public string Id => "deadeye";
        public string Name => "Deadeye";
        public string IntroText => "Ölj meg 25 zombit precíz találattal!";

        public TimeSpan Duration => TimeSpan.FromMinutes(10);

        public void Register(int entityId)
        {
            context.GetPlayer(entityId).Deadeye++;
        }
        public void RegisterHeadshot(int entityId)
        {
            PlayerEventState state = context.GetPlayer(entityId);
            state.Deadeye++;
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
            context.GetPlayer(entityId);
        }

        public void EventPlayerLeft(int entityId)
        {
        }

        public void OnGameUpdate()
        {
        }

        public void OnEntityKilled(EntityPlayer player, EntityZombie zombie)
        {
            context.GetPlayer(player.entityId).Deadeye++;
        }

        public bool IsCompleted(int entityId)
        {
            return context.GetPlayer(entityId).Deadeye >= Goal;
        }

        public string GetProgressText(int entityId)
        {
            return context.GetPlayer(entityId).Deadeye + " / " + Goal;
        }
    }
}
