using System.Collections.Generic;

namespace MakeMeHero.Core
{
    public sealed class ResearchRunLog
    {
        public ResearchRunLog(RunMetadata metadata) { Metadata = metadata; DailySnapshots = new List<DailySnapshot>(); LifetimeStatistics = new Dictionary<string, LifetimeUnitStatistics>(); }
        public RunMetadata Metadata { get; private set; }
        public IList<DailySnapshot> DailySnapshots { get; private set; }
        public IDictionary<string, LifetimeUnitStatistics> LifetimeStatistics { get; private set; }
        public int TotalCompletedDays { get { return DailySnapshots.Count; } }
        public void AddSnapshot(DailySnapshot snapshot)
        {
            DailySnapshots.Add(snapshot);
            foreach (var result in snapshot.UnitCombatResults)
            {
                LifetimeUnitStatistics lifetime;
                if (!LifetimeStatistics.TryGetValue(result.UnitId, out lifetime)) { lifetime = new LifetimeUnitStatistics(result.UnitId); LifetimeStatistics.Add(result.UnitId, lifetime); }
                lifetime.Add(result);
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
