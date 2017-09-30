using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Paycor.Import.ImportHubTest.ApiTest.Types;

namespace Paycor.Import.ImportHubTest.ApiTest.TestBase
{

    public class FileUploadRequest
    {
        public string filename { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ImportSourceType name { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<Map> mappings { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Map mapping { get; set; }

        public FileUploadRequest(string filePath, IEnumerable<Map> mappings, bool isMultiType = true, ImportSourceType sourceType = ImportSourceType.File)
        {
            name = sourceType;
            filename = filePath;
            if (isMultiType)
            {
                this.mappings = mappings;
            }
            else
            {
                mapping = mappings.FirstOrDefault();
            }

        }

        public override string ToString()
        {
            //return JsonConvert.SerializeObject(this, Formatting.Indented, new JsonSerializerSettings());
            return JsonConvert.SerializeObject(this);
        }
    }
}
