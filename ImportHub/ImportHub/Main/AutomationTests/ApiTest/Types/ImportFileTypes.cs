using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Paycor.Import.ImportHubTest.ApiTest.Types
{
    public class ImportFileTypes
    {
        [JsonProperty("columnHeaders")]
        public string ColumnHeaders { get; set; }

        [JsonProperty("isCustomMap")]
        public bool IsCustomMap { get; set; }

        [JsonProperty("columnCount")]
        public int ColumnCount { get; set; }

        [JsonProperty("allMappings")]
        public IEnumerable<Map> AllMappings { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("mappings")]
        public IEnumerable<Map> Mappings { get; set; }

        [JsonProperty("fileType")]
        public string FileType { get; set; }
    }
}
