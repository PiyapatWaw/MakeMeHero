using System;
using System.Collections.Generic;
using System.Text.Json;
using MakeMeHero.Core;

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
                using (var document = JsonDocument.Parse(json))
                {
                    var root = document.RootElement;
                    if (root.ValueKind != JsonValueKind.Object) { result.AddError("MALFORMED_JSON", "Root JSON value must be an object.", "json"); return result; }
                    foreach (var property in root.EnumerateObject()) if (!RootFields.Contains(property.Name)) result.AddError("UNKNOWN_FIELD", "Unknown field: " + property.Name, property.Name);
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
            }
            catch (JsonException exception) { result.AddError("MALFORMED_JSON", exception.Message, "json"); }
            return result;
        }

        private static IList<StatAllocation> ParseAllocations(JsonElement root, EvolutionDecisionResult result)
        {
            var allocations = new List<StatAllocation>(); JsonElement value;
            if (!root.TryGetProperty("statAllocations", out value) || value.ValueKind != JsonValueKind.Array) { result.AddError("MISSING_FIELD", "statAllocations array is required.", "statAllocations"); return allocations; }
            foreach (var item in value.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.Object) { result.AddError("INVALID_ALLOCATION", "Each allocation must be an object.", "statAllocations"); continue; }
                foreach (var property in item.EnumerateObject()) if (!AllocationFields.Contains(property.Name)) result.AddError("UNKNOWN_FIELD", "Unknown allocation field: " + property.Name, "statAllocations." + property.Name);
                string statName = GetRequiredString(item, "stat", result); var points = GetRequiredInt(item, "points", result);
                EvolvableStat stat;
                if (!Enum.TryParse(statName, true, out stat)) result.AddError("UNKNOWN_STAT", "Unknown evolvable stat: " + statName, "statAllocations.stat");
                else allocations.Add(new StatAllocation(stat, points));
            }
            return allocations;
        }

        private static EvolutionDecisionMetadata ParseMetadata(JsonElement root, EvolutionDecisionResult result)
        {
            JsonElement value;
            if (!root.TryGetProperty("metadata", out value)) return EvolutionDecisionMetadata.Unknown;
            if (value.ValueKind != JsonValueKind.Object) { result.AddError("INVALID_METADATA", "metadata must be an object.", "metadata"); return EvolutionDecisionMetadata.Unknown; }
            foreach (var property in value.EnumerateObject()) if (!MetadataFields.Contains(property.Name)) result.AddError("UNKNOWN_FIELD", "Unknown metadata field: " + property.Name, "metadata." + property.Name);
            var sourceName = GetOptionalString(value, "source", result); var experimentId = GetOptionalString(value, "experimentId", result);
            EvolutionDecisionSource source;
            if (String.IsNullOrWhiteSpace(sourceName)) source = EvolutionDecisionSource.Unknown;
            else if (!Enum.TryParse(sourceName, true, out source)) { result.AddError("UNKNOWN_SOURCE", "Unknown evolution source: " + sourceName, "metadata.source"); source = EvolutionDecisionSource.Unknown; }
            return new EvolutionDecisionMetadata(source, experimentId);
        }

        private static string GetRequiredString(JsonElement objectElement, string name, EvolutionDecisionResult result) { var value = GetOptionalString(objectElement, name, result); if (String.IsNullOrWhiteSpace(value)) result.AddError("MISSING_FIELD", name + " is required.", name); return value; }
        private static string GetOptionalString(JsonElement objectElement, string name, EvolutionDecisionResult result) { JsonElement value; if (!objectElement.TryGetProperty(name, out value) || value.ValueKind == JsonValueKind.Null) return null; if (value.ValueKind != JsonValueKind.String) { result.AddError("INVALID_FIELD", name + " must be a string.", name); return null; } return value.GetString(); }
        private static int GetRequiredInt(JsonElement objectElement, string name, EvolutionDecisionResult result) { JsonElement value; if (!objectElement.TryGetProperty(name, out value) || value.ValueKind != JsonValueKind.Number || !value.TryGetInt32(out var parsed)) { result.AddError("MISSING_FIELD", name + " must be an integer.", name); return 0; } return parsed; }
    }
}
