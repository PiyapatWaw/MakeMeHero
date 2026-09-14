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
        public void AwardSurvivalDay() { SurvivedDays++; }
        public bool CanRankUp { get { return RankStars < 7 && SurvivedDays >= RankStars * 5; } }
        public void RankUp() { if (!CanRankUp) throw new InvalidOperationException("Hero is not eligible to rank up."); RankStars++; SkillSlots++; }
    }
}
