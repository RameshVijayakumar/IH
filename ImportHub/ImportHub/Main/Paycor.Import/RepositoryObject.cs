using Newtonsoft.Json;

namespace Paycor.Import
{
    public class RepositoryObject
    {
        [JsonProperty("systemType")]
        public string SystemType { get; set; }
        
    }
}
