using System.Collections.Generic;
using System.Linq;

namespace MakeMeHero.Core
{
    public sealed class UnitCombatAccumulator
    {
        private readonly Dictionary<string, UnitCombatResult> _results = new Dictionary<string, UnitCombatResult>();
        public void Start(BoardSnapshot before) { _results.Clear(); foreach (var unit in before.Units) _results.Add(unit.UnitId, new UnitCombatResult(unit)); }
        public void Ensure(UnitStateSnapshot unit) { if (!_results.ContainsKey(unit.UnitId)) _results.Add(unit.UnitId, new UnitCombatResult(unit)); }
        public void Apply(CombatEvent combatEvent)
        {
            UnitCombatResult source; UnitCombatResult target;
            _results.TryGetValue(combatEvent.SourceId ?? string.Empty, out source); _results.TryGetValue(combatEvent.TargetId ?? string.Empty, out target);
            if (combatEvent.Type == EventType.Attack && source != null) source.RecordAttack();
            if (combatEvent.Type == EventType.SkillCast && source != null) source.RecordSkill(combatEvent.SkillId);
            if (combatEvent.Type == EventType.Damage) { if (source != null) source.RecordDamageDealt(combatEvent.Value, combatEvent.ActionKind); if (target != null) target.RecordDamageTaken(combatEvent.Value); }
            if (combatEvent.Type == EventType.Heal) { if (source != null) { source.RecordHealDone(combatEvent.Value); source.RecordSupport(new SupportContribution(source.UnitId, combatEvent.TargetId, combatEvent.SkillId ?? SkillId.Heal, combatEvent.Value)); } if (target != null) target.RecordHealReceived(combatEvent.Value); }
            if (combatEvent.Type == EventType.DamageMitigated) { if (target != null) target.RecordMitigated(combatEvent.Value); if (source != null) source.RecordSupport(new SupportContribution(source.UnitId, combatEvent.TargetId, combatEvent.SkillId ?? SkillId.Guard, combatEvent.Value)); }
            if (combatEvent.Type == EventType.Kill && source != null) source.RecordKill();
            if (combatEvent.Type == EventType.UnitDied && target != null) target.RecordDeath(combatEvent.SourceId, combatEvent.Detail);
        }
        public IList<UnitCombatResult> Complete(BoardSnapshot after)
        {
            foreach (var unit in after.Units) { Ensure(unit); _results[unit.UnitId].Complete(unit); }
            return _results.Values.OrderBy(x => x.UnitId).ToList();
        }
    }
}
