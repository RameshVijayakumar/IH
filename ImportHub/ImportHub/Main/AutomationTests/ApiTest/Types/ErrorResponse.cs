using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Paycor.Import.ImportHubTest.ApiTest.Types
{
    public class ErrorResponse
    {
        [JsonProperty(PropertyName = "correlationId")]
        public string CorrelationId { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        [JsonProperty(PropertyName = "detail")]
        public string Detail { get; set; }

        [JsonProperty(PropertyName = "source")]
        public Dictionary<string, string> Source { get; set; }
    }
}
