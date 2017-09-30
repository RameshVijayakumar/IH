using System.Collections.Generic;
using Paycor.Import.Messaging;

namespace Paycor.Import.JsonFormat
{
    public interface IApiRecordJsonGenerator
    {
        string MergeWithSubArrayJson(IEnumerable<ApiPayloadArray> apiPayloadArrays, string jsonData);
        string MergeWithStringArrayJson(IEnumerable<ApiPayloadStringArray> apiPayloadStringArrays, string jsonData);

        string SerializeRecordJson(IDictionary<string, string> record);
        string SerializeRecordsListJson(List<IDictionary<string, string>> recordsList);


        string MergeJson(ApiRecord apiRecord, string jsonData);
        string MergeJson(List<ApiRecord> apiRecords, string jsonData);
    }
}