using System;
using System.Collections.Generic;
using MakeMeHero.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MakeMeHero.Game
{
    /// <summary>Untrusted JSON adapter. It parses schema v1 but never changes gameplay state.</summary>
    public sealed class CharacterEvolutionJsonImporter
    {
        private static readonly HashSet<string> RootFields = new HashSet<string> { "schemaVersion", "decisionId", "runId", "unitId", "decisionDay", "statAllocations", "selectedSkillId", "metadata" };
        private static readonly HashSet<string> AllocationFields = new HashSet<string> { "stat", "points" };
        private static readonly HashSet<string> MetadataFields = new HashSet<string> { "source", "experimentId" };
        public EvolutionDecisionResult TryImport(string json, out CharacterEvolutionDecision decision)
        {
            decision = null;
            var result = new EvolutionDecisionResult();
            if (String.IsNullOrWhiteSpace(json)) { result.AddError("MALFORMED_JSON", "JSON input is required.", "json"); return result; }
            try
            {
                var root = JToken.Parse(json) as JObject;
                if (root == null) { result.AddError("MALFORMED_JSON", "Root JSON value must be an object.", "json"); return result; }
                foreach (var property in root.Properties()) if (!RootFields.Contains(property.Name)) result.AddError("UNKNOWN_FIELD", "Unknown field: " + property.Name, property.Name);
                var schemaVersion = GetRequiredInt(root, "schemaVersion", result);
                var decisionId = GetRequiredString(root, "decisionId", result);
                var runId = GetRequiredString(root, "runId", result);
                var unitId = GetRequiredString(root, "unitId", result);
                var decisionDay = GetRequiredInt(root, "decisionDay", result);
                var selectedSkillId = GetOptionalString(root, "selectedSkillId", result);
                var allocations = ParseAllocations(root, result);
                var metadata = ParseMetadata(root, result);
                if (result.IsValid) decision = new CharacterEvolutionDecision(schemaVersion, decisionId, runId, unitId, decisionDay, allocations, selectedSkillId, metadata);
            }
            catch (JsonException exception) { result.AddError("MALFORMED_JSON", exception.Message, "json"); }
            return result;
        }

        private static IList<StatAllocation> ParseAllocations(JObject root, EvolutionDecisionResult result)
        {
            var allocations = new List<StatAllocation>(); JToken value;
            if (!root.TryGetValue("statAllocations", out value) || value.Type != JTokenType.Array) { result.AddError("MISSING_FIELD", "statAllocations array is required.", "statAllocations"); return allocations; }
            foreach (var item in value.Children())
            {
                var allocation = item as JObject;
                if (allocation == null) { result.AddError("INVALID_ALLOCATION", "Each allocation must be an object.", "statAllocations"); continue; }
                foreach (var property in allocation.Properties()) if (!AllocationFields.Contains(property.Name)) result.AddError("UNKNOWN_FIELD", "Unknown allocation field: " + property.Name, "statAllocations." + property.Name);
                string statName = GetRequiredString(allocation, "stat", result); var points = GetRequiredInt(allocation, "points", result);
                EvolvableStat stat;
                if (!Enum.TryParse(statName, true, out stat)) result.AddError("UNKNOWN_STAT", "Unknown evolvable stat: " + statName, "statAllocations.stat");
                else allocations.Add(new StatAllocation(stat, points));
            }
            return allocations;
        }

        private static EvolutionDecisionMetadata ParseMetadata(JObject root, EvolutionDecisionResult result)
        {
            JToken value;
            if (!root.TryGetValue("metadata", out value)) return EvolutionDecisionMetadata.Unknown;
            var metadata = value as JObject;
            if (metadata == null) { result.AddError("INVALID_METADATA", "metadata must be an object.", "metadata"); return EvolutionDecisionMetadata.Unknown; }
            foreach (var property in metadata.Properties()) if (!MetadataFields.Contains(property.Name)) result.AddError("UNKNOWN_FIELD", "Unknown metadata field: " + property.Name, "metadata." + property.Name);
            var sourceName = GetOptionalString(metadata, "source", result); var experimentId = GetOptionalString(metadata, "experimentId", result);
            EvolutionDecisionSource source;
            if (String.IsNullOrWhiteSpace(sourceName)) source = EvolutionDecisionSource.Unknown;
            else if (!Enum.TryParse(sourceName, true, out source)) { result.AddError("UNKNOWN_SOURCE", "Unknown evolution source: " + sourceName, "metadata.source"); source = EvolutionDecisionSource.Unknown; }
            return new EvolutionDecisionMetadata(source, experimentId);
        }

        private static string GetRequiredString(JObject objectElement, string name, EvolutionDecisionResult result) { var value = GetOptionalString(objectElement, name, result); if (String.IsNullOrWhiteSpace(value)) result.AddError("MISSING_FIELD", name + " is required.", name); return value; }
        private static string GetOptionalString(JObject objectElement, string name, EvolutionDecisionResult result) { JToken value; if (!objectElement.TryGetValue(name, out value) || value.Type == JTokenType.Null) return null; if (value.Type != JTokenType.String) { result.AddError("INVALID_FIELD", name + " must be a string.", name); return null; } return value.Value<string>(); }
        private static int GetRequiredInt(JObject objectElement, string name, EvolutionDecisionResult result) { JToken value; int parsed; if (!objectElement.TryGetValue(name, out value) || value.Type != JTokenType.Integer || !Int32.TryParse(value.ToString(), out parsed)) { result.AddError("MISSING_FIELD", name + " must be an integer.", name); return 0; } return parsed; }
    }
}
