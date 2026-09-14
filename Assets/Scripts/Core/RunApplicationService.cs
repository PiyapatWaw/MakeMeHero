using System;

namespace MakeMeHero.Core
{
    public sealed class RunApplicationService
    {
        private readonly IRunRepository _repository; private readonly CombatTuning _tuning; private readonly IEncounterScalingPolicy _scaling; private readonly IRandomSource _random;
        public RunApplicationService(IRunRepository repository, CombatTuning tuning, IEncounterScalingPolicy scaling, IRandomSource random) { _repository = repository; _tuning = tuning; _scaling = scaling; _random = random; }
        public Run CreateRun() { var run = new Run(Guid.NewGuid().ToString("N"), _tuning, _scaling, _random); _repository.Save(run); return run; }
        public void Recruit(string runId, HeroClass type) { var run = _repository.Find(runId); run.Recruit(type); _repository.Save(run); }
        public void Deploy(string runId, string heroId, GridPosition tile) { var run = _repository.Find(runId); run.Deploy(heroId, tile); _repository.Save(run); }
        public void Undeploy(string runId, string heroId) { var run = _repository.Find(runId); run.Undeploy(heroId); _repository.Save(run); }
        public void RankUp(string runId, string heroId) { var run = _repository.Find(runId); run.RankUp(heroId); _repository.Save(run); }
        public void StartDay(string runId) { var run = _repository.Find(runId); run.StartDay(); _repository.Save(run); }
        public void AdvanceTime(string runId, decimal seconds) { var run = _repository.Find(runId); run.Advance(seconds); _repository.Save(run); }
    }
}
