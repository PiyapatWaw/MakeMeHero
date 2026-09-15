using System.Collections.Generic;
using System.Linq;

namespace MakeMeHero.Core
{
    public sealed class ResearchRunLog
    {
        public ResearchRunLog(RunMetadata metadata) { Metadata = metadata; DailySnapshots = new List<DailySnapshot>(); LifetimeStatistics = new Dictionary<string, LifetimeUnitStatistics>(); CharacterHistories = new Dictionary<string, CharacterHistory>(); }
        public RunMetadata Metadata { get; private set; }
        public IList<DailySnapshot> DailySnapshots { get; private set; }
        public IDictionary<string, LifetimeUnitStatistics> LifetimeStatistics { get; private set; }
        public IDictionary<string, CharacterHistory> CharacterHistories { get; private set; }
        public int TotalCompletedDays { get { return DailySnapshots.Count; } }
        public void AddSnapshot(DailySnapshot snapshot)
        {
            DailySnapshots.Add(snapshot);
            foreach (var hero in snapshot.BoardBeforeBattle.Units.Where(x => x.Kind == UnitKind.Hero)) EnsureCharacterHistory(hero, snapshot.DayNumber).AddDay(snapshot);
            foreach (var result in snapshot.UnitCombatResults)
            {
                LifetimeUnitStatistics lifetime;
                if (!LifetimeStatistics.TryGetValue(result.UnitId, out lifetime)) { lifetime = new LifetimeUnitStatistics(result.UnitId); LifetimeStatistics.Add(result.UnitId, lifetime); }
                lifetime.Add(result);
            }
            foreach (var result in snapshot.UnitCombatResults.Where(x => x.Kind == UnitKind.Hero && !x.Survived))
            {
                CharacterHistory history;
                if (CharacterHistories.TryGetValue(result.UnitId, out history)) history.End(snapshot.DayNumber, CharacterHistoryEndReason.Died);
            }
        }
        public CharacterHistory EnsureCharacterHistory(UnitStateSnapshot hero, int joinedDay)
        {
            CharacterHistory history;
            if (!CharacterHistories.TryGetValue(hero.UnitId, out history)) { history = new CharacterHistory(hero, joinedDay); CharacterHistories.Add(hero.UnitId, history); }
            return history;
        }
        public void RecordEvolution(EvolutionDecisionAudit audit)
        {
            if (audit == null) return;
            CharacterHistory history;
            if (CharacterHistories.TryGetValue(audit.Decision.UnitId, out history)) history.AddEvolution(audit);
        }
        public void CloseActiveHeroes(Run run, CharacterHistoryEndReason reason)
        {
            foreach (var hero in run.Heroes.Where(x => !x.IsDead))
            {
                var state = new UnitStateSnapshot(hero);
                EnsureCharacterHistory(state, run.Day).End(run.Day, reason);
            }
        }
    }

    public sealed class LifetimeUnitStatistics
    {
        public LifetimeUnitStatistics(string unitId) { UnitId = unitId; }
        public string UnitId { get; private set; }
        public int DaysObserved { get; private set; }
        public decimal DamageDealt { get; private set; }
        public decimal DamageTaken { get; private set; }
        public decimal HealingDone { get; private set; }
        public int Kills { get; private set; }
        public int SkillCasts { get; private set; }
        public void Add(UnitCombatResult result) { DaysObserved++; DamageDealt += result.DamageDealt; DamageTaken += result.DamageTaken; HealingDone += result.HealingDone; Kills += result.Kills; foreach (var usage in result.SkillUsage) SkillCasts += usage.Value; }
    }
}
