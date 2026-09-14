using System.Collections.Generic;
using System.Linq;

namespace MakeMeHero.Core
{
    public sealed class InMemoryRunRepository : IRunRepository
    {
        private readonly Dictionary<string, Run> _runs = new Dictionary<string, Run>();
        public void Save(Run run) { _runs[run.Id] = run; }
        public Run Find(string runId) { return _runs[runId]; }
        public IEnumerable<Run> CompletedRuns() { return _runs.Values.Where(x => x.Phase == RunPhase.Lost); }
    }
}
