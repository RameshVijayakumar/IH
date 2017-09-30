using System.Collections.Generic;
using System.Linq;
using Paycor.Import.Extensions;
using Paycor.Import.JsonFormat;
using Paycor.Import.MapFileImport.Contract;
using Paycor.Import.Mapping;
using Paycor.Import.Messaging;

namespace Paycor.Import.MapFileImport.Implementation.Transformers
{
    public class FlatToApiRecordsTransformer : IFlatToApiRecordsTransformer
    {
        private readonly ICalculate _calculate;

        public FlatToApiRecordsTransformer(ICalculate calculate)
        {
            _calculate = calculate;
        }

        public IList<ApiRecord> TranslateFlatRecordsToApiRecords(IEnumerable<IDictionary<string, string>> records, ApiMapping mapping, ImportContext context)
        {
            var apiRecords = new List<ApiRecord>();

            var subArrayList = GetSubArrayFields(mapping.Mapping).ToList();

            var count = 0;
            foreach (var record in records)
            {
                count++;
                var apiRecord = new ApiRecord
                {
                    Record = GetNonSubArrayItems(record),
                    ApiPayloadArrays = new List<ApiPayloadArray>(),
                    ApiPayloadStringArrays = new List<ApiPayloadStringArray>(),
                    RowNumber = context.IsMultiSheetImport ? count : _calculate.GetFileRowNumber(context.ChunkSize, context.ChunkNumber, count),
                    IsPayloadMissing = IsMissingPayload(record, mapping.Mapping),
                    ImportType = mapping.MappingName
                };

                foreach (var subArray in subArrayList)
                {
                    if (subArray.ArrayFieldNames.Any())
                    {
                        AddPayLoadArray(subArray, record, apiRecord);
                    }
                    else
                    {
                        AddStringArray(subArray, record, apiRecord);
                    }
                }
                apiRecords.Add(apiRecord);
            }
            return apiRecords;
        }

        private static void AddPayLoadArray(SubArrayFieldInfo subArrayDetail, IDictionary<string, string> record, ApiRecord apiRecord)
        {
            var arrayName = subArrayDetail.ArrayName;
            var arrayData = ExtractSubArrayData(subArrayDetail, record);

            if(arrayData == null) return;

            var apiPayloadArray = new ApiPayloadArray
            {
                ArrayName = arrayName,
                ArrayData = arrayData
            };
            apiRecord.ApiPayloadArrays.Add(apiPayloadArray);
        }

        private static void AddStringArray(SubArrayFieldInfo subArrayDetail, IDictionary<string, string> record, ApiRecord apiRecord)
        {
            var stringArrayName = subArrayDetail.ArrayName;
            var stringArrayData = ExtractStringArrayData(subArrayDetail, record);

            if (stringArrayData == null) return;
            var apiPayloadStringArray = new ApiPayloadStringArray
            {
                StringArrayName = stringArrayName,
                StringArrayData = stringArrayData
            };
            apiRecord.ApiPayloadStringArrays.Add(apiPayloadStringArray);
        }

        private static List<string> ExtractStringArrayData(SubArrayFieldInfo subArray, IDictionary<string, string> record)
        {
            var allStringArrayData = new List<string>();

            var keyIndex = GetKeysofTheArray(record.Keys, subArray.ArrayName);

            var enumerable = keyIndex as int[] ?? keyIndex.ToArray();
            if (!enumerable.Any()) return null;
            var numberOfElements = enumerable.Max();

            for (var i = 0; i <= numberOfElements; i++)
            {
                var key = $"{subArray.ArrayName}[{i}]";
                if (!record.ContainsKey(key) || string.IsNullOrWhiteSpace(record[key])) continue;

                var stringArrayData = record[key];
                allStringArrayData.Add(stringArrayData);
            }
            return allStringArrayData;
        }

        private static IDictionary<string, string> GetNonSubArrayItems(IDictionary<string, string> record)
        {
            var nonSubArrayItems = record.Where(p => !(p.Key.Contains("[") && p.Key.Contains("]")));

            var dictionary = nonSubArrayItems.ToDictionary(x => x.Key, x => x.Value);

            return dictionary;
        }

        private static IEnumerable<IDictionary<string, string>> ExtractSubArrayData(SubArrayFieldInfo subArray, IDictionary<string, string> record)
        {
            var allSubarrayData = new List<IDictionary<string, string>>();
            var keyIndex = GetKeysofTheArray(record.Keys, subArray.ArrayName);

            var enumerable = keyIndex as int[] ?? keyIndex.ToArray();
            if (!enumerable.Any()) return null;

            var numberOfElements = enumerable.Max();

            for (var i = 0; i <= numberOfElements; i++)
            {
                IDictionary<string, string> subArrayData = new Dictionary<string, string>();

                foreach (var field in subArray.ArrayFieldNames)
                {
                    if (record.ContainsKey($"{subArray.ArrayName}[{i}].{field}") &&
                        !string.IsNullOrWhiteSpace(record[$"{subArray.ArrayName}[{i}].{field}"]))
                    {
                        subArrayData[field] = record[$"{subArray.ArrayName}[{i}].{field}"];
                    }
                }
                if (subArrayData.Any())
                {
                    allSubarrayData.Add(subArrayData);
                }
            }
            return allSubarrayData;
        }

        private static IEnumerable<int> GetKeysofTheArray(IEnumerable<string> keys, string arrayName)
        {
            var t = from k in keys
                    where k.StartsWith(arrayName + "[")
                    select GetIndexof(k);
            return t;

        }

        private static int GetIndexof(string s)
        {
            int index;

            var suffixPart = s.Split('[');
            var indexPart = suffixPart[1].Split(']');
            var indexAsString = indexPart[0];

            int.TryParse(indexAsString, out index);
            return index;
        }

        private static IEnumerable<SubArrayFieldInfo> GetSubArrayFields(MappingDefinition mappingDefinition)
        {
            var arrayFields =
                mappingDefinition.FieldDefinitions.Where(m => m.Destination.Contains("[") && m.Destination.Contains("]"))
                    .ToList();

            var arrayNames = arrayFields.Select(GetArrayName).Distinct().ToList();

            var subArrayDetails = new List<SubArrayFieldInfo>();
            foreach (var arrayName in arrayNames)
            {
                var subarray = new SubArrayFieldInfo() { ArrayName = arrayName };

                var arrayFieldName =
                    arrayFields.Where(p => p.Destination.Contains(arrayName + "[")).Select(GetArrayElements).Where(p => p != string.Empty).Distinct().ToList();

                subarray.ArrayFieldNames = arrayFieldName;
                subArrayDetails.Add(subarray);
            }
            return subArrayDetails;
        }

        private static string GetArrayElements(MappingFieldDefinition field)
        {
            var fieldName = string.Empty;

            var fieldNameHolder = field.Destination.Split('.');

            if (fieldNameHolder.Length > 1)
            {
                fieldName = fieldNameHolder[1];
            }
            return fieldName;
        }

        private static string GetArrayName(MappingFieldDefinition field)
        {
            var result = field.Destination.Split('[');
            return result[0];
        }

        private static bool IsMissingPayload(IDictionary<string, string> record, MappingDefinition mappingDefinition)
        {
            var lookupMapFieldsWithPayloadRequired = mappingDefinition.FieldDefinitions.Where(
                    t => t.Source.ContainsOpenAndClosedBraces() && t.IsRequiredForPayload);

            foreach (var mapFields in lookupMapFieldsWithPayloadRequired)
            {
                if (!record.Keys.Contains(mapFields.Destination.RemoveBraces()))
                {
                    return true;
                }
            }
            return false;
        }


    }

}
