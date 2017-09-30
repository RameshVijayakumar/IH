using System.Collections.Generic;
using Newtonsoft.Json;

namespace Paycor.Import.Service.Shared
{
    public class FileTypeFilteredPayload
    {
        [JsonProperty(PropertyName = "objectType")]
        public string ObjectType { get; set; }

        [JsonProperty(PropertyName = "payload")]
        public IEnumerable<string> Payload { get; set; }
    }
}