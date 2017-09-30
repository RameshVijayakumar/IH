using System.Collections.Generic;
using Newtonsoft.Json;

namespace Paycor.Import.ImportHubTest.ApiTest.Types
{
    public class SystemCheckResponse
    {
        [JsonProperty(PropertyName = "result")]
        public string Result { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "currentSetting")]
        public string CurrentSetting { get; set; }

        [JsonProperty(PropertyName = "additionalInformation")]
        public string AdditionalInformation { get; set; }

    }
}
