using System;
using System.Collections.Generic;

namespace MakeMeHero.Core
{
    /// <summary>Method-agnostic observation sent to a human or external decision provider before it returns a decision.</summary>
    public sealed class CharacterEvolutionRequest
    {
        public CharacterEvolutionRequest(Run run, Hero hero)
        {
            SchemaVersion = 1; RunId = run.Id; UnitId = hero.Id; DecisionDay = run.Day;
            Class = hero.Class; CurrentStats = hero.Stats; AvailableDevelopmentPoints = hero.DevelopmentPoints;
            RankStars = hero.RankStars; AllowedStats = new List<EvolvableStat> { EvolvableStat.MaximumHp, EvolvableStat.AttackDamage, EvolvableStat.AttackInterval };
        }
        public int SchemaVersion { get; private set; }
        public string RunId { get; private set; }
        public string UnitId { get; private set; }
        public int DecisionDay { get; private set; }
        public HeroClass Class { get; private set; }
        public int RankStars { get; private set; }
        public int AvailableDevelopmentPoints { get; private set; }
        public CharacterStats CurrentStats { get; private set; }
        public IList<EvolvableStat> AllowedStats { get; private set; }
    }
}
