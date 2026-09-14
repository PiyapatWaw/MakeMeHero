using System;

namespace MakeMeHero.Core
{
    public sealed class ExponentialEncounterScalingPolicy : IEncounterScalingPolicy
    {
        public decimal SpawnInterval { get { return 1m; } }
        public int MonsterCountForDay(int day) { return (int)Math.Ceiling(3d * Math.Pow(1.3d, day - 1)); }
    }
}
