using System.Collections.Generic;

namespace MakeMeHero.Core
{
    /// <summary>Method-agnostic decision after an outer adapter has parsed untrusted input.</summary>
    public sealed class CharacterEvolutionDecision
    {
        public CharacterEvolutionDecision(int schemaVersion, string decisionId, string runId, string unitId, int decisionDay, IList<StatAllocation> statAllocations, string selectedSkillId, EvolutionDecisionMetadata metadata)
        { SchemaVersion = schemaVersion; DecisionId = decisionId; RunId = runId; UnitId = unitId; DecisionDay = decisionDay; StatAllocations = statAllocations ?? new List<StatAllocation>(); SelectedSkillId = selectedSkillId; Metadata = metadata ?? EvolutionDecisionMetadata.Unknown; }
        public int SchemaVersion { get; private set; }
        public string DecisionId { get; private set; }
        public string RunId { get; private set; }
        public string UnitId { get; private set; }
        public int DecisionDay { get; private set; }
        public IList<StatAllocation> StatAllocations { get; private set; }
        public string SelectedSkillId { get; private set; }
        public EvolutionDecisionMetadata Metadata { get; private set; }
    }

    public sealed class StatAllocation
    {
        public StatAllocation(EvolvableStat stat, int points) { Stat = stat; Points = points; }
        public EvolvableStat Stat { get; private set; }
        public int Points { get; private set; }
    }

    public sealed class EvolutionDecisionMetadata
    {
        public static readonly EvolutionDecisionMetadata Unknown = new EvolutionDecisionMetadata(EvolutionDecisionSource.Unknown, null);
        public EvolutionDecisionMetadata(EvolutionDecisionSource source, string experimentId) { Source = source; ExperimentId = experimentId; }
        public EvolutionDecisionSource Source { get; private set; }
        public string ExperimentId { get; private set; }
    }
}
