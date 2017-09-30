using Newtonsoft.Json;
using Paycor.Import.Mapping;

namespace ServiceRegistrationWorker
{
    /// <summary>
    /// Used for migration only. This class is a union of the following classes:
    /// UserMapping, GlobalMapping, GeneratedMapping
    /// </summary>
    public class LegacyApiMapping : UserMapping
    {
        [JsonProperty(PropertyName = "docUrl")]
        public string DocUrl { get; set; }
    }
}
