namespace MakeMeHero.Core
{
    public sealed class DevelopmentDecision
    {
        public DevelopmentDecision(EventType type, string unitId, int day, decimal time, decimal value, string detail)
        { Type = type; UnitId = unitId; Day = day; Time = time; Value = value; Detail = detail; }
        public DevelopmentDecision(EvolutionDecisionAudit audit, int day, decimal time)
        { Type = EventType.EvolutionApplied; UnitId = audit.Decision.UnitId; Day = day; Time = time; Detail = audit.Decision.DecisionId; EvolutionAudit = audit; }
        public EventType Type { get; private set; }
        public string UnitId { get; private set; }
        public int Day { get; private set; }
        public decimal Time { get; private set; }
        public decimal Value { get; private set; }
        public string Detail { get; private set; }
        public EvolutionDecisionAudit EvolutionAudit { get; private set; }
    }
}
