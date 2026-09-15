using System.Collections.Generic;
using System.Linq;

namespace MakeMeHero.Core
{
    /// <summary>Observes Run events and creates research records; it never changes combat state.</summary>
    public sealed class ResearchLogger : IRunEventObserver
    {
        private readonly ResearchRunLog _log;
        private readonly UnitCombatAccumulator _accumulator = new UnitCombatAccumulator();
        private readonly List<DevelopmentDecision> _pendingDecisions = new List<DevelopmentDecision>();
        private DailySnapshot _activeDay;

        public ResearchLogger(RunMetadata metadata) { _log = new ResearchRunLog(metadata); }
        public ResearchRunLog Log { get { return _log; } }

        public void OnEventRecorded(Run run, CombatEvent combatEvent)
        {
            if (combatEvent.Type == EventType.Recruited || combatEvent.Type == EventType.Deployed || combatEvent.Type == EventType.Undeployed || combatEvent.Type == EventType.RankUp)
            {
                _pendingDecisions.Add(new DevelopmentDecision(combatEvent.Type, combatEvent.SourceId, combatEvent.Day, combatEvent.Time, combatEvent.Value, combatEvent.Detail));
            }
            if (combatEvent.Type == EventType.EvolutionApplied)
            {
                var audit = run.EvolutionAudits.LastOrDefault(x => x.Decision.DecisionId == combatEvent.Detail);
                if (audit != null) _pendingDecisions.Add(new DevelopmentDecision(audit, combatEvent.Day, combatEvent.Time));
            }
            if (combatEvent.Type == EventType.DayStarted)
            {
                var before = BoardSnapshot.Capture(run);
                _activeDay = new DailySnapshot(combatEvent.Day, before, new EncounterInfo((int)combatEvent.Value, combatEvent.Detail, 1m), new List<DevelopmentDecision>(_pendingDecisions));
                _pendingDecisions.Clear(); _accumulator.Start(before);
                return;
            }
            if (_activeDay != null)
            {
                if (combatEvent.Type == EventType.MonsterSpawned) foreach (var unit in BoardSnapshot.Capture(run).Units) if (unit.UnitId == combatEvent.SourceId) _accumulator.Ensure(unit);
                _accumulator.Apply(combatEvent);
                if (combatEvent.Type == EventType.BattleCompleted) _activeDay.CompleteBattle(BoardSnapshot.Capture(run), _accumulator.Complete(BoardSnapshot.Capture(run)));
                if (combatEvent.Type == EventType.DayCompleted) { _activeDay.CompleteReward(BoardSnapshot.Capture(run)); _log.AddSnapshot(_activeDay); _activeDay = null; }
                if (combatEvent.Type == EventType.RunLost) { _activeDay.CompleteBattle(BoardSnapshot.Capture(run), _accumulator.Complete(BoardSnapshot.Capture(run))); _log.AddSnapshot(_activeDay); _activeDay = null; }
            }
        }
    }
}
