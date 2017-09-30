using System.Collections.Generic;
using Newtonsoft.Json;
using Paycor.Import.Messaging;

namespace Paycor.Import.Mapping
{
    public class MappingEndpoints
    {
        [JsonProperty(PropertyName = "post")]
        public string Post { get; set; }

        [JsonProperty(PropertyName = "put")]
        public string Put { get; set; }

        [JsonProperty(PropertyName = "delete")]
        public string Delete { get; set; }

        [JsonProperty(PropertyName = "patch")]
        public string Patch { get; set; }

        public Dictionary<HtmlVerb, string> ToEndpointDictionary()
        {
            return new Dictionary<HtmlVerb, string>
            {
                {HtmlVerb.Post, Post},
                {HtmlVerb.Put, Put},
                {HtmlVerb.Delete, Delete},
                {HtmlVerb.Patch, Patch}
            };
        }
    }
}
