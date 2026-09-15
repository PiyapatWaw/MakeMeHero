using System;

namespace MakeMeHero.Core
{
    public sealed class EvolutionPolicy
    {
        public const decimal MaximumHpPerPoint = 5m;
        public const decimal AttackDamagePerPoint = 1m;
        public const decimal AttackIntervalReductionPerPoint = .05m;
        public const decimal MinimumAttackInterval = .20m;

        public decimal DeltaFor(EvolvableStat stat, int points)
        {
            if (points <= 0) throw new ArgumentOutOfRangeException("points");
            switch (stat)
            {
                case EvolvableStat.MaximumHp: return MaximumHpPerPoint * points;
                case EvolvableStat.AttackDamage: return AttackDamagePerPoint * points;
                case EvolvableStat.AttackInterval: return AttackIntervalReductionPerPoint * points;
                default: throw new ArgumentOutOfRangeException("stat");
            }
        }
    }
}
