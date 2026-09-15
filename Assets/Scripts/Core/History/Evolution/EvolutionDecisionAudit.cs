using System.Collections.Generic;

namespace MakeMeHero.Core
{
    public sealed class EvolutionDecisionAudit
    {
        public EvolutionDecisionAudit(CharacterEvolutionDecision decision, int appliedDay, decimal appliedTime, EvolutionStatState before, EvolutionStatState after)
        { Decision = decision; AppliedDay = appliedDay; AppliedTime = appliedTime; Before = before; After = after; }
        public CharacterEvolutionDecision Decision { get; private set; }
        public int AppliedDay { get; private set; }
        public decimal AppliedTime { get; private set; }
        public EvolutionStatState Before { get; private set; }
        public EvolutionStatState After { get; private set; }
    }

    public sealed class EvolutionStatState
    {
        public EvolutionStatState(Hero hero) { MaximumHp = hero.MaximumHp; Hp = hero.Hp; AttackDamage = hero.AttackDamage; AttackInterval = hero.AttackInterval; DevelopmentPoints = hero.DevelopmentPoints; RankStars = hero.RankStars; LifetimeExperience = hero.LifetimeExperience; UnspentRankExperience = hero.UnspentRankExperience; }
        public decimal MaximumHp { get; private set; }
        public decimal Hp { get; private set; }
        public decimal AttackDamage { get; private set; }
        public decimal AttackInterval { get; private set; }
        public int DevelopmentPoints { get; private set; }
        public int RankStars { get; private set; }
        public int LifetimeExperience { get; private set; }
        public int UnspentRankExperience { get; private set; }
    }
}
