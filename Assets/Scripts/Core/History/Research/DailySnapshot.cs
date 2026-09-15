using System.Collections.Generic;

namespace MakeMeHero.Core
{
    public sealed class DailySnapshot
    {
        public DailySnapshot(int dayNumber, BoardSnapshot boardBeforeBattle, EncounterInfo encounter, IList<DevelopmentDecision> decisions)
        { DayNumber = dayNumber; BoardBeforeBattle = boardBeforeBattle; Encounter = encounter; DevelopmentDecisions = decisions; UnitCombatResults = new List<UnitCombatResult>(); }
        public int DayNumber { get; private set; }
        public BoardSnapshot BoardBeforeBattle { get; private set; }
        public EncounterInfo Encounter { get; private set; }
        public IList<DevelopmentDecision> DevelopmentDecisions { get; private set; }
        public IList<UnitCombatResult> UnitCombatResults { get; private set; }
        public BoardSnapshot BoardAfterBattle { get; private set; }
        public BoardSnapshot StandbyStateAfterReward { get; private set; }
        public void CompleteBattle(BoardSnapshot afterBattle, IList<UnitCombatResult> results) { BoardAfterBattle = afterBattle; UnitCombatResults = results; }
        public void CompleteReward(BoardSnapshot standbyState) { StandbyStateAfterReward = standbyState; }
    }

    public sealed class EncounterInfo
    {
        public EncounterInfo(int plannedEnemyCount, string enemyArchetype, decimal spawnInterval)
        { PlannedEnemyCount = plannedEnemyCount; EnemyArchetype = enemyArchetype; SpawnInterval = spawnInterval; WaveCount = 1; }
        public int PlannedEnemyCount { get; private set; }
        public string EnemyArchetype { get; private set; }
        public int WaveCount { get; private set; }
        public decimal SpawnInterval { get; private set; }
        public string Difficulty { get; private set; }
        public bool HasBoss { get; private set; }
    }
}
