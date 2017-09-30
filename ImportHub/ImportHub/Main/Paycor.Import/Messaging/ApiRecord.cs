using System.Collections.Generic;
using Paycor.Import.JsonFormat;

namespace Paycor.Import.Messaging
{
    public class ApiRecord
    {
        public IDictionary<string, string> Record { get; set; }
        public List<ApiPayloadArray> ApiPayloadArrays { get; set; }
        public List<ApiPayloadStringArray> ApiPayloadStringArrays { get; set; }
        public int RowNumber { get; set; }
        public bool IsPayloadMissing { get; set; }
        public string ImportType { get; set; }
    }
}