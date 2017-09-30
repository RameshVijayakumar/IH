using System.Collections.Generic;
using Newtonsoft.Json.Linq;
//TODO: Partial unit test

namespace Paycor.Import.JsonFormat
{
    public class JsonFormatter : IFormatJson
    {
        public string RemoveProperties(string data, List<string> removeList)
        {
            var parsedData = JObject.Parse(data);

            if (removeList == null) return data;

            foreach (var remove in removeList)
            {
                if (remove != null) parsedData.Remove(remove);
            }

            return parsedData?.ToString() ?? data;
        }
    }
}
