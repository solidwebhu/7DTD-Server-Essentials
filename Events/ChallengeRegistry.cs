using System;
using System.Collections.Generic;
using OperenciaManager.Systems.Events.Challenges;

namespace OperenciaManager.Systems.Events.Core
{
    public static class ChallengeRegistry
    {
        private static readonly List<IEventChallenge> challenges = new List<IEventChallenge>()
        {
            new ZombieSlayerChallenge(),
            new DistanceRunnerChallenge(),
            new DeadeyeChallenge(),

        };

        private static readonly Random rng = new Random();

        public static IEventChallenge GetRandom()
        {
            if (challenges.Count == 0)
                return null;

            int index = rng.Next(challenges.Count);
            return challenges[index];
        }

        public static IEnumerable<IEventChallenge> All
        {
            get { return challenges; }
        }
    }
}
