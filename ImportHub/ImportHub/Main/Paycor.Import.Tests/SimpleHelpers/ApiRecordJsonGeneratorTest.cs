using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Paycor.Import.JsonFormat;
using Paycor.Import.Mapping;
using Paycor.Import.Messaging;

namespace Paycor.Import.Tests.SimpleHelpers
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ApiPayloadMergerTest
    {
        [TestMethod]
        public void Merge_Json_With_SubArray()
        {
            var apiPayloadArray1 = new ApiPayloadArray
            {
                ArrayName = "deductionLimits",
                ArrayData = new List<IDictionary<string, string>>
                {
                    new Dictionary<string, string>
                    {
                        ["id"] = "1",
                        ["frequency"] = "monthly",
                        ["MaxAmount"] = "100"
                    },
                    new Dictionary<string, string>
                    {
                        ["id"] = "2",
                        ["frequency"] = "quarterly",
                        ["MaxAmount"] = "300"
                    },
                    new Dictionary<string, string>
                    {
                        ["id"] = "3",
                        ["frequency"] = "half",
                        ["MaxAmount"] = "600"
                    },
                }

            };
            var apiPayloadArray2 = new ApiPayloadArray
            {
                ArrayName = "deductionLimits2",
                ArrayData = new List<IDictionary<string, string>>
                {
                    new Dictionary<string, string>
                    {
                        ["id"] = "1",
                        ["frequency"] = "monthly",
                        ["MaxAmount"] = "100"
                    },
                    new Dictionary<string, string>
                    {
                        ["id"] = "2",
                        ["frequency"] = "quarterly",
                        ["MaxAmount"] = "300"
                    },
                    new Dictionary<string, string>
                    {
                        ["id"] = "3",
                        ["frequency"] = "half",
                        ["MaxAmount"] = "600"
                    }
                }

            };
            var apiPayloadArrays = new List<ApiPayloadArray> { apiPayloadArray1, apiPayloadArray2 };

            var merger = new ApiRecordJsonGenerator();
            var mergedData = merger.MergeWithSubArrayJson(apiPayloadArrays, File.ReadAllText("Json\\Merge.json"));

            var diff = ObjectDiffPatch.GenerateDiff(JObject.Parse(mergedData), JObject.Parse(File.ReadAllText("Json\\SubArrayCombined.json")));
            Assert.IsTrue(diff.AreEqual);
        }

        [TestMethod]
        public void Merge_Json_With_StringArray()
        {
            var apiPayloadStringArray1 = new ApiPayloadStringArray()
            {
                StringArrayData = new List<string> { "101", "102" },
                StringArrayName = "linkedDeductionCodes",
            };

            var apiPayloadStringArray2 = new ApiPayloadStringArray()
            {
                StringArrayData = new List<string> { "101", "102" },
                StringArrayName = "linkedDeductionCodes1",
            };
            var apiPayloadStringArrays = new List<ApiPayloadStringArray> { apiPayloadStringArray1, apiPayloadStringArray2 };

            var merger = new ApiRecordJsonGenerator();
            var mergedData = merger.MergeWithStringArrayJson(apiPayloadStringArrays, File.ReadAllText("Json\\Merge.json"));

            var diff = ObjectDiffPatch.GenerateDiff(JObject.Parse(mergedData), JObject.Parse(File.ReadAllText("Json\\StringArrayCombined.json")));
            Assert.IsTrue(diff.AreEqual);
        }

        [TestMethod]
        public void Merge_Json_With_StringArray_And_SubArray()
        {
            var apiPayloadStringArray1 = new ApiPayloadStringArray()
            {
                StringArrayData = new List<string> { "101", "102" },
                StringArrayName = "linkedDeductionCodes",
            };

            var apiPayloadStringArray2 = new ApiPayloadStringArray()
            {
                StringArrayData = new List<string> { "101", "102" },
                StringArrayName = "linkedDeductionCodes1",
            };
            var apiPayloadStringArrays = new List<ApiPayloadStringArray> { apiPayloadStringArray1, apiPayloadStringArray2 };

            var apiPayloadArray1 = new ApiPayloadArray()
            {
                ArrayName = "deductionLimits",
                ArrayData = new List<IDictionary<string, string>>()
                {
                    new Dictionary<string, string>()
                    {
                        ["id"] = "1",
                        ["frequency"] = "monthly",
                        ["MaxAmount"] = "100"
                    },
                    new Dictionary<string, string>()
                    {
                        ["id"] = "2",
                        ["frequency"] = "quarterly",
                        ["MaxAmount"] = "300"
                    },
                    new Dictionary<string, string>()
                    {
                        ["id"] = "3",
                        ["frequency"] = "half",
                        ["MaxAmount"] = "600"
                    },
                }

            };
            var apiPayloadArray2 = new ApiPayloadArray()
            {
                ArrayName = "deductionLimits2",
                ArrayData = new List<IDictionary<string, string>>()
                {
                    new Dictionary<string, string>()
                    {
                        ["id"] = "1",
                        ["frequency"] = "monthly",
                        ["MaxAmount"] = "100"
                    },
                    new Dictionary<string, string>()
                    {
                        ["id"] = "2",
                        ["frequency"] = "quarterly",
                        ["MaxAmount"] = "300"
                    },
                    new Dictionary<string, string>()
                    {
                        ["id"] = "3",
                        ["frequency"] = "half",
                        ["MaxAmount"] = "600"
                    },
                }

            };
            var apiPayloadArrays = new List<ApiPayloadArray> { apiPayloadArray1, apiPayloadArray2 };

            var apiRecord = new ApiRecord()
            {
                ApiPayloadStringArrays = apiPayloadStringArrays,
                ApiPayloadArrays = apiPayloadArrays,
                Record = new ConcurrentDictionary<string, string>()
                {
                    ["clientId"] = "91970",
                    ["publisher"] = "AT",
                    ["retailPrice"] = "50",
                    ["title"] = "NFL17"
                }
            };

            var merger = new ApiRecordJsonGenerator();

            var mergedData = merger.MergeJson(apiRecord, JsonConvert.SerializeObject(apiRecord.Record, new NestedDictionaryConverter()));
            var diff = ObjectDiffPatch.GenerateDiff(JObject.Parse(mergedData), JObject.Parse(File.ReadAllText("Json\\StringArrayAndSubArrayCombined.json")));
            Assert.IsTrue(diff.AreEqual);
        }
    }
}
