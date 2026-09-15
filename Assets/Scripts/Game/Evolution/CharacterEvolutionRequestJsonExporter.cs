using System.Text.Json;
using MakeMeHero.Core;

namespace MakeMeHero.Game
{
    /// <summary>Outer adapter for sending a stat/point observation to an external decision maker.</summary>
    public sealed class CharacterEvolutionRequestJsonExporter
    {
        private readonly JsonSerializerOptions _options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        public string Serialize(CharacterEvolutionRequest request) { return JsonSerializer.Serialize(request, _options); }
    }
}
