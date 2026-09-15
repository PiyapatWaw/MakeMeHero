using System;
using System.Collections.Generic;

namespace MakeMeHero.Core
{
    public sealed class RunApplicationService
    {
        private readonly IRunRepository _repository; private readonly CombatTuning _tuning; private readonly IEncounterScalingPolicy _scaling; private readonly Func<int, IRandomSource> _randomFactory; private readonly IRunClock _clock;
        private readonly CharacterEvolutionService _evolutionService;
        private readonly Dictionary<string, ResearchLogger> _researchLoggers = new Dictionary<string, ResearchLogger>();
        public RunApplicationService(IRunRepository repository, CombatTuning tuning, IEncounterScalingPolicy scaling, IRandomSource random) : this(repository, tuning, scaling, seed => random, new SystemRunClock()) { }
        public RunApplicationService(IRunRepository repository, CombatTuning tuning, IEncounterScalingPolicy scaling, Func<int, IRandomSource> randomFactory, IRunClock clock = null, CharacterEvolutionService evolutionService = null) { _repository = repository; _tuning = tuning; _scaling = scaling; _randomFactory = randomFactory; _clock = clock ?? new SystemRunClock(); _evolutionService = evolutionService ?? new CharacterEvolutionService(); }
        public Run CreateRun(RunStartOptions options = null)
        {
            options = options ?? new RunStartOptions(Guid.NewGuid().GetHashCode());
            var metadata = new RunMetadata(Guid.NewGuid().ToString("N"), options.Seed, _clock.UtcNow, options.ScenarioId, options.GameVersion, options.DataSchemaVersion);
            var logger = new ResearchLogger(metadata);
            var run = new Run(metadata, _tuning, _scaling, _randomFactory(options.Seed), new IRunEventObserver[] { logger }, _clock);
            _researchLoggers.Add(run.Id, logger); _repository.Save(run); return run;
        }
        public ResearchRunLog ResearchLog(string runId) { return _researchLoggers[runId].Log; }
        public void Recruit(string runId, HeroClass type) { var run = _repository.Find(runId); run.Recruit(type); _repository.Save(run); }
        public void Deploy(string runId, string heroId, GridPosition tile) { var run = _repository.Find(runId); run.Deploy(heroId, tile); _repository.Save(run); }
        public void Undeploy(string runId, string heroId) { var run = _repository.Find(runId); run.Undeploy(heroId); _repository.Save(run); }
        /// <summary>Creates an external decision request; it deliberately does not mutate rank, experience, or stats.</summary>
        public CharacterEvolutionRequest RequestRankUp(string runId, string heroId)
        {
            var run = _repository.Find(runId);
            run.EnsureRankUpRequestEligible(heroId);
            var hero = run.FindHero(heroId);
            return new CharacterEvolutionRequest(run, hero, ResearchLog(runId));
        }
        public void StartDay(string runId) { var run = _repository.Find(runId); run.StartDay(); _repository.Save(run); }
        public void AdvanceTime(string runId, decimal seconds) { var run = _repository.Find(runId); run.Advance(seconds); _repository.Save(run); }
        public void EndRun(string runId) { var run = _repository.Find(runId); run.Abandon(); _repository.Save(run); }
        public EvolutionDecisionResult ValidateEvolutionDecision(string runId, CharacterEvolutionDecision decision) { return _evolutionService.Validate(_repository.Find(runId), decision); }
        public EvolutionDecisionResult ApplyEvolutionDecision(string runId, CharacterEvolutionDecision decision) { var run = _repository.Find(runId); var result = _evolutionService.Apply(run, decision); if (result.IsValid) _repository.Save(run); return result; }
        public CharacterEvolutionRequest CreateEvolutionRequest(string runId, string heroId)
        {
            var run = _repository.Find(runId);
            run.EnsureRankUpRequestEligible(heroId);
            return RequestRankUp(runId, heroId);
        }
    }
}
