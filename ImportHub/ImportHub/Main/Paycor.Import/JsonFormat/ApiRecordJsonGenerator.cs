using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Paycor.Import.Extensions;
using Paycor.Import.Mapping;
using Paycor.Import.Messaging;
//TODO: Missing unit tests

namespace Paycor.Import.JsonFormat
{
    public class ApiRecordJsonGenerator : IApiRecordJsonGenerator
    {
        private static string MakeApiPayLoadArrayJson(ApiPayloadArray apiPayloadArray)
        {
            Ensure.ThatArgumentIsNotNull(apiPayloadArray, nameof(apiPayloadArray));
            Ensure.ThatArgumentIsNotNull(apiPayloadArray.ArrayData, nameof(apiPayloadArray.ArrayData));
            Ensure.ThatArgumentIsNotNull(apiPayloadArray.ArrayName, nameof(apiPayloadArray.ArrayName));

            var jObject = JObject.FromObject(new
            {
                apiPayloadArrayArrayData = apiPayloadArray.ArrayData,
            });

            var token = jObject.SelectToken("apiPayloadArrayArrayData");
            token.Rename(apiPayloadArray.ArrayName);

            return jObject.ToString();
        }

        private static string MakeApiPayLoadStringArrayJson(ApiPayloadStringArray apiPayloadStringArray)
        {
            Ensure.ThatArgumentIsNotNull(apiPayloadStringArray, nameof(apiPayloadStringArray));
            Ensure.ThatArgumentIsNotNull(apiPayloadStringArray.StringArrayData, nameof(apiPayloadStringArray.StringArrayData));
            Ensure.ThatArgumentIsNotNull(apiPayloadStringArray.StringArrayName, nameof(apiPayloadStringArray.StringArrayName));

            var jObject = JObject.FromObject(new
            {
                apiPayloadArrayStringArray = apiPayloadStringArray.StringArrayData
            });

            var token = jObject.SelectToken("apiPayloadArrayStringArray");
            token.Rename(apiPayloadStringArray.StringArrayName);

            return jObject.ToString();
        }

        public string SerializeRecordJson(IDictionary<string,string> record)
        {
            return JsonConvert.SerializeObject(record, new NestedDictionaryConverter());
        }

        public string SerializeRecordsListJson(List<IDictionary<string, string>> recordsList)
        {
            return JsonConvert.SerializeObject(recordsList);
        }

        public string MergeWithSubArrayJson(IEnumerable<ApiPayloadArray> apiPayloadArrays, string jsonData)
        {
            var jObject = JObject.Parse(jsonData);
            if (apiPayloadArrays == null) return jObject.ToString();

            foreach (var payloadArray in apiPayloadArrays)
            {
                if (payloadArray == null) continue;
                var payloadArrayJson = JObject.Parse(MakeApiPayLoadArrayJson(payloadArray));
                jObject.Merge(payloadArrayJson);
            }
            return jObject.ToString();
        }

        public string MergeWithStringArrayJson(IEnumerable<ApiPayloadStringArray> apiPayloadStringArrays, string jsonData)
        {
            var jObject = JObject.Parse(jsonData);
            if (apiPayloadStringArrays == null) return jObject.ToString();

            foreach (var apiPayloadStringArray in apiPayloadStringArrays)
            {
                if (apiPayloadStringArray == null) continue;
                var payloadArrayJson = JObject.Parse(MakeApiPayLoadStringArrayJson(apiPayloadStringArray));
                jObject.Merge(payloadArrayJson);
            }
            return jObject.ToString();
        }

        public string MergeJson(ApiRecord apiRecord, string jsonData)
        {
            if (apiRecord?.ApiPayloadArrays != null && apiRecord.ApiPayloadArrays.Any())
            {
                jsonData = MergeWithSubArrayJson(apiRecord.ApiPayloadArrays, jsonData);
            }
            if (apiRecord?.ApiPayloadStringArrays != null && apiRecord.ApiPayloadStringArrays.Any())
            {
                jsonData = MergeWithStringArrayJson(apiRecord.ApiPayloadStringArrays,
                    jsonData);
            }

            return jsonData;
        }

        public string MergeJson(List<ApiRecord> apiRecords, string jsonData)
        {
            if (apiRecords == null) return jsonData;
            foreach (var batchRecord in apiRecords)
            {
                jsonData = MergeJson(batchRecord, jsonData);
            }

            return jsonData;
        }
    }
}