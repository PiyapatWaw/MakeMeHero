namespace MakeMeHero.Core
{
    public interface IEncounterScalingPolicy
    {
        int MonsterCountForDay(int day);
        decimal SpawnInterval { get; }
    }
}
