namespace MakeMeHero.Core
{
    public sealed class CombatEvent
    {
        public CombatEvent(EventType type, int day, decimal time, string sourceId = null, string targetId = null, decimal value = 0m, string detail = null)
        { Type = type; Day = day; Time = time; SourceId = sourceId; TargetId = targetId; Value = value; Detail = detail; }

        public EventType Type { get; private set; }
        public int Day { get; private set; }
        public decimal Time { get; private set; }
        public string SourceId { get; private set; }
        public string TargetId { get; private set; }
        public decimal Value { get; private set; }
        public string Detail { get; private set; }
    }
}
