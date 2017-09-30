using System.Collections.Generic;
using Newtonsoft.Json;

namespace Paycor.Import.Mapping
{
    public class GlobalLookupDefinition : RepositoryObject
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "lookupTypeName")]
        public string LookupTypeName { get; set; }

        [JsonProperty(PropertyName = "lookupTypeValue")]
        public IDictionary<string, string> LookupTypeValue { get; set; }
    }
}