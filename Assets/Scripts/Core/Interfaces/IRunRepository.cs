using System.Collections.Generic;

namespace MakeMeHero.Core
{
    public interface IRunRepository
    {
        void Save(Run run);
        Run Find(string runId);
        IEnumerable<Run> CompletedRuns();
    }
}
