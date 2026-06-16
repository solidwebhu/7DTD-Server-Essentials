using System;
using OperenciaManager.Systems.Events.Core;

namespace OperenciaManager.Systems.Events.Challenges
{
    public class ZombieSlayerChallenge : IEventChallenge
    {
        private const int Goal = 50;

        private EventContext context;

        public string Id { get { return "zombieslayer"; } }
        public string Name { get { return "Zombi ölés"; } }
        public string IntroText { get { return "Ölj meg 50 zombit 10 perc alatt!"; } }

        public TimeSpan Duration
        {
            get { return TimeSpan.FromMinutes(10); }
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
            PlayerEventState state = context.GetPlayer(player.entityId);
            state.Kills++;
        }



        public bool IsCompleted(int entityId)
        {
            return context.GetPlayer(entityId).Kills >= Goal;
        }

        public string GetProgressText(int entityId)
        {
            PlayerEventState state = context.GetPlayer(entityId);
            return state.Kills + " / " + Goal + " zombi";
        }
    }
}
