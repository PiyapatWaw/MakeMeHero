using System;
using System.Collections.Generic;
using System.Linq;

namespace MakeMeHero.Core
{
    /// <summary>Canonical, append-only lifetime record for one hero. Export views never remove its days.</summary>
    public sealed class CharacterHistory
    {
        public CharacterHistory(UnitStateSnapshot hero, int joinedDay)
        {
            UnitId = hero.UnitId; DisplayName = hero.DisplayName; Class = hero.Archetype; JoinedDay = joinedDay;
            Days = new List<CharacterDayHistory>(); EvolutionAudits = new List<EvolutionDecisionAudit>();
        }

        public string UnitId { get; private set; }
        public string DisplayName { get; private set; }
        public string Class { get; private set; }
        public int JoinedDay { get; private set; }
        public int? EndedDay { get; private set; }
        public CharacterHistoryEndReason? EndReason { get; private set; }
        public IList<CharacterDayHistory> Days { get; private set; }
        public IList<EvolutionDecisionAudit> EvolutionAudits { get; private set; }

        public void AddDay(DailySnapshot snapshot)
        {
            if (Days.Any(x => x.Day == snapshot.DayNumber)) return;
            if (!snapshot.BoardBeforeBattle.Units.Any(x => x.UnitId == UnitId)) return;
            Days.Add(new CharacterDayHistory(snapshot, UnitId));
        }

        public void AddEvolution(EvolutionDecisionAudit audit)
        {
            if (audit != null && audit.Decision.UnitId == UnitId && EvolutionAudits.All(x => x.Decision.DecisionId != audit.Decision.DecisionId)) EvolutionAudits.Add(audit);
        }

        public void End(int day, CharacterHistoryEndReason reason)
        {
            if (EndReason.HasValue) return;
            EndedDay = day; EndReason = reason;
        }
    }

    /// <summary>One target-centric day; formation changes are inferred from end-to-next-standby snapshots.</summary>
    public sealed class CharacterDayHistory
    {
        public CharacterDayHistory(DailySnapshot snapshot, string unitId)
        {
            Day = snapshot.DayNumber;
            Standby = new EvolutionFormationSnapshot(snapshot.BoardBeforeBattle, unitId);
            Battle = new EvolutionBattleSnapshot(snapshot.UnitCombatResults, unitId);
            End = new EvolutionFormationSnapshot(snapshot.BoardAfterBattle, unitId);
        }

        public int Day { get; private set; }
        public EvolutionFormationSnapshot Standby { get; private set; }
        public EvolutionBattleSnapshot Battle { get; private set; }
        public EvolutionFormationSnapshot End { get; private set; }
    }
}
