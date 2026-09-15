using System;

namespace MakeMeHero.Core
{
    public sealed class EvolutionPolicy
    {
        public const decimal MaximumHpPerPoint = 5m;
        public const decimal AttackDamagePerPoint = 1m;
        public const decimal AttackIntervalReductionPerPoint = .05m;
        public CharacterStatDelta CreateDelta(System.Collections.Generic.IEnumerable<StatAllocation> allocations)
        {
            var result = new CharacterStatDelta();
            foreach (var allocation in allocations)
            {
                if (allocation.Points <= 0) throw new ArgumentOutOfRangeException("points");
                switch (allocation.Stat)
                {
                    case EvolvableStat.MaximumHp: result += new CharacterStatDelta(MaximumHpPerPoint * allocation.Points, 0m, 0m); break;
                    case EvolvableStat.AttackDamage: result += new CharacterStatDelta(0m, AttackDamagePerPoint * allocation.Points, 0m); break;
                    case EvolvableStat.AttackInterval: result += new CharacterStatDelta(0m, 0m, -AttackIntervalReductionPerPoint * allocation.Points); break;
                    default: throw new ArgumentOutOfRangeException("stat");
                }
            }
            return result;
        }
    }
}
