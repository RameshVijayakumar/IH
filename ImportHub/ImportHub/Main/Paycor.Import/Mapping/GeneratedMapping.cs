using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Paycor.Import.Mapping
{

    [ExcludeFromCodeCoverage]
    public class GeneratedMapping : ApiMapping 
    {
        [JsonProperty(PropertyName = "docUrl")]
        public string DocUrl { get; set; }
    }
}
