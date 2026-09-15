using System;
using System.Collections.Generic;
using System.Linq;

namespace MakeMeHero.Core
{
    public sealed class CharacterEvolutionValidator
    {
        public EvolutionDecisionResult Validate(Run run, CharacterEvolutionDecision decision)
        {
            var result = new EvolutionDecisionResult();
            if (run == null) { result.AddError("RUN_NOT_FOUND", "Run was not found.", "runId"); return result; }
            if (decision == null) { result.AddError("INVALID_DECISION", "Decision is required.", "decision"); return result; }
            if (decision.SchemaVersion != 1) result.AddError("INVALID_SCHEMA_VERSION", "Only schema version 1 is supported.", "schemaVersion");
            if (String.IsNullOrWhiteSpace(decision.DecisionId)) result.AddError("INVALID_DECISION_ID", "decisionId is required.", "decisionId");
            if (decision.RunId != run.Id) result.AddError("RUN_ID_MISMATCH", "Decision runId does not match the current run.", "runId");
            if (run.Phase != RunPhase.Standby) result.AddError("INVALID_RUN_PHASE", "Evolution can only be applied during Standby.", "phase");
            if (decision.DecisionDay != run.Day) result.AddError("INVALID_DECISION_DAY", "decisionDay must equal the current Standby day.", "decisionDay");
            if (!String.IsNullOrWhiteSpace(decision.SelectedSkillId)) result.AddError("SKILL_SELECTION_OUT_OF_SCOPE", "Stat-only evolution does not accept selectedSkillId.", "selectedSkillId");
            if (!String.IsNullOrWhiteSpace(decision.DecisionId) && run.HasAppliedEvolutionDecision(decision.DecisionId)) result.AddError("DUPLICATE_DECISION", "decisionId has already been applied in this run.", "decisionId");

            var hero = run.FindHero(decision.UnitId);
            if (hero == null) result.AddError("UNIT_NOT_FOUND", "Hero unitId was not found.", "unitId");
            else if (hero.IsDead) result.AddError("UNIT_DEAD", "Dead heroes cannot evolve.", "unitId");

            if (decision.StatAllocations == null || decision.StatAllocations.Count == 0) result.AddError("NO_ACTION", "At least one stat allocation is required.", "statAllocations");
            var seen = new HashSet<EvolvableStat>(); long total = 0;
            foreach (var allocation in decision.StatAllocations ?? Enumerable.Empty<StatAllocation>())
            {
                if (allocation == null) { result.AddError("INVALID_ALLOCATION", "Allocation cannot be null.", "statAllocations"); continue; }
                if (!seen.Add(allocation.Stat)) result.AddError("DUPLICATE_STAT", "Each stat can appear only once.", "statAllocations");
                if (allocation.Points <= 0) result.AddError("INVALID_POINTS", "Allocated points must be positive.", "statAllocations");
                total += allocation.Points;
            }
            if (hero != null && total > hero.DevelopmentPoints) result.AddError("INSUFFICIENT_DEVELOPMENT_POINTS", "Allocated points exceed the hero's available points.", "statAllocations");
            return result;
        }
    }
}
