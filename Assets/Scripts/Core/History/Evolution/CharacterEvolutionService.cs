namespace MakeMeHero.Core
{
    public sealed class CharacterEvolutionService
    {
        private readonly CharacterEvolutionValidator _validator;
        private readonly EvolutionPolicy _statPolicy;
        public CharacterEvolutionService(CharacterEvolutionValidator validator = null, EvolutionPolicy statPolicy = null) { _validator = validator ?? new CharacterEvolutionValidator(); _statPolicy = statPolicy ?? new EvolutionPolicy(); }
        public EvolutionDecisionResult Validate(Run run, CharacterEvolutionDecision decision) { return _validator.Validate(run, decision); }
        public EvolutionDecisionResult Apply(Run run, CharacterEvolutionDecision decision)
        {
            var result = Validate(run, decision);
            if (!result.IsValid) return result;
            var hero = run.FindHero(decision.UnitId);
            var before = new EvolutionStatState(hero);
            hero.ApplyEvolution(decision.StatAllocations, _statPolicy);
            run.RecordEvolutionApplied(new EvolutionDecisionAudit(decision, before, new EvolutionStatState(hero)));
            return result;
        }
    }
}
