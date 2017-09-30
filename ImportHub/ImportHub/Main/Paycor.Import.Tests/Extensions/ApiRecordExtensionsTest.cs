using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paycor.Import.Extensions;
using Paycor.Import.Messaging;

namespace Paycor.Import.Tests.Extensions
{
    [TestClass]
    public class ApiRecordExtensionsTest
    {
        [TestMethod]
        public void GetApiRecordByRowNumberTest_IndexValueGreater()
        {
            var apirecords = new List<ApiRecord>
            {
                new ApiRecord
                {
                    Record = new Dictionary<string, string>
                    {
                        {"ClientId", "102" },
                        {"employeeNumber", "10052" },
                        {"Name","Ashif" }
                    },
                    ImportType = "Test Import",
                    RowNumber = 1
                },
                new ApiRecord
                {
                    Record = new Dictionary<string, string>
                    {
                        {"ClientId", "102" },
                        {"employeeNumber", "10043" },
                        {"Name","Anwar" }
                    },
                    ImportType = "Test Import",
                    RowNumber = 2
                },
                new ApiRecord
                {
                    Record = new Dictionary<string, string>
                    {
                        {"ClientId", "102" },
                        {"employeeNumber", "10044" },
                        {"Name","Mark Yang" }
                    },
                    ImportType = "Test Import",
                    RowNumber = 3
                }
            };

            var result = apirecords.GetApiRecordByRowNumber(4);
            Assert.AreEqual(result.RowNumber, 0);
            Assert.AreEqual(result.Record.Count, 0);
            Assert.AreEqual(result.ApiPayloadArrays.Count, 0);
            Assert.AreEqual(result.ApiPayloadStringArrays.Count, 0);
        }

        [TestMethod]
        public void GetApiRecordByRowNumberTest()
        {
            var apirecords = new List<ApiRecord>
            {
                new ApiRecord
                {
                    Record = new Dictionary<string, string>
                    {
                        {"ClientId", "102" },
                        {"employeeNumber", "10052" },
                        {"Name","Ashif" }
                    },
                    ImportType = "Test Import",
                    RowNumber = 1
                },
                new ApiRecord
                {
                    Record = new Dictionary<string, string>
                    {
                        {"ClientId", "102" },
                        {"employeeNumber", "10043" },
                        {"Name","Anwar" }
                    },
                    ImportType = "Test Import",
                    RowNumber = 2
                },
                new ApiRecord
                {
                    Record = new Dictionary<string, string>
                    {
                        {"ClientId", "102" },
                        {"employeeNumber", "10044" },
                        {"Name","Mark Yang" }
                    },
                    ImportType = "Test Import",
                    RowNumber = 3
                }
            };

            var result = apirecords.GetApiRecordByRowNumber(1);
            Assert.AreEqual(result.RowNumber, 1);
            Assert.AreEqual(result.Record.Count,3);
            Assert.AreEqual(result.ApiPayloadArrays, null);
            Assert.AreEqual(result.ApiPayloadStringArrays, null);


        }
    }
}
