using System;

namespace MakeMeHero.Core
{
    public abstract class Hero : Character
    {
        protected Hero(string id, string name, HeroClass heroClass, HeroStats stats) : base(id, name, stats.Hp, stats.Attack, stats.AttackInterval)
        { Class = heroClass; Skill = stats.Skill; RankStars = 1; }

        public HeroClass Class { get; private set; }
        public SkillDefinition Skill { get; private set; }
        public decimal SkillReadyAt { get; set; }
        public int RankStars { get; private set; }
        public int SurvivedDays { get; private set; }
        public int SkillSlots { get; private set; }
        public int DevelopmentPoints { get; private set; }
        public void AwardSurvivalDay() { SurvivedDays++; }
        public bool CanRankUp { get { return RankStars < 7 && SurvivedDays >= RankStars * 5; } }
        public void RankUp() { if (!CanRankUp) throw new InvalidOperationException("Hero is not eligible to rank up."); RankStars++; SkillSlots++; }
        internal void AwardDevelopmentPoints(int points) { if (points <= 0) throw new ArgumentOutOfRangeException("points"); DevelopmentPoints += points; }
        internal void ApplyEvolution(System.Collections.Generic.IEnumerable<StatAllocation> allocations, EvolutionPolicy policy)
        {
            var spent = 0;
            foreach (var allocation in allocations)
            {
                spent += allocation.Points;
                var delta = policy.DeltaFor(allocation.Stat, allocation.Points);
                switch (allocation.Stat)
                {
                    case EvolvableStat.MaximumHp: IncreaseMaximumHp(delta); break;
                    case EvolvableStat.AttackDamage: IncreaseAttackDamage(delta); break;
                    case EvolvableStat.AttackInterval: ReduceAttackInterval(delta, EvolutionPolicy.MinimumAttackInterval); break;
                }
            }
            DevelopmentPoints -= spent;
        }
    }
}
