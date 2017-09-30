using System.Collections.Generic;
using System.Linq;
using Paycor.Import.JsonFormat;
using Paycor.Import.Messaging;
namespace Paycor.Import.Extensions
{
    public static class ApiRecordExtensions
    {
        public static void RemoveKeyFromRecord(this ApiRecord apiRecord, string removeKey)
        {
            apiRecord.Record =  apiRecord.Record.Where(t => t.Key != null && t.Key != removeKey)
                    .Select(t => t)
                    .ToDictionary(t => t.Key, t => t.Value);
        }

        public static ApiRecord GetApiRecordByRowNumber(this IEnumerable<ApiRecord> apiRecords, int lineNumber)
        {
            if(apiRecords == null) return new ApiRecord();
            return lineNumber <= apiRecords.Count()
                ? apiRecords.ToList().ElementAt(lineNumber - 1)
                : new ApiRecord
                {
                    RowNumber = 0,
                    ImportType = apiRecords.FirstOrDefault().ImportType,
                    ApiPayloadStringArrays = new List<ApiPayloadStringArray>(),
                    ApiPayloadArrays = new List<ApiPayloadArray>(),
                    Record = new Dictionary<string, string>()
                };
        }
    }
}
