using System.IO;
using MakeMeHero.Core;
using Newtonsoft.Json;
using UnityEngine;

namespace MakeMeHero.Game
{
    /// <summary>Unity-side persistence adapter. Core owns research facts; this class only writes canonical JSON.</summary>
    public sealed class ResearchJsonExporter
    {
        private readonly string _rootDirectory;
        private readonly JsonSerializerSettings _options = new JsonSerializerSettings { Formatting = Formatting.Indented };

        public ResearchJsonExporter(string rootDirectory) { _rootDirectory = rootDirectory; }

        public void Export(ResearchRunLog log)
        {
            var runDirectory = Path.Combine(_rootDirectory, log.Metadata.RunId);
            var daysDirectory = Path.Combine(runDirectory, "days");
            Directory.CreateDirectory(daysDirectory);
            File.WriteAllText(Path.Combine(runDirectory, "run.json"), JsonConvert.SerializeObject(new RunExportSummary(log), _options));
            foreach (var day in log.DailySnapshots)
            {
                var filename = "day-" + day.DayNumber.ToString("000") + ".json";
                File.WriteAllText(Path.Combine(daysDirectory, filename), JsonConvert.SerializeObject(day, _options));
            }
        }

        private sealed class RunExportSummary
        {
            public RunExportSummary(ResearchRunLog log) { Metadata = log.Metadata; TotalCompletedDays = log.TotalCompletedDays; LifetimeStatistics = log.LifetimeStatistics; CharacterHistories = log.CharacterHistories; }
            public RunMetadata Metadata { get; private set; }
            public int TotalCompletedDays { get; private set; }
            public System.Collections.Generic.IDictionary<string, LifetimeUnitStatistics> LifetimeStatistics { get; private set; }
            public System.Collections.Generic.IDictionary<string, CharacterHistory> CharacterHistories { get; private set; }
        }
    }
}
