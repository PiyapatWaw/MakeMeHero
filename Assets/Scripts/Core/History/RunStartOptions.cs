namespace MakeMeHero.Core
{
    public sealed class RunStartOptions
    {
        public RunStartOptions(int seed, string scenarioId = "standard-defense", string gameVersion = "phase-0", string dataSchemaVersion = "1.0")
        { Seed = seed; ScenarioId = scenarioId; GameVersion = gameVersion; DataSchemaVersion = dataSchemaVersion; }

        public int Seed { get; private set; }
        public string ScenarioId { get; private set; }
        public string GameVersion { get; private set; }
        public string DataSchemaVersion { get; private set; }
    }
}
