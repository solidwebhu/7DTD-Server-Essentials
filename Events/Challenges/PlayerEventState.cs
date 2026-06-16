using System;
using UnityEngine;

namespace OperenciaManager.Systems.Events.Core
{
    public class PlayerEventState
    {
        // DistanceRunner
        public float Distance;

        // ZombieSlayer
        public int Kills;

        // Deadeye
        public int Deadeye;

        // Movement tracking
        public Vector3 LastPosition;

        // Progress throttling
        public DateTime LastNotify;

        // Milestones
        public int LastMilestone;

        public PlayerEventState()
        {
            Distance = 0f;
            Kills = 0;
            Deadeye = 0;

            LastPosition = Vector3.zero;
            LastNotify = DateTime.MinValue;
            LastMilestone = 0;
        }
    }
}
