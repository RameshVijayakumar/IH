using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Runtime.Remoting.Messaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Paycor.Import.Extensions;
using Paycor.Import.Http;
using Paycor.Import.JsonFormat;
using Paycor.Import.MapFileImport.Contract;
using Paycor.Import.MapFileImport.Implementation.FailedRecords;
using Paycor.Import.Messaging;

namespace Paycor.Import.MapFileImport.Implementation.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class GenerateFailedRecordTests
    {
        [TestCleanup]
        public void TestCleanup()
        {
            CallContext.FreeNamedDataSlot("log4net.Util.LogicalThreadContextProperties");
        }

        [TestMethod]
        public void GetFailedRecordTest()
        {
            var errorFormatter = new Mock<IErrorFormatter>();
            var generateFailedRecord = new GenerateFailedRecord(errorFormatter.Object);
            var apiPayloadStringArray1 = new ApiPayloadStringArray
            {
                StringArrayData = new List<string> { "101", "102" },
                StringArrayName = "linkedDeductionCodes",
            };

            var apiPayloadStringArray2 = new ApiPayloadStringArray
            {
                StringArrayData = new List<string> { "101", "102" },
                StringArrayName = "linkedDeductionCodes1",
            };
            var apiPayloadStringArrays = new List<ApiPayloadStringArray> { apiPayloadStringArray1, apiPayloadStringArray2 };

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
                    },
                }

            };
            var apiPayloadArrays = new List<ApiPayloadArray> { apiPayloadArray1, apiPayloadArray2 };

            var apiRecord = new ApiRecord()
            {
                ApiPayloadStringArrays = apiPayloadStringArrays,
                ApiPayloadArrays = apiPayloadArrays,
                Record = new ConcurrentDictionary<string, string>
                {
                    ["clientId"] = "91970",
                    ["publisher"] = "AT",
                    ["retailPrice"] = "50",
                    ["title"] = "NFL17"
                }
            };


            generateFailedRecord.GetFailedRecord(apiRecord, new ErrorResponse { Source = null }, null);

            var apiRecord1 = new ApiRecord
            {
                ApiPayloadStringArrays = apiPayloadStringArrays,
                ApiPayloadArrays = apiPayloadArrays,
                Record = new ConcurrentDictionary<string, string>
                {
                    ["clientId"] = "91970",
                    ["publisher"] = "AT",
                    ["retailPrice"] = "50",
                    ["title"] = "NFL17"
                }
            };

            var failedRecord = generateFailedRecord.GetFailedRecord(apiRecord1, new ErrorResponse(), new HttpExporterResult());

            Assert.AreEqual(2,failedRecord.CustomData.Count);
            Assert.AreEqual(26, failedRecord.Record.Count);

            var apiRecord2 = new ApiRecord
            {
                ApiPayloadStringArrays = apiPayloadStringArrays,
                ApiPayloadArrays = apiPayloadArrays,
                Record = new ConcurrentDictionary<string, string>
                {
                    ["clientId"] = "91970",
                    ["publisher"] = "AT",
                    ["retailPrice"] = "50",
                    ["title"] = "NFL17"
                }
            };

            failedRecord = generateFailedRecord.GetFailedRecord(apiRecord2, new ErrorResponse(), new HttpExporterResult { Response = new HttpResponseMessage() });
            Assert.AreEqual(2, failedRecord.CustomData.Count);
            Assert.AreEqual(26, failedRecord.Record.Count);
            var apiRecord3 = new ApiRecord
            {
                ApiPayloadStringArrays = apiPayloadStringArrays,
                ApiPayloadArrays = apiPayloadArrays,
                Record = new ConcurrentDictionary<string, string>
                {
                    ["clientId"] = "91970",
                    ["publisher"] = "AT",
                    ["retailPrice"] = "50",
                    ["title"] = "NFL17"
                }
            };
            failedRecord = generateFailedRecord.GetFailedRecord(apiRecord3, new ErrorResponse
            {
                Source = new ConcurrentDictionary<string, string>
                {
                    ["clientId"] = "91970"
                }
            }, new HttpExporterResult());

            Assert.AreEqual(1, failedRecord.CustomData.Count);
            Assert.AreEqual(26, failedRecord.Record.Count);

            var apiRecord4 = new ApiRecord()
            {
                ApiPayloadStringArrays = null,
                ApiPayloadArrays = null,
                Record = new ConcurrentDictionary<string, string>()
                {
                    ["clientId"] = "91970",
                    ["publisher"] = "AT",
                    ["retailPrice"] = "50",
                    ["title"] = "NFL17"
                }
            };
            failedRecord = generateFailedRecord.GetFailedRecord(apiRecord4, new ErrorResponse
            {
                Source = new ConcurrentDictionary<string, string>
                {
                    ["clientId"] = "91970"
                }
            }, new HttpExporterResult());

            Assert.AreEqual(1, failedRecord.Errors.Count);
            Assert.AreEqual(1, failedRecord.CustomData.Count);
            Assert.AreEqual(4, failedRecord.Record.Count);
        }
    }
}