using System.Collections.Generic;

namespace Paycor.Import.JsonFormat
{
    public class ApiPayloadArray
    {
        public string ArrayName { get; set; }
        public IEnumerable<IDictionary<string,string>> ArrayData { get; set; }
    }
}
