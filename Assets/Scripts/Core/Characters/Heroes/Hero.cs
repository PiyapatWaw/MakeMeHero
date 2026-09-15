using System;

namespace MakeMeHero.Core
{
    public abstract class Hero : Character
    {
        protected Hero(string id, string name, HeroClass heroClass, HeroStats stats) : base(id, name, stats.CharacterStats)
        { Class = heroClass; Skill = stats.Skill; RankStars = 1; }

        public HeroClass Class { get; private set; }
        public SkillDefinition Skill { get; private set; }
        public decimal SkillReadyAt { get; set; }
        public int RankStars { get; private set; }
        public int LifetimeExperience { get; private set; }
        public int UnspentRankExperience { get; private set; }
        public int SkillSlots { get; private set; }
        public int DevelopmentPoints { get; private set; }
        public void AwardSurvivalExperience() { LifetimeExperience++; UnspentRankExperience++; }
        public bool CanRankUp { get { return RankStars < 7 && UnspentRankExperience >= 5; } }
        internal void CompleteRankUp(int developmentPoints)
        {
            if (!CanRankUp) throw new InvalidOperationException("Hero is not eligible to rank up.");
            RankStars++; SkillSlots++; UnspentRankExperience -= 5;
            AwardDevelopmentPoints(developmentPoints);
        }
        internal void AwardDevelopmentPoints(int points) { if (points <= 0) throw new ArgumentOutOfRangeException("points"); DevelopmentPoints += points; }
        internal void ApplyEvolution(System.Collections.Generic.IEnumerable<StatAllocation> allocations, EvolutionPolicy policy)
        {
            var spent = 0;
            foreach (var allocation in allocations) spent += allocation.Points;
            ApplyStatDelta(policy.CreateDelta(allocations));
            DevelopmentPoints -= spent;
        }
    }
}
