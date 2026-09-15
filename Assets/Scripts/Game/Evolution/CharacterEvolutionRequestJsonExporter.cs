using MakeMeHero.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MakeMeHero.Game
{
    /// <summary>Outer adapter for sending a stat/point observation to an external decision maker.</summary>
    public sealed class CharacterEvolutionRequestJsonExporter
    {
        private readonly JsonSerializerSettings _options = new JsonSerializerSettings { Formatting = Formatting.Indented, ContractResolver = new CamelCasePropertyNamesContractResolver() };
        public string Serialize(CharacterEvolutionRequest request) { return JsonConvert.SerializeObject(request, _options); }
    }
}
