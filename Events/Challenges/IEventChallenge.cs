using System;

namespace OperenciaManager.Systems.Events.Core
{
    public interface IEventChallenge
    {
        string Id { get; }
        string Name { get; }
        string IntroText { get; }
        TimeSpan Duration { get; }

        void OnStart(EventContext ctx);
        void OnEnd(EventContext ctx);

        void EventPlayerJoined(int entityId);
        void EventPlayerLeft(int entityId);

        void OnGameUpdate();
        void OnEntityKilled(EntityPlayer player, EntityZombie zombie);

        bool IsCompleted(int entityId);
        string GetProgressText(int entityId);
    }
}
