using System;
using System.Collections.Generic;
using System.Linq;

namespace MakeMeHero.Core
{
    /// <summary>Method-agnostic observation sent to a human or external decision provider before it returns a decision.</summary>
    public sealed class CharacterEvolutionRequest
    {
        public CharacterEvolutionRequest(Run run, Hero hero, ResearchRunLog researchLog)
        {
            SchemaVersion = 1; RunId = run.Id; UnitId = hero.Id; DecisionDay = run.Day;
            Class = hero.Class; CurrentStats = hero.Stats; AvailableDevelopmentPoints = run.DevelopmentPointsAvailableForRankUp(hero);
            RankStars = hero.RankStars; AllowedStats = new List<EvolvableStat> { EvolvableStat.MaximumHp, EvolvableStat.AttackDamage, EvolvableStat.AttackInterval };
            LifetimeExperience = hero.LifetimeExperience; UnspentRankExperience = hero.UnspentRankExperience;
            CharacterHistory history = null;
            if (researchLog != null) researchLog.CharacterHistories.TryGetValue(hero.Id, out history);
            BaselineEvolution = history == null ? null : history.EvolutionAudits.OrderBy(x => x.AppliedDay).LastOrDefault();
            HistoryWindowStartDay = BaselineEvolution == null ? (history == null ? run.Day : history.JoinedDay) : BaselineEvolution.AppliedDay;
            Days = history == null ? new List<CharacterDayHistory>() : history.Days.Where(x => x.Day >= HistoryWindowStartDay).ToList();
            CurrentStandby = new EvolutionFormationSnapshot(BoardSnapshot.Capture(run), hero.Id);
        }
        public int SchemaVersion { get; private set; }
        public string RunId { get; private set; }
        public string UnitId { get; private set; }
        public int DecisionDay { get; private set; }
        public HeroClass Class { get; private set; }
        public int RankStars { get; private set; }
        public int AvailableDevelopmentPoints { get; private set; }
        public int LifetimeExperience { get; private set; }
        public int UnspentRankExperience { get; private set; }
        public CharacterStats CurrentStats { get; private set; }
        public IList<EvolvableStat> AllowedStats { get; private set; }
        public int HistoryWindowStartDay { get; private set; }
        public EvolutionDecisionAudit BaselineEvolution { get; private set; }
        public IList<CharacterDayHistory> Days { get; private set; }
        public EvolutionFormationSnapshot CurrentStandby { get; private set; }
    }

    public sealed class EvolutionDaySnapshot
    {
        public EvolutionDaySnapshot(DailySnapshot snapshot, string unitId)
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

    public sealed class EvolutionFormationSnapshot
    {
        public EvolutionFormationSnapshot(BoardSnapshot board, string unitId)
        {
            Self = new EvolutionUnitPresence(board.Units.Single(x => x.UnitId == unitId));
            Rooms = board.Rooms.Where(x => x.Members.Count > 0).Select(x => new EvolutionRoomSnapshot(x)).ToList();
            Reserve = board.Reserve.Select(x => new EvolutionUnitPresence(x)).ToList();
        }
        public EvolutionUnitPresence Self { get; private set; }
        public IList<EvolutionRoomSnapshot> Rooms { get; private set; }
        public IList<EvolutionUnitPresence> Reserve { get; private set; }
    }

    public sealed class EvolutionRoomSnapshot
    {
        public EvolutionRoomSnapshot(RoomSnapshot room) { Position = room.Position; Members = room.Members.Select(x => new EvolutionUnitPresence(x)).ToList(); }
        public string Position { get; private set; }
        public IList<EvolutionUnitPresence> Members { get; private set; }
    }

    public sealed class EvolutionUnitPresence
    {
        public EvolutionUnitPresence(UnitStateSnapshot unit) { UnitId = unit.UnitId; Class = unit.Archetype; Alive = !unit.IsDead; Position = unit.Position; Hp = unit.Hp; }
        public string UnitId { get; private set; }
        public string Class { get; private set; }
        public bool Alive { get; private set; }
        public string Position { get; private set; }
        public decimal Hp { get; private set; }
    }

    public sealed class EvolutionBattleSnapshot
    {
        public EvolutionBattleSnapshot(IList<UnitCombatResult> results, string unitId)
        {
            var target = results.Single(x => x.UnitId == unitId);
            DamageDealt = target.DamageDealt; DamageTaken = target.DamageTaken; HealingReceived = target.HealingReceived; Kills = target.Kills; SkillUsage = target.SkillUsage;
            SupportReceived = results.SelectMany(x => x.SupportContributions).Where(x => x.TargetId == unitId).ToList();
        }
        public decimal DamageDealt { get; private set; }
        public decimal DamageTaken { get; private set; }
        public decimal HealingReceived { get; private set; }
        public int Kills { get; private set; }
        public IDictionary<string, int> SkillUsage { get; private set; }
        public IList<SupportContribution> SupportReceived { get; private set; }
    }
}
