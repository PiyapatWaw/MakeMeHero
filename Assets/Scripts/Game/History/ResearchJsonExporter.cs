using System.IO;
using System.Text.Json;
using MakeMeHero.Core;
using UnityEngine;

namespace MakeMeHero.Game
{
    /// <summary>Unity-side persistence adapter. Core owns research facts; this class only writes canonical JSON.</summary>
    public sealed class ResearchJsonExporter
    {
        private readonly string _rootDirectory;
        private readonly JsonSerializerOptions _options = new JsonSerializerOptions { WriteIndented = true };

        public ResearchJsonExporter(string rootDirectory) { _rootDirectory = rootDirectory; }

        public void Export(ResearchRunLog log)
        {
            var runDirectory = Path.Combine(_rootDirectory, log.Metadata.RunId);
            var daysDirectory = Path.Combine(runDirectory, "days");
            Directory.CreateDirectory(daysDirectory);
            File.WriteAllText(Path.Combine(runDirectory, "run.json"), JsonSerializer.Serialize(new RunExportSummary(log), _options));
            foreach (var day in log.DailySnapshots)
            {
                var filename = "day-" + day.DayNumber.ToString("000") + ".json";
                File.WriteAllText(Path.Combine(daysDirectory, filename), JsonSerializer.Serialize(day, _options));
            }
        }

        private sealed class RunExportSummary
        {
            public RunExportSummary(ResearchRunLog log) { Metadata = log.Metadata; TotalCompletedDays = log.TotalCompletedDays; LifetimeStatistics = log.LifetimeStatistics; }
            public RunMetadata Metadata { get; private set; }
            public int TotalCompletedDays { get; private set; }
            public System.Collections.Generic.IDictionary<string, LifetimeUnitStatistics> LifetimeStatistics { get; private set; }
        }
    }
}
