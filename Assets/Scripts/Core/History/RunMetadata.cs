using System;

namespace MakeMeHero.Core
{
    public sealed class RunMetadata
    {
        public RunMetadata(string runId, int seed, DateTimeOffset startedAtUtc, string scenarioId, string gameVersion, string dataSchemaVersion)
        {
            RunId = runId; Seed = seed; StartedAtUtc = startedAtUtc; ScenarioId = scenarioId; GameVersion = gameVersion; DataSchemaVersion = dataSchemaVersion;
        }

        public string RunId { get; private set; }
        public int Seed { get; private set; }
        public DateTimeOffset StartedAtUtc { get; private set; }
        public DateTimeOffset? EndedAtUtc { get; private set; }
        public string ScenarioId { get; private set; }
        public string GameVersion { get; private set; }
        public string DataSchemaVersion { get; private set; }
        public RunEndReason? EndReason { get; private set; }
        public void End(DateTimeOffset endedAtUtc, RunEndReason reason) { if (!EndedAtUtc.HasValue) { EndedAtUtc = endedAtUtc; EndReason = reason; } }
    }
}
