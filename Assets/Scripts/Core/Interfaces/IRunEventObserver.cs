namespace MakeMeHero.Core
{
    /// <summary>Receives immutable domain events without making Run depend on a consumer such as research logging.</summary>
    public interface IRunEventObserver
    {
        void OnEventRecorded(Run run, CombatEvent combatEvent);
    }
}
