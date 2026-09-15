using System.Collections.Generic;

namespace MakeMeHero.Core
{
    public sealed class EvolutionDecisionResult
    {
        public EvolutionDecisionResult() { Errors = new List<EvolutionValidationIssue>(); Warnings = new List<EvolutionValidationIssue>(); }
        public bool IsValid { get { return Errors.Count == 0; } }
        public IList<EvolutionValidationIssue> Errors { get; private set; }
        public IList<EvolutionValidationIssue> Warnings { get; private set; }
        public void AddError(string code, string message, string field) { Errors.Add(new EvolutionValidationIssue(code, message, field)); }
        public void AddWarning(string code, string message, string field) { Warnings.Add(new EvolutionValidationIssue(code, message, field)); }
    }

    public sealed class EvolutionValidationIssue
    {
        public EvolutionValidationIssue(string code, string message, string field) { Code = code; Message = message; Field = field; }
        public string Code { get; private set; }
        public string Message { get; private set; }
        public string Field { get; private set; }
    }
}
