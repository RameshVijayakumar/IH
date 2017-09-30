using System;
using Newtonsoft.Json;

namespace Paycor.Import.Mapping
{
    public class ApiMappingStatusInfo : RepositoryObject
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "docUrl")]
        public string DocUrl { get; set; }

        [JsonProperty("lastRegistered")]
        public DateTime? LastRegistered { get; set; }

    }
}